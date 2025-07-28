using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X07] – TEST REQUEST
//Note : Force test is an optional parameter. If the parameter is not present, a new test result is done.
internal class TestRequest
{
    //[0x3100] 
    //1 to force a new self-test
    //0 to retrieve the last self-test performed
    public UCharField ForceTest;
    public MessageHeader Header;

    public TestRequest()
    {
        Header = new MessageHeader(MessageId.MreDemTest, 0);
        ForceTest = new UCharField(0x3100);
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        Header.ContentSize = UCharField.GetSize();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ForceTest.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize();
    }
}