using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

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
        List<ChannelProgramming> tempChannels = new();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            ChannelProgramming tempChannel = new(value, ref startIndex);
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
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        NumOfChannels.Value = (byte)(Channels == null ? 0 : Channels.Length);
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfChannels.GetBytes());
        if (Channels != null)
            foreach (var channel in Channels)
                bytes.AddRange(channel.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() + UCharField.GetSize();
        if (Channels != null)
            foreach (var channel in Channels)
                totalSize += channel.GetSize();
        return totalSize;
    }
}