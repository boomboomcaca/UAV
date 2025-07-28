using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable RCS1181 // Convert comment to documentation comment. [MR3000A]csharp(RCS1181)
namespace Magneto.Device.MR3000A;

#region 枚举/类定义

[Flags]
internal enum DataType
{
    None = 0,
    Iq = 1,
    Level = Iq << 1,
    Spectrum = Level << 1,
    Dfind = Spectrum << 1,
    Dfpan = Dfind << 1,
    Audio = Dfpan << 1,
    Itu = Audio << 1,
    Scan = Itu << 1,
    Tdoa = Scan << 1,
    Dfiq = Tdoa << 1,
    Sms = Dfiq << 1,
    Ddc = Sms << 1,
    Gps = Ddc << 1,
    Dfc = Gps << 1,
    Compass = Dfc << 1
}

#endregion

#region 数据协议

[Serializable]
internal class RawPacket
{
    public int Version { get; private set; }
    public int Count { get; private set; }
    public List<RawData> DataCollection { get; private set; }

    public static RawPacket Parse(byte[] value, int offset)
    {
        var packet = new RawPacket
        {
            Version = BitConverter.ToInt32(value, offset)
        };
        offset += 4;
        packet.Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        packet.DataCollection = new List<RawData>();
#if WRITE_DEBUG_INFO
			if (packet.Count <= 0)
			{
				Console.WriteLine(string.Format("Invalid data parsing, {0} data to be parsed", packet.Count));
			}
#endif
        for (var index = 0; index < packet.Count && offset < value.Length; index++)
        {
            var data = RawData.Parse(value, offset, packet.Version);
            if (data != null)
            {
                packet.DataCollection.Add(data);
                offset += data.Size;
            }
        }

        return packet;
    }
}

/// <summary>
///     数据基类
/// </summary>
[Serializable]
internal class RawData
{
    public RawData(int packetVersion)
    {
        PacketVersion = packetVersion;
    }

    public int PacketVersion { get; }
    public int Tag { get; private set; }

    public int Version { get; private set; }

    //
    // 以下三个字段适用于Packet Version小于2的情况
    public byte DdcCount { get; private set; }
    public byte DdcIndex { get; private set; }

    public short State { get; private set; }

    //
    // 以下两个字段适用于Packet Version大于等于2的情况
    public int TimestampSecond { get; private set; }
    public int TimestampNanoSecond { get; private set; }
    public int Size { get; private set; }

    public static RawData Parse(byte[] value, int offset, int packetVersion = 0)
    {
        RawData raw = null;
        var tag = (DataType)BitConverter.ToUInt32(value, offset);
        switch (tag)
        {
            case DataType.Iq:
            case DataType.Tdoa:
                if (packetVersion >= 2) raw = new RawIq(packetVersion);
                break;
            case DataType.Level:
                raw = new RawLevel(packetVersion);
                break;
            case DataType.Spectrum:
                raw = new RawSpectrum(packetVersion);
                break;
            case DataType.Dfind:
                raw = new RawDFind(packetVersion);
                break;
            case DataType.Dfpan:
                raw = new RawDfPan(packetVersion);
                break;
            case DataType.Audio:
            {
                var version = BitConverter.ToUInt32(value, offset + 4);
                if (version == 1)
                    raw = new RawDdcAudio(packetVersion);
                else
                    raw = new RawAudio(packetVersion);
            }
                break;
            case DataType.Itu:
                raw = new RawItu(packetVersion);
                break;
            case DataType.Scan: // PSCAN
            {
                var version = BitConverter.ToUInt32(value, offset + 4);
                if (version == 40)
                    raw = new RawScan(packetVersion);
                else
                    raw = new RawFastScan(packetVersion);
            }
                break;
            case DataType.Scan + 2: // FSCAN
            case DataType.Scan + 4: // MScan
                raw = new RawScan(packetVersion);
                break;
            case DataType.Dfiq:
            {
                var version = BitConverter.ToUInt32(value, offset + 4);
                if (version == 9)
                    raw = new RawDfiQforNine(packetVersion);
                else
                    raw = new RawDfiq(packetVersion);
            }
                break;
            case DataType.Dfc: // FIXDF 
            case DataType.Dfc + 4: // WBDF
            case DataType.Dfc + 8: // ScanDF
            {
                var version = BitConverter.ToUInt32(value, offset + 4);
                if (version == 9)
                    raw = new RawDfCforNine(packetVersion);
                else
                    raw = new RawDfc(packetVersion);
            }
                break;
            case DataType.Sms:
                raw = new RawSms(packetVersion);
                break;
            case DataType.Ddc:
                raw = new RawDdc(packetVersion);
                break;
            case DataType.Gps:
                raw = new RawGps(packetVersion);
                break;
            case DataType.Compass:
                raw = new RawCompass(packetVersion);
                break;
        }

        raw?.Convert(value, offset);
        return raw;
    }

