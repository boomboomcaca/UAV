using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0XA0] – NEW MEMORY SCANNING EMISSION END
//End a memory scanning emission
internal class MScanEmissionEnd
{
    //[0x9302] Emission information
    public GroupField Emission;

    public MessageHeader Header;

    //[0x9301] Identifier of the emission
    public UInt32Field Identifier;

    //[0x9309] Request of IQ flux
    public UCharField IqFlux;

    //[0x9307] Request of IQ recording, 0,1
    public UCharField IqRecord;

    //[0x9314] IQ recording filename
    public MultiBytesField IqRecordFileName;

    //[0x9310] ITU measurements
    public UCharField Itu;

    //[0x9306] Request of listening, 0,1
    public UCharField Listening;

    //[0x9308] Request of Wave recording, 0,1
    public UCharField WaveRecord;

    //[0x9315] Wave recording filename
    public MultiBytesField WaveRecordFileName;

    public MScanEmissionEnd(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Emission = new GroupField(value, ref startIndex);
        Identifier = new UInt32Field(value, ref startIndex);
        Listening = new UCharField(value, ref startIndex);
        IqRecord = new UCharField(value, ref startIndex);
        IqRecordFileName = new MultiBytesField(value, ref startIndex);
        WaveRecord = new UCharField(value, ref startIndex);
        WaveRecordFileName = new MultiBytesField(value, ref startIndex);
        IqFlux = new UCharField(value, ref startIndex);
        Itu = new UCharField(value, ref startIndex);
    }
}