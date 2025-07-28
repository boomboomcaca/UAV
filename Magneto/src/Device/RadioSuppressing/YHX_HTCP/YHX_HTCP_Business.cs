using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract;

namespace Magneto.Device.YHX_HTCP;

public partial class YhxHtcp
{
    private static bool GetLegalSegments(RftxSegmentsTemplate[] rftxSegments, RftxBandsTemplateEx[] rftxBands,
        out List<RftxSegmentsTemplate> results, out int rftxFrequencyMode)
    {
        //段1：100MHz-500MHz【100-300；300-500】
        //段2：500MHz-1000MHz【500-750；750-1000】
        //段3：1000MHz-1700MHz【1000-1250；1250-1500；1500-1700】
        //其中方括号内为每个段的子段；每个段最多支持100个离散点的发射，总共支持最多300点
        //同步发射：每个段内只能设置4个频点最多12个频点，而且这四个频点必须在每个大段的同一子段内，否则不成功；
        //跳频发射：每个子频段最多设置32个频点，系统共可设置96个频点，频点间隔≥500kHz。
        //扫频发射：最多可以添加3段且必须在不同的主段的不同子段内
        results = new List<RftxSegmentsTemplate>();
        rftxFrequencyMode = -1;
        var groups = rftxSegments.Where(p => p?.RftxSwitch == true).GroupBy(p => p.RftxFrequencyMode).ToList();
        foreach (var group in groups)
        {
            var frequencyMode = group.Key;
            var legal = false;
            switch (frequencyMode)
            {
                case 0: //定频
                {
                    foreach (var seg in group)
                    {
                        var physicalChannelNumber = seg.PhysicalChannelNumber;
                        var band = Array.Find(rftxBands, p => p.PhysicalChannelNumber == physicalChannelNumber);
                        if (band == null) continue;
                        var b = seg.Frequency.CompareWith(band.StartFrequency) >= 0 &&
                                seg.Frequency.CompareWith(band.StopFrequency) <= 0 && results.All(p =>
                                    p.PhysicalChannelNumber != seg.PhysicalChannelNumber);
                        if (b)
                        {
                            results.Add(seg.Clone());
                            legal = true;
                        }
                    }
                }
                    break;
                case 1: //跳频
                {
                    var dicSegments = new Dictionary<int, RftxSegmentsTemplate>();
                    var dicBands = new Dictionary<int, RftxBandsTemplateEx>();
                    foreach (var seg in group)
                    {
                        var physicalChannelNumber = seg.PhysicalChannelNumber;
                        var band = Array.Find(rftxBands, p => p.PhysicalChannelNumber == physicalChannelNumber);
                        if (band == null) continue;
                        var b = seg.Frequencies?.All(p =>
                            p.CompareWith(band.StartFrequency) >= 0 && p.CompareWith(band.StopFrequency) <= 0) == true;
                        if (!b) continue;
                        if (dicSegments.ContainsKey(physicalChannelNumber))
                        {
                            var segment = dicSegments[physicalChannelNumber];
                            var freqs = new List<double>(segment.Frequencies);
                            freqs.AddRange(seg.Frequencies);
                            dicSegments[physicalChannelNumber].Frequencies = freqs.ToArray();
                        }
                        else
                        {
                            dicSegments.Add(physicalChannelNumber, seg.Clone());
                        }

                        dicBands.TryAdd(physicalChannelNumber, band);
                    }

                    var total = 0;
                    foreach (var kv in dicSegments)
                    {
                        var seg = kv.Value;
                        var temp = GetMergedFrequencyList(seg.Frequencies);
                        var band = dicBands[kv.Key];
                        var freqList = new List<double>();
                        foreach (var subBand in band.ChannelBands)
                        {
                            var count = Math.Min(96 - total, 32);
                            var points = temp.Where(p =>
                                    p.CompareWith(subBand.StartFrequency) >= 0 &&
                                    p.CompareWith(subBand.StopFrequency) <= 0)
                                .Take(count).ToList();
                            freqList.AddRange(points);
                            total += points.Count;
                            if (total >= 96) break;
                        }

                        seg.Frequencies = freqList.ToArray();
                        results.Add(seg);
                        if (total >= 96) break;
                    }
                }
                    break;
                case 2: //扫频
                case 3: //梳状谱 该设备不支持 直接用扫频模式
                {
                    var channelGroups = group.GroupBy(p => p.PhysicalChannelNumber);
                    foreach (var channelGroup in channelGroups)
                    {
                        var physicalChannelNumber = channelGroup.Key;
                        var band = Array.Find(rftxBands, p => p.PhysicalChannelNumber == physicalChannelNumber);
                        if (band == null) continue;
                        var list = new List<(double range, RftxSegmentsTemplate seg)>();
                        foreach (var seg in channelGroup)
                        {
                            var startFrequency = seg.StartFrequency;
                            var stopFrequency = seg.StopFrequency;
                            var stepFrequency = seg.StepFrequency;
                            foreach (var subband in band.ChannelBands)
                            {
                                if (startFrequency.CompareWith(subband.StopFrequency) >= 0 ||
                                    stopFrequency.CompareWith(subband.StartFrequency) <= 0) continue;
                                var start = Math.Max(startFrequency, subband.StartFrequency);
                                var stop = Math.Min(stopFrequency, subband.StopFrequency);
                                var range = stop - start;
                                var count = Utils.GetTotalCount(start, stop, stepFrequency);
                                var step = stepFrequency;
                                if (count > 100) step = range * 1000 / 100;
                                var segment = seg.Clone();
                                segment.StartFrequency = start;
                                segment.StopFrequency = stop;
                                segment.StepFrequency = step;
                                list.Add((range, segment));
                            }
                        }

                        var item = list.OrderByDescending(p => p.range).FirstOrDefault();
                        if (item.seg != null) results.Add(item.seg);
                    }
                }
                    break;
                case 4: //多音
                {
                    var channelGroups = group.GroupBy(p => p.PhysicalChannelNumber);
                    var list = new List<(int count, RftxSegmentsTemplate seg)>();
                    foreach (var channelGroup in channelGroups)
                    {
                        var physicalChannelNumber = channelGroup.Key;
                        var band = Array.Find(rftxBands, p => p.PhysicalChannelNumber == physicalChannelNumber);
                        if (band == null) continue;
                        foreach (var seg in channelGroup)
                        {
                            if (seg.Frequencies?.Any() != true) continue;
                            foreach (var subband in band.ChannelBands)
                            {
                                var temp = seg.Frequencies.Where(p =>
                                    p.CompareWith(subband.StartFrequency) >= 0 &&
                                    p.CompareWith(subband.StopFrequency) <= 0).ToArray();
                                var freqs = GetMergedFrequencyList(temp);
                                if (freqs?.Any() != true) continue;
                                var segment = seg.Clone();
                                segment.Frequencies = freqs.ToArray();
                                list.Add((freqs.Count, seg));
                            }
                        }

                        var item = list.OrderByDescending(p => p.count).FirstOrDefault();
                        if (item.seg != null)
                        {
                            item.seg.Frequencies = item.seg.Frequencies.Take(4).ToArray();
                            results.Add(item.seg);
                        }
                    }
                }
                    break;
            }

            if (legal)
            {
                rftxFrequencyMode = frequencyMode;
                break;
            }
        }

        return results.Any();
    }

