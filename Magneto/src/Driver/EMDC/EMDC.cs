using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.EMDC;

public partial class Emdc : ScanBase
{
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();

    /// <summary>
    ///     空闲频点是否合并
    /// </summary>
    private readonly bool _isFreeSignalsMerge = true;

    /// <summary>
    ///     信号是否合并
    /// </summary>
    private readonly bool _isSignalsMerge = true;

    private readonly object _locker = new();

    private readonly object _lockFirstTimeDic = new();
    private readonly object _lockStationSignalsList = new();
    private readonly AutoResetEvent _saveDataHandle = new(false);

    /// <summary>
    ///     统计间隔，单位min
    /// </summary>
    private readonly int _statInterval = 15;

    private readonly List<StationSignal> _stationSignalsList = new();
    private OccupancyStructNew _occupancy;
    private Dictionary<int, double[]> _occupancyData = new();
    private SDataFreeSignals _preFreeSignals;
    private SDataSignalsList _preSignals;

    /// <summary>
    ///     上次统计的时间
    /// </summary>
    private DateTime _preStatTime = DateTime.MinValue;

    private double _preThresholdValue;
    private bool _send2CloudSign;

    public Emdc(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
        MaximumSwitch = true;
        MinimumSwitch = true;
        MeanSwitch = true;
        NoiseSwitch = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        StartMultiSegments();
        _preStatTime = DateTime.Now;
        return true;
    }

    public override bool Stop()
    {
        if (_occupancy != null)
        {
            _occupancy.OccupancyChanged -= CalOccupancyChanged;
            _occupancy.Stop();
            _occupancy = null;
        }

        _send2CloudSign = false;
        _saveDataHandle.Reset();
        // 停止功能的时候需要提交一次数据
        var time = DateTime.Now;
        _preStatTime = DateTime.Now.AddMinutes(-1 - _statInterval);
        _ = Task.Run(async () =>
        {
            await UpdateData2CloudAsync();
            _send2CloudSign = true;
            _saveDataHandle.Set();
        });
        _saveDataHandle.WaitOne(8000);
        if (!_send2CloudSign)
        {
            Trace.WriteLine("保存数据超时!");
        }
        else
        {
            var span = DateTime.Now.Subtract(time).TotalSeconds;
            Trace.WriteLine($"保存数据耗时{span}s");
        }

        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
        if (name == ParameterNames.ScanSegments)
        {
            lock (_lockFirstTimeDic)
            {
                _firstTimeDic.Clear();
            }

            ResetOccupancy();
        }
        else if (name == ParameterNames.ThresholdValue)
        {
            if (Math.Abs(_preThresholdValue - ThresholdValue) > 1e-9)
            {
                ResetOccupancy();
                _preThresholdValue = ThresholdValue;
            }
        }
    }

    public override void OnData(List<object> data)
    {
        SendDataWithSpan(data);
        lock (LockSegmentList)
        {
            if (SegmentList?.All(i => i.IsOver) == true && IsTaskRunning)
                for (var i = 0; i < SegmentList.Count; i++)
                {
                    var realData = Array.ConvertAll(SegmentList[i].Data, item => item / 10f);
                    _occupancy?.AddData(i, realData);
                }
        }
    }

