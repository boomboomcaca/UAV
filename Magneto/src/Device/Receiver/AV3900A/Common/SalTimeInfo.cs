using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalTimeInfo
{
    /// <summary>
    ///     Integer part of the timestamp (in UTC seconds since January 1, 1970).
    /// </summary>
    public readonly uint TimestampSeconds;

    /// <summary>
    ///     Fractional part of the timestamp (in Nanoseconds).
    /// </summary>
    public readonly uint TimestampNSeconds;

    /// <summary>
    ///     If nonzero, the clock is not synced.
    /// </summary>
    public readonly uint IsNotSynced;
}