using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Magneto.Device.DDF255;

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

    public static RawPacket Parse(byte[] value, int offset, FirmwareVersion firmwareVersion = FirmwareVersion.Default)
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
                var data = RawData.Parse(value, offset, firmwareVersion);
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

    public static RawData Parse(byte[] value, int offset, FirmwareVersion firmwareVersion = FirmwareVersion.Default)
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
            case DataType.DfPan:
                raw = new RawDfPan();
                break;
            case DataType.Audio:
                raw = new RawAudio();
                break;
            case DataType.Pscan: // PSCAN
                raw = new RawPScan(firmwareVersion);
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
    public uint FrequencyLow { get; private set; }

    /// <summary>
    ///     IF bandwidth
    /// </summary>
    public uint Bandwidth { get; private set; }

    public ushort Demodulation { get; private set; }
    public short RxAtt { get; private set; }
    public ushort Flags { get; private set; }
    public short KFactor { get; private set; }
    public string DemodulationString { get; private set; }
    public ulong SampleCount { get; private set; }
    public uint FrequencyHigh { get; private set; }
    public byte[] ReservedIf { get; private set; }
    public ulong StartTimestamp { get; private set; }
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
        DemodulationString = BitConverter.ToString(value, offset, 8);
        offset += 8;
        SampleCount = BitConverter.ToUInt64(value, offset);
        offset += 8;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        ReservedIf = new byte[4];
        offset += 4;
        StartTimestamp = BitConverter.ToUInt64(value, offset);
        //offset += 8;
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
        StartFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequency = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StepFrequency = BitConverter.ToUInt32(value, offset);
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
    public uint ChildReserved1 { get; private set; }
    public ushort ChildReserved2 { get; private set; }
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
        ChildReserved1 = BitConverter.ToUInt32(value, offset);
        offset += 4;
        ChildReserved2 = BitConverter.ToUInt16(value, offset);
        offset += 2;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
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
    private readonly FirmwareVersion _firmwareVersion;

    public RawPScan(FirmwareVersion firmwareVersion = FirmwareVersion.Default)
    {
        _firmwareVersion = firmwareVersion;
    }

    public uint StartFrequency { get; private set; }
    public uint StopFrequency { get; private set; }
    public uint StepFrequency { get; private set; }
    public uint StartFrequencyHigh { get; private set; }
    public uint StopFrequencyHigh { get; private set; }
    public uint ReservedPscan { get; private set; }
    public ulong OutputTimestamp { get; private set; }
    public uint StepFrequencyNumerator { get; private set; }
    public uint StepFrequencyDenominator { get; private set; }
    public ulong FreqOfFirstStep { get; private set; }
    public short[] DataCollection { get; private set; }
    public uint[] FreqLowCollection { get; private set; }
    public uint[] FreqHighCollection { get; private set; }

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
        StartFrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        StopFrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        ReservedPscan = BitConverter.ToUInt32(value, offset);
        offset += 4;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        offset = startIndex + OptionalHeaderLength;
        switch (_firmwareVersion)
        {
            case FirmwareVersion.Old:
            {
                offset = startIndex + OptionalHeaderLength;
                DataCollection = new short[NumberOfTraceItems];
                Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
                offset += NumberOfTraceItems * 2;
            }
                break;
            case FirmwareVersion.OldSubscribe:
            {
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
            }
                break;
            case FirmwareVersion.OffsetFrequencySwaped:
            {
                StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
                offset += 4;
                StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
                offset += 4;
                Array.Reverse(value, offset, 4);
                var low = BitConverter.ToUInt32(value, offset);
                offset += 4;
                Array.Reverse(value, offset, 4);
                var high = BitConverter.ToUInt32(value, offset);
                FreqOfFirstStep = ((ulong)high << 32) + low;
                DataCollection = new short[NumberOfTraceItems];
                Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
            }
                break;
            case FirmwareVersion.Default:
            default:
            {
                StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
                offset += 4;
                StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
                offset += 4;
                FreqOfFirstStep = FreqOfFirstStep = BitConverter.ToUInt64(value, offset);
                DataCollection = new short[NumberOfTraceItems];
                Buffer.BlockCopy(value, offset, DataCollection, 0, NumberOfTraceItems * 2);
            }
                break;
        }

        return offset;
    }
}

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

[Serializable]
internal class RawAudio : RawData
{
    public short AudioMode { get; private set; }
    public short FrameLen { get; private set; }
    public uint FrequencyLow { get; private set; }
    public uint Bandwidth { get; private set; }
    public ushort Demodulation { get; private set; }
    public string DemodulationString { get; private set; }
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
        DemodulationString = BitConverter.ToString(value, offset);
        offset += 8;
        FrequencyHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Reserved = new byte[6];
        offset += 6;
        OutputTimestamp = BitConverter.ToUInt64(value, offset);
        //offset += 8;
        //SignalSource = BitConverter.ToInt16(value, offset);
        ////offset += 2;
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

[Serializable]
internal class RawGps : RawData
{
    /// <summary>
    ///     denotes whether GPS data are to be considered valid
    /// </summary>
    public short Valid { get; set; }

