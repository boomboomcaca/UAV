using System;
using System.Runtime.InteropServices;

namespace Magneto.Device;

[Flags]
public enum Tags
{
    Fscan = 10101,
    Mscan = 10201,
    Dscan = 301,
    Audio = 401,
    Ifpan = 10501,
    Cw = 10801,
    If = 901,
    Video = 1001,
    Vdpan = 1101,
    Pscan = 11201,
    Selcall = 1301,
    DfPan = 1401,
    PifPan = 1601,
    LastTag
}

[Flags]
public enum Flags : ulong
{
    /// <summary>
    ///     1/10 dBμV
    /// </summary>
    Level = 0x1,

    /// <summary>
    ///     Hz
    /// </summary>
    Offset = 0x2,

    /// <summary>
    ///     1/10 dBμV/m
    /// </summary>
    Fstrength = 0x4,

    /// <summary>
    ///     1/10 %
    /// </summary>
    Am = 0x8,

    /// <summary>
    ///     1/10 %
    /// </summary>
    AmPos = 0x10,

    /// <summary>
    ///     1/10 %
    /// </summary>
    AmNeg = 0x20,

    /// <summary>
    ///     Hz
    /// </summary>
    Fm = 0x40,

    /// <summary>
    ///     Hz
    /// </summary>
    FmPos = 0x80,

    /// <summary>
    ///     Hz
    /// </summary>
    FmNeg = 0x100,

    /// <summary>
    ///     1/100 rad
    /// </summary>
    Pm = 0x200,

    /// <summary>
    ///     1/10 dBμV
    /// </summary>
    DfLevel = 0x800,

    /// <summary>
    ///     1/10 °
    /// </summary>
    Azimuth = 0x1000,

    /// <summary>
    ///     1/10 %
    /// </summary>
    DfQuality = 0x2000,

    /// <summary>
    ///     1/10 dBμV/m
    /// </summary>
    DfFstrength = 0x4000,

    /// <summary>
    ///     1/10 °
    /// </summary>
    Elevation = 0x00040000,

