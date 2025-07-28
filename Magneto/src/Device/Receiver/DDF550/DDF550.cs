using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

public partial class Ddf550 : DeviceBase
{
    public Ddf550(Guid deviceId) : base(deviceId)
    {
        _dataIqQueue = new ConcurrentQueue<byte[]>();
        _dataQueue = new ConcurrentQueue<byte[]>();
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            // 释放资源
            ReleaseResource();
            // 初始化网络套接字
            if (!InitSocket()) return false;
            Thread.Sleep(200);
            // 初始化天线
            //InitAntennas();
            // 初始化设备
            InitDevice();
            // 初始化线程
            InitThread();
            // 监测设备连接状态
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _dataQueue.Clear();
        TraceDeleteInactive(out _);
        DisableAllSwitch();
        // 删除所有数据的连接
        TraceDelete(_localIp, _localPort);
        TraceDelete(_localIqIp, _localIqPort);
        if (CurFeature == FeatureType.FFM)
        {
            SetParamsFq();
            // 单频测量永远输出电平
            _mediaType |= MediaType.Level;
        }
        else if (CurFeature is FeatureType.FFDF or FeatureType.WBDF)
        {
            _mediaType &= ~MediaType.Scan;
            SetParamsDf();
            if (CurFeature == FeatureType.FFDF)
            {
                _mediaType |= MediaType.Dfind;
                _mediaType |= MediaType.Spectrum;
            }
            else
            {
                _mediaType |= MediaType.Dfpan;
                _mediaType |= MediaType.Spectrum;
            }
        }
        // TODO DFF550不支持输出FREQ_LOW
        else if (CurFeature == FeatureType.SCAN)
        {
            _mediaType = MediaType.None;
            //TODO 设置频段扫描参数
            SetRxpscanParams();
            _mediaType |= MediaType.Scan;
        }
        else if (CurFeature == FeatureType.ScanDF)
        {
            _mediaType = MediaType.None;
            SetParamsScan();
            _mediaType |= MediaType.Scan;
        }

        if (CurFeature is FeatureType.FFDF or FeatureType.WBDF or FeatureType.ScanDF or FeatureType.MScanDf)
        {
            if (DdfAntenna >= 0)
            {
                //SendControlCmd("ROUTe:VUHF (@" + _dfAntenna.ToString() + ")");
                var input = (ErfInput)DdfAntenna;
                AntennaControl(EAntCtrlMode.AntCtrlModeManual, input, null);
            }
            else if (DdfAntenna == -1)
            {
                //SendControlCmd("ROUTe:AUTO ON");
                AntennaControl(EAntCtrlMode.AntCtrlModeAuto, null, null);
            }
        }
        else if (MonitorAntenna >= 0)
        {
            //SendControlCmd("ROUTe:VUHF (@" + _monitorAntenna.ToString() + ")");
            var input = (ErfInput)MonitorAntenna;
            AntennaControl(EAntCtrlMode.AntCtrlModeManual, input, null);
        }
        else if (MonitorAntenna == -1)
        {
            //SendControlCmd("ROUTe:AUTO ON");
            AntennaControl(EAntCtrlMode.AntCtrlModeAuto, null, null);
        }

        if (UseGps || UseCompass)
            CheckGpsCompassSwitch(true);
        else
            CheckGpsCompassSwitch(false);
    }

    /// <summary>
    ///     停止功能
    /// </summary>
    public override void Stop()
    {
        DisableAllSwitch();
        SetAllSwitchOff();
        TraceDeleteInactive(out var msg);
        if (!string.IsNullOrWhiteSpace(msg)) Trace.WriteLine($"删除连接消息：{msg}");
        SetModeRx();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name is ParameterNames.DfindMode
            // or ParameterNames.LevelThreshold
            or ParameterNames.AttCtrlType
            or ParameterNames.Attenuation
            or ParameterNames.IntegrationTime)
            SetParamsScan();
    }

    #region 成员变量

    private readonly double _epsilon = 1.0E-7d;

    /// <summary>
    ///     命令发送套接字
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     数据接收套接字
    /// </summary>
    private Socket _dataReceiveSocket;

    /// <summary>
    ///     接收IQ数据套接字
    /// </summary>
    private Socket _dataReceiveIqSocket;

    /// <summary>
    ///     数据接收线程
    /// </summary>
    private Task _dataReceiveTask;

    private CancellationTokenSource _dataReceiveCts;

    /// <summary>
    ///     双缓存线程
    /// </summary>
    private Task _dataReceiveIqTask;

    private CancellationTokenSource _dataReceiveIqCts;

    /// <summary>
    ///     数据处理线程
    /// </summary>
    private Task _dataProcTask;

    private CancellationTokenSource _dataProcCts;

    /// <summary>
    ///     IQ数据单独处理线程
    /// </summary>
    private Task _dataProcIqTask;

    private CancellationTokenSource _dataProcIqCts;

    /// <summary>
    ///     命令应答信息
    /// </summary>
    private readonly byte[] _cmdReply = null;

    //待处理业务数据
    private readonly ConcurrentQueue<byte[]> _dataQueue;

    //IQ数据缓存
    private readonly ConcurrentQueue<byte[]> _dataIqQueue;

    // 连接到设备的数据通道的IP地址
    private string _localIp = "127.0.0.1";

    // 连接到设备的数据通道的端口
    private int _localPort;

    // 连接到设备的数据通道的IP地址-IQ数据使用
    private string _localIqIp = "127.0.0.1";

    // 连接到设备的数据通道的端口-IQ数据使用
    private int _localIqPort;

    /// <summary>
    ///     订阅的数据类型
    /// </summary>
    private MediaType _mediaType = MediaType.None;

    // 扫描测向当前正在使用的频段ID
    private int _scanRangeId;

    // 扫描测向当前频段每一个完整的帧需要返回的数据包数
    private int _numHops;

    // 内部使用的中频带宽,Params.cs中暴露的中频带宽其实是解调带宽
    private double _ifbw = 100.0d;

    // Xml协议包头
    public static readonly byte[] XmlStart = { 0xB1, 0xC2, 0xD3, 0xE4 };

    // Xml协议包尾
    public static readonly byte[] XmlEnd = { 0xE4, 0xD3, 0xC2, 0xB1 };

    #endregion
}