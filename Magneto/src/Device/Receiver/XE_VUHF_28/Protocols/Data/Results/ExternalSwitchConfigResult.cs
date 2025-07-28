using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X06] – RESULT OF THE EXTERNAL SWITCH CONFIGURATION
//For TRC6200 unit only.
internal class ExternalSwitchConfigResult
{
    //[0x9006] LO test of Direction Finding
    public LoTest DfloTest;

    //[0x9004] Direction Finding switch amplification available
    public UCharField DfSwitchAmpli;

    public MessageHeader Header;

    //[0x9007] LO test of Listening
    public LoTest ListeningLoTest;

    //[0x9005] Listening switch amplification available
    public UCharField ListeningSwitchAmpli;

    public ExternalSwitchConfigResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        DfSwitchAmpli = new UCharField(value, ref startIndex);
        ListeningSwitchAmpli = new UCharField(value, ref startIndex);
        DfloTest = new LoTest(value, ref startIndex);
        ListeningLoTest = new LoTest(value, ref startIndex);
    }
}

internal class LoTest
{
    //[0x9008] Frequency used in this LO test in Hz (N*DOUBLE)
    public UserField<double> FreqList1; //Hz

    //[0x9008] Frequency used in this LO test in Hz (N*DOUBLE)
    public UserField<double> FreqList2; //Hz

    //[0x9006] DF LO Test or [0x9007] Listening LO Test
    public GroupField GroupLoTest;

    //[0x202E] Name of the antenna used in this LO test
    public MultiBytesField Name;

    public LoTest(byte[] value, ref int startIndex)
    {
        GroupLoTest = new GroupField(value, ref startIndex);
        Name = new MultiBytesField(value, ref startIndex);
        FreqList1 = new UserField<double>(value, ref startIndex);
        FreqList2 = new UserField<double>(value, ref startIndex);
    }
}