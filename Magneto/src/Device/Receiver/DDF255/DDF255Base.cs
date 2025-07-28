using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

public partial class Ddf255
{
    #region 成员变量

    /// <summary>
    ///     同步锁
    /// </summary>
    private readonly object _ctrlChannelLock = new(); // 控制通道锁

    /// <summary>
    ///     一般数据缓存
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _dataQueue = new();

    /// <summary>
    ///     DDC数据缓存
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _ddcQueue = new();

    /// <summary>
    ///     TCP PSCAN数据缓存
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _pscanQueue = new();

    /// <summary>
    ///     指令发送与查询通道（TCP）
    /// </summary>
    private Socket _ctrlChannel;

    /// <summary>
    ///     子通道指令发送与查询通道（TCP）
    /// </summary>
    private Socket _ddcCtrlChannel;

    /// <summary>
    ///     数据通道（UDP Server）
    /// </summary>
    private Socket _dataChannel;

    /// <summary>
    ///     DDC数据通道（UDP Server）
    /// </summary>
    private Socket _ddcChannel;

    private Task _dataCaptureTask;
    private CancellationTokenSource _dataCaptureCts;
    private Task _dataDispatchTask;
    private CancellationTokenSource _dataDispatchCts;
    private Task _ddcCaptureTask;
    private CancellationTokenSource _ddcCaptureCts;
    private Task _ddcDispatchTask;
    private CancellationTokenSource _ddcDispatchCts;

    /// <summary>
    ///     电平数据处理任务
    /// </summary>
    private Task _levelDataTask;

    /// <summary>
    ///     子通道数据处理任务
    /// </summary>
    private Task _ddcDataDispatchTask;

    private CancellationTokenSource _levelDataTokenSource;
    private CancellationTokenSource _ddcDataDispatchTokenSource;

    #endregion 成员变量

    #region 初始化

    private void InitResources()
    {
        Console.WriteLine($"固件版本：{FirmwareVersion}");
        ReleaseTasks();
        ReleaseNetworks();
        //初始化角度补偿
        InitAngleCompensation();
        ClearQueues();
        InitNetworks();
        Preset();
        InitTasks();
        SetHeartBeat(_ctrlChannel);
    }

    /// <summary>
    ///     初始化角度补偿
    /// </summary>
    private void InitAngleCompensation()
    {
        if (string.IsNullOrEmpty(AngleCompensation))
        {
            _angleCompensationList = new List<AngleCompensationInfo>();
            return;
        }

        if (AngleCompensation.Contains('|') || AngleCompensation.Contains(','))
        {
            _angleCompensationList.Clear();
            var array = AngleCompensation.Split('|');
            foreach (var str in array)
            {
                var freqs = str.Split(',');
                if (freqs.Length != 3) throw new Exception("天线角度补偿设置有误，请按照正确的格式设置！");
                if (!double.TryParse(freqs[0], out var start)
                    || !double.TryParse(freqs[1], out var stop)
                    || !float.TryParse(freqs[2], out var angle))
                    throw new Exception("天线角度补偿设置有误，请按照正确的格式设置！");
                var info = new AngleCompensationInfo
                {
                    StartFrequency = start,
                    StopFrequency = stop,
                    Angle = angle
                };
                _angleCompensationList.Add(info);
            }
        }
        else
        {
            if (!float.TryParse(AngleCompensation, out var angle)) throw new Exception("天线角度补偿设置有误，请按照正确的格式设置！");
            var info = new AngleCompensationInfo
            {
                StartFrequency = double.MinValue,
                StopFrequency = double.MaxValue,
                Angle = angle
            };
            _angleCompensationList.Add(info);
        }
    }

