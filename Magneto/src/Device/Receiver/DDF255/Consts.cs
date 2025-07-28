using System;
using System.Collections.Generic;

namespace Magneto.Device.DDF255;

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
    Swap = 0x20000000,
    SignalGreaterSquelch = 0x40000000,
    OptionalHeader = 0x80000000
}

[Flags]
internal enum DataType
{
    Fscan = 101,
    Mscan = 201,
    Dscan = 301,
    Audio = 401,
    Ifpan = 501,
    Fastl = 601,
    Listf = 701,
    Cw = 801,
    If = 901,
    Video = 1001,
    Vdpan = 1101,
    Pscan = 1201,
    Selcall = 1301,
    DfPan = 1401,
    LastTag
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

public enum FirmwareVersion : byte
{
    /// <summary>
    ///     默认
    /// </summary>
    Default,

    /// <summary>
    ///     第一个数据的频率格式反转
    /// </summary>
    OffsetFrequencySwaped,

    /// <summary>
    ///     旧版本（订阅模式）
    /// </summary>
    OldSubscribe,

    /// <summary>
    ///     旧版本（拼包）
    /// </summary>
    Old
}