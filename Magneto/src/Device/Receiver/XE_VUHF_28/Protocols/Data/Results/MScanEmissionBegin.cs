using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X9F] – NEW MEMORY SCANNING EMISSION BEGINNING
//Begin a new memory scanning emission
internal class MScanEmissionBegin
{
    //[0x2002] Bandwidth in Hz
    public UInt32Field Bandwidth;

    //[0x830A] Center frequency in Hz
    public UInt64Field CentreFrequency;

    //[0x9316] Emission Detection, 0,1
    public UCharField Detection;

    //[0x9302] Emission information
    public GroupField Emission;

    //[0x9317] Type of emission : 0 : discrete frequency, 1 : band of frequency
    public UCharField EmissionType;

    //[0x2010] Maximum frequency of wide band in Hz
    public UInt64Field FMax;

    //[0x200F] Minimum frequency of wide band in Hz
    public UInt64Field FMin;

    public MessageHeader Header;

    //[0x9301] Identifier of the emission
    public UInt32Field Identifier;

    //[0x9309] Request of IQ flux, 0,1
    public UCharField IqFlux;

    //[0x9307] Request of IQ recording, 0,1
    public UCharField IqRecord;

    //[0x9310] ITU measurements, 0,1
    public UCharField Itu;

    //[0x9306] Request of listening, 0,1
    public UCharField Listening;

    //[0x9308] Request of Wave recording, 0,1
    public UCharField WaveRecord;

    //TODO:抓包发现，设备返回的消息可能为以下两个结构(21版本为长度52和74的两个结构，25版本为长度为52和111的两个结构)
    public MScanEmissionBegin(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        if (Header.ContentSize == 52) //Version 21 和 Version25
        {
            Emission = new GroupField(value, ref startIndex);
            Identifier = new UInt32Field(value, ref startIndex);
            FMin = new UInt64Field(value, ref startIndex);
            FMax = new UInt64Field(value, ref startIndex);
        }
        else if (Header.ContentSize == 111) //Version 25
        {
            Emission = new GroupField(value, ref startIndex);
            Identifier = new UInt32Field(value, ref startIndex);
            Detection = new UCharField(value, ref startIndex);
            EmissionType = new UCharField(value, ref startIndex);
            CentreFrequency = new UInt64Field(value, ref startIndex);
            Bandwidth = new UInt32Field(value, ref startIndex);
            Listening = new UCharField(value, ref startIndex);
            IqRecord = new UCharField(value, ref startIndex);
            WaveRecord = new UCharField(value, ref startIndex);
            IqFlux = new UCharField(value, ref startIndex);
            Itu = new UCharField(value, ref startIndex);
        }
        else if (Header.ContentSize == 74) //Version 21
        {
            Emission = new GroupField(value, ref startIndex);
            Identifier = new UInt32Field(value, ref startIndex);
            Detection = new UCharField(value, ref startIndex);
            Listening = new UCharField(value, ref startIndex);
            IqRecord = new UCharField(value, ref startIndex);
            WaveRecord = new UCharField(value, ref startIndex);
            IqFlux = new UCharField(value, ref startIndex);
            Itu = new UCharField(value, ref startIndex);
        }
    }
}