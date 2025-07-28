using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x41] Maintenance direction-finding results (digit algorithm)
internal class MaintenanceDfResults
{
    public MessageHeader Header;

    //List of maintenance direction-finding results
    public MaintenanceDfInfo[] MaintenanceDfInfos;

    //[0x8A0F] Message number of the fragmented maintenance result
    public UInt32Field MessageNo;

    //[0x8A03] Number of valid direction-finding results
    public UInt32Field NumOfValidDf;

    //[0x8A02] Number of the current active phase
    public UInt32Field PhaseNo;

    //[0x8A0E] Sequence number
    public UInt32Field SequenceNo;

    public MaintenanceDfResults(byte[] value, ref int startIndex, uint version)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        NumOfValidDf = new UInt32Field(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        var tempAzimuths = new List<MaintenanceDfInfo>();
        for (var i = 0; i < NumOfValidDf.Value; ++i)
        {
            var temp = new MaintenanceDfInfo(value, ref startIndex, version);
            tempAzimuths.Add(temp);
        }

        MaintenanceDfInfos = tempAzimuths.ToArray();
    }
}