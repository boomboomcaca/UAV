using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class HomingInfo
{
    public GroupField HomingGroup;
    public UCharField HomingIdChannel;
    public UInt32Field HomingIdentifier;

    public HomingInfo()
    {
        HomingGroup = new GroupField(0x8F02, 0);
        HomingIdChannel = new UCharField(0x204B);
        HomingIdentifier = new UInt32Field(0x6005);
    }

    public byte[] GetBytes()
    {
        HomingGroup.DataSize = GetSize() - HomingGroup.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(HomingGroup.GetBytes());
        bytes.AddRange(HomingIdChannel.GetBytes());
        bytes.AddRange(HomingIdentifier.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return HomingGroup.GetSize() + HomingIdChannel.GetSize() + HomingIdentifier.GetSize();
    }
}