using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X68] – RECEIVER PROGRAMMING MODIFICATION/RESULT
//WBAT  Operation or Operation  WBAT
//This message is used for modifying the receiver parameters and enables the subscriber to know the current programming of the channels.
internal class ReceiverProgramingInfo
{
    //List of programs per channel
    public ChannelProgramming[] Channels;

    public MessageHeader Header;

    //[0x2032] Number of channels
    public UCharField NumOfChannels;

    public ReceiverProgramingInfo(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        var tempChannels = new List<ChannelProgramming>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelProgramming(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }

    public ReceiverProgramingInfo()
    {
        Header = new MessageHeader(MessageId.MreProgrx, 0);
        NumOfChannels = new UCharField(0x2032);
        Channels = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        NumOfChannels.Value = (byte)(Channels == null ? 0 : Channels.Length);
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfChannels.GetBytes());
        if (Channels != null)
            foreach (var channel in Channels)
                bytes.AddRange(channel.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Header.GetSize() + NumOfChannels.GetSize();
        if (Channels != null)
            foreach (var channel in Channels)
                totalSize += channel.GetSize();
        return totalSize;
    }
}