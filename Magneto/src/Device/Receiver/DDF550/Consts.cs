using System.Collections.Generic;

namespace Magneto.Device.DDF550;

internal class Consts
{
    internal static readonly Dictionary<int, AfMode> AfModes = new()
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
}

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