using System.Collections.Generic;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

internal struct UShortField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public ushort Value;

    public UShortField(int fieldId)
    {
        TypeAndId = (DataType.TypeUshort << 24) + fieldId;
        DataSize = 2;
        Value = 0;
    }

    public UShortField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToUInt16(data, startIndex + 8);
        startIndex += 10;
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
        return 10;
    }
}