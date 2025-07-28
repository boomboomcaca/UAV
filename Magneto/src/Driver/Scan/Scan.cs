using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.Scan;

public partial class Scan : ScanBase
{
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();
    private readonly object _lockFirstTimeDic = new();
    private readonly ConcurrentDictionary<int, short[]> _occupancyCache = new();
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<double, SignalsResult>> _signalsCache = new();
    private readonly float _snr = 12;
    private OccupancyStructNew _occupancy;

    /// <summary>
    ///     手动-自动门限切换
    /// </summary>
    private bool _preAutoThresholdSign;

    private double[] _preThresholdValue = Array.Empty<double>();

    public Scan(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        StartMultiSegments();
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

        SendReportDataToCloud();
        return base.Stop();
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

                _occupancy?.SetThreshold(AutoThreshold, i, thd, _tolerance);
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
            total += count;
            var to = occ.Sum(item => item > OccupancyThreshold ? 1 : 0);
            occTotal += to;
            occupancy.Data[i] = new SegmentOccupancyData
            {
                SegmentIndex = i,
                TotalOccupancy = Math.Round((double)to / count * 100d, 2),
                Occupancy = Array.ConvertAll(occ, item => (short)(item * 10)),
                Snr = null
            };
            _occupancyCache.AddOrUpdate(i, _ => Array.ConvertAll(occ, item => (short)(item * 10)),
                (_, _) => Array.ConvertAll(occ, item => (short)(item * 10)));
        }

        occupancy.TotalOccupancy = Math.Round(occTotal / total * 100, 2);
        sendData.Add(occupancy);
        var signals = ProcessSignals(e.Occupancy, e.Snr);
        if (signals != null)
        {
            if (signals.Data?.Length > 0)
                for (var i = 0; i < signals.Data.Length; i++)
                {
                    var tmps = signals.Data[i].Results;
                    if (tmps != null)
                    {
                        Comparison<SignalsResult> comparison = SortByLevel;
                        tmps.Sort(comparison);
                        signals.Data[i].Results = tmps.Take(SignalsCount).ToList();
                    }
                    else
                    {
                        signals.Data[i].Results = new List<SignalsResult>();
                    }
                }

            // sendData.Add(signals);
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000).ConfigureAwait(false);
                SendData(new List<object> { signals });
            });
        }

        SendData(sendData);
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

            if (segSignals.Count > 0)
                for (var i = 0; i < segSignals.Count; i++)
                {
                    var i1 = i;
                    var i2 = i;
                    _signalsCache.AddOrUpdate(i, _ =>
                        {
                            var dic = new ConcurrentDictionary<double, SignalsResult>();
                            if (segSignals[i1] == null || segSignals.Count == 0) return dic;
                            foreach (var item in segSignals[i1]) dic.TryAdd(item.Frequency, item);
                            return dic;
                        },
                        (_, value) =>
                        {
                            if (segSignals[i2] == null || segSignals.Count == 0) return value;
                            foreach (var pair in value)
                            {
                                var sig = pair.Value;
                                sig.IsActive = false;
                                value.TryUpdate(pair.Key, sig, pair.Value);
                            }

                            foreach (var item in segSignals[i2])
                                value.AddOrUpdate(item.Frequency, _ => item, (_, v) =>
                                {
                                    v.Bandwidth = item.Bandwidth;
                                    v.LastTime = item.LastTime;
                                    v.MaxLevel = item.MaxLevel;
                                    v.AvgLevel = item.AvgLevel;
                                    v.IsActive = true;
                                    v.Occupancy = item.Occupancy;
                                    return v;
                                });
                            return value;
                        });
                }

            if (_signalsCache.IsEmpty) return null;
            var sendData = new SDataSignalsList
            {
                Data = new SignalsData[_signalsCache.Count]
            };
            foreach (var pair in _signalsCache)
            {
                if (pair.Value?.IsEmpty != false) continue;
                var segment = new SignalsData
                {
                    SegmentIndex = pair.Key,
                    Results = pair.Value.Select(item => item.Value).ToList()
                };
                sendData.Data[pair.Key] = segment;
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
            if (occ[i] >= OccupancyThreshold)
            {
                if (!AutoThreshold)
                    signalBuffer.Add(i);
                else if (snr[i] >= _snr) signalBuffer.Add(i);
            }

        List<SignalsResult> singalUnite = new();
        //对有信号的频点进行排序
        var pointIndex = signalBuffer.ToArray();
        Array.Sort(pointIndex);
        //提取信号
        for (var i = 0; i < pointIndex.Length; i++)
        {
            int freqIndex;
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
            singalUnite.Add(signal);
            i = e - 1;
        }

        return singalUnite;
    }

    /// <summary>
    ///     按照电平从大到小排序
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private int SortByLevel(SignalsResult x, SignalsResult y)
    {
        return y.MaxLevel.CompareTo(x.MaxLevel);
    }

    /// <summary>
    ///     任务停止时向云端发送报表所需数据
    /// </summary>
    private void SendReportDataToCloud()
    {
        try
        {
            var data = new SDataScanStat
            {
                TaskId = TaskId,
                Data = new List<StatScanInfo>()
            };
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var freqs = new double[SegmentList[i].Total];
                var start = SegmentList[i].StartFrequency;
                var step = SegmentList[i].StepFrequency;
                for (var index = 0; index < SegmentList[i].Total; index++) freqs[index] = start + step * index / 1000;
                var max = SegmentList[i].Max;
                var min = SegmentList[i].Min;
                var mean = SegmentList[i].Mean;
                var info = new StatScanInfo
                {
                    SegmentIndex = i,
                    Frequency = freqs,
                    Maximum = new short[freqs.Length],
                    Minimum = new short[freqs.Length],
                    Average = new short[freqs.Length],
                    Occupancy = new short[freqs.Length]
                };
                Buffer.BlockCopy(max, 0, info.Maximum, 0, freqs.Length * sizeof(short));
                Buffer.BlockCopy(min, 0, info.Minimum, 0, freqs.Length * sizeof(short));
                Buffer.BlockCopy(mean, 0, info.Average, 0, freqs.Length * sizeof(short));
                if (_occupancyCache.TryGetValue(i, out var occ))
                    Buffer.BlockCopy(occ, 0, info.Occupancy, 0, freqs.Length * sizeof(short));
                data.Data.Add(info);
            }

            SendMessageData(new List<object> { data });
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"任务:{TaskId} 报表数据生成失败{ex.Message}");
        }
    }

    //private class ReportCacheInfo
    //{
    //    public short[] Average;
    //    public double[] Frequency;
    //    public short[] Maximum;
    //    public short[] Minimum;
    //    public short[] Occupancy;
    //}
}