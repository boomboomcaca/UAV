using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Device.G33DDC.SDK;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

public partial class G33Ddc : DeviceBase
{
    private CancellationTokenSource _cts;
    private G33DdcCommon _device;

    /// <summary>
    ///     第一包iq数据有问题，需要丢弃
    /// </summary>
    private bool _firstIfSign;

    private Task _heartbeatTask;

    public G33Ddc(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        if (!base.Initialized(device)) return false;
        try
        {
            ReleaseSource();
            _device = new G33DdcCommon();
            _device.IfDataReceived += IfDataReceived;
            _device.Ddc1DataReceived += Ddc1DataReceived;
            _device.Ddc2DataReceived += Ddc2DataReceived;
            _device.AudioDataReceived += AudioDataReceived;
            _device.Ddc1IqDataReceived += Ddc1IqDataReceived;
            _device.Ddc2PreprocessedDataReceived += Ddc2PreprocessedDataReceived;
            if (!_device.Connect())
            {
                ReleaseSource();
                return false;
            }

            _device.SetDithering(EnabledAdcDithering);
            _device.SetPreamp(EnabledPreamplifier);
            _device.SetAgcEnabled(0, true);
            IniTasks();
            return true;
        }
        catch
        {
            ReleaseSource();
            return false;
        }
    }

    public override void Start(FeatureType feature, IDataPort dataPort)
    {
        base.Start(feature, dataPort);
        StartFeature();
    }

    public override void Stop()
    {
        base.Stop();
        StopFscan();
        StopMscan();
        StopItu();
        StopDetector();
        _device?.Stop();
        _device?.PowerOff();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        _spectrumsCache?.Clear();
    }

    public override void Dispose()
    {
        ReleaseSource();
        base.Dispose();
    }

    private void UpdateDdcChannels()
    {
        if (CurFeature != FeatureType.IFMCA) return;
        if (_ddcChannels == null || (_ddcChannels.Length == 0 && TaskState == TaskState.Start))
        {
            for (uint i = 0; i < 2; i++) _device.StopDdc2(i);
            return;
        }

        for (uint i = 0; i < _ddcChannels.Length; i++)
        {
            var channel = _ddcChannels[i];
            if (channel == null) continue;
            var freq = channel.Frequency;
            // 超出中频带宽范围就不下发（防止设备频繁切换）
            if (freq <= _frequency - _ifBandwidth / 2000 || freq >= _frequency + _ifBandwidth / 2000) continue;
            var ddc2Freq = freq - _frequency;
            var mode = (DemodulatorMode)channel.DemMode;
            _device.SetDdc2Frequency(i, ddc2Freq);
            _device.SetDemMode(i, mode);
            _device.SetDemBandwidth(i, channel.FilterBandwidth);
            if (channel.SpectrumSwitch || channel.IqSwitch)
            {
                Console.WriteLine($"DDC 通道{i}开启");
                _device.StartDdc2(i);
            }

            if (channel.AudioSwitch)
            {
                _device.StartDdc2(i);
                _device.StartAudio(i);
            }
        }
    }

    private void IniTasks()
    {
        _cts = new CancellationTokenSource();
        _heartbeatTask = new Task(p => HeartbeatAsync(p).ConfigureAwait(false), _cts.Token);
        _heartbeatTask.Start();
    }

    private void ReleaseSource()
    {
        try
        {
            _cts?.Cancel();
            // _heartbeatTask.Wait();
            _heartbeatTask?.Dispose();
        }
        catch
        {
        }

        try
        {
            if (_device != null)
            {
                _device.IfDataReceived -= IfDataReceived;
                _device.Ddc1DataReceived -= Ddc1DataReceived;
                _device.Ddc2DataReceived -= Ddc2DataReceived;
                _device.AudioDataReceived -= AudioDataReceived;
                _device.Ddc1IqDataReceived -= Ddc1IqDataReceived;
                _device.Ddc2PreprocessedDataReceived -= Ddc2PreprocessedDataReceived;
            }

            _device?.Close();
        }
        catch
        {
        }
    }

