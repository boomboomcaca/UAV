namespace Magneto.Device.DDF255;

/// <summary>
///     角度补偿信息
/// </summary>
internal class AngleCompensationInfo
{
    /// <summary>
    ///     起始频率
    /// </summary>
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率
    /// </summary>
    public double StopFrequency { get; set; }

    /// <summary>
    ///     补偿值
    /// </summary>
    public float Angle { get; set; }
}