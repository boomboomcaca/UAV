using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.E3238S;

public struct E3238SCommandTag
{
    public const int E3238SCertifiedSocketCommand = 96;
    public const int E3238SCertifiedSocketResponse = 97;
    public const int E3238SCertifiedSocketQuery = 98;
    public const int E3238SSocketCommand = 99;
    public const int SpewE3238SEnergyTag = 100;
    public const int SpewE3238SNewEnergyTag = 101;
    public const int SpewE3238SEnergyAlarmTag = 102;
    public const int SpewE3238SAlarmTag = 102;
    public const int SpewE3238SHandoffTag = 103;
    public const int SpewE3238SRawDataTag = 104;
    public const int SpewE3238STimeSnapshotFilenameTag = 105;
    public const int SpewE3238SFreqSnapshotFilenameTag = 106;
    public const int SpewE3238STerminationTag = 107;
    public const int SpewE3238STunerInfoTag = 108;
    public const int SpewE3238SRcvrAssignmentTag = 109;
    public const int SpewE3238SSearchSetupChangedTag = 110;
    public const int SpewE3238SOverloadTag = 111;
    public const int SpewE3238SUserTraceVectorTag = 112;
    public const int SpewE3238SSignalAlarmTag = 113;
    public const int SpewE3238SSweepStatusTag = 114;
    public const int SpewE3238SEndOfSweepTag = 115;
    public const int SpewE3238SRawTimeDataTag = 116;
    public const int SpewE3238SDfTag = 117;
    public const int SpewE3238SModRecTag = 118;
    public const int SpewE3238SErrorTag = 119;
    public const int SpewE3238SThresholdTag = 120;
    public const int SpewE3238SEnergyUdpPort = 0;
    public const int SpewE3238SNewEnergyUdpPort = 1;
    public const int SpewE3238SEnergyAlarmUdpPort = 2;
    public const int SpewE3238SAlarmUdpPort = 2;
    public const int SpewE3238SHandoffUdpPort = 3;
    public const int SpewE3238SRawDataUdpPort = 4;
    public const int SpewE3238STimeSnapshotFilenameUdpPort = 5;
    public const int SpewE3238SFreqSnapshotFilenameUdpPort = 6;
    public const int SpewE3238STerminationUdpPort = 7;
    public const int SpewE3238STunerInfoUdpPort = 8;
    public const int SpewE3238SRcvrAssignmentUdpPort = 9;
    public const int SpewE3238SSearchSetupChangedUdpPort = 10;
    public const int SpewE3238SOverloadUdpPort = 11;
    public const int SpewE3238SUserTraceVectorUdpPort = 12;
    public const int SpewE3238SSignalAlarmUdpPort = 13;
    public const int SpewE3238SSweepStatusUdpPort = 14;
    public const int SpewE3238SEndOfSweepUdpPort = 15;
    public const int SpewE3238SRawTimeDataUdpPort = 16;
    public const int SpewE3238SDfUdpPort = 17;
    public const int SpewE3238SModRecUdpPort = 18;
    public const int SpewE3238SErrorUdpPort = 19;
    public const int NumSpewE3238SUdpPorts = 20;
    public const int QueryE3238SSearchModeTag = 200;
    public const int QueryE3238SGeneralSearchSetupTag = 201;
    public const int QueryE3238SDirectedSearchBandSetupTag = 202;
    public const int QueryE3238SFrequencyLimitsTag = 203;
    public const int QueryE3238STraceScaleTag = 204;
    public const int QueryE3238SMarkerValueTag = 205;
    public const int QueryE3238SSweepTimeTag = 206;
    public const int QueryE3238SSweepCountTag = 207;
    public const int QueryE3238STunerInfoTag = 208;
    public const int QueryE3238SEnergyDetectionTag = 209;
    public const int QueryE3238SUserTraceVectorTag = 210;
    public const int QueryE3238SSpewPortTag = 211;
    public const int QueryE3238SSweepStatusTag = 212;
    public const int QueryE3238SAntennaTag = 213;
    public const int QueryE3238SSweepRangeTag = 214;
    public const int QueryE3238SHwInfoTag = 215;
    public const int QueryE3238SHostInfoTag = 216;
    public const int QueryE3238SThresholdTag = 217;
    public const int QueryE3238SCommandTag = 218;
    public const int E3238SMaxChannels = 4;
    public const int E3238SCertifiedCommandVersion = 0;
    public const int E3238SCertifiedResponseVersion = 0;
    public const int E3238SCertifiedQueryVersion = 0;
    public const int SpewE3238SSweepStatusVersion = 0;
    public const int SpewE3238SEnergyVersion = 1;
    public const int AlarmSpewNameLength = 32;
    public const int AlarmSpewModeSignalPresent = 0;
    public const int AlarmSpewModeSignalNotPresent = 1;
    public const int AlarmSpewModeMultipleSignal = 2;
    public const int SpewE3238SAlarmVersion = 1;
    public const int SignalAlarmSpewNameLength = 32;
    public const int SignalAlarmSignalProcNameLength = 16;
    public const int SpewE3238SSignalAlarmVersion = 0;
    public const int SignalAlarmSpewModeFirst = 0;
    public const int SignalAlarmSpewModeIntermediate = 1;
    public const int SignalAlarmSpewModeLast = 2;
    public const int SignalAlarmSpewClassIdentification = 0;
    public const int SignalAlarmSpewClassLocation = 1;
    public const int SignalAlarmSpewClassCollection = 2;
    public const int SpewE3238SRawDataVersion = 2;
    public const int RawDataSpewEndOfSweepBit = 0x0001;
    public const int RawDataSpewOverloadBit = 0x0002;
    public const int SpewE3238SRawTimeDataVersion = 1;
    public const int RawTimeDataSpewEndOfSweepBit = 0x0001;
    public const int RawTimeDataSpewOverloadBit = 0x0002;
    public const int SpewE3238STunerLockVersion = 1;
    public const int SpewE3238SRcvrAssignmentVersion = 0;
    public const int QueryE3238SSearchModeVersion = 0;
    public const int SpewE3238SOverloadVersion = 1;
    public const int SpewQueryE3238SUserTraceVectorVersion = 2;
    public const int SpewE3238SEndOfSweepVersion = 0;
    public const int SpewE3238SErrorVersion = 0;
    public const int E3238SErrorNumInvalid = -1;
    public const int SpewE3238SDfVersion = 0;
    public const int SpewE3238SDfValidSiteLocation = 0x00000001;
    public const int SpewE3238SDfValidAzimuth = 0x00000002;
    public const int SpewE3238SDfValidElevation = 0x00000004;
    public const int SpewE3238SDfValidQuality = 0x00000008;
    public const int SpewE3238SDfValidLocation = 0x00000010;
    public const int SpewE3238SDfValidHeading = 0x00000020;
    public const int SpewE3238SDfValidDeclination = 0x00000040;
    public const int SpewE3238SDfValidUser1 = 0x00000080;
    public const int SpewE3238SDfValidUser2 = 0x00000100;
    public const int SpewE3238SDfValidUser3 = 0x00000200;
    public const int SpewE3238SDfValidUser4 = 0x00000400;
    public const int SpewE3238SModRecVersion = 0;
    public const int SpewE3238SModRecValidTimestamp = 0x00000001;
    public const int SpewE3238SModRecValidDuration = 0x00000002;
    public const int SpewE3238SModRecValidBandwidth = 0x00000004;
    public const int SpewE3238SModRecValidSymbolRate = 0x00000008;
    public const int SpewE3238SModRecValidDeviation = 0x00000010;
    public const int SpewE3238SModRecValidModTypeName = 0x00000020;
    public const int SpewE3238SModRecValidModType = 0x00000040;
    public const int SpewE3238SModRecValidConfidence = 0x00000080;
    public const int SpewE3238SModRecValidSnr = 0x00000100;
    public const int QueryE3238SGeneralSearchSetupVersion = 1;
    public const int QueryE3238SDirectedSearchBandSetupVersion = 1;
    public const int QueryE3238SFrequencyLimitsVersion = 0;
    public const int QueryE3238STraceScaleVersion = 0;
    public const int QueryE3238SMarkerValueVersion = 0;
    public const int QueryE3238SSweepTimeVersion = 0;
    public const int QueryE3238SSweepCountVersion = 0;
    public const int QueryE3238STunerInfoVersion = 1;
    public const int QueryEdThresholdModeOff = 0;
    public const int QueryEdThresholdModeLevel = 1;
    public const int QueryEdThresholdModeAuto = 2;
    public const int QueryEdThresholdModeEnvironment = 3;
    public const int QueryEdThresholdModeFile = 4;
    public const int QueryEdThresholdModeChannel = 5;
    public const int QueryEdThresholdModeUser1 = 6;
    public const int QueryEdThresholdModeUser2 = 7;
    public const int QueryEdThresholdModeUser3 = 8;
    public const int QueryEdThresholdModeUser4 = 9;
    public const int QueryEdMaxFeatureGroups = 4;
    public const int QueryEdFeatureGroupNameLength = 32;
    public const int QueryEdMaxFilters = 5;
    public const int QueryEdFilterNameLength = 16;
    public const int QueryE3238SEnergyDetectionVersion = 1;
    public const int QueryE3238SSpewPortVersion = 0;
    public const int QueryE3238SSweepStatusVersion = 0;
    public const int QueryE3238SAntennaVersion = 0;
    public const int QueryE3238SSweepRangeVersion = 0;
    public const int QueryE3238SHwInfoVersion = 1;
    public const int QueryE3238SHostInfoVersion = 0;
}

