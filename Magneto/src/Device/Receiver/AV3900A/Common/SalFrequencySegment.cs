using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

/// <summary>
///     频谱扫描单段扫描参数
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SalFrequencySegment
{
    /// <summary>
    ///     天线输入设置
    /// </summary>
    public SalAntennaType Antenna;

    /// <summary>
    ///     前放输入设置(0表示关，非0打开)
    /// </summary>
    public int Preamp;

    /// <summary>
    ///     FFT点数，必须是2的乘方。
    /// </summary>
    public uint NumFftPoints;

    /// <summary>
    ///     平均类型
    /// </summary>
    public SalAverageType AverageType;

    /// <summary>
    ///     平均数量
    /// </summary>
    public uint NumAverages;

    /// <summary>
    ///     返回的第一个点的索引，必须小于numFftPoints
    /// </summary>
    public uint FirstPoint;

    /// <summary>
    ///     返回的点数，必须小于等于numFftPoints
    /// </summary>
    public uint NumPoints;

    /// <summary>
    ///     If true, repeat the measurement until duration has elapsed (not supported)
    /// </summary>
    public readonly uint RepeatAverage;

    /// <summary>
    ///     输入衰减，单位dB
    /// </summary>
    public double Attenuation;

    /// <summary>
    ///     射频中心频率
    /// </summary>
    public double CenterFrequency;

    /// <summary>
    ///     采样率
    /// </summary>
    public double SampleRate;

    /// <summary>
    ///     本段扫描开始到下段扫描开始的时间间隔，单位秒
    /// </summary>
    public readonly double Duration;

    /// <summary>
    ///     Mixer level in dB; range is -10 to 10 dB, 0 dB is give best compromise between SNR and distortion.
    /// </summary>
    public readonly double MixerLevel;

    /// <summary>
    ///     平均时开启交叠模式
    /// </summary>
    public SalOverlapType OverlapMode;

    /// <summary>
    ///     FFT数据类型
    /// </summary>
    public readonly SalFftDataType FftDataType;

    /// <summary>
    ///     设置为非0时表示FFT不进行调谐参数的改变，通常设置为0
    /// </summary>
    public int NoTunerChange;

    /// <summary>
    ///     In almost all cases, this parameter should be set to 0. Set this bitfield to non-zero value to limit data return
    ///     data for this segment
    /// </summary>
    public readonly uint NoDataTransfer;

    public readonly double Reserved3;
}