using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FixDFEx;

public partial class FixDfEx : DriverBase
{
    #region 构造函数

    public FixDfEx(Guid id) : base(id)
    {
    }

    #endregion

    #region 成员变量

    private CancellationTokenSource _cts;
    private Task _calculatingTask;
    private readonly ManualResetEventSlim _mre = new();
    private ConcurrentQueue<object> _queue;
    private DfBearingStatistics _dfBearingStatistics;
    private float _optimalAzimuth = -1f;

    #endregion

    #region 重新基类方法

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _cts = new CancellationTokenSource();
        // _mre ??= new ManualResetEventSlim();
        _mre.Reset();
        _queue ??= new ConcurrentQueue<object>();
        _queue.Clear();
        _optimalAzimuth = -1;
        _dfBearingStatistics = new DfBearingStatistics();
        _dfBearingStatistics.Resolution = 0.1f;
        _dfBearingStatistics.ProbabilityChanged += DfBearingStatistics_ProbabilityChanged;
        _dfBearingStatistics.AngleStatisticsChanged += DfBearingStatistics_AngleStatisticsChanged;
        _dfBearingStatistics.Start();
        _calculatingTask = CalculateDFindAsync(_cts.Token);
        (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        (AntennaSwivel as DeviceBase)?.Start(FeatureType.AmpDF, this);
        return true;
    }

    public override bool Stop()
    {
        (Receiver as DeviceBase)?.Stop();
        (AntennaSwivel as DeviceBase)?.Stop();
        _cts.Cancel();
        _calculatingTask.ConfigureAwait(false).GetAwaiter().GetResult();
        _cts.Dispose();
        _queue?.Clear();
        _optimalAzimuth = -1;
        _dfBearingStatistics?.Clear();
        _dfBearingStatistics?.Stop();
        if (_dfBearingStatistics != null)
        {
            _dfBearingStatistics.ProbabilityChanged -= DfBearingStatistics_ProbabilityChanged;
            _dfBearingStatistics.AngleStatisticsChanged -= DfBearingStatistics_AngleStatisticsChanged;
        }

        _dfBearingStatistics?.Dispose();
        return base.Stop();
    }

    public override bool Pause()
    {
        return false;
    }

    public override void SetParameter(string name, object value)
    {
        if (name.Equals("frequency", StringComparison.OrdinalIgnoreCase)
            || name.Equals("ifbandwidth", StringComparison.OrdinalIgnoreCase)
            || name.Equals("iqsamplingcount", StringComparison.OrdinalIgnoreCase))
        {
            if (name.Equals("frequency", StringComparison.OrdinalIgnoreCase))
                ((IAntennaController)AntennaController).Frequency = Convert.ToDouble(value);
            _dfBearingStatistics?.Clear();
            _mre.Reset();
            _queue?.Clear();
        }

        base.SetParameter(name, value);
    }

    public override void OnData(List<object> data)
    {
        var epsilon = 1.0e-7;
        var angle = data.Find(item => item is SDataAngle) as SDataAngle;
        if (!_mre.Wait(1))
        {
            if (angle != null && Math.Abs(angle.Azimuth - 0) <= epsilon) _mre.Set();
            return;
        }

        if (angle != null && Math.Abs(angle.Azimuth - 0) <= epsilon) _queue.Enqueue(angle);
        if (data.Find(item => item is SDataLevel) is SDataLevel level) _queue.Enqueue(level);
        var filters = data.Where(item => item is SDataLevel or SDataSpectrum);
        var enumerable = filters as object[] ?? filters.ToArray();
        if (enumerable.Any()) SendData(enumerable.ToList());
    }

    #endregion

    #region Helper

