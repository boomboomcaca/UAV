using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoReceiver;

public partial class DemoReceiver(Guid deviceId) : DeviceBase(deviceId)
{
    private void InitResource()
    {
        InitAudioData();
        // IniGpsData();
        IniTdoaData();
        if (AsRealDevice)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse(IpAddress);
            _socket.Connect(ip, Port);
            _socket.NoDelay = true;
        }
        // var endPoint = _socket.LocalEndPoint as IPEndPoint;
        // var udpSvr = new UdpClient(5555, AddressFamily.InterNetwork);
        // udpSvr.Client.ReceiveTimeout = 20000;
        // var udpClient = new UdpClient("192.168.102.88", 6666);
        // udpClient.Connect("192.168.102.88", 6666);
        // System.Threading.Tasks.Task.Run(() =>
        // {
        //     while (true)
        //     {
        //         Thread.Sleep(100);
        //         try
        //         {
        //             var remote = new IPEndPoint(IPAddress.Any, 0);
        //             byte[] buffer = udpSvr.Receive(ref remote);
        //             if (buffer?.Length > 0)
        //             {
        //                 var str = Encoding.ASCII.GetString(buffer);
        //                 Trace.WriteLine($"UDP通道{remote} 发来数据： {str}");
        //                 var reply = $"The Edge Receive:{str},Thanks!";
        //                 var data = Encoding.ASCII.GetBytes(reply);
        //                 udpSvr.Send(data, data.Length, remote);
        //                 udpClient.Send(data, data.Length);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             Trace.WriteLine($"Error {ex}");
        //         }
        //     }
        // });
    }

    private void IniTdoaData()
    {
        if (string.IsNullOrEmpty(TdoaDataPath)) return;
        var path = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}tdoa", TdoaDataPath);
        if (!File.Exists(path)) return;
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var streamReader = new StreamReader(fileStream);
        var samplingRate = 0d;
        var attenuation = 0;
        long timeStamp = 0;
        while (!streamReader.EndOfStream)
        {
            var str = streamReader.ReadLine();
            if (str != null && str.Contains("Sample Rate"))
            {
                var split = str.Split(':');
                if (split.Length <= 1) continue;
                var tmp = split[1].Replace("Hz", "");
                if (!double.TryParse(tmp, out var rate)) continue;
                samplingRate = rate / 1000.0;
            }

            if (str != null && str.Contains("Attenuation"))
            {
                var split = str.Split(':');
                if (split.Length <= 1) continue;
                if (!int.TryParse(split[1], out attenuation)) continue;
            }

            if (str != null && str.Contains("Time"))
            {
                var split = str.Split(':');
                if (split.Length <= 1) continue;
                if (!long.TryParse(split[1], out timeStamp)) continue;
            }

            if (str != null && str.Contains('|'))
            {
                var arr = str.TrimEnd('|').Split('|');
                var iq16 = new short[arr.Length * 2];
                var iq32 = new int[arr.Length * 2];
                var is32Iq = false;
                for (var i = 0; i < arr.Length; i++)
                {
                    var tmp = arr[i].Split(',');
                    if (tmp.Length <= 1) continue;
                    if (short.TryParse(tmp[0], out var iData16) && short.TryParse(tmp[1], out var qData16))
                    {
                        iq16[i * 2] = iData16;
                        iq16[i * 2 + 1] = qData16;
                    }
                    else if (int.TryParse(tmp[0], out var iData32) && int.TryParse(tmp[1], out var qData32))
                    {
                        is32Iq = true;
                        iq32[i * 2] = iData32;
                        iq32[i * 2 + 1] = qData32;
                    }
                }

                if (is32Iq)
                    for (var i = 0; i < iq32.Length; i++)
                        if (iq32[i] == 0 && iq16[i] != 0)
                            iq32[i] = iq16[i];
                var data = new TdoaData
                {
                    SamplingRate = samplingRate,
                    Attenuation = attenuation,
                    TimeStamp = timeStamp,
                    Data16 = is32Iq ? null : iq16,
                    Data32 = is32Iq ? iq32 : null
                };
                lock (_lockTdoa)
                {
                    _tdoaData.Add(data);
                }
            }
        }
    }

    #region IDisposable 成员

    /// <summary>
    ///     IDisposable 成员，在这里释放资源
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();
        ReleaseResource();
    }

    #endregion

    private class TdoaData
    {
        public double SamplingRate { get; set; }
        public int Attenuation { get; set; }
        public long TimeStamp { get; set; }
        public short[] Data16 { get; set; }
        public int[] Data32 { get; set; }
    }

    #region 成员变量

    /// <summary>
    ///     当前订阅的数据类型
    ///     ///
    /// </summary>
    private MediaType _media = MediaType.None;

    /// <summary>
    ///     数据处理线程
    /// </summary>
    private Task _dataProcessTask;

    private CancellationTokenSource _dataProcessCts;

    /// <summary>
    ///     模拟的连接通道
    /// </summary>
    private Socket _socket;

    /// <summary>
    ///     模拟音频数据
    /// </summary>
    private const int AudioPacketLen = 8820;

    private readonly List<byte[]> _listAudio = [];
    private int _indexAudio;
    private const float MaxLevel = 48;
    private DateTime _prevAudioReadTime = DateTime.Now;
    private readonly int _audioReadFrequency = 190;
    private readonly List<TdoaData> _tdoaData = [];
    private readonly object _lockTdoa = new();
    private int _indexTdoa;

    #endregion

    #region 重载DeviceBase函数

    /// <summary>
    ///     初始化设备
    ///     在该过程中连接设备、设置必要的初始参数，实现设备的请求（子类需要先显示调用基类函数）
    /// </summary>
    /// <param name="device">设备的配置信息</param>
    public override bool Initialized(ModuleInfo device)
    {
        if (!base.Initialized(device)) return false;
        ReleaseResource(); //检查非托管资源并释放
        InitResource(); //初始化资源
        SetHeartBeat(_socket); //设置心跳包
        return true;
    }

    /// <summary>
    ///     启动
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="dataPort"></param>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _indexTdoa = 0;
        if ((CurFeature & FeatureType.Ffm) > 0)
        {
            _media |= MediaType.Level; //单频测量返回电平数据
        }
        else if ((CurFeature & FeatureType.Itum) > 0)
        {
            _media |= MediaType.Itu; //ITU测量返回ITU数据
        }
        else if ((CurFeature & FeatureType.Ffdf) > 0)
        {
            _media |= MediaType.Dfind; //单频测向返回示向度
        }
        else if ((CurFeature & FeatureType.Scan) > 0)
        {
            _media |= MediaType.Scan;
        }
        else if ((CurFeature & FeatureType.Wbdf) > 0)
        {
            _media |= MediaType.Dfpan;
            _media |= MediaType.Spectrum;
        }
        else if ((CurFeature & FeatureType.Tdoa) > 0)
        {
            _media |= MediaType.Tdoa;
        }
        else if ((CurFeature & FeatureType.MScan) > 0 || (CurFeature & FeatureType.MScne) > 0)
        {
            // 离散扫描必须返回扫描数据
            _media |= MediaType.Scan;
            _media |= MediaType.Spectrum;
            // 设置扫描模式
            ScanMode = ScanMode.MScan;
        }

        _index = 0;
        if (_listAudio.Count > 0) _indexAudio = _random.Next(0, _listAudio.Count - 1);
        var currentFeature = CurFeature;
        _dataProcessCts = new CancellationTokenSource();
        _dataProcessTask = new Task(() => DataProcess(currentFeature), _dataProcessCts.Token);
        _dataProcessTask?.Start();
    }

    private int _index;

    public override void Stop()
    {
        base.Stop();
        _mchEvent?.Set();
        _ = CancelTaskAsync(_dataProcessTask, _dataProcessCts);
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.Detector)
        {
        }

        base.SetParameter(name, value);
    }

    #endregion

    #region 数据处理线程

    private void DataProcess(FeatureType curFeature)
    {
        switch (curFeature)
        {
            case FeatureType.Itum:
            case FeatureType.Ffm:
                FixFqDataProcess();
                break;
            case FeatureType.Ffdf:
                FixDfDataProcess();
                break;
            case FeatureType.Scan:
                ScanDataProcess();
                break;
            case FeatureType.Tdoa:
                TdoaDataProcess();
                break;
            case FeatureType.Wbdf:
                WbdfDataProcess();
                break;
            case FeatureType.ScanDf:
                ScanDfDataProcess();
                break;
            case FeatureType.FScne:
            case FeatureType.MScne:
            case FeatureType.MScan:
                MScanDataProcess();
                break;
            case FeatureType.Ifmca:
                MchDataProcess();
                break;
            case FeatureType.Sse:
                SseDataProcess();
                break;
            case FeatureType.MScanDf:
                MScanDfDataProcess();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(curFeature));
        }
    }

    /// <summary>
    ///     单频测量数据处理
    /// </summary>
    private void FixFqDataProcess()
    {
        var startMeasureTime = DateTime.Now;
        _random.Next(1, 799);
        while (TaskState == TaskState.Start)
        {
            var timeSpan = DateTime.Now - startMeasureTime;
            //1.如果是Fast，则发送数据
            //2.如果不是Fast，则只有当时间间隔大于测量时间以后再发送
            var bMeasure = Detector == DetectMode.Fast || timeSpan.TotalMilliseconds > MeasureTime / 1000.0f;
            var data = new List<object>();
            if (!bMeasure)
            {
                Thread.Sleep(10);
                continue;
            }

            if ((_media & MediaType.Spectrum) > 0)
            {
                var spectrum = GetSpectrum(Frequency, IfBandwidth, _random);
                if (spectrum != null) data.Add(spectrum);
            }

            var level = GetLevel(Frequency, IfBandwidth, _random);
            if (level != null) data.Add(level);
            if ((_media & MediaType.Iq) > 0)
            {
                var iq = GetIq();
                data.Add(iq);
            }

            if ((_media & MediaType.Audio) > 0)
            {
                var diff1 = DateTime.Now.Subtract(_prevAudioReadTime);
                if (diff1.TotalMilliseconds >= _audioReadFrequency)
                {
                    var audio = GetAudio();
                    if (audio != null) data.Add(audio);
                    _prevAudioReadTime = DateTime.Now;
                }
            }

            if ((_media & MediaType.Itu) > 0)
            {
                var itu = GetItu(Frequency, FilterBandwidth, _random);
                if (itu != null) data.Add(itu);
            }

            if (data.Count > 0)
            {
                SendData(data);
                startMeasureTime = DateTime.Now;
                Thread.Sleep(10);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    /// <summary>
    ///     单频测向数据处理
    /// </summary>
    private void FixDfDataProcess()
    {
        var startMeasureTime = DateTime.Now;
        while (TaskState == TaskState.Start)
        {
            var timeSpan = DateTime.Now - startMeasureTime;
            var bMeasure = Detector == DetectMode.Fast || timeSpan.TotalMilliseconds >= MeasureTime / 1000.0f;
            // timeSpan = DateTime.Now - startAvgTime;
            var data = new List<object>();
            if (!bMeasure)
            {
                Thread.Sleep(1);
                continue;
            }

            var levelValue = 0.0f;
            if ((_media & MediaType.Level) > 0)
            {
                var level = GetLevel(Frequency, DfBandwidth, _random);
                if (level != null)
                {
                    levelValue = level.Data;
                    data.Add(level);
                }
            }

            if ((_media & MediaType.Spectrum) > 0)
            {
                var spectrum = GetSpectrum(Frequency, DfBandwidth, _random);
                if (spectrum != null) data.Add(spectrum);
            }

            if ((_media & MediaType.Audio) > 0)
            {
                var diff1 = DateTime.Now.Subtract(_prevAudioReadTime);
                if (diff1.TotalMilliseconds >= _audioReadFrequency - 5)
                {
                    var audio = GetAudio();
                    if (audio != null) data.Add(audio);
                    _prevAudioReadTime = DateTime.Now;
                }
            }

            startMeasureTime = DateTime.Now;
            var dfind = GetDfind();
            if (levelValue < LevelThreshold || dfind.Quality < QualityThreshold)
                // -1 约定为无效值
                dfind.Azimuth = -1;
            data.Add(dfind);
            // DateTime startAvgTime = DateTime.Now;
            if (data.Count > 0)
            {
                SendData(data);
                Thread.Sleep(10);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void WbdfDataProcess()
    {
        //表示是否继续平均
        var count = 0;
        while (TaskState == TaskState.Start)
        {
            var data = new List<object>
            {
                GetWbdfSpectrum()
            };
            count++;
            if (count > AvgTimes)
            {
                count = 0;
                data.Add(GetDFindPan());
            }

            if (data.Count > 0)
            {
                SendData(data);
                Thread.Sleep(10);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    /// <summary>
    ///     扫描测向数据处理
    /// </summary>
    private void ScanDfDataProcess()
    {
        //表示是否继续平均
        var count = 0;
        while (TaskState == TaskState.Start)
        {
            var data = new List<object>
            {
                GetScanDf()
            };
            count++;
            if (count > AvgTimes)
            {
                count = 0;
                data.Add(GetScanDfLevel());
            }

            if (data.Count > 0)
            {
                SendData(data);
                Thread.Sleep(50);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void TdoaDataProcess()
    {
        var lastTime = DateTime.Now.AddSeconds(-1);
        while (TaskState == TaskState.Start)
        {
            var data = new List<object>();
            if (_levelSwitch)
            {
                var level = GetLevel(Frequency, IfBandwidth, _random);
                data.Add(level);
            }

            if (_spectrumSwitch)
            {
                var spectrum = GetSpectrum(Frequency, IfBandwidth, _random);
                data.Add(spectrum);
            }

            if (data.Count > 0) SendData(data);
            if (DateTime.Now.Subtract(lastTime).TotalSeconds > 1)
            {
                var tdoa = GetTdoaData();
                lastTime = DateTime.Now;
                SendData([tdoa]);
            }

            Thread.Sleep(10);
        }
    }

    private void MScanDataProcess()
    {
        //测量时间相关
        var startMeasureTime = DateTime.Now;
        //驻留时间相关
        double goalDwellTimeSpan = 0;
        var startDwellTime = DateTime.Now;
        var bDwell = false;
        //等待时间相关
        var goalHoldTimeSpan = HoldTime * 1000;
        var startHoldTime = DateTime.Now;
        var bHold = true;
        while (TaskState.Start == TaskState)
        {
            double goalMeasureTimeSpan = Detector != DetectMode.Fast ? MeasureTime / 1000.0f : 0;
            var data = new List<object>();
            var dwellScan = GetDwellScan();
            if (dwellScan == null)
            {
                Thread.Sleep(1);
                continue;
            }

            var freq = Frequency;
            var span = IfBandwidth;
            if (ScanMode == ScanMode.Fscan)
            {
                freq = StartFrequency + _index * StepFrequency / 1000;
                span = IfBandwidth;
            }
            else if (ScanMode == ScanMode.MScan)
            {
                lock (_lockMscanPoints)
                {
                    if (_index >= _mscanPoints.Length) _index = 0;
                    freq = Convert.ToDouble(_mscanPoints[_index][ParameterNames.Frequency]);
                    span = Convert.ToDouble(_mscanPoints[_index][ParameterNames.FilterBandwidth]);
                }
                // span = _ifBandwidth;
            }

            var dt = DateTime.Now;
            var ts = dt - startMeasureTime;
            var bMeasure = ts.TotalMilliseconds < goalMeasureTimeSpan;
            if (!bMeasure)
            {
                data.Add(dwellScan);
                startMeasureTime = DateTime.Now;
            }

            if (bHold)
            {
                ts = dt - startHoldTime;
                bHold = ts.TotalMilliseconds < goalHoldTimeSpan && SquelchSwitch &&
                        dwellScan.Data[0] < SquelchThreshold * 10;
                if (!bHold)
                {
                    if (SquelchSwitch && dwellScan.Data[0] >= SquelchThreshold * 10)
                    {
                        //继续驻留
                        if (goalDwellTimeSpan <= 0)
                        {
                            goalDwellTimeSpan = DwellTime * 1000;
                            startDwellTime = dt;
                        }

                        bDwell = true;
                    }
                    else if (ts.TotalMilliseconds >= goalHoldTimeSpan)
                    {
                        //切换频点
                        goalDwellTimeSpan = 0;
                        _index = _index + dwellScan.Data.Length >= dwellScan.Total ? 0 : _index + dwellScan.Data.Length;
                        //为了确保每个频点至少有一包数据
                        if (bMeasure)
                        {
                            data.Add(dwellScan);
                            startMeasureTime = DateTime.Now;
                        }

                        bHold = true;
                    }
                }
            }

            if (bDwell)
            {
                ts = dt - startDwellTime;
                bDwell = ts.TotalMilliseconds < goalDwellTimeSpan;
                if (!bDwell)
                {
                    goalDwellTimeSpan = 0;
                    _index = _index + dwellScan.Data.Length >= dwellScan.Total ? 0 : _index + dwellScan.Data.Length;
                    //为了确保每个频点至少有一包数据
                    if (bMeasure)
                    {
                        data.Add(dwellScan);
                        startMeasureTime = DateTime.Now;
                    }

                    bHold = true;
                }
            }

            //进入驻留时间才返回频谱数据和音频数据
            if ((_media & MediaType.Spectrum) > 0 && bDwell && CurFeature == FeatureType.MScne)
                //若是有扫描数据再添加频谱数据
                if (data.Count > 0)
                {
                    var spectrum = GetSpectrum(freq, span, _random);
                    if (spectrum != null) data.Add(spectrum);
                }

            if ((_media & MediaType.Audio) > 0 && bDwell && CurFeature == FeatureType.MScne)
            {
                var audio = GetAudio();
                if (audio != null) data.Add(audio);
            }

            if (data.Count > 0)
                SendData(data);
            else
                Thread.Sleep(1);
        }
    }

    private void InitAudioData()
    {
        if (_listAudio.Count != 0) return;
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audiodata.bin");
        if (!File.Exists(filePath)) return;
        using var fsAudio = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        for (var index = 0; (index + 1) * AudioPacketLen <= fsAudio.Length; index++)
        {
            var data = new byte[AudioPacketLen];
            _ = fsAudio.Read(data, 0, AudioPacketLen);
            _listAudio.Add(data);
        }
    }

    private void ScanDataProcess()
    {
        var cacheData = new List<short>();
        var startMeasureTime = DateTime.Now;
        while (TaskState == TaskState.Start)
        {
            var data = new List<object>();
            var scan = GetScan();
            var timeSpan = DateTime.Now - startMeasureTime;
            var bMeasure = Detector == DetectMode.Fast || timeSpan.TotalMilliseconds >= MeasureTime / 1000.0f;
            if (scan != null)
            {
                cacheData.AddRange(scan.Data);
                if (bMeasure)
                {
                    scan.Data = new short[cacheData.Count];
                    Array.Copy(cacheData.ToArray(), 0, scan.Data, 0, cacheData.Count);
                    data.Add(scan);
                    cacheData.Clear();
                    startMeasureTime = DateTime.Now;
                }
            }

            if (data.Count > 0)
                SendData(data);
            else
                Thread.Sleep(1);
        }
    }

    private void SseDataProcess()
    {
        // DateTime startMeasureTime = DateTime.Now;
        // DateTime startAvgTime = startMeasureTime;
        while (TaskState == TaskState.Start)
        {
            // TimeSpan timeSpan = DateTime.Now - startMeasureTime;
            // timeSpan = DateTime.Now - startAvgTime;
            var data = new List<object>();
            var level = GetLevel(Frequency, DfBandwidth, _random);
            if (level != null)
                // float levelValue = level.Data;
                data.Add(level);
            if ((_media & MediaType.Spectrum) > 0)
            {
                var spectrum = GetSpectrum(Frequency, DfBandwidth, _random);
                if (spectrum != null) data.Add(spectrum);
            }

            if ((_media & MediaType.Audio) > 0)
            {
                var diff1 = DateTime.Now.Subtract(_prevAudioReadTime);
                if (diff1.TotalMilliseconds >= _audioReadFrequency - 35)
                {
                    var audio = GetAudio();
                    if (audio != null) data.Add(audio);
                    _prevAudioReadTime = DateTime.Now;
                }
            }

            // startMeasureTime = DateTime.Now;
            var sse = GetSseData();
            if (sse != null) data.Add(sse);
            // startAvgTime = DateTime.Now;
            if (data.Count > 0)
            {
                SendData(data);
                Thread.Sleep(10);
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void MScanDfDataProcess()
    {
        var index = 0;
        while (TaskState.Start == TaskState)
        {
            var data = new List<object>();
            var freq = Convert.ToDouble(_mscandfPoints[index][ParameterNames.Frequency]);
            var total = _mscandfPoints.Length;
            var datum = GetMScanDf(freq, total, index);
            data.Add(datum);
            SendData(data);
            index++;
            if (index >= total) index = 0;
            Thread.Sleep(30);
        }
    }

    #region 中频多路

    /// <summary>
    ///     控制中频多路分析调度逻辑
    /// </summary>
    private AutoResetEvent _mchEvent;

    private readonly object _lockChannel = new();
    private bool[] _isChannelRunning;

    /// <summary>
    ///     中频多路数据模拟线程
    /// </summary>
    private void MchDataProcess()
    {
        _mchEvent = new AutoResetEvent(true);
        var listChannelThread = new Thread[MaxChanCount].ToList();
        _isChannelRunning = new bool[MaxChanCount];
        Dictionary<string, object>[] preChannels = null;
        //首先开启主通道数据模拟线程
        var mainThread = new Thread(MainChannelDataProcess)
        {
            Name = $"{DeviceInfo.DisplayName}.MainChannelDataProcess",
            IsBackground = true
        };
        mainThread.Start();
        //以下控制各路子通道
        while (TaskState == TaskState.Start)
        {
            _mchEvent.WaitOne(1000);
            Dictionary<string, object>[] currChannels;
            lock (_lockChannel)
            {
                _ifMultiChannels ??= [];
                //先创建一个副本，以免在比较过程中_ifMultiChannels参数更改
                currChannels = (Dictionary<string, object>[])_ifMultiChannels.Clone();
            }

            if (IsChannelsChanged(preChannels, currChannels))
            {
                preChannels = currChannels;
                for (var i = 0; i < listChannelThread.Count; ++i)
                    if (i <= preChannels.Length - 1)
                    {
                        if (listChannelThread[i] == null)
                        {
                            _isChannelRunning[i] = true;
                            var thread = new Thread(ChannelDataProcess)
                            {
                                Name = $"{DeviceInfo.DisplayName}.ChannelDataProcess 【{i}】",
                                IsBackground = true
                            };
                            thread.Start(i);
                            listChannelThread[i] = thread;
                        }
                    }
                    else if (listChannelThread[i] != null)
                    {
                        _isChannelRunning[i] = false;
                        if (listChannelThread[i].IsAlive)
                            try
                            {
                                listChannelThread[i].Join();
                            }
                            catch
                            {
                                // ignored
                            }

                        listChannelThread[i] = null;
                    }
            }
        }

        try
        {
            mainThread.Join();
        }
        catch
        {
            // ignored
        }

        for (var i = 0; i < listChannelThread.Count; i++)
        {
            _isChannelRunning[i] = false;
            var chanThread = listChannelThread[i];
            try
            {
                chanThread?.Join();
            }
            catch
            {
                // ignored
            }
        }
    }

    private bool IsChannelsChanged(Dictionary<string, object>[] preChannels, Dictionary<string, object>[] currChannels)
    {
        if (preChannels == null || preChannels.Length != currChannels.Length) return true;
        for (var i = 0; i < currChannels.Length; ++i)
        {
            var tempDicNew = currChannels[i];
            var tempDicOld = preChannels[i];
            foreach (var item in tempDicOld)
            {
                if (item.Key is ParameterNames.MaximumSwitch or ParameterNames.MinimumSwitch
                    or ParameterNames.MeanSwitch or ParameterNames.NoiseSwitch
                    or ParameterNames.UnitSelection) continue;
                if (!Equals(item.Value, tempDicNew[item.Key])) return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     中频多路主通道数据模拟线程
    /// </summary>
    private void MainChannelDataProcess()
    {
        //主通道只返回频谱数据即可
        while (TaskState == TaskState.Start)
        {
            var data = new List<object>();
            var dataSpectrum = GetSpectrum(Frequency, IfBandwidth, _random);
            data.Add(dataSpectrum);
            SendData(data);
            Thread.Sleep(10);
        }
    }

    /// <summary>
    ///     中频多路子通道数据模拟线程
    /// </summary>
    /// <param name="chanNo">从0开始的通道号</param>
    private void ChannelDataProcess(object chanNo)
    {
        Console.WriteLine($"通道{chanNo}启动");
        //音频模拟数据包索引
        var indexAudio = 0;
        //电平和频谱50ms出一帧，音频200ms出一帧，此处简写为按次数来大概计时
        var count = 0;
        var index = (int)chanNo;
        var random = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")) + (int)chanNo);
        while (TaskState == TaskState.Start)
        {
            if (!_isChannelRunning[index]) break;
            IfMultiChannelTemplate channel = null;
            lock (_lockChannel)
            {
                if (_ifMultiChannelsArray != null && _ifMultiChannelsArray.Length > (int)chanNo)
                    channel = _ifMultiChannelsArray[(int)chanNo];
            }

            if (channel == null)
            {
                Thread.Sleep(10);
                continue;
            }

            var data = new List<object>();
            var dataMch = new SDataDdc
            {
                ChannelNumber = index,
                Data = []
            };
            if (channel.LevelSwitch && channel.IfSwitch)
            {
                var dataLevel = GetLevel(channel.Frequency, channel.FilterBandwidth, random);
                if (dataLevel != null) dataMch.Data.Add(dataLevel);
            }

            if (channel.SpectrumSwitch && channel.IfSwitch)
            {
                var dataSpectrum = GetSpectrum(channel.Frequency, channel.FilterBandwidth, random);
                if (dataSpectrum != null) dataMch.Data.Add(dataSpectrum);
            }

            if (channel.AudioSwitch && count % 4 == 0 && channel.IfSwitch)
            {
                //音频约200ms发送一次保证与本包音频实际播放时长一致
                var dataAudio = new SDataAudio
                {
                    Format = AudioFormat.Pcm,
                    SamplingRate = 22050,
                    Data = new byte[AudioPacketLen]
                };
                if (indexAudio >= _listAudio.Count) indexAudio = 0;
                Buffer.BlockCopy(_listAudio[indexAudio], 0, dataAudio.Data, 0, AudioPacketLen);
                dataMch.Data.Add(dataAudio);
                indexAudio++;
            }

            count++;
            if (dataMch.Data.Count > 0)
            {
                data.Add(dataMch);
                SendData(data);
            }

            Thread.Sleep(40);
        }
    }

    #endregion

    #endregion

    #region 释放非托管资源

    private void ReleaseResource()
    {
        base.Stop();
        if (_socket != null)
        {
            try
            {
                _socket.Close();
            }
            catch
            {
                // 容错代码
            }

            _socket = null;
        }

        _ = CancelTaskAsync(_dataProcessTask, _dataProcessCts);
        _listAudio.Clear();
        lock (_lockTdoa)
        {
            _tdoaData.Clear();
        }
    }

    private static async Task CancelTaskAsync(Task task, CancellationTokenSource tokenSource)
    {
        if (tokenSource == null) return;
        try
        {
            if (task?.IsCompleted == false) return;
            await tokenSource.CancelAsync();
            if (task != null)
            {
                await task.WaitAsync(TimeSpan.FromSeconds(5));
                task.Dispose();
            }
        }
        catch (AggregateException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            tokenSource.Dispose();
        }
    }

    #endregion
}