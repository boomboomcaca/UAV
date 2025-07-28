using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;
using Timer = System.Timers.Timer;

namespace Magneto.Device.YHX_HTCP;

public partial class YhxHtcp : DeviceBase
{
    /// <summary>
    ///     由于设备存在相应问题，添加命令锁
    /// </summary>
    private readonly object _lockCmd = new();
    ///// <summary>
    /////     将频率有客户端的MHz转换为Hz发送给设备(1000000)
    ///// </summary>
    //private const uint OneMega = 1000000;

    /// <summary>
    ///     定时休眠计时器
    /// </summary>
    private readonly Timer _timer;

    private CancellationTokenSource _checkCts;

    /// <summary>
    ///     设备连接检测线程
    /// </summary>
    private Task _checkTask;

    ///// <summary>
    /////     固定的8字节控制块
    ///// </summary>
    //private readonly byte[] _controlBlock = new byte[8];

    /// <summary>
    ///     设备自检各模块是否正常，目前处理方式是只要有一段不正常就返回设备初始化失败
    /// </summary>
    private bool _isOk = true;

    /// <summary>
    ///     与设备通信用的socket对象
    /// </summary>
    private Socket _socket;

    private DateTime _timeStart = DateTime.MinValue;

    public YhxHtcp(Guid deviceId) : base(deviceId)
    {
        _timer = new Timer
        {
            Interval = 1000
        };
        _timer.Elapsed += Timer_Elapsed;
    }

    public override bool Initialized(ModuleInfo device)
    {
        var flag = base.Initialized(device);
        ReleaseResources();
        InitNet();
        InitThreads();
        // Y8 公众移动通信频段管制设备较特殊 上电-》网络连接-》开机（发射）-》自检正常
        // 上电自检的时候射频模块还没有供电 因此自检会得到设备模块不正常 特殊处理网络连接就判断Y8初始化成功
        if (IsY8) return true;
        DeviceSelfCheck();
        DeviceInfoQuery();
        if (!_isOk)
        {
            PowerOff();
            Thread.Sleep(3000);
            PowerOn();
            Thread.Sleep(3000);
        }

        flag &= _isOk;
        return flag;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _timer.Stop();
        PowerOn();
        if (IsY8 && CurFeature == FeatureType.PCOMS) return;
        Thread.Sleep(3000);
        DeviceSelfCheck();
        if (_isOk) StartAbility();
    }

    public override void Stop()
    {
        base.Stop();
        if (IsY8)
        {
            PowerOff();
        }
        else
        {
            StopEmit();
            _timeStart = DateTime.Now;
            _timer.Start();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResources();
    }
}