    /// <summary>
    ///     合并频点
    /// </summary>
    /// <param name="frequencies">频点列表，单位为MHz</param>
    /// <param name="bandwidth">带宽，单位为MHz</param>
    /// <param name="reserve"></param>
    /// <returns></returns>
    private static List<double> GetMergedFrequencyList(double[] frequencies, double bandwidth = 0.5, double reserve = 0)
    {
        var list = new List<double>();
        if (frequencies?.Any() != true) return list;
        if (bandwidth.CompareWith(0.2) <= 0)
        {
            list.AddRange(frequencies);
            return list;
        }

        var maxDelta = bandwidth - reserve; //跟王凡确认，与带宽边缘保留100kHz
        var freqList = frequencies.Distinct().OrderBy(p => p).ToList();
        var index = 0;
        var rangeList = new List<List<double>>(freqList.Count);
        var temp = new List<double>();
        while (index < freqList.Count)
        {
            var frequency = freqList[index];
            if (index == 0)
            {
                temp.Add(frequency);
            }
            else
            {
                var before = temp.FirstOrDefault();
                var delta = Math.Abs(frequency - before);
                if (delta.CompareWith(maxDelta) > 0)
                {
                    rangeList.Add(new List<double>(temp));
                    temp.Clear();
                    temp.Add(frequency);
                }
                else
                {
                    temp.Add(frequency);
                }
            }

            if (index == freqList.Count - 1)
                if (temp.Any())
                    rangeList.Add(temp);
            index++;
        }

        foreach (var range in rangeList)
        {
            if (range.Count == 0) continue;
            if (range.Count == 1)
            {
                list.Add(range[0]);
            }
            else
            {
                var freq = range[0] + (bandwidth - 0.2) / 2;
                list.Add(freq);
            }
        }

        return list;
    }
}