using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class ChannelTest
{
    //Result of the direction-finding antennas test
    public AntennaTest[] Antennas;

    //[0x3117] Channel information
    public GroupField Channel;

    //[0x310D] [0, 4] Result of the LO module test
    public UCharField LoModule;

    //[0x311A] Number of direction-finding antennas tested
    public UCharField NumOfDfAntennas;

    //[0x310C] [0, 4] Result of the RF HF head test
    public UCharField RfhfModule;

    //[0x310B] [0, 4] Result of the RF VUHF head test
    public UCharField RfvuhfModule;

    public ChannelTest(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        RfvuhfModule = new UCharField(value, ref startIndex);
        LoModule = new UCharField(value, ref startIndex);
        RfhfModule = new UCharField(value, ref startIndex);
        NumOfDfAntennas = new UCharField(value, ref startIndex);
        var tempAntennas = new List<AntennaTest>();
        for (var i = 0; i < NumOfDfAntennas.Value; ++i)
        {
            var tempAntenna = new AntennaTest(value, ref startIndex);
            tempAntennas.Add(tempAntenna);
        }

        Antennas = tempAntennas.ToArray();
    }
}