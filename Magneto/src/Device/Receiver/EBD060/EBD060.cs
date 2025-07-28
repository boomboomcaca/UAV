using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD060;

public partial class Ebd060 : DeviceBase
{
    #region 构造函数

    public Ebd060(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 全局变量

    /// <summary>
    ///     TCP Socket 用于发送命令接收业务数据
    /// </summary>
    private Socket _tcpSocket;

    /// <summary>
    ///     实际进行数据交换的TCP端口号
    /// </summary>
    private int _sencodPort;

    /// <summary>
    ///     COM地址
    /// </summary>
    private int _comPointer;

    /// <summary>
    ///     数据采集线程
    /// </summary>
    private Task _dataCaptureTask;

    /// <summary>
    ///     数据加工处理线程
    /// </summary>
    private Task _dataProcessTask;

    private CancellationTokenSource _dataCaptureTokenSource;
    private CancellationTokenSource _dataProcessTokenSource;

    /// <summary>
    ///     保存接收到的业务数据
    /// </summary>
    private MQueue<byte[]> _dataQueue = new();

    /// <summary>
    ///     缓存安装参数中低端天线与正北方向的夹角
    /// </summary>
    private float _lowCorrValue;

    /// <summary>
    ///     缓存安装参数中高端天线与正北方向的夹角
    /// </summary>
    private float _highCorrValue;

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            //释放非托管资源
            ReleaseResources();
            //解析高低端天线分别与正北方向的夹角
            ParseCorrValue();
            //连接及数据通道的初始化操作
            InitDataChannle();
            //预设
            Preset();
            //初始化收发数据线程
            InitAllThread();
            //启动心跳检测线程
            SetHeartBeat(_tcpSocket);
        }

        return result;
    }

    /// <summary>
    ///     开始任务
    /// </summary>
    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        SendCmd("ffm");
        //Thread.Sleep(100);
        SendCmd("data on");
        Thread.Sleep(100);
        SendCmd("start");
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public override void Stop()
    {
        SendCmd("data off");
        Thread.Sleep(100);
        SendCmd("stop");
        Thread.Sleep(100);
        //清除缓存数据
        _dataQueue.Clear();
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