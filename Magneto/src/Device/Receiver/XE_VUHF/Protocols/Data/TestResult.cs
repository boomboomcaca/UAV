using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X08] – TEST RESULT
internal class TestResult
{
    //List of the results of the blocks described in the following table
    public BlockTest[] Blocks;

    public MessageHeader Header;

    //[0x801A] Number of blocks
    public UShortField NumOfBlocks;

    public TestResult(byte[] value, ref int startIndex, uint version)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfBlocks = new UShortField(value, ref startIndex);
        List<BlockTest> tempBlocks = new();
        for (var i = 0; i < NumOfBlocks.Value; ++i)
        {
            BlockTest tempBlock = new(value, ref startIndex, version);
            tempBlocks.Add(tempBlock);
        }

        Blocks = tempBlocks.ToArray();
    }
}