using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD100;

public partial class Ebd100 : DeviceBase
{
    #region 构造函数

    public Ebd100(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 变量/属性

    // 串口通信
    private SerialPort _serialPort;

    // 标识是否在测试过程中
    private bool _isRunning;

    // 缓存从串口读取到的数据
    private string _recvData = "";

    // 上次从串口读取到数据的时间，心跳检测使用
    private DateTime _lastGetDataTime = DateTime.Now;

    // 当前订阅的数据类型
    private MediaType _media = MediaType.Dfind;

    // 缓存上次的信息
    private short _ddf;
    private short _quality;
    private short _level;

    private short _compass;

    // 缓存最后一次罗盘数据，主要用于与后续收到的数据进行比对
    private SDataCompass _data;

    /// <summary>
    ///     罗盘线程
    /// </summary>
    private Task _thdScanCompassTask;

    private CancellationTokenSource _thdScanCompassTokenSource;

    /// <summary>
    ///     心跳线程
    /// </summary>
    private Task _thdHeartBeatTask;

    private CancellationTokenSource _thdHeartBeatTokenSource;
    private readonly object _lockComport = new();

    #endregion 变量/属性

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        ReleaseResources();
        if (!base.Initialized(moduleInfo)) return false;
        if (!InitSerialPort()) return false;
        InitDevice();
        InitThread();
        return true;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (_isRunning) return;
        try
        {
            Thread.Sleep(100);
            InitDevice();
            Thread.Sleep(100);
            GotoRemote();
            Thread.Sleep(100);
            SetCompassAvers(5);
            Thread.Sleep(100);
            SetDataFormat(3);
            Thread.Sleep(100);
            SendCmd(Ebd100Command.StartDdfCommand);
            Thread.Sleep(100);
            SendCmd(Ebd100Command.StartDdfCommand);
            _isRunning = true;
        }
        catch
        {
            return;
        }

        _serialPort.DiscardInBuffer();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        try
        {
            if (_isRunning)
            {
                SendCmd(Ebd100Command.StopDdfCommand);
                _serialPort.DiscardInBuffer();
                Thread.Sleep(100);
                for (var i = 0; i < 5; i++)
                    if (_serialPort.BytesToRead > 0)
                    {
                        SendCmd(Ebd100Command.StopDdfCommand);
                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }

                GotoLocal();
                _isRunning = false;
            }
        }
        catch
        {
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