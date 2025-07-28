using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

#pragma warning disable RCS1181, RCS0014, RCS1146, RCS1202, RCS1211
namespace Magneto.Device.MR5000A;

public partial class Mr5000A : DeviceBase
{
    #region 构造函数

    public Mr5000A(Guid id) : base(id)
    {
        _isDisposed = false;
    }

    #endregion

    #region 成员变量

    private bool _isDisposed;

    //
    // 常量
    private readonly string[] _identifiers = { "audio", "ddc", "data", "gps", "compass" };
    private readonly double _epsilon = 1.0E-7d;
    private readonly int _maxDdcCount = 64;

    private readonly int _normSamplingRateIqCount = 4096;

    //
    // 同步锁
    private readonly object _ctrlChannelLock = new(); // 控制通道锁
    private readonly object _identifierLock = new(); // 数据标识同步锁 
    private readonly object _parameterLock = new(); // 参数同步锁
    private readonly object _gpsLock = new(); // GPS数据同步锁
    private readonly object _ddcLock = new(); // 中频多路数据同步锁
    private readonly object _scanDfLock = new(); // 扫描测向参数同步锁
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

    public object InvalidCountLock { get; } = new();
    public object InvalidCountLock1 => InvalidCountLock;
    public object LevelSpectrumLock { get; } = new();
    private NetworkStream _networkStream;
    private StreamReader _streamReader;
    private IDictionary<string, Socket> _channels;
    private IDictionary<string, Thread> _captures;
    private IDictionary<string, Thread> _dispatches;

    private IDictionary<string, MQueue<byte[]>> _queues; // 数据队列集合

    //
    // 业务数据
    private DataType _subscribedData; // 当前订阅的主通道业务数据

    // 
    // 扫描配置
    private int _invalidScanCount; // 无效扫描次数
    private int _scanDataLength; // 频段离散总点数
    private int _invalidSpectrumCount; // 无效频谱次数

    private int _invalidLevelCount; // 无效电平次数

    //
    // 缓存
    private SDataGps _bufferedGps; // 缓存GPS数据
    private DateTime _preGpsTimeStamp; // 缓存GPS时间戳

    private IfmcaTemplate[] _prevDdcSettings; // 缓存前一次DDC参数设置

    //
    // 修正值
    private IDictionary<long, long> _frequencyOffsetDic; // 频率修正表

    private IDictionary<long, long> _reverseFrequencyOffsetDic; // 频率逆向修正表

    //
    // 测向天线
    private DfAntennaInfo[] _dfindAntennas; // 配置的测向天线
    private int _dfAntennaGroupCount; // 测向天线分组数（用于测向算法）

    private int _dfAntennaIndex; // 当前选中的天线序号（包括监测和测向）

    //
    // 测向数据相关，参数对照表，校准器，数据源，过滤器，接收器
    private IDictionary<string, string> _refDfParameters; // 测向参数对照表
    private IDictionary<int, IDFindCalibration> _dualDFindCalibrators; // 双通道测向数据校准器（应用于收数测向）
    private IDictionary<int, IDFindCalibration> _nineDFindCalibrators; // 九通道测向数据校准器（应用于收数测向）
    private IDictionary<int, float[]> _channelPhaseDiffDic; // 九通道通道相位差，键为带宽（单位：kHz），值为接收机每次开机时的通道相位差
    private IChannelCalibration _nineChannelCalibrator; // 九通道通道校准器
    private IBandCalibration _nineBandCalibrator; // 九通道带内校准器
    private IDataSource _dfDataSource;
    private IDataFilter _dfindPhaseFilterforDuplexChannel;
    private IDataSinker _dfindPhaseSinkerforDuplexChannel;
    private IDataFilter _wbdfPhaseFilterforDuplexChannel;
    private IDataSinker _wbdfPhaseSinkerforDuplexChannel;
    private IDataFilter _scanDfPhaseFilterforDuplexChannel;
    private IDataSinker _scanDfPhaseSinkerforDuplexChannel;
    private IDataFilter _dfindPhaseFilterforNineChannel;
    private IDataSinker _dfindPhaseSinkerforNineChannel;
    private IDataFilter _sseRawFilterforNineChannel;
    private IDataSinker _sseRawSinkerforNineChannel;
    private IDataFilter _wbdfPhaseFilterforNineChannel;
    private IDataSinker _wbdfPhaseSinkerforNineChannel;
    private IDataFilter _levelRawFilterforNineChannel;
    private IDataSinker _levelRawSinkerforNineChannel;
    private IDataFilter _spectrumRawFilterforNineChannel;
    private IDataSinker _spectrumRawSinkerforNineChannel;
    private IDataFilter _scanDfPhaseFilterforNineChannel;
    private IDataSinker _scanDfPhaseSinkerforNineChannel;
    private IDictionary<double, double> _bandwidthAndSamplingRateDic;
    private IDictionary<string, IList<double>> _bandwidthAndSamplingRateInParamsDic;
    private dynamic _bufferedIq;
    private float _levelCalibrationForFixFq;
    private float _levelCalibrationForScan;
    private AntennaRefFrequencyRange[] _exceptionalAntennaRefFrequencyRanges;
    private Task<Tuple<int, long, long, int, int>> _ddrParameterReadingTask;
    private CancellationTokenSource _ddrCts;
    private readonly Dictionary<int, List<byte>> _ddcAudioBuffer = new();

    #endregion

