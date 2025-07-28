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
using Magneto.Contract.Storage;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.AMIA;

public partial class Amia : ScanBase
{
    /// <summary>
    ///     采集间隔 默认5秒
    /// </summary>
    private const int ProcessInterval = 5;

    private readonly Dictionary<int, ConcurrentQueue<float[]>> _dataCache = new();
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();

    /// <summary>
    ///     信号是否合并
    /// </summary>
    //private readonly bool _isSignalsMerge = true;
    private readonly object _lockFirstTimeDic = new();

    private readonly Dictionary<int, float[]> _maxDataDic = new();

    // 暂时不缓存了
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<double, SignalsResult>> _signalsCache = new();

    private readonly float _snr = 12;
    private CancellationTokenSource _cts;
    private OccupancyStructNew _occupancy;

    /// <summary>
    ///     手动-自动门限切换
    /// </summary>
    private bool _preAutoThresholdSign;

    private DateTime _preProcessTime = DateTime.Now;
    private double[] _preThresholdValue = Array.Empty<double>();
    private Task _processTask;
    private Dictionary<string, Parameter> _runningParameters = new();
    private SDataSignalsList _signalsListCache;

    /// <summary>
    ///     需要每五秒存一次最大值频谱与信号列表，因此需要在存频谱时判断信号列表是否存在了
    /// </summary>
    private bool _signalsSign;

    private Guid _taskId = Guid.Empty;

    public Amia(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        _runningParameters = module.Parameters.ToDictionary(item => item.Name, item => item);
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _taskId = dataPort.TaskId;
        StartMultiSegments();
        if (SaveStatData) StartProcess();
        return true;
    }

    public override bool Stop()
    {
        SaveStatData = false;
        if (_occupancy != null)
        {
            _occupancy.OccupancyChanged -= CalOccupancyChanged;
            _occupancy.Stop();
            _occupancy = null;
        }

        StopProcess();
        return base.Stop();
    }

