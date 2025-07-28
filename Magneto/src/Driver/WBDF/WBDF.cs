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

namespace Magneto.Driver.WBDF;

public partial class Wbdf : DriverBase
{
    private readonly WbdfBearingStatistics _bearingStatistics = new();
    private readonly Dictionary<int, Dictionary<int, int>> _probabilityCounterDic = new();
    private readonly ConcurrentDictionary<double, ReportCacheInfo> _reportDataCache = new();
    private double _dfBandwidth;
    private int _freqCount;
    private double _frequency;

    /// <summary>
    ///     最优值
    /// </summary>
    private float[] _maxProbabilityAngle;

    private DateTime _preSignalsTime = DateTime.Now;
    private double _resBandwidth;
    private float[] _spectra;
    private bool _thresholdChanged;

    public Wbdf(Guid driverId) : base(driverId)
    {
        CanPause = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _spectra = null;
        (DFinder as DeviceBase)?.Start(FeatureType.WBDF, this);
        _bearingStatistics.BearingStatisticsMethod = StatisticsMethod.Normal;
        _bearingStatistics.AngleResolution = 2;
        _bearingStatistics.TimeResolution = 15000;
        _bearingStatistics.NormalizeScale = 100; // 控件内部需要使用0-100的区间来渲染概率
        _bearingStatistics.BearingStatisticsMethod = StatisticsMethod.SectorFilter;
        _bearingStatistics.AngleResolution = 1;
        _bearingStatistics.ProbabilityChanged += WBDFBearingStatistics_ProbabilityChanged;
        _bearingStatistics.StartStatistics();
        return true;
    }

    public override bool Stop()
    {
        if (!base.Stop()) return false;
        (DFinder as DeviceBase)?.Stop();
        _bearingStatistics?.StopStatistics();
        _bearingStatistics?.Dispose();
        SendReportDataToCloud();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency && AntennaController != null)
        {
            AntennaController.SetParameter(ParameterNames.AntennaId, Guid.Empty);
            ((IAntennaController)AntennaController).Frequency = Convert.ToDouble(value);
        }

