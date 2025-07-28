using System.Collections.Generic;

namespace Magneto.Device.XE_HF.Protocols.Field;

internal struct ShortField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public short Value;

    public ShortField(int fieldId)
    {
        TypeAndId = (DataType.TypeShort << 24) + fieldId;
        DataSize = 2;
        Value = 0;
    }

    public ShortField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToInt16(data, startIndex + 8);
        startIndex += 10;
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
        return 10;
    }
}