    /// <summary>
    ///     number of satellites in view 0-12; only valid, if GGA msg is received, else -1 (GPS_UNDEFINDED)
    /// </summary>
    public short NoOfSatInView { get; set; }

    /// <summary>
    ///     latitude direction ('N' or 'S')
    /// </summary>
    public short LatRef { get; set; }

    /// <summary>
    ///     latitude degrees
    /// </summary>
    public short LatDeg { get; set; }

    /// <summary>
    ///     geographical latitude: minutes
    /// </summary>
    public float LatMin { get; set; }

    /// <summary>
    ///     longitude direction ('E' or 'W')
    /// </summary>
    public short LonRef { get; set; }

    /// <summary>
    ///     longitude degrees
    /// </summary>
    public short LonDeg { get; set; }

    /// <summary>
    ///     geographical longitude: minutes
    /// </summary>
    public float LonMin { get; set; }

    public float Hdop { get; set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Valid = BitConverter.ToInt16(value, offset);
        offset += 2;
        NoOfSatInView = BitConverter.ToInt16(value, offset);
        offset += 2;
        LatRef = BitConverter.ToInt16(value, offset);
        offset += 2;
        LatDeg = BitConverter.ToInt16(value, offset);
        offset += 2;
        LatMin = BitConverter.ToSingle(value, offset);
        offset += 4;
        LonRef = BitConverter.ToInt16(value, offset);
        offset += 2;
        LonDeg = BitConverter.ToInt16(value, offset);
        offset += 2;
        LonMin = BitConverter.ToSingle(value, offset);
        offset += 4;
        Hdop = BitConverter.ToSingle(value, offset);
        offset += 4;
        return offset;
    }
}

[Serializable]
internal class RawDfPan : RawData
{
    public uint FreqLow { get; set; }
    public uint FreqHigh { get; set; }
    public uint FreqSpan { get; set; }
    public int DfThresholdMode { get; set; }
    public int DfThresholdValue { get; set; }
    public uint DfBandWidth { get; set; }
    public uint Stepwidth { get; set; }
    public int DfMeasureTime { get; set; }
    public int DfOption { get; set; }
    public ushort CompassHeading { get; set; }
    public short CompassHeadingType { get; set; }
    public int AntennaFactor { get; set; }
    public int DemodFreqChannel { get; set; }
    public uint DemodfreqLow { get; set; }
    public uint DemodfreqHigh { get; set; }
    public ulong OutputTimeStamp { get; set; }
    public RawGps GpsHeader { get; set; }
    public short[] LevelCollection { get; private set; }
    public short[] AzimuthCollection { get; private set; }
    public short[] QualityCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        var startIndex = offset;
        FreqLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FreqHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        FreqSpan = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DfThresholdMode = BitConverter.ToInt32(value, offset);
        offset += 4;
        DfThresholdValue = BitConverter.ToInt32(value, offset);
        offset += 4;
        DfBandWidth = BitConverter.ToUInt32(value, offset);
        offset += 4;
        Stepwidth = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DfMeasureTime = BitConverter.ToInt32(value, offset);
        offset += 4;
        DfOption = BitConverter.ToInt32(value, offset);
        offset += 4;
        CompassHeading = BitConverter.ToUInt16(value, offset);
        offset += 2;
        CompassHeadingType = BitConverter.ToInt16(value, offset);
        offset += 2;
        AntennaFactor = BitConverter.ToInt32(value, offset);
        offset += 4;
        DemodFreqChannel = BitConverter.ToInt32(value, offset);
        offset += 4;
        DemodfreqLow = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DemodfreqHigh = BitConverter.ToUInt32(value, offset);
        offset += 4;
        OutputTimeStamp = BitConverter.ToUInt64(value, offset);
        offset += 8;
        GpsHeader = new RawGps();
        GpsHeader.Convert(value, offset);
        offset = startIndex + OptionalHeaderLength;
        var flag = Flags.DfLevel;
        while (flag != Flags.OptionalHeader)
        {
            if ((SelectorFlags & (uint)flag) > 0)
                switch (flag)
                {
                    case Flags.DfLevel:
                    {
                        LevelCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, LevelCollection, 0, NumberOfTraceItems * 2);
                        offset += NumberOfTraceItems * 2;
                        break;
                    }
                    case Flags.Azimuth:
                    {
                        AzimuthCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, AzimuthCollection, 0, NumberOfTraceItems * 2);
                        offset += 2 * NumberOfTraceItems;
                        break;
                    }
                    case Flags.DfQuality:
                    {
                        QualityCollection = new short[NumberOfTraceItems];
                        Buffer.BlockCopy(value, offset, QualityCollection, 0, NumberOfTraceItems * 2);
                        offset += 2 * NumberOfTraceItems;
                        break;
                    }
                }

            flag = (Flags)((uint)flag << 1);
        }

        return offset;
    }
}