using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.DDF200M;

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
    Swap = 0x20000000, // swap ON means: do NOT swap (for little endian machines)
    SignalGreaterSquelch = 0x40000000,
    OptionalHeader = 0x80000000
}

[Flags]
internal enum Tags
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

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct Eb200DatagramFormat
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint Magic;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort VersionMinor;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort VersionMajor;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Sequence;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort reserved;
    [MarshalAs(UnmanagedType.U4)] public readonly uint DataSize;

    public Eb200DatagramFormat(byte[] buffer, int index)
    {
        Magic = BitConverter.ToUInt32(buffer, index);
        VersionMinor = BitConverter.ToUInt16(buffer, index + 4);
        VersionMajor = BitConverter.ToUInt16(buffer, index + 6);
        Sequence = BitConverter.ToUInt16(buffer, index + 8);
        reserved = BitConverter.ToUInt16(buffer, index + 10);
        DataSize = BitConverter.ToUInt32(buffer, 14);
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct GenericAttribute
{
    [MarshalAs(UnmanagedType.U2)] public readonly short tag;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort length;

    public GenericAttribute(byte[] buffer, int index)
    {
        Array.Reverse(buffer, index, 2);
        tag = BitConverter.ToInt16(buffer, index);
        Array.Reverse(buffer, index + 2, 2);
        length = BitConverter.ToUInt16(buffer, index + 2);
    }
}

//advanced attribute type. Tag > 5000
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct GenericAttributeA
{
    [MarshalAs(UnmanagedType.U2)] public readonly ushort tag;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort reserved;
    [MarshalAs(UnmanagedType.U4)] public readonly uint length;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved0;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved1;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved2;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved3;

    public GenericAttributeA(byte[] value, int startIndex)
    {
        //TODO: lx确定此处是否需要转换
        Array.Reverse(value, startIndex, 2);
        tag = BitConverter.ToUInt16(value, startIndex);
        Array.Reverse(value, startIndex + 2, 2);
        reserved = BitConverter.ToUInt16(value, startIndex + 2);
        Array.Reverse(value, startIndex + 4, 4);
        length = BitConverter.ToUInt32(value, startIndex + 4);
        Array.Reverse(value, startIndex + 8, 4);
        reserved0 = BitConverter.ToUInt32(value, startIndex + 8);
        Array.Reverse(value, startIndex + 12, 4);
        reserved1 = BitConverter.ToUInt32(value, startIndex + 12);
        Array.Reverse(value, startIndex + 16, 4);
        reserved2 = BitConverter.ToUInt32(value, startIndex + 16);
        Array.Reverse(value, startIndex + 20, 4);
        reserved3 = BitConverter.ToUInt32(value, startIndex + 20);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct TraceAttribute
{
    [MarshalAs(UnmanagedType.I2)] public readonly short number_of_trace_items;
    [MarshalAs(UnmanagedType.U1)] public readonly byte ChannelNumber;
    [MarshalAs(UnmanagedType.U1)] public readonly byte optional_header_length;
    [MarshalAs(UnmanagedType.U4)] public readonly int selectorFlags;

    public TraceAttribute(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        number_of_trace_items = BitConverter.ToInt16(buffer, offset);
        ChannelNumber = buffer[offset + 2];
        optional_header_length = buffer[offset + 3];
        Array.Reverse(buffer, offset + 4, 4);
        selectorFlags = BitConverter.ToInt32(buffer, offset + 4);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderIf
{
    //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
    [MarshalAs(UnmanagedType.I2)] public readonly short IFMode;
    [MarshalAs(UnmanagedType.I2)] public readonly short FrameLen;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Samplerate;
    [MarshalAs(UnmanagedType.U4)] public readonly uint FrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Bandwidth; //IF bandwidth
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Demodulation;
    [MarshalAs(UnmanagedType.I2)] public readonly short RxAttenuation;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Flags;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public readonly string sDemodulation;

    [MarshalAs(UnmanagedType.U8)] public readonly ulong SampleCount;
    [MarshalAs(UnmanagedType.U4)] public readonly uint FrequencyHigh;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
    public readonly byte[] reserved;

    [MarshalAs(UnmanagedType.U8)] public readonly ulong StartTimestamp;
    [MarshalAs(UnmanagedType.I2)] public readonly short SignalSource;

    public OptionalHeaderIf(byte[] value, int startIndex)
    {
        IFMode = BitConverter.ToInt16(value, startIndex);
        FrameLen = BitConverter.ToInt16(value, startIndex + 2);
        Samplerate = BitConverter.ToUInt32(value, startIndex + 4);
        FrequencyLow = BitConverter.ToUInt32(value, startIndex + 8);
        Bandwidth = BitConverter.ToUInt32(value, startIndex + 12);
        Demodulation = BitConverter.ToUInt16(value, startIndex + 16);
        RxAttenuation = BitConverter.ToInt16(value, startIndex + 18);
        Flags = BitConverter.ToUInt16(value, startIndex + 20);
        sDemodulation = BitConverter.ToString(value, startIndex + 22, 8);
        SampleCount = BitConverter.ToUInt64(value, startIndex + 30);
        FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 38);
        reserved = new byte[4];
        StartTimestamp = BitConverter.ToUInt64(value, startIndex + 46);
        SignalSource = BitConverter.ToInt16(value, startIndex + 54);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderFScan
{
    [MarshalAs(UnmanagedType.I2)] public readonly short cycleCount;
    [MarshalAs(UnmanagedType.I2)] public readonly short holdTime;
    [MarshalAs(UnmanagedType.I2)] public readonly short dwellTime;
    [MarshalAs(UnmanagedType.I2)] public readonly short directionUp;
    [MarshalAs(UnmanagedType.I2)] public readonly short stopSignal;
    [MarshalAs(UnmanagedType.U4)] public readonly uint startFrequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stopFrequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFrequency;

    public OptionalHeaderFScan(byte[] buffer, int offset)
    {
        cycleCount = BitConverter.ToInt16(buffer, offset);
        holdTime = BitConverter.ToInt16(buffer, offset + 2);
        dwellTime = BitConverter.ToInt16(buffer, offset + 4);
        directionUp = BitConverter.ToInt16(buffer, offset + 6);
        stopSignal = BitConverter.ToInt16(buffer, offset + 8);
        startFrequency = BitConverter.ToUInt32(buffer, offset + 10);
        stopFrequency = BitConverter.ToUInt32(buffer, offset + 14);
        stepFrequency = BitConverter.ToUInt32(buffer, offset + 18);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderMScan
{
    [MarshalAs(UnmanagedType.I2)] public readonly short cycleCount;
    [MarshalAs(UnmanagedType.I2)] public readonly short holdTime;
    [MarshalAs(UnmanagedType.I2)] public readonly short dwellTime;
    [MarshalAs(UnmanagedType.I2)] public readonly short directionUp;
    [MarshalAs(UnmanagedType.I2)] public readonly short stopSignal;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved1;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort reserved2;

    [MarshalAs(UnmanagedType.U8)] public readonly ulong outputTimestamp;

    //[MarshalAs(UnmanagedType.U8)]
    //public UInt64 startFreq;
    //[MarshalAs(UnmanagedType.U8)]
    //public UInt64 stopFreq;   
    public OptionalHeaderMScan(byte[] buffer, int offset)
    {
        cycleCount = BitConverter.ToInt16(buffer, offset);
        holdTime = BitConverter.ToInt16(buffer, offset + 2);
        dwellTime = BitConverter.ToInt16(buffer, offset + 4);
        directionUp = BitConverter.ToInt16(buffer, offset + 6);
        stopSignal = BitConverter.ToInt16(buffer, offset + 8);
        reserved1 = BitConverter.ToUInt32(buffer, offset + 12);
        reserved2 = BitConverter.ToUInt16(buffer, offset + 14);
        outputTimestamp = BitConverter.ToUInt64(buffer, offset + 22);
        //startFreq = BitConverter.ToUInt64(buffer, offset + 30);
        //stopFreq = BitConverter.ToUInt64(buffer, offset + 38);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderPScan
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint startFrequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stopFrequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFrequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint startFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stopFrequencyHigh;
    [MarshalAs(UnmanagedType.U4)] public readonly uint reserved;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong outputTimeStamp;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFreqNumerator;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFreqDenominator;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong firstFreq;
    [MarshalAs(UnmanagedType.U4)] public readonly uint unknown;

    public OptionalHeaderPScan(byte[] buffer, int offset)
    {
        startFrequency = BitConverter.ToUInt32(buffer, offset);
        stopFrequency = BitConverter.ToUInt32(buffer, offset + 4);
        stepFrequency = BitConverter.ToUInt32(buffer, offset + 8);
        startFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 12);
        stopFrequencyHigh = BitConverter.ToUInt32(buffer, offset + 16);
        reserved = BitConverter.ToUInt32(buffer, offset + 20);
        outputTimeStamp = BitConverter.ToUInt64(buffer, offset + 24);
        stepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 32);
        stepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 36);
        firstFreq = BitConverter.ToUInt64(buffer, offset + 40);
        unknown = BitConverter.ToUInt32(buffer, offset + 48);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderIfPan
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint freqlow;
    [MarshalAs(UnmanagedType.U4)] public readonly uint spanFrequency;
    [MarshalAs(UnmanagedType.I2)] public readonly short avgtime;
    [MarshalAs(UnmanagedType.I2)] public readonly short avgType;
    [MarshalAs(UnmanagedType.I4)] public readonly int measureTime;
    [MarshalAs(UnmanagedType.U4)] public readonly uint freqhigh;
    [MarshalAs(UnmanagedType.I4)] public readonly int demFreqChan;
    [MarshalAs(UnmanagedType.U4)] public readonly uint demFreqlow;
    [MarshalAs(UnmanagedType.U4)] public readonly uint demFreqhigh;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong outputTimestamp;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFreqNumerator;
    [MarshalAs(UnmanagedType.U4)] public readonly uint stepFreqDenominator;
    [MarshalAs(UnmanagedType.I2)] public readonly short SignalSource;
    [MarshalAs(UnmanagedType.I2)] public readonly short MeasureMode;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong MeasureTimestamp;
    [MarshalAs(UnmanagedType.I2)] public readonly short Selectivity;

    public OptionalHeaderIfPan(byte[] buffer, int offset)
    {
        freqlow = BitConverter.ToUInt32(buffer, offset);
        spanFrequency = BitConverter.ToUInt32(buffer, offset + 4);
        avgtime = BitConverter.ToInt16(buffer, offset + 8);
        avgType = BitConverter.ToInt16(buffer, offset + 10);
        measureTime = BitConverter.ToInt32(buffer, offset + 12);
        freqhigh = BitConverter.ToUInt32(buffer, offset + 16);
        demFreqChan = BitConverter.ToInt32(buffer, offset + 20);
        demFreqlow = BitConverter.ToUInt32(buffer, offset + 24);
        demFreqhigh = BitConverter.ToUInt32(buffer, offset + 28);
        outputTimestamp = BitConverter.ToUInt64(buffer, offset + 32);
        stepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 36);
        stepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 40);
        SignalSource = BitConverter.ToInt16(buffer, offset + 42);
        MeasureMode = BitConverter.ToInt16(buffer, offset + 44);
        MeasureTimestamp = BitConverter.ToUInt64(buffer, offset + 52);
        Selectivity = BitConverter.ToInt16(buffer, offset + 60);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderAudio
{
    [MarshalAs(UnmanagedType.I2)] public readonly short AudioMode;
    [MarshalAs(UnmanagedType.I2)] public readonly short FrameLen;
    [MarshalAs(UnmanagedType.U4)] public readonly uint FrequencyLow;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Bandwidth;

    [MarshalAs(UnmanagedType.U2)]
    public readonly ushort Demodulation; //FM:0, AM:1, PULS:2, PM:3, IQ:4, ISB:5, CW:6, USB:7, LSB:8, TV:9

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public readonly string sDemodulation;

    [MarshalAs(UnmanagedType.U4)] public readonly uint FrequencyHigh;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
    public readonly byte[] reserved;

    [MarshalAs(UnmanagedType.U8)] public readonly ulong OutputTimestamp;
    [MarshalAs(UnmanagedType.I2)] public readonly short SignalSource;

    public OptionalHeaderAudio(byte[] value, int startIndex)
    {
        AudioMode = BitConverter.ToInt16(value, startIndex);
        FrameLen = BitConverter.ToInt16(value, startIndex + 2);
        FrequencyLow = BitConverter.ToUInt32(value, startIndex + 4);
        Bandwidth = BitConverter.ToUInt32(value, startIndex + 8);
        Demodulation = BitConverter.ToUInt16(value, startIndex + 12);
        sDemodulation = BitConverter.ToString(value, startIndex + 14, 8);
        FrequencyHigh = BitConverter.ToUInt32(value, startIndex + 22);
        reserved = new byte[6];
        OutputTimestamp = BitConverter.ToUInt64(value, startIndex + 32);
        SignalSource = BitConverter.ToInt16(value, startIndex + 40);
    }
}

/// <summary>
///     GPS数据结构
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct GpsHeader
{
    /* denotes whether GPS data are to be considered valid */
    [MarshalAs(UnmanagedType.I2)] public readonly short bValid;

    /* number of satellites in view 0-12; only
     * valid, if GGA msg is received, else -1 (GPS_UNDEFINDED) */
    [MarshalAs(UnmanagedType.I2)] public readonly short iNoOfSatInView;
    [MarshalAs(UnmanagedType.I2)] public readonly short iLatRef; /* latitude direction ('N' or 'S') */
    [MarshalAs(UnmanagedType.I2)] public readonly short iLatDeg; /* latitude degrees */
    [MarshalAs(UnmanagedType.R4)] public readonly float fLatMin; /* geographical latitude: minutes */
    [MarshalAs(UnmanagedType.I2)] public readonly short iLonRef; /* longitude direction ('E' or 'W') */
    [MarshalAs(UnmanagedType.I2)] public readonly short iLonDeg; /* longitude degrees */
    [MarshalAs(UnmanagedType.R4)] public readonly float fLonMin; /* geographical longitude: minutes */
    [MarshalAs(UnmanagedType.R4)] public readonly float HDOP;

    public GpsHeader(byte[] buffer, int offset)
    {
        bValid = BitConverter.ToInt16(buffer, offset);
        iNoOfSatInView = BitConverter.ToInt16(buffer, offset + 2);
        iLatRef = BitConverter.ToInt16(buffer, offset + 4);
        iLatDeg = BitConverter.ToInt16(buffer, offset + 6);
        fLatMin = BitConverter.ToInt16(buffer, offset + 8);
        iLonRef = BitConverter.ToInt16(buffer, offset + 12);
        iLonDeg = BitConverter.ToInt16(buffer, offset + 14);
        fLonMin = BitConverter.ToInt16(buffer, offset + 16);
        HDOP = BitConverter.ToUInt16(buffer, offset + 20);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderDfPan
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint Freq_low;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Freq_high;
    [MarshalAs(UnmanagedType.U4)] public readonly uint FreqSpan;
    [MarshalAs(UnmanagedType.I4)] public readonly int DFThresholdMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int DFThresholdValue;
    [MarshalAs(UnmanagedType.U4)] public readonly uint DFBandWidth;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Stepwidth;
    [MarshalAs(UnmanagedType.I4)] public readonly int DFMeasureTime;
    [MarshalAs(UnmanagedType.I4)] public readonly int DFOption;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort CompassHeading;
    [MarshalAs(UnmanagedType.I2)] public readonly short CompassHeadingType;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Reserved;
    [MarshalAs(UnmanagedType.I4)] public readonly int DemodFreqChannel;
    [MarshalAs(UnmanagedType.U4)] public readonly uint DemodFreq_low;
    [MarshalAs(UnmanagedType.U4)] public readonly uint DemodFreq_high;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong OutputTimeStamp; // reserved for future use  
    [MarshalAs(UnmanagedType.Struct)] public GpsHeader GPSHeader;
    [MarshalAs(UnmanagedType.U4)] public readonly uint StepFreqNumerator;
    [MarshalAs(UnmanagedType.U4)] public readonly uint StepFreqDenominator;
    [MarshalAs(UnmanagedType.U8)] public readonly ulong DFBandwidthHighRes;
    [MarshalAs(UnmanagedType.I2)] public readonly short Level;
    [MarshalAs(UnmanagedType.I2)] public readonly short Azimuth;
    [MarshalAs(UnmanagedType.I2)] public readonly short Quality;
    [MarshalAs(UnmanagedType.I2)] public readonly short Elevation;
    [MarshalAs(UnmanagedType.I2)] public readonly short Omniphase;

    public OptionalHeaderDfPan(byte[] buffer, int offset)
    {
        Freq_low = BitConverter.ToUInt32(buffer, offset);
        Freq_high = BitConverter.ToUInt32(buffer, offset + 4);
        FreqSpan = BitConverter.ToUInt32(buffer, offset + 8);
        DFThresholdMode = BitConverter.ToInt32(buffer, offset + 12);
        DFThresholdValue = BitConverter.ToInt32(buffer, offset + 16);
        DFBandWidth = BitConverter.ToUInt32(buffer, offset + 20);
        Stepwidth = BitConverter.ToUInt32(buffer, offset + 24);
        DFMeasureTime = BitConverter.ToInt32(buffer, offset + 28);
        DFOption = BitConverter.ToInt32(buffer, offset + 32);
        CompassHeading = BitConverter.ToUInt16(buffer, offset + 36);
        CompassHeadingType = BitConverter.ToInt16(buffer, offset + 38);
        Reserved = BitConverter.ToUInt32(buffer, offset + 40);
        DemodFreqChannel = BitConverter.ToInt32(buffer, offset + 44);
        DemodFreq_low = BitConverter.ToUInt32(buffer, offset + 48);
        DemodFreq_high = BitConverter.ToUInt32(buffer, offset + 52);
        OutputTimeStamp = BitConverter.ToUInt64(buffer, offset + 56);
        GPSHeader = new GpsHeader(buffer, offset + 64);
        StepFreqNumerator = BitConverter.ToUInt32(buffer, offset + 88);
        StepFreqDenominator = BitConverter.ToUInt32(buffer, offset + 92);
        DFBandwidthHighRes = BitConverter.ToUInt64(buffer, offset + 96);
        Level = BitConverter.ToInt16(buffer, offset + 104);
        Azimuth = BitConverter.ToInt16(buffer, offset + 106);
        Quality = BitConverter.ToInt16(buffer, offset + 108);
        Elevation = BitConverter.ToInt16(buffer, offset + 110);
        Omniphase = BitConverter.ToInt16(buffer, offset + 112);
    }
}