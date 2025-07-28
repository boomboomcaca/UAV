using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0x50] IQ production results
internal class IqResults
{
    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x8317] Date of the 1st sample in ns (see § 2.3.2.2)
    public UInt64Field Date;

    public MessageHeader Header;

    //[0x8319] List of samples N*(2+2)
    public UserField<short> Iq;

    //[0x8316] Number of samples
    public UInt32Field Number;

    //[0x8318] Number of samples lost in the event of actual time lost (generally 0)
    public UInt32Field NumOfLost;

    public IqResults(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        ChannelId = new UCharField(value, ref startIndex);
        Number = new UInt32Field(value, ref startIndex);
        Date = new UInt64Field(value, ref startIndex);
        NumOfLost = new UInt32Field(value, ref startIndex);
        Iq = new UserField<short>(value, ref startIndex);
    }
}