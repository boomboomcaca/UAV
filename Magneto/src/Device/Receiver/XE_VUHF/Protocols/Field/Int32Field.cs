using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

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
        List<byte> bytes = new();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        bytes.AddRange(XeBitConverter.GetBytes(Value));
        return bytes.ToArray();
    }

    public static int GetSize()
    {
        return 12;
    }
}