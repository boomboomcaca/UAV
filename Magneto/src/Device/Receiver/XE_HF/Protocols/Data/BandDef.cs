using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class BandDef
{
    //[0x2036] Interception band information
    public GroupField Band;

    //[0x2010] Maximum interception frequency in Hz
    public UInt32Field FMax;

    //[0x200F] Minimum interception frequency in Hz
    public UInt32Field FMin;

    public BandDef()
    {
        Band = new GroupField(0x2036, 0);
        FMin = new UInt32Field(0x200F);
        FMax = new UInt32Field(0x2010);
    }

    public byte[] GetBytes()
    {
        Band.DataSize = GetSize() - Band.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Band.GetBytes());
        bytes.AddRange(FMin.GetBytes());
        bytes.AddRange(FMax.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Band.GetSize() + FMin.GetSize() + FMax.GetSize();
    }
}