using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

public partial class Ap510 : DeviceBase
{
    private const int Buffersize = 8192;
    private readonly Encoding _encodingDefault;

    #region 构造函数

    public Ap510(Guid deviceId) : base(deviceId)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _encodingDefault = Encoding.GetEncoding("gb2312");
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     SingleCall命令通道
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     ServiceCall命令通道和数据通道
    /// </summary>
    private Socket _dataSocket;

    /// <summary>
    ///     音频通道
    /// </summary>
    private Socket _audioSocket;

    // 当前功能对应的接收机功能指令前缀
    private string _devTaskPref = string.Empty;

    // 当前数据包第一个点对应频段上的偏移量
    private int _pdloc;
    private readonly ConcurrentQueue<byte> _dataQueue = new();
    private readonly ConcurrentQueue<byte> _cmdQueue = new();
    private readonly ConcurrentQueue<ScpiDataStruct> _dataCache = new();

    private readonly ConcurrentQueue<string> _cmdCache = new();

    // 数据处理线程，每个实例独有
    private CancellationTokenSource _dataReceiveCts;
    private Task _dataReceiveTask;
    private CancellationTokenSource _dataProcessCts;
    private Task _dataProcessTask;
    private CancellationTokenSource _cmdReceiveCts;
    private Task _cmdReceiveTask;
    private CancellationTokenSource _cmdProcessCts;
    private Task _cmdProcessTask;
    private CancellationTokenSource _audioCts;
    private Task _audioTask;
    private CancellationTokenSource _dataCts;
    private Task _dataTask;
    private readonly Dictionary<string, string> _features = new();
    private readonly AutoResetEvent _singleCallEvent = new(true);
    private readonly ManualResetEvent _serviceCallEvent = new(true);
    private volatile string _taskId;
    private readonly object _objLock = new();

    #endregion

    #region ReceiverBase

    /// <summary>
    ///     设备初始化
    /// </summary>
    /// <param name="mi">模块对象</param>
    /// <returns></returns>
    public override bool Initialized(ModuleInfo mi)
    {
        ClearMsgQueues();
        _features.Clear();
        // 注：以下操作必须按顺序执行
        var result = base.Initialized(mi);
        if (result)
        {
            // 初始化数据套接字接口，以下操作需要依赖初始化后的套接字
            InitSocket();
            InitTasks();
            // 初始化天线
            InitAntenna();
            // 初始化设备功能/参数
            InitFeatures();
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        switch (CurFeature)
        {
            case FeatureType.FFM:
            case FeatureType.ITUM: // 单频测量
                _devTaskPref = Constants.RxSingle;
                if (IqSwitch) _devTaskPref = Constants.RxCiq;
                break;
            case FeatureType.FFDF: // 单频测向
                _devTaskPref = Constants.DfNarrow;
                break;
            case FeatureType.WBDF: // 宽带测向
                _devTaskPref = Constants.DfWideband;
                // 宽带测向必须返回频谱
                SpectrumSwitch = true;
                break;
            case FeatureType.SCAN: // 频段扫描（FSCAN/PSCAN）
                _devTaskPref = ScanMode.Equals(ScanMode.Pscan) ? Constants.RxPscan : Constants.RxFscan;
                break;
            case FeatureType.MScan: // (驻留)离散扫描
            case FeatureType.MScne:
                _devTaskPref = Constants.RxMscan;
                if (CurFeature == FeatureType.MScan)
                {
                    // 离散扫描和驻留离散扫描的区别在于驻留时间和等待时间为零
                    DwellTime = 0;
                    Holdtime = 0;
                    LevelThreshold = -100;
                    // 离散不需要单个频率的频谱数据
                    SpectrumSwitch = false;
                    AudioSwitch = false;
                }

                // （驻留）离散扫描不需要设置ITU测量
                ItuSwitch = false;
                break;
            default:
                throw new Exception("不支持的功能模块");
        }

        // 保证数据接收线程一直处于运行状态
        AbortCmd();
        // 启动数据接收线程，并执行启动任务指令
        // 下发启动指令，准备接收数据
        StartCmd();
    }

    /// <summary>
    ///     停止当前任务。
    ///     注：任务停止，并不代表当前实例被销毁，具体的操作由应用服务框架维护，
    ///     因此当前实例可能被其它类型的任务复用，应该重置可能影响结果的参数
    /// </summary>
    public override void Stop()
    {
        // 下发终止任务指令
        AbortCmd();
        // 重置下次任务执行时可能引发的异常的参数
        // _curFeature = SpecificAbility.None;
        _devTaskPref = string.Empty;
        _audioSwitch = false;
        _pdloc = 0;
        base.Stop();
    }

    /// <summary>
    ///     实现运行时修改参数
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="value">参数值</param>
    /// <returns>成功返回true, 否则返回false</returns>
    public override void SetParameter(string name, object value)
    {
        // 基类方法已经完成大部分工作，即完成对参数的修改
        base.SetParameter(name, value);
        if (name is ParameterNames.Xdb or ParameterNames.BetaValue &&
            TaskState == TaskState.Start) //xdb和beta不能在任务运行中修改参数
        {
            AbortCmd();
            StartCmd();
            return;
        }

        if (name == ParameterNames.IqSwitch)
        {
            AbortCmd();
            if (IqSwitch)
                _devTaskPref = Constants.RxCiq;
            else
                _devTaskPref = Constants.RxSingle;
            StartCmd();
            return;
        }

        /* 没法保证此方法一定在任务运行时才调用（比如频段扫描是在任务执行之前调用），
         * 因此需要判断当前是否有任务在执行，否则不能对参数进行修改
         * 此判断操作为容错处理，实际对设备下发指令时，一定已经对任务编号进行了判断
         */
        if (!string.IsNullOrWhiteSpace(_taskId))
            // 单频测量，单频测向，宽带测向在任务运行时可以对任务进行修改
            switch (CurFeature)
            {
                case FeatureType.FFM:
                case FeatureType.ITUM:
                case FeatureType.FFDF:
                case FeatureType.WBDF:
                    // 修改参数的方法：设置所有参数为最新值
                    AlterCmd();
                    break;
            }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _serviceCallEvent?.Dispose();
        _singleCallEvent?.Dispose();
        Utils.CancelTask(_dataTask, _dataCts);
        Utils.CancelTask(_audioTask, _audioCts);
        Utils.CancelTask(_dataProcessTask, _dataProcessCts);
        Utils.CancelTask(_cmdProcessTask, _cmdProcessCts);
        Utils.CancelTask(_dataReceiveTask, _dataReceiveCts);
        Utils.CancelTask(_cmdReceiveTask, _cmdReceiveCts);
        Utils.CloseSocket(_cmdSocket);
        Utils.CloseSocket(_dataSocket);
        Utils.CloseSocket(_audioSocket);
    }

    #endregion
}