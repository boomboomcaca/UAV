using System;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.AV3900A;

public partial class Av3900A : DeviceBase
{
    private readonly object _locker = new();
    private bool _isMeasureRunning;

    public Av3900A(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo moduleInfo)
    {
        // 首先调用基类方法
        var result = base.Initialized(moduleInfo);
        if (result) result = InitResources();
        return result;
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        lock (_locker)
        {
            base.Start(feature, dataPort);
            if (_isMeasureRunning) throw new Exception($"{Utils.GetNameByDescription(CurFeature)}正在运行");
            StartTask(feature);
            _isMeasureRunning = true;
        }
    }

    public override void Stop()
    {
        lock (_locker)
        {
            _isMeasureRunning = false;
            StopTask();
            base.Stop();
        }
    }

    public override void Dispose()
    {
        lock (_locker)
        {
            ReleaseResources();
            base.Dispose();
        }
    }

    public override void SetParameter(string name, object value)
    {
        lock (_locker)
        {
            if (_isMeasureRunning) StopTask();
            base.SetParameter(name, value);
            if (_isMeasureRunning) StartTask(CurFeature);
        }
    }
}