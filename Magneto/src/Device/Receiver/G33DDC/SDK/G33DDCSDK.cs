using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.G33DDC.SDK;

public static class G33Ddcsdk
{
    public delegate void G33DdcAudioPlaybackStreamCallback(uint channel, IntPtr intPtr, uint numberOfSamples,
        IntPtr userData);

    public delegate void G33DdcAudioStreamCallback(uint channel, IntPtr intPtr, IntPtr bufferFiltered,
        uint numberOfSamples, IntPtr userData);

    public delegate void G33DdcDdc1PlaybackStreamCallback(short[] buffer, uint numberOfSamples, uint bitsPerSample,
        IntPtr userData);

    public delegate void G33DdcDdc1StreamCallback(IntPtr intPtr, uint numberOfSamples, uint bitsPerSample,
        IntPtr userData);

    public delegate void G33DdcDdc2PreprocessedStreamCallback(uint channel, IntPtr intPtr, uint numberOfSamples,
        float slevelPeak, float slevelRms, IntPtr userData);

    public delegate void
        G33DdcDdc2StreamCallback(uint channel, IntPtr intPtr, uint numberOfSamples, IntPtr userData);

    public delegate void G33DdcIfCallback(IntPtr intPtr, uint numberOfSamples, short maxAdcAmplitude,
        uint adcSamplingRate, IntPtr userData);

    private static int _deviceId = -1;

    public static G33DdcDeviceInfo[] GetDeviceList(uint bufferSize)
    {
        var list = new G33DdcDeviceInfo[1];
        var count = GetDeviceList(list, bufferSize);
        if (count > 0) return list;
        return null;
    }

    public static bool Open(string serialNumber)
    {
        var id = OpenDevice(serialNumber);
        if (id >= 0)
        {
            _deviceId = id;
            return true;
        }

        GetLastError();
        return false;
    }

    public static bool Close()
    {
        if (_deviceId < 0) return true;
        if (CloseDevice(_deviceId))
        {
            _deviceId = -1;
            return true;
        }

        return false;
    }

    public static bool IsDeviceConnected()
    {
        return IsDeviceConnected(_deviceId);
    }

    public static G33DdcDeviceInfo[] GetDeviceInfo(uint bufferLength)
    {
        var list = new G33DdcDeviceInfo[1];
        if (!GetDeviceInfo(_deviceId, list, bufferLength)) return null;
        return list;
    }

    public static bool SetLed(uint ledMode)
    {
        return SetLED(_deviceId, ledMode);
    }

    public static bool GetLed(out uint ledMode)
    {
        return GetLED(_deviceId, out ledMode);
    }

    public static bool SetPower(bool power)
    {
        return SetPower(_deviceId, power);
    }

    public static bool GetPower(out bool power)
    {
        return GetPower(_deviceId, out power);
    }

    /// <summary>
    ///     设置衰减
    ///     Value that specifies attenuation level in dB.
    ///     Possible values are: 0, 3, 6, 9, 12, 15, 18, 21.
    ///     If the value is not from the list,
    ///     the SetAttenuator function rounds the value to nearest lower one.
    /// </summary>
    /// <param name="attenuator">0, 3, 6, 9, 12, 15, 18, 21</param>
    public static bool SetAttenuator(uint attenuator)
    {
        return SetAttenuator(_deviceId, attenuator);
    }

    public static bool GetAttenuator(out uint attenuator)
    {
        return GetAttenuator(_deviceId, out attenuator);
    }

    /// <summary>
    ///     控制输入端带通滤波器。
    ///     Low
    ///     [in] Specifies cut-off low frequency of the filter in Hz.Possible values are:
    ///     0, 700000, 1200000, 2000000, 2900000, 4000000, 5100000, 5600000, 8300000, 9200000, 9500000, 13300000, 14600000,
    ///     16100000, 21600000.
    ///     If the value is not from the list, the function rounds it to nearest one.
    ///     High
    ///     [in] Specifies cut-off high frequency of the filter in Hz.Possible values are:
    ///     2200000, 2700000, 4000000, 4900000, 6200000, 7400000, 9400000, 10700000, 12900000, 14600000, 18900000, 23400000,
    ///     25800000, 32100000, 50000000.
    ///     If the value is not from the list, the function rounds it to nearest one.
    /// </summary>
    /// <param name="low"></param>
    /// <param name="high"></param>
    public static bool SetPreselectors(uint low, uint high)
    {
        return SetPreselectors(_deviceId, low, high);
    }

    public static bool GetPreselectors(out uint low, out uint high)
    {
        return GetPreselectors(_deviceId, out low, out high);
    }

    /// <summary>
    ///     启用或禁用前置放大器
    /// </summary>
    /// <param name="preamp"></param>
    public static bool SetPreamp(bool preamp)
    {
        return SetPreamp(_deviceId, preamp);
    }

    public static bool GetPreamp(out bool preamp)
    {
        return GetPreamp(_deviceId, out preamp);
    }

    /// <summary>
    ///     启用或禁用ADC抖动。
    ///     打开
    /// </summary>
    /// <param name="enabled"></param>
    public static bool SetDithering(bool enabled)
    {
        return SetDithering(_deviceId, enabled);
    }

    public static bool GetDithering(out bool enabled)
    {
        return GetDithering(_deviceId, out enabled);
    }

