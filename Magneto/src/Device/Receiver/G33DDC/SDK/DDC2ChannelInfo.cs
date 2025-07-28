using System;

namespace Magneto.Device.G33DDC.SDK;

internal class Ddc2ChannelInfo
{
    private readonly AudioDemodulator _audioDemodulator;
    private readonly uint _channelNumber;
    private readonly float[] _ddc2Coe;
    private readonly float[] _ddc2SpectrumCompensation;
    private readonly uint _maxFftSize;
    private readonly uint _minFftSize;
    private uint _audioInterval;
    private int _ddc1Frequency;
    private float[] _ddc2Buffer;
    private uint _ddc2FftBinCount;
    private uint _ddc2FftFirstBin;
    private double _ddc2FftOffset;
    private double _ddc2FftRange;
    private uint _ddc2FftSize;
    private int _ddc2Frequency;
    private G3XddcDdcInfo _ddc2Info;
    private uint _ddc2Interval;
    private uint _ddc2Offset;
    private int _ddc2RbwIndex;
    private int _ddc2RealFrequency;
    private int _demoRealFrequency;
    private bool _isAudioStart;
    private bool _isStart;

    public Ddc2ChannelInfo(uint channelNummber, uint maxSize, uint minSize)
    {
        _channelNumber = channelNummber;
        _maxFftSize = maxSize;
        _minFftSize = minSize;
        _ddc2Buffer = new float[maxSize * 2];
        _ddc2Coe = new float[maxSize];
        _ddc2SpectrumCompensation = new float[maxSize];
        _audioDemodulator = new AudioDemodulator(channelNummber, maxSize, minSize);
    }

    /// <summary>
    ///     更新DDC1频率
    /// </summary>
    /// <param name="ddc1Frequency"></param>
    public void UpdateDdc1Frequency(int ddc1Frequency)
    {
        _ddc1Frequency = ddc1Frequency;
    }

    /// <summary>
    ///     更新DDC2相对中心频率
    /// </summary>
    /// <param name="ddc2Frequency"></param>
    public bool UpdateDdc2Frequency(int ddc2Frequency)
    {
        _ddc2Frequency = ddc2Frequency;
        _ddc2RealFrequency = _ddc1Frequency + _ddc2Frequency;
        return G33Ddcsdk.SetDdc2Frequency(_channelNumber, _ddc2Frequency);
    }

    /// <summary>
    ///     更新DDC2的相对解调频率
    /// </summary>
    /// <param name="demoFrequency"></param>
    public bool UpdateDemoFrequency(int demoFrequency)
    {
        _demoRealFrequency = _ddc1Frequency + _ddc2Frequency + demoFrequency;
        return _audioDemodulator.UpdateDemoFrequency(demoFrequency);
    }

    public bool SetDemodulatorMode(DemodulatorMode mode)
    {
        return _audioDemodulator.SetDemodulatorMode(mode);
    }

    public bool SetDemodulatorBandwidth(uint bandwidth)
    {
        if (bandwidth > _ddc2Info.Bandwidth) bandwidth = _ddc2Info.Bandwidth;
        return _audioDemodulator.SetDemodulatorBandwidth(bandwidth);
    }

    public void UpdateDdc2FftCoeffs(int rbwIndex, G3XddcDdcInfo ddc2Info)
    {
        _ddc2Info = ddc2Info;
        _ddc2RbwIndex = rbwIndex;
        _ddc2FftSize = _minFftSize << rbwIndex;
        if (_ddc2FftSize > _maxFftSize) _ddc2FftSize = _maxFftSize;
        Helper.GetNormalizedWindowCoeffs(_ddc2Coe, (int)_ddc2FftSize, 1);
        var sampleRate = _ddc2Info.SampleRate;
        var bandwidth = _ddc2Info.Bandwidth;
        //var rbw = sampleRate / _ddc2FFTSize;
        var k = (double)sampleRate / _ddc2FftSize;
        var fFirst = (sampleRate - bandwidth) * 0.5 / k;
        var fLast = (sampleRate - (sampleRate - bandwidth) * 0.5) / k;
        var first = (int)fFirst;
        var last = (int)Math.Ceiling(fLast);
        _ddc2FftBinCount = (uint)(last - first + 1);
        _ddc2FftFirstBin = (uint)first;
        _ddc2FftRange = _ddc2FftBinCount * k;
        _ddc2FftOffset = (fFirst - (first + 0.5)) * k;
        G33Ddcsdk.GetSpectrumCompensation((ulong)_ddc2RealFrequency, sampleRate, _ddc2SpectrumCompensation,
            _ddc2FftSize);
        /////////
    }

