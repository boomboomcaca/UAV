using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.G33DDC.SDK;

internal class AudioDemodulator
{
    //private readonly float[] _audioBuffer;
    private readonly float[] _audioCoe;

    //private uint _audioOffset = 0;
    //private int _audioRbwIndex;
    //private readonly float[] _audioSpectrumCompensation;
    private readonly uint _channelNumber;
    private readonly uint _maxFftSize;

    private readonly uint _minFftSize;
    private uint _audioFftSize;
    private int _demoFrequency;

    //for(i=0;i<RBW_COUNT;i++)
    //{
    //    RBWs[i]=(double) AUDIO_SAMPLE_RATE/(MIN_FFT_SIZE<<i);
    //}
    public AudioDemodulator(uint channelNummber, uint maxSize, uint minSize)
    {
        _channelNumber = channelNummber;
        _maxFftSize = maxSize;
        _minFftSize = minSize;
        _audioCoe = new float[maxSize];
        //_audioBuffer = new float[maxSize];
        //_audioSpectrumCompensation = new float[maxSize];
    }

    /// <summary>
    ///     更新DDC2的相对解调频率
    /// </summary>
    /// <param name="demoFrequency"></param>
    public bool UpdateDemoFrequency(int demoFrequency)
    {
        _demoFrequency = demoFrequency;
        return G33Ddcsdk.SetDemodulatorFrequency(_channelNumber, _demoFrequency);
    }

    public void UpdateAudioFftCoeffs(int rbwIndex)
    {
        //_audioRbwIndex = rbwIndex;
        _audioFftSize = _minFftSize << rbwIndex;
        if (_audioFftSize > _maxFftSize) _audioFftSize = _maxFftSize;
        Helper.GetNormalizedWindowCoeffs(_audioCoe, (int)_audioFftSize, 1);
    }

    /// <summary>
    ///     启动音频输出
    /// </summary>
    /// <param name="interval">数据输出间隔 ms</param>
    /// <returns></returns>
    public bool Start(uint interval = 50)
    {
        var samplesPerBuffer = (uint)((Define.AudioSampleRate * interval / 1000 + 63) & ~63);
        //Console.WriteLine($"Set Audio SamplePerBuffer:{samplesPerBuffer}");
        return G33Ddcsdk.StartAudio(_channelNumber, samplesPerBuffer);
    }

    public bool Stop()
    {
        return G33Ddcsdk.StopAudio(_channelNumber);
    }

    public bool SetDemodulatorMode(DemodulatorMode mode)
    {
        return G33Ddcsdk.SetDemodulatorMode(_channelNumber, (uint)mode);
    }

    public bool SetDemodulatorBandwidth(uint bandwidth)
    {
        return G33Ddcsdk.SetDemodulatorFilterBandwidth(_channelNumber, bandwidth);
    }

    /// <summary>
    ///     解调相关设置
    ///     AMS与DRM两种解调模式暂时不做
    /// </summary>
    /// <param name="demodulatorType"></param>
    /// <param name="value">
    ///     每种解调模式对应的值不同：
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamAmsSideBand" />
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamDsbSideBand" />
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamIsbSideBand" />
    ///     时下发的值为<see cref="SideBandType" />,
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamAmsCaptureRange" />
    ///     时下发的值为<see cref="G3XddcAmsCaptureRange" />,
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamCwFrequency" />
    ///     时下发的值为<see cref="int" />,
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamDrmAudioService" />
    ///     <see cref="DemodulatorCode.G3XddcDemodulatorParamDrmMultimediaService" />
    ///     时下发的值为<see cref="uint" />,
    /// </param>
    /// <returns></returns>
    public bool SetDemodulator(DemodulatorCode demodulatorType, object value)
    {
        IntPtr ptr;
        int size;
        switch (demodulatorType)
        {
            case DemodulatorCode.G3XddcDemodulatorParamAmsSideBand:
            case DemodulatorCode.G3XddcDemodulatorParamDsbSideBand:
            case DemodulatorCode.G3XddcDemodulatorParamIsbSideBand:
                if (value is not SideBandType type) break;
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr((uint)type, ptr, true);
                return G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
            case DemodulatorCode.G3XddcDemodulatorParamAmsCaptureRange:
                if (value is not G3XddcAmsCaptureRange range) break;
                size = Marshal.SizeOf(typeof(G3XddcAmsCaptureRange));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(range, ptr, true);
                return G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
            case DemodulatorCode.G3XddcDemodulatorParamCwFrequency:
                if (value is not int freq) break;
                size = Marshal.SizeOf(typeof(int));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(freq, ptr, true);
                return G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
            case DemodulatorCode.G3XddcDemodulatorParamDrmAudioService:
                if (value is not uint audioSvr) break;
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(audioSvr, ptr, true);
                return G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
            case DemodulatorCode.G3XddcDemodulatorParamDrmMultimediaService:
                if (value is not uint timedia) break;
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(timedia, ptr, true);
                return G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
        }

        return false;
    }

