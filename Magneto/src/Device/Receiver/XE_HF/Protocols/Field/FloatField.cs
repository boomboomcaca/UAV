using System.Collections.Generic;

namespace Magneto.Device.XE_HF.Protocols.Field;

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