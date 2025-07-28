using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X84] – AUDIO DEMODULATION REQUEST
internal class AudioDemodulationRequest
{
    //[0x2042] Action :
    //0 : Stop, 1 : With audio flux, 2 : Without audio flux, 3 : End audio flux
    public CharField Action;

    //[0x2017] Translation frequency in Hz
    public Int32Field Bfo;

    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2032] Channel number
    public UCharField ChannelNo;

    //[0x2041] Demodulation output frequency in Hz (8000 by default)
    public UInt32Field Frequency;

    public MessageHeader Header;

    //[0x200B] Type of modulation :
    //0 : A3E, 1 : F3E, 2 : H3E- , 3 : H3E+ , 4 : J3E- , 5 : J3E+ , 6 : A0, 
    //7 : F1B, 8 : A1A, 9 : N0N, 10 : R3E- , 11 : R3E+ , 12 : G3E
    public UInt32Field ModulationType;

    //[0x202C] UDP data reception port//TODO:手册为0x202B,但抓包发现为0x202C
    public UShortField UdpPort;

    public AudioDemodulationRequest()
    {
        Header = new MessageHeader(MessageId.MreDemDemodaudio, 0);
        ChannelNo = new UCharField(0x2032);
        ChannelId = new UCharField(0x204B);
        ModulationType = new UInt32Field(0x200B);
        Bfo = new Int32Field(0x2017);
        Frequency = new UInt32Field(0x2041);
        UdpPort = new UShortField(0x202C);
        Action = new CharField(0x2042);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(ChannelId.GetBytes());
        bytes.AddRange(ModulationType.GetBytes());
        bytes.AddRange(Bfo.GetBytes());
        bytes.AddRange(Frequency.GetBytes());
        bytes.AddRange(UdpPort.GetBytes());
        bytes.AddRange(Action.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize() + UCharField.GetSize() + UInt32Field.GetSize() +
               Int32Field.GetSize() + UInt32Field.GetSize() + UShortField.GetSize() + CharField.GetSize();
    }
}