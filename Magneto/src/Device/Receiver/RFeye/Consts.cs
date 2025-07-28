namespace Magneto.Device.RFeye;

internal static class Consts
{
    /// <summary>
    ///     地球半径，km
    /// </summary>
    internal const double EarthAxis = 6378.137;

    /// <summary>
    ///     该函数主要用于获取当前扫描数据对应的扫描步进（只包含频段扫描中列举出来的参数表）
    /// </summary>
    internal static readonly double[] ArrStep =
        { 3.90625d, 9.765625d, 19.53125d, 39.0625d, 78.125d, 156.25d, 312.5d, 625d };

    internal static readonly int[] ArrResBw = { 7382, 18457, 36914, 73828, 147656, 295312, 590625, 1181250 };

    /// <summary>
    ///     该函数主要用于单频测量根据频谱带宽设置对应的分辨率带宽，以保证多任务切换时数据的准确性
    /// </summary>
    internal static readonly double[] ArrSpecSpan =
        { 20000d, 10000d, 5000d, 2000d, 1000d, 500d, 200d, 100d, 50d, 20d, 10d };

    internal static readonly int[] ArrResBwFixFq = { 36914, 18457, 7382, 3691, 1845, 738, 369, 184, 73, 36, 18 };
}