using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF_28.Protocols.Field;

internal struct UInt32Field : IField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public uint Value;
    public int Size => GetSize();

    public UInt32Field(int fieldId)
    {
        TypeAndId = (DataType.TypeUlong << 24) + fieldId;
        DataSize = 4;
        Value = 0;
    }

    public UInt32Field(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToUInt32(data, startIndex + 8);
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

    public static int GetSize()
    {
        return 12;
    }
}