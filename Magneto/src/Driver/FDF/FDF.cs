using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver;

public partial class Fdf : DriverBase
{
    private readonly object _lockParam = new();
    private readonly Dictionary<string, object> _parameterCache = new();
    private readonly ConcurrentDictionary<short, ReportCacheInfo> _reportDataCache = new();
    private double _bandwidth;

    /// <summary>
    ///     设备运行标记，防止设备中途切换时将部分参数重置
    /// </summary>
    private bool _deviceRunning;

    private DfBearingStatistics _dfBearingStatistics;
    private double _frequency;
    private bool _hasReceiver;

    private float _levelData;

    // 最优值
    private float _optimalAzimuth = -1f;
    private DateTime _predfpdTime = DateTime.Now;
    private DateTime _preDfTime = DateTime.Now;
    private DfindMethod _preMethod = DfindMethod.Ci;

    private DateTime _preReportTime = DateTime.Now;

    // 概率区间
    private float _wisk;

    public Fdf(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _hasReceiver = Receiver is not null;
        _preDfTime = DateTime.Now;
        return Restart();
    }

    public override bool Stop()
    {
        base.Stop();
        _optimalAzimuth = -1;
        (Dfinder as DeviceBase)?.Stop();
        _deviceRunning = false;
        _dfBearingStatistics?.Clear();
        _dfBearingStatistics?.Stop();
        SendReportDataToCloud();
        if (_dfBearingStatistics != null)
        {
            _dfBearingStatistics.ProbabilityChanged -= DfBearingStatistics_ProbabilityChanged;
            _dfBearingStatistics.AngleStatisticsChanged -= DfBearingStatistics_AngleStatisticsChanged;
        }

        _dfBearingStatistics?.Dispose();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        lock (_lockParam)
        {
            _parameterCache[name] = value;
        }

        if (name is ParameterNames.Frequency or ParameterNames.DfBandwidth)
        {
            _reportDataCache.Clear();
            _dfBearingStatistics?.Clear();
        }

        if (name is ParameterNames.ResolutionBandwidth
            or ParameterNames.QualityThreshold
            or ParameterNames.LevelThreshold)
            _reportDataCache.Clear();
        if (name == "dfindMethod" && _preMethod != DfindMethod) Restart();
        if (name == ParameterNames.Frequency && AntennaController != null)
        {
            AntennaController.SetParameter(ParameterNames.AntennaId, Guid.Empty);
            ((IAntennaController)AntennaController).Frequency = Convert.ToDouble(value);
        }

        if (name == ParameterNames.Frequency && double.TryParse(value?.ToString(), out var freq)) _frequency = freq;
        if (name == ParameterNames.DfBandwidth && double.TryParse(value?.ToString(), out var bw)) _bandwidth = bw;
        if (name is ParameterNames.DfBandwidth or ParameterNames.QualityThreshold or ParameterNames.LevelThreshold)
            _preDfTime = DateTime.Now.AddSeconds(-6);
    }

