using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class ChannelNddc
{
    //[0x830B] Bandwidth of the filter at 3dB in Hz
    public UInt32Field Bandwidth;

    //[0x830A] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x8702] Channel information
    public GroupField Channel;

    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x8309] Sampling frequency in Hz
    public UInt32Field Fs;

    //[0x8303] Current gain
    public ShortField Gain;

    //[0x830E] IP address of the IQ server, len : 16
    public MultiBytesField Ip;

    //[0x830C] Number of encoding bits per sample (16 for an MRE)
    public UCharField NumOfBits;

    //[0x830D] Maximum power in dBm at full scale (2^16) floating. Value in the MRE = 5.5 dBm (specific to acquisition board ICS554)
    public FloatField Power;

    public ChannelNddc(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        ChannelId = new UCharField(value, ref startIndex);
        Fs = new UInt32Field(value, ref startIndex);
        Bandwidth = new UInt32Field(value, ref startIndex);
        CentreFrequency = new UInt32Field(value, ref startIndex);
        Gain = new ShortField(value, ref startIndex);
        NumOfBits = new UCharField(value, ref startIndex);
        Power = new FloatField(value, ref startIndex);
        Ip = new MultiBytesField(value, ref startIndex);
    }
}