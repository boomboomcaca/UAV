using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

public partial class Ap510
{
    #region 监测数据分发

    /// <summary>
    ///     分发（推送到事件注册者）数据
    /// </summary>
    private void DispatchData()
    {
        while (_dataCts?.IsCancellationRequested == false)
            try
            {
                _serviceCallEvent.WaitOne();
                var b = _dataCache.TryDequeue(out var data);
                if (!b || data is not ScpiMapData mapData)
                {
                    Thread.Sleep(50);
                    continue;
                }

                /* 发送数据之前先判断任务号是否为空，
                 * 一方面，如果任务号为空，发送数据没有意义，
                 * 另一方面，在任务还没有正式下发到设备，但任务已注册了数据接收事件(当前正是这种情况)，
                 * 这可能导致上一次任务遗留的数据（TCP连接特性的原因）错误的发送给新任务
                 * 任务结束时设置stoptag，保证其在任何条件下都不再回传数据
                 */
                var taskId = data.GetValueByKey("taskid");
                if (!string.IsNullOrWhiteSpace(_taskId) && taskId == _taskId && TaskState == TaskState.Start)
                    // 引发数据到达事件
                    OnDataArrive(mapData.Content);
            }
            catch (IOException ex)
            {
                Trace.WriteLine($"操作失败，设备不可读写，网络中断或设备已关闭，异常信息：{ex}");
                break;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"分发数据异常，异常信息：{ex}");
            }
    }

    #endregion

    #region 音频数据捕获

    /// <summary>
    ///     采集音频数据，独立的线程调度函数
    /// </summary>
    private void CaptureAudio()
    {
        // 绑定UDP端口，接收来自远程节点的访问
        // 创建发送音频的远程节点，并设置为任意节点
        EndPoint remoteEp = new IPEndPoint(0, 0);
        var buffer = new byte[50 * 1024];
        // 如果引用计数大于零，说明不应该关闭音频并保持音频采集线程处理活动状态
        while (_audioCts?.IsCancellationRequested == false)
            try
            {
                // 音频数据返回的目的对象以最后申请音频数据的任务实例为准，即连接本服务的不同客户程序，后来者拥有音频的使用权
                // 接收来自设备的无损音频数据（立体声，16位，44.1KHz）
                var cnt = _audioSocket.ReceiveFrom(buffer, ref remoteEp);
                if (cnt == 0)
                {
                    Thread.Sleep(20);
                    continue;
                }

                var reData = new byte[cnt];
                Array.Copy(buffer, reData, cnt);
                var generalAudio = new SDataAudio
                {
                    Format = AudioFormat.Pcm,
                    Channels = 2,
                    BlockAlign = 4,
                    SamplingRate = 44100,
                    BitsPerSample = 16,
                    BytesPerSecond = 44100 * 4,
                    Data = reData
                };
                var result = new List<object>
                {
                    generalAudio
                };
                // 返回音频数据到客户程序
                SendData(result);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine($"音频异常，异常信息：{ex}");
#endif
            }

        SingleCall(CombineCmd(Constants.Audio, Constants.Space, Constants.AudioOff));
    }

    #endregion

    private Modulation GetModulation(string sModulation)
    {
        //AM,FM,2FSK,4FSK,2PSK,4PSK,2ASK,4ASK,DSB,VSB,CW
        if (string.IsNullOrWhiteSpace(sModulation)) return Modulation.Iq;
        switch (sModulation.ToUpper())
        {
            case "FM":
                return Modulation.Fm;
            case "AM":
                return Modulation.Am;
            case "2FSK":
                return Modulation._2FSK;
            case "4FSK":
                return Modulation._4FSK;
            case "CW":
                return Modulation.Cw;
            case "2PSK":
                return Modulation.Bpsk;
            case "4PSK":
                return Modulation.Qpsk;
            case "2ASK":
            case "4ASK":
                return Modulation.Ask;
            default:
                return Modulation.Iq;
        }
    }

    #region 监测数据捕获

    private void OnDataArrive(Dictionary<string, ScpiMapItem> contentMap)
    {
        switch (CurFeature)
        {
            case FeatureType.FFM:
            case FeatureType.ITUM:
                MonitorCallback(contentMap);
                break;
            case FeatureType.FFDF:
                DdfCallback(contentMap);
                break;
            case FeatureType.WBDF:
                WbdfCallback(contentMap);
                break;
            case FeatureType.SCAN:
                ScanCallback(contentMap);
                break;
            case FeatureType.MScan:
                MScanCallback(contentMap);
                break;
        }
    }

    /// <summary>
    ///     返回单频测量数据
    /// </summary>
    /// <param name="value">监测数据</param>
    private void MonitorCallback(Dictionary<string, ScpiMapItem> value)
    {
        var result = new List<object>();
        if (value.ContainsKey("level"))
        {
            var level = ToSingle(value, "level");
            var generalLevel = new SDataLevel
            {
                Bandwidth = IfBandwidth,
                Frequency = Frequency,
                Data = level
            };
            result.Add(generalLevel);
        }

        if (SpectrumSwitch && value.ContainsKey("spectrum"))
        {
            var shortData = ToDataBlocks(value, "spectrum");
            var generalSpectrum = new SDataSpectrum
            {
                Span = IfBandwidth,
                Frequency = Frequency,
                Data = shortData
            };
            result.Add(generalSpectrum);
        }

        if (IqSwitch && value.ContainsKey("i") && value.ContainsKey("q"))
        {
            var iData = ToDataBlocks(value, "i");
            var qData = ToDataBlocks(value, "q");
            var generalIq = new SDataIq
            {
                Attenuation = (int)Attenuation,
                Bandwidth = IfBandwidth,
                Frequency = Frequency,
                Data16 = new short[iData.Length * 2]
            };
            for (var i = 0; i < iData.Length; i++)
            {
                generalIq.Data16[i * 2] = iData[i];
                generalIq.Data16[i * 2 + 1] = qData[i];
            }

            generalIq.SamplingRate = Constants.BwRateMap.TryGetValue(IfBandwidth, out var value1)
                ? value1
                : IfBandwidth * 1.28;
            result.Add(generalIq);
        }

        //注意：打开ITU测量，会影响数据采集速度
        if (ItuSwitch && value.ContainsKey("itu_deviation"))
        {
            var dAmDepth = double.NaN;
            var dFmDev = double.NaN;
            var dFmDevPos = double.NaN;
            var dFmDevNeg = double.NaN;
            var dPmDepth = double.NaN;
            var modulation = Modulation.Iq;
            if (value.ContainsKey("modulation")) modulation = GetModulation(ToValueString(value, "modulation"));
            var deviation = ToDouble(value, "itu_deviation");
            var posdev = ToDouble(value, "itu_posdev");
            var negdev = ToDouble(value, "itu_negdev");
            var xdbbw = ToDouble(value, "itu_xdbbw");
            var betabw = ToDouble(value, "itu_betabw");
            if (modulation.Equals(Modulation.Am))
            {
                dAmDepth = deviation;
            }
            else if (modulation.Equals(Modulation.Fm))
            {
                dFmDev = deviation;
                dFmDevPos = posdev;
                dFmDevNeg = negdev;
            }
            else if (modulation.Equals(Modulation.Pm))
            {
                dPmDepth = deviation;
            }

            var dataItu = new SDataItu
            {
                Frequency = Frequency,
                Bandwidth = IfBandwidth,
                Misc = new Dictionary<string, object>
                {
                    { ParameterNames.ItuXdb, xdbbw },
                    { ParameterNames.ItuBeta, betabw },
                    { ParameterNames.ItuAmDepth, dAmDepth },
                    { ParameterNames.ItuFmDev, dFmDev },
                    { ParameterNames.ItuFmDevPos, dFmDevPos },
                    { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                    { ParameterNames.ItuPmDepth, dPmDepth }
                },
                Modulation = Modulation.Iq
            };
            result.Add(dataItu);
        }

        SendData(result);
    }

    /// <summary>
    ///     返回单频测向数据
    /// </summary>
    /// <param name="value">监测数据</param>
    private void DdfCallback(Dictionary<string, ScpiMapItem> value)
    {
        if (!(value.ContainsKey("azimuth") || value.ContainsKey("quality")
                                           || value.ContainsKey("spectrum") || value.ContainsKey("level")))
            return;
        var result = new List<object>();
        var generalDFind = new SDataDfind();
        if (value.ContainsKey("azimuth"))
        {
            var azimuth = ToSingle(value, "azimuth");
            if (float.IsNaN(azimuth)) return;
            generalDFind.Azimuth = azimuth;
        }

        generalDFind.BandWidth = IfBandwidth;
        generalDFind.Frequency = Frequency;
        if (value.ContainsKey("quality"))
        {
            var quality = ToSingle(value, "quality");
            if (float.IsNaN(quality)) return;
            generalDFind.Quality = quality;
        }

        if (SpectrumSwitch && value.ContainsKey("spectrum"))
        {
            var shortData = ToDataBlocks(value, "spectrum");
            var generalSpectrum = new SDataSpectrum
            {
                Span = IfBandwidth,
                Frequency = Frequency,
                Data = shortData
            };
            result.Add(generalSpectrum);
        }

        if (LevelSwitch)
        {
            var generalLevel = new SDataLevel();
            if (value.ContainsKey("level"))
            {
                var level = ToSingle(value, "level");
                if (float.IsNaN(level)) return;
                generalLevel.Data = level;
            }

            generalLevel.Bandwidth = IfBandwidth;
            generalLevel.Frequency = Frequency;
            result.Add(generalLevel);
        }

        result.Add(generalDFind);
        SendData(result);
    }

    /// <summary>
    ///     返回宽带测向数据
    /// </summary>
    /// <param name="value">监测数据</param>
    private void WbdfCallback(Dictionary<string, ScpiMapItem> value)
    {
        if (!(value.ContainsKey("azimuth") || value.ContainsKey("quality") || value.ContainsKey("level"))) return;
        var result = new List<object>();
        var generalPanoramaDFind = new SDataDfpan();
        if (value.ContainsKey("azimuth"))
        {
            // 频谱数据转换为 short，再除以10转换为float
            var shortData = ToDataBlocks(value, "azimuth");
            var floatData = new float[shortData.Length / 2];
            for (var i = 0; i < shortData.Length; i++) floatData[i] = shortData[i] / 10.0f;
            generalPanoramaDFind.Azimuths = floatData;
            generalPanoramaDFind.Span = ResolutionBandwidth; // double.Parse(this._channelBandwidth) / 1000;
            generalPanoramaDFind.Frequency = Frequency;
        }

        if (value.ContainsKey("quality"))
        {
            var shortData = ToDataBlocks(value, "quality");
            var floatData = new float[shortData.Length / 2];
            for (var i = 0; i < shortData.Length; i++) floatData[i] = shortData[i] / 10.0f;
            generalPanoramaDFind.Qualities = floatData;
        }

        if (SpectrumSwitch && value.ContainsKey("level"))
        {
            var shortData = ToDataBlocks(value, "level");
            var generalSpectrum = new SDataSpectrum
            {
                Span = ResolutionBandwidth, // double.Parse(this.CHBW) / 1000;
                Frequency = Frequency,
                Data = shortData
            };
            result.Add(generalSpectrum);
        }

        result.Add(generalPanoramaDFind);
        if (generalPanoramaDFind.Azimuths == null) return;
        SendData(result);
    }

    /// <summary>
    ///     返回频段扫描数据（PSCAN/FSCAN）
    /// </summary>
    /// <param name="value">数据</param>
    private void ScanCallback(Dictionary<string, ScpiMapItem> value)
    {
        var result = new List<object>();
        var generalScan = new SDataScan();
        // 总点数，四舍五入后加一，保证点数据正确
        var count = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        // 收到“OK”表示当前段已经扫描结束，将起始索引设置为“0”，并直接返回，下一包数据无论哪个频段，一定是从索引为“0”开始
        if (value.ContainsKey("OK"))
        {
            // 当设备返回“OK”时，表明其已完成该频段的扫描，但是并不能验证数据的完整性
            // 因此当结束标志返回时，如果没有收到完整的数据，需要手动补齐数据
            if (_pdloc > 0 && _pdloc < count)
            {
                var data = new short[count - _pdloc];
                for (var i = 0; i < data.Length; ++i) data[i] = -200;
                generalScan.StartFrequency = StartFrequency;
                generalScan.StepFrequency = StepFrequency;
                generalScan.StopFrequency = StopFrequency;
                generalScan.Total = count;
                generalScan.Offset = _pdloc;
                generalScan.Data = data;
                result.Add(generalScan);
                SendData(result);
            }

            _pdloc = 0;
            return;
        }

        var levelShort = ToDataBlocks(value, "level");
        var levels = new short[levelShort.Length];
        for (var i = 0; i < levelShort.Length; i++) levels[i] = levelShort[i]; // short除以10得到实际的电平值
        // 是否超出频段扫描范围，可能的原因是接收机返回点数比理伦值大，需要减去多余的点
        if (_pdloc + levels.Length > count) Array.Resize(ref levels, count - _pdloc);
        generalScan.StartFrequency = StartFrequency;
        generalScan.StepFrequency = StepFrequency;
        generalScan.StopFrequency = StopFrequency;
        generalScan.Total = count;
        generalScan.Offset = _pdloc;
        generalScan.Data = levels;
        result.Add(generalScan);
        SendData(result);
        _pdloc += levels.Length;
        if (_pdloc == count) _pdloc = 0;
    }

    /// <summary>
    ///     返回离散扫描数据(MSCAN/RMSCAN)
    /// </summary>
    /// <param name="value">数据</param>
    private void MScanCallback(Dictionary<string, ScpiMapItem> value)
    {
        var mscanPoints = Array.ConvertAll(MscanPoints, p => (DiscreteFrequencyTemplate)p);
        var offset = Array.FindIndex(mscanPoints,
            item => item.Frequency.CompareWith(ToDouble(value, "frequency")) == 0);
        if (offset < 0 || offset >= mscanPoints.Length) return;
        var generalScan = new SDataScan
        {
            StartFrequency = ToDouble(value, "frequency"),
            Offset = offset,
            Data = new short[1],
            Total = MscanPoints.Length
        };
        generalScan.Data[0] = (short)(ToSingle(value, "level") * 10);
        var result = new List<object>();
        if (SpectrumSwitch && value.ContainsKey("spectrum"))
        {
            var shortData = ToDataBlocks(value, "spectrum");
            var generalSpectrum = new SDataSpectrum
            {
                Span = mscanPoints[offset].FilterBandwidth,
                Frequency = generalScan.StartFrequency,
                Data = shortData
            };
            result.Add(generalSpectrum);
        }

        //注意：打开ITU测量，会影响数据采集速度
        if (ItuSwitch && value.ContainsKey("itu_deviation"))
        {
            var dAmDepth = double.NaN;
            var dFmDev = double.NaN;
            var dFmDevPos = double.NaN;
            var dFmDevNeg = double.NaN;
            var dPmDepth = double.NaN;
            var modulation = Modulation.Iq;
            if (value.ContainsKey("modulation")) modulation = GetModulation(ToValueString(value, "modulation"));
            var deviation = ToDouble(value, "itu_deviation");
            var posdev = ToDouble(value, "itu_posdev");
            var negdev = ToDouble(value, "itu_negdev");
            var xdbbw = ToDouble(value, "itu_xdbbw");
            var betabw = ToDouble(value, "itu_betabw");
            if (modulation.Equals(Modulation.Am))
            {
                dAmDepth = deviation;
            }
            else if (modulation.Equals(Modulation.Fm))
            {
                dFmDev = deviation;
                dFmDevPos = posdev;
                dFmDevNeg = negdev;
            }
            else if (modulation.Equals(Modulation.Pm))
            {
                dPmDepth = deviation;
            }

            var dataItu = new SDataItu
            {
                Frequency = Frequency,
                Bandwidth = IfBandwidth,
                Misc = new Dictionary<string, object>
                {
                    { ParameterNames.ItuXdb, xdbbw },
                    { ParameterNames.ItuBeta, betabw },
                    { ParameterNames.ItuAmDepth, dAmDepth },
                    { ParameterNames.ItuFmDev, dFmDev },
                    { ParameterNames.ItuFmDevPos, dFmDevPos },
                    { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                    { ParameterNames.ItuPmDepth, dPmDepth }
                },
                Modulation = Modulation.Iq
            };
            result.Add(dataItu);
        }

        result.Add(generalScan);
        SendData(result);
    }

    #endregion
}