using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X77] – REQUEST FOR DELETION OF MANUAL CHANNELS
internal class ManualChannelsDeletionRequest
{
    //[0x6007] List of the identifiers of the channels to be deleted(N*ULONG)
    public UserField<uint> ChannelsTobeDeleted;

    public MessageHeader Header;

    //[0x6006] Number of channels to be deleted. Deletion of all manual channels if equal to 0.
    public UInt32Field NumOfChannels;

    public ManualChannelsDeletionRequest()
    {
        Header = new MessageHeader(MessageId.MreDemSuppGonioManuel, 0);
        NumOfChannels = new UInt32Field(0x6006);
        ChannelsTobeDeleted = new UserField<uint>(0x6007, 0);
    }

    public byte[] GetBytes()
    {
        NumOfChannels.Value = (uint)(ChannelsTobeDeleted.Value == null ? 0 : ChannelsTobeDeleted.Value.Length);
        Header.ContentSize = GetSize() - Header.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfChannels.GetBytes());
        bytes.AddRange(ChannelsTobeDeleted.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + NumOfChannels.GetSize() + ChannelsTobeDeleted.GetSize();
    }
}