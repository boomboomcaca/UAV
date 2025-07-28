using System.Collections.Generic;

namespace Magneto.Device.XE_HF.Protocols.Field;

internal struct Int32Field
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public int Value;

    public Int32Field(int fieldId)
    {
        TypeAndId = (DataType.TypeLong << 24) + fieldId;
        DataSize = 4;
        Value = 0;
    }

    public Int32Field(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToInt32(data, startIndex + 8);
        startIndex += 12;
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
        return 12;
    }
}