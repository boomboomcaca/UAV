using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X86] – REQUEST FOR ADDITION OF A LIST OF MANUAL CHANNELS
internal class ManualChannelsAddtionRequest
{
    public MessageHeader Header;

    //List of manual channels
    public ManualChannel[] ManualChannels;

    //[0x8701] Number of manual channels
    public UInt32Field NumOfManualChannels;

    public ManualChannelsAddtionRequest()
    {
        Header = new MessageHeader(MessageId.MreDemListeCanauxManuels, 0);
        NumOfManualChannels = new UInt32Field(0x8701);
        ManualChannels = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        NumOfManualChannels.Value = (uint)(ManualChannels == null ? 0 : ManualChannels.Length);
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfManualChannels.GetBytes());
        if (ManualChannels != null)
            foreach (var channel in ManualChannels)
                bytes.AddRange(channel.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Header.GetSize() + NumOfManualChannels.GetSize();
        if (ManualChannels != null)
            foreach (var channel in ManualChannels)
                totalSize += channel.GetSize();
        return totalSize;
    }
}