#region 框架结构

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct CommonHeader
{
    [MarshalAs(UnmanagedType.I2)] public readonly short number_of_trace_items;
    [MarshalAs(UnmanagedType.I1)] public readonly sbyte reserved;
    [MarshalAs(UnmanagedType.U1)] public readonly byte optional_header_length;
    [MarshalAs(UnmanagedType.U4)] public readonly uint selectorFlags;

    public CommonHeader(byte[] buffer, int offset)
    {
        Array.Reverse(buffer, offset, 2);
        number_of_trace_items = BitConverter.ToInt16(buffer, offset);
        reserved = (sbyte)buffer[offset + 2];
        optional_header_length = buffer[offset + 3];
        Array.Reverse(buffer, offset + 4, 4);
        selectorFlags = BitConverter.ToUInt32(buffer, offset + 4);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct OptionalHeaderIfPan
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint frequency;
    [MarshalAs(UnmanagedType.U4)] public readonly uint spanFrequency;
    [MarshalAs(UnmanagedType.I2)] public readonly short reserved;
    [MarshalAs(UnmanagedType.I2)] public readonly short averageType;
    [MarshalAs(UnmanagedType.U4)] public readonly uint measureTime;

    public OptionalHeaderIfPan(byte[] buffer, int offset)
    {
        frequency = BitConverter.ToUInt32(buffer, offset);
        spanFrequency = BitConverter.ToUInt32(buffer, offset + 4);
        reserved = BitConverter.ToInt16(buffer, offset + 8);
        averageType = BitConverter.ToInt16(buffer, offset + 10);
        measureTime = BitConverter.ToUInt32(buffer, offset + 12);
    }
}

#endregion

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SCertifiedCommand
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int user_receipt;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SCertifiedResponse
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int user_receipt;
    [MarshalAs(UnmanagedType.I4)] public readonly int error;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SCertifiedQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int user_receipt;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSweepStatusSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int sweeping;
}

