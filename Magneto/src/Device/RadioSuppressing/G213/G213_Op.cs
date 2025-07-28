using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device.G213;

public partial class G213
{
    /// <summary>
    ///     压制参数下发
    /// </summary>
    public void SendSuppressParameters()
    {
        if (!_running) return;
        if (_rftxSegments?.Any() != true || _rftxBands?.Any() != true || _powers?.Any() != true) return;
        var segGroups = _rftxSegments.GroupBy(p => p.PhysicalChannelNumber).ToList();
        var powerSw = FrequencyBand.AllClose;
        foreach (var group in segGroups)
        {
            var channelNumber = group.Key;
            var bandEx = Array.Find(_rftxBandExs, p => p.PhysicalChannelNumber == channelNumber);
            if (bandEx == null || bandEx.Index >= _powers.Length) continue;
            var segAllBand = group.FirstOrDefault(p => p.RftxSwitch && p.SubBandIndex == -1);
            var paras = new List<SignalParameter>();
            if (segAllBand != null) //不区分子频段
            {
                //0-定频，1-多音， 2-窄带跳频， 3-梳状谱， 4-窄带噪声调频， 5-宽带噪声， 6-线性调频， 7-协议压制
                switch (segAllBand.RftxFrequencyMode)
                {
                    case 3:
                    {
                        var b = GetFreqDistribution(bandEx.StartFrequency, bandEx.StopFrequency,
                            segAllBand.StartFrequency,
                            segAllBand.StopFrequency, segAllBand.StepFrequency, out var freqPoints, true);
                        if (!b) continue;
                        foreach (var (_, stepKHz, freqNumber) in freqPoints)
                        {
                            var para = new SignalParameter
                            {
                                SignalType = GetSignalType(segAllBand.RftxFrequencyMode),
                                Frequency = (uint)(freqNumber * 1000 * 100),
                                Bandwidth = (uint)(segAllBand.Bandwidth * 100),
                                Attenuation = (ushort)segAllBand.Attenuation,
                                FrequencyNumber = (ushort)freqNumber,
                                CombBandwidth = (uint)(segAllBand.CombBandwidth * 100),
                                CombStep = (uint)(stepKHz * 100),
                                LFMCycle = (uint)segAllBand.Cycle,
                                HoppingSpeed = segAllBand.HoppingSpeed,
                                ProtocolSuppressType = segAllBand.ProtocolSuppressType
                            };
                            paras.Add(para);
                        }
                    }
                        break;
                    case 1:
                    case 2:
                    {
                        var b = GetFreqDistribution(bandEx.StartFrequency, bandEx.StopFrequency,
                            segAllBand.Frequencies, out var freqPoints, true);
                        if (!b) continue;
                        foreach (var (_, stepKHz, freqNumber) in freqPoints)
                        {
                            var para = new SignalParameter
                            {
                                SignalType = GetSignalType(segAllBand.RftxFrequencyMode),
                                Frequency = (uint)(freqNumber * 1000 * 100),
                                Bandwidth = (uint)(segAllBand.Bandwidth * 100),
                                Attenuation = (ushort)segAllBand.Attenuation,
                                FrequencyNumber = (ushort)freqNumber,
                                CombBandwidth = segAllBand.RftxFrequencyMode == 1
                                    ? 0
                                    : (uint)(segAllBand.CombBandwidth * 100),
                                CombStep = (uint)(stepKHz * 100),
                                LFMCycle = (uint)segAllBand.Cycle,
                                HoppingSpeed = segAllBand.HoppingSpeed,
                                ProtocolSuppressType = segAllBand.ProtocolSuppressType
                            };
                            paras.Add(para);
                        }
                    }
                        break;
                    default:
                    {
                        var para = new SignalParameter
                        {
                            SignalType = GetSignalType(segAllBand.RftxFrequencyMode),
                            Frequency = (uint)(segAllBand.Frequency * 1000 * 100),
                            Bandwidth = (uint)(segAllBand.Bandwidth * 100),
                            Attenuation = (ushort)segAllBand.Attenuation,
                            FrequencyNumber = 1,
                            CombBandwidth = (uint)(segAllBand.CombBandwidth * 100),
                            CombStep = (uint)segAllBand.StepFrequency,
                            LFMCycle = (uint)segAllBand.Cycle,
                            HoppingSpeed = segAllBand.HoppingSpeed,
                            ProtocolSuppressType = segAllBand.ProtocolSuppressType
                        };
                        paras.Add(para);
                    }
                        break;
                }
            }
            else //区分子频段
            {
                var validSubBandDic = new Dictionary<int, RftxSegmentsTemplate>();
                var segSubBands = group.Where(p =>
                    p.RftxSwitch && p.SubBandIndex >= 0 && p.SubBandIndex < bandEx.ChannelBands.Length).ToList();
                foreach (var subBand in segSubBands) validSubBandDic.TryAdd(subBand.SubBandIndex, subBand);
                foreach (var kv in validSubBandDic)
                {
                    var channelBand = bandEx.ChannelBands[kv.Key];
                    switch (kv.Value.RftxFrequencyMode)
                    {
                        case 3:
                        {
                            var b = GetFreqDistribution(channelBand.StartFrequency, channelBand.StopFrequency,
                                kv.Value.StartFrequency,
                                kv.Value.StopFrequency, kv.Value.StepFrequency, out var freqPoints, true);
                            if (!b) continue;
                            foreach (var (_, stepKHz, freqNumber) in freqPoints)
                            {
                                var para = new SignalParameter
                                {
                                    SignalType = GetSignalType(kv.Value.RftxFrequencyMode),
                                    Frequency = (uint)(freqNumber * 1000 * 100),
                                    Bandwidth = (uint)(kv.Value.Bandwidth * 100),
                                    Attenuation = (ushort)kv.Value.Attenuation,
                                    FrequencyNumber = (ushort)freqNumber,
                                    CombBandwidth = (uint)(kv.Value.CombBandwidth * 100),
                                    CombStep = (uint)(stepKHz * 100),
                                    LFMCycle = (uint)kv.Value.Cycle,
                                    HoppingSpeed = kv.Value.HoppingSpeed,
                                    ProtocolSuppressType = kv.Value.ProtocolSuppressType
                                };
                                paras.Add(para);
                            }
                        }
                            break;
                        case 1:
                        case 2:
                        {
                            var b = GetFreqDistribution(channelBand.StartFrequency, channelBand.StopFrequency,
                                kv.Value.Frequencies, out var freqPoints, true);
                            if (!b) continue;
                            foreach (var (_, stepKHz, freqNumber) in freqPoints)
                            {
                                var para = new SignalParameter
                                {
                                    SignalType = GetSignalType(kv.Value.RftxFrequencyMode),
                                    Frequency = (uint)(freqNumber * 1000 * 100),
                                    Bandwidth = (uint)(kv.Value.Bandwidth * 100),
                                    Attenuation = (ushort)kv.Value.Attenuation,
                                    FrequencyNumber = (ushort)freqNumber,
                                    CombBandwidth = kv.Value.RftxFrequencyMode == 1
                                        ? 0
                                        : (uint)(kv.Value.CombBandwidth * 100),
                                    CombStep = (uint)(stepKHz * 100),
                                    LFMCycle = (uint)kv.Value.Cycle,
                                    HoppingSpeed = kv.Value.HoppingSpeed,
                                    ProtocolSuppressType = kv.Value.ProtocolSuppressType
                                };
                                paras.Add(para);
                            }
                        }
                            break;
                        default:
                        {
                            var para = new SignalParameter
                            {
                                SignalType = GetSignalType(kv.Value.RftxFrequencyMode),
                                Frequency = (uint)(kv.Value.Frequency * 1000 * 100),
                                Bandwidth = (uint)(kv.Value.Bandwidth * 100),
                                Attenuation = (ushort)kv.Value.Attenuation,
                                FrequencyNumber = 1,
                                CombBandwidth = (uint)(kv.Value.CombBandwidth * 100),
                                CombStep = (uint)kv.Value.StepFrequency,
                                LFMCycle = (uint)kv.Value.Cycle,
                                HoppingSpeed = kv.Value.HoppingSpeed,
                                ProtocolSuppressType = kv.Value.ProtocolSuppressType
                            };
                            paras.Add(para);
                        }
                            break;
                    }
                }
            }

            if (paras.Count == 0) continue;
            if (paras.Count > 5) paras.RemoveRange(5, paras.Count - 5 + 1);
            powerSw |= bandEx.FrequencyBand;
            var body = new ParameterBody
            {
                FreqChannel = bandEx.FrequencyBand,
                Power = (short)(_powers[bandEx.Index] * 10),
                Parameters = paras,
                SignalNumber = (short)paras.Count
            };
            var deviceId = channelNumber > 3 ? 1 : 0;
            var parameter = new CommandFrame(deviceId, CommandType.SendParameter, body);
            _sendDataCache.Enqueue(parameter);
        }

        for (var i = 0; i < 2; i++)
        {
            var deviceId = i;
            var powerBody = new PowerControlBody
            {
                Switch = powerSw
            };
            var power = new CommandFrame(deviceId, CommandType.PowerControl, powerBody);
            _sendDataCache.Enqueue(power);
        }
    }

