using System;
using System.Collections.Generic;
using System.Linq;

namespace Magneto.Device.XE_VUHF_28.Protocols.Field;

internal struct MessageHeader : IField
{
    public readonly int MessageId;
    public int ContentSize;
    public int Size => GetSize();

    public MessageHeader(int id, int size)
    {
        MessageId = id;
        ContentSize = size;
    }

    public MessageHeader(byte[] value, ref int startIndex)
    {
        MessageId = XeBitConverter.ToInt32(value, startIndex);
        ContentSize = XeBitConverter.ToInt32(value, startIndex + 4);
        startIndex += 8;
    }

    public byte[] GetBytes()
    {
        var bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(MessageId).Reverse());
        bytes.AddRange(BitConverter.GetBytes(ContentSize).Reverse());
        return bytes.ToArray();
    }

    public static int GetSize()
    {
        return 8;
    }
}