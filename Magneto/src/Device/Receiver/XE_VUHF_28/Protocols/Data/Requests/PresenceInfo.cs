using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0XA7] – PRESENCE INFORMATION
//Message sent by Operation to verify the presence of the WBAT.
internal class PresenceInfo
{
    public MessageHeader Header;

    public PresenceInfo()
    {
        Header = new MessageHeader(MessageId.MreInfoPresence, 0);
    }

    public byte[] GetBytes()
    {
        return Header.GetBytes();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize();
    }
}