    private void TcpDataProcessProc()
    {
        while (_tcpDataProcessCts?.IsCancellationRequested == false)
            try
            {
                Thread.Sleep(10);
                _preGetStatusTime = DateTime.Now;
                var list = new List<object>();
                var remote = new IPEndPoint(IPAddress.Any, 0);
                var buffer = _udpServer.Receive(ref remote);
                var devId = 0;
                if (remote.Address.ToString() == SlaveIp0)
                    devId = 0;
                else if (remote.Address.ToString() == SlaveIp1)
                    devId = 1;
                else
                    continue;
                var result = new CommandFrame(buffer, 0);
                var body = result.Body as StatusResultBody;
                var alarms = GetAlarmInfo(body, devId, Powers);
#if DEBUG
                var str = DateTime.Now.ToString("HH:mm:ss.fff ");
                foreach (var item in alarms) str += item.ChannelNumber + ":" + item.Power + ";";
                Console.WriteLine(str);
#endif
                list.AddRange(alarms);
                SendData(list);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void SendDataProc()
    {
        while (_sendDataAsyncCts?.IsCancellationRequested == false)
            try
            {
                while (!_sendDataCache.IsEmpty)
                {
                    var b = _sendDataCache.TryDequeue(out var cmd);
                    if (!b || cmd == null) continue;
                    SendCmd(cmd.ToBytes(), cmd.DeviceId);
                    Thread.Sleep(30);
                }

                if (_sendDataCache.IsEmpty && DateTime.Now.Subtract(_preGetStatusTime).TotalSeconds > 10)
                {
                    _preGetStatusTime = DateTime.Now;
                    var cmd = new CommandFrame(0, CommandType.QueryStatus, null);
                    SendCmd(cmd.ToBytes(), 0);
                    Thread.Sleep(30);
                    cmd = new CommandFrame(1, CommandType.QueryStatus, null);
                    SendCmd(cmd.ToBytes(), 1);
                }

                Thread.Sleep(30);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void StopSuppress()
    {
        var body = new PowerControlBody
        {
            Switch = FrequencyBand.AllClose
        };
        for (var i = 0; i < 2; i++)
        {
            var cmd = new CommandFrame(i, CommandType.PowerControl, body);
            _sendDataCache.Enqueue(cmd);
        }
    }

    private static List<SDataRadioSuppressing> GetAlarmInfo(StatusResultBody body, int deviceId, float[] powers)
    {
        var list = new List<SDataRadioSuppressing>();
        if (body != null)
            for (var i = 0; i < 4; i++)
            {
                //ushort f = (ushort)Math.Pow(2, i);
                //var freq = (FrequencyBand)f;
                //string str = freq.ToString();
                //var split = str.Split('_');
                //alarm.StartFrequency = Convert.ToDouble(split[0].Replace("F", ""));
                //alarm.StopFrequency = Convert.ToDouble(split[1].Replace("F", ""));
                var channelNumber = i + deviceId * 4;
                var alarm = new SDataRadioSuppressing
                {
                    ChannelNumber = channelNumber,
                    Power = powers == null || powers.Length <= channelNumber ? 0 : powers[channelNumber]
                };
                if (((body.PowerState >> i) & 0x01) <= 0) alarm.Power = -1;
                if (((body.Vswr >> i) & 0x01) > 0) alarm.Vsw = true;
                if (((body.Temp >> i) & 0x01) > 0) alarm.OverHeating = true;
                if (((body.Current >> i) & 0x01) > 0)
                {
                    alarm.Warning += string.IsNullOrWhiteSpace(alarm.Warning) ? string.Empty : ",";
                    alarm.Warning += "电流告警";
                }

                if (((body.Volt >> i) & 0x01) > 0)
                {
                    alarm.Warning += string.IsNullOrWhiteSpace(alarm.Warning) ? string.Empty : ",";
                    alarm.Warning += "电压告警";
                }

                list.Add(alarm);
            }

        return list;
    }

    private static SignalType GetSignalType(int rftxFrequencyMode)
    {
        //|定频|多音|窄带跳频|梳状谱|窄带噪声调频|宽带噪声|线性调频|协议压制
        return rftxFrequencyMode switch
        {
            //单音
            0 => SignalType.DotFrequency,
            //窄带跳频
            1 => SignalType.NarrowbandFrequencyHopping,
            //多音
            2 => SignalType.Multitone,
            //梳状谱
            3 => SignalType.CombSpectrum,
            //窄带噪声调频
            4 => SignalType.NarrowBandNoiseModulation,
            //宽带噪声
            5 => SignalType.BroadbandNoise,
            //线性调频
            6 => SignalType.LineFrequencyModulation,
            //协议压制
            7 => SignalType.Protocol,
            _ => SignalType.DotFrequency
        };
    }

    /// <summary>
    ///     获取梳状谱频率分配
    /// </summary>
    /// <param name="bandStart">功放/运营商频段起始频率，单位MHz</param>
    /// <param name="bandStop">功放/运营商频段结束频率，单位MHz</param>
    /// <param name="start">管制频段起始频率，单位MHz</param>
    /// <param name="stop">管制频段结束频率，单位MHz</param>
    /// <param name="step">管制频段步进，单位kHz</param>
    /// <param name="freqPoints">频率分配信息</param>
    /// <param name="isAmplifierGroup">是否为功放分组</param>
    /// <param name="canOverRange">能否超过频段范围</param>
    /// <param name="canModifyStep">能否修改步进</param>
    /// <returns></returns>
    private static bool GetFreqDistribution(double bandStart, double bandStop, double start, double stop, double step,
        out List<(double freqMHz, double stepKHz, int freqNumber)> freqPoints, bool isAmplifierGroup,
        bool canOverRange = true, bool canModifyStep = true)
    {
        freqPoints = new List<(double freqMHz, double stepKHz, int freqNumber)>();
        var maxRangeCount = isAmplifierGroup ? 5 : 1;
        const int maxFreqCount = 16;
        const int minStepHz = 1000;
        const int maxStepHz = 10 * 1000 * 1000;
        const int maxRangeHz = maxStepHz * (maxFreqCount - 1);
        var maxBandHz = maxRangeHz * maxRangeCount;
        maxBandHz = Math.Min(maxBandHz, (int)((bandStop - bandStart) * 1e6));
        var rangeHz = (stop - start) * 1e6;
        if (rangeHz.CompareWith(minStepHz) <= 0)
        {
            var centerFrequency = (stop + start) / 2;
            var stepFrequency = minStepHz / 1e3;
            var freqNumber = 1;
            freqPoints.Add((centerFrequency, stepFrequency, freqNumber));
        }
        else if (rangeHz.CompareWith(maxBandHz) > 0)
        {
            //参数正确的情况下 理论上不会进入该分支。
            Trace.WriteLine("参数的频段范围超出理论范围。");
            return false;
        }
        else
        {
            var rationalMinStep = rangeHz / 1e3 / (maxFreqCount * maxRangeCount);
            var centerFrequency = (stop + start) / 2;
            var realStep = step;
            if (step.CompareWith(rationalMinStep) < 0)
            {
                Trace.WriteLine($"参数的步进小于理论最小步进（{rationalMinStep / 1e3}kHz）。");
                if (!canModifyStep) return false;
                realStep = rationalMinStep;
            }

            var pointTotal = canOverRange ? Math.Ceiling(rangeHz / realStep) : Math.Floor(rangeHz / realStep);
            var rangeCount = (int)Math.Ceiling(pointTotal / maxFreqCount);
            var lastRangePointCnt = (int)Math.Ceiling(pointTotal % maxFreqCount);
            if (rangeCount == 1)
            {
                freqPoints.Add((centerFrequency, realStep, lastRangePointCnt));
            }
            else
            {
                var rangeIndex = 0;
                var firstRangeStartFreq = centerFrequency - pointTotal * realStep / 2;
                while (rangeIndex < rangeCount)
                {
                    var rangePointCnt = rangeIndex == rangeCount - 1 ? lastRangePointCnt : maxFreqCount;
                    var rangeCenterFrequency = firstRangeStartFreq + rangeIndex * rangePointCnt * realStep / 2;
                    freqPoints.Add((rangeCenterFrequency, realStep, rangePointCnt));
                    rangeIndex++;
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     获取跳频/多音频率分配
    /// </summary>
    /// <param name="bandStart">功放/运营商频段起始频率，单位MHz</param>
    /// <param name="bandStop">功放/运营商频段结束频率，单位MHz</param>
    /// <param name="frequencies">频点集合</param>
    /// <param name="freqPoints">频率分配信息</param>
    /// <param name="isAmplifierGroup">是否为功放分组</param>
    /// <returns></returns>
    private static bool GetFreqDistribution(double bandStart, double bandStop, double[] frequencies,
        out List<(double freqMHz, double stepKHz, int freqNumber)> freqPoints, bool isAmplifierGroup)
    {
        freqPoints = new List<(double freqMHz, double stepKHz, int freqNumber)>();
        var maxRangeCount = isAmplifierGroup ? 5 : 1;
        const int maxFreqCount = 16;
        if (frequencies?.Any() != true) return false;
        var validFrequencies =
            (from p in frequencies
                where p.CompareWith(bandStart) >= 0 && p.CompareWith(bandStop) <= 0
                orderby p
                select p).ToList();
        if (validFrequencies.Any() != true) return false;
        var maxPointTotal = maxFreqCount * maxRangeCount;
        if (validFrequencies.Count > maxPointTotal) Trace.WriteLine("频点数量大于设备支持频点上限，部分频点将被舍弃。");
        var sequences = GetSequenceList(validFrequencies, maxPointTotal);
        if (sequences?.Any() == true)
            freqPoints.AddRange(sequences);
        else
            return false;
        return true;
    }

    /// <summary>
    ///     从数值集合查找频段信息
    /// </summary>
    /// <param name="array">从小到大排序的数值集合</param>
    /// <param name="maxSequenceCount">频段最大数量</param>
    /// <returns>频段信息列表</returns>
    public static List<(double centerFrequency, double step, int freqNumber)> GetSequenceList(List<double> array,
        int maxSequenceCount)
    {
        var ranges = new List<(double centerFrequency, double step, int freqNumber)>();
        var list = new List<double>(array);
        var i = 0;
        while (i < maxSequenceCount)
        {
            if (list.Count == 0) break;
            var b = GetLongestSequence(ref list, out var range);
            if (!b) break;
            ranges.Add(range);
            i++;
        }

        return ranges;
    }

    private static bool GetLongestSequence(ref List<double> list,
        out (double centerFrequency, double step, int freqNumber) rangeInfo)
    {
        const int maxFreqCount = 16;
        rangeInfo = (0, 0, 0);
        if (list.Count == 0) return false;

        if (list.Count == 1)
        {
            rangeInfo = (list.FirstOrDefault(), 1000d, 1);
            list.Clear();
            return true;
        }

        var startFrequency = list.FirstOrDefault();
        var stopFrequency = list.LastOrDefault();
        var dicStep = new Dictionary<int, int>();
        for (var i = 1; i < list.Count; i++)
        {
            var delta = (int)((list[i] - list[i - 1]) * 1e6);
            if (dicStep.ContainsKey(delta))
                dicStep[delta] += 1;
            else
                dicStep.Add(delta, 1);
        }

        var steps = (from p in dicStep orderby p select p.Key).ToList();
        var resBw = 0d;
        if (steps.Count == 1)
        {
            var b = GetLegalStep(steps.FirstOrDefault(), out var resBwHz);
            if (!b) return false;
            resBw = resBwHz % 1e3;
            var index = 0;
            var temp = startFrequency;
            do
            {
                temp += resBw;
                index++;
            } while (temp < stopFrequency && index < 15);

            var centerFrequency = (startFrequency + temp) / 2;
            var freqNumber = index + 1;
            list.RemoveRange(0, freqNumber);
            rangeInfo = new ValueTuple<double, double, int>(centerFrequency, resBw, freqNumber);
        }
        else
        {
            var temp = new List<double>();
            var possibleSteps = new List<int>();
            for (var i = 0; i < steps.Count; i++)
            {
                var legal = GetLegalStep(steps.FirstOrDefault(), out var resBwHz);
                if (!legal) continue;
                possibleSteps.Add(resBwHz);
                for (var j = i + 1; j < steps.Count; j++)
                {
                    var b = GetCommonStep(steps[i], steps[j]);
                    if (!b) continue;
                    b = GetLegalStep(steps.FirstOrDefault(), out var possibleLegalStep);
                    if (!b) continue;
                    if (!possibleSteps.Contains(possibleLegalStep)) possibleSteps.Add(possibleLegalStep);
                }
            }

            foreach (var possibleStep in possibleSteps)
                for (var m = 0; m < list.Count; m++)
                {
                    var tempList = new List<double>
                    {
                        list[m]
                    };
                    for (var n = m + 1; n < list.Count; n++)
                    {
                        var gap = (int)((list[n] - list[m]) * 1e6);
                        if (gap / (double)possibleStep > maxFreqCount - 1) break;
                        if (gap % possibleStep != 0) continue;
                        tempList.Add(list[n]);
                    }

                    if (tempList.Count > temp.Count)
                    {
                        temp = new List<double>(tempList);
                        resBw = possibleStep / 1e3;
                    }
                }

            if (temp.Count < 1) return false;
            if (temp.Count == 1)
            {
                rangeInfo = (list.FirstOrDefault(), 1000d, 1);
            }
            else
            {
                var range = temp.LastOrDefault() - temp.FirstOrDefault();
                var centerFrequency = (temp.FirstOrDefault() + temp.LastOrDefault()) / 2;
                var freqNumber = (int)(range * 1e3 / resBw + 1);
                rangeInfo = new ValueTuple<double, double, int>(centerFrequency, resBw, freqNumber);
            }

            for (var i = list.Count - 1; i >= 0; i--)
                if (temp.Contains(list[i]))
                    list.RemoveAt(i);
        }

        return true;
    }

    private static bool GetCommonStep(int step1, int step2)
    {
        var step = GetMaxCommonDivisor(step1, step2);
        return step % 10 == 0;
    }

    /// <summary>
    ///     获取最大公约数
    /// </summary>
    /// <param name="num1">数值1</param>
    /// <param name="num2">数值2</param>
    /// <returns>最大公约数</returns>
    private static int GetMaxCommonDivisor(int num1, int num2)
    {
        var temp = num1 % num2;
        do
        {
            num1 = num2;
            num2 = temp;
            temp = num1 % num2;
        } while (temp != 0);

        return num2;
    }

    private static bool GetLegalStep(int stepHz, out int realStep)
    {
        const int minStepHz = 1000;
        const int maxStepHz = 10 * 1000 * 1000;
        realStep = stepHz;
        if (stepHz % 10 != 0) return false;
        if (stepHz is >= minStepHz and <= maxStepHz) return true;

        if (stepHz < minStepHz)
        {
            var n = 2;
            var temp = stepHz;
            while (temp < minStepHz)
            {
                temp = stepHz * n;
                n++;
            }

            realStep = temp;
            return realStep % 10 == 0;
        }

        realStep = stepHz;
        return true;
    }
}