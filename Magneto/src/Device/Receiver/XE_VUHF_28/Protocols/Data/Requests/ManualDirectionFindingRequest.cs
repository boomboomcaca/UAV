using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X15] – MANUAL DIRECTION-FINDING REQUEST
//Message for positioning a manual track in the current interception.
internal class ManualDirectionFindingRequest
{
    //[0x2001] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x2003] Track duration validity in ms. 0 for infinite duration
    public UInt64Field Duration;

    public MessageHeader Header;

    //[0x6005] Track identifier used in the results (direction-finding, ITU, extraction etc.)
    public UInt32Field Identifier;

    //[0x2019] Threshold in dBm used to trigger the measurements requested during an interception
    public ShortField Threshold;

    //[0x2002] Bandwidth in Hz
    public UInt32Field Width;

    public ManualDirectionFindingRequest()
    {
        Header = new MessageHeader(MessageId.MreDemGonioManual, 0);
        Identifier = new UInt32Field(0x6005);
        CentreFrequency = new UInt32Field(0x2001);
        Width = new UInt32Field(0x2002);
        Duration = new UInt64Field(0x2003);
        Threshold = new ShortField(0x2019);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(Identifier.GetBytes());
        bytes.AddRange(CentreFrequency.GetBytes());
        bytes.AddRange(Width.GetBytes());
        bytes.AddRange(Duration.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize() +
               UInt32Field.GetSize() + UInt64Field.GetSize() + ShortField.GetSize();
    }
}