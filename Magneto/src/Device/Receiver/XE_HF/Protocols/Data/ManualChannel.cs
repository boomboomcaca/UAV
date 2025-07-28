using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class ManualChannel
{
    //[0x2002] Bandwidth in Hz
    public UInt32Field Bandwidth;

    //[0x2001] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x8702] Channel information
    public GroupField Channel;

    //[0x8700] Identifier of the channel (used in the results and in the deletion of a channel)
    public UInt32Field Identifier;

    //[0x2019] Channel threshold in dBm
    public ShortField Threshold;

    //[0x450E] Type Emission threshold, 0 : Absolute, 1 : Relative
    public UCharField ThresholdType;

    public ManualChannel()
    {
        Channel = new GroupField(0x8702, 0);
        Identifier = new UInt32Field(0x8700);
        CentreFrequency = new UInt32Field(0x2001);
        Bandwidth = new UInt32Field(0x2002);
        ThresholdType = new UCharField(0x450E);
        Threshold = new ShortField(0x2019);
    }

    public byte[] GetBytes()
    {
        Channel.DataSize = GetSize() - Channel.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Channel.GetBytes());
        bytes.AddRange(Identifier.GetBytes());
        bytes.AddRange(CentreFrequency.GetBytes());
        bytes.AddRange(Bandwidth.GetBytes());
        bytes.AddRange(ThresholdType.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Channel.GetSize() + Identifier.GetSize() + CentreFrequency.GetSize() +
               Bandwidth.GetSize() + ThresholdType.GetSize() + Threshold.GetSize();
    }
}