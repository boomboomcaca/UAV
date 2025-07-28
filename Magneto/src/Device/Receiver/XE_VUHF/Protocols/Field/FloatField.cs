using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

internal struct FloatField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public readonly float Value;

    public FloatField(int fieldId)
    {
        TypeAndId = (DataType.TypeFloat << 24) + fieldId;
        DataSize = 4;
        Value = 0;
    }

    public FloatField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToFloat(data, startIndex + 8);
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