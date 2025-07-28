using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.TX3010A.Protocol;

/// <summary>
///     子系统类
///     根据配置不同，可能有多个子系统
///     比如公众移动通信压制有三个子系统，总共9个通道
///     第一个子系统的通道号为1~3
///     第二个子系统通道号为4~6
///     第三个子系统通道号为7~9
/// </summary>
public sealed class Subsystem : IDisposable
{
    /// <summary>
    ///     存放每个通道是否可用
    ///     K: 物理通道号
    ///     V: 通道可用状态
    /// </summary>
    private readonly ConcurrentDictionary<int, bool> _channelEnableMap = new();

    /// <summary>
    ///     子系统的通道功率集合
    ///     K: 物理通道号
    ///     V: 通道功率
    /// </summary>
    private readonly ConcurrentDictionary<int, float> _channelPowerMap = new();

    /// <summary>
    ///     当前子系统包含的通道
    ///     K: 物理通道号
    ///     V: 设备通道号
    /// </summary>
    private readonly ConcurrentDictionary<int, int> _channels = new();

    /// <summary>
    ///     存放每个通道是否需要开启压制
    ///     K: 物理通道号
    ///     V: 通道开启状态
    /// </summary>
    private readonly ConcurrentDictionary<int, bool> _powerSwitchMap = new();

    /// <summary>
    ///     子系统的信号源集合
    ///     K: 物理通道号
    ///     V: 信号源
    /// </summary>
    private readonly ConcurrentDictionary<int, SignalSource> _signalSources = new();

    /// <summary>
    ///     子系统的控制板
    /// </summary>
    private ControlPanel _controlPanel;

    /// <summary>
    ///     控制板可用状态
    /// </summary>
    private bool _controlPanelAvailable;

    private CancellationTokenSource _cts;
    private bool _disposed;

    /// <summary>
    ///     是否有信号源可用
    /// </summary>
    private bool _signalSourcesAvailable;

    private Task _task;

    public Subsystem()
    {
        Enabled = _controlPanelAvailable && _signalSourcesAvailable;
    }

    /// <summary>
    ///     当前子系统序号
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    ///     当前子系统是否可用
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    ///     当前子系统包含的通道
    ///     K: 物理通道号
    ///     V: 设备通道号
    /// </summary>
    public Dictionary<int, int> Channels
    {
        get { return _channels.ToDictionary(p => p.Key, p => p.Value); }
    }

    /// <summary>
    ///     清理所有资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler<List<SDataRadioSuppressing>> DataReceived;
    public event EventHandler<SubsystemStateChangedEventArgs> StateChanged;

    ~Subsystem()
    {
        Dispose(false);
    }

    public void Init()
    {
        _cts = new CancellationTokenSource();
        _task = new Task(ReconnectUnit, _cts.Token);
        _task.Start();
    }

    /// <summary>
    ///     关联控制板
    /// </summary>
    /// <param name="subsystemIndex">子系统编号</param>
    /// <param name="ip">IP地址</param>
    /// <param name="port">端口号</param>
    public void AttachControlPanel(int subsystemIndex, string ip, int port)
    {
        _controlPanel?.Dispose();
        Index = subsystemIndex;
        _controlPanel = new ControlPanel(ip, port, subsystemIndex);
        _controlPanel.Disconnected += ControlPanel_Disconnected;
        _controlPanel.StatusChanged += ControlPanel_StatusReport;
        _controlPanelAvailable = _controlPanel.Initialized();
        Enabled = _controlPanelAvailable && _signalSourcesAvailable;
    }

    /// <summary>
    ///     关联信号源
    /// </summary>
    public void AttachSignalSource(int subsystemIndex, int channel, int deviceChannel, string ip, int port)
    {
        var signalSource = new SignalSource(ip, port, subsystemIndex, channel, deviceChannel);
        signalSource.Disconnected += SignalSource_Disconnected;
        _signalSources.AddOrUpdate(signalSource.PhysicalChannelNumber, signalSource, (_, v) =>
        {
            if (v != null)
            {
                v.Disconnected -= SignalSource_Disconnected;
                v.Dispose();
            }

            return signalSource;
        });
        _channels.AddOrUpdate(signalSource.PhysicalChannelNumber, signalSource.DeviceChannelNumber,
            (_, _) => signalSource.DeviceChannelNumber);
        // 对物理通道号进行排序
        var success = signalSource.Initialized();
        _channelEnableMap.AddOrUpdate(signalSource.PhysicalChannelNumber, success, (_, _) => success);
        _signalSourcesAvailable |= success;
        Enabled = _controlPanelAvailable && _signalSourcesAvailable;
    }

