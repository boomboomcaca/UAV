using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magneto.Device.EB200;

#region 协议头

internal interface IRawHeader
{
    int DataLength { get; }
    int Convert(byte[] value, int offset);
}

internal class Eb200Header : IRawHeader
{
    public uint MagicNumber { get; private set; }
    public ushort VersionMinor { get; private set; }
    public ushort VersionMajor { get; private set; }
    public ushort Sequence { get; private set; }
    public short Reserved0 { get; private set; }
    public short Reserved1 { get; private set; }
    public short Reserved2 { get; private set; }
    public int DataLength => 16;

    public int Convert(byte[] value, int offset)
    {
        MagicNumber = BitConverter.ToUInt32(value, offset);
        offset += 4;
        VersionMinor = BitConverter.ToUInt16(value, offset);
        offset += 2;
        VersionMajor = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Sequence = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Reserved0 = BitConverter.ToInt16(value, offset);
        offset += 2;
        Reserved1 = BitConverter.ToInt16(value, offset);
        offset += 2;
        Reserved2 = BitConverter.ToInt16(value, offset);
        offset += 2;
        return offset;
    }
}

internal class GenericAttributeHeader : IRawHeader
{
    public ushort Tag { get; protected set; }
    public ushort Length { get; protected set; }
    public int DataLength => 4;

    public int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        Tag = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Array.Reverse(value, offset, 2);
        Length = BitConverter.ToUInt16(value, offset);
        offset += 2;
        return offset;
    }
}

internal class TraceAttributeHeader : IRawHeader
{
    public short NumberOfTraceItems { get; protected set; }
    public sbyte ChannelNumber { get; protected set; }
    public byte OptionalHeaderLength { get; protected set; }
    public uint SelectorFlags { get; protected set; }
    public int DataLength => 8;

    public int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        NumberOfTraceItems = BitConverter.ToInt16(value, offset);
        offset += 2;
        ChannelNumber = (sbyte)value[offset];
        offset++;
        OptionalHeaderLength = value[offset];
        offset++;
        Array.Reverse(value, offset, 4);
        SelectorFlags = BitConverter.ToUInt32(value, offset);
        offset += 4;
        return offset;
    }
}

#endregion

#region 数据协议

[Serializable]
internal sealed class RawPacket
{
    private RawPacket()
    {
    }

    public Eb200Header Header { get; private set; }
    public List<RawData> DataCollection { get; private set; }

    public static RawPacket Parse(byte[] value, int offset)
    {
        try
        {
            var eb200Header = new Eb200Header();
            offset = eb200Header.Convert(value, offset);
            var packet = new RawPacket
            {
                Header = eb200Header,
                DataCollection = new List<RawData>()
            };
            while (offset < value.Length)
            {
                var data = RawData.Parse(value, offset);
                if (data != null)
                {
                    packet.DataCollection.Add(data);
                    offset += 4;
                    offset += data.Generic.Length;
                }
            }

            return packet;
        }
        catch (ArgumentException e)
        {
#if DEBUG
            Trace.WriteLine(e.ToString());
#endif
            return null;
        }
    }
}

/// <summary>
///     数据基类
/// </summary>
[Serializable]
internal class RawData
{
    public GenericAttributeHeader Generic { get; private set; }
    public TraceAttributeHeader Trace { get; private set; }

    public static RawData Parse(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        var tag = (DataType)BitConverter.ToUInt16(value, offset);
        Array.Reverse(value, offset, 2);
        var raw = tag switch
        {
            DataType.Ifpan => new RawIfPan(),
            DataType.Dscan => new RawDScan(),
            DataType.Audio => new RawAudio(),
            // FSCAN
            DataType.Fscan => new RawFScan(),
            // MScan
            DataType.Mscan => new RawMScan(),
            _ => new RawData()
        };
        raw.Convert(value, offset);
        return raw;
    }

    public virtual int Convert(byte[] value, int offset)
    {
        var generic = new GenericAttributeHeader();
        offset = generic.Convert(value, offset);
        var trace = new TraceAttributeHeader();
        offset = trace.Convert(value, offset);
        Generic = generic;
        Trace = trace;
        return offset;
    }

    public override string ToString()
    {
        return $"tag=\"{Generic.Tag}\"";
    }
}

