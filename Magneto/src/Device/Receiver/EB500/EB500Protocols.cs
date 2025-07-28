using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magneto.Device.EB500;

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
    public ushort SequenceNumberLow { get; private set; }
    public ushort SequenceNumberHigh { get; private set; }
    public uint DataSize { get; private set; }
    public int DataLength => 16;

    public int Convert(byte[] value, int offset)
    {
        MagicNumber = BitConverter.ToUInt32(value, offset);
        offset += 4;
        VersionMinor = BitConverter.ToUInt16(value, offset);
        offset += 2;
        VersionMajor = BitConverter.ToUInt16(value, offset);
        offset += 2;
        SequenceNumberLow = BitConverter.ToUInt16(value, offset);
        offset += 2;
        SequenceNumberHigh = BitConverter.ToUInt16(value, offset);
        offset += 2;
        DataSize = BitConverter.ToUInt32(value, offset);
        offset += 4;
        return offset;
    }
}

internal abstract class GenericAttributeHeader : IRawHeader
{
    public ushort Tag { get; protected set; }
    public uint Length { get; protected set; }
    public abstract int DataLength { get; }
    public abstract int Convert(byte[] value, int offset);
}

internal class ConventionalGenericAttributeHeader : GenericAttributeHeader
{
    public override int DataLength => 4;
    public ushort HeaderLength { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        Tag = BitConverter.ToUInt16(value, offset);
        offset += 2;
        Array.Reverse(value, offset, 2);
        HeaderLength = BitConverter.ToUInt16(value, offset);
        Length = HeaderLength;
        offset += 2;
        return offset;
    }
}

internal class AdvancedGenericAttributeHeader : GenericAttributeHeader
{
    public ushort Reserved { get; private set; }
    public uint Reserved0 { get; private set; }
    public uint Reserved1 { get; private set; }
    public uint Reserved2 { get; private set; }
    public uint Reserved3 { get; private set; }
    public override int DataLength => 24;

    public override int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        Tag = BitConverter.ToUInt16(value, offset);
        offset += 2;
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
        return offset;
    }
}

internal abstract class TraceAttributeHeader : IRawHeader
{
    public int NumberOfTraceItems { get; protected set; }
    public uint ChannelNumber { get; protected set; }
    public int OptionalHeaderLength { get; protected set; }
    public uint SelectorFlags { get; protected set; }
    public abstract int DataLength { get; }
    public abstract int Convert(byte[] value, int offset);
}

internal class ConventionalTraceAttributeHeader : TraceAttributeHeader
{
    public short NoOfTraceItems { get; protected set; }
    public byte ChannelNo { get; protected set; }
    public byte OptHeaderLength { get; protected set; }
    public override int DataLength => 8;

    public override int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 2);
        NoOfTraceItems = BitConverter.ToInt16(value, offset);
        NumberOfTraceItems = NoOfTraceItems;
        offset += 2;
        ChannelNo = value[offset];
        ChannelNumber = ChannelNo;
        offset++;
        OptHeaderLength = value[offset];
        OptionalHeaderLength = OptHeaderLength;
        offset++;
        Array.Reverse(value, offset, 4);
        SelectorFlags = BitConverter.ToUInt32(value, offset);
        offset += 4;
        return offset;
    }
}

internal class AdvancedTraceAttributeHeader : TraceAttributeHeader
{
    public uint NoOfTraceItems { get; protected set; }
    public uint ChannelNo { get; protected set; }
    public uint OptHeaderLength { get; protected set; }
    public uint SelectorFlagsLow { get; private set; }
    public uint SelectorFlagsHigh { get; private set; }
    public uint Reserved0 { get; private set; }
    public uint Reserved1 { get; private set; }
    public uint Reserved2 { get; private set; }
    public uint Reserved3 { get; private set; }
    public override int DataLength => 36;

