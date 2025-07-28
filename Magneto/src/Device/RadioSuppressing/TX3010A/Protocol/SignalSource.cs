using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.TX3010A.Protocol;

/// <summary>
///     信号源
/// </summary>
public class SignalSource : RadioSuppressingBase
{
    private CancellationTokenSource _cts;
    private Task _monitorTask;

    public SignalSource(string ipAddress, int port, int index, int physicalChannel, int deviceChannel) : base(ipAddress,
        port, index)
    {
        SubSystemIndex = index;
        PhysicalChannelNumber = physicalChannel;
        DeviceChannelNumber = deviceChannel;
        Name = $"信号源{physicalChannel}({ipAddress}:{port})";
    }

    /// <summary>
    ///     子系统编号
    /// </summary>
    public int SubSystemIndex { get; }

    /// <summary>
    ///     当前信号源在总系统中的序号
    /// </summary>
    public int PhysicalChannelNumber { get; }

    public int DeviceChannelNumber { get; }
    public double DeviceStartFrequency { get; private set; }
    public double DeviceStopFrequency { get; private set; }
    public int MaxSignalCount { get; private set; }
    public override DeviceType DeviceType => DeviceType.SignalSource;

    /// <summary>
    ///     设备掉线事件
    /// </summary>
    public event EventHandler<DeviceDisconnectedEventArgs> Disconnected;

    public override bool Initialized()
    {
        var success = base.Initialized();
        if (!success)
        {
            Init = true;
            return false;
        }

        _cts = new CancellationTokenSource();
        _monitorTask = new Task(Monitor, _cts.Token);
        _monitorTask.Start();
        Init = true;
        return true;
    }

    public override bool Reinitialize()
    {
        if (!Init) return false;
        if (Enabled) return true;
        Utils.CancelTask(_monitorTask, _cts);
        Socket?.Dispose();
        Enabled = false;
        return Initialized();
    }

    public void UpdateFrequencyRange(double startFrequency, double stopFrequency, int maxSignalCount)
    {
        DeviceStartFrequency = startFrequency;
        DeviceStopFrequency = stopFrequency;
        MaxSignalCount = maxSignalCount;
    }

    /// <summary>
    ///     设置功放功率
    /// </summary>
    /// <param name="power">功放功率，单位为dBm</param>
    public void SetPower(float power)
    {
        var np = Math.Round(power, 0);
        //var cmd = $"{SC_AMP_ATT_RF} {np}";
        var cmd = $"{ScSignalPower} {np}";
        SendCmd(cmd);
    }

    /// <summary>
    ///     打开功放
    /// </summary>
    public void PowerOn()
    {
        var cmd = $"{ScTrace} ON";
        SendCmd(cmd);
    }

    /// <summary>
    ///     关闭功放
    /// </summary>
    public void PowerOff()
    {
        var cmd = $"{ScTrace} OFF";
        SendCmd(cmd);
    }

    /// <summary>
    ///     发送单音/定频参数
    /// </summary>
    /// <param name="frequency">中心频率</param>
    /// <param name="bandwidth">调制带宽 kHz</param>
    /// <param name="modulation">调制模式</param>
    public void SendSingleCmd(double frequency, double bandwidth, Modulation modulation)
    {
        /*
            TX:SIGNal:MODE SINGLET
            TX:MULTITone:COUNt 1
            TX:MULTITone:CONTents 0,700.000000MHz,2MHz,CW
        */
        var bw = bandwidth / 1000;
        SendCmd($"{ScFreqCent} {frequency}MHz");
        SendCmd($"{ScSignalMode} {SignalMode.SingleTones}");
        SendCmd($"{ScToneCount} 1");
        SendCmd($"{ScFreqCent} {frequency}MHz");
        SendCmd($"{ScToneContents} 0,{frequency}MHz,{bw}MHz,{modulation}");
    }

    /// <summary>
    ///     发送多音/跳频参数
    /// </summary>
    /// <param name="frequencies"></param>
    /// <param name="bandwidth"></param>
    /// <param name="modulation"></param>
    public void SendMultiCmd(double[] frequencies, double bandwidth, Modulation modulation)
    {
        /*
            TX:SIGNal:MODE MULTIT
            TX:MULTITone:COUNt 8
            TX:MULTITone:CONTents 0,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 1,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 2,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 3,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 4,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 5,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 6,700.000000MHz,2MHz,CW
            TX:MULTITone:CONTents 7,700.000000MHz,2MHz,CW
        */
        var bw = bandwidth / 1000;
        var freqList = GetMergedFrequencyList(frequencies, bw);
        Trace.WriteLine($"合并后的频点列表：{Utils.ConvertToJson(freqList)}");
        if (!freqList.Any()) return;
        var centerFrequency = (DeviceStartFrequency + DeviceStopFrequency) / 2;
        SendCmd($"{ScFreqCent} {centerFrequency}MHz");
        SendCmd($"{ScSignalMode} {SignalMode.MultiTones}");
        SendCmd($"{ScToneCount} {freqList.Count}");
        for (var i = 0; i < freqList.Count && i < MaxSignalCount; i++)
            SendCmd($"{ScToneContents} {i},{freqList[i]}MHz,{bw}MHz,{modulation}");
    }

