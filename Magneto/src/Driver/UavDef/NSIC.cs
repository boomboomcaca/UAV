using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Driver.UavDef.NSIC;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UavDef;

public partial class UavDef
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly AutoResetEvent _saveDataHandle = new(false);
    private IDataProcess _dataProcess;
    private bool _isGetTemplateParameters;
    private SegmentResultData[] _resultDataCache;
    private bool _saveDataSign;
    private DateTime _startTime = DateTime.MinValue;

    public override bool Pause()
    {
        return false;
    }

    protected override void SetParameterInternal(string name, object value)
    {
        base.SetParameterInternal(name, value);
        switch (name)
        {
            case ParameterNames.Receiver:
            case ParameterNames.AntennaController:
                return;
            case ParameterNames.ScanSegments:
                break;
        }

        _parameters[name] = new Parameter
        {
            Name = name,
            Value = value
        };
        _dataProcess?.SetParameter(_parameters[name]);
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
            _dataProcess.DataProcessComplete -= ProcessDataComplete;
        }

        if (FunctionSwitch)
            _dataProcess = new ComparisonProcess();
        else
            _dataProcess = new TemplateProcess();
        foreach (var parameter in _parameters) _dataProcess.SetParameter(parameter.Value);
    }

    private void ProcessDataComplete(object sender, List<object> e)
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
            foreach (var pair in parameters.Where(pair => pair.Key is not ("templateID"
                         or ParameterNames.ThresholdValue
                         or "functionSwitch"
                         or ParameterNames.StartFrequency or ParameterNames.StopFrequency
                         or ParameterNames.StepFrequency)))
                SetParameter(pair.Key, pair.Value);
        }

        var info = (List<IList<CompareSignalInfo>>)result.Find(item => item is List<IList<CompareSignalInfo>>);
        if (info == null || info.Count != SegmentList.Count)
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