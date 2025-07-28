using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class ChannelHardwareConfig
{
    //Configuration of the antennas described in the following table
    public readonly AntennaHardwareConfig[] Antennas;

    //[0x8002] [0, 1] Indicates the presence of an LO
    public UCharField AssociatedLo;

    //[0x8018] Channel information
    public GroupField Channel;

    //[0x8001] [0, 2] Type of HF channel :
    //0 : No HF channel
    //1 : HF 300 kHz
    //2 : HF 1.2 MHz
    public UCharField ChannelTypeHf;

    //[0x8000] [0, 2] Type of VU channel :
    //0 : No VU channel
    //1 : VU 40 MHz
    //2 : VU 20 MHz
    public UCharField ChannelTypeVu;

    //[0x8008] Part number of the HF module
    public MultiBytesField HfModuleNo;

    //[0x8004] Hardware version of the HF module
    public MultiBytesField HfModuleVersion;

    //[0x8009] Part number of the LO module
    public MultiBytesField LoModuleNo;

    //[0x8005] Hardware version of the LO module
    public MultiBytesField LoModuleVersion;

    //[0x8006] No LO: 0 : 10 MHz,  1 : 25 kHz
    public CharField LoSteps;

    //[0x800A] Number of antennas associated with the reception channel
    public UCharField NumOfAntennas;

    //[0x8007] Part number of the RF module
    public MultiBytesField RfModuleNo;

    //[0x8003] Hardware version of the RF module
    public MultiBytesField RfModuleVersion;

    public ChannelHardwareConfig(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        ChannelTypeVu = new UCharField(value, ref startIndex);
        ChannelTypeHf = new UCharField(value, ref startIndex);
        AssociatedLo = new UCharField(value, ref startIndex);
        RfModuleVersion = new MultiBytesField(value, ref startIndex);
        HfModuleVersion = new MultiBytesField(value, ref startIndex);
        LoModuleVersion = new MultiBytesField(value, ref startIndex);
        LoSteps = new CharField(value, ref startIndex);
        RfModuleNo = new MultiBytesField(value, ref startIndex);
        LoModuleNo = new MultiBytesField(value, ref startIndex);
        HfModuleNo = new MultiBytesField(value, ref startIndex);
        NumOfAntennas = new UCharField(value, ref startIndex);
        List<AntennaHardwareConfig> tempAntennas = new();
        for (var i = 0; i < NumOfAntennas.Value; ++i)
        {
            AntennaHardwareConfig tempAntenna = new(value, ref startIndex);
            tempAntennas.Add(tempAntenna);
        }

        Antennas = tempAntennas.ToArray();
    }
}