    public override void OnData(List<object> data)
    {
        var devid = data.Find(item => item is Guid);
        var deviceId = Guid.Empty;
        if (devid != null) deviceId = (Guid)devid;
        if (data.Exists(item => item is SDataDfind)) CanPause = true;
        data.RemoveAll(item => item is Guid);
        if (data.All(item => item is SDataAudio) && _hasReceiver && deviceId == Receiver.Id)
        {
            SendData(data);
            return;
        }

        if (_hasReceiver && deviceId == Dfinder.Id)
            data.RemoveAll(item => item is SDataSpectrum or SDataLevel or SDataAudio);
        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        var dfind = (SDataDfind)data.Find(item => item is SDataDfind);
        var heading = RunningInfo.BufCompassData.Heading;
        if (level != null) _levelData = level.Data;
        if (dfind != null)
        {
            if (dfind.Azimuth > 0)
            {
                _preDfTime = DateTime.Now;
                // dfind.Azimuth += RunningInfo.CompassData.Heading;
                _dfBearingStatistics.AddData(new SdFindData(dfind.Frequency)
                {
                    Azimuth = dfind.Azimuth + heading,
                    Level = _levelData,
                    Quality = dfind.Quality,
                    TimeStamp = DateTime.Now
                });
                // dfind.OptimalAzimuth = _dfBearingStatistics.MaxProbability;
                if (_optimalAzimuth >= 0)
                {
                    var opt = _optimalAzimuth - heading;
                    dfind.OptimalAzimuth = (opt + 360) % 360;
                    dfind.ProbabilityInterval = _wisk > 60 ? 60 : _wisk;
                }

                ProcessReportCi(dfind, _levelData, heading);
            }
            else
            {
                data.Remove(dfind);
            }
        }

        if (DateTime.Now.Subtract(_preDfTime).TotalSeconds > 5)
        {
            _preDfTime = DateTime.Now;
            var dfindNo = new SDataDfind
            {
                Azimuth = -1,
                OptimalAzimuth = -1,
                Frequency = _frequency,
                BandWidth = _bandwidth,
                Quality = -1
            };
            data.Add(dfindNo);
        }

        var sse = (SDataSse)data.Find(item => item is SDataSse);
        if (sse != null)
        {
            CanPause = true;
            var zoom = (float)sse.Data.Length / 360;
            // 这里不需要发送正北示向度
            // var newData = new float[sse.Data.Length];
            // for (int i = 0; i < sse.Data.Length; i++)
            // {
            //     if ((i + calib) >= sse.Data.Length)
            //     {
            //         newData[i + calib - sse.Data.Length] = sse.Data[i];
            //     }
            //     else
            //     {
            //         newData[i + calib] = sse.Data[i];
            //     }
            // }
            // sse.Data = newData;
            var results = PeakAlgorithm.GetPeak(sse.Data, sse.AzimuthCount);
            sse.Results = results.Select(i => (i / zoom + 360) % 360).ToArray();
            // var str = "";
            // foreach (var item in sse.Results)
            // {
            //     str += $"{item}°,";
            // }
            // Console.WriteLine($"估算示向度：{str}");
            ProcessReportSse(sse, results, heading);
        }

        if (DateTime.Now.Subtract(_preReportTime).TotalSeconds > 1)
        {
            _preReportTime = DateTime.Now;
            var optimal = new SDataDfStatOptimal();
            switch (DfindMethod)
            {
                case DfindMethod.Ci:
                    var azi = _reportDataCache.Values.MaxBy(item => item.Count)?.Azimuth ?? 0;
                    optimal.Data = new short[1];
                    optimal.Data[0] = azi;
                    data.Add(optimal);
                    break;
                case DfindMethod.Sse:
                    optimal.Data = _reportDataCache.Values.Select(item => item.Azimuth).ToArray();
                    data.Add(optimal);
                    break;
            }
        }

        SendData(data);
    }

    private void DfBearingStatistics_AngleStatisticsChanged(object sender, BearingStatisticsArgs e)
    {
    }

    private void DfBearingStatistics_ProbabilityChanged(object sender, BearingStatisticsArgs e)
    {
        _optimalAzimuth = (e.MaxProbability + 360) % 360;
        var probitValues = e.AngleProbability.Count > 0
            ? NormalizedProbit(e.AngleProbability, e.Min, e.Max)
            : null;
        var startAngle = probitValues?.FirstOrDefault(p => p.Value > 0.2f).Key ?? 0f;
        var endAngle = probitValues?.LastOrDefault(p => p.Value > 0.2f).Key ?? 0f;
        _wisk = endAngle - startAngle;
        if (DateTime.Now.Subtract(_predfpdTime).TotalMilliseconds > 100)
        {
            _predfpdTime = DateTime.Now;
            var array = new byte[360];
            if (probitValues != null)
                for (var i = 0; i < probitValues.Count; i++)
                {
                    var index = (int)probitValues.Keys[i];
                    if (array[index] < probitValues.Values[i]) array[index] = (byte)probitValues.Values[i];
                }

            SDataDfindProbDist dfpd = new()
            {
                Data = array
            };
            SendData(new List<object> { dfpd });
        }
    }

