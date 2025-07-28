using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

/// <summary>
///     时域测量返回数据
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SalTimeData
{
    /// <summary>
    ///     序号，从0开始递增
    /// </summary>
    public readonly ulong SequenceNumber;

    /// <summary>
    ///     数据类型
    /// </summary>
    public readonly int DataType;

    /// <summary>
    ///     数据块的数据数目
    /// </summary>
    public readonly uint NumSamples;

    /// <summary>
    /// </summary>
    public readonly double ScaleToVolts;

    /// <summary>
    ///     事件变化掩码
    /// </summary>
    public readonly uint StateEventIndicator;

    /// <summary>
    ///     时间戳
    /// </summary>
    public readonly uint TimestampSeconds;

    /// <summary>
    ///     时间戳小数部分
    /// </summary>
    public readonly uint TimestampNSeconds;

    /// <summary>
    ///     数据取得是接收机位置
    /// </summary>
    public SalLocation Location;

    /// <summary>
    ///     本段数据段有效的天线输入
    /// </summary>
    public readonly SalAntennaType Antenna;

    public readonly int Unknown;

    /// <summary>
    ///     衰减
    /// </summary>
    public readonly double Attenuation;

    /// <summary>
    ///     中心频率
    /// </summary>
    public readonly double CenterFrequency;

    /// <summary>
    ///     采样率
    /// </summary>
    public readonly double SampleRate;

    /// <summary>
    ///     接收机同步时间指示
    /// </summary>
    public readonly uint TimeAlarms;
}