    public void UpdateChannelFrequencyRange(RftxBandsTemplate rftxBand)
    {
        if (rftxBand == null) return;
        var b = _signalSources.TryGetValue(rftxBand.PhysicalChannelNumber, out var signalSource);
        if (!b || signalSource == null) return;
        signalSource.UpdateFrequencyRange(rftxBand.StartFrequency, rftxBand.StopFrequency, rftxBand.MaxSignalCount);
    }

    /// <summary>
    ///     重置缓存的功放开关使能
    /// </summary>
    public bool ResetPower(bool retry = true)
    {
        if (_controlPanel?.Enabled != true) return false;
        _powerSwitchMap.Clear();
        foreach (var chn in _channels) _powerSwitchMap.AddOrUpdate(chn.Key, false, (_, _) => false);
        foreach (var kv in _signalSources)
        {
            var b = _channelEnableMap.TryGetValue(kv.Key, out var enabled);
            if (b && enabled)
                if (kv.Value?.Enabled == true)
                    kv.Value?.PowerOff();
        }

        return _controlPanel.EnabledPower(new Dictionary<int, bool> { { 1, false }, { 2, false }, { 3, false } },
            retry); //关闭使能
    }

    /// <summary>
    ///     设置压制参数
    /// </summary>
    public void SetSegmentGroup(FeatureType feature, int frequencyMode, int channelNumber,
        RftxSegmentsTemplate[] segments)
    {
        if (!_powerSwitchMap.ContainsKey(channelNumber) || segments?.Any() != true) return;
        var exist = _channelEnableMap.TryGetValue(channelNumber, out var enable);
        if (!exist || !enable) return;
        exist = _signalSources.TryGetValue(channelNumber, out var signalSource);
        if (!exist || signalSource?.Enabled != true) return;
        var state = segments.Any(p =>
            p.RftxFrequencyMode == frequencyMode && p.RftxSwitch && p.PhysicalChannelNumber == channelNumber);
        _powerSwitchMap.AddOrUpdate(channelNumber, state, (_, _) => state);
        if (!state) return;
        switch ((SignalMode)frequencyMode)
        {
            case SignalMode.SingleTones:
            {
                var segment = segments.FirstOrDefault(p =>
                    p != null && p.RftxFrequencyMode == frequencyMode && p.RftxSwitch &&
                    p.PhysicalChannelNumber == channelNumber);
                if (segment != null)
                    signalSource.SendSingleCmd(segment.Frequency, segment.Bandwidth, segment.Modulation);
            }
                break;
            case SignalMode.MultiTones:
            {
                var segment = segments.FirstOrDefault(p =>
                    p != null && p.RftxFrequencyMode == frequencyMode && p.RftxSwitch &&
                    p.PhysicalChannelNumber == channelNumber);
                if (segment != null)
                    signalSource.SendMultiCmd(segment.Frequencies, segment.Bandwidth, segment.Modulation);
            }
                break;
            case SignalMode.Scan:
            {
                var segment = segments.FirstOrDefault(p =>
                    p != null && p.RftxFrequencyMode == frequencyMode && p.RftxSwitch &&
                    p.PhysicalChannelNumber == channelNumber);
                if (segment != null)
                    signalSource.SendScanCmd(segment.StartFrequency, segment.StopFrequency, segment.StepFrequency,
                        segment.HoldTime);
            }
                break;
            case SignalMode.Comb:
            {
                var combParas =
                    new List<(double startFrequency, double stopFrequency, double stepFrequency, double frequency)>();
                foreach (var segment in segments)
                {
                    if (segment == null || segment.RftxFrequencyMode != frequencyMode ||
                        segment.PhysicalChannelNumber != channelNumber || !segment.RftxSwitch) continue;
                    if (feature == FeatureType.PCOMS)
                        combParas.Add((segment.StartFrequency, segment.StopFrequency, segment.StepFrequency,
                            segment.Frequency));
                    else
                        combParas.Add((segment.StartFrequency, segment.StopFrequency, segment.StepFrequency, 0d));
                }

                signalSource.SendCombCmd(combParas);
            }
                break;
        }
    }

