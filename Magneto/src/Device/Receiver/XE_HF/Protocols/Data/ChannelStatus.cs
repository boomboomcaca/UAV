using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class ChannelStatus
{
    public AntennaStatus[] Antennas;

    //[0x8907] Channel number associated with this channel
    public UCharField AssociatedChannelNo;

    //[0x8906] Broad band IQ flux available
    public UCharField BroadbandIq;

    //[0x8702]
    public GroupField Channel;

    //[0x8905] Available direction-finding identifier for the channel
    //(0 : direction-finding not available)
    //OR logic for the following flags :
    //0x01 : VU Direction Finding
    //0x02 : HF Direction Finding / Watson Watt
    //0x04 : HF Direction Finding / Interferometry
    public UCharField DirectionFinding;

    //[0x8908] Channel number which cannot operate during this one's activity
    public UCharField ExclusiveChannelNo;

    //[0x2010] Maximum frequency of the channel in Hz
    public UInt32Field FMax;

    //[0x200F] Minimum frequency of the channel in Hz
    public UInt32Field FMin;

    //[0x8904] Interception/Extraction available for the channel[0,1]
    public UCharField InterceptionOrExtraction;

    //[0x800A] Number of antennas available on the channel
    public UCharField NumOfAntennas;

    //[0x8903] Number of narrow band channels available for the reception channel
    public UCharField NumOfNbChannels;

    public ChannelStatus(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        NumOfNbChannels = new UCharField(value, ref startIndex);
        InterceptionOrExtraction = new UCharField(value, ref startIndex);
        DirectionFinding = new UCharField(value, ref startIndex);
        BroadbandIq = new UCharField(value, ref startIndex);
        AssociatedChannelNo = new UCharField(value, ref startIndex);
        ExclusiveChannelNo = new UCharField(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        FMax = new UInt32Field(value, ref startIndex);
        NumOfAntennas = new UCharField(value, ref startIndex);
        var tempAntennas = new List<AntennaStatus>();
        for (var i = 0; i < NumOfAntennas.Value; ++i)
        {
            var tempAntenna = new AntennaStatus(value, ref startIndex);
            tempAntennas.Add(tempAntenna);
        }

        Antennas = tempAntennas.ToArray();
    }
}