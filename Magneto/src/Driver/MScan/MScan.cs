using System;
using System.Collections.Generic;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.MScan;

public partial class MScan : ScanBase
{
    private bool _dwellSwitch;
    private double _dwellTime;

    private double _holdTime;

    //private double _measureTime;
    private Dictionary<string, object>[] _mscanPoints;

    public MScan(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        StartMultiSegments();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
        if (name == ParameterNames.MscanPoints && value != null)
        {
            _mscanPoints = (Dictionary<string, object>[])value;
            if (_mscanPoints != null) UpdateScanSegments();
        }

        if (name == ParameterNames.DwellTime && value != null && double.TryParse(value.ToString(), out var dwell))
            _dwellTime = dwell;
        //if (name == ParameterNames.MeasureTime && value != null && double.TryParse(value.ToString(), out var measure))
        //    _measureTime = measure;
        if (name == ParameterNames.HoldTime && value != null && double.TryParse(value.ToString(), out var hold))
            _holdTime = hold;
        if (name == ParameterNames.DwellSwitch && value != null && bool.TryParse(value.ToString(), out var dw))
        {
            _dwellSwitch = dw;
            if (_mscanPoints != null) UpdateScanSegments();
            if (_dwellSwitch)
            {
                SetParameterInternal(ParameterNames.DwellTime, _dwellTime);
                SetParameterInternal(ParameterNames.HoldTime, _holdTime);
                SetParameterInternal(ParameterNames.SquelchSwitch, true);
            }
            else
            {
                SetParameterInternal(ParameterNames.DwellTime, 0);
                SetParameterInternal(ParameterNames.HoldTime, 0);
                SetParameterInternal(ParameterNames.SquelchSwitch, false);
            }
        }
    }

    public override bool Stop()
    {
        CanPause = false;
        return base.Stop();
    }

    protected override void UpdateScanSegments()
    {
        if (_mscanPoints == null || _mscanPoints.Length == 0) return;
        lock (LockSegmentList)
        {
            SegmentList = new List<ScanSegmentStatistics>();
            var total = _mscanPoints.Length;
            var seg = new ScanSegmentStatistics(0, total, true)
            {
                ScanIndex = 0,
                Offset = 0
            };
            SegmentList.Add(seg);
            Factors = GetFactors();
            FactorSendOk = false;
        }

        SegmentIndex = -1;
        AutoResetEvent.Set();
    }

    public override void OnData(List<object> data)
    {
        SendDataWithSpan(data);
    }

    protected override void SendDataWithSpan(List<object> data)
    {
        lock (LockSegmentList)
        {
            if (SegmentList == null
                || SegmentIndex < 0
                || SegmentList.Count <= SegmentIndex
                || (SegmentList[SegmentIndex].IsOver && SegmentList.Count > 1))
                return;
        }

        try
        {
            var scan = (SDataScan)data.Find(item => item is SDataScan);
            if (scan != null)
            {
                SegmentList[0].AppendData(scan.Data, scan.Offset, scan.Data.Length);
                // 现在暂时不做缓存，后续需要与前端协商是发整包数据还是分包数据
                var isOver = SegmentList[0].IsOver;
                SendDataForAppend();
                // 当前频段扫描完毕则继续扫描下一个频段
                if (isOver && SegmentList.Count > 1)
                {
                    IsOver = true;
                    AutoResetEvent.Set();
                }

                data.Remove(scan);
            }

            // 发送驻留离散扫描相关数据
            SendData(data);
        }
        catch
        {
            // 容错代码
        }
    }

    protected override void StartDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.MScan, this);
    }

    protected override void StopDevice()
    {
        if (!IsTaskRunning)
        {
            var dev = Receiver as DeviceBase;
            dev?.Stop();
        }
    }

    protected override List<SDataFactor> GetFactors()
    {
        var factors = new List<SDataFactor>();
        if (_mscanPoints == null || _mscanPoints.Length == 0) return factors;
        var freqs = new double[_mscanPoints.Length];
        for (var i = 0; i < _mscanPoints.Length; ++i)
            freqs[i] = Convert.ToDouble(_mscanPoints[i][ParameterNames.Frequency]);
        if (base.AntennaController == null || base.AntennaController is not IAntennaController antennaController)
            return factors;
        antennaController.Frequency = freqs[0];
        try
        {
            var datas = antennaController.GetFactor(freqs);
            if (datas != null && datas.Length == freqs.Length)
            {
                var factor = new SDataFactor
                {
                    Total = datas.Length,
                    Data = datas
                };
                factors.Add(factor);
            }

            return factors;
        }
        catch (Exception exception)
        {
            var msg = new SDataMessage
            {
                LogType = LogType.Error,
                ErrorCode = (int)InternalMessageType.Error,
                Description = exception.Message,
                Detail = exception.ToString()
            };
            SendMessage(msg);
        }

        return new List<SDataFactor>();
    }
}