using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0XA5] – SYNCHRONIZATION WITH A JAMMER
//WBAT Operation : Indicate if the mode of synchronization with a jammer is available / not available.
internal class JammerSyncState
{
    public MessageHeader Header;

    //[0x9500] -1 : not available, 0 : available
    public CharField State;

    public JammerSyncState(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        State = new CharField(value, ref startIndex);
    }
}