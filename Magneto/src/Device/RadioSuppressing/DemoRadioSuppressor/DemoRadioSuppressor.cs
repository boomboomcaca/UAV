using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoRadioSuppressor;

public partial class DemoRadioSuppressor : DeviceBase
{
    private readonly ConcurrentDictionary<int, bool> _channelSwitchMap = new();
    private volatile bool _enableSuppressGnss;

    /// <summary>
    ///     模拟的连接通道
    /// </summary>
    private Socket _socket;

    private Timer _timer;

    public DemoRadioSuppressor(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var b = base.Initialized(device);
        if (!b) return false;
        if (AsRealDevice)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var ip = IPAddress.Parse(IpAddress);
            _socket.Connect(ip, Port);
            _socket.NoDelay = true;
        }

        SetHeartBeat(_socket); //设置心跳包
        _timer = new Timer
        {
            Interval = 500
        };
        _timer.Elapsed += Timer_Elapsed;
        return true;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        if (feature != FeatureType.PCOMS && feature != FeatureType.FBANDS &&
            feature != FeatureType.SATELS && feature != FeatureType.UAVS)
            throw new InvalidOperationException("不支持的功能类型");
        var legal = CheckRftxBands(_rftxBands);
        if (!legal) throw new ArgumentException("管制频段配置错误");
        legal = CheckPowers(_powers, _rftxBands);
        if (!legal) throw new ArgumentException("功放功率配置错误");
        Trace.WriteLine($"功能类型：{feature}");
        Trace.WriteLine($"是否压制卫星导航GNSS：{_enableSuppressGnss}");
        Trace.WriteLine($"射频管控参数：{Utils.ConvertToJson(_rftxSegments)}");
        var segments = _rftxSegments == null
            ? new List<RftxSegmentsTemplate>()
            : new List<RftxSegmentsTemplate>(_rftxSegments);
        if (_enableSuppressGnss)
        {
            var band1 = Array.Find(_rftxBands, p => p.StartFrequency <= 1575.4d && p.StopFrequency >= 1575.4d);
            if (band1 != null)
                segments.Add(new RftxSegmentsTemplate
                {
                    Frequency = 1575.4d,
                    Bandwidth = 2000,
                    RftxFrequencyMode = 0,
                    Modulation = Modulation.Qpsk,
                    RftxSwitch = true,
                    PhysicalChannelNumber = band1.PhysicalChannelNumber,
                    LogicalChannelNumber = band1.LogicalChannelCount
                });
            var band2 = Array.Find(_rftxBands, p => p.StartFrequency <= 1561.1d && p.StopFrequency >= 1561.1d);
            if (band1 != null)
                segments.Add(new RftxSegmentsTemplate
                {
                    Frequency = 1561.1d,
                    Bandwidth = 2000,
                    RftxFrequencyMode = 0,
                    Modulation = Modulation.Qpsk,
                    RftxSwitch = true,
                    PhysicalChannelNumber = band2.PhysicalChannelNumber,
                    LogicalChannelNumber = band2.LogicalChannelCount
                });
        }

        legal = CheckRftxSegments(segments.ToArray(), _rftxBands);
        if (!legal) throw new ArgumentException("射频管控配置错误");
        Trace.WriteLine("参数检查结果：无问题");
        _channelSwitchMap.Clear();
        foreach (var rftxBand in _rftxBands)
        {
            var segment = segments.Find(p => p.PhysicalChannelNumber == rftxBand.PhysicalChannelNumber);
            var state = segment?.RftxSwitch == true;
            _channelSwitchMap.AddOrUpdate(rftxBand.PhysicalChannelNumber, state, (_, _) => state);
        }

        Trace.WriteLine($"通道开启状态：{Utils.ConvertToJson(_channelSwitchMap)}");
        _timer?.Start();
    }

    public override void Stop()
    {
        base.Stop();
        _timer?.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.EnableSuppressGnss)
        {
            _enableSuppressGnss = CurFeature == FeatureType.UAVS && value is true;
            return;
        }

        base.SetParameter(name, value);
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
        _timer?.Dispose();
        if (_socket != null)
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch
            {
                // 容错代码
            }
            finally
            {
                _socket?.Dispose();
            }

            _socket = null;
        }
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

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (_rftxBands?.Any() != true) return;
        var random = new Random();
        var faultIndex = random.Next(0, 200);
        var vswIndex = random.Next(0, 200);
        var overHeating = random.Next(0, 200);
        var list = new List<SDataRadioSuppressing>();
        for (var i = 0; i < _rftxBands.Length; i++)
        {
            var rftxBand = _rftxBands[i];
            _channelSwitchMap.TryGetValue(rftxBand.PhysicalChannelNumber, out var state);
            if (_powers != null)
                list.Add(new SDataRadioSuppressing
                {
                    ChannelNumber = rftxBand.PhysicalChannelNumber,
                    Power = faultIndex == i || state || _powers == null ? (float)Math.Pow(10, _powers[i] / 10 - 3) : -1,
                    OverHeating = overHeating == i && state,
                    Vsw = vswIndex == i && state,
                    Warning = faultIndex == i && state ? "功放故障" : string.Empty
                });
        }

        if (list.Any(p => p.OverHeating || p.Vsw || !string.IsNullOrWhiteSpace(p.Warning))) Trace.WriteLine("数据有功放告警");
        SendData(list.ConvertAll(p => (object)p));
    }
}

/// <summary>
///     信号源模式
/// </summary>
public enum SignalMode
{
    /// <summary>
    ///     单音
    /// </summary>
    SingleTones = 0,

    /// <summary>
    ///     多音
    /// </summary>
    MultiTones = 1,

    /// <summary>
    ///     扫频
    /// </summary>
    Scan = 2,

    /// <summary>
    ///     梳状谱
    /// </summary>
    Comb = 3
}