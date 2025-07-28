using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Magneto.Device.FTAS3500;

#region 上行数据定义

internal static class WrappedIq
{
    private const uint Sync = 0x000000ff;
    private const ushort Length = 16;
    private const ushort Minor = 12;
    private const ushort Major = 10;
    private const uint Sequence = 0;
    private const ushort Reserved = 0;
    private const ushort BodyType = 0;
    private const uint BodyLength = 32768;
    private const uint BodySampleRate = 320000;
    private const ulong BodySampleCount = 8192;
    private static readonly List<short[]> _iqData = new();
    private static ulong _frequency;
    private static uint _bandwidth;

    public static byte[] Wrap(ulong frequency, uint bandwidth, int[] iq)
    {
        var iq16 = Iq32BitTo16Bit(iq);
        return Wrap(frequency, bandwidth, iq16);
    }

    public static byte[] Wrap(ulong frequency, uint bandwidth, short[] iq)
    {
        if (iq == null) return null;
        if ((_frequency != 0 && frequency != _frequency) || (_bandwidth != 0 && bandwidth != _bandwidth))
            _iqData.Clear();
        _frequency = frequency;
        _bandwidth = bandwidth;
        var body = ToBodyBytes(frequency, bandwidth, iq);
        if (body == null) return null;
        var header = ToHeaderBytes();
        var wrappedBytes = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, wrappedBytes, 0, header.Length);
        Buffer.BlockCopy(body, 0, wrappedBytes, header.Length, body.Length);
        return wrappedBytes;
    }

    private static byte[] ToHeaderBytes()
    {
        var header = new List<byte[]>
        {
            BitConverter.GetBytes(Sync),
            BitConverter.GetBytes(Length),
            BitConverter.GetBytes(Minor),
            BitConverter.GetBytes(Major),
            BitConverter.GetBytes(Sequence + 1),
            BitConverter.GetBytes(Reserved)
        };
        var headerBytes = new byte[header.Sum(item => item.Length)];
        var offset = 0;
        foreach (var item in header)
        {
            Buffer.BlockCopy(item, 0, headerBytes, offset, item.Length);
            offset += item.Length;
        }

        return headerBytes;
    }

    private static byte[] ToBodyBytes(ulong frequency, uint bandwidth, short[] iq)
    {
        _iqData.Add(iq);
        var iqCount = _iqData.Sum(item => item.Length);
        if (iqCount / 2 < 8192) return null;
        var meta = new List<byte[]>
        {
            BitConverter.GetBytes(BodyType),
            BitConverter.GetBytes(BodyLength),
            BitConverter.GetBytes(BodySampleRate),
            BitConverter.GetBytes(frequency),
            BitConverter.GetBytes(bandwidth),
            BitConverter.GetBytes(BodySampleCount)
        };
        var offset = 0;
        var metaBytes = new byte[meta.Sum(item => item.Length)];
        foreach (var item in meta)
        {
            Buffer.BlockCopy(item, 0, metaBytes, offset, item.Length);
            offset += item.Length;
        }

        offset = 0;
        var iqBytes = new byte[iqCount * 2];
        foreach (var item in _iqData)
        {
            Buffer.BlockCopy(item, 0, iqBytes, offset, item.Length * sizeof(short));
            offset += item.Length * sizeof(short);
        }

        _iqData.Clear();
        var bodyBytes = new byte[metaBytes.Length + iqBytes.Length];
        Buffer.BlockCopy(metaBytes, 0, bodyBytes, 0, metaBytes.Length);
        Buffer.BlockCopy(iqBytes, 0, bodyBytes, metaBytes.Length, iqBytes.Length);
        return bodyBytes;
    }

    private static short[] Iq32BitTo16Bit(int[] iq32)
    {
        if (iq32 == null) return null;
        var maximum = iq32.Max();
        var minimum = iq32.Min();
        var scale = 1;
        if (maximum > 1000 || minimum < -1000)
        {
            var tempMaximum = maximum / 1000;
            var tempMinimum = minimum / -1000;
            scale = tempMaximum > tempMinimum ? tempMaximum : tempMinimum;
        }

        var iq16 = new short[iq32.Length];
        for (var index = 0; index < iq32.Length; ++index) iq16[index] = (short)(iq32[index] / scale);
        return iq16;
    }
}

#endregion

#region 下行数据定义