//单频测量数据结构
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SEnergySpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int timeStamp;
    [MarshalAs(UnmanagedType.R8)] public readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] public readonly double bandwidth;
    [MarshalAs(UnmanagedType.R4)] public readonly float amplitude;
    [MarshalAs(UnmanagedType.R4)] public readonly float timeStampSubSecond;

    public E3238SEnergySpew(byte[] buffer, int offset)
    {
        version = BitConverter.ToInt32(buffer, offset);
        timeStamp = BitConverter.ToInt32(buffer, offset + 4);
        frequency = BitConverter.ToDouble(buffer, offset + 12);
        bandwidth = BitConverter.ToDouble(buffer, offset + 20);
        amplitude = BitConverter.ToSingle(buffer, offset + 24);
        timeStampSubSecond = BitConverter.ToSingle(buffer, offset + 28);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SAlarmSpew
{
    [MarshalAs(UnmanagedType.I4)] internal readonly int version;
    [MarshalAs(UnmanagedType.I4)] internal readonly int alarm;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.AlarmSpewNameLength)]
    internal readonly byte[] name;

    [MarshalAs(UnmanagedType.I4)] internal readonly int timeStamp;
    [MarshalAs(UnmanagedType.I4)] internal readonly int mode;
    [MarshalAs(UnmanagedType.R8)] internal readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] internal readonly double bandwidth;
    [MarshalAs(UnmanagedType.R4)] internal readonly float amplitude;
    [MarshalAs(UnmanagedType.I4)] internal readonly int detections;
    [MarshalAs(UnmanagedType.R8)] internal readonly double duration;
    [MarshalAs(UnmanagedType.I4)] internal readonly int intercepts;
    [MarshalAs(UnmanagedType.R4)] internal readonly float timeStampSubSecond;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSignalAlarmSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int alarm;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.SignalAlarmSpewNameLength)]
    public readonly byte[] name; //[SIGNAL_ALARM_SPEW_NAME_LENGTH];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.SignalAlarmSignalProcNameLength)]
    public readonly byte[] signalProcName; //[SIGNAL_ALARM_SIGNAL_PROC_NAME_LENGTH];

    [MarshalAs(UnmanagedType.I4)] public readonly int signalProcClass;
    [MarshalAs(UnmanagedType.I4)] public readonly int mode;
    [MarshalAs(UnmanagedType.R8)] public readonly double timeStamp;
    [MarshalAs(UnmanagedType.R8)] public readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] public readonly double duration;
    [MarshalAs(UnmanagedType.R8)] public readonly double tipTag;
    [MarshalAs(UnmanagedType.I4)] public readonly int infoSize;
    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRawDataSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.I4)] public readonly int syncStatus;
    [MarshalAs(UnmanagedType.I4)] public readonly int segment;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRawDataSpewV1
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.I4)] public readonly int syncStatus;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRawDataSpewV0
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRawTimeDataSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.I4)] public readonly int dataType;
    [MarshalAs(UnmanagedType.R4)] public readonly float scalar;
    [MarshalAs(UnmanagedType.R8)] public readonly double centerFreq;
    [MarshalAs(UnmanagedType.R8)] public readonly double sampleRate;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.I4)] public readonly int syncStatus;
    [MarshalAs(UnmanagedType.I4)] public readonly int segment;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRawTimeDataSpewV0
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.I4)] public readonly int dataType;
    [MarshalAs(UnmanagedType.R4)] public readonly float scalar;
    [MarshalAs(UnmanagedType.R8)] public readonly double centerFreq;
    [MarshalAs(UnmanagedType.R8)] public readonly double sampleRate;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct FrequencyData
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.R4)] public readonly float startFrequency;
    [MarshalAs(UnmanagedType.R4)] public readonly float stopFrequency;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.I4)] public readonly int syncStatus;
    [MarshalAs(UnmanagedType.I4)] public readonly int segment;

    public FrequencyData(byte[] buffer)
    {
        var offset = 0;
        Array.Reverse(buffer, offset, 4);
        version = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        mask = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        startFrequency = BitConverter.ToSingle(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        stopFrequency = BitConverter.ToSingle(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        numPoints = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        channel = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        syncStatus = BitConverter.ToInt32(buffer, offset);
        offset += 4;
        Array.Reverse(buffer, offset, 4);
        segment = BitConverter.ToInt32(buffer, offset);
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct TimeData
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mask;
    [MarshalAs(UnmanagedType.I4)] public readonly int dataType;
    [MarshalAs(UnmanagedType.R4)] public readonly float scalar;
    [MarshalAs(UnmanagedType.R8)] public readonly double centerFreq;
    [MarshalAs(UnmanagedType.R8)] public readonly double sampleRate;
    [MarshalAs(UnmanagedType.I4)] public readonly int numPoints;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.I4)] public readonly int syncStatus;
    [MarshalAs(UnmanagedType.I4)] public readonly int segment;

    public TimeData(byte[] data)
    {
        var offset = 0;
        Array.Reverse(data, offset, 4);
        version = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        mask = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        dataType = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        scalar = BitConverter.ToSingle(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 8);
        centerFreq = BitConverter.ToDouble(data, offset);
        offset += 8;
        Array.Reverse(data, offset, 8);
        sampleRate = BitConverter.ToDouble(data, offset);
        offset += 8;
        Array.Reverse(data, offset, 4);
        numPoints = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        channel = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        syncStatus = BitConverter.ToInt32(data, offset);
        offset += 4;
        Array.Reverse(data, offset, 4);
        segment = BitConverter.ToInt32(data, offset);
        offset += 4;
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238STunerLockSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int tunerLocked;
    [MarshalAs(UnmanagedType.R4)] public readonly float adcFs;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.R4)] public readonly float cf1;
    [MarshalAs(UnmanagedType.R4)] public readonly float cf2;
    [MarshalAs(UnmanagedType.R4)] public readonly float ddcOffset;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SRcvrAssignmentSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int Rx;
    [MarshalAs(UnmanagedType.R4)] public readonly float frequency;
    [MarshalAs(UnmanagedType.R4)] public readonly float bandwidth;
    [MarshalAs(UnmanagedType.I4)] public readonly int detection;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSearchModeQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int mode;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SOverloadSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int adcOverload;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SOverloadSpewV0
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int adcOverload;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SUserTraceSegment16Bit
{
    [MarshalAs(UnmanagedType.U2)] public readonly ushort p1;
    [MarshalAs(UnmanagedType.U2)] public readonly ushort p2;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SUserTraceSegment8Bit
{
    [MarshalAs(UnmanagedType.U1)] internal readonly byte p1;
    [MarshalAs(UnmanagedType.U1)] internal readonly byte p2;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SUserTraceVectorSpewQuery
{
    [MarshalAs(UnmanagedType.R4)] public readonly float xMin;
    [MarshalAs(UnmanagedType.R4)] public readonly float xMax;
    [MarshalAs(UnmanagedType.R4)] public readonly float yMin;
    [MarshalAs(UnmanagedType.R4)] public readonly float yMax;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int height;
    [MarshalAs(UnmanagedType.I4)] public readonly int width;
    [MarshalAs(UnmanagedType.I4)] public readonly int port;
    [MarshalAs(UnmanagedType.I4)] public readonly int dataMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int channel;
    [MarshalAs(UnmanagedType.R8)] public readonly double timeStamp;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SEndOfSweepSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SErrorSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int errorNum;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SDfSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int valid;
    [MarshalAs(UnmanagedType.R8)] public readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] public readonly double timeStamp;
    [MarshalAs(UnmanagedType.R4)] public readonly float duration;
    [MarshalAs(UnmanagedType.R4)] public readonly float site_latitude;
    [MarshalAs(UnmanagedType.R4)] public readonly float site_longitude;
    [MarshalAs(UnmanagedType.R4)] public readonly float bandwidth;
    [MarshalAs(UnmanagedType.R4)] public readonly float quality;
    [MarshalAs(UnmanagedType.R4)] public readonly float azimuth;
    [MarshalAs(UnmanagedType.R4)] public readonly float heading;
    [MarshalAs(UnmanagedType.R4)] public readonly float declination;
    [MarshalAs(UnmanagedType.R4)] public readonly float latitude;
    [MarshalAs(UnmanagedType.R4)] public readonly float longitude;
    [MarshalAs(UnmanagedType.R4)] public readonly float elevation;
    [MarshalAs(UnmanagedType.R4)] public readonly float user1;
    [MarshalAs(UnmanagedType.R4)] public readonly float user2;
    [MarshalAs(UnmanagedType.R4)] public readonly float user3;
    [MarshalAs(UnmanagedType.R4)] public readonly float user4;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 28)]
    private readonly byte[] pad; //[28];
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SModRecSpew
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int valid;
    [MarshalAs(UnmanagedType.R8)] public readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] public readonly double timeStamp;
    [MarshalAs(UnmanagedType.R4)] public readonly float duration;
    [MarshalAs(UnmanagedType.R4)] public readonly float bandwidth;
    [MarshalAs(UnmanagedType.R4)] public readonly float snr_est_db;
    [MarshalAs(UnmanagedType.R4)] public readonly float symrate;
    [MarshalAs(UnmanagedType.R4)] public readonly float deviation;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 40)]
    public readonly byte[] modTypeName;

    [MarshalAs(UnmanagedType.I4)] public readonly int modType;
    [MarshalAs(UnmanagedType.I4)] public readonly int confidence;
    [MarshalAs(UnmanagedType.I4)] public readonly int numMeas;
    [MarshalAs(UnmanagedType.I4)] public readonly int totalMeas;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
    public readonly byte[] pad;

    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SGeneralSearchSetupQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double f1;
    [MarshalAs(UnmanagedType.R8)] public readonly double f2;
    [MarshalAs(UnmanagedType.R8)] public readonly double binSpacing;
    [MarshalAs(UnmanagedType.R8)] public readonly double resBW;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int antenna;
    [MarshalAs(UnmanagedType.I4)] public readonly int attenuation;
    [MarshalAs(UnmanagedType.I4)] public readonly int avgMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int numAvgs;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4,
        SizeConst = E3238SCommandTag.E3238SMaxChannels)]
    public readonly int[] attenOffset;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4,
        SizeConst = E3238SCommandTag.E3238SMaxChannels)]
    public readonly int[] antennaArray;

    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SDirectedSearchBandSetupQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double f1;
    [MarshalAs(UnmanagedType.R8)] public readonly double f2;
    [MarshalAs(UnmanagedType.R8)] public readonly double binSpacing;
    [MarshalAs(UnmanagedType.R8)] public readonly double resBW;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int band;
    [MarshalAs(UnmanagedType.I4)] public readonly int active;
    [MarshalAs(UnmanagedType.I4)] public readonly int sweepInterval;
    [MarshalAs(UnmanagedType.I4)] public readonly int antenna;
    [MarshalAs(UnmanagedType.I4)] public readonly int attenuation;
    [MarshalAs(UnmanagedType.I4)] public readonly int avgMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int numAvgs;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4,
        SizeConst = E3238SCommandTag.E3238SMaxChannels)]
    public readonly int[] attenOffset;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.I4,
        SizeConst = E3238SCommandTag.E3238SMaxChannels)]
    public readonly int[] antennaArray;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SFrequencyLimitQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double f1;
    [MarshalAs(UnmanagedType.R8)] public readonly double f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238STraceScaleQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double xMin;
    [MarshalAs(UnmanagedType.R8)] public readonly double xMax;
    [MarshalAs(UnmanagedType.R8)] public readonly double yMin;
    [MarshalAs(UnmanagedType.R8)] public readonly double yMax;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int trace;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SMarkerValueQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double timeStamp;
    [MarshalAs(UnmanagedType.R8)] public readonly double frequency;
    [MarshalAs(UnmanagedType.R8)] public readonly double amplitude;
    [MarshalAs(UnmanagedType.I4)] public readonly int adcOverload;
    [MarshalAs(UnmanagedType.I4)] public readonly int valid;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int trace;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSweepTimeQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.R4)] public readonly float sweepTime;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSweepCountQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.U4)] public readonly uint sweepCount;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238STunerInfoQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int tunerLocked;
    [MarshalAs(UnmanagedType.R4)] public readonly float adcFs;
    [MarshalAs(UnmanagedType.R4)] public readonly float f1;
    [MarshalAs(UnmanagedType.R4)] public readonly float f2;
    [MarshalAs(UnmanagedType.R4)] public readonly float cf1;
    [MarshalAs(UnmanagedType.R4)] public readonly float cf2;
    [MarshalAs(UnmanagedType.R4)] public readonly float ddcOffset;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SEnergyDetectionQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int energyDetection;
    [MarshalAs(UnmanagedType.I4)] public readonly int peakCriterion;
    [MarshalAs(UnmanagedType.I4)] public readonly int bandwidthCriterion;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdLevel;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMargin;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdOffset;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdTimer;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdSegment;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdSmoothing;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMinLevel;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 255)]
    public readonly byte[] thresholdFilename;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames1;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames2;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames3;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames4;

    [MarshalAs(UnmanagedType.I4)] public readonly int featureEnabled; //[QUERY_ED_MAX_FEATURE_GROUPS];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] preFilterName1; //[QUERY_ED_MAX_FILTERS][QUERY_ED_FILTER_NAME_LENGTH];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] preFilterName2;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] preFilterName3;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] preFilterName4;

    [MarshalAs(UnmanagedType.I4)] public readonly int preFilterEnabled;
    [MarshalAs(UnmanagedType.I4)] public readonly int preFilterLogic;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] postFilterName1; //[QUERY_ED_MAX_FILTERS][QUERY_ED_FILTER_NAME_LENGTH];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] postFilterName2;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] postFilterName3;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] postFilterName4;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFilterNameLength)]
    public readonly byte[] postFilterName5;

    [MarshalAs(UnmanagedType.I4)] public readonly int postFilterEnabled;
    [MarshalAs(UnmanagedType.I4)] public readonly int postFilterLogic;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SEnergyDetectionQueryV0
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int energyDetection;
    [MarshalAs(UnmanagedType.I4)] public readonly int peakCriterion;
    [MarshalAs(UnmanagedType.I4)] public readonly int bandwidthCriterion;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMode;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdLevel;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMargin;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdOffset;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdTimer;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdSegment;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdSmoothing;
    [MarshalAs(UnmanagedType.I4)] public readonly int thresholdMinLevel;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
    public readonly byte[] thresholdFilename; //[256];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames1; //[QUERY_ED_MAX_FEATURE_GROUPS][QUERY_ED_FEATURE_GROUP_NAME_LENGTH];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames2;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames3;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1,
        SizeConst = E3238SCommandTag.QueryEdFeatureGroupNameLength)]
    public readonly byte[] featureGroupNames4;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSpewPortQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int port;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct E3238SSweepStatusQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int status;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SAntennaQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int antenna;
    [MarshalAs(UnmanagedType.R4)] public readonly float gain;
    [MarshalAs(UnmanagedType.I4)] public readonly int pad;
    [MarshalAs(UnmanagedType.R8)] public readonly double lowF;
    [MarshalAs(UnmanagedType.R8)] public readonly double highF;
    [MarshalAs(UnmanagedType.I4)] public readonly int active;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
    public readonly byte[] name; //[32];

    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SSweepRangeQuery
{
    [MarshalAs(UnmanagedType.R8)] public readonly double f1;
    [MarshalAs(UnmanagedType.R8)] public readonly double f2;
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int alignmentPad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SHwInfoQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;
    [MarshalAs(UnmanagedType.I4)] public readonly int tunerModel;
    [MarshalAs(UnmanagedType.I4)] public readonly int adcModel;
    [MarshalAs(UnmanagedType.I4)] public readonly int numChannels;
    [MarshalAs(UnmanagedType.I4)] public readonly int timeReference;
    [MarshalAs(UnmanagedType.I4)] public readonly int sigProcEnabled;
    [MarshalAs(UnmanagedType.R8)] public readonly double fMin;
    [MarshalAs(UnmanagedType.R8)] public readonly double fMax;
    [MarshalAs(UnmanagedType.I4)] public readonly int dfEnabled;
    [MarshalAs(UnmanagedType.I4)] public readonly int modRecEnabled;
    [MarshalAs(UnmanagedType.I4)] public readonly int numModRecSystems;
    [MarshalAs(UnmanagedType.I4)] public readonly int numDfSystems;
    [MarshalAs(UnmanagedType.I4)] public readonly int numSignals;
    [MarshalAs(UnmanagedType.I4)] public readonly int pad;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct E3238SHostInfoQuery
{
    [MarshalAs(UnmanagedType.I4)] public readonly int version;

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 40)]
    public readonly byte[] softwareVersion; //[40];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 40)]
    public readonly byte[] dataCode; //[40];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 80)]
    public readonly byte[] osVersion; //[80];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 80)]
    public readonly byte[] type; //[80];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 40)]
    public readonly byte[] speed; //[40];

    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 40)]
    public readonly byte[] memory; //[40];
}

