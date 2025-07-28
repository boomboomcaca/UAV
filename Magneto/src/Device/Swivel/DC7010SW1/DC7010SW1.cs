using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7010SW1;

public partial class Dc7010Sw1 : DeviceBase
{
    #region 构造函数

    public Dc7010Sw1(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     设备连接套接字
    /// </summary>
    private Socket _socket;

    /// <summary>
    ///     业务数据采集处理线程
    /// </summary>
    private Task _dataProcessTask;

    /// <summary>
    ///     发送查询命令、设置客户端命令线程
    /// </summary>
    private Task _operationCmdTask;

    private CancellationTokenSource _dataProcessTokenSource;
    private CancellationTokenSource _operationCmdTokenSource;

    /// <summary>
    ///     互斥锁
    /// </summary>
    private readonly object _locker = new();

    /// <summary>
    ///     比幅测向时，云台转动的圈数(一圈为一次比幅测向)
    /// </summary>
    private int _ampldfTime;

    /// <summary>
    ///     启动收天线标志
    /// </summary>
    private bool _isRecoverAntenna;

    /// <summary>
    ///     启动比幅测向标志
    /// </summary>
    private bool _isAmpdf;

    /// <summary>
    ///     转动天线标志
    /// </summary>
    private bool _isRotateAzimuth;

    /// <summary>
    ///     启动比幅测向标志，以便转台归零处
    /// </summary>
    private bool _isBeginAmpdf;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo device)
    {
        try
        {
            var result = base.Initialized(device);
            if (!result) return false;
            //释放资源
            ReleaseResources();
            //初始化网络连接
            InitNetwork();
            //初始化任务
            InitAllTask();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
            ReleaseResources();
            return false;
        }
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        lock (_locker)
        {
            switch (Movement)
            {
                case 0:
                    StopMoving();
                    break;
                case 1:
                    _isRecoverAntenna = true;
                    break;
                case 2:
                    _ampldfTime = MeasureCount;
                    _isAmpdf = true;
                    break;
                case 3:
                    _isRotateAzimuth = true;
                    break;
            }
        }
    }

    public override void Stop()
    {
        StopMoving();
        base.Stop();
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        Utils.CloseSocket(_socket);
        Utils.CancelTask(_dataProcessTask, _dataProcessTokenSource);
        Utils.CancelTask(_operationCmdTask, _operationCmdTokenSource);
    }

    /// <summary>
    ///     初始化网络
    /// </summary>
    private void InitNetwork()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            SendTimeout = 10000,
            ReceiveTimeout = 10000
        };
        _socket.Connect(ep);
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitAllTask()
    {
        _operationCmdTokenSource = new CancellationTokenSource();
        _operationCmdTask = new Task(OperationCmd, _operationCmdTokenSource.Token);
        _operationCmdTask.Start();
        _dataProcessTokenSource = new CancellationTokenSource();
        _dataProcessTask = new Task(DataProcess, _dataProcessTokenSource.Token);
        _dataProcessTask.Start();
    }

    #endregion

    #region 辅助方法

    /// <summary>
    ///     向转台发送查询方位角命令、响应客户端操作线程方法
    /// </summary>
    private void OperationCmd()
    {
        while (!_operationCmdTokenSource.IsCancellationRequested)
        {
            SendCmd("PH?");
            if (_isRecoverAntenna)
            {
                RecoverAntenna();
                _isRecoverAntenna = false;
            }

            if (_isRotateAzimuth)
            {
                RotateAzimuth();
                _isRotateAzimuth = false;
            }

            if (_isAmpdf)
            {
                //转台归零
                SendCmd("PH 0");
                _isAmpdf = false;
            }

            if (_isBeginAmpdf)
            {
                SendCmd("PH 360");
                _isBeginAmpdf = false;
            }
        }
    }

    /// <summary>
    ///     数据获取及查询方法
    /// </summary>
    private void DataProcess()
    {
        while (!_dataProcessTokenSource.IsCancellationRequested)
            try
            {
                var buffer = new byte[4];
                _socket.Receive(buffer, SocketFlags.None);
                var result = Encoding.ASCII.GetString(buffer);
                var ph = float.Parse(result[..4]);
                var dataAngle = new SDataAngle
                {
                    Azimuth = ph,
                    Completed = false
                };
                //收天线1
                if (Movement == 1)
                    if (ph.Equals(0))
                        dataAngle.Completed = true;
                //如果是比幅测向2
                if (Movement == 2)
                {
                    if (ph.Equals(0) && _ampldfTime > -1)
                    {
                        _isBeginAmpdf = true;
                        _ampldfTime--;
                    }

                    if (ph.Equals(0) && _ampldfTime == -1)
                    {
                        dataAngle.Completed = true;
                        _isBeginAmpdf = false;
                    }
                }

                //如果是转动天线3
                if (Movement == 3)
                    if (ph.Equals(AzimuthAngle))
                        dataAngle.Completed = true;
                var data = new List<object> { dataAngle };
                SendData(data);
            }
            catch (Exception ex)
            {
                if (ex is SocketException)
                {
                    var info = new SDataMessage
                    {
                        LogType = LogType.Warning,
                        ErrorCode = (int)InternalMessageType.DeviceRestart,
                        Description = DeviceId.ToString(),
                        Detail = DeviceInfo.DisplayName
                    };
                    SendMessage(info);
                    break;
                }

                //如果是其他异常，继续查询
            }
    }

    ///// <summary>
    /////     查询相关参数
    ///// </summary>
    ///// <param name="cmd">查询命令</param>
    ///// <returns></returns>
    //private string SendAndReceive(string cmd)
    //{
    //    var re = string.Empty;
    //    var data = Encoding.ASCII.GetBytes(cmd + "\r\n");
    //    _socket.Send(data);
    //    Thread.Sleep(500); //延时一段时间，以便云台正确返回数据，否则云台不返数据
    //    if (cmd.Last().Equals('?'))
    //    {
    //        data = new byte[1024];
    //        _socket.Receive(data);
    //        re = Encoding.ASCII.GetString(data).Trim('\n', '\0');
    //    }

    //    return re;
    //}

    /// <summary>
    ///     设置相关参数
    /// </summary>
    /// <param name="cmd">设置命令</param>
    private void SendCmd(string cmd)
    {
        var data = Encoding.ASCII.GetBytes(cmd + "\r\n");
        _socket.Send(data);
        Thread.Sleep(500);
    }

    /// <summary>
    ///     云台停止转动
    /// </summary>
    private void StopMoving()
    {
        SendCmd("PH:STOP");
        //SendCmd("PV:STOP");//此版本不支持俯仰角
    }

    /// <summary>
    ///     收天线
    /// </summary>
    private void RecoverAntenna()
    {
        SendCmd("PH 0");
        //SendCmd("PV:000");//此版本不支持俯仰角
    }

    /// <summary>
    ///     转动方位角
    /// </summary>
    private void RotateAzimuth()
    {
        SendCmd($"PH {AzimuthAngle}");
    }

    #endregion
}