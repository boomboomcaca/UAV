using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x81] Change of active phase at the initiative of LINUX
//Message sent spontaneously by the sensor after restarting the interception at its initiative. This message is sent after :
//A broadband IQ recording
//A single-shot direction-finding request
//The result of a test
internal class ActivePhaseChange
{
    //[0x2004] Current date in ns (see § 2.3.2.2)
    public UInt64Field Date;

    public MessageHeader Header;

    //[0x202D] New active phase number used in the results.
    public UInt32Field PhaseNo;

    public ActivePhaseChange(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        Date = new UInt64Field(value, ref startIndex);
    }
}