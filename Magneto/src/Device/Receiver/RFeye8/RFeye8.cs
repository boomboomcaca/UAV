using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye8;

public partial class RFeye8 : DeviceBase
{
    #region 构造函数

    public RFeye8(Guid id)
        : base(id)
    {
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     用于指令发送和数据接收
    /// </summary>
    private Socket _tcpSocket;

    /// <summary>
    ///     用于获取gps数据
    /// </summary>
    private Socket _gpsSocket;

    /// <summary>
    ///     外接天线控制器单元，20171207增加
    /// </summary>
    private Socket _switchSocket;

    /// <summary>
    ///     数据采集线程
    /// </summary>
    private Task _dataCaptureTask;

    private CancellationTokenSource _dataCaptureTokenSource;

    /// <summary>
    ///     数据处理线程
    /// </summary>
    private Task _dataProcessTask;

    private CancellationTokenSource _dataProcessTokenSource;

    /// <summary>
    ///     Gps数据采集线程
    /// </summary>
    private Task _gpsProcessTask;

    private CancellationTokenSource _gpsProcessTokenSource;

    /// <summary>
    ///     控制频点切换的线程
    /// </summary>
    private Task _freqsSwitchTask;

    private CancellationTokenSource _freqsSwitchTokenSource;

    /// <summary>
    ///     _switchSocket连接线程，主要用于实现重连，因为天线控制器的断线不体现该设备的连接状态，以避免网络断线恢复后只能重启服务端才能恢复
    /// </summary>
    private Task _extAntControllerConnectTask;

    private CancellationTokenSource _extAntControllerConnectTokenSource;

    /// <summary>
    ///     消息队列
    /// </summary>
    private readonly ConcurrentQueue<byte[]> _dataQueue = new();

    /// <summary>
    ///     订阅的数据类型
    /// </summary>
    private MediaType _media = MediaType.None;

    /// <summary>
    ///     ITU计算类
    /// </summary>
    private Itu _itu;

    /// <summary>
    ///     每次启动自增1已标识当前的测量状态
    /// </summary>
    private long _id;

    /// <summary>
    ///     频点列表（FSCNE,MSCAN,MSCNE），包含频率,中频带宽和解调模式
    /// </summary>
    private readonly List<ScanFreqInfo> _scanFreqs = new();

    /// <summary>
    ///     当前频点索引
    /// </summary>
    private int _scanFreqIndex;

    /// <summary>
    ///     是否停止切换的标志
    /// </summary>
    private volatile bool _stopSwitch;

    /// <summary>
    ///     可以切换到下一个频点的信号量
    /// </summary>
    private AutoResetEvent _switchEvent;

    /// <summary>
    ///     用于实现等待时间
    /// </summary>
    private readonly Stopwatch _watcherHoldTime = new();

    /// <summary>
    ///     用于实现驻留时间
    /// </summary>
    private readonly Stopwatch _watcherDwellTime = new();

    /// <summary>
    ///     标志是否已收到扫描、频谱、音频
    /// </summary>
    private volatile bool _bReceivedScan;

    /// <summary>
    ///     线程锁，用于同步启动、停止和数据处理过程
    /// </summary>
    private readonly object _dataProcessLock = new();

    /// <summary>
    ///     用于实现FixFQ/FSCNE/和MSCNE中的静噪门限
    /// </summary>
    private float _currLevel;

    /// <summary>
    ///     频谱缓存和电平缓存用于实现检波方式
    /// </summary>
    private int _realNumLoops;

    /// <summary>
    ///     频谱计算缓存
    /// </summary>
    private readonly double[] _arrSpecBuffer = new double[16384];

    private int _specCount;

    /// <summary>
    ///     电平计算缓存
    /// </summary>
    private float _levelSum;

    private int _levelCount;

    /// <summary>
    ///     音频计算缓存,音频达到一定数量后再发送
    /// </summary>
    private readonly short[] _arrAudioBuffer = new short[73728];

    private int _audioCount;

    /// <summary>
    ///     FM解调使用
    /// </summary>
    private double _i0;

    private double _q0;

    /// <summary>
    ///     音频滤波使用
    /// </summary>
    private const int F256Point = 51;

    private readonly double[] _arrPz = new double[F256Point];

    /// <summary>
    ///     _switchSocket互斥锁
    /// </summary>
    private readonly object _switchSocketLock = new();

    #endregion

    #region ReceiverBase

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        InitResources();
        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _dataQueue.Clear();
        lock (_dataProcessLock)
        {
            _id++;
            _currLevel = 0;
            ClearBuffer();
            switch (CurFeature)
            {
                case FeatureType.FFM:
                    _media |= MediaType.Level;
                    break;
                case FeatureType.SCAN:
                case FeatureType.FScne:
                case FeatureType.MScan:
                case FeatureType.MScne:
                    _media |= MediaType.Scan;
                    break;
                case FeatureType.TDOA:
                    _media |= MediaType.Tdoa;
                    break;
            }

            if ((CurFeature & (FeatureType.FFM | FeatureType.SCAN)) == 0)
            {
                //TODO:目前只在单频测量和频段扫描中实现检波方式,其它功能暂不提供此参数,
                //为保证数据处理逻辑的统一性,其它功能将检波方式相关的两个参数置为默认值
                Detector = DetectMode.Fast;
                _realNumLoops = 0;
            }

            if ((CurFeature & (FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)) > 0)
            {
                _stopSwitch = false;
                _freqsSwitchTokenSource = new CancellationTokenSource();
                _freqsSwitchTask = new Task(FreqsSwitchProc, _freqsSwitchTokenSource.Token);
                _freqsSwitchTask.Start();
            }
            else
            {
                SendMediaRequest();
            }
        }
    }

    public override void Stop()
    {
        lock (_dataProcessLock)
        {
            if (_freqsSwitchTask != null)
            {
                if (_switchEvent != null)
                {
                    _stopSwitch = true;
                    _switchEvent.Set();
                    Utils.CancelTask(_freqsSwitchTask, _freqsSwitchTokenSource);
                }

                _freqsSwitchTask = null;
            }

            var packet = new Packet();
            packet.BeginPacket(PacketType.DspLoop, -1);
            packet.EndPacket();
            SendPacket(packet, _tcpSocket);
            _media = MediaType.None;
            _dataQueue.Clear();
        }

        base.Stop();
    }

    public override void Dispose()
    {
        //清理设备自身相关资源
        ReleaseResources();
        //清理基类资源
        base.Dispose();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        //TODO:此处注意，以下两个参数是在创建任务构造驱动链时就会下发，此处需过滤掉此参数
        if (TaskState != TaskState.Start || name == ParameterNames.AntennaSelectionMode ||
            name == ParameterNames.Antennas) return;
        //先保存当前订阅的数据类型，因为Stop()中会重置为none
        var tempMedia = _media;
        Stop();
        _media = tempMedia;
        Start(CurFeature, DataPort);
    }

    protected override void KeepAlive(object connObject)
    {
        if (connObject is not Socket socket) return;
        //每隔1s发送设备的心跳包，发送空包无法检测到服务端关闭该连接的情况
        var alivePacket = new Packet();
        alivePacket.BeginPacket(PacketType.Link, -1);
        alivePacket.EndPacket();
        while (true)
            try
            {
                SendPacket(alivePacket, socket);
                Thread.Sleep(1000);
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }

        var info = new SDataMessage
        {
            LogType = LogType.Warning,
            ErrorCode = (int)InternalMessageType.DeviceRestart,
            Description = DeviceId.ToString(),
            Detail = DeviceInfo.DisplayName
        };
        SendMessage(info);
    }

    #endregion
}