    public virtual int Convert(byte[] value, int offset)
    {
        Tag = BitConverter.ToInt32(value, offset);
        offset += 4;
        Version = BitConverter.ToInt32(value, offset);
        offset += 4;
        if (PacketVersion < 2)
        {
            DdcCount = value[offset];
            offset++;
            DdcIndex = value[offset];
            offset++;
            State = BitConverter.ToInt16(value, offset);
            offset += 2;
        }
        else
        {
            TimestampSecond = BitConverter.ToInt32(value, offset);
            offset += 4;
            TimestampNanoSecond = BitConverter.ToInt32(value, offset);
            offset += 4;
        }

        Size = BitConverter.ToInt32(value, offset);
        offset += 4;
        return offset;
    }

    public override string ToString()
    {
        return $"tag=\"{((DataType)Tag).ToString().Replace(", ", "|")}\", ver={Version}";
    }
}

[Serializable]
internal class RawIq : RawData
{
    public RawIq(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; set; }
    public long Bandwidth { get; set; }
    public long SampleRate { get; set; }
    public int SynCode { get; set; }
    public int Offset { get; set; }
    public int Attenuation { get; set; }
    public int Total { get; set; }
    public int Count { get; set; }
    public int[] DataCollection { get; set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        SampleRate = BitConverter.ToInt64(value, offset);
        offset += 8;
        if (PacketVersion >= 2)
        {
            SynCode = BitConverter.ToInt32(value, offset);
            offset += 4;
            Offset = BitConverter.ToInt32(value, offset);
            offset += 4;
        }

        Attenuation = BitConverter.ToInt32(value, offset);
        offset += 4;
        if (PacketVersion >= 2)
        {
            Total = BitConverter.ToInt32(value, offset);
            offset += 4;
        }

        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        if (Version == 16)
        {
            var temp = new short[Count * 2];
            Buffer.BlockCopy(value, offset, temp, 0, Count * 4);
            DataCollection = Array.ConvertAll(temp, item => (int)item);
            offset += Count * 4;
        }
        else
        {
            DataCollection = new int[Count * 2];
            Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 8);
            offset += Count * 8;
        }

        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, sr={SampleRate}, att={Attenuation}, cnt={Count} size={DataCollection.Length * 4}";
    }
}

[Serializable]
internal class RawLevel : RawData
{
    public RawLevel(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public int Attenuation { get; private set; }

    public float Level { get; private set; }

    // 场强，单位dBuV/m，暂未使用，在监测软件中统一处理
    public float FieldStrength { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        Attenuation = BitConverter.ToInt32(value, offset);
        offset += 4;
        Level = BitConverter.ToSingle(value, offset);
        offset += 4;
        FieldStrength = BitConverter.ToSingle(value, offset);
        offset += 4;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, att={Attenuation}, lev={Level}, fs={FieldStrength}";
    }
}

[Serializable]
internal class RawSpectrum : RawData
{
    public RawSpectrum(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Span { get; private set; }
    public int Attenuation { get; private set; }
    public int Count { get; private set; }
    public short[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Span = BitConverter.ToInt64(value, offset);
        offset += 8;
        Attenuation = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        DataCollection = new short[Count];
        Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 2);
        offset += Count * 2;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, span={Span}, att={Attenuation}, cnt={Count}, size={DataCollection.Length * 2}";
    }
}

[Serializable]
internal class RawAudio : RawData
{
    public RawAudio(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }

