using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMB;

public partial class Esmb : DeviceBase
{
    #region 构造函数

    public Esmb(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     TCP Socket 用于发送指令
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     UDP Socket 用于接收业务数据
    /// </summary>
    private Socket _dataSocket;

    //本地用于接收业务数据的UDP端口
    private readonly int _localUdpPort = 19000;

    /// <summary>
    ///     采集业务数据线程(ITU数据、电平数据)
    /// </summary>
    private Task _tcpDataTask;

    private CancellationTokenSource _tcpDataTokenSource;

    /// <summary>
    ///     采集业务数据线程(音频数据、频谱数据)
    /// </summary>
    private Task _udpDataCaptureTask;

    private CancellationTokenSource _udpDataCaptureTokenSource;

    /// <summary>
    ///     解析UDP接收到的数据线程
    /// </summary>
    private Task _udpDataDispatchTask;

    private CancellationTokenSource _udpDataDispatchTokenSource;

    /// <summary>
    ///     保存当前任务所订阅的数据类型
    /// </summary>
    private MediaType _media = MediaType.None;

    /// <summary>
    ///     存放UDP接收到的数据
    /// </summary>
    private ConcurrentQueue<byte[]> _udpDataQueue;

    /// <summary>
    ///     保留离散扫描的频点，用于判断本包数据的索引
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    private readonly object _ctrlLocker = new();
    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024];

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        if (!base.Initialized(moduleInfo)) return false;
        //清理所有非托管资源
        ReleaseResources();
        //初始化用于通信的Socket
        InitNetWork();
        //初始化设备参数
        InitDevice();
        //初始化线程
        InitTasks();
        //启动心跳检测
        SetHeartBeat(_cmdSocket);
        return true;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (CurFeature.Equals(FeatureType.FFM))
        {
            _media |= MediaType.Level;
        }
        else if ((CurFeature & (FeatureType.MScan | FeatureType.MScne | FeatureType.SCAN | FeatureType.FScne)) > 0)
        {
            if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
                ScanMode = ScanMode.MScan;
            else if (CurFeature == FeatureType.FScne) ScanMode = ScanMode.Fscan;
            _media |= MediaType.Scan;
        }
        else if (CurFeature == FeatureType.IFOUT)
        {
            _media |= MediaType.Iq;
            _media |= MediaType.Spectrum;
        }

        _udpDataQueue.Clear();
        SendMeadiaRequest();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        SendCmd("ABORT");
        //DSCAN扫描停止后若直接删除UDP通道会导致设备死机
        //此处统一处理为先将设备置为单频测量模式再删除UDP通道
        SendCmd("SENS:FREQ:MODE FIX");
        CloseUdpPath();
        Thread.Sleep(200);
        _media = MediaType.None;
        _udpDataQueue.Clear();
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
    }

    #endregion
}