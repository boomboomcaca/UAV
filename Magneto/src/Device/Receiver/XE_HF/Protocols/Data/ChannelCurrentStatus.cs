using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class ChannelCurrentStatus
{
    //[0x8702] Channel information
    public GroupField Channel;

    //[0x890D] [0,1] Bit fields indicating the measurements in progress on the channel => format identical to IntLB
    public UInt32Field CurrentMeasurements;

    //[0x890E] [0,1] Indicates whether a start/stop has just been performed on the channel
    public UCharField Init;

    public ChannelCurrentStatus(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        Init = new UCharField(value, ref startIndex);
        CurrentMeasurements = new UInt32Field(value, ref startIndex);
    }
}