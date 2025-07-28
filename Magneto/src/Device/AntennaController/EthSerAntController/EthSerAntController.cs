/*********************************************************************************************
 *
 * 文件名称:		..\Tracker800 V9\Server\Source\Device\AntennaController\EthernetSerialPortAntennaController\EthernetSerialPortAntennaController.cs
 *
 * 作    者:		蒋波
 *
 * 创作日期:		2022-05-09
 *
 * 修    改:		无
 *
 * 备    注:		网口串口级联天线控制器，使用TCP套接字和串口进行通信的的天线控制器，用于同时控制两个天线控制器。
 *
 *********************************************************************************************/

using System;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Tracker800.Server.Device;

/// <summary>
///     网口和串口级联天线控制器，包含设备网络连接、网络设备心跳等特性
///     具备断网重连等特性
/// </summary>
public partial class EthSerAntController : AntennaControllerBase
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="id">模块ID</param>
    public EthSerAntController(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    private Socket _socket; // 控制流网络套接字
    private SerialPort _serialPort; // 串口
    private Task _heartBeat;

    private CancellationTokenSource _tokenSource;

    // 网口天线控制器控制流对象，TCP套接字
    private Stream _ethernetStream;

    // 网口天线控制器字节写入流对象
    private BinaryWriter _ethernetWriter;

    // 串口天线控制器控制流对象，串口
    private Stream _serialPortStream;

    // 串口天线控制器字节写入流对象
    private BinaryWriter _serialPortWriter;

    #endregion

    #region 重写基类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        try
        {
            // 必需先初始化基类，否则可能出现意想不到的情况
            var result = base.Initialized(moduleInfo);
            if (!result) return false;
            InitNetworks();
            InitSerialPort();
            InitHeartBeat();
            return true;
        }
        catch
        {
            Dispose();
            return false;
        }
    }

    //发送控制码，码值配置成A_B，A为网口控制码值，B为串口控制器码
    public override bool SendControlCode(string code)
    {
        try
        {
            var tempCodes = code.Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (tempCodes.Length == 2)
            {
                //发送网口天线切换器天线码
                SendEthernetControllerCode(tempCodes[0]);
                //发送外串口切换器天线码
                SendSerialPortControllerCode(tempCodes[1]);
            }
            else
            {
                _ = $"天线码个数{tempCodes.Length}";
                throw new FormatException($"天线码值配置有误,请检查！天线码个数{tempCodes.Length}");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    /// <summary>
    ///     发送网口天线控制器指令
    /// </summary>
    /// <param name="code"></param>
    private bool SendEthernetControllerCode(string code)
    {
        if (_ethernetStream == null)
        {
            if (_socket == null) return false;
            _ethernetStream = new NetworkStream(_socket);
        }

        if (_ethernetStream != null && _ethernetWriter == null) _ethernetWriter = new BinaryWriter(_ethernetStream);
        if (_ethernetWriter == null || string.IsNullOrEmpty(code)) return false;
        //发送天线码
        SendAntCode(_ethernetWriter, code);
        return true;
    }

    /// <summary>
    ///     发送串口天线控制器指令
    /// </summary>
    /// <param name="code"></param>
    private bool SendSerialPortControllerCode(string code)
    {
        if (_serialPortStream == null)
        {
            if (_serialPort == null) return false;
            _serialPortStream = _serialPort.BaseStream;
        }

        if (_serialPortStream != null && _serialPortWriter == null)
            _serialPortWriter = new BinaryWriter(_serialPortStream);
        if (_serialPortWriter == null || string.IsNullOrEmpty(code)) return false;
        //发送天线码
        SendAntCode(_serialPortWriter, code);
        return true;
    }

    #endregion

    #region 资源释放

    // 释放资源
    public override void Dispose()
    {
        ReleaseHeatBeat();
        ReleaseSocket();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    // 停止线程
    private void ReleaseHeatBeat()
    {
        if (_heartBeat == null || _tokenSource == null) return;
        try
        {
            _tokenSource?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
        catch (AggregateException)
        {
        }
        finally
        {
            _tokenSource?.Dispose();
        }
    }

    // 关闭套接字
    private void ReleaseSocket()
    {
        if (_socket != null)
        {
            _socket.Close();
            _socket = null;
        }
    }

    // 关闭串口
    private void ReleaseSerialPort()
    {
        if (_serialPort != null)
        {
            _serialPort.Close();
            _serialPort = null;
        }
    }

    #endregion

    #region 辅助方法

    // 初始化网络
    private void InitNetworks()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;
        // 设置TCP心跳选项，用于检测物理网络连接状态
        // var bytes = new byte[12];
        // BitConverter.GetBytes((int)1).CopyTo(bytes, 0);
        // BitConverter.GetBytes((int)1000).CopyTo(bytes, 4);
        // BitConverter.GetBytes((int)500).CopyTo(bytes, 8);
        // _socket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
        _socket.Connect(Ip, Port);
    }

    // 初始化串口
    private void InitSerialPort()
    {
        _serialPort = new SerialPort(Com, BaudRate);
        _serialPort.Open();
    }

    // 初始化心跳线程
    private void InitHeartBeat()
    {
        if (_heartBeat?.IsCompleted == false) return;
        _tokenSource = new CancellationTokenSource();
        _heartBeat = new Task(KeepAlive, _tokenSource.Token);
        _heartBeat.Start();
    }

    // 心跳线程
    private void KeepAlive()
    {
        try
        {
            while (true)
            {
                //判断Socket连接
                Thread.Sleep(5000);
                if (!IsSocketConnected()) throw new Exception("设备已离线");
                //判断串口是否打开
                if (_serialPort is { IsOpen: true })
                {
                    Thread.Sleep(1000);
                    continue;
                }

                throw new Exception("串口未打开或已关闭");
            }
        }
        catch (Exception ex)
        {
            ReleaseSocket();
            ReleaseSerialPort();
            if (ex is ThreadAbortException) return;
            // 发送设备需要重启（重新初始化）的消息
            SendMessage(new SDataMessage
            {
                LogType = LogType.Error,
                Description = "设备重连异常"
            });
        }
    }

    // 仅适用于物理连接正常情况下的网络网状检查，实际网络连接情况，需要配合TCP心跳包一起使用（检测物理连接状态是否异常）
    private bool IsSocketConnected(int maxRetry = 3)
    {
        if (_socket == null || maxRetry == 0) return false;
        var localEndPoint = _socket.LocalEndPoint?.ToString();
        var remoteEndPoint = _socket.RemoteEndPoint?.ToString();
        var validConnection = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
            .FirstOrDefault(item =>
                item.LocalEndPoint.ToString().Equals(localEndPoint) &&
                item.RemoteEndPoint.ToString().Equals(remoteEndPoint));
        if (validConnection != null) return validConnection.State == TcpState.Established;
        Thread.Sleep(2000);
        return IsSocketConnected(--maxRetry);
    }

    /// <summary>
    ///     发送天线码
    /// </summary>
    /// <param name="receiverWriter"></param>
    /// <param name="code">以“|”分隔的十六进制字节码</param>
    /// <returns>成功返回True，否则返回False</returns>
    private bool SendAntCode(BinaryWriter receiverWriter, string code)
    {
        var pattern = @"^((0[xX])?[0-9A-Fa-f]{1,2}\|)*(0[xX])?[0-9A-Fa-f]{1,2}$"; // 只匹配以“|”分隔的16进制数据，已考虑大小写情况
        var reg = new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);
        if (!reg.IsMatch(code)) throw new FormatException("天线控制码格式有误");
        if (receiverWriter == null || string.IsNullOrEmpty(code)) return false;
        var codeArray = code.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        var commands = new byte[codeArray.Length];
        for (var index = 0; index < codeArray.Length; ++index)
            commands[index] = Convert.ToByte(codeArray[index], 16); // 无论字符串是否带有“0x”前缀，都可以转换为对应的16进制数据
        receiverWriter.Write(commands, 0, commands.Length);
        receiverWriter.Flush();
        return true;
    }

    #endregion
}