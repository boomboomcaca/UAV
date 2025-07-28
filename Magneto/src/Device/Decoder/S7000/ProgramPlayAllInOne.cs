/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramPlayAllInOne.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/07/02
 *
 * 修    改:
 *
 * 备    注:	集成DTMB、DVB-T、DVB-T2数字节目播放功能的实现类
 *
 *********************************************************************************************/

using System;
using System.Text;
using System.Threading.Tasks;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     数字节目播放（集成DTMB、DVB-T、DVB-T2）
/// </summary>
internal class ProgramPlayAllInOne : ProgramPlay
{
    /// <summary>
    ///     当前播放的频点
    /// </summary>
    private double _currentFreq = -999;

    public ProgramPlayAllInOne(Client client)
        : base(client)
    {
    }

    public override void OpenDecoder(double freq, double band, TvStandard standard)
    {
        if (Math.Abs(freq - _currentFreq) < 1e-9 && Standard == standard && IsStable)
        {
            // 加上Stable == true是因为有可能前面的节目播放失败，那么就要重新开启码流分析，再开启解码
            OpenDecoder();
            return;
        }

        CloseDecoder();
        _currentFreq = freq;
        Standard = standard;
        byte[] rec = null;
        // 设置信号类型
        Buffer = S7000Protocol.SetSignalType.GetOrder(GetSignalType(standard));
        rec = Client.Send(Buffer, true);
        var s = Encoding.UTF8.GetString(rec);
        if (string.IsNullOrEmpty(s))
        {
            IsStable = false;
            return;
        }

        // 设置频率
        Buffer = S7000Protocol.SetFreq.GetOrder((int)(_currentFreq * 1000000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        if (string.IsNullOrEmpty(s))
        {
            IsStable = false;
            return;
        }

        // 设置带宽
        Buffer = S7000Protocol.SetBw.GetOrder((int)(band * 1000));
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        if (string.IsNullOrEmpty(s))
        {
            IsStable = false;
            return;
        }

        // 启动测试，等待信号稳定
        if (!SignalTest(standard))
        {
            Console.WriteLine("测试异常，需要重启设备！");
            IsStable = false;
            return;
        }

        // 开始码流分析
        Buffer = S7000Protocol.SetTsStart.GetOrder(1, 1);
        rec = Client.Send(Buffer, true);
        s = Encoding.UTF8.GetString(rec);
        // 判断是否稳定解码
        IsStable = Stable.Equals(s);
        if (IsStable) OpenDecoder();
    }

    /// <summary>
    ///     重置：在进行搜索、或者关闭功能时需要重置，初始化。
    /// </summary>
    public override void Reset()
    {
        _currentFreq = -999;
    }

    /// <summary>
    ///     开启码流分析
    /// </summary>
    private void OpenDecoder()
    {
        // 开启解码之前需要停止解码
        Buffer = S7000Protocol.SetTsCloseDecoder.GetOrder();
        Client.Send(Buffer);
        // 等待一段时间，切换节目基本上不会 出现 "No program"
        // System.Threading.Thread.Sleep(500);
        Task.Delay(2000).ConfigureAwait(false).GetAwaiter().GetResult();
        // 开启解码
        Console.WriteLine("开始第一次播放尝试...");
        Buffer = S7000Protocol.SetTsOpenDecoder.GetOrder(ProNum);
        var rec = Client.Send(Buffer, true);
        var s = Encoding.UTF8.GetString(rec);
        IsStable = s.Equals("Set program OK");
        // 如果开启码流失败，则就是切换节目失败
        // // 如果开启失败则再试一次
        // Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();
        // // 开启解码
        // Console.WriteLine("开始第二次播放尝试...");
        // _buffer = S7000Protocol.SetTSOpenDecoder.GetOrder<int>(ProNum);
        // rec = _client.Send(_buffer, true);
        // s = Encoding.UTF8.GetString(rec);
        // if (s.Equals("Set program OK"))
        // {
        //     Stable = true;
        // }
        // else
        // {
        //     // 如果开启码流失败，则就是切换节目失败
        //     Stable = false;
        // }
    }
}