using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.SignalDemod;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace FixFQ;

public partial class FixFq(Guid functionId) : DriverBase(functionId)
{
    private readonly Dictionary<double, long> _iqSaveLenMap = new();
    private readonly Dictionary<string, ItuStatData> _ituStat = new();
    private Asr _asr;
    private short _factor;
    private double _frequency;
    private IqRecorder _iqRecorder;
    private float _level;
    private int _lvlReportCount;
    private float _lvlReportTotal;
    private StatFfmInfo _reportCache;

    private volatile bool _running;
    //private bool _signLevel;
    //private bool _signSpectrum;

    public override void Initialized(ModuleInfo module)
    {
        _iqRecorder = new IqRecorder();
        _iqRecorder.StorageCompleted += IQRecorder_StorageCompleted;
        base.Initialized(module);
        _iqSaveLenMap.Clear();
        if (!string.IsNullOrWhiteSpace(IqSaveLenConfig))
        {
            var lens = IqSaveLenConfig.Split(';');
            foreach (var item in lens)
            {
                var dd = item.Split('|');
                if (dd.Length != 2) continue;
                var b1 = double.TryParse(dd[0], out var bw);
                var b2 = long.TryParse(dd[1], out var count);
                if (b1 && b2) _iqSaveLenMap.Add(bw, count);
            }
        }
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        //_signLevel = false;
        //_signSpectrum = false;
        if (!base.Start(dataPort, mediaType)) return false;
        (Receiver as DeviceBase)?.Start(FeatureType.Ffm, this);
        _running = true;
        RecorderOnOrOff(IqRecordSwitch);
        // System.Threading.Tasks.Task.Run(DataProcess);
        if (_preDemMode is Modulation.Tetra or Modulation.Dmr)
        {
            base.SetParameter(ParameterNames.IqSwitch, true);
            if (Math.Abs(_ifBandwidth - 200d) < 1e-9) base.SetParameter(ParameterNames.IfBandwidth, 250d);
            (Decoder as DeviceBase)?.Start(FeatureType.Ese, this);
            _isInTetraMode = true;
        }

        // 现在暂时启动就计算占用度
        StartOcc();
        return true;
    }