    /// <summary>
    ///     在ADC流上启用或禁用噪声屏蔽。
    /// </summary>
    /// <param name="enabled"></param>
    public static bool SetAdcNoiseBlanker(bool enabled)
    {
        return SetADCNoiseBlanker(_deviceId, enabled);
    }

    public static bool GetAdcNoiseBlanker(out bool enabled)
    {
        return GetADCNoiseBlanker(_deviceId, out enabled);
    }

    /// <summary>
    ///     ADC噪声抑制阈值。
    ///     可接受的最大输入信号。
    ///     阈值的最大可能值是32767，
    ///     在这种情况下，即使使用SetADCNoiseBlanker函数使能了噪声屏蔽，它也没有影响。
    /// </summary>
    /// <param name="threshold"></param>
    public static bool SetAdcNoiseBlankerThreshold(short threshold)
    {
        return SetADCNoiseBlankerThreshold(_deviceId, threshold);
    }

    public static bool GetAdcNoiseBlankerThreshold(out short threshold)
    {
        return GetADCNoiseBlankerThreshold(_deviceId, out threshold);
    }

    /// <summary>
    ///     开始发送IF快照。
    /// </summary>
    /// <param name="period">以毫秒为单位的时间间隔将IF快照发送给IFCallback回调函数。</param>
    public static bool StartIf(short period)
    {
        return StartIF(_deviceId, period);
    }

    /// <summary>
    ///     停止发送IF快照。
    /// </summary>
    public static bool StopIf()
    {
        return StopIF(_deviceId);
    }

    /// <summary>
    ///     启用或禁用频谱反转。
    /// </summary>
    /// <param name="inverted"></param>
    public static bool SetInverted(bool inverted)
    {
        return SetInverted(_deviceId, inverted);
    }

    public static bool GetInverted(out bool inverted)
    {
        return GetInverted(_deviceId, out inverted);
    }

    public static bool GetDdcInfo(uint ddcTypeIndex, out G3XddcDdcInfo info)
    {
        return GetDDCInfo(_deviceId, ddcTypeIndex, out info);
    }

    public static bool GetDdc1Count(out uint count)
    {
        return GetDDC1Count(_deviceId, out count);
    }

    /// <summary>
    ///     设置当前DDC1的DDC类型。
    ///     使用GetDDC1Count函数确定DDC1可能的DDC类型的数量。
    ///     参数ddcTypeIndex为从0到比GetDDC1Count获取的值小1。
    /// </summary>
    /// <param name="ddcTypeIndex">指定在DDC1中使用的DDC类型的索引。</param>
    public static bool SetDdc1(uint ddcTypeIndex)
    {
        return SetDDC1(_deviceId, ddcTypeIndex);
    }

    public static bool GetDdc1(out uint ddcTypeIndex, out G3XddcDdcInfo ddcInfo)
    {
        return GetDDC1(_deviceId, out ddcTypeIndex, out ddcInfo);
    }

    /// <summary>
    ///     设置DDC1的中心频率
    /// </summary>
    /// <param name="frequency"></param>
    public static bool SetDdc1Frequency(uint frequency)
    {
        return SetDDC1Frequency(_deviceId, frequency);
    }

    public static bool GetDdc1Frequency(out uint frequency)
    {
        return GetDDC1Frequency(_deviceId, out frequency);
    }

    /// <summary>
    ///     启动DDC1
    ///     每个缓冲区中传递给DDC1StreamCallback回调函数的I/Q样本集的数量。
    ///     该值必须是大于0的64的倍数。
    ///     如果该值为0，则StartDDC1函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </summary>
    /// <param name="samplesPerBuffer"></param>
    public static bool StartDdc1(uint samplesPerBuffer)
    {
        return StartDDC1(_deviceId, samplesPerBuffer);
    }

    public static bool StopDdc1()
    {
        return StopDDC1(_deviceId);
    }

    public static bool StartDdc1Playback(uint samplesPerBuffer, uint bitsPerSample)
    {
        return StartDDC1Playback(_deviceId, samplesPerBuffer, bitsPerSample);
    }

    public static bool PauseDdc1Playback()
    {
        return PauseDDC1Playback(_deviceId);
    }

    public static bool ResumeDdc1Playback()
    {
        return ResumeDDC1Playback(_deviceId);
    }

    public static bool GetDdc2(out uint ddcTypeIndex, out G3XddcDdcInfo ddcInfo)
    {
        return GetDDC2(_deviceId, out ddcTypeIndex, out ddcInfo);
    }

    /// <summary>
    ///     设置DDC2的中心频率
    ///     faDDC2[i] = fDDC1 + frDDC2[i]
    /// </summary>
    /// <param name="channel">DDC2的通道号 0,1,2</param>
    /// <param name="frequency">DDC2的相对中心频率Hz</param>
    public static bool SetDdc2Frequency(uint channel, int frequency)
    {
        return SetDDC2Frequency(_deviceId, channel, frequency);
    }

    public static bool GetDdc2Frequency(uint channel, out int frequency)
    {
        return GetDDC2Frequency(_deviceId, channel, out frequency);
    }

