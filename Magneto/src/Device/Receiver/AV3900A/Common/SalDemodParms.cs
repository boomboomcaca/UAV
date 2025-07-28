using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalDemodParms
{
    public double TunerCenterFrequency;
    public double TunerSampleRate;
    public double DemodCenterFrequency;
    public double DemodSampleRate;

    /// <summary>
    ///     Total number of samples to acquire. 0 means to capture until stopped.
    /// </summary>
    public ulong NumSamples;

    /// <summary>
    ///     Demodulation type
    /// </summary>
    public SalDemodulation Demodulation;
}