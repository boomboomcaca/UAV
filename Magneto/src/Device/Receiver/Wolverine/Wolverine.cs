using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Magneto.Device.Wolverine;

public partial class Wolverine(Guid id) : DeviceBase(id)
{
    #region 成员变量

    private bool _isDisposed;

    //
    // 常量
    private readonly string[] _identifiers = ["audio", "ddc", "data", "dev"];
    private const int NormSamplingRateIqCount = 1000 * 1024;

    private const int AmpdfSamplingRateIqCount = 2048;

    //
    // 同步锁
    private readonly object _ctrlChannelLock = new(); // 控制通道锁
    private readonly object _identifierLock = new(); // 数据标识同步锁 
    private Socket _ctrlChannel; // 指令发送与查询通道（TCP）

    private Socket WritingChannel
    {
        get
        {
            lock (_ctrlChannelLock)
            {
                return _ctrlChannel;
            }
        }
    }

    private object InvalidCountLock { get; } = new();
    private NetworkStream _networkStream;
    private StreamReader _streamReader;
    private IDictionary<string, Socket> _channels;
    private IDictionary<string, Thread> _captures;
    private IDictionary<string, Thread> _dispatches;

    private readonly List<SignalProcess> _bufSignalProcesses = [];
    private readonly List<SignalInner> _signalInners = [];
    private IDictionary<string, MQueue<byte[]>> _queues; // 数据队列集合

    //
    // 业务数据
    private DataType _subscribedData; // 当前订阅的主通道业务数据

    // 
    // 扫描配置
    private int _invalidScanCount; // 无效扫描次数
    private int _scanDataLength; // 频段离散总点数

