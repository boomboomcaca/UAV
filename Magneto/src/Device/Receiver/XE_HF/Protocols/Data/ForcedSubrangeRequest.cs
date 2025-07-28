using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x76] Forced sub-band request
//Message for forcing the ranges of the direction-finding antenna sub-ranges.
internal class ForcedSubrangeRequest
{
    //List of modified sub-ranges
    public readonly SubRangeInfo[] Subranges;
    public MessageHeader Header;

    //[0x8B0B] Number of sub-ranges modified. The parameters of the calibration file are reset at 0
    public UInt32Field NumOfSubranges;

    public ForcedSubrangeRequest()
    {
        Header = new MessageHeader(MessageId.MreDemForcageSousGamme, 0);
        NumOfSubranges = new UInt32Field(0x8B0B);
        Subranges = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        NumOfSubranges.Value = (uint)(Subranges == null ? 0 : Subranges.Length);
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfSubranges.GetBytes());
        if (Subranges != null)
            foreach (var subrange in Subranges)
                bytes.AddRange(subrange.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Header.GetSize() + NumOfSubranges.GetSize();
        if (Subranges != null)
            foreach (var subrange in Subranges)
                totalSize += subrange.GetSize();
        return totalSize;
    }
}