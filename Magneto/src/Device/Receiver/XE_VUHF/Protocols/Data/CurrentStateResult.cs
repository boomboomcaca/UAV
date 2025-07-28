using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X72] – CURRENT STATUS OF CHANNELS
//This message is sent spontaneously after each programming of channels. It represents the active functions on each channel.
internal class CurrentStateResult
{
    //List of current statuses for each channel
    public ChannelCurrentStatus[] Channels;

    //[0x2004] Current date in ns (see § 2.3.2.2)
    public UInt64Field Date;

    public MessageHeader Header;

    //[0x2032] Number of logic channels (all blocks included)
    public UCharField NumOfChannels;

    public CurrentStateResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        List<ChannelCurrentStatus> tempChannels = new();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            ChannelCurrentStatus tempChannel = new(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
        Date = new UInt64Field(value, ref startIndex);
    }
}