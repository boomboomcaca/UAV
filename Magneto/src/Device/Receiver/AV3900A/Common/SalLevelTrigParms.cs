using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalLevelTrigParms
{
    public readonly SalLevelTrigType LevelTriggerType;
    public readonly float TriggerLevel;
}