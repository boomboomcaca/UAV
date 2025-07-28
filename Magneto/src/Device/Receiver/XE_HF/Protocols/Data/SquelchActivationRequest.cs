using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X93] – SQUELCH ACTIVATION REQUEST
internal class SquelchActivationRequest
{
    //[0x2042] 0 : squelch de-activation, 1 : squelch activation
    public CharField Activation;

    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2032] Number of the logic channel
    public UCharField ChannelNo;

    public MessageHeader Header;

    //[0x2019] Squelch threshold in dBm.
    public ShortField Threshold;

    public SquelchActivationRequest()
    {
        Header = new MessageHeader(MessageId.MreDemSilencieux, 0);
        ChannelNo = new UCharField(0x2032);
        ChannelId = new UCharField(0x204B);
        Threshold = new ShortField(0x2019);
        Activation = new CharField(0x2042);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(ChannelId.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        bytes.AddRange(Activation.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + ChannelNo.GetSize() + ChannelId.GetSize() +
               Threshold.GetSize() + Activation.GetSize();
    }
}