using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X80] – VERSION OF THE INTERFACE USED BY WBAT
//Message sent spontaneously to the sensor at the start of TCP communication.
internal class InterfaceVersionInfo
{
    public MessageHeader Header;

    //[0x203B] Version of the WBAT/Operation interface
    public UInt32Field Version;

    public InterfaceVersionInfo(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Version = new UInt32Field(value, ref startIndex);
    }
}