/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramPlay.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	节目播放基类
 *
 *********************************************************************************************/

using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     节目播放基类
/// </summary>
internal abstract class ProgramPlay : ProgramBase
{
    /// <summary>
    ///     判断模拟电视信号稳定的标准
    /// </summary>
    protected readonly string AntvStable = "Set ATV decoder OK";

    /// <summary>
    ///     判断数字信号稳定的标准
    /// </summary>
    protected readonly string Stable = "start parse thread ok\n";

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="client">设备客户端交互对象</param>
    protected ProgramPlay(Client client)
    {
        Client = client;
    }

    /// <summary>
    ///     搜索的电视制式
    /// </summary>
    public TvStandard Standard { get; set; }

    /// <summary>
    ///     信号已经稳定,可以进行节目切换
    /// </summary>
    public bool IsStable { get; set; }

    /// <summary>
    ///     数字电视的节目号
    /// </summary>
    public int ProNum { get; set; }

    /// <summary>
    ///     电视解码
    /// </summary>
    /// <param name="freq">需要解码的频率</param>
    /// <param name="band">带宽</param>
    /// <param name="standard"></param>
    public abstract void OpenDecoder(double freq, double band, TvStandard standard);

    /// <summary>
    ///     重置复位
    /// </summary>
    public abstract void Reset();
}