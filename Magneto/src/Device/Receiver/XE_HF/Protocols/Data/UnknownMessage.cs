using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x07]
internal class UnknownMessage
{
    public MessageHeader Header;

    public UnknownMessage()
    {
        Header = new MessageHeader(0xA7, 0);
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