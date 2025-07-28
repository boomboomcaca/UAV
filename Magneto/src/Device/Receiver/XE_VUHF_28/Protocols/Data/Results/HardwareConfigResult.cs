using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X0A] – RESULT OF THE HARDWARE CONFIGURATION
internal class HardwareConfigResult
{
    //Configuration of the blocks described in the following table
    public readonly BlockHardwareConfig[] Blocks;

    //[0x8019] Name of the equipment
    public MultiBytesField EquipmentName;

    public MessageHeader Header;

    //[0x801A] Number of blocks
    public UShortField NumOfBlocks;

    public HardwareConfigResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        EquipmentName = new MultiBytesField(value, ref startIndex);
        NumOfBlocks = new UShortField(value, ref startIndex);
        var tempBlocks = new List<BlockHardwareConfig>();
        for (var i = 0; i < NumOfBlocks.Value; ++i)
        {
            var tempBlock = new BlockHardwareConfig(value, ref startIndex);
            tempBlocks.Add(tempBlock);
        }

        Blocks = tempBlocks.ToArray();
    }
}

internal class BlockHardwareConfig
{
    //Channels configuration described in the following table
    public readonly ChannelHardwareConfig[] Channels;

    //[0x801B] Block information
    public GroupField Block;

    //[0x800E] Hardware version of the Carrier board driver
    public MultiBytesField Fe307TDriver;

    //[0x800F] FPGA software version of the Carrier board
    public MultiBytesField Fe307Tfpgaserv;

    //[0x8011] Hardware version of the acquisition board driver
    public MultiBytesField Fe307Tfpgatshf;

    //[0x8010] Hardware version of the acquisition board
    public MultiBytesField Fe307Tfpgatsvuhf;

    //[0x800D] Hardware version of the Carrier board
    public MultiBytesField Fe307THardware;

    //[0x800C] Hardware version of the processing module
    public MultiBytesField HwProcessingVersion;

    //[0x8013] Version of the Linux kernel
    public MultiBytesField LinuxKernel;

    //[0x8017] Number of reception channels
    public UShortField NumOfChannels;

    //[0x8014] Version of the Linux on-board software
    public MultiBytesField OnboardSoftware;

    //[0x8016] RAM capacity in Mb
    public UInt32Field RamCapacity;

    //[0x8015] Hardware version of the switching unit
    public MultiBytesField SwitchingUnitHard;

    public BlockHardwareConfig(byte[] value, ref int startIndex)
    {
        Block = new GroupField(value, ref startIndex);
        HwProcessingVersion = new MultiBytesField(value, ref startIndex);
        Fe307THardware = new MultiBytesField(value, ref startIndex);
        Fe307TDriver = new MultiBytesField(value, ref startIndex);
        Fe307Tfpgaserv = new MultiBytesField(value, ref startIndex);
        Fe307Tfpgatsvuhf = new MultiBytesField(value, ref startIndex);
        Fe307Tfpgatshf = new MultiBytesField(value, ref startIndex);
        LinuxKernel = new MultiBytesField(value, ref startIndex);
        OnboardSoftware = new MultiBytesField(value, ref startIndex);
        SwitchingUnitHard = new MultiBytesField(value, ref startIndex);
        RamCapacity = new UInt32Field(value, ref startIndex);
        NumOfChannels = new UShortField(value, ref startIndex);
        var tempChannels = new List<ChannelHardwareConfig>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelHardwareConfig(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}

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
    //3 : HF 30 MHz
    public UCharField ChannelTypeHf;

    //[0x8000] [0, 2] Type of VU channel :
    //0 : No VU channel
    //1 : VU 40 MHz
    //2 : VU 20 MHz
    public UCharField ChannelTypeVu;

    //[0x8006] No LO: 0 : 10 MHz,  1 : 25 kHz,  2 : 1MHz
    public CharField LoSteps;

    //[0x800A] Number of antennas associated with the reception channel
    public UCharField NumOfAntennas;

    //[0x8007] Part number of the RF Sigma module
    public MultiBytesField RfSigmaModuleNo;

    //[0x8003] Hardware version of the RF Sigma module
    public MultiBytesField RfSigmaModuleVersion;

    //[0x802b] TODO:抓包多出来的数据
    public MultiBytesField Unknown;

    public ChannelHardwareConfig(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        ChannelTypeVu = new UCharField(value, ref startIndex);
        ChannelTypeHf = new UCharField(value, ref startIndex);
        AssociatedLo = new UCharField(value, ref startIndex);
        RfSigmaModuleVersion = new MultiBytesField(value, ref startIndex);
        LoSteps = new CharField(value, ref startIndex);
        RfSigmaModuleNo = new MultiBytesField(value, ref startIndex);
        Unknown = new MultiBytesField(value, ref startIndex);
        NumOfAntennas = new UCharField(value, ref startIndex);
        var tempAntennas = new List<AntennaHardwareConfig>();
        for (var i = 0; i < NumOfAntennas.Value; ++i)
        {
            var tempAntenna = new AntennaHardwareConfig(value, ref startIndex);
            tempAntennas.Add(tempAntenna);
        }

        Antennas = tempAntennas.ToArray();
    }
}

internal class AntennaHardwareConfig
{
    //Configuration of the sub-ranges described in the following table
    public readonly AntennaSubRangeConfig[] SubRanges;

