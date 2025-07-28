using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

/// <summary>
///     接收机的调谐参数
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SalTunerParams
{
    /// <summary>
    ///     Tuner center frequency in Hertz
    /// </summary>
    public double CenterFrequency;

    /// <summary>
    ///     Tuner sample rate in Hertz
    /// </summary>
    public double SampleRate;

    /// <summary>
    ///     Front end attenuation in dB (-10 to 30)
    /// </summary>
    public double Attenuation;

    /// <summary>
    ///     Mixer level in dB; range is -10 to 10 dB, 0 dB gives best compromise between SNR and distortion.
    /// </summary>
    public readonly double MixerLevel;

    /// <summary>
    ///     Front end input to use
    /// </summary>
    public SalAntennaType Antenna;

    /// <summary>
    ///     If non-zero, turn preamp on
    /// </summary>
    public int Preamp;

    /// <summary>
    ///     non-zero when the FPGA write process is filling capture memory
    /// </summary>
    public readonly int SdramWriting;
}