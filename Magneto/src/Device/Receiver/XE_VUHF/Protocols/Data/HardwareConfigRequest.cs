using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X09] – HARDWARE CONFIGURATION REQUEST
//This message contains no parameters.
internal class HardwareConfigRequest
{
    public MessageHeader Header;

    public HardwareConfigRequest()
    {
        Header = new MessageHeader(MessageId.MreDemConfigmat, 0);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = 0;
        return Header.GetBytes();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize();
    }
}