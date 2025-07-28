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

namespace Magneto.Driver.MFDF;

public partial class Mfdf : DriverBase
{
    private readonly ConcurrentDictionary<double, ReportCacheInfo> _reportDataCache = new();
    private Task _changeFreqTask;
    private CancellationTokenSource _cts;
    private DfBearingStatistics _dfBearingStatistics;
    private bool _dwellOk;
    private double _frequency;
    private bool _holdOk;
    private int _index = -1;
    private float _levelData;
    private int _measureThreshold;
    private float _optimalAzimuth = -1f;
    private DateTime _preDwellTime = DateTime.Now;
    private DateTime _preHoldTime = DateTime.Now;
    private ReportCacheInfo _reportInfo;
    private bool _startSign;
    private int _total;

    /// <summary>
    ///     归一化区间
    /// </summary>
    private float _wisk = 60f;

    public Mfdf(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _preHoldTime = DateTime.Now;
        _preDwellTime = DateTime.Now;
        _holdOk = false;
        _dwellOk = false;
        _index = -1;
        _optimalAzimuth = -1;
        _dfBearingStatistics = new DfBearingStatistics();
        _dfBearingStatistics.Resolution = 0.1f;
        _dfBearingStatistics.ProbabilityChanged += DfBearingStatistics_ProbabilityChanged;
        _dfBearingStatistics.AngleStatisticsChanged += DfBearingStatistics_AngleStatisticsChanged;
        _dfBearingStatistics.Start();
        _cts = new CancellationTokenSource();
        _changeFreqTask = new Task(p => ChangeFrequencyAsync(p).ConfigureAwait(false), _cts.Token);
        _changeFreqTask.Start();
        _reportDataCache.Clear();
        return true;
    }

