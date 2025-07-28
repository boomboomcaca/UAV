using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X08] – TEST RESULT
internal class TestResult
{
    //List of the results of the blocks described in the following table
    public readonly BlockTest[] Blocks;

    public MessageHeader Header;

    //[0x801A] Number of blocks
    public UShortField NumOfBlocks;

    public TestResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfBlocks = new UShortField(value, ref startIndex);
        var tempBlocks = new List<BlockTest>();
        for (var i = 0; i < NumOfBlocks.Value; ++i)
        {
            var tempBlock = new BlockTest(value, ref startIndex);
            tempBlocks.Add(tempBlock);
        }

        Blocks = tempBlocks.ToArray();
    }
}

internal class BlockTest
{
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

    //Results of the channels
    public ChannelTest[] Channels;

    //[0x310E] [0, 4] Result of the FE307T Hardware test
    public UCharField Fe307TBoard;

    //[0x311C] [0, 4] Result of loading the FPGA SERV
    public UCharField Fpgaserv;

    //[0x311E] [0, 4] Result of loading the FPGA TS HF 0x3121
    public UCharField Fpgatshf;

    //[0x311D] [0, 4] Result of loading the FPGA TS VUHF 0x3120
    public UCharField Fpgatsvu;

    //[0x3110] [0, 4] Result of the Linux software test
    public UCharField LinuxSoftware;

    //[0x3116] Number of channels tested
    public UCharField NumOfChannels;

    //[0x311B] [0, 1] Result of the remote shared directory mount
    public UCharField SambaDirectory;

    //[0x3111] [0, 4] Result of the switching unit test
    public UCharField SwitchingUnit;

    //[0x3114] [0, 4] Result of the switching unit supply test
    public UCharField SwitchingUnitSupply;

    public BlockTest(byte[] value, ref int startIndex)
    {
        Block = new GroupField(value, ref startIndex);
        Fe307TBoard = new UCharField(value, ref startIndex);
        Fpgaserv = new UCharField(value, ref startIndex);
        Fpgatsvu = new UCharField(value, ref startIndex);
        Fpgatshf = new UCharField(value, ref startIndex);
        LinuxSoftware = new UCharField(value, ref startIndex);
        SwitchingUnit = new UCharField(value, ref startIndex);
        SwitchingUnitSupply = new UCharField(value, ref startIndex);
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

internal class ChannelTest
{
    //Result of the direction-finding antennas test
    public AntennaTest[] Antennas;

    //[0x3117] Channel information
    public GroupField Channel;

    //[0x311A] Number of direction-finding antennas tested
    public UCharField NumOfDfAntennas;

    //[0x310B] [0, 4] Result of the RF VUHF head test
    public UCharField RfSigmaModule;

    public ChannelTest(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        RfSigmaModule = new UCharField(value, ref startIndex);
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

internal class AntennaTest
{
    //[0x3118] Sub-range information
    public GroupField AntennaSubRange;

    //[0x202E] Name of the antenna
    public MultiBytesField Name;

    //[0x3109] [0, 4] Result of the antenna overall test
    public UCharField OverallResult;

    //[0x310A] Result of the test per dipole (5*UCHAR)
    public UserField<byte> ResultPerDipole;

    public AntennaTest(byte[] value, ref int startIndex)
    {
        AntennaSubRange = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        OverallResult = new UCharField(value, ref startIndex);
        ResultPerDipole = new UserField<byte>(value, ref startIndex);
    }
}