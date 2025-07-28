using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

public partial class Esmd
{
    #region 成员变量

    private readonly string _configPath = @"Devices\ESMD.ini";
    private readonly string[] _identifiers = { "data", "ddc" };

    /// <summary>
    ///     同步锁
    /// </summary>
    private readonly object _ctrlChannelLock = new(); // 控制通道锁

    /// <summary>
    ///     指令发送与查询通道（TCP）
    /// </summary>
    private Socket _ctrlChannel;

    /// <summary>
    ///     子通道指令发送与查询通道（TCP）
    /// </summary>
    private Socket _ddcCtrlChannel;

    /// <summary>
    ///     TDOA数据通道
    /// </summary>
    private Socket _tdoaDataChannel;

    private ConcurrentDictionary<string, Socket> _channels;
    private Dictionary<string, (Task task, CancellationTokenSource tokenSource)> _captures;
    private Dictionary<string, (Task task, CancellationTokenSource tokenSource)> _dispatches;

    /// <summary>
    ///     数据队列集合
    /// </summary>
    private ConcurrentDictionary<string, ConcurrentQueue<byte[]>> _queues;

    /// <summary>
    ///     电平数据处理任务
    /// </summary>
    private Task _levelDataTask;

    /// <summary>
    ///     IQ数据处理任务
    /// </summary>
    private Task _tdoaDataCaptureTask;

    /// <summary>
    ///     TDOA数据处理任务
    /// </summary>
    private Task _tdoaDataDispatchTask;

    /// <summary>
    ///     子通道数据处理任务
    /// </summary>
    private Task _ddcDataTask;

    private CancellationTokenSource _levelDataTokenSource;
    private CancellationTokenSource _tdoaDataCaptureTokenSource;
    private CancellationTokenSource _tdoaDataDispatchTokenSource;
    private CancellationTokenSource _ddcDataTokenSource;
    private ConcurrentQueue<byte[]> _tdoaQueue;

    #endregion

    #region 初始化

