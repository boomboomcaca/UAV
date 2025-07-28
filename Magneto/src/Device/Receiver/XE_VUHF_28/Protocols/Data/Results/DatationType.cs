using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0x6A] Datation Type, Operation  WBAT or WBAT -> Operation
//From WBAT : Indicates the type of datation used
//From Operation : Indicates the type of datation requested for next active phase.
internal class DatationType
{
    public MessageHeader Header;

    //[0x204E] Type of datation : 0 : Relative 1 : Absolute (see § 2.3.2.2)
    public UInt32Field Type;

    public DatationType(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Type = new UInt32Field(value, ref startIndex);
    }

    public DatationType()
    {
        Header = new MessageHeader(MessageId.MreDemTypeDatation, 0);
        Type = new UInt32Field(0x204E);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = UInt32Field.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Type.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UInt32Field.GetSize();
    }
}