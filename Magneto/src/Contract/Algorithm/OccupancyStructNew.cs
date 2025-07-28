using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.Algorithm;

#region 一些定义

internal class SegmentInfo
{
    public SegmentInfo(double startFrequency, double stopFrequency, double stepFrequency)
    {
        StartFrequency = startFrequency;
        StopFrequency = stopFrequency;
        StepFrequency = (float)stepFrequency;
        Total = Utils.GetTotalCount(startFrequency, stopFrequency, stepFrequency);
        Max = Enumerable.Repeat<float>(-999, Total).ToArray();
        MaxPerSlice = Enumerable.Repeat<float>(-999, Total).ToArray();
        OverThresholdTimes = new int[Total];
        Snr = new float[Total];
    }

    /// <summary>
    ///     起始频率
    /// </summary>
    public double StartFrequency { get; }

    /// <summary>
    ///     结束频率
    /// </summary>
    public double StopFrequency { get; }

    /// <summary>
    ///     频率步进
    /// </summary>
    public float StepFrequency { get; }

    /// <summary>
    ///     总频点个数
    /// </summary>
    public int Total { get; }

    /// <summary>
    ///     最大值
    /// </summary>
    public float[] Max { get; set; }

    public float[] MaxPerSlice { get; set; }

    /// <summary>
    ///     超过门限的次数
    /// </summary>
    public int[] OverThresholdTimes { get; set; }

    /// <summary>
    ///     信噪比
    ///     手动门限下不再有效
    /// </summary>
    public float[] Snr { get; set; }

    /// <summary>
    ///     总帧数
    /// </summary>
    public long Count { get; set; }

    public bool AutoThreshold { get; set; }
    public float[] Threshold { get; set; }
    public float Tolerance { get; set; }
    private ConcurrentQueue<float[]> DataCache { get; } = new();

    public void AddData(float[] data)
    {
        if (data == null || data.Length != Total) return;
        DataCache.Enqueue(data);
    }

    public float[] GetData()
    {
        if (DataCache.TryDequeue(out var data)) return data;
        return null;
    }

    public void Reset()
    {
        DataCache?.Clear();
        MaxPerSlice = Enumerable.Repeat<float>(-999, Total).ToArray();
        Max = Enumerable.Repeat<float>(-999, Total).ToArray();
        OverThresholdTimes = new int[Total];
        Snr = new float[Total];
        Count = 0;
    }

    public bool HasData()
    {
        return !DataCache.IsEmpty;
    }

    public void Dispose()
    {
        DataCache?.Clear();
        Max = null;
        MaxPerSlice = null;
        Snr = null;
    }
}

/// <summary>
///     StationInfomationChange 监测站信息发生变化事件参数
/// </summary>
public class SegmentsOccupancyChangedEventArgs(
    Dictionary<int, double[]> occupancy,
    Dictionary<int, float[]> snr,
    Dictionary<int, float[]> threshold)
    : EventArgs
{
    /// <summary>
    ///     占用度数据 单位 %
    /// </summary>
    public Dictionary<int, double[]> Occupancy { get; set; } = occupancy;

    /// <summary>
    ///     获取信噪比
    /// </summary>
    public Dictionary<int, float[]> Snr { get; } = snr;

    public Dictionary<int, float[]> Threshold { get; } = threshold;
}

#endregion

#region 占用度相关

/// <summary>
///     占用度计算结构
/// </summary>
public class OccupancyStructNew
{
    private readonly TheoryThreshold _calcThreshold = new();

    /// <summary>
    ///     发送占用度的时间间隔，单位毫秒
    /// </summary>
    private readonly int _interval;

    private readonly Dictionary<int, SegmentInfo> _segmentsDic = new();
    private CancellationTokenSource _cts;

    private DateTime _preSendTime = DateTime.Now;
    private Task _processTask;
    private bool _running;

    public OccupancyStructNew(List<Tuple<double, double, double>> segments, int interval = 5000)
    {
        _interval = interval;
        for (var i = 0; i < segments.Count; i++)
        {
            var start = segments[i].Item1;
            var stop = segments[i].Item2;
            var step = segments[i].Item3;
            var seg = new SegmentInfo(start, stop, step);
            _segmentsDic.Add(i, seg);
        }
    }

