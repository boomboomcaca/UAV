using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Magneto.Device.DDF5GTS;

public partial class Ddf5Gts
{
    /// <summary>
    ///     返回的数据类型
    /// </summary>
    [Flags]
    public enum DataType
    {
        Fscan = 101,
        Mscan = 201,
        Audio = 401,
        Ifpan = 501,
        Cw = 801,
        If = 901,
        Video = 1001,
        Vdpan = 1101,
        Pscan = 1201,
        SeleCall = 1301,
        Dfpan = 1401,
        Pifpan = 1601,
        GpsCompass = 1801,
        AntLevel = 1901,
        DfpScan = 5301,
        Sigp = 5501,
        Hrpan = 5601,
        LastTag
    }

    /// <summary>
    ///     SelectorFlags
    /// </summary>
    [Flags]
    public enum Flags : uint
    {
        /// <summary>
        ///     1/10 dBμV
        /// </summary>
        Level = 0x1,

        /// <summary>
        ///     Hz
        /// </summary>
        Offset = 0x2,

        /// <summary>
        ///     1/10 dBμV/m
        /// </summary>
        Fstrength = 0x4,

        /// <summary>
        ///     1/10 %
        /// </summary>
        Am = 0x8,

        /// <summary>
        ///     1/10 %
        /// </summary>
        AmPos = 0x10,

        /// <summary>
        ///     1/10 %
        /// </summary>
        AmNeg = 0x20,

        /// <summary>
        ///     Hz
        /// </summary>
        Fm = 0x40,

        /// <summary>
        ///     Hz
        /// </summary>
        FmPos = 0x80,

        /// <summary>
        ///     Hz
        /// </summary>
        FmNeg = 0x100,

        /// <summary>
        ///     1/100 rad
        /// </summary>
        Pm = 0x200,

        /// <summary>
        ///     Hz
        /// </summary>
        Bandwidth = 0x400,

        /// <summary>
        ///     1/10 dBμV
        /// </summary>
        DfLevel = 0x800,

        /// <summary>
        ///     1/10 °
        /// </summary>
        Azimuth = 0x1000,

        /// <summary>
        ///     1/10 %
        /// </summary>
        DfQuality = 0x2000,

        /// <summary>
        ///     1/10 dBμV/m
        /// </summary>
        DfFstrength = 0x4000,

        /// <summary>
        ///     1/10 dBμV
        /// </summary>
        DfLevelCont = 0x8000,
        Channel = 0x00010000,
        Freqlow = 0x00020000,

        /// <summary>
        ///     1/10 °
        /// </summary>
        Elevation = 0x00040000,
        DfChannelStatus = 0x80000,

        /// <summary>
        ///     1/10 °
        /// </summary>
        DfOmniphase = 0x00100000,
        Freqhigh = 0x00200000,
        BandwidthCenter = 0x00400000,
        FreqOffsetRel = 0x00800000,
        Private = 0x10000000,
        Swap = 0x20000000, // swap ON means: do NOT swap (for little endian machines)
        SignalGreaterSquelch = 0x40000000,
        OptionalHeader = 0x80000000
    }

    [Flags]
    public enum SrFlags : short
    {
        Eigenvalue = 0x01,
        Level = 0x02,
        Azimuth = 0x04,
        Quality = 0x08,
        Fstrength = 0x10,
        Elevation = 0x20,
        Wavestatus = 0x40
    }

