using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X20] – FFT RESULTS
internal class FftResults
{
    //List of FFTs
    public readonly FftInfo[] FfTs;

    //[0x8205] Information on blanking
    public UCharField Blanking;

    //[0x204A] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2004] FFT acquisition time in ns
    public UInt64Field Date;

    public MessageHeader Header;

    //[0x8203] Message number of the fragmented FFT
    public UInt32Field MessageNo;

    //[0x8207] Number of FFTs contained in the message
    public UShortField NumOfFft;

    //[0x8206] Information on the offset to be applied to the list of levels of each FFT.
    public UCharField Offset;

    //[0x8201] Phase number of the broadband interception request
    public UInt32Field PhaseNo;

    //[0x450C] Resolution in Hz
    public DoubleField Resolution;

    //[0x8204] Information on saturation
    public UCharField Saturation;

    //[0x8202] Sequence number used to identify a lost message on the UDP link. Re-initialised after changing programming
    public UInt32Field SequenceNo;

    public FftResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        Date = new UInt64Field(value, ref startIndex);
        Resolution = new DoubleField(value, ref startIndex);
        SequenceNo = new UInt32Field(value, ref startIndex);
        MessageNo = new UInt32Field(value, ref startIndex);
        Saturation = new UCharField(value, ref startIndex);
        Blanking = new UCharField(value, ref startIndex);
        Offset = new UCharField(value, ref startIndex);
        ChannelId = new UCharField(value, ref startIndex);
        NumOfFft = new UShortField(value, ref startIndex);
        var tempFfTs = new List<FftInfo>();
        for (var i = 0; i < NumOfFft.Value; ++i)
        {
            var tempFft = new FftInfo(value, ref startIndex);
            tempFfTs.Add(tempFft);
        }

        FfTs = tempFfTs.ToArray();
    }
}