#region 枚举

public enum E3238SCommandError
{
    NoCommandError,
    CommandErrorUnknown,
    CommandErrorUnknownTag,
    CommandErrorTooManyFields,
    CommandErrorInvalidIndex,
    CommandErrorDotFirstField,
    CommandErrorIncompleteLine,
    CommandErrorTokenNotFound,
    CommandErrorValueFieldEmpty,
    CommandErrorNoEnumMatch,
    CommandErrorValueOutOfRange,
    CommandErrorFrequencyUnit,
    CommandErrorAmplitudeUnit,
    CommandErrorIndexValueOutOfRange,
    CommandErrorUndefinedIndex,
    CommandErrorInvalidHexNumber,
    CommandErrorInvalidIntegerNumber,
    CommandErrorInvalidFloatingPointNumber,
    CommandErrorDefinedIndex,
    CommandErrorConfigCommand,
    CommandErrorOptionLevel,
    CommandErrorBadGroup,
    CommandErrorModifierReused,
    CommandErrorBadModifier,
    CommandErrorGlobalNotAllowed,
    CommandErrorGlobalNotSpecified,
    CommandErrorModifierBadOrder,
    CommandErrorUnmatchedQuotes,
    CommandErrorParser,
    CommandErrorCommandNotFromFile,
    CommandErrorTooManyErrors,
    CommandErrorBadQuery,
    CommandErrorBadQueryTiming,
    CommandErrorOnlyQueryAllowed,
    CommandErrorNum
}

