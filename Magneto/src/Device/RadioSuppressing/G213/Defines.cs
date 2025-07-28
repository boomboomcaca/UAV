using System;
using System.Collections.Generic;
using Magneto.Contract;

namespace Magneto.Device.G213;

/// <summary>
///     命令字
/// </summary>
internal enum CommandType : ushort
{
    /// <summary>
    ///     下发干扰信号参数
    /// </summary>
    SendParameter = 0x71,

    /// <summary>
    ///     功放控制
    /// </summary>
    PowerControl = 0x72,

    /// <summary>
    ///     执行固化
    /// </summary>
    PerformCuring = 0x76,

    /// <summary>
    ///     状态查询和回报
    /// </summary>
    QueryStatus = 0x78
}

/// <summary>
///     频段范围定义
/// </summary>
[Flags]
internal enum FrequencyBand : ushort
{
    AllClose = 0x00,
    F870F960 = 0x01, //870~960?
    F1805F1880 = 0x02,
    F1880F2025 = 0x04,
    F2110F2170 = 0x08,
    F2300F2390 = 0x10,
    F2515F2675 = 0x20,
    F3400F3600 = 0x40,
    F4800F4900 = 0x80
}

/// <summary>
///     干扰信号类型
/// </summary>
internal enum SignalType : short
{
    /// <summary>
    ///     梳状谱干扰
    /// </summary>
    CombSpectrum = 0,

    /// <summary>
    ///     点频干扰（单音）
    /// </summary>
    DotFrequency,

    /// <summary>
    ///     窄带噪声调频干扰
    /// </summary>
    NarrowBandNoiseModulation,

    /// <summary>
    ///     宽带噪声干扰
    /// </summary>
    BroadbandNoise,

    /// <summary>
    ///     多音干扰
    /// </summary>
    Multitone,

    /// <summary>
    ///     线性调频
    /// </summary>
    LineFrequencyModulation,

    /// <summary>
    ///     协议压制
    /// </summary>
    Protocol,

    /// <summary>
    ///     窄带跳频
    /// </summary>
    NarrowbandFrequencyHopping
}

/// <summary>
///     公众移动通信频率信息
/// </summary>
internal enum FrequencyInfo
{
    CmccGsm930,
    CmccGsm1805,
    CmccTdScdma2010,
    CmccLteFdd937,
    CmccTdLte1880,
    CmccTdLte2320,
    Cmcc5G2515,
    Cmcc5G4800,
    CuGsm1840,
    CuWcdma2130,
    CuLteFdd949,
    CuLteFdd1850,
    CuTdLte2300,
    Cu5G3500,
    CtCdmaFdd870,
    CtCdma2110,
    CtLteFdd871,
    CtLteFdd1860,
    CtTdLte2370,
    Ct5G3400
}

internal class ChannelBand
{
    public string Name { get; set; }
    public double StartFrequency { get; set; }
    public double StopFrequency { get; set; }
}

internal class RftxBandsTemplateEx
{
    public int PhysicalChannelNumber { get; set; }
    public double StartFrequency { get; set; }
    public double StopFrequency { get; set; }

    /// <summary>
    ///     频率间隔，用于前端读取
    /// </summary>
    public double StepFrequency { get; set; } = 100d;

    public FrequencyBand FrequencyBand { get; set; }
    public ChannelBand[] ChannelBands { get; set; }
    public int Index { get; set; }

    public static RftxBandsTemplateEx ToTemplateEx(int index, RftxBandsTemplate template)
    {
        if (template == null) return null;
        var freqBand = FrequencyBand.AllClose;
        if (template.StartFrequency.CompareWith(870) >= 0 && template.StopFrequency.CompareWith(960) <= 0)
            freqBand = FrequencyBand.F870F960;
        else if (template.StartFrequency.CompareTo(1805) >= 0 && template.StopFrequency.CompareWith(1880) <= 0)
            freqBand = FrequencyBand.F1805F1880;
        else if (template.StartFrequency.CompareTo(1880) >= 0 && template.StopFrequency.CompareWith(2025) <= 0)
            freqBand = FrequencyBand.F1880F2025;
        else if (template.StartFrequency.CompareTo(2110) >= 0 && template.StopFrequency.CompareWith(2170) <= 0)
            freqBand = FrequencyBand.F2110F2170;
        else if (template.StartFrequency.CompareTo(2300) >= 0 && template.StopFrequency.CompareWith(2390) <= 0)
            freqBand = FrequencyBand.F2300F2390;
        else if (template.StartFrequency.CompareTo(2515) >= 0 && template.StopFrequency.CompareWith(2675) <= 0)
            freqBand = FrequencyBand.F2515F2675;
        else if (template.StartFrequency.CompareTo(3400) >= 0 && template.StopFrequency.CompareWith(3600) <= 0)
            freqBand = FrequencyBand.F3400F3600;
        else if (template.StartFrequency.CompareTo(4800) >= 0 && template.StopFrequency.CompareWith(4900) <= 0)
            freqBand = FrequencyBand.F4800F4900;
        var channelBandList = new List<ChannelBand>();
        if (freqBand != FrequencyBand.AllClose)
        {
            if (!string.IsNullOrWhiteSpace(template.ChannelSubBands))
            {
                var subBandArray = template.ChannelSubBands.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subBand in subBandArray)
                {
                    var fields = subBand.Split(new[] { ";" }, StringSplitOptions.None);
                    if (fields.Length < 3) continue;
                    if (double.TryParse(fields[1], out var start) && double.TryParse(fields[2], out var stop)
                                                                  && start.CompareWith(template.StartFrequency) >= 0 &&
                                                                  stop.CompareWith(template.StopFrequency) <= 0)
                        channelBandList.Add(new ChannelBand
                        {
                            Name = fields[0],
                            StartFrequency = start,
                            StopFrequency = stop
                        });
                }
            }

            if (channelBandList.Count == 0)
                channelBandList.Add(new ChannelBand
                {
                    Name = "全频段",
                    StartFrequency = template.StartFrequency,
                    StopFrequency = template.StopFrequency
                });
        }

        var templateEx = new RftxBandsTemplateEx
        {
            Index = index,
            PhysicalChannelNumber = template.PhysicalChannelNumber,
            StartFrequency = template.StartFrequency,
            StopFrequency = template.StopFrequency,
            StepFrequency = template.StepFrequency,
            FrequencyBand = freqBand,
            ChannelBands = channelBandList.ToArray()
        };
        return templateEx;
    }
}