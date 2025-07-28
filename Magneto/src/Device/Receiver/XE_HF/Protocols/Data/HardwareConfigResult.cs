using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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

    public HardwareConfigResult(byte[] value, ref int startIndex, uint version)
    {
        Header = new MessageHeader(value, ref startIndex);
        EquipmentName = new MultiBytesField(value, ref startIndex);
        NumOfBlocks = new UShortField(value, ref startIndex);
        var tempBlocks = new List<BlockHardwareConfig>();
        for (var i = 0; i < NumOfBlocks.Value; ++i)
        {
            var tempBlock = new BlockHardwareConfig(value, ref startIndex, version);
            tempBlocks.Add(tempBlock);
        }

        Blocks = tempBlocks.ToArray();
    }
}