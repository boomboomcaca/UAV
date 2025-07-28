using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

internal struct CharField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public sbyte Value;

    public CharField(int fieldId)
    {
        TypeAndId = (DataType.TypeChar << 24) + fieldId;
        DataSize = 1;
        Value = 0;
    }

    public CharField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = (sbyte)data[startIndex + 8];
        startIndex += 9;
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = new();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        bytes.Add((byte)Value);
        return bytes.ToArray();
    }

    public static int GetSize()
    {
        return 9;
    }
}