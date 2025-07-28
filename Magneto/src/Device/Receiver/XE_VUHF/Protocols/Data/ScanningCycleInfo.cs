using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X97] – SCANNING CYCLE
//Message sent spontaneously by the sensor after configuration of the interception.
internal class ScanningCycleInfo
{
    //[0x2003] Duration of a scanning cycle.
    public DoubleField Duration;
    public MessageHeader Header;

    public ScanningCycleInfo(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Duration = new DoubleField(value, ref startIndex);
    }
}