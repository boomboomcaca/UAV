using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X70] – SOFTWARE CONFIGURATION REQUEST
internal class SoftwareConfigRequest
{
    public MessageHeader Header;

    public SoftwareConfigRequest()
    {
        Header = new MessageHeader(MessageId.MreDemConfiglog, 0);
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