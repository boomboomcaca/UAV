using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

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
        Mask.DataSize = GetSize() - GroupField.GetSize();
        List<byte> bytes = new();
        bytes.AddRange(Mask.GetBytes());
        bytes.AddRange(NumberOfBands.GetBytes());
        if (Bands != null)
            foreach (var band in Bands)
                bytes.AddRange(band.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = GroupField.GetSize() + UShortField.GetSize();
        if (Bands != null)
            foreach (var band in Bands)
                totalSize += band.GetSize();
        return totalSize;
    }
}