    /// <summary>
    ///     发送扫频参数
    /// </summary>
    /// <param name="startFrequency"></param>
    /// <param name="stopFrequency"></param>
    /// <param name="stepFrequency"></param>
    /// <param name="holdTime"></param>
    public void SendScanCmd(double startFrequency, double stopFrequency, double stepFrequency, float holdTime)
    {
        /*
            TX:SIGNal:MODE SCAN
            FREQuency:STARt 700.000000MHz
            FREQuency:STOP 6000.000000MHz
            TIME 1μs
            Step 1kHz
        */
        var centerFreq = (startFrequency + stopFrequency) / 2;
        SendCmd($"{ScFreqCent} {centerFreq}MHz");
        SendCmd($"{ScToneContents} 0,{centerFreq}MHz,2MHz,CW");
        SendCmd($"{ScSignalMode} {SignalMode.Scan}");
        SendCmd($"{ScFreqStart} {startFrequency}MHz");
        SendCmd($"{ScFreqStop} {stopFrequency}MHz");
        //SendCmd($"{SC_TIME} {holdTime}us");//2022-06-01 taojin：字符编码:ACSII 单位:us 当前硬件默认写死10μs，设置无意义  
        SendCmd($"{ScScanStep} {stepFrequency}kHz");
    }

    /// <summary>
    ///     发送梳状谱参数
    /// </summary>
    /// <param name="startFrequency"></param>
    /// <param name="stopFrequency"></param>
    /// <param name="stepFrequency"></param>
    [Obsolete("设备协议已改")]
    public void SendCombCmd(double startFrequency, double stopFrequency, double stepFrequency)
    {
        /*
            TX:SIGNal:MODE COMB
            FREQuency:STARt 700.000000MHz
            FREQuency:STOP 6000.000000MHz
            TX:COMB:STEP 15kHz
        */
        var centerFreq = (startFrequency + stopFrequency) / 2;
        SendCmd($"{ScFreqCent} {centerFreq}MHz");
        SendCmd($"{ScToneContents} 0,{centerFreq}MHz,2MHz,CW");
        SendCmd($"{ScSignalMode} {SignalMode.Comb}");
        SendCmd($"{ScFreqStart} {startFrequency}MHz");
        SendCmd($"{ScFreqStop} {stopFrequency}MHz");
        SendCmd($"{ScCombStep} {stepFrequency}kHz");
    }

    public void SendCombCmd(
        List<(double startFrequency, double stopFrequency, double stepFrequency, double frequency)> combParas)
    {
        /*
         * TX:SIGNal:MODE COMB
         * TX:MULTIComb:COUNt 1
         * TX:MULTIComb:CONTents 0,1000MHz,1010MHz,15KHz，0Hz
         */
        if (combParas?.Any() != true) return;
        var centerFrequency = (DeviceStartFrequency + DeviceStopFrequency) / 2;
        SendCmd($"FREQuency {centerFrequency}MHz");
        SendCmd("TX:SIGNal:MODE COMB");
        var list = GetMergedFrequencyRangeList(combParas);
        var count = Math.Min(list.Count, MaxSignalCount);
        if (count == 0) return;
        SendCmd($"TX:MULTIComb:COUNt {count}");
        for (var i = 0; i < count; i++)
        {
            var (startFrequency, stopFrequency, stepFrequency) = list[i];
            SendCmd($"TX:MULTIComb:CONTents {i},{startFrequency}MHz,{stopFrequency}MHz,{stepFrequency}KHz,0Hz");
        }
    }

    /// <summary>
    ///     合并频点
    /// </summary>
    /// <param name="frequencies">频点列表，单位为MHz</param>
    /// <param name="bandwidth">带宽，单位为MHz</param>
    /// <returns></returns>
    private static List<double> GetMergedFrequencyList(double[] frequencies, double bandwidth)
    {
        var list = new List<double>();
        if (frequencies?.Any() != true) return list;
        if (bandwidth.CompareWith(0.2) <= 0)
        {
            list.AddRange(frequencies);
            return list;
        }

        var maxDelta = bandwidth - 0.2; //跟王凡确认，与带宽边缘保留100kHz
        var freqList = frequencies.Distinct().OrderBy(p => p).ToList();
        var index = 0;
        var rangeList = new List<List<double>>(freqList.Count);
        var temp = new List<double>();
        while (index < freqList.Count)
        {
            var frequency = freqList[index];
            if (index == 0)
            {
                temp.Add(frequency);
            }
            else
            {
                var before = temp.FirstOrDefault();
                var delta = Math.Abs(frequency - before);
                if (delta.CompareWith(maxDelta) > 0)
                {
                    rangeList.Add(new List<double>(temp));
                    temp.Clear();
                    temp.Add(frequency);
                }
                else
                {
                    temp.Add(frequency);
                }
            }

            if (index == freqList.Count - 1)
                if (temp.Any())
                    rangeList.Add(temp);
            index++;
        }

        foreach (var range in rangeList)
        {
            if (range.Count == 0) continue;
            if (range.Count == 1)
            {
                list.Add(range[0]);
            }
            else
            {
                var freq = range[0] + (bandwidth - 0.2) / 2;
                list.Add(freq);
            }
        }

        return list;
    }