internal class RawPacket
{
    public uint Sync { get; private set; }
    public ushort Length { get; private set; }
    public ushort Minor { get; private set; }
    public ushort Major { get; private set; }
    public ushort Type { get; private set; }
    public uint Reserved { get; private set; }
    public List<RawData> DataCollection { get; private set; }

    public static RawPacket Parse(byte[] buffer, int offset)
    {
        var sync = BitConverter.ToUInt32(buffer, 0);
        var type = BitConverter.ToUInt16(buffer, 10);
        if (sync != 0xffffffff || type != 201) return null;
        var packet = new RawPacket { Sync = sync, Type = type };
        offset += 4;
        packet.Length = BitConverter.ToUInt16(buffer, offset);
        offset += 2;
        packet.Minor = BitConverter.ToUInt16(buffer, offset);
        offset += 2;
        packet.Major = BitConverter.ToUInt16(buffer, offset);
        offset += 4;
        packet.Reserved = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        packet.DataCollection = new List<RawData>();
        while (offset < buffer.Length)
        {
            var data = RawData.Parse(buffer, offset);
            if (data != null)
            {
                packet.DataCollection.Add(data);
                offset += data.Length;
            }
        }

        packet.DataCollection = packet.DataCollection.Where(item => item is not (null or RawOthers)).ToList();
        return packet;
    }
}