public enum RawTimeDataSpewType
{
    RawTimeDataSpew16BitComplex,
    RawTimeDataSpew32BitComplex,
    RawTimeDataSpew16BitReal,
    RawTimeDataSpew32BitReal
}

public enum SearchMode
{
    SearchModeGeneral,
    SearchModeDirected
}

public enum E3238SUserTraceVectorDataMode
{
    E3238SUserTraceVectorDataMode8Bit,
    E3238SUserTraceVectorDataMode16Bit
}

public enum SpewE3238SMrModType
{
    SpewE3238SMrModTypeUserDefined = -1,
    SpewE3238SMrModTypeUnknown,
    SpewE3238SMrModTypeUnknownDigital,
    SpewE3238SMrModTypeNoise,
    SpewE3238SMrModTypeMsk,
    SpewE3238SMrModType2LevelFsk,
    SpewE3238SMrModType3LevelFsk,
    SpewE3238SMrModType4LevelFsk,
    SpewE3238SMrModType8LevelFsk,
    SpewE3238SMrModTypeBpsk,
    SpewE3238SMrModType8Psk,
    SpewE3238SMrModType16Psk,
    SpewE3238SMrModTypeQpsk,
    SpewE3238SMrModTypePi4Dqpsk,
    SpewE3238SMrModType16Qam,
    SpewE3238SMrModType32Qam,
    SpewE3238SMrModType64Qam,
    SpewE3238SMrModType256Qam,
    SpewE3238SMrModTypeAm,
    SpewE3238SMrModTypeAmDsbsc,
    SpewE3238SMrModTypeSsbLsb,
    SpewE3238SMrModTypeSsbUsb,
    SpewE3238SMrModTypeAnalogFm,
    SpewE3238SMrModTypeManualMorse,
    SpewE3238SMrModTypeMachineMorse,
    SpewE3238SMrModTypeOok,
    SpewE3238SMrModType4Pam,
    SpewE3238SMrModTypePureCarrier,
    SpewE3238SMrModType128Qam,
    SpewE3238SMrModTypeV29
}