    /// <summary>
    ///     测向数据
    /// </summary>
    internal readonly struct DfpScanData
    {
        public readonly uint DataCnt;

        /// <summary>
        ///     电平值 0.1dBμV
        /// </summary>
        public readonly short[] DfLevel;

        /// <summary>
        ///     示相角度 0.1°
        /// </summary>
        public readonly short[] Azimuth;

        /// <summary>
        ///     测向质量 0.1%
        /// </summary>
        public readonly short[] DfQuality;

        /// <summary>
        ///     测向场强 0.1dBμV/m
        /// </summary>
        public readonly short[] DfFstrength;

        /// <summary>
        ///     连续测向电平值?(DF level (continuous) 0.1dBμV
        /// </summary>
        public readonly short[] DfLevelCont;

        /// <summary>
        ///     海拔? 0.1°
        /// </summary>
        public readonly short[] Elevation;

        /// <summary>
        ///     测向通道状态
        ///     数组中每个值的每位代表的意义:
        ///     0:是不是第一个通道
        ///     1:DF level (continuous)是否有效
        ///     2:测向电平值是否有效
        ///     3:未使用
        ///     4:当前通道是否有效
        ///     5~15:对DDF550无效
        /// </summary>
        public readonly short[] DfChannelStatus;

        public readonly short[] DfOmniphase;

        /// <summary>
        ///     解析后的电平值 dbμV
        /// </summary>
        public readonly short[] LevelF;

        /// <summary>
        ///     解析后的示向度 °
        /// </summary>
        public readonly float[] AzimuthF;

        /// <summary>
        ///     解析后的测向质量 %
        /// </summary>
        public readonly float[] QualityF;

        /// <summary>
        ///     解析DFPScan数据结构
        ///     需要返回相对示向度
        /// </summary>
        /// <param name="dataCnt">本次发来数据的点数</param>
        /// <param name="buffer">数据包</param>
        /// <param name="startIndex">数据包解析数据的起始位置</param>
        /// <param name="selectorFlags">selectorFlags</param>
        /// <param name="logChannel">本包数据的第一个点在频段所有点的序号</param>
        /// <param name="scanRangeCnt">频段所有的点数</param>
        /// <param name="isLast">是否是最后一包数据</param>
        /// <param name="isCorrection">是否进行了方位校正</param>
        /// <param name="angleOffset">天线安装方位</param>
        public DfpScanData(uint dataCnt, byte[] buffer, ref int startIndex, ulong selectorFlags, uint logChannel,
            uint scanRangeCnt, bool isLast, bool isCorrection, int angleOffset)
        {
            var isOutOfRange = false;
            var isLessRange = false;
            if (isLast)
            {
                DataCnt = scanRangeCnt - logChannel;
                if (logChannel + dataCnt > scanRangeCnt)
                {
                    isOutOfRange = true;
                    isLessRange = false;
                }
                else if (logChannel + dataCnt < scanRangeCnt)
                {
                    isOutOfRange = false;
                    isLessRange = true;
                }
                else
                {
                    DataCnt = dataCnt;
                }
            }
            else
            {
                DataCnt = dataCnt;
            }

            DfLevel = null;
            Azimuth = null;
            DfQuality = null;
            DfFstrength = null;
            DfLevelCont = null;
            Elevation = null;
            DfChannelStatus = null;
            DfOmniphase = null;
            LevelF = null;
            AzimuthF = null;
            QualityF = null;
            var offset = startIndex;
            if ((selectorFlags & (uint)Flags.DfLevel) > 0)
            {
                DfLevel = new short[DataCnt];
                LevelF = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfLevel[i] = BitConverter.ToInt16(buffer, offset);
                    LevelF[i] = DfLevel[i];
                    if (DfLevel[i] == 2000 || DfLevel[i] == 1999)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = short.MinValue;
                    }

                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.Azimuth) > 0)
            {
                Azimuth = new short[DataCnt];
                AzimuthF = new float[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    Azimuth[i] = BitConverter.ToInt16(buffer, offset);
                    AzimuthF[i] = Azimuth[i] / 10f;
                    if (isCorrection) AzimuthF[i] -= angleOffset;
                    if (AzimuthF[i] < 0) AzimuthF[i] += 360;
                    if (Azimuth[i] == 0x7FFF || Azimuth[i] == 0x7FFE)
                    {
                        Azimuth[i] = short.MinValue;
                        AzimuthF[i] = float.MinValue;
                    }

                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.DfQuality) > 0)
            {
                DfQuality = new short[DataCnt];
                QualityF = new float[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfQuality[i] = BitConverter.ToInt16(buffer, offset);
                    QualityF[i] = DfQuality[i] / 10f;
                    if (DfQuality[i] == 0x7FFF || DfQuality[i] == 0x7FFE)
                    {
                        DfQuality[i] = short.MinValue;
                        QualityF[i] = float.MinValue;
                    }

                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.DfFstrength) > 0)
            {
                DfFstrength = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfFstrength[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfFstrength[i] == 0x7FFF || DfFstrength[i] == 0x7FFE) DfFstrength[i] = short.MinValue;
                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.DfLevelCont) > 0)
            {
                DfLevelCont = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfLevelCont[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.Elevation) > 0)
            {
                Elevation = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    Elevation[i] = BitConverter.ToInt16(buffer, offset);
                    if (Elevation[i] == 0x7FFF || Elevation[i] == 0x7FFE) Elevation[i] = short.MinValue;
                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.DfChannelStatus) > 0)
            {
                DfChannelStatus = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfChannelStatus[i] = BitConverter.ToInt16(buffer, offset);
                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }

            if ((selectorFlags & (uint)Flags.DfOmniphase) > 0)
            {
                DfOmniphase = new short[DataCnt];
                for (var i = 0; i < dataCnt; i++)
                {
                    if (isOutOfRange && i + logChannel > scanRangeCnt)
                    {
                        offset += 2;
                        continue;
                    }

                    Array.Reverse(buffer, offset, 2);
                    DfOmniphase[i] = BitConverter.ToInt16(buffer, offset);
                    if (DfOmniphase[i] == 0x7FFF || DfOmniphase[i] == 0x7FFE) DfOmniphase[i] = short.MinValue;
                    offset += 2;
                }

                if (isLessRange)
                    for (var i = dataCnt; i < scanRangeCnt - logChannel; i++)
                    {
                        DfLevel[i] = 0;
                        LevelF[i] = 0;
                    }
            }
        }
    }

    /// <summary>
    ///     超分辨率测向数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal readonly struct SRdfData
    {
        public readonly uint DataCnt;
        public readonly uint EigenCnt;
        public readonly uint WaveCnt;
        public readonly float[,] Eigenvalue;
        public readonly float[,] Level;
        public readonly float[,] Azimuth;
        public readonly float[,] Quality;
        public readonly float[,] Fstrength;
        public readonly float[,] Elevation;
        public readonly ushort[,] WaveStatus;

        public SRdfData(uint dataCnt, uint eigenCnt, uint waveCnt, byte[] buffer, ref int offset, short srSelectorflags)
        {
            DataCnt = dataCnt;
            EigenCnt = eigenCnt;
            WaveCnt = waveCnt;
            Eigenvalue = null;
            Level = null;
            Azimuth = null;
            Quality = null;
            Fstrength = null;
            Elevation = null;
            WaveStatus = null;
            if ((srSelectorflags & (short)SrFlags.Eigenvalue) > 0)
            {
                Eigenvalue = new float[EigenCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < EigenCnt; j++)
                {
                    Array.Reverse(buffer, offset, 4);
                    Eigenvalue[j, i] = BitConverter.ToSingle(buffer, offset);
                    offset += 4;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Level) > 0)
            {
                Level = new float[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Level[j, i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Azimuth) > 0)
            {
                Azimuth = new float[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Azimuth[j, i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Quality) > 0)
            {
                Quality = new float[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Quality[j, i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Fstrength) > 0)
            {
                Fstrength = new float[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Fstrength[j, i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Elevation) > 0)
            {
                Elevation = new float[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    Elevation[j, i] = BitConverter.ToInt16(buffer, offset) / 10f;
                    offset += 2;
                }
            }

            if ((srSelectorflags & (short)SrFlags.Wavestatus) > 0)
            {
                WaveStatus = new ushort[WaveCnt, DataCnt];
                for (var i = 0; i < DataCnt; i++)
                for (var j = 0; j < WaveCnt; j++)
                {
                    Array.Reverse(buffer, offset, 2);
                    WaveStatus[j, i] = BitConverter.ToUInt16(buffer, offset);
                    offset += 2;
                }
            }
        }
    }

    #region 数据协议

    [Serializable]
    internal sealed class RawPacket
    {
        [MarshalAs(UnmanagedType.U4)] public uint DataSize;

        [MarshalAs(UnmanagedType.U4)] public uint MagicNumber;

        [MarshalAs(UnmanagedType.U2)] public ushort SequenceNumberHigh;

        [MarshalAs(UnmanagedType.U2)] public ushort SequenceNumberLow;

        [MarshalAs(UnmanagedType.U2)] public ushort VersionMajor;

        [MarshalAs(UnmanagedType.U2)] public ushort VersionMinor;

        private RawPacket()
        {
        }

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
                        offset += data.GenericAttribute.Size;
                        offset += (int)data.DataLength;
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

    internal interface IGenericAttribute
    {
        ushort TraceTag { get; }
        uint DataLength { get; }
        int Size { get; }
        void Parse(ushort traceTag, byte[] value, ref int offset);
    }

    internal class GenericAttributeConventional : IGenericAttribute
    {
        [MarshalAs(UnmanagedType.U2)] public ushort Length;

        [MarshalAs(UnmanagedType.U2)] public ushort Tag;

        public ushort TraceTag => Tag;
        public uint DataLength => Length;
        public int Size => 4;

        public void Parse(ushort traceTag, byte[] value, ref int offset)
        {
            Tag = traceTag;
            Array.Reverse(value, offset, 2);
            Length = BitConverter.ToUInt16(value, offset);
            offset += 2;
        }
    }

    internal class GenericAttributeAdvanced : IGenericAttribute
    {
        [MarshalAs(UnmanagedType.U4)] public uint Length;

        [MarshalAs(UnmanagedType.U2)] public ushort Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] ReservedArray;

        [MarshalAs(UnmanagedType.U2)] public ushort Tag;

        public int Size => 24;
        public ushort TraceTag => Tag;
        public uint DataLength => Length;

        public void Parse(ushort traceTag, byte[] value, ref int offset)
        {
            Tag = traceTag;
            Array.Reverse(value, offset, 2);
            Reserved = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            Length = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            var reserved0 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            var reserved1 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            var reserved2 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            var reserved3 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            ReservedArray = new[] { reserved0, reserved1, reserved2, reserved3 };
        }
    }

    internal interface ITraceAttribute
    {
        uint NumberOfItems { get; }
        uint HeaderLength { get; }
        ulong FlagsTrace { get; }
        int Size { get; }
        void Parse(byte[] value, ref int offset);
    }

    public class TraceAttributeConventional : ITraceAttribute
    {
        [MarshalAs(UnmanagedType.U1)] public byte ChannelNumber;

        [MarshalAs(UnmanagedType.I2)] public short NumberOfTraceItems;

        [MarshalAs(UnmanagedType.U1)] public byte OptionalHeaderLength;

        [MarshalAs(UnmanagedType.U4)] public uint SelectorFlags;

        public uint NumberOfItems => (uint)NumberOfTraceItems;
        public uint HeaderLength => OptionalHeaderLength;
        public ulong FlagsTrace => SelectorFlags;
        public int Size => 8;

        public void Parse(byte[] value, ref int offset)
        {
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
    }

    /// <summary>
    ///     TraceAttribute 高级版本
    /// </summary>
    public class TraceAttributeAdvanced : ITraceAttribute
    {
        [MarshalAs(UnmanagedType.U4)] public uint NumberOfTraceItems;

        [MarshalAs(UnmanagedType.U4)] public uint OptionalHeaderLength;

        [MarshalAs(UnmanagedType.U4)] public uint Reserved1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Reserved2;

        [MarshalAs(UnmanagedType.U4)] public uint SelectorFlagsHigh;

        [MarshalAs(UnmanagedType.U4)] public uint SelectorFlagsLow;

        public uint NumberOfItems => NumberOfTraceItems;
        public uint HeaderLength => OptionalHeaderLength;
        public ulong FlagsTrace => ((ulong)SelectorFlagsHigh << 32) + SelectorFlagsLow;
        public int Size => 36;

        public void Parse(byte[] value, ref int offset)
        {
            Array.Reverse(value, offset, 4);
            NumberOfTraceItems = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Reserved1 = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            OptionalHeaderLength = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            SelectorFlagsLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            SelectorFlagsHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Reserved2 = new uint[4];
            for (var i = 0; i < 4; i++)
            {
                Array.Reverse(value, offset, 4);
                Reserved2[i] = BitConverter.ToUInt32(value, offset);
                offset += 4;
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
        public uint DataLength { get; private set; }
        public int NumberOfTraceItems { get; private set; }
        public int OptionalHeaderLength { get; private set; }
        public ulong SelectorFlags { get; private set; }
        public IGenericAttribute GenericAttribute { get; private set; }
        public ITraceAttribute TraceAttribute { get; private set; }

        public static RawData Parse(byte[] value, int offset)
        {
            Array.Reverse(value, offset, 2);
            var tag = (DataType)BitConverter.ToUInt16(value, offset);
            Array.Reverse(value, offset, 2);
            var raw = tag switch
            {
                DataType.If => new RawIf(),
                DataType.Cw => new RawCw(),
                DataType.Ifpan => new RawIfPan(),
                DataType.DfpScan => new RawDfPscan(),
                DataType.Audio => new RawAudio(),
                // PSCAN
                DataType.Pscan => new RawPScan(),
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
            Array.Reverse(value, offset, 2);
            Tag = BitConverter.ToUInt16(value, offset);
            offset += 2;
            if (Tag >= 5000)
            {
                GenericAttribute = new GenericAttributeAdvanced();
                TraceAttribute = new TraceAttributeAdvanced();
            }
            else
            {
                GenericAttribute = new GenericAttributeConventional();
                TraceAttribute = new TraceAttributeConventional();
            }

            GenericAttribute.Parse(Tag, value, ref offset);
            TraceAttribute.Parse(value, ref offset);
            DataLength = GenericAttribute.DataLength;
            NumberOfTraceItems = (int)TraceAttribute.NumberOfItems; //应该不会溢出，溢出的情况数据量非常之大
            OptionalHeaderLength = (int)TraceAttribute.HeaderLength;
            SelectorFlags = TraceAttribute.FlagsTrace;
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
        [MarshalAs(UnmanagedType.I2)] public short CycleCount;

        [MarshalAs(UnmanagedType.I2)] public short DirectionUp;

        [MarshalAs(UnmanagedType.I2)] public short DwellTime;

        [MarshalAs(UnmanagedType.I2)] public short HoldTime;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.U2)] public ushort Reserved;

        [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyLow;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequency;

        [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyLow;

        [MarshalAs(UnmanagedType.I2)] public short StopSignal;

        public short[] DataCollection { get; private set; }
        public uint[] FreqLowCollection { get; private set; }
        public uint[] FreqHighCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 2);
            CycleCount = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            HoldTime = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            DwellTime = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            DirectionUp = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            StopSignal = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            StartFrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StopFrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StepFrequency = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StartFrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StopFrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            Reserved = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 8);
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
                            for (var i = 0; i < DataCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 2);
                                DataCollection[i] = BitConverter.ToInt16(value, offset);
                                offset += 2;
                            }

                            break;
                        }
                        case Flags.Freqlow:
                        {
                            FreqLowCollection = new uint[NumberOfTraceItems];
                            for (var i = 0; i < FreqLowCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 4);
                                FreqLowCollection[i] = BitConverter.ToUInt32(value, offset);
                                offset += 4;
                            }

                            break;
                        }
                        case Flags.Freqhigh:
                        {
                            FreqHighCollection = new uint[NumberOfTraceItems];
                            for (var i = 0; i < FreqHighCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 4);
                                FreqHighCollection[i] = BitConverter.ToUInt32(value, offset);
                                offset += 4;
                            }

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
        [MarshalAs(UnmanagedType.I2)] public short CycleCount;

        [MarshalAs(UnmanagedType.I2)] public short DirectionUp;

        [MarshalAs(UnmanagedType.I2)] public short DwellTime;

        [MarshalAs(UnmanagedType.I2)] public short HoldTime;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.I2)] public short StopSignal;

        public ulong StartFrequency { get; private set; }
        public ulong StopFrequency { get; private set; }
        public short[] DataCollection { get; private set; }
        public uint[] FreqLowCollection { get; private set; }
        public uint[] FreqHighCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 2);
            CycleCount = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            HoldTime = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            DwellTime = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            DirectionUp = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            StopSignal = BitConverter.ToInt16(value, offset);
            offset += 2;
            Reserved = new byte[6];
            offset += 6;
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 8);
            StartFrequency = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 8);
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
                            for (var i = 0; i < DataCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 2);
                                DataCollection[i] = BitConverter.ToInt16(value, offset);
                                offset += 2;
                            }

                            break;
                        }
                        case Flags.Freqlow:
                        {
                            FreqLowCollection = new uint[NumberOfTraceItems];
                            for (var i = 0; i < FreqLowCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 4);
                                FreqLowCollection[i] = BitConverter.ToUInt32(value, offset);
                                offset += 4;
                            }

                            break;
                        }
                        case Flags.Freqhigh:
                        {
                            FreqHighCollection = new uint[NumberOfTraceItems];
                            for (var i = 0; i < FreqHighCollection.Length; ++i)
                            {
                                Array.Reverse(value, offset, 4);
                                FreqHighCollection[i] = BitConverter.ToUInt32(value, offset);
                                offset += 4;
                            }

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
        [MarshalAs(UnmanagedType.U8)] public ulong FreqOfFirstStep;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint StartFrequencyLow;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequency;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyDenominator;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyNumerator;

        [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint StopFrequencyLow;

        public short[] DataCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 4);
            StartFrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StopFrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StepFrequency = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StartFrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StopFrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Reserved = new byte[4];
            offset += 4;
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 4);
            StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 8);
            FreqOfFirstStep = BitConverter.ToUInt64(value, offset);
            //offset += 8;
            offset = startIndex + OptionalHeaderLength;
            DataCollection = new short[NumberOfTraceItems];
            for (var i = 0; i < DataCollection.Length; ++i)
            {
                Array.Reverse(value, offset, 2);
                DataCollection[i] = BitConverter.ToInt16(value, offset);
                offset += 2;
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
        [MarshalAs(UnmanagedType.I2)] public short AudioMode;

        [MarshalAs(UnmanagedType.U4)] public uint Bandwidth;

        [MarshalAs(UnmanagedType.U2)]
        public ushort Demodulation; //FM:0, AM:1, PULS:2, PM:3, IQ:4, ISB:5, CW:6, USB:7, LSB:8, TV:9

        [MarshalAs(UnmanagedType.I2)] public short FrameLen;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 6)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SDemodulation;

        [MarshalAs(UnmanagedType.I2)] public short SignalSource;

        public byte[] DataCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 2);
            AudioMode = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            FrameLen = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            FrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Bandwidth = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            Demodulation = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 8);
            SDemodulation = BitConverter.ToString(value, offset, 8);
            offset += 8;
            Array.Reverse(value, offset, 4);
            FrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Reserved = new byte[6];
            offset += 6;
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 2);
            SignalSource = BitConverter.ToInt16(value, offset);
            //offset += 2;
            offset = startIndex + OptionalHeaderLength;
            Consts.AfModes.TryGetValue(AudioMode, out _);
            var audio = new byte[NumberOfTraceItems * FrameLen];
            for (var i = 0; i < NumberOfTraceItems; i++) Array.Reverse(value, offset + i * FrameLen, FrameLen);
            Buffer.BlockCopy(value, offset, audio, 0, audio.Length);
            DataCollection = audio;
            offset += audio.Length;
            return offset;
        }
    }

    /// <summary>
    ///     optional_header_length is thus either 0 or 60.
    /// </summary>
    [Serializable]
    internal class RawIfPan : RawData
    {
        [MarshalAs(UnmanagedType.I2)] public short AverageTime; //Not used and always set to 0

        [MarshalAs(UnmanagedType.I2)] public short AverageType;

        [MarshalAs(UnmanagedType.I4)] public int DemodFreqChannel;

        [MarshalAs(UnmanagedType.U4)] public uint DemodFreqHigh;

        [MarshalAs(UnmanagedType.U4)] public uint DemodFreqLow;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;

        [MarshalAs(UnmanagedType.I2)] public short MeasureMode;

        [MarshalAs(UnmanagedType.U4)] public uint MeasureTime; //us

        [MarshalAs(UnmanagedType.U8)] public ulong MeasureTimestamp;

        //[MarshalAs(UnmanagedType.U8)]
        //public ulong DemodFreq;
        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.I2)] public short SignalSource;

        [MarshalAs(UnmanagedType.U4)] public uint SpanFrequency;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyDenominator;

        [MarshalAs(UnmanagedType.U4)] public uint StepFrequencyNumerator;

        public short[] DataCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 4);
            FrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            SpanFrequency = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            AverageTime = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            AverageType = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            MeasureTime = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            FrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            DemodFreqChannel = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            DemodFreqLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            DemodFreqHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 4);
            StepFrequencyNumerator = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            StepFrequencyDenominator = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            SignalSource = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            MeasureMode = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 8);
            MeasureTimestamp = BitConverter.ToUInt64(value, offset);
            offset = startIndex + OptionalHeaderLength;
            DataCollection = new short[NumberOfTraceItems];
            for (var i = 0; i < DataCollection.Length; ++i)
            {
                Array.Reverse(value, offset, 2);
                DataCollection[i] = BitConverter.ToInt16(value, offset);
                offset += 2;
            }

            return offset;
        }
    }

    /// <summary>
    ///     With this trace type, as with "FScanTrace", all data specified in the "selectorFlags" is relevant.
    /// </summary>
    [Serializable]
    internal class RawCw : RawData
    {
        [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        [MarshalAs(UnmanagedType.I2)] public short SignalSource;

        public short[] Level { get; private set; }
        public int[] FreqOffset { get; private set; }
        public short[] FStrength { get; private set; }
        public short[] AmDepth { get; private set; }
        public short[] AmDepthPos { get; private set; }
        public short[] AmDepthNeg { get; private set; }
        public int[] FmDev { get; private set; }
        public int[] FmDevPos { get; private set; }
        public int[] FmDevNeg { get; private set; }
        public short[] PmDepth { get; private set; }
        public int[] BandWidth { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 4);
            FrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            FrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 2);
            SignalSource = BitConverter.ToInt16(value, offset);
            //offset += 2;
            offset = startIndex + OptionalHeaderLength;
            ParseData(value, NumberOfTraceItems, offset);
            offset += NumberOfTraceItems;
            return offset;
        }

        private void ParseData(byte[] buffer, int dataCnt, int index)
        {
            Level = new short[dataCnt];
            FreqOffset = new int[dataCnt];
            FStrength = new short[dataCnt];
            AmDepth = new short[dataCnt];
            AmDepthPos = new short[dataCnt];
            AmDepthNeg = new short[dataCnt];
            FmDev = new int[dataCnt];
            FmDevPos = new int[dataCnt];
            FmDevNeg = new int[dataCnt];
            PmDepth = new short[dataCnt];
            BandWidth = new int[dataCnt];
            for (var i = 0; i < dataCnt; i++)
            {
                Array.Reverse(buffer, index, 2);
                Level[i] = BitConverter.ToInt16(buffer, index);
                if (Level[i] == 2000 || Level[i] == 1999) Level[i] = short.MinValue;
                index += 2;
            }

            for (var i = 0; i < dataCnt; i++)
            {
                Array.Reverse(buffer, index, 4);
                FreqOffset[i] = BitConverter.ToInt32(buffer, index);
                if (FreqOffset[i] == 10000000 || FreqOffset[i] == 9999999) FreqOffset[i] = int.MinValue;
                index += 4;
            }

            if ((SelectorFlags & (uint)Flags.Fstrength) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 2);
                    FStrength[i] = BitConverter.ToInt16(buffer, index);
                    if (FStrength[i] == 0x7FFF || FStrength[i] == 0x7FFE) FStrength[i] = short.MinValue;
                    index += 2;
                }

            if ((SelectorFlags & (uint)Flags.Am) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 2);
                    AmDepth[i] = BitConverter.ToInt16(buffer, index);
                    if (AmDepth[i] == 0x7FFF || AmDepth[i] == 0x7FFE) AmDepth[i] = short.MinValue;
                    index += 2;
                }

            if ((SelectorFlags & (uint)Flags.AmPos) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 2);
                    AmDepthPos[i] = BitConverter.ToInt16(buffer, index);
                    index += 2;
                }

            if ((SelectorFlags & (uint)Flags.AmNeg) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 2);
                    AmDepthNeg[i] = BitConverter.ToInt16(buffer, index);
                    index += 2;
                }

            if ((SelectorFlags & (uint)Flags.Fm) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 4);
                    FmDev[i] = BitConverter.ToInt32(buffer, index);
                    if (FmDev[i] == 0x7FFFFFFF || FmDev[i] == 0x7FFFFFFE) FmDev[i] = int.MinValue;
                    index += 4;
                }

            if ((SelectorFlags & (uint)Flags.FmPos) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 4);
                    FmDevPos[i] = BitConverter.ToInt32(buffer, index);
                    index += 4;
                }

            if ((SelectorFlags & (uint)Flags.FmNeg) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 4);
                    FmDevNeg[i] = BitConverter.ToInt32(buffer, index);
                    index += 4;
                }

            if ((SelectorFlags & (uint)Flags.Pm) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 2);
                    PmDepth[i] = BitConverter.ToInt16(buffer, index);
                    if (PmDepth[i] == 0x7FFF || PmDepth[i] == 0x7FFE) PmDepth[i] = short.MinValue;
                    index += 2;
                }

            if ((SelectorFlags & (uint)Flags.Bandwidth) > 0)
                for (var i = 0; i < dataCnt; i++)
                {
                    Array.Reverse(buffer, index, 4);
                    BandWidth[i] = BitConverter.ToInt32(buffer, index);
                    if (BandWidth[i] == 0x7FFFFFFF || BandWidth[i] == 0x7FFFFFFE) BandWidth[i] = int.MinValue;
                    index += 4;
                }
        }
    }

    [Serializable]
    internal class RawIf : RawData
    {
        [MarshalAs(UnmanagedType.U4)] public uint Bandwidth; //IF bandwidth

        [MarshalAs(UnmanagedType.U2)] public ushort Demodulation;

        [MarshalAs(UnmanagedType.U2)] public ushort FlagsIf;

        [MarshalAs(UnmanagedType.I2)] public short FrameLen;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyHigh;

        [MarshalAs(UnmanagedType.U4)] public uint FrequencyLow;

        [MarshalAs(UnmanagedType.I2)] public short KFactor;

        //SYSTem:IF:REMote:MODE OFF|SHORT|LONG
        [MarshalAs(UnmanagedType.I2)] public short Mode;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 4)]
        public byte[] Reserved;

        [MarshalAs(UnmanagedType.I2)] public short RxAtt;

        [MarshalAs(UnmanagedType.U8)] public ulong SampleCount;

        [MarshalAs(UnmanagedType.U4)] public uint Samplerate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string SDemodulation;

        [MarshalAs(UnmanagedType.I2)] public short SignalSource;

        [MarshalAs(UnmanagedType.U8)] public ulong StartTimestamp;

        public short[] DataCollection16 { get; private set; }
        public int[] DataCollection32 { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 2);
            Mode = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            FrameLen = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            Samplerate = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            FrequencyLow = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Bandwidth = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            Demodulation = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            RxAtt = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            FlagsIf = BitConverter.ToUInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            KFactor = BitConverter.ToInt16(value, offset);
            offset += 2;
            SDemodulation = BitConverter.ToString(value, offset, 8);
            offset += 8;
            Array.Reverse(value, offset, 8);
            SampleCount = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 4);
            FrequencyHigh = BitConverter.ToUInt32(value, offset);
            offset += 4;
            Reserved = new byte[4];
            offset += 4;
            Array.Reverse(value, offset, 8);
            StartTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 2);
            SignalSource = BitConverter.ToInt16(value, offset);
            //offset += 2;
            offset = startIndex + OptionalHeaderLength;
            if (Mode == 1)
            {
                DataCollection16 = new short[NumberOfTraceItems * 2];
                Buffer.BlockCopy(value, offset, DataCollection16, 0, NumberOfTraceItems * 2 * 2);
                offset += NumberOfTraceItems * 4;
            }
            else if (Mode == 2)
            {
                DataCollection32 = new int[NumberOfTraceItems * 2];
                Buffer.BlockCopy(value, offset, DataCollection32, 0, NumberOfTraceItems * 2 * 4);
                offset += NumberOfTraceItems * 8;
            }

            return offset;
        }
    }

    internal class RawDfPscan : RawData
    {
        ///// <summary>
        /////     解析后的示向度 °
        ///// </summary>
        //public float[] AzimuthF;

        [MarshalAs(UnmanagedType.R4)] public float Bandwidth;

        [MarshalAs(UnmanagedType.I4)] public int ChannelsInScanRange;

        [MarshalAs(UnmanagedType.I2)] public short CompassHeading;

        [MarshalAs(UnmanagedType.I2)] public short CompassHeadingType;

        //public uint DataCnt;

        ///// <summary>
        /////     测向通道状态
        /////     数组中每个值的每位代表的意义:
        /////     0:是不是第一个通道
        /////     1:DF level (continuous)是否有效
        /////     2:测向电平值是否有效
        /////     3:未使用
        /////     4:当前通道是否有效
        /////     5~15:对DDF550无效
        ///// </summary>
        //public short[] DfChannelStatus;

        ///// <summary>
        /////     测向场强 0.1dBμV/m
        ///// </summary>
        //public short[] DfFstrength;

        ///// <summary>
        /////     电平值 0.1dBμV
        ///// </summary>
        //public short[] DfLevel;

        ///// <summary>
        /////     连续测向电平值?(DF level (continuous) 0.1dBμV
        ///// </summary>
        //public short[] DfLevelCont;

        //public short[] DfOmniphase;

        ///// <summary>
        /////     测向质量 0.1%
        ///// </summary>
        //public short[] DfQuality;

        [MarshalAs(UnmanagedType.I4)] public int DfStatus;

        ///// <summary>
        /////     海拔? 0.1°
        ///// </summary>
        //public short[] Elevation;

        [MarshalAs(UnmanagedType.U8)] public ulong Frequency;

        [MarshalAs(UnmanagedType.I4)] public int FrequencyStepDenominator;

        [MarshalAs(UnmanagedType.I4)] public int FrequencyStepNumerator;

        [MarshalAs(UnmanagedType.U2)] public ushort JobId;

        ///// <summary>
        /////     解析后的电平值 dbμV
        ///// </summary>
        //public short[] LevelF;

        [MarshalAs(UnmanagedType.I4)] public int LogChannel;

        [MarshalAs(UnmanagedType.I2)] public short MeasureCount;

        [MarshalAs(UnmanagedType.I4)] public int MeasureTime;

        [MarshalAs(UnmanagedType.U8)] public ulong MeasureTimestamp;

        [MarshalAs(UnmanagedType.U1)] public byte NumberOfEigenvalues;

        ///// <summary>
        /////     解析后的测向质量 %
        ///// </summary>
        //public float[] QualityF;

        [MarshalAs(UnmanagedType.I2)] public short Reserved;

        [MarshalAs(UnmanagedType.I4)] public int ScanRangeId;

        [MarshalAs(UnmanagedType.I4)] public int Span;

        [MarshalAs(UnmanagedType.I2)] public short SrSelectorflags;

        [MarshalAs(UnmanagedType.U1)] public byte SrWaveCount;

        [MarshalAs(UnmanagedType.U8)] public ulong SweepTime;

        [MarshalAs(UnmanagedType.I2)] public short Threshold;

        public byte[] DataCollection { get; private set; }

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            var startIndex = offset;
            Array.Reverse(value, offset, 4);
            ScanRangeId = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            ChannelsInScanRange = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 8);
            Frequency = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 4);
            LogChannel = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            FrequencyStepNumerator = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            FrequencyStepDenominator = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Span = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            Bandwidth = BitConverter.ToSingle(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 4);
            MeasureTime = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 2);
            MeasureCount = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            Threshold = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            CompassHeading = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 2);
            CompassHeadingType = BitConverter.ToInt16(value, offset);
            offset += 2;
            Array.Reverse(value, offset, 4);
            DfStatus = BitConverter.ToInt32(value, offset);
            offset += 4;
            Array.Reverse(value, offset, 8);
            SweepTime = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 8);
            MeasureTimestamp = BitConverter.ToUInt64(value, offset);
            offset += 8;
            Array.Reverse(value, offset, 2);
            JobId = BitConverter.ToUInt16(value, offset);
            Array.Reverse(value, offset, 2);
            SrSelectorflags = BitConverter.ToInt16(value, offset);
            offset += 2;
            SrWaveCount = value[offset];
            offset++;
            NumberOfEigenvalues = value[offset];
            offset++;
            Array.Reverse(value, offset, 2);
            Reserved = BitConverter.ToInt16(value, offset);
            //offset += 2;
            offset = startIndex + OptionalHeaderLength;
            var dataSize = (int)(DataLength - OptionalHeaderLength - TraceAttribute.Size);
            DataCollection = new byte[dataSize];
            Buffer.BlockCopy(value, offset, DataCollection, 0, dataSize);
            offset += dataSize;
            return offset;
        }
    }

    internal class RawGpsCompass : RawData
    {
        /// <summary>
        ///     海拔高度(厘米)
        /// </summary>
        public int Altitude;

        public short AngularRatesValid;
        public short AntElevation;
        public short AntElevationExact;
        public short AntRoll;
        public short AntRollExact;
        public short AntTiltOver;
        public short AntValid;
        public ushort CompassHeading;

        /// <summary>
        ///     Heading type of the connected compass:
        ///     ● –1: undefined: no compass value available
        ///     ● 0: unknown: heading unknown
        ///     ● 1: compass: heading value uncorrected
        ///     ● 2: magnetic: heading value related to magnetic north (magnetic heading)
        ///     ● 3: true: heading value related to true (geographic) north (true heading)
        ///     ● 4: bad: heading value bad because movement too slow (only for GPS compass, see Chapter 3.5.1, "Definition of
        ///     Terms", on page 48)
        ///     ● 5: GPS: heading value derived from movement related to GPS
        ///     ● 6: GPS slow: heading value derived from movement related to GPS, but speed too low (1 m/ s)
        /// </summary>
        public short CompassHeadingType;

        public ulong CompassTimestamp;
        public float Dilution;
        public short ElevationAngularRate;
        public int GeoidalSeparation;
        public short GeoidalSeparationValid;

        /// <summary>
        ///     GPS时间刻度数(ns)
        /// </summary>
        public ulong GpsTimestamp;

        /// <summary>
        ///     GPS数据是否有效,1-有效,0-无效
        /// </summary>
        public short GpsValid;

        public short HeadingAngularRate;

        /// <summary>
        ///     纬度度数°
        /// </summary>
        public short LatDeg;

        /// <summary>
        ///     纬度分数′
        /// </summary>
        public float LatMin;

        /// <summary>
        ///     地理纬度方向(N/S)
        /// </summary>
        public short LatRef;

        /// <summary>
        ///     经度度数°
        /// </summary>
        public short LonDeg;

        /// <summary>
        ///     经度分数′
        /// </summary>
        public float LonMin;

        /// <summary>
        ///     地理经度方向(E/W)
        /// </summary>
        public short LonRef;

        public short MagneticDeclination;
        public short MagneticDeclinationSource;

        /// <summary>
        ///     GPS所接收的星数
        /// </summary>
        public short NoOfSatInView;

        [MarshalAs(UnmanagedType.U8)] public ulong OutputTimestamp;

        public float Pdop;
        public int Reserved;
        public short RollAngularRate;
        public short SignalSource;

        /// <summary>
        ///     对地速度(0.1m/s)
        /// </summary>
        public short SpeedOverGround;

        public short TrackMadeGood;
        public float Vdop;

        public override int Convert(byte[] value, int offset)
        {
            offset = base.Convert(value, offset);
            Array.Reverse(value, offset, 8);
            OutputTimestamp = BitConverter.ToUInt64(value, offset);
            var startIndex = offset;
            offset = startIndex + OptionalHeaderLength;
            ParseData(value, offset);
            offset += NumberOfTraceItems;
            return offset;
        }

        private void ParseData(byte[] buffer, int offset)
        {
            Array.Reverse(buffer, offset, 2);
            CompassHeading = BitConverter.ToUInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            CompassHeadingType = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            GpsValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            NoOfSatInView = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LatRef = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LatDeg = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            LatMin = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            LonRef = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            LonDeg = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            LonMin = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            Dilution = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            AntValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntTiltOver = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntElevation = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntRoll = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            SignalSource = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AngularRatesValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            HeadingAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            ElevationAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            RollAngularRate = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            GeoidalSeparationValid = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            GeoidalSeparation = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            Altitude = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 2);
            SpeedOverGround = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            TrackMadeGood = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 4);
            Pdop = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 4);
            Vdop = BitConverter.ToSingle(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 8);
            GpsTimestamp = BitConverter.ToUInt64(buffer, offset);
            offset += 8;
            Array.Reverse(buffer, offset, 4);
            Reserved = BitConverter.ToInt32(buffer, offset);
            offset += 4;
            Array.Reverse(buffer, offset, 8);
            CompassTimestamp = BitConverter.ToUInt64(buffer, offset);
            offset += 8;
            Array.Reverse(buffer, offset, 2);
            MagneticDeclinationSource = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            MagneticDeclination = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntRollExact = BitConverter.ToInt16(buffer, offset);
            offset += 2;
            Array.Reverse(buffer, offset, 2);
            AntElevationExact = BitConverter.ToInt16(buffer, offset);
        }
    }

    #endregion
}