    /// <summary>
    ///     1/10 °
    /// </summary>
    DfOmniphase = 0x00100000,
    Level2 = 0x02000000,
    Level3 = 0x04000000,
    Level4 = 0x08000000,
    Fstrength2 = 0x100000000,
    Fstrength3 = 0x200000000,
    Fstrength4 = 0x400000000,
    Ifpan1 = 0x800000000,
    Ifpan2 = 0x1000000000,
    Ifpan3 = 0x2000000000,
    Ifpan4 = 0x4000000000,
    Zsam1 = 0x8000000000,
    Zsam2 = 0x10000000000,
    Zsam3 = 0x20000000000,
    Zsam4 = 0x40000000000,
    Zsfm1 = 0x80000000000,
    Zsfm2 = 0x100000000000,
    Channel = 0x00010000,
    Freqlow = 0x00020000,
    Freqhigh = 0x00200000,
    FreqOffsetRel = 0x00800000,
    Private = 0x10000000,
    Swap = 0x20000000, // swap ON means: do NOT swap (for little endian machines)
    SignalGreaterSquelch = 0x40000000,
    OptionalHeader = 0x80000000
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct Eb200DatagramFormat
{
    [MarshalAs(UnmanagedType.U4)] public uint Magic;
    [MarshalAs(UnmanagedType.U2)] public ushort VersionMinor;
    [MarshalAs(UnmanagedType.U2)] public ushort VersionMajor;
    [MarshalAs(UnmanagedType.U2)] public ushort Sequence_low;
    [MarshalAs(UnmanagedType.U2)] public ushort Sequency_high;
    [MarshalAs(UnmanagedType.U4)] public uint DataSize;

    public Eb200DatagramFormat(byte[] buffer, int offset)
    {
        Magic = BitConverter.ToUInt32(buffer, offset);
        VersionMinor = BitConverter.ToUInt16(buffer, offset + 4);
        VersionMajor = BitConverter.ToUInt16(buffer, offset + 6);
        Sequence_low = BitConverter.ToUInt16(buffer, offset + 8);
        Sequency_high = BitConverter.ToUInt16(buffer, offset + 10);
        DataSize = BitConverter.ToUInt32(buffer, offset + 12);
    }
}

public interface IGenericAttribute
{
    ushort TraceTag { get; }
    uint DataLength { get; }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct GenericAttributeConventional : IGenericAttribute
{
    [MarshalAs(UnmanagedType.U2)] public ushort Tag;
    [MarshalAs(UnmanagedType.U2)] public ushort Length;

    public GenericAttributeConventional(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        Tag = BitConverter.ToUInt16(buffer, offset);
        Array.Reverse(buffer, offset, 2);
        Array.Reverse(buffer, offset + 2, 2);
        Length = BitConverter.ToUInt16(buffer, offset + 2);
        Array.Reverse(buffer, offset + 2, 2);
    }

    public ushort TraceTag => Tag;
    public uint DataLength => Length;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct GenericAttributeAdvanced : IGenericAttribute
{
    [MarshalAs(UnmanagedType.U2)] public ushort Tag;
    [MarshalAs(UnmanagedType.U2)] public ushort Reserved;
    [MarshalAs(UnmanagedType.U4)] public uint Length;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved1;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved2;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved3;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved4;

    public GenericAttributeAdvanced(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        Tag = BitConverter.ToUInt16(buffer, offset);
        Array.Reverse(buffer, offset + 2, 2);
        Reserved = BitConverter.ToUInt16(buffer, offset + 2);
        Array.Reverse(buffer, offset + 4, 4);
        Length = BitConverter.ToUInt32(buffer, offset + 4);
        Array.Reverse(buffer, offset + 8, 4);
        Reserved1 = BitConverter.ToUInt32(buffer, offset + 8);
        Array.Reverse(buffer, offset + 12, 4);
        Reserved2 = BitConverter.ToUInt32(buffer, offset + 12);
        Array.Reverse(buffer, offset + 16, 4);
        Reserved3 = BitConverter.ToUInt32(buffer, offset + 16);
        Array.Reverse(buffer, offset + 20, 4);
        Reserved4 = BitConverter.ToUInt32(buffer, offset + 20);
    }

    public ushort TraceTag => Tag;
    public uint DataLength => Length;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct CommonHeaderConventional
{
    [MarshalAs(UnmanagedType.I2)] public short Number_of_trace_items;
    [MarshalAs(UnmanagedType.I1)] public sbyte ChannelNumber;
    [MarshalAs(UnmanagedType.U1)] public byte Optional_header_length;
    [MarshalAs(UnmanagedType.U4)] public uint SelectorFlags;

    public CommonHeaderConventional(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        Number_of_trace_items = BitConverter.ToInt16(buffer, offset);
        ChannelNumber = (sbyte)buffer[offset + 2];
        Optional_header_length = buffer[offset + 3];
        Array.Reverse(buffer, offset + 4, 4);
        SelectorFlags = BitConverter.ToUInt32(buffer, offset + 4);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct CommonHeaderAdvanced
{
    [MarshalAs(UnmanagedType.U4)] public uint Number_of_trace_items;
    [MarshalAs(UnmanagedType.U4)] public uint ChannelNumber;
    [MarshalAs(UnmanagedType.U4)] public uint Optional_header_length;
    [MarshalAs(UnmanagedType.U8)] public ulong SelectorFlags;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved1;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved2;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved3;
    [MarshalAs(UnmanagedType.U4)] public uint Reserved4;

    public CommonHeaderAdvanced(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 4);
        Number_of_trace_items = BitConverter.ToUInt32(buffer, offset);
        Array.Reverse(buffer, offset + 4, 4);
        ChannelNumber = BitConverter.ToUInt32(buffer, offset + 4);
        Array.Reverse(buffer, offset + 8, 4);
        Optional_header_length = BitConverter.ToUInt32(buffer, offset + 8);
        Array.Reverse(buffer, offset + 12, 4);
        var low = BitConverter.ToUInt32(buffer, offset + 12);
        Array.Reverse(buffer, offset + 16, 4);
        var high = BitConverter.ToUInt32(buffer, offset + 16);
        SelectorFlags = ((ulong)high << 32) + low;
        Array.Reverse(buffer, offset + 20, 4);
        Reserved1 = BitConverter.ToUInt32(buffer, offset + 20);
        Array.Reverse(buffer, offset + 24, 4);
        Reserved2 = BitConverter.ToUInt32(buffer, offset + 24);
        Array.Reverse(buffer, offset + 28, 4);
        Reserved3 = BitConverter.ToUInt32(buffer, offset + 28);
        Array.Reverse(buffer, offset + 32, 4);
        Reserved4 = BitConverter.ToUInt32(buffer, offset + 32);
    }
}

//optional_header_length is thus either 0 or 40.
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderFScan
{
    [MarshalAs(UnmanagedType.I2)] public short CycleCount;
    [MarshalAs(UnmanagedType.I2)] public short HoldTime;
    [MarshalAs(UnmanagedType.I2)] public short DwellTime;
    [MarshalAs(UnmanagedType.I2)] public short DirectionUP;
    [MarshalAs(UnmanagedType.I2)] public short StopSignal;
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyHigh;
    [MarshalAs(UnmanagedType.U2)] public ushort reserved;
    [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

    public OptionalHeaderFScan(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 2);
        CycleCount = BitConverter.ToInt16(value, startIndex);
        Array.Reverse(value, startIndex + 2, 2);
        HoldTime = BitConverter.ToInt16(value, startIndex + 2);
        Array.Reverse(value, startIndex + 4, 2);
        DwellTime = BitConverter.ToInt16(value, startIndex + 4);
        Array.Reverse(value, startIndex + 6, 2);
        DirectionUP = BitConverter.ToInt16(value, startIndex + 6);
        Array.Reverse(value, startIndex + 8, 2);
        StopSignal = BitConverter.ToInt16(value, startIndex + 8);
        Array.Reverse(value, startIndex + 10, 4);
        StartFrequencyLow = BitConverter.ToUInt32(value, startIndex + 10);
        Array.Reverse(value, startIndex + 14, 4);
        StopFrequencyLow = BitConverter.ToUInt32(value, startIndex + 14);
        Array.Reverse(value, startIndex + 18, 4);
        StepFrequency = BitConverter.ToUInt32(value, startIndex + 18);
        Array.Reverse(value, startIndex + 22, 4);
        StartFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
        Array.Reverse(value, startIndex + 26, 4);
        StopFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 26);
        Array.Reverse(value, startIndex + 30, 2);
        reserved = BitConverter.ToUInt16(value, startIndex + 30);
        Array.Reverse(value, startIndex + 32, 8);
        OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderMScan
{
    [MarshalAs(UnmanagedType.I2)] public short CycleCount;
    [MarshalAs(UnmanagedType.I2)] public short HoldTime;
    [MarshalAs(UnmanagedType.I2)] public short DwellTime;
    [MarshalAs(UnmanagedType.I2)] public short DirectionUp;
    [MarshalAs(UnmanagedType.I2)] public short StopSignal;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
    public byte[] reserved;

    [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyLow;
    [MarshalAs(UnmanagedType.I2)] public short Detector1;
    [MarshalAs(UnmanagedType.I2)] public short Detector2;
    [MarshalAs(UnmanagedType.I2)] public short Detector3;
    [MarshalAs(UnmanagedType.I2)] public short Detector4;

    public OptionalHeaderMScan(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        CycleCount = BitConverter.ToInt16(buffer, offset);
        Array.Reverse(buffer, offset + 2, 2);
        HoldTime = BitConverter.ToInt16(buffer, offset + 2);
        Array.Reverse(buffer, offset + 4, 2);
        DwellTime = BitConverter.ToInt16(buffer, offset + 4);
        Array.Reverse(buffer, offset + 6, 2);
        DirectionUp = BitConverter.ToInt16(buffer, offset + 6);
        Array.Reverse(buffer, offset + 8, 2);
        StopSignal = BitConverter.ToInt16(buffer, offset + 8);
        reserved = new byte[6];
        Array.Reverse(buffer, offset + 16, 8);
        OutputTimestamp = BitConverter.ToUInt64(buffer, offset + 16);
        Array.Reverse(buffer, offset + 24, 4);
        StartFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 24);
        Array.Reverse(buffer, offset + 28, 4);
        StartFrequencyLow = BitConverter.ToUInt32(buffer, offset + 28);
        Array.Reverse(buffer, offset + 32, 4);
        StopFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 32);
        Array.Reverse(buffer, offset + 36, 4);
        StopFrequencyLow = BitConverter.ToUInt32(buffer, offset + 36);
        Array.Reverse(buffer, offset + 40, 2);
        Detector1 = BitConverter.ToInt16(buffer, offset + 40);
        Array.Reverse(buffer, offset + 42, 2);
        Detector2 = BitConverter.ToInt16(buffer, offset + 42);
        Array.Reverse(buffer, offset + 44, 2);
        Detector3 = BitConverter.ToInt16(buffer, offset + 44);
        Array.Reverse(buffer, offset + 46, 2);
        Detector4 = BitConverter.ToInt16(buffer, offset + 46);
    }
}

/// <summary>
///     OptionalHeaderIF
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderIf
{
    //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
    [MarshalAs(UnmanagedType.I2)] public short Mode;
    [MarshalAs(UnmanagedType.I2)] public short FrameLen;
    [MarshalAs(UnmanagedType.U4)] public uint Samplerate;
    [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint Bandwidth; //IF bandwidth
    [MarshalAs(UnmanagedType.U2)] public ushort Demodulation;
    [MarshalAs(UnmanagedType.I2)] public short RxAtt;
    [MarshalAs(UnmanagedType.U2)] public ushort Flags;
    [MarshalAs(UnmanagedType.I2)] public short KFactor;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string sDemodulation;

    [MarshalAs(UnmanagedType.U8)] public ulong SampleCount;
    [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;
    [MarshalAs(UnmanagedType.I2)] public short RxGain;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
    public byte[] Reserved;

    [MarshalAs(UnmanagedType.U8)] public ulong StartTimestamp;
    [MarshalAs(UnmanagedType.I2)] public short SignalSource;

    public OptionalHeaderIf(byte[] value, int startIndex)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value, startIndex, 2);
            Array.Reverse(value, startIndex + 2, 2);
            Array.Reverse(value, startIndex + 4, 4);
            Array.Reverse(value, startIndex + 8, 4);
            Array.Reverse(value, startIndex + 12, 4);
            Array.Reverse(value, startIndex + 16, 2);
            Array.Reverse(value, startIndex + 18, 2);
            Array.Reverse(value, startIndex + 20, 2);
            Array.Reverse(value, startIndex + 22, 2);
            Array.Reverse(value, startIndex + 32, 8);
            Array.Reverse(value, startIndex + 40, 4);
            Array.Reverse(value, startIndex + 44, 2);
            Array.Reverse(value, startIndex + 48, 8);
            Array.Reverse(value, startIndex + 56, 2);
        }

        Mode = BitConverter.ToInt16(value, startIndex);
        FrameLen = BitConverter.ToInt16(value, startIndex + 2);
        Samplerate = BitConverter.ToUInt32(value, startIndex + 4);
        FrequencyLow = BitConverter.ToUInt32(value, startIndex + 8);
        Bandwidth = BitConverter.ToUInt32(value, startIndex + 12);
        Demodulation = BitConverter.ToUInt16(value, startIndex + 16);
        RxAtt = BitConverter.ToInt16(value, startIndex + 18);
        Flags = BitConverter.ToUInt16(value, startIndex + 20);
        KFactor = BitConverter.ToInt16(value, startIndex + 22);
        sDemodulation = BitConverter.ToString(value, startIndex + 24, 8);
        SampleCount = BitConverter.ToUInt64(value, startIndex + 32);
        FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 40);
        RxGain = BitConverter.ToInt16(value, startIndex + 44);
        Reserved = new byte[2];
        StartTimestamp = BitConverter.ToUInt64(value, startIndex + 48);
        SignalSource = BitConverter.ToInt16(value, startIndex + 56);
    }
}

//optional_header_length is thus either 0 or 48.
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderPScan
{
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyHigh;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public byte[] reserved;

    [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyNumerator;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyDenominator;
    [MarshalAs(UnmanagedType.U8)] public ulong FreqOfFirstStep;
    [MarshalAs(UnmanagedType.I2)] public short AvgType;
    [MarshalAs(UnmanagedType.I2)] public short SetIndex;
    [MarshalAs(UnmanagedType.I2)] public short AvgType2;
    [MarshalAs(UnmanagedType.I2)] public short AvgType3;
    [MarshalAs(UnmanagedType.I2)] public short AvgType4;
    [MarshalAs(UnmanagedType.I2)] public short AttMax;
    [MarshalAs(UnmanagedType.I2)] public short Overload;

    public OptionalHeaderPScan(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 4);
        StartFrequencyLow = BitConverter.ToUInt32(value, startIndex);
        Array.Reverse(value, startIndex + 4, 4);
        StopFrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);
        Array.Reverse(value, startIndex + 8, 4);
        StepFrequency = BitConverter.ToUInt32(value, startIndex + 8);
        Array.Reverse(value, startIndex + 12, 4);
        StartFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 12);
        Array.Reverse(value, startIndex + 16, 4);
        StopFrequencyHigh = BitConverter.ToUInt32(value, startIndex + 16);
        reserved = new byte[4];
        Array.Reverse(value, startIndex + 24, 8);
        OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 24);
        Array.Reverse(value, startIndex + 32, 4);
        StepFrequencyNumerator = BitConverter.ToUInt32(value, startIndex + 32);
        Array.Reverse(value, startIndex + 36, 4);
        StepFrequencyDenominator = BitConverter.ToUInt32(value, startIndex + 36);
        Array.Reverse(value, startIndex + 40, 8);
        FreqOfFirstStep = BitConverter.ToUInt64(value, startIndex + 40);
        Array.Reverse(value, startIndex + 48, 2);
        AvgType = BitConverter.ToInt16(value, startIndex + 48);
        Array.Reverse(value, startIndex + 50, 2);
        SetIndex = BitConverter.ToInt16(value, startIndex + 50);
        Array.Reverse(value, startIndex + 52, 2);
        AvgType2 = BitConverter.ToInt16(value, startIndex + 52);
        Array.Reverse(value, startIndex + 54, 2);
        AvgType3 = BitConverter.ToInt16(value, startIndex + 54);
        Array.Reverse(value, startIndex + 56, 2);
        AvgType4 = BitConverter.ToInt16(value, startIndex + 56);
        Array.Reverse(value, startIndex + 58, 2);
        AttMax = BitConverter.ToInt16(value, startIndex + 58);
        Array.Reverse(value, startIndex + 60, 2);
        Overload = BitConverter.ToInt16(value, startIndex + 60);
    }
}

