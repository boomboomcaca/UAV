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
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye;

public partial class RFeye : DeviceBase
{
    #region 构造函数

    public RFeye(Guid id)
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
    ///     控制频点的切换的线程
    /// </summary>
    private Task _freqsSwitchTask;

    private CancellationTokenSource _freqsSwitchTokenSource;

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
    ///     由于设备提供的静噪门限控制为一个百分比没有实际意义，
    ///     此处自行通过最新的电平值来实现是否返回音频数据
    /// </summary>
    private float _currLevel;

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

    private volatile bool _bReceivedSpec;
    private volatile bool _bReceivedAudio;

    /// <summary>
    ///     线程锁，用于同步启动、停止和数据处理过程
    /// </summary>
    private readonly object _dataProcessLock = new();

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
        lock (_dataProcessLock)
        {
            _dataQueue.Clear();
            _id++;
            _currLevel = 0;
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
                //TODO:目前只在单频测量和频段扫描中提供检波方式,其它功能暂不提供此参数,
                //为保证数据处理逻辑的统一性,其它功能将检波方式设置成默认值
                Detector = DetectMode.Fast;
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
            //TODO:
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
        ReleaseResources();
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
    }

    #endregion
}