    #region 重写父类方法

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        try
        {
            var result = base.Initialized(moduleInfo);
            if (result)
            {
                InitMiscs();
                InitNetworks();
                InitChannelPhaseDiffs();
                InitAntennas();
                InitDfComponents();
                InitChannels();
                InitHandshake();
                InitThreads();
                SetHeartBeat(WritingChannel);
            }

            return result;
        }
        catch (Exception)
        {
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
        if (name.Equals("frequency", StringComparison.OrdinalIgnoreCase) && value != null)
        {
            var temp = (long)(Convert.ToDouble(value) * 1000000);
            if (_frequencyOffsetDic.TryGetValue(temp, out var value1)) value = value1 / 1000000.0d;
        }

        if (name.Equals("levelthreshold", StringComparison.OrdinalIgnoreCase))
        {
            var temp = Convert.ToInt32(value);
            value = temp - LevelCalibrationFromDf;
        }

        //
        // 当前判断及以下分支纯属胡扯
        if (TaskState == TaskState.Start)
        {
            CancelTask();
            if ((CurFeature & FeatureType.ScanDF) == 0) Thread.Sleep(100);
            base.SetParameter(name, value);
            RequestTask(_subscribedData);
        }
        else
        {
            base.SetParameter(name, value);
        }

        var propertyNameValue = Utils.GetPropertyNameValue(name, this);
        //
        // 与测向相关的参数
        if (name.Equals("spectrumspan", StringComparison.OrdinalIgnoreCase)
            || name.Equals("dfbandwidth", StringComparison.OrdinalIgnoreCase)
            || name.Equals("dfsamplingcount", StringComparison.OrdinalIgnoreCase)
            || name.Equals("integrationtime", StringComparison.OrdinalIgnoreCase))
        {
            var count = GetDfavgTimes();
            RaiseDfParameterSelection("AVGTimes", count);
        }
        else if (name.Equals("integrationgear", StringComparison.OrdinalIgnoreCase))
        {
            if (propertyNameValue?.Item2 != null)
                RaiseDfParameterSelection("DFSamplingCount", (int)(128 * Math.Pow(2, (int)propertyNameValue.Item2)));
        }

        RaiseDfParameterSelection(propertyNameValue?.Item1, propertyNameValue?.Item2);
        // 运行时修改参数
        // 单频测量/测向或类单频功能，在参数有变更的情况下都需要清理缓存，保证实时数据的实时响应
        if (TaskState == TaskState.Start &&
            (CurFeature & (FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne | FeatureType.MScne)) ==
            0) ClearAll();
        // 测向控制
        if (TaskState == TaskState.Start &&
            (CurFeature & (FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF | FeatureType.SSE)) > 0)
            //天线采用极化与频率自动的方式切换，因此当相应的参数变更时，需要及时选择天线，同时清空无效的测向缓存数据
            if (name.Equals("frequency", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("dfpolarization", StringComparison.OrdinalIgnoreCase))
                RaiseDfAntennaSelection();
        _invalidSpectrumCount = InvalidScanAndSpectrumCount;
        _invalidLevelCount = InvalidScanAndSpectrumCount;
    }

    public override void Dispose()
    {
        _isDisposed = true;
        ReleaseResource();
        base.Dispose();
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
        _preGpsTimeStamp = DateTime.MinValue;
        _bufferedGps = new SDataGps();
        //
        // 预定义的DDC通道
        _prevDdcSettings = new IfmcaTemplate[_maxDdcCount];
        for (var index = 0; index < _prevDdcSettings.Length; ++index)
            _prevDdcSettings[index] = new IfmcaTemplate
            {
                Frequency = 101.7d,
                FilterBandwidth = 120.0d,
                DemMode = Modulation.Fm,
                Beta = 1.0f,
                XdB = 26.0f,
                IfSwitch = false
            };
        //
        // 带宽采样率关系表
        var bandwidthAndSamplingRateArray = new[,]
        {
            { 160000.0d, 204.8d }, { 80000.0d, 102.4d },
            { 40000.0d, 51.2d }, { 20000.0d, 51.2d }, { 10000.0d, 12.8d }, { 5000.0d, 6.4d },
            { 2000.0d, 3.2d }, { 1000.0d, 1.6d }, { 500.0d, 0.8d }, { 200.0d, 0.4d },
            { 150.0d, 0.2d }, { 120.0d, 0.2d }, { 100.0d, 0.2d }, { 50.0d, 0.1d },
            { 20.0d, 0.05d }, { 10.0d, 0.025d }, { 5.0d, 0.025d }, { 2.0d, 0.025d },
            { 1.0d, 0.025d }
        };
        _bandwidthAndSamplingRateDic = new Dictionary<double, double>();
        var count = bandwidthAndSamplingRateArray.Length / bandwidthAndSamplingRateArray.Rank;
        for (var index = 0; index < count; ++index)
            _bandwidthAndSamplingRateDic[bandwidthAndSamplingRateArray[index, 0]] =
                bandwidthAndSamplingRateArray[index, 1];
        //
        // 带宽与全采样率关系表
        var bandwidthAndSamplingRateJsonTable =
            @"{ '160000': [ 204800 ], '80000': [ 102400 ], '40000': [ 51200 ], '25600': [ 51200 ], '20000': [ 25600 ], '12800': [ 25600 ], '10000': [ 12800 ], 
				'6400': [ 12800, 51200 ], '5000': [ 6400 ], '3200': [ 6400, 25600 ], '2500': [ 3200 ], '1600': [ 3200, 12800 ], '1250': [ 1600 ], 
				'1000': [ 1280 ], '800': [ 1600, 6400 ], '640': [ 1280 ], '625': [ 800 ], '500': [ 640 ], '400': [ 800, 3200 ], '320': [ 640 ], 
				'312.5': [ 400 ], '250': [ 320 ], '200': [ 400, 1600 ], '160': [ 320, 1280 ], '156.25': [ 200 ], '125': [ 160 ], '100': [ 128, 200, 800 ], 
				'80': [ 160, 640 ], '78.125': [ 100 ], '64': [ 128 ], '50': [ 64, 100, 400 ], '40': [ 320 ], '39.0625': [ 50 ], '32': [ 64 ], '25': [ 50, 200 ], 
				'20': [ 160 ], '19.53125': [ 25 ], '16': [ 128 ], '12.5': [ 25, 100 ], '8': [ 64 ], '6.25': [ 50 ], '3.125': [ 25 ] }";
        _bandwidthAndSamplingRateInParamsDic =
            Utils.ConvertFromJson<Dictionary<string, IList<double>>>(bandwidthAndSamplingRateJsonTable);
        //
        // 模块参数与数据源参数对照表
        var refDfParameterArray = new[,]
        {
            { "Frequency", "Frequency" },
            { "DFBandwidth", "Bandwidth" },
            { "ResolutionBandwidth", "DFBandwidth" },
            { "AVGTimes", "AVGTimes" },
            { "DFSamplingCount", "DFSamplingCount" },
            { "LevelThreshold", "LevelThreshold" },
            { "QualityThreshold", "QualityThreshold" },
            { "EstimatedSSECount", "EstimatedSSECount" },
            { "IntegratedSSETimes", "IntegratedSSETimes" },
            { "SSEAutomaticMethod", "SSEAutomaticMethod" },
            { "SSEAutomaticCoe", "SSEAutomaticCoe" },
            { "StartFrequency", "StartFrequency" },
            { "StopFrequency", "StopFrequency" },
            { "StepFrequency", "StepFrequency" }
        };
        _refDfParameters = new Dictionary<string, string>();
        count = refDfParameterArray.Length / refDfParameterArray.Rank;
        for (var index = 0; index < count; ++index)
            _refDfParameters[refDfParameterArray[index, 0]] = refDfParameterArray[index, 1];
        _bufferedIq = new ExpandoObject();
        _bufferedIq.TimestampSecond = 0;
        _bufferedIq.TimestampNanoSecond = 0;
        _bufferedIq.Data = null;
        _bufferedIq.SynCode = int.MinValue;
        //
        // 频率修正
        _frequencyOffsetDic = new Dictionary<long, long>();
        _reverseFrequencyOffsetDic = new Dictionary<long, long>();
        //
        // 配置信息
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Devices\MR5000A.ini");
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
                    if (pairs == null) break;
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
                case "level_calibration_for_fixfq":
                    if (!float.TryParse(config[1].Trim(), out _levelCalibrationForFixFq)) _levelCalibrationForFixFq = 0;
                    break;
                case "level_calibration_for_scan":
                    if (!float.TryParse(config[1].Trim(), out _levelCalibrationForScan)) _levelCalibrationForScan = 0;
                    break;
                case "antenna_ref":
                {
                    var antennaRefFrequencyRanges = new List<AntennaRefFrequencyRange>();
                    var frequencyRanges = config[1].Trim().Split(new[] { ';' });
                    if (frequencyRanges == null) break;
                    foreach (var frequencyRange in frequencyRanges)
                        try
                        {
                            var indexRangePair = frequencyRange.Trim().Split(new[] { ':' });
                            if (indexRangePair is not { Length: 2 }) continue;
                            var index = int.Parse(indexRangePair[0].Trim());
                            var startStopPair = indexRangePair[1].Trim().Split(new[] { ',' });
                            if (startStopPair is not { Length: 2 }) continue;
                            var start = double.Parse(startStopPair[0].Trim());
                            var stop = double.Parse(startStopPair[1].Trim());
                            antennaRefFrequencyRanges.Add(new AntennaRefFrequencyRange
                                { AntennaReferenceType = index, StartFrequency = start, StopFrequency = stop });
                        }
                        catch
                        {
                        }

                    _exceptionalAntennaRefFrequencyRanges = antennaRefFrequencyRanges.Count > 0
                        ? antennaRefFrequencyRanges.ToArray()
                        : null;
                }
                    break;
            }
        }

        _invalidScanCount = InvalidScanAndSpectrumCount;
        _dfAntennaIndex = -1;
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

        // 注意：千万不要注释下面这行语句！！！
        SendCommand("SYST:COMM:ECHO OFF");
    }

    private void InitChannelPhaseDiffs()
    {
        _channelPhaseDiffDic = new Dictionary<int, float[]>();
        var result = GetChannelPhaseDiffsInString();
        if (string.IsNullOrEmpty(result))
        {
            _channelPhaseDiffDic[160000] = new float[DfChannelCount];
            _channelPhaseDiffDic[500] = new float[DfChannelCount];
        }
        else
        {
            var semicolonSplit = result.Split(new[] { ';' });
            foreach (var semicolon in semicolonSplit)
            {
                var colonSplit = semicolon.Split(new[] { ':' });
                if (colonSplit is { Length: 3 })
                {
                    var bandwidth = (int)(int.Parse(colonSplit[1]) / 1000.0d);
                    if (bandwidth is 40000 or 80000) bandwidth = 160000;
                    var commaSplit = colonSplit[2].Split(new[] { ',' });
                    if (commaSplit is { Length: > 0 })
                    {
                        _channelPhaseDiffDic[bandwidth] = new float[commaSplit.Length];
                        for (var index = 0; index < commaSplit.Length; ++index)
                            _channelPhaseDiffDic[bandwidth][index] = Convert.ToInt32(commaSplit[index]) / 10.0f;
                    }
                }
            }
        }
    }

    private void InitHandshake()
    {
        var token = GetAuthToken();
        if (!string.IsNullOrEmpty(token))
        {
            var key = DllInvoker.GenerateRadioKey(token);
            if (key != -1) SendCommand($"VERS:AUTH {key}");
        }
    }

    private void InitAntennas()
    {
        var path = _dfDataFolder == "." ? AppDomain.CurrentDomain.BaseDirectory : _dfDataFolder;
        var antennaConfigFileName = Path.Combine(path, "antennas.json");
        _dfindAntennas = DfAntennaInfo.CreateAntennas(antennaConfigFileName);
        if (_dfindAntennas is { Length: > 0 })
        {
            SendCommand($"ANT:COUN {_dfindAntennas.Length}");
            byte antennaIndex = 0;
            if (VerifyDFindAntennas(out var exception, ref antennaIndex))
                foreach (var antenna in _dfindAntennas)
                    SendCommand($"ANT:APP {antenna.ToProtocolString()}");
            else if (!string.IsNullOrEmpty(exception)) throw new Exception($"测向天线配置有误，{exception}");
        }
    }

    private void InitDfComponents()
    {
        // 双通道测向校准
        _dualDFindCalibrators = new Dictionary<int, IDFindCalibration>();
        // 九通道测向校准
        _nineDFindCalibrators = new Dictionary<int, IDFindCalibration>();
        // 九通道通道校准
        _nineChannelCalibrator =
            NineChannelCalibrator.CreateInstance(_dfDataFolder, _channelPhaseDiffDic?.Keys.ToArray());
        // 九通道带内校准
        _nineBandCalibrator = NineBandCalibrator.CreateInstance(_dfDataFolder);
        // 初始化测向校准器
        if (_dfindAntennas != null)
        {
            if (DfChannelCount == 2)
                foreach (var antenna in _dfindAntennas)
                    // 为每幅测向天线添加测向数据校准器
                    _dualDFindCalibrators[antenna.Index] = DualDFindCalibrator.CreateInstance(antenna, _dfDataFolder);
            else if (DfChannelCount == 9)
                foreach (var antenna in _dfindAntennas)
                    _nineDFindCalibrators[antenna.Index] = NineDFindCalibrator.CreateInstance(antenna, _dfDataFolder);
        }

        // 
        // 数据源
        _dfDataSource = new DataSource();
        //
        // 数据过滤器与接收器
        // 单频测向（双通道）
        _dfindPhaseSinkerforDuplexChannel = IsTheoryDFind
            ? new DFindPhaseDifferenceSinkerforDuplexChannel(null, DfAntennaRef, _exceptionalAntennaRefFrequencyRanges)
            : new DFindPhaseDifferenceSinkerforDuplexChannel(_dualDFindCalibrators, DfAntennaRef,
                _exceptionalAntennaRefFrequencyRanges);
        _dfindPhaseSinkerforDuplexChannel.DataArrived += OnDFRelatedDataArrived;
        _dfindPhaseFilterforDuplexChannel =
            new PhaseDifferenceFilterforDuplexChannel(_dfindPhaseSinkerforDuplexChannel);
        // 宽带测向（双通道）
        _wbdfPhaseSinkerforDuplexChannel = IsTheoryDFind
            ? new WbdfPhaseDifferenceSinkerforDuplexChannel(null, DfAntennaRef, _exceptionalAntennaRefFrequencyRanges)
            : new WbdfPhaseDifferenceSinkerforDuplexChannel(_dualDFindCalibrators, DfAntennaRef,
                _exceptionalAntennaRefFrequencyRanges);
        _wbdfPhaseSinkerforDuplexChannel.DataArrived += OnDFRelatedDataArrived;
        _wbdfPhaseFilterforDuplexChannel = new PhaseDifferenceFilterforDuplexChannel(_wbdfPhaseSinkerforDuplexChannel);
        // 扫描测向（双通道）
        _scanDfPhaseSinkerforDuplexChannel = IsTheoryDFind
            ? new ScanDfPhaseDifferenceSinkerforDuplexChannel(null, DfAntennaRef, _exceptionalAntennaRefFrequencyRanges)
            : new ScanDfPhaseDifferenceSinkerforDuplexChannel(_dualDFindCalibrators, DfAntennaRef,
                _exceptionalAntennaRefFrequencyRanges);
        _scanDfPhaseSinkerforDuplexChannel.DataArrived += OnDFRelatedDataArrived;
        _scanDfPhaseFilterforDuplexChannel =
            new ScanDfPhaseDifferenceFilterforDuplexChannel(_scanDfPhaseSinkerforDuplexChannel);
        // 单频测向（九通道）
        _dfindPhaseSinkerforNineChannel = IsTheoryDFind
            ? new DFindPhaseDifferenceSinkerforNineChannel(null, _nineChannelCalibrator, _nineBandCalibrator)
            : new DFindPhaseDifferenceSinkerforNineChannel(_nineDFindCalibrators, _nineChannelCalibrator,
                _nineBandCalibrator);
        _dfindPhaseSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _dfindPhaseFilterforNineChannel = new PhaseDifferenceFilterforNineChannel(_dfindPhaseSinkerforNineChannel);
        // 宽带测向（九通道）
        _wbdfPhaseSinkerforNineChannel = IsTheoryDFind
            ? new WbdfPhaseDifferenceSinkerforNineChannel(null, _nineChannelCalibrator, _nineBandCalibrator)
            : new WbdfPhaseDifferenceSinkerforNineChannel(_nineDFindCalibrators, _nineChannelCalibrator,
                _nineBandCalibrator);
        _wbdfPhaseSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _wbdfPhaseFilterforNineChannel = new PhaseDifferenceFilterforNineChannel(_wbdfPhaseSinkerforNineChannel);
        // 扫描测向（九通道）
        _scanDfPhaseSinkerforNineChannel = IsTheoryDFind
            ? new ScanDfPhaseDifferenceSinkerforNineChannel(null, _nineChannelCalibrator, _nineBandCalibrator)
            : new ScanDfPhaseDifferenceSinkerforNineChannel(_nineDFindCalibrators, _nineChannelCalibrator,
                _nineBandCalibrator);
        _scanDfPhaseSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _scanDfPhaseFilterforNineChannel =
            new ScanDfPhaseDifferenceFilterforNineChannel(_scanDfPhaseSinkerforNineChannel);
        // 空间谱测向（九通道）
        _sseRawSinkerforNineChannel = new SseRawSinkerforNineChannel(_nineChannelCalibrator, _channelPhaseDiffDic);
        _sseRawSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _sseRawFilterforNineChannel = new RawFilterforNineChannel(_sseRawSinkerforNineChannel);
        // 测向电平（九通道）
        _levelRawSinkerforNineChannel = new LevelSinkerforNineChannel();
        _levelRawSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _levelRawFilterforNineChannel = new LevelFilterforNineChannel(_levelRawSinkerforNineChannel);
        // 测向频谱（九通道）
        _spectrumRawSinkerforNineChannel = new SpectrumSinkerforNineChannel();
        _spectrumRawSinkerforNineChannel.DataArrived += OnDFRelatedDataArrived;
        _spectrumRawFilterforNineChannel = new SpectrumFilterforNineChannel(_spectrumRawSinkerforNineChannel);
    }

    private void InitChannels()
    {
        _ddrCts?.Cancel();
        SendCommand("TRAC OFF");
        var address = (_ctrlChannel.LocalEndPoint as IPEndPoint)?.Address.ToString();
        if (_channels.TryGetValue("audio", out var channel))
        {
            var audioPort = ((IPEndPoint)channel.LocalEndPoint)!.Port;
            SendCommand($"TRAC:UDP \"{address}\",{audioPort},AUDIO");
        }

        if (_channels.TryGetValue("ddc", out var channel1))
        {
            var ddcPort = ((IPEndPoint)channel1.LocalEndPoint)!.Port;
            SendCommand($"TRAC:UDP \"{address}\",{ddcPort},DDC");
        }

        if (_channels.TryGetValue("data", out var channel2))
        {
            var dataPort = ((IPEndPoint)channel2.LocalEndPoint)!.Port;
            SendCommand($"TRAC:UDP \"{address}\",{dataPort},IF");
            SendCommand($"TRAC:UDP \"{address}\",{dataPort},MEAS");
            SendCommand($"TRAC:UDP \"{address}\",{dataPort},DFIN");
            SendCommand($"TRAC:UDP \"{address}\",{dataPort},SCAN");
        }

        if (_channels.TryGetValue("gps", out var channel3))
        {
            var gpsPort = ((IPEndPoint)channel3.LocalEndPoint)!.Port;
            SendCommand($"TRAC:UDP \"{address}\",{gpsPort},GPS");
        }

        if (_channels.TryGetValue("compass", out var channel4))
        {
            var compassPort = ((IPEndPoint)channel4.LocalEndPoint)!.Port;
            SendCommand($"TRAC:UDP \"{address}\",{compassPort},COMPASS");
        }

        SendCommand($"HARD:GPS:ENAB {(EnableGps ? "ON" : "OFF")}");
        SendCommand($"HARD:COMP:ENAB {(EnableCompass ? "ON" : "OFF")}");
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
        if ((CurFeature & FeatureType.SCAN) == 0 || _scanMode != ScanMode.Pscan) SendCommand("SYST:RESP:IF OFF");
        if ((CurFeature & (FeatureType.WBDF | FeatureType.SCAN
                                            | FeatureType.TDOA | FeatureType.ScanDF)) > 0)
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
            if ((CurFeature & FeatureType.FFDF) > 0) // 设置很低的静噪门限，保证测向时可以输出音频
                SendCommand("MEAS:THR -120");
            if ((CurFeature & FeatureType.IFMCA) > 0) SendCommand("DEM FM");
        }

        // 只有TDOA才需要做GPS触发采集，其它功能下，做连续采集或按固定才度采集
        SendCommand((CurFeature & FeatureType.TDOA) > 0 ? "TRIG:TDO GPS" : "TRIG:TDO NON");
        SendCommand($"AMPL {(EnableRfAmplifier ? "ON" : "OFF")}");
        SendCommand($"MEAS:IQ:WIDT {IqWidth}");
    }

    private void SetDataByAbility()
    {
        if ((CurFeature & FeatureType.FFM) > 0)
        {
            SetFixFq();
        }
        else if ((CurFeature & FeatureType.FFDF) > 0)
        {
            SetFixDf();
            RaiseDfAntennaSelection();
        }
        else if ((CurFeature & FeatureType.WBDF) > 0)
        {
            SetWbdf();
            RaiseDfAntennaSelection();
        }
        else if ((CurFeature & FeatureType.SSE) > 0)
        {
            SetSse();
            RaiseDfAntennaSelection();
        }
        else if ((CurFeature & FeatureType.ScanDF) > 0)
        {
            SetScanDf();
            RaiseDfAntennaSelection();
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne)) > 0)
        {
            SetScan();
        }
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            SetMScan();
        }
        else if ((CurFeature & FeatureType.TDOA) > 0)
        {
            SetTdoa();
        }
        else if ((CurFeature & FeatureType.IFMCA) > 0)
        {
            SetDdc();
        }
        else if ((CurFeature & FeatureType.AmpDF) > 0)
        {
            SetAmpdf();
        }
    }

    private void PostSet()
    {
        if ((CurFeature & FeatureType.FFM) > 0)
        {
            SendCommand("FREQ:MODE FIX");
            WaitForModeSwitchFeedback();
        }
        else if ((CurFeature & (FeatureType.TDOA | FeatureType.AmpDF)) > 0)
        {
            SendCommand("FREQ:MODE FIX");
            WaitForModeSwitchFeedback();
        }
        else if ((CurFeature & (FeatureType.FFDF | FeatureType.WBDF | FeatureType.SSE)) > 0)
        {
            SendCommand("FREQ:MODE DFIN");
            WaitForModeSwitchFeedback();
        }
        else if ((CurFeature & FeatureType.ScanDF) > 0)
        {
            SendCommand("FREQ:MODE DFS");
            WaitForModeSwitchFeedback();
        }
        else if ((CurFeature & FeatureType.IFMCA) > 0)
        {
            SendCommand("FREQ:MODE DDC");
            WaitForModeSwitchFeedback();
        }
    }

    private void PreReset()
    {
        // deliberately left blank
    }

    private void ResetDataByAbility()
    {
        if ((CurFeature & FeatureType.FFM) > 0)
            ResetFixFq();
        else if ((CurFeature & FeatureType.FFDF) > 0)
            ResetFixDf();
        else if ((CurFeature & FeatureType.WBDF) > 0)
            ResetWbdf();
        else if ((CurFeature & FeatureType.SSE) > 0)
            ResetSse();
        else if ((CurFeature & FeatureType.ScanDF) > 0)
            ResetScanDf();
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne)) > 0)
            ResetScan();
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
            ResetMScan();
        else if ((CurFeature & FeatureType.TDOA) > 0)
            ResetTdoa();
        else if ((CurFeature & FeatureType.IFMCA) > 0)
            ResetDdc();
        else if ((CurFeature & FeatureType.AmpDF) > 0) ResetAmpdf();
        // 数据请求置空
        _subscribedData = DataType.None;
    }

    private void PostReset()
    {
        if (CurFeature == FeatureType.SCAN)
            Thread.Sleep(20);
        else if (CurFeature == FeatureType.IFMCA) // 按天津接收机开发团队的要求，从DDC功能切换出来，需要延时5秒后才能进行其它功能的测量
            Thread.Sleep(5000);
        // 停止任务后，清空所有的过滤器
        _dfDataSource.Clear();
    }

    private void SetFixFq()
    {
        // 单频测量默认需要向接收机请求电平数据和短信数据
        _subscribedData |= DataType.Level | DataType.Sms;
    }

    private void ResetFixFq()
    {
        // deliberately left blank
    }

    private void SetFixDf()
    {
        _subscribedData |= DataType.Dfc | DataType.Level | DataType.Spectrum;
        _dfDataSource.Register(
            DfChannelCount == 2 ? _dfindPhaseFilterforDuplexChannel : _dfindPhaseFilterforNineChannel);
    }

    private void ResetFixDf()
    {
        _dfDataSource.UnRegister(DfChannelCount == 2
            ? _dfindPhaseFilterforDuplexChannel
            : _dfindPhaseFilterforNineChannel);
        // 避免可能因为触发测向而且关闭了电子罗盘
        SendCommand($"HARD:COMP:ENAB {(EnableCompass ? "ON" : "OFF")}");
    }

    private void SetWbdf()
    {
        SendCommand("MEAS:DFIN:PAN ON");
        SendCommand("MEAS:DFIN:MODE NORM");
        _subscribedData |= DataType.Dfc;
        _dfDataSource.Register(DfChannelCount == 2 ? _wbdfPhaseFilterforDuplexChannel : _wbdfPhaseFilterforNineChannel);
    }

    private void ResetWbdf()
    {
        _dfDataSource.UnRegister(
            DfChannelCount == 2 ? _wbdfPhaseFilterforDuplexChannel : _wbdfPhaseFilterforNineChannel);
    }

    private void SetSse()
    {
        SendCommand("MEAS:DFIN:PAN ON");
        _subscribedData |= DataType.Dfiq;
        _dfDataSource.Register(_sseRawFilterforNineChannel);
    }

    private void ResetSse()
    {
        _dfDataSource.UnRegister(_sseRawFilterforNineChannel);
    }

    private void SetScanDf()
    {
        SendCommand($"FREQ:START {_startFrequency} MHz");
        SendCommand($"FREQ:STOP {_stopFrequency} MHz");
        SendCommand($"FREQ:STEP {_stepFrequency} kHz");
        _dfDataSource.Register(DfChannelCount == 2
            ? _scanDfPhaseFilterforDuplexChannel
            : _scanDfPhaseFilterforNineChannel);
        _subscribedData |= DataType.Dfc;
    }

    private void ResetScanDf()
    {
        _dfDataSource.UnRegister(DfChannelCount == 2
            ? _scanDfPhaseFilterforDuplexChannel
            : _scanDfPhaseFilterforNineChannel);
    }

    private void SetScan()
    {
        if ((CurFeature & FeatureType.SCAN) > 0) // 频段扫描
        {
            if (_scanMode == ScanMode.Pscan) // 此参数已暴露到功能
            {
                SendCommand("FREQ:MODE PSC");
                // SendCommand(string.Format("MEAS:PSC:IFSP {0} kHz", _fastPScan));
                WaitForModeSwitchFeedback();
                SendCommand("FREQ:PSC:MODE NORM"); // 常规的PSCAN 
                if (CalibPScanNoise) SendCommand("SYST:RESP:IF ON");
            }
            else if (_scanMode == ScanMode.Fscan)
            {
                SendCommand("FREQ:MODE SWE");
                WaitForModeSwitchFeedback();
            }

            // 
            // 当前功能不包含等待时间和驻留时间，因此需要隐式向接收机设置，全部置为零
            // SendCommand("MEAS:HOLD 0");
            SendCommand("MEAS:DWEL 0");
        }
        else
        {
            // 频段搜索采用FSCAN的模式
            SendCommand("FREQ:MODE SWE");
            WaitForModeSwitchFeedback();
            // SendCommand("MEAS:HOLD 0"); // 按业务需求，将等待时间设置为零（内部依赖测量时间）
            _scanMode = ScanMode.Fscan; // 此参数未暴露到用户能表，因此手动更新
        }

        // 按MR3300A接收机开发者的要求，扫描参数（起始频率、結束频率、扫描步进）需要后于频率模式设置
        SendCommand($"FREQ:START {_startFrequency} MHz");
        SendCommand($"FREQ:STOP {_stopFrequency} MHz");
        SendCommand($"FREQ:STEP {_stepFrequency} kHz");
        _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
        _subscribedData |= DataType.Scan;
    }

    private void ResetScan()
    {
        // delibrately left blank
    }

    private void SetMScan()
    {
        if (MScanPoints == null) return;
        SendCommand("FREQ:MODE MSC");
        WaitForModeSwitchFeedback();
        // SendCommand("MEAS:HOLD 0"); // 按业务需求，将等待时间设置为零（内部依赖测量时间）
        SendCommand($"MSC:COUN {MScanPoints.Length}");
        var cmdBuilder = new StringBuilder();
        for (var index = 0; index < MScanPoints.Length; ++index)
        {
            var cmd =
                $"MEM:CONT {index},{MScanPoints[index]["frequency"]} MHz,{MScanPoints[index]["filterBandwidth"]} kHz,{(Utils.ConvertStringToEnum<Modulation>(MScanPoints[index]["demMode"].ToString()) == Modulation.Pdt ? Modulation.Dmr : MScanPoints[index]["demMode"])};";
            cmdBuilder.Append(cmd);
            // if ((index + 1) % 50 == 0)
            // {
            // 	SendCommand(cmdBuilder.ToString().TrimEnd(';'));
            // 	cmdBuidler.Clear();
            // }
        }

        if (cmdBuilder.Length > 0)
        {
            SendCommand(cmdBuilder.ToString().TrimEnd(';'));
            cmdBuilder.Clear();
        }

        if (CurFeature == FeatureType.MScan) // 如果是离散扫描，等待时间和驻留时间需要隐式设置到接收机，取值为零
            SendCommand("MEAS:DWEL 0");
        _scanMode = ScanMode.MScan;
        _scanDataLength = MScanPoints.Length;
        _subscribedData |= DataType.Scan;
    }

    private void ResetMScan()
    {
        // clear all mscan frequencies
    }

    private void SetFastScan()
    {
        SendCommand("HARD:COMP:ENAB OFF");
        SendCommand("FREQ:MODE PSC");
        // SendCommand(string.Format("MEAS:PSC:IFSP {0} kHz", _fastPScan));
        WaitForModeSwitchFeedback();
        SendCommand("FREQ:PSC:MODE FAST"); // 快速扫描属于PSCAN的子模式
        if (CalibPScanNoise) SendCommand("SYST:RESP:IF ON");
        // 按MR3300A接收机开发者的要求，扫描参数（起始频率、結束频率、扫描步进）需要后于频率模式设置
        SendCommand($"FREQ:START {_startFrequency} MHz");
        SendCommand($"FREQ:STOP {_stopFrequency} MHz");
        SendCommand($"FREQ:STEP {_stepFrequency} kHz");
        _scanMode = ScanMode.Pscan;
        _subscribedData |= DataType.Scan;
    }

    private void ResetFastScan()
    {
        SendCommand($"HARD:COMP:ENAB {(EnableCompass ? "ON" : "OFF")}");
    }

    private void SetTdoa()
    {
        _subscribedData |= DataType.Iq;
    }

    private void ResetTdoa()
    {
        SendCommand("TRIG:TDO NON");
    }

    private void SetDdc()
    {
        lock (_ddcLock)
        {
            SquelchThreshold = -40;
            AutoAttenuation = false;
            _subscribedData |= DataType.Spectrum;
            SendCommand("DDC:CONT:FREQ OFF");
            if (_ddcChannels == null)
            {
                foreach (var t in _prevDdcSettings)
                    t.IfSwitch = false;

                SendCommand("DDC:CLOS:ALL");
            }
            else
            {
                // if (_prevDDCSettings != null && _prevDDCSettings.Length > _ddcChannels.Length)
                // {
                //     for (var index = _ddcChannels.Length; index < _prevDDCSettings.Length; index++)
                //     {
                //         SendCommand(string.Format("DDC:CONT {0},{1} MHz,{2} kHz,{3},None,{4},{5}",
                //                                   index,
                //                                   _prevDDCSettings[index].Frequency,
                //                                   _prevDDCSettings[index].FilterBandwidth,
                //                                   _prevDDCSettings[index].DemMode,
                //                                   _prevDDCSettings[index].Beta / 100.0f,
                //                                   _prevDDCSettings[index].XdB));
                //         _prevDDCSettings[index].IFSwitch = false;
                //     }
                // }
                SendCommand($"DDC:COUN {_ddcChannels.Length}");
                for (var index = 0; index < _ddcChannels.Length; ++index)
                {
                    var template = (IfmcaTemplate)_ddcChannels[index];
                    var frequency = template.Frequency;
                    var ifBandwidth = template.FilterBandwidth;
                    var demMode = template.DemMode;
                    var ifSwitch = template.IfSwitch;
                    var levelSwitch = template.LevelSwitch;
                    var spectrumSwitch = template.SpectrumSwitch;
                    var audioSwitch = template.AudioSwitch;
                    var xdB = template.XdB;
                    var beta = template.Beta;
                    // // 如果所有数据没有变化，则不进行任何变更
                    // if (Math.Abs(frequency - _prevDDCSettings[index].Frequency) <= _epsilon
                    //     && Math.Abs(ifBandwidth - _prevDDCSettings[index].FilterBandwidth) <= _epsilon
                    //     && demMode == _prevDDCSettings[index].DemMode
                    //     && ifSwitch == _prevDDCSettings[index].IFSwitch
                    //     && levelSwitch == _prevDDCSettings[index].LevelSwitch
                    //     && spectrumSwitch == _prevDDCSettings[index].SpectrumSwitch
                    //     && audioSwitch == _prevDDCSettings[index].AudioSwitch
                    //     && iqSwitch == _prevDDCSettings[index].IQSwitch
                    //     && Math.Abs(xdB - _prevDDCSettings[index].XdB) <= _epsilon
                    //     && Math.Abs(beta - _prevDDCSettings[index].Beta) <= _epsilon
                    //     )
                    // {
                    //     continue;
                    // }
                    var dataType = DataType.None;
                    if (ifSwitch) // 相当于数据总开关，如果为False，数据类型默认为None
                        dataType = DataType.Level | DataType.Spectrum | DataType.Audio;
                    SendCommand(
                        $"DDC:CONT {index},{frequency} MHz,{ifBandwidth} kHz,{demMode},{dataType.ToString().Replace(", ", "|")},{beta / 100.0f},{xdB}");
                    // 更新缓存的DDC通道参数
                    _prevDdcSettings[index].Frequency = frequency;
                    _prevDdcSettings[index].FilterBandwidth = ifBandwidth;
                    _prevDdcSettings[index].DemMode = demMode == Modulation.Pdt ? Modulation.Dmr : demMode;
                    _prevDdcSettings[index].IfSwitch = ifSwitch;
                    _prevDdcSettings[index].LevelSwitch = levelSwitch;
                    _prevDdcSettings[index].SpectrumSwitch = spectrumSwitch;
                    _prevDdcSettings[index].AudioSwitch = audioSwitch;
                    _prevDdcSettings[index].XdB = xdB;
                    _prevDdcSettings[index].Beta = beta;
                }
            }
        }
    }

    private void ResetDdc()
    {
        lock (_ddcLock)
        {
            foreach (var t in _prevDdcSettings)
                t.IfSwitch = false;

            _ddcChannels = null;
        }

        SendCommand("DDC:CLOS:ALL");
    }

    private void SetAmpdf()
    {
        _subscribedData |= DataType.Iq | DataType.Level;
    }

    private void ResetAmpdf()
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
        lock (InvalidCountLock)
        {
            if ((dataType & DataType.Scan) > 0) _invalidScanCount = InvalidScanAndSpectrumCount;
        }

        // 切换单频与信号识别
        if ((CurFeature & FeatureType.FFM) > 0)
        {
            SendCommand("FREQ:MODE FIX");
            WaitForModeSwitchFeedback();
        }
        else if ((CurFeature & FeatureType.TDOA) > 0)
        {
            SendCommand("TRIG:TDO GPS");
        }

        if (dataType != DataType.None)
        {
            SendCommand($"TRAC:MED {dataType.ToString().Replace(", ", ",")}");
            SendCommand("TRAC ON");
            OperateDdr();
        }
        else
        {
            _ddrCts?.Cancel();
            SendCommand("TRAC OFF");
        }
    }

    private void CancelTask()
    {
        _ddrCts?.Cancel();
        SendCommand("TRAC OFF");
    }

    private void ClearAll()
    {
        // 清空缓存队列
        foreach (var identifier in _identifiers)
            if (_queues.ContainsKey(identifier) && _queues[identifier] != null)
                _queues[identifier].Clear();
        // 清空DDC音频缓存
        foreach (var keyvaluePair in _ddcAudioBuffer) _ddcAudioBuffer[keyvaluePair.Key]?.Clear();
        _ddcAudioBuffer.Clear();
        // 清空数据源数据
        _dfDataSource.ClearData();
    }

    #endregion

    #region 资源释放

    private void ReleaseResource()
    {
        ReleaseNetworks();
        ReleaseQueues();
        ReleaseDdcAudioBuffer();
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

            if (_dispatches.ContainsKey(identifier) && _dispatches[identifier]?.IsAlive == true)
            {
                try
                {
                    _dispatches[identifier].Join();
                }
                catch
                {
                    // ignored
                }

                _dispatches[identifier] = null;
            }
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

    private void ReleaseDdcAudioBuffer()
    {
        foreach (var keyvaluePair in _ddcAudioBuffer) _ddcAudioBuffer[keyvaluePair.Key]?.Clear();
        _ddcAudioBuffer.Clear();
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
        if (socket != null)
        {
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
                        || identifier.Equals("gps", StringComparison.OrdinalIgnoreCase)
                        || identifier.Equals("compass", StringComparison.OrdinalIgnoreCase))
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
                    // TODO
                    // var item = new LogItem(ex);
                    // LogManager.Add(item);
                }
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
                // TODO
                // var item = new LogItem(ex);
                // LogManager.Add(item);
                // SendMessage(MessageDomain.Task, MessageType.Warning, "推送数据时发生异常，正尝试恢复...");
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
                    if (data.ReservedTag == 0)
                        lock (_parameterLock)
                        {
                            if ((CurFeature & FeatureType.AmpDF) > 0)
                            {
                                ProcessAmpdf(data, _subscribedData, _iqSamplingCount);
                                continue;
                            }

                            result.Add((CurFeature & FeatureType.TDOA) > 0 ? ToTdoa(data) : ToIq(data));
                        }
                    else
                        result.AddRange(ToDdciq(data));

                    break;
                case DataType.Level:
                    result.Add(ToLevel(data));
                    break;
                case DataType.Spectrum:
                    result.Add(ToSpectrum(data));
                    break;
                case DataType.Dpx:
                    break;
                case DataType.Audio:
                    if (data.ReservedTag == 0)
                        result.Add(ToAudio(data));
                    else
                        result.AddRange(ToDdcAudio(data));
                    break;
                case DataType.Itu:
                    result.Add(ToItu(data));
                    break;
                case DataType.Scan:
                    result.Add(ToScan(data));
                    break;
                case DataType.Dfiq:
                    ProcessDfiq(data);
                    break;
                case DataType.Dfc:
                    ProcessDfc(data);
                    break;
                case DataType.Ddc:
                    result.Add(ToDdc(data));
                    break;
                case DataType.Sms:
                    result.Add(ToSms(data));
                    break;
                case DataType.Gps:
                    ProcessGps(data);
                    continue;
                case DataType.Compass:
                    ProcessCompass(data);
                    continue;
                case DataType.Dfind:
                    break;
            }

        result = result.Where(item => item != null).ToList();
        if (result.Count > 0 && TaskState == TaskState.Start) SendData(result);
    }

    private void ProcessAmpdf(object data, DataType dataType = DataType.Level, int segmentIqCount = 128)
    {
        if (data is RawIq iq)
        {
            var action = new Action<object, DataType>((item, convertedType) =>
            {
                var result = new List<object>();
                if ((convertedType | DataType.Level) > 0) result.Add(ToLevelByIq(item));
                if ((convertedType | DataType.Spectrum) > 0) result.Add(ToSpectrumByIq(item));
                result = result.Where(value => value != null).ToList();
                if (result.Count > 0) SendData(result);
            });
            var validCount = iq.DataCollection.Length % (segmentIqCount * 2) / 2;
            var index = 0;
            for (;
                 iq.DataCollection.Length >= segmentIqCount * 2 && index < iq.DataCollection.Length;
                 index += segmentIqCount * 2)
            {
                var temp = new RawIq
                {
                    Frequency = iq.Frequency,
                    Bandwidth = iq.Bandwidth,
                    SampleRate = iq.SampleRate,
                    SynCode = iq.SynCode,
                    Offset = iq.Offset + index / 2,
                    Attenuation = iq.Attenuation,
                    Count = segmentIqCount,
                    Total = iq.Total,
                    DataCollection = new int[segmentIqCount * 2]
                };
                Buffer.BlockCopy(iq.DataCollection, sizeof(int) * index, temp.DataCollection, 0,
                    sizeof(int) * temp.DataCollection.Length);
                action.Invoke(temp, dataType);
            }

            if (validCount > 0)
            {
                if (index > 0)
                {
                    index -= segmentIqCount * 2;
                    var temp = new RawIq
                    {
                        Frequency = iq.Frequency,
                        Bandwidth = iq.Bandwidth,
                        SampleRate = iq.SampleRate,
                        SynCode = iq.SynCode,
                        Offset = iq.Offset + index / 2,
                        Attenuation = iq.Attenuation,
                        Count = segmentIqCount,
                        Total = iq.Total,
                        DataCollection = new int[validCount * 2]
                    };
                    Buffer.BlockCopy(iq.DataCollection, sizeof(int) * index, temp.DataCollection, 0,
                        sizeof(int) * temp.DataCollection.Length);
                    action.Invoke(temp, dataType);
                }
                else
                {
                    action.Invoke(iq, dataType);
                }
            }
        }
    }

    private void ProcessGps(object data)
    {
        if (data is not RawGps gps) return;
        var dataCollection = gps.Text.Split('\r', '\n').ToList().Where(item => item.StartsWith("$"));
        new Task(() =>
        {
            foreach (var item in dataCollection) ParseGps(item.Trim());
        }).RunSynchronously();
    }

    private void ProcessCompass(object data)
    {
        if (data is not RawCompass raw || raw.Heading < 0 || raw.Heading > 3600) return;
        var compass = new SDataCompass
        {
            Heading = ((raw.Heading / 10.0f + CompassInstallingAngle) % 360 + 360) % 360
        };
        SendMessageData(new List<object> { compass });
    }

    private object ToDdc(object data)
    {
        if (data is not RawDdc raw) return null;
        var channelNo = raw.DdcIndex;
        var ddc = new SDataDdc
        {
            ChannelNumber = channelNo,
            Data = new List<object>()
        };
        lock (_ddcLock)
        {
            if (!_prevDdcSettings[channelNo].IfSwitch) return null;
            if (_prevDdcSettings[channelNo].LevelSwitch)
            {
                var level = new SDataLevel
                {
                    Frequency = _prevDdcSettings[channelNo].Frequency,
                    Bandwidth = _prevDdcSettings[channelNo].FilterBandwidth,
                    Data = raw.Level / 10.0f
                };
                ddc.Data.Add(level);
            }

            if (_prevDdcSettings[channelNo].SpectrumSwitch)
            {
                var spectrum = new SDataSpectrum
                {
                    Frequency = _prevDdcSettings[channelNo].Frequency,
                    Span = _prevDdcSettings[channelNo].FilterBandwidth,
                    Data = raw.Spectrum
                };
                ddc.Data.Add(spectrum);
            }
        }

        if (ddc.Data.Count > 0) return ddc;
        return null;
    }

    private List<object> ToDdciq(object data)
    {
        if (data is not RawIQforDdc raw) return null;
        var validChannels = raw.Total / raw.CountPerChannel;
        var ddcChannels = new int[validChannels];
        for (int ddcIndex = 0, index = 0; index < 128 && ddcIndex < validChannels; ++index)
        {
            if (index < 64 && ((raw.EnabledChannels1 >> index) & 0x1) == 0x1) ddcChannels[ddcIndex++] = index;
            if (index >= 64 && ((raw.EnabledChannels2 >> index) & 0x1) == 0x1) ddcChannels[ddcIndex++] = 64 + index;
        }

        var result = new List<object>();
        for (var index = 0; index < validChannels; ++index)
        {
            var channelNo = ddcChannels[index];
            var ddc = new SDataDdc
            {
                ChannelNumber = channelNo,
                Data = new List<object>()
            };
            lock (_ddcLock)
            {
                if (_prevDdcSettings[channelNo].IfSwitch && _prevDdcSettings[channelNo].IqSwitch)
                {
                    var iqArray = new int[raw.CountPerChannel * 2];
                    Buffer.BlockCopy(raw.DataCollection, index * iqArray.Length * sizeof(int), iqArray, 0,
                        iqArray.Length * sizeof(int));
                    var iq = new SDataIq
                    {
                        Frequency = _prevDdcSettings[channelNo].Frequency,
                        SamplingRate = raw.SampleRate / 1000.0d,
                        Attenuation = raw.Attenuation,
                        Bandwidth = _prevDdcSettings[channelNo].FilterBandwidth,
                        Data32 = iqArray
                    };
                    ddc.Data.Add(iq);
                }

                if (ddc.Data.Count > 0) result.Add(ddc);
            }
        }

        return result;
    }

    private List<object> ToDdcAudio(object data)
    {
        if (data is not RawAudioforDdc raw) return null;
        var validChannels = raw.Total / 2 / raw.CountPerChannel;
        var ddcChannels = new int[validChannels];
        for (int ddcIndex = 0, index = 0; index < 128 && ddcIndex < validChannels; ++index)
        {
            if (index < 64 && ((raw.EnabledChannels1 >> index) & 0x1) == 0x1) ddcChannels[ddcIndex++] = index;
            if (index >= 64 && ((raw.EnabledChannels2 >> index) & 0x1) == 0x1) ddcChannels[ddcIndex++] = 64 + index;
        }

        var result = new List<object>();
        for (var index = 0; index < validChannels; ++index)
        {
            var channelNo = ddcChannels[index];
            var ddc = new SDataDdc
            {
                ChannelNumber = channelNo,
                Data = new List<object>()
            };
            lock (_ddcLock)
            {
                if (_prevDdcSettings[channelNo].IfSwitch && _prevDdcSettings[channelNo].AudioSwitch)
                {
                    var buffer = new byte[raw.CountPerChannel * 2];
                    Buffer.BlockCopy(raw.DataCollection, index * buffer.Length, buffer, 0, buffer.Length);
                    if (!_ddcAudioBuffer.ContainsKey(channelNo)) _ddcAudioBuffer[channelNo] = new List<byte>();
                    _ddcAudioBuffer[channelNo].AddRange(buffer);
                    if (_ddcAudioBuffer[channelNo].Count < 512 * 100) continue;
                    var audio = new SDataAudio
                    {
                        Format = AudioFormat.Pcm,
                        SamplingRate = (int)raw.SampleRate,
                        Data = _ddcAudioBuffer[channelNo].ToArray()
                    };
                    _ddcAudioBuffer[channelNo].Clear();
                    ddc.Data.Add(audio);
                }
            }

            if (ddc.Data.Count > 0) result.Add(ddc);
        }

        return result;
    }

    private void ProcessDfiq(object data)
    {
        if (data is RawDfiQforDuplexChannel)
            ProcessDfiQforDual(data);
        else if (data is RawDfiQforNineChannel) ProcessDfiQforNine(data);
    }

    private void ProcessDfiQforDual(object data)
    {
        var raw = data as RawDfiQforDuplexChannel;
        lock (_parameterLock)
        {
            // 有效的测向数据应该同时满足测量频率一致，测量带宽一致，打通天线一致，采样点数一致，否则为无效的数据（可能来自于之前无效的数据）
            if (raw != null && (Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon
                                || Math.Abs(raw.Bandwidth / 1000.0d - _ifBandwidth) > _epsilon
                                || raw.AntennaIndex != _dfAntennaIndex))
            {
#if WRITE_DEBUG_INFO
					Console.WriteLine(string.Format("Invalid parameters: Frequency: {0}Hz, {1}MHz; Bandwidth: {2}Hz, {3}kHz; AntennaIndex: {4}, {5}", raw.Frequency, _frequency, raw.Bandwidth, _spectrumSpan, raw.AntennaIndex, _dfAntennaIndex));
#endif
                return;
            }
        }

        _dfDataSource.Receive(data);
    }

    private void ProcessDfiQforNine(object data)
    {
        var raw = data as RawDfiQforNineChannel;
        lock (_parameterLock)
        {
            // 有效的测向数据应该同时满足测量频率一致，测量带宽一致，打通天线一致，采样点数一致，否则为无效的数据（可能来自于之前无效的数据）
            if (raw != null && (Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon
                                || Math.Abs(raw.Bandwidth / 1000.0d - _ifBandwidth) > _epsilon
                                || raw.AntennaIndex != _dfAntennaIndex))
            {
#if WRITE_DEBUG_INFO
					Console.WriteLine(string.Format("Invalid parameters: Frequency: {0}Hz, {1}MHz; Bandwidth: {2}Hz, {3}kHz; AntennaIndex: {4}, {5}", raw.Frequency, _frequency, raw.Bandwidth, _spectrumSpan, raw.AntennaIndex, _dfAntennaIndex));
#endif
                return;
            }
        }

        _dfDataSource.Receive(data);
    }

    private void ProcessDfc(object data)
    {
        if (data is RawDfCforDuplexChannel)
            ProcessDfCforDual(data);
        else if (data is RawDfCforNineChannel) ProcessDfCforNine(data);
    }

    private void ProcessDfCforDual(object data)
    {
        var raw = data as RawDfCforDuplexChannel;
        lock (_parameterLock)
        {
            // 有效的测向数据应该同时满足测量频率一致，测量带宽一致，打通天线一致，采样点数一致，否则为无效的数据（可能来自于之前无效的数据）
            if (raw != null && (((CurFeature & FeatureType.ScanDF) == 0 &&
                                 (Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon ||
                                  Math.Abs(raw.Bandwidth / 1000.0d - _ifBandwidth) > _epsilon))
                                || raw.AntennaIndex != _dfAntennaIndex))
            {
#if WRITE_DEBUG_INFO
					Console.WriteLine(string.Format("Invalid parameters: Frequency: {0}Hz, {1}MHz; Bandwidth: {2}Hz, {3}kHz; AntennaIndex: {4}, {5}", raw.Frequency, _frequency, raw.Bandwidth, _spectrumSpan, raw.AntennaIndex, _dfAntennaIndex));
#endif
                return;
            }
        }

        _dfDataSource.Receive(data);
    }

    private void ProcessDfCforNine(object data)
    {
        var raw = data as RawDfCforNineChannel;
        lock (_parameterLock)
        {
            // 有效的测向数据应该同时满足测量频率一致，测量带宽一致，打通天线一致，采样点数一致，否则为无效的数据（可能来自于之前无效的数据）
            if (raw != null && (((CurFeature & FeatureType.ScanDF) == 0 &&
                                 (Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon ||
                                  Math.Abs(raw.Bandwidth / 1000.0d - _ifBandwidth) > _epsilon))
                                || raw.AntennaIndex != _dfAntennaIndex))
            {
#if WRITE_DEBUG_INFO
					Console.WriteLine(string.Format("Invalid parameters: Frequency: {0}Hz, {1}MHz; Bandwidth: {2}Hz, {3}kHz; AntennaIndex: {4}, {5}", raw.Frequency, _frequency, raw.Bandwidth, _spectrumSpan, raw.AntennaIndex, _dfAntennaIndex));
#endif
                return;
            }
        }

        _dfDataSource.Receive(data);
    }

    private void OnDFRelatedDataArrived(object sender, DfRelatedDataArrivedEventArgs e)
    {
        if (e.Data is not List<object> data) return;
        var level = data.Find(item => item is SDataLevel) as SDataLevel;
        if (level != null)
        {
            var temp = (long)(level.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) level.Frequency = value / 1000000.0d;
            level.Data += LevelCalibrationFromDf;
        }

        var spectrum = data.Find(item => item is SDataSpectrum) as SDataSpectrum;
        if (spectrum != null)
        {
            var temp = (long)(spectrum.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) spectrum.Frequency = value / 1000000.0d;
            for (var index = 0; index < spectrum.Data.Length; ++index)
                spectrum.Data[index] += (short)(LevelCalibrationFromDf * 10);
        }

        if (data.Find(item => item is SDataDfind) is SDataDfind dfind)
        {
            var temp = (long)(dfind.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) dfind.Frequency = value / 1000000.0d;
        }

        var wbdf = data.Find(item => item is SDataDfpan) as SDataDfpan;
        if (wbdf != null)
        {
            var temp = (long)(wbdf.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) wbdf.Frequency = value / 1000000.0d;
        }

        if (data.Find(item => item is SDataSse) is SDataSse sse)
        {
            var temp = (long)(sse.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) sse.Frequency = value / 1000000.0d;
        }

        var scan = data.Find(item => item is SDataScan) as SDataScan;
        if (scan != null)
        {
            for (var index = 0; index < scan.Data.Length; ++index)
                scan.Data[index] += (short)(LevelCalibrationFromDf * 10);
            var delta = scan.Offset + scan.Data.Length - scan.Total;
            if (Math.Abs(delta) == 1) // 扫描最后一包数据长度与理论值相比可能相差一个点
            {
                Array.Resize(ref scan.Data, scan.Data.Length - delta); // 将数组调整和理论值一样
                if (delta == -1)
                    // 如果比理论值少一个点，最补齐点的值设置和倒数第二点一致
                    scan.Data[^1] = scan.Data[^2];
            }
        }

        var scanDf = data.Find(item => item is SDataDfScan);
        if ((CurFeature & FeatureType.ScanDF) > 0)
        {
            if (scanDf != null && scan != null) SendData(new List<object> { DeviceId, scan, scanDf });
        }
        else
        {
            // 无效频谱次数不为零，且有频谱数据，并且当前功能不为单频测向或者测向模式不是突发模式
            if (_invalidSpectrumCount > 0 && spectrum != null &&
                ((CurFeature & FeatureType.FFDF) == 0 || _dfindMode != DFindMode.Gate))
            {
                --_invalidSpectrumCount;
                data.Remove(spectrum);
                if (wbdf != null) // 由于宽带测向是同时要求输出对应的频谱，因此，在频谱被移除的情况下，宽带测向数据也要移除
                    data.Remove(wbdf);
            }

            if (_invalidLevelCount > 0 && level != null &&
                ((CurFeature & FeatureType.FFDF) == 0 || _dfindMode != DFindMode.Gate))
            {
                --_invalidLevelCount;
                data.Remove(level);
            }

            if (data.Count > 0) SendData(data);
        }
    }

    #endregion

    #region 业务数据转换

    private object ToIq(object data)
    {
        if (data is not RawIq raw || ((CurFeature & (FeatureType.MScan | FeatureType.FScne)) == 0 &&
                                      Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid IQ data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        raw.Bandwidth = (long)(_ifBandwidth * 1000d);
#if !IQ_ALL_IN_ONE
        var iq = new SDataIq
        {
            Timestamp = raw.TimestampSecond * 1000000000L + raw.TimestampNanoSecond,
            Frequency = raw.Frequency / 1000000.0d,
            Bandwidth = raw.Bandwidth / 1000.0d,
            Attenuation = raw.Attenuation,
            SamplingRate = raw.SampleRate / 1000.0d
        };
        if (raw.Width == 16)
            iq.Data16 = Array.ConvertAll(raw.DataCollection, item => (short)item);
        else
            iq.Data32 = raw.DataCollection;
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) iq.Frequency = value / 1000000.0d;
        return iq;
#else
			if (raw.Count == raw.Total)
			{
				var iq = new SDataIQ
				{
					Timestamp = raw.TimestampSecond * 1000000000L + raw.TimestampNanoSecond,
					Frequency = raw.Frequency / 1000000.0d,
					Bandwidth = raw.Bandwidth / 1000.0d,
					Attenuation = raw.Attenuation,
					SamplingRate = raw.SampleRate / 1000.0d
				};
				if (raw.Width == 16)
				{
					iq.Data16 = Array.ConvertAll(raw.DataCollection, item => (short)item);
				}
				else
				{
					iq.Data32 = raw.DataCollection;
				}
				if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
				{
					iq.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
				}
				return iq;
			}
			else
			{
				if (_bufferedIQ.SynCode != raw.SynCode && raw.Offset != 0)
				{
					return null;
				}
				else if (_bufferedIQ.SynCode != raw.SynCode || _bufferedIQ.Data == null)
				{
					_bufferedIQ.Data = new int[raw.Total * 2];
				}
				_bufferedIQ.SynCode = raw.SynCode;
				Array.Copy(raw.DataCollection, 0, _bufferedIQ.Data, raw.Offset * 2, raw.Count * 2);
				if (raw.Offset + raw.Count == raw.Total)
				{
					var iq = new SDataIQ
					{
						Timestamp = raw.TimestampSecond * 1000000000L + raw.TimestampNanoSecond,
						Frequency = raw.Frequency / 1000000.0d,
						Bandwidth = raw.Bandwidth / 1000.0d,
						Attenuation = raw.Attenuation,
						SamplingRate = raw.SampleRate / 1000.0d
					};
					if (raw.Width == 16)
					{
						int[] temp = _bufferedIQ.Data;
						iq.Data16 = Array.ConvertAll(temp, item => (short)item);
					}
					else
					{
						iq.Data32 = _bufferedIQ.Data;
					}
					if (_reverseFrequencyOffsetDic.ContainsKey(raw.Frequency))
					{
						iq.Frequency = _reverseFrequencyOffsetDic[raw.Frequency] / 1000000.0d;
					}
					return iq;
				}
				else
				{
					return null;
				}
			}
#endif
    }

    private object ToLevelByIq(object data)
    {
        if (data is not RawIq raw || ((CurFeature & (FeatureType.MScan | FeatureType.FScne)) == 0 &&
                                      Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid IQ data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        raw.Bandwidth = (long)(_ifBandwidth * 1000d);
        if (--_invalidLevelCount > 0)
        {
            --_invalidLevelCount;
            return null;
        }

        var iq = Array.ConvertAll(raw.DataCollection, item => (float)item);
        var level = Utilities.GetLevel(iq);
        level += raw.Attenuation / 10.0f + LevelCalibrationFromIq;
        var result = new SDataLevel
        {
            Frequency = raw.Frequency / 1000000.0d,
            Bandwidth = raw.Bandwidth / 1000.0d,
            Data = level
        };
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) result.Frequency = value / 1000000.0d;
        return result;
    }

    private object ToSpectrumByIq(object data)
    {
        if (data is not RawIq raw || ((CurFeature & (FeatureType.MScan | FeatureType.FScne)) == 0 &&
                                      Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid IQ data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        raw.Bandwidth = (long)(_ifBandwidth * 1000d); // 临时加的，没有任何意义，LZ已经生无可恋了
        if (_invalidSpectrumCount > 0)
        {
            --_invalidSpectrumCount;
            return null;
        }

        var iq = Array.ConvertAll(raw.DataCollection, item => (float)item);
        var exp = Utilities.Log2N(iq.Length / 2);
        var length = 1 << exp;
        var windowValue = new float[length];
        var coe = Utilities.Window(ref windowValue, WindowType.Hanning);
        var spectrum = Utilities.GetWindowData(iq, windowValue, length);
        Utilities.Fft(ref spectrum);
        var efficientLength = (int)(length * 1.0 * raw.Bandwidth / raw.SampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        coe += (float)(-20 * Math.Log10(length) + raw.Attenuation / 10.0f + LevelCalibrationFromIq);
        var spectrumEx = new float[length];
        for (var index = 0; index < length; ++index)
            spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
        var validSpectrum = new short[efficientLength];
        for (var index = 0; index < validSpectrum.Length; ++index)
            validSpectrum[index] = (short)((spectrumEx[(efficientIndex + index) % length] + coe) * 10);
        var result = new SDataSpectrum
        {
            Frequency = raw.Frequency / 1000000.0d,
            Span = raw.Bandwidth / 1000.0d,
            Data = validSpectrum
        };
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) result.Frequency = value / 1000000.0d;
        return result;
    }

    private object ToAudio(object data)
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

    private object ToLevel(object data)
    {
        if (data is not RawLevel raw ||
            ((CurFeature & (FeatureType.MScne | FeatureType.FScne | FeatureType.MScan)) == 0 &&
             Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid Level data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        if (--_invalidLevelCount > 0)
        {
            --_invalidLevelCount;
            return null;
        }

        var level = new SDataLevel
        {
            Frequency = raw.Frequency / 1000000.0d,
            Bandwidth = raw.Bandwidth / 1000.0d,
            Data = raw.Level + _levelCalibrationForFixFq
        };
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) level.Frequency = value / 1000000.0d;
        return level;
    }

    private object ToSpectrum(object data)
    {
        if (data is not RawSpectrum raw ||
            ((CurFeature & (FeatureType.FScne | FeatureType.MScne | FeatureType.MScan)) == 0 &&
             Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid spectrum data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        if (_invalidSpectrumCount > 0)
        {
            --_invalidSpectrumCount;
            return null;
        }

        var spectrum = new SDataSpectrum
        {
            Frequency = raw.Frequency / 1000000d,
            Span = raw.Span / 1000.0d,
            Data = new short[raw.DataCollection.Length]
        };
        // 电平修正，中心频率 -> 偏移量
        for (var index = 0; index < spectrum.Data.Length; ++index)
            // 得到频谱数据
            spectrum.Data[index] = (short)(raw.DataCollection[index] + _levelCalibrationForFixFq * 10);
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value))
            spectrum.Frequency = value / 1000000.0d;
        return spectrum;
    }

    private object ToItu(object data)
    {
        if (data is not RawItu raw || raw.Modulation == -1 ||
            ((CurFeature & (FeatureType.MScne | FeatureType.FScne)) == 0 &&
             Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid ITU data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        var itu = new SDataItu
        {
            Frequency = raw.Frequency / 1000000.0d,
            Misc = new Dictionary<string, object>
            {
                [ParameterNames.ItuAmDepth] = raw.Am is < 0 or > 100 ? double.MinValue : raw.Am,
                [ParameterNames.ItuAmDepthPos] = raw.AmPos is < 0 or > 100 ? double.MinValue : raw.AmPos,
                [ParameterNames.ItuAmDepthNeg] = raw.AmNeg is < 0 or > 100 ? double.MinValue : raw.AmNeg,
                [ParameterNames.ItuFmDev] = raw.Fm > _filterBandwidth * 1000 * 2 ? double.MinValue : raw.Fm / 1000.0d,
                [ParameterNames.ItuFmDevPos] =
                    raw.FmPos > _filterBandwidth * 1000 * 2 ? double.MinValue : raw.FmPos / 1000.0d,
                [ParameterNames.ItuFmDevNeg] =
                    raw.FmNeg > _filterBandwidth * 1000 * 2 ? double.MinValue : raw.FmNeg / 1000.0d,
                [ParameterNames.ItuPmDepth] = raw.Pm <= -2 * Math.PI || raw.Pm >= 2 * Math.PI ? double.MinValue : raw.Pm
            }
        };
        if (raw.Beta < _filterBandwidth * 1000 * 2) itu.Misc[ParameterNames.ItuBeta] = raw.Beta / 1000.0d;
        if (raw.XdB < _filterBandwidth * 1000 * 2) itu.Misc[ParameterNames.ItuBeta] = raw.XdB / 1000.0d;
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) itu.Frequency = value / 1000000.0d;
        return itu;
    }

    private object ToScan(object data)
    {
        if (data is RawScan) return ToConventionalScan(data);

        if (data is RawFastScan)
        {
            // return ToFastScan(data);
        }

        return null;
    }

    private object ToConventionalScan(object data)
    {
        var raw = data as RawScan;
        if (raw != null)
        {
            var delta = _scanDataLength - raw.Total;
            if (delta != 0)
            {
                if (raw.Offset + raw.DataCollection.Length == raw.Total)
                    Array.Resize(ref raw.DataCollection, raw.DataCollection.Length + delta);
                raw.Total += delta;
            }
        }

        var scan = new SDataScan
        {
            SegmentOffset = raw!.SegmentIndex,
            StartFrequency = raw.StartFrequency / 1000000.0d,
            StopFrequency = raw.StopFrequency / 1000000.0d,
            StepFrequency = raw.StepFrequency / 1000.0d,
            Total = raw.Total,
            Offset = raw.Offset,
            Data = new short[raw.DataCollection.Length]
        };
        for (var i = 0; i < raw.DataCollection.Length; ++i)
            scan.Data[i] = (short)(raw.DataCollection[i] + _levelCalibrationForScan * 10);
        lock (InvalidCountLock)
        {
            if (CurFeature == FeatureType.SCAN && _scanMode == ScanMode.Pscan && _invalidScanCount > 0)
            {
                if (scan.Offset + scan.Data.Length == scan.Total) --_invalidScanCount;
                return null;
            }
        }

        return scan;
    }

    //      private object ToFastScan(object data)
    // 		{
    // 			var raw = data as RawFastScan;
    // 			if (raw.StartFrequency != (long)(_startFrequency * 1000000.0d)
    // 				|| raw.StopFrequency != (long)(_stopFrequency * 1000000.0d)
    // 				|| raw.StepFrequency != (long)(_stepFrequency * 1000.0d))
    // 			{
    // #if WRITE_DEBUG_INFO && DATA_INFO
    // 				Console.WriteLine("Invalid scan data: {0}", raw == null ? "null info" : string.Format("start={1}, stop={2}, step={3}", raw.StartFrequency, raw.StopFrequency, raw.StepFrequency));
    // #endif
    // 				return null;
    // 			}
    // 
    // 			var scan = new SDataFastScan
    // 			{
    // 				StartFrequency = raw.StartFrequency / 1000000.0d,
    // 				StopFrequency = raw.StopFrequency / 1000000.0d,
    // 				StepFrequency = raw.StepFrequency / 1000.0d,
    // 				Signals = new float[raw.Count],
    // 				Noises = new float[raw.Count],
    // 				Indices = new int[raw.Count]
    // 			};
    // 			for (var index = 0; index < raw.Count; ++index)
    // 			{
    // 				scan.Signals[index] = raw.SignalCollection[index] / 10.0f;
    // 				scan.Noises[index] = raw.NoiseCollection[index] / 10.0f;
    // 				scan.Indices[index] = raw.SignalIndexCollection[index];
    // 			}
    // 
    // 			return scan;
    // 		}
    private object ToTdoa(object data)
    {
        if (data is not RawIq raw || ((CurFeature & (FeatureType.MScan | FeatureType.FScne)) == 0 &&
                                      Math.Abs(raw.Frequency / 1000000.0d - _frequency) > _epsilon))
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid IQ data: {0}", raw == null ? "null info" : string.Format("freq={0}", raw.Frequency));
#endif
            return null;
        }

        raw.Bandwidth = (long)(_ifBandwidth * 1000d);
        if (raw.Count == raw.Total)
        {
            var tdoa = new SDataIq
            {
                Timestamp = raw.TimestampSecond * 1000000000L + raw.TimestampNanoSecond,
                Frequency = raw.Frequency / 1000000.0d,
                Bandwidth = raw.Bandwidth / 1000.0d,
                Attenuation = raw.Attenuation,
                SamplingRate = raw.SampleRate / 1000.0d
            };
            if (raw.Width == 16)
                tdoa.Data16 = Array.ConvertAll(raw.DataCollection, item => (short)item);
            else
                tdoa.Data32 = raw.DataCollection;
            if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value))
                tdoa.Frequency = value / 1000000.0d;
            return tdoa;
        }

        if (_bufferedIq.SynCode != raw.SynCode && raw.Offset != 0) return null;

        if (_bufferedIq.SynCode != raw.SynCode || _bufferedIq.Data == null)
        {
            _bufferedIq.TimestampSecond = raw.TimestampSecond;
            _bufferedIq.TimestampNanoSecond = raw.TimestampNanoSecond;
            _bufferedIq.Data = new int[raw.Total * 2];
        }

        _bufferedIq.SynCode = raw.SynCode;
        Array.Copy(raw.DataCollection, 0, _bufferedIq.Data, raw.Offset * 2, raw.Count * 2);
        if (raw.Offset + raw.Count == raw.Total)
        {
            var tdoa = new SDataIq
            {
                Timestamp = raw.TimestampSecond * 1000000000L + raw.TimestampNanoSecond,
                Frequency = raw.Frequency / 1000000.0d,
                Bandwidth = raw.Bandwidth / 1000.0d,
                Attenuation = raw.Attenuation,
                SamplingRate = raw.SampleRate / 1000.0d
            };
            if (raw.Width == 16)
            {
                int[] temp = _bufferedIq.Data;
                tdoa.Data16 = Array.ConvertAll(temp, item => (short)item);
            }
            else
            {
                tdoa.Data32 = _bufferedIq.Data;
            }

            if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value))
                tdoa.Frequency = value / 1000000.0d;
            return tdoa;
        }

        return null;
    }

    private object ToSms(object data)
    {
        if (data is not RawSms raw) return null;
        var sms = new SDataSms
        {
            Frequency = raw.Frequency / 1000000.0d,
            Dialer = raw.CallingNumber.ToString(),
            Dialee = raw.CalledNumber.ToString(),
            Text = raw.Text
        };
        if (_reverseFrequencyOffsetDic.TryGetValue(raw.Frequency, out var value)) sms.Frequency = value / 1000000.0d;
        return null;
        // var message = new SDataDMRMessage
        // {
        // 	Message = string.Format("频率：{0} | 色码：{1} | 主叫号码：{2} | 被叫号码：{3} | 短信内容：{4}",
        // 	sms.Frequency, sms.ColourCode, sms.CallingNumber, sms.CalledNumber, sms.Text.Trim())
        // };
        // // return sms;
        // return message;
    }

    #endregion

    #region GPS Parsing Helper

    private void ParseGps(string data)
    {
        var previousTimeStamp = _preGpsTimeStamp;
        var bufferedGps = new SDataGps
        {
            Latitude = _bufferedGps.Latitude,
            Longitude = _bufferedGps.Longitude
        };
        var rmc = ParseRmc(data) as SDataGps; // RMC
        var gga = ParseGga(data) as SDataGps; // GGA
        var gll = ParseGll(data) as SDataGps; // GLL
        var dataCollection = from x in new object[] { rmc, gga, gll } where x is SDataGps select x;
        var collection = dataCollection as object[] ?? dataCollection.ToArray();
        if (collection.Length > 0)
        {
            if (collection.ElementAt(Index.Start) is SDataGps gps)
            {
                var distance =
                    GetDistanceByPosition(bufferedGps.Latitude, bufferedGps.Longitude, gps.Latitude, gps.Longitude);
                var currentTimeStamp = DateTime.Now;
                var timespan = currentTimeStamp - previousTimeStamp;
                if (distance > 2 || timespan.TotalMilliseconds > 10000)
                {
                    // SendMessage(MessageDomain.Network, MessageType.MonNodeGPSChange, gps[0]);
                    // SendData(new List<object>() { gps[0] });
                    SendMessageData(new List<object> { gps });
                    previousTimeStamp = currentTimeStamp;
                    bufferedGps = gps;
                }
            }

            lock (_gpsLock)
            {
                _preGpsTimeStamp = previousTimeStamp;
                _bufferedGps.Latitude = bufferedGps.Latitude;
                _bufferedGps.Longitude = bufferedGps.Longitude;
            }
        }
    }

    private static object ParseGga(string data)
    {
        // 筛选GPGGA/BDGGA/GNGGA，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}GGA)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!reg.IsMatch(data)) return null;
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 8) return null;
        // “1”为单点定位，“2”为伪距差分定位，其它为无效定位或未定位
        if (!dataArray[6].Equals("1") && !dataArray[6].Equals("2")) return null;
        var lat = ToDegree(dataArray[2]);
        var lon = ToDegree(dataArray[4]);
        if (lat == null || lon == null) return null;
        if (dataArray[3].Equals("s", StringComparison.OrdinalIgnoreCase)) lat *= -1;
        if (dataArray[5].Equals("w", StringComparison.OrdinalIgnoreCase)) lon *= -1;
        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        return gps;
    }

    private static object ParseGll(string data)
    {
        // 筛选GPGLL/BDGLL/GNGLL，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}GLL)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!reg.IsMatch(data)) return null;
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 7) return null;
        // 未定位
        if (!dataArray[6].Equals("a", StringComparison.OrdinalIgnoreCase)) return null;
        var lat = ToDegree(dataArray[1]);
        var lon = ToDegree(dataArray[3]);
        if (lat == null || lon == null) return null;
        if (dataArray[2].Equals("s", StringComparison.OrdinalIgnoreCase)) lat *= -1;
        if (dataArray[4].Equals("w", StringComparison.OrdinalIgnoreCase)) lon *= -1;
        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        return gps;
    }

    // private object ParseGSA(string data)
    // {
    // 	//TODO: 按实际需求编写
    // 	return null;
    // }
    // private object ParseGSV(string data)
    // {
    // 	//TODO: 按实际需求编写
    // 	return null;
    // }
    private static object ParseRmc(string data)
    {
        // 筛选GPRMC/BDRMC/GNRMC，分别表示GPS/北斗/GPS+北斗
        var reg = new Regex(@"^(\$\w{2}RMC)", RegexOptions.IgnoreCase);
        if (!reg.IsMatch(data)) return null;
        var dataArray = data.Split(new[] { ',' });
        if (dataArray.Length < 12) return null;
        // 未定位
        if (!dataArray[2].Equals("a", StringComparison.OrdinalIgnoreCase)) return null;
        var lat = ToDegree(dataArray[3]);
        var lon = ToDegree(dataArray[5]);
        var dec = ToDeclination(dataArray[10]);
        if (lat == null || lon == null) return null;
        if (dec == null)
        {
            // dec = 0;
        }

        if (dataArray[4].Equals("s", StringComparison.OrdinalIgnoreCase)) lat *= -1;
        if (dataArray[6].Equals("w", StringComparison.OrdinalIgnoreCase)) lon *= -1;
        if (dataArray[11].Equals("w", StringComparison.OrdinalIgnoreCase))
        {
            // dec *= -1;
        }

        var gps = new SDataGps
        {
            Latitude = lat.Value,
            Longitude = lon.Value
        };
        return gps;
    }

    // private object ParseVTG(string data)
    // {
    //     //TODO: 按实际需求编写
    //     return null;
    // }
    // private object ParseZDA(string data)
    // {
    // 	//TODO: 按实际需求编写
    // 	return null;
    // }
    // private object ParseGST(string data)
    // {
    // 	//TODO: 按实际需求编写
    // 	return null;
    // }
    // 将字符串转换为浮点数度，原始格式为：ddmm.mmmmmm
    private static double? ToDegree(string value)
    {
        try
        {
            var raw = decimal.Parse(value);
            raw /= 100;
            decimal deg = (int)raw;
            var min = (raw - deg) * 100;
            return (double)(deg + min / 60);
        }
        catch
        {
            return null;
        }
    }

    // 转换为磁偏角
    private static float? ToDeclination(string value)
    {
        try
        {
            var raw = float.Parse(value);
            return (short)(raw * 10);
        }
        catch
        {
            return null;
        }
    }

    // 获取两个点的距离，单位米
    private static double GetDistanceByPosition(double lantitude1, double longitude1, double lantitude2,
        double longitude2)
    {
        var dLat1InRad = lantitude1 * (Math.PI / 180);
        var dLong1InRad = longitude1 * (Math.PI / 180);
        var dLat2InRad = lantitude2 * (Math.PI / 180);
        var dLong2InRad = longitude2 * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var dDistance = 6378.137 * c * 1000;
        return dDistance;
    }

    #endregion

    #region Helper

    private bool VerifyDFindAntennas(out string exception, ref byte antennaIndex)
    {
        exception = string.Empty;
        if (_dfindAntennas == null || _dfindAntennas.Length == 0) return false;
        _dfAntennaGroupCount = _dfindAntennas[0].GroupCount;
        foreach (var antenna in _dfindAntennas)
        {
            // 生成天线编号
            antenna.Index = antennaIndex++;
            var pattern =
                "^((00)?[0-9]|0?[0-9]{{2}}|1[0-9][0-9]|2[0-4][0-9]|25[0-5]),{0},(0[xX])?[0-9a-fA-F]{{1,2}},((0[xX])?[0-9a-fA-F]{{1,2}}\\|){{{1}}}(0[xX])?[0-9a-fA-F]{{1,2}}$";
            pattern = string.Format(pattern, antenna.GroupCount, antenna.GroupCount - 1);
            var reg = new Regex(pattern,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace |
                RegexOptions.CultureInvariant); // 忽略大小写，忽略空白符
            if (!reg.IsMatch(antenna))
            {
                exception = $"天线名称：{antenna.Name}，天线摘要：{antenna}";
                return false;
            }

            if (antenna.GroupCount <= 0)
            {
                exception = "天线分组不得小于零";
                return false;
            }

            if (antenna.StartFrequency > antenna.StopFrequency)
            {
                exception = "天线频率上限不得小于下限";
                return false;
            }

            if (antenna.GroupCount != _dfAntennaGroupCount)
            {
                exception = $"天线：{antenna.Name} 与其天线分组数不一致，天线分组数应为：{_dfAntennaGroupCount}，实际分组数为：{antenna.GroupCount}";
                return false;
            }
        }

        return true;
    }

    private void RaiseDfAntennaSelection()
    {
        if (TaskState != TaskState.Start ||
            (CurFeature & (FeatureType.FFDF | FeatureType.WBDF | FeatureType.SSE | FeatureType.ScanDF)) == 0) return;
        if (_dfindAntennas == null || _dfindAntennas.Length == 0)
            // TODO
            // SendMessage(MessageDomain.Task, MessageType.Error, "未检测到可用的测向天线，请检查测向天线配置");
            return;
        // 筛选频段范围和极化方式同时满足要求的测向天线
        DfAntennaInfo selectedAntenna = null;
        if ((CurFeature & FeatureType.ScanDF) > 0)
        {
            selectedAntenna = Array.Find(_dfindAntennas,
                item => DfPolarization == item.Polarization && _startFrequency >= item.StartFrequency &&
                        _stopFrequency <= item.StopFrequency);
            if (selectedAntenna == null)
            {
                var frequencyRange = new StringBuilder();
                foreach (var antenna in _dfindAntennas)
                {
                    var range =
                        $"【{(antenna.Polarization == Polarization.Vertical ? "垂直极化" : "水平极化")}, {antenna.StartFrequency}MHz ~ {antenna.StopFrequency}MHz】";
                    frequencyRange.AppendLine(range);
                }

                // TODO 
                // SendMessage(MessageDomain.Task, MessageType.Error, string.Format("扫描测向时，频率不能跨越天线分段；请确保频段范围在以下任意天线分段以内：\n{0}", frequencyRange.ToString()));
                return;
            }
        }
        else
        {
            selectedAntenna = Array.Find(_dfindAntennas,
                item => DfPolarization == item.Polarization && _frequency >= item.StartFrequency &&
                        _frequency <= item.StopFrequency);
            if (selectedAntenna == null)
                // TODO
                // SendMessage(MessageDomain.Task, MessageType.Error, "未检测到适用于当前频率的测向天线，请检查天线配置");
                return;
        }

        lock (_parameterLock)
        {
            _dfAntennaIndex = selectedAntenna.Index;
        }

        // 更新测向参数
        _dfDataSource.SetParameter("ChannelCount", DfChannelCount);
        _dfDataSource.SetParameter("AntennaIndex", selectedAntenna.Index);
        _dfDataSource.SetParameter("Aperture", selectedAntenna.Aperture);
        _dfDataSource.SetParameter("AngleOffset", selectedAntenna.Deviation);
        _dfDataSource.SetParameter("GroupCount", selectedAntenna.GroupCount);
        _dfDataSource.SetParameter("AngleCount", selectedAntenna.AngleCount);
        SendCommand($"ANT:SEL:IND {selectedAntenna.Index}");
    }

    private void RaiseDfParameterSelection(string name, object value)
    {
        try
        {
            _dfDataSource?.SetParameter(
                _refDfParameters?.TryGetValue(name, out var parameter) is true ? parameter : name, value);
        }
        catch
        {
            // ignored
        }
    }

    private bool IsScanValid(object data)
    {
        // 验证扫描模式是否匹配
        if (data is not RawScan raw
            || (_scanMode == ScanMode.Pscan && raw.Tag != (int)DataType.Scan)
            || (_scanMode == ScanMode.Fscan && raw.Tag != (int)DataType.Scan + 2)
            || (_scanMode == ScanMode.MScan && raw.Tag != (int)DataType.Scan + 4)
            || (_scanMode != ScanMode.MScan
                && (Math.Abs(raw.StartFrequency / 1000000.0d - _startFrequency) > _epsilon
                    || Math.Abs(raw.StopFrequency / 1000000.0d - _stopFrequency) > _epsilon
                    || Math.Abs(raw.StepFrequency / 1000.0d - _stepFrequency) > _epsilon)
            )
           )
        {
#if WRITE_DEBUG_INFO && DATA_INFO
				Console.WriteLine("Invalid scan info: {0}", raw == null ? "null info" : string.Format("start={1}, stop={2}, step={3}", raw.StartFrequency, raw.StopFrequency, raw.StepFrequency));
#endif
            return false;
        }

        return true;
    }

    private void SendCommand(string cmd)
    {
        var sendBuffer = Encoding.Default.GetBytes(cmd + "\r\n");
        var bytesToSend = sendBuffer.Length;
        var offset = 0;
        var total = 0;
        try
        {
            // 流式套接字，循环发送，直到所有数据全部发送完毕
            while (_ctrlChannel != null && total < bytesToSend)
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

    private string GetChannelPhaseDiffsInString()
    {
        SendCommand("DFIN:PHASE:CAL?");
        while (true)
            try
            {
                var result = _streamReader.ReadLine();
                if (result != null && result.StartsWith("##")) return result;
            }
            catch
            {
                return string.Empty;
            }
    }

    private bool WaitForModeSwitchFeedback(int times = 5, int delay = 10)
    {
        Thread.Sleep(delay);
        var waitingTimes = 0;
        while (waitingTimes < times)
            try
            {
                SendCommand("MEAS:SWIT:FINI?");
                var result = _streamReader.ReadLine();
                if (result != null && result.ToLower().Contains("on")) return true;
            }
            catch
            {
            }
            finally
            {
                waitingTimes++;
            }

        return false;
    }

    private void OperateDdr()
    {
        if ((CurFeature & FeatureType.FFM) == 0) return;
        SendCommand("DDR:READ:CONT DIS");
        if (_ssMode.ToLower().Equals("record"))
        {
            SendCommand("DDR:TRIG FRE");
            SendCommand("DDR:WRIT:STAR 0");
            SendCommand("DDR:WRIT:LENG 1073741824");
            SendCommand("DDR:WRIT");
            _ddrParameterReadingTask = GetDdrParameterReadingTaskAsync(1000, 200);
        }
        else if (_ssMode.ToLower().Equals("playback"))
        {
            var tuple = _ddrParameterReadingTask?.Result;
            if (tuple == null)
                // SendMessage(MessageDomain.Task, MessageType.Warning, "未找到有效的记录");
                return;
            var offset = tuple.Item1;
            var frequency = tuple.Item2;
            var span = tuple.Item3;
            var spanRate = tuple.Item4;
            var samplingRate = tuple.Item5;
            SendCommand($"FREQ:OFFS {_frequencyOffset} MHz");
            SendCommand(
                $"DDR:READ:CONT MULT,{frequency} Hz,{spanRate},{span} Hz,{0},{1073741824},{samplingRate} Hz,{offset},normal,{0},{0}");
        }
        else
        {
            var shift = Math.Pow(2, RbwShift % 8) * 100;
            SendCommand($"BAND:RES {_ifBandwidth / shift} kHz");
        }
    }

    private Task<Tuple<int, long, long, int, int>> GetDdrParameterReadingTaskAsync(int interval, int cycles)
    {
        _ddrCts = new CancellationTokenSource();
        return Task.Factory.StartNew(() =>
        {
            var waitingCycles = 0;
            while (waitingCycles < cycles && !_ddrCts.IsCancellationRequested)
                try
                {
                    SendCommand("DDR:READ:OFFS?");
                    var offsetResult = _streamReader.ReadLine();
                    SendCommand("DDR:READ:SPAN?");
                    var spanRateResult = _streamReader.ReadLine();
                    SendCommand("DDR:READ:SRAT?");
                    var srateResult = _streamReader.ReadLine();
                    if (int.TryParse(offsetResult, out var offset) && offset > 0 &&
                        int.TryParse(spanRateResult, out var spanRate) && int.TryParse(srateResult, out var srate))
                    {
                        SendData(new List<object> { "completed" });
                        return new Tuple<int, long, long, int, int>(offset, (long)(_frequency * 1000000),
                            (long)(_ifBandwidth * 1000), spanRate, srate);
                    }
                    else
                    {
                        SendData(new List<object> { "in progress" });
                    }
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch
                {
                }
                finally
                {
                    Thread.Sleep(interval);
                    waitingCycles++;
                }

            return null;
        }, _ddrCts.Token, TaskCreationOptions.None, TaskScheduler.Default);
    }

    private string GetAuthToken()
    {
        SendCommand("VERS:HW?");
        for (var index = 0; index < 5; ++index)
            try
            {
                var result = _streamReader.ReadLine();
                if (string.IsNullOrEmpty(result) || result.Length != 16) continue;
                return result;
            }
            catch
            {
            }

        return string.Empty;
    }

    private int GetDfavgTimes()
    {
        if (_dfSamplingCount != 0 && _bandwidthAndSamplingRateDic.TryGetValue(_ifBandwidth, out var value))
        {
            var us = _dfSamplingCount / value;
            var count = (int)(IntegrationTime / us);
            if (count > 120) // 不要超过120次
                count = 120;
            return count;
        }

        return 0;
    }

    private double GetProperSamplingRateByBandwidthString(string bandwidth, double samplingRate)
    {
        if (_bandwidthAndSamplingRateInParamsDic != null && _bandwidthAndSamplingRateInParamsDic.ContainsKey(bandwidth))
        {
            var delta = 9999.0d;
            var minimum = 0;
            for (var index = 0; index < _bandwidthAndSamplingRateInParamsDic[bandwidth].Count; ++index)
            {
                var temp = Math.Abs(_bandwidthAndSamplingRateInParamsDic[bandwidth][index] - samplingRate);
                if (temp < delta)
                {
                    delta = temp;
                    minimum = index;
                }
            }

            return _bandwidthAndSamplingRateInParamsDic[bandwidth][minimum];
        }

        return double.NaN;
    }

    #endregion
}