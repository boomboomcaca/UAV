using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X03] – STOP PROCESSING
//This message must be sent to stop processing on a channel.
internal class StopProcessing
{
    //[0x4516] Channel logic number on which processing is to be stopped. If the value –1 is exceeded in this field then the processing is stopped on all the channels
    public CharField ChannelId;

    public MessageHeader Header;

    //[0x4517] Processing to be stopped (cf. broadband interception request). 0 for all the processing on the channel.
    public UInt32Field Measurements;

    public StopProcessing()
    {
        Header = new MessageHeader(MessageId.MreArretTrait, 0);
        ChannelId = new CharField(0x4516);
        Measurements = new UInt32Field(0x4517);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelId.GetBytes());
        bytes.AddRange(Measurements.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + ChannelId.GetSize() + Measurements.GetSize();
    }
}