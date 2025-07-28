using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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
        Subrange.DataSize = GetSize() - Subrange.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Subrange.GetBytes());
        bytes.AddRange(SubrangeNo.GetBytes());
        bytes.AddRange(FMin.GetBytes());
        bytes.AddRange(FMax.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Subrange.GetSize() + SubrangeNo.GetSize() + FMin.GetSize() + FMax.GetSize();
    }
}