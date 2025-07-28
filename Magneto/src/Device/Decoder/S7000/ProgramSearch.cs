/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\ProgramSearch.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	节目搜索基类
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

/// <summary>
///     节目搜索基类
/// </summary>
internal abstract class ProgramSearch : ProgramBase
{
    /// <summary>
    ///     判断数字信号稳定的标准
    /// </summary>
    protected readonly string Stable = "start parse thread ok\n";

    /// <summary>
    ///     判断模拟电视信号稳定的标准
    /// </summary>
    protected string AntvStable = "Set ATV decoder OK";

    /// <summary>
    ///     查找到一个稳定的频道
    /// </summary>
    public Action<double, TvStandard, List<ChannelProgramInfo>, string> SearchComplete;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="client">设备交互对象</param>
    protected ProgramSearch(Client client)
    {
        Client = client;
    }

    /// <summary>
    ///     搜索的电视制式
    /// </summary>
    public TvStandard Standard { get; protected set; }

    /// <summary>
    ///     开始搜索节目
    /// </summary>
    /// <param name="freq">频率</param>
    /// <param name="band">带宽</param>
    /// <param name="standard"></param>
    public abstract void Search(double freq, double band, TvStandard standard);

    /// <summary>
    ///     添加当前频率的节目列表
    /// </summary>
    /// <param name="stable"></param>
    /// <param name="freq"></param>
    protected void AddPrograms(bool stable, double freq)
    {
        if (!stable)
        {
            SearchComplete?.Invoke(freq, Standard, null, "");
            return;
        }

        // 经测试，这里如果不调用Thread.Sleep()的话，本来有节目的可能搜不到节目，应该与设备本身性能有关,
        // 而且这个时间可能对解析的结果有影响，比如解析不出节目名称，或者节目名称全都一样。
        // Thread.Sleep(8000);
        Task.Delay(8000).ConfigureAwait(false).GetAwaiter().GetResult();
        Buffer = S7000Protocol.GetTsProgList.GetOrder();
        var rec = Client.Send(Buffer, true);
        // string spro = System.Text.Encoding.Default.GetString(rec);
        var spro = Utils.GetGb2312String(rec);
        var pros = spro.GetSubElement("[No.]");
        if (pros == null || spro.Equals("No Decoder info!"))
        {
            SearchComplete?.Invoke(freq, Standard, null, "No Decoder info!");
            return;
        }

        var list = new List<ChannelProgramInfo>();
        foreach (var pro in pros)
        {
            var s7000Pro = new S7000Program();
            var ret = s7000Pro.GetData(pro);
            if (ret)
                // 数据有可能会问题，待数据正常才添加到节目列表
                list.Add(s7000Pro.ToProgram());
        }

        // 过滤掉加密节目
        list = list.FindAll(o => !o.Ca && !o.ProgramName.Contains("测试") && !string.IsNullOrWhiteSpace(o.ProgramName));
        list = DistinctSameTvName(list);
        if (list?.Count > 0)
        {
            var nl = new List<ChannelProgramInfo>();
            // var nl = list.Select(item =>
            //                {
            //                    item.Frequency = freq;
            //                    item.Standard = Standard.ToString();
            //                    item.ProgramNumber = $"{freq}{item.ProgramNumber}";
            //                    return item;
            //                });
            var index = 0;
            foreach (var item in list)
            {
                var info = item;
                info.Index = index;
                info.Frequency = freq;
                info.Standard = Standard.ToString();
                info.ProgramNumber = $"{freq}{item.ProgramNumber}";
                index++;
                nl.Add(info);
            }

            list = nl.ToList();
        }

        SearchComplete?.Invoke(freq, Standard, list, "");
        pros = null;
    }

    /// <summary>
    ///     去除名字相同的电视台
    /// </summary>
    /// <param name="sDataAvPrograms"></param>
    private List<ChannelProgramInfo> DistinctSameTvName(List<ChannelProgramInfo> sDataAvPrograms)
    {
        var serNames = sDataAvPrograms.Select(item => item.ProgramName).Distinct().ToList();
        var result = new List<ChannelProgramInfo>();
        foreach (var serName in serNames)
        {
            var program = sDataAvPrograms.Find(item => item.ProgramName == serName);
            result.Add(program);
        }

        return result;
    }

    /// <summary>
    ///     添加当前频率的模拟电视节目列表
    /// </summary>
    /// <param name="stable"></param>
    /// <param name="freq"></param>
    protected void AddAntvPrograms(bool stable, double freq)
    {
        if (!stable)
        {
            SearchComplete?.Invoke(freq, Standard, null, "");
            return;
        }

        var list = new List<ChannelProgramInfo>
        {
            new()
            {
                Frequency = freq,
                Standard = Standard.ToString(),
                ProgramNumber = $"{freq}0",
                Index = 0,
                Ca = false,
                ProgramName = "模拟电视",
                FlowType = "ANA TV"
            }
        };
        SearchComplete?.Invoke(freq, Standard, list, "");
    }
}