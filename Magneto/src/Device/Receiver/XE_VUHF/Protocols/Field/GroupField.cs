using System;
using System.Collections.Generic;
using System.Linq;

namespace Magneto.Device.XE_VUHF.Protocols.Field;

internal struct GroupField
{
    public readonly int TypeAndId;
    public int DataSize;

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
        List<byte> bytes = new();
        bytes.AddRange(BitConverter.GetBytes(TypeAndId).Reverse());
        bytes.AddRange(BitConverter.GetBytes(DataSize).Reverse());
        return bytes.ToArray();
    }

    public static int GetSize()
    {
        return 8;
    }
}