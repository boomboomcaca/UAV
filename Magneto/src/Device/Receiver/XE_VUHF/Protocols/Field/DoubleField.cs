using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

internal struct DoubleField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public double Value;

    public DoubleField(int field)
    {
        TypeAndId = (DataType.TypeDouble << 24) + field;
        DataSize = 8;
        Value = 0;
    }

    public DoubleField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToDouble(data, startIndex + 8);
        startIndex += 16;
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
        return 16;
    }
}