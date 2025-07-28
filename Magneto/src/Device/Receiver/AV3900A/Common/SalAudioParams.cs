using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public class SalAudioParams
{
    /// <summary>
    ///     音频采样率
    /// </summary>
    public double SampleRate;

    /// <summary>
    ///     抑制级别(SAL_MIN_SQUELCH到SAL_MAX_SQUELCH)，为0时，使用SAL_MIN_SQUELCH。
    /// </summary>
    public double SquelchLevel;

    /// <summary>
    ///     1为开，0为关闭
    /// </summary>
    public int SquelchState;
}