    /// <summary>
    ///     启动DDC2
    ///     samplesPerBuffer为每个缓冲区中传递给DDC2StreamCallback回调函数的I/Q样本集的数量。
    ///     该值必须是大于0的64的倍数。
    ///     如果该值为0，则StartDDC2函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="samplesPerBuffer"></param>
    public static bool StartDdc2(uint channel, uint samplesPerBuffer)
    {
        return StartDDC2(_deviceId, channel, samplesPerBuffer);
    }

    public static bool StopDdc2(uint channel)
    {
        return StopDDC2(_deviceId, channel);
    }

    /// <summary>
    ///     启用或禁用DDC2流上的噪声屏蔽。
    ///     静噪开关
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    public static bool SetDdc2NoiseBlanker(uint channel, bool enabled)
    {
        return SetDDC2NoiseBlanker(_deviceId, channel, enabled);
    }

    public static bool GetDdc2NoiseBlanker(uint channel, out bool enabled)
    {
        return GetDDC2NoiseBlanker(_deviceId, channel, out enabled);
    }

    /// <summary>
    ///     DDC2噪声抑制阈值。
    ///     静噪门限
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="threshold"></param>
    public static bool SetDdc2NoiseBlankerThreshold(uint channel, double threshold)
    {
        return SetDDC2NoiseBlankerThreshold(_deviceId, channel, threshold);
    }

    public static bool GetDdc2NoiseBlankerThreshold(uint channel, out double threshold)
    {
        return GetDDC2NoiseBlankerThreshold(_deviceId, channel, out threshold);
    }

    /// <summary>
    ///     表示短时平均信号电平与最大信号电平的百分比比值。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="value"></param>
    public static bool GetDdc2NoiseBlankerExcessValue(uint channel, out double value)
    {
        return GetDDC2NoiseBlankerExcessValue(_deviceId, channel, out value);
    }

    /// <summary>
    ///     确定给定信道的当前信号电平。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="peak">指向接收电流信号电平(峰值)的变量的指针，单位为伏特。 如果应用程序不需要此信息，此参数可以为NULL。</param>
    /// <param name="rms">指针指向一个变量，该变量以伏特为单位接收电流信号电平(RMS)。 如果应用程序不需要此信息，此参数可以为NULL。  </param>
    public static bool GetSignalLevel(uint channel, out float peak, out float rms)
    {
        return GetSignalLevel(_deviceId, channel, out peak, out rms);
    }

    /// <summary>
    ///     为给定通道启用或禁用陷波滤波器。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="notchFilterIndex"></param>
    /// <param name="enabled"></param>
    public static bool SetNotchFilter(uint channel, uint notchFilterIndex, bool enabled)
    {
        return SetNotchFilter(_deviceId, channel, notchFilterIndex, enabled);
    }

    public static bool GetNotchFilter(uint channel, uint notchFilterIndex, out bool enabled)
    {
        return GetNotchFilter(_deviceId, channel, notchFilterIndex, out enabled);
    }

    public static bool SetNotchFilterFrequency(uint channel, uint notchFilterIndex, int frequency)
    {
        return SetNotchFilterFrequency(_deviceId, channel, notchFilterIndex, frequency);
    }

    public static bool GetNotchFilterFrequency(uint channel, uint notchFilterIndex, out int frequency)
    {
        return GetNotchFilterFrequency(_deviceId, channel, notchFilterIndex, out frequency);
    }

    public static bool SetNotchFilterBandwidth(uint channel, uint notchFilterIndex, uint bandwidth)
    {
        return SetNotchFilterBandwidth(_deviceId, channel, notchFilterIndex, bandwidth);
    }

    public static bool GetNotchFilterBandwidth(uint channel, uint notchFilterIndex, out uint bandwidth)
    {
        return GetNotchFilterBandwidth(_deviceId, channel, notchFilterIndex, out bandwidth);
    }

    public static bool SetNotchFilterLength(uint channel, uint notchFilterIndex, uint length)
    {
        return SetNotchFilterLength(_deviceId, channel, notchFilterIndex, length);
    }

    public static bool GetNotchFilterLength(uint channel, uint notchFilterIndex, out uint length)
    {
        return GetNotchFilterLength(_deviceId, channel, notchFilterIndex, out length);
    }

    /// <summary>
    ///     为给定的通道启用或禁用AGC。
    ///     启用
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    public static bool SetAgc(uint channel, bool enabled)
    {
        return SetAGC(_deviceId, channel, enabled);
    }

    public static bool GetAgc(uint channel, bool enabled)
    {
        return GetAGC(_deviceId, channel, enabled);
    }

    /// <summary>
    ///     设置给定通道的AGC参数。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="attackTime">以秒为单位设置新的AGC攻击时间。  </param>
    /// <param name="decayTime">新的AGC衰减时间，单位为秒。</param>
    /// <param name="referenceLevel">指定AGC的新引用电平 dB</param>
    public static bool SetAgcParams(uint channel, double attackTime, double decayTime, double referenceLevel)
    {
        return SetAGCParams(_deviceId, channel, attackTime, decayTime, referenceLevel);
    }

    public static bool GetAgcParams(uint channel, out double attackTime, out double decayTime,
        out double referenceLevel)
    {
        return GetAGCParams(_deviceId, channel, out attackTime, out decayTime, out referenceLevel);
    }

    /// <summary>
    ///     设置给定信道的AGC最大增益。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="maxGain"></param>
    public static bool SetMaxAgcGain(uint channel, double maxGain)
    {
        return SetMaxAGCGain(_deviceId, channel, maxGain);
    }

