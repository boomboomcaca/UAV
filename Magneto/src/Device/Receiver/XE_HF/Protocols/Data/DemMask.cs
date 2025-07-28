using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class DemMask
{
    //List of bands to be masked
    public readonly BandDef[] Bands;

    //[0x4515] Mask information
    public GroupField Mask;

    //[0x2035] Number of bands to be masked
    public UShortField NumberOfBands;

    public DemMask()
    {
        Mask = new GroupField(0x4515, 0);
        NumberOfBands = new UShortField(0x2035);
        Bands = null;
    }

    public byte[] GetBytes()
    {
        NumberOfBands.Value = (ushort)(Bands == null ? 0 : Bands.Length);
        Mask.DataSize = GetSize() - Mask.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Mask.GetBytes());
        bytes.AddRange(NumberOfBands.GetBytes());
        if (Bands != null)
            foreach (var band in Bands)
                bytes.AddRange(band.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Mask.GetSize() + NumberOfBands.GetSize();
        if (Bands != null)
            foreach (var band in Bands)
                totalSize += band.GetSize();
        return totalSize;
    }
}