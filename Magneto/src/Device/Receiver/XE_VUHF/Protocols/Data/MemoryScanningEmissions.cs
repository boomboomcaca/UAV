using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X9E] – SEND TABLE OF MEMORY SCANNING EMISSION
//List of emission for memory scanning
internal class MemoryScanningEmissions
{
    //List of emission
    public MScanEmission[] Emissions;

    public MessageHeader Header;

    //[0x9300] Number of emission
    public UInt32Field NumOfEmission;

    public MemoryScanningEmissions()
    {
        Header = new MessageHeader(MessageId.MreVcyTableEmission, 0);
        NumOfEmission = new UInt32Field(0x9300);
        Emissions = null;
    }

    public byte[] GetBytes(uint version)
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        NumOfEmission.Value = (uint)(Emissions == null ? 0 : Emissions.Length);
        List<byte> bytes = new();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfEmission.GetBytes());
        if (Emissions != null)
            foreach (var emission in Emissions)
                bytes.AddRange(emission.GetBytes(version));
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() + UInt32Field.GetSize();
        if (Emissions != null)
            foreach (var emission in Emissions)
                totalSize += emission.GetSize();
        return totalSize;
    }
}