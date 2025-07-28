using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Magneto.Contract;
using Magneto.Protocol.Data;

namespace Magneto.Device;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal class PacketHeader
{
    [MarshalAs(UnmanagedType.I1)] public readonly byte MessageID;

    [MarshalAs(UnmanagedType.I1)] public readonly byte PayloadLength;
    [MarshalAs(UnmanagedType.I1)] public byte Component;

    [MarshalAs(UnmanagedType.I1)] public byte Fe;

    [MarshalAs(UnmanagedType.I1)] public byte PacketSequence;

    [MarshalAs(UnmanagedType.I1)] public byte SystemID;

    public PacketHeader(byte[] value, int startIndex)
    {
        Fe = value[startIndex];
        PayloadLength = value[startIndex + 1];
        PacketSequence = value[startIndex + 2];
        SystemID = value[startIndex + 3];
        Component = value[startIndex + 4];
        MessageID = value[startIndex + 5];
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal class TrafficReportMessage
{
    [MarshalAs(UnmanagedType.I4)] public readonly int Altitude;

    [MarshalAs(UnmanagedType.U1)] public readonly byte AltitudeType;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
    public readonly string CallSign;

    [MarshalAs(UnmanagedType.U2)] public readonly ushort Heading;

    [MarshalAs(UnmanagedType.U2)] public readonly ushort HorVelocity;

    [MarshalAs(UnmanagedType.U4)] public readonly uint IcaoAddress;

    [MarshalAs(UnmanagedType.I4)] public readonly int Lat;

    [MarshalAs(UnmanagedType.I4)] public readonly int Lon;

    [MarshalAs(UnmanagedType.U2)] public readonly ushort Squawk;

    [MarshalAs(UnmanagedType.U2)] public readonly ushort ValidFlags;

    [MarshalAs(UnmanagedType.I2)] public readonly short VerVelocity;

    [MarshalAs(UnmanagedType.U1)] public byte EmitterType;

    [MarshalAs(UnmanagedType.U1)] public byte Tslc;

    public TrafficReportMessage(byte[] value, int startIndex)
    {
        IcaoAddress = BitConverter.ToUInt32(value, startIndex);
        Lat = BitConverter.ToInt32(value, startIndex + 4);
        Lon = BitConverter.ToInt32(value, startIndex + 8);
        Altitude = BitConverter.ToInt32(value, startIndex + 12);
        Heading = BitConverter.ToUInt16(value, startIndex + 16);
        HorVelocity = BitConverter.ToUInt16(value, startIndex + 18);
        VerVelocity = BitConverter.ToInt16(value, startIndex + 20);
        ValidFlags = BitConverter.ToUInt16(value, startIndex + 22);
        Squawk = BitConverter.ToUInt16(value, startIndex + 24);
        AltitudeType = value[26];
        CallSign = Encoding.ASCII.GetString(value, startIndex + 27, 9).TrimEnd('\0');
        EmitterType = value[36];
        Tslc = value[37];
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal class OwnshipMessage
{
    [MarshalAs(UnmanagedType.U4)] public uint AccHoriz;

    [MarshalAs(UnmanagedType.U2)] public ushort AccVel;

    [MarshalAs(UnmanagedType.U2)] public ushort AccVert;

    [MarshalAs(UnmanagedType.I4)] public int AltGnss;

    [MarshalAs(UnmanagedType.I4)] public int AltPres;

    [MarshalAs(UnmanagedType.U1)] public byte Control;

    [MarshalAs(UnmanagedType.U1)] public byte EmStatus;

    [MarshalAs(UnmanagedType.I2)] public short EwVog;

    [MarshalAs(UnmanagedType.U1)] public byte FixType;

    [MarshalAs(UnmanagedType.I4)] public int Latitude;

    [MarshalAs(UnmanagedType.I4)] public int Longitude;

    [MarshalAs(UnmanagedType.I2)] public short NsVog;

    [MarshalAs(UnmanagedType.U1)] public byte NumSats;

    [MarshalAs(UnmanagedType.U2)] public ushort Squawk;

    [MarshalAs(UnmanagedType.U2)] public ushort State;

    [MarshalAs(UnmanagedType.U4)] public uint UtcTime;

    [MarshalAs(UnmanagedType.I2)] public short VelVert;

    public OwnshipMessage(byte[] value, int startIndex)
    {
        UtcTime = BitConverter.ToUInt32(value, startIndex);
        Latitude = BitConverter.ToInt32(value, startIndex + 4);
        Longitude = BitConverter.ToInt32(value, startIndex + 8);
        AltPres = BitConverter.ToInt32(value, startIndex + 12);
        AltGnss = BitConverter.ToInt32(value, startIndex + 16);
        AccHoriz = BitConverter.ToUInt32(value, startIndex + 20);
        AccVert = BitConverter.ToUInt16(value, startIndex + 24);
        AccVel = BitConverter.ToUInt16(value, startIndex + 26);
        VelVert = BitConverter.ToInt16(value, startIndex + 28);
        NsVog = BitConverter.ToInt16(value, startIndex + 30);
        EwVog = BitConverter.ToInt16(value, startIndex + 32);
        State = BitConverter.ToUInt16(value, startIndex + 34);
        Squawk = BitConverter.ToUInt16(value, startIndex + 36);
        FixType = value[startIndex + 38];
        NumSats = value[startIndex + 39];
        EmStatus = value[startIndex + 40];
        Control = value[startIndex + 41];
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal class StatusMessage
{
    [MarshalAs(UnmanagedType.U1)] public byte Status;

    public StatusMessage(byte[] value, int startIndex)
    {
        Status = value[startIndex];
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal class MessageHeader
{
    [MarshalAs(UnmanagedType.I1)] public byte CrcExtra;

    [MarshalAs(UnmanagedType.I1)] public byte MessageID;

    [MarshalAs(UnmanagedType.I1)] public byte PayloadLength;

    public MessageHeader(byte[] value, int startIndex)
    {
        MessageID = value[startIndex];
        PayloadLength = value[startIndex + 1];
        CrcExtra = value[startIndex + 2];
    }
}

internal enum MessageId
{
    DataStreamRequestMessagId = 66,
    DynamicMessageId = 202,
    StatusMessageId = 203,
    TrafficeReportMessageId = 246
}

public partial class AdsBUsb
{
    private FlightInfo? ParseAircraft(byte[] buffer)
    {
        var trafficreportMessage = new TrafficReportMessage(buffer, 0);
#if DEBUG
        Trace.WriteLine("trafficreportMessage=>" + Utils.ConvertToJson(trafficreportMessage));
#endif
        //alitutdeType 0x00: PRESSURE_ALTITUDE (AMSL, QNH)   0x01: GEOMETRIC(GNSS, WGS84) | flags 0x0001: LATLON_VALID
        if (trafficreportMessage.AltitudeType != 0x01 || (trafficreportMessage.ValidFlags & 0x0001) <= 0) return null;
        return new FlightInfo
        {
            PlaneAddress = trafficreportMessage.IcaoAddress.ToString("X6"),
            Altitude = (double)trafficreportMessage.Altitude / 1000,
            TransponderCode = trafficreportMessage.Squawk.ToString(), // SQUAWK of aircraft 为应答机编码
            Latitude = (double)trafficreportMessage.Lat / 10000000,
            Longitude = (double)trafficreportMessage.Lon / 10000000,
            Azimuth = (double)trafficreportMessage.Heading / 100,
            HorizontalSpeed = Math.Round((double)trafficreportMessage.HorVelocity / 100 * 3.6, 3), // m/s转换为km/h
            VerticalSpeed = (double)trafficreportMessage.VerVelocity / 100,
            UpdateTime = Utils.GetNowTimestamp(),
            Model = string.Empty,
            Age = 0,
            Country = "",
            FlightNumber = trafficreportMessage.CallSign
        };
    }
}