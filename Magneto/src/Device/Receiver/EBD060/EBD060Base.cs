using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.EBD060;

public partial class Ebd060
{
    #region 释放资源

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResources()
    {
        Utils.CancelTask(_dataCaptureTask, _dataCaptureTokenSource);
        Utils.CancelTask(_dataProcessTask, _dataProcessTokenSource);
        Utils.CloseSocket(_tcpSocket);
        try
        {
            _dataQueue.Dispose();
        }
        catch
        {
        }

        _dataQueue = null;
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     向设备发送命令
    /// </summary>
    /// <param name="cmd"></param>
    private void SendCmd(string cmd)
    {
        if (_tcpSocket == null) return;
        cmd += "\n";
        var command = Encoding.Default.GetBytes(cmd);
        var content = new byte[command.Length + 4];
        var offset = 0;
        Buffer.BlockCopy(BitConverter.GetBytes((short)command.Length), 0, content, offset, 2);
        offset += 2;
        Buffer.BlockCopy(BitConverter.GetBytes((short)0x01), 0, content, offset, 2);
        offset += 2;
        Buffer.BlockCopy(command, 0, content, offset, command.Length);
        var modifier = new byte[] { 0x22 };
        var ticks = BitConverter.GetBytes(Environment.TickCount);
        var type = new byte[] { 0x06, 0x00 };
        var total = BitConverter.GetBytes((short)content.Length);
        var end = new byte[] { 0x00, 0x00 };
        var send = new byte[11 + content.Length];
        offset = 0;
        Buffer.BlockCopy(modifier, 0, send, offset, 1);
        offset += 1;
        Buffer.BlockCopy(ticks, 0, send, offset, 4);
        offset += 4;
        Buffer.BlockCopy(type, 0, send, offset, 2);
        offset += 2;
        Buffer.BlockCopy(total, 0, send, offset, 2);
        offset += 2;
        Buffer.BlockCopy(content, 0, send, offset, content.Length);
        offset += content.Length;
        Buffer.BlockCopy(end, 0, send, offset, 2);
        _tcpSocket.Send(send);
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化设备网络连接
    /// </summary>
    private void InitNetWork()
    {
        InitFirstConnectionAndRetrieceFeedback(out var feedback);
        if (feedback == null) throw new NullReferenceException("打开设备错误:无效的端口号!");
        _sencodPort = BitConverter.ToInt32(feedback, 0);
        _sencodPort = IPAddress.NetworkToHostOrder((short)_sencodPort);
        _comPointer = BitConverter.ToInt32(feedback, 4);
        if (_sencodPort == 0) throw new Exception("打开设备错误!");
        InitSecondConnection();
    }

    /// <summary>
    ///     初始化第一个连接，发送设备ID并返回建立第二个连接时应具备的信息（包括套接字的端口号）
    /// </summary>
    /// <param name="feedback"></param>
    private void InitFirstConnectionAndRetrieceFeedback(out byte[] feedback)
    {
        feedback = null;
        // 与第一个端口建立连接并进行验证
        try
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Select(new List<Socket> { socket }, null, null, 5000);
            socket.Connect(Ip, Port);
            // 设置需要连接的设备ID
            var deviceGuid = Guid.Parse("91b66af0-c280-11ce-af27-008029835a90");
            var deviceGuidBytes = deviceGuid.ToByteArray();
            //发送设备ID进行验证
            socket.Send(deviceGuidBytes);
            feedback = new byte[8];
            var offset = 0;
            var remainedBytes = feedback.Length;
            while (remainedBytes > 0)
            {
                var readBytes = socket.Receive(feedback, offset, remainedBytes, SocketFlags.None);
                remainedBytes -= readBytes;
                offset += readBytes;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EBD060初始化第一个连接失败:" + ex);
        }
    }

    /// <summary>
    ///     初始化第二个连接，用于接收业务数据
    /// </summary>
    private void InitSecondConnection()
    {
        try
        {
            var ep = new IPEndPoint(IPAddress.Parse(Ip), _sencodPort);
            _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _tcpSocket.Connect(ep);
            _tcpSocket.NoDelay = true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EBD060初始化第二个连接失败:" + ex);
        }
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void InitDevice()
    {
        // 与第二个端口建立连接（用于接收业务数据）
        try
        {
            var devPointerBytes = BitConverter.GetBytes(_comPointer);
            var feedBackTag = new byte[] { 0x20 };
            // 首先发送服务地址
            _tcpSocket.Send(devPointerBytes);
            // 然后发送收数据标识
            _tcpSocket.Send(feedBackTag);
            // 最后获取反馈结果
            _tcpSocket.Receive(feedBackTag, 0, 1, SocketFlags.None);
            var occupied = -1;
            var occupiedTag = new byte[4];
            // 获取设备是否已被占用
            _tcpSocket.Receive(occupiedTag, 0, 4, SocketFlags.None);
            occupied = BitConverter.ToInt32(occupiedTag, 0);
            if (occupied != 0) throw new Exception("设备已被占用");
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EBD060初始化设备失败:" + ex);
        }
    }

    /// <summary>
    ///     初始化连接通道和数据通道
    /// </summary>
    private void InitDataChannle()
    {
        //连接及数据通道的初始化操作
        //经多次实验发现LD_DDF和objectStarter启动后并未对设备进行任何检查及初始化操作，
        //反而是在与之连接的服务端退出时会促发设备的初始化操作，且初始化时间较长（大于35秒），
        //而且设备参数的设置必须是在初始完成之后才可以，否则设备会出错不会返回任何数据，
        //因此服务端在第一次连接时，并未发生任何初始化动作，所以不会有任何数据返回，
        //故此处处理为连接成功后再断连，等待40秒（设备初始化）后再重连的方式
        try
        {
            InitNetWork();
            InitDevice();
            ReleaseResources();
            Thread.Sleep(40000);
            InitNetWork();
            InitDevice();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EBD060初始化通道失败:" + ex);
        }
    }

    /// <summary>
    ///     预设置参数，为部分参数设置为固定值
    /// </summary>
    private void Preset()
    {
        Thread.Sleep(1000); //设备启动需要时间较长
        SendCmd("qu 1");
        Thread.Sleep(100);
        SendCmd("ift 100ms"); //必须显示设置此参数否则频谱数据不返回
        Thread.Sleep(100);
        SendCmd("at 0ms");
        Thread.Sleep(100);
        SendCmd("compass nmea off");
    }

    /// <summary>
    ///     初始化所有线程(数据采集线程、数据加工线程)
    /// </summary>
    private void InitAllThread()
    {
        _dataQueue = new MQueue<byte[]>();
        _dataCaptureTokenSource = new CancellationTokenSource();
        _dataCaptureTask = new Task(CaptureData, _dataCaptureTokenSource.Token);
        _dataCaptureTask.Start();
        _dataProcessTokenSource = new CancellationTokenSource();
        _dataProcessTask = new Task(ProcessData, _dataProcessTokenSource.Token);
        _dataProcessTask.Start();
    }

    #endregion
}