    private async Task CalculateDFindAsync(CancellationToken token)
    {
        await Task.Factory.StartNew(() =>
        {
            var levelList = new List<SDataLevel>();
            while (!token.IsCancellationRequested)
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_queue.TryDequeue(out var data))
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    if (data is SDataLevel level)
                        levelList.Add(level);
                    else
                        break;
                }

                if (token.IsCancellationRequested) break;
                if (!levelList.Any()) continue;
                var levelArray = levelList.ToArray();
                var result = CalculateAzimuth(levelArray, out var index);
                if (result != null)
                {
                    var level = levelArray[index];
                    var azimuth = level.Data >= LevelThreshold ? ((result.Value + Deviation) % 360 + 360) % 360 : -1;
                    var quality = level.Data >= LevelThreshold ? 99 : 0;
                    var dfind = new SDataDfind
                    {
                        Frequency = level.Frequency, BandWidth = level.Bandwidth, Azimuth = azimuth, Quality = quality
                    };
                    if (dfind.Azimuth > 0)
                    {
                        _dfBearingStatistics.AddData(new SdFindData(dfind.Frequency)
                        {
                            Azimuth = dfind.Azimuth + RunningInfo.BufCompassData.Heading,
                            Quality = dfind.Quality,
                            TimeStamp = DateTime.Now
                        });
                        dfind.OptimalAzimuth = dfind.Azimuth;
                    }

                    if (_optimalAzimuth >= 0)
                    {
                        var opt = _optimalAzimuth - RunningInfo.BufCompassData.Heading;
                        dfind.OptimalAzimuth = (opt + 360) % 360;
                    }

                    SendData(new List<object> { dfind });
                    // Console.WriteLine("Level count: {0}, Azimuth: {1}", levelArray.Length, dfind.Azimuth);
                }
            }
            // var iqlist = new List<SDataIQ>();
            // while (true)
            // {
            // 	iqlist.Clear();
            // 	while (true)
            // 	{
            // 		var data = _queue.DeQueue();
            // 		if (data is SDataIQ)
            // 		{
            // 			iqlist.Add(data as SDataIQ);
            // 		}
            // 		else
            // 		{
            // 			break;
            // 		}
            // 	}
            // 	if (iqlist.Count() == 0)
            // 	{
            // 		continue;
            // 	}
            // 	var iqArray = iqlist.ToArray();
            // 	var levelArray = Array.ConvertAll<SDataIQ, SDataLevel>(iqArray, item => ToLevelByIQ(item) as SDataLevel);
            // 	var result = CalculateAzimuth(levelArray, out int index);
            // 	if (result != null)
            // 	{
            // 		var level = levelArray[index];
            // 		var spectrum = ToSpectrumByIQ(iqArray[index]);
            // 		var dfind = new SDataDFind { Frequency = level.Frequency, DFBandwidth = level.IFBandWidth, Azimuth = (result.Value + _deviation) % 360, Quality = 99 };
            // 		Console.WriteLine("Level count: {0}, Azimuth: {1}", levelArray.Length, dfind.Azimuth);
            // 		SendData(new List<object> { level, spectrum, dfind });
            // 	}
            // }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
    }

    private float? CalculateAzimuth(IEnumerable<SDataLevel> levelCollection, out int validIndex, bool clockwise = true)
    {
        validIndex = -1;
        var sDataLevels = levelCollection as SDataLevel[] ?? levelCollection.ToArray();
        if (!sDataLevels.Any()) return null;
        var levelArray = sDataLevels.Select(item => item.Data).ToArray();
        var length = levelArray.Length;
        var anglePerValue = 360.0d / length;
        var maxItem = levelArray.Select((item, index) => new { m = item, Index = index }).OrderByDescending(n => n.m)
            .Take(1).ToArray();
        if (!maxItem.Any()) return null;
        var indexOfMax = maxItem.ToArray()[0].Index;
        var validSegmentLength = length / DivisionCount;
        if (validSegmentLength < 3) return null;
        var lowerIndex = (indexOfMax - validSegmentLength + length) % length;
        var partialArray = new float[2 * validSegmentLength];
        for (var index = 0; index < partialArray.Length; ++index)
            partialArray[index] = levelArray[(lowerIndex + index) % length];
        // 最小二乘法
        var n = 1e-13;
        double a = 0, b = 0, c = 0;
        double z1, z2, z3;
        double sumX = 0d, sumX2 = 0d, sumX3 = 0d, sumX4 = 0d, sumY = 0d, sumXy = 0d, sumX2Y = 0d;
        for (var index = 0; index < partialArray.Length; ++index)
        {
            sumX += index;
            sumY += partialArray[index];
            sumX2 += Math.Pow(index, 2);
            sumXy += index * partialArray[index];
            sumX3 += Math.Pow(index, 3);
            sumX2Y += Math.Pow(index, 2) * partialArray[index];
            sumX4 += Math.Pow(index, 4);
        }

        do
        {
            var m1 = a;
            a = (sumX2Y - sumX3 * b - sumX2 * c) / sumX4;
            z1 = (a - m1) * (a - m1);
            var m2 = b;
            b = (sumXy - sumX * c - sumX3 * a) / sumX2;
            z2 = (b - m2) * (b - m2);
            var m3 = c;
            c = (sumY - sumX2 * a - sumX * b) / partialArray.Length;
            z3 = (c - m3) * (c - m3);
        } while (z1 > n || z2 > n || z3 > n);

        var validIndexTemp = (int)(-b / (2 * a)); // 抛物线顶点对应的导数为零时，得到此时X的值
        if (validIndexTemp < 0 || validIndexTemp >= partialArray.Length) return null;
        validIndex = (validIndexTemp + lowerIndex) % length;
        var angle = (float)(anglePerValue * validIndex);
        return clockwise ? angle : 360 - angle; // 天线是离时针转动的逆时针转动
    }

    #endregion

    #region 事件响应

    private void DfBearingStatistics_AngleStatisticsChanged(object sender, BearingStatisticsArgs e)
    {
    }

    private void DfBearingStatistics_ProbabilityChanged(object sender, BearingStatisticsArgs e)
    {
        _optimalAzimuth = (e.MaxProbability + 360) % 360;
    }

    #endregion
}