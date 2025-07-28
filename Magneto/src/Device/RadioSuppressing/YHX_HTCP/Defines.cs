using System;
using System.Collections.Generic;
using Magneto.Contract;

namespace Magneto.Device.YHX_HTCP;

[Serializable]
public enum Satp
{
    /// <summary>
    ///     无亚音
    /// </summary>
    E = 0,

    /// <summary>
    ///     模拟亚音
    /// </summary>
    A = 1,

    /// <summary>
    ///     数字亚音正序
    /// </summary>
    I = 2,

    /// <summary>
    ///     数字亚音反序
    /// </summary>
    N
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

    public ChannelBand[] ChannelBands { get; set; }
    public int Index { get; set; }
    public double ChannelMaxPower { get; set; }

    public static RftxBandsTemplateEx ToTemplateEx(int index, RftxBandsTemplate template)
    {
        if (template == null) return null;
        var channelBandList = new List<ChannelBand>();
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
        var templateEx = new RftxBandsTemplateEx
        {
            Index = index,
            PhysicalChannelNumber = template.PhysicalChannelNumber,
            StartFrequency = template.StartFrequency,
            StopFrequency = template.StopFrequency,
            StepFrequency = template.StepFrequency,
            ChannelMaxPower = template.ChannelMaxPower,
            ChannelBands = channelBandList.ToArray()
        };
        return templateEx;
    }
}