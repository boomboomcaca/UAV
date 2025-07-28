using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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