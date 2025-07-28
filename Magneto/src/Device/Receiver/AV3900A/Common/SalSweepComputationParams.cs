using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalSweepComputationParams
{
    /// <summary>
    ///     Start frequency for the sweep (Hz)
    /// </summary>
    public double StartFrequency;

    /// <summary>
    ///     Stop frequency for the sweep (Hz)
    /// </summary>
    public double StopFrequency;

    /// <summary>
    ///     Resolution band-width (Hz)
    /// </summary>
    public double Rbw;
}