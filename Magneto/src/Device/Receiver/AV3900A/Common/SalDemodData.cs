using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalDemodData
{
    /// <summary>
    ///     starts at 0; incremented by 1 for each data block
    /// </summary>
    public readonly ulong SequenceNumber;

    /// <summary>
    ///     Number of samples in this data block. A complex pair is considered 1 sample.
    /// </summary>
    public readonly uint NumSamples;

    /// <summary>
    ///     Mask of indicators for various conditions (see ::salSTATE_EVENT).
    /// </summary>
    public readonly uint StateEventIndicator;

    /// <summary>
    ///     Integer part of the timestamp (in UTC seconds since January 1, 1970).
    /// </summary>
    public readonly uint TimestampSeconds;

    /// <summary>
    ///     Fractional seconds part of timestamp
    /// </summary>
    public readonly uint TimestampNSeconds;

    /// <summary>
    ///     Location of sensor when data was acquired
    /// </summary>
    public SalLocation Location;

    /// <summary>
    ///     Antenna input active for this data block.
    /// </summary>
    public readonly int Antenna;

    /// <summary>
    ///     Attenuation in dB; negative values indicate gain.
    /// </summary>
    public readonly double Attenuation;

    /// <summary>
    ///     RF center frequency in Hertz for this data block.
    /// </summary>
    public readonly double CenterFrequency;

    /// <summary>
    ///     Sample rate in Hertz.
    /// </summary>
    public readonly double SampleRate;

    public readonly uint TimeAlarms;
}