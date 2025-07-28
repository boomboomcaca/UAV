using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalFlowControl
{
    /// <summary>
    ///     Pacing policy, 0 == disabled (no pacing), 1 == wait when full policy, 2 == flush when full policy
    /// </summary>
    public readonly int PacingPolicy;

    /// <summary>
    ///     Max backlogSeconds, 0 == disabled
    /// </summary>
    public readonly float MaxBacklogSeconds;

    /// <summary>
    ///     TX data rate threshold, 0 == disabled
    /// </summary>
    public readonly float MaxBytesPerSec;

    /// <summary>
    ///     Max bytes threshold, 0 == disabled
    /// </summary>
    public readonly int MaxBacklogBytes;

    /// <summary>
    ///     Max messages threshold, 0 == disabled
    /// </summary>
    public readonly int MaxBacklogMessages;
}