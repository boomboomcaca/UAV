using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF200M;

public partial class Ddf200M
{
    private object ToAudio(byte[] buffer, int offset)
    {
        var trace = new TraceAttribute(buffer, offset);
        offset += Marshal.SizeOf(typeof(TraceAttribute));
        var header = new OptionalHeaderAudio(buffer, offset);
        offset += trace.optional_header_length;
        //校验数据有效性
        if (header.FrequencyHigh != 0) return null;
        var data = new byte[trace.number_of_trace_items * 2];
        Buffer.BlockCopy(buffer, offset, data, 0, data.Length);
        Consts.AfModes.TryGetValue(header.AudioMode, out var afMode);
        if (afMode == null) return null;
        var audio = new SDataAudio
        {
            Format = AudioFormat.Pcm,
            SamplingRate = afMode.SamplingRate * 1000,
            BytesPerSecond = afMode.DataRate * 1000,
            BitsPerSample = afMode.BitsPerSample,
            BlockAlign = afMode.LengthPerFrame,
            Channels = afMode.Channels,
            Data = data
        };
        return audio;
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToSpectrum(byte[] buffer, int offset)
    {
        try
        {
            var trace = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var header = new OptionalHeaderIfPan(buffer, offset);
            offset += trace.optional_header_length;
            //校验数据有效性
            if (header.freqhigh != 0) return null;
            var spectrum = new short[trace.number_of_trace_items];
            for (var i = 0; i < spectrum.Length; ++i)
            {
                spectrum[i] = BitConverter.ToInt16(buffer, offset);
                offset += 2;
                if (spectrum[i] >= 201) //过滤掉设备偶发性的发送很大的无效数据，由于静噪门限最大130dB，200为设备每包数据分隔符，此处将201以上的数据屏蔽掉
                    return null;
            }

            var dataSpec = new SDataSpectrum
            {
                Frequency = header.freqlow / 1000000d,
                Span = header.spanFrequency / 1000d,
                Data = spectrum
            };
            return dataSpec;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToIq(byte[] buffer, int offset)
    {
        try
        {
            var trace = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var header = new OptionalHeaderIf(buffer, offset);
            offset += trace.optional_header_length;
            //校验数据有效性
            if (header.FrequencyHigh != 0) return null;
            var iq = new short[trace.number_of_trace_items * 2];
            Buffer.BlockCopy(buffer, offset, iq, 0, trace.number_of_trace_items * 4);
            var dataIq = new SDataIq
            {
                Frequency = header.FrequencyLow / 1000000d,
                Bandwidth = header.Bandwidth / 1000d,
                SamplingRate = header.Samplerate / 1000000d,
                Attenuation = header.RxAttenuation,
                Data16 = iq
            };
            return dataIq;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     解析FSCAN扫描数据
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToFScan(byte[] buffer, int offset)
    {
        try
        {
            var flag = Flags.Level;
            var pCommon = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var optionalHeaderFScan = new OptionalHeaderFScan(buffer, offset);
            offset += pCommon.optional_header_length;
            var levels = new short[pCommon.number_of_trace_items];
            var freqsLow = new uint[pCommon.number_of_trace_items];
            var freqsHigh = new uint[pCommon.number_of_trace_items];
            while (flag != Flags.OptionalHeader)
            {
                if (((uint)pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            offset += 2 * pCommon.number_of_trace_items;
                            break;
                        case Flags.Freqlow:
                            for (var i = 0; i < freqsLow.Length; i++)
                                freqsLow[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            offset += 4 * pCommon.number_of_trace_items;
                            break;
                        case Flags.Freqhigh:
                            for (var i = 0; i < freqsHigh.Length; i++)
                                freqsHigh[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            offset += 4 * pCommon.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            //有可能包含多帧数据（当测量时间极小，频段个数极少时）
            var tempLevels = new List<short>();
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            var j = 0;
            for (; j < pCommon.number_of_trace_items; ++j)
                if (Math.Abs(levels[j] - 200f) > 1e-9)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) /
                               1000000d;
                    var currIndex = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
                    if (currIndex >= 0)
                    {
                        for (var k = currIndex + tempLevels.Count; k < total; ++k) tempLevels.Add(0);
                        var scan = new SDataScan
                        {
                            StartFrequency = StartFrequency,
                            StopFrequency = StopFrequency,
                            StepFrequency = StepFrequency,
                            Offset = currIndex,
                            Total = total,
                            Data = tempLevels.ToArray()
                        };
                        if (TaskState == TaskState.Start) SendData(new List<object> { scan });
                    }

                    tempLevels.Clear();
                }

            if (tempLevels.Count > 0)
            {
                var freq = (((long)freqsHigh[j - tempLevels.Count] << 32) + freqsLow[j - tempLevels.Count]) / 1000000d;
                var currIndex = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
                if (currIndex >= 0)
                {
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
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     解析PSCAN扫描数据
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToPScan(byte[] buffer, int offset)
    {
        try
        {
            var shorts = new List<short>();
            var scandata = shorts;
            var pCommon = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var opt = new OptionalHeaderPScan(buffer, offset);
            offset += pCommon.optional_header_length;
            scandata.Capacity = pCommon.number_of_trace_items;
            var flag = Flags.Level;
            var currIndex = Utils.GetCurrIndex(opt.firstFreq / 1000000d, StartFrequency, StepFrequency);
            while (flag != Flags.OptionalHeader)
            {
                if (((uint)pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < pCommon.number_of_trace_items; i++)
                                scandata.Add(BitConverter.ToInt16(buffer, i * 2 + offset));
                            offset += 2 * pCommon.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            if (currIndex < 0) return null;
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            //表示本次扫描结束,此处仅补齐缺少的点
            if (Math.Abs(scandata.Last() - 200f) < 1e-9)
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
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     解析离散扫描数据
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToMScan(byte[] buffer, int offset)
    {
        try
        {
            var flag = Flags.Level;
            var trace = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var optionalHeaderMScan = new OptionalHeaderMScan(buffer, offset);
            offset += trace.optional_header_length;
            var levels = new short[trace.number_of_trace_items];
            var frequencieslow = new uint[trace.number_of_trace_items];
            var frequencieshigh = new uint[trace.number_of_trace_items];
            while (flag != Flags.OptionalHeader)
            {
                if (((uint)trace.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            offset += 2 * trace.number_of_trace_items;
                            break;
                        case Flags.Freqlow:
                            for (var i = 0; i < frequencieslow.Length; i++)
                                frequencieslow[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            offset += 4 * trace.number_of_trace_items;
                            break;
                        case Flags.Freqhigh:
                            for (var i = 0; i < frequencieshigh.Length; i++)
                                frequencieshigh[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            offset += 4 * trace.number_of_trace_items;
                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            var j = 0;
            var tempLevels = new List<short>();
            for (; j < trace.number_of_trace_items; ++j)
                //设备会返回以200为该包数据截止的无效数据
                if (Math.Abs(levels[j] - 200f) > 1e-9)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var freq = (((long)frequencieshigh[j - tempLevels.Count] << 32) +
                                frequencieslow[j - tempLevels.Count]) / 1000000d;
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
                var freq =
                    (((long)frequencieshigh[j - tempLevels.Count] << 32) + frequencieslow[j - tempLevels.Count]) /
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
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     解析测向数据(宽带测向、单频测向)
    /// </summary>
    /// <param name="buffer">设备接收数据</param>
    /// <param name="offset">偏移量</param>
    private object ToDfPan(byte[] buffer, int offset)
    {
        try
        {
            var pCommon = new TraceAttribute(buffer, offset);
            offset += Marshal.SizeOf(typeof(TraceAttribute));
            var opt = new OptionalHeaderDfPan(buffer, offset);
            offset += pCommon.optional_header_length;
            //int dffstrength = opt.AntennaFactor;
            var pLevel = new short[pCommon.number_of_trace_items];
            var pAzimuth = new float[pCommon.number_of_trace_items];
            var pQuality = new float[pCommon.number_of_trace_items];
            var flag = Flags.DfLevel;
            while (flag != Flags.OptionalHeader)
            {
                if ((pCommon.selectorFlags & (uint)flag) > 0)
                    switch (flag)
                    {
                        case Flags.DfLevel:
                            for (var i = 0; i < pLevel.Length; i++)
                            {
                                pLevel[i] = BitConverter.ToInt16(buffer, offset);
                                offset += 2;
                            }

                            break;
                        case Flags.Azimuth:
                            for (var i = 0; i < pAzimuth.Length; i++)
                            {
                                pAzimuth[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                                if (pAzimuth[i] >= 3276.6) //当门限过大，会返回3276.6无效数据
                                    pAzimuth[i] = float.MinValue;
                                offset += 2;
                            }

                            break;
                        case Flags.DfQuality:
                            for (var i = 0; i < pQuality.Length; i++)
                            {
                                pQuality[i] = BitConverter.ToInt16(buffer, offset) / 10f;
                                if (pQuality[i] >= 3276.6) //当门限过大，会返回3276.6无效数据
                                    pQuality[i] = float.MinValue;
                                offset += 2;
                            }

                            break;
                    }

                flag = (Flags)((uint)flag << 1);
            }

            var freq = opt.Freq_low | ((long)opt.Freq_high << 32);
            var freqIndex = opt.DemodFreqChannel;
            var frequency = freq / 1000000d;
            if ((_media & (MediaType.Dfind | MediaType.Dfpan)) > 0)
            {
                var result = new List<object>();
                if ((_media & MediaType.Level) > 0)
                {
                    var dataLevel = new SDataLevel
                    {
                        Data = pLevel[freqIndex],
                        Frequency = _frequency,
                        Bandwidth = _ifbandwidth
                    };
                    result.Add(dataLevel);
                }

                if ((_media & MediaType.Spectrum) > 0)
                {
                    var dataSpectrum = new SDataSpectrum
                    {
                        Span = CurFeature == FeatureType.FFDF ? _fixdfSpectrumSpan : SpectrumSpan,
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
                        BandWidth = _dfBandWidth,
                        Azimuth = pAzimuth[freqIndex],
                        Quality = pQuality[freqIndex]
                    };
                    result.Add(dataDFind);
                }

                if ((_media & MediaType.Dfpan) > 0)
                {
                    var dataWbdf = new SDataDfpan
                    {
                        Frequency = frequency,
                        Span = _spectrumspan,
                        Azimuths = pAzimuth,
                        Qualities = pQuality
                    };
                    result.Add(dataWbdf);
                }

                return result;
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}