    private void InitResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
        ReleaseQueues();
        InitMiscs();
        InitQueues();
        InitNetworks();
        Preset();
        InitTasks();
        SetHeartBeat(_ctrlChannel);
    }

    private void InitMiscs()
    {
        _media = MediaType.None;
        // 频率修正
        _frequencyOffsetDic = new Dictionary<long, long>();
        _reverseFrequencyOffsetDic = new Dictionary<long, long>();
        //
        // 配置信息
        var configFile = AppDomain.CurrentDomain.BaseDirectory + _configPath;
        if (!File.Exists(configFile))
            File.WriteAllLines(configFile, new[]
            {
                "frequency_pair=0,0;1,1"
            });
        var configLines = File.ReadLines(configFile).ToArray();
        foreach (var line in configLines)
        {
            var config = line.ToLower().Split(new[] { '=' });
            if (config is not { Length: 2 }) continue;
            switch (config[0].Trim())
            {
                case "frequency_pair":
                {
                    var pairs = config[1].Trim().Split(new[] { ';' });
                    foreach (var pair in pairs)
                        try
                        {
                            var keyValue = pair.Trim().Split(new[] { ',' });
                            if (keyValue is not { Length: 2 }) continue;
                            var key = long.Parse(keyValue[0].Trim());
                            var value = long.Parse(keyValue[1].Trim());
                            _frequencyOffsetDic[key] = value;
                            _reverseFrequencyOffsetDic[value] = key;
                        }
                        catch
                        {
                            // ignored
                        }
                }
                    break;
            }
        }
    }

    private void InitQueues()
    {
        //TDOA消息队列
        _tdoaQueue = new ConcurrentQueue<byte[]>();
        //其他数据消息队列
        _queues = new ConcurrentDictionary<string, ConcurrentQueue<byte[]>>();
        foreach (var identifier in _identifiers)
        {
            var queue = new ConcurrentQueue<byte[]>();
            _queues.TryAdd(identifier, queue);
        }
    }

    private void InitNetworks()
    {
        _channels = new ConcurrentDictionary<string, Socket>();
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
        _tdoaDataChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        _tdoaDataChannel.Connect(Ip, Port + 10);
        foreach (var identifier in _identifiers)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            socket.Bind(new IPEndPoint(endPoint.Address, 0));
            socket.Connect(Ip, 0);
            _channels.TryAdd(identifier, socket);
        }
    }

    private void InitTasks()
    {
        // 数据回传通道与队列
        _captures = new Dictionary<string, (Task task, CancellationTokenSource tokenSource)>();
        _dispatches = new Dictionary<string, (Task task, CancellationTokenSource tokenSource)>();
        _levelDataTokenSource = new CancellationTokenSource();
        _levelDataTask = new Task(p => DispatchLevelDataAsync(p).ConfigureAwait(false), new TaskParam
        {
            Name = $"{DeviceInfo.DisplayName}({DeviceId})_level_dispatch",
            Tag = "level",
            Token = _levelDataTokenSource.Token
        }, _levelDataTokenSource.Token);
        _levelDataTask.Start();
        _tdoaDataCaptureTokenSource = new CancellationTokenSource();
        _tdoaDataCaptureTask = new Task(p => CaptureTdoaDataAsync(p).ConfigureAwait(false), new TaskParam
        {
            Name = $"{DeviceInfo.DisplayName}({DeviceId})_tdoa_capture",
            Tag = "tdoa",
            Token = _tdoaDataCaptureTokenSource.Token
        }, _tdoaDataCaptureTokenSource.Token);
        _tdoaDataCaptureTask.Start();
        _tdoaDataDispatchTokenSource = new CancellationTokenSource();
        _tdoaDataDispatchTask = new Task(p => DispatchTdoaDataAsync(p).ConfigureAwait(false), new TaskParam
        {
            Name = $"{DeviceInfo.DisplayName}({DeviceId})_tdoa_dispatch",
            Tag = "tdoa",
            Token = _tdoaDataDispatchTokenSource.Token
        }, _tdoaDataDispatchTokenSource.Token);
        _tdoaDataDispatchTask.Start();
        _ddcDataTokenSource = new CancellationTokenSource();
        _ddcDataTask = new Task(p => DispatchDdcDataAsync(p).ConfigureAwait(false), new TaskParam
        {
            Name = $"{DeviceInfo.DisplayName}({DeviceId})_ddc_dispatch",
            Tag = "ddc",
            Token = _ddcDataTokenSource.Token
        }, _ddcDataTokenSource.Token);
        _ddcDataTask.Start();
        foreach (var identifier in _identifiers)
        {
            var captureTokenSource = new CancellationTokenSource();
            var captureTask = new Task(p => CapturePacketAsync(p).ConfigureAwait(false), new TaskParam
            {
                Name = $"{DeviceInfo.DisplayName}({DeviceId})_{identifier}_capture",
                Tag = identifier,
                IsIfmch = "ddc".Equals(identifier, StringComparison.OrdinalIgnoreCase),
                Token = captureTokenSource.Token
            }, captureTokenSource.Token);
            if (identifier != null)
            {
                _captures.Add(identifier, (captureTask, captureTokenSource));
                captureTask.Start();
                var dispatchTokenSource = new CancellationTokenSource();
                var dispatchTask = new Task(p => DispatchPacketAsync(p).ConfigureAwait(false), new TaskParam
                {
                    Name = $"{DeviceInfo.DisplayName}({DeviceId})_{identifier}_dispatch",
                    Tag = identifier,
                    IsIfmch = "ddc".Equals(identifier, StringComparison.OrdinalIgnoreCase),
                    Token = dispatchTokenSource.Token
                }, dispatchTokenSource.Token);
                _dispatches.Add(identifier, (dispatchTask, dispatchTokenSource));
                dispatchTask.Start();
            }
        }
    }

    /// <summary>
    ///     初始化DDC数据通道
    /// </summary>
    private void InitDdcPaths()
    {
        var ddcIpEndPoint = _channels["ddc"].LocalEndPoint as IPEndPoint;
        var address = ddcIpEndPoint?.Address.ToString();
        if (ddcIpEndPoint != null)
        {
            var port = ddcIpEndPoint.Port;
            for (var i = 0; i < 4; ++i)
            {
                SendCmd($"TRAC:DDC{i + 1}:UDP:DEL ALL");
                SendCmd($"TRAC:DDC{i + 1}:UDP:TAG \"{address}\", {port}, AUD, IF");
                SendCmd($"TRAC:DDC{i + 1}:UDP:FLAG \"{address}\", {port}, \"SWAP\",\"OPT\",\"VOLTage:AC\"");
            }
        }

        ResetDdcPath();
    }

    private void Preset()
    {
        //将接收机状态恢复为默认状态
        SendCmd("*RST");
        //由于默认状态下音量不为0，此处将音量关闭
        SendCmd("SYSTEM:AUDIO:VOLUME 0");
        SendCmd("FORM ASC"); //默认为ASC
        SendCmd("FORM:BORD SWAP"); //默认为NORM
        SendCmd("SYST:AUD:REM:MOD 2"); //PCM 32k,16bit,1ch ,默认为0
        //SendCmd("MEASure:BANDwidth:LIMits:AUTO ON");//默认ON
        //SendCmd("MEASure:MODE CONTinuous");//默认CONTinuous
        SendCmd("SENS:FREQ:AFC OFF"); //默认OFF
        SendCmd("FREQ:SYNT:MODE LOWP"); //合成器模式，TODO:
        SendCmd("CALC:IFP:STEP:AUTO ON"); //默认为ON
        //SetSpan(200d);
        SendCmd("FREQ:SPAN 200 kHz");
        SendCmd("SYSTem:IF:REMote:MODe SHORT");
        InitDdcPaths();
    }

    #endregion

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
        ReleaseQueues();
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_ctrlChannel);
        Utils.CloseSocket(_tdoaDataChannel);
        Utils.CloseSocket(_ddcCtrlChannel);
        if (_channels == null) return;
        foreach (var identifier in _identifiers)
            if (_channels.ContainsKey(identifier) && _channels[identifier] != null)
                try
                {
                    _channels[identifier].Disconnect(true);
                    _channels[identifier].Close();
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    _channels[identifier] = null;
                }

        _channels.Clear();
    }

    private void ReleaseQueues()
    {
        _tdoaQueue?.Clear();
        if (_queues == null) return;
        foreach (var identifier in _identifiers)
        {
            var hasKey = _queues.TryGetValue(identifier, out var queue);
            if (!hasKey) continue;
            queue?.Clear();
        }

        _queues.Clear();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_levelDataTask, _levelDataTokenSource);
        Utils.CancelTask(_tdoaDataCaptureTask, _tdoaDataCaptureTokenSource);
        Utils.CancelTask(_tdoaDataDispatchTask, _tdoaDataDispatchTokenSource);
        Utils.CancelTask(_ddcDataTask, _ddcDataTokenSource);
        foreach (var identifier in _identifiers)
        {
            if (_captures != null)
            {
                var hasCapture = _captures.TryGetValue(identifier, out var captureTaskInfo);
                if (hasCapture) Utils.CancelTask(captureTaskInfo.task, captureTaskInfo.tokenSource);
            }

            if (_dispatches != null)
            {
                var hasDispatch = _dispatches.TryGetValue(identifier, out var dispatchTaskInfo);
                if (hasDispatch) Utils.CancelTask(dispatchTaskInfo.task, dispatchTaskInfo.tokenSource);
            }
        }

        _captures?.Clear();
        _dispatches?.Clear();
    }

    #endregion

    #region 开始任务

    private void StartTask()
    {
        SetDataByAbility();
        ClearQueues();
        SendMediaRequest();
    }

    private void SetDataByAbility()
    {
        if (MonitorAntenna >= 0)
            SendCmd("ROUTe:VUHF (@" + MonitorAntenna + ")");
        else if (MonitorAntenna == -1) SendCmd("ROUTe:AUTO ON");
        if (CurFeature.Equals(FeatureType.FFM))
        {
            SetFfm();
        }
        else if ((CurFeature & FeatureType.AmpDF) > 0)
        {
            SetAmpDf();
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            SetScan();
        }
        else if (CurFeature == FeatureType.TDOA)
        {
            SetTdoa();
            StartTdoa();
        }
        else if (CurFeature == FeatureType.IFMCA)
        {
            SetIfmca();
        }
        else if (CurFeature == FeatureType.IFOUT)
        {
            SetIfout();
        }
    }

    private void SetIfout()
    {
        IfBandwidth = _ifBandwidth > 500 ? 800 : 300;
    }

    private void SetIfmca()
    {
        _preChannels = null;
        _media |= MediaType.Spectrum;
    }

    private void SetTdoa()
    {
        _media = MediaType.Iq;
        _media |= MediaType.Spectrum;
    }

    private void SetScan()
    {
        if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
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

    private void SetAmpDf()
    {
        _media |= MediaType.Iq;
    }

    private void SetFfm()
    {
        _media |= MediaType.Level;
        if (_spectrumSwitch) _media |= MediaType.Spectrum;
        if (_audioSwitch) _media |= MediaType.Audio;
        if (_iqSwitch) _media |= MediaType.Iq;
        if (_ituSwitch) _media |= MediaType.Itu;
    }

    /// <summary>
    ///     新处理逻辑
    ///     发送数据请求
    /// </summary>
    private void SendMediaRequest()
    {
        if (_media == MediaType.None) return;
        //由于单频测量可以在任务运行过程中更改参数，所以此处需要先删除之前的UDP通道
        if ((CurFeature & (FeatureType.FFM | FeatureType.AmpDF)) > 0) CloseUdpPath();
        OpenUdpPath();
        StartMeasure();
        if (CurFeature == FeatureType.TDOA) _isReadTdoaStart = true;
        if (CurFeature == FeatureType.FFM && _demMode == Modulation.Tv) StartGetTvBmp();
    }

    private void ClearQueues()
    {
        _tdoaQueue?.Clear();
        if (_queues == null) return;
        // 清空缓存队列
        foreach (var identifier in _identifiers)
            if (_queues.TryGetValue(identifier, out var queue))
                queue?.Clear();
    }

    private void OpenUdpPath()
    {
        var address = (_ctrlChannel.LocalEndPoint as IPEndPoint)?.Address.ToString();
        var tdoaPort = (_tdoaDataChannel.LocalEndPoint as IPEndPoint)?.Port.ToString();
        var dataPort = (_channels["data"].LocalEndPoint as IPEndPoint)?.Port.ToString();
        string tag = null;
        if ((_media & MediaType.Audio) > 0) tag += "AUD,";
        if ((_media & MediaType.Scan) > 0) tag += "FSC,PSC,MSC,";
        if ((_media & MediaType.Spectrum) > 0 && (CurFeature & FeatureType.AmpDF) == 0) tag += "IFP,";
        if ((_media & MediaType.Iq) > 0)
        {
            if (CurFeature != FeatureType.TDOA && _iqMode)
            {
                if (!string.IsNullOrEmpty(tag)) tag = null;
                StartTdoa();
                _isReadTdoaStart = true;
            }

            tag += "IF,";
            if (CurFeature == FeatureType.TDOA || _iqMode)
            {
                tag = tag.Remove(tag.Length - 1);
                SendCmd($"TRAC:TCP:TAG:ON \"{address}\",{tdoaPort},{tag}");
                Thread.Sleep(10);
                var str2 = $"TRAC:TCP:FLAG:ON \"{address}\",{tdoaPort},\"SWAP\",\"OPT\",\"VOLT:AC\"";
                SendCmd(str2);
                Thread.Sleep(10);
                SendCmd($"TRAC:UDP:TAG:ON \"{address}\",{dataPort},GPSC");
                Thread.Sleep(10);
                SendCmd($"TRAC:UDP:FLAG:ON \"{address}\",{dataPort},\"OPT\"");
                Thread.Sleep(10);
                SendCmd($"TRAC:UDP:TAG:ON \"{address}\",{dataPort},CW");
                Thread.Sleep(10);
                SendCmd(
                    $"TRAC:UDP:FLAG:ON \"{address}\",{dataPort},\"VOLT: AC\",\"OPT\",\"FREQ: RX\",\"FREQ: HIGH:RX\"");
                Thread.Sleep(10);

                return;
            }
        }

        if (tag != null)
        {
            tag = tag.Remove(tag.Length - 1);
            SendCmd($"TRAC:UDP:TAG:ON \"{address}\",{dataPort},{tag}");
            Thread.Sleep(tag.Split(',').ToList().Contains("IF") ? 50 : 10);
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Pscan)
                SendCmd($"TRAC:UDP:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"VOLT:AC\"");
            else
                SendCmd(
                    $"TRAC:UDP:FLAG:ON \"{address}\",{dataPort},\"SWAP\",\"OPT\",\"VOLT:AC\",\"FREQ:RX\",\"FREQ:HIGH:RX\"");
            Thread.Sleep(10);
        }
    }

    #endregion

    #region 停止任务

    private void StopTask()
    {
        SendCmd("ABORT");
        SendCmd("FREQ:SYNT:MODE LOWP");
        _media = MediaType.None;
        CloseUdpPath();
        Thread.Sleep(20);
        ResetDdcPath();
        ClearQueues();
        // 设置数字中频模式 16bit I/16bit Q
        SendCmd("SYSTem:IF:REMote:MODe SHORT");
    }

    private void CloseUdpPath()
    {
        SendCmd("TRAC:UDP:DEL ALL");
        Thread.Sleep(10);
        SendCmd("TRAC:UDP:DEF:DEL ALL");
        Thread.Sleep(10);
        SendCmd("TRAC:TCP:DEL ALL");
        Thread.Sleep(10);
    }

    #endregion

    #region 指令发送

    /// <summary>
    ///     用于发送设置类指令
    /// </summary>
    /// <param name="cmd"></param>
    private void SendCmd(string cmd)
    {
        Console.WriteLine($"==> {cmd}");
        var buffer = Encoding.ASCII.GetBytes(cmd + "\n");
        lock (_ctrlChannelLock)
        {
            _ctrlChannel.Send(buffer);
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
        int recvCount;
        lock (_ctrlChannelLock)
        {
            _ctrlChannel.Send(sendBuffer);
            recvCount = _ctrlChannel.Receive(_tcpRecvBuffer, SocketFlags.None);
        }

        var result = string.Empty;
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
    public bool IsIfmch { get; set; }
    public CancellationToken Token { get; set; }
}