using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.EB200;

public partial class Eb200
{
    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_tcpDataTask, _tcpDataTokenSource);
        Utils.CancelTask(_udpDataCaptureTask, _udpDataCaptureTokenSource);
        Utils.CancelTask(_udpDataDispatchTask, _udpDataDispatchTokenSource);
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_dataSocket);
        lock (_ctrlLocker)
        {
            Utils.CloseSocket(_cmdSocket);
        }
    }

    #endregion

    #region 初始化

    /// <summary>
    ///     初始化
    /// </summary>
    private void InitDevice()
    {
        //初始化接收机恢复默认状态
        SendCmd("*RST");
        //由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("OUTP:TONE OFF");
        SendCmd("FORM ASC"); //定义二进制方式,制动高字节在高位;:FORM:BORD SWAP
        SendCmd("FORM:BORD NORM"); //SWAP
        SendCmd("SYST:AUD:REM:MOD 12"); //PCM 32k,16bit,1ch 
        SendCmd("SENS:FREQ:AFC OFF"); //不使用自动频率控制
        SendCmd("MEAS:TIME DEF"); //测量时间为默认
        SendCmd("MSC:CONT \"STOP:SIGN\""); //驻留离散扫描设置等待时间时间时需要
        _udpDataQueue = new ConcurrentQueue<byte[]>();
    }

    /// <summary>
    ///     初始化socket
    /// </summary>
    private void InitNetWork()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        lock (_ctrlLocker)
        {
            _cmdSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        _cmdSocket.Connect(ep);
        _cmdSocket.NoDelay = true;
        var udpAddr = (_cmdSocket.LocalEndPoint as IPEndPoint)?.Address;
        if (udpAddr != null) ep = new IPEndPoint(udpAddr, 0);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSocket.Bind(ep);
    }

    /// <summary>
    ///     初始化线程
    /// </summary>
    private void InitTasks()
    {
        _udpDataQueue = new ConcurrentQueue<byte[]>();
        _tcpDataTokenSource = new CancellationTokenSource();
        _tcpDataTask = new Task(DispatchLevelData, new TaskParam
        {
            Name = "level_dispatch",
            Tag = "level",
            Token = _tcpDataTokenSource.Token
        }, _tcpDataTokenSource.Token);
        _tcpDataTask.Start();
        _udpDataCaptureTokenSource = new CancellationTokenSource();
        _udpDataCaptureTask = new Task(CapturePacket, new TaskParam
        {
            Name = "data_capture",
            Tag = "data",
            Token = _udpDataCaptureTokenSource.Token
        }, _udpDataCaptureTokenSource.Token);
        _udpDataCaptureTask.Start();
        _udpDataDispatchTokenSource = new CancellationTokenSource();
        _udpDataDispatchTask = new Task(DispatchPacket, new TaskParam
        {
            Name = "data_dispatch",
            Tag = "data",
            Token = _udpDataDispatchTokenSource.Token
        }, _udpDataDispatchTokenSource.Token);
        _udpDataDispatchTask.Start();
    }

    #endregion

    #region 指令发送

    private void SendCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        lock (_ctrlLocker)
        {
            _cmdSocket.Send(buffer);
        }
    }

    /// <summary>
    ///     用于发送查询类指令并获取结果，加锁是为了同步查询指令，以避免得到其它线程查询的结果
    /// </summary>
    /// <param name="cmd"></param>
    private string SendSyncCmd(string cmd)
    {
        var sendBuffer = Encoding.ASCII.GetBytes(cmd + "\n");
        var result = string.Empty;
        int recvCount;
        lock (_ctrlLocker)
        {
            _cmdSocket.Send(sendBuffer);
            recvCount = _cmdSocket.Receive(_tcpRecvBuffer, SocketFlags.None);
        }

        if (recvCount > 0)
            result = _tcpRecvBuffer[recvCount - 1] == '\n'
                ? Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount - 1)
                : Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount);

        return result;
    }

    #endregion
}

internal class TaskParam
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public CancellationToken Token { get; set; }
}