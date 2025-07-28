using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

public partial class Ddf255
{
    private void CaptureDataPacket()
    {
        var socket = _dataChannel;
        var queue = _dataQueue;
        var buffer = new byte[1024 * 1024];
        socket.ReceiveBufferSize = buffer.Length;
        while (_dataCaptureCts?.IsCancellationRequested == false)
            try
            {
                //await Task.Delay(1).ConfigureAwait(false);
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    private void CaptureDdcPacket()
    {
        var socket = _ddcChannel;
        var queue = _ddcQueue;
        var buffer = new byte[1024 * 1024];
        socket.ReceiveBufferSize = buffer.Length;
        while (_ddcCaptureCts?.IsCancellationRequested == false)
            try
            {
                //await Task.Delay(1).ConfigureAwait(false);
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
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
    }

    private void DispatchDataPacket()
    {
        var queue = _dataQueue;
        //var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PscanData.txt");
        while (_dataDispatchCts?.IsCancellationRequested == false)
            try
            {
                if (queue.IsEmpty)
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

                //using (var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                //{
                //    fs.Seek(fs.Length, SeekOrigin.Begin);
                //    fs.Write(buffer);
                //}
                var packet = RawPacket.Parse(buffer, 0, FirmwareVersion);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (OperationCanceledException)
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

    private void DispatchDdcPacket()
    {
        var queue = _ddcQueue;
        while (_ddcDispatchCts?.IsCancellationRequested == false)
            try
            {
                if (queue.IsEmpty)
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

                var packet = RawPacket.Parse(buffer, 0, FirmwareVersion);
                if (packet == null || packet.DataCollection.Count == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                ForwardPacket(packet);
            }
            catch (OperationCanceledException)
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
            switch ((DataType)data.Tag)
            {
                case DataType.Audio:
                    obj = ToAudio(data as RawAudio);
                    break;
                case DataType.Ifpan:
                    obj = ToSpectrum(data as RawIfPan);
                    break;
                case DataType.If:
                    obj = ToIq(data as RawIf);
                    break;
                case DataType.Fscan:
                    obj = ToFScan(data as RawFScan);
                    break;
                case DataType.Mscan:
                    obj = ToMScan(data as RawMScan);
                    break;
                case DataType.Pscan:
                    obj = ToPScan(data as RawPScan);
                    break;
                case DataType.DfPan:
                    obj = ToDfPan(data as RawDfPan);
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
        if (result.Count > 0 && TaskState == TaskState.Start)
            //result.Add(_deviceId);
            SendData(result);
    }

    private void DispatchLevelData()
    {
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送，否则界面参数列表刷新时也会出现卡顿
        //此处处理为2秒左右发送一次到客户端
        var startTime = DateTime.Now;
        var count = 10;
        while (_levelDataTokenSource?.IsCancellationRequested == false)
            try
            {
                //发送数据缓存列表
                var data = new List<object>();
                count++;
                if (count >= 10)
                {
                    // 获取电子罗盘信息
                    if (_useCompass > 0)
                    {
                        var result = SendSyncCmd($"SYSTem:COMPass:DATA? \"{_compassName}\"");
                        if (result.Length > 2)
                        {
                            var valueCompass = float.Parse(result.Split(',')[0]);
                            var dataCompass = new SDataCompass
                            {
                                Heading = valueCompass
                            };
                            SendMessageData(new List<object> { dataCompass });
                        }
                    }

                    // 获取GPS数据信息
                    if (UseGps)
                    {
                        var result = SendSyncCmd("SYSTem:GPS:DATA?");
                        var gpsInfos = result.Split(',');
                        var dataGps = new SDataGps
                        {
                            Latitude = double.Parse(gpsInfos[6]) + double.Parse(gpsInfos[7]) / 60 +
                                       double.Parse(gpsInfos[8]) / 3600,
                            Longitude = double.Parse(gpsInfos[10]) + double.Parse(gpsInfos[11]) / 60 +
                                        double.Parse(gpsInfos[12]) / 3600,
                            Heading = float.Parse(gpsInfos[21]) / 10000 * 90,
                            Satellites = int.Parse(gpsInfos[4]),
                            Speed = (ushort)(double.Parse(gpsInfos[19]) * 1.852 / 100),
                            Altitude = float.Parse(gpsInfos[^1])
                        };
                        SendMessageData(new List<object> { dataGps });
                    }

                    count = 0;
                }

                // 获取电平数据和ITU信息
                if ((_media & (MediaType.Level | MediaType.Itu)) > 0 && (_media & MediaType.Scan) == 0)
                {
                    var result = SendSyncCmd("SENS:DATA?");
                    var values = result.Split(',');
                    //解析本次电平值
                    var lev = float.Parse(values[0]);
                    if ((_media & MediaType.Level) > 0)
                        if (lev > -999f)
                        {
                            var dataLevel = new SDataLevel
                            {
                                Data = lev,
                                Frequency = _frequency,
                                Bandwidth = _filterBandwidth
                            };
                            data.Add(dataLevel);
                        }

                    //解析ITU数据,3秒发送一次
                    if (values.Length >= 9 && (_media & MediaType.Itu) > 0)
                    {
                        var ts = DateTime.Now - startTime;
                        if (ts.TotalMilliseconds >= 3000)
                        {
                            var am = double.Parse(values[1]);
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
                            if (_bandMeasureMode == "XDB")
                            {
                                var value = bw < -1000000000f ? double.MinValue : bw;
                                dataItu.Misc.Add(ParameterNames.ItuXdb, value);
                            }
                            else
                            {
                                var value = bw < -1000000000f ? double.MinValue : bw;
                                dataItu.Misc.Add(ParameterNames.ItuBeta, value);
                            }

                            data.Add(dataItu);
                            startTime = DateTime.Now; //保存最新的时间
                        }
                    }
                }

                if (data?.Count > 0 && TaskState == TaskState.Start) SendData(data);
                Thread.Sleep(30);
            }
            catch (OperationCanceledException)
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

    /// <summary>
    ///     采集子通道电平数据
    /// </summary>
    private void DispatchDdcData()
    {
        while (_ddcDataDispatchTokenSource?.IsCancellationRequested == false)
            try
            {
                Thread.Sleep(1);
                if (CurFeature != FeatureType.IFMCA)
                {
                    Thread.Sleep(100);
                    continue;
                }

                if (_preChannels == null) continue;
                var chans = _preChannels.ToArray();
                for (var i = 0; i < chans.Length; ++i)
                    if (chans[i].IfSwitch && chans[i].LevelSwitch)
                    {
                        var buffer = Encoding.ASCII.GetBytes($"SENS:DATA:DDC{i + 1}?\n");
                        _ddcCtrlChannel.Send(buffer);
                        var result = RecvIfmchLevels('\n');
                        var dataLevel = new SDataLevel
                        {
                            Frequency = chans[i].Frequency,
                            Bandwidth = chans[i].FilterBandwidth
                        };
                        var level = float.Parse(result);
                        if (level < -200f) continue;
                        dataLevel.Data = level;
                        var dataMchChannel = new SDataDdc
                        {
                            ChannelNumber = i,
                            Data = new List<object> { dataLevel }
                        };
                        SendData(new List<object> { dataMchChannel });
                    }
            }
            catch (OperationCanceledException)
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

    #region 解析业务数据

    /// <summary>
    ///     解析音频数据
    /// </summary>
    /// <param name="data"></param>
    private object ToAudio(RawAudio data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6,当大于6时，表明该包数据无效
        if (data.FrequencyHigh > 6) return null;
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
        if (data.ChannelNumber > 0)
        {
            var list = new List<object> { audio };
            var dataMchChannel = new SDataDdc
            {
                ChannelNumber = data.ChannelNumber - 1,
                Data = list
            };
            return dataMchChannel;
        }

        return audio;
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    /// <param name="data"></param>
    private object ToSpectrum(RawIfPan data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6,当大于6时，表明该包数据无效
        if (data.FrequencyHigh > 6) return null;
        // var spectrum = new float[data.NumberOfTraceItems];
        // for (int i = 0; i < spectrum.Length; i++)
        // {
        //     spectrum[i] = data.DataCollection[i] / 10f;
        // }
        var dataSpectrum = new SDataSpectrum();
        var freq = (((long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d;
        dataSpectrum.Frequency = freq;
        dataSpectrum.Span = data.SpanFrequency / 1000d;
        dataSpectrum.Data = data.DataCollection;
        return dataSpectrum;
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    /// <param name="data"></param>
    private object ToIq(RawIf data)
    {
        if (data == null) return null;
        //校验数据有效性,最高频率到26.5G时 FrequencyHigh = 6,当大于6时，该包数据无效
        if (data.FrequencyHigh > 6) return null;
        var dataIq = new SDataIq();
        var freq = (((long)data.FrequencyHigh << 32) + data.FrequencyLow) / 1000000d;
        dataIq.Frequency = freq;
        dataIq.Bandwidth = data.Bandwidth / 1000d;
        dataIq.SamplingRate = data.Samplerate / 1000d;
        dataIq.Attenuation = data.RxAtt;
        //如果是子通道
        if (data.ChannelNumber > 0)
        {
            if (data.Mode == 1)
                dataIq.Data16 = Array.ConvertAll(data.DataCollection, item => (short)item);
            else if (data.Mode == 2)
                dataIq.Data32 = data.DataCollection;
            else
                return null;
            var dataMchChannel = new SDataDdc
            {
                Data = new List<object> { dataIq },
                ChannelNumber = data.ChannelNumber - 1
            };
            return dataMchChannel;
        }

        //如果是主通道
        if (data.NumberOfTraceItems > 0)
        {
            dataIq.Data16 = Array.ConvertAll(data.DataCollection, item => (short)item);
            return dataIq;
        }

        return null;
    }

    /// <summary>
    ///     解析FSCAN扫描数据
    /// </summary>
    /// <param name="data"></param>
    private object ToFScan(RawFScan data)
    {
        if (data == null) return null;
        var levels = data.DataCollection;
        // for (int i = 0; i < data.NumberOfTraceItems; i++)
        // {
        //     levels[i] = data.DataCollection[i] / 10f;
        // }
        var freqsLow = data.FreqLowCollection;
        var freqsHigh = data.FreqHighCollection;
        //有可能包含多帧数据（当测量时间极小，频段个数极少时）
        var tempLevels = new List<short>();
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var j = 0;
        for (; j < data.NumberOfTraceItems; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
                var index = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
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
        var currfreq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
        var currIndex = Utils.GetCurrIndex(currfreq, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var dataScan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = tempLevels.ToArray()
        };
        return dataScan;
    }

    /// <summary>
    ///     解析PSCAN扫描数据
    /// </summary>
    /// <param name="data"></param>
    private object ToPScan(RawPScan data)
    {
        if (data?.DataCollection == null) return null;
        return FirmwareVersion switch
        {
            FirmwareVersion.OldSubscribe => ToOldSubcribePScan(data),
            FirmwareVersion.Old => ToOldPScan(data),
            _ => ToNewPScan(data)
        };
    }

    private object ToNewPScan(RawPScan data)
    {
        var scandata = new List<short>
        {
            Capacity = data.NumberOfTraceItems
        };
        scandata.AddRange(data.DataCollection);
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var currIndex = Utils.GetCurrIndex(data.FreqOfFirstStep / 1000000d, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        //表示本次扫描结束,此处仅补齐缺少的点
        if (scandata.Last() == 2000)
        {
            scandata.RemoveAt(scandata.Count - 1);
            for (var i = currIndex + scandata.Count; i < total; ++i) scandata.Add(0);
        }

        var dataScan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = scandata.ToArray()
        };
        return dataScan;
    }

    private object ToOldPScan(RawPScan data)
    {
        var scandata = new List<short>
        {
            Capacity = data.NumberOfTraceItems
        };
        scandata.AddRange(data.DataCollection);
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
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

    private object ToOldSubcribePScan(RawPScan data)
    {
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        var levels = data.DataCollection;
        var freqsLow = data.FreqLowCollection;
        var freqsHigh = data.FreqHighCollection;
        if (freqsHigh == null || freqsLow == null) return null;
        //有可能包含多帧数据（当测量时间极小，频段个数极少时）
        var tempLevels = new List<short>();
        var j = 0;
        for (; j < data.NumberOfTraceItems; ++j)
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
                var index = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
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
        var currfreq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
        var currIndex = Utils.GetCurrIndex(currfreq, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var dataScan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Offset = currIndex,
            Total = total,
            Data = tempLevels.ToArray()
        };
        return dataScan;
    }

    /// <summary>
    ///     解析离散扫描数据
    /// </summary>
    /// <param name="data"></param>
    private object ToMScan(RawMScan data)
    {
        if (data?.DataCollection == null) return null;
        var levels = data.DataCollection;
        var frequencieslow = data.FreqLowCollection;
        var frequencieshigh = data.FreqHighCollection;
        if (frequencieslow == null || frequencieshigh == null) return null;
        var j = 0;
        var tempLevels = new List<short>();
        for (; j < data.NumberOfTraceItems; ++j)
            //设备会返回以200为该包数据截止的无效数据
            if (levels[j] != 2000)
            {
                tempLevels.Add(levels[j]);
            }
            else
            {
                //完成一次完整扫描
                var freq =
                    (((long)frequencieshigh[j - tempLevels.Count] << 32) + frequencieslow[j - tempLevels.Count]) /
                    1000000d;
                var currIndex = _scanFreqs.IndexOf(freq);
                if (currIndex >= 0)
                {
                    var scan = new SDataScan
                    {
                        Offset = currIndex,
                        Total = _scanFreqs.Count,
                        Data = tempLevels.ToArray()
                    };
                    if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                }

                tempLevels.Clear();
            }

        if (tempLevels.Count > 0)
        {
            var freq = (((long)frequencieshigh[j - tempLevels.Count] << 32) + frequencieslow[j - tempLevels.Count]) /
                       1000000d;
            var currIndex = _scanFreqs.IndexOf(freq);
            if (currIndex >= 0)
            {
                var dataScan = new SDataScan
                {
                    Offset = currIndex,
                    Total = _scanFreqs.Count,
                    Data = tempLevels.ToArray()
                };
                return dataScan;
            }
        }

        return null;
    }

    /// <summary>
    ///     解析测向数据(宽带测向、单频测向)
    /// </summary>
    /// <param name="data"></param>
    private object ToDfPan(RawDfPan data)
    {
        if (data?.LevelCollection == null || data.AzimuthCollection == null ||
            data.QualityCollection == null) return null;
        var pLevel = data.LevelCollection;
        var pAzimuth = new float[data.NumberOfTraceItems];
        var pQuality = new float[data.NumberOfTraceItems];
        for (var index = 0; index < data.NumberOfTraceItems; ++index)
        {
            // pLevel[index] = data.LevelCollection[index] / 10.0f;
            var azimuth = data.AzimuthCollection[index];
            //当门限过大，会返回32766无效数据
            pAzimuth[index] = azimuth >= short.MaxValue - 1 ? float.MinValue : azimuth / 10.0f;
            var quality = data.QualityCollection[index];
            //当门限过大，会返回32766无效数据
            pQuality[index] = quality >= short.MaxValue - 1 ? float.MinValue : quality / 10.0f;
        }

        var freq = data.FreqLow | ((long)data.FreqHigh << 32);
        var freqIndex = data.DemodFreqChannel;
        var frequency = freq / 1000000d;
        if ((_media & (MediaType.Dfind | MediaType.Dfpan)) > 0)
        {
            var result = new List<object>();
            if ((_media & MediaType.Level) > 0)
            {
                var dataLevel = new SDataLevel
                {
                    Data = pLevel[freqIndex] / 10f,
                    Frequency = _frequency,
                    Bandwidth = _filterBandwidth
                };
                result.Add(dataLevel);
            }

            if ((_media & MediaType.Spectrum) > 0)
            {
                var dataSpectrum = new SDataSpectrum
                {
                    Span = IfBandwidth, // _curFeature == FeatureType.FFDF ? _fixdfSpectrumSpan : SpectrumSpan;
                    Frequency = frequency,
                    Data = pLevel
                };
                result.Add(dataSpectrum);
            }

            if ((_media & MediaType.Dfind) > 0)
            {
                var dataDFind = new SDataDfind
                {
                    Frequency = frequency,
                    BandWidth = data.DfBandWidth / 1000d,
                    Azimuth = AngleAdjust(pAzimuth[freqIndex], frequency),
                    Quality = pQuality[freqIndex]
                };
                if (dataDFind.Quality > QualityThreshold) result.Add(dataDFind);
            }

            if ((_media & MediaType.Dfpan) > 0)
            {
                var dataWbdf = new SDataDfpan
                {
                    Frequency = frequency,
                    Span = _dfBandwidth,
                    Azimuths = AngleAdjust(pAzimuth, frequency),
                    Qualities = pQuality
                };
                result.Add(dataWbdf);
            }

            return result;
        }

        return null;
    }

    #endregion
}