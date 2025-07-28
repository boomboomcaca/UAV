using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Driver.UAVS.Algorithm;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UAVS;

public partial class Uavs : ScanBase
{
    #region 构造函数

    public Uavs(Guid functionId) : base(functionId)
    {
        IsSupportMultiSegments = false;
    }

    #endregion

    #region 成员变量

    private readonly double _ePsilon = 1.0E-7;
    private readonly object _lockCachedScan = new();
    private double _startFrequency;
    private double _stopFrequency;
    private double _stepFrequency;
    private float[] _cachedScan;
    private readonly ConcurrentDictionary<string, SDataUavd> _uavCounter = new();
    private readonly ConcurrentQueue<List<float[]>> _spectraQueue = new();
    private List<float[]> _latestSpectraCollection = new();
    private CancellationTokenSource _uavCaptureCts;

    private Task _uavCaptureTask;

    //private readonly ManualResetEvent _startTaskEvent = new ManualResetEvent(false);
    private readonly List<DroneModel> _droneTemplates = new();
    private readonly object _droneTemplateLocker = new();
    private readonly List<int> _registeredSwitches = new();
    private volatile bool _enableRadioSuppressingSwitch;

    #endregion

    #region 任务相关

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        RegisterSwitch();
        InitMonitor();
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
        SetSuppressParameter(name, value);
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
        Utils.CancelTask(_uavCaptureTask, _uavCaptureCts);
        base.Dispose();
    }

    protected override void StartDevice()
    {
        (Receiver as DeviceBase)?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        (Receiver as DeviceBase)?.Stop();
    }

    #endregion
}