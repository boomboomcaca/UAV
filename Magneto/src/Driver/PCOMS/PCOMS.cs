using System;
using System.Collections.Generic;
using System.Timers;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.PCOMS;

public partial class Pcoms : DriverBase
{
    private readonly List<SDataCellular> _datas;
    private readonly List<int> _registeredSwitches = new();
    private readonly object _syncLocker = new();
    private readonly Timer _timer;
    private volatile bool _enableRadioSuppressingSwitch;

    public Pcoms(Guid driverId) : base(driverId)
    {
        _datas = new List<SDataCellular>();
        _timer = new Timer
        {
            Interval = DecodeDataSendInterval
        };
        _timer.Elapsed += Timer_Elapsed;
        _timer.Enabled = true;
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
        base.SetParameter(name, value);
        SetMonitorParameter(name);
    }

    public override void OnData(List<object> data)
    {
        OnSuppressingData(ref data);
        OnMonitoringData(data);
    }

    public override void Dispose()
    {
        _timer.Dispose();
        UnRegisterSwitch();
        base.Dispose();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        lock (_syncLocker)
        {
            if (_datas.Count == 0) return;
            var list = _datas.ConvertAll(p => (object)p);
            SendData(list);
        }
    }
}