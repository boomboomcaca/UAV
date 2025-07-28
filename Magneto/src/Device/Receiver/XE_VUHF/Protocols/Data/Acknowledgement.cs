using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//收到的消息
//[0X01] - ACKNOWLEDGEMENT
internal class Acknowledgement
{
    public MessageHeader Header;

    //[0x0003] Number of the message acknowledged
    public UInt32Field MessageNo;

    //[0x1002] 取值参见ACKReturnCode
    public UInt32Field ReturnCode;

    public Acknowledgement(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        ReturnCode = new UInt32Field(value, ref startIndex);
    }
}