[Serializable]
internal class RawFScan : RawData
{
    public short CycleCount { get; private set; }
    public short HoldTime { get; private set; }
    public short DwellTime { get; private set; }
    public short DirectionUp { get; private set; }
    public short StopSignal { get; private set; }
    public uint StartFrequencyLow { get; private set; }
    public uint StopFrequencyLow { get; private set; }
    public uint StepFrequency { get; private set; }
    public uint StartFrequencyHigh { get; private set; }
    public uint StopFrequencyHigh { get; private set; }
    public ushort Reserved { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        CycleCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        HoldTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        DwellTime = BitConverter.ToInt16(value, offset + 4);
        offset += 2;
        DirectionUp = BitConverter.ToInt16(value, offset + 6);
        offset += 2;
        StopSignal = BitConverter.ToInt16(value, offset + 8);
        offset += 2;
        StartFrequencyLow = BitConverter.ToUInt32(value, offset + 10);
        offset += 4;
        StopFrequencyLow = BitConverter.ToUInt32(value, offset + 14);
        offset += 4;
        StepFrequency = BitConverter.ToUInt32(value, offset + 18);
        offset += 4;
        StartFrequencyHigh = BitConverter.ToUInt32(value, offset + 22);
        offset += 4;
        StopFrequencyHigh = BitConverter.ToUInt32(value, offset + 26);
        offset += 4;
        Reserved = BitConverter.ToUInt16(value, offset + 30);
        offset += 2;
        OutputTimestamp = BitConverter.ToUInt64(value, offset + 32);
        //offset += 8;
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((Trace.SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 2);
                        offset += numberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, numberOfTraceItems * 4);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}

[Serializable]
internal class RawMScan : RawData
{
    public short CycleCount { get; private set; }
    public short HoldTime { get; private set; }
    public short DwellTime { get; private set; }
    public short DirectionUp { get; private set; }
    public short StopSignal { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        CycleCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        HoldTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        DwellTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        DirectionUp = BitConverter.ToInt16(value, offset);
        offset += 2;
        StopSignal = BitConverter.ToInt16(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((Trace.SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 2);
                        offset += numberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, numberOfTraceItems * 4);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}

[Serializable]
internal class RawDScan : RawData
{
    public uint StartFrequency { get; private set; }
    public uint StopFrequency { get; private set; }
    public uint StepFrequency { get; private set; }
    public uint MarkFrequency { get; private set; }
    public short BwZoom { get; private set; }
    public short ReferenceLevel { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        StartFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        MarkFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        BwZoom = BitConverter.ToInt16(value, offset);
        offset += 2;
        ReferenceLevel = BitConverter.ToInt16(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((Trace.SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 2);
                        offset += numberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, numberOfTraceItems * 4);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}

/// <summary>
///     optional_header_length is thus either 0 or 42.
/// </summary>
[Serializable]
internal class RawAudio : RawData
{
    public short AudioMode { get; private set; }
    public short FrameLen { get; private set; }
    public uint FrequencyLow { get; private set; }
    public uint Bandwidth { get; private set; }
    public ushort Demodulation { get; private set; }
    public byte[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        AudioMode = BitConverter.ToInt16(value, offset);
        offset += 2;
        FrameLen = BitConverter.ToInt16(value, offset);
        offset += 2;
        FrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Bandwidth = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Demodulation = BitConverter.ToUInt16(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        Consts.AfModes.TryGetValue(AudioMode, out var afMode);
        var len = afMode?.LengthPerFrame ?? 2;
        var count = numberOfTraceItems * len;
        var dataCollection = new byte[count]; // 从接收机到达的数据是16位的字节序列
        Buffer.BlockCopy(value, offset, dataCollection, 0, count);
        DataCollection = dataCollection;
        offset += count;
        return offset;
    }
}

/// <summary>
///     optional_header_length is thus either 0 or 60.
/// </summary>
[Serializable]
internal class RawIfPan : RawData
{
    public uint Frequency { get; private set; }
    public uint SpanFrequency { get; private set; }
    public short AverageTime { get; private set; }
    public short AverageType { get; private set; }
    public uint MeasureTime { get; private set; }
    public short[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        Frequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        SpanFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        AverageTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        AverageType = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureTime = BitConverter.ToUInt32(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        DataCollection = new short[numberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 2);
        offset += numberOfTraceItems * 2;
        return offset;
    }
}

#endregion