using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalSweepComputationResults
{
    /// <summary>
    ///     Computed desired FFT bin size (converted from rbw and window)
    /// </summary>
    public readonly double StepFreq;

    /// <summary>
    ///     Actual FFT bin size (some power of 2)
    /// </summary>
    public readonly double FftBinSize;

    /// <summary>
    ///     Actual RBW (related to fftBinSize by window type)
    /// </summary>
    public readonly double ActualRbw;

    /// <summary>
    ///     Actual tuner sample rate (Hz)
    /// </summary>
    public readonly double TunerSampleRate;

    /// <summary>
    ///     FFT size
    /// </summary>
    public readonly uint FftBlockSize;

    /// <summary>
    ///     Either 1.4 or 1.28 depending on tunerSampleRate
    /// </summary>
    public readonly double NyquistFactor;

    /// <summary>
    ///     Number of FFT bins returned in each segment
    /// </summary>
    public readonly uint NumBinsReturned;

    /// <summary>
    ///     Number of FFT bins returned in the last segment
    /// </summary>
    public readonly uint NumBinsReturnedLastSegment;

    /// <summary>
    ///     Index of first FFT bin returned
    /// </summary>
    public readonly uint FirstPointIdx;

    /// <summary>
    ///     Index of first FFT bin returned in the last segment
    /// </summary>
    public readonly uint FirstPointIdxLastSegment;

    /// <summary>
    ///     Number of FFT segments to cover the span
    /// </summary>
    public readonly uint NumSegments;

    /// <summary>
    ///     Center frequency of the first segment
    /// </summary>
    public readonly double CenterFrequencyFirstSegment;

    /// <summary>
    ///     Center frequency of the last segment
    /// </summary>
    public readonly double CenterFrequencyLastSegment;
}