using System;
using System.Collections.Generic;
using System.Linq;

namespace Magneto.Device.AV3900A;

public partial class Av3900A
{
    /// <summary>
    ///     根据原始数据生成扫描数据
    /// </summary>
    /// <param name="rawData">原始数据</param>
    /// <param name="startFrequencyHz">起始频率，单位为Hz</param>
    /// <param name="stopFrequencyHz">终止频率，单位为Hz</param>
    /// <param name="stepFrequencyHz">步进，单位为Hz</param>
    /// <param name="realStartFrequencyHz">原始数据的起始频率，单位为Hz</param>
    /// <param name="realStepFrequencyHz">原始数据的步进，单位为Hz</param>
    /// <param name="offset">偏移量</param>
    /// <returns>扫描数据</returns>
    private float[] GetScanData(float[] rawData, double startFrequencyHz, double stopFrequencyHz,
        double stepFrequencyHz,
        double realStartFrequencyHz, double realStepFrequencyHz, out int offset)
    {
        var list = new List<float>();
        var currentIndex = 0;
        var currentFrequency = realStartFrequencyHz;
        offset = 0;
        if (rawData?.Any() != true) return null;
        var success = GetCurrentScanFrequency(startFrequencyHz, stopFrequencyHz, stepFrequencyHz, realStartFrequencyHz,
            out var currentScanFrequency, out offset);
        if (!success) return null;
        do
        {
            if (currentScanFrequency > stopFrequencyHz)
            {
                var num = (int)Math.Round((currentScanFrequency - stopFrequencyHz) / stepFrequencyHz);
                if (num > 0)
                    currentScanFrequency = stopFrequencyHz;
                else
                    break;
            }

            var nextFrequency = currentFrequency + realStepFrequencyHz;
            if (nextFrequency > currentScanFrequency && currentFrequency <= currentScanFrequency)
            {
                if (Math.Abs(currentFrequency - currentScanFrequency) < 1e-9)
                {
                    list.Add(rawData[currentIndex]);
                }
                else
                {
                    if (currentIndex + 1 >= rawData.Length) break;
                    var k = (rawData[currentIndex + 1] - rawData[currentIndex]) / (nextFrequency - currentFrequency);
                    var level = rawData[currentIndex] + (float)(k * (currentScanFrequency - currentFrequency));
                    list.Add(level);
                }

                currentScanFrequency += stepFrequencyHz;
            }

            currentIndex++;
            currentFrequency = nextFrequency;
        } while (currentIndex < rawData.Length && currentFrequency <= stopFrequencyHz);

        return list.ToArray();
    }

    /// <summary>
    ///     获取当前扫描频率
    /// </summary>
    /// <param name="startFrequencyHz">起始频率，单位为Hz</param>
    /// <param name="stopFrequencyHz">终止频率，单位为Hz</param>
    /// <param name="stepFrequencyHz">步进，单位为Hz</param>
    /// <param name="realStartFrequencyHz">原始数据的起始频率，单位为Hz</param>
    /// <param name="currentScanFrequency">原始数据的步进，单位为Hz</param>
    /// <param name="offset">偏移量</param>
    /// <returns>当前扫描频率</returns>
    private bool GetCurrentScanFrequency(double startFrequencyHz, double stopFrequencyHz, double stepFrequencyHz,
        double realStartFrequencyHz, out double currentScanFrequency, out int offset)
    {
        offset = 0;
        currentScanFrequency = 0;
        if (realStartFrequencyHz > stopFrequencyHz) return false;
        if (realStartFrequencyHz <= startFrequencyHz)
        {
            currentScanFrequency = startFrequencyHz;
        }
        else
        {
            offset = (int)Math.Ceiling((realStartFrequencyHz - startFrequencyHz) / stepFrequencyHz);
            currentScanFrequency = startFrequencyHz + stepFrequencyHz * offset;
        }

        return true;
    }

    /// <summary>
    ///     IQ数据转频谱数据
    /// </summary>
    /// <param name="iqDatas">IQ数据</param>
    /// <param name="bandwidth">带宽</param>
    /// <param name="sampleRate">采样率</param>
    /// <param name="attenuation">衰减</param>
    /// <returns>频谱数据</returns>
    private short[] ToSpectrumByIq(short[] iqDatas, double bandwidth, double sampleRate, double attenuation)
    {
        var iq = Array.ConvertAll(iqDatas, item => (float)item);
        var exp = Utilities.Log2N(iq.Length / 2);
        var length = 1 << exp;
        var windowValue = new float[length];
        var coe = Utilities.Window(ref windowValue);
        var spectrum = Utilities.GetWindowData(iq, windowValue, length);
        Utilities.Fft(ref spectrum);
        var efficientLength = (int)(length * 1.0 * bandwidth / sampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        coe += (float)(-20 * Math.Log10(length) + attenuation) + LevelCalibrationFromIq;
        var spectrumEx = new float[length];
        for (var index = 0; index < length; ++index)
            spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
        var validSpectrum = new short[efficientLength];
        for (var index = 0; index < validSpectrum.Length; ++index)
            validSpectrum[index] = (short)((spectrumEx[(efficientIndex + index) % length] + coe) * 10);
        return validSpectrum;
    }

    /// <summary>
    ///     IQ数据转电平
    /// </summary>
    /// <param name="iqDatas">IQ数据</param>
    /// <param name="attenuation">衰减</param>
    /// <returns>电平</returns>
    private float ToLevelByIq(short[] iqDatas, double attenuation)
    {
        var iq = Array.ConvertAll(iqDatas, item => (float)item);
        var level = Utilities.GetLevel(iq);
        level += (float)(attenuation + LevelCalibrationFromIq);
        return level;
    }
}