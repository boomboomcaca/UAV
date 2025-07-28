using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.TX3010A.Protocol;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.TX3010A;

/// <summary>
///     这个奇葩的设备包含最多三个子系统，每个子系统下包含一个控制板与多个信号源（每个信号源对应一个功放通道）
/// </summary>
public partial class Tx3010A : DeviceBase
{
    private readonly ConcurrentBag<int> _channelNumbers = new();
    private readonly ConcurrentDictionary<int, SDataRadioSuppressing> _dataCache = new();

    /// <summary>
    ///     存放所有的子系统
    ///     Key: 子系统编号
    ///     Value: 子系统
    /// </summary>
    private readonly ConcurrentDictionary<int, Subsystem> _subsystemMap = new();

    /// <summary>
    ///     存放所有的子系统可用状态
    ///     Key: 子系统编号
    ///     Value: 子系统可用状态
    /// </summary>
    private readonly ConcurrentDictionary<int, bool> _subsystemStateMap = new();

    private volatile bool _enableSuppressGnss;

    public Tx3010A(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        if (!base.Initialized(device)) return false;
        var success = CheckDeviceConfig(_deviceConfig);
        if (!success)
        {
            Trace.WriteLine("初始化失败，设备配置错误");
            return false;
        }

        success = InitSubsystems();
        if (!success)
        {
            Trace.WriteLine("初始化失败，子系统集合初始化失败");
            return false;
        }

        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.EnableSuppressGnss)
        {
            _enableSuppressGnss = value is true;
            return;
        }

        base.SetParameter(name, value);
        if (TaskState != TaskState.Start) return;
        if ((name == ParameterNames.EnableSuppressGnss && CurFeature == FeatureType.UAVS) ||
            name == ParameterNames.Powers || name == ParameterNames.RftxSegments)
        {
            Stop();
            Thread.Sleep(100);
            Start(CurFeature, DataPort);
        }
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (feature != FeatureType.PCOMS &&
            feature != FeatureType.SATELS && feature != FeatureType.UAVS)
            throw new InvalidOperationException("不支持的功能类型");
        var legal = CheckRftxBands(_rftxBands);
        if (!legal) throw new ArgumentException("管制频段配置错误");
        legal = CheckPowers(_powers, _rftxBands);
        if (!legal) throw new ArgumentException("功放功率配置错误");
        var segments = _rftxSegments == null
            ? new List<RftxSegmentsTemplate>()
            : new List<RftxSegmentsTemplate>(_rftxSegments);
        if (CurFeature == FeatureType.UAVS && _enableSuppressGnss)
        {
            var gpsFreqList = new List<(double freq, int channel)>();
            var band1 = Array.Find(_rftxBands, p => p.StartFrequency <= 1575.4d && p.StopFrequency >= 1575.4d);
            if (band1 != null) gpsFreqList.Add((1575.4d, band1.PhysicalChannelNumber));
            var band2 = Array.Find(_rftxBands, p => p.StartFrequency <= 1561.1d && p.StopFrequency >= 1561.1d);
            if (band2 != null) gpsFreqList.Add((1561.1d, band2.PhysicalChannelNumber));
            if (gpsFreqList.Count > 0)
            {
                var groups = from p in gpsFreqList group p by p.channel;
                foreach (var group in groups)
                {
                    var channel = group.Key;
                    var freqs = new List<double>();
                    foreach (var info in group) freqs.Add(info.freq);
                    if (freqs.Count == 0) continue;
                    //segments.Add(new RftxSegmentsTemplate()
                    //{
                    //    Frequencies = freqs.ToArray(),
                    //    Bandwidth = 2000,
                    //    RftxFrequencyMode = 3,
                    //    Modulation = Modulation.QPSK,
                    //    RftxSwitch = true,
                    //    PhysicalChannelNumber = channel,
                    //    LogicalChannelNumber = 1
                    //});
                    foreach (var freq in freqs)
                        segments.Add(new RftxSegmentsTemplate
                        {
                            StartFrequency = freq - 1d,
                            StopFrequency = freq + 1d,
                            StepFrequency = 15,
                            RftxSwitch = true,
                            RftxFrequencyMode = 3,
                            PhysicalChannelNumber = channel,
                            LogicalChannelNumber = 1
                        });
                }
            }
        }

