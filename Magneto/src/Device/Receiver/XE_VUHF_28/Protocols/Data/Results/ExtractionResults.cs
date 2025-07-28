using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

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

internal class ExtractionInfo
{
    public FhChannelInfo[] Channels;

    //[0x8A04] Direction-finding measurement result described in 0x40
    public DfInfo DfResult;

    //[0x2003] Signal duration in ns (see 2.3.2.2)Maximum value is duration of extraction cycle.
    public UInt64Field Duration;

    //[0x2006]
    //Stop time if the transmission is stopped in ns (see §2.3.2.2).
    //Minimum value is start time of extraction cycle.
    //Maximum value is end time of extraction cycle (if emission is still active).
    public UInt64Field End;

    //[0x8417] Extraction result information
    public GroupField Extraction;

    //[0x840F] FH channeling in Hz
    public UInt32Field FhChanneling;

    //[0x840C] Average duration of an FH plot in ns
    public UInt32Field FhPlot;

    //[0x840D] Average duration of an FH silence in ns
    public UInt32Field FhSilence;

    //[0x840E] Average hopping speed of an FH in hops per s
    public UInt32Field FhSpeed;

    //[0x8411] Type of FH : 0 : channel type FH, 1 : sub-band type FH
    public CharField FhType;

    //[0x2010] Maximum frequency in Hz (FH/FF/BURST/ MANUAL CHANNEL)
    public UInt32Field FMax;

    //[0x200F] Minimum frequency in Hz (FH/FF/BURST/MANUAL CHANNEL)
    public UInt32Field FMin;

    //[0x8400] Identifier of the extraction result
    public UInt32Field Identifier;

    //[0x840A] Average level in dBm
    public ShortField Level;

    //[0x8412] Number of FH sub-bands or channels
    public UShortField NumOfSubbandsOrChannels;

    //[0x2005]
    //Start of detection time in ns (see § 2.3.2.2).
    //Minimum value is start time of extraction cycle.
    //Maximum value is end time of extraction cycle.
    public UInt64Field Start;

    //List of channels or list of sub-bands according to the type of FH
    public FhSubBandInfo[] SubBands;

    //[0x840B] Type of detection :
    //0 : FH
    //1 : Fixed frequency
    //2 : Burst
    //3 : Manual channel
    public UCharField Type;

    public ExtractionInfo(byte[] value, ref int startIndex)
    {
        Extraction = new GroupField(value, ref startIndex);
        Identifier = new UInt32Field(value, ref startIndex);
        Start = new UInt64Field(value, ref startIndex);
        End = new UInt64Field(value, ref startIndex);
        Duration = new UInt64Field(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        FMax = new UInt32Field(value, ref startIndex);
        Type = new UCharField(value, ref startIndex);
        Level = new ShortField(value, ref startIndex);
        if (Type.Value != 1 && Type.Value != 2) // 0: FH, 1: Fixed frequency, 2: Burst, 3: Manual channel
        {
            //TODO:目前暂时发现Type为1,2时不会有以下几个参数，3有，0按解释也应该有
            FhPlot = new UInt32Field(value, ref startIndex);
            FhSilence = new UInt32Field(value, ref startIndex);
            FhSpeed = new UInt32Field(value, ref startIndex);
        }

        FhChanneling = new UInt32Field(value, ref startIndex);
        DfResult = new DfInfo(value, ref startIndex);
        FhType = new CharField(value, ref startIndex);
        NumOfSubbandsOrChannels = new UShortField(value, ref startIndex);
        if (FhType.Value == 0)
        {
            var tempChannels = new List<FhChannelInfo>();
            for (var i = 0; i < NumOfSubbandsOrChannels.Value; ++i)
            {
                var tempChannel = new FhChannelInfo(value, ref startIndex);
                tempChannels.Add(tempChannel);
            }

            Channels = tempChannels.ToArray();
        }
        else if (FhType.Value == 1)
        {
            var tempSubBands = new List<FhSubBandInfo>();
            for (var i = 0; i < NumOfSubbandsOrChannels.Value; ++i)
            {
                var tempSubband = new FhSubBandInfo(value, ref startIndex);
                tempSubBands.Add(tempSubband);
            }

            SubBands = tempSubBands.ToArray();
        }
    }
}

internal class FhChannelInfo
{
    //[0x8413] Centre frequency of the channel in Hz (N*ULONG)(4*N)
    public UserField<uint> ChannelList;

    public FhChannelInfo(byte[] value, ref int startIndex)
    {
        ChannelList = new UserField<uint>(value, ref startIndex);
    }
}

internal class FhSubBandInfo
{
    //[0x8405]Centre frequency of the band in Hz
    public UInt32Field CentreFrequency;

    //[0x840A]Average level in dBm
    public ShortField Level;

    //[0x840A]Average level in dBm (OBSOLETE, use Level field bases on short type)
    public CharField LevelObsolete;

    //[0x8414] Sub-band information
    public GroupField SubBand;

    //[0x8406] Width of the band in Hz
    public UInt32Field Width;

    public FhSubBandInfo(byte[] value, ref int startIndex)
    {
        SubBand = new GroupField(value, ref startIndex);
        CentreFrequency = new UInt32Field(value, ref startIndex);
        Width = new UInt32Field(value, ref startIndex);
        LevelObsolete = new CharField(value, ref startIndex);
        Level = new ShortField(value, ref startIndex);
    }
}