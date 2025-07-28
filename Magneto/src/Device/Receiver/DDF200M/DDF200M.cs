using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF200M;

public partial class Ddf200M : DeviceBase
{
    /// <summary>
    ///     保留离散扫描应包含的频点，用于判断本包数据的索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    /// <summary>
    ///     缓存UDP业务数据
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _udpDataQueue = new();

    /// <summary>
    ///     TCP套接字 主要用于发送命令和接收ITU和电平数据
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     UDP 套接字 用于接收扫描数据
    /// </summary>
    private Socket _dataSocket;

    /// <summary>
    ///     本地连接设备的IP
    /// </summary>
    private string _localaddr;

    /// <summary>
    ///     本地用于接收业务数据的UDP端口
    /// </summary>
    private int _localudpport = 19000;

    /// <summary>
    ///     业务数据类型
    /// </summary>
    private MediaType _media = MediaType.None;

    private CancellationTokenSource _tcpDataCts;

    /// <summary>
    ///     TCP 采集ITU 数据和电平数据线程
    /// </summary>
    private Task _tcpDataTask;

    /// <summary>
    ///     UDP 采集业务数据(频谱数据、测向数据)
    /// </summary>
    private Task _udpDataCaptrueTask;

    private CancellationTokenSource _udpDataCaptureCts;
    private CancellationTokenSource _udpDataProcessCts;

    /// <summary>
    ///     解析 UDP 数据并发送客户端
    /// </summary>
    private Task _udpDataProcessTask;

    public Ddf200M(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            //释放非托管资源
            ReleaseSource();
            //初始化网络
            InitNetWork();
            //初始化设备
            InitDevice();
            //初始化线程
            InitAllThread();
            //启动心跳检测
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if ((CurFeature & (FeatureType.FDF | FeatureType.WBDF)) > 0)
        {
            //设置为测向机模式
            SendCmd("MEAS:APPL DF");
            //关闭步进自动
            SendCmd("CALC:IFP:STEP:AUTO OFF");
            if (CurFeature == FeatureType.WBDF)
            {
                SetWbSpan(_spectrumspan);
                SendCmd($"CALCulate:IFPan:STEP {_channelBandWidth} kHz");
            }
            else
            {
                SetDfBandwidth(_dfBandWidth);
            }

            if (DdfAntenna == -1)
                SendCmd("ROUTe:Auto ON");
            else
                SendCmd("ROUTe:VUHF (@" + DdfAntenna + ")");
            // 宽带测向必须包含频谱数据
            _media = (FeatureType.FDF & CurFeature) > 0
                ? _media |= MediaType.Dfind
                : _media |= MediaType.Dfpan | MediaType.Spectrum;
        }
        else
        {
            //设置为接收机模式
            SendCmd("MEAS:APPL RX");
            //打开步进自动以便频谱图最直观展示
            SendCmd("CALC:IFP:STEP:AUTO ON");
            if (MonitorAntenna == -1)
                SendCmd("ROUTe:Auto ON");
            else
                SendCmd("ROUTe:VUHF (@" + MonitorAntenna + ")");
            if (CurFeature.Equals(FeatureType.FFM))
                _media |= MediaType.Level;
            else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne | FeatureType.SCAN | FeatureType.FScne)) > 0)
                //if (_curFeature.Equals(SpecificAbility.MSCAN) || _curFeature.Equals(FeatureType.MScne))
                //{
                //    ScanMode = ScanMode.MSCAN;
                //}
                //else if (_curFeature == FeatureType.FScne)
                //{
                //    _scanMode = ScanMode.FSCAN;
                //}
                _media |= MediaType.Scan;
        }

        _udpDataQueue.Clear();
        InitUdpPath();
    }

    public override void Stop()
    {
        SendCmd("ABORT");
        CloseUdpPath();
        _media = MediaType.None;
        _udpDataQueue.Clear();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseSource();
    }
}