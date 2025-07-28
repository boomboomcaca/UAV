using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     模拟电视返回信息
/// </summary>
public class AntvInfo
{
    /// <summary>
    ///     载噪比
    /// </summary>
    public double Cnr;

    /// <summary>
    ///     哼调
    /// </summary>
    public double Hum;

    /// <summary>
    ///     模拟电平值
    /// </summary>
    public double Level;

    /// <summary>
    ///     调制度
    /// </summary>
    public double Mod;
}

/// <summary>
///     待搜索的节目信息
/// </summary>
internal class SearchInfo
{
    public double Frequency { get; set; }
    public TvStandard Standard { get; set; }
}