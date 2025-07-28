using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Contract;

/// <summary>
///     设备描述类
/// </summary>
public class DeviceDescription
{
    public DeviceBase Device { get; set; }
    public ModuleInfo Description { get; set; }

    public bool IsParameterExists(string parameterName)
    {
        return Description.Parameters != null || Description.Parameters?.Find(i => i.Name == parameterName) != null;
    }
}