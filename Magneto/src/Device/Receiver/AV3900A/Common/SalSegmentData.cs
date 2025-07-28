using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

/// <summary>
///     频谱测量返回数据
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SalSegmentData
{
    /// <summary>
    ///     salSweepParms中的自定义数据
    /// </summary>
    public UIntPtr UserWorkspace;

    /// <summary>
    ///     0-based index of this segment in the segmentTable
    /// </summary>
    public readonly uint SegmentIndex;

    /// <summary>
    ///     从0开始，每一段频谱结果递增1
    /// </summary>
    public readonly uint SequenceNumber;

    /// <summary>
    ///     starts at 0; incremented by 1 at the end of a sweep
    /// </summary>
    public readonly uint SweepIndex;

    /// <summary>
    ///     本次扫描第一个时间点的时间戳，秒数值
    /// </summary>
    public readonly uint TimestampSec;

    /// <summary>
    ///     本次扫描第一个时间点的时间戳，纳秒数值
    /// </summary>
    public readonly uint TimestampNSec;

    /// <summary>
    ///     时间戳的时间属性质量
    /// </summary>
    public readonly uint TimeQuality;

    /// <summary>
    ///     本段测量接收机的位置
    /// </summary>
    public SalLocation Location;

    /// <summary>
    ///     本次测量第一个点的频率
    /// </summary>
    public readonly double StartFrequency;

    /// <summary>
    ///     频域数据的频率间隔
    /// </summary>
    public readonly double FrequencyStep;

    public readonly int NumPoints;

    /// <summary>
    ///     非0表示本段测量接收机输入超载
    /// </summary>
    public readonly uint Overload;

    /// <summary>
    ///     返回的频谱数据类型
    /// </summary>
    public readonly int DataType;

    /// <summary>
    ///     非0表示此段是停止扫描之前的最后一段测量数据
    /// </summary>
    public readonly uint LastSegment;

    /// <summary>
    ///     本次测量的窗函数类型
    /// </summary>
    public readonly SalWindowType Window;

    /// <summary>
    ///     测量的平均类型
    /// </summary>
    public readonly SalAverageType AverageType;

    /// <summary>
    ///     平均数目
    /// </summary>
    public readonly uint NumAverages;

    /// <summary>
    ///     FFT结果持续时间
    /// </summary>
    public readonly double FFtDuration;

    /// <summary>
    ///     完成测量的持续时间
    /// </summary>
    public readonly double AverageDuration;

    /// <summary>
    ///     If true, the segment table from another request is controlling the measurement
    /// </summary>
    public readonly uint IsMonitor;

    /// <summary>
    ///     扫描中发生各种事件的掩码
    /// </summary>
    public readonly uint SweepFlags;

    /// <summary>
    ///     只是接收机的时间状态
    /// </summary>
    public readonly uint TimeAlarms;

    /// <summary>
    ///     本段测量的采样率
    /// </summary>
    public readonly double SimpleRate;
}