    public long SampleRate { get; private set; }

    // 保留值
    public int Reserved { get; private set; }
    public int Count { get; private set; }
    public byte[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        SampleRate = BitConverter.ToInt64(value, offset);
        offset += 8;
        Reserved = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        DataCollection = new byte[Count * 2]; // 从接收机到达的数据是16位的字节序列
        Buffer.BlockCopy(value, offset, DataCollection, 0, Count * 2);
        offset += Count * 2;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, sr={SampleRate}, cnt={Count}, size={DataCollection.Length}";
    }
}

[Serializable]
internal class RawDdcAudio : RawData
{
    public RawDdcAudio(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long SampleRate { get; private set; }

    // 使能的通道位，Flags，最大表示32位
    public uint EnabledChannels { get; private set; }

    // 音频数据个数（SHORT类型）
    public int Count { get; private set; }

    // 音频数据，包含最大32位音频
    public byte[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        SampleRate = BitConverter.ToInt64(value, offset);
        offset += 8;
        EnabledChannels = BitConverter.ToUInt32(value, offset);
        offset += 4;
        var ddcCount = 0;
        for (var index = 0; index < 32; ++index)
            if (((EnabledChannels >> index) & 0x1) == 0x1)
                ddcCount++;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        DataCollection = new byte[ddcCount * Count * 2];
        Buffer.BlockCopy(value, offset, DataCollection, 0, DataCollection.Length);
        offset += DataCollection.Length;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, sr={SampleRate}, enabled_ch_cnt={DataCollection.Length / Count / 2}, cnt={Count}, size={DataCollection.Length}";
    }
}

[Serializable]
internal class RawItu : RawData
{
    public RawItu(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Beta { get; private set; }
    public long XdB { get; private set; }
    public float Am { get; private set; }
    public float AmPos { get; private set; }
    public float AmNeg { get; private set; }
    public float Fm { get; private set; }
    public float FmPos { get; private set; }
    public float FmNeg { get; private set; }
    public float Pm { get; private set; }
    public float PmPos { get; private set; }

    public float PmNeg { get; private set; }

    // 调制识别结果，值从-1开始依次对应Error, AM, FM, 2FSK, 4FSK, 2PSK, 4PSK, 2ASK, 4ASK, DSB, CW
    public int Modulation { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Beta = BitConverter.ToInt64(value, offset);
        offset += 8;
        XdB = BitConverter.ToInt64(value, offset);
        offset += 8;
        Am = BitConverter.ToSingle(value, offset);
        offset += 4;
        AmPos = BitConverter.ToSingle(value, offset);
        offset += 4;
        AmNeg = BitConverter.ToSingle(value, offset);
        offset += 4;
        Fm = BitConverter.ToSingle(value, offset);
        offset += 4;
        FmPos = BitConverter.ToSingle(value, offset);
        offset += 4;
        FmNeg = BitConverter.ToSingle(value, offset);
        offset += 4;
        Pm = BitConverter.ToSingle(value, offset);
        offset += 4;
        PmPos = BitConverter.ToSingle(value, offset);
        offset += 4;
        PmNeg = BitConverter.ToSingle(value, offset);
        offset += 4;
        Modulation = BitConverter.ToInt32(value, offset);
        offset += 4;
        return offset;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, freq={Frequency}, beta={Beta}, xdb={XdB}";
    }
}

[Serializable]
internal class RawScan : RawData
{
    public short[] DataCollection;