        if (name is ParameterNames.Frequency or ParameterNames.DfBandwidth or ParameterNames.ResolutionBandwidth
            or "dfSamplingCount")
        {
            ClearStatData();
            if (name == ParameterNames.Frequency && double.TryParse(value?.ToString(), out var freq)) _frequency = freq;
            if (name == ParameterNames.DfBandwidth && double.TryParse(value?.ToString(), out var bw)) _dfBandwidth = bw;
            if (name == ParameterNames.ResolutionBandwidth && double.TryParse(value?.ToString(), out var rbw))
                _resBandwidth = rbw;
            if (name == "dfSamplingCount" && int.TryParse(value?.ToString(), out var cnt)) _freqCount = cnt;
            if (_frequency > 0 && _dfBandwidth > 0 && (_resBandwidth > 0 || _freqCount > 0))
            {
                int count;
                if (_freqCount > 0)
                {
                    count = _freqCount;
                    _resBandwidth = _dfBandwidth / (_freqCount - 1);
                }
                else
                {
                    count = (int)Math.Round(_dfBandwidth / _resBandwidth) + 1;
                }

                var startFreq = _frequency - _dfBandwidth / 2000;
                var freqs = new double[count];
                for (var i = 0; i < count; i++) freqs[i] = startFreq + i * _resBandwidth / 1000;
                _bearingStatistics.ClearData();
                _bearingStatistics.InitFreqs(freqs);
            }

            if (name is ParameterNames.LevelThreshold or ParameterNames.SquelchThreshold) _thresholdChanged = true;
        }
    }

    public override void OnData(List<object> data)
    {
        if (data.Find(item => item is SDataSpectrum) is SDataSpectrum spectra)
        {
            _spectra ??= new float[spectra.Data.Length];
            if (_spectra.Length != spectra.Data.Length)
            {
                Array.Resize(ref _spectra, spectra.Data.Length);
                _probabilityCounterDic.Clear();
            }

            var spec = Array.ConvertAll(spectra.Data, item => item / 10f);
            Buffer.BlockCopy(spec, 0, _spectra, 0, spectra.Data.Length * sizeof(float));
        }

        if (data.Find(item => item is SDataDfpan) is SDataDfpan wbdf)
        {
            _preSignalsTime = DateTime.Now;
            var frequencyOffset = wbdf.Frequency - wbdf.Span / 2 / 1000;
            var frequencyResolution =
                wbdf.Span / (wbdf.Azimuths.Length % 2 > 0 ? wbdf.Azimuths.Length - 1 : wbdf.Azimuths.Length);
            if (_spectra?.Length != wbdf.Azimuths.Length)
            {
                Array.Resize(ref _spectra, wbdf.Azimuths.Length);
                _probabilityCounterDic.Clear();
            }

            wbdf.OptimalAzimuths = wbdf.Azimuths;
            if (_maxProbabilityAngle?.Length == wbdf.Azimuths.Length) wbdf.OptimalAzimuths = _maxProbabilityAngle;
            FillProbabilityCounterDictionary(wbdf);
            var signals = GetDfSignalsFromWbdf(frequencyOffset, frequencyResolution, wbdf.Azimuths, wbdf.Qualities,
                _spectra);
            if (signals?.Data?.Length > 0)
                for (var index = 0; index < signals.Data.Length; ++index)
                    signals.Data[index].OptimalAzimuth =
                        CalculateMaxProbabilityAngleWithSpecficKey(signals.Data[index].Index);
            else if (signals == null)
                signals = new SDataDfSignalList
                {
                    Data = Array.Empty<DfSignalData>()
                };
            // 原子化服务和一体化服务需要SDataDfpan数据 暂时不移除。
            //data.Remove(wbdf);
            data.Add(signals);
            // 添加数据到统计算法
            var gps = RunningInfo.BufGpsData;
            var heading = RunningInfo.BufCompassData.Heading;
            _bearingStatistics.AddData(new SwbdFindData()
            {
                Azimuth = Array.ConvertAll(wbdf.Azimuths, item => item + heading),
                Lat = gps.Latitude,
                Lng = gps.Longitude,
                Quality = wbdf.Qualities,
                TimeStamp = DateTime.Now
            });
        }

        if (DateTime.Now.Subtract(_preSignalsTime).TotalSeconds > 2 && _thresholdChanged)
        {
            _thresholdChanged = false;
            var signals = new SDataDfSignalList
            {
                Data = Array.Empty<DfSignalData>()
            };
            data.Add(signals);
        }

        SendData(data);
    }

    private void ClearStatData()
    {
        _bearingStatistics?.ClearData();
        _reportDataCache.Clear();
    }

    /// <summary>
    ///     提取信号
    /// </summary>
    /// <param name="frequencyOffset">起始频率</param>
    /// <param name="frequencyResolution">频率步进</param>
    /// <param name="azimuths">角度集合</param>
    /// <param name="qualities">质量集合</param>
    /// <param name="spectra">电平集合</param>
    private SDataDfSignalList GetDfSignalsFromWbdf(double frequencyOffset, double frequencyResolution, float[] azimuths,
        float[] qualities, float[] spectra)
    {
        if (azimuths == null || qualities == null || spectra == null
            || azimuths.Length != qualities.Length || azimuths.Length != spectra.Length)
            return null;
        var signalIdentifier = false;
        var signalStartIndex = -1;
        var signalIndexCounter = 0;
        var signalList = new List<object>();
        var compass = RunningInfo.BufCompassData.Heading;
        for (var index = 0; index < azimuths.Length; ++index)
        {
            if (signalIdentifier)
            {
                if (azimuths[index] >= 0 && index != azimuths.Length - 1)
                {
                    signalIndexCounter++;
                }
                else
                {
                    var offset = signalStartIndex + signalIndexCounter / 2;
                    var signal = new DfSignalData
                    {
                        Index = offset,
                        Frequency = frequencyOffset + offset * frequencyResolution / 1000.0d,
                        Bandwidth = frequencyResolution * signalIndexCounter,
                        Amplitude = spectra[offset],
                        Azimuth = azimuths[offset],
                        Quality = qualities[offset]
                    };
                    signalList.Add(signal);
                    signalIdentifier = false;
                    signalIndexCounter = 0;
                }
            }
            else if (azimuths[index] >= 0)
            {
                signalIdentifier = true;
                signalStartIndex = index;
                signalIndexCounter = 1;
            }

            var freq = frequencyOffset + index * frequencyResolution / 1000d;
            var level = spectra[index];
            var azimuth = azimuths[index] + compass;
            var quality = qualities[index];
            _reportDataCache.AddOrUpdate(freq,
                _ => new ReportCacheInfo
                {
                    Frequency = freq,
                    Level = (short)(level * 10),
                    Azimuth = (short)(azimuth * 10),
                    Quality = (short)(quality * 10)
                },
                (_, v) =>
                {
                    if (quality > v.Quality)
                    {
                        v.Level = (short)(level * 10);
                        v.Azimuth = (short)(azimuth * 10);
                        v.Quality = (short)(quality * 10);
                    }

                    return v;
                });
        }

        if (signalList.Count > 0)
            return new SDataDfSignalList { Data = Array.ConvertAll(signalList.ToArray(), item => (DfSignalData)item) };
        return null;
    }

    private void FillProbabilityCounterDictionary(SDataDfpan wbdf)
    {
        if (wbdf?.Azimuths == null) return;
        for (var index = 0; index < wbdf.Azimuths.Length; ++index)
            try
            {
                int currentAngle;
                if (Math.Abs(wbdf.Azimuths[index] - -1) > 1e-9 && !float.IsNaN(wbdf.Azimuths[index]))
                    currentAngle = (int)(wbdf.Azimuths[index] * 10);
                else
                    currentAngle = -10;
                if (!_probabilityCounterDic.ContainsKey(index))
                    _probabilityCounterDic[index] = new Dictionary<int, int>
                    {
                        [currentAngle] = 1
                    };
                else if (!_probabilityCounterDic[index].ContainsKey(currentAngle))
                    _probabilityCounterDic[index][currentAngle] = 1;
                else
                    _probabilityCounterDic[index][currentAngle]++;
            }
            catch
            {
            }
    }

    private float CalculateMaxProbabilityAngleWithSpecficKey(int key)
    {
        if (!_probabilityCounterDic.ContainsKey(key)) return 0;
        try
        {
            var angleCountDictionary = _probabilityCounterDic[key];
            var maximum = -1;
            var angle = -10;
            foreach (var item in angleCountDictionary)
            {
                if (item.Key < 0) continue;
                if (item.Value > maximum)
                {
                    maximum = item.Value;
                    angle = item.Key;
                }
            }

            return angle / 10.0f;
        }
        catch
        {
            return 0;
        }
    }

    private void WBDFBearingStatistics_ProbabilityChanged(object sender, WbdfBearingStatisticsArgs e)
    {
        // double[angle count][freq count]
        // 最优值的质量从这里面取，暂时不给
        // 后面给的话不从这里取，直接修改算法，让算法输出最优值的概率
        var compass = RunningInfo.BufCompassData.Heading;
        _maxProbabilityAngle = Array.ConvertAll(e.MaxProbabilityAngle, item => item - compass);
        // if (prop == null || prop.GetLength(0) == 0 || prop.GetLength(1) == 0)
        // {
        //     return;
        // }
    }

    /// <summary>
    ///     任务停止时向云端发送报表所需数据
    /// </summary>
    private void SendReportDataToCloud()
    {
        try
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
        catch (Exception ex)
        {
            Trace.WriteLine($"任务:{TaskId} 报表数据生成失败{ex.Message}");
        }
    }

    private class ReportCacheInfo
    {
        public short Azimuth;
        public double Frequency;
        public short Level;
        public short Quality;
    }
}