    public void UpdateAudioFftCoeffs(int rbwIndex)
    {
        _audioDemodulator.UpdateAudioFftCoeffs(rbwIndex);
    }

    /// <summary>
    ///     启动DDC2数据输出
    /// </summary>
    /// <param name="interval">数据输出间隔 ms</param>
    /// <returns></returns>
    public bool Start(uint interval = 50)
    {
        _isStart = true;
        _ddc2Interval = interval;
        var samplesPerBuffer = (uint)((_ddc2Info.SampleRate * interval / 1000 + 63) & ~63);
        // Console.WriteLine($"Set DDC2 SamplePerBuffer:{samplesPerBuffer}");
        return G33Ddcsdk.StartDdc2(_channelNumber, samplesPerBuffer);
    }

    public bool StartAudio(uint interval)
    {
        _isAudioStart = true;
        _audioInterval = interval;
        return _audioDemodulator.Start(_audioInterval);
    }

    public bool Stop()
    {
        _audioDemodulator.Stop();
        _isAudioStart = false;
        _isStart = false;
        return G33Ddcsdk.StopDdc2(_channelNumber);
    }

    public bool StopAudio()
    {
        _isAudioStart = false;
        return _audioDemodulator.Stop();
    }

    public void Pause()
    {
        _audioDemodulator.Stop();
        G33Ddcsdk.StopDdc2(_channelNumber);
    }

    public void Resume()
    {
        if (_isStart) Start(_ddc2Interval);
        if (_isAudioStart) _audioDemodulator.Start(_audioInterval);
    }

    public float[] SetData(IntPtr intPtr, uint numberOfSamples, IntPtr userData)
    {
        if (!Helper.AddSample_F32(intPtr, ref _ddc2Buffer, ref _ddc2Offset, numberOfSamples, _ddc2FftSize)) return null;
        _ddc2Offset = 0;
        if (_ddc2Buffer == null) return null;
        //apply normalization and window coefficients at once
        var buffer = new float[_ddc2FftSize * 2];
        for (var i = 0; i < _ddc2FftSize; i++)
        {
            buffer[i * 2] = _ddc2Buffer[i * 2] * _ddc2Coe[i];
            buffer[i * 2 + 1] = _ddc2Buffer[i * 2 + 1] * _ddc2Coe[i];
        }

        Helper.Fft(ref buffer, _ddc2FftSize);
        //swap lower and upper half of the FFT result
        var binCount = _ddc2FftSize / 2;
        float tmpI;
        float tmpQ;
        for (var i = 0; i < binCount; i++)
        {
            tmpI = buffer[i * 2];
            tmpQ = buffer[i * 2 + 1];
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
        binCount = _ddc2FftBinCount;
        var comp = _ddc2SpectrumCompensation;
        for (int i = 0, j = (int)_ddc2FftFirstBin; i < binCount; i++, j++)
        {
            var m = buffer[j * 2] * buffer[j * 2] + buffer[j * 2 + 1] * buffer[j * 2 + 1];
            if (m > 0)
                buffer[j] = (float)(10 * Math.Log10(m));
            else
                buffer[j] = -180;
            buffer[j] += comp[j];
        }

        var data = new float[binCount];
        Buffer.BlockCopy(buffer, (int)_ddc2FftFirstBin * sizeof(float), data, 0, (int)binCount * sizeof(float));
        return data;
    }

    public byte[] SetAudio(IntPtr intPtr, uint numberOfSamples, IntPtr userData)
    {
        return AudioDemodulator.SetAudioData(intPtr, numberOfSamples);
    }

    public bool SetDemodulator(DemodulatorCode demodulatorType, object value)
    {
        return _audioDemodulator.SetDemodulator(demodulatorType, value);
    }
}