    public RawScan(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long StartFrequency { get; private set; }
    public long StopFrequency { get; private set; }
    public long StepFrequency { get; private set; }
    public int SegmentIndex { get; private set; }
    public int Total { get; set; }
    public int Offset { get; private set; }
    public int Count { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        StartFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        StopFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        StepFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        SegmentIndex = BitConverter.ToInt32(value, offset);
        offset += 4;
        Total = BitConverter.ToInt32(value, offset);
        offset += 4;
        Offset = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        DataCollection = new short[Count];
        Buffer.BlockCopy(value, offset, DataCollection, 0, 2 * Count);
        offset += 2 * Count;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, start={StartFrequency}, stop={StopFrequency}, step={StepFrequency}, total={Total}, offset={Offset}, cnt={Count}, size={DataCollection.Length * 2}";
    }
}

[Serializable]
internal class RawFastScan : RawData
{
    public RawFastScan(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long StartFrequency { get; private set; }
    public long StopFrequency { get; private set; }
    public long StepFrequency { get; private set; }
    public int SegmentIndex { get; private set; }
    public int Count { get; private set; }
    public short[] SignalCollection { get; private set; }

    public short[] NoiseCollection { get; private set; }

    // 信号索引
    public int[] SignalIndexCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        StartFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        StopFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        StepFrequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        SegmentIndex = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        SignalCollection = new short[Count];
        Buffer.BlockCopy(value, offset, SignalCollection, 0, 2 * Count);
        offset += 2 * Count;
        NoiseCollection = new short[Count];
        Buffer.BlockCopy(value, offset, NoiseCollection, 0, 2 * Count);
        offset += 2 * Count;
        SignalIndexCollection = new int[Count];
        Buffer.BlockCopy(value, offset, SignalIndexCollection, 0, 4 * Count);
        offset += 4 * Count;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, start={StartFrequency}, stop={StopFrequency}, step={StepFrequency}, cnt={Count}, s_len={SignalCollection.Length}, n_len={NoiseCollection.Length}, idx_len={SignalIndexCollection.Length}";
    }
}

[Serializable]
internal class RawDfiq : RawData
{
    public RawDfiq(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public long SampleRate { get; private set; }
    public int Attenuation { get; private set; }
    public byte SyncCode { get; private set; }
    public byte AntennaIndex { get; private set; }

    public byte ChannelCount { get; private set; }

    // 当前通道编号，单通道固定为-1，双通道取值为0,1,-1， 多通道取值为0,1,2,..,N-1,-1
    public sbyte ChannelIndex { get; private set; }
    public short GroupCount { get; private set; }

    public short GroupOffset { get; private set; }

    // 单天线IQ采样数
    public int Count { get; private set; }

    // IQ测向原始数据
    public int[] DataColleciton { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        SampleRate = BitConverter.ToInt64(value, offset);
        offset += 8;
        Attenuation = BitConverter.ToInt32(value, offset);
        offset += 4;
        SyncCode = value[offset];
        offset++;
        AntennaIndex = value[offset];
        offset++;
        ChannelCount = value[offset];
        offset++;
        ChannelIndex = (sbyte)value[offset];
        offset++;
        GroupCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        GroupOffset = BitConverter.ToInt16(value, offset);
        offset += 2;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        if (ChannelIndex == -1)
        {
            DataColleciton = new int[Count * 2 * ChannelCount];
            Buffer.BlockCopy(value, offset, DataColleciton, 0, Count * 8 * ChannelCount);
            offset += Count * 8 * ChannelCount;
        }
        else
        {
            DataColleciton = new int[Count * 2];
            Buffer.BlockCopy(value, offset, DataColleciton, 0, Count * 8);
            offset += Count * 8;
        }

        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, sr={SampleRate}, att={Attenuation}, sync_code={SyncCode}, ant_idx={AntennaIndex}, ch_cnt={ChannelCount}, ch_idx={ChannelIndex}, gp_cnt={GroupCount}, gp_offset={GroupOffset}, cnt={Count}, size={DataColleciton.Length * 4}";
    }
}

[Serializable]
internal class RawDfc : RawData
{
    public RawDfc(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public byte AntennaIndex { get; private set; }
    public byte ChannelCount { get; private set; }
    public short GroupCount { get; private set; }
    public short SpectrumCount { get; private set; }
    public short CharacterCount { get; private set; }
    public short[] Levels { get; private set; }

    public short[] Spectra { get; private set; }

    // 特征数据，多组数据依次排列，每组长度为Count，总共GroupCount组
    public short[] Characters { get; private set; }
    public int Level { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        AntennaIndex = value[offset];
        offset++;
        ChannelCount = value[offset];
        offset++;
        GroupCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        SpectrumCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        CharacterCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        // Level = BitConverter.ToInt32(value, offset);
        // offset += 4;
        Levels = new short[ChannelCount * GroupCount];
        Buffer.BlockCopy(value, offset, Levels, 0, 2 * ChannelCount * GroupCount);
        offset += 2 * ChannelCount * GroupCount;
        Spectra = new short[SpectrumCount];
        Buffer.BlockCopy(value, offset, Spectra, 0, 2 * SpectrumCount);
        offset += 2 * SpectrumCount;
        Characters = new short[CharacterCount * GroupCount];
        Buffer.BlockCopy(value, offset, Characters, 0, 2 * CharacterCount * GroupCount);
        offset += 2 * CharacterCount * GroupCount;
        Level = Levels.Max();
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, ant_idx={AntennaIndex}, ch_cnt={ChannelCount}, gp_cnt={GroupCount}, sp_cnt={SpectrumCount}, c_cnt={CharacterCount}, lev={Level / 10.0f}";
    }
}

[Serializable]
internal class RawDfiQforNine : RawData
{
    public RawDfiQforNine(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public long SampleRate { get; private set; }
    public int Attenuation { get; private set; }
    public byte SyncCode { get; private set; }
    public byte AntennaIndex { get; private set; }

    public byte ChannelCount { get; private set; } // 接收机回传结果应固定为9

    // 当前通道编号
    public sbyte ChannelIndex { get; private set; }

    // 数据包数，由于网终环境下，单次测向结果不确定能够一次性回传完毕，因此存在分多次传送的情况，此处为所需的总包数
    public short PacketCount { get; private set; }

    // 数据包偏移量，用于标识当前回传的数据来自于哪些通道
    public short PacketOffset { get; private set; }

    // 单天线IQ采样数
    public int Count { get; private set; }

    // IQ测向原始数据，标识本次回传的通道数据
    public int[] DataCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        SampleRate = BitConverter.ToInt64(value, offset);
        offset += 8;
        Attenuation = BitConverter.ToInt32(value, offset);
        offset += 4;
        SyncCode = value[offset];
        offset++;
        AntennaIndex = value[offset];
        offset++;
        ChannelCount = value[offset];
        offset++;
        ChannelIndex = (sbyte)value[offset];
        offset++;
        PacketCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        PacketOffset = BitConverter.ToInt16(value, offset);
        offset += 2;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        // 计算当前数据的长度
        var dataLength =
            (ChannelCount / PacketCount +
             (ChannelCount % PacketCount == 0 ? 0 : PacketOffset > 0 ? ChannelCount % PacketOffset : 0)) * Count * 2;
        DataCollection = new int[dataLength];
        Buffer.BlockCopy(value, offset, DataCollection, 0, dataLength * 4);
        offset += dataLength * 4;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, sr={SampleRate}, att={Attenuation}, sync_code={SyncCode}, ant_idx={AntennaIndex}, ch_cnt={ChannelCount}, ch_idx={ChannelIndex}, gp_cnt={PacketCount}, gp_offset={PacketOffset}, cnt={Count}, size={DataCollection.Length * 4}";
    }
}

[Serializable]
internal class RawDfCforNine : RawData
{
    public RawDfCforNine(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public byte AntennaIndex { get; private set; }
    public byte ChannelCount { get; private set; } // 接收机回传结果应固定为9

    public short GroupCount { get; private set; } // 接收机回传结果应固定为1（与天线码一致）

    // 频谱数据长度
    public short SpectrumCount { get; private set; }

    // 特征数据长度，单频测向为1，宽带测向与频谱点数相同
    public short CharacterCount { get; private set; }

    // 测向电平
    public int Level { get; private set; }

    // 频谱数据
    public short[] Spectra { get; private set; }

    // 特征数据，为CharacterCount * ChannelCount
    public short[] Characters { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        AntennaIndex = value[offset];
        offset++;
        ChannelCount = value[offset];
        offset++;
        GroupCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        SpectrumCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        CharacterCount = BitConverter.ToInt16(value, offset);
        offset += 2;
        Level = BitConverter.ToInt32(value, offset);
        offset += 4;
        Spectra = new short[SpectrumCount];
        Buffer.BlockCopy(value, offset, Spectra, 0, 2 * SpectrumCount);
        offset += 2 * SpectrumCount;
        Characters = new short[CharacterCount * ChannelCount];
        Buffer.BlockCopy(value, offset, Characters, 0, 2 * CharacterCount * ChannelCount);
        offset += 2 * CharacterCount * ChannelCount;
        // 接收机回转的相位差数据为0-1,0-2,0-3,0-4,0-5,0-6,0-7,0-8，并不包含8-1的部分，此处人为添加8-1的相位差，为后续算法提供一致的输入
        for (var index = 0; index < CharacterCount; ++index)
            Characters[CharacterCount * (ChannelCount - 1) + index] =
                (short)(Characters[CharacterCount * (ChannelCount - 2) + index] - Characters[index]);
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, ant_idx={AntennaIndex}, ch_cnt={ChannelCount}, gp_cnt={GroupCount}, sp_cnt={SpectrumCount}, c_cnt={CharacterCount}, lev={Level / 10.0f}";
    }
}

[Serializable]
internal class RawSms : RawData
{
    public RawSms(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public int ColorCode { get; private set; }
    public int CalledNumber { get; private set; }
    public int CallingNumber { get; private set; }
    public int IsAscii { get; private set; }
    public int Count { get; private set; }
    public string Text { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        ColorCode = BitConverter.ToInt32(value, offset);
        offset += 4;
        CalledNumber = BitConverter.ToInt32(value, offset);
        offset += 4;
        CallingNumber = BitConverter.ToInt32(value, offset);
        offset += 4;
        IsAscii = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        if (IsAscii == 1)
            Text = Encoding.ASCII.GetString(value, offset, Count);
        else
            Text = Encoding.Unicode.GetString(value, offset, Count);
        offset += Count;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, cc={ColorCode}, calling={CallingNumber}, called={CalledNumber}, ascii={IsAscii}, count={Count}, text={Text}";
    }
}

[Serializable]
internal class RawDFind : RawData
{
    public RawDFind(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public int Method { get; private set; }
    public int Reserved { get; private set; }
    public float Level { get; private set; }
    public float Azimuth { get; private set; }

    public float Quality { get; private set; }

    // 附带数据字节长度(只在收数复测中有效，此处不解析，附加数据协议格式尚未定义)
    public short ExtraSize { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        Method = BitConverter.ToInt32(value, offset);
        offset += 4;
        Reserved = BitConverter.ToInt32(value, offset);
        offset += 4;
        Level = BitConverter.ToSingle(value, offset);
        offset += 4;
        Azimuth = BitConverter.ToSingle(value, offset);
        offset += 4;
        Quality = BitConverter.ToSingle(value, offset);
        offset += 4;
        ExtraSize = BitConverter.ToInt16(value, offset);
        offset += 2;
        // 后续一段固定长度的字节对结果没有意义，直接将解析偏移值往后移动
        offset += ExtraSize;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, mt={Method}, lev={Level}, azi={Azimuth}, qua={Quality}";
    }
}

[Serializable]
internal class RawDfPan : RawData
{
    public RawDfPan(int packetVersion = 0) : base(packetVersion)
    {
    }

