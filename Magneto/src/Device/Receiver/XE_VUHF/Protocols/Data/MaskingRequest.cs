using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0x69] Masking request
internal class MaskingRequest
{
    //List of bands to be masked
    public readonly BandDef[] BandsTobeMasked;

    public MessageHeader Header;

    //[0x4515] Mask information
    public GroupField Mask;

    //[0x2035] Number of bands to be masked
    public UShortField NumOfBands;

    public MaskingRequest()
    {
        Header = new MessageHeader(MessageId.MreDemMask, 0);
        Mask = new GroupField(0x4515, 0);
        NumOfBands = new UShortField(0x2035);
        BandsTobeMasked = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        Mask.DataSize = GetSize() - MessageHeader.GetSize() - GroupField.GetSize();
        NumOfBands.Value = (ushort)(BandsTobeMasked == null ? 0 : BandsTobeMasked.Length);
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Mask.GetBytes());
        bytes.AddRange(NumOfBands.GetBytes());
        if (BandsTobeMasked != null)
            foreach (var band in BandsTobeMasked)
                bytes.AddRange(band.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() + GroupField.GetSize() + UShortField.GetSize();
        if (BandsTobeMasked != null)
            foreach (var band in BandsTobeMasked)
                totalSize += band.GetSize();
        return totalSize;
    }
}