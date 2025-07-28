using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class BlockHardwareConfig
{
    //Channels configuration described in the following table
    public readonly ChannelHardwareConfig[] Channels;

    //[0x8011] Hardware version of the acquisition board driver
    public MultiBytesField AcquisitionCardDriver;

    //[0x8012] FPGA software version of the acquisition board
    public MultiBytesField AcquisitionCardFpga;

    //[0x802A] FPGA software version for the turbo HF mode of the acquisition board (empty if not available)
    public MultiBytesField AcquisitionCardFpgaTurboHf; //21版本中没有此字段

    //[0x8010] Hardware version of the acquisition board
    public MultiBytesField AcquisitionCardHard;

    //[0x801B] Block information
    public GroupField Block;

    //[0x800E] Hardware version of the Carrier board driver
    public MultiBytesField CarrierDriver;

    //[0x800F] FPGA software version of the Carrier board
    public MultiBytesField CarrierFpga;

    //[0x800D] Hardware version of the Carrier board
    public MultiBytesField CarrierHard;

    //[0x800C] Hardware version of the processing module
    public MultiBytesField HwProcessingVersion;

    //[0x8013] Version of the Linux kernel
    public MultiBytesField LinuxKernel;

    //[0x8017] Number of reception channels
    public UShortField NumOfChannels;

    //[0x8014] Version of the Linux on-board software
    public MultiBytesField OnboardSoftware;

    //[0x8016] RAM capacity in Mb
    public UShortField RamCapacity;

    //[0x8015] Hardware version of the switching unit
    public MultiBytesField SwitchingUnitHard;

    public BlockHardwareConfig(byte[] value, ref int startIndex, uint version)
    {
        Block = new GroupField(value, ref startIndex);
        HwProcessingVersion = new MultiBytesField(value, ref startIndex);
        CarrierHard = new MultiBytesField(value, ref startIndex);
        CarrierDriver = new MultiBytesField(value, ref startIndex);
        CarrierFpga = new MultiBytesField(value, ref startIndex);
        AcquisitionCardHard = new MultiBytesField(value, ref startIndex);
        AcquisitionCardDriver = new MultiBytesField(value, ref startIndex);
        AcquisitionCardFpga = new MultiBytesField(value, ref startIndex);
        if (version == 25)
            AcquisitionCardFpgaTurboHf = new MultiBytesField(value, ref startIndex);
        else
            AcquisitionCardFpgaTurboHf = default;
        LinuxKernel = new MultiBytesField(value, ref startIndex);
        OnboardSoftware = new MultiBytesField(value, ref startIndex);
        SwitchingUnitHard = new MultiBytesField(value, ref startIndex);
        RamCapacity = new UShortField(value, ref startIndex);
        NumOfChannels = new UShortField(value, ref startIndex);
        List<ChannelHardwareConfig> tempChannels = new();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            ChannelHardwareConfig tempChannel = new(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}