        var rftxSegments = segments.ToArray();
        legal = CheckRftxSegments(rftxSegments, _rftxBands);
        if (!legal) throw new ArgumentException("射频管控配置错误");
        var channels = _channelNumbers.OrderBy(x => x).ToArray();
        // 开始压制了
        StartSuppressWithTasks(rftxSegments, channels, _powers);
        //_ = Task.Run(() => UpdatePowerMap(rftxSegments, _powers));
    }

    public override void Stop()
    {
        base.Stop();
        foreach (var pair in _subsystemMap) pair.Value?.ResetPower(false);
        foreach (var pair in _dataCache)
        {
            pair.Value.Power = -1;
            pair.Value.OverHeating = false;
            pair.Value.Vsw = false;
            pair.Value.Warning = null;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResources();
    }

    //private void UpdatePowerMap(RftxSegmentsTemplate[] segmentsTemplates, float[] powers)
    //{
    //    if (segmentsTemplates?.Any() != true)
    //    {
    //        return;
    //    }
    //    var validChannels = segmentsTemplates.Where(p => p.RftxSwitch).Select(p => p.PhysicalChannelNumber).Distinct().ToList();
    //    foreach (var channel in validChannels)
    //    {
    //        var b = _dataCache.TryGetValue(channel, out var data);
    //        if (b && data != null && channel > 0 && channel <= powers.Length)
    //        {
    //            data.Power = powers[channel-1];
    //            data.OverHeating = false;
    //            data.VSW = false;
    //            data.Warning = null;
    //        }
    //    }
    //    Thread.Sleep(200);
    //    SendData(_dataCache.Select(p => (object)p.Value).ToList());
    //}
    private bool InitSubsystems()
    {
        _channelNumbers.Clear();
        if (_deviceConfig == null || _deviceConfig.Length == 0)
        {
            Trace.WriteLine("未找到设备配置");
            return false;
        }

        var tasks = new List<Task>();
        foreach (var dev in _deviceConfig)
        {
            Subsystem subsystem;
            if (!_subsystemMap.ContainsKey(dev.Index))
            {
                subsystem = new Subsystem();
                subsystem.Init();
                subsystem.StateChanged += Subsystem_StateChanged;
                subsystem.DataReceived += Subsystem_DataReceived;
                _subsystemMap.AddOrUpdate(dev.Index, subsystem, (_, v) =>
                {
                    if (v != null)
                    {
                        v.DataReceived -= Subsystem_DataReceived;
                        v.StateChanged -= Subsystem_StateChanged;
                        v.Dispose();
                    }

                    return subsystem;
                });
            }
            else
            {
                subsystem = _subsystemMap[dev.Index];
            }

            switch (dev.DeviceType)
            {
                case 0:
                {
                    var task = Task.Run(() => { subsystem.AttachControlPanel(dev.Index, dev.IpAddress, dev.Port); });
                    tasks.Add(task);
                }
                    break;
                case 1:
                {
                    var task = Task.Run(() =>
                    {
                        subsystem.AttachSignalSource(dev.Index, dev.ChannelNumber, dev.DeviceChannelNumber,
                            dev.IpAddress, dev.Port);
                    });
                    tasks.Add(task);
                }
                    break;
            }
        }

        if (tasks.Count > 0) Task.WhenAll(tasks.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();
        var enabled = false;
        foreach (var pair in _subsystemMap)
        {
            _subsystemStateMap.AddOrUpdate(pair.Key, pair.Value.Enabled, (_, _) => pair.Value.Enabled);
            foreach (var channel in pair.Value.Channels.Keys)
            {
                _channelNumbers.Add(channel);
                _dataCache.TryAdd(channel, new SDataRadioSuppressing { ChannelNumber = channel, Power = -1 });
            }

            enabled |= pair.Value.Enabled;
        }

        if (!enabled)
        {
            Trace.WriteLine("没有可用的子系统");
            return false;
        }

        return true;
    }

    private void UpdateFrequencyRanges(RftxBandsTemplate[] rftxBands)
    {
        if (rftxBands?.Any() != true) return;
        foreach (var rftxBand in rftxBands)
        {
            var sub = GetSubsystemByChannel(rftxBand.PhysicalChannelNumber);
            sub?.UpdateChannelFrequencyRange(rftxBand);
        }
    }

    private void StartSuppressWithTasks(RftxSegmentsTemplate[] rftxSegments, int[] channels, float[] powers)
    {
        var tasks = new List<Task>();
        foreach (var pair in _subsystemMap)
        {
            var sub = pair.Value;
            if (sub?.Enabled != true || sub.Channels?.Any() != true || rftxSegments?.Any() != true) continue;
            var segments = Array.FindAll(rftxSegments,
                p => p != null && sub.Channels.ContainsKey(p.PhysicalChannelNumber));
            if (powers?.Any() != true) return;
            var powerList = new List<(int channel, float power)>();
            for (var i = 0; i < _powers.Length; i++)
            {
                if (i < 0 || i >= channels.Length) continue;
                var channel = channels[i];
                if (!sub.Channels.ContainsKey(channel)) continue;
                powerList.Add((channel, powers[i]));
            }

            var task = new Task(() => StartSuppress(CurFeature, sub, segments, powerList.ToArray()));
            tasks.Add(task);
            task.Start();
        }

        Task.WhenAll(tasks.ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private static void StartSuppress(FeatureType feature, Subsystem sub, RftxSegmentsTemplate[] segments,
        (int channel, float power)[] powers)
    {
        if (sub?.Enabled != true) return;
        // 1. 重置使能缓存
        sub.ResetPower();
        // 2. 设置压制参数（每个子系统单独设置）
        if (segments?.Any() != true) return;
        sub.SetSegments(feature, segments);
        // 3. 设置压制功率
        if (powers?.Any() != true) return;
        sub.SetPowers(powers);
        // 4. 开启压制
        sub.StartSuppress();
    }

    private void ReleaseResources()
    {
        var subList = _subsystemMap.Values.ToList();
        for (var i = subList.Count - 1; i >= 0; i--)
        {
            var sub = subList[i];
            if (sub == null) continue;
            sub.ResetPower(false);
            sub.DataReceived -= Subsystem_DataReceived;
            sub.StateChanged -= Subsystem_StateChanged;
            sub.Dispose();
        }

        _channelNumbers.Clear();
        _subsystemStateMap.Clear();
        _subsystemMap.Clear();
    }

    private void Subsystem_DataReceived(object sender, List<SDataRadioSuppressing> e)
    {
        if (e?.Any() != true) return;
        foreach (var data in e)
        {
            var b = _dataCache.TryGetValue(data.ChannelNumber, out var value);
            if (!b || value == null) continue;
            value.Power = data.Power;
            value.OverHeating = data.OverHeating;
            value.Vsw = data.Vsw;
            value.Warning = data.Warning;
        }

        SendData(_dataCache.Select(p => (object)p.Value).ToList());
    }

    private void Subsystem_StateChanged(object sender, SubsystemStateChangedEventArgs e)
    {
        var exist = _subsystemStateMap.TryGetValue(e.Index, out var state);
        if (!exist) return;
        _subsystemStateMap.TryUpdate(e.Index, e.Enabled, state);
        var enabled = false;
        foreach (var pair in _subsystemStateMap) enabled |= pair.Value;
        if (!enabled)
        {
            var info = new SDataMessage
            {
                LogType = LogType.Warning,
                ErrorCode = (int)InternalMessageType.DeviceRestart,
                Description = DeviceId.ToString(),
                Detail = DeviceInfo.DisplayName
            };
            SendMessage(info);
        }
    }

    /// <summary>
    ///     根据逻辑通道号获取子系统
    ///     比如公众移动通信压制有三个子系统总共9个通道，则：
    ///     第一个子系统的通道号为1~3
    ///     第二个子系统通道号为4~6
    ///     第三个子系统通道号为7~9
    /// </summary>
    /// <param name="channelNumber">逻辑通道号</param>
    private Subsystem GetSubsystemByChannel(int channelNumber)
    {
        foreach (var pair in _subsystemMap)
            if (pair.Value.Channels.ContainsKey(channelNumber))
                return pair.Value;
        return null;
    }

    private static bool CheckDeviceConfig(DeviceConfigTemplate[] deviceConfigs)
    {
        if (deviceConfigs?.Any() != true) return false;
        var isConnectionLegal = deviceConfigs.All(p => IsIpAddress(p.IpAddress) && p.Port is >= 1024 and <= 65535);
        if (!isConnectionLegal)
        {
            Trace.WriteLine("连接配置不合法");
            return false;
        }

        var groupsSignalSource = from p in deviceConfigs
            where p.DeviceType == 1
            group p by new { p.ChannelNumber, p.DeviceType };
        var signalSource = groupsSignalSource.ToList();
        if (signalSource.Any() != true)
        {
            Trace.WriteLine("无信号源");
            return false;
        }

        foreach (var group in signalSource)
            if (group.Count() > 1)
            {
                Trace.WriteLine($"信号源配置重复，物理通道号：{group.Key.ChannelNumber}");
                return false;
            }

        var groupsSignalSource2 = from p in deviceConfigs
            where p.DeviceType == 1
            group p by new { p.Index, p.DeviceChannelNumber, p.DeviceType };
        var signalSource2 = groupsSignalSource2.ToList();
        if (signalSource2.Any() != true)
        {
            Trace.WriteLine("无信号源");
            return false;
        }

        foreach (var group in signalSource2)
            if (group.Count() > 1)
            {
                Trace.WriteLine($"信号源配置重复，子系统编号：{group.Key.Index}，设备真实通道号：{group.Key.DeviceChannelNumber}");
                return false;
            }

        var groupsControlPanel =
            from p in deviceConfigs where p.DeviceType == 0 group p by new { p.Index, p.DeviceType };
        var controlPanel = groupsControlPanel.ToList();
        if (controlPanel.Any() != true)
        {
            Trace.WriteLine("无控制板");
            return false;
        }

        foreach (var group in controlPanel)
        {
            if (group.Count() > 1)
            {
                Trace.WriteLine($"控制板配置重复，子系统编号：{group.Key.Index}");
                return false;
            }

            var existSignalSource = deviceConfigs.Any(p => p.DeviceType == 1 && p.Index == group.Key.Index);
            if (!existSignalSource)
            {
                Trace.WriteLine($"子系统编号为{group.Key.Index}的控制板未配置信号源");
                return false;
            }
        }

        var groupsConnection = from p in deviceConfigs group p by new { p.IpAddress, p.Port };
        var connection = groupsConnection.ToList();
        if (connection.Any() != true)
        {
            Trace.WriteLine("未配置连接");
            return false;
        }

        foreach (var group in connection)
            if (group.Count() > 1)
            {
                Trace.WriteLine($"连接配置重复，IP地址：{group.Key.IpAddress}，端口号：{group.Key.Port}");
                return false;
            }

        return true;
    }

    private static bool CheckRftxBands(RftxBandsTemplate[] rftxBands)
    {
        return rftxBands?.Any() == true;
    }

    private static bool CheckRftxSegments(RftxSegmentsTemplate[] rftxSegments, RftxBandsTemplate[] rftxBands)
    {
        if (rftxBands?.Any() != true) return false;
        if (rftxSegments?.Any() != true)
        {
            Trace.WriteLine("射频管控参数为空");
            return false;
        }

        if (rftxSegments.All(p => !p.RftxSwitch))
        {
            Trace.WriteLine("射频管控参数射频开关全部未打开");
            return false;
        }

        foreach (var segment in rftxSegments)
        {
            if (!segment.RftxSwitch) continue;
            switch ((SignalMode)segment.RftxFrequencyMode)
            {
                case SignalMode.SingleTones:
                {
                    var legal = rftxBands.Any(p => p.PhysicalChannelNumber == segment.PhysicalChannelNumber &&
                                                   segment.Frequency.CompareWith(p.StartFrequency) >= 0 &&
                                                   segment.Frequency.CompareWith(p.StopFrequency) <= 0);
                    if (!legal)
                    {
                        Trace.WriteLine("射频管控参数中的单音参数不合法");
                        return false;
                    }
                }
                    break;
                case SignalMode.MultiTones:
                {
                    var legal = rftxBands.Any(p => p.PhysicalChannelNumber == segment.PhysicalChannelNumber &&
                                                   segment.Frequencies?.All(q => q.CompareTo(p.StartFrequency) >= 0) ==
                                                   true &&
                                                   segment.Frequencies?.All(q => q.CompareTo(p.StopFrequency) <= 0) ==
                                                   true);
                    if (!legal)
                    {
                        Trace.WriteLine("射频管控参数中的多音参数不合法");
                        return false;
                    }

                    var band = rftxBands.FirstOrDefault(p => p.PhysicalChannelNumber == segment.PhysicalChannelNumber);
                    if (band != null) segment.Frequency = (band.StartFrequency + band.StopFrequency) / 2;
                }
                    break;
                case SignalMode.Comb:
                case SignalMode.Scan:
                {
                    var legal = rftxBands.Any(p => p.PhysicalChannelNumber == segment.PhysicalChannelNumber &&
                                                   segment.StartFrequency.CompareWith(p.StartFrequency) >= 0 &&
                                                   segment.StopFrequency.CompareWith(p.StopFrequency) <= 0);
                    if (!legal)
                    {
                        Trace.WriteLine("射频管控参数中的扫频参数不合法");
                        return false;
                    }
                }
                    break;
            }
        }

        return true;
    }

    private static bool CheckPowers(float[] powers, RftxBandsTemplate[] rftxBands)
    {
        if (powers?.Any() != true)
        {
            Trace.WriteLine("功放功率参数为空");
            return false;
        }

        var channelCount = rftxBands.Select(p => p.PhysicalChannelNumber).Distinct().Count();
        if (powers.Length < channelCount)
        {
            Trace.WriteLine("功放功率数组长度小于功放数量");
            return false;
        }

        return powers.All(p => p is >= 30 and <= 50);
    }

    private static bool IsIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;
        var pattern =
            @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
        return Regex.IsMatch(ipAddress.Trim(), pattern);
    }
}