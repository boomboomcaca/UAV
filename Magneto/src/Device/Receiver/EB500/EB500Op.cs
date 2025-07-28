using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB500;

public partial class Eb500
{
    private int _offsetPscan;

    private void CapturePacket(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        var socket = _dataSocket;
        var queue = _udpDataQueue;
        var buffer = new byte[1024 * 1024];
        socket.ReceiveBufferSize = buffer.Length;
        while (!token.IsCancellationRequested)
            try
            {
                var receivedCount = socket.Receive(buffer);
                if (receivedCount <= 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var receivedBuffer = new byte[receivedCount];
                Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);
                if (TaskState == TaskState.Start) queue.Enqueue(receivedBuffer);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    private void DispatchPacket(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        var queue = _udpDataQueue;
        while (!token.IsCancellationRequested)
            try
            {
                if (queue.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                queue.TryDequeue(out var buffer);
                if (buffer == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
#if DEBUG
                Trace.WriteLine(e.ToString());
#endif
            }
    }

    private void ForwardPacket(RawPacket packet)
    {
        if (packet == null) return;
        var result = new List<object>();
        object obj = null;
        foreach (var data in packet.DataCollection)
        {
            switch ((DataType)data.Generic.Tag)
            {
                case DataType.Audio:
                    obj = ToAudio(data as RawAudio);
                    break;
                case DataType.Ifpan:
                    if (CurFeature is FeatureType.FScne or FeatureType.MScne
                        or FeatureType.MScan) //处理在离散搜索、频段搜索时，偶发性出现第一包为无效数据的情况
                    {
                        if (_flag) obj = ToSpectrum(data as RawIfPan);
                    }
                    else
                    {
                        obj = ToSpectrum(data as RawIfPan);
                    }

                    break;
                case DataType.If:
                    obj = ToIq(data as RawIf);
                    break;
                case DataType.Fscan:
                    obj = ToFScan(data as RawFScan);
                    _flag = true;
                    break;
                case DataType.MScan:
                    obj = ToMScan(data as RawMScan);
                    _flag = true;
                    break;
                case DataType.Pscan:
                    obj = ToPScan(data as RawPScan);
                    break;
            }

            if (obj != null)
            {
                if (obj is List<object> list)
                    result.AddRange(list);
                else
                    result.Add(obj);
            }
        }

        result = result.Where(item => item != null).ToList();
        if (result.Count == 0 || TaskState != TaskState.Start) return;
        //result.Add(_deviceId);
        SendData(result);
    }

    private void DispatchLevelData(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        //"VOLT:AC", "AM", "AM:POS", "AM:NEG", "FM", "FM:POS", "FM:NEG", "PM", "BAND"
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送
        //否则界面参数列表刷新时也会出现卡顿,此处处理为3秒左右发送一次到客户端
        var sendtime = DateTime.MinValue;
        while (!token.IsCancellationRequested)
            try
            {
                if ((_mediaType & (MediaType.Level | MediaType.Itu)) == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var datas = new List<object>();
                //获取电平数据和ITU数据
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                if ((_mediaType & MediaType.Level) > 0)
                {
                    var level = float.Parse(values[0]);
                    var datalevel = new SDataLevel
                    {
                        Frequency = _frequency,
                        Bandwidth = _filterBandwidth,
                        Data = level
                    };
                    datas.Add(datalevel);
                }

                if ((_mediaType & MediaType.Itu) > 0 && values.Length >= 9)
                {
                    var ts = DateTime.Now - sendtime;
                    if (ts.TotalMilliseconds >= 3000)
                    {
                        var am = double.Parse(values[1]);
                        var fm = double.Parse(values[4]) / 1000d;
                        var fmpos = double.Parse(values[5]) / 1000d;
                        var fmneg = double.Parse(values[6]) / 1000d;
                        var pm = double.Parse(values[7]);
                        var bw = double.Parse(values[8]) / 1000d;
                        var dAmDepth = am is < 0 or > 100 ? double.MinValue : am;
                        var dFmDev = fm < -1000000000f ? double.MinValue : fm;
                        var dFmDevPos = fmpos < -1000000000f ? double.MinValue : fmpos;
                        var dFmDevNeg = fmneg < -1000000000f ? double.MinValue : fmneg;
                        var dPmDepth = pm < -1000000000f ? double.MinValue : pm;
                        const Modulation demMode = Modulation.Iq;
                        var dataItu = new SDataItu
                        {
                            Frequency = _frequency,
                            Modulation = demMode,
                            Misc = new Dictionary<string, object>
                            {
                                { ParameterNames.ItuAmDepth, dAmDepth },
                                { ParameterNames.ItuFmDev, dFmDev },
                                { ParameterNames.ItuFmDevPos, dFmDevPos },
                                { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                                { ParameterNames.ItuPmDepth, dPmDepth }
                            }
                        };
                        if ("XDB".Equals(_bandMeasureMode, StringComparison.OrdinalIgnoreCase))
                        {
                            var value = bw < -1000000000f ? double.MinValue : bw;
                            dataItu.Misc.Add(ParameterNames.ItuXdb, value);
                        }
                        else
                        {
                            var value = bw < -1000000000f ? double.MinValue : bw;
                            dataItu.Misc.Add(ParameterNames.ItuBeta, value);
                        }

                        datas.Add(dataItu);
                        sendtime = DateTime.Now;
                    }
                }

                if (datas.Count > 0 && TaskState == TaskState.Start) SendData(datas);
                Thread.Sleep(20);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    #region 数据采集与处理

    private SDataAudio ToAudio(RawAudio data)
    {
        if (data == null) return null;
        Consts.AfModes.TryGetValue(data.AudioMode, out var afMode);
        if (afMode == null) return null;
        return new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = afMode.SamplingRate * 1000,
            BytesPerSecond = afMode.DataRate * 1000,
            BitsPerSample = afMode.BitsPerSample,
            BlockAlign = afMode.LengthPerFrame,
            Channels = afMode.Channels,
            Data = data.DataCollection
        };
    }

    /// <summary>
    ///     解析并发送频率数据
    /// </summary>
    private SDataSpectrum ToSpectrum(RawIfPan data)
    {
        if (data == null) return null;
        var spectrum = new short[data.Trace.NumberOfTraceItems];
        for (var i = 0; i < spectrum.Length; ++i)
        {
            spectrum[i] = data.DataCollection[i];
            if (spectrum[i] >= 2010) //过滤掉设备偶发性的发送很大的无效数据，由于静噪门限最大130dB，200为设备每包数据分隔符，此处将201以上的数据屏蔽掉
                return null;
        }

        var freq = ((ulong)data.FrequencyHigh << 32) + data.FrequencyLow;
        return new SDataSpectrum
        {
            Frequency = freq / 1000000d,
            Span = data.SpanFrequency / 1000d,
            Data = spectrum
        };
    }

    /// <summary>
    ///     解析并发送IQ数据
    /// </summary>
    private SDataIq ToIq(RawIf data)
    {
        if (data?.DataCollection == null) return null;
        var iq = Array.ConvertAll(data.DataCollection, p => (short)p);
        return new SDataIq
        {
            Frequency = data.FrequencyLow / 1000000d,
            Bandwidth = data.Bandwidth / 1000d,
            SamplingRate = data.Samplerate / 1000d,
            Attenuation = data.RxAtt,
            Data16 = iq
        };
    }

    /// <summary>
    ///     解析并发送频段扫描数据
    /// </summary>
    private object ToFScan(RawFScan data)
    {
        if (data == null) return null;
        var levels = data.DataCollection;
        var freqs = new double[data.Trace.NumberOfTraceItems];
        if (_lastFreqHigh == uint.MaxValue) _lastFreqHigh = data.StartFrequencyHigh;
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++)
        {
            var freqLow = data.FreqLowCollection[i];
            var freq = (((ulong)_lastFreqHigh << 32) + freqLow) / 1000000d;
            if (i > 0 && freq < freqs[i - 1])
            {
                _lastFreqHigh++;
                freq = (((ulong)_lastFreqHigh << 32) + freqLow) / 1000000d;
            }

            freqs[i] = freq;
        }

        var nextFreq = freqs.Last() + StepFrequency / 1000d;
        _lastFreqHigh = (uint)((ulong)(nextFreq * 1000000) >> 32);
        if (nextFreq > StopFrequency) _lastFreqHigh = uint.MaxValue;
        var tempLevels = new List<short>();
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var j = 0;
        for (; j < data.Trace.NumberOfTraceItems; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var index = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
                if (index >= 0)
                {
                    for (var k = index + tempLevels.Count; k < total; ++k) tempLevels.Add(0);
                    var scan = new SDataScan
                    {
                        StartFrequency = StartFrequency,
                        StopFrequency = StopFrequency,
                        StepFrequency = StepFrequency,
                        Offset = index,
                        Total = total,
                        Data = tempLevels.ToArray()
                    };
                    if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                }

                tempLevels.Clear();
            }

        if (tempLevels.Count == 0) return null;
        var currIndex = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        return new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = tempLevels.ToArray()
        };
    }

    /// <summary>
    ///     解析并发送离散扫描数据
    /// </summary>
    private object ToMScan(RawMScan data)
    {
        if (data == null) return null;
        var levels = data.DataCollection;
        var freqs = new double[data.Trace.NumberOfTraceItems];
        if (_lastFreqHigh == uint.MaxValue) _lastFreqHigh = 0;
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++)
        {
            var freqLow = data.FreqLowCollection[i];
            var freq = (((ulong)_lastFreqHigh << 32) + freqLow) / 1000000d;
            if (!_scanFreqs.Contains(freq))
            {
                if (_lastFreqHigh == 0)
                    _lastFreqHigh = 1;
                else
                    _lastFreqHigh = 0;
                freq = (((ulong)_lastFreqHigh << 32) + freqLow) / 1000000d;
            }

            freqs[i] = freq;
        }

        var tempLevels = new List<short>();
        var j = 0;
        for (; j < data.Trace.NumberOfTraceItems; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var index = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
                if (index >= 0)
                {
                    var scan = new SDataScan
                    {
                        Offset = index,
                        Total = _scanFreqs.Count,
                        Data = tempLevels.ToArray()
                    };
                    if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                }

                tempLevels.Clear();
            }

        if (tempLevels.Count == 0) return null;
        var currIndex = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
        if (currIndex < 0) return null;
        return new SDataScan
        {
            Offset = currIndex,
            Total = _scanFreqs.Count,
            Data = tempLevels.ToArray()
        };
    }

    /// <summary>
    ///     解析并发送PScan数据
    /// </summary>
    private object ToPScan(RawPScan data)
    {
        if (data?.DataCollection == null) return null;
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        if (data.Trace.OptionalHeaderLength == 32 && data.FreqOfFirstStep == 0)
        {
            var scandata = new List<short>
            {
                Capacity = data.Trace.NumberOfTraceItems
            };
            scandata.AddRange(data.DataCollection);
            if (scandata.Last() == 2000) scandata.RemoveAt(scandata.Count - 1);
            var scan = new SDataScan
            {
                StartFrequency = StartFrequency,
                StopFrequency = StopFrequency,
                StepFrequency = StepFrequency,
                Offset = _offsetPscan,
                Total = total,
                Data = scandata.ToArray()
            };
            _offsetPscan += scandata.Count;
            if (_offsetPscan >= total - 1) _offsetPscan = 0;
            return scan;
        }

        var levels = new List<short>();
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++) levels.Add(data.DataCollection[i]);
        var freq = data.FreqOfFirstStep / 1000000d;
        var currIndex = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var nextFreq = freq + StepFrequency / 1000d * data.Trace.NumberOfTraceItems;
        if (nextFreq > StopFrequency) _lastFreqHigh = uint.MaxValue;
        //表示本次扫描结束,此处仅补齐缺少的点
        if (levels.Last() == 2000)
        {
            levels.RemoveAt(levels.Count - 1);
            for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
        }

        return new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = levels.ToArray()
        };
    }

    #endregion
}