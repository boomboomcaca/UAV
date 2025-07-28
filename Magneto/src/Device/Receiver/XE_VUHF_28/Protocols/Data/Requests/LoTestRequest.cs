using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X16] – LO TEST REQUEST
//Message to start/stop LO test for TRC6200 unit only.
internal class LoTestRequest
{
    public MessageHeader Header;

    //Action requested :
    //0 : stop 1 : start
    public UCharField Starting;

    public LoTestRequest()
    {
        Header = new MessageHeader(MessageId.MreDemLoTest, 0);
        Starting = new UCharField(0x9000);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = UCharField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Starting.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize();
    }
}