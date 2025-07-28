using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.XE_VUHF_28.Commands;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF_28;

public partial class XeVuhf28 : DeviceBase
{
    private const int IqServerPort = 51000;

    //存放待处理数据
    private readonly ConcurrentQueue<RawData> _dataQueue = new();

    private readonly ConcurrentDictionary<uint, MScanSegmentInfo> _mscanSegmentInfos = new();
    private readonly object _syncObj = new();

    //用于实现积分时间,返回积分时间内质量最高的那个示向度
    private readonly Stopwatch _watcherIntegrationTime = new();

    //当前天线分段信息
    private Dictionary<int, XeAntennaSubRange> _antennaSubRanges;

    //扫描数据计算缓存
    private short[] _arrScanBuffer;

    //保存测向质量最大的示向度信息,注意在频率切换时重置
    private DfInfo _azi;

    //用于设置参数信息，任务启动时根据功能初始化，旨在减少不必要的参数设置
    private CommandBase _cmdCollector;

    //TCP指令端口
    //Open a client TCP/IP link on port 49000 for the transmission and reception of commands.
    private Socket _cmdSocket;

    //由于XE返回的是正北示向度，设备返回到客户端的必须为相对示向度，缓存最新的罗盘值供计算相对示向度
    private float _compass;
    private uint _currentPhaseNo;

    private CancellationTokenSource _dataProcessCts;

    //数据处理线程
    private Task _dataProcessTask;

    //最新收到0x66消息(TCP消息，每隔1s上报)的时间，以此来检查XE是否断连
    private DateTime _detectTime;

    //离散扫描待发送数据包的索引,并非所有频点都有数据返回，待下次收到数据时以理论低噪填充
    private volatile int _dstIndex;
    private CancellationTokenSource _iqCaptureCts;
    private Task _iqCaptureTask;
    private volatile bool _isMscanDataValid;

    private volatile bool _isMscanRunning;

    //订阅的数据类型
    private MediaType _media = MediaType.None;

    //用于实现频段扫描的检波方式
    private int _realNumLoops;
    private bool _sharedFolderValid;

    private CancellationTokenSource _tcpDataProcessCts;

    //接收从tcp返回的消息
    private Task _tcpDataProcessTask;
    private Socket _tcpIqSocket;
    private CancellationTokenSource _udpAudioCaptureCts;
    private Task _udpAudioCaptureTask;

    private int _udpAudioPort = 49400;

    //音频数据端口
    private Socket _udpAudioSocket;
    private CancellationTokenSource _udpBbfftCaptureCts;
    private Task _udpBbfftCaptureTask;

    private int _udpBbfftPort = 49100;

    //UDP数据端口，由于测向数据端口不可修改，目前如果需要在一个服务端配置多台XE的话，这多台XE的IP必须要设置为不同的分段，这样服务端才能绑定同一个端口
    //宽带FFT数据
    private Socket _udpBbfftSocket;
    private CancellationTokenSource _udpBbituCaptureCts;
    private Task _udpBbituCaptureTask;

    private int _udpBbituPort = 49700;

    //宽带ITU数据端口
    private Socket _udpBbituSocket;
    private CancellationTokenSource _udpDfCaptureCts;
    private Task _udpDfCaptureTask;

    private int _udpDfPort = 49300;

    //测向数据端口，不可修改
    private Socket _udpDfSocket;
    private CancellationTokenSource _udpExtractionCaptureCts;
    private Task _udpExtractionCaptureTask;

    private int _udpExtractionPort = 49200;

    //宽带下根据测量门限抽取结果数据端口
    private Socket _udpExtractionSocket;
    private CancellationTokenSource _udpLevelCalibrationCaptureCts;
    private Task _udpLevelCalibrationCaptureTask;

    private int _udpLevelCalibrationPort = 49600;

    //电平校准端口
    private Socket _udpLevelCalibrationSocket;
    private CancellationTokenSource _udpNbfftCaptureCts;
    private Task _udpNbfftCaptureTask;

    private int _udpNbfftPort = 50000;

    //窄带FFT数据端口
    private Socket _udpNbfftSocket;
    private CancellationTokenSource _udpNbituCaptureCts;
    private Task _udpNbituCaptureTask;

    private int _udpNbituPort = 49800;

    //窄带ITU数据端口
    private Socket _udpNbituSocket;

    //当前版本信息
    public uint Version = 28;

    public XeVuhf28(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            //检查非托管资源并释放
            ReleaseResource();
            //初始化网络连接等
            result &= InitNetwork();
            if (result)
            {
                //初始化数据接收和处理线程
                InitTasks();
                //设置心跳检查
                SetHeartBeat(_cmdSocket);
            }
        }

