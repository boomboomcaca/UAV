using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

public partial class Esmd
{
    private async Task CapturePacketAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var identifier = taskParam.Tag;
        var token = taskParam.Token;
        var socket = _channels[identifier];
        var queue = _queues[identifier];
        var buffer = new byte[1024 * 1024];
        socket.ReceiveBufferSize = buffer.Length;
        while (!token.IsCancellationRequested)
            try
            {
                var receivedCount = socket.Receive(buffer);
                if (receivedCount <= 0)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                var receivedBuffer = new byte[receivedCount];
                Buffer.BlockCopy(buffer, 0, receivedBuffer, 0, receivedCount);
                if (TaskState == TaskState.Start) queue.Enqueue(receivedBuffer);
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                // await Task.Delay(10).ConfigureAwait(false);
                break;
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
    }

    private async Task DispatchPacketAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var identifier = taskParam.Tag;
        var isIfmch = taskParam.IsIfmch;
        var token = taskParam.Token;
        var queue = _queues[identifier];
        while (!token.IsCancellationRequested)
            try
            {
                if (queue.IsEmpty)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                //Thread.Sleep(100);
                queue.TryDequeue(out var buffer);
                if (buffer == null)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                ForwardPacket(packet, isIfmch);
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

    private void ForwardPacket(RawPacket packet, bool isIfmch)
    {
        if (packet == null) return;
        var result = new List<object>();
        var freq = 0d;
        object obj = null;
        foreach (var data in packet.DataCollection)
        {
            switch ((DataType)data.Tag)
            {
                case DataType.Audio:
                {
                    var rawAudio = data as RawAudio;
                    obj = ToAudio(rawAudio);
                    freq = (((long)rawAudio.FrequencyHigh << 32) + rawAudio.FrequencyLow) / 1000000d;
                }
                    break;
                case DataType.Ifpan:
                    var list = ToSpectrum(data as RawIfPan);
                    obj = list;
                    if (list?.Find(item => item is SDataSpectrum) is not SDataSpectrum spec) break;
                    freq = spec.Frequency;
                    var startFreq = spec.Frequency - spec.Span / 1000 / 2;
                    var step = spec.Span / (spec.Data.Length - 1);
                    if (CurFeature != FeatureType.IFMCA || _preChannels == null) break;
                    // 解析频谱
                    var chans = _preChannels.ToArray();
                    for (var m = 0; m < chans.Length; m++)
                    {
                        var mf = chans[m].Frequency;
                        var bw = chans[m].FilterBandwidth;
                        var start = mf - bw / 1000 / 2;
                        var startIndex = (int)Math.Round((start - startFreq) / step * 1000);
                        var count = (int)(bw / step) + 1;
                        startIndex = Math.Max(startIndex, 0);
                        if (startIndex + count > spec.Data.Length) count = spec.Data.Length - startIndex;
                        if (count > 0)
                        {
                            var buffer = new short[count];
                            Array.Copy(spec.Data, startIndex, buffer, 0, count);
                            var specData = new SDataSpectrum
                            {
                                Frequency = mf,
                                Span = bw,
                                Data = buffer
                            };
                            var dataMchChannel = new SDataDdc
                            {
                                ChannelNumber = m,
                                Data = new List<object> { specData }
                            };
                            SendData(new List<object> { dataMchChannel });
                        }
                    }

                    break;
                case DataType.If:
                    var iq = ToIq(data as RawIf);
                    obj = iq;
                    freq = iq.Frequency;
                    if ((CurFeature & FeatureType.AmpDF) > 0)
                    {
                        ProcessAmpDf(iq, IqSamplingCount);
                        continue;
                    }

                    break;
                case DataType.Fscan:
                    obj = ToFScan(data as RawFScan);
                    break;
                case DataType.MScan:
                    obj = ToMScan(data as RawMScan);
                    break;
                case DataType.Pscan:
                    obj = ToPScan(data as RawPScan);
                    break;
            }

            if (isIfmch)
            {
                var chans = _preChannels;
                var index = chans.FindIndex(p => p.Frequency.EqualTo(freq, _epsilon));
                if (index < 0) continue;
                var dataMchChannel = new SDataDdc
                {
                    ChannelNumber = index,
                    Data = new List<object> { obj }
                };
                result.Add(dataMchChannel);
            }
            else if (obj != null)
            {
                if (obj is List<SDataRaw> list)
                    list.ForEach(p => result.Add(p));
                else
                    result.Add(obj);
            }
        }

        result = result.Where(item => item != null).ToList();
        if (result.Count == 0 || TaskState != TaskState.Start) return;
        //result.Add(_deviceId);
        SendData(result);
    }

    private async Task DispatchLevelDataAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        //"VOLT:AC", "AM", "AM:POS", "AM:NEG", "FM", "FM:POS", "FM:NEG", "PM", "BAND"
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送
        //否则界面参数列表刷新时也会出现卡顿,此处处理为3秒左右发送一次到客户端
        var sendTime = DateTime.MinValue;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(10, token).ConfigureAwait(false);
            try
            {
                //当前未订阅电平或者ITU数据则直接返回
                if ((_media & (MediaType.Level | MediaType.Itu)) == 0)
                {
                    //释放时间片，促进线程切换
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                //发送数据缓存列表
                var datas = new List<object>();
                //获取电平和ITU数据
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                if ((_media & MediaType.Level) > 0)
                {
                    //无效会返回 - 9E37
                    _level = float.Parse(values[0]);
                    if (_level < -200f) continue;
                    if ((_media & MediaType.Spectrum) == 0)
                    {
                        //频谱开关关闭时在此发送否则和频谱一起发送
                        var dataLevel = new SDataLevel
                        {
                            Frequency = _frequency,
                            Bandwidth = _ifBandwidth,
                            Data = _level - (EnableLna ? Lna : 0)
                        };
                        var temp = (long)(dataLevel.Frequency * 1000000);
                        if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value))
                            dataLevel.Frequency = value / 1000000.0d;
                        datas.Add(dataLevel);
                        Thread.Sleep(5);
                    }
                }

                if ((_media & MediaType.Itu) > 0 && values.Length >= 9)
                {
                    var ts = DateTime.Now - sendTime;
                    if (ts.TotalMilliseconds >= 3000)
                    {
                        var am = double.Parse(values[1]);
                        //float ampos = float.Parse(values[2]);
                        //float amneg = float.Parse(values[3]);
                        var fm = double.Parse(values[4]) / 1000d;
                        var fmpos = double.Parse(values[5]) / 1000d;
                        var fmneg = double.Parse(values[6]) / 1000d;
                        var pm = double.Parse(values[7]);
                        var bw = double.Parse(values[8]) / 1000d;
                        //TODO:无效为-9E37,此处用-1000000000f判断即可
                        var dataItu = new SDataItu
                        {
                            Frequency = _frequency
                        };
                        var temp = (long)(dataItu.Frequency * 1000000);
                        if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value1))
                            dataItu.Frequency = value1 / 1000000.0d;
                        var dAmDepth = am is < 0 or > 100 ? double.MinValue : am;
                        var dFmDev = fm < -1000000000f ? double.MinValue : fm;
                        var dFmDevPos = fmpos < -1000000000f ? double.MinValue : fmpos;
                        var dFmDevNeg = fmneg < -1000000000f ? double.MinValue : fmneg;
                        var dPmDepth = pm < -1000000000f ? double.MinValue : pm;
                        dataItu.Misc = new Dictionary<string, object>
                        {
                            { ParameterNames.ItuAmDepth, dAmDepth },
                            { ParameterNames.ItuFmDev, dFmDev },
                            { ParameterNames.ItuFmDevPos, dFmDevPos },
                            { ParameterNames.ItuFmDevNeg, dFmDevNeg },
                            { ParameterNames.ItuPmDepth, dPmDepth }
                        };
                        dataItu.Modulation = Modulation.Iq;
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
                        sendTime = DateTime.Now;
                    }
                }

                if (datas.Count > 0 && TaskState == TaskState.Start) SendData(datas);
                // Thread.Sleep(10);
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
    }

    private async Task DispatchDdcDataAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token).ConfigureAwait(false);
            try
            {
                if (CurFeature == FeatureType.IFMCA)
                    if (_preChannels != null)
                    {
                        var chans = _preChannels.ToArray();
                        for (var i = 0; i < chans.Length; ++i)
                            if (chans[i].IfSwitch && chans[i].LevelSwitch)
                            {
                                var buffer = Encoding.ASCII.GetBytes($"SENS:DATA:DDC{i + 1}?\n");
                                _ddcCtrlChannel.Send(buffer);
                                var result = CaptureDdcData('\n');
                                var level = float.Parse(result);
                                if (level < -200f) continue;
                                level -= EnableLna ? Lna : 0;
                                var dataLevel = new SDataLevel
                                {
                                    Frequency = chans[i].Frequency,
                                    Bandwidth = chans[i].FilterBandwidth,
                                    Data = level
                                };
                                var dataMchChannel = new SDataDdc
                                {
                                    ChannelNumber = i,
                                    Data = new List<object> { dataLevel }
                                };
                                //dataMCHChannel.Frequency = chans[i].Frequency;
                                //dataMCHChannel.IFBandWidth = chans[i].IFBandWidth;
                                SendData(new List<object> { dataMchChannel });
                            }
                    }
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
    }

    /// <summary>
    ///     接收TDOA数据并放入缓存
    ///     TDOA也是IQ数据，但是需要从TCP通道返回，而不是向单频测向的IQ那样从UDP返回
    /// </summary>
    /// <param name="obj"></param>
    private async Task CaptureTdoaDataAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        var buffer = new byte[1024 * 1024];
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token).ConfigureAwait(false);
            if (!_isReadTdoaStart) continue;
            try
            {
                var length = _tdoaDataChannel.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                if (length <= 0) break;
                var data = new byte[length];
                Buffer.BlockCopy(buffer, 0, data, 0, length);
                if (TaskState == TaskState.Start) _tdoaQueue.Enqueue(data);
            }
            catch (SocketException)
            {
                break;
            }
            catch (TaskCanceledException)
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
    }

    /// <summary>
    ///     TDOA数据处理线程
    /// </summary>
    /// <param name="obj"></param>
    private async Task DispatchTdoaDataAsync(object obj)
    {
        if (obj is not TaskParam taskParam) return;
        var token = taskParam.Token;
        while (!token.IsCancellationRequested)
        {
            if (!_isReadTdoaStart)
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            try
            {
                if (_tdoaQueue.IsEmpty || !_tdoaQueue.TryDequeue(out var buffer))
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                // 根据数据结构定义，出队可能为null
                if (buffer == null)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                var packet = RawPacket.Parse(buffer, 0);
                var sendDatas = new List<object>();
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    await Task.Delay(1, token).ConfigureAwait(false);
                    continue;
                }

                foreach (var data in packet.DataCollection)
                    switch ((DataType)data.Tag)
                    {
                        case DataType.If:
                            var iq = ToIq(data as RawIf);
                            sendDatas.Add(iq);
                            if ((_media & MediaType.Level) > 0) sendDatas.Add(ToLevelByIq(iq));
                            if ((_media & MediaType.Spectrum) > 0) sendDatas.Add(ToSpectrumByIq(iq));
                            break;
                    }

                if (sendDatas.Count > 0 && TaskState == TaskState.Start)
                {
                    sendDatas.RemoveAll(x => x == null);
                    SendData(sendDatas);
                }
            }
            catch (TaskCanceledException)
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
    }

    private string CaptureDdcData(int endflag)
    {
        var total = 0;
        var buffer = new byte[1024 * 1024];
        while (_ddcCtrlChannel.Receive(buffer, total, 1, SocketFlags.None) > 0)
            if (buffer[total++] == endflag)
                break;
        return Encoding.ASCII.GetString(buffer, 0, total);
    }

    #region 业务数据转换

    private SDataAudio ToAudio(RawAudio data)
    {
        if (data == null) return null;
        Consts.AfModes.TryGetValue(data.AudioMode, out var afMode);
        if (afMode == null) return null;
        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = afMode.SamplingRate * 1000,
            BytesPerSecond = afMode.DataRate * 1000,
            BitsPerSample = afMode.BitsPerSample,
            BlockAlign = afMode.LengthPerFrame,
            Channels = afMode.Channels,
            Data = data.DataCollection
        };
        return audio;
    }

    private List<SDataRaw> ToSpectrum(RawIfPan data)
    {
        if ((_media & MediaType.Scan) > 0 && !_bReceivedScan) return null;
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6
        if (data.FrequencyHigh > 6) return null;
        var datas = new List<SDataRaw>();
        var dataSpec = new SDataSpectrum
        {
            Frequency = (((ulong)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d,
            Span = data.SpanFrequency / 1000d,
            Data = new short[data.DataCollection.Length]
        };
        // 电平修正，中心频率 -> 偏移量
        for (var index = 0; index < dataSpec.Data.Length; ++index)
            // 得到频谱数据
            dataSpec.Data[index] = (short)(data.DataCollection[index] - (short)((EnableLna ? Lna : 0) * 10));
        var temp = (long)(dataSpec.Frequency * 1000000);
        if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) dataSpec.Frequency = value / 1000000.0d;
        datas.Add(dataSpec);
        if ((_media & MediaType.Level) > 0 && Math.Abs(_level - float.MinValue) > 1e-9)
        {
            var dataLevel = new SDataLevel
            {
                Frequency = _frequency,
                Bandwidth = _filterBandwidth,
                Data = _level - (EnableLna ? Lna : 0)
            };
            var temp1 = (long)(dataLevel.Frequency * 1000000);
            if (_reverseFrequencyOffsetDic.TryGetValue(temp1, out var value1))
                dataLevel.Frequency = value1 / 1000000.0d;
            datas.Add(dataLevel);
        }

        return datas;
    }

    private SDataIq ToIq(RawIf data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6
        if (data.FrequencyHigh > 6) return null;
        var dataIq = new SDataIq
        {
            Frequency = (((ulong)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d,
            Bandwidth = data.Bandwidth / 1000d,
            SamplingRate = data.Samplerate / 1000d,
            Attenuation = data.RxAtt
        };
        if (data.Mode == 1)
            dataIq.Data16 = Array.ConvertAll(data.DataCollection, item => (short)item);
        else if (data.Mode == 2) dataIq.Data32 = data.DataCollection;
        var temp = (long)(dataIq.Frequency * 1000000);
        if (_reverseFrequencyOffsetDic.TryGetValue(temp, out var value)) dataIq.Frequency = value / 1000000.0d;
        return dataIq;
    }

    private SDataScan ToFScan(RawFScan data)
    {
        _bReceivedScan = true;
        if (data == null) return null;
        var levels = new short[data.DataCollection.Length];
        var freqsLow = data.FreqLowCollection;
        var freqsHigh = data.FreqHighCollection;
        // 电平修正，中心频率 -> 偏移量
        for (var index = 0; index < data.DataCollection.Length; ++index)
            levels[index] = (short)(data.DataCollection[index] - 10 * (EnableLna ? Lna : 0));
        return GetScanData(levels, freqsLow, freqsHigh);
    }

    private SDataScan GetScanData(short[] levels, uint[] freqsLow, uint[] freqsHigh)
    {
        //2017/11/1 有可能包含多帧数据（当测量时间极小，频段个数极少时）
        var tempLevels = new List<short>();
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var j = 0;
        for (; j < levels.Length; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
                var index = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
                if (index < 0) continue;
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
                tempLevels.Clear();
            }

        if (tempLevels.Count == 0) return null;
        var frequency = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
        var currIndex = Utils.GetCurrIndex(frequency, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var result = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = tempLevels.ToArray()
        };
        return result;
    }

    private SDataScan ToMScan(RawMScan data)
    {
        _bReceivedScan = true;
        if (data == null) return null;
        var levels = new short[data.DataCollection.Length];
        var freqsLow = data.FreqLowCollection;
        var freqsHigh = data.FreqHighCollection;
        if (freqsLow == null || freqsHigh == null) return null;
        // 电平修正，中心频率 -> 偏移量
        for (var index = 0; index < data.DataCollection.Length; ++index)
            levels[index] = (short)(data.DataCollection[index] - 10 * (EnableLna ? Lna : 0));
        // return GetScanData(levels, freqsLow, freqsHigh);
        var tempLevels = new List<short>();
        var j = 0;
        for (; j < levels.Length; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
                var index = _scanFreqs.IndexOf(freq);
                if (index < 0) continue;
                var scan = new SDataScan
                {
                    StartFrequency = StartFrequency,
                    StopFrequency = StopFrequency,
                    StepFrequency = StepFrequency,
                    Offset = index,
                    Total = _scanFreqs.Count,
                    Data = tempLevels.ToArray()
                };
                if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                tempLevels.Clear();
            }

        if (tempLevels.Count == 0) return null;
        var frequency = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
        var currIndex = _scanFreqs.IndexOf(frequency);
        if (currIndex < 0) return null;
        var result = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = _scanFreqs.Count,
            Data = tempLevels.ToArray()
        };
        return result;
    }

    private SDataScan ToPScan(RawPScan data)
    {
        var levels = new List<short>();
        var freq = data.FreqOfFirstStep / 1000000d;
        foreach (var t in data.DataCollection)
        {
            var level = (short)(t - 10 * (EnableLna ? Lna : 0));
            levels.Add(level);
        }

        var currIndex = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        //表示本次扫描结束,此处仅补齐缺少的点
        if (levels.Last() == 2000)
        {
            levels.RemoveAt(levels.Count - 1);
            for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
        }

        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = levels.ToArray()
        };
        return scan;
    }

    private void ProcessAmpDf(SDataIq iq, int segmentIqCount = 128)
    {
        if (iq == null) return;
        var action = new Action<SDataIq>(item =>
            {
                var result = new List<object>
                {
                    ToLevelByIq(item)
                };
                if ((_media & MediaType.Spectrum) > 0) result.Add(ToSpectrumByIq(item));
                result = result.Where(value => value != null).ToList();
                if (result.Count == 0) return;
                SendData(result);
            }
        );
        var validCount = iq.Data16.Length % (segmentIqCount * 2) / 2;
        var index = 0;
        for (; iq.Data16.Length >= segmentIqCount * 2 && index < iq.Data16.Length; index += segmentIqCount * 2)
        {
            var temp = new SDataIq
            {
                Frequency = iq.Frequency,
                Bandwidth = iq.Bandwidth,
                SamplingRate = iq.SamplingRate,
                Attenuation = iq.Attenuation,
                Data16 = new short[segmentIqCount * 2]
            };
            Buffer.BlockCopy(iq.Data16, sizeof(short) * index, temp.Data16, 0, sizeof(short) * temp.Data16.Length);
            action.Invoke(temp);
        }

        if (validCount <= 0) return;
        if (index > 0)
        {
            index -= segmentIqCount * 2;
            var temp = new SDataIq
            {
                Frequency = iq.Frequency,
                Bandwidth = iq.Bandwidth,
                SamplingRate = iq.SamplingRate,
                Attenuation = iq.Attenuation,
                Data16 = new short[validCount * 2]
            };
            Buffer.BlockCopy(iq.Data16, sizeof(short) * index, temp.Data16, 0, sizeof(short) * temp.Data16.Length);
            action.Invoke(temp);
        }
        else
        {
            action.Invoke(iq);
        }
    }

    private SDataLevel ToLevelByIq(SDataIq iq)
    {
        if (iq == null) return null;
        var value = iq.Data16 != null
            ? Array.ConvertAll(iq.Data16, item => (float)item)
            : Array.ConvertAll(iq.Data32, item => (float)item);
        var level = Utilities.GetLevel(value);
        level += iq.Attenuation;
        var result = new SDataLevel
        {
            Frequency = iq.Frequency,
            Bandwidth = iq.Bandwidth,
            Data = level + LevelCalibrationFromIq
        };
        return result;
    }

    private SDataSpectrum ToSpectrumByIq(SDataIq iq)
    {
        if (iq == null) return null;
        var value = iq.Data16 != null
            ? Array.ConvertAll(iq.Data16, item => (float)item)
            : Array.ConvertAll(iq.Data32, item => (float)item);
        var exp = Utilities.Log2N(value.Length / 2);
        var length = 1 << exp;
        var windowValue = new float[length];
        var coe = Utilities.Window(ref windowValue, WindowType.HanningWin);
        var spectrum = Utilities.GetWindowData(value, windowValue, length);
        Utilities.Fft(ref spectrum);
        var efficientLength = (int)(length * 1.0 * iq.Bandwidth / (iq.SamplingRate * 1000) + 0.5);
        var efficientIndex = length - efficientLength / 2;
        coe += (float)(-20 * Math.Log10(length) + iq.Attenuation);
        var spectrumEx = new float[length];
        for (var index = 0; index < length; ++index)
            spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
        var validSpectrum = new short[efficientLength];
        for (var index = 0; index < validSpectrum.Length; ++index)
            validSpectrum[index] =
                (short)((spectrumEx[(efficientIndex + index) % length] + coe + LevelCalibrationFromIq) * 10);
        var result = new SDataSpectrum
        {
            Frequency = iq.Frequency,
            Span = iq.Bandwidth,
            Data = validSpectrum
        };
        return result;
    }

    #endregion

    #region DDC

    /// <summary>
    ///     设置中频多路窄带参数
    /// </summary>
    /// <param name="channels"></param>
    private void SetIfmch(Dictionary<string, object>[] channels)
    {
        if (channels == null) return;
        for (var i = 0; i < channels.Length; ++i)
        {
            var template = (IfmcaTemplate)_ddcChannels[i];
            var frequency = template.Frequency;
            var filterBw = template.FilterBandwidth;
            var demodulation = template.DemMode;
            var sqc = template.SquelchThreshold;
            var sqcswitch = template.SquelchSwitch;
            var iqswitch = template.IqSwitch;
            var audioswitch = template.AudioSwitch;
            var ifswitch = template.IfSwitch;
            var levelswitch = template.LevelSwitch;
            if (_preChannels == null || i >= _preChannels.Count)
            {
                _preChannels ??= new List<IfmcaTemplate>();
                var channel = new IfmcaTemplate
                {
                    Frequency = frequency,
                    FilterBandwidth = filterBw,
                    DemMode = demodulation,
                    SquelchThreshold = sqc,
                    SquelchSwitch = sqcswitch.Equals("ON"),
                    LevelSwitch = levelswitch,
                    IqSwitch = iqswitch,
                    AudioSwitch = audioswitch,
                    IfSwitch = ifswitch
                };
                SendCmd($"FREQ:DDC{i + 1} {frequency} MHz");
                SendCmd($"BAND:DDC{i + 1} {filterBw} kHz");
                SendCmd($"DEM:DDC{i + 1} {demodulation}");
                SendCmd($"OUTP:SQU:DDC{i + 1}:THR {sqc} dbuV");
                SendCmd($"OUTP:SQU:DDC{i + 1} {sqcswitch}");
                if (ifswitch)
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 1 : 0)}");
                }
                else
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
                }

                _preChannels.Add(channel);
            }
            else
            {
                //和之前的对比
                if (!frequency.EqualTo(_preChannels[i].Frequency) || !filterBw.EqualTo(_preChannels[i].FilterBandwidth))
                {
                    SendCmd($"FREQ:DDC{i + 1} {frequency} MHz");
                    _preChannels[i].Frequency = frequency;
                    //任何时候中心频率与带宽都要同时下发，不然解调的音频频率与下发的频率不一致
                    SendCmd($"BAND:DDC{i + 1} {filterBw} kHz");
                    _preChannels[i].FilterBandwidth = filterBw;
                }

                if (demodulation != _preChannels[i].DemMode)
                {
                    SendCmd($"DEM:DDC{i + 1} {demodulation}");
                    _preChannels[i].DemMode = demodulation;
                }

                if (!sqc.EqualTo(_preChannels[i].SquelchThreshold))
                {
                    SendCmd($"OUTP:SQU:DDC{i + 1}:THR {sqc} dbuV");
                    _preChannels[i].SquelchThreshold = sqc;
                }

                if (!sqcswitch.Equals(_preChannels[i].SquelchSwitch))
                {
                    SendCmd($"OUTP:SQU:DDC{i + 1} {sqcswitch}");
                    _preChannels[i].SquelchSwitch = sqcswitch;
                }

                if (iqswitch != _preChannels[i].IqSwitch)
                {
                    if (ifswitch)
                        SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                    _preChannels[i].IqSwitch = iqswitch;
                }

                if (levelswitch != _preChannels[i].LevelSwitch) _preChannels[i].LevelSwitch = levelswitch;
                if (audioswitch != _preChannels[i].AudioSwitch)
                {
                    if (ifswitch) SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 2 : 0)}");
                    _preChannels[i].AudioSwitch = audioswitch;
                }

                if (ifswitch)
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE {(iqswitch ? "SHORT" : "OFF")}");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {(audioswitch ? 1 : 0)}");
                }
                else
                {
                    SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
                    SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
                }

                _preChannels[i].IfSwitch = ifswitch;
            }
        }
    }

    /// <summary>
    ///     重置DDC子通道
    /// </summary>
    private void ResetDdcPath()
    {
        for (var i = 0; i < 4; ++i)
        {
            SendCmd($"SYST:IF:DDC{i + 1}:REM:MODE OFF");
            SendCmd($"SYST:AUD:DDC{i + 1}:REM:MOD {0}");
        }
    }

    #endregion
}