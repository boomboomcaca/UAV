using System.Collections.Generic;

namespace Magneto.Device.XE_HF.Protocols.Field;

internal struct UCharField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public byte Value;

    public UCharField(int fieldId)
    {
        //注意此处必须加括号，因为"<<" 的优先级 低于 “+” 
        TypeAndId = (DataType.TypeUchar << 24) + fieldId;
        DataSize = 1;
        Value = 0;
    }

    public UCharField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = data[startIndex + 8];
        startIndex += 9;
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        bytes.Add(Value);
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return 9;
    }
}