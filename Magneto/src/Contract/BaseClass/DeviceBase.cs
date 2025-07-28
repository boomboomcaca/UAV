using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace Magneto.Contract.BaseClass;

public abstract class DeviceBase(Guid deviceId) : IDevice, IDisposable
{
    public virtual bool Initialized(ModuleInfo device)
    {
        DeviceInfo = device;
        return true;
    }

    public virtual void Attach(IDataPort dataPort)
    {
        MessageDataPort = dataPort;
    }

    public virtual void SetParameter(string name, object value)
    {
        if (name == ParameterNames.MfdfPoints)
            // 离散测向频点为功能中设置的参数，这里不做处理
            return;
        var type = GetType();
        var prop = Utils.FindPropertyByName(name, type);
        if (prop == null) return;
        if (value is null) return;
        try
        {
            object objValue = null;
            if (prop.PropertyType.IsEnum)
            {
                if (value is List<object> list)
                {
                    // Flag
                    var num = list.Select(item => Utils.ConvertStringToEnum(item.ToString(), prop.PropertyType))
                        .Aggregate<object, long>(0, (current, enum1) => current | (long)enum1);
                    objValue = Enum.ToObject(prop.PropertyType, num);
                }
                else
                {
                    objValue = Utils.ConvertStringToEnum(value.ToString(), prop.PropertyType);
                }
            }
            else if (prop.PropertyType == typeof(Guid))
            {
                objValue = Guid.Parse(value.ToString()!);
            }
            else if (prop.PropertyType.IsValueType)
            {
                objValue = Convert.ChangeType(value, prop.PropertyType);
            }
            else if (prop.PropertyType.IsArray)
            {
                // 没有其他办法，只能暴力解决
                var tName = prop.PropertyType.FullName?.Replace("[]", string.Empty);
                if (tName != null)
                {
                    var itemType = prop.PropertyType.Assembly.GetType(tName);
                    var tmpValue = value;
                    if (value is JArray jArray)
                        tmpValue = jArray.Select(item =>
                        {
                            if (itemType != null) return item.ToObject(itemType);
                            return null;
                        }).ToArray();
                    if (tmpValue is object[] array)
                    {
                        // objValue = array.Select(item => Convert.ChangeType(item, itemType));
                        // 这里没办法，上面那种转换方式不可行，只能用下面这种方式了
                        if (itemType != null && itemType == typeof(int))
                            objValue = array.Select(Convert.ToInt32).ToArray();
                        else if (itemType != null && itemType == typeof(double))
                            objValue = array.Select(Convert.ToDouble).ToArray();
                        else if (itemType != null && itemType == typeof(float))
                            objValue = array.Select(Convert.ToSingle).ToArray();
                        else if (itemType != null && itemType == typeof(short))
                            objValue = array.Select(Convert.ToInt16).ToArray();
                        else
                            objValue = tmpValue;
                    }
                }
            }
            else
            {
                // objValue = Convert.ChangeType(value, prop.PropertyType);
                objValue = value; //Convert.ChangeType(value, prop.PropertyType);
            }

            if (prop.PropertyType == typeof(double) && name.Contains("frequency", StringComparison.OrdinalIgnoreCase) &&
                double.TryParse(objValue?.ToString(), out var db)) objValue = Math.Round(db, 6);
            prop.SetValue(this, objValue);
        }
        catch (Exception ex)
        {
            // 这里如果报错就直接返回，暂时不需要做异常处理
            Console.WriteLine($"参数设置失败:{ex}");
        }
    }

    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DeviceBase()
    {
        Dispose(false);
    }

    public virtual void Start(FeatureType feature, IDataPort dataPort)
    {
        CurFeature = feature;
        DataPort = dataPort;
        TaskState = TaskState.Start;
    }

    public virtual void Stop()
    {
        TaskState = TaskState.Stop;
    }

    /// <summary>
    ///     发送数据
    /// </summary>
    /// <param name="data"></param>
    protected virtual void SendData(List<object> data)
    {
        /*
            数据中添加设备ID，此举是为了满足以下场景
            单频测向功能中配置了接收机时，示向度数据通过测向机获取，而频谱、电平、音频数据则从接收机获取
        */
        data.Insert(0, DeviceId);
        DataPort?.OnData(data);
    }