    private void InitNetworks()
    {
        _ctrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            ReceiveTimeout = 5000 // 避免存在查询操作，造成套接字永久等待
        };
        _ctrlChannel.Connect(Ip, Port);
        if (_ctrlChannel.LocalEndPoint is not IPEndPoint endPoint) throw new Exception("无可用网络地址");
        _ddcCtrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            ReceiveTimeout = 5000
        };
        _ddcCtrlChannel.Connect(Ip, Port);
        _dataChannel = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _dataChannel.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _dataChannel.Bind(new IPEndPoint(endPoint.Address, 0));
        _dataChannel.Connect(Ip, 0);
        _ddcChannel = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _ddcChannel.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _ddcChannel.Bind(new IPEndPoint(endPoint.Address, 0));
        _ddcChannel.Connect(Ip, 0);
    }

    private void InitTasks()
    {
        // 数据回传通道与队列
        _levelDataTokenSource = new CancellationTokenSource();
        _levelDataTask = new Task(DispatchLevelData, _levelDataTokenSource.Token);
        _levelDataTask.Start();
        _ddcDataDispatchTokenSource = new CancellationTokenSource();
        _ddcDataDispatchTask = new Task(DispatchDdcData, _ddcDataDispatchTokenSource.Token);
        _ddcDataDispatchTask.Start();
        _dataCaptureCts = new CancellationTokenSource();
        _dataCaptureTask = new Task(CaptureDataPacket, _dataCaptureCts.Token);
        _dataCaptureTask.Start();
        _dataDispatchCts = new CancellationTokenSource();
        _dataDispatchTask = new Task(DispatchDataPacket, _dataDispatchCts.Token);
        _dataDispatchTask.Start();
        _ddcCaptureCts = new CancellationTokenSource();
        _ddcCaptureTask = new Task(CaptureDdcPacket, _ddcCaptureCts.Token);
        _ddcCaptureTask.Start();
        _ddcDispatchCts = new CancellationTokenSource();
        _ddcDispatchTask = new Task(DispatchDdcPacket, _ddcDispatchCts.Token);
        _ddcDispatchTask.Start();
    }

    private void InitPath()
    {
        if (_media == MediaType.None) return;
        ClosePath();
        OpenPath();
        if ((_media & MediaType.Scan) == 0)
        {
            if ((_media & MediaType.Itu) > 0)
            {
                SendCmd("FUNC:CONC ON");
                SendCmd(
                    "FUNC \"VOLT:AC\", \"AM\", \"AM:POS\", \"AM:NEG\", \"FM\", \"FM:POS\", \"FM:NEG\", \"PM\", \"BAND\"");
            }
            else
            {
                SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            }

            SendCmd("FREQ:MODE FIX");
            if (CurFeature == FeatureType.IFMCA) SetIfmch(_ddcChannels);
        }
        else
        {
            SendCmd("FUNC:CONC OFF;:FUNC \"VOLT:AC\"");
            StartScan();
        }
    }

    /// <summary>
    ///     初始化设备
    /// </summary>
    private void Preset()
    {
        //查询设备所有选件
        //初始化接收机恢复默认状态
        SendCmd("*RST");
        ////由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("OUTP:TONE OFF");
        SendCmd("FORM ASC"); //定义二进制方式,制动高字节在高位;:FORM:BORD SWAP
        SendCmd("FORM:BORD SWAP"); //SWAP
        SendCmd("SYST:IF:REM:MODE SHORT");
        SendCmd("SYST:AUD:REM:MOD 2"); //PCM 32k,16bit,2ch 
        SendCmd("SENS:FREQ:AFC OFF"); //不使用自动频率控制
        SendCmd("DIAG:INF:PER"); //文档245页，启动对新连接设备的搜索
        //初始化子通道
        InitDdcPath();
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
        ClearQueues();
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_ctrlChannel);
        Utils.CloseSocket(_ddcCtrlChannel);
        Utils.CloseSocket(_ddcChannel);
        Utils.CloseSocket(_dataChannel);
    }

    private void ClearQueues()
    {
        _dataQueue.Clear();
        _ddcQueue.Clear();
        _pscanQueue.Clear();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_levelDataTask, _levelDataTokenSource);
        Utils.CancelTask(_ddcDataDispatchTask, _ddcDataDispatchTokenSource);
        Utils.CancelTask(_ddcCaptureTask, _ddcCaptureCts);
        Utils.CancelTask(_ddcDispatchTask, _ddcDispatchCts);
        Utils.CancelTask(_dataCaptureTask, _dataCaptureCts);
        Utils.CancelTask(_dataDispatchTask, _dataDispatchCts);
    }

    #endregion

    #region 开始任务

    private void StartTask()
    {
        SetDataByAbility();
        ClearQueues();
        InitPath();
    }

    private void SetDataByAbility()
    {
        if ((CurFeature & (FeatureType.FFDF | FeatureType.WBDF)) > 0)
        {
            //设置为测向机模式
            SendCmd("MEAS:APPL DF");
            //关闭步进自动
            SendCmd("CALC:IFP:STEP:AUTO OFF");
            if (CurFeature == FeatureType.WBDF)
            {
                SetWbSpan(_ifBandwidth);
                SendCmd($"CALCulate:IFPan:STEP {_channelBandWidth} kHz");
            }

            if (DfAntenna >= 0)
                SendCmd("ROUTe:VUHF (@" + DfAntenna + ")");
            else if (DfAntenna == -1)
                SendCmd("ROUTe:AUTO ON");
            else
                SendCmd("ROUTe:AUTO OFF");
            if (HfAntenna >= 0) SendCmd("ROUTe:HF (@" + HfAntenna + ")");
            // 宽带测向必须包含频谱数据
            _media = (FeatureType.FFDF & CurFeature) > 0
                ? _media |= MediaType.Dfind
                : _media |= MediaType.Dfpan | MediaType.Spectrum;
        }
        else
        {
            //设置为接收机模式
            SendCmd("MEAS:APPL RX");
            //打开步进自动以便频谱图最直观展示
            SendCmd("CALC:IFP:STEP:AUTO ON");
            if (MonitorAntenna >= 0)
                SendCmd("ROUTe:VUHF (@" + MonitorAntenna + ")");
            else if (MonitorAntenna == -1) SendCmd("ROUTe:AUTO ON");
            if (HfAntenna >= 0) SendCmd("ROUTe:HF (@" + HfAntenna + ")");
            if (CurFeature.Equals(FeatureType.FFM) || CurFeature.Equals(FeatureType.AmpDF))
            {
                _media |= MediaType.Level;
            }
            else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne | FeatureType.SCAN | FeatureType.FScne)) > 0)
            {
                if (CurFeature.Equals(FeatureType.MScan) || CurFeature.Equals(FeatureType.MScne))
                {
                    ScanMode = ScanMode.MScan;
                    if (CurFeature == FeatureType.MScne)
                    {
                        if (_audioSwitch)
                            _media |= MediaType.Audio;
                        else
                            _media &= ~MediaType.Audio;
                        if (_spectrumSwitch)
                            _media |= MediaType.Spectrum;
                        else
                            _media &= ~MediaType.Spectrum;
                    }
                    else
                    {
                        _media &= ~MediaType.Audio;
                        _media &= ~MediaType.Spectrum;
                    }
                }
                else if (CurFeature == FeatureType.FScne)
                {
                    ScanMode = ScanMode.Fscan;
                }

                _media |= MediaType.Scan;
            }
            else if (CurFeature == FeatureType.IFMCA)
            {
                _preChannels = null;
                _media |= MediaType.Spectrum;
            }
        }
    }

    /// <summary>
    ///     打通各数据通道
    /// </summary>
    private void OpenPath()
    {
        var address = (_ctrlChannel.LocalEndPoint as IPEndPoint)?.Address.ToString();
        var connType = "UDP";
        var dataPort = (_dataChannel.LocalEndPoint as IPEndPoint)?.Port.ToString();
        string tag = null;
        if ((_media & MediaType.Iq) > 0) tag += "IF,";
        if ((_media & MediaType.Spectrum) > 0) tag += "IFP,";
        if ((_media & MediaType.Audio) > 0) tag += "AUD,";
        if ((_media & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_media & (MediaType.Dfind | MediaType.Dfpan)) > 0) tag += "DFP,";
        if (tag == null) return;
        tag = tag.Remove(tag.Length - 1);
        SendCmd($"TRAC:{connType}:TAG:ON \"{address}\",{dataPort},{tag}");
        Thread.Sleep(10);
        if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Pscan)
        {
            if (FirmwareVersion == FirmwareVersion.OldSubscribe)
                SendCmd(
                    $"TRAC:{connType}:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\",\"FREQ:HIGH:RX\"");
            else
                SendCmd($"TRAC:{connType}:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"VOLT:AC\"");
        }
        else if (CurFeature is FeatureType.FFDF or FeatureType.WBDF)
        {
            SendCmd($"TRAC:{connType}:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"AZIM\",\"DFQ\",\"DFL\"");
        }
        else
        {
            SendCmd(
                $"TRAC:{connType}:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\",\"FREQ:HIGH:RX\"");
        }

        Thread.Sleep(10);
    }

    #endregion

    #region 停止任务

    private void StopTask()
    {
        SendCmd("ABORT");
        ClosePath();
        _media = MediaType.None;
        ResetDdcPath();
        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
        ClearQueues();
    }

    /// <summary>
    ///     删除数据通道
    /// </summary>
    private void ClosePath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Task.Delay(10).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="cmd">命令</param>
    private void SendCmd(string cmd)
    {
        Console.WriteLine($"==> {cmd}");
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        lock (_ctrlChannelLock)
        {
            try
            {
                _ctrlChannel.Send(buffer);
            }
            catch (SocketException)
            {
            }
        }
    }

    /// <summary>
    ///     发送命令并检查设置命令结果
    /// </summary>
    /// <param name="cmd"></param>
    private string SendSyncCmd(string cmd)
    {
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        var result = string.Empty;
        lock (_ctrlChannelLock)
        {
            _ctrlChannel.Send(buffer);
            result = RecvResult('\n');
        }

        return result;
    }

    private string RecvResult(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        while (_ctrlChannel.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    #endregion
}

internal class TaskParam
{
    public string Name { get; set; }
    public string Tag { get; set; }
    public bool IsIfmch { get; set; } = false;
    public CancellationToken Token { get; set; }
}