    public override int Convert(byte[] value, int offset)
    {
        Array.Reverse(value, offset, 4);
        NoOfTraceItems = BitConverter.ToUInt32(value, offset);
        NumberOfTraceItems = (int)NoOfTraceItems;
        offset += 4;
        Array.Reverse(value, offset, 4);
        ChannelNo = BitConverter.ToUInt32(value, offset);
        ChannelNumber = ChannelNo;
        offset += 4;
        Array.Reverse(value, offset, 4);
        OptHeaderLength = BitConverter.ToUInt32(value, offset);
        OptionalHeaderLength = (int)OptHeaderLength;
        offset += 4;
        Array.Reverse(value, offset, 4);
        SelectorFlagsLow = BitConverter.ToUInt32(value, offset);
        SelectorFlags = SelectorFlagsLow;
        offset += 4;
        Array.Reverse(value, offset, 4);
        SelectorFlagsHigh = BitConverter.ToUInt32(value, offset);
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
                    offset += data.Generic.Tag >= 5000 ? 8 : 4;
                    offset += (int)data.Generic.Length;
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
        var tag = BitConverter.ToUInt16(value, offset);
        Array.Reverse(value, offset, 2);
        GenericAttributeHeader generic;
        TraceAttributeHeader trace;
        if (tag >= 5000)
        {
            generic = new AdvancedGenericAttributeHeader();
            offset = generic.Convert(value, offset);
            trace = new AdvancedTraceAttributeHeader();
            offset = trace.Convert(value, offset);
        }
        else
        {
            generic = new ConventionalGenericAttributeHeader();
            offset = generic.Convert(value, offset);
            trace = new ConventionalTraceAttributeHeader();
            offset = trace.Convert(value, offset);
        }

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
        Reserved = BitConverter.ToUInt16(value, offset);
        offset += 2;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        //offset += 8;
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.OptionalHeader)
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
                    case Flags.Freqlow:
                    {
                        FreqLowCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqLowCollection, 0, numberOfTraceItems * 4);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                    case Flags.Freqhigh:
                    {
                        FreqHighCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqHighCollection, 0, numberOfTraceItems * 4);
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
    public byte[] Reserved { get; private set; }
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
        DwellTime = BitConverter.ToInt16(value, offset);
        offset += 2;
        DirectionUp = BitConverter.ToInt16(value, offset);
        offset += 2;
        StopSignal = BitConverter.ToInt16(value, offset);
        offset += 2;
        Reserved = new byte[6];
        offset += 6;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        while (flag != Flags.OptionalHeader)
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
                    case Flags.Freqlow:
                    {
                        FreqLowCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqLowCollection, 0, numberOfTraceItems * 4);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                    case Flags.Freqhigh:
                    {
                        FreqHighCollection = new uint[numberOfTraceItems];
                        Buffer.BlockCopy(value, offset, FreqHighCollection, 0, numberOfTraceItems * 4);
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
internal class RawPScan : RawData
{
    public uint StartFrequencyLow { get; private set; }
    public uint StopFrequencyLow { get; private set; }
    public uint StepFrequency { get; private set; }
    public uint StartFrequencyHigh { get; private set; }
    public uint StopFrequencyHigh { get; private set; }
    public byte[] Reserved { get; private set; }
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
        Reserved = new byte[4];
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        var flag = Flags.Level;
        uint low = 0;
        uint high = 0;
        while (flag != Flags.OptionalHeader)
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
                    case Flags.Freqlow:
                    {
                        low = BitConverter.ToUInt32(value, offset);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                    case Flags.Freqhigh:
                    {
                        high = BitConverter.ToUInt32(value, offset);
                        offset += 4 * numberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        FreqOfFirstStep = ((ulong)high << 32) + low;
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
    public byte[] Reserved { get; private set; }
    public ulong OutputTimestamp { get; private set; }
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
        Reserved = new byte[6];
        offset += 6;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
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
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        DataCollection = new short[numberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 2);
        offset += numberOfTraceItems * 2;
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
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        DataCollection = new byte[numberOfTraceItems];
        Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems);
        offset += numberOfTraceItems;
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
        var optionalHeaderLength = Trace.OptionalHeaderLength;
        var numberOfTraceItems = Trace.NumberOfTraceItems;
        offset = startIndex + optionalHeaderLength;
        if (Mode == 1)
        {
            var temp = new short[numberOfTraceItems * 2];
            Buffer.BlockCopy(value, offset, temp, 0, numberOfTraceItems * 4);
            DataCollection = Array.ConvertAll<short, int>(temp, item => item);
            offset += numberOfTraceItems * 4;
        }
        else if (Mode == 2)
        {
            DataCollection = new int[numberOfTraceItems * 2];
            Buffer.BlockCopy(value, offset, DataCollection, 0, numberOfTraceItems * 8);
            offset += numberOfTraceItems * 8;
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