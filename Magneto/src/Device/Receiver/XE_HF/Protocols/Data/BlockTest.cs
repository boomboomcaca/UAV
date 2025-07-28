using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class BlockTest
{
    //[0x310F] [0, 4] Result of the acquisition board test
    public UCharField AcquisitionBoard;

    /*Each result may take the following values :
    0 : OK
    1 : KO
    2 : Missing
    3 : Insignificant
    4 : Not tested
    5: Not tested, blanking activated (used for the antenna test)*/
    //Note : if the result for the Turbo HF FPGA is “Insignificant”, this means there is no need for this FPGA in the configuration used.
    //[0x3119] Information of a block
    public GroupField Block;

    //[0x310E] [0, 4] Result of the Carrier board test
    public UCharField CarrierBoard;

    //Results of the channels
    public ChannelTest[] Channels;

    //[0x311C] [0, 4] Result of loading the classic FPGA
    public UCharField ClassicFpga;

    //[0x3115] [0, 4] Result of the acquisition board clock test
    public UCharField ClockBoard;

    //[0x3113] [0, 4] Result of the cover supply test
    public UCharField CoverSupply;

    //[0x3110] [0, 4] Result of the Linux software test
    public UCharField LinuxSoftware;

    //[0x3116] Number of channels tested
    public UCharField NumOfChannels;

    //[0x311B] [0, 1] Result of the remote shared directory mount
    public UCharField SambaDirectory;

    //[0x3112] [0, 4] Result of the shell supply test
    public UCharField ShellSupply;

    //[0x3111] [0, 4] Result of the switching unit test
    public UCharField SwitchingUnit;

    //[0x3114] [0, 4] Result of the switching unit supply test
    public UCharField SwitchingUnitSupply;

    //[0x311D] [0, 4] Result of loading the FPGA for turbo HF
    public UCharField TurboHffpga;

    public BlockTest(byte[] value, ref int startIndex, uint version)
    {
        Block = new GroupField(value, ref startIndex);
        CarrierBoard = new UCharField(value, ref startIndex);
        AcquisitionBoard = new UCharField(value, ref startIndex);
        if (version == 25) //TODO:
        {
            ClassicFpga = new UCharField(value, ref startIndex);
            TurboHffpga = new UCharField(value, ref startIndex);
        }
        else
        {
            ClassicFpga = default;
            TurboHffpga = default;
        }

        LinuxSoftware = new UCharField(value, ref startIndex);
        SwitchingUnit = new UCharField(value, ref startIndex);
        ShellSupply = new UCharField(value, ref startIndex);
        CoverSupply = new UCharField(value, ref startIndex);
        SwitchingUnitSupply = new UCharField(value, ref startIndex);
        ClockBoard = new UCharField(value, ref startIndex);
        SambaDirectory = new UCharField(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        var tempChannels = new List<ChannelTest>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelTest(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}