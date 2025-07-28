using Magneto.Protocol.Define;

namespace Magneto.Device.CA300B;

/// <summary>
///     加密类型
/// </summary>
internal enum CaType
{
    UnEncryped,
    Encryped
}

/// <summary>
///     节目操作类型
/// </summary>
internal enum ProgramType
{
    /// <summary>
    ///     搜索
    /// </summary>
    Search,

    /// <summary>
    ///     播放
    /// </summary>
    Play
}

/// <summary>
///     参数下达指令
/// </summary>
internal class ProgramCmd
{
    /// <summary>
    ///     指令类容
    /// </summary>
    public object Cmd;

    /// <summary>
    ///     指令类型
    /// </summary>
    public ProgramType Type;
}

/// <summary>
///     待搜索的节目信息
/// </summary>
internal class SearchInfo
{
    public double Frequency { get; set; }
    public TvStandard Standard { get; set; }
}