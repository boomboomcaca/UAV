using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Audio;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ESE;

public partial class Ese : ScanBase
{
    private readonly Dictionary<string, Parameter> _parameters = new();
    private readonly AutoResetEvent _saveDataHandle = new(false);
    private IDataProcess _dataProcess;
    private bool _isGetTemplateParameters;

    /// <summary>
    ///     当前正在进行解码的中心频率
    /// </summary>
    private double _nowDecodingFrequency;

    private EseSegmentResultData[] _resultDataCache;
    private bool _saveDataSign;
    private DateTime _startTime = DateTime.MinValue;
    private Guid _taskId;

    public Ese(Guid driverId) : base(driverId)
    {
        IsSupportMultiSegments = true;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _taskId = dataPort.TaskId;
        if (_startTime == DateTime.MinValue) _startTime = Utils.GetNowTime().ToUniversalTime();
        SetFunctionSwitch();
        if (_dataProcess != null)
        {
            _dataProcess.DataProcessComplate += ProcessDataComplate;
            _dataProcess.Start();
        }

        StartMultiSegments();
        if (FunctionSwitch)
        {
            _cts = new CancellationTokenSource();
            _taskArray = new[]
            {
                new Task(p => SignalDecodeAsync(p).ConfigureAwait(false), _cts.Token),
                new Task(p => SignalDecodeProcessAsync(p).ConfigureAwait(false), _cts.Token),
                new Task(p => QueryWhiteListAsync(p).ConfigureAwait(false), _cts.Token)
            };
            Array.ForEach(_taskArray, item => item.Start());
        }

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
        IsTaskRunning = false;
        _isGetTemplateParameters = false;
        CanPause = false;
        if (_dataProcess != null)
        {
            _dataProcess.Stop();
            _dataProcess.DataProcessComplate -= ProcessDataComplate;
        }

        if (FunctionSwitch)
        {
            _isDecoding = false;
            _decodeAutoResetEvent.Set();
            _iqDataCache.Clear();
            _ = Task.Run(async () =>
            {
                await UpdateResultDataToCloudAsync();
                _saveDataSign = true;
                _saveDataHandle.Set();
            });
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}的比对数据超时!");
            _cts?.Cancel();
            try
            {
                Task.WhenAll(_taskArray).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }

            (Analyzer as DeviceBase)?.Stop();
            (Decoder as DeviceBase)?.Stop();
        }
        else
        {
            _saveDataHandle.WaitOne(5000);
            if (!_saveDataSign) Trace.WriteLine($"保存模板{TemplateId}超时!");
        }

        (Receiver as DeviceBase)?.Stop();
        SegmentList = null;
        StopSaveAudio();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        if (name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.StepFrequency) return;
        SetParameterInternal(name, value);
        if (name == ParameterNames.ThresholdValue)
        {
            _isThresholdChanged = true;
            lock (_lockSignals)
            {
                _signalsForDecode?.Clear();
            }

            AutoResetEvent.Set();
        }
    }

    protected override void StartDevice()
    {
        // Console.WriteLine("设备启动");
        var dev = Receiver as DeviceBase;
        dev?.Start(FeatureType.SCAN, this);
    }

    protected override void StopDevice()
    {
        // Console.WriteLine("设备停止");
        var dev = Receiver as DeviceBase;
        dev?.Stop();
    }

    protected override void SetParameterInternal(string name, object value)
    {
        base.SetParameterInternal(name, value);
        if (name != ParameterNames.Receiver && name != "analyzer" && name != "decoder" &&
            name != ParameterNames.AntennaController)
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
            if (FunctionSwitch)
            {
                var eses = data.Where(item => item is SDataEseResult);
                foreach (var ese in eses)
                {
                    var tmp = (SDataEseResult)ese;
                    var s = _signalsForDecode?.Find(item =>
                        Math.Abs(item.Frequency - ((SDataEseResult)ese).Frequency) < 1e-9);
                    if (s != null && Math.Abs(s.Frequency - _nowDecodingFrequency) > 1e-9) _currentMessageCache.Clear();
                    if (s is { IsDecodeComplete: false })
                    {
                        s.IsDecodeComplete = true;
                        s.Result = tmp;
                    }

                    if (!_currentMessageCache.ContainsKey(tmp.System))
                        _currentMessageCache.Add(tmp.System, new List<string>());
                    _currentMessageCache[tmp.System].Add(tmp.Decoder);
                }

                data.RemoveAll(item => item is SDataEseResult);
                DecodeSignal(data);
                if (data.Find(item => item is SDataSpectrum) is SDataSpectrum spec) _dataCache[spec.Frequency] = spec;
                if (data.Count > 0 && !data.Exists(item => item is SDataScan) && _comparisonCount >= 2)
                    base.SendData(data);
                if (_decodeMode && _comparisonCount >= 2) return;
            }

            if (data == null) return;
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

            SendDataWithSpan(data);
        }
        catch
        {
            // 容错代码
        }
    }

    protected override void SendData(List<object> data)
    {
        _dataProcess?.OnData(data);
        base.SendData(data);
    }

    private void SetFunctionSwitch()
    {
        if (_dataProcess != null)
        {
            _dataProcess.Stop();
            _dataProcess.DataProcessComplate -= ProcessDataComplate;
        }

        if (FunctionSwitch)
        {
            _dataProcess = new ComparisonProcess();
            _decodeMode = Receiver.Id == Analyzer.Id;
        }
        else
        {
            _dataProcess = new TemplateProcess();
        }

        foreach (var parameter in _parameters) _dataProcess.SetParameter(parameter.Value);
    }

    private void ProcessDataComplate(object sender, List<object> e)
    {
        _ = FunctionSwitch
            ? Task.Run(() => ComparisonDataProcess(e))
            : Task.Run(async () => await TemplateDataProcessAsync(e));
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
                    or ParameterNames.StartFrequency or ParameterNames.StopFrequency or "decodeReceiver" or "setSimData"
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
                Results = new List<FrequencyResult>()
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
                lock (_lockSignals)
                {
                    if (!_signalsForDecode.Exists(item => Math.Abs(item.Frequency - cr.Frequency) < 1e-9))
                    {
                        var s = new SignalsInfo
                        {
                            SegmentIndex = i,
                            Frequency = cr.Frequency,
                            Bandwidth = cr.Bandwidth,
                            IsActive = cr.IsLunching,
                            IsDecodeComplete = false
                            // IsWhiteSignal = cr.IsWhiteSignal
                        };
                        _signalsForDecode.Add(s);
                    }
                    else
                    {
                        var s = _signalsForDecode.Find(item => Math.Abs(item.Frequency - cr.Frequency) < 1e-9);
                        s.IsActive = cr.IsLunching;
                        // s.IsWhiteSignal = cr.IsWhiteSignal;
                    }
                }
            }
        }

        _resultDataCache = data.Data.Select(item => new EseSegmentResultData
            {
                SegmentIndex = item.SegmentIndex,
                Results = item.Results.ConvertAll(r => new EseFrequencyResult
                {
                    FrequencyIndex = r.FrequencyIndex,
                    Frequency = r.Frequency,
                    Bandwidth = r.Bandwidth,
                    FirstTime = r.FirstTime,
                    LastTime = r.LastTime,
                    MaxLevel = r.MaxLevel,
                    AvgLevel = r.AvgLevel,
                    IsActive = r.IsActive,
                    Result = r.Result,
                    Name = r.Name
                })
            })
            .ToArray();
        _comparisonCount++;
        if (_decodeMode)
        {
            if (_comparisonCount == 2)
            {
                _isDecoding = false;
                _decodeAutoResetEvent.Set();
            }

            if (_comparisonCount > 2) return;
        }
        else
        {
            _decodeAutoResetEvent.Set();
        }

        base.SendData(new List<object> { data });
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
        // Trace.WriteLine($"模板采集结束,{sendToCloud.StartTime},{sendToCloud.StopTime}");
        try
        {
            await CloudClient.Instance.UpdateEseTemplateDataAsync(sendToCloud).ConfigureAwait(false);
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

        _saveDataSign = true;
        _saveDataHandle.Set();
    }

    private async Task UpdateResultDataToCloudAsync()
    {
        if (_resultDataCache?.Length > 0)
        {
            _signalsForDecode?.ForEach(item =>
            {
                var seg = item.SegmentIndex;
                if (_resultDataCache.Length > seg)
                {
                    var res = _resultDataCache[seg].Results.Find(f => Math.Abs(f.Frequency - item.Frequency) < 1e-9);
                    _resultDataCache[seg].Results.Remove(res);
                    var list = new List<object> { item.Result };
                    if (_dataCache.TryGetValue(res.Frequency, out var value)) list.Add(value);
                    res.Decoder = Utils.ConvertToMessagePackJson(list);
                    _resultDataCache[seg].Results.Add(res);
                }
            });
            var result = new EseResultDataSendToCloud
            {
                TemplateId = TemplateId,
                Data = _resultDataCache,
                StartTime = Utils.GetTimestamp(_startTime),
                StopTime = Utils.GetNowTimestamp()
            };
            try
            {
                await CloudClient.Instance.UpdateEseResultDataAsync(result);
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

    protected override bool CanChangeSegments()
    {
        if (FunctionSwitch)
        {
            if (!_isGetTemplateParameters) return false;
            if (_decodeMode && _isThresholdChanged)
            {
                // 在解码过程中如果修改了阈值，需要将解码过程初始化
                _isThresholdChanged = false;
                _comparisonCount = 0;
                return true;
            }

            if (_decodeMode && _comparisonCount >= 2) return false;
        }

        return base.CanChangeSegments();
    }

    private void SaveAudio(SDataAudio audio)
    {
        if (_nowDecodingFrequency == 0) return;
        if (_audioSave == null)
        {
            _audioSave = new AudioDataSave();
            var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
            var folder = Path.Combine(_startTime.ToString("yyyyMMdd"), _taskId.ToString());
            var fileName = ((long)(_nowDecodingFrequency * Math.Pow(10, 6))).ToString();
            _audioSave.SaveStart(rootPath, folder, fileName, _nowDecodingFrequency, audio.Channels, audio.SamplingRate,
                audio.BitsPerSample);
            var notify = new FileSavedNotification
            {
                NotificationType = FileNotificationType.Created,
                TaskId = _taskId.ToString(),
                DriverId = DriverId.ToString(),
                PluginId = "",
                Parameters = "",
                RootPath = _audioSave.RootPath.Replace('\\', '/'),
                RelativePath = _audioSave.RelativePath.Replace('\\', '/'),
                ComputerId = RunningInfo.ComputerId,
                FileName = _audioSave.FileName,
                DataType = FileDataType.Wav,
                BeginRecordTime = Utils.GetTimestamp(_audioSave.StartTime),
                EndRecordTime = null
            };
            SendMessage(new List<object> { notify });
        }

        _audioSave.SaveData(audio.Data);
    }

    private void StopSaveAudio()
    {
        if (_audioSave == null) return;
        _audioSave.SaveComplete();
        var notify = new FileSavedNotification
        {
            NotificationType = FileNotificationType.Modified,
            TaskId = _taskId.ToString(),
            DriverId = DriverId.ToString(),
            PluginId = "",
            RootPath = _audioSave.RootPath.Replace('\\', '/'),
            RelativePath = _audioSave.RelativePath.Replace('\\', '/'),
            ComputerId = RunningInfo.ComputerId,
            FileName = _audioSave.FileName,
            DataType = FileDataType.Wav,
            Parameters = "",
            BeginRecordTime = Utils.GetTimestamp(_audioSave.StartTime),
            EndRecordTime = Utils.GetTimestamp(_audioSave.StopTime),
            LastModifiedTime = Utils.GetTimestamp(_audioSave.StopTime),
            RecordCount = _audioSave.RecordCount,
            Size = _audioSave.Size
        };
        SendMessage(new List<object> { notify });
        var freq = _audioSave.Frequency;
        var item = _signalsForDecode?.Find(p => Math.Abs(p.Frequency - freq) < 1e-9);
        if (item != null)
            item.Result.AudioFile =
                $"/{Path.Combine(_audioSave.RelativePath, $"{_audioSave.FileName}.wav").Replace('\\', '/')}";
        _audioSave.Dispose();
        _audioSave = null;
    }

    private void SendMessage(List<object> data)
    {
        MessageDataPort?.OnData(data);
    }

    #region 解码相关

    //private List<EseWhiteListFromCloud> _whiteList;

    /// <summary>
    ///     解码模式
    ///     false:异步解码，新信号截获的接收机与解码接收机配置为不同的设备时可以异步解码，两个任务同时运行
    ///     true:顺序解码，两个接收机配置为相同的设备时，由于存在设备争用，因此需要先扫描新信号，再进行新信号的解码工作，任务不能同时运行
    ///     如果为顺序解码，需要每出两次信号比对结果就进行一次解码动作（只对新产生的信号进行解码，已经解码完毕的不再进行解码）
    /// </summary>
    private bool _decodeMode;

    /// <summary>
    ///     比对结果的个数
    ///     先进行频段扫描，每次扫描出比对结果本属性+1，当本属性=2时进行信号解码
    /// </summary>
    private int _comparisonCount;

    private readonly List<SignalsInfo> _signalsForDecode = new();
    private readonly Dictionary<double, SDataSpectrum> _dataCache = new();
    private readonly Dictionary<string, List<string>> _currentMessageCache = new();
    private readonly object _lockSignals = new();

    private readonly AutoResetEvent _decodeAutoResetEvent = new(false);

    // private readonly bool _decodeComplete = false;
    private Task[] _taskArray;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<SDataIq> _iqDataCache = new();
    private bool _isThresholdChanged;
    private AudioDataSave _audioSave;

    #endregion

    #region 信号解码

    /// <summary>
    ///     指示当前正在进行解码
    /// </summary>
    private bool _isDecoding;

    /// <summary>
    ///     指示当前解码设备在运行中
    /// </summary>
    private bool _isDecodeDeviceRunning;

    private async Task SignalDecodeAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(0, token);
            _decodeAutoResetEvent.WaitOne();
            if (!IsTaskRunning) break;
            if (_isDecoding) continue;
            try
            {
                var dev = Analyzer as DeviceBase;
                if (_isDecodeDeviceRunning)
                {
                    Console.WriteLine($"解码 {_nowDecodingFrequency} 停止");
                    dev.Stop();
                    (Decoder as DeviceBase)?.Stop();
                    _isDecodeDeviceRunning = false;
                }

                StopSaveAudio();
                var list = _signalsForDecode?.Where(item => !item.IsDecodeComplete && !item.IsWhiteSignal).ToList();
                if (list == null || list.Count == 0)
                {
                    if (_decodeMode && _comparisonCount >= 2)
                    {
                        _nowDecodingFrequency = 0;
                        _comparisonCount = 0;
                        // Thread.Sleep(31); // 添加这条延时防止抽点
                        await Task.Delay(31, token);
                        var complete = new SDataSpectrum
                        {
                            Frequency = -1,
                            Span = -1,
                            Data = Array.Empty<short>()
                        };
                        base.SendData(new List<object> { complete });
                        AutoResetEvent.Set();
                    }

                    continue;
                }

                var tmp = list.Find(item => Math.Abs(item.Frequency - PriorityFrequency) < 1e-9);
                if (tmp != null)
                {
                    list.Remove(tmp);
                    list.Insert(0, tmp);
                }

                var s = list[0];
                _nowDecodingFrequency = s.Frequency;
                _iqDataCache.Clear();
                UpdateAntennaControllerFrequency(s.Frequency);
                dev.SetParameter(ParameterNames.IqSwitch, true);
                dev.SetParameter(ParameterNames.AudioSwitch, false);
                dev.SetParameter(ParameterNames.SpectrumSwitch, true);
                dev.SetParameter(ParameterNames.Frequency, s.Frequency);
                // dev.SetParameter(ParameterNames.IfBandwidth, s.Bandwidth);
                dev.SetParameter(ParameterNames.IfBandwidth, IfBandwidthEse);
                dev.Start(FeatureType.FFM, this);
                (Decoder as DeviceBase)?.Start(FeatureType.ESE, this);
                _isDecoding = true;
                _isDecodeDeviceRunning = true;
                Console.WriteLine($"解码 {_nowDecodingFrequency} 启动");
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                Trace.WriteLine(msg);
            }
        }
    }

    private void DecodeSignal(List<object> data)
    {
        if (data.Find(item => item is SDataAudio) is SDataAudio audio)
        {
            data.Remove(audio);
            SendData(new List<object> { audio });
            SaveAudio(audio);
        }

        if (data.Find(item => item is SDataIq) is SDataIq iq)
        {
            if (Math.Abs(iq.Frequency - _nowDecodingFrequency) > 1e-9) return;
            data.Remove(iq);
            _iqDataCache.Enqueue(iq);
        }
    }

    private async Task SignalDecodeProcessAsync(object obj)
    {
        var token = (CancellationToken)obj;
        var count = 0;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(0, token);
            if (_iqDataCache.IsEmpty) continue;
            if (!_iqDataCache.TryDequeue(out var iq)) continue;
            if (iq != null)
            {
                if (Math.Abs(iq.Frequency - _nowDecodingFrequency) > 1e-9)
                {
                    count = 0;
                    continue;
                }

                count++;
                if (count < 1000)
                {
                    (Decoder as IDataPort)?.OnData(new List<object> { iq });
                }
                else
                {
                    SignalsInfo s = null;
                    lock (_lockSignals)
                    {
                        s = _signalsForDecode?.Find(item => Math.Abs(item.Frequency - iq.Frequency) < 1e-9);
                        if (s is { IsDecodeComplete: false })
                        {
                            var res = new SDataEseResult
                            {
                                Frequency = iq.Frequency,
                                Decoder = "解析失败",
                                Result = true
                                // Audio = null
                            };
                            s.Result = res;
                            s.IsDecodeComplete = true;
                        }
                        else
                        {
                            var sb = new StringBuilder();
                            sb.Append("制式: ");
                            sb.AppendJoin(";", _currentMessageCache.Keys);
                            sb.AppendLine();
                            sb.Append("消息: ");
                            foreach (var item in _currentMessageCache.Values)
                            foreach (var str in item)
                            {
                                if (string.IsNullOrEmpty(str)) continue;
                                sb.Append(str);
                                sb.AppendLine();
                            }

                            sb.AppendLine();
                            if (s != null) s.Result.Decoder = sb.ToString();
                        }

                        _nowDecodingFrequency = 0;
                    }

                    count = 0;
                    SendData(new List<object> { s?.Result });
                    _isDecoding = false;
                    _decodeAutoResetEvent.Set();
                }
            }
        }
    }

    private async Task QueryWhiteListAsync(object obj)
    {
        var token = (CancellationToken)obj;
        var lastTime = DateTime.Now;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            if (DateTime.Now.Subtract(lastTime).TotalSeconds > 10)
                try
                {
                    var list = await CloudClient.Instance.GetEseWhiteListAsync() ?? new List<EseWhiteListFromCloud>();
                    lastTime = DateTime.Now;
                    lock (_lockSignals)
                    {
                        if (_signalsForDecode != null)
                            foreach (var item in _signalsForDecode)
                            {
                                var find = list.Find(p => Math.Abs(p.Frequency - item.Frequency) < 1e-9);
                                item.IsWhiteSignal = Math.Abs(find.Frequency - item.Frequency) < 1e-9;
                            }
                    }
                }
                catch
                {
                }
        }
    }

    #endregion
}

internal class SignalsInfo
{
    /// <summary>
    ///     频段号
    /// </summary>
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     中心频率
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    ///     估测带宽
    /// </summary>
    public double Bandwidth { get; set; }

    /// <summary>
    ///     是否活跃
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    ///     是否解码完毕
    /// </summary>
    public bool IsDecodeComplete { get; set; }

    /// <summary>
    ///     是否为白名单信号
    /// </summary>
    public bool IsWhiteSignal { get; set; }

    /// <summary>
    ///     解码结果
    /// </summary>
    public SDataEseResult Result { get; set; }

    public SDataSpectrum Spectrum { get; set; }
}