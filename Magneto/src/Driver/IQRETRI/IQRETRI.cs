using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.SignalDemod;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.IQRETRI;

public partial class Iqretri : DriverBase
{
    private readonly Dictionary<double, long> _iqSaveLenMap = new();
    private readonly Dictionary<string, ItuStatData> _ituStat = new();
    private readonly Dictionary<string, object> _parameterCache = new();
    private short _factor;
    private double _frequency;
    private IqRecorder _iqRecorder;
    private bool _isPlayback;
    private bool _isPlaybackPause;
    private float _level;
    private string _recordId = string.Empty;
    private volatile bool _running;
    private string _ssMode = string.Empty;

    public Iqretri(Guid driverId) : base(driverId)
    {
    }

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
        if (!base.Start(dataPort, mediaType)) return false;
        (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        (SsDevice as DeviceBase)?.Start(FeatureType.IQRETRI, this);
        _running = true;
        RecorderOnOrOff(IqRecordSwitch);
        return true;
    }

    public override bool Stop()
    {
        RecorderOnOrOff(false);
        _running = false;
        base.Stop();
        (Receiver as DeviceBase)?.Stop();
        (SsDevice as DeviceBase)?.Stop();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name != "receiver" && name != "ssDevice" && name != "antennaController") _parameterCache[name] = value;
        if (name == ParameterNames.Frequency)
        {
            if (AntennaController is IAntennaController antennaController
                && double.TryParse(value.ToString(), out var freq))
                antennaController.Frequency = freq;
            ClearData();
        }

        if (name == "recordId")
        {
            _recordId = value == null ? string.Empty : value.ToString();
            CheckSsMode();
        }

        if (name == "ssMode" && value != null)
        {
            _ssMode = value.ToString();
            CheckSsMode();
        }

        if (name is "iqRecordSwitch" or ParameterNames.Frequency or "frequencyOffset" or ParameterNames.Bandwidth
            or ParameterNames.IqSwitch) RecorderOnOrOff(IqRecordSwitch);
    }

    public override void OnData(List<object> data)
    {
        _iqRecorder.OnData(ref data);
        if (data.Exists(item => item is SDataLevel)) CanPause = true;
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
        if (level != null) _level = level.Data;
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
                var value = 0d;
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

        if (_isPlayback && _isPlaybackPause)
        {
            if (spectrum != null) data.Remove(spectrum);
            if (level != null) data.Remove(level);
            if (itu != null) data.Remove(itu);
            var audio = data.Find(item => item is SDataAudio);
            if (audio != null) data.Remove(audio);
        }

        var nf = data.Find(item => item is FileSavedNotification);
        if (nf is FileSavedNotification notify)
        {
            data.Remove(nf);
            var list = new List<SimpleParameter>();
            foreach (var pair in _parameterCache)
            {
                var sp = new SimpleParameter
                {
                    Name = pair.Key,
                    Value = pair.Value
                };
                list.Add(sp);
            }

            notify.Parameters = Utils.ConvertToJson(list);
            SendMessage(new List<object> { notify });
        }

        if (data.Find(item => item is string) is string str && !string.IsNullOrEmpty(str))
        {
            // 如果记录失败，这里需要解析
            data.Remove(str);
            if (!string.Equals(str, "completed", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(str, "in progress", StringComparison.OrdinalIgnoreCase))
            {
                var msg = new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = 0,
                    Description = "流盘记录结果",
                    Detail = str
                };
                data.Add(msg);
            }
        }

        SendData(data);
    }

    private void SendMessage(List<object> data)
    {
        MessageDataPort?.OnData(data);
    }

    private void CheckSsMode()
    {
        switch (_ssMode)
        {
            case "record":
                _isPlayback = false;
                _isPlaybackPause = false;
                break;
            case "playback":
                _isPlayback = true;
                _isPlaybackPause = false;
                break;
            case "drop":
                break;
            case "none":
                if (!string.IsNullOrEmpty(_recordId))
                {
                    _isPlaybackPause = true;
                }
                else
                {
                    _isPlayback = false;
                    _isPlaybackPause = false;
                }

                break;
        }
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
        SendData(new List<object>
        {
            new SDataIqRecordNotice
            {
                IqFileName = e.DataFile
            }
        });
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

                SendMessageData(new List<object>
                {
                    new SDataSignalDecodeNotice
                    {
                        Feature = FeatureType.IQRETRI,
                        FileName = fileName,
                        TaskId = TaskId
                    }
                });
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
}