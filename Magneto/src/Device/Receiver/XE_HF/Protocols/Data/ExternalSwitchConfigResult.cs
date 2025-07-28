using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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