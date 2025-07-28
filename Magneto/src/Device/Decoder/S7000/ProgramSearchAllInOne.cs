/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramSearchAllInOne.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/07/02
 *
 * 修    改:
 *
 * 备    注:	集成DTMB、DVB-T、DVB-T2节目搜索功能实现类
 *
 *********************************************************************************************/

using System.Text;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     数字节目搜索(含DTMB/DVB-T/DVB-T2)
/// </summary>
internal class ProgramSearchAllInOne : ProgramSearch
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="client">与设备交互的客户端对象</param>
    public ProgramSearchAllInOne(Client client)
        : base(client)
    {
    }

    /// <summary>
    ///     搜索数字节目
    /// </summary>
    /// <param name="freq">频率</param>
    /// <param name="band">带宽</param>
    /// <param name="standard">制式</param>
    public override void Search(double freq, double band, TvStandard standard)
    {
        Standard = standard;
        // 需要关闭码流分析：在播放的时候切换到节目搜索，第一次搜索有可能显示没有信号
        CloseDecoder();
        // 设置码流类型
        Buffer = S7000Protocol.SetSignalType.GetOrder(GetSignalType(standard));
        var rec = Client.Send(Buffer, true, 0, false);
        var s = Encoding.UTF8.GetString(rec);
        // 设置频率
        Buffer = S7000Protocol.SetFreq.GetOrder((int)(freq * 1000000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        // 带宽
        Buffer = S7000Protocol.SetBw.GetOrder((int)(band * 1000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        // 开启测试，等待信号稳定
        SignalTest(standard);
        // 判断是否稳定解码
        Buffer = S7000Protocol.SetTsStart.GetOrder(1, 1);
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        var stable = Stable.Equals(s);
        // 添加节目
        AddPrograms(stable, freq);
        // 关闭码流分析
        Buffer = S7000Protocol.SetTsStop.GetOrder();
        Client.Send(Buffer);
    }
}