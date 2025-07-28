using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Protocol.Define;

namespace Magneto.Driver.FBANDS;

public partial class Fbands : ScanBase
{
    #region 扫描逻辑

    private OccupancyStructNew _occupancy;

    /// <summary>
    ///     信号是否合并
    /// </summary>
    private readonly bool _isSignalsMerge = true;

    /// <summary>
    ///     手动-自动门限切换
    /// </summary>
    private bool _preAutoThresholdSign;

    private double[] _preThresholdValue = Array.Empty<double>();
    private readonly Dictionary<int, DateTime> _firstTimeDic = new();
    private readonly object _lockFirstTimeDic = new();

    public Fbands(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        ScheduleDeviceWork();
        return true;
    }

    public override bool Stop()
    {
        UnScheduleDeviceWork();
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
    }

    public override void OnData(List<object> data)
    {
        OnRsdData(data);
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
        Console.WriteLine("重置占用度统计");
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
            _occupancy.Start();
            var offset = 0;
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var thd = new double[SegmentList[i].Total];
                if (!AutoThreshold)
                {
                    // Buffer.BlockCopy(_thresholdValue, offset * sizeof(double), thd, 0, thd.Length * sizeof(double));
                    for (var j = 0; j < SegmentList[i].Total; j++)
                        if (ThresholdValue.Length > j + offset)
                            thd[j] = ThresholdValue[offset + j];
                        else
                            thd[j] = ThresholdValue[^1];
                    SegmentList[i].SetThreshold(thd.Select(item => (float)item).ToArray(), AutoThreshold, false);
                }
                else
                {
                    SegmentList[i].SetThreshold(null, AutoThreshold, false);
                }

                offset += SegmentList[i].Total;
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
        var signals = ProcessSignals(e.Occupancy);
        if (signals != null) sendData.Add(signals);
        SendData(sendData);
    }

    private SDataSignalsList ProcessSignals(Dictionary<int, double[]> data)
    {
        try
        {
            var segSignals = new List<List<int>>();
            var segSignalSpans = new List<List<int>>();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                if (!data.ContainsKey(i) || data[i].Length != SegmentList[i].Total) continue;
                var ave = Array.ConvertAll(SegmentList[i].Mean, p => p / 10f);
                var occupancy = data[i].Select(o => o > OccupancyThreshold ? Convert.ToSingle(o) : 0).ToArray();
                var signalsIndex = new List<int>();
                var signalsSpan = new List<int>();
                var freeSignals = new List<int>();
                if (_isSignalsMerge)
                    SignalExtractHelper.ExtractSignalsByOccupancyWidthCombine(ave,
                        occupancy,
                        (float)OccupancyThreshold,
                        true,
                        ref signalsIndex,
                        ref signalsSpan,
                        ref freeSignals);
                else
                    SignalExtractHelper.ExtractSignalsByOccupancy(occupancy, (float)OccupancyThreshold, true,
                        ref signalsIndex, ref signalsSpan);
                segSignals.Add(signalsIndex);
                segSignalSpans.Add(signalsSpan);
            }

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
                    var firstTime = Utils.GetNowTime();
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
                        LastTime = Utils.GetNowTimestamp(),
                        MaxLevel = SegmentList[i].Max[freqIndex],
                        AvgLevel = SegmentList[i].Mean[freqIndex],
                        IsActive = true,
                        Result = "新信号",
                        Name = "新信号",
                        Occupancy = data[i][freqIndex]
                    };
                    segment.Results.Add(result);
                }

                sendData.Data[i] = segment;
            }

            return sendData;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region 管制辅助信息

    private readonly List<int> _registeredSwitches = new();
    private volatile bool _enableRadioSuppressingSwitch;

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        RegisterSwitch();
    }

    public override void Dispose()
    {
        UnRegisterSwitch();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void RegisterSwitch()
    {
        if (SwitchArray is ISwitchCallback callback)
        {
            var rmdSwitch = callback.Register(SwitchUsage.RadioMonitoring, OnSwitchChanged);
            var rsdSwitch = callback.Register(SwitchUsage.RadioSuppressing, OnSwitchChanged);
            _registeredSwitches.AddRange(new[] { rmdSwitch, rsdSwitch });
        }
    }

    private void UnRegisterSwitch()
    {
        if (SwitchArray is ISwitchCallback callback)
            _registeredSwitches.ForEach(item => callback.UnRegister(item, OnSwitchChanged));
    }

    private void StartRadioMoinitoring()
    {
        StartMultiSegments();
    }

    private void StopRadioMoinitoring()
    {
        if (_occupancy != null)
        {
            _occupancy.OccupancyChanged -= CalOccupancyChanged;
            _occupancy.Stop();
            _occupancy = null;
        }
    }

    private void StartRadioSuppressing()
    {
        var dev = Suppressor as DeviceBase;
        dev?.Start(FeatureType.FBANDS, this);
    }

    private void StopRadioSuppressing()
    {
        var dev = Suppressor as DeviceBase;
        dev?.Stop();
    }

    private void ScheduleDeviceWork()
    {
        if (_enableRadioSuppressingSwitch)
        {
            StopRadioMoinitoring();
            StartRadioSuppressing();
        }
        else
        {
            StopRadioSuppressing();
            StartRadioMoinitoring();
        }
    }

    private void UnScheduleDeviceWork()
    {
        // 先对开关矩阵复位，避免因第三方功能对开关的默认状态有依赖（如打通某个开关）而无法正常工作的情况
        if (SwitchArray is ISwitchCallback callback) callback.Reset();
        StopRadioSuppressing();
        StopRadioMoinitoring();
    }

    private void OnSwitchChanged(SwitchInfo switchInfo)
    {
        _enableRadioSuppressingSwitch = switchInfo.Usage == SwitchUsage.RadioSuppressing;
        Debug.WriteLine(
            $"任务状态：{IsTaskRunning}, 开关编号：{switchInfo.Index}, 开关名称：{switchInfo.Name}, 开关用途：{switchInfo.Usage}");
        if (IsTaskRunning) ScheduleDeviceWork();
    }

    private void OnRsdData(List<object> data)
    {
        if (data.Any(item => item is SDataRadioSuppressing))
            SendData(data.Where(item => item is SDataRadioSuppressing).ToList());
        data.RemoveAll(item => item is SDataRadioSuppressing);
    }
}

#endregion