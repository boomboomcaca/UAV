using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.E3238S;

public partial class E3238S : DeviceBase
{
    #region 构造函数

    public E3238S(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 全局变量

    /// <summary>
    ///     TCP 套接字，用于发送命令，接收数据(频谱数据、IQ数据),检查设备连接,()
    /// </summary>
    private Socket _socket;

    /// <summary>
    ///     缓冲业务数据
    /// </summary>
    private readonly MQueue<byte[]> _dataQueue = new();

    /// <summary>
    ///     接收数据线程(频谱数据、IQ数据)
    /// </summary>
    private Task _receiveDataTask;

    /// <summary>
    ///     解析接收数据线程
    /// </summary>
    private Task _paseDataTask;

    private CancellationTokenSource _receiveDataTokenSource;
    private CancellationTokenSource _paseDataTaskTokenSource;

    /// <summary>
    ///     判断功能是否在运行
    /// </summary>
    private bool _isRunning;

    /// <summary>
    ///     处理单频测量切换频谱带宽时，出现返回数据与发送的命令不一致时，重新发送命令标志
    /// </summary>
    private bool _resetFixFqParms = true;

    /// <summary>
    ///     互斥锁
    /// </summary>
    private readonly object _locker = new();

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            ReleaseResources();
            InitSocket();
            InitE3238S();
            InitAllTask();
            SetHeartBeat(_socket);
        }

        return result;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        lock (_locker)
        {
            _dataQueue.Clear();
        }

        _isRunning = true;
        SendCommand("start");
        if (CurFeature == FeatureType.FFM) InitFixFqParms();
        if (CurFeature == FeatureType.SCAN) InitScanParms();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        _isRunning = false;
        //停止扫描 停止接收数据
        SendCommand("stop");
        //关闭频域数据
        SendCommand("spewRawData:Off");
        //关闭时域数据
        SendCommand("spewRawTimeData:Off");
        //关闭所有扫描段
        SendCommand("*band.bandStatus:Inactive");
        //关闭所有声音 
        SendCommand("*audioParms:1 100 0 0 0 0 1750 0");
        lock (_locker)
        {
            _dataQueue.Clear();
        }

        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}