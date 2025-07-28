using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

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

    public MaintenanceDfInfo(byte[] value, ref int startIndex, uint version)
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
        if (version == 25) ElevationTableNo = new UCharField(value, ref startIndex);
        DfResult = new DfInfo(value, ref startIndex);
    }
}