    public override bool Stop()
    {
        if (!base.Stop()) return false;
        try
        {
            _cts?.Cancel();
        }
        catch
        {
        }

        try
        {
            _changeFreqTask?.Dispose();
        }
        catch
        {
        }

        SendReportDataToCloud();
        try
        {
            _dfBearingStatistics?.Clear();
            _dfBearingStatistics?.Stop();
            if (_dfBearingStatistics != null)
            {
                _dfBearingStatistics.ProbabilityChanged -= DfBearingStatistics_ProbabilityChanged;
                _dfBearingStatistics.AngleStatisticsChanged -= DfBearingStatistics_AngleStatisticsChanged;
            }

            _dfBearingStatistics?.Dispose();
        }
        catch
        {
        }

        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.Frequency or ParameterNames.DfBandwidth) return;
        base.SetParameter(name, value);
        if (name == ParameterNames.MfdfPoints) _reportDataCache.Clear();
    }

    public override void OnData(List<object> data)
    {
        base.OnData(data);
        ProcessMfdf(data);
        SendData(data);
    }

    private void StartDevice()
    {
        (Receiver as DeviceBase)?.Start(FeatureType.FFDF, this);
    }

    private void StopDevice()
    {
        (Receiver as DeviceBase)?.Stop();
    }

    private void ProcessMfdf(List<object> data)
    {
        data.RemoveAll(item => item is SDataSpectrum);
        if (data.Find(item => item is SDataLevel) is SDataLevel level)
        {
            data.Remove(level);
            if (Math.Abs(level.Frequency - _frequency) < 1e-9)
            {
                _levelData = level.Data;
                var scan = new SDataScan
                {
                    SegmentOffset = 0,
                    Offset = _index,
                    Total = _total,
                    DataMark = new byte[4],
                    Data = new[] { (short)(_levelData * 10) }
                };
                data.Add(scan);
                if (!_holdOk)
                {
                    if (level.Data >= _measureThreshold)
                    {
                        _holdOk = true;
                    }
                    else if (DateTime.Now.Subtract(_preHoldTime).TotalSeconds > HoldTime)
                    {
                        _holdOk = true;
                        _dwellOk = true;
                        _startSign = false;
                    }
                }
            }
        }

        if (data.Find(item => item is SDataDfind) is SDataDfind dfind)
        {
            if (Math.Abs(dfind.Frequency - _frequency) > 1e-9)
            {
                data.Remove(dfind);
            }
            else if (dfind.Azimuth > 0)
            {
                var index = _index;
                var qua = (short)(dfind.Quality * 10);
                if (_reportInfo.Quality < qua)
                {
                    _reportInfo.Quality = qua;
                    _reportInfo.Level = (short)(_levelData * 10);
                    _reportInfo.Azimuth = (short)(dfind.Azimuth * 10);
                }

                // dfind.Azimuth += RunningInfo.CompassData.Heading;
                _dfBearingStatistics.AddData(new SdFindData(dfind.Frequency)
                {
                    Azimuth = dfind.Azimuth + RunningInfo.BufCompassData.Heading,
                    Level = _levelData,
                    Quality = dfind.Quality,
                    TimeStamp = DateTime.Now
                });
                // dfind.OptimalAzimuth = _dfBearingStatistics.MaxProbability;
                if (_optimalAzimuth >= 0)
                {
                    var opt = _optimalAzimuth - RunningInfo.BufCompassData.Heading;
                    dfind.OptimalAzimuth = (opt + 360) % 360;
                    dfind.ProbabilityInterval = _wisk > 60 ? 60 : _wisk;
                }

                var signal = new SDataMfdfSignal
                {
                    Index = index,
                    Frequency = dfind.Frequency,
                    Azimuth = dfind.Azimuth,
                    OptimalAzimuth = dfind.OptimalAzimuth,
                    Level = _levelData,
                    Quality = dfind.Quality
                };
                data.Add(signal);
            }
            else
            {
                data.Remove(dfind);
            }
        }

        if (!_holdOk || (_holdOk && _dwellOk)) data.RemoveAll(item => item is SDataAudio or SDataDfind);
        if (_holdOk && !_dwellOk && DateTime.Now.Subtract(_preDwellTime).TotalSeconds > DwellTime)
        {
            _dwellOk = true;
            _startSign = false;
        }
    }

    private async Task ChangeFrequencyAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(10, token).ConfigureAwait(false);
                if (!IsTaskRunning) return;
                if (_startSign) continue;
                StopDevice();
                if (_index >= 0)
                    _reportDataCache.AddOrUpdate(_frequency,
                        k => new ReportCacheInfo
                        {
                            Frequency = k,
                            Level = _reportInfo.Level,
                            Azimuth = _reportInfo.Azimuth,
                            Quality = _reportInfo.Quality
                        },
                        (_, v) =>
                        {
                            if (!Utils.IsNumberEquals(v.Frequency, _reportInfo.Frequency)) return v;
                            v.Level = _reportInfo.Level;
                            v.Azimuth = _reportInfo.Azimuth;
                            v.Quality = _reportInfo.Quality;
                            return v;
                        });
                MfdfTemplate point = null;
                lock (_lockMfdfPoints)
                {
                    if (_mfdfFrequencies == null || _mfdfFrequencies.Length == 0) continue;
                    _index++;
                    if (_index >= _mfdfFrequencies.Length) _index = 0;
                    point = _mfdfFrequencies[_index];
                }

                _optimalAzimuth = -1;
                _dfBearingStatistics?.Clear();
                _wisk = 0;
                _frequency = point.Frequency;
                _reportInfo = new ReportCacheInfo
                {
                    Frequency = _frequency
                };
                if (_reportDataCache.TryGetValue(_frequency, out var cache))
                {
                    _reportInfo.Level = cache.Level;
                    _reportInfo.Azimuth = cache.Azimuth;
                    _reportInfo.Quality = cache.Quality;
                }

                _measureThreshold = point.MeasureThreshold;
                base.SetParameter(ParameterNames.Frequency, point.Frequency);
                base.SetParameter(ParameterNames.DfBandwidth, point.DfBandwidth);
                base.SetParameter(ParameterNames.AudioSwitch, true);
                _holdOk = false;
                _dwellOk = false;
                _startSign = true;
                _preHoldTime = DateTime.Now;
                _preDwellTime = DateTime.Now;
                StartDevice();
            }
            catch
            {
            }
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
        if (_wisk > 60) _wisk = 60;
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

    /// <summary>
    ///     任务停止时向云端发送报表所需数据
    /// </summary>
    private void SendReportDataToCloud()
    {
        var infos = _reportDataCache.Values.ToList();
        var data = new SDataMfdfStat
        {
            TaskId = TaskId,
            Data = new StatMfdfInfo
            {
                Frequency = infos.Select(item => item.Frequency).ToArray(),
                Azimuth = infos.Select(item => item.Azimuth).ToArray(),
                Level = infos.Select(item => item.Level).ToArray(),
                Quality = infos.Select(item => item.Quality).ToArray()
            }
        };
        SendMessageData(new List<object> { data });
    }

    private class ReportCacheInfo
    {
        public short Azimuth;
        public double Frequency;
        public short Level;
        public short Quality;
    }
}