    private List<(double startFrequency, double stopFrequency, double stepFrequency)> GetMergedFrequencyRangeList(
        List<(double startFrequency, double stopFrequency, double stepFrequency, double frequency)> list,
        double maxDelta = 2d)
    {
        var result = new List<(double startFrequency, double stopFrequency, double stepFrequency)>();
        if (list?.Any() != true) return result;
        var freqList = new List<double>();
        foreach (var (start, stop, step, freq) in list)
        {
            if (start.CompareWith(stop) >= 0) continue;
            var range = Math.Abs(stop - start);
            if (freq.CompareWith(start) < 0 || freq.CompareWith(stop) > 0 || range.CompareWith(4d) > 0)
                result.Add((start, stop, step));
            else
                freqList.Add(freq);
        }

        var freqs = freqList.Distinct().OrderBy(p => p).ToList();
        var index = 0;
        var temp = new List<double>();
        while (index < freqs.Count)
        {
            var frequency = freqs[index];
            if (index == 0)
            {
                temp.Add(frequency);
            }
            else
            {
                var before = temp.LastOrDefault();
                var delta = Math.Abs(frequency - before);
                if (delta.CompareWith(maxDelta) > 0)
                {
                    var b = GetFrequencyRange(temp, out var range);
                    if (b) result.Add(range);
                    temp.Clear();
                    temp.Add(frequency);
                }
                else
                {
                    temp.Add(frequency);
                }
            }

            if (index == freqs.Count - 1)
                if (temp.Any())
                {
                    var b = GetFrequencyRange(temp, out var range);
                    if (b) result.Add(range);
                }

            index++;
        }

        return result;
    }

    private bool GetFrequencyRange(List<double> freqs,
        out (double startFrequency, double stopFrequency, double stepFrequency) range)
    {
        range = (0, 0, 0);
        if (freqs?.Any() != true) return false;
        if (freqs.Count == 1)
        {
            var freq = freqs[0];
            var start = freq - 0.7;
            var stop = freq + 0.7;
            if (start.CompareWith(DeviceStartFrequency) < 0) start = DeviceStartFrequency;
            if (stop.CompareWith(DeviceStopFrequency) > 0) stop = DeviceStopFrequency;
            range = (start, stop, 15);
        }
        else
        {
            var start = freqs[0];
            var stop = freqs[^1];
            range = (start, stop, 15);
        }

        return true;
    }

    private void Monitor()
    {
        var buffer = new byte[1024];
        while (_cts?.IsCancellationRequested == false)
            try
            {
                if (Socket is not { Connected: true })
                {
                    Disconnected?.Invoke(this, new DeviceDisconnectedEventArgs(SubSystemIndex, PhysicalChannelNumber));
                    break;
                }

                var count = Socket.Receive(buffer, SocketFlags.None);
                if (count == 0)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var str = Encoding.ASCII.GetString(buffer, 0, count);
                if (string.IsNullOrEmpty(str))
                {
                    Thread.Sleep(10);
                    continue;
                }

                Trace.WriteLine($"信号源{PhysicalChannelNumber}：{str}");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.Shutdown or SocketError.NotConnected)
                    Disconnected?.Invoke(this, new DeviceDisconnectedEventArgs(SubSystemIndex, PhysicalChannelNumber));
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex);
#endif
            }
    }

    #region 指令

    /// <summary>
    ///     信号源发射开关
    /// </summary>
    private const string ScTrace = "TRACe";

    /// <summary>
    ///     中心频率
    /// </summary>
    private const string ScFreqCent = "FREQuency";

    /// <summary>
    ///     起始频率
    /// </summary>
    private const string ScFreqStart = "FREQuency:STARt";

    /// <summary>
    ///     终止频率
    /// </summary>
    private const string ScFreqStop = "FREQuency:STOP";

    /// <summary>
    ///     衰减器设置。1dB步进
    ///     TODO: 后续修改为功率设置命令
    /// </summary>
    private const string ScAmpAttRf = "ATTenuation:RF";

    private const string ScSignalPower = "TX:SIGNal:POWER";

    /// <summary>
    ///     信号源模式
    /// </summary>
    private const string ScSignalMode = "TX:SIGNal:MODE";

    /// <summary>
    ///     梳状谱频率间隔
    /// </summary>
    private const string ScCombStep = "TX:COMB:STEP";

    /// <summary>
    ///     多音通道个数
    /// </summary>
    private const string ScToneCount = "TX:MULTITone:COUNt";

    /// <summary>
    ///     多音通道表
    /// </summary>
    private const string ScToneContents = "TX:MULTITone:CONTents";

    /// <summary>
    ///     DAC输出最大幅度线性值，
    ///     默认值1024，最大值16383，一般不要超过2000
    /// </summary>
    private const string ScFullScale = "TX:FULL:SCALE";

    private const string ScTime = "TIME";
    private const string ScScanStep = "Step";

    #endregion
}