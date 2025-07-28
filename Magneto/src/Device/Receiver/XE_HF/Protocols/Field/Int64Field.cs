using System.Collections.Generic;

namespace Magneto.Device.XE_HF.Protocols.Field;

internal struct Int64Field
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public readonly long Value;

    public Int64Field(int fieldId)
    {
        TypeAndId = (DataType.TypeInt64 << 24) + fieldId;
        DataSize = 8;
        Value = 0;
    }

    public Int64Field(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToInt64(data, startIndex + 8);
        startIndex += 16;
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        bytes.AddRange(XeBitConverter.GetBytes(Value));
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return 16;
    }
}