        if (!result) ReleaseResource();
        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _dataQueue.Clear();
        _mscanSegmentInfos.Clear();
        switch (CurFeature)
        {
            case FeatureType.FFM:
            case FeatureType.ITUM:
                _media |= MediaType.Level;
                _cmdCollector = new FfmCommand();
                break;
            case FeatureType.SCAN:
                _media |= MediaType.Scan;
                _cmdCollector = new ScanCommand();
                break;
            case FeatureType.FFDF:
                _media |= MediaType.Dfind;
                _cmdCollector = new FdfCommand();
                break;
            case FeatureType.IFMCA:
                _media |= MediaType.Spectrum;
                _cmdCollector = new IfmcaCommand();
                break;
            case FeatureType.ScanDF:
                _media |= MediaType.Scan;
                _cmdCollector = new ScanDfCommand();
                break;
            case FeatureType.MScan:
                _media |= MediaType.Scan;
                _cmdCollector = new MScanCommand();
                break;
            case FeatureType.WBDF:
                _media |= MediaType.Dfpan;
                _cmdCollector = new WbdfCommand();
                break;
        }

        if ((CurFeature & (FeatureType.SCAN | FeatureType.ScanDF)) > 0)
        {
            var total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            /*TODO: XE不管是多段还是1段都是以最小频率和最大频率为扫描范围，例如[20,21],[2999,3000]，设备实际是从20扫描到3000然后抽取你要的分段数据给你，
             * 当分辨率较小时扫描将无法进行，例如LG319软件上会提示“信号扫描操作被拒绝执行：分段超限！”，由于在实验中每个分辨率可支持的最大扫描点数不同，
             * 此处统一限制为64W个频点，当超过此点数就发送一条消息到客户端用以提示用户并停止该任务，V10里再完善该处理，信号侦察功能由客户端进行提示此处不再处理
             */
            if (total > 640000) throw new Exception("扫描点数超过640000，请重新设置频段信息。");
            if (CurFeature == FeatureType.SCAN)
                lock (_syncObj)
                {
                    _realNumLoops = 0;
                    _arrScanBuffer = new short[total];
                }
        }

        if (_cmdCollector != null)
        {
            var deviceParams = new DeviceParams
            {
                UdpAudioPort = _udpAudioPort,
                UdpDfPort = _udpDfPort,
                UdpNbfftPort = _udpNbfftPort,
                UdpNbituPort = _udpNbituPort,
                UdpBbituPort = _udpBbituPort,
                UdpLevelCalibrationPort = _udpLevelCalibrationPort,
                Frequency = Frequency,
                IfBandWidth = IfBandWidth,
                FilterBandWidth = FilterBandwidth,
                DfBandWidth = DfBandWidth,
                StartFrequency = StartFrequency,
                StepFrequency = StepFrequency,
                StopFrequency = StopFrequency,
                AmpliConfig = AmpliConfig,
                Attenuation = Attenuation,
                AudioSwitch = AudioSwitch,
                ItuSwitch = ItuSwitch,
                IqSwitch = IqSwitch,
                Beta = Beta,
                UdpBbfftPort = _udpBbfftPort,
                DemMode = DemMode,
                DetectionMode = DetectionMode,
                LevelThreshold = LevelThreshold,
                SquelchSwitch = SquelchSwitch,
                SquelchThreshold = SquelchThreshold,
                Version = Version,
                XdB = XdB,
                XeIntTime = XeIntTime,
                FmFilter = FmFilter,
                MScanPoints = MscanPoints,
                DdcChannels = _ddcChannels,
                ResolutionBandwidth = ResolutionBandwidth,
                MaxChanCount = MaxChanCount,
                QualityThreshold = QualityThreshold,
                CurrAntenna = CurrAntenna,
                DwellTime = DwellTime,
                HoldTime = HoldTime,
                SharedFolderValid = _sharedFolderValid
            };
            _currentPhaseNo = _cmdCollector.Init(deviceParams, SendCmd);
            _isMscanRunning = CurFeature == FeatureType.MScan;
            if (CurFeature == FeatureType.FFDF && _integralTime != 0)
            {
                _watcherIntegrationTime.Reset();
                _watcherIntegrationTime.Start();
            }

            _dstIndex = 0;
        }
    }

    public override void Stop()
    {
        if (CurFeature == FeatureType.MScan)
        {
            _isMscanRunning = false;
            _isMscanDataValid = false;
        }

        base.Stop();
        _cmdCollector?.Stop();
        _watcherIntegrationTime.Reset();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        _detectTime = DateTime.Now;
    }

    protected override void KeepAlive(object connObject)
    {
        /*1.通过常规的心跳检测机制检查设备连接是否异常
         * 2.连接上XE后,XE会每隔1s左右返回一条0x66信息(包含GPS和Compass信息)，即使没有连接GPS或者Compass该消息也会正常返回(只是
         * 相关字段值无效而已)，可同时通过比较最新收到的该消息的时间与当前时间的间隔信息来检查设备是否异常*/
        if (connObject is not Socket socket) return;
        while (true)
        {
            if (!IsSocketConnected(socket)) break;
            //TODO:在对设备下发参数的时候该消息会有延迟，此处暂时设置30s
            var timeSpan = DateTime.Now - _detectTime;
            if (timeSpan.TotalMilliseconds > 30000)
            {
                Trace.WriteLine($"{DeviceInfo.DisplayName}（{DeviceId}）心跳线程检测到异常");
                break;
            }

            Thread.Sleep(1000);
        }

        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }
}

internal class MScanSegmentInfo
{
    public uint Identifier { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? StopTime { get; set; }
    public bool Detected { get; set; }
    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
}