    private float _compassAngle;

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        try
        {
            var result = base.Initialized(moduleInfo);
            if (!result) return false;
            InitMiscs();
            InitNetworks();
            //InitChannelPhaseDiffs();
            //InitAntennas();
            InitChannels();
            //InitHandshake();
            InitThreads();
            SetHeartBeat(WritingChannel);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            //检查非托管资源并释放
            ReleaseResource();
            throw;
        }
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        ClearAll();
        PreSet();
        SetDataByAbility();
        PostSet();
        RequestTask(_subscribedData);
    }

    public override void Stop()
    {
        PreReset();
        ResetDataByAbility();
        CancelTask();
        ClearAll();
        PostReset();
        base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.MfdfPoints)
            // 离散测向频点为功能中设置的参数，这里不做处理
            return;

        //
        // 当前判断及以下分支纯属胡扯
        if (TaskState == TaskState.Start)
        {
            //CancelTask();
            if ((CurFeature & FeatureType.ScanDf) == 0) Thread.Sleep(100);
            base.SetParameter(name, value);
            //RequestTask(_subscribedData);
        }
        else
        {
            base.SetParameter(name, value);
        }

        Utils.GetPropertyNameValue(name, this);
        // 运行时修改参数
        // 单频测量/测向或类单频功能，在参数有变更的情况下都需要清理缓存，保证实时数据的实时响应
        if (TaskState == TaskState.Start &&
            (CurFeature & (FeatureType.Scan | FeatureType.MScan | FeatureType.FScne | FeatureType.MScne)) ==
            0) ClearAll();
        // 测向控制
        //if (TaskState == TaskState.Start &&
        //    (CurFeature & (FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF | FeatureType.SSE)) > 0)
        //    //天线采用极化与频率自动的方式切换，因此当相应的参数变更时，需要及时选择天线，同时清空无效的测向缓存数据
        //    if (name.Equals("frequency", StringComparison.OrdinalIgnoreCase) ||
        //        name.Equals("dfpolarization", StringComparison.OrdinalIgnoreCase))
        //        RaiseDfAntennaSelection();
        if ((CurFeature & FeatureType.AmpDf) > 0 && name.Equals("iqsamplingcount", StringComparison.OrdinalIgnoreCase))
            SendCommand($"IQ:COUN {AmpdfSamplingRateIqCount}");
    }

    public override void Dispose()
    {
        _isDisposed = true;
        ReleaseResource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 初始化

    private void InitMiscs()
    {
        _subscribedData = DataType.None;
        //
        // 数据回传通道与队列
        _channels = new Dictionary<string, Socket>();
        _captures = new Dictionary<string, Thread>();
        _dispatches = new Dictionary<string, Thread>();
        _queues = new Dictionary<string, MQueue<byte[]>>();
    }

    private void InitNetworks()
    {
        _ctrlChannel = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
            ReceiveTimeout = 5000 // 避免存在查询操作，造成套接字永久等待
        };
        _ctrlChannel.Connect(Ip, Port);
        _networkStream = new NetworkStream(_ctrlChannel, FileAccess.Read, false);
        _streamReader = new StreamReader(_networkStream);
        if (_ctrlChannel.LocalEndPoint is not IPEndPoint endPoint) throw new Exception("无可用网络地址");
        foreach (var identifier in _identifiers)
        {
            _channels[identifier] = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _channels[identifier].SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _channels[identifier].Bind(new IPEndPoint(endPoint.Address, 0));
            _channels[identifier].Connect(Ip, 0);
        }
    }

    private void InitChannels()
    {
        //SendCommand("TRAN:CLE");
        var address = (_ctrlChannel.LocalEndPoint as IPEndPoint)?.Address.ToString();
        if (_channels.TryGetValue("audio", out var channel))
        {
            var audioPort = ((IPEndPoint)channel.LocalEndPoint)!.Port;
            SendCommand($"TRAN:UDP \"{address}\",{audioPort},AUDIO");
        }

        if (_channels.TryGetValue("ddc", out var channel1))
        {
            var ddcPort = ((IPEndPoint)channel1.LocalEndPoint)!.Port;
            SendCommand($"TRAN:UDP \"{address}\",{ddcPort},DDC");
        }

        if (_channels.TryGetValue("data", out var channel2))
        {
            var dataPort = ((IPEndPoint)channel2.LocalEndPoint)!.Port;
            SendCommand($"TRAN:UDP \"{address}\",{dataPort},IF");
            SendCommand($"TRAN:UDP \"{address}\",{dataPort},MEAS");
            SendCommand($"TRAN:UDP \"{address}\",{dataPort},SCAN");
        }

        if (_channels.TryGetValue("dev", out var channel3))
        {
            var devPort = ((IPEndPoint)channel3?.LocalEndPoint)!.Port;
            SendCommand($"TRAN:UDP \"{address}\",{devPort},DEV");
        }

        SendCommand($"DEV:GPS:ENAB {(EnableGps ? "ON" : "OFF")}");
        SendCommand($"DEV:COMP:ENAB {(EnableCompass ? "ON" : "OFF")}");
    }

    private void InitThreads()
    {
        foreach (var identifier in _identifiers)
        {
            _captures[identifier] = new Thread(CapturePacket)
            {
                IsBackground = true,
                Name = $"{DeviceInfo.DisplayName}({DeviceId})_{identifier}_capture"
            };
            _captures[identifier].Start(identifier);
            _dispatches[identifier] = new Thread(DispatchPacket)
            {
                IsBackground = true,
                Name = $"{DeviceInfo.DisplayName}({DeviceId})_{identifier}_dispatch"
            };
            _dispatches[identifier].Start(identifier);
        }
    }

    #endregion

    #region 功能设置

    private void PreSet()
    {
        if ((CurFeature & (FeatureType.Wbdf | FeatureType.Scan
                                            | FeatureType.Tdoa | FeatureType.ScanDf)) > 0)
        {
            SendCommand("MEAS:SQU OFF");
        }
        else if (CurFeature == FeatureType.MScan)
        {
            SendCommand("MEAS:SQU OFF");
        }
        else if (CurFeature == FeatureType.MScne)
        {
            SendCommand("MEAS:SQU ON");
        }
        else
        {
            SendCommand("MEAS:SQU ON");
            if ((CurFeature & FeatureType.Ffdf) > 0) // 设置很低的静噪门限，保证测向时可以输出音频
                SendCommand("MEAS:THR -120");
            if ((CurFeature & FeatureType.Ifmca) > 0) SendCommand("DEM FM");
        }

        // 只有TDOA才需要做GPS触发采集，其它功能下，做连续采集或按固定才度采集
        //SendCommand((CurFeature & FeatureType.TDOA) > 0 ? "TRIG:TDO GPS" : "TRIG:TDO NON");
        //SendCommand($"AMPL {(EnableRfAmplifier ? "ON" : "OFF")}");
        //SendCommand($"MEAS:IQ:WIDT {IqWidth}");
    }

    private void SetDataByAbility()
    {
        if ((CurFeature & FeatureType.Ffm) > 0)
        {
            //SetFixFq();
        }
        else if ((CurFeature & FeatureType.Ffdf) > 0)
        {
        }
        else if ((CurFeature & FeatureType.Wbdf) > 0)
        {
        }
        else if ((CurFeature & FeatureType.Sse) > 0)
        {
            //RaiseDfAntennaSelection();
        }
        else if ((CurFeature & FeatureType.ScanDf) > 0)
        {
            SetScanDf();
            //RaiseDfAntennaSelection();
        }
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            //SetMScan();
        }
        else if ((CurFeature & FeatureType.Tdoa) > 0)
        {
            SetTdoa();
        }
        else if ((CurFeature & FeatureType.AmpDf) > 0)
        {
            SetAmpdf();
        }
    }

    private void PostSet()
    {
        if ((CurFeature & FeatureType.Ffm) > 0) // DDCA本质是单频测量
        {
            SendCommand("FREQ:MOD FIX");
        }
        else if ((CurFeature & (FeatureType.Tdoa | FeatureType.AmpDf)) > 0)
        {
            SendCommand("FREQ:MOD IQ");
            SendCommand("FREQ:MOE TDOA");
        }
        else if ((CurFeature & FeatureType.ScanDf) > 0)
        {
            SendCommand("FREQ:MOD DFSC");
        }
        else if ((CurFeature & FeatureType.Ifmca) > 0)
        {
            // SendCommand("DEM FM"); // 尽管主通道不进行音频解调，但是主通道依然受到该参数影响，这此也仅仅是容错处理，最终解决方案还是该依赖接收机
            SendCommand("FREQ:MOD DDC");
            Thread.Sleep(5000); // 按天津接收机团队的要求，从任何功能切换进DDC时，延时5秒
        }
    }

    private static void PreReset()
    {
        // deliberately left blank
    }

    private void ResetDataByAbility()
    {
        if ((CurFeature & FeatureType.Ffm) > 0)
            ResetFixFq();
        else if ((CurFeature & (FeatureType.Scan | FeatureType.FScne)) > 0)
            ResetScan();
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
            ResetMScan();
        else if ((CurFeature & FeatureType.AmpDf) > 0) ResetAmpdf();
        // 数据请求置空
        _subscribedData = DataType.None;
    }

    private void PostReset()
    {
        if (CurFeature == FeatureType.Scan)
            Thread.Sleep(20);
        else if (CurFeature == FeatureType.Ifmca) // 按天津接收机开发团队的要求，从DDC功能切换出来，需要延时5秒后才能进行其它功能的测量
            Thread.Sleep(5000);
        // 停止任务后，清空所有的过滤器
    }

    private static void ResetFixFq()
    {
        // deliberately left blank
    }

    private void SetScanDf()
    {
        SendCommand($"FREQ:STAR {_startFrequency * 1e6}");
        SendCommand($"FREQ:STOP {_stopFrequency * 1e6}");
        SendCommand($"FREQ:STEP {_stepFrequency * 1e3}");
        _subscribedData |= DataType.Dfscan;
    }

    private static void ResetScan()
    {
        // delibrately left blank
    }

    //private void SetMScan()
    //{
    //    if (MScanPoints == null) return;
    //    SendCommand("FREQ:MOD MSC");
    //    // SendCommand("MEAS:HOLD 0"); // 按业务需求，将等待时间设置为零（内部依赖测量时间）
    //    SendCommand($"MSC:COUN {MScanPoints.Length}");
    //    var cmdBuilder = new StringBuilder();
    //    for (var index = 0; index < MScanPoints.Length; ++index)
    //    {
    //        var cmd =
    //            $"MEM:CONT {index},{MScanPoints[index]["frequency"]} MHz,{MScanPoints[index]["filterBandwidth"]} kHz,{MScanPoints[index]["demMode"]};";
    //        cmdBuilder.Append(cmd);
    //        // if ((index + 1) % 50 == 0)
    //        // {
    //        // 	SendCommand(cmdBuilder.ToString().TrimEnd(';'));
    //        // 	cmdBuidler.Clear();
    //        // }
    //    }

    //    if (cmdBuilder.Length > 0)
    //    {
    //        SendCommand(cmdBuilder.ToString().TrimEnd(';'));
    //        cmdBuilder.Clear();
    //    }

    //    if (CurFeature == FeatureType.MScan) // 如果是离散扫描，等待时间和驻留时间需要隐式设置到接收机，取值为零
    //        SendCommand("MEAS:DWEL 0");
    //    _scanMode = ScanMode.MScan;
    //    _scanDataLength = MScanPoints.Length;
    //    _subscribedData |= DataType.Scan;
    //}

    private static void ResetMScan()
    {
        // clear all mscan frequencies
    }

    private void SetTdoa()
    {
        _subscribedData |= DataType.Tdoa;
    }

    private void SetAmpdf()
    {
        _subscribedData |= DataType.Iq | DataType.Level;
    }

    private static void ResetAmpdf()
    {
        // delibrately left blank
    }

    #endregion

    #region 任务处理

    private void AcceptDataRequest(DataType dataType)
    {
        _subscribedData |= dataType;
        if (TaskState == TaskState.Start) RequestTask(_subscribedData);
    }

    private void RejectDataRequest(DataType dataType)
    {
        _subscribedData &= ~dataType;
        if (TaskState == TaskState.Start) RequestTask(_subscribedData);
    }

    private void RequestTask(DataType dataType)
    {
        // 切换单频与信号识别
        if ((CurFeature & FeatureType.Ffm) > 0)
        {
            if ((dataType & DataType.Iq) > 0)
            {
                dataType = DataType.Iq;
                SendCommand("FREQ:MOD IQ");
                SendCommand("FREQ:MOD SD"); //TDDO; to be removed after 2020/04/01
                SendCommand($"IQ:COUN {NormSamplingRateIqCount}");
            }
            else
            {
                SendCommand("FREQ:MOD FIX");
            }
        }
        else if ((CurFeature & FeatureType.Tdoa) > 0)
        {
            dataType = DataType.Tdoa;
            SendCommand("TRIG:TDO GPS");
        }
        else if ((CurFeature & FeatureType.AmpDf) > 0)
        {
            dataType = DataType.Iq;
            SendCommand($"IQ:COUN {AmpdfSamplingRateIqCount}");
        }

        SendCommand(dataType != DataType.None ? $"TRAN:MED {dataType.ToString().Replace(", ", ",")}" : "TRAN:CLE");
    }

    private void CancelTask()
    {
        SendCommand("TRAN:MED NON");
    }

    private void ClearAll()
    {
        // 清空缓存队列
        foreach (var identifier in _identifiers)
            if (_queues.ContainsKey(identifier) && _queues[identifier] != null)
                _queues[identifier].Clear();
    }

    #endregion

    #region 资源释放

    private void ReleaseResource()
    {
        ReleaseNetworks();
        ReleaseQueues();
        ReleaseThreads();
    }

    private void ReleaseThreads()
    {
        foreach (var identifier in _identifiers)
        {
            if (_captures.ContainsKey(identifier) && _captures[identifier]?.IsAlive == true)
            {
                try
                {
                    _captures[identifier].Join();
                }
                catch
                {
                    // ignored 
                }

                _captures[identifier] = null;
            }

            if (!_dispatches.ContainsKey(identifier) || _dispatches[identifier]?.IsAlive != true) continue;
            try
            {
                _dispatches[identifier].Join(1000);
            }
            catch
            {
                // ignored
            }

            _dispatches[identifier] = null;
        }
    }

    private void ReleaseNetworks()
    {
        if (_streamReader != null)
            try
            {
                _streamReader.Close();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _streamReader = null;
            }

        if (_networkStream != null)
            try
            {
                _networkStream.Close();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _networkStream = null;
            }

        if (_ctrlChannel != null)
            try
            {
                _ctrlChannel.Close();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _ctrlChannel = null;
            }

        foreach (var identifier in _identifiers)
            if (_channels.ContainsKey(identifier) && _channels[identifier] != null)
                try
                {
                    // _channels[identifier].Disconnect(true);
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
    }

    private void ReleaseQueues()
    {
        foreach (var identifier in _identifiers)
            if (_queues.ContainsKey(identifier) && _queues[identifier] != null)
            {
                try
                {
                    _queues[identifier].Dispose();
                }
                catch
                {
                    // ignored
                }

                _queues[identifier] = null;
            }
    }

    #endregion

    #region 数据接收与处理

    private void CapturePacket(object obj)
    {
        var identifier = obj.ToString();
        MQueue<byte[]> queue = null;
        Socket socket = null;
        lock (_identifierLock)
        {
            if (identifier != null)
            {
                socket = _channels[identifier];
                if (!_queues.ContainsKey(identifier)) _queues[identifier] = new MQueue<byte[]>();
                queue = _queues[identifier];
            }
        }

        var buffer = new byte[1024 * 1024];
        if (socket == null) return;
        socket.ReceiveBufferSize = buffer.Length;
        while (!_isDisposed)

            try
            {
                var receivedCount = socket.Receive(buffer);
                if (receivedCount <= 0)
                {
#if WRITE_DEBUG_INFO
						Console.WriteLine(string.Format("Received data size: {0}", receivedCount));
#endif
                    Thread.Sleep(1);
                    continue;
                }

                var receivedBuffer = new byte[receivedCount];
                Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);
                if (TaskState == TaskState.Start
                    || identifier.Equals("dev", StringComparison.OrdinalIgnoreCase))
                {
                    queue.EnQueue(receivedBuffer);
                }
                else
                {
#if WRITE_DEBUG_INFO
						Console.WriteLine(string.Format("Task has been aborted, received data size: {0}", receivedCount));
#endif
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException) return;

                if (ex is SocketException) Thread.Sleep(10);
            }
    }

    private void DispatchPacket(object obj)
    {
        var identifier = obj.ToString();
        MQueue<byte[]> queue = null;
        lock (_identifierLock)
        {
            if (identifier != null && !_queues.ContainsKey(identifier)) _queues[identifier] = new MQueue<byte[]>();
            if (identifier != null) queue = _queues[identifier];
        }

        while (!_isDisposed)
            try
            {
                if (queue is { Count: 0 })
                {
                    Thread.Sleep(1);
                    continue;
                }

                var buffer = queue?.DeQueue(100);
                if (buffer == null) continue;
                var packet = RawPacket.Parse(buffer, 0);
                if (packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                    // netcore不支持线程调用Abort()方法了
                    return;
            }
    }

    private void ForwardPacket(object obj)
    {
        if (obj is not RawPacket packet) return;
#if WRITE_DEBUG_INFO && DATA_INFO
			packet.DataCollection.ForEach(item => { Console.WriteLine(item.ToString()); });
#endif
        var result = new List<object>();
        foreach (var data in packet.DataCollection)
            switch ((DataType)data.Tag)
            {
                case DataType.Iq:
                    break;
                case DataType.Level:
                    break;
                case DataType.Audio:
                    //result.Add(ToAudio(data));
                    break;
                case DataType.Spectrum:
                    break;
                case DataType.Itu:
                    break;
                case DataType.Scan: // PSCAN
                case DataType.Scan + 2: // FSCAN
                case DataType.Scan + 4: // MScan
                    //result.Add(ToScan(data));
                    break;
                case DataType.Tdoa:
                    break;
                case DataType.Dfscan: // ScanDF
                    ProcessDfscan(data);
                    break;
                case DataType.Gps:
                    ProcessGps(data);
                    break;
                case DataType.Compass:
                    ProcessCompass(data);
                    break;
                case DataType.None:
                    break;
                case DataType.Dfind:
                    break;
                case DataType.Dfpan:
                    break;
                case DataType.Sse:
                    break;
                default:
                    continue;
            }

        result = result.Where(item => item != null).ToList();
        if (result.Count > 0 && TaskState == TaskState.Start)
            // result.Add(_deviceId);
            SendData(result);
    }

    private void ProcessGps(object data)
    {
        if (data is not RawGps gps) return;
        new Task(() =>
        {
            //foreach (var item in dataCollection) ParseGps(item.Trim());
            var gpsData = new SDataGps
            {
                Altitude = (float)(gps.Altitude / 1e1),
                Latitude = gps.Latitude / 1e6,
                Longitude = gps.Longitude / 1e6,
                Heading = (float)(gps.Heading / 1e1),
                Satellites = gps.Satellites,
                Speed = (ushort)(gps.Speed / 1e6)
            };
            SendMessageData([gpsData]);
            SendData([gpsData]);
        })
            .RunSynchronously();
    }

    private void ProcessCompass(object data)
    {
        if (data is not RawCompass raw || raw.Heading < 0 || raw.Heading > 3600) return;
        var compass = new SDataCompass
        {
            Heading = ((raw.Heading / 10.0f + CompassInstallingAngle) % 360 + 360) % 360
        };
        _compassAngle = compass.Heading * 10f;
        SendData([compass]);
    }

    private int _curSegmentIndex;
    private void ProcessDfscan(object data)
    {
        if (data is not RawDfScan bufRaw) return;
        var result = new List<object>();
        bufRaw.SegmentIndex = _curSegmentIndex;
        var reDfScan = new SDataDfScan
        {
            SegmentOffset = bufRaw.SegmentIndex,
            Count = bufRaw.DataCount,
            Offset = bufRaw.Offset,
            StartFrequency = bufRaw.StartFrequency / 1e6,
            StepFrequency = bufRaw.StepFrequency / 1e3,
            StopFrequency = bufRaw.StopFrequency / 1e6,
            Azimuths = Array.ConvertAll(bufRaw.AzimuthArray, s =>
            {
                if (EnableCompass && s > -1)
                    return (s + _compassAngle) % 3600f;
                return s % 3600f;
            }),
            OptimalAzimuths = new float[bufRaw.DataCount],
            Qualities = Array.ConvertAll(bufRaw.QualityArray, s => (float)s)
        };
        result.Add(reDfScan);

        for (var i = 0; i < bufRaw.SignalCount; i++)
        {
            var isJoinAny = _signalInners.Any(a =>
            {
                if (!bufRaw.SegmentIndex.Equals(a.SegmentIdx)) return false;
                if (a.IsOver) return false;
                if (bufRaw.Offset + bufRaw.IndexPairs[i * 2] - 1 != a.FreqIdxs.StartFreqIdx) return a.IsOver;
                a.FreqIdxs = (a.FreqIdxs.StartFreqIdx,
                    bufRaw.Offset + bufRaw.IndexPairs[i * 2]);
                a.Azimuths = a.Azimuths
                    .Concat(reDfScan.Azimuths.Skip(bufRaw.IndexPairs[i * 2]).Take(bufRaw.IndexPairs[i * 2 + 1]))
                    .ToArray();
                a.IsOver = !bufRaw.IndexPairs[i * 2 + 1].Equals(bufRaw.Offset + bufRaw.DataCount);
                if (!a.IsOver) return false;
                var isExitSignal = _bufSignalProcesses.Any(b =>
                {
                    if (!NormSignals(b.SignalInner.FreqIdxs.StartFreqIdx,
                            b.SignalInner.FreqIdxs.StopFreqIdx,
                            a.FreqIdxs.StartFreqIdx, a.FreqIdxs.StopFreqIdx)) return false;
                    b.SignalInner.FreqIdxs = (
                        Math.Min(b.SignalInner.FreqIdxs.StartFreqIdx, a.FreqIdxs.StartFreqIdx),
                        Math.Max(b.SignalInner.FreqIdxs.StopFreqIdx, a.FreqIdxs.StopFreqIdx));
                    b.OptimizeAzimuths(a.Azimuths);
                    return true;
                });
                if (!isExitSignal && a.IsInBandWidth) _bufSignalProcesses.Add(new SignalProcess { SignalInner = a });
                return true;
            });
            if (isJoinAny) continue;
            var newSignal = new SignalInner(StepFrequency)
            {
                SegmentIdx = bufRaw.SegmentIndex,
                Total = bufRaw.TotalCount,
                FreqIdxs = (bufRaw.Offset + bufRaw.IndexPairs[i * 2], bufRaw.Offset + bufRaw.IndexPairs[i * 2 + 1]),
                Azimuths =
                    reDfScan.Azimuths.Skip(bufRaw.IndexPairs[i * 2]).Take(bufRaw.IndexPairs[i * 2 + 1]).ToArray(),
                IsOver = true
            };
            _signalInners.Add(newSignal);
            if (bufRaw.IndexPairs[i * 2 + 1].Equals(bufRaw.Offset + bufRaw.DataCount) &&
                bufRaw.Offset + bufRaw.DataCount != bufRaw.TotalCount)
            {
                newSignal.IsOver = false;
                break;
            }

            var isExitSignal = _bufSignalProcesses.Any(a =>
            {
                if (!NormSignals(a.SignalInner.FreqIdxs.StartFreqIdx,
                        a.SignalInner.FreqIdxs.StopFreqIdx,
                        newSignal.FreqIdxs.StartFreqIdx, newSignal.FreqIdxs.StopFreqIdx)) return false;
                a.SignalInner.FreqIdxs = (
                    Math.Min(a.SignalInner.FreqIdxs.StartFreqIdx, newSignal.FreqIdxs.StartFreqIdx),
                    Math.Max(a.SignalInner.FreqIdxs.StopFreqIdx, newSignal.FreqIdxs.StopFreqIdx));
                a.OptimizeAzimuths(newSignal.Azimuths);
                return true;
            });
            if (!isExitSignal && newSignal.IsInBandWidth)
                _bufSignalProcesses.Add(new SignalProcess { SignalInner = newSignal });
        }

        if (_bufSignalProcesses.Count < 0)
            result.Add(new SDataSignals());
        else
            result.Add(new SDataSignals
            {
                Signals = _bufSignalProcesses.Select(s => new Signal
                {
                    FreqIdxs = s.SignalInner.FreqIdxs,
                    Azimuth = s.SignalInner.Azimuth,
                    SegmentIdx = s.SignalInner.SegmentIdx,
                    Guid = s.SignalInner.Guid
                }).ToList()
            });

        var reScan = new SDataScan
        {
            StartFrequency = bufRaw.StartFrequency / 1e6,
            StepFrequency = bufRaw.StepFrequency / 1e3,
            StopFrequency = bufRaw.StopFrequency / 1e6,
            Total = bufRaw.DataCount,
            Data = bufRaw.LevelArray,
            Offset = bufRaw.Offset,
            SegmentOffset = bufRaw.SegmentIndex,
            DataMark = [0x00, 0x00, 0x00, 0x01]
            //Threshold = Array.ConvertAll(re, c => (short)(c * 10))
        };

        result.Add(reScan);
        SendData(result);
        if (!(bufRaw.Offset + bufRaw.DataCount).Equals(bufRaw.TotalCount)) return;
        _signalInners.RemoveAll(f => f.SegmentIdx.Equals(bufRaw.SegmentIndex));
        _bufSignalProcesses.RemoveAll(r =>
        {
            if (DateTime.Now - r.LastTime <= TimeSpan.FromSeconds(5)) return false;
            r.Dispose();
            return true;
        });
        _curSegmentIndex++;
        if (_curSegmentIndex.Equals(_segments.Length)) _curSegmentIndex = 0;
    }

    #endregion

    #region 业务数据转换

    private static object ToAudio(object data)
    {
        if (data is not RawAudio raw)
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid null audio data: {0}");
#endif
            return null;
        }

        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = (int)raw.SampleRate,
            Data = raw.DataCollection
        };
        return audio;
    }

    private object ToScan(object data)
    {
        switch (data)
        {
            case RawScan:
                return ToConventionalScan(data);
            case RawFastScan:
                // return ToFastScan(data);
                break;
        }

        return null;
    }

    private object ToConventionalScan(object data)
    {
        if (data is not RawScan raw) return null;
        var delta = _scanDataLength - raw.Total;
        if (delta != 0)
        {
            if (raw.Offset + raw.DataCollection.Length == raw.Total)
                Array.Resize(ref raw.DataCollection, raw.DataCollection.Length + delta);
            raw.Total += delta;
        }

        var scan = new SDataScan
        {
            SegmentOffset = raw.SegmentIndex,
            StartFrequency = raw.StartFrequency / 1000000.0d,
            StopFrequency = raw.StopFrequency / 1000000.0d,
            StepFrequency = raw.StepFrequency / 1000.0d,
            Total = raw.Total,
            Offset = raw.Offset,
            Data = new short[raw.DataCollection.Length]
        };
        for (var i = 0; i < raw.DataCollection.Length; ++i)
            scan.Data[i] = raw.DataCollection[i];
        lock (InvalidCountLock)
        {
            if (CurFeature != FeatureType.Scan || _invalidScanCount <= 0) return scan;
            if (scan.Offset + scan.Data.Length == scan.Total) --_invalidScanCount;
            return null;
        }
    }

    #endregion

    #region GPS Parsing Helper

    // 将字符串转换为浮点数度，原始格式为：ddmm.mmmmmm

    // 转换为磁偏角

    // 获取两个点的距离，单位米

    #endregion

    #region Helper

    private void SendCommand(string cmd)
    {
        var sendBuffer = Encoding.Default.GetBytes(cmd + "\r\n");
        var bytesToSend = sendBuffer.Length;
        var offset = 0;
        var total = 0;
        try
        {
            // 流式套接字，循环发送，直到所有数据全部发送完毕
            while (total < bytesToSend)
            {
                var sentBytes = _ctrlChannel.Send(sendBuffer, offset, bytesToSend - total, SocketFlags.None);
                offset += sentBytes;
                total += sentBytes;
            }
#if !WRITE_DEBUG_INFO
            Console.WriteLine("{0:HH:mm:ss} <-- {1}", DateTime.Now, cmd.ToLower());
#endif
        }
        catch
        {
            // 此处的出现的异常（套机字异常），不在向上抛，会通过别的地方（比如心跳线程）对该异常进行处理
        }
    }

    private static bool NormSignals(int srcStartFreq, int srcStopFreq, int destStartFreq, int destStopFreq)
    {
        if (destStartFreq >= srcStartFreq && destStopFreq <= srcStopFreq)
            return true;
        var overlapStart = Math.Max(srcStartFreq, destStartFreq);
        var overlapStop = Math.Min(srcStopFreq, destStopFreq);
        if (overlapStart >= overlapStop) return false;
        var minStart = Math.Min(srcStartFreq, destStartFreq);
        var maxStop = Math.Max(srcStopFreq, destStopFreq);
        var overlap = overlapStop - overlapStart;
        var total = maxStop - minStart;
        return (double)overlap / total > 0.3;
    }

    #endregion
}