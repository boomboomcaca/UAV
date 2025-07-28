/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramPlayANATV.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/29
 *
 * 修    改:
 *
 * 备    注:	模拟电视播放功能实现类
 *
 *********************************************************************************************/

using System;
using System.Text;
using System.Threading.Tasks;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     模拟电视播放模块
/// </summary>
internal class ProgramPlayAnatv : ProgramPlay
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="client">设备客户端交互对象</param>
    public ProgramPlayAnatv(Client client)
        : base(client)
    {
        Standard = TvStandard.ANATV;
    }

    /// <summary>
    ///     解码
    /// </summary>
    /// <param name="freq">频率</param>
    /// <param name="band">带宽</param>
    /// <param name="standard"></param>
    public override void OpenDecoder(double freq, double band, TvStandard standard)
    {
        CloseDecoder();
        var s = string.Empty;
        var info = new AntvInfo();
        Buffer = S7000Protocol.SetSignalType.GetOrder(1);
        var rec = Client.Send(Buffer, true);
        //s = System.Text.Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.SetFreq.GetOrder((int)(freq * 1000000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.GetLevel.GetOrder();
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        info.Level = s.GetPowerValue();
        int noiseBw;
        if (Math.Abs(band - 4000) < 1e-9)
            noiseBw = 400;
        else if (Math.Abs(band - 7000) < 1e-9)
            noiseBw = 475;
        else
            noiseBw = 575;
        Buffer = S7000Protocol.SetCatvcnrTest.GetOrder(noiseBw);
        rec = Client.Send(Buffer, true, 1000);
        s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.GetCatvcnr.GetOrder();
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        double cnr;
        if (!s.Contains("="))
            double.TryParse(s.Replace("dB", ""), out cnr);
        else
            double.TryParse(s.Split('=')[0].Replace("dB", ""), out cnr);
        info.Cnr = cnr;
        Buffer = S7000Protocol.SetCatvhumTest.GetOrder(0);
        rec = Client.Send(Buffer, true, 1000);
        s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.GetCatvhum.GetOrder();
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        double.TryParse(s.Trim().Split(' ')[0], out var hum);
        info.Hum = hum;
        Buffer = S7000Protocol.SetCatvmodUlateTest.GetOrder();
        rec = Client.Send(Buffer, true, 1000);
        s = Encoding.UTF8.GetString(rec);
        Buffer = S7000Protocol.GetCatvmodUlate.GetOrder();
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        double.TryParse(s.Trim().Split(' ')[0], out var mod);
        info.Mod = mod;
        // 有应答，但需要等待一段时间
        var freqI = (int)(freq * 1000000);
        Buffer = S7000Protocol.SetAtvOpenDecoder.GetOrder(freqI, freqI + 6500000, 0);
        Client.Send(Buffer);
        // Thread.Sleep(1000);
        Task.Delay(1000).ConfigureAwait(false).GetAwaiter().GetResult();
        rec = Client.Receive(false);
        s = Encoding.UTF8.GetString(rec);
        IsStable = s.Contains(AntvStable);
    }

    public override void Reset()
    {
        // 预留，模拟电视暂不需要重置
    }
}