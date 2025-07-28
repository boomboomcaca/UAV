using System;
using System.Collections.Generic;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SATELS;

public partial class Satels : ScanBase
{
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();

    /// <summary>
    ///     信号是否合并
    /// </summary>
    private readonly bool _isSignalsMerge = true;

    private readonly object _lockFirstTimeDic = new();
    private readonly List<int> _registeredSwitches = new();
    private volatile bool _enableRadioSuppressingSwitch;
    private OccupancyStructNew _occupancy;

    /// <summary>
    ///     手动-自动门限切换
    /// </summary>
    private bool _preAutoThresholdSign;

    private double[] _preThresholdValue = Array.Empty<double>();

    public Satels(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        RegisterSwitch();
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        ScheduleDeviceWork();
        return true;
    }

    public override bool Stop()
    {
        if (SwitchArray is ISwitchCallback callback) callback.Reset();
        StopSuppressing();
        StopMonitoring();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        SetMonitorParameter(name, value);
    }

    public override void OnData(List<object> data)
    {
        OnSuppressingData(ref data);
        OnMonitoringData(data);
    }

    public override void Dispose()
    {
        UnRegisterSwitch();
        base.Dispose();
    }

    protected override void StartDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Stop();
    }
}