using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

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
        List<byte> bytes = new();
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