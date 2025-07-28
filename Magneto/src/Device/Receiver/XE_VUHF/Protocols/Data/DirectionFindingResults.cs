using System.Collections.Generic;
using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0x40] direction-finding results
internal class DirectionFindingResults
{
    //List of direction-finding measurement results
    public readonly DfInfo[] Azimuths;

    public MessageHeader Header;

    //[0x8A0F] Message number of the fragmented maintenance result
    public UInt32Field MessageNo;

    //[0x8A03] Number of valid direction-finding results
    public UInt32Field NumOfValidDf;

    //[0x8A02] Number of the current active phase
    public UInt32Field PhaseNo;

    //[0x8A0E] Sequence number
    public UInt32Field SequenceNo;

    public DirectionFindingResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        NumOfValidDf = new UInt32Field(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        List<DfInfo> tempAzimuths = new();
        for (var i = 0; i < NumOfValidDf.Value; ++i)
        {
            DfInfo temp = new(value, ref startIndex);
            tempAzimuths.Add(temp);
        }

        Azimuths = tempAzimuths.ToArray();
    }
}