    protected override void StartDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Stop();
    }

    public override bool Pause()
    {
        return false;
    }

    protected override void UpdateScanSegments()
    {
        base.UpdateScanSegments();
        if (!IsTaskRunning) return;
        // 查询台站数据库并更新本地
        lock (LockSegmentList)
        {
            _stationSignalsList.Clear();
            if (SegmentList == null) return;
            SegmentList.ForEach(item =>
            {
                var start = item.StartFrequency;
                var stop = item.StopFrequency;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var res = await CloudClient.Instance.GetStationSignalsInfoAsync(start, stop);
                        UpdateStationSignals(res);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"更新台站数据失败:{ex.Message}");
                    }
                });
            });
        }
    }

    /// <summary>
    ///     更新台站数据
    /// </summary>
    /// <param name="result"></param>
    private void UpdateStationSignals(StationSignalsInfo[] result)
    {
        lock (_lockStationSignalsList)
        {
            foreach (var item in result)
                if (!_stationSignalsList.Exists(s => IsSignalsExists(item, s)))
                {
                    var freq = Convert.ToDouble(item.FrequencyEfBegin);
                    var bandwidth = Convert.ToDouble(item.FrequencyEBand);
                    var lng = Convert.ToDouble(item.StationLongitude);
                    var lat = Convert.ToDouble(item.StationLatitude);
                    var ss = new StationSignal
                    {
                        Frequency = freq,
                        Bandwidth = bandwidth,
                        StationName = item.StationName,
                        Longitude = lng,
                        Latitude = lat,
                        DemMode = item.DemMode,
                        StationType = item.StationType,
                        TechnicalSystem = item.TechnicalSystem
                    };
                    _stationSignalsList.Add(ss);
                }
        }
    }

    private void ResetOccupancy()
    {
        lock (LockSegmentList)
        {
            if (_occupancy != null)
            {
                _occupancy.OccupancyChanged -= CalOccupancyChanged;
                _occupancy.Stop();
                _occupancy = null;
            }

            if (ThresholdSwitch && SegmentList.Count > 0)
            {
                var seg = SegmentList.ConvertAll(item =>
                    new Tuple<double, double, double>(item.StartFrequency, item.StopFrequency, item.StepFrequency));
                _occupancy = new OccupancyStructNew(seg);
                _occupancy.OccupancyChanged += CalOccupancyChanged;
                _occupancy.Start();
                for (var i = 0; i < SegmentList.Count; i++) _occupancy?.SetThreshold(true, i, null, ThresholdValue);
            }
        }
    }

    private void CalOccupancyChanged(object sender, SegmentsOccupancyChangedEventArgs e)
    {
        var sendData = new List<object>();
        if (e.Occupancy == null
            || e.Snr == null)
            return;
        try
        {
            var occupancy = new SDataOccupancy
            {
                Data = new SegmentOccupancyData[SegmentList.Count]
            };
            for (var i = 0; i < SegmentList.Count; i++)
            {
                if (!e.Occupancy.ContainsKey(i)) return;
                var count = SegmentList[i].Total;
                var occ = e.Occupancy[i];
                if (occ.Length != count) return;
                var snr = e.Snr[i];
                occupancy.Data[i] = new SegmentOccupancyData
                {
                    SegmentIndex = i,
                    Occupancy = Array.ConvertAll(occ, item => (short)(item * 10)),
                    Snr = snr
                };
            }

            _occupancyData = e.Occupancy;
            sendData.Add(occupancy);
            ProcessSignals(_occupancyData, out var signals, out var freeSignals);
            if (signals != null)
            {
                _preSignals = signals;
                sendData.Add(signals);
            }

            if (freeSignals != null)
            {
                _preFreeSignals = freeSignals;
                sendData.Add(freeSignals);
            }

            _ = Task.Run(UpdateData2CloudAsync);
            SendData(sendData);
        }
        catch
        {
            //
        }
    }

    /// <summary>
    ///     处理信号列表以及空闲频点
    /// </summary>
    /// <param name="data">占用度</param>
    /// <param name="signals"></param>
    /// <param name="freeSignals"></param>
    private void ProcessSignals(Dictionary<int, double[]> data, out SDataSignalsList signals,
        out SDataFreeSignals freeSignals)
    {
        signals = null;
        freeSignals = null;
        try
        {
            var segSignals = new List<List<int>>();
            var segSignalSpans = new List<List<int>>();
            var segFrees = new List<List<int>>();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                if (!data.ContainsKey(i) || data[i].Length != SegmentList[i].Total) continue;
                var ave = Array.ConvertAll(SegmentList[i].Mean, p => p / 10f);
                var occupancy = data[i].Select(o => o > OccupancyThreshold ? Convert.ToSingle(o) : 0).ToArray();
                var signalsIndex = new List<int>();
                var signalsSpan = new List<int>();
                var frees = new List<int>();
                if (_isSignalsMerge)
                    SignalExtractHelper.ExtractSignalsByOccupancyWidthCombine(ave,
                        occupancy,
                        (float)OccupancyThreshold,
                        true,
                        ref signalsIndex,
                        ref signalsSpan,
                        ref frees);
                else
                    SignalExtractHelper.ExtractSignalsByOccupancy(occupancy, (float)OccupancyThreshold, true,
                        ref signalsIndex, ref signalsSpan);
                segSignals.Add(signalsIndex);
                segSignalSpans.Add(signalsSpan);
                segFrees.Add(frees);
            }

            signals = GetSignals(data, segSignals, segSignalSpans);
            freeSignals = GetFreeSignals(data, segFrees);
        }
        catch
        {
            //
        }
    }

    private bool IsSignalsExists(StationSignalsInfo s1, StationSignal s2)
    {
        var freq = Convert.ToDouble(s1.FrequencyEfBegin);
        var bandwidth = Convert.ToDouble(s1.FrequencyEBand);
        if (Math.Abs(freq - s2.Frequency) > Epsilon) return false;
        if (Math.Abs(bandwidth - s2.Bandwidth) > Epsilon) return false;
        if (string.Equals(s1.StationName, s2.StationName)) return false;
        return true;
    }

    /// <summary>
    ///     从台站数据集合中查询当前频点的台站数据
    ///     如果有重复的台站，则找到距离本站最近的一个发射源
    /// </summary>
    /// <param name="freq"></param>
    private string GetSignalName(double freq)
    {
        lock (_locker)
        {
            var list = _stationSignalsList.Where(item => freq >= item.Frequency - item.Bandwidth / 1000.0 / 2.0
                                                         && freq <= item.Frequency + item.Bandwidth / 1000.0 / 2.0);
            var stationSignals = list as StationSignal[] ?? list.ToArray();
            if (stationSignals.Any() != true) return string.Empty;
            if (stationSignals.Length == 1) return stationSignals.First().StationName;
            var lng = RunningInfo.BufGpsData.Longitude;
            var lat = RunningInfo.BufGpsData.Latitude;
            var name = "";
            var maxDistance = double.MaxValue;
            foreach (var item in stationSignals)
            {
                var distance = Utils.GetDistance(lat, lng, item.Latitude, item.Longitude);
                if (distance < maxDistance)
                {
                    maxDistance = distance;
                    name = item.StationName;
                }
            }

            return name;
        }
    }

    private SDataSignalsList GetSignals(Dictionary<int, double[]> occupancy, List<List<int>> segSignals,
        List<List<int>> segSignalSpans)
    {
        if (segSignals.Count == 0) return null;
        var sendData = new SDataSignalsList
        {
            Data = new SignalsData[segSignals.Count]
        };
        for (var i = 0; i < segSignals.Count; i++)
        {
            var signalsIndex = segSignals[i];
            var signalsSpan = segSignalSpans[i];
            if (signalsIndex.Count == 0) continue;
            var segment = new SignalsData
            {
                SegmentIndex = i,
                Results = new List<SignalsResult>()
            };
            for (var s = 0; s < signalsIndex.Count; s++)
            {
                var freqIndex = signalsIndex[s];
                var spanStep = signalsSpan[s];
                var firstTime = DateTime.Now;
                lock (_lockFirstTimeDic)
                {
                    if (_firstTimeDic.TryGetValue(freqIndex, out var value))
                        firstTime = value;
                    else
                        _firstTimeDic.Add(freqIndex, firstTime);
                }

                var freq = SegmentList[i].StartFrequency + freqIndex * SegmentList[i].StepFrequency / 1000;
                var result = new SignalsResult
                {
                    FrequencyIndex = freqIndex,
                    Frequency = freq,
                    Bandwidth = SegmentList[i].StepFrequency * spanStep,
                    FirstTime = Utils.GetTimestamp(firstTime),
                    LastTime = Utils.GetTimestamp(DateTime.Now),
                    MaxLevel = (float)Math.Round(SegmentList[i].Max[freqIndex] / 10f, 2),
                    AvgLevel = (float)Math.Round(SegmentList[i].Mean[freqIndex] / 10f, 2),
                    IsActive = true,
                    Result = "新信号",
                    Name = GetSignalName(freq),
                    Occupancy = Math.Round(occupancy[i][freqIndex], 2)
                };
                segment.Results.Add(result);
            }

            sendData.Data[i] = segment;
        }

        return sendData;
    }

    private SDataFreeSignals GetFreeSignals(Dictionary<int, double[]> occupancy, List<List<int>> freeSignals)
    {
        if (freeSignals.Count == 0) return null;
        var sendData = new SDataFreeSignals
        {
            Data = new FreeSignalsData[freeSignals.Count]
        };
        for (var i = 0; i < freeSignals.Count; i++)
        {
            var freesIndex = freeSignals[i];
            if (freesIndex.Count == 0) continue;
            var segment = new FreeSignalsData
            {
                SegmentIndex = i,
                Results = new List<FreeSignalsResult>()
            };
            MergeFreeSignals(freesIndex, out var mergeFrees, out var spans);
            for (var s = 0; s < mergeFrees.Count; s++)
            {
                var freqIndex = mergeFrees[s];
                var spanStep = spans[s];
                var freq = SegmentList[i].StartFrequency + freqIndex * SegmentList[i].StepFrequency / 1000;
                var result = new FreeSignalsResult
                {
                    FrequencyIndex = freqIndex,
                    Frequency = freq,
                    Bandwidth = SegmentList[i].StepFrequency * spanStep,
                    Name = GetSignalName(freq),
                    Occupancy = Math.Round(occupancy[i][freqIndex], 2)
                };
                segment.Results.Add(result);
            }

            sendData.Data[i] = segment;
        }

        return sendData;
    }

    private void MergeFreeSignals(List<int> freeSignals, out List<int> signalsMerge, out List<int> spans)
    {
        signalsMerge = null;
        spans = null;
        if (freeSignals.Count == 0) return;
        var preIndex = freeSignals[0] - 1;
        var startIndex = 0;
        signalsMerge = new List<int>();
        spans = new List<int>();
        if (!_isFreeSignalsMerge)
        {
            signalsMerge.AddRange(freeSignals);
            spans = Enumerable.Repeat(1, freeSignals.Count).ToList();
            return;
        }

        var sign = false;
        for (var i = 0; i < freeSignals.Count; i++)
        {
            sign = freeSignals[i] == preIndex + 1;
            preIndex = freeSignals[i];
            if (!sign)
            {
                var span = i - startIndex;
                var index = freeSignals[startIndex + span / 2];
                signalsMerge.Add(index);
                spans.Add(span);
                startIndex = i;
            }
        }

        if (sign)
        {
            var span = freeSignals.Count - 1 - startIndex;
            var index = freeSignals[startIndex + span / 2];
            signalsMerge.Add(index);
            spans.Add(span);
        }
    }

    private async Task<bool> UpdateData2CloudAsync()
    {
        if (DateTime.Now.Subtract(_preStatTime).TotalMinutes < _statInterval) return false;
        Trace.WriteLine("开始更新电磁数据...");
        _preStatTime = DateTime.Now;
        if (SegmentList.Count == 0)
        {
            Trace.WriteLine("更新电磁数据失败,没有频段数据");
            return false;
        }

        if (_occupancyData == null)
        {
            Trace.WriteLine("更新电磁数据失败,没有占用度数据");
            return false;
        }

        var allCount = SegmentList.Sum(i => i.Total);
        var all1 = _occupancyData.Sum(i => i.Value?.Length ?? 0);
        if (allCount != all1)
        {
            Trace.WriteLine("更新电磁数据失败,占用度数据与频段数据不匹配");
            return false;
        }

        try
        {
            var elecDatas = new List<ElectromagneticData>();
            var signalsList = new List<Signals2Cloud<SignalsResult>>();
            var freesList = new List<Signals2Cloud<FreeSignalsResult>>();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var item = SegmentList[i];
                var startFreq = item.StartFrequency;
                var stopFreq = item.StopFrequency;
                var step = item.StepFrequency;
                var total = item.Total;
                var maximum = item.GetMaxData(0, total);
                var minimum = item.GetMinData(0, total);
                var average = item.GetMeanData(0, total);
                var threshold = new short[total];
                Array.Copy(item.Threshold, 0, threshold, 0, total);
                var occ = _occupancyData[i];
                var elec = new ElectromagneticData
                {
                    StartFrequency = startFreq,
                    StopFrequency = stopFreq,
                    StepFrequency = step,
                    Maximum = Array.ConvertAll(maximum, p => p / 10f),
                    Minimum = Array.ConvertAll(minimum, p => p / 10f),
                    Average = Array.ConvertAll(average, p => p / 10f),
                    Threshold = Array.ConvertAll(threshold, p => p / 10f),
                    Occupancy = occ.Select(d => (float)d).ToArray()
                };
                elecDatas.Add(elec);
                if (_preSignals != null)
                {
                    var signals = new Signals2Cloud<SignalsResult>
                    {
                        StartFrequency = startFreq,
                        StopFrequency = stopFreq,
                        StepFrequency = step,
                        Data = new List<SignalsResult>()
                    };
                    var data = Array.Find(_preSignals.Data, d => d.SegmentIndex == i);
                    data.Results?.ForEach(signals.Data.Add);
                    if (signals.Data.Count > 0) signalsList.Add(signals);
                }

                if (_preFreeSignals != null)
                {
                    var frees = new Signals2Cloud<FreeSignalsResult>
                    {
                        StartFrequency = startFreq,
                        StopFrequency = stopFreq,
                        StepFrequency = step,
                        Data = new List<FreeSignalsResult>()
                    };
                    var data = Array.Find(_preFreeSignals.Data, d => d.SegmentIndex == i);
                    data.Results?.ForEach(frees.Data.Add);
                    if (frees.Data.Count > 0) freesList.Add(frees);
                }
            }

            ResetStat();
            var elecData = new Emdc2CloudData<ElectromagneticData>
            {
                EdgeId = RunningInfo.EdgeId,
                Segments = elecDatas
            };
            var signalsData = new Emdc2CloudData<Signals2Cloud<SignalsResult>>
            {
                EdgeId = RunningInfo.EdgeId,
                Segments = signalsList
            };
            var freesData = new Emdc2CloudData<Signals2Cloud<FreeSignalsResult>>
            {
                EdgeId = RunningInfo.EdgeId,
                Segments = freesList
            };
            await CloudClient.Instance.AddElectromagneticDataAsync(elecData).ConfigureAwait(false);
            await CloudClient.Instance.AddSignalsDataAsync(signalsData).ConfigureAwait(false);
            await CloudClient.Instance.AddFreeSignalsDataAsync(freesData).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    ///     重置统计
    /// </summary>
    private void ResetStat()
    {
        Trace.WriteLine("重置电磁数据统计...");
        SegmentList?.ForEach(item => item.ResetStat());
        _occupancy?.Reset();
    }
}