    public long Frequency { get; private set; }
    public long Bandwidth { get; private set; }
    public int Method { get; private set; }
    public int Count { get; private set; }
    public short[] LevelCollection { get; private set; }
    public short[] AzimuthCollecton { get; private set; }
    public short[] QualityCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Frequency = BitConverter.ToInt64(value, offset);
        offset += 8;
        Bandwidth = BitConverter.ToInt64(value, offset);
        offset += 8;
        Method = BitConverter.ToInt32(value, offset);
        offset += 4;
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        LevelCollection = new short[Count];
        Buffer.BlockCopy(value, offset, LevelCollection, 0, 2 * Count);
        offset += 2 * Count;
        AzimuthCollecton = new short[Count];
        Buffer.BlockCopy(value, offset, AzimuthCollecton, 0, 2 * Count);
        offset += 2 * Count;
        QualityCollection = new short[Count];
        Buffer.BlockCopy(value, offset, QualityCollection, 0, 2 * Count);
        offset += 2 * Count;
        return offset;
    }

    public override string ToString()
    {
        return
            $"{base.ToString()}, freq={Frequency}, bw={Bandwidth}, mt={Method}, cnt={Count}, lev_len={LevelCollection.Length}, azi_len={AzimuthCollecton.Length}, qua_len={QualityCollection.Length}";
    }
}

[Serializable]
internal class RawDdc : RawData
{
    public RawDdc(int packetVersion = 0) : base(packetVersion)
    {
    }

