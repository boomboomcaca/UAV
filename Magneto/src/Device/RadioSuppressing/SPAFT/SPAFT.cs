using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.SPAFT;

public partial class Spaft : DeviceBase
{
    #region 构造函数

    public Spaft(Guid id) : base(id)
    {
        _channelPowerMap = new ConcurrentDictionary<int, SDataRadioSuppressing>();
        _channelStateMap = new ConcurrentDictionary<int, bool>();
        _lowDataQueue = new ConcurrentQueue<byte>();
        _highDataQueue = new ConcurrentQueue<byte>();
        _lowSyncData = new ConcurrentQueue<(byte cmdKey, byte[] content)>();
        _highSyncData = new ConcurrentQueue<(byte cmdKey, byte[] content)>();
    }

    #endregion

    #region 成员变量

    private const uint Head = 0xBEBEBEBE;
    private const uint Tail = 0xDEADDEAD;
    private const int LockMs = 400;

    /// <summary>
    ///     键为通道编号，值为元组，分别表示当前通道的CheckedMinFrequency, CheckedMaxFrequency, MinFrequency, MaxFrequency
    /// </summary>
    private Dictionary<int, Tuple<long, long, long, long>> _channelFrequencyEnds;

    /// <summary>
    ///     键为物理通道号 值为压制数据
    /// </summary>
    private readonly ConcurrentDictionary<int, SDataRadioSuppressing> _channelPowerMap;

    /// <summary>
    ///     键为物理通道号 值为通道状态
    /// </summary>
    private readonly ConcurrentDictionary<int, bool> _channelStateMap;

    private readonly object _audioLock = new();

    /// <summary>
    ///     低端字节数据缓存
    /// </summary>
    private readonly ConcurrentQueue<byte> _lowDataQueue;

    /// <summary>
    ///     高端字节数据缓存
    /// </summary>
    private readonly ConcurrentQueue<byte> _highDataQueue;

    /// <summary>
    ///     低端解析后字节流数据缓存
    /// </summary>
    private readonly ConcurrentQueue<(byte cmdKey, byte[] content)> _lowSyncData;

    /// <summary>
    ///     高端解析后字节流数据缓存
    /// </summary>
    private readonly ConcurrentQueue<(byte cmdKey, byte[] content)> _highSyncData;

    /// <summary>
    ///     低端设备TCP连接
    /// </summary>
    private Socket _lowSocket;

    /// <summary>
    ///     高端设备TCP连接
    /// </summary>
    private Socket _highSocket;

    private Task _lowReceiveTask;
    private CancellationTokenSource _lowReceiveCts;
    private Task _highReceiveTask;
    private CancellationTokenSource _highReceiveCts;
    private Task _lowParseTask;
    private CancellationTokenSource _lowParseCts;
    private Task _highParseTask;
    private CancellationTokenSource _highParseCts;
    private Task _sendAudioStreamTask;
    private CancellationTokenSource _sendAudioStreamCts;
    private Task _queryPowerInfoTask;
    private CancellationTokenSource _queryPowerInfoCts;
    private volatile bool _isLowSync;
    private volatile bool _isHighSync;

    /// <summary>
    ///     低端TCP通道同步对象
    /// </summary>
    private AutoResetEvent _lowResetEvent;

    /// <summary>
    ///     高端TCP通道同步对象
    /// </summary>
    private AutoResetEvent _highResetEvent;

    /// <summary>
    ///     警示语音发送同步对象
    /// </summary>
    private ManualResetEvent _audioSendingEvent;

    private string[] _audioFiles;
    private FileStream _audioStream;
    private bool _isInitialized;

    #endregion

    #region 重写父类

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        var result = base.Initialized(moduleInfo);
        if (!result) return false;
        try
        {
            InitMisc();
            InitNetworks();
            InitWorks();
            _isInitialized = true;
            return true;
        }
        catch (Exception ex)
        {
            Trace.Write($"{DeviceInfo.DisplayName}初始化异常，异常信息：{ex}");
            return false;
        }
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        _channelPowerMap.Clear();
        _channelStateMap.Clear();
        foreach (var kv in _indexChannelNumberMap)
        {
            var data = new SDataRadioSuppressing { ChannelNumber = kv.Value };
            _channelPowerMap.AddOrUpdate(kv.Value, data, (_, _) => data);
            _channelStateMap.AddOrUpdate(kv.Value, false, (_, _) => false);
        }

        if (!_isInitialized) throw new Exception($"{DeviceInfo.DisplayName}未初始化");
        _sendAudioStreamCts = new CancellationTokenSource();
        _sendAudioStreamTask = new Task(() => SendAudioStream(), _sendAudioStreamCts.Token);
        _sendAudioStreamTask.Start();
        var cmdBytes = ToStartFrame();
        SendCmdToHighRange(cmdBytes);
        SendCmdToLowRange(cmdBytes);
        var audioEnabled = _rftxSegments?.Any(p => p.RftxSwitch && p.ModulationSource == 1) == true;
        RaiseAudioSetting(audioEnabled, _audioIndex);
        if (_rftxSegments?.Any() == true && _powers?.Any() == true)
        {
            var validChannels = _rftxSegments.Where(p => p.RftxSwitch).Select(p => p.PhysicalChannelNumber).Distinct()
                .ToList();
            foreach (var channel in validChannels)
            {
                _channelStateMap.TryUpdate(channel, true, false);
                var b = _channelPowerMap.TryGetValue(channel, out var data);
                if (b && data != null && channel >= 0 && channel < _powers.Length)
                {
                    data.Power = _powers[channel];
                    data.OverHeating = false;
                    data.Vsw = false;
                    data.Warning = null;
                }
            }

            var datas = _channelPowerMap.ToDictionary(p => p.Key, p => p.Value).Values.ToList();
            _ = Task.Run(() => UpdatePowerMap(datas));
        }

        Trace.WriteLine($"{DeviceInfo.DisplayName}启动{feature}任务成功。");
    }

    public override void Stop()
    {
        base.Stop();
        RaiseAudioSetting(false, _audioIndex);
        Utils.CancelTask(_sendAudioStreamTask, _sendAudioStreamCts);
        RaiseDeviceQueryingOrSetting(ToStopFrame);
        var cmdBytes = ToStopFrame();
        SendCmdToLowRange(cmdBytes);
        SendCmdToHighRange(cmdBytes);
        ResetChannelSetting();
        foreach (var kv in _indexChannelNumberMap)
        {
            var channel = kv.Value;
            _channelPowerMap.AddOrUpdate(channel, new SDataRadioSuppressing { ChannelNumber = channel, Power = -1 },
                (_, v) =>
                {
                    v.Power = -1;
                    v.OverHeating = false;
                    v.Vsw = false;
                    v.Warning = null;
                    return v;
                });
            _channelStateMap.AddOrUpdate(channel, false, (_, _) => false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResources();
    }

    #endregion
}