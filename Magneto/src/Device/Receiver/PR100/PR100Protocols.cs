using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magneto.Device.PR100;

[Serializable]
internal sealed class RawPacket
{
    private RawPacket()
    {
    }

    public uint MagicNumber { get; private set; }
    public ushort VersionMinor { get; private set; }
    public ushort VersionMajor { get; private set; }
    public ushort Sequence { get; private set; }
    public short Reserved0 { get; private set; }
    public short Reserved1 { get; private set; }
    public short Reserved2 { get; private set; }
    public List<RawData> DataCollection { get; private set; }

    public static RawPacket Parse(byte[] value, int offset)
    {
        try
        {
            var magicNumber = BitConverter.ToUInt32(value, offset);
            offset += 4;
            var versionMinor = BitConverter.ToUInt16(value, offset);
            offset += 2;
            var versionMajor = BitConverter.ToUInt16(value, offset);
            offset += 2;
            var sequence = BitConverter.ToUInt16(value, offset);
            offset += 2;
            var reserved0 = BitConverter.ToInt16(value, offset);
            offset += 2;
            var reserved1 = BitConverter.ToInt16(value, offset);
            offset += 2;
            var reserved2 = BitConverter.ToInt16(value, offset);
            offset += 4;
            var packet = new RawPacket
            {
                MagicNumber = magicNumber,
                VersionMajor = versionMajor,
                VersionMinor = versionMinor,
                Sequence = sequence,
                Reserved0 = reserved0,
                Reserved1 = reserved1,
                Reserved2 = reserved2,
                DataCollection = new List<RawData>()
            };
            while (offset < value.Length)
            {
                var data = RawData.Parse(value, offset);
                if (data != null)
                {
                    packet.DataCollection.Add(data);
                    offset += data.Tag >= 5000 ? 8 : 4;
                    offset += data.Length;
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
    public ushort Tag { get; private set; }
    public ushort Length { get; private set; }
    public short NumberOfTraceItems { get; private set; }
    public byte ChannelNumber { get; private set; }
    public byte OptionalHeaderLength { get; private set; }
    public uint SelectorFlags { get; private set; }

    public static RawData Parse(byte[] value, int offset)
    {
        RawData raw;
        Array.Reverse(value, offset, 2);
        var tag = (DataType)BitConverter.ToUInt16(value, offset);
        Array.Reverse(value, offset, 2);
        switch (tag)
        {
            case DataType.If:
                raw = new RawIf();
                break;
            case DataType.Ifpan:
                raw = new RawIfPan();
                break;
            case DataType.Audio:
                raw = new RawAudio();
                break;
            case DataType.Pscan: // PSCAN
                raw = new RawPScan();
                break;
            case DataType.Fscan: // FSCAN
                raw = new RawFScan();
                break;
            case DataType.Mscan: // MScan
                raw = new RawMScan();
                break;
            default:
                raw = new RawData();
                break;
        }

        raw.Convert(value, offset);
        return raw;
    }

    public virtual int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        Tag = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Array.Reverse(value, offset, 2);
        Length = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Array.Reverse(value, offset, 2);
        NumberOfTraceItems = BitConverter.ToInt16(value, offset);
        offset += 2;
        ChannelNumber = value[offset];
        offset++;
        OptionalHeaderLength = value[offset];
        offset++;
        Array.Reverse(value, offset, 4);
        SelectorFlags = BitConverter.ToUInt32(value, offset);
        offset += 4;
        return offset;
    }

    public override string ToString()
    {
        return $"tag=\"{Tag}\"";
    }
}

[Serializable]
internal class RawIf : RawData
{
    public short Mode { get; private set; }
    public short FrameLen { get; private set; }
    public uint Samplerate { get; private set; }
    public uint Frequency { get; private set; }

    /// <summary>
    ///     IF bandwidth
    /// </summary>
    public uint Bandwidth { get; private set; }

    public ushort Demodulation { get; private set; }
    public short RxAtt { get; private set; }
    public ushort Flags { get; private set; }
    public short IfReserved { get; private set; }
    public string DemodulationString { get; private set; }
    public ulong SampleCount { get; private set; }
    public ulong FrequencyHigh { get; private set; }
    public int[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        Mode = BitConverter.ToInt16(value, offset);
        offset += 2;
        FrameLen = BitConverter.ToInt16(value, offset);
        offset += 2;
        Samplerate = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Frequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Bandwidth = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Demodulation = BitConverter.ToUInt16(value, offset);
        offset += 2;
        RxAtt = BitConverter.ToInt16(value, offset);
        offset += 2;
        Flags = BitConverter.ToUInt16(value, offset);
        offset += 2;
        IfReserved = BitConverter.ToInt16(value, offset);
        offset += 2;
        DemodulationString = BitConverter.ToString(value, offset, 8);
        offset += 8;
        SampleCount = BitConverter.ToUInt64(value, offset);
        offset += 8;
        FrequencyHigh = BitConverter.ToUInt64(value, offset);
        offset = startIndex + OptionalHeaderLength;
        if (Mode == 1)
        {
            var temp = new short[NumberOfTraceItems * 2];
            Buffer.BlockCopy(value, offset, temp, 0, NumberOfTraceItems * 4);
            DataCollection = Array.ConvertAll<short, int>(temp, item => item);
            offset += NumberOfTraceItems * 4;
        }
        else if (Mode == 2)
        {
            DataCollection = new int[NumberOfTraceItems * 2];
            Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 8);
            offset += NumberOfTraceItems * 8;
        }

        return offset;
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
    public uint StartFrequency { get; private set; }
    public uint StopFrequency { get; private set; }
    public uint StepFrequency { get; private set; }
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
        offset += 2;
        StartFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequency = BitConverter.ToUInt32(value, offset);
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
                        offset += NumberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, NumberOfTraceItems * 4);
                        offset += 4 * NumberOfTraceItems;
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
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
                        offset += NumberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, NumberOfTraceItems * 4);
                        offset += 4 * NumberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}

[Serializable]
internal class RawPScan : RawData
{
    public uint StartFrequency { get; private set; }
    public uint StopFrequency { get; private set; }
    public uint StepFrequency { get; private set; }
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
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.Siggtsqu)
        {
            if ((SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.Level:
                    {
                        DataCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
                        offset += NumberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Frequency:
                    {
                        FreqCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqCollection, 0, NumberOfTraceItems * 4);
                        offset += 4 * NumberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}

[Serializable]
internal class RawIfPan : RawData
{
    public uint Frequency { get; private set; }
    public uint SpanFrequency { get; private set; }
    public short IfPanReserved { get; private set; }
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
        IfPanReserved = BitConverter.ToInt16(value, offset);
        offset += 2;
        AverageType = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureTime = BitConverter.ToUInt32(value, offset);
        offset = startIndex + OptionalHeaderLength;
        DataCollection = new short[NumberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
        offset += NumberOfTraceItems * 2;
        return offset;
    }
}

[Serializable]
internal class RawAudio : RawData
{
    public byte[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        offset = startIndex + OptionalHeaderLength;
        var len = 2;
        var count = NumberOfTraceItems * len;
        var dataCollection = new byte[count]; // 从接收机到达的数据是16位的字节序列
        Buffer.BlockCopy(value, offset, dataCollection, 0, count);
        DataCollection = dataCollection;
        offset += count;
        return offset;
    }
}