    //[0x800B] Antenna information
    public GroupField Antenna;

    //[0x2016] Name of the antenna
    public MultiBytesField AntennaName;

    //[0x2026] Antenna directivity :
    //1 : Omni directional
    //2 : Directional
    public UCharField Directivity;

    //[0x2030] Identification used for the CDS networks
    public MultiBytesField Identification;

    //[0x2031] Number of antenna sub-ranges
    public UCharField NumOfSubRanges;

    //[0x2027] Antenna polarisation :
    //1 : Horizontal
    //2 : Vertical
    public UCharField Polarisation;

    //[0x2029] Type of antenna :
    //0 : Listening (0 dipole)
    //1 : Direction-finding (5 dipoles)
    //2 : Direction-finding (3 dipoles)
    //3 : Listening Virtual (0 dipole)
    public UCharField Type;

    public AntennaHardwareConfig(byte[] value, ref int startIndex)
    {
        Antenna = new GroupField(value, ref startIndex);
        AntennaName = new MultiBytesField(value, ref startIndex);
        Identification = new MultiBytesField(value, ref startIndex);
        Type = new UCharField(value, ref startIndex);
        Directivity = new UCharField(value, ref startIndex);
        Polarisation = new UCharField(value, ref startIndex);
        NumOfSubRanges = new UCharField(value, ref startIndex);
        var tempSubRanges = new List<AntennaSubRangeConfig>();
        for (var i = 0; i < NumOfSubRanges.Value; ++i)
        {
            var tempSubRange = new AntennaSubRangeConfig(value, ref startIndex);
            tempSubRanges.Add(tempSubRange);
        }

        SubRanges = tempSubRanges.ToArray();
    }
}

internal class AntennaSubRangeConfig
{
    //[0x2028] Sub-range information
    public GroupField AntennaSubRange;

    //[0x2010] Maximum frequency in Hz. This value is read from the XML file for listening antennas and from the calibration file for direction-finding antennas.
    public UInt32Field FMax;

    //[0x200F] Minimum frequency in Hz. This value is read from the XML file for listening antennas and from the calibration file for direction-finding antennas.
    public UInt32Field FMin;

    //[0x202E] Name of the sub-range
    public MultiBytesField Name;

    //[0x204F] Power supply of antenna sub range (1 if power supply is commutable, 0 if only ON or only OFF)
    public UCharField PowerSupply;

    //[0x202F] Version of the sub-range
    public MultiBytesField Version;

    public AntennaSubRangeConfig(byte[] value, ref int startIndex)
    {
        AntennaSubRange = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        Version = new MultiBytesField(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        FMax = new UInt32Field(value, ref startIndex);
        PowerSupply = new UCharField(value, ref startIndex);
    }
}