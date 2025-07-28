using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.PR100;

public partial class Pr100
{
    /// <summary>
    ///     避免该方法被调用时频繁申请内存
    /// </summary>
    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024];

    private void InitResources()
    {
        ReleaseResources();
        InitNetworks();
        InitDevice();
        InitTasks();
        SetHeartBeat(_cmdSock);
    }

    /// <summary>
    ///     初始化网络连接
    /// </summary>
    private void InitNetworks()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _cmdSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _cmdSock.Connect(ep);
        _cmdSock.NoDelay = true;
        if (_cmdSock.LocalEndPoint is not IPEndPoint endPoint) throw new Exception("无可用网络地址");
        ep = new IPEndPoint(endPoint.Address, 0);
        _dataSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSock.Bind(ep);
    }

    /// <summary>
    ///     设备功能，参数初始化
    /// </summary>
    private void InitDevice()
    {
        //初始化接收机恢复默认状态
        SendCmd("*RST");
        //由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("OUTP:TONE OFF");
        SendCmd("FORM ASC"); //定义二进制方式,制动高字节在高位;:FORM:BORD SWAP
        SendCmd("FORM:BORD SWAP"); //SWAP
        SendCmd("SYST:AUD:REM:MOD 2"); //PCM 32k,16bit,1ch 
        SendCmd("SENS:FREQ:AFC OFF"); //不使用自动频率控制
        SendCmd("MEAS:TIME DEF"); //测量时间为默认
        SendCmd("MSC:CONT \"STOP:SIGN\""); //驻留离散扫描设置等待时间时间时需要
    }

    /// <summary>
    ///     清理所有非托管资源
    /// </summary>
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
        CloseSocket(_dataSock);
        CloseSocket(_cmdSock);
    }

    private void CloseSocket(Socket socket)
    {
        socket?.Close();
    }

    /// <summary>
    ///     初始化所有线程
    /// </summary>
    private void InitTasks()
    {
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
            Tag = "level",
            Token = _udpDataCaptureTokenSource.Token
        }, _udpDataCaptureTokenSource.Token);
        _udpDataCaptureTask.Start();
        _udpDataDispatchTokenSource = new CancellationTokenSource();
        _udpDataDispatchTask = new Task(DispatchPacket, new TaskParam
        {
            Name = "data_dispatch",
            Tag = "level",
            Token = _udpDataDispatchTokenSource.Token
        }, _udpDataDispatchTokenSource.Token);
        _udpDataDispatchTask.Start();
    }

    private void InitUdpPath()
    {
        if (_mediaType == MediaType.None) return;
        //由于单频测量可以在任务运行过程中更改参数，所以此处需要先删除之前的UDP通道
        if (CurFeature == FeatureType.FFM) CloseUdpPath();
        OpenUdpPath();
        if ((_mediaType & MediaType.Scan) == 0)
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            SendCmd("FREQ:MODE FIX");
        }
        else
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
        SendCmd("TRAC:UDP:DEF:DEL ALL");
        Thread.Sleep(10);
    }

    private void OpenUdpPath()
    {
        var localAddr = (_cmdSock.LocalEndPoint as IPEndPoint)?.Address.ToString(); //本地连接设备的ip
        var localUdpPort = (_dataSock.LocalEndPoint as IPEndPoint)?.Port ?? 0;
        string tag = null;
        if ((_mediaType & MediaType.Audio) > 0) tag += "AUD,";
        if ((_mediaType & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_mediaType & MediaType.Spectrum) > 0) tag += "IFP,";
        if ((_mediaType & MediaType.Iq) > 0) tag += "IF,";
        if (tag == null) return;
        tag = tag.Remove(tag.Length - 1);
        SendCmd($"TRAC:UDP:TAG:ON \"{localAddr}\",{localUdpPort},{tag}");
        if (tag.Split(',').ToList().Contains("IF"))
            Thread.Sleep(50);
        else
            Thread.Sleep(10);
        SendCmd($"TRAC:UDP:FLAG:ON \"{localAddr}\",{localUdpPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\"");
        Thread.Sleep(10);
    }

    /// <summary>
    ///     向设备发送命令
    /// </summary>
    /// <param name="cmd">SCPI命令</param>
    private void SendCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        lock (_ctrlLocker)
        {
            _cmdSock.Send(buffer);
        }
    }

    /// <summary>
    ///     用于发送查询类指令并获取查询结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>查询结果</returns>
    private string SendSyncCmd(string cmd)
    {
        var sendBuffer = Encoding.ASCII.GetBytes(cmd + "\n");
        var result = string.Empty;
        int recvCount;
        lock (_ctrlLocker)
        {
            _cmdSock.Send(sendBuffer);
            recvCount = _cmdSock.Receive(_tcpRecvBuffer, SocketFlags.None);
        }

        if (recvCount > 0)
        {
            if (_tcpRecvBuffer[recvCount - 1] == '\n')
                result = Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount - 1);
            else
                result = Encoding.ASCII.GetString(_tcpRecvBuffer, 0, recvCount);
        }

        return result;
    }
}

internal class TaskParam
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public CancellationToken Token { get; set; }
}