    public static bool GetMaxAgcGain(uint channel, out double maxGain)
    {
        return GetMaxAGCGain(_deviceId, channel, out maxGain);
    }

    /// <summary>
    ///     为给定的信道设置固定增益。 如果AGC被禁用，则该增益应用于I/Q信号，否则不使用。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="gain"></param>
    public static bool SetGain(uint channel, double gain)
    {
        return SetGain(_deviceId, channel, gain);
    }

    public static bool GetGain(uint channel, out double gain)
    {
        return GetGain(_deviceId, channel, out gain);
    }

    /// <summary>
    ///     获取应用于I/Q信号的电流增益。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="currentGain"></param>
    public static bool GetCurrentGain(uint channel, out double currentGain)
    {
        return GetCurrentGain(_deviceId, channel, out currentGain);
    }

    /// <summary>
    ///     为给定的信道设置解调器滤波器的带宽。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="bandwidth"></param>
    public static bool SetDemodulatorFilterBandwidth(uint channel, uint bandwidth)
    {
        return SetDemodulatorFilterBandwidth(_deviceId, channel, bandwidth);
    }

    public static bool GetDemodulatorFilterBandwidth(uint channel, out uint bandwidth)
    {
        return GetDemodulatorFilterBandwidth(_deviceId, channel, out bandwidth);
    }

    /// <summary>
    ///     为给定信道设置解调器滤波器移位。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="shift"></param>
    public static bool SetDemodulatorFilterShift(uint channel, int shift)
    {
        return SetDemodulatorFilterShift(_deviceId, channel, shift);
    }

    public static bool GetDemodulatorFilterShift(uint channel, out int shift)
    {
        return GetDemodulatorFilterShift(_deviceId, channel, out shift);
    }

    /// <summary>
    ///     指定给定通道的解调滤波器长度。
    ///     解调器滤波器采用FIR滤波器实现。
    ///     这个函数指定了过滤过程中使用的系数的数量。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="length"></param>
    public static bool SetDemodulatorFilterLength(uint channel, uint length)
    {
        return SetDemodulatorFilterLength(_deviceId, channel, length);
    }

    public static bool GetDemodulatorFilterLength(uint channel, out uint length)
    {
        return GetDemodulatorFilterLength(_deviceId, channel, out length);
    }

    /// <summary>
    ///     为给定的信道设置解调器模式。
    ///     Value               Meaning
    ///     G3XDDC_MODE_CW      Continuous wave
    ///     G3XDDC_MODE_AM      Amplitude modulation
    ///     G3XDDC_MODE_FM      Frequency modulation
    ///     G3XDDC_MODE_LSB     Lower sideband modulation
    ///     G3XDDC_MODE_USB     Upper sideband modulation
    ///     G3XDDC_MODE_AMS     Amplitude modulation
    ///     G3XDDC_MODE_DSB     Double sideband modulation
    ///     G3XDDC_MODE_ISB     Independent sideband modulation
    ///     G3XDDC_MODE_DRM     Digital Radio Mondiale
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="mode"></param>
    public static bool SetDemodulatorMode(uint channel, uint mode)
    {
        return SetDemodulatorMode(_deviceId, channel, mode);
    }

    public static bool GetDemodulatorMode(uint channel, out uint mode)
    {
        return GetDemodulatorMode(_deviceId, channel, out mode);
    }

    /// <summary>
    ///     设置解调带宽（相对值）
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="frequency"></param>
    public static bool SetDemodulatorFrequency(uint channel, int frequency)
    {
        return SetDemodulatorFrequency(_deviceId, channel, frequency);
    }

    public static bool GetDemodulatorFrequency(uint channel, out int frequency)
    {
        return GetDemodulatorFrequency(_deviceId, channel, out frequency);
    }

    public static bool SetDemodulatorParam(uint channel, uint code, IntPtr buffer, uint bufferSize)
    {
        return SetDemodulatorParam(_deviceId, channel, code, buffer, bufferSize);
    }

    public static bool GetDemodulatorParam(uint channel, uint code, [Out] IntPtr buffer, uint bufferSize)
    {
        return GetDemodulatorParam(_deviceId, channel, code, buffer, bufferSize);
    }

    public static bool GetDemodulatorState(uint channel, uint code, [Out] IntPtr buffer, uint bufferSize)
    {
        return GetDemodulatorState(_deviceId, channel, code, buffer, bufferSize);
    }

    /// <summary>
    ///     启动音频输出
    ///     AudioStreamCallback
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="samplePerBuffer">
    ///     指定传递给AudioStreamCallback回调函数的每个缓冲区中的样本数量。
    ///     该值必须是大于0的64的倍数。 如果为零，StartAudio函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数
    /// </param>
    public static bool StartAudio(uint channel, uint samplePerBuffer)
    {
        return StartAudio(_deviceId, channel, samplePerBuffer);
    }

    public static bool StopAudio(uint channel)
    {
        return StopAudio(_deviceId, channel);
    }

    public static bool StartAudioPlayback(uint channel, uint samplePerBuffer)
    {
        return StartAudioPlayback(_deviceId, channel, samplePerBuffer);
    }

    public static bool PauseAudioPlayback(uint channel)
    {
        return PauseAudioPlayback(_deviceId, channel);
    }

