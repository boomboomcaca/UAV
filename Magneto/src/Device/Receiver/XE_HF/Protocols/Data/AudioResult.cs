using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X90] – DEMODULATED AUDIO RESULT
internal class AudioResult
{
    //[0x2045] List audio samples in PCM (N*SHORT)
    public UserField<short> AudioSamples;

    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2032] Logic channel number
    public UCharField ChannelNo;

    public MessageHeader Header;

    //[0x2044] Number of audio samples in the message
    public UInt32Field NumOfSamples;

    //[0x2043] Audio sample number
    public UInt32Field SampleNum;

    public AudioResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        ChannelNo = new UCharField(value, ref startIndex);
        ChannelId = new UCharField(value, ref startIndex);
        SampleNum = new UInt32Field(value, ref startIndex);
        NumOfSamples = new UInt32Field(value, ref startIndex);
        AudioSamples = new UserField<short>(value, ref startIndex);
    }
}