using System;

namespace Magneto.Driver.ESE;

public class CompareSignalInfo : ICloneable
{
    /// <summary>
    ///     构造函数
    /// </summary>
    public CompareSignalInfo()
    {
        SignalName = string.Empty;
    }

    /// <summary>
    ///     频点
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    public double Bandwidth { get; set; }

    /// <summary>
    ///     第一次捕获时间
    /// </summary>
    public DateTime FirstCaptureTime { get; set; }

    /// <summary>
    ///     最后一次捕获时间
    /// </summary>
    public DateTime LastCaptureTime { get; set; }

    /// <summary>
    ///     最大电平值
    /// </summary>
    public double MaxLevel { get; set; }

    /// <summary>
    ///     均值电平
    /// </summary>
    public double AveLevel { get; set; }

    /// <summary>
    ///     新增信号是否正在发射
    ///     可疑信号是否依旧可疑
    /// </summary>
    public bool IsLunching { get; set; }

    /// <summary>
    ///     分为新信号、可疑信号、小信号
    /// </summary>
    public string CompareResult { get; set; }

    /// <summary>
    ///     监测库/台站库中此信号的名称，默认为null
    /// </summary>
    public string SignalName { get; set; }

    /// <summary>
    ///     是否是白名单信号
    /// </summary>
    public bool IsWhiteSignal { get; set; }

    /// <summary>
    ///     拷贝接口实现
    /// </summary>
    public object Clone()
    {
        return MemberwiseClone();
    }
}