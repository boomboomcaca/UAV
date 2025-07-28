using System;
using System.Linq;
using System.Threading.Tasks;
using Magneto.Contract.Algorithm;

namespace Magneto.Device.Wolverine;

public class SignalInner(double stepFreq)
{
    /// <summary>
    ///     信号标识号
    /// </summary>
    public Guid Guid { get; } = Guid.NewGuid();

    /// <summary>
    ///     表示频段索引号，适用于多频段扫描的情况，索引从0开始。
    /// </summary>
    /// <value>The segment offset.</value>
    public int SegmentIdx { get; set; }

    /// <summary>
    ///     表示当前单段频段的总点数。
    /// </summary>
    /// <value>The total.</value>
    public int Total { get; set; }

    /// <summary>
    ///     是否接收完整一段
    /// </summary>
    public bool IsOver { get; set; }

    public bool IsInBandWidth
    {
        get
        {
            var bandWidth = (FreqIdxs.StopFreqIdx - FreqIdxs.StartFreqIdx) * stepFreq / 1000d;
            return bandWidth is > 9d and < 11d or > 19 and < 21 && IsOver;
        }
    }

    /// <summary>
    ///     信号索引
    /// </summary>
    public (int StartFreqIdx, int StopFreqIdx) FreqIdxs { get; set; }

    /// <summary>
    ///     宽带测向数据
    /// </summary>
    public float[] Azimuths { get; set; }

    /// <summary>
    ///     最优示向度
    /// </summary>
    public float Azimuth { get; set; } = -1.0f;
}

/// <summary>
///     信号宽带测向数据处理。
/// </summary>
public class SignalProcess
{
    private SignalInner _signalInner;

    public DateTime LastTime = DateTime.Now;

    public SignalProcess()
    {
        DfBearingStatistics.ProbabilityChanged += DfBearingStatistics_ProbabilityChanged;
        DfBearingStatistics.AngleStatisticsChanged += DfBearingStatistics_AngleStatisticsChanged;
        DfBearingStatistics.Resolution = 0.1f;
        DfBearingStatistics.TimeLength = TimeLength;
        DfBearingStatistics.Start();
    }

    public SignalInner SignalInner
    {
        get => _signalInner;
        set
        {
            _signalInner = value;
            if (_signalInner.IsOver)
                OptimizeAzimuths(_signalInner.Azimuths);
        }
    }

    public int TimeLength
    {
        get => DfBearingStatistics.TimeLength;
        set => DfBearingStatistics.TimeLength = value;
    }

    /// <summary>
    ///     统计示向度。
    /// </summary>
    private DfBearingStatistics DfBearingStatistics { get; } = new(45, 0);

    public void Dispose()
    {
        DfBearingStatistics?.Clear();
        DfBearingStatistics?.Stop();
        if (DfBearingStatistics == null) return;
        DfBearingStatistics.ProbabilityChanged -= DfBearingStatistics_ProbabilityChanged;
        DfBearingStatistics?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void DfBearingStatistics_AngleStatisticsChanged(object sender, BearingStatisticsArgs e)
    {
    }

    public void OptimizeAzimuths(float[] azimuths)
    {
        LastTime = DateTime.Now;
        if (!SignalInner.IsOver) return;
        azimuths = ExtractMaxProbability(azimuths);
        if (azimuths.Length.Equals(0)) return;
        foreach (var signalInnerAzimuth in azimuths)
        {
            if (signalInnerAzimuth < 0) continue;
            _ = Task.Run(() =>
            {
                DfBearingStatistics.AddData(new SdFindData()
                {
                    Azimuth = NormalizeAngle(signalInnerAzimuth / 10f), // 这里加上罗盘使出来的数据不再做场地模式判断
                    Level = 80f,
                    Quality = 90f,
                    TimeStamp = DateTime.Now
                });
            });
        }
    }

    private void DfBearingStatistics_ProbabilityChanged(object sender, BearingStatisticsArgs args)
    {
        var dlt = new DelegateProbabilityChanged(e => { _signalInner.Azimuth = NormalizeAngle(e.MaxProbability); });
        dlt.Invoke(args);
    }

    private static float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle < 0) angle += 360;
        return angle;
    }

    private static float[] ExtractMaxProbability(float[] azimuths)
    {
        azimuths = azimuths.Where(val => val > -1f).ToArray();
        var bufAzimuths = azimuths.Select(s =>
        {
            s /= 10f;
            if (s > 180)
                return s - 360;
            return s;
        }).ToArray();
        var sumAzimuth = bufAzimuths.Sum();
        var meanAzimuth = sumAzimuth / bufAzimuths.Length;
        var variance = bufAzimuths.Sum(s => Math.Pow(s - meanAzimuth, 2));
        var standardDeviation = Math.Sqrt(variance / bufAzimuths.Length);
        if (standardDeviation > 100)
            for (var i = 0; i < bufAzimuths.Length; i++)
                azimuths[i] = -1f; // I assume this is what you meant by azimuthsrstd;:get<0>(item)7 12
        else
            for (var i = 0; i < bufAzimuths.Length; i++)
                if (Math.Abs(bufAzimuths[i] - meanAzimuth) > 1.5 * standardDeviation && standardDeviation > 0)
                    azimuths[i] = -1f; // I assume this is what you meant by azimuths[std::get<0>(item)]-14
        return azimuths;
    }

    private delegate void DelegateProbabilityChanged(BearingStatisticsArgs args);
}