    public uint EnabledChannels { get; private set; }
    public int DdcSize { get; private set; }
    public List<RawSubDdc> DdcCollection { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        EnabledChannels = BitConverter.ToUInt32(value, offset);
        offset += 4;
        DdcSize = BitConverter.ToInt32(value, offset);
        offset += 4;
        DdcCollection = new List<RawSubDdc>();
        while (offset < value.Length)
        {
            var subDdc = RawSubDdc.Parse(value, ref offset);
            if (subDdc == null) break;
            DdcCollection.Add(subDdc);
        }

        return offset;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, enabled_chn_cnt={DdcCollection.Count}, sub_chn_size={DdcSize}";
    }
}

[Serializable]
internal class RawSubDdc
{
    public short Level { get; private set; }
    public short Count { get; private set; }
    public int Beta { get; private set; }
    public int XdB { get; private set; }
    public short FrequencyDeviation { get; private set; }
    public short Demodulation { get; private set; }
    public short[] Spectrum { get; private set; }

    public static RawSubDdc Parse(byte[] value, ref int offset)
    {
        try
        {
            var subDdc = new RawSubDdc
            {
                Level = BitConverter.ToInt16(value, offset)
            };
            offset += 2;
            subDdc.Count = BitConverter.ToInt16(value, offset);
            offset += 2;
            subDdc.Beta = BitConverter.ToInt32(value, offset);
            offset += 4;
            subDdc.XdB = BitConverter.ToInt32(value, offset);
            offset += 4;
            subDdc.FrequencyDeviation = BitConverter.ToInt16(value, offset);
            offset += 2;
            subDdc.Demodulation = BitConverter.ToInt16(value, offset);
            offset += 2;
            subDdc.Spectrum = new short[subDdc.Count];
            Buffer.BlockCopy(value, offset, subDdc.Spectrum, 0, sizeof(short) * subDdc.Count);
            offset += sizeof(short) * subDdc.Count;
            return subDdc;
        }
        catch
        {
            return null;
        }
    }
}

[Serializable]
internal class RawGps : RawData
{
    public RawGps(int packetVersion = 0) : base(packetVersion)
    {
    }