    public override void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
        RawDataStorage.Instance.Complete(_taskId);
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency or ParameterNames.StepFrequency)
            // 过滤从前端直接设置的起始结束频率等参数
            // 如果不过滤，这三个参数会与频段参数冲突
            return;
        SetParameterInternal(name, value);
        if (name == ParameterNames.AutoThreshold)
        {
            if (_preAutoThresholdSign != AutoThreshold)
            {
                ResetOccupancy();
                _preAutoThresholdSign = AutoThreshold;
            }
        }
        else if (name == ParameterNames.ThresholdSwitch)
        {
            if (ThresholdSwitch)
            {
                ResetOccupancy();
            }
            else if (_occupancy != null)
            {
                _occupancy.OccupancyChanged -= CalOccupancyChanged;
                _occupancy.Stop();
                _occupancy = null;
            }
        }
        else if (name is ParameterNames.ScanSegments or ParameterNames.ScanMode)
        {
            lock (_lockFirstTimeDic)
            {
                _firstTimeDic.Clear();
            }

            ResetOccupancy();
        }
        else if (name == ParameterNames.ThresholdValue && !AutoThreshold)
        {
            lock (LockSegmentList)
            {
                ThresholdValue ??= Array.Empty<double>();
                if (_preThresholdValue.Length != ThresholdValue.Length ||
                    !_preThresholdValue.All(ThresholdValue.Contains))
                {
                    ResetOccupancy();
                    _preThresholdValue = ThresholdValue;
                }
            }
        }
        else if (name == "tolerance" && AutoThreshold)
        {
            ResetOccupancy();
        }
        else if (name == ParameterNames.UnitSelection && !AutoThreshold)
        {
            ResetOccupancy();
        }

        if (_runningParameters.TryGetValue(name, out var parameter)) parameter.Value = value;
    }

    public override void OnData(List<object> data)
    {
        SendDataWithSpan(data);
        lock (LockSegmentList)
        {
            if (ThresholdSwitch && SegmentList?.All(i => i.IsOver) == true && IsTaskRunning)
                for (var i = 0; i < SegmentList.Count; i++)
                {
                    var realData = Array.ConvertAll(SegmentList[i].Data, p => p / 10f);
                    _occupancy?.AddData(i, realData);
                    ProcessStaticsData(i, realData);
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

    private void StartProcess()
    {
        foreach (var pair in _dataCache) pair.Value.Clear();
        _dataCache.Clear();
        _maxDataDic.Clear();
        for (var i = 0; i < SegmentList.Count; i++)
        {
            _dataCache.Add(i, new ConcurrentQueue<float[]>());
            var arr = new float[SegmentList[i].Total];
            Array.Fill(arr, -9999);
            _maxDataDic.Add(i, arr);
        }

        _preProcessTime = DateTime.Now;
        StartSaveData();
        _cts = new CancellationTokenSource();
        _processTask = new Task(p => ProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _processTask.Start();
    }

    private void StopProcess()
    {
        StopSaveData();
        foreach (var pair in _dataCache) pair.Value.Clear();
        _dataCache.Clear();
        _maxDataDic.Clear();
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        catch
        {
        }
    }

    private void ResetOccupancy()
    {
        if (_occupancy != null)
        {
            _occupancy.OccupancyChanged -= CalOccupancyChanged;
            _occupancy.Stop();
            _occupancy = null;
        }

        _signalsCache.Clear();
        if (ThresholdSwitch && SegmentList.Count > 0)
        {
            Console.WriteLine("重置占用度统计");
            var seg = SegmentList.ConvertAll(item =>
                new Tuple<double, double, double>(item.StartFrequency, item.StopFrequency, item.StepFrequency));
            _occupancy = new OccupancyStructNew(seg);
            _occupancy.Start();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var thd = new double[SegmentList[i].Total];
                if (!AutoThreshold)
                {
                    double thv = 0;
                    if (ThresholdValue.Length > 0)
                        thv = ThresholdValue.Length > i ? ThresholdValue[i] : ThresholdValue[^1];

                    // Buffer.BlockCopy(_thresholdValue, offset * sizeof(double), thd, 0, thd.Length * sizeof(double));
                    var array = new short[SegmentList[i].Total];
                    if (UnitSelection == 1)
                    {
                        // 添加天线因子
                        var factor = Factors.Find(item =>
                            Utils.IsNumberEquals(item.StartFrequency, SegmentList[i].StartFrequency)
                            && Utils.IsNumberEquals(item.StopFrequency, SegmentList[i].StopFrequency)
                            && Utils.IsNumberEquals(item.StepFrequency, SegmentList[i].StepFrequency)
                            && item.SegmentOffset == SegmentList[i].Index
                            && item.Total == SegmentList[i].Total
                            && item.Data.Length >= SegmentList[i].Total);
                        Array.Copy(factor.Data, 0, array, 0, array.Length);
                    }

                    for (var j = 0; j < SegmentList[i].Total; j++)
                    {
                        // 对门限进行单位换算
                        switch (UnitSelection)
                        {
                            case 1:
                                thv -= array[j] / 10f;
                                break;
                            case 2:
                                thv += 107f;
                                break;
                        }

                        thd[j] = thv;
                    }

                    SegmentList[i].SetThreshold(thd.Select(item => (float)item).ToArray(), AutoThreshold, false);
                }
                else
                {
                    SegmentList[i].SetThreshold(null, AutoThreshold, false);
                }

                _occupancy?.SetThreshold(AutoThreshold, i, thd, Tolerance);
            }

            if (_occupancy != null) _occupancy.OccupancyChanged += CalOccupancyChanged;
        }
    }

    private void CalOccupancyChanged(object sender, SegmentsOccupancyChangedEventArgs e)
    {
        var sendData = new List<object>();
        if (e.Occupancy == null
            || e.Snr == null)
            return;
        var occupancy = new SDataOccupancy
        {
            Data = new SegmentOccupancyData[SegmentList.Count]
        };
        double total = 0;
        double occTotal = 0;
        for (var i = 0; i < SegmentList.Count; i++)
        {
            if (!e.Occupancy.ContainsKey(i)) return;
            var count = SegmentList[i].Total;
            var occ = e.Occupancy[i];
            if (occ.Length != count) return;
            SegmentList[i].SetThreshold(e.Threshold[i], AutoThreshold, true);
            var snr = e.Snr[i];
            total += count;
            var to = occ.Sum(item => item > OccupancyThreshold ? 1 : 0);
            occTotal += to;
            occupancy.Data[i] = new SegmentOccupancyData
            {
                SegmentIndex = i,
                TotalOccupancy = Math.Round((double)to / count * 100d, 2),
                Occupancy = Array.ConvertAll(occ, item => (short)(item * 10)),
                Snr = snr
            };
        }

        occupancy.TotalOccupancy = Math.Round(occTotal / total * 100, 2);
        sendData.Add(occupancy);
        var signals = ProcessSignals(e.Occupancy, e.Snr);
        if (signals != null) sendData.Add(signals);
        SendData(sendData);
        // 处理完毕信号以后需要重置占用度统计
        _occupancy.Reset();
        if (signals?.Data == null
            || signals.Data.Length == 0
            || signals.Data[0].Results == null
            || signals.Data[0].Results.Count == 0)
            return;
        var sis = signals.Data[0].Results.Where(item => item.Frequency is >= 118d and <= 138d);
        var signalsResults = sis as SignalsResult[] ?? sis.ToArray();
        var freqs = signalsResults.Select(item => item.Frequency).ToArray();
        var bws = signalsResults.Select(item => item.Bandwidth).ToArray();
        var timestamp = Utils.GetNowTimestamp();
        var dic = new Dictionary<string, object>
        {
            { "edgeId", RunningInfo.EdgeId },
            { "frequency", freqs },
            { "bandwidth", bws },
            { "timestamp", timestamp }
        };
        const string uri = "/avim/signalWarning/checkSignalWarning";
        _signalsSign = true;
        _signalsListCache = new SDataSignalsList
        {
            Data = new SignalsData[signals.Data.Length]
        };
        Array.Copy(signals.Data, _signalsListCache.Data, _signalsListCache.Data.Length);
        _ = Task.Run(async () =>
        {
            try
            {
                var res = await CloudClient.Instance.CommonPostAsync(uri, dic);
                if (!res) Console.WriteLine($"调用云端接口{uri}成功但提交失败");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调用云端接口{uri}失败:{ex.Message}");
            }
        });
    }

    private SDataSignalsList ProcessSignals(Dictionary<int, double[]> data, Dictionary<int, float[]> snr)
    {
        try
        {
            List<List<SignalsResult>> segSignals = new();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                if (!data.ContainsKey(i) || data[i].Length != SegmentList[i].Total) continue;
                var signals = UniteSignal(data[i], snr[i], SegmentList[i].StartFrequency, SegmentList[i].StepFrequency,
                    SegmentList[i].Max);
                segSignals.Add(signals);
            }

            // if (segSignals.Count > 0)
            // {
            //     for (int i = 0; i < segSignals.Count; i++)
            //     {
            //         _signalsCache.AddOrUpdate(i, key =>
            //         {
            //             var dic = new ConcurrentDictionary<double, SignalsResult>();
            //             if (segSignals[i] == null || segSignals.Count == 0)
            //             {
            //                 return dic;
            //             }
            //             foreach (var item in segSignals[i])
            //             {
            //                 dic.TryAdd(item.Frequency, item);
            //             }
            //             return dic;
            //         },
            //         (key, value) =>
            //         {
            //             if (segSignals[i] == null || segSignals.Count == 0)
            //             {
            //                 return value;
            //             }
            //             foreach (var pair in value)
            //             {
            //                 var sig = pair.Value;
            //                 sig.IsActive = false;
            //                 value.TryUpdate(pair.Key, sig, pair.Value);
            //             }
            //             foreach (var item in segSignals[i])
            //             {
            //                 value.AddOrUpdate(item.Frequency, k => item, (k, v) =>
            //                 {
            //                     v.Bandwidth = item.Bandwidth;
            //                     v.LastTime = item.LastTime;
            //                     v.MaxLevel = item.MaxLevel;
            //                     v.AvgLevel = item.AvgLevel;
            //                     v.IsActive = true;
            //                     v.Occupancy = item.Occupancy;
            //                     return v;
            //                 });
            //             }
            //            return value;
            //        });
            //    }
            // 
            // i (_signalsCache.IsEmpty)
            // 
            //     return null;
            // }
            if (segSignals.Count == 0) return null;
            var sendData = new SDataSignalsList
            {
                Data = new SignalsData[segSignals.Count]
            };
            // foreach (var pair in _signalsCache)
            // {
            //     if (pair.Value?.IsEmpty != false)
            //     {
            //         continue;
            //     }
            //     var segment = new SignalsData()
            //     {
            //         SegmentIndex = pair.Key,
            //         Results = pair.Value.Select(item => item.Value).ToList()
            //     };
            //     sendData.Data[pair.Key] = segment;
            // }
            for (var i = 0; i < segSignals.Count; i++)
            {
                var segment = new SignalsData
                {
                    SegmentIndex = i,
                    Results = segSignals[i]
                };
                sendData.Data[i] = segment;
            }

            return sendData;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     将有信号的频点合并为信号
    /// </summary>
    /// <param name="occ"></param>
    /// <param name="snr"></param>
    /// <param name="startFrequency"></param>
    /// <param name="stepFrequency"></param>
    /// <param name="maxLevels"></param>
    private List<SignalsResult> UniteSignal(double[] occ, float[] snr, double startFrequency, double stepFrequency,
        short[] maxLevels)
    {
        List<int> signalBuffer = new();
        for (var i = 0; i < occ.Length; i++)
            if (occ[i] >= OccupancyThreshold && snr[i] >= _snr)
                signalBuffer.Add(i);
        List<SignalsResult> singalUnite = new();
        //对有信号的频点进行排序
        var pointIndex = signalBuffer.ToArray();
        Array.Sort(pointIndex);
        //提取信号
        for (var i = 0; i < pointIndex.Length; i++)
        {
            var freqIndex = i;
            var s = i;
            var e = i + 1;
            //判断一个信号的起始索引和结束索引(连续超过门限的频点为一个信号)
            while (e < pointIndex.Length && pointIndex[e] - pointIndex[s] == 1)
            {
                s++;
                e++;
            }

            //计算信号中心频率
            var snrMaxIndex = pointIndex[i];
            var occMaxIndex = pointIndex[i];
            for (var v = i; v < e; v++)
            {
                //计算信噪比最高的频率索引
                if (snr[snrMaxIndex] < snr[pointIndex[v]]) snrMaxIndex = pointIndex[v];
                //计算占用度最高的频率索引
                if (occ[occMaxIndex] < occ[pointIndex[v]]) occMaxIndex = pointIndex[v];
            }

            //如果计算出的占用度最大频点和信噪比最大频点不同，并且两个点的占用度相差 > 1 %，则以占用度大的为中心频率
            if (snrMaxIndex != occMaxIndex && occ[occMaxIndex] - occ[snrMaxIndex] > OccupancyThreshold)
                freqIndex = occMaxIndex;
            else
                freqIndex = snrMaxIndex;
            //计算估测带宽
            var bw = (e - i) * (float)stepFrequency;
            // bool findSkip = false;
            // //公众移动通讯频段不提取
            // if (_skipMobile)
            // {
            //     foreach (KeyValuePair<double, double> mItem in GeneralClass.MobileSegment)
            //     {
            //         if (FrequencyList[freqIndex] >= mItem.Key && FrequencyList[freqIndex] <= mItem.Value)
            //         {
            //             findSkip = true;
            //             break;
            //         }
            //     }
            //     if (findSkip) continue;
            // }
            //生成信号信息
            var freq = startFrequency + freqIndex * stepFrequency / 1000;
            if (freq is >= 88 and <= 108) freq = Math.Round(freq, 1);
            SignalsResult signal = new()
            {
                FrequencyIndex = freqIndex,
                Frequency = freq,
                Bandwidth = bw,
                MaxLevel = maxLevels[freqIndex] / 10f,
                LastTime = Utils.GetNowTimestamp(),
                FirstTime = Utils.GetNowTimestamp(), //背噪 = 实时值 - 信噪比
                Name = "新信号",
                Result = "新信号",
                Occupancy = occ[freqIndex]
            };
            if (!singalUnite.Any(item => Math.Abs(item.Frequency - signal.Frequency) < 1e-9)) singalUnite.Add(signal);
            i = e - 1;
        }

        return singalUnite;
    }

    private void ProcessStaticsData(int segmentIndex, float[] data)
    {
        if (!SaveStatData) return;
        if (!_dataCache.ContainsKey(segmentIndex)) _dataCache.Add(segmentIndex, new ConcurrentQueue<float[]>());
        _dataCache[segmentIndex].Enqueue(data);
    }

    private async Task ProcessAsync(object obj)
    {
        if (!SaveStatData) return;
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            foreach (var pair in _dataCache)
            {
                var info = pair.Value;
                if (info.IsEmpty) continue;
                var max = _maxDataDic[pair.Key];
                while (info.TryDequeue(out var data))
                    for (var i = 0; i < max.Length; i++)
                        if (max[i] < data[i])
                            max[i] = data[i];
                _maxDataDic[pair.Key] = max;
            }

            if (DateTime.Now.Subtract(_preProcessTime).TotalSeconds > ProcessInterval && _signalsSign)
            {
                _signalsSign = false;
                _preProcessTime = DateTime.Now;
                List<object> data = new();
                foreach (var pair in _maxDataDic)
                {
                    var index = pair.Key;
                    var segment = SegmentList[pair.Key];
                    SDataScan scan = new()
                    {
                        SegmentOffset = index,
                        StartFrequency = segment.StartFrequency,
                        StopFrequency = segment.StopFrequency,
                        StepFrequency = segment.StepFrequency,
                        Offset = 0,
                        Total = segment.Total,
                        Data = Array.ConvertAll(pair.Value, item => (short)(item * 10)),
                        DataMark = new byte[] { 0, 0, 0, 0 }
                    };
                    data.Add(scan);
                }

                if (_signalsListCache != null)
                {
                    data.Add(_signalsListCache);
                    _signalsListCache = null;
                }

                RawDataStorage.Instance.Write(_taskId, data);
            }
        }
    }

    /// <summary>
    ///     启动原始数据保存
    /// </summary>
    private void StartSaveData()
    {
        var summary = new FileDataSummary
        {
            EdgeId = RunningInfo.EdgeId,
            TaskId = _taskId.ToString(),
            PluginId = "",
            DriverId = Module.Id.ToString(),
            DeviceId = Receiver.Id.ToString()
        };
        var parameters = _runningParameters.Values.Select(item => item.Clone()).ToList();
        if (RawDataStorage.Instance.IsTaskWriting(_taskId))
            RawDataStorage.Instance.ChangeParameters(_taskId, parameters, true);
        else
            RawDataStorage.Instance.Create(summary, FeatureType.AMIA, DateTime.Now, parameters);
    }

    /// <summary>
    ///     停止保存
    /// </summary>
    private void StopSaveData()
    {
        RawDataStorage.Instance.StopSaveFile(_taskId);
    }
}