internal class RawData
{
    private static readonly List<KeyValuePair<ushort, string>> _protocolList = new()
    {
        new KeyValuePair<ushort, string>(7, "iden"),
        new KeyValuePair<ushort, string>(8, "C4FM,12.5kHz"),
        new KeyValuePair<ushort, string>(9, "C4FM,6.25kHz"),
        new KeyValuePair<ushort, string>(11, "FM"),
        new KeyValuePair<ushort, string>(12, "DMR,9.6kb/s,4FSK"),
        new KeyValuePair<ushort, string>(13, "PDT,9.6kb/s,4FSK"),
        new KeyValuePair<ushort, string>(14, "dPMR,4.8kb/s,4FSK"),
        new KeyValuePair<ushort, string>(15, "TETRA,36kb/s,4FSK"),
        new KeyValuePair<ushort, string>(16, "NXDN,9.6kb/s,4FSK"),
        new KeyValuePair<ushort, string>(17, "NXDN,4.8kb/s,4FSK"),
        new KeyValuePair<ushort, string>(19, "TETRA DMO,9kb/s,4FSK"),
        new KeyValuePair<ushort, string>(21, "260M DT,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(22, "269M DT,1.2kb/s,4FSK"),
        new KeyValuePair<ushort, string>(23, "CLOUD 5,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(24, "Apollo,9.6kb/s,4FSK"),
        new KeyValuePair<ushort, string>(25, "MST,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(26, "RS 2009,5.3kb/s,4FSK"),
        new KeyValuePair<ushort, string>(27, "RS watch/eraser,9.6kb/s,4FSK"),
        new KeyValuePair<ushort, string>(28, "RS eraser,4.9kb/s,4FSK"),
        new KeyValuePair<ushort, string>(29, "sunlips 5,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(30, "sunlips 1,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(31, "sunlips 2,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(32, "sunlips 4,2.4kb/s,4FSK"),
        new KeyValuePair<ushort, string>(33, "tk 1.25GHz,1.2kb/s,4FSK"),
        new KeyValuePair<ushort, string>(34, "southern lights,1.2kb/s,4FSK"),
        new KeyValuePair<ushort, string>(41, "lora,146b/s,4FSK")
    };

    public ushort Type { get; private set; }
    public int Length { get; private set; }
    public ushort SignalType { get; private set; }
    public uint Frequency { get; private set; }
    public short SignalLength { get; private set; }

    protected virtual int Convert(byte[] buffer, int offset)
    {
        Type = BitConverter.ToUInt16(buffer, offset);
        offset += 2;
        Length = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        SignalType = BitConverter.ToUInt16(buffer, offset);
        offset += 2;
        Frequency = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        SignalLength = BitConverter.ToInt16(buffer, offset);
        offset += 2;
        return offset;
    }

    public static RawData Parse(byte[] buffer, int offset)
    {
        RawData raw;
        var signalType = BitConverter.ToUInt16(buffer, offset + 6);
        if (signalType >= 23)
            raw = new RawFraud();
        else
            raw = signalType switch
            {
                // FM
                11 => new RawFm(),
                // DMR/PDT
                12 or 13 => new RawDmrpdt(),
                // dPMR
                14 or 18 => new RawDpmr(),
                // TETRA
                15 => new RawTetra(),
                // NXDN
                16 or 17 => new RawNxdn(),
                // TETRA DMO
                19 => new RawTetraDmo(),
                _ => new RawOthers()
            };
        raw.Convert(buffer, offset);
        return raw;
    }

    public static string GetProtocolByCode(ushort code)
    {
        return _protocolList.Find(item => item.Key == code).Value;
    }
}

internal class RawFm : RawData
{
    public int PayloadLength { get; private set; }
    public byte[] Audio { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        Array.Reverse(buffer, offset, 4);
        // PayloadLength = BitConverter.ToInt32(buffer, offset);
        PayloadLength = 1024; //BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Audio = new byte[PayloadLength];
        Buffer.BlockCopy(buffer, offset, Audio, 0, PayloadLength);
        offset += PayloadLength;
        return offset;
    }
}

internal class RawDmrpdt : RawData
{
    public uint Bsms { get; private set; }
    public uint Cc { get; private set; }
    public uint RxId { get; private set; }
    public uint TxId { get; private set; }
    public uint[] Reserved1 { get; private set; }
    public uint DataType { get; private set; }
    public uint[] Reserved2 { get; private set; }
    public int PayloadLength { get; private set; }
    public byte[] Audio { get; private set; }
    public string Message { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        Bsms = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Cc = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        RxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        TxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Reserved1 = new uint[4];
        Buffer.BlockCopy(buffer, offset, Reserved1, 0, sizeof(uint) * 4);
        offset += sizeof(uint) * 4;
        DataType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Reserved2 = new uint[3];
        Buffer.BlockCopy(buffer, offset, Reserved2, 0, sizeof(uint) * 3);
        offset += sizeof(uint) * 3;
        PayloadLength = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        if (Bsms is 0 or 2)
        {
            PayloadLength = 960;
            Audio = new byte[PayloadLength];
            Buffer.BlockCopy(buffer, offset, Audio, 0, PayloadLength);
        }
        else if (Bsms is 1 or 3)
        {
            Message = Encoding.Unicode.GetString(buffer, offset, PayloadLength);
        }

        offset += PayloadLength;
        return offset;
    }
}

internal class RawDpmr : RawData
{
    public uint Reserved1 { get; private set; }
    public uint Cc { get; private set; }
    public uint RxId { get; private set; }
    public uint TxId { get; private set; }
    public uint Mode { get; private set; }
    public uint Version { get; private set; }
    public uint Format { get; private set; }
    public uint DataType { get; private set; }
    public ulong Reserved2 { get; private set; }
    public int PayloadLength { get; private set; }
    public byte[] Audio { get; private set; }
    public string Message { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        Reserved1 = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Cc = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        RxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        TxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Mode = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Version = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Format = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        DataType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Reserved2 = BitConverter.ToUInt64(buffer, offset);
        offset += 8;
        PayloadLength = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        if (Mode is 0 or 1 && PayloadLength == 1280)
        {
            Audio = new byte[PayloadLength];
            Buffer.BlockCopy(buffer, offset, Audio, 0, PayloadLength);
        }
        else
        {
            Message = Encoding.Unicode.GetString(buffer, offset, PayloadLength);
        }

        offset += PayloadLength;
        return offset;
    }
}

internal class RawTetra : RawData
{
    public uint SignalFlag { get; private set; }
    public uint SystemCode { get; private set; }
    public uint ColorCode { get; private set; }
    public uint SlotNumber { get; private set; }
    public uint FrameNumber { get; private set; }
    public uint MultiFrameNumber { get; private set; }
    public uint SharingMode { get; private set; }
    public byte[] Reserved1 { get; private set; }
    public uint Mcc { get; private set; }
    public uint Mnc { get; private set; }
    public uint PduType { get; private set; }
    public uint BroadcastType { get; private set; }
    public uint MainCarrier { get; private set; }
    public uint FrequencyBand { get; private set; }
    public uint Offset { get; private set; }
    public uint DuplexSpacing { get; private set; }
    public uint ReverseOperation { get; private set; }
    public uint AttachedCtrlChannel { get; private set; }
    public byte[] Reserved2 { get; private set; }
    public uint Location { get; private set; }
    public uint AudioMultiFrameNumber { get; private set; }
    public uint AudioFrameNumber { get; private set; }
    public uint AudioSlotNumber { get; private set; }
    public int AudioLength { get; private set; }
    public byte[] Audio { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        SignalFlag = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        if (SignalFlag == 0)
        {
            SystemCode = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            ColorCode = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            SlotNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            FrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            MultiFrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            SharingMode = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Reserved1 = new byte[16];
            Buffer.BlockCopy(buffer, offset, Reserved1, 0, 16);
            offset += 16;
            Mcc = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Mnc = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            PduType = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            BroadcastType = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            MainCarrier = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            FrequencyBand = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Offset = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            DuplexSpacing = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            ReverseOperation = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AttachedCtrlChannel = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Reserved2 = new byte[48];
            Buffer.BlockCopy(buffer, offset, Reserved2, 0, 48);
            offset += 48;
            Location = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
        }
        else
        {
            AudioMultiFrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AudioFrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            AudioSlotNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            // AudioLength = BitConverter.ToInt32(buffer, offset);
            AudioLength = 960;
            offset += 4;
            Audio = new byte[AudioLength];
            Buffer.BlockCopy(buffer, offset, Audio, 0, AudioLength);
            offset += AudioLength;
        }

        return offset;
    }
}

internal class RawNxdn : RawData
{
    public uint Bsms { get; private set; }
    public uint DataType { get; private set; }
    public uint MessageType { get; private set; }
    public uint CallType { get; private set; }
    public byte[] Reserved1 { get; private set; }
    public uint RxId { get; private set; }
    public uint TxId { get; private set; }
    public byte[] Reserved2 { get; private set; }
    public int PayloadLength { get; private set; }
    public byte[] Audio { get; private set; }
    public string Message { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        Bsms = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        DataType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        MessageType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        CallType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Reserved1 = new byte[16];
        Buffer.BlockCopy(buffer, offset, Reserved1, 0, 16);
        offset += 16;
        RxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        TxId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        Reserved2 = new byte[20];
        Buffer.BlockCopy(buffer, offset, Reserved2, 0, 20);
        offset += 20;
        PayloadLength = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        if (DataType == 0x56 && PayloadLength == 1280)
        {
            Audio = new byte[PayloadLength];
            Buffer.BlockCopy(buffer, offset, Audio, 0, PayloadLength);
        }
        else
        {
            Message = Encoding.Unicode.GetString(buffer, 0, PayloadLength);
        }

        offset += PayloadLength;
        return offset;
    }
}

internal class RawTetraDmo : RawData
{
    public uint SignalFlag { get; private set; }
    public uint SystemCode { get; private set; }
    public uint Reserved1 { get; private set; }
    public uint CcType { get; private set; }
    public byte[] Reserved2 { get; private set; }
    public uint RxId { get; private set; }
    public uint Reserved3 { get; private set; }
    public uint TxId { get; private set; }
    public uint Mni { get; private set; }
    public uint MessageType { get; private set; }
    public uint MultiFrameNumber { get; private set; }
    public uint FrameNumber { get; private set; }
    public uint SlotNumber { get; private set; }
    public int AudioLength { get; private set; }
    public byte[] Audio { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        SignalFlag = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        if (SignalFlag == 0)
        {
            SystemCode = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Reserved1 = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            CcType = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Reserved2 = new byte[52];
            Buffer.BlockCopy(buffer, offset, Reserved2, 0, 52);
            offset += 52;
            RxId = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Reserved3 = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            TxId = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            Mni = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            MessageType = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
        }
        else
        {
            MultiFrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            FrameNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            SlotNumber = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            // AudioLength = BitConverter.ToInt32(buffer, offset);
            AudioLength = 960;
            offset += 4;
            Audio = new byte[AudioLength];
            Buffer.BlockCopy(buffer, offset, Audio, 0, AudioLength);
            offset += AudioLength;
        }

        return offset;
    }
}

internal class RawFraud : RawData
{
    public uint UserId { get; private set; }
    public uint ChannelId { get; private set; }
    public uint DataType { get; private set; }
    public int PayloadLength { get; private set; }
    public string Message { get; private set; }

    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        UserId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        ChannelId = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        DataType = BitConverter.ToUInt32(buffer, offset);
        offset += 4;
        PayloadLength = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Message = Encoding.Unicode.GetString(buffer, offset, PayloadLength);
        offset += PayloadLength;
        return offset;
    }
}

internal class RawOthers : RawData
{
    protected override int Convert(byte[] buffer, int offset)
    {
        offset = base.Convert(buffer, offset);
        offset += SignalLength;
        return offset;
    }
}

#endregion