    public static bool ResumeAudioPlayback(uint channel)
    {
        return ResumeAudioPlayback(_deviceId, channel);
    }

    /// <summary>
    ///     为给定的频道设置固定的音频增益。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="gain">以dB为单位指定新的固定音频增益。</param>
    public static bool SetAudioGain(uint channel, double gain)
    {
        return SetAudioGain(_deviceId, channel, gain);
    }

    public static bool GetAudioGain(uint channel, out double gain)
    {
        return GetAudioGain(_deviceId, channel, out gain);
    }

    /// <summary>
    ///     启用或禁用给定通道的音频过滤器。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    public static bool SetAudioFilter(uint channel, bool enabled)
    {
        return SetAudioFilter(_deviceId, channel, enabled);
    }

    public static bool GetAudioFilter(uint channel, out bool enabled)
    {
        return GetAudioFilter(_deviceId, channel, out enabled);
    }

    /// <summary>
    ///     为给定通道设置音频过滤器的参数。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="cutOffLow">
    ///     滤波器的截止低频，单位是Hz。
    ///     这是滤波器通带的起始频率，可以从0到23999 Hz。
    ///     该值必须小于CutOffHigh参数指定的截止高频。
    /// </param>
    /// <param name="cutOffHigh">
    ///     滤波器的截止频率，以Hz为单位。
    ///     这是滤波器通带的结束频率，它可以在1 - 24000 Hz范围内。
    ///     该值必须大于CutOffLow参数指定的截止低频。
    /// </param>
    /// <param name="deemphasis">
    ///     以每个八度音阶的dB为单位指定取消强调过滤器。
    ///     去强调从滤波器的截止低频开始。
    ///     取值范围为-9.9 ~ 0.0 dB/octave。
    ///     零表示不强调。
    /// </param>
    public static bool SetAudioFilterParams(uint channel, uint cutOffLow, uint cutOffHigh, double deemphasis)
    {
        return SetAudioFilterParams(_deviceId, channel, cutOffLow, cutOffHigh, deemphasis);
    }

    public static bool GetAudioFilterParams(uint channel, out uint cutOffLow, out uint cutOffHigh,
        out double deemphasis)
    {
        return GetAudioFilterParams(_deviceId, channel, out cutOffLow, out cutOffHigh, out deemphasis);
    }

    /// <summary>
    ///     指定给定通道的音频过滤器长度。
    ///     音频滤波器采用FIR滤波器实现。
    ///     此函数指定过滤过程中使用的系数数目。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="length">
    ///     指定音频过滤器的长度。
    ///     必须为64的整数倍，且大于等于64且小于等于32768。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </param>
    public static bool SetAudioFilterLength(uint channel, uint length)
    {
        return SetAudioFilterLength(_deviceId, channel, length);
    }

    public static bool GetAudioFilterLength(uint channel, out uint length)
    {
        return GetAudioFilterLength(_deviceId, channel, out length);
    }

    /// <summary>
    ///     设置给定信道的解调器的绝对频率。
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="frequency"></param>
    public static bool SetFrequency(uint channel, uint frequency)
    {
        return SetFrequency(_deviceId, channel, frequency);
    }

    public static bool GetFrequency(uint channel, out uint frequency)
    {
        return GetFrequency(_deviceId, channel, out frequency);
    }

    /// <summary>
    ///     确定从DDC1或DDC2信号计算出的频谱补偿数据。
    ///     它用于将相对振幅dB转换为绝对振幅dBm。
    ///     将电平转为场强
    /// </summary>
    /// <param name="centerFrequency"></param>
    /// <param name="width"></param>
    /// <param name="buffer"></param>
    /// <param name="count"></param>
    public static bool GetSpectrumCompensation(ulong centerFrequency, uint width, [Out] float[] buffer, uint count)
    {
        return GetSpectrumCompensation(_deviceId, centerFrequency, width, buffer, count);
    }

    public static bool SetCallbacks(ref G33DdcCallbacks callbacks, IntPtr userData)
    {
        return SetCallbacks(_deviceId, ref callbacks, userData);
    }

    public static int GetErrorCode()
    {
        return GetLastError();
    }

    public struct G33DdcCallbacks
    {
        public G33DdcIfCallback IfCallback;
        public G33DdcDdc1StreamCallback Ddc1StreamCallback;
        public G33DdcDdc1PlaybackStreamCallback Ddc1PlaybackStreamCallback;
        public G33DdcDdc2StreamCallback Ddc2StreamCallback;
        public G33DdcDdc2PreprocessedStreamCallback Ddc2PreprocessedStreamCallback;
        public G33DdcAudioStreamCallback AudioStreamCallback;
        public G33DdcAudioPlaybackStreamCallback AudioPlaybackStreamCallback;
    }

    #region P/Invoke