    /// <summary>
    ///     发送消息
    /// </summary>
    /// <param name="message"></param>
    protected virtual void SendMessage(SDataMessage message)
    {
        MessageDataPort?.OnMessage(message);
    }

    /// <summary>
    ///     通过消息通道发送数据
    /// </summary>
    /// <param name="data"></param>
    protected virtual void SendMessageData(List<object> data)
    {
        MessageDataPort?.OnData(data);
    }

    public T GetParameter<T>(string name)
    {
        try
        {
            var type = GetType();
            var prop = Utils.FindPropertyByName(name, type);
            if (prop == null) return default;
            return (T)prop.GetValue(this, null);
        }
        catch
        {
            return default;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed) return;
        if (disposing)
        {
            DataPort = null;
            MessageDataPort = null;
        }

        Disposed = true;
    }

    /// <summary>
    ///     返回RPC异常
    ///     注意，调用本方法会抛出异常！
    /// </summary>
    /// <param name="errorCode">
    ///     异常代码，<see cref="ErrorCode" />
    /// </param>
    /// <param name="error">错误信息</param>
    protected virtual void ThrowRpcException(int errorCode, string error)
    {
        var exc = new LocalRpcException(error)
        {
            ErrorCode = errorCode
        };
        throw exc;
    }

    #region Global Members

    protected FeatureType CurFeature;
    protected IDataPort DataPort;
    protected Guid DeviceId = deviceId;
    protected ModuleInfo DeviceInfo;
    protected bool Disposed;
    protected IDataPort MessageDataPort;
    protected TaskState TaskState = TaskState.Stop;
    public Guid Id => DeviceId;

    #endregion

    #region 设备重连

    /// <summary>
    ///     设备连接成功后，可调用此方法来设置TCP-KeepAlive模式并启动心跳检查线程
    /// </summary>
    /// <param name="tcpSocket">tcp连接对象</param>
    protected void SetHeartBeat(Socket tcpSocket)
    {
        if (tcpSocket == null) return;
        //设置TCP-KeepAlive模式，若1秒钟之内没有收到探测包回复则再尝试以500ms为间隔发送10次，如果一直都没有回复则认为TCP连接已经断开（为了检测网线断连等异常情况）
        var bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xE8, 0x03, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00 };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 这段代码在linux下会报错Socket.IOControl handles Windows-specific control codes and is not supported
            tcpSocket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
        }
        else
        {
            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
            tcpSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }

        //启动TCP连接检查线程
        var thHeartBeat = new Thread(KeepAlive)
        {
            IsBackground = true,
            Name = $"{DeviceInfo.DisplayName}({DeviceId}) KeepAlive Thread"
        };
        thHeartBeat.Start(tcpSocket);
    }

    /// <summary>
    ///     心跳检查线程函数
    ///     实现 tcp 连接的心跳检查；子类可重载此方法，实现其它连接方式的心跳检查
    /// </summary>
    /// <param name="connObject">tcp连接对象</param>
    protected virtual void KeepAlive(object connObject)
    {
        if (connObject is not Socket socket) return;
        while (!Disposed && IsSocketConnected(socket)) Thread.Sleep(1000);
        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }

    /// <summary>
    ///     检查当前TCP连接的状态
    /// </summary>
    /// <param name="socket">目标tcp连接</param>
    /// <param name="maxRetry">寻找目标连接的最大尝试次数</param>
    protected bool IsSocketConnected(Socket socket, int maxRetry = 3)
    {
        if (socket == null || maxRetry == 0) return false;
        try
        {
            var localEndPoint = socket.LocalEndPoint?.ToString();
            var remoteEndPoint = socket.RemoteEndPoint?.ToString();
            var validConnection = Array.Find(IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections(), item => item.LocalEndPoint.ToString().Equals(localEndPoint)
                                                    && item.RemoteEndPoint.ToString().Equals(remoteEndPoint));
            if (validConnection != null) return validConnection.State == TcpState.Established;
        }
        catch
        {
            // 理论上此处不会抛出任何异常
            // 异常:
            // T:System.Net.NetworkInformation.NetworkInformationException: Win32 函数 GetTcpTable 失败。
        }

        Thread.Sleep(1000);
        return IsSocketConnected(socket, --maxRetry);
    }

    #endregion
}