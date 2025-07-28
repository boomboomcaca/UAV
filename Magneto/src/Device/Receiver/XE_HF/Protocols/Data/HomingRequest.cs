using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//21版本Homing测向控制参数（通过LG319软件抓包发现21版本Homing模式下会发送此结构，但开发文档中没有任何地方有该结构的描述，此处不清楚字段用Unclear代替）
internal class HomingRequest
{
    public readonly HomingInfo Info;
    public MessageHeader Header;
    public ShortField HomingThreshold;
    public UCharField Start;
    public UShortField Unclear;

    public HomingRequest()
    {
        Header = new MessageHeader(0x98, 0);
        Start = new UCharField(0x8F00);
        HomingThreshold = new ShortField(0x450F);
        Unclear = new UShortField(0x8F01);
        Info = new HomingInfo();
    }

    public byte[] GetBytes(uint version)
    {
        Header.ContentSize = GetSize() - Header.GetSize();
        var bytes = new List<byte>();
        if (version == 21)
        {
            bytes.AddRange(Header.GetBytes());
            bytes.AddRange(Start.GetBytes());
            bytes.AddRange(HomingThreshold.GetBytes());
            bytes.AddRange(Unclear.GetBytes());
            bytes.AddRange(Info.GetBytes());
        }

        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Header.GetSize() + Start.GetSize() + HomingThreshold.GetSize() + Unclear.GetSize() + Info.GetSize();
    }
}