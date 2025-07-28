using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X64] – REQUEST FOR IQ SAMPLES
internal class IqSamplesRequest
{
    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2032] Number of the logic channel for which the samples are being requested. It must correspond with a prior IQ flux request.
    public UCharField ChannelNo;

    public MessageHeader Header;

    //[0x8316] Number of samples requested :
    //0 : flux I&Q (continuous mode)
    //N : one single burst containing N samples (block mode)
    public UInt32Field Number;

    //[0x831A] Action requested : 0 : stop 1 : start
    public CharField Starting;

    public IqSamplesRequest()
    {
        Header = new MessageHeader(MessageId.MreDemEchiq, 0);
        ChannelNo = new UCharField(0x2032);
        Number = new UInt32Field(0x8316);
        ChannelId = new UCharField(0x204B);
        Starting = new CharField(0x831A);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(Number.GetBytes());
        bytes.AddRange(ChannelId.GetBytes());
        bytes.AddRange(Starting.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize() + UInt32Field.GetSize()
               + UCharField.GetSize() + CharField.GetSize();
    }
}