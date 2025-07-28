using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x30] Extraction results
//[0x33] GSM Extraction results
internal class ExtractionResults
{
    //[0x2003] Duration of current extraction cycle in ns (see §2.3.2.2)
    public UInt64Field DurationExtraction;

    //List of extraction results
    public ExtractionInfo[] Extractions;

    public MessageHeader Header;

    //[0x841C] Message number of the fragmented extraction result
    public UInt32Field MessageNo;

    //[0x8416] Number of extraction results
    public UInt32Field NumOfExtractions;

    //[0x8415] Current phase number of the interception
    public UInt32Field PhaseNo;

    //[0x841B] Sequence number
    public UInt32Field SequenceNo;

    //[0x2005] Start time of current extraction cycle in ns (see §2.3.2.2)
    public UInt64Field StartExtraction;

    public ExtractionResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        NumOfExtractions = new UInt32Field(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        StartExtraction = new UInt64Field(value, ref startIndex);
        DurationExtraction = new UInt64Field(value, ref startIndex);
        var tempExtractions = new List<ExtractionInfo>();
        for (var i = 0; i < NumOfExtractions.Value; ++i)
        {
            var temp = new ExtractionInfo(value, ref startIndex);
            tempExtractions.Add(temp);
        }

        Extractions = tempExtractions.ToArray();
    }
}