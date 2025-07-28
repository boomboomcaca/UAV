using System.Runtime.InteropServices;

namespace Magneto.Contract.Algorithm;

public static class ItuMeasure
{
    private const string LibPath = "library\\ITU\\x86\\ITU.dll";

    /// <summary>
    ///     根据16位IQ计算ITU数据
    /// </summary>
    /// <param name="arrIq">IQ数组</param>
    /// <param name="length">数据长度</param>
    /// <param name="fs">信号采样率（单位Hz）</param>
    /// <param name="xdB">xdB带宽选择（范围[3,60]，默认26dB）</param>
    /// <param name="beta">beta带宽选择（暂定固定输入1）</param>
    /// <param name="fcc">接收机当前调谐频率（单位Hz）</param>
    /// <param name="detaF">频率测量容限（单位：Hz,暂定固定输入5000）</param>
    /// <param name="ituResult">ITU测量结果</param>
    [DllImport(LibPath, CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeasureITU(short[] arrIq, int length, int fs, int xdB, int beta, double fcc, int detaF,
        ref ItuResult ituResult);

    /// <summary>
    ///     根据16位IQ计算ITU数据
    /// </summary>
    /// <param name="iqData">IQ数组</param>
    /// <param name="samplingrate">信号采样率（单位Hz）</param>
    /// <param name="xdb">xdB带宽选择（范围[3,60]，默认26dB）</param>
    /// <param name="beta">beta带宽选择（暂定固定输入1）</param>
    /// <param name="frequency">接收机当前调谐频率（单位Hz）</param>
    /// <param name="result"></param>
    public static bool MeasureItu(short[] iqData, int samplingrate, int xdb, int beta, double frequency,
        out ItuResult result)
    {
        result = new ItuResult();
        return MeasureITU(iqData, iqData.Length, samplingrate, xdb, beta, frequency, 5000, ref result);
    }
}

/// <summary>
///     ITU.dll动态库ITU计算结果输出结构体
/// </summary>
public struct ItuResult
{
    /// <summary>
    ///     功率谱密度上的β带宽（单位：Hz）
    /// </summary>
    public double BetaBwPsd;

    /// <summary>
    ///     功率谱密度上的xdb带宽（单位：Hz）
    /// </summary>
    public double XdBBwPsd;

    /// <summary>
    ///     中心频率（单位：Hz）
    /// </summary>
    public double Frequency;

    /// <summary>
    ///     AM调制度，取值范围0~1，如调制度为50%则为0.5 (0 %-100 %)
    /// </summary>
    public double AmMod;

    /// <summary>
    ///     FM最大频偏(单位：Hz)
    /// </summary>
    public double FmMod;

    /// <summary>
    ///     FM正向调制深度(单位：Hz)
    /// </summary>
    public double FmPos;

    /// <summary>
    ///     FM负向调制深度(单位：Hz)
    /// </summary>
    public double FmNeg;

    /// <summary>
    ///     PM最大调制相偏(单位：rad)
    /// </summary>
    public double PmMod;
}