using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC.SDK;

public class G33DdcCommon
{
    #region DDC2

    private readonly Dictionary<uint, Ddc2ChannelInfo> _ddc2Channels = new();

    #endregion

    public void SetDetector(DetectMode mode)
    {
        _detectMode = mode;
    }

    #region IF

    private readonly float[] _ifSpectrumCompensation = new float[Define.MaxIfFftSize * 2];
    private int _ifRbwIndex;

    /// <summary>
    ///     实际设置到设备中的起始频率
    /// </summary>
    private uint _startFrequency;

    /// <summary>
    ///     实际设置到设备中的结束频率
    /// </summary>
    private uint _stopFrequency;

    /// <summary>
    ///     实际设置到设备中的步进
    /// </summary>
    private uint _stepFrequency;

    private bool _isIfStart;

    #endregion

    #region DDC1

    private float[] _ddc1Buffer = new float[Define.MaxFftSize * 2];
    private short[] _ddc1IqBuffer16 = new short[Define.MaxFftSize * 2];
    private int[] _ddc1IqBuffer32 = new int[Define.MaxFftSize * 2];
    private readonly float[] _ddc1Coe = new float[Define.MaxFftSize];
    private readonly float[] _ddc1SpectrumCompensation = new float[Define.MaxFftSize];
    private int _ddc1BwIndex;
    private int _ddc1RbwIndex;
    private uint _ddc1FftSize;
    private uint _ddc1Offset;
    private uint _ddc1FftBinCount;
    private uint _ddc1FftFirstBin;
    private double _ddc1FftRange;
    private double _ddc1FftOffset;
    private uint _ddc1Frequency;
    private uint _ddc1Bandwidth;
    private uint _ddc1SampleRate;
    private bool _isDdc1Start;
    private uint _interval = 10;

    /// <summary>
    ///     采样率 kHz
    /// </summary>
    public double Ddc1SampleRate => _ddc1SampleRate / 1000d;

    #endregion

    #region 其他定义

    private G33Ddcsdk.G33DdcCallbacks _callbacks;

    /*
        channel:0,sampleRate:25000,bw:20000,bitsPerSample:32
        channel:1,sampleRate:32000,bw:24000,bitsPerSample:32
        channel:2,sampleRate:40000,bw:32000,bitsPerSample:32
        channel:3,sampleRate:50000,bw:40000,bitsPerSample:32
        channel:4,sampleRate:62500,bw:50000,bitsPerSample:32
        channel:5,sampleRate:80000,bw:64000,bitsPerSample:32
        channel:6,sampleRate:100000,bw:80000,bitsPerSample:32
        channel:7,sampleRate:125000,bw:100000,bitsPerSample:32
        channel:8,sampleRate:160000,bw:125000,bitsPerSample:32
        channel:9,sampleRate:200000,bw:160000,bitsPerSample:32
        channel:10,sampleRate:250000,bw:200000,bitsPerSample:32
        channel:11,sampleRate:312500,bw:250000,bitsPerSample:32
        channel:12,sampleRate:400000,bw:320000,bitsPerSample:32
        channel:13,sampleRate:500000,bw:400000,bitsPerSample:32
        channel:14,sampleRate:625000,bw:500000,bitsPerSample:32
        channel:15,sampleRate:800000,bw:640000,bitsPerSample:32
        channel:16,sampleRate:1000000,bw:800000,bitsPerSample:32
        channel:17,sampleRate:1250000,bw:1000000,bitsPerSample:32
        channel:18,sampleRate:1666667,bw:1250000,bitsPerSample:32
        channel:19,sampleRate:2000000,bw:1600000,bitsPerSample:32
        channel:20,sampleRate:2500000,bw:2000000,bitsPerSample:32
        channel:21,sampleRate:3333333,bw:2500000,bitsPerSample:32
        channel:22,sampleRate:4000000,bw:3200000,bitsPerSample:16
        channel:23,sampleRate:5000000,bw:4000000,bitsPerSample:16
     */
    private readonly uint[] _ddc1SampleRates =
    {
        25000, 32000, 40000, 50000, 62500, 80000,
        100000, 125000, 160000, 200000, 250000, 312500,
        400000, 500000, 625000, 800000, 1000000, 1250000,
        1666667, 2000000, 2500000, 3333333, 4000000, 5000000
    };

    private readonly uint[] _ddc1Bandwidths =
    {
        20000, 24000, 32000, 40000, 50000, 64000,
        80000, 100000, 125000, 160000, 200000, 250000,
        320000, 400000, 500000, 640000, 800000, 1000000,
        1250000, 1600000, 2000000, 2500000, 3200000, 4000000
    };

    private readonly uint[] _startFrequencies =
    {
        0, 700000, 1200000, 2000000, 2900000, 4000000, 5100000, 5600000, 8300000, 9200000, 9500000, 13300000, 14600000,
        16100000, 21600000
    };