    public event EventHandler<SegmentsOccupancyChangedEventArgs> OccupancyChanged;

    public void Start()
    {
        _preSendTime = DateTime.Now.AddSeconds(-6);
        _running = true;
        _cts = new CancellationTokenSource();
        _processTask = Task.Run(() => ProcessAsync(_cts.Token));
    }

    public void Stop()
    {
        _running = false;
        _cts?.Cancel();
        try
        {
            _processTask?.Wait();
            _processTask?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts?.Dispose();
        }

        foreach (var pair in _segmentsDic) pair.Value.Dispose();
        _segmentsDic.Clear();
    }

    public void Reset()
    {
        _running = false;
        foreach (var pair in _segmentsDic) pair.Value.Reset();
        _preSendTime = DateTime.Now;
        _running = true;
    }

    public void AddData(int segmentIndex, float[] data)
    {
        if (!_running) return;
        if (!_segmentsDic.ContainsKey(segmentIndex)) return;
        var seg = _segmentsDic[segmentIndex];
        seg.AddData(data);
    }

    /// <summary>
    ///     门限有改变
    /// </summary>
    /// <param name="autoThreshold"></param>
    /// <param name="segmentIndex"></param>
    /// <param name="threshold"></param>
    /// <param name="tolerance">自动门限容差</param>
    public void SetThreshold(bool autoThreshold, int segmentIndex, double[] threshold, double tolerance)
    {
        if (!_segmentsDic.ContainsKey(segmentIndex)) return;
        var seg = _segmentsDic[segmentIndex];
        seg.AutoThreshold = autoThreshold;
        if (autoThreshold)
        {
            seg.Tolerance = (float)tolerance;
            _calcThreshold.ThresholdMargin = (float)tolerance;
        }
        else
        {
            seg.Threshold = threshold.Select(item => (float)item).ToArray();
        }
    }

    private async Task ProcessAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
            try
            {
                if (token.IsCancellationRequested) return;
                if (_segmentsDic == null || _segmentsDic.Count == 0)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                    continue;
                }

                if (!_segmentsDic.All(item => item.Value.HasData()))
                {
                    await Task.Delay(2, token).ConfigureAwait(false);
                    continue;
                }

                var occDic = new Dictionary<int, double[]>();
                var snrDic = new Dictionary<int, float[]>();
                var thdDic = new Dictionary<int, float[]>();
                for (var i = 0; i < _segmentsDic.Count; i++)
                {
                    if (token.IsCancellationRequested) return;
                    var seg = _segmentsDic[i];
                    var occ = new double[seg.Total];
                    var data = seg.GetData();
                    if (data == null || data.Length != seg.Total)
                    {
                        await Task.Delay(1, token).ConfigureAwait(false);
                        continue;
                    }

                    float[] threshold;
                    if (seg.AutoThreshold)
                        threshold = _calcThreshold.CalThreshold(data, seg.StartFrequency, seg.StopFrequency,
                            seg.StepFrequency);
                    else
                        threshold = seg.Threshold;
                    seg.Count++;
                    for (var j = 0; j < seg.Total; j++)
                    {
                        if (token.IsCancellationRequested) return;
                        // if (seg.AutoThreshold)
                        // {
                        //     threshold[j] += seg.Tolerance;
                        // }
                        if (data[j] > threshold[j])
                        {
                            seg.OverThresholdTimes[j]++;
                            var snr = data[j] - threshold[j] - seg.Tolerance;
                            if (seg.Snr[j] < snr) seg.Snr[j] = snr;
                        }

                        occ[j] = Math.Round((double)seg.OverThresholdTimes[j] / seg.Count * 100.0d, 1);
                    }

                    occDic[i] = occ;
                    snrDic[i] = seg.Snr;
                    thdDic[i] = threshold;
                }

                var span = DateTime.Now.Subtract(_preSendTime);
                if (span.TotalMilliseconds >= _interval)
                {
                    var arg = new SegmentsOccupancyChangedEventArgs(occDic, snrDic, thdDic);
                    OccupancyChanged?.Invoke(null, arg);
                    _preSendTime = DateTime.Now;
                }
            }
            catch
            {
                // ignored
            }
    }
}

#endregion