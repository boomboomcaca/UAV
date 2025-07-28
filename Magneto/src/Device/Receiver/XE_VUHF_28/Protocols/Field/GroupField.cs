using System;
using System.Collections.Generic;
using System.Linq;

namespace Magneto.Device.XE_VUHF_28.Protocols.Field;

internal struct GroupField : IField
{
    public readonly int TypeAndId;
    public int DataSize;
    public int Size => GetSize();

    public GroupField(int fieldId, int dataSize)
    {
        TypeAndId = fieldId;
        DataSize = dataSize;
    }

    public GroupField(byte[] value, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(value, startIndex);
        DataSize = XeBitConverter.ToInt32(value, startIndex + 4);
        startIndex += 8;
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(TypeAndId).Reverse());
        bytes.AddRange(BitConverter.GetBytes(DataSize).Reverse());
        return bytes.ToArray();
    }

    public static int GetSize()
    {
        return 8;
    }
}