public enum AverageMode
{
    AverageModeOff,
    AverageModeRms,
    AverageModePeak
}

public enum QueryEdFilterLogic
{
    QueryEdFilterLogicAnd,
    QueryEdFilterLogicOr
}

public enum SweepStatus
{
    SweepStatusStopped,
    SweepStatusRunning
}

public enum HwInfoAdc
{
    HwInfoAdcUnknown,
    HwInfoAdcE1430,
    HwInfoAdcE1431,
    HwInfoAdcE1437,
    HwInfoAdcE1438,
    HwInfoAdcE1439Bb,
    HwInfoAdcE1439If,
    HwInfoAdcN6830AHf,
    HwInfoAdcN6830AIf
}

public enum HwInfoTuner
{
    HwInfoTunerNone,
    HwInfoTuner89430A,
    HwInfoTuner89431A,
    HwInfoTuner9119,
    HwInfoTuner91198Mhz,
    HwInfoTunerRaptor1Ghz,
    HwInfoTunerRaptor3Ghz,
    HwInfoTunerE2730A,
    HwInfoTunerCs5040,
    HwInfoTunerInterad9640,
    HwInfoTunerInterad9643,
    HwInfoTunerE2731A,
    HwInfoTunerCs5320A,
    HwInfoTunerSi9250, /* HW_INFO_TUNER_SI9250 is SI9250 and E273X */
    HwInfoTunerOff,
    HwInfoTunerSi9250Adv3000,
    HwInfoTunerAdv3000,
    HwInfoTunerPsa,
    HwInfoTunerSi9136,
    HwInfoTunerSi9250Si9136,
    HwInfoTunerN6830AHf
}

public enum HwInfoClock
{
    HwInfoClockSystem,
    HwInfoClockIrig
}

public enum DemodulationType
{
    Off,
    Am,
    Fm,
    Cw,
    Lsb,
    Usb,
    Log,
    Pulse,
    Isb
}

#endregion