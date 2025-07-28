using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X67] – RECEIVER PROGRAMMING REQUEST
//This message is used for requesting the receiver parameters and for programming the channels.
internal class ReceiverProgrammingRequest
{
    public MessageHeader Header;

    public ReceiverProgrammingRequest()
    {
        Header = new MessageHeader(MessageId.MreDemProgrx, 0);
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