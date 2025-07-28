using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0x40] direction-finding results
internal class DirectionFindingResults
{
    //List of direction-finding measurement results
    public readonly DfInfo[] Azimuths;

    public MessageHeader Header;

    //[0x8A0F] Message number of the fragmented maintenance result
    public UInt32Field MessageNo;

    //[0x8A03] Number of valid direction-finding results
    public UInt32Field NumOfValidDf;

    //[0x8A02] Number of the current active phase
    public UInt32Field PhaseNo;

    //[0x8A0E] Sequence number
    public UInt32Field SequenceNo;

    public DirectionFindingResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        NumOfValidDf = new UInt32Field(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        var tempAzimuths = new List<DfInfo>();
        for (var i = 0; i < NumOfValidDf.Value; ++i)
        {
            var temp = new DfInfo(value, ref startIndex);
            tempAzimuths.Add(temp);
        }

        Azimuths = tempAzimuths.ToArray();
    }
}

internal class DfInfo
{
    //[0x8407] Azimuth in tenths of degrees
    public UShortField Azimuth;

    //[0x8405] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x8403] Date of the measurement in ns (see § 2.3.2.2)
    public UInt64Field Date;

    //[0x8A04] Direction-finding measurement result
    public GroupField DfResult;

    //[0x8404] Signal duration in ns
    public UInt64Field Duration;

    //[0x8400] Identifier of the extraction result or of the manual channel
    public UInt32Field Identifier;

    //[0x8408] Latitude of the sensor in ten-millionths of a degree
    public Int32Field Latitude;

    //[0x840A] Average level in dBm
    public ShortField Level;

    //[0x840A] Average level in dBm (OBSOLETE, use Level field bases on short type)
    public CharField LevelObsolete;

    //[0x8409] Longitude of the sensor in ten-millionths of a degree
    public Int32Field Longitude;

    //[0x8402] Quality mark between 0 and 9
    public UShortField QualityMask;

    //[0x8401] Sigma Azimuth in tenths of degrees
    public UInt32Field SigmaAzimuth;

    //[0x8406] Bandwidth in Hz
    public UInt32Field Width;

    public DfInfo(byte[] value, ref int startIndex)
    {
        DfResult = new GroupField(value, ref startIndex);
        Identifier = new UInt32Field(value, ref startIndex);
        SigmaAzimuth = new UInt32Field(value, ref startIndex);
        QualityMask = new UShortField(value, ref startIndex);
        Date = new UInt64Field(value, ref startIndex);
        Duration = new UInt64Field(value, ref startIndex);
        CentreFrequency = new UInt32Field(value, ref startIndex);
        Width = new UInt32Field(value, ref startIndex);
        Azimuth = new UShortField(value, ref startIndex);
        Latitude = new Int32Field(value, ref startIndex);
        Longitude = new Int32Field(value, ref startIndex);
        LevelObsolete = new CharField(value, ref startIndex);
        Level = new ShortField(value, ref startIndex);
    }
}