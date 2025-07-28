using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x0E] NDDC status
//Message that indicates the state of the narrow band channels.
internal class NddcStatus
{
    //List of channel
    public ChannelNddc[] Channels;

    public MessageHeader Header;

    //[0x2032] Number of channel
    public UCharField NumOfChannel;

    public NddcStatus(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannel = new UCharField(value, ref startIndex);
        var tempChannels = new List<ChannelNddc>();
        for (var i = 0; i < NumOfChannel.Value; ++i)
        {
            var tempChannel = new ChannelNddc(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}