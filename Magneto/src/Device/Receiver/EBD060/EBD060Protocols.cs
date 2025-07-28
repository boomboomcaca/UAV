using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.EBD060;

[Flags]
internal enum Flags : uint
{
    /// <summary>
    ///     同步数据帧（00000000）
    /// </summary>
    SyncMsg = 0x00,

    /// <summary>
    ///     异步数据帧（00000001）
    /// </summary>
    AsyncMsg = 0x01,

    /// <summary>
    ///     单频开始消息，FFM消息类型（10000001，异步）
    /// </summary>
    StartFfm = 0x81,

    /// <summary>
    ///     单频结束消息，FFM消息类型（10000011，异步）
    /// </summary>
    StopFfm = 0x83,

    /// <summary>
    ///     单频消息，FFM消息类型（10000101，异步）
    /// </summary>
    Ffm = 0x85,

    /// <summary>
    ///     频谱消息，FFM消息类型（10000111，异步）
    /// </summary>
    Spectrum = 0x87,

    /// <summary>
    ///     音频消息，FFM消息类型（10001001，异步）
    /// </summary>
    Audio = 0x89,

    /// <summary>
    ///     搜索开始消息，Search消息类型（10100001，异步）
    /// </summary>
    StartSearch = 0xA1,

    /// <summary>
    ///     搜索结束消息，Search消息类型（10100011，异步）
    /// </summary>
    StopSearch = 0xA3,

    /// <summary>
    ///     搜索消息，Search消息类型（10100101，异步）
    /// </summary>
    Search = 0xA5,

    /// <summary>
    ///     扫描开始消息，Scan消息类型（11000001，异步）
    /// </summary>
    StartScan = 0xC1,

    /// <summary>
    ///     扫描结束消息，Scan消息类型（11000011，异步）
    /// </summary>
    StopScan = 0xC3,
    ScanCont = 0xC5,

    /// <summary>
    ///     扫频开始消息，Scan消息类型（11000111，异步）
    /// </summary>
    StartSweep = 0xC7,

    /// <summary>
    ///     扫频结束消息，Scan消息类型（11001001，异步）
    /// </summary>
    StopSweep = 0xC9,

    /// <summary>
    ///     扫描消息，Scan消息类型（11001011，异步）
    /// </summary>
    Scan = 0xCB,
    SpectrumC0 = 0xCD,
    SpectrumCe = 0xCF,
    Hop = 0xD1,

    /// <summary>
    ///     TDMA消息（11010011，异步）
    /// </summary>
    Tdma = 0xD3,

    /// <summary>
    ///     测试开始消息，Test消息类型（11100001，异步）
    /// </summary>
    StartTest = 0xE1,

    /// <summary>
    ///     测试结束消息，Test消息类型（11100011，异步）
    /// </summary>
    StopTest = 0xE3,
    Correlation = 0xE5,

    /// <summary>
    ///     测试确认消息，Test消息类型（11100110，同步）
    /// </summary>
    TestAck = 0xE6,

    /// <summary>
    ///     GPS消息，Test消息类型(11101001，异步）
    /// </summary>
    Gps = 0xE9,

    /// <summary>
    ///     其它同步掩码消息（11110000）
    /// </summary>
    SyncMask = 0xF0,

    /// <summary>
    ///     其它异步掩码消息（11110001）
    /// </summary>
    AsyncMask = 0xF1
}

internal enum TcpExt
{
    SetReceiveData = 0x20,
    ReleaseReceiveData = 0x21,
    Send = 0x22,
    Receive = 0x23,
    Compression = 0x24,
    RegQueryValue = 0x25,
    RegQueryTree = 0x26,
    RegSetValue = 0x27
}

internal enum LdDdf
{
    Close = 0,
    Time = 1,
    Ping = 2,
    Audioformat = 3,
    Audio = 4,
    Data = 5,
    Write = 6,
    Boot = 7,
    Reset = 8,
    Window = 9
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct MessageHeader
{
    [MarshalAs(UnmanagedType.I2)] public readonly short Type;
    [MarshalAs(UnmanagedType.I2)] public readonly short Length;

    public MessageHeader(byte[] buffer, int offset)
    {
        Type = BitConverter.ToInt16(buffer, offset);
        Length = BitConverter.ToInt16(buffer, offset + 2);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct Ebd060Header
{
    [MarshalAs(UnmanagedType.U1)] public readonly byte Modifier;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Ticks;
    [MarshalAs(UnmanagedType.I2)] public readonly short Type;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Total;

    public Ebd060Header(byte[] buffer, int offset)
    {
        Modifier = buffer[offset];
        Ticks = BitConverter.ToUInt32(buffer, offset + 1);
        Type = BitConverter.ToInt16(buffer, offset + 5);
        Total = BitConverter.ToUInt16(buffer, offset + 7);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct FfmMessage
{
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Len;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Type;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort ChannelNo;
    [MarshalAs(UnmanagedType.U1)] public readonly byte Status;
    [MarshalAs(UnmanagedType.U1)] public readonly byte SIG;
    [MarshalAs(UnmanagedType.I4)] public readonly int TimeCode;
    [MarshalAs(UnmanagedType.I2)] public readonly short Feldstarke;
    [MarshalAs(UnmanagedType.I2)] public readonly short Level;
    [MarshalAs(UnmanagedType.I2)] public readonly short AzimuthVariance;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Azimuth;
    [MarshalAs(UnmanagedType.I2)] public readonly short Quality;
    [MarshalAs(UnmanagedType.I2)] public readonly short Reserved;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort Compass; // NMEA
    [MarshalAs(UnmanagedType.I2)] public readonly short Elevation;

    public FfmMessage(byte[] buffer, ref int startIndex)
    {
        Len = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Type = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        ChannelNo = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Status = buffer[startIndex];
        startIndex += 1;
        SIG = buffer[startIndex];
        startIndex += 1;
        TimeCode = BitConverter.ToInt32(buffer, startIndex);
        startIndex += 4;
        Feldstarke = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
        Level = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
        AzimuthVariance = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
        Azimuth = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Quality = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
        Reserved = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
        Compass = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Elevation = BitConverter.ToInt16(buffer, startIndex);
        startIndex += 2;
    }
}

internal struct IfSpetrumMessage
{
    [MarshalAs(UnmanagedType.U2)] public ushort Len;
    [MarshalAs(UnmanagedType.U2)] public ushort Type;
    [MarshalAs(UnmanagedType.U2)] public ushort ChannelNo;
    [MarshalAs(UnmanagedType.U1)] public byte Status;
    [MarshalAs(UnmanagedType.U1)] public readonly byte Length;
    public readonly byte[] Data;

    public IfSpetrumMessage(byte[] buffer, ref int startIndex)
    {
        Len = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Type = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        ChannelNo = BitConverter.ToUInt16(buffer, startIndex);
        startIndex += 2;
        Status = buffer[startIndex];
        startIndex += 1;
        Length = buffer[startIndex];
        startIndex += 1;
        Data = new byte[Length];
        Array.Copy(buffer, startIndex, Data, 0, Length);
        startIndex += Length;
    }
}