    private readonly uint[] _stopFrequencies =
    {
        2200000, 2700000, 4000000, 4900000, 6200000, 7400000, 9400000, 10700000, 12900000, 14600000, 18900000, 23400000,
        25800000, 32100000, 50000000
    };

    private readonly uint[] _stepFrequencies = { 98000, 48800, 24400, 12200, 6100, 3100, 1500 };
    public event EventHandler<float[]> IfDataReceived;
    public event EventHandler<float[]> Ddc1DataReceived;

    public delegate void Ddc2DataReceivedDelegate(int channel, float[] data);

    public delegate void Ddc2AudioDataReceivedDelegate(int channel, byte[] data);

    public delegate void Ddc1IqDataReceivedDelegate(short[] iqData16, int[] iqData32);

    public delegate void Ddc2PreprocessedStreamDelegate(uint channel, float level, float slevelPeak, float slevelRms,
        double gain);

    public event Ddc2DataReceivedDelegate Ddc2DataReceived;
    public event Ddc2AudioDataReceivedDelegate AudioDataReceived;
    public event Ddc1IqDataReceivedDelegate Ddc1IqDataReceived;
    public event Ddc2PreprocessedStreamDelegate Ddc2PreprocessedDataReceived;

    #endregion

    #region 启动/停止

    /// <summary>
    ///     连接设备
    /// </summary>
    /// <returns></returns>
    internal bool Connect()
    {
        _callbacks.IfCallback += IfCallback;
        _callbacks.Ddc1StreamCallback += Ddc1StreamCallback;
        _callbacks.Ddc2StreamCallback += Ddc2StreamCallback;
        _callbacks.AudioStreamCallback += AudioStreamCallback;
        _callbacks.Ddc2PreprocessedStreamCallback += Ddc2PreprocessedStreamCallback;
        var info = G33Ddcsdk.GetDeviceList((uint)Marshal.SizeOf(typeof(G33DdcDeviceInfo)));
        if (info == null) return false;
        Console.WriteLine($"Count:{info.Length},{info[0].SerialNumber}");
        var serialNumber = info[0].SerialNumber;
        var res = G33Ddcsdk.Open(serialNumber);
        if (!res) return false;
        res = G33Ddcsdk.SetCallbacks(ref _callbacks, IntPtr.Zero);
        var connected = G33Ddcsdk.IsDeviceConnected();
        _ddc2Channels.Clear();
        for (uint i = 0; i < 3; i++)
        {
            var ddc2 = new Ddc2ChannelInfo(i, Define.MaxFftSize, Define.MinFftSize);
            _ddc2Channels.Add(i, ddc2);
        }

        return connected;
    }

    internal void Close()
    {
        if (G33Ddcsdk.IsDeviceConnected())
        {
            if (_ddc2Channels != null)
                foreach (var item in _ddc2Channels)
                    item.Value.Stop();
            G33Ddcsdk.StopDdc1();
            G33Ddcsdk.StopIf();
            G33Ddcsdk.SetPower(false);
            G33Ddcsdk.Close();
        }

        _callbacks.IfCallback -= IfCallback;
        _callbacks.Ddc1StreamCallback -= Ddc1StreamCallback;
        _callbacks.Ddc2StreamCallback -= Ddc2StreamCallback;
        _callbacks.AudioStreamCallback -= AudioStreamCallback;
        _callbacks.Ddc2PreprocessedStreamCallback -= Ddc2PreprocessedStreamCallback;
    }

    internal bool PowerOn()
    {
        if (!G33Ddcsdk.IsDeviceConnected()) return false;
        return G33Ddcsdk.SetPower(true);
    }

    internal bool PowerOff()
    {
        if (!G33Ddcsdk.IsDeviceConnected()) return false;
        return G33Ddcsdk.SetPower(false);
    }

    internal void Test()
    {
        G33Ddcsdk.SetPreselectors(_startFrequencies[1], _stopFrequencies[2]);
        //var chn = _ddc2Channels[0];
        //foreach (DemodulatorCode code in Enum.GetValues(typeof(DemodulatorCode)))
        //{
        //    var obj = chn.GetDemodulator(code);
        //}
        //bool res;
        //var size = Marshal.SizeOf(typeof(uint));
        //IntPtr ptr = Marshal.AllocHGlobal(size);
        //Marshal.StructureToPtr((uint)2, ptr, true);
        //var kk = Marshal.PtrToStructure<uint>(ptr);
        //res = G33DDCSDK.SetDemodulatorParam( 0, 1, ptr, (uint)size);
        //if (!res)
        //{
        //    var code = G33DDCSDK.GetLastError();
        //}
        //uint[] bf = new uint[1] { 3 };
        //IntPtr ptr1 = Marshal.AllocHGlobal(size);
        //res = G33DDCSDK.GetDemodulatorParam( 0, 1, ptr1, (uint)size);
        //var kk2 = Marshal.PtrToStructure<uint>(ptr1);
        //if (!res)
        //{
        //    var code = G33DDCSDK.GetLastError();
        //}
        //res = G33DDCSDK.GetDemodulatorMode( 0, out var mode);
    }

