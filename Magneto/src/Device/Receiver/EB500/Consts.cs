using System;
using System.Collections.Generic;

namespace Magneto.Device.EB500;

internal static class Consts
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

    internal static readonly Dictionary<int, (int start, int end)> Spans = new()
    {
        { 0, (0, 1) },
        { 1, (0, 1) },
        { 2, (0, 2) },
        { 3, (0, 3) },
        { 4, (0, 4) },
        { 5, (0, 4) },
        { 6, (0, 5) },
        { 7, (0, 6) },
        { 8, (0, 6) },
        { 9, (0, 7) },
        { 10, (0, 7) },
        { 11, (0, 7) },
        { 12, (1, 8) },
        { 13, (1, 8) },
        { 14, (2, 9) },
        { 15, (2, 9) },
        { 16, (2, 10) },
        { 17, (3, 10) },
        { 18, (3, 10) },
        { 19, (4, 11) },
        { 20, (4, 11) },
        { 21, (5, 12) },
        { 22, (5, 12) },
        { 23, (5, 13) },
        { 24, (6, 13) },
        { 25, (6, 13) },
        { 26, (6, 13) },
        { 27, (7, 13) },
        { 28, (7, 13) },
        { 29, (8, 13) },
        { 30, (9, 13) },
        { 31, (10, 13) },
        { 32, (11, 13) },
        { 33, (12, 13) },
        { 34, (13, 13) }
    };

    internal static readonly Dictionary<int, (int start, int end)> IfSteps = new()
    {
        { 0, (0, 11) },
        { 1, (0, 13) },
        { 2, (2, 16) },
        { 3, (3, 18) },
        { 4, (4, 20) },
        { 5, (6, 23) },
        { 6, (7, 26) },
        { 7, (9, 28) },
        { 8, (12, 29) },
        { 9, (14, 30) },
        { 10, (16, 31) },
        { 11, (19, 32) },
        { 12, (21, 33) },
        { 13, (23, 34) }
    };

    /// <summary>
    ///     参数单位都为kHz
    /// </summary>
    internal static readonly double[] ArraySpan =
    {
        1d, 2d, 5d, 10d, 20d, 50d, 100d, 200d, 500d, 1000d, 2000d, 5000d, 1000d, 20000d
    };

    internal static readonly double[] ArrayStep =
    {
        0.000625d, 0.00125d, 0.0025d, 0.003125d, 0.00625d, 0.0125d, 0.025d,
        0.03125d, 0.05d, 0.0625d, 0.1d, 0.125d,
        0.2d, 0.25d, 0.315d, 0.5d, 0.625d, 1d, 1.25d, 2d, 2.5d, 3.125d,
        5d, 6.25d, 8.333d, 10d, 12.5d, 20d, 25d, 50d, 100d, 200d, 500d, 1000d, 2000d
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

#region 枚举/类定义

[Flags]
internal enum DataType
{
    Fscan = 101,
    MScan = 201,
    Dscan = 301,
    Audio = 401,
    Ifpan = 501,
    Cw = 801,
    If = 901,
    Video = 1001,
    Vdpan = 1101,
    Pscan = 1201,
    SeleCall = 1301,
    Dfpan = 1401,
    Pifpan = 1601,
    GpsCompass = 1801,
    FmtRigger = 2201,
    Zspan = 2401,
    Hrpan = 5601, //advanced attribute
    LastTag //遍历数据结构时用
}

[Flags]
internal enum Flags : uint
{
    Level = 0x1,
    Offset = 0x2,
    Fstrength = 0x4,
    Am = 0x8,
    AmPos = 0x10,
    AmNeg = 0x20,
    Fm = 0x40,
    FmPos = 0x80,
    FmNeg = 0x100,
    Pm = 0x200,
    Bandwidth = 0x400,
    DfLevel = 0x800,
    Azimuth = 0x1000,
    DfQuality = 0x2000,
    DfFstrength = 0x4000,
    Channel = 0x00010000,
    Freqlow = 0x00020000,
    Elevation = 0x00040000,
    DfOmniphase = 0x00100000,
    Freqhigh = 0x00200000,
    BandwidthCenter = 0x00400000,
    FreqOffsetRel = 0x00800000,
    Private = 0x10000000,

    /// <summary>
    ///     swap ON means: do NOT swap (for little endian machines)
    /// </summary>
    Swap = 0x20000000,
    SignalGreaterSquelch = 0x40000000,
    OptionalHeader = 0x80000000
}

#endregion