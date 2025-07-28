using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.PR100;

public partial class Pr100 : DeviceBase
{
    #region 构造函数

    public Pr100(Guid id) : base(id)
    {
    }

    #endregion 构造函数

    #region 全局变量

    /// <summary>
    ///     TCP socket,主要用于发送指令
    /// </summary>
    private Socket _cmdSock;

    /// <summary>
    ///     UDP socket ,用于接收扫描数据
    /// </summary>
    private Socket _dataSock;

    /// <summary>
    ///     TCP查询命令及电平数据线程
    /// </summary>
    private Task _tcpDataTask;

    private CancellationTokenSource _tcpDataTokenSource;

    /// <summary>
    ///     UDP获取业务数据线程
    /// </summary>
    private Task _udpDataCaptureTask;

    private CancellationTokenSource _udpDataCaptureTokenSource;

    /// <summary>
    ///     UDP业务数据处理线程
    /// </summary>
    private Task _udpDataDispatchTask;

    private CancellationTokenSource _udpDataDispatchTokenSource;

    /// <summary>
    ///     缓存接收到的业务数据
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _udpDataQueue = new();

    /// <summary>
    ///     保存DSCan扫描数据中本包数据的索引
    /// </summary>
    private int _index;

    /// <summary>
    ///     保留离散扫描应包含的频点，用于判断本包数据的索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    /// <summary>
    ///     保留最新扫描数据，丢包时用此缓存数据填补
    /// </summary>
    private short[] _cacheData;

    /// <summary>
    ///     订阅的数据类型
    /// </summary>
    private MediaType _mediaType = MediaType.None;

    private readonly object _ctrlLocker = new();

    #endregion 全局变量

    #region 框架接口

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (!result) return false;
        InitResources();
        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
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

        InitUdpPath();
        _udpDataQueue.Clear();
        base.Start(feature, dataPort);
    }

    public override void Stop()
    {
        SendCmd("ABORT");
        SendCmd("SENS:FREQ:MODE FIX");
        CloseUdpPath();
        _udpDataQueue.Clear();
        _mediaType = MediaType.None;
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    #endregion
}