/// <summary>
///     OptionalHeaderIFPan
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderIfPan
{
    [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint SpanFrequency;
    [MarshalAs(UnmanagedType.I2)] public short AverageTime; //Not used and always set to 0
    [MarshalAs(UnmanagedType.I2)] public short AverageType;
    [MarshalAs(UnmanagedType.U4)] public uint MeasureTime; //us
    [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;
    [MarshalAs(UnmanagedType.I4)] public int DemodFreqChannel;
    [MarshalAs(UnmanagedType.U4)] public uint DemodFreqLow;

    [MarshalAs(UnmanagedType.U4)] public uint DemodFreqHigh;

    //[MarshalAs(UnmanagedType.U8)]
    //public ulong DemodFreq;
    [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyNumerator;
    [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyDenominator;
    [MarshalAs(UnmanagedType.I2)] public short SignalSource;
    [MarshalAs(UnmanagedType.I2)] public short MeasureMode;
    [MarshalAs(UnmanagedType.U8)] public ulong MeasureTimestamp;
    [MarshalAs(UnmanagedType.I2)] public short Selectivity;
    [MarshalAs(UnmanagedType.I2)] public short AvgType2;
    [MarshalAs(UnmanagedType.I2)] public short AvgType3;
    [MarshalAs(UnmanagedType.I2)] public short AvgType4;

    [MarshalAs(UnmanagedType.U2)] public ushort Reserved;

    // GateHeader
    [MarshalAs(UnmanagedType.I2)] public short Enabled;
    [MarshalAs(UnmanagedType.I8)] public long Interval;
    [MarshalAs(UnmanagedType.I8)] public long Offset;
    [MarshalAs(UnmanagedType.I8)] public long Length;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="value"></param>
    /// <param name="startIndex"></param>
    /// <param name="sign">发来的数据与官方文档的协议不匹配，协议命名长度为96，结果发来的数据长度为68</param>
    public OptionalHeaderIfPan(byte[] value, int startIndex, bool sign)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value, startIndex, 4);
            Array.Reverse(value, startIndex + 4, 4);
            Array.Reverse(value, startIndex + 8, 2);
            Array.Reverse(value, startIndex + 10, 2);
            Array.Reverse(value, startIndex + 12, 4);
            Array.Reverse(value, startIndex + 16, 4);
            Array.Reverse(value, startIndex + 20, 4);
            Array.Reverse(value, startIndex + 24, 4);
            Array.Reverse(value, startIndex + 28, 4);
            Array.Reverse(value, startIndex + 32, 8);
            Array.Reverse(value, startIndex + 40, 4);
            Array.Reverse(value, startIndex + 44, 4);
            Array.Reverse(value, startIndex + 48, 2);
            Array.Reverse(value, startIndex + 50, 2);
            Array.Reverse(value, startIndex + 52, 8);
            Array.Reverse(value, startIndex + 60, 2);
            Array.Reverse(value, startIndex + 62, 2);
            Array.Reverse(value, startIndex + 64, 2);
            Array.Reverse(value, startIndex + 66, 2);
            if (sign)
            {
                Array.Reverse(value, startIndex + 68, 2);
                Array.Reverse(value, startIndex + 70, 2);
                Array.Reverse(value, startIndex + 72, 8);
                Array.Reverse(value, startIndex + 80, 8);
                Array.Reverse(value, startIndex + 88, 8);
            }
        }

        FrequencyLow = BitConverter.ToUInt32(value, startIndex);
        SpanFrequency = BitConverter.ToUInt32(value, startIndex + 4);
        AverageTime = BitConverter.ToInt16(value, startIndex + 8);
        AverageType = BitConverter.ToInt16(value, startIndex + 10);
        MeasureTime = BitConverter.ToUInt32(value, startIndex + 12);
        FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 16);
        DemodFreqChannel = BitConverter.ToInt32(value, startIndex + 20);
        DemodFreqLow = BitConverter.ToUInt32(value, startIndex + 24);
        DemodFreqHigh = BitConverter.ToUInt32(value, startIndex + 28);
        OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
        StepFrequencyNumerator = BitConverter.ToUInt32(value, startIndex + 40);
        StepFrequencyDenominator = BitConverter.ToUInt32(value, startIndex + 44);
        SignalSource = BitConverter.ToInt16(value, startIndex + 48);
        MeasureMode = BitConverter.ToInt16(value, startIndex + 50);
        MeasureTimestamp = BitConverter.ToUInt64(value, startIndex + 52);
        Selectivity = BitConverter.ToInt16(value, startIndex + 60);
        AvgType2 = BitConverter.ToInt16(value, startIndex + 62);
        AvgType3 = BitConverter.ToInt16(value, startIndex + 64);
        AvgType4 = BitConverter.ToInt16(value, startIndex + 66);
        if (sign)
        {
            Reserved = BitConverter.ToUInt16(value, startIndex + 68);
            Enabled = BitConverter.ToInt16(value, startIndex + 70);
            Interval = BitConverter.ToInt64(value, startIndex + 72);
            Offset = BitConverter.ToInt64(value, startIndex + 80);
            Length = BitConverter.ToInt64(value, startIndex + 88);
        }
        else
        {
            Reserved = 0;
            Enabled = 0;
            Interval = 0;
            Offset = 0;
            Length = 0;
        }
    }
}

