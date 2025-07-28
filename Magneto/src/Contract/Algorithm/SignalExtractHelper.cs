/************************************
 * 创建人：liujian
 * 创建时间：2019/8/28 9:55:46
 * 版权所有：
 ***********************************/

using System.Collections.Generic;
using System.Linq;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     信号融合帮助类
/// </summary>
public static class SignalExtractHelper
{
    /// <summary>
    ///     根据频谱进行信号融合
    /// </summary>
    /// <param name="spectrum">频谱数据</param>
    /// <param name="autoThreshold">是否使用自动门限</param>
    /// <param name="threshold">门限值，autoThreshold=true 则为自动门限容差，autoThreshold=false 则为手动门限值</param>
    /// <param name="onlyGenerator">true > occupancyThreshold  false >= occupancyThreshold</param>
    /// <param name="signals">输出信号中心频点索引</param>
    /// <param name="signalsSpan">输出信号跨度（跨步进数）</param>
    /// <param name="freeSignals"></param>
    public static bool ExtractSignalsBySpectrum(float[] spectrum,
        bool autoThreshold,
        int threshold,
        bool onlyGenerator,
        ref List<int> signals,
        ref List<int> signalsSpan,
        ref List<int> freeSignals)
    {
        // 自动门限计算实例
        var theoryThreshold = new AutoThreshold();
        float[] noise = null;
        // 计算信噪比
        var snr = GetSnr(spectrum, ref noise);
        // 计算门限
        var thresholds = Enumerable.Repeat<float>(threshold, spectrum.Length).ToArray();
        if (autoThreshold)
        {
            theoryThreshold.ThreadsholdMargion = threshold;
            thresholds = theoryThreshold.GetThreshold(spectrum);
        }

        // 合并信号
        signals = new List<int>();
        signalsSpan = new List<int>();
        freeSignals = new List<int>();
        CombineSignals(spectrum, snr, thresholds, onlyGenerator, ref signals, ref signalsSpan, ref freeSignals);
        return true;
    }

    /// <summary>
    ///     根据占用度结合占用度过滤门限进行信号融合
    /// </summary>
    /// <param name="occupancy">占用度数据</param>
    /// <param name="occupancyThreshold">占用度过滤门限</param>
    /// <param name="onlyGenerator">true > occupancyThreshold  false >= occupancyThreshold</param>
    /// <param name="signals">输出信号中心频点索引</param>
    /// <param name="signalsSpan">输出信号跨度（跨步进数）</param>
    public static bool ExtractSignalsByOccupancy(float[] occupancy, float occupancyThreshold, bool onlyGenerator,
        ref List<int> signals, ref List<int> signalsSpan)
    {
        signals = new List<int>();
        signalsSpan = new List<int>();
        if (onlyGenerator)
        {
            for (var i = 0; i < occupancy.Length; i++)
                if (occupancy[i] > occupancyThreshold)
                {
                    signals.Add(i);
                    signalsSpan.Add(1);
                }
        }
        else
        {
            for (var i = 0; i < occupancy.Length; i++)
                if (occupancy[i] >= occupancyThreshold)
                {
                    signals.Add(i);
                    signalsSpan.Add(1);
                }
        }

        return true;
    }

    /// <summary>
    ///     使用占用都提取信号，并合并信号
    /// </summary>
    /// <param name="spectrum">用于计算信噪比</param>
    /// <param name="occupancy">占用度用于提取信号</param>
    /// <param name="occupancyThreshold">占用度过滤门限</param>
    /// <param name="onlyGenerator">true > occupancyThreshold  false >= occupancyThreshold</param>
    /// <param name="signals">输出信号中心频点索引</param>
    /// <param name="signalsSpan">输出信号跨度（跨步进数）</param>
    /// <param name="freeSignals"></param>
    public static bool ExtractSignalsByOccupancyWidthCombine(float[] spectrum,
        float[] occupancy,
        float occupancyThreshold,
        bool onlyGenerator,
        ref List<int> signals,
        ref List<int> signalsSpan,
        ref List<int> freeSignals)
    {
        float[] noise = null;
        // 计算信噪比
        var snr = GetSnr(spectrum, ref noise);
        var thresholds = Enumerable.Repeat(occupancyThreshold, spectrum.Length).ToArray();
        // 合并信号
        signals = new List<int>();
        signalsSpan = new List<int>();
        freeSignals = new List<int>();
        //occupancy = occupancy.Select((o) =>
        //{
        //    if (onlyGenerator)
        //    {
        //        return o > occupancyThreshold ? o : 0;
        //    }
        //    else
        //        return o >= occupancyThreshold ? o : 0; ;
        //}).ToArray();
        CombineSignals(occupancy, snr, thresholds, onlyGenerator, ref signals, ref signalsSpan, ref freeSignals);
        return true;
    }

    /// <summary>
    ///     计算信噪比
    /// </summary>
    /// <param name="spectrum"></param>
    /// <param name="noise"></param>
    public static float[] GetSnr(float[] spectrum, ref float[] noise)
    {
        // 自动门限计算实例
        var theoryThreshold = new AutoThreshold
        {
            // 计算噪声
            ThreadsholdMargion = 1
        };
        noise = theoryThreshold.GetThreshold(spectrum);
        // 计算信噪比
        var snr = new float[noise.Length];
        for (var i = 0; i < spectrum.Length; i++) snr[i] = spectrum[i] - noise[i];
        return snr;
    }

    /// <summary>
    ///     信号融合
    /// </summary>
    /// <param name="data"></param>
    /// <param name="snr"></param>
    /// <param name="threshold"></param>
    /// <param name="onlyGenerator"></param>
    /// <param name="signals"></param>
    /// <param name="signalsSpan"></param>
    /// <param name="freeSignals">空闲频点</param>
    private static void CombineSignals(float[] data, float[] snr, float[] threshold, bool onlyGenerator,
        ref List<int> signals, ref List<int> signalsSpan, ref List<int> freeSignals)
    {
        var overStart = -1; // 超过门限起始位置
        var maxSnrIndex = -1; // 连续信号信噪比最大位置
        float maxSnr = 0; // 连续信号信噪比
        float maxOcc = 0;
        for (var i = 0; i < data.Length; i++)
        {
            var a = onlyGenerator ? data[i] > threshold[i] : data[i] >= threshold[i];
            if (a)
            {
                if (overStart < 0)
                {
                    maxSnrIndex = i;
                    overStart = i;
                    maxSnr = snr[i];
                    maxOcc = data[i];
                }
                else
                {
                    if (snr[i] > maxSnr)
                    {
                        maxSnr = snr[i];
                        maxSnrIndex = i;
                    }

                    if (data[i] > maxOcc) maxOcc = data[i];
                }
            }
            else
            {
                // 中断，处理信号
                if (overStart >= 0)
                {
                    if (maxSnr > 0.5)
                    {
                        // 有信号
                        signals.Add(maxSnrIndex); // 以最大信噪比作为中心点
                        //signals.Add(maxOccIndex);// 以最大占用度作为中心点
                        signalsSpan.Add(i - overStart);
                    }

                    overStart = -1; // 继续查找
                }
                else
                {
                    freeSignals.Add(i);
                }
            }
        }

        if (overStart >= 0)
            if (maxSnr > 0.5)
            {
                // 有信号
                signals.Add(maxSnrIndex);
                signalsSpan.Add(data.Length - 1 - overStart);
            }
    }
}