    private void StartFeature()
    {
        _firstIfSign = false;
        if (!_device.PowerOn())
            //Trace.WriteLine($"任务{_curFeature}启动失败！设备打开失败");
            throw new Exception("任务启动失败！设备打开失败");
        //return;
        switch (CurFeature)
        {
            case FeatureType.FFM:
            {
                uint interval = 10;
                //if (_detector != DetectMode.FAST && _measureTime > 0)
                //{
                //    interval = (uint)_measureTime;
                //}
                if (_detector != DetectMode.Fast)
                    StartDetector();
                else
                    StopDetector();
                _device.StartDdc1(interval);
                _device.SetDdc2(0, 0, 0, 0);
                _device.StartDdc2(0);
                if (_audioSwitch)
                {
                    _device.SetDemMode(0, (DemodulatorMode)_demMode);
                    _device.SetAgcEnabled(0, true);
                    _device.StartAudio(0);
                }

                if (_ituSwitch) StartItu();
                break;
            }
            case FeatureType.SCAN:
                StartScan();
                break;
            case FeatureType.IFMCA:
            {
                _device.StartDdc1();
                UpdateDdcChannels();
            }
                break;
            case FeatureType.MScan:
                StartMscan();
                break;
        }
    }

    private async Task HeartbeatAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(1000, token).ConfigureAwait(false);
                if (G33Ddcsdk.IsDeviceConnected()) continue;
                var info = new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                };
                SendMessage(info);
                break;
            }
            catch (Exception)
            {
                await Task.Yield();
            }
    }

    #region Events

    private void AudioDataReceived(int channel, byte[] data)
    {
        try
        {
            if (!_firstIfSign)
            {
                _firstIfSign = true;
                return;
            }

            var audio = new SDataAudio
            {
                Format = AudioFormat.Pcm,
                SamplingRate = 48000,
                Channels = 1,
                BitsPerSample = 16,
                BlockAlign = 2,
                BytesPerSecond = 48000 * 2,
                Data = data
            };
            if (CurFeature != FeatureType.IFMCA)
            {
                if (!_audioSwitch) return;
                if (SquelchSwitch && _level < SquelchThreshold) return;
                if (CurFeature == FeatureType.MScan && _mscanComplete) return;
                SendData(new List<object> { audio });
            }
            else
            {
                if (_ddcChannels.Length <= channel) return;
                var info = _ddcChannels[channel];
                var level = _device.GetLevel((uint)channel);
                if (!info.AudioSwitch) return;
                if (info.SquelchSwitch && level < info.SquelchThreshold) return;
                var ddc = new SDataDdc
                {
                    ChannelNumber = channel,
                    Data = new List<object> { audio }
                };
                SendData(new List<object> { ddc });
            }
            //_count += audio.Data.Length;
            //var span = DateTime.Now.Subtract(_time).TotalMilliseconds;
            //if (span > 1000)
            //{
            //    Console.WriteLine($"间隔:{span}音频长度:{_count}");
            //    _time = DateTime.Now;
            //    _count = 0;
            //}
        }
        catch
        {
        }
    }

    private void Ddc2DataReceived(int channel, float[] data)
    {
        try
        {
            if (CurFeature != FeatureType.IFMCA) return;
            if (_ddcChannels.Length <= channel) return;
            var info = _ddcChannels[channel];
            if (!info.SpectrumSwitch) return;
            var ddc = new SDataDdc
            {
                ChannelNumber = channel,
                Data = new List<object>()
            };
            var spec = new SDataSpectrum
            {
                Frequency = _frequency,
                Span = _ifBandwidth,
                Data = Array.ConvertAll(data, item => (short)((item + 107) * 10))
            };
            ddc.Data.Add(spec);
            SendData(new List<object> { ddc });
        }
        catch
        {
        }
    }

    private float _level;

    private void Ddc1DataReceived(object sender, float[] e)
    {
        try
        {
            if (!_firstIfSign)
            {
                _firstIfSign = true;
                return;
            }

            var list = new List<object>();
            if (CurFeature == FeatureType.MScan)
            {
                var ms = ProcessMScan(e);
                if (ms?.Count > 0) list.AddRange(ms);
            }
            else if (CurFeature == FeatureType.IFMCA || (CurFeature != FeatureType.IFMCA && SpectrumSwitch))
            {
                var spec = CacheSpectrum(e);
                if (spec?.Count > 0) list.AddRange(spec);
            }

            if (list.Count > 0) SendData(list);
        }
        catch
        {
        }
    }

    private void IfDataReceived(object sender, float[] e)
    {
        try
        {
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Fscan) return;
            if (e == null || e.Length == 0) return;
            if (!_firstIfSign)
            {
                _firstIfSign = true;
                return;
            }

            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            var step = (double)Define.MaxIfFreq / e.Length;
            var offset = (int)Math.Round(StartFrequency / step * 1000000, 0);
            var data = new float[total];
            var count = total;
            if (count > e.Length - offset) count = e.Length - offset;
            Buffer.BlockCopy(e, offset * sizeof(float), data, 0, count * sizeof(float));
            if (count < total)
                for (var i = count; i < total; i++)
                    data[i] = data[count - 1];
            var scan = new SDataScan
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency,
                Offset = 0,
                Total = total,
                Data = Array.ConvertAll(data, item => (short)((item + 107) * 10))
            };
            SendData(new List<object> { scan });
        }
        catch
        {
        }
    }

    private void Ddc1IqDataReceived(short[] iqData16, int[] iqData32)
    {
        try
        {
            if (_ituSwitch)
            {
                if (iqData16 != null)
                    lock (_iqDataLock)
                    {
                        _iqDataForItu = new short[iqData16.Length];
                        Buffer.BlockCopy(iqData16, 0, _iqDataForItu, 0, iqData16.Length * sizeof(short));
                    }
                else if (iqData32 != null)
                    lock (_iqDataLock)
                    {
                        _iqDataForItu = Iq32ToIq16(iqData32);
                    }
            }

            if (!IqSwitch) return;
            var iq = new SDataIq
            {
                Timestamp = (long)Utils.GetNowTimestamp(),
                Frequency = _frequency,
                Bandwidth = _ifBandwidth,
                SamplingRate = _device?.Ddc1SampleRate ?? 0,
                Attenuation = _attenuation
            };
            if (iqData16 != null)
            {
                iq.Data16 = new short[iqData16.Length];
                Buffer.BlockCopy(iqData16, 0, iq.Data16, 0, iqData16.Length * sizeof(short));
            }
            else if (iqData32 != null)
            {
                iq.Data32 = new int[iqData32.Length];
                Buffer.BlockCopy(iqData32, 0, iq.Data32, 0, iqData32.Length * sizeof(int));
                _iqDataForItu = null;
            }

            SendData(new List<object> { iq });
        }
        catch
        {
        }
    }

    private void Ddc2PreprocessedDataReceived(uint channel, float level, float slevelPeak, float slevelRms, double gain)
    {
        if (_isInFScanMode)
        {
            ProcessFscan(level);
        }
        else if (CurFeature == FeatureType.MScan)
        {
            var list = ProcessMScan(slevelRms);
            if (list?.Count > 0) SendData(list);
        }
        else if (CurFeature is FeatureType.FFM or FeatureType.ITUM or FeatureType.SSOA)
        {
            if (float.IsFinite(level))
            {
                var list = CacheLevel(level, slevelPeak, slevelRms);
                if (list?.Count > 0) SendData(list);
            }
        }
    }

    #endregion
}