    internal bool StartIf()
    {
        _isIfStart = true;
        return G33Ddcsdk.StartIf(Define.IfUpdateInterval);
    }

    /// <summary>
    ///     开启DDC1
    /// </summary>
    /// <param name="interval">数据发送间隔 ms</param>
    /// <returns></returns>
    internal bool StartDdc1(uint interval = 10)
    {
        _interval = interval;
        _isDdc1Start = true;
        _ddc1Offset = 0;
        var samplesPerBuffer = (uint)((_ddc1SampleRate * interval / 1000 + 63) & ~63);
        //Console.WriteLine($"Set DDC1 SamplePerBuffer:{samplesPerBuffer}");
        return G33Ddcsdk.StartDdc1(samplesPerBuffer);
    }

    /// <summary>
    ///     启动DDC2之前必须SetPower(true)并且启动DDC1
    /// </summary>
    /// <param name="startChannels"></param>
    /// <returns></returns>
    internal bool StartDdc2(List<uint> startChannels)
    {
        foreach (var item in _ddc2Channels)
        {
            if (!startChannels.Contains(item.Key))
            {
                item.Value.Stop();
                continue;
            }

            item.Value.Start();
        }

        return true;
    }

    /// <summary>
    ///     启动DDC2之前必须SetPower(true)并且启动DDC1
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="interval"></param>
    internal bool StartDdc2(uint channel, uint interval = 50)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return false;
        var info = _ddc2Channels[channel];
        return info.Start(interval);
    }

    internal bool StopDdc2(uint channel)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return false;
        var info = _ddc2Channels[channel];
        return info.Stop();
    }

    internal bool StopDdc2()
    {
        foreach (var item in _ddc2Channels) item.Value.Stop();
        return true;
    }

    /// <summary>
    ///     启动Audio之前必须SetPower(true)并且启动DDC2
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="interval"></param>
    internal bool StartAudio(uint channel, uint interval = 200)
    {
        foreach (var item in _ddc2Channels)
        {
            if (item.Key != channel)
            {
                item.Value.Stop();
                continue;
            }

            item.Value.StartAudio(interval);
        }

        return true;
    }

    internal bool StopAudio(uint channel)
    {
        foreach (var item in _ddc2Channels)
        {
            if (item.Key != channel) continue;
            item.Value.StopAudio();
            return true;
        }

        return false;
    }

    internal void Stop()
    {
        foreach (var item in _ddc2Channels) item.Value.Stop();
        _isIfStart = false;
        _isDdc1Start = false;
        G33Ddcsdk.StopDdc1();
        G33Ddcsdk.StopIf();
    }

    internal void SetDdc1(uint frequency, int ddc1BwIndex, int ddc1RbwIndex)
    {
        try
        {
            _ddc1Frequency = frequency;
            //RBWs[i] = (double)m_DDC1_Info.SampleRate / (MIN_FFT_SIZE << i);
            // ddc1的rbw = SampleRate / (MIN_FFT_SIZE << i)
            // i: 0 1 2 3 4 5
            _ddc1RbwIndex = ddc1RbwIndex;
            _ddc1BwIndex = ddc1BwIndex;
            G33Ddcsdk.SetDdc1Frequency(frequency);
            G33Ddcsdk.SetDdc1((uint)_ddc1BwIndex);
            //var res1 = G33DDCSDK.GetDDC1Count( out var channels);
            //if (res1)
            //{
            //    for (uint i = 0; i < channels; i++)
            //    {
            //        res1 = G33DDCSDK.GetDDCInfo( i, out var ddcInfo);
            //        Console.WriteLine($"channel:{i},sampleRate:{ddcInfo.SampleRate},bw:{ddcInfo.Bandwidth},bitsPerSample:{ddcInfo.BitsPerSample}");
            //        var kkk = G33DDCSDK.GetDDC2( out uint ddc2Index, out var ddc2Info);
            //    }
            //}
            //ddc1 
            _ddc1FftSize = Define.MinFftSize << ddc1RbwIndex;
            if (_ddc1FftSize > Define.MaxFftSize) _ddc1FftSize = Define.MaxFftSize;
            Helper.GetNormalizedWindowCoeffs(_ddc1Coe, (int)_ddc1FftSize, 1);
            _ddc1SampleRate = _ddc1SampleRates[ddc1BwIndex];
            _ddc1Bandwidth = _ddc1Bandwidths[ddc1BwIndex];
            var k = (double)_ddc1SampleRate / _ddc1FftSize;
            var fFirst = (_ddc1SampleRate - _ddc1Bandwidth) * 0.5 / k;
            var fLast = (_ddc1SampleRate - (_ddc1SampleRate - _ddc1Bandwidth) * 0.5) / k;
            var first = (int)fFirst;
            var last = (int)Math.Ceiling(fLast);
            _ddc1FftBinCount = (uint)(last - first + 1);
            _ddc1FftFirstBin = (uint)first;
            _ddc1FftRange = _ddc1FftBinCount * k;
            _ddc1FftOffset = (fFirst - (first + 0.5)) * k;
            G33Ddcsdk.GetSpectrumCompensation(frequency, _ddc1SampleRate, _ddc1SpectrumCompensation, _ddc1FftSize);
            /////////
            _ddc1Offset = 0;
        }
        catch
        {
        }
    }

    internal void SetDdc2(uint channel, int ddc2Frequency, int ddc2DemoFrequency, int ddc2RbwIndex)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        var info = _ddc2Channels[channel];
        info.UpdateDdc2Frequency(ddc2Frequency);
        info.UpdateDemoFrequency(ddc2DemoFrequency);
        G33Ddcsdk.GetDdc2(out _, out var ddc2Info);
        info.UpdateDdc2FftCoeffs(ddc2RbwIndex, ddc2Info);
        var bw = ddc2Info.Bandwidth;
        //Console.WriteLine($"DDC2参数改变:{ddc2Info.Bandwidth},{ddc2Info.BitsPerSample},{ddc2Info.SampleRate}");
        info.UpdateAudioFftCoeffs(0);
        info.SetDemodulatorBandwidth(bw);
        //info.SetDemodulatorMode(DemodulatorMode.G3XDDC_MODE_FM);
    }

    #endregion

    #region 设置参数

    /// <summary>
    ///     设置起始频率与终止频率
    /// </summary>
    /// <param name="startFrequency">起始频率 Hz</param>
    /// <param name="stopFrequency">结束频率 Hz</param>
    /// <param name="stepFrequency">频率步进 Hz</param>
    public bool SetSegments(uint startFrequency, uint stopFrequency, uint stepFrequency)
    {
        var startIndex = Helper.FindNearestFrequency(_startFrequencies, startFrequency, false);
        var stopIndex = Helper.FindNearestFrequency(_stopFrequencies, stopFrequency, true);
        if (startIndex < 0 || stopIndex < 0) return false;
        var start = _startFrequencies[startIndex];
        var stop = _stopFrequencies[stopIndex];
        if (stop <= start) return false;
        var stepIndex = Array.IndexOf(_stepFrequencies, stepFrequency);
        if (stepIndex < 0) return false;
        _ifRbwIndex = stepIndex;
        _startFrequency = start;
        _stopFrequency = stop;
        _stepFrequency = stepFrequency;
        return G33Ddcsdk.SetPreselectors(start, stop);
    }

    /// <summary>
    ///     设置中心频率
    /// </summary>
    /// <param name="frequency">MHz</param>
    internal void SetFrequency(double frequency)
    {
        var freq = (uint)(frequency * 1000000);
        Pause();
        SetDdc1(freq, _ddc1BwIndex, _ddc1RbwIndex);
        Resume();
    }

    /// <summary>
    ///     设置中频带宽
    /// </summary>
    /// <param name="bandwidth">kHz</param>
    /// <param name="isDdc"></param>
    internal void SetIfBandwidth(double bandwidth, bool isDdc = false)
    {
        var bw = (uint)(bandwidth * 1000);
        var ddc1BwIndex = Helper.FindNearestFrequency(_ddc1Bandwidths, bw, true);
        if (ddc1BwIndex < 0) return;
        Pause();
        //Console.WriteLine($"中频带宽设置为:{_ddc1Bandwidths[ddc1BwIndex]}");
        SetDdc1(_ddc1Frequency, ddc1BwIndex, _ddc1RbwIndex);
        if (!isDdc) SetDdc2(0, 0, 0, 0);
        Resume();
    }

    internal void SetDemMode(uint channel, DemodulatorMode mode)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        var info = _ddc2Channels[channel];
        info.SetDemodulatorMode(mode);
    }

    internal void SetDemBandwidth(uint channel, double bandwidth)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        //Pause();
        var bw = (uint)(bandwidth * 1000);
        var info = _ddc2Channels[channel];
        info.SetDemodulatorBandwidth(bw);
        //Resume();
    }

    internal bool SetDemodulator(uint channel, DemodulatorCode demodulatorType, object value)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return false;
        var info = _ddc2Channels[channel];
        return info.SetDemodulator(demodulatorType, value);
    }

    /// <summary>
    ///     设置采样点数
    /// </summary>
    /// <param name="count"></param>
    internal void SetSamplingCount(uint count)
    {
        _ddc1FftSize = count;
        _ddc1RbwIndex = Helper.Find2Xn(_ddc1FftSize / Define.MinFftSize);
        Pause();
        SetDdc1(_ddc1Frequency, _ddc1BwIndex, _ddc1RbwIndex);
        Resume();
    }

    internal void SetDdc2Frequency(uint channel, double frequency)
    {
        var freq = (int)(frequency * 1000000);
        if (!_ddc2Channels.ContainsKey(channel)) return;
        Pause();
        SetDdc2(channel, freq, 0, 0);
        Resume();
    }

    /// <summary>
    ///     设置静噪门限
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    internal void SetSquelchSwitch(uint channel, bool enabled)
    {
    }

    internal bool SetAgcEnabled(uint channel, bool enabled)
    {
        return G33Ddcsdk.SetAgc(channel, enabled);
    }

    /// <summary>
    ///     设置衰减值
    /// </summary>
    /// <param name="attenuator"></param>
    internal bool SetAttenuator(uint attenuator)
    {
        return G33Ddcsdk.SetAttenuator(attenuator);
    }

    /// <summary>
    ///     启用或禁用前置放大器
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    internal bool SetPreamp(bool enabled)
    {
        return G33Ddcsdk.SetPreamp(enabled);
    }

    /// <summary>
    ///     启用或禁用ADC抖动
    /// </summary>
    /// <param name="enabled"></param>
    /// <returns></returns>
    internal bool SetDithering(bool enabled)
    {
        // 默认打开ADC抖动
        return G33Ddcsdk.SetDithering(enabled);
    }

    internal bool SetAgcMode(uint channel, AgcMode agcMode)
    {
        /*
            case AGC_OFF:
                m_Device->SetAGC(Channel,FALSE);
                break;
            case AGC_SLOW:
                m_Device->SetAGCParams(Channel,0.025,4.0,-15);
                m_Device->SetAGC(Channel,TRUE);
                break;
            case AGC_MEDIUM:
                m_Device->SetAGCParams(Channel,0.015,2.0,-15);
                m_Device->SetAGC(Channel,TRUE);
                break;
            case AGC_FAST:
                m_Device->SetAGCParams(Channel,0.005,0.2,-15);
                m_Device->SetAGC(Channel,TRUE);
                break;
         */
        switch (agcMode)
        {
            case AgcMode.AgcOff:
                return G33Ddcsdk.SetAgc(channel, false);
            case AgcMode.AgcSlow:
                G33Ddcsdk.SetAgcParams(channel, 0.025, 4.0, -15);
                return G33Ddcsdk.SetAgc(channel, true);
            case AgcMode.AgcMedium:
                G33Ddcsdk.SetAgcParams(channel, 0.015, 2.0, -15);
                return G33Ddcsdk.SetAgc(channel, true);
            case AgcMode.AgcFast:
                G33Ddcsdk.SetAgcParams(channel, 0.005, 0.2, -15);
                return G33Ddcsdk.SetAgc(channel, true);
        }

        return true;
    }

    internal bool SetGain(uint channel, double gain)
    {
        return G33Ddcsdk.SetGain(channel, gain);
    }

    internal void SetMeasureTime(uint interval)
    {
        Pause();
        if (interval == 0) interval = 10;
        _interval = interval;
        Resume();
    }

    #endregion

    #region 查询

    /// <summary>
    ///     查询指定通道的电平值
    /// </summary>
    /// <param name="channel"></param>
    public float GetLevel(uint channel)
    {
        if (!G33Ddcsdk.GetSignalLevel(channel, out _, out var rms)) return float.NaN;
        var level = 10.0f * (float)Math.Log10(rms * rms * (1000.0 / 50.0)) + 107;
        return level;
    }

    public bool GetLevel(uint channel, out float peak, out float rms)
    {
        peak = 0f;
        rms = 0f;
        if (!G33Ddcsdk.GetSignalLevel(channel, out var pk, out var rs)) return false;
        if (rs == 0 || pk == 0) return false;
        peak = 10.0f * (float)Math.Log10(pk * pk * (1000.0 / 50.0)) + 107;
        rms = 10.0f * (float)Math.Log10(rs * rs * (1000.0 / 50.0)) + 107;
        return true;
    }

    #region ITU测量

    /// <summary>
    ///     获取频差
    /// </summary>
    /// <returns></returns>
    public int GetDemTuneError(uint channel)
    {
        var size = Marshal.SizeOf(typeof(int));
        var ptr = Marshal.AllocHGlobal(size);
        G33Ddcsdk.GetDemodulatorState(channel, (uint)DemodulatorState.G3XddcDemodulatorStateTuneError,
            ptr, (uint)size);
        return Marshal.PtrToStructure<int>(ptr);
    }

    /// <summary>
    ///     获取AM调制深度
    /// </summary>
    /// <returns></returns>
    public double GetAmDepth(uint channel)
    {
        var size = Marshal.SizeOf(typeof(double));
        var ptr = Marshal.AllocHGlobal(size);
        G33Ddcsdk.GetDemodulatorState(channel, (uint)DemodulatorState.G3XddcDemodulatorStateAmDepth, ptr,
            (uint)size);
        return Marshal.PtrToStructure<double>(ptr);
    }

    /// <summary>
    ///     获取FM频偏
    /// </summary>
    /// <returns></returns>
    public double GetDeviation(uint channel)
    {
        var size = Marshal.SizeOf(typeof(uint));
        var ptr = Marshal.AllocHGlobal(size);
        G33Ddcsdk.GetDemodulatorState(channel, (uint)DemodulatorState.G3XddcDemodulatorStateFmDeviation,
            ptr, (uint)size);
        return Marshal.PtrToStructure<uint>(ptr);
    }

    #endregion

    #endregion

    #region 私有方法

    private void Pause()
    {
        foreach (var item in _ddc2Channels) item.Value.Pause();
        if (_isDdc1Start) G33Ddcsdk.StopDdc1();
        if (_isIfStart) G33Ddcsdk.StopIf();
    }

    private void Resume()
    {
        if (_isIfStart) StartIf();
        if (_isDdc1Start) StartDdc1(_interval);
        foreach (var item in _ddc2Channels) item.Value.Resume();
    }

    #endregion

    #region Callback

    private void AudioStreamCallback(uint channel, IntPtr intPtr, IntPtr bufferFiltered, uint numberOfSamples,
        IntPtr userData)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        var info = _ddc2Channels[channel];
        var data = info.SetAudio(bufferFiltered, numberOfSamples, userData);
        if (data == null) return;
        AudioDataReceived?.Invoke((int)channel, data);
    }

    private void Ddc2StreamCallback(uint channel, IntPtr intPtr, uint numberOfSamples, IntPtr userData)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        if (!_ddc2Channels.ContainsKey(channel)) return;
        //if (_isInFScanMode && !_canFscanChangeFrequency)
        //{
        //    var res = G33DDCSDK.GetCurrentGain(channel, out var gain);
        //    _levelCount++;
        //    var buffer = new float[numberOfSamples * 2];
        //    Marshal.Copy(intPtr, buffer, 0, (int)numberOfSamples * 2);
        //    var level = Helper.GetLevel(buffer) + 111;
        //    if (_levelMax < level)
        //    {
        //        _levelMax = level;
        //    }
        //    if (_levelMin > level)
        //    {
        //        _levelMin = level;
        //    }
        //    _level = level;
        //    _levelMean = (_levelMean * (_levelCount - 1) + level) / _levelCount;
        //    //_peak = 10.0f * (float)Math.Log10(slevelPeak * slevelPeak * (1000.0 / 50.0)) + 107;
        //    //_rms = 10.0f * (float)Math.Log10(slevelRms * slevelRms * (1000.0 / 50.0)) + 107;
        //    var span = DateTime.Now.Subtract(_preScanTime).TotalMilliseconds;
        //    if (span > interval)
        //    {
        //        Console.WriteLine($"频率:{_frequency},gain:{gain},电平:{_level},max:{_levelMax},mean:{_levelMean},peak:{_peak},rms:{_rms},span:{span}");
        //        _preScanTime = DateTime.Now;
        //        _canFscanChangeFrequency = true;
        //    }
        //}
        //else
        {
            var info = _ddc2Channels[channel];
            var data = info.SetData(intPtr, numberOfSamples, userData);
            if (data == null) return;
            Ddc2DataReceived?.Invoke((int)channel, data);
        }
    }

    private DetectMode _detectMode = DetectMode.Fast;

    private void Ddc1StreamCallback(IntPtr intPtr, uint numberOfSamples, uint bitsPerSample, IntPtr userData)
    {
        try
        {
            //if (_detectMode != DetectMode.FAST && numberOfSamples > _ddc1FFTSize)
            //{
            //    AVGDetectorProcess(intPtr, numberOfSamples, bitsPerSample);
            //}
            //else
            {
                FastDetectorProcess(intPtr, numberOfSamples, bitsPerSample);
            }
        }
        catch (Exception)
        {
        }
    }

    private void FastDetectorProcess(IntPtr intPtr, uint numberOfSamples, uint bitsPerSample)
    {
        if (bitsPerSample == 16)
        {
            var res = Helper.AddSample_16(intPtr, ref _ddc1Buffer, ref _ddc1Offset, numberOfSamples, _ddc1FftSize,
                ref _ddc1IqBuffer16);
            if (!res) return;
            var iqData = new short[_ddc1FftSize * 2];
            Buffer.BlockCopy(_ddc1IqBuffer16, 0, iqData, 0, iqData.Length * sizeof(short));
            Ddc1IqDataReceived?.Invoke(iqData, null);
        }
        else if (bitsPerSample == 32)
        {
            var res = Helper.AddSample_32(intPtr, ref _ddc1Buffer, ref _ddc1Offset, numberOfSamples, _ddc1FftSize,
                ref _ddc1IqBuffer32);
            if (!res) return;
            var iqData = new int[_ddc1FftSize * 2];
            Buffer.BlockCopy(_ddc1IqBuffer32, 0, iqData, 0, iqData.Length * sizeof(int));
            Ddc1IqDataReceived?.Invoke(null, iqData);
        }

        _ddc1Offset = 0;
        if (_ddc1Buffer == null) return;
        //apply FFT normalization and window coefficients at once
        var buffer = new float[_ddc1FftSize * 2];
        for (var i = 0; i < _ddc1FftSize; i++)
        {
            buffer[i * 2] = _ddc1Buffer[i * 2] * _ddc1Coe[i];
            buffer[i * 2 + 1] = _ddc1Buffer[i * 2 + 1] * _ddc1Coe[i];
        }

        Helper.Fft(ref buffer, _ddc1FftSize);
        //swap lower and upper half of the FFT result
        var binCount = _ddc1FftSize / 2;
        for (var i = 0; i < binCount; i++)
        {
            var tmpI = buffer[i * 2];
            var tmpQ = buffer[i * 2 + 1];
            buffer[i * 2] = buffer[(i + binCount) * 2];
            buffer[i * 2 + 1] = buffer[(i + binCount) * 2 + 1];
            buffer[(i + binCount) * 2] = tmpI;
            buffer[(i + binCount) * 2 + 1] = tmpQ;
        }

        //now frequency spectrum after this FFT looks like the following
        // |                                     |                                     |
        // |                                     |                                     |
        // |------*******************************|*******************************------|
        // -DDC1 sample rate/2        baseband frequency (0)        +DDC1 sample rate /2
        // The first FFT bin               Middle FFT bin               The last FFT bin
        //asterisks show useful band (G3XDDC_DDC_INFO.Bandwidth) in FFT result
        //the following converts amplitudes to dB and applies compensation coeffs
        //to have absolute values in dBm instead of relative ones in dB
        binCount = _ddc1FftBinCount;
        var comp = _ddc1SpectrumCompensation;
        for (int i = 0, j = (int)_ddc1FftFirstBin; i < binCount; i++, j++)
        {
            var m = buffer[j * 2] * buffer[j * 2] + buffer[j * 2 + 1] * buffer[j * 2 + 1];
            if (m > 0)
                buffer[j] = (float)(10 * Math.Log10(m));
            else
                buffer[j] = -180;
            buffer[j] += comp[j];
        }

        var data = new float[binCount];
        Buffer.BlockCopy(buffer, (int)_ddc1FftFirstBin * sizeof(float), data, 0, (int)binCount * sizeof(float));
        Ddc1DataReceived?.Invoke(null, data);
    }

    private void AvgDetectorProcess(IntPtr intPtr, uint numberOfSamples, uint bitsPerSample)
    {
        Stopwatch sw = new();
        sw.Start();
        _ddc1Offset = 0;
        List<float[]> list = new();
        if (bitsPerSample == 16)
        {
            var res = Helper.AddSample_16(intPtr, out list, ref _ddc1Offset, numberOfSamples, _ddc1FftSize,
                out var iqData);
            Ddc1IqDataReceived?.Invoke(iqData, null);
            if (!res) return;
        }
        else if (bitsPerSample == 32)
        {
            var res = Helper.AddSample_32(intPtr, out list, ref _ddc1Offset, numberOfSamples, _ddc1FftSize,
                out var iqData);
            Ddc1IqDataReceived?.Invoke(null, iqData);
            if (!res) return;
        }

        var finalData = Enumerable.Repeat(float.MinValue, (int)_ddc1FftBinCount).ToArray();
        if (_detectMode is DetectMode.Avg or DetectMode.Rms) finalData = new float[_ddc1FftBinCount];
        var count = 0;
        Console.WriteLine($"IQCount:{list.Count}");
        foreach (var item in list)
        {
            //apply FFT normalization and window coefficients at once
            var buffer = new float[_ddc1FftSize * 2];
            for (var i = 0; i < _ddc1FftSize; i++)
            {
                buffer[i * 2] = item[i * 2] * _ddc1Coe[i];
                buffer[i * 2 + 1] = item[i * 2 + 1] * _ddc1Coe[i];
            }

            Helper.Fft(ref buffer, _ddc1FftSize);
            //swap lower and upper half of the FFT result
            var binCount = _ddc1FftSize / 2;
            for (var i = 0; i < binCount; i++)
            {
                var tmpI = buffer[i * 2];
                var tmpQ = buffer[i * 2 + 1];
                buffer[i * 2] = buffer[(i + binCount) * 2];
                buffer[i * 2 + 1] = buffer[(i + binCount) * 2 + 1];
                buffer[(i + binCount) * 2] = tmpI;
                buffer[(i + binCount) * 2 + 1] = tmpQ;
            }

            //now frequency spectrum after this FFT looks like the following
            // |                                     |                                     |
            // |                                     |                                     |
            // |------*******************************|*******************************------|
            // -DDC1 sample rate/2        baseband frequency (0)        +DDC1 sample rate /2
            // The first FFT bin               Middle FFT bin               The last FFT bin
            //asterisks show useful band (G3XDDC_DDC_INFO.Bandwidth) in FFT result
            //the following converts amplitudes to dB and applies compensation coeffs
            //to have absolute values in dBm instead of relative ones in dB
            binCount = _ddc1FftBinCount;
            var comp = _ddc1SpectrumCompensation;
            count++;
            for (int i = 0, j = (int)_ddc1FftFirstBin; i < binCount; i++, j++)
            {
                var m = buffer[j * 2] * buffer[j * 2] + buffer[j * 2 + 1] * buffer[j * 2 + 1];
                if (_detectMode == DetectMode.Rms)
                {
                    finalData[i] += m;
                    if (count == list.Count)
                    {
                        if (finalData[i] > 0)
                            finalData[i] = (float)(10 * Math.Log10(finalData[i] / count));
                        else
                            finalData[i] = -180;
                        finalData[i] += comp[j];
                    }
                }
                else
                {
                    if (m > 0)
                        buffer[j] = (float)(10 * Math.Log10(m));
                    else
                        buffer[j] = -180;
                    buffer[j] += comp[j];
                    switch (_detectMode)
                    {
                        case DetectMode.Pos:
                            if (finalData[i] < buffer[j]) finalData[i] = buffer[j];
                            break;
                        case DetectMode.Avg:
                            finalData[i] = (finalData[i] * (count - 1) + buffer[j]) / count;
                            break;
                        case DetectMode.Fast:
                        case DetectMode.Rms:
                            break;
                    }
                }
            }
            //var data = new float[binCount];
            //Buffer.BlockCopy(buffer, (int)_ddc1FFTFirstBin * sizeof(float), data, 0, (int)binCount * sizeof(float));
        }

        sw.Stop();
        Console.WriteLine($"处理时长:{sw.ElapsedMilliseconds}ms");
        Ddc1DataReceived?.Invoke(null, finalData);
    }

    private void IfCallback(IntPtr intPtr, uint numberOfSamples, short maxAdcAmplitude, uint adcSamplingRate,
        IntPtr userData)
    {
        var buffer = new short[numberOfSamples];
        Marshal.Copy(intPtr, buffer, 0, (int)numberOfSamples);
        //var buffer = ConvertIQ2Spectrum(Array.ConvertAll(data, item => (object)item));
        // if rbw: 100000000.0/Min_FFT_SIZE<<index
        // index: 0 1 2 3 4 5 6
        // rbw: 98 48.8 24.4 12.2 6.1 3.1 1.5 
        var fftSize = Define.MinIfFftSize << _ifRbwIndex;
        var coe = new float[Define.MaxIfFftSize];
        var fftBuffer = new float[Define.MaxIfFftSize * 2];
        Helper.GetNormalizedWindowCoeffs(coe, (int)fftSize, 1.0 / 32768.0d);
        G33Ddcsdk.GetSpectrumCompensation(Define.MaxIfFreq / 2, Define.MaxIfFreq, _ifSpectrumCompensation,
            fftSize / 2);
        for (var i = 0; i < fftSize; i++)
        {
            fftBuffer[i * 2] = buffer[i] * coe[i];
            fftBuffer[i * 2 + 1] = 0;
        }

        Helper.Fft(ref fftBuffer, fftSize);
        var half = fftSize / 2;
        for (var i = 0; i < half; i++)
        {
            var m = fftBuffer[i * 2] * fftBuffer[i * 2] + fftBuffer[i * 2 + 1] * fftBuffer[i * 2 + 1];
            if (m > 0)
                fftBuffer[i] = 10 * (float)Math.Log10(m);
            else
                fftBuffer[i] = -180;
            fftBuffer[i] += _ifSpectrumCompensation[i];
        }

        var data = new float[half];
        Buffer.BlockCopy(fftBuffer, 0, data, 0, (int)half * sizeof(float));
        IfDataReceived?.Invoke(null, data);
    }

    private void Ddc2PreprocessedStreamCallback(uint channel, IntPtr intPtr, uint numberOfSamples, float slevelPeak,
        float slevelRms, IntPtr userData)
    {
        if (!_ddc2Channels.ContainsKey(channel)) return;
        G33Ddcsdk.GetCurrentGain(channel, out var gain);
        var buffer = new float[numberOfSamples * 2];
        Marshal.Copy(intPtr, buffer, 0, (int)numberOfSamples * 2);
        var calibration = 107;
        var level = Helper.GetLevel(buffer) + calibration - (float)gain;
        //var rms1 = Helper.GetRMSLevel(buffer) + calibration - (float)gain;
        //var max = Helper.GetMaxLevel(buffer) + calibration - (float)gain;
        var peak = 10.0f * (float)Math.Log10(slevelPeak * slevelPeak * (1000.0 / 50.0)) + 107;
        var rms = 10.0f * (float)Math.Log10(slevelRms * slevelRms * (1000.0 / 50.0)) + 107;
        // Console.WriteLine($"count:{numberOfSamples},lvl:{level:0.000},rmsb:{rms:0.000},maxb:{peak:0.000},gain:{gain:0.000}");
        Ddc2PreprocessedDataReceived?.Invoke(channel, level, peak, rms, gain);
    }

    #endregion
}