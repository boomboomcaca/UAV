using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalTimeTrigParms
{
    public SalTimeTrigType TimeTriggerType;
    public uint TriggerTimeSecs;
    public uint TriggerTimeNSecs;

    /// <summary>
    ///     以ms为单位的触发间隔时间
    /// </summary>
    public uint TriggerInterval;

    public uint TriggerCount;
}