using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.EBD190.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD190;

public partial class Ebd190 : DeviceBase
{
    #region 构造函数

    public Ebd190(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 变量/属性

    private IClient _client;

    // 标识是否在测试过程中
    private bool _isRunning;

    // 缓存从串口读取到的数据
    private string _recvData = "";

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

    #endregion 变量/属性

    #region 重载基类

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        ReleaseResources();
        if (!base.Initialized(moduleInfo)) return false;
        // 实例化设备控制客户端
        _client = GetClient();
        if (_client == null)
        {
            Trace.WriteLine("初始化设备失败");
            return false;
        }

        //初始化
        _ = _client.Init(out _);
        _client.DataReceived -= _client_DataReceived;
        _client.DataReceived += _client_DataReceived;
        //初始化设备参数
        InitDevice();
        //初始化线程
        InitTasks();
        return true;
    }

    private void _client_DataReceived(object sender, string e)
    {
        ReceivedData(e);
    }

    /// <summary>
    ///     任务启动
    /// </summary>
    /// <returns></returns>
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
            SetCompassAvers(4);
            Thread.Sleep(100);
            SetDataFormat(3);
            Thread.Sleep(100);
            SetDdfMode(0); //门限测向
            Thread.Sleep(100);
            _client.SendCmd(Ebd190Command.StartDdfCommand);
            Thread.Sleep(100);
            _client.SendCmd(Ebd190Command.StartDdfCommand);
            _isRunning = true;
        }
        catch
        {
        }

        _client.DiscardInBuffer();
    }

    /// <summary>
    ///     任务停止
    /// </summary>
    /// <returns></returns>
    public override void Stop()
    {
        try
        {
            if (_isRunning)
            {
                _client.SendCmd(Ebd190Command.StopDdfCommand);
                _client.DiscardInBuffer();
                Thread.Sleep(100);
                for (var i = 0; i < 5; i++)
                    if (_client.BytesToRead > 0)
                    {
                        _client.SendCmd(Ebd190Command.StopDdfCommand);
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

    #endregion 重载基类
}