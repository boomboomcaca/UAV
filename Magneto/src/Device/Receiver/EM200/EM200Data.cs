using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class Em200
{
    internal class AfMode
    {
        public AfMode()
        {
        }

        public AfMode(int mode, int samplingRate, int bitsPerSample, int channels, int dataRate, int lengthPerFrame)
        {
            Mode = mode;
            SamplingRate = samplingRate;
            BitsPerSample = bitsPerSample;
            Channels = channels;
            DataRate = dataRate;
            LengthPerFrame = lengthPerFrame;
        }

        /// <summary>
        ///     模式
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        ///     采样率 kHz
        /// </summary>
        public int SamplingRate { get; set; }

        /// <summary>
        ///     采样位数
        /// </summary>
        public int BitsPerSample { get; set; }

        /// <summary>
        ///     通道数
        /// </summary>
        public int Channels { get; set; }

        /// <summary>
        ///     数据传输率 kbyte/s
        /// </summary>
        public int DataRate { get; set; }

        /// <summary>
        ///     块大小
        /// </summary>
        public int LengthPerFrame { get; set; }
    }

    #region 线程实现方法

    /// <summary>
    ///     TCP获取电平和ITU信息的线程函数
    /// </summary>
    /// <param name="obj"></param>
    public async Task TcpDataProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        //"VOLT:AC", "AM", "AM:POS", "AM:NEG", "FM", "FM:POS", "FM:NEG", "PM", "BAND"
        //设备的ITU数据大概5秒左右更新一次，因此不需要每次和电平值一起发送
        //否则界面参数列表刷新时也会出现卡顿,此处处理为3秒左右发送一次到客户端
        var sendTime = DateTime.MinValue;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(10).ConfigureAwait(false);
                //当前未订阅电平或者ITU数据则直接返回
                if ((_mediaType & (MediaType.Level | MediaType.Itu)) == 0)
                {
                    //释放时间片，促进线程切换
                    await Task.Delay(1).ConfigureAwait(false);
                    continue;
                }

                //发送数据缓存列表
                List<object> datas = new();
                //获取电平和ITU数据
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                if ((_mediaType & MediaType.Level) > 0)
                {
                    _level = float.Parse(values[0]);
                    if ((_mediaType & MediaType.Spectrum) == 0)
                    {
                        //频谱开关关闭时在此发送否则和频谱一起发送
                        SDataLevel dataLevel = new()
                        {
                            Frequency = _frequency,
                            Bandwidth = _filterBandwidth,
                            Data = _level
                        };
                        datas.Add(dataLevel);
                        Thread.Sleep(5);
                    }
                }

                if ((_mediaType & MediaType.Itu) > 0 && values.Length >= 9)
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
            }
            catch (Exception ex)
            {
                if (ex is SocketException) break;
            }
    }

    /// <summary>
    ///     UDP读取数据线程函数
    /// </summary>
    /// <param name="obj"></param>
    public void UdpDataProcess(object obj)
    {
        if (obj is not CancellationToken token) return;
        var buffer = new byte[1024 * 1024];
        _dataSocket.ReceiveBufferSize = buffer.Length;
        //new IPEndPoint(IPAddress.Any, 0);
        while (!token.IsCancellationRequested)
            try
            {
                // await Task.Delay(1).ConfigureAwait(false);
                var recvBytes = _dataSocket.Receive(buffer);
                if (recvBytes <= 0) break;
                var recvData = new byte[recvBytes];
                Buffer.BlockCopy(buffer, 0, recvData, 0, recvBytes);
                if (TaskState == TaskState.Start) _udpDataQueue.Enqueue(recvData);
            }
            catch (Exception ex)
            {
                if (ex is SocketException) break;
            }
    }

    /// <summary>
    ///     UDP数据处理线程函数
    /// </summary>
    /// <param name="obj"></param>
    private async Task UdpDataConvertProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
        {
            if (_udpDataQueue.IsEmpty)
            {
                await Task.Delay(1).ConfigureAwait(false);
                continue;
            }

            if (!_udpDataQueue.TryDequeue(out var data))
            {
                await Task.Delay(1).ConfigureAwait(false);
                continue;
            }

            try
            {
                var offset = Marshal.SizeOf(typeof(Eb200DatagramFormat));
                List<object> sendDatas = new();
                while (offset < data.Length)
                {
                    IGenericAttribute ga = new GenericAttributeConventional(data, offset);
                    if (ga.TraceTag > 5000)
                    {
                        //大于5000的都用高级的数据包
                        ga = new GenericAttributeAdvanced(data, offset);
                        offset += Marshal.SizeOf(typeof(GenericAttributeAdvanced));
                    }
                    else
                    {
                        offset += Marshal.SizeOf(typeof(GenericAttributeConventional));
                    }

                    object value = null;
                    switch (ga.TraceTag)
                    {
                        case (int)Tags.Audio:
                            value = ToAudio(data, offset);
                            break;
                        case (int)Tags.Ifpan:
                            value = ToSpectrum(data, offset);
                            break;
                        case (int)Tags.Fscan:
                            value = ToFScan(data, offset);
                            break;
                        case (int)Tags.Pscan:
                            value = ToPScan(data, offset);
                            break;
                        case (int)Tags.Mscan:
                            value = ToMScan(data, offset);
                            break;
                        case (int)Tags.If:
                            value = ToIq(data, offset);
                            break;
                    }

                    if (value != null)
                    {
                        if (value is List<object> list)
                            sendDatas.AddRange(list);
                        else
                            sendDatas.Add(value);
                    }

                    offset += (int)ga.DataLength;
                }

                if (sendDatas.Count > 0 && TaskState == TaskState.Start)
                {
                    sendDatas.RemoveAll(x => x == null);
                    SendData(sendDatas);
                }
            }
            catch
            {
            }
        }
    }

    #endregion

    #region 解析业务数据

    private static readonly Dictionary<int, AfMode> _afModes = new()
    {
        { 0, new AfMode(0, 0, 0, 0, 0, 0) },
        { 1, new AfMode(1, 32, 16, 2, 128, 4) },
        { 2, new AfMode(2, 32, 16, 1, 64, 2) },
        { 3, new AfMode(3, 32, 8, 2, 64, 2) },
        { 4, new AfMode(4, 32, 8, 1, 32, 1) },
        { 5, new AfMode(5, 16, 16, 2, 64, 4) },
        { 6, new AfMode(6, 16, 16, 1, 32, 2) },
        { 7, new AfMode(7, 16, 8, 2, 32, 2) },
        { 8, new AfMode(8, 16, 8, 1, 16, 1) },
        { 9, new AfMode(9, 8, 16, 2, 32, 4) },
        { 10, new AfMode(10, 8, 16, 1, 16, 2) },
        { 11, new AfMode(11, 8, 8, 2, 16, 2) },
        { 12, new AfMode(12, 8, 8, 1, 8, 1) }
    };

    /// <summary>
    ///     解析音频数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>音频类数据</returns>
    private static object ToAudio(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderConventional pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderConventional));
            OptionalHeaderAudio header = new(buffer, offset);
            offset += pCommon.Optional_header_length;
            var pAudioData = new byte[pCommon.Number_of_trace_items * 2];
            for (var i = 0; i < pCommon.Number_of_trace_items; i++)
            {
                pAudioData[i * 2] = buffer[offset + i * 2 + 1];
                pAudioData[i * 2 + 1] = buffer[offset + i * 2];
            }

            // Buffer.BlockCopy(buffer, offset, pAudioData, 0, pAudioData.Length);
            if (!_afModes.TryGetValue(header.AudioMode, out var afMode)) return null;
            SDataAudio dataAudio = new()
            {
                Format = AudioFormat.Pcm,
                SamplingRate = afMode.SamplingRate * 1000,
                BytesPerSecond = afMode.DataRate * 1000,
                BitsPerSample = afMode.BitsPerSample,
                BlockAlign = afMode.LengthPerFrame,
                Channels = afMode.Channels,
                Data = pAudioData
            };
            return dataAudio;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析IQ数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>IQ类数据</returns>
    private object ToIq(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderConventional pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderConventional));
            //new(buffer, offset);
            offset += pCommon.Optional_header_length;
            if (pCommon.Number_of_trace_items == 0) return null;
            var iq = new short[pCommon.Number_of_trace_items * 2];
            for (var i = 0; i < iq.Length; i++)
            {
                Array.Reverse(buffer, offset + i * 2, 2);
                iq[i] = BitConverter.ToInt16(buffer, offset + i * 2);
            }

            //Buffer.BlockCopy(buffer, offset, iq, 0, (int)pCommon.Number_of_trace_items * 4);
            //var freq = (((ulong)header.FrequencyHigh) << 32) + header.FrequencyLow;
            SDataIq dataIq = new()
            {
                Frequency = _frequency, //freq / 1000000d;
                Bandwidth = _filterBandwidth, //header.Bandwidth / 1000d;
                SamplingRate = 0, //header.Samplerate / 1000000d;
                Attenuation = 0, //header.RxAtt;
                Data16 = iq
            };
            return dataIq;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析频谱数据(单频测量使用)
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>音频类数据</returns>
    private object ToSpectrum(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderAdvanced pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderAdvanced));
            var sign = pCommon.Optional_header_length == Marshal.SizeOf(typeof(OptionalHeaderIfPan));
            OptionalHeaderIfPan header = new(buffer, offset, sign);
            offset += (int)pCommon.Optional_header_length;
            var spectrum = new short[pCommon.Number_of_trace_items];
            for (var i = 0; i < spectrum.Length; i++)
            {
                Array.Reverse(buffer, offset, 2);
                spectrum[i] = BitConverter.ToInt16(buffer, offset);
                if (spectrum[i] > 1200 || spectrum[i] < -9990) return null;
                offset += 2;
            }

            List<object> datas = new();
            SDataSpectrum dataSpectrum = new();
            var freq = ((long)header.FrequencyHigh << 32) + header.FrequencyLow;
            dataSpectrum.Frequency = freq / 1000000d;
            dataSpectrum.Span = header.SpanFrequency / 1000d;
            dataSpectrum.Data = spectrum;
            datas.Add(dataSpectrum);
            if ((_mediaType & MediaType.Level) > 0 && Math.Abs(_level - float.MinValue) > 1e-9)
            {
                SDataLevel dataLevel = new()
                {
                    Frequency = _frequency,
                    Bandwidth = _filterBandwidth,
                    Data = _level
                };
                datas.Add(dataLevel);
            }

            return datas;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析频段扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>扫描类数据</returns>
    private object ToFScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderAdvanced pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderAdvanced));
            //new(buffer, offset);
            offset += (int)pCommon.Optional_header_length;
            var levels = new short[pCommon.Number_of_trace_items];
            var freqs = new double[pCommon.Number_of_trace_items];
            var freqLows = new uint[pCommon.Number_of_trace_items];
            var freqHighs = new uint[pCommon.Number_of_trace_items];
            var flag = Flags.Level;
            while (flag != Flags.Ifpan1)
            {
                if ((pCommon.SelectorFlags & (ulong)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                            {
                                Array.Reverse(buffer, i * 2 + offset, 2);
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            }

                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqlow:
                            for (var i = 0; i < freqs.Length; i++)
                            {
                                Array.Reverse(buffer, i * 4 + offset, 4);
                                freqLows[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            }

                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqhigh:
                            for (var i = 0; i < freqs.Length; i++)
                            {
                                Array.Reverse(buffer, i * 4 + offset, 4);
                                freqHighs[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            }

                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Offset:
                        case Flags.Fstrength:
                        case Flags.Am:
                        case Flags.AmPos:
                        case Flags.AmNeg:
                        case Flags.Fm:
                        case Flags.FmPos:
                        case Flags.FmNeg:
                        case Flags.Level2:
                        case Flags.Level3:
                        case Flags.Level4:
                        case Flags.Fstrength2:
                        case Flags.Fstrength3:
                        case Flags.Fstrength4:
                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                    }

                flag = (Flags)((ulong)flag << 1);
            }

            for (var i = 0; i < pCommon.Number_of_trace_items; i++)
                freqs[i] = (((ulong)freqHighs[i] << 32) + freqLows[i]) / 1000000d;
            //有可能包含多帧数据（当测量时间极小，频段个数极少时）
            List<short> tempLevels = new();
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            var j = 0;
            for (; j < pCommon.Number_of_trace_items; ++j)
                if (levels[j] != 2000)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var currIndex = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
                    if (currIndex >= 0)
                    {
                        for (var k = currIndex + tempLevels.Count; k < total; ++k) tempLevels.Add(0);
                        SDataScan scan = new()
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
                var currIndex = Utils.GetCurrIndex(freqs[j - tempLevels.Count], StartFrequency, StepFrequency);
                if (currIndex >= 0)
                {
                    SDataScan scan = new()
                    {
                        StartFrequency = StartFrequency,
                        StopFrequency = StopFrequency,
                        StepFrequency = StepFrequency,
                        Offset = currIndex,
                        Total = total,
                        Data = tempLevels.ToArray()
                    };
                    return scan;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析全景扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>扫描类数据</returns>
    private object ToPScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderAdvanced pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderAdvanced));
            OptionalHeaderPScan header = new(buffer, offset);
            offset += (int)pCommon.Optional_header_length;
            List<short> levels = new()
            {
                Capacity = (int)pCommon.Number_of_trace_items
            };
            //本包数据第一个频点的索引
            var freq = header.FreqOfFirstStep;
            var currIndex = Utils.GetCurrIndex(freq / 1000000d, StartFrequency, StepFrequency);
            var flag = Flags.Level;
            while (flag != Flags.Zsam1)
            {
                if ((pCommon.SelectorFlags & (ulong)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Ifpan1:
                        case Flags.Ifpan2:
                        case Flags.Ifpan3:
                        case Flags.Ifpan4:
                            for (var i = 0; i < pCommon.Number_of_trace_items; i++)
                            {
                                Array.Reverse(buffer, i * 2 + offset, 2);
                                levels.Add(BitConverter.ToInt16(buffer, i * 2 + offset));
                            }

                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqlow:
                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqhigh:
                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Level:
                        case Flags.Offset:
                        case Flags.Fstrength:
                        case Flags.Am:
                        case Flags.AmPos:
                        case Flags.AmNeg:
                        case Flags.Fm:
                        case Flags.FmPos:
                        case Flags.FmNeg:
                        case Flags.Level2:
                        case Flags.Level3:
                        case Flags.Level4:
                        case Flags.Fstrength2:
                        case Flags.Fstrength3:
                        case Flags.Fstrength4:
                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                    }

                flag = (Flags)((ulong)flag << 1);
            }

            if (currIndex < 0) return null;
            var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            //表示本次扫描结束,此处仅补齐缺少的点
            if (levels[^1] == 2000)
            {
                levels.RemoveAt(levels.Count - 1);
                for (var i = currIndex + levels.Count; i < total; ++i) levels.Add(0);
            }

            SDataScan scan = new()
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
        catch
        {
            return null;
        }
    }

    /// <summary>
    ///     解析离散扫描数据
    /// </summary>
    /// <param name="buffer">设备返回数据</param>
    /// <param name="offset">偏移量</param>
    /// <returns>离散扫描类数据</returns>
    private object ToMScan(byte[] buffer, int offset)
    {
        try
        {
            CommonHeaderAdvanced pCommon = new(buffer, offset);
            offset += Marshal.SizeOf(typeof(CommonHeaderAdvanced));
            //new(buffer, offset);
            offset += (int)pCommon.Optional_header_length;
            var levels = new short[pCommon.Number_of_trace_items];
            var freqs = new double[pCommon.Number_of_trace_items];
            var freqLows = new uint[pCommon.Number_of_trace_items];
            var freqHighs = new uint[pCommon.Number_of_trace_items];
            var flag = Flags.Level;
            while (flag != Flags.Ifpan1)
            {
                if ((pCommon.SelectorFlags & (ulong)flag) > 0)
                    switch (flag)
                    {
                        case Flags.Level:
                            for (var i = 0; i < levels.Length; i++)
                            {
                                Array.Reverse(buffer, i * 2 + offset, 2);
                                levels[i] = BitConverter.ToInt16(buffer, i * 2 + offset);
                            }

                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqlow:
                            for (var i = 0; i < freqs.Length; i++)
                            {
                                Array.Reverse(buffer, i * 4 + offset, 4);
                                freqLows[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            }

                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Freqhigh:
                            for (var i = 0; i < freqs.Length; i++)
                            {
                                Array.Reverse(buffer, i * 4 + offset, 4);
                                freqHighs[i] = BitConverter.ToUInt32(buffer, i * 4 + offset);
                            }

                            offset += 4 * (int)pCommon.Number_of_trace_items;
                            break;
                        case Flags.Offset:
                        case Flags.Am:
                        case Flags.Level2:
                        case Flags.Level3:
                        case Flags.Level4:
                        case Flags.Fstrength2:
                        case Flags.Fstrength3:
                        case Flags.Fstrength4:
                            offset += 2 * (int)pCommon.Number_of_trace_items;
                            break;
                    }

                flag = (Flags)((ulong)flag << 1);
            }

            for (var i = 0; i < pCommon.Number_of_trace_items; i++)
                freqs[i] = (((ulong)freqHighs[i] << 32) + freqLows[i]) / 1000000d;
            List<short> tempLevels = new();
            var j = 0;
            for (; j < pCommon.Number_of_trace_items; ++j)
                if (levels[j] != 2000)
                {
                    tempLevels.Add(levels[j]);
                }
                else
                {
                    //完成一次完整扫描
                    var currIndex = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
                    if (currIndex >= 0)
                    {
                        SDataScan scan = new()
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
                var currIndex = _scanFreqs.IndexOf(freqs[j - tempLevels.Count]);
                if (currIndex >= 0)
                {
                    SDataScan scan = new()
                    {
                        Offset = currIndex,
                        Total = _scanFreqs.Count,
                        Data = tempLevels.ToArray()
                    };
                    return scan;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}