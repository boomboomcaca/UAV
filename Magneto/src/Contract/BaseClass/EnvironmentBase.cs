using System;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Contract.BaseClass;

public abstract class EnvironmentBase(Guid deviceId) : DeviceBase(deviceId)
{
    /// <summary>
    ///     每个环境监控设备对应一个边缘端
    /// </summary>
    protected string EdgeId;

    public virtual void Start(IDataPort dataPort, string edgeId)
    {
        EdgeId = edgeId;
        base.Start(FeatureType.None, dataPort);
    }
}