/// <summary>
///     OptionalHeaderAudio
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderAudio
{
    [MarshalAs(UnmanagedType.I2)] public short AudioMode;
    [MarshalAs(UnmanagedType.I2)] public short FrameLen;
    [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public uint Bandwidth;

    [MarshalAs(UnmanagedType.U2)]
    public ushort Demodulation; //FM:0, AM:1, PULS:2, PM:3, IQ:4, ISB:5, CW:6, USB:7, LSB:8, TV:9

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string sDemodulation;

    [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
    public byte[] Reserved;

    [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;
    [MarshalAs(UnmanagedType.I2)] public short SignalSource;

    public OptionalHeaderAudio(byte[] value, int startIndex)
    {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(value, startIndex, 2);
            Array.Reverse(value, startIndex + 2, 2);
            Array.Reverse(value, startIndex + 4, 4);
            Array.Reverse(value, startIndex + 8, 4);
            Array.Reverse(value, startIndex + 12, 2);
            Array.Reverse(value, startIndex + 22, 4);
            Array.Reverse(value, startIndex + 32, 8);
            Array.Reverse(value, startIndex + 40, 2);
        }

        AudioMode = BitConverter.ToInt16(value, startIndex);
        FrameLen = BitConverter.ToInt16(value, startIndex + 2);
        FrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);
        Bandwidth = BitConverter.ToUInt32(value, startIndex + 8);
        Demodulation = BitConverter.ToUInt16(value, startIndex + 12);
        sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
        FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
        Reserved = new byte[6];
        OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
        SignalSource = BitConverter.ToInt16(value, startIndex + 40);
    }
}