    private bool Restart()
    {
        CanPause = false;
        if (_deviceRunning)
        {
            (Dfinder as DeviceBase)?.Stop();
            if (_hasReceiver) (Receiver as DeviceBase)?.Stop();
            _deviceRunning = false;
        }

        _reportDataCache.Clear();
        lock (_lockParam)
        {
            foreach (var pair in _parameterCache) base.SetParameter(pair.Key, pair.Value);
        }

        if (_hasReceiver) (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        switch (DfindMethod)
        {
            case DfindMethod.Ci:
                try
                {
                    _optimalAzimuth = -1;
                    _dfBearingStatistics = new DfBearingStatistics();
                    _dfBearingStatistics.Resolution = 0.1f;
                    _dfBearingStatistics.ProbabilityChanged += DfBearingStatistics_ProbabilityChanged;
                    _dfBearingStatistics.AngleStatisticsChanged += DfBearingStatistics_AngleStatisticsChanged;
                    _dfBearingStatistics.Start();
                    (Dfinder as DeviceBase)?.Start(FeatureType.FFDF, this);
                    _deviceRunning = true;
                    _preMethod = DfindMethod;
                    return true;
                }
                catch
                {
                    return false;
                }
            case DfindMethod.Sse:
                try
                {
                    (Dfinder as DeviceBase)?.Start(FeatureType.SSE, this);
                    _deviceRunning = true;
                    _preMethod = DfindMethod;
                    return true;
                }
                catch
                {
                    return false;
                }
            default:
                return false;
        }
    }

    /// <summary>
    ///     归一化
    /// </summary>
    /// <param name="probitValues">最优值实际概率</param>
    /// <param name="ciStart">可信区间起始值</param>
    /// <param name="ciEnd">可信区间结束值</param>
    private SortedList<float, float> NormalizedProbit(SortedList<float, float> probitValues,
        float ciStart = float.MaxValue,
        float ciEnd = float.MaxValue)
    {
        var range = 360 - ciEnd + ciStart; // 用于根据可信区间缩放
        var scale = 75f; //GetMaxPercent(realMaxProbit);
        SortedList<float, float> normalized = new();
        foreach (var item in probitValues)
        {
            var p = item.Value * scale;
            if (!ciStart.Equals(float.MaxValue))
                // 根据可信区间缩放
                if (item.Key < ciStart || item.Key > ciEnd)
                {
                    var gap1 = Math.Abs(item.Key - ciStart);
                    var gap2 = Math.Abs(item.Key - ciEnd);
                    var gap = gap1 < gap2 ? gap1 : gap2;
                    p /= 1 + gap * 2 / range;
                }

            var angle = (item.Key + 360) % 360f;
            normalized.Add(angle, p);
        }

        return normalized;
    }

    private void ProcessReportCi(SDataDfind data, float level, float compass)
    {
        var azi = (short)((data.Azimuth + compass) * 10);
        var slevel = (short)(level * 10);
        var quality = (short)(data.Quality * 10);
        _reportDataCache.AddOrUpdate(azi,
            _ => new ReportCacheInfo
            {
                Azimuth = azi,
                MaxLevel = slevel,
                MaxQuality = quality,
                Count = 1
            },
            (_, v) =>
            {
                v.Count++;
                if (slevel > v.MaxLevel) v.MaxLevel = slevel;
                if (quality > v.MaxQuality) v.MaxQuality = quality;
                return v;
            });
    }

    private void ProcessReportSse(SDataSse data, List<int> results, float compass)
    {
        var count = data.AzimuthCount;
        if (count > results.Count) count = results.Count;
        for (short i = 0; i < count; i++)
        {
            var azi = (short)((data.Results[i] + compass) * 10);
            var quality = (short)(data.Data[results[i]] * 10);
            _reportDataCache.AddOrUpdate(i,
                _ => new ReportCacheInfo
                {
                    Azimuth = azi,
                    MaxQuality = quality
                },
                (_, v) =>
                {
                    if (quality > v.MaxQuality)
                    {
                        v.Azimuth = azi;
                        v.MaxQuality = quality;
                    }

                    return v;
                });
        }
    }

    /// <summary>
    ///     任务停止时向云端发送报表所需数据
    /// </summary>
    private void SendReportDataToCloud()
    {
        try
        {
            var infos = _reportDataCache.Values.ToList();

            int Comparison(ReportCacheInfo x, ReportCacheInfo y)
            {
                return y.Azimuth.CompareTo(x.Azimuth);
            }

            infos.Sort((Comparison<ReportCacheInfo>)Comparison);
            switch (DfindMethod)
            {
                case DfindMethod.Ci:
                {
                    var data = new SDataDfStatCi
                    {
                        TaskId = TaskId,
                        Data = new StatCiInfo
                        {
                            Azimuth = infos.Select(item => item.Azimuth).ToArray(),
                            MaxLevel = infos.Select(item => item.MaxLevel).ToArray(),
                            MaxQuality = infos.Select(item => item.MaxQuality).ToArray(),
                            Count = infos.Select(item => item.Count).ToArray()
                        }
                    };
                    SendMessageData(new List<object> { data });
                }
                    break;
                case DfindMethod.Sse:
                {
                    var data = new SDataDfStatSse
                    {
                        TaskId = TaskId,
                        Data = new StatSseInfo
                        {
                            Azimuth = infos.Select(item => item.Azimuth).ToArray(),
                            MaxQuality = infos.Select(item => item.MaxQuality).ToArray()
                        }
                    };
                    SendMessageData(new List<object> { data });
                }
                    break;
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"任务:{TaskId} 报表数据生成失败{ex.Message}");
        }
    }

    private class ReportCacheInfo
    {
        public short Azimuth;
        public ushort Count;
        public short MaxLevel;
        public short MaxQuality;
    }
}