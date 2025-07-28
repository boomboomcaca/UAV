using System;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.ESMC.SDK;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMC;

public partial class Esmc : DeviceBase
{
    #region 构造函数

    public Esmc(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 变量/属性

    // private AudioRecorder _audioRecorder = null;
    private GpibClass _gpib;

    /// <summary>
    ///     检查连接的任务
    /// </summary>
    private Task _checkConnectTask;

    private CancellationTokenSource _checkConnectTokenSource;
    private Task _sendSpectrumTask;
    private CancellationTokenSource _sendSpectrumTokenSource;

    /// <summary>
    ///     用于频段扫描的线程
    /// </summary>
    private Task _fScanTask;

    private CancellationTokenSource _fScanTokenSource;

    /// <summary>
    ///     发送扫描数据任务
    /// </summary>
    private Task _sendScanDataTask;

    private CancellationTokenSource _sendScanDataTokenSource;
    private float _preLevel;

    private int _currentMeasureDataCount;

    //private int _SendSpectrumCount = 0;
    private int _fScanNum; //用于判断
    private int _currNum; //FSCAN当前数据接收点数
    private int _totalCount; //扫描的总点数
    private readonly MQueue<object> _scanQueue = new();

    private bool _haveSu; //中频全景组件
    //private bool _haveBp; //电池块
    //private bool _haveDs; //射频频谱数字扫描
    //private bool _haveEr; //扩展RAM组件;
    //private bool _haveHf; //高频组件
    //private bool _haveCw; //覆盖测量
    //private bool _haveFs; //场强测量

    #endregion 变量/属性

    #region 重载基类

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        if (!base.Initialized(moduleInfo)) return false;
        // 此处无任何意义，仅在于屏蔽编译时value并未使用的提示。
        // 释放资源
        ReleaseResources();
        // 初始化设备
        InitDevice();
        InitFeature();
        return true;
    }

    /// <summary>
    ///     任务启动
    /// </summary>
    /// <returns></returns>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (CurFeature.Equals(FeatureType.FFM))
        {
            CurWorkMode = WorkMode.Cw;
            Thread.Sleep(200);
            if (_sendSpectrumTask == null)
            {
                _sendSpectrumTokenSource = new CancellationTokenSource();
                _sendSpectrumTask = new Task(SendSpectrum, _sendSpectrumTokenSource.Token);
                _sendSpectrumTask.Start();
            }
        }
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
        {
            if ((CurFeature & (FeatureType.MScan | FeatureType.MScne)) > 0)
                _scanMode = ScanMode.MScan;
            else
                _scanMode = ScanMode.Fscan;
            StartScan();
        }
    }

    /// <summary>
    ///     任务停止
    /// </summary>
    /// <returns></returns>
    public override void Stop()
    {
        if (CurFeature.Equals(FeatureType.FFM))
            Utils.CancelTask(_sendScanDataTask, _sendSpectrumTokenSource);
        else if ((CurFeature & (FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) >
                 0) StopScan();
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion 重载基类
}