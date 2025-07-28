using System;
using System.Collections.Generic;
using Core.Define;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Environment;

public class EnvBusiness : IDataPort
{
    private readonly string _edgeId;
    private readonly Guid _moduleId;
    private EnvironmentBase _device;

    public EnvBusiness(Guid moduleId, string edgeId)
    {
        _moduleId = moduleId;
        _edgeId = edgeId;
        DataPort = new DataPort(_moduleId);
    }

    public DataPort DataPort { get; }
    public bool IsRunning { get; private set; }
    public Guid TaskId => Guid.Empty;

    public void OnData(List<object> data)
    {
        if (data == null) return;
        DataPort?.OnData(data);
    }

    public void OnMessage(SDataMessage message)
    {
    }

    internal void Start()
    {
        var list = new List<ModuleChain<IDevice>>();
        _device = ModuleManager.Instance.BuildDeviceChain(_moduleId, Guid.Empty, ref list, out _) as EnvironmentBase;
        if (_device == null)
            // throw new Exception(msg);
            return;
        _device.Start(this, _edgeId);
        IsRunning = true;
    }

    internal void Stop()
    {
        _device.Stop();
        IsRunning = false;
    }

    internal void SetParameter(Parameter parameter)
    {
        _device?.SetParameter(parameter.Name, parameter.Value);
    }

    internal void SetParameters(List<Parameter> parameters)
    {
        if (_device == null) return;
        parameters.ForEach(parameter => _device.SetParameter(parameter.Name, parameter.Value));
    }
}