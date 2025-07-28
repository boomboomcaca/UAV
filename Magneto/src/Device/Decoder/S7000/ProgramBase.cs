/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramBase.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	节目搜索/播放基类
 *
 *********************************************************************************************/

using System.Text;
using System.Threading.Tasks;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     节目搜索/播放基类
/// </summary>
public abstract class ProgramBase
{
    /// <summary>
    ///     指令缓存
    /// </summary>
    protected byte[] Buffer;

    /// <summary>
    ///     发送指令操作
    /// </summary>
    protected Client Client;

    /// <summary>
    ///     停止数字解码和音频解码
    /// </summary>
    protected void CloseDecoder()
    {
        // 停止数字解码
        Buffer = S7000Protocol.SetTsStop.GetOrder();
        Client.Send(Buffer, false, 100);
        // 停止模拟解码
        Buffer = S7000Protocol.SetAtvCloseDecoder.GetOrder();
        Client.Send(Buffer, false, 100);
    }

    /// <summary>
    ///     根据制式获取型号类型
    /// </summary>
    /// <param name="standard"></param>
    /// <returns>int信号类型</returns>
    protected int GetSignalType(TvStandard standard)
    {
        switch (standard)
        {
            case TvStandard.DTMB:
                return 5;
            case TvStandard.DVBT:
                return 3;
            case TvStandard.DVBT2:
                return 4;
        }

        return 5;
    }

    /// <summary>
    ///     信号测试
    /// </summary>
    /// <param name="standard"></param>
    protected bool SignalTest(TvStandard standard)
    {
        return standard switch
        {
            TvStandard.DTMB => DtmbTest(),
            TvStandard.DVBT => DvbtTest(),
            TvStandard.DVBT2 => Dvbt2Test(),
            _ => false
        };
    }

    /// <summary>
    ///     DTMB制式测试
    /// </summary>
    private bool DtmbTest()
    {
        try
        {
            Buffer = S7000Protocol.SetDtmbTest.GetOrder(1, 1);
            var rec = Client.Send(Buffer, true);
            Encoding.UTF8.GetString(rec);
            Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
            for (var i = 0; i < S7000Protocol.StableNum; i++)
            {
                Task.Delay(10).ConfigureAwait(false).GetAwaiter().GetResult();
                Buffer = S7000Protocol.GetDtmbResult.GetOrder();
                rec = Client.Send(Buffer, true);
                Encoding.UTF8.GetString(rec);
            }

            return true;
        }
        catch
        {
            // 信号测试中出现的异常不影响整体流程
            return false;
        }
    }

    /// <summary>
    ///     DVB-T制式测试
    /// </summary>
    private bool DvbtTest()
    {
        try
        {
            Buffer = S7000Protocol.SetDvbtTest.GetOrder(1, 1);
            var rec = Client.Send(Buffer, true);
            Encoding.UTF8.GetString(rec);
            for (var i = 0; i < S7000Protocol.StableNum; i++)
            {
                Buffer = S7000Protocol.GetDvbtResult.GetOrder();
                rec = Client.Send(Buffer, true);
                Encoding.UTF8.GetString(rec);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    ///     DVB-T2制式测试
    /// </summary>
    private bool Dvbt2Test()
    {
        try
        {
            Buffer = S7000Protocol.SetDvbt2Test.GetOrder(0, 1, 1);
            var rec = Client.Send(Buffer, true);
            Encoding.UTF8.GetString(rec);
            for (var i = 0; i < S7000Protocol.StableNum; i++)
            {
                Buffer = S7000Protocol.GetDvbt2Result.GetOrder();
                rec = Client.Send(Buffer, true);
                Encoding.UTF8.GetString(rec);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}