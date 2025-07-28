using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0x76] Forced sub-band request
//Message for forcing the ranges of the direction-finding antenna sub-ranges.
internal class ForcedSubrangeRequest
{
    public MessageHeader Header;

    //[0x8B0B] Number of sub-ranges modified. The parameters of the calibration file are reset at 0
    public UInt32Field NumOfSubranges;

    //List of modified sub-ranges
    public SubRangeInfo[] Subranges;

    public ForcedSubrangeRequest()
    {
        Header = new MessageHeader(MessageId.MreDemForcageSousGamme, 0);
        NumOfSubranges = new UInt32Field(0x8B0B);
        Subranges = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
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
        var totalSize = MessageHeader.GetSize() + UInt32Field.GetSize();
        if (Subranges != null)
            foreach (var subrange in Subranges)
                totalSize += subrange.GetSize();
        return totalSize;
    }
}

internal class SubRangeInfo
{
    //[0x2010] New maximum frequency of the sub-range in Hz
    public UInt32Field FMax;

    //[0x200F] New minimum frequency of the sub-range in Hz
    public UInt32Field FMin;

    //[0x8B0C] Sub-range information
    public GroupField Subrange;

    //[0x8B0A] Number of the sub-range modified
    public UInt32Field SubrangeNo;

    public SubRangeInfo()
    {
        Subrange = new GroupField(0x8B0C, 0);
        SubrangeNo = new UInt32Field(0x8B0A);
        FMin = new UInt32Field(0x200F);
        FMax = new UInt32Field(0x2010);
    }

    public byte[] GetBytes()
    {
        Subrange.DataSize = GetSize() - GroupField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Subrange.GetBytes());
        bytes.AddRange(SubrangeNo.GetBytes());
        bytes.AddRange(FMin.GetBytes());
        bytes.AddRange(FMax.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return GroupField.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize();
    }
}