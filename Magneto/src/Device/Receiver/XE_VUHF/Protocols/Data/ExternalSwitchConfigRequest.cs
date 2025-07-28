using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X05] – EXTERNAL SWITCH CONFIGURATION REQUEST
//This message contains no parameters.
internal class ExternalSwitchConfigRequest
{
    public MessageHeader Header;

    public ExternalSwitchConfigRequest()
    {
        Header = new MessageHeader(MessageId.MreDemConfigcommut, 0);
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