    public void SetSegments(FeatureType feature, RftxSegmentsTemplate[] segments)
    {
        if (segments?.Any() != true) return;
        var segmentGroup = segments.Where(p => p.RftxSwitch)
            .GroupBy(p => new { p.PhysicalChannelNumber, p.RftxFrequencyMode });
        var tasks = new List<Task>();
        foreach (var group in segmentGroup)
        {
            var task = new Task(() => SetSegmentGroup(feature, group.Key.RftxFrequencyMode,
                group.Key.PhysicalChannelNumber, group.ToArray()));
            tasks.Add(task);
            task.Start();
        }

        Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     设置功放功率
    /// </summary>
    /// <param name="power">功放功率，单位为dBm</param>
    /// <param name="channel">物理通道号</param>
    public void SetPower(float power, int channel)
    {
        if (power is < 30 or > 50)
        {
            Trace.WriteLine($"通道{channel}功率超出范围（30-50dBm），当前功率：{power}dBm，修改为默认功率30dBm");
            power = 30;
        }

        _channelPowerMap.AddOrUpdate(channel, power, (_, _) => power);
        if (!_powerSwitchMap.ContainsKey(channel) || !_channelEnableMap.ContainsKey(channel)) return;
        var b = _channelEnableMap.TryGetValue(channel, out var enable);
        if (!b || !enable) return;
        b = _powerSwitchMap.TryGetValue(channel, out var state);
        if (!b || !state) return;
        var exist = _signalSources.TryGetValue(channel, out var signalSource);
        if (!exist || signalSource?.Enabled != true) return;
        signalSource.SetPower(power);
    }

    public void SetPowers((int channel, float power)[] powers)
    {
        if (powers?.Any() == null) return;
        var tasks = new List<Task>();
        foreach (var (channel, power) in powers)
        {
            var task = new Task(() => SetPower(power, channel));
            tasks.Add(task);
            task.Start();
        }

        Task.WhenAll(tasks).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     启动压制
    /// </summary>
    public bool StartSuppress()
    {
        if (_controlPanel?.Enabled != true) return false;
        Dictionary<int, bool> ps = new();
        var signalSources = new List<SignalSource>();
        foreach (var pair in _powerSwitchMap)
        {
            var b = _channels.TryGetValue(pair.Key, out var deviceChannel);
            if (!b) continue;
            var exist = _channelEnableMap.TryGetValue(pair.Key, out var enabled);
            if (!exist || !enabled)
                ps.Add(deviceChannel, false);
            else
                ps.Add(deviceChannel, pair.Value);
            exist = _signalSources.TryGetValue(pair.Key, out var signalSource);
            if (exist && pair.Value && signalSource != null) signalSources.Add(signalSource);
        }

        if (!_controlPanel.EnabledPower(ps)) return false;
        signalSources.ForEach(p => p.PowerOn());
        return true;
    }

    private void Dispose(bool disposing)
    {
        Enabled = false;
        if (_disposed) return;
        Utils.CancelTask(_task, _cts);
        _channels.Clear();
        _channelEnableMap.Clear();
        _powerSwitchMap.Clear();
        if (disposing)
        {
            if (_controlPanel != null)
            {
                _controlPanel.StatusChanged -= ControlPanel_StatusReport;
                _controlPanel.Disconnected -= ControlPanel_Disconnected;
                _controlPanel.Dispose();
            }

            foreach (var pair in _signalSources)
            {
                var signalSource = pair.Value;
                if (signalSource == null) continue;
                signalSource.Disconnected -= SignalSource_Disconnected;
                signalSource.Dispose();
            }
        }

        _signalSources.Clear();
        _disposed = true;
    }

    private void ControlPanel_StatusReport(object sender, List<SDataRadioSuppressing> e)
    {
        if (e?.Any() != true) return;
        var datas = new List<SDataRadioSuppressing>();
        foreach (var item in e)
        {
            if (item == null) continue;
            var channelNumber = _channels.FirstOrDefault(p => p.Value == item.ChannelNumber).Key;
            if (channelNumber == 0) continue;
            var temp = new SDataRadioSuppressing
            {
                ChannelNumber = channelNumber,
                Vsw = item.Vsw,
                OverHeating = item.OverHeating
            };
            var exist = _channelEnableMap.TryGetValue(channelNumber, out var enabled);
            if (!exist) continue;
            exist = _powerSwitchMap.TryGetValue(channelNumber, out var powerOn);
            if (!exist) continue;
            if (!enabled || !powerOn)
            {
                temp.Power = -1;
            }
            else
            {
                exist = _channelPowerMap.TryGetValue(channelNumber, out var power);
                if (Math.Abs(temp.Power - -1) > 1e-9 && exist)
                    //dBm转换为Watt
                    temp.Power = (float)Math.Pow(10, power / 10 - 3);
            }

            datas.Add(temp);
        }

        DataReceived?.Invoke(this, datas);
    }

    private void ControlPanel_Disconnected(object sender, DeviceDisconnectedEventArgs e)
    {
        _controlPanelAvailable = false;
        var old = Enabled;
        Enabled = false;
        if (old != Enabled) StateChanged?.Invoke(this, new SubsystemStateChangedEventArgs(Index, Enabled));
        if (!Enabled)
        {
            var datas = new List<SDataRadioSuppressing>();
            foreach (var channel in _channels)
                datas.Add(new SDataRadioSuppressing
                {
                    ChannelNumber = channel.Key,
                    Power = -1,
                    Warning = "通道不可用"
                });
            DataReceived?.Invoke(this, datas);
        }
    }

    private void SignalSource_Disconnected(object sender, DeviceDisconnectedEventArgs e)
    {
        if (!_channelEnableMap.ContainsKey(e.ChannelNumber)) return;
        _channelEnableMap.TryUpdate(e.ChannelNumber, false, true);
        DataReceived?.Invoke(this, new List<SDataRadioSuppressing>
        {
            new()
            {
                ChannelNumber = e.ChannelNumber,
                Power = -1,
                Warning = "通道不可用"
            }
        });
        var avaliable = false;
        foreach (var pair in _channelEnableMap) avaliable |= pair.Value;
        _signalSourcesAvailable = avaliable;
        var old = Enabled;
        Enabled = _controlPanelAvailable && _signalSourcesAvailable;
        if (old != Enabled) StateChanged?.Invoke(this, new SubsystemStateChangedEventArgs(Index, Enabled));
    }

    private void ReconnectUnit()
    {
        while (_cts?.IsCancellationRequested == false)
            try
            {
                //控制板不为空且已初始化且不可用 
                if (_controlPanel is { Enabled: false, IsInitialized: true } && !_controlPanelAvailable)
                {
                    _controlPanelAvailable = _controlPanel.Reinitialize();
                    Enabled = _controlPanelAvailable && _signalSourcesAvailable;
                }

                var channels = _channelEnableMap.Where(p => !p.Value).Select(p => p.Key).ToArray();
                if (channels.Any())
                    foreach (var channel in channels)
                    {
                        if (_cts?.IsCancellationRequested != false) break;
                        var exist = _signalSources.TryGetValue(channel, out var signalSource);
                        if (!exist || signalSource == null || signalSource.Enabled || !signalSource.IsInitialized)
                            continue;
                        var success = signalSource.Reinitialize();
                        _channelEnableMap.AddOrUpdate(signalSource.PhysicalChannelNumber, success, (_, _) => success);
                        _signalSourcesAvailable |= success;
                        Enabled = _controlPanelAvailable && _signalSourcesAvailable;
                    }

                if (_cts?.IsCancellationRequested != false) break;
                Task.Delay(3000, _cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"子系统单元重连异常，异常信息：{ex}");
            }
    }
}

public class SubsystemStateChangedEventArgs : EventArgs
{
    public SubsystemStateChangedEventArgs(int index, bool enabled)
    {
        Index = index;
        Enabled = enabled;
    }

    public int Index { get; }
    public bool Enabled { get; }
}