    public object GetDemodulator(DemodulatorCode demodulatorType)
    {
        IntPtr ptr;
        int size;
        bool res;
        switch (demodulatorType)
        {
            case DemodulatorCode.G3XddcDemodulatorParamAmsSideBand:
            case DemodulatorCode.G3XddcDemodulatorParamDsbSideBand:
            case DemodulatorCode.G3XddcDemodulatorParamIsbSideBand:
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                res = G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
                if (!res) return null;
                var band = (SideBandType)Marshal.PtrToStructure<uint>(ptr);
                return band;
            case DemodulatorCode.G3XddcDemodulatorParamAmsCaptureRange:
                size = Marshal.SizeOf(typeof(G3XddcAmsCaptureRange));
                ptr = Marshal.AllocHGlobal(size);
                res = G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
                if (!res) return null;
                var range = Marshal.PtrToStructure<G3XddcAmsCaptureRange>(ptr);
                return range;
            case DemodulatorCode.G3XddcDemodulatorParamCwFrequency:
                size = Marshal.SizeOf(typeof(int));
                ptr = Marshal.AllocHGlobal(size);
                res = G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
                if (!res) return null;
                var freq = Marshal.PtrToStructure<int>(ptr);
                return freq;
            case DemodulatorCode.G3XddcDemodulatorParamDrmAudioService:
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                res = G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
                if (!res) return null;
                var svr = Marshal.PtrToStructure<uint>(ptr);
                return svr;
            case DemodulatorCode.G3XddcDemodulatorParamDrmMultimediaService:
                size = Marshal.SizeOf(typeof(uint));
                ptr = Marshal.AllocHGlobal(size);
                res = G33Ddcsdk.SetDemodulatorParam(_channelNumber, (uint)demodulatorType, ptr, (uint)size);
                if (!res) return null;
                var svr1 = Marshal.PtrToStructure<uint>(ptr);
                return svr1;
        }

        return null;
    }

    public static byte[] SetAudioData(IntPtr intPtr, uint numberOfSamples)
    {
        var audio = new float[numberOfSamples];
        Marshal.Copy(intPtr, audio, 0, (int)numberOfSamples);
        var shortData = new short[numberOfSamples];
        for (var i = 0; i < numberOfSamples; i++) shortData[i] = (short)(audio[i] * 32767);
        var data = new byte[shortData.Length * 2];
        Buffer.BlockCopy(shortData, 0, data, 0, data.Length);
        return data;
        // 下面的为时域转频域，不需要
        //if (!Helper.AddSamples_Real_Mono_F32(intPtr, ref _audioBuffer, ref _audioOffset, numberOfSamples, _audioFFTSize))
        //{
        //    return null;
        //}
        //var buffer = new float[_audioFFTSize * 2];
        ////convet to mode and apply normalization and window coeffs
        //for (int i = 0; i < _audioFFTSize; i++)
        //{
        //    buffer[i * 2] = _audioBuffer[i] * _audioCoe[i];
        //    buffer[(i * 2) + 1] = 0; //imaginary component is zero, audio signal does not have it
        //}
        //Helper.FFT(ref buffer, _audioFFTSize);
        ////frequency spectrum after this FFT (with real signal at the input) looks like the following
        //// |                                     |                                     |
        //// |                                     |             inverted part           |
        //// |-------------------------------------|-------------------------------------|
        //// 0 Hz                         audio_sample_rate/2                        ~0 Hz
        //// The first FFT bin               Middle FFT bin               The last FFT bin 
        ////the following converts amplitudes to dB on lower half of the FFT result,
        ////inverted part is not used
        //var half = _audioFFTSize / 2;
        //for (int i = 0; i < half; i++)
        //{
        //    var m = (buffer[i * 2] * buffer[i * 2]) + (buffer[(i * 2) + 1] * buffer[(i * 2) + 1]);
        //    if (m > 0)
        //    {
        //        buffer[i] = 10 * (float)Math.Log10(m);
        //    }
        //    else
        //    {
        //        buffer[i] = -180f;
        //    }
        //}
        //var data = new float[half];
        //Buffer.BlockCopy(buffer, 0, data, 0, (int)half * sizeof(float));
        //return data;
    }
}