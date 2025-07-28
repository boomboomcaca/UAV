using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X9D] – CONTROL OF MEMORY SCANNING
//Control command for memory scanning (start, stop, pause)
internal class MemoryScanningControl
{
    //[0x9200] Command
    //0 : Stop memory scanning 
    //1 : Start memory scanning
    //Not used any more (obsolete) :
    //2 : Pause memory scanning
    //3 : Rerun memory scanning
    public CharField Action;
    public MessageHeader Header;

    public MemoryScanningControl()
    {
        Header = new MessageHeader(MessageId.MreVcyCommand, 0);
        Action = new CharField(0x9200);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = Action.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Action.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + Action.GetSize();
    }
}