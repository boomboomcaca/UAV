using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB500;

public partial class Eb500
{
    private void StartTask()
    {
        if (CurFeature == FeatureType.MScne)
        {
            if (_audioSwitch)
                _mediaType |= MediaType.Audio;
            else
                _mediaType &= ~MediaType.Audio;
            if (_spectrumSwitch)
                _mediaType |= MediaType.Spectrum;
            else
                _mediaType &= ~MediaType.Spectrum;
        }
        else if (CurFeature == FeatureType.MScan)
        {
            _mediaType &= ~MediaType.Audio;
            _mediaType &= ~MediaType.Spectrum;
        }

        _offsetPscan = 0;
        _flag = false;
        if (CurFeature.Equals(FeatureType.FFM))
        {
            _mediaType |= MediaType.Level;
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
                ScanMode = ScanMode.MScan;
            else if (CurFeature == FeatureType.FScne) ScanMode = ScanMode.Fscan;
            _mediaType |= MediaType.Scan;
        }

        _udpDataQueue.Clear();
        SendMediaRequest();
    }

    private void StopTask()
    {
        _offsetPscan = 0;
        _lastFreqHigh = uint.MaxValue;
        _mediaType = MediaType.None;
        _scanFreqs.Clear();
        SendCmd("ABORT");
        CloseUdpPath();
        Thread.Sleep(50);
        _udpDataQueue.Clear();
    }

    #region 初始化

    /// <summary>
    ///     初始化发送命令和接收数据网络连接
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
        if (_cmdSocket.LocalEndPoint is not IPEndPoint endPoint) throw new Exception("无可用网络地址");
        ep = new IPEndPoint(endPoint.Address, 0);
        _dataSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataSocket.Bind(ep);
    }

    /// <summary>
    ///     在设备开始功能前初始化相关参数
    /// </summary>
    private void InitDevice()
    {
        //查询当前设备是否具有ITU选件
        var result = SendSyncCmd("*OPT?");
        var options = result.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        _ituOption = options.Contains("IM");
        //将接收机恢复为默认状态
        SendCmd("*RST");
        //出于性能考虑，将接收机的外置播放器音量设为0，但此参数不影响客户端音量 
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        //设置EB500接收数据为ASCII形式
        SendCmd("FORM ASC");
        SendCmd("FORM:BORD SWAP"); //默认为NORM
        //设置数字音频模式Mode=2;Sampling rate:32;Bits Per Sample:16;Channels:1;Data Rate:64;Length Per Frame:2 
        SendCmd("SYST:AUD:REM:MOD 2");
        //设置数字中频模式 16bit I/16bit Q
        SendCmd("SYSTem:IF:REMote:MODe SHORT");
        SendCmd("FREQ:SPAN 200 kHz");
    }

    /// <summary>
    ///     初始化相关线程
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
        CloseSocket(_dataSocket);
        CloseSocket(_cmdSocket);
    }

    private void CloseSocket(Socket socket)
    {
        socket?.Close();
    }

    #endregion

    #region 仪表命令

    /// <summary>
    ///     向设备发送命令
    /// </summary>
    /// <param name="cmd">SCPI命令</param>
    private void SendCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        lock (_ctrlLocker)
        {
            _cmdSocket.Send(buffer);
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