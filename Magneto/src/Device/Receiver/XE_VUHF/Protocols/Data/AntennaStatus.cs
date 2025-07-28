using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class AntennaStatus
{
    //[0x8909] Antenna information
    public GroupField Antenna;

    //[0x202E] Name of the antenna, len: 32
    public MultiBytesField Name;

    public AntennaStatus(byte[] value, ref int startIndex)
    {
        Antenna = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
    }
}