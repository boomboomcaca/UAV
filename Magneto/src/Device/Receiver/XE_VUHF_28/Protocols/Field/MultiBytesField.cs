using System;
using System.Collections.Generic;
using System.Text;

namespace Magneto.Device.XE_VUHF_28.Protocols.Field;

internal struct MultiBytesField : IField
{
    public readonly int TypeAndId;
    public readonly int DataSize;
    public readonly byte[] Value; //TODO:此字段为定长需通过拷贝赋值
    public int Size => GetSize();

    public MultiBytesField(int fieldId, int dataSize)
    {
        TypeAndId = (DataType.TypeStringMultiBytes << 24) + fieldId;
        DataSize = dataSize;
        Value = new byte[DataSize];
    }

    public MultiBytesField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = new byte[DataSize];
        Array.Copy(data, startIndex + 8, Value, 0, DataSize);
        startIndex += 8 + DataSize;
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        bytes.AddRange(Value);
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return 8 + DataSize;
    }

    public override string ToString()
    {
        return Encoding.ASCII.GetString(Value).TrimEnd('\0');
    }
}