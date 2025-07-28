using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF5GTS;

public partial class Ddf5Gts : DeviceBase
{
    public Ddf5Gts(Guid deviceId) : base(deviceId)
    {
        _cmdReply = new byte[4096];
    }

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="moduleInfo"></param>
    /// <returns></returns>
    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            // 释放资源
            ReleaseResource();
            // 初始化网络套接字
            InitSocket();
            // 初始化天线
            InitAntennas();
            // 初始化设备
            InitDevice();
            // 初始化线程
            InitTasks();
            // 监测设备连接状态
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    /// <summary>
    ///     开启功能
    /// </summary>
    /// <returns></returns>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _dataQueue.Clear();
        TraceDeleteInactive();
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
        else if (CurFeature is FeatureType.FDF or FeatureType.WBDF or FeatureType.SSE)
        {
            _mediaType &= ~MediaType.Scan;
            SetParamsDf();
            if (CurFeature == FeatureType.FDF)
            {
                _mediaType |= MediaType.Dfind;
                _mediaType |= MediaType.Spectrum;
            }
            else if (CurFeature is FeatureType.WBDF or FeatureType.SSE)
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
    }

    /// <summary>
    ///     停止功能
    /// </summary>
    /// <returns></returns>
    public override void Stop()
    {
        DisableAllSwitch();
        SetAllSwitchOff();
        TraceDeleteInactive();
        SetModeRx();
        base.Stop();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }

    #region 成员变量

    private readonly double _epsilon = 1.0E-7d;

    /// <summary>
    ///     命令发送套接字
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     旧协议的5555端口套接字，用来查询GPS
    /// </summary>
    private Socket _oldSocket;

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
    ///     GPS与罗盘读取线程
    /// </summary>
    private Task _dataGpsCompassTask;

    private CancellationTokenSource _dataGpsCompassCts;

    /// <summary>
    ///     命令应答信息
    /// </summary>
    private readonly byte[] _cmdReply;

    //待处理业务数据
    private readonly ConcurrentQueue<byte[]> _dataQueue = new();

    //IQ数据缓存
    private readonly ConcurrentQueue<byte[]> _dataIqQueue = new();

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

    #region 存储从设备中获取的设备信息

    private string _compassName;
    private readonly List<string> _antennaName = new();
    private double _heading;

    #endregion

    #endregion
}