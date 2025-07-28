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

namespace Magneto.Device.ESMB;

public partial class Esmb
{
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
                    obj = ToSpectrum(data as RawIfPan);
                    break;
                case DataType.Fscan:
                    obj = ToFScan(data as RawFScan);
                    break;
                case DataType.Dscan:
                    obj = ToDScan(data as RawDScan);
                    break;
                case DataType.Mscan:
                    obj = ToMScan(data as RawMScan);
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
        var startTime = DateTime.Now;
        while (!token.IsCancellationRequested)
        {
            //当前未订阅电平或者ITU数据则直接返回
            if ((_media & (MediaType.Level | MediaType.Itu)) == 0)
            {
                //释放时间片，促进线程切换
                Thread.Sleep(1);
                continue;
            }

            //发送数据缓存列表
            var datas = new List<object>();
            try
            {
                var result = SendSyncCmd("SENS:DATA?");
                var values = result.Split(',');
                var level = float.Parse(values[0]);
                if ((_media & MediaType.Level) > 0)
                    if (level > -999f) //排除返回的无效数
                    {
                        var dataLevel = new SDataLevel
                        {
                            Data = level,
                            Frequency = _frequency,
                            Bandwidth = _filterBandwidth
                        };
                        datas.Add(dataLevel);
                    }

                //解析ITU数据 每3秒发送一次
                if (values.Length >= 9 && (_media & MediaType.Itu) > 0)
                {
                    var ts = DateTime.Now - startTime;
                    if (ts.TotalMilliseconds > 3000)
                    {
                        double am = float.Parse(values[1]);
                        //float ampos = float.Parse(values[2]);
                        //float amneg = float.Parse(values[3]);
                        var fm = float.Parse(values[4]) / 1000d;
                        var fmpos = float.Parse(values[5]) / 1000d;
                        var fmneg = float.Parse(values[6]) / 1000d;
                        double pm = float.Parse(values[7]);
                        var bw = float.Parse(values[8]) / 1000d;
                        //TODO:无效为-9E37,此处用-1000000000判断即可
                        var dAmDepth = am is < 0 or > 100 ? double.MinValue : am;
                        var dFmDev = fm <= -1000000000 ? double.MinValue : fm;
                        var dFmDevPos = fmpos <= -1000000000 ? double.MinValue : fmpos;
                        var dFmDevNeg = fmneg <= -1000000000 ? double.MinValue : fmneg;
                        var dPmDepth = pm <= -1000000000 ? double.MinValue : pm;
                        const Modulation modulation = Modulation.Iq;
                        var dataItu = new SDataItu
                        {
                            Frequency = _frequency,
                            Modulation = modulation,
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
                        startTime = DateTime.Now; //保存最新的时间
                    }
                }

                if (datas.Count > 0 && TaskState == TaskState.Start) SendData(datas);
                //释放时间片，促进线程切换
                Thread.Sleep(10);
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

    #region 解析数据

    private object ToAudio(RawAudio data)
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

    private object ToSpectrum(RawIfPan data)
    {
        if (data == null) return null;
        // 频段搜索丢弃与频段范围不匹配数据
        if (CurFeature == FeatureType.FScne)
            if (StartFrequency.CompareWith(data.Frequency / 1000000d) > 0 ||
                StopFrequency.CompareWith(data.Frequency / 1000000d) < 0)
                return null;
        // var levels = new float[data.Trace.NumberOfTraceItems];
        // for (int i = 0; i < levels.Length; i++)
        // {
        //     levels[i] = data.DataCollection[i] / 10f;
        // }
        return new SDataSpectrum
        {
            Frequency = data.Frequency / 1000000d,
            Span = data.SpanFrequency / 1000d,
            Data = data.DataCollection
        };
    }

    private object ToFScan(RawFScan data)
    {
        if (data == null) return null;
        var levels = data.DataCollection;
        var freqs = new double[data.Trace.NumberOfTraceItems];
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++) freqs[i] = data.FreqCollection[i] / 1000000d;
        //2017/11/1 有可能包含多帧数据（当测量时间极小，频段个数极少时）
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

    private object ToDScan(RawDScan data)
    {
        if (data == null) return null;
        var levels = new List<short>();
        var freq = data.FreqCollection[0] / 1000000d;
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++) levels.Add(data.DataCollection[i]);
        var currIndex = Utils.GetCurrIndex(freq, StartFrequency, StepFrequency);
        if (currIndex < 0) return null;
        var total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
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

    private object ToMScan(RawMScan data)
    {
        if (data == null) return null;
        var levels = data.DataCollection;
        var freqs = new double[data.Trace.NumberOfTraceItems];
        for (var i = 0; i < data.Trace.NumberOfTraceItems; i++) freqs[i] = data.FreqCollection[i] / 1000000d;
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

    #endregion
}