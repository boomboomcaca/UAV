using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

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
        var tempChannels = new List<ChannelCurrentStatus>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelCurrentStatus(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
        Date = new UInt64Field(value, ref startIndex);
    }
}

internal class ChannelCurrentStatus
{
    //[0x8702] Channel information
    public GroupField Channel;

    //[0x890D] [0,1] Bit fields indicating the measurements in progress on the channel => format identical to IntLB
    public UInt32Field CurrentMeasurements;

    //[0x890E] [0,1] Indicates whether a start/stop has just been performed on the channel
    public UCharField Init;

    public ChannelCurrentStatus(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        Init = new UCharField(value, ref startIndex);
        CurrentMeasurements = new UInt32Field(value, ref startIndex);
    }
}