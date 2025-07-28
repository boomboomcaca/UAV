namespace Magneto.Driver.EMDC;

/// <summary>
///     合并后的台站信息
/// </summary>
public class StationSignal
{
    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public string StationName { get; set; }

    /// <summary>
    ///     台站经度
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     台站纬度
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     解调模式
    /// </summary>
    public string DemMode { get; set; }

    /// <summary>
    ///     台站类型
    /// </summary>
    public string StationType { get; set; }

    /// <summary>
    ///     技术体制
    /// </summary>
    public string TechnicalSystem { get; set; }
}