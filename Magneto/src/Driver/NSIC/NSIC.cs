using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace NSIC;

public partial class Nsic : ScanBase
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly AutoResetEvent _saveDataHandle = new(false);
    private IDataProcess _dataProcess;
    private bool _isGetTemplateParameters;
    private SegmentResultData[] _resultDataCache;
    private bool _saveDataSign;
    private DateTime _startTime = DateTime.MinValue;

    public Nsic(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        if (_startTime == DateTime.MinValue) _startTime = Utils.GetNowTime().ToUniversalTime();
        SetFunctionSwitch();
        if (_dataProcess != null)
        {
            _dataProcess.DataProcessComplete += ProcessDataComplate;
            _dataProcess.Start();
        }

        StartMultiSegments();
        return true;
    }

    public override bool Pause()
    {
        return false;
    }

    public override bool Stop()
    {
        _saveDataSign = false;
        _saveDataHandle.Reset();
        _isGetTemplateParameters = false;
        if (_dataProcess != null)
        {
            _dataProcess.Stop();
            _dataProcess.DataProcessComplete -= ProcessDataComplate;
        }

        if (FunctionSwitch)
        {
            _ = Task.Run(async () =>
            {
                await UpdateResultDataToCloudAsync();
                _saveDataSign = true;
                _saveDataHandle.Set();
            });
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}的比对数据超时!");
        }
        else
        {
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}超时!");
        }

        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
    }

    protected override void StartDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.Scan, this);
    }

    protected override void StopDevice()
    {
        var dev = Receiver as DeviceBase;
        dev?.Stop();
    }

    protected override void SetParameterInternal(string name, object value)
    {
        base.SetParameterInternal(name, value);
        if (name != ParameterNames.Receiver && name != ParameterNames.AntennaController)
        {
            if (name == ParameterNames.ScanSegments)
            {
            }

            _parameters[name] = new Parameter
            {
                Name = name,
                Value = value
            };
            _dataProcess?.SetParameter(_parameters[name]);
        }
    }

    public override void OnData(List<object> data)
    {
        try
        {
            var scan = (SDataScan)data.Find(item => item is SDataScan);
            if (scan != null && FunctionSwitch && SetSimData)
            {
                var num1 = scan.Total / 4;
                var num2 = scan.Total / 2;
                var num3 = scan.Total / 4 * 3;
                var num11 = num1 - scan.Offset;
                var num22 = num2 - scan.Offset;
                var num33 = num3 - scan.Offset;
                if (num11 >= 0 && num11 < scan.Data.Length) scan.Data[num11] += 200;
                if (num22 >= 0 && num22 < scan.Data.Length) scan.Data[num22] += 200;
                if (num33 >= 0 && num33 < scan.Data.Length) scan.Data[num33] += 200;
            }
        }
        catch
        {
            // 容错代码
        }

        SendDataWithSpan(data);
    }

    protected override void SendData(List<object> data)
    {
        _dataProcess?.OnData(data);
        base.SendData(data);
    }

    protected override bool CanChangeSegments()
    {
        if (FunctionSwitch)
            if (!_isGetTemplateParameters)
                return false;
        return base.CanChangeSegments();
    }

    private void SetFunctionSwitch()
    {
        if (_dataProcess != null)
        {
            _dataProcess.Stop();
            _dataProcess.DataProcessComplete -= ProcessDataComplate;
        }

        if (FunctionSwitch)
            _dataProcess = new ComparisonProcess();
        else
            _dataProcess = new TemplateProcess();
        foreach (var parameter in _parameters) _dataProcess.SetParameter(parameter.Value);
    }

    private void ProcessDataComplate(object sender, List<object> e)
    {
        if (FunctionSwitch)
            ComparisonDataProcess(e);
        else
            _ = Task.Run(() => TemplateDataProcessAsync(e));
    }

    private void ComparisonDataProcess(List<object> result)
    {
        if (result == null || result.Count == 0) return;
        var parameters = (Dictionary<string, object>)result.Find(item => item is Dictionary<string, object>);
        if (parameters != null)
        {
            _isGetTemplateParameters = true;
            foreach (var pair in parameters)
            {
                if (pair.Key is "templateID" or ParameterNames.ThresholdValue or "functionSwitch"
                    or ParameterNames.StartFrequency or ParameterNames.StopFrequency
                    or ParameterNames.StepFrequency) continue;
                SetParameter(pair.Key, pair.Value);
            }
        }

        var info = (List<IList<CompareSignalInfo>>)result.Find(item => item is List<IList<CompareSignalInfo>>);
        if (info == null || info.Count != SegmentList.Count)
            // TODO: wudepeng 这种情况代表有问题，需要记录日志？
            return;
        var data = new SDataNsicResult
        {
            Data = new SegmentResultData[SegmentList.Count]
        };
        for (var i = 0; i < SegmentList.Count; i++)
        {
            data.Data[i] = new SegmentResultData
            {
                SegmentIndex = i,
                Results = []
            };
            foreach (var cr in info[i])
            {
                var index =
                    (int)((cr.Frequency - SegmentList[i].StartFrequency) / (SegmentList[i].StepFrequency / 1000));
                var fr = new FrequencyResult
                {
                    FrequencyIndex = index,
                    Frequency = cr.Frequency,
                    Bandwidth = cr.Bandwidth,
                    FirstTime = Utils.GetTimestamp(cr.FirstCaptureTime),
                    LastTime = cr.LastCaptureTime == DateTime.MinValue ? 0 : Utils.GetTimestamp(cr.LastCaptureTime),
                    MaxLevel = (float)cr.MaxLevel,
                    AvgLevel = (float)cr.AveLevel,
                    IsActive = cr.IsLunching,
                    Result = cr.CompareResult
                };
                data.Data[i].Results.Add(fr);
            }
        }

        _resultDataCache = data.Data;
        base.SendData([data]);
    }

    private async Task TemplateDataProcessAsync(List<object> result)
    {
        // 最大值  均值  门限  最大值-门限
        var info = (List<Tuple<float[], float[], float[], float[]>>)result.Find(i =>
            i is List<Tuple<float[], float[], float[], float[]>>);
        if (info == null || info.Count != SegmentList.Count)
            // TODO: wudepeng 这种情况代表有问题，需要记录日志？
            return;
        var data = new SDataNsicTemplate
        {
            TemplateId = TemplateId,
            Data = new SegmentTemplateData[SegmentList.Count]
        };
        for (var i = 0; i < SegmentList.Count; i++)
            data.Data[i] = new SegmentTemplateData
            {
                SegmentIndex = i,
                StartFrequency = SegmentList[i].StartFrequency,
                StopFrequency = SegmentList[i].StopFrequency,
                StepFrequency = SegmentList[i].StepFrequency,
                Maximum = info[i].Item1,
                Average = info[i].Item2,
                Threshold = info[i].Item3,
                Signals = info[i].Item4
            };
        var gps = RunningInfo.BufGpsData;
        var ew = gps.Longitude > 0 ? "E" : "W";
        var ns = gps.Latitude > 0 ? "N" : "S";
        var sendToCloud = new TemplateDataSendToCloud
        {
            EdgeId = RunningInfo.EdgeId,
            TemplateId = data.TemplateId,
            Location = $"{gps.Longitude:0.000000}{ew},{gps.Latitude:0.000000}{ns}",
            Data = data.Data,
            Parameters = _parameters.ToDictionary(p => p.Key, p => p.Value.Value),
            StartTime = Utils.GetTimestamp(_startTime),
            StopTime = Utils.GetNowTimestamp()
        };
        Trace.WriteLine($"模板采集结束,{sendToCloud.StartTime},{sendToCloud.StopTime}");
        try
        {
            await CloudClient.Instance.UpdateNsicTemplateDataAsync(sendToCloud).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"保存模板{TemplateId}失败!{ex.Message}");
            var msg = new SDataMessage
            {
                LogType = LogType.Error,
                ErrorCode = (int)InternalMessageType.Error,
                Description = ex.Message,
                Detail = ex.ToString()
            };
            SendMessage(msg);
        }

        _saveDataSign = true;
        _saveDataHandle.Set();
    }

    private async Task UpdateResultDataToCloudAsync()
    {
        if (_resultDataCache?.Length > 0)
        {
            var result = new ResultDataSendToCloud
            {
                TemplateId = TemplateId,
                Data = _resultDataCache,
                StartTime = Utils.GetTimestamp(_startTime),
                StopTime = Utils.GetNowTimestamp()
            };
            try
            {
                await CloudClient.Instance.UpdateNsicResultDataAsync(result).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var msg = new SDataMessage
                {
                    LogType = LogType.Error,
                    ErrorCode = (int)InternalMessageType.Error,
                    Description = ex.Message,
                    Detail = ex.ToString()
                };
                SendMessage(msg);
            }
        }
    }
}