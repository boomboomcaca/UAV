using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver;

public partial class Emda : ScanBase
{
    /// <summary>
    ///     所有数据缓存
    /// </summary>
    private readonly Dictionary<int, float[]> _dataCache = new();

    private OccupancyStructNew _occupancy;

    public Emda(Guid driverId) : base(driverId)
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
        var res = base.Stop();
        if (_occupancy != null)
        {
            _occupancy.OccupancyChanged -= CalOccupancyChanged;
            _occupancy.Stop();
            _occupancy = null;
        }

        _dataCache.Clear();
        return res;
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
        if (name is ParameterNames.ScanSegments or ParameterNames.ScanMode or ParameterNames.MeasureThreshold)
            ResetOccupancy();
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
                    _dataCache.TryAdd(i, new float[SegmentList[i].Total]);
                    Buffer.BlockCopy(realData, 0, _dataCache[i], 0, realData.Length * sizeof(float));
                    // _dataCache[i] = realData;
                }
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

        if (ThresholdSwitch && SegmentList.Count > 0)
        {
            var seg = SegmentList.ConvertAll(item =>
                new Tuple<double, double, double>(item.StartFrequency, item.StopFrequency, item.StepFrequency));
            _occupancy = new OccupancyStructNew(seg);
            _occupancy.OccupancyChanged += CalOccupancyChanged;
            _occupancy.Start();
            for (var i = 0; i < SegmentList.Count; i++)
            {
                var thd = Enumerable.Repeat(MeasureThreshold, SegmentList[i].Total).ToArray();
                _occupancy?.SetThreshold(false, i, thd, 0);
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
            for (var i = 0; i < SegmentList.Count; i++)
            {
                if (!e.Occupancy.ContainsKey(i)) return;
                var count = SegmentList[i].Total;
                var occ = e.Occupancy[i];
                if (occ.Length != count) return;
                var data = _dataCache[i];
                var signals = GetEmdaInfo(data, occ, i);
                _dataCache[i] = new float[SegmentList[i].Total];
                if (signals != null) sendData.Add(signals);
            }

            if (sendData.Count > 0) SendData(sendData);
        }
        catch
        {
        }
    }

    private SDataEmdaSignals GetEmdaInfo(float[] data, double[] occupancy, int segmentIndex)
    {
        if (SegmentList.Count <= segmentIndex) return null;
        var seg = SegmentList[segmentIndex];
        var step = seg.StepFrequency;
        var total = seg.Total;
        var start = -1;
        var signals = new SDataEmdaSignals
        {
            SegmentIndex = segmentIndex,
            Data = new List<EmdaInfo>()
        };
        var index = -1;
        var maxLevel = -1000d;
        var maxOcc = 0d;
        for (var i = 0; i < total; i++)
        {
            var add = data[i] >= MeasureThreshold;
            if (add)
            {
                maxLevel = Math.Max(maxLevel, data[i]);
                maxOcc = Math.Max(maxOcc, occupancy[i]);
                if (start < 0) start = i;
            }
            else
            {
                var stop = i - 1;
                if (start >= 0 && stop >= 0)
                {
                    index++;
                    var startFreq = Math.Round(seg.StartFrequency + start * step / 1000, 6);
                    var stopFreq = Math.Round(seg.StartFrequency + stop * step / 1000, 6);
                    var emda = new EmdaInfo
                    {
                        Index = index,
                        StartFrequency = startFreq,
                        StopFrequency = stopFreq,
                        Bandwidth = Math.Round(step * (stop - start + 1), 2),
                        StepFrequency = step,
                        Total = stop - start + 1,
                        MaxLevel = Math.Round(maxLevel, 2),
                        Threshold = MeasureThreshold,
                        Occupancy = Math.Round(maxOcc, 2)
                    };
                    signals.Data.Add(emda);
                    start = -1;
                    maxLevel = -1000d;
                }
            }
        }

        if (start >= 0)
        {
            var stop = total - 1;
            index++;
            var startFreq = Math.Round(seg.StartFrequency + start * step / 1000, 6);
            var stopFreq = Math.Round(seg.StartFrequency + stop * step / 1000, 6);
            var emda = new EmdaInfo
            {
                Index = index,
                StartFrequency = startFreq,
                StopFrequency = stopFreq,
                Bandwidth = Math.Round(step * (stop - start + 1), 2),
                StepFrequency = step,
                Total = stop - start + 1,
                MaxLevel = Math.Round(maxLevel, 2),
                Threshold = MeasureThreshold,
                Occupancy = Math.Round(maxOcc, 2)
            };
            signals.Data.Add(emda);
        }

        return signals;
    }
}