using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X9E] – SEND TABLE OF MEMORY SCANNING EMISSION
//List of emission for memory scanning
internal class MemoryScanningEmissions
{
    //List of emission
    public readonly MScanEmission[] Emissions;

    public MessageHeader Header;

    //[0x9300] Number of emission
    public UInt32Field NumOfEmission;

    public MemoryScanningEmissions()
    {
        Header = new MessageHeader(MessageId.MreVcyTableEmission, 0);
        NumOfEmission = new UInt32Field(0x9300);
        Emissions = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        NumOfEmission.Value = (uint)(Emissions == null ? 0 : Emissions.Length);
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfEmission.GetBytes());
        if (Emissions != null)
            foreach (var emission in Emissions)
                bytes.AddRange(emission.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = Header.GetSize() + NumOfEmission.GetSize();
        if (Emissions != null)
            foreach (var emission in Emissions)
                totalSize += emission.GetSize();
        return totalSize;
    }
}