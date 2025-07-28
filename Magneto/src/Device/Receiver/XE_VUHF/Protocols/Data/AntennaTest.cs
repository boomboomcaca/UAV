using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class AntennaTest
{
    //[0x3118] Sub-range information
    public GroupField AntennaSubRange;

    //[0x202E] Name of the antenna
    public MultiBytesField Name;

    //[0x3109] [0, 4] Result of the antenna overall test
    public UCharField OverallResult;

    //[0x310A] Result of the test per dipole (5*UCHAR)
    public UserField<byte> ResultPerDipole;

    public AntennaTest(byte[] value, ref int startIndex)
    {
        AntennaSubRange = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        OverallResult = new UCharField(value, ref startIndex);
        ResultPerDipole = new UserField<byte>(value, ref startIndex);
    }
}