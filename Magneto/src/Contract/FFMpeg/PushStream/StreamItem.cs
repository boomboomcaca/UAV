namespace Magneto.Contract.FFMpeg.PushStream;

/// <summary>
///     文件信息
/// </summary>
public struct StreamItem
{
    /// <summary>
    ///     偏移量
    /// </summary>
    public int Offset;

    /// <summary>
    ///     通道号
    /// </summary>
    public ushort Channel;

    /// <summary>
    ///     设备号
    /// </summary>
    public string TerminalNum;

    /// <summary>
    ///     标记
    /// </summary>
    public string Tag;

    /// <summary>
    ///     文件完整路径
    /// </summary>
    public string FileName;
}