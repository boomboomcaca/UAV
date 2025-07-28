using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0XA4] – DIRECTION FINDING QUALITY MARK THRESHOLD CONFIGURATION
//Configuration of the DF Quality mark threshold. For example, if the Quality mark threshold is set to :
//0, DF results will quality mark equal and up to 0 will be displayed.
//1, DF results will quality mark equal and up to 1 will be displayed.
internal class DfQualityThresholdConfig
{
    public MessageHeader Header;

    //[0x8402] Quality mark threshold (between 0 and 9). [0,9]
    public UShortField QualityMask;

    public DfQualityThresholdConfig()
    {
        Header = new MessageHeader(MessageId.MreDemDfQualityThreshold, 0);
        QualityMask = new UShortField(0x8402);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = QualityMask.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(QualityMask.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + QualityMask.GetSize();
    }
}