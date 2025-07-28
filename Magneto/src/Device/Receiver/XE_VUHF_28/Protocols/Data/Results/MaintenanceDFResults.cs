using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0x41] Maintenance direction-finding results (digit algorithm)
internal class MaintenanceDfResults
{
    public MessageHeader Header;

    //List of maintenance direction-finding results
    public MaintenanceDfInfo[] MaintenanceDfInfos;

    //[0x8A0F] Message number of the fragmented maintenance result
    public UInt32Field MessageNo;

    //[0x8A03] Number of valid direction-finding results
    public UInt32Field NumOfValidDf;

    //[0x8A02] Number of the current active phase
    public UInt32Field PhaseNo;

    //[0x8A0E] Sequence number
    public UInt32Field SequenceNo;

    public MaintenanceDfResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        NumOfValidDf = new UInt32Field(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        var tempAzimuths = new List<MaintenanceDfInfo>();
        for (var i = 0; i < NumOfValidDf.Value; ++i)
        {
            var temp = new MaintenanceDfInfo(value, ref startIndex);
            tempAzimuths.Add(temp);
        }

        MaintenanceDfInfos = tempAzimuths.ToArray();
    }
}

internal class MaintenanceDfInfo
{
    //[0x8605] Approximate azimuth in tenths of a degree
    public UShortField ApproximateAzimuth;

    //[0x8603] Table of 5 corrected phases in tenths of a degree(5*SHORT)
    public UserField<short> CorrectedPhases;

    //[0x8A04] Structure based on the result of an azimuth from the direction-finding result message
    public DfInfo DfResult;

    //[0x861D] Number of elevation table
    public UCharField ElevationTableNo;

    //[0x202B] Heading in tenths of degrees
    public ShortField Heading;

    //[0x8602] Table of 5 levels measured in dBm (5*CHAR)
    public UserField<sbyte> Levels;

    //[0x8609] Maintenance result information
    public GroupField MaintenanceGroup;

    //[0x8607] Azimuth modulus
    public UShortField Modulus;

    //[0x8604] Indicates which dipoles are used for the corrected phases (5*UCHAR)
    public UserField<byte> PairsOfCorrectedPhases;

    //[0x8601] Indicates which dipoles are used for the phases measured (5*UCHAR)
    public UserField<byte> PhasePairsMeasured;

    //[0x8600] Table of 5 phases measured in tenths of a degree(5*SHORT)
    public UserField<short> PhasesMeasured;

    //[0x8606] Precise azimuth in tenths of a degree
    public UShortField PreciseAzimuth;

    public MaintenanceDfInfo(byte[] value, ref int startIndex)
    {
        MaintenanceGroup = new GroupField(value, ref startIndex);
        PhasesMeasured = new UserField<short>(value, ref startIndex);
        PhasePairsMeasured = new UserField<byte>(value, ref startIndex);
        Levels = new UserField<sbyte>(value, ref startIndex);
        CorrectedPhases = new UserField<short>(value, ref startIndex);
        PairsOfCorrectedPhases = new UserField<byte>(value, ref startIndex);
        ApproximateAzimuth = new UShortField(value, ref startIndex);
        PreciseAzimuth = new UShortField(value, ref startIndex);
        Modulus = new UShortField(value, ref startIndex);
        Heading = new ShortField(value, ref startIndex);
        ElevationTableNo = new UCharField(value, ref startIndex);
        DfResult = new DfInfo(value, ref startIndex);
    }
}