    // GPS字符串文本包含的字节总数
    public int Count { get; private set; }

    // GPS原始NMEA0183协议字符串
    public string Text { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        Count = BitConverter.ToInt32(value, offset);
        offset += 4;
        Text = Encoding.ASCII.GetString(value, offset, Count);
        offset += Count;
        return offset;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, text=\"{Text.Replace('\r', ';').Replace('\n', ' ')}\"";
    }
}

[Serializable]
internal class RawCompass : RawData
{
    public RawCompass(int packetVersion = 0) : base(packetVersion)
    {
    }

    // 是否为校准数据
    public int IsCalibration { get; private set; }

    // 方位角
    public int Heading { get; private set; }

    // 俯仰角
    public int Pitch { get; private set; }

    // 横滚角
    public int Rolling { get; private set; }

    public override int Convert(byte[] value, int offset)
    {
        offset = base.Convert(value, offset);
        IsCalibration = BitConverter.ToInt32(value, offset);
        offset += 4;
        Heading = BitConverter.ToInt32(value, offset);
        offset += 4;
        Pitch = BitConverter.ToInt32(value, offset);
        offset += 4;
        Rolling = BitConverter.ToInt32(value, offset);
        offset += 4;
        return offset;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, heading=\"{Heading}\", rolling=\"{Rolling}\", pitch=\"{Pitch}\"";
    }
}

#endregion