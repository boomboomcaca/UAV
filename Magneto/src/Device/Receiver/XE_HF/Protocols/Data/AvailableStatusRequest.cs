using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X62] – AVAILABLE STATUS REQUEST
//This message contains no parameters.
internal class AvailableStatusRequest
{
    public MessageHeader Header;

    public AvailableStatusRequest()
    {
        Header = new MessageHeader(MessageId.MreDemEtatDispo, 0);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = 0;
        return Header.GetBytes();
    }

    public int GetSize()
    {
        return Header.GetSize();
    }
}