    private const string ApiLibName = "library\\G33DDC\\x86\\G33DDCAPI.dll";

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDeviceList")]
    private static extern int GetDeviceList([Out] G33DdcDeviceInfo[] deviceList, uint bufferSize);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "OpenDevice")]
    private static extern int OpenDevice(string serialNumber);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "CloseDevice")]
    private static extern bool CloseDevice(int hDevice);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "IsDeviceConnected")]
    private static extern bool IsDeviceConnected(int hDevice);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDeviceInfo")]
    private static extern bool GetDeviceInfo(int hDevice, [Out] G33DdcDeviceInfo[] info, uint bufferLength);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetLED")]
    private static extern bool SetLED(int hDevice, uint ledMode);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetLED")]
    private static extern bool GetLED(int hDevice, out uint ledMode);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetPower")]
    private static extern bool SetPower(int hDevice, bool power);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPower")]
    private static extern bool GetPower(int hDevice, out bool power);

    /// <summary>
    ///     设置衰减
    ///     Value that specifies attenuation level in dB.
    ///     Possible values are: 0, 3, 6, 9, 12, 15, 18, 21.
    ///     If the value is not from the list,
    ///     the SetAttenuator function rounds the value to nearest lower one.
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="attenuator">0, 3, 6, 9, 12, 15, 18, 21</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetAttenuator")]
    private static extern bool SetAttenuator(int hDevice, uint attenuator);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetAttenuator")]
    private static extern bool GetAttenuator(int hDevice, out uint attenuator);

    /// <summary>
    ///     控制输入端带通滤波器。
    ///     Low
    ///     [in] Specifies cut-off low frequency of the filter in Hz.Possible values are:
    ///     0, 700000, 1200000, 2000000, 2900000, 4000000, 5100000, 5600000, 8300000, 9200000, 9500000, 13300000, 14600000,
    ///     16100000, 21600000.
    ///     If the value is not from the list, the function rounds it to nearest one.
    ///     High
    ///     [in] Specifies cut-off high frequency of the filter in Hz.Possible values are:
    ///     2200000, 2700000, 4000000, 4900000, 6200000, 7400000, 9400000, 10700000, 12900000, 14600000, 18900000, 23400000,
    ///     25800000, 32100000, 50000000.
    ///     If the value is not from the list, the function rounds it to nearest one.
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="low"></param>
    /// <param name="high"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetPreselectors")]
    private static extern bool SetPreselectors(int hDevice, uint low, uint high);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPreselectors")]
    private static extern bool GetPreselectors(int hDevice, out uint low, out uint high);

    /// <summary>
    ///     启用或禁用前置放大器
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="preamp"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetPreamp")]
    private static extern bool SetPreamp(int hDevice, bool preamp);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetPreamp")]
    private static extern bool GetPreamp(int hDevice, out bool preamp);

    /// <summary>
    ///     启用或禁用ADC抖动。
    ///     打开
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetDithering")]
    private static extern bool SetDithering(int hDevice, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetDithering")]
    private static extern bool GetDithering(int hDevice, out bool enabled);

    /// <summary>
    ///     在ADC流上启用或禁用噪声屏蔽。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetADCNoiseBlanker")]
    private static extern bool SetADCNoiseBlanker(int hDevice, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetADCNoiseBlanker")]
    private static extern bool GetADCNoiseBlanker(int hDevice, out bool enabled);

    /// <summary>
    ///     ADC噪声抑制阈值。
    ///     可接受的最大输入信号。
    ///     阈值的最大可能值是32767，
    ///     在这种情况下，即使使用SetADCNoiseBlanker函数使能了噪声屏蔽，它也没有影响。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="threshold"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetADCNoiseBlankerThreshold")]
    private static extern bool SetADCNoiseBlankerThreshold(int hDevice, short threshold);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetADCNoiseBlankerThreshold")]
    private static extern bool GetADCNoiseBlankerThreshold(int hDevice, out short threshold);

    /// <summary>
    ///     开始发送IF快照。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="period">以毫秒为单位的时间间隔将IF快照发送给IFCallback回调函数。</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "StartIF")]
    private static extern bool StartIF(int hDevice, short period);

    /// <summary>
    ///     停止发送IF快照。
    /// </summary>
    /// <param name="hDevice"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall, EntryPoint = "StopIF")]
    private static extern bool StopIF(int hDevice);

    /// <summary>
    ///     启用或禁用频谱反转。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="inverted"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetInverted(int hDevice, bool inverted);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetInverted(int hDevice, out bool inverted);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDCInfo(int hDevice, uint ddcTypeIndex, out G3XddcDdcInfo info);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC1Count(int hDevice, out uint count);

    /// <summary>
    ///     设置当前DDC1的DDC类型。
    ///     使用GetDDC1Count函数确定DDC1可能的DDC类型的数量。
    ///     参数ddcTypeIndex为从0到比GetDDC1Count获取的值小1。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="ddcTypeIndex">指定在DDC1中使用的DDC类型的索引。</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDDC1(int hDevice, uint ddcTypeIndex);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC1(int hDevice, out uint ddcTypeIndex, out G3XddcDdcInfo ddcInfo);

    /// <summary>
    ///     设置DDC1的中心频率
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="frequency"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDDC1Frequency(int hDevice, uint frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC1Frequency(int hDevice, out uint frequency);

    /// <summary>
    ///     启动DDC1
    ///     每个缓冲区中传递给DDC1StreamCallback回调函数的I/Q样本集的数量。
    ///     该值必须是大于0的64的倍数。
    ///     如果该值为0，则StartDDC1函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="samplesPerBuffer"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDDC1(int hDevice, uint samplesPerBuffer);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StopDDC1(int hDevice);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDDC1Playback(int hDevice, uint samplesPerBuffer, uint bitsPerSample);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool PauseDDC1Playback(int hDevice);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ResumeDDC1Playback(int hDevice);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC2(int hDevice, out uint ddcTypeIndex, out G3XddcDdcInfo ddcInfo);

    /// <summary>
    ///     设置DDC2的中心频率
    ///     faDDC2[i] = fDDC1 + frDDC2[i]
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel">DDC2的通道号 0,1,2</param>
    /// <param name="frequency">DDC2的相对中心频率Hz</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDDC2Frequency(int hDevice, uint channel, int frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC2Frequency(int hDevice, uint channel, out int frequency);

    /// <summary>
    ///     启动DDC2
    ///     samplesPerBuffer为每个缓冲区中传递给DDC2StreamCallback回调函数的I/Q样本集的数量。
    ///     该值必须是大于0的64的倍数。
    ///     如果该值为0，则StartDDC2函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="samplesPerBuffer"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartDDC2(int hDevice, uint channel, uint samplesPerBuffer);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StopDDC2(int hDevice, uint channel);

    /// <summary>
    ///     启用或禁用DDC2流上的噪声屏蔽。
    ///     静噪开关
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDDC2NoiseBlanker(int hDevice, uint channel, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC2NoiseBlanker(int hDevice, uint channel, out bool enabled);

    /// <summary>
    ///     DDC2噪声抑制阈值。
    ///     静噪门限
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="threshold"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDDC2NoiseBlankerThreshold(int hDevice, uint channel, double threshold);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC2NoiseBlankerThreshold(int hDevice, uint channel, out double threshold);

    /// <summary>
    ///     表示短时平均信号电平与最大信号电平的百分比比值。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="value"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDDC2NoiseBlankerExcessValue(int hDevice, uint channel, out double value);

    /// <summary>
    ///     确定给定信道的当前信号电平。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="peak">指向接收电流信号电平(峰值)的变量的指针，单位为伏特。 如果应用程序不需要此信息，此参数可以为NULL。</param>
    /// <param name="rms">指针指向一个变量，该变量以伏特为单位接收电流信号电平(RMS)。 如果应用程序不需要此信息，此参数可以为NULL。  </param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetSignalLevel(int hDevice, uint channel, out float peak, out float rms);

    /// <summary>
    ///     为给定通道启用或禁用陷波滤波器。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="notchFilterIndex"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetNotchFilter(int hDevice, uint channel, uint notchFilterIndex, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetNotchFilter(int hDevice, uint channel, uint notchFilterIndex, out bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetNotchFilterFrequency(int hDevice, uint channel, uint notchFilterIndex, int frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetNotchFilterFrequency(int hDevice, uint channel, uint notchFilterIndex,
        out int frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool
        SetNotchFilterBandwidth(int hDevice, uint channel, uint notchFilterIndex, uint bandwidth);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetNotchFilterBandwidth(int hDevice, uint channel, uint notchFilterIndex,
        out uint bandwidth);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetNotchFilterLength(int hDevice, uint channel, uint notchFilterIndex, uint length);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetNotchFilterLength(int hDevice, uint channel, uint notchFilterIndex, out uint length);

    /// <summary>
    ///     为给定的通道启用或禁用AGC。
    ///     启用
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAGC(int hDevice, uint channel, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAGC(int hDevice, uint channel, bool enabled);

    /// <summary>
    ///     设置给定通道的AGC参数。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="attackTime">以秒为单位设置新的AGC攻击时间。  </param>
    /// <param name="decayTime">新的AGC衰减时间，单位为秒。</param>
    /// <param name="referenceLevel">指定AGC的新引用电平 dB</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAGCParams(int hDevice, uint channel, double attackTime, double decayTime,
        double referenceLevel);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAGCParams(int hDevice, uint channel, out double attackTime, out double decayTime,
        out double referenceLevel);

    /// <summary>
    ///     设置给定信道的AGC最大增益。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="maxGain"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetMaxAGCGain(int hDevice, uint channel, double maxGain);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetMaxAGCGain(int hDevice, uint channel, out double maxGain);

    /// <summary>
    ///     为给定的信道设置固定增益。 如果AGC被禁用，则该增益应用于I/Q信号，否则不使用。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="gain"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetGain(int hDevice, uint channel, double gain);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetGain(int hDevice, uint channel, out double gain);

    /// <summary>
    ///     获取应用于I/Q信号的电流增益。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="currentGain"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetCurrentGain(int hDevice, uint channel, out double currentGain);

    /// <summary>
    ///     为给定的信道设置解调器滤波器的带宽。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="bandwidth"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDemodulatorFilterBandwidth(int hDevice, uint channel, uint bandwidth);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorFilterBandwidth(int hDevice, uint channel, out uint bandwidth);

    /// <summary>
    ///     为给定信道设置解调器滤波器移位。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="shift"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDemodulatorFilterShift(int hDevice, uint channel, int shift);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorFilterShift(int hDevice, uint channel, out int shift);

    /// <summary>
    ///     指定给定通道的解调滤波器长度。
    ///     解调器滤波器采用FIR滤波器实现。
    ///     这个函数指定了过滤过程中使用的系数的数量。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="length"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDemodulatorFilterLength(int hDevice, uint channel, uint length);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorFilterLength(int hDevice, uint channel, out uint length);

    /// <summary>
    ///     为给定的信道设置解调器模式。
    ///     Value               Meaning
    ///     G3XDDC_MODE_CW      Continuous wave
    ///     G3XDDC_MODE_AM      Amplitude modulation
    ///     G3XDDC_MODE_FM      Frequency modulation
    ///     G3XDDC_MODE_LSB     Lower sideband modulation
    ///     G3XDDC_MODE_USB     Upper sideband modulation
    ///     G3XDDC_MODE_AMS     Amplitude modulation
    ///     G3XDDC_MODE_DSB     Double sideband modulation
    ///     G3XDDC_MODE_ISB     Independent sideband modulation
    ///     G3XDDC_MODE_DRM     Digital Radio Mondiale
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="mode"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDemodulatorMode(int hDevice, uint channel, uint mode);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorMode(int hDevice, uint channel, out uint mode);

    /// <summary>
    ///     设置解调带宽（相对值）
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="frequency"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetDemodulatorFrequency(int hDevice, uint channel, int frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorFrequency(int hDevice, uint channel, out int frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool
        SetDemodulatorParam(int hDevice, uint channel, uint code, IntPtr buffer, uint bufferSize);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorParam(int hDevice, uint channel, uint code, [Out] IntPtr buffer,
        uint bufferSize);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetDemodulatorState(int hDevice, uint channel, uint code, [Out] IntPtr buffer,
        uint bufferSize);

    /// <summary>
    ///     启动音频输出
    ///     AudioStreamCallback
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="samplePerBuffer">
    ///     指定传递给AudioStreamCallback回调函数的每个缓冲区中的样本数量。
    ///     该值必须是大于0的64的倍数。 如果为零，StartAudio函数失败。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数
    /// </param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartAudio(int hDevice, uint channel, uint samplePerBuffer);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StopAudio(int hDevice, uint channel);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool StartAudioPlayback(int hDevice, uint channel, uint samplePerBuffer);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool PauseAudioPlayback(int hDevice, uint channel);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool ResumeAudioPlayback(int hDevice, uint channel);

    /// <summary>
    ///     为给定的频道设置固定的音频增益。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="gain">以dB为单位指定新的固定音频增益。</param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAudioGain(int hDevice, uint channel, double gain);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAudioGain(int hDevice, uint channel, out double gain);

    /// <summary>
    ///     启用或禁用给定通道的音频过滤器。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="enabled"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAudioFilter(int hDevice, uint channel, bool enabled);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAudioFilter(int hDevice, uint channel, out bool enabled);

    /// <summary>
    ///     为给定通道设置音频过滤器的参数。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="cutOffLow">
    ///     滤波器的截止低频，单位是Hz。
    ///     这是滤波器通带的起始频率，可以从0到23999 Hz。
    ///     该值必须小于CutOffHigh参数指定的截止高频。
    /// </param>
    /// <param name="cutOffHigh">
    ///     滤波器的截止频率，以Hz为单位。
    ///     这是滤波器通带的结束频率，它可以在1 - 24000 Hz范围内。
    ///     该值必须大于CutOffLow参数指定的截止低频。
    /// </param>
    /// <param name="deemphasis">
    ///     以每个八度音阶的dB为单位指定取消强调过滤器。
    ///     去强调从滤波器的截止低频开始。
    ///     取值范围为-9.9 ~ 0.0 dB/octave。
    ///     零表示不强调。
    /// </param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAudioFilterParams(int hDevice, uint channel, uint cutOffLow, uint cutOffHigh,
        double deemphasis);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAudioFilterParams(int hDevice, uint channel, out uint cutOffLow, out uint cutOffHigh,
        out double deemphasis);

    /// <summary>
    ///     指定给定通道的音频过滤器长度。
    ///     音频滤波器采用FIR滤波器实现。
    ///     此函数指定过滤过程中使用的系数数目。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="length">
    ///     指定音频过滤器的长度。
    ///     必须为64的整数倍，且大于等于64且小于等于32768。
    ///     如果它不是64的倍数，函数将它四舍五入到最接近64的倍数。
    /// </param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetAudioFilterLength(int hDevice, uint channel, uint length);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetAudioFilterLength(int hDevice, uint channel, out uint length);

    /// <summary>
    ///     设置给定信道的解调器的绝对频率。
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="channel"></param>
    /// <param name="frequency"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetFrequency(int hDevice, uint channel, uint frequency);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetFrequency(int hDevice, uint channel, out uint frequency);

    /// <summary>
    ///     确定从DDC1或DDC2信号计算出的频谱补偿数据。
    ///     它用于将相对振幅dB转换为绝对振幅dBm。
    ///     将电平转为场强
    /// </summary>
    /// <param name="hDevice"></param>
    /// <param name="centerFrequency"></param>
    /// <param name="width"></param>
    /// <param name="buffer"></param>
    /// <param name="count"></param>
    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool GetSpectrumCompensation(int hDevice, ulong centerFrequency, uint width,
        [Out] float[] buffer, uint count);

    [DllImport(ApiLibName, CallingConvention = CallingConvention.StdCall)]
    private static extern bool SetCallbacks(int hDevice, ref G33DdcCallbacks callbacks, IntPtr userData);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall)]
    private static extern int GetLastError();

    #endregion
}