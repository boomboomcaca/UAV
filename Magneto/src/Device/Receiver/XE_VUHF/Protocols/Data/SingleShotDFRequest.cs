using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0x73] Single shot DF request
internal class SingleShotDfRequest
{
    //[0x2003] Duration of the request in ms
    public UInt64Field Duration;

    //[0x2001] Centre frequency in Hz
    public UInt32Field Frequency;

    public MessageHeader Header;

    //[0x6005] Identifier of the request used in the result
    public UInt32Field Identifier;

    //[0x2019] Detection threshold in dBm
    public ShortField Threshold;

    //[0x202C] UDP reception port for the result, //TODO:手册为0x202B,但抓包发现为0x202C
    public UShortField UdpPort;

    //[0x2002] Bandwidth in Hz
    public UInt32Field Width;

    public SingleShotDfRequest()
    {
        Header = new MessageHeader(MessageId.MreDemGonioMonocoup, 0);
        Identifier = new UInt32Field(0x6005);
        Frequency = new UInt32Field(0x2001);
        Width = new UInt32Field(0x2002);
        Duration = new UInt64Field(0x2003);
        Threshold = new ShortField(0x2019);
        UdpPort = new UShortField(0x202C);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Identifier.GetBytes());
        bytes.AddRange(Frequency.GetBytes());
        bytes.AddRange(Width.GetBytes());
        bytes.AddRange(Duration.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        bytes.AddRange(UdpPort.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() +
               UInt32Field.GetSize() + UInt64Field.GetSize() + ShortField.GetSize() + UShortField.GetSize();
    }
}