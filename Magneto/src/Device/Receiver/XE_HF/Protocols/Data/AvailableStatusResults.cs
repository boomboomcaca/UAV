using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X63] – AVAILABLE STATUS RESULTS
//This message indicates to the caller the functions available on each of the channels. This message is merely a response message to a request for the available statuses.
internal class AvailableStatusResults
{
    //List of statuses available for each channel
    public ChannelStatus[] Channels;

    public MessageHeader Header;

    //[0x890B] Max number of channels operating simultaneously
    public UCharField MaxNumOfSimultaneousChannels;

    //[0x2032] Number of logic channels (all blocks included)
    public UCharField NumOfChannels;

    public AvailableStatusResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        MaxNumOfSimultaneousChannels = new UCharField(value, ref startIndex);
        var tempChannels = new List<ChannelStatus>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelStatus(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}