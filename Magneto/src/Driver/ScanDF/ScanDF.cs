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

namespace Magneto.Driver.ScanDF;

public partial class ScanDf : ScanBase
{
    private readonly WbdfBearingStatistics _bearingStatistics = new();
    private readonly Dictionary<int, Dictionary<int, int>> _probabilityCounterDic = new();
    private readonly ConcurrentDictionary<double, ReportCacheInfo> _reportDataCache = new();

    /// <summary>
    ///     最优值
    /// </summary>
    private float[] _maxProbabilityAngle;

    private float[] _scan;

    public ScanDf(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = false;
        CanPause = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _scan = null;
        StartMultiSegments();
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
        if (name.Equals(ParameterNames.StartFrequency)
            || name.Equals(ParameterNames.StopFrequency)
            || name.Equals(ParameterNames.StepFrequency))
        {
            base.AntennaController?.SetParameter(ParameterNames.AntennaId, Guid.Empty);
            UpdateScanSegments();
            ClearStatData();
            if (SegmentList?.Count > 0)
            {
                var count = SegmentList[0].Total;
                var start = SegmentList[0].StartFrequency;
                var step = SegmentList[0].StepFrequency;
                var freqs = new double[count];
                for (var i = 0; i < count; i++) freqs[i] = start + i * step / 1000;
                _bearingStatistics.ClearData();
                _bearingStatistics.InitFreqs(freqs);
            }
        }
    }

    public override void OnData(List<object> data)
    {
        if (data.Find(item => item is SDataScan) is SDataScan scan)
        {
            _scan ??= new float[scan.Total];
            if (_scan.Length != scan.Total)
            {
                Array.Resize(ref _scan, scan.Total);
                _probabilityCounterDic.Clear();
            }

            var scanData = Array.ConvertAll(scan.Data, item => item / 10f);
            Buffer.BlockCopy(scanData, 0, _scan, scan.Offset * sizeof(float), scanData.Length * sizeof(float));
            SendDataWithSpan(new List<object> { scan });
        }

        if (data.Find(item => item is SDataDfScan) is SDataDfScan dfscan)
        {
            dfscan.OptimalAzimuths = dfscan.Azimuths;
            if (_maxProbabilityAngle?.Length == dfscan.Azimuths.Length) dfscan.OptimalAzimuths = _maxProbabilityAngle;
            FillProbabilityCounterDictionary(dfscan);
            var signals = GetDfSignalListFromScandf(dfscan, _scan);
            if (signals?.Data?.Length > 0)
            {
                for (var index = 0; index < signals.Data.Length; ++index)
                    signals.Data[index].OptimalAzimuth =
                        CalculateMaxProbabilityAngleWithSpecficKey(signals.Data[index].Index);
                // 原子化服务和一体化服务需要SDataDfScan数据 暂时不移除。
                //data.Remove(dfscan);
                data.Add(signals);
            }

            /*
                signals ??= new SDataDfSignalList
                {
                    Data = Array.Empty<DfSignalData>()
                };
*/
            // 添加数据到统计算法
            var gps = RunningInfo.BufGpsData;
            var heading = RunningInfo.BufCompassData.Heading;
            _bearingStatistics.AddData(new SwbdFindData()
            {
                Azimuth = Array.ConvertAll(dfscan.Azimuths, item => item + heading),
                Lat = gps.Latitude,
                Lng = gps.Longitude,
                Quality = dfscan.Qualities,
                TimeStamp = DateTime.Now
            });
        }

        data.RemoveAll(item => item is SDataScan);
        SendData(data);
    }

    protected override void StartDevice()
    {
        (DFinder as DeviceBase)?.Start(FeatureType.ScanDF, this);
    }

    protected override void StopDevice()
    {
        (DFinder as DeviceBase)?.Stop();
    }

    private void ClearStatData()
    {
        _bearingStatistics?.ClearData();
        _reportDataCache.Clear();
    }

    private SDataDfSignalList GetDfSignalListFromScandf(SDataDfScan dfscan, float[] scan)
    {
        if (dfscan?.Indices == null || dfscan.Azimuths == null || dfscan.Qualities == null
            || scan == null || dfscan.Indices[0] + dfscan.Indices.Length > scan.Length)
            return null;
        var signalIdentifier = false;
        var signalStartIndex = 0;
        var signalIndexCounter = 0;
        var signalList = new List<object>();
        var compass = RunningInfo.BufCompassData.Heading;
        for (var index = 0; index < dfscan.Azimuths.Length; ++index)
        {
            if (signalIdentifier)
            {
                if (dfscan.Azimuths[index] >= 0 && index != dfscan.Azimuths.Length - 1)
                {
                    signalIndexCounter++;
                }
                else
                {
                    var offset = signalStartIndex + signalIndexCounter / 2;
                    var signal = new DfSignalData
                    {
                        Index = dfscan.Indices[0] + offset,
                        Frequency = dfscan.StartFrequency +
                                    (dfscan.Indices[0] + offset) * dfscan.StepFrequency / 1000.0d,
                        Bandwidth = dfscan.StepFrequency * signalIndexCounter,
                        Amplitude = scan[dfscan.Indices[0] + offset],
                        Azimuth = dfscan.Azimuths[offset],
                        Quality = dfscan.Qualities[offset]
                    };
                    signalList.Add(signal);
                    signalIdentifier = false;
                    signalIndexCounter = 0;
                }
            }
            else if (dfscan.Azimuths[index] >= 0)
            {
                signalIdentifier = true;
                signalStartIndex = index;
                signalIndexCounter = 1;
            }

            var freqIndex = dfscan.Indices[0] + index;
            var freq = dfscan.StartFrequency + dfscan.StepFrequency * freqIndex / 1000;
            var level = scan[freqIndex];
            var azimuth = dfscan.Azimuths[index] + compass;
            var quality = dfscan.Qualities[index];
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

    private void FillProbabilityCounterDictionary(SDataDfScan dfscan)
    {
        if (dfscan?.Azimuths == null || dfscan.Indices == null) return;
        for (var index = 0; index < dfscan.Azimuths.Length; ++index)
            try
            {
                int currentAngle;
                if (Math.Abs(dfscan.Azimuths[index] - -1) > 1e-9 && !float.IsNaN(dfscan.Azimuths[index]))
                    currentAngle = (int)(dfscan.Azimuths[index] * 10);
                else
                    currentAngle = -10;
                var frequencyOffsetIndex = index + dfscan.Indices[0];
                if (!_probabilityCounterDic.ContainsKey(frequencyOffsetIndex))
                    _probabilityCounterDic[frequencyOffsetIndex] = new Dictionary<int, int>
                    {
                        [currentAngle] = 1
                    };
                else if (!_probabilityCounterDic[frequencyOffsetIndex].ContainsKey(currentAngle))
                    _probabilityCounterDic[frequencyOffsetIndex][currentAngle] = 1;
                else
                    _probabilityCounterDic[frequencyOffsetIndex][currentAngle]++;
            }
            catch
            {
                // ignored
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
        var heading = RunningInfo.BufCompassData.Heading;
        _maxProbabilityAngle = Array.ConvertAll(e.MaxProbabilityAngle, item => item - heading);
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