    public override bool Stop()
    {
        StopOcc();
        RecorderOnOrOff(false);
        SendReportDataToCloud();
        _running = false;
        base.Stop();
        (Receiver as DeviceBase)?.Stop();
        //_signLevel = false;
        //_signSpectrum = false;
        if (_asr != null)
        {
            _asr.AudioRecogResultArrivedEvent -= AudioRecogResultArrived;
            _asr.Stop();
        }

        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.MeasureThreshold)
            // 现在禁用测量门限了
            return;
        if (name == ParameterNames.IfBandwidth)
        {
            _ifBandwidth = Convert.ToDouble(value);
            if (_isInTetraMode && Utils.IsNumberEquals(Convert.ToDouble(value), 200d)) value = 250d;
        }

        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency)
        {
            if (AntennaController is IAntennaController antennaController
                && double.TryParse(value.ToString(), out var freq))
                antennaController.Frequency = freq;
            ClearData();
        }

        if (name == ParameterNames.DemMode)
        {
            var mode = Utils.ConvertStringToEnum<Modulation>(value?.ToString());
            if (_preDemMode != mode)
            {
                _preDemMode = mode;
                if ((mode == Modulation.Tetra || _preDemMode == Modulation.Dmr) && IsTaskRunning)
                {
                    if (Decoder == null) throw new Exception("未配置相关设备，无法解调本模式");
                    base.SetParameter(ParameterNames.IqSwitch, true);
                    if (Math.Abs(_ifBandwidth - 200d) < 1e-9) base.SetParameter(ParameterNames.IfBandwidth, 250d);
                    (Decoder as DeviceBase)?.Start(FeatureType.Ese, this);
                    _isInTetraMode = true;
                }
                else
                {
                    base.SetParameter(ParameterNames.IqSwitch, _preIqSwitch);
                    _isInTetraMode = false;
                }
            }
        }

        if (name != "mrSwitch" && name != "captureThreshold")
        {
            var sign = true;
            if (name == "capture" && int.TryParse(value?.ToString(), out var val))
            {
                sign = val != _preCapture;
                _preCapture = val;
            }

            if (sign)
                lock (_lockCapture)
                {
                    _captureCache.Clear();
                    _lastCaptureTime = DateTime.MinValue;
                    _captureOk = false;
                }
        }

        if (name is "iqRecordSwitch" or ParameterNames.Frequency or ParameterNames.Bandwidth or ParameterNames.IqSwitch)
            RecorderOnOrOff(IqRecordSwitch);
        if (name == ParameterNames.IqSwitch && bool.TryParse(value?.ToString(), out var bl)) _preIqSwitch = bl;
        if (name is ParameterNames.Frequency or ParameterNames.SquelchThreshold) ClearOcc();
        if (name == ParameterNames.SquelchThreshold && int.TryParse(value?.ToString(), out var thd))
            MeasureThreshold = thd;
        if (name is ParameterNames.Frequency
            or ParameterNames.IfBandwidth
            or ParameterNames.FilterBandwidth)
            ClearReport();
        if (name == "asrSwitch")
        {
            if (AsrSwitch)
            {
                if (_asr != null)
                {
                    _asr.AudioRecogResultArrivedEvent -= AudioRecogResultArrived;
                    _asr.Stop();
                }

                _asr = new Asr();
                _asr.AudioRecogResultArrivedEvent += AudioRecogResultArrived;
                _asr.Start(16000, 16, 1);
            }
            else if (_asr != null)
            {
                _asr.AudioRecogResultArrivedEvent -= AudioRecogResultArrived;
                _asr.Stop();
            }
        }
        // if (name == "measureSwitch")
        // {
        //     if (_measureSwitch)
        //     {
        //         StartOcc();
        //     }
        //     else
        //     {
        //         StopOcc();
        //     }
        // }
    }

    /// <summary>
    ///     重置报表数据
    /// </summary>
    private void ClearReport()
    {
        _reportCache.MaxLevel = 0;
        _reportCache.AveLevel = 0;
        _reportCache.MaxFieldStrength = 0;
        _reportCache.AveFieldStrength = 0;
        _lvlReportTotal = 0f;
        _lvlReportCount = 0;
    }

    /// <summary>
    ///     更新报表数据
    /// </summary>
    private void UpdateReport()
    {
        _lvlReportCount++;
        _lvlReportTotal += _level;
        if (_reportCache.MaxLevel < _level) _reportCache.MaxLevel = _level;
        _reportCache.AveLevel = _lvlReportTotal / _lvlReportCount;
        var factor = _factor / 10f;
        _reportCache.MaxFieldStrength = _reportCache.MaxLevel + factor;
        _reportCache.AveFieldStrength = _reportCache.AveLevel + factor;
    }

    public override void OnData(List<object> data)
    {
        _iqRecorder.OnData(ref data);
        // if (data.Exists(item => item is SDataLevel))
        // {
        //     _signLevel = true;
        // }
        if (data.Exists(item => item is SDataSpectrum))
            // _signSpectrum = true;
            CanPause = true;
        // if (_signLevel && _signSpectrum && !_canPause)
        // {
        //     Console.WriteLine("ffm可以被暂停");
        //     _canPause = true;
        // }
        var spectrum = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        if (spectrum != null && (!Utils.IsNumberEquals(_frequency, spectrum.Frequency) || AntennaChanged)
                             && AntennaController is IAntennaController antennaController
           )
        {
            _frequency = spectrum.Frequency;
            _factor = antennaController.GetFactor(_frequency);
            var factor = new SDataFactor
            {
                Data = new short[1]
            };
            factor.Data[0] = _factor;
            data.Add(factor);
            _ituStat.Clear();
            // Console.WriteLine($"天线因子修改为:{_factor}");
            AntennaChanged = false;
        }

        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        if (level != null)
        {
            _level = level.Data;
            UpdateReport();
            UpdateOcc(level);
        }

        var itu = (SDataItu)data.Find(item => item is SDataItu);
        if (itu != null)
        {
            itu.FieldStrength = _level + _factor / 10f;
            var misc = itu.Misc ?? new Dictionary<string, object>();
            misc.TryAdd(ParameterNames.ItuLevel, _level);
            misc[ParameterNames.ItuStrength] = itu.FieldStrength;
            misc.TryAdd(ParameterNames.ItuFrequency, itu.Frequency);
            foreach (var pair in misc)
            {
                if (pair.Value == null) continue;
                var obj = pair.Value;
                double value;
                if (obj is ItuStatData s)
                    value = s.Value;
                else if (double.TryParse(obj.ToString(), out var db))
                    value = db;
                else
                    continue;
                var round = 2;
                if (pair.Key == ParameterNames.ItuFrequency) round = 6;
                UpdateItuStat(pair.Key, value, round);
            }

            misc = _ituStat.ToDictionary(p => p.Key, p => (object)p.Value.ToDictionary());
            itu.Misc = misc;
        }

        UpdateCapture(data, spectrum);
        if (_isInTetraMode)
        {
            if (Math.Abs(spectrum!.Span - 250d) < 1e-9) spectrum.Span = 200d;
            if (data.Find(item => item is SDataIq) is SDataIq iq)
            {
                data.Remove(iq);
                (Decoder as IDataPort)?.OnData([iq]);
            }

            if (data.Find(item => item is SDataEseResult) is SDataEseResult res)
            {
                data.Remove(res);
                if (!string.IsNullOrEmpty(res.Decoder))
                {
                    var sms = new SDataSms
                    {
                        Frequency = res.Frequency,
                        Text = res.Decoder
                    };
                    data.Add(sms);
                    Console.WriteLine($"解码结果:{sms.Text}");
                }
            }
        }

        if (data.Find(item => item is SDataIq) is SDataIq iqd) UpdateIqDraw(iqd);
        if (data.Find(item => item is SDataAudio) is SDataAudio audio) _asr?.AddAudio(audio);
        SendData(data);
    }

    private void UpdateCapture(List<object> data, SDataSpectrum spectrum)
    {
        if (Capture == 1 && spectrum?.Data?.Length > 0)
        {
            // var idx = new List<int>();
            // for (int i = 0; i < spectrum.Data.Length; i++)
            // {
            //     if (spectrum.Data[i] > _captureThreshold)
            //     {
            //         idx.Add(i);
            //     }
            // }
            // var sign = spectrum.Data.Max() >= _captureThreshold;
            var sign = false;
            for (var i = 0; i < spectrum.Data.Length; i++)
            {
                if (CaptureThreshold == null) break;
                var thresold = CaptureThreshold.Length <= i ? CaptureThreshold[^1] : CaptureThreshold[i];
                if (spectrum.Data[i] > thresold * 10)
                {
                    sign = true;
                    break;
                }
            }

            if (sign)
            {
                _lastCaptureTime = Utils.GetNowTime();
                if (!_captureOk)
                {
                    var cs = new CaptureStruct
                    {
                        StartTime = Utils.GetNowTimestamp(),
                        StopTime = 0
                    };
                    lock (_lockCapture)
                    {
                        _captureCache.Add(cs);
                        var capture = new SDataCapture
                        {
                            Frequency = spectrum.Frequency,
                            Data = _captureCache.ToList()
                        };
                        Console.WriteLine($"捕获到信号:{_lastCaptureTime}");
                        data.Add(capture);
                    }

                    _captureOk = true;
                }
            }
            else if (_captureOk && Utils.GetNowTime().Subtract(_lastCaptureTime).TotalSeconds >= 1)
            {
                lock (_lockCapture)
                {
                    if (_captureCache.Count > 0 && _captureCache.Last().StartTime > 0 &&
                        _captureCache.Last().StopTime == 0)
                    {
                        var cs = _captureCache[^1];
                        cs.StopTime = Utils.GetTimestamp(_lastCaptureTime);
                        _captureCache[^1] = cs;
                    }

                    var capture = new SDataCapture
                    {
                        Frequency = spectrum.Frequency,
                        Data = _captureCache.ToList()
                    };
                    data.Add(capture);
                }

                _captureOk = false;
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _iqRecorder.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     更新ITU统计数据
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="round">保留的小数位数</param>
    private void UpdateItuStat(string name, double value, int round)
    {
        if (Math.Abs(value - double.MinValue) < 1e-9) return;
        if (!_ituStat.ContainsKey(name))
        {
            var nu = Utils.ConvertNameAndUnit(name);
            _ituStat.Add(name, new ItuStatData(nu.Item1, nu.Item2));
        }

        var obj = _ituStat[name];
        if (obj == null)
        {
            var nu = Utils.ConvertNameAndUnit(name);
            _ituStat[name] = new ItuStatData(nu.Item1, nu.Item2);
        }

        value = Math.Round(value, round);
        _ituStat[name].Count++;
        _ituStat[name].Value = value;
        var max = _ituStat[name].Max;
        var min = _ituStat[name].Min;
        var avg = _ituStat[name].Avg;
        var rms = _ituStat[name].Rms;
        var cnt = _ituStat[name].Count;
        // max
        _ituStat[name].Max = Math.Max(value, max);
        // min
        _ituStat[name].Min = Math.Min(value, min);
        // avg
        var avgNew = (avg * (cnt - 1) + value) / cnt;
        _ituStat[name].Avg = Math.Round(avgNew, round);
        // rms
        var rmsNew = Math.Sqrt((rms * rms * (cnt - 1) + value * value) / cnt);
        _ituStat[name].Rms = Math.Round(rmsNew, round);
    }

    private void IQRecorder_StorageCompleted(object sender, StorageCompletedEventArg e)
    {
        if (e == null || string.IsNullOrWhiteSpace(e.DataFile)) return;
        SendData([
            new SDataIqRecordNotice
            {
                IqFileName = e.DataFile
            }
        ]);
        _ = Task.Run(() =>
        {
            try
            {
                var dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2", PublicDefine.PathSgldec);
                if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
                var fileName = Path.GetFileName(e.DataFile);
                {
                    var dataFileName = Path.Combine(dataPath, fileName);
                    if (File.Exists(dataFileName)) File.Delete(dataFileName);
                    File.Copy(e.DataFile, dataFileName);
                }

                SendMessageData([
                    new SDataSignalDecodeNotice
                    {
                        Feature = FeatureType.Ffm,
                        FileName = fileName,
                        TaskId = TaskId
                    }
                ]);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"复制文件异常，异常信息：{ex}");
            }
        });
    }

    private void RecorderOnOrOff(bool iqRecordSwitch)
    {
        if (!_running) return;
        if (Receiver is not DeviceBase device) return;
        var iqSwitch = device.GetParameter<bool>(ParameterNames.IqSwitch);
        if (iqRecordSwitch && iqSwitch)
        {
            var bw = device.GetParameter<double>(ParameterNames.IfBandwidth);
            _iqRecorder.Start(RunningInfo.EdgeId, _iqSaveLenMap, bw, IqCalibrationValue);
        }
        else
        {
            _iqRecorder.Stop();
        }
    }

    /// <summary>
    ///     任务停止时向云端发送报表所需数据
    /// </summary>
    private void SendReportDataToCloud()
    {
        var data = new SDataFfmStat
        {
            TaskId = TaskId,
            Data = new StatFfmInfo
            {
                MaxLevel = _reportCache.MaxLevel,
                AveLevel = _reportCache.AveLevel,
                MaxFieldStrength = _reportCache.MaxFieldStrength,
                AveFieldStrength = _reportCache.AveFieldStrength
            }
        };
        SendMessageData([data]);
    }

    private void AudioRecogResultArrived(object sender, SDataAudioRecognition e)
    {
        Console.WriteLine($"翻译最终结果:{e.Message},关键字:{string.Join(",", e.Keywords)}");
        SendData([e]);
    }

    #region capture

    private readonly List<CaptureStruct> _captureCache = [];
    private readonly object _lockCapture = new();
    private DateTime _lastCaptureTime = DateTime.MinValue;
    private bool _captureOk;
    private int _preCapture;
    private Modulation _preDemMode = Modulation.None;
    private bool _isInTetraMode;
    private double _ifBandwidth;
    private bool _preIqSwitch;

    #endregion

    #region 占用度测量

    /// <summary>
    ///     上次发送占用度数据的时间
    /// </summary>
    private DateTime _preSendOccTime = DateTime.Now;

    /// <summary>
    ///     获取到电平值的数量
    /// </summary>
    private long _levelCount;

    /// <summary>
    ///     超过门限的电平值的数量
    /// </summary>
    private long _overLevelCount;

    private bool _occSign;
    private CancellationTokenSource _cts;
    private Task _occTask;
    private readonly ConcurrentQueue<float> _levelCache = new();

    private void ClearOcc()
    {
        _occSign = false;
        _preSendOccTime = DateTime.Now;
        _levelCount = 0;
        _overLevelCount = 0;
        _levelCache.Clear();
        _occSign = true;
    }

    private void StopOcc()
    {
        _occSign = false;
        _levelCache.Clear();
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }

        try
        {
            _occTask?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    private void StartOcc()
    {
        _occSign = true;
        _cts = new CancellationTokenSource();
        _occTask = Task.Run(() => ProcessOccupancyAsync(_cts.Token));
    }

    private void UpdateOcc(SDataLevel level)
    {
        if (!_occSign) return;
        _levelCache.Enqueue(level.Data);
    }

    private async Task ProcessOccupancyAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                if (_levelCache.IsEmpty)
                {
                    await Task.Delay(100, token).ConfigureAwait(false);
                    continue;
                }

                if (!_levelCache.TryDequeue(out var data))
                {
                    await Task.Delay(100, token).ConfigureAwait(false);
                    continue;
                }

                _levelCount++;
                if (data > MeasureThreshold) _overLevelCount++;
                if (DateTime.Now.Subtract(_preSendOccTime).TotalSeconds > 5)
                {
                    _preSendOccTime = DateTime.Now;
                    var rate = (double)_overLevelCount / _levelCount * 100d;
                    SDataOccupancy occ = new()
                    {
                        TotalOccupancy = Math.Round(rate, 2),
                        Data = null
                    };
                    SendData([occ]);
                }
            }
            catch
            {
                // ignored
            }
    }

    #endregion

    #region IQ图绘制

    private DateTime _preIqTime = DateTime.Now;

    private void UpdateIqDraw(SDataIq iq)
    {
        if (iq == null) return;
        if (DateTime.Now.Subtract(_preIqTime).TotalMilliseconds < 1000) return;
        _preIqTime = DateTime.Now;
        var len = 512;
        int[] iData;
        int[] qData;
        if (iq.Data16 != null)
        {
            if (len > iq.Data16.Length / 2) len = iq.Data16.Length / 2;
            iData = new int[len];
            qData = new int[len];
            for (var i = 0; i < len; i++)
            {
                iData[i] = iq.Data16[2 * i];
                qData[i] = iq.Data16[2 * i + 1];
            }
        }
        else if (iq.Data32 != null)
        {
            if (len > iq.Data32.Length / 2) len = iq.Data32.Length / 2;
            iData = new int[len];
            qData = new int[len];
            for (var i = 0; i < len; i++)
            {
                iData[i] = iq.Data32[2 * i];
                qData[i] = iq.Data32[2 * i + 1];
            }
        }
        else
        {
            return;
        }

        SDataIqConstellations data = new()
        {
            Frequency = iq.Frequency,
            Bandwidth = iq.Bandwidth,
            Timestamp = iq.Timestamp,
            SamplingRate = iq.SamplingRate,
            Attenuation = iq.Attenuation,
            Data = iData,
            QData = qData
        };
        SendData([data]);
    }

    #endregion
}