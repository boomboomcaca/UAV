using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class AntennaSubRangeConfig
{
    //[0x2028] Sub-range information
    public GroupField AntennaSubRange;

    //[0x2010] Maximum frequency in Hz. This value is read from the XML file for listening antennas and from the calibration file for direction-finding antennas.
    public UInt32Field FMax;

    //[0x200F] Minimum frequency in Hz. This value is read from the XML file for listening antennas and from the calibration file for direction-finding antennas.
    public UInt32Field FMin;

    //[0x202E] Name of the sub-range
    public MultiBytesField Name;

    //[0x204F] Power supply of antenna sub range (1 if power supply is commutable, 0 if only ON or only OFF)
    public UCharField PowerSupply;

    //[0x202F] Version of the sub-range
    public MultiBytesField Version;

    public AntennaSubRangeConfig(byte[] value, ref int startIndex)
    {
        AntennaSubRange = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        Version = new MultiBytesField(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        FMax = new UInt32Field(value, ref startIndex);
        PowerSupply = new UCharField(value, ref startIndex);
    }
}