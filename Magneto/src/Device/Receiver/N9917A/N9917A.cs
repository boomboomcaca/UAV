using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9917A;

public partial class N9917A : DeviceBase
{
    #region 构造函数

    public N9917A(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     用于发送设备指令与数据的接收
    /// </summary>
    private Socket _cmdSock;

    /// <summary>
    ///     数据接收线程
    /// </summary>
    private Task _recvDataTask;

    /// <summary>
    /// </summary>
    private CancellationTokenSource _recvDataTokenSource;

    /// <summary>
    ///     功能运行状态
    /// </summary>
    private bool _isRunning;

    /// <summary>
    ///     数据接收锁
    /// </summary>
    private readonly object _lockData = new();

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        if (!base.Initialized(moduleInfo)) return false;
        //清理一下所有非托管资源
        ClearResource();
        //初始化用于通信套接字
        InitSocket();
        //初始化设备
        InitFeatures();
        SetHeartBeat(_cmdSock);
        return true;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _isRunning = true;
        if (CurFeature != FeatureType.None)
        {
            SendCmd("*CLS");
            //设置窗口自动调整
            SendCmd("DISP:WIND:TRAC1:Y:AUTO");
            _isRunning = true;
            //初始化线程
            InitTasks();
        }
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        base.Stop();
        try
        {
            Utils.CancelTask(_recvDataTask, _recvDataTokenSource);
        }
        catch
        {
        }
        finally
        {
            _isRunning = false;
        }
    }

    public override void Dispose()
    {
        ClearResource();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}