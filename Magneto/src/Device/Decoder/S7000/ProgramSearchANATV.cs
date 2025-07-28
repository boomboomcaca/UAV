/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramSearchANATV.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	模拟节目搜索模块功能实现类
 *
 *********************************************************************************************/

using System;
using System.Text;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     模拟节目搜索模块
/// </summary>
internal class ProgramSearchAnatv : ProgramSearch
{
    /// <summary>
    ///     噪声带宽
    ///     常用设置 575---5.75MHz对应8M带宽信号
    ///     常用设置 475---4.75MHz对应7M带宽信号
    ///     常用设置 400---4.00MHz对应6M带宽信号
    /// </summary>
    private int _noiseBw;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="client">与设备交互的对象</param>
    public ProgramSearchAnatv(Client client)
        : base(client)
    {
        Standard = TvStandard.ANATV;
    }

    /// <summary>
    ///     载噪比门限,用于判断模拟信号是否稳定(dB)
    /// </summary>
    public int Cnr { get; set; }

    /// <summary>
    ///     模拟电平门限,用于判断模拟信号是否稳定(dBuV)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    ///     模拟电视搜索
    /// </summary>
    /// <param name="freq">频率</param>
    /// <param name="band">带宽</param>
    /// <param name="standard">制式</param>
    public override void Search(double freq, double band, TvStandard standard)
    {
        CloseDecoder();
        Buffer = S7000Protocol.SetSignalType.GetOrder(1);
        var rec = Client.Send(Buffer, true);
        var s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.SetFreq.GetOrder((int)(freq * 1000000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.GetLevel.GetOrder();
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        if (!s.Contains("Hz"))
        {
            // 指令下达与实际获取的回复可能会不一样，所以如果不一样时，就再获取一次
            Buffer = S7000Protocol.GetLevel.GetOrder();
            rec = Client.Send(Buffer, true);
            s = Encoding.UTF8.GetString(rec);
        }

        var isStable = s.GetPowerValue() >= Level; //用于判断模拟信号是否稳定
        if (isStable)
        {
            if (Math.Abs(band - 4000) < 1e-9)
                _noiseBw = 400;
            else if (Math.Abs(band - 7000) < 1e-9)
                _noiseBw = 475;
            else
                _noiseBw = 575;
            Buffer = S7000Protocol.SetCatvcnrTest.GetOrder(_noiseBw);
            rec = Client.Send(Buffer, true, 1000);
            s = Encoding.UTF8.GetString(rec);
            Buffer = S7000Protocol.GetCatvcnr.GetOrder();
            rec = Client.Send(Buffer, true, 500);
            s = Encoding.UTF8.GetString(rec);
            var cnr = double.MinValue;
            if (!s.Contains('='))
                double.TryParse(s.Replace("dB", ""), out cnr);
            else
                double.TryParse(s.Split('=')[0].Replace("dB", ""), out cnr);
            isStable = cnr >= Cnr;
            Buffer = S7000Protocol.SetTsStop.GetOrder();
            Client.Send(Buffer);
        }

        AddAntvPrograms(isStable, freq);
    }
}