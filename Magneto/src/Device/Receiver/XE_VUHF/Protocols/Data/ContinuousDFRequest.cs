using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X78] – REQUEST FOR CONTINUOUS DIRECTION-FINDING
internal class ContinuousDfRequest
{
    //[0x2002] Bandwidth in Hz
    public UInt32Field Bandwidth;

    //[0x2001] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    public MessageHeader Header;

    //[0x6008] Identifier of the request used in the direction-finding result
    public UInt32Field Identifier;

    //[0x6009] Mode : 0 : fast 1 : sensitive
    public UCharField Mode;

    //[0x600A] Action : 
    //0 : end of continuous direction-finding measurement
    //1 : start of continuous direction-finding measurement
    public CharField Start;

    //[0x2019] Detection threshold in dBm
    public ShortField Threshold;

    //[0x202C] UDP reception port for the result, //TODO:手册为0x202B,但抓包发现为0x202C
    public UShortField UdpPort;

    public ContinuousDfRequest()
    {
        Header = new MessageHeader(MessageId.MreDemGonioContinu, 0);
        Identifier = new UInt32Field(0x6008);
        CentreFrequency = new UInt32Field(0x2001);
        Bandwidth = new UInt32Field(0x2002);
        Threshold = new ShortField(0x2019);
        UdpPort = new UShortField(0x202C);
        Mode = new UCharField(0x6009);
        Start = new CharField(0x600A);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Identifier.GetBytes());
        bytes.AddRange(CentreFrequency.GetBytes());
        bytes.AddRange(Bandwidth.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        bytes.AddRange(UdpPort.GetBytes());
        bytes.AddRange(Mode.GetBytes());
        bytes.AddRange(Start.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() +
               ShortField.GetSize() + UShortField.GetSize() + UCharField.GetSize() + CharField.GetSize();
    }
}