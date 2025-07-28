using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magneto.Device.ESMD;

#region 数据协议

[Serializable]
internal sealed class RawPacket
{
    private RawPacket()
    {
    }

    public uint MagicNumber { get; private set; }
    public ushort VersionMinor { get; private set; }
    public ushort VersionMajor { get; private set; }
    public ushort SequenceNumberLow { get; private set; }
    public ushort SequenceNumberHigh { get; private set; }
    public uint DataSize { get; private set; }
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
            var sequenceNumberLow = BitConverter.ToUInt16(value, offset);
            offset += 2;
            var sequenceNumberHigh = BitConverter.ToUInt16(value, offset);
            offset += 2;
            var dataSize = BitConverter.ToUInt32(value, offset);
            offset += 4;
            var packet = new RawPacket
            {
                MagicNumber = magicNumber,
                VersionMajor = versionMajor,
                VersionMinor = versionMinor,
                SequenceNumberLow = sequenceNumberLow,
                SequenceNumberHigh = sequenceNumberHigh,
                DataSize = dataSize,
                DataCollection = new List<RawData>()
            };
            while (offset < value.Length)
            {
                var data = RawData.Parse(value, offset);
                if (data != null)
                {
                    packet.DataCollection.Add(data);
                    offset += data.Tag >= 5000 ? 8 : 4;
                    offset += (int)data.Length;
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
    public uint Length { get; private set; }
    public ushort Reserved { get; private set; }
    public uint Reserved0 { get; private set; }
    public uint Reserved1 { get; private set; }
    public uint Reserved2 { get; private set; }
    public uint Reserved3 { get; private set; }
    public int NumberOfTraceItems { get; private set; }
    public uint ChannelNumber { get; private set; }
    public int OptionalHeaderLength { get; private set; }
    public uint SelectorFlags { get; private set; }

    public static RawData Parse(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        var tag = (DataType)BitConverter.ToUInt16(value, offset);
        Array.Reverse(value, offset, 2);
        var raw = tag switch
        {
            DataType.If => new RawIf(),
            DataType.Ifpan => new RawIfPan(),
            DataType.Vdpan => new RawVdPan(),
            DataType.Audio => new RawAudio(),
            // PSCAN
            DataType.Pscan => new RawPScan(),
            // FSCAN
            DataType.Fscan => new RawFScan(),
            // MScan
            DataType.MScan => new RawMScan(),
            _ => new RawData()
        };
        raw.Convert(value, offset);
        return raw;
    }

    public virtual int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        Tag = BitConverter.ToUInt16(value, offset);
        offset += 2;
        if (Tag >= 5000)
        {
            Array.Reverse(value, offset, 2);
            Reserved = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            Length = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Reserved0 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Reserved1 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Reserved2 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Reserved3 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            NumberOfTraceItems = (int)BitConverter.ToUInt32(value, offset);
            offset += 4;
            ChannelNumber = BitConverter.ToUInt32(value, offset);
            offset += 4;
            OptionalHeaderLength = (int)BitConverter.ToUInt32(value, offset);
            offset += 4;
            offset += 4;
            offset += 4;
        }
        else
        {
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
        }

        return offset;
    }

    public override string ToString()
    {
        return $"tag=\"{Tag}\"";
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
    public ushort ChildReserved { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqLowCollection { get; private set; }
    public uint[] FreqHighCollection { get; private set; }

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
        ChildReserved = BitConverter.ToUInt16(value, offset + 30);
        offset += 2;
        OutputTimestamp = BitConverter.ToUInt64(value, offset + 32);
        //offset += 8;
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.OptionalHeader)
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
                    case Flags.Freqlow:
                    {
                        FreqLowCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqLowCollection, 0, NumberOfTraceItems * 4);
                        offset += 4 * NumberOfTraceItems;
                        break;
                    }
                    case Flags.Freqhigh:
                    {
                        FreqHighCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqHighCollection, 0, NumberOfTraceItems * 4);
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
    public byte[] ChildReserved { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public ulong StartFrequency { get; private set; }
    public ulong StopFrequency { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqLowCollection { get; private set; }
    public uint[] FreqHighCollection { get; private set; }

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
        ChildReserved = new byte[6];
        offset += 6;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        StartFrequency = BitConverter.ToUInt64(value, offset);
        offset += 8;
        StopFrequency = BitConverter.ToUInt64(value, offset);
        //offset += 8;
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.OptionalHeader)
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
                    case Flags.Freqlow:
                    {
                        FreqLowCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqLowCollection, 0, NumberOfTraceItems * 4);
                        offset += 4 * NumberOfTraceItems;
                        break;
                    }
                    case Flags.Freqhigh:
                    {
                        FreqHighCollection = new uint[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqHighCollection, 0, NumberOfTraceItems * 4);
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
    public uint StartFrequencyLow { get; private set; }
    public uint StopFrequencyLow { get; private set; }
    public uint StepFrequency { get; private set; }
    public uint StartFrequencyHigh { get; private set; }
    public uint StopFrequencyHigh { get; private set; }
    public byte[] Reservedn { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public uint StepFrequencyNumerator { get; private set; }
    public uint StepFrequencyDenominator { get; private set; }
    public ulong FreqOfFirstStep { get; private set; }
    public short[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        StartFrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StartFrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Reservedn = new byte[4];
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FreqOfFirstStep = BitConverter.ToUInt64(value, offset);
        //offset += 8;
        offset = startIndex + OptionalHeaderLength;
        DataCollection = new short[NumberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
        offset += NumberOfTraceItems * 2;
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
    public string SDemodulation { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public byte[] Reservedn { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public short SignalSource { get; private set; }
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
        offset += 2;
        SDemodulation = BitConverter.ToString(value, offset);
        offset += 8;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Reservedn = new byte[6];
        offset += 6;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        SignalSource = BitConverter.ToInt16(value, offset);
        //offset += 2;
        offset = startIndex + OptionalHeaderLength;
        Consts.AfModes.TryGetValue(AudioMode, out var afMode);
        var len = afMode?.LengthPerFrame ?? 2;
        var count = NumberOfTraceItems * len;
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
    public uint FrequencyLow { get; private set; }
    public uint SpanFrequency { get; private set; }
    public short AverageTime { get; private set; }
    public short AverageType { get; private set; }
    public uint MeasureTime { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public int DemodFreqChannel { get; private set; }
    public uint DemodFreqLow { get; private set; }
    public uint DemodFreqHigh { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public uint StepFrequencyNumerator { get; private set; }
    public uint StepFrequencyDenominator { get; private set; }
    public short SignalSource { get; private set; }
    public short MeasureMode { get; private set; }
    public ulong MeasureTimestamp { get; private set; }
    public short[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        FrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        SpanFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        AverageTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        AverageType = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureTime = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DemodFreqChannel = BitConverter.ToInt32(value, offset);
        offset += 4;
        DemodFreqLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DemodFreqHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        SignalSource = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureMode = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureTimestamp = BitConverter.ToUInt64(value, offset);
        //offset += 8;
        offset = startIndex + OptionalHeaderLength;
        DataCollection = new short[NumberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
        offset += NumberOfTraceItems * 2;
        return offset;
    }
}

/// <summary>
///     With this trace type, as with "FScanTrace", all data specified in the "selectorFlags" is relevant.
/// </summary>
[Serializable]
internal class RawCw : RawData
{
    public uint FrequencyLow { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public short SignalSource { get; private set; }
    public byte[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        FrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        SignalSource = BitConverter.ToInt16(value, offset);
        //offset += 2;
        offset = startIndex + OptionalHeaderLength;
        DataCollection = new byte[NumberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems);
        offset += NumberOfTraceItems;
        return offset;
    }
}

[Serializable]
internal class RawIf : RawData
{
    public short Mode { get; private set; }
    public short FrameLen { get; private set; }
    public uint Samplerate { get; private set; }
    public uint FrequencyLow { get; private set; }

    /// <summary>
    ///     IF bandwidth
    /// </summary>
    public uint Bandwidth { get; private set; }

    public ushort Demodulation { get; private set; }
    public short RxAtt { get; private set; }
    public ushort Flags { get; private set; }
    public short KFactor { get; private set; }
    public string SDemodulation { get; private set; }
    public ulong SampleCount { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public byte[] ReservedIf { get; private set; }
    public ulong StartTimestamp { get; private set; }
    public short SignalSource { get; private set; }
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
        FrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Bandwidth = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Demodulation = BitConverter.ToUInt16(value, offset);
        offset += 2;
        RxAtt = BitConverter.ToInt16(value, offset);
        offset += 2;
        Flags = BitConverter.ToUInt16(value, offset);
        offset += 2;
        KFactor = BitConverter.ToInt16(value, offset);
        offset += 2;
        SDemodulation = BitConverter.ToString(value, offset, 8);
        offset += 8;
        SampleCount = BitConverter.ToUInt64(value, offset);
        offset += 8;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        ReservedIf = new byte[4];
        offset += 4;
        StartTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        SignalSource = BitConverter.ToInt16(value, offset);
        //offset += 2;
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
internal class RawVdPan : RawData
{
    public uint FrequencyLow { get; private set; }
    public uint SpanFrequency { get; private set; }

    /// <summary>
    ///     Not used and always set to 0
    /// </summary>
    public short AverageTime { get; private set; }

    /// <summary>
    ///     2 AM | LEFT
    ///     3 FM | RIGHt
    ///     4 IQ
    ///     5 AMSquare
    ///     6 FMSquare
    ///     7 IQSquare
    /// </summary>
    public short DispayVariant { get; private set; }

    public uint MeasureTime { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public int DemodFreqChannel { get; private set; }
    public uint DemodFreqLow { get; private set; }
    public uint DemodFreqHigh { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public uint StepFrequencyNumerator { get; private set; }
    public uint StepFrequencyDenominator { get; private set; }
    public short SignalSource { get; private set; }
    public short MeasureMode { get; private set; }
    public short[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        FrequencyLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        SpanFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        AverageTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        DispayVariant = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureTime = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DemodFreqChannel = BitConverter.ToInt32(value, offset);
        offset += 4;
        DemodFreqLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DemodFreqHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
        offset += 4;
        SignalSource = BitConverter.ToInt16(value, offset);
        offset += 2;
        MeasureMode = BitConverter.ToInt16(value, offset);
        //offset += 2;
        offset = startIndex + OptionalHeaderLength;
        DataCollection = new short[NumberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
        offset += NumberOfTraceItems * 2;
        return offset;
    }
}

#endregion