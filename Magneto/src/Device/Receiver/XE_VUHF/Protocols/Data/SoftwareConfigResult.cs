using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X71] – SOFTWARE CONFIGURATION RESULT
internal class SoftwareConfigResult
{
    //List of software configurations for each channel
    public ChannelSoftwareConfig[] Channels;

    public MessageHeader Header;

    //[0x2032] Number of logic channels (all blocks included)
    public UCharField NumOfChannels;

    public SoftwareConfigResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        List<ChannelSoftwareConfig> tempChannels = new();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            ChannelSoftwareConfig tempChannel = new(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}