using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB500;

public partial class Eb500 : DeviceBase
{
    #region 全局变量

    /// <summary>
    ///     发送命令套接字 TCP
    /// </summary>
    private Socket _cmdSocket;

    /// <summary>
    ///     接收数据套接字 UDP
    /// </summary>
    private Socket _dataSocket;

    /// <summary>
    ///     电平和ITU数据采集并处理线程
    /// </summary>
    private Task _tcpDataTask;

    private CancellationTokenSource _tcpDataTokenSource;

    /// <summary>
    ///     业务数据采集线程
    /// </summary>
    private Task _udpDataCaptureTask;

    private CancellationTokenSource _udpDataCaptureTokenSource;

    /// <summary>
    ///     业务数据处理线程(解析并发送)
    /// </summary>
    private Task _udpDataDispatchTask;

    private CancellationTokenSource _udpDataDispatchTokenSource;

    /// <summary>
    ///     装载从设备接收到的相关业务数据
    /// </summary>
    private ConcurrentQueue<byte[]> _udpDataQueue = new();

    /// <summary>
    ///     订阅的数据类型
    /// </summary>
    private MediaType _mediaType = MediaType.None;

    /// <summary>
    ///     保存离散扫描频点列表
    /// </summary>
    private readonly List<double> _scanFreqs = new();

    /// <summary>
    ///     用于处理频段搜索和离散搜索时，出现第一包无效数据的问题
    /// </summary>
    private bool _flag;

    /// <summary>
    ///     是否具有ITU选件，初始化设备时通过查询指令获得
    /// </summary>
    private bool _ituOption;

    /// <summary>
    ///     避免该方法被调用时频繁申请内存
    /// </summary>
    private readonly byte[] _tcpRecvBuffer = new byte[1024 * 1024];

    private readonly object _ctrlLocker = new();
    private uint _lastFreqHigh = uint.MaxValue;

    #endregion

    #region 框架实现

    public Eb500(Guid id)
        : base(id)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (result)
        {
            //释放非托管资源
            ReleaseResources();
            //初始化网络连接
            InitNetWork();
            //初始化设备
            InitDevice();
            //初始化相关线程
            InitTasks();
            //启动心跳检测
            SetHeartBeat(_cmdSocket);
        }

        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        StartTask();
    }

    public override void Stop()
    {
        StopTask();
        base.Stop();
    }

    public override void Dispose()
    {
        ReleaseResources();
        _udpDataQueue.Clear();
        base.Dispose();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (TaskState == TaskState.Start
            && ((CurFeature == FeatureType.MScan
                 && name.Equals(ParameterNames.MscanPoints))
                || (CurFeature == FeatureType.MScne
                    && (name.Equals(ParameterNames.MscanPoints)
                        || name.Equals(ParameterNames.DwellSwitch)
                        || name.Equals(ParameterNames.SquelchThreshold)
                        || name.Equals(ParameterNames.DwellTime)
                        || name.Equals(ParameterNames.HoldTime))
                )))
        {
            StopTask();
            Thread.Sleep(10);
            StartTask();
        }
    }

    #endregion
}