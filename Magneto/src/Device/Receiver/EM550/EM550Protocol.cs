/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Receiver\EM550\EM550Protocol.cs
 *
 * 作    者:    苏 林 国
 *
 * 创作日期:    2018-04-03
 *
 * 备    注:	    EM550数据协议定义
 *
 *********************************************************************************************/

using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.EM550;

[Flags]
public enum Tags
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

[Flags]
public enum Flags : uint
{
    Level = 0x1,
    Offset = 0x2,
    Fstrength = 0x4,
    Am = 0x8,
    Ampos = 0x10,
    Amneg = 0x20,
    Fm = 0x40,
    Fmpos = 0x80,
    Fmneg = 0x100,
    Pm = 0x200,
    Band = 0x400,
    Channel = 0x00010000,
    Frequency = 0x00020000,
    Audio = 0x00040000,
    If = 0x00080000,
    Video = 0x00100000,
    Swap = 0x20000000,
    Siggtsqu = 0x40000000,
    Optheader = 0x80000000
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct Eb200DatagramFormat
{
    [MarshalAs(UnmanagedType.U4)] public uint Magic;
    [MarshalAs(UnmanagedType.U2)] public ushort VersionMinor;
    [MarshalAs(UnmanagedType.U2)] public ushort VersionMajor;
    [MarshalAs(UnmanagedType.U2)] public ushort Sequence;
    [MarshalAs(UnmanagedType.I2)] public short reserved0;
    [MarshalAs(UnmanagedType.I2)] public short reserved1;
    [MarshalAs(UnmanagedType.I2)] public short reserved2;

    public Eb200DatagramFormat(byte[] buffer, int offset)
    {
        Magic = BitConverter.ToUInt32(buffer, offset);
        VersionMinor = BitConverter.ToUInt16(buffer, offset + 4);
        VersionMajor = BitConverter.ToUInt16(buffer, offset + 6);
        Sequence = BitConverter.ToUInt16(buffer, offset + 8);
        reserved0 = BitConverter.ToInt16(buffer, offset + 10);
        reserved1 = BitConverter.ToInt16(buffer, offset + 12);
        reserved2 = BitConverter.ToInt16(buffer, offset + 14);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct GenericAttribute
{
    [MarshalAs(UnmanagedType.U2)] public ushort tag;
    [MarshalAs(UnmanagedType.U2)] public ushort length;

    public GenericAttribute(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        tag = BitConverter.ToUInt16(buffer, offset);
        Array.Reverse(buffer, offset + 2, 2);
        length = BitConverter.ToUInt16(buffer, offset + 2);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct CommonHeader
{
    [MarshalAs(UnmanagedType.I2)] public short number_of_trace_items;
    [MarshalAs(UnmanagedType.I1)] public sbyte reserved;
    [MarshalAs(UnmanagedType.U1)] public byte optional_header_length;
    [MarshalAs(UnmanagedType.U4)] public uint selectorFlags;

    public CommonHeader(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        number_of_trace_items = BitConverter.ToInt16(buffer, offset);
        reserved = (sbyte)buffer[offset + 2];
        optional_header_length = buffer[offset + 3];
        Array.Reverse(buffer, offset + 4, 4);
        selectorFlags = BitConverter.ToUInt32(buffer, offset + 4);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderFScan
{
    [MarshalAs(UnmanagedType.I2)] public short cycleCount;
    [MarshalAs(UnmanagedType.I2)] public short holdTime;
    [MarshalAs(UnmanagedType.I2)] public short dwellTime;
    [MarshalAs(UnmanagedType.I2)] public short directionUp;
    [MarshalAs(UnmanagedType.I2)] public short stopSignal;
    [MarshalAs(UnmanagedType.U4)] public uint startFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint stopFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint stepFrequency;

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
public struct OptionalHeaderMScan
{
    [MarshalAs(UnmanagedType.I2)] public short cycleCount;
    [MarshalAs(UnmanagedType.I2)] public short holdTime;
    [MarshalAs(UnmanagedType.I2)] public short dwellTime;
    [MarshalAs(UnmanagedType.I2)] public short directionUp;
    [MarshalAs(UnmanagedType.I2)] public short stopSignal;

    public OptionalHeaderMScan(byte[] buffer, int offset)
    {
        cycleCount = BitConverter.ToInt16(buffer, offset);
        holdTime = BitConverter.ToInt16(buffer, offset + 2);
        dwellTime = BitConverter.ToInt16(buffer, offset + 4);
        directionUp = BitConverter.ToInt16(buffer, offset + 6);
        stopSignal = BitConverter.ToInt16(buffer, offset + 8);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderIf
{
    [MarshalAs(UnmanagedType.U2)] public ushort Mode; /* see command: SYSTem:IF:REMote:MODE OFF|SHORT|LONG */
    [MarshalAs(UnmanagedType.U2)] public ushort FrameLen; /* describes the number of bytes per frame */
    [MarshalAs(UnmanagedType.U4)] public uint Samplerate; /* current sampling rate */
    [MarshalAs(UnmanagedType.U4)] public uint Freq; /* current receive frequency */
    [MarshalAs(UnmanagedType.U4)] public uint Bw; /* current IF bandwidth */
    [MarshalAs(UnmanagedType.U2)] public ushort Dem; /* current demodulation, see below */
    [MarshalAs(UnmanagedType.I2)] public short RxAtt; /* current attenuation from antenna to IQ */
    [MarshalAs(UnmanagedType.U2)] public ushort Flags; /* valid flag, see below*/
    [MarshalAs(UnmanagedType.U2)] public ushort reserved; /* reserved for 64-Bit alignement */

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string sDemodulation; /* demodulation mode as string */

    [MarshalAs(UnmanagedType.U8)] public ulong samplecount;

    public OptionalHeaderIf(byte[] buffer, int offset)
    {
        Mode = BitConverter.ToUInt16(buffer, offset);
        FrameLen = BitConverter.ToUInt16(buffer, offset + 2);
        Samplerate = BitConverter.ToUInt32(buffer, offset + 4);
        Freq = BitConverter.ToUInt32(buffer, offset + 8);
        Bw = BitConverter.ToUInt32(buffer, offset + 12);
        Dem = BitConverter.ToUInt16(buffer, offset + 16);
        RxAtt = BitConverter.ToInt16(buffer, offset + 18);
        Flags = BitConverter.ToUInt16(buffer, offset + 20);
        reserved = BitConverter.ToUInt16(buffer, offset + 22);
        sDemodulation = BitConverter.ToString(buffer, offset + 24, 8);
        samplecount = BitConverter.ToUInt64(buffer, offset + 32);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderPScan
{
    [MarshalAs(UnmanagedType.U4)] public uint startFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint stopFrequency;
    [MarshalAs(UnmanagedType.U4)] public uint stepFrequency;

    public OptionalHeaderPScan(byte[] buffer, int offset)
    {
        startFrequency = BitConverter.ToUInt32(buffer, offset);
        stopFrequency = BitConverter.ToUInt32(buffer, offset + 4);
        stepFrequency = BitConverter.ToUInt32(buffer, offset + 8);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderIfPan
{
    [MarshalAs(UnmanagedType.U4)] public uint frequency;
    [MarshalAs(UnmanagedType.U4)] public uint spanFrequency;
    [MarshalAs(UnmanagedType.I2)] public short reserved;
    [MarshalAs(UnmanagedType.I2)] public short averageType;
    [MarshalAs(UnmanagedType.U4)] public uint measureTime;

    public OptionalHeaderIfPan(byte[] buffer, int offset)
    {
        frequency = BitConverter.ToUInt32(buffer, offset);
        spanFrequency = BitConverter.ToUInt32(buffer, offset + 4);
        reserved = BitConverter.ToInt16(buffer, offset + 8);
        averageType = BitConverter.ToInt16(buffer, offset + 10);
        measureTime = BitConverter.ToUInt32(buffer, offset + 12);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct OptionalHeaderAudio
{
    [MarshalAs(UnmanagedType.I2)] public short audioMode;
    [MarshalAs(UnmanagedType.I2)] public short frameLen;
    [MarshalAs(UnmanagedType.U4)] public uint frequency;
    [MarshalAs(UnmanagedType.U4)] public uint bandwidth;
    [MarshalAs(UnmanagedType.U2)] public ushort demodulation;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string sDemodulation;

    public OptionalHeaderAudio(byte[] value, int offset)
    {
        audioMode = BitConverter.ToInt16(value, offset);
        frameLen = BitConverter.ToInt16(value, offset + 2);
        frequency = BitConverter.ToUInt32(value, offset + 4);
        bandwidth = BitConverter.ToUInt32(value, offset + 8);
        demodulation = BitConverter.ToUInt16(value, offset + 12);
        sDemodulation = BitConverter.ToString(value, offset + 14, 8);
    }
}