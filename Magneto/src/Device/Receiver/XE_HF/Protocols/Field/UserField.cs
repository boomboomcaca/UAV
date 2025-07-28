using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Magneto.Device.XE_HF.Protocols.Field;

internal struct UserField<T> where T : struct, IConvertible
{
    public readonly int TypeAndId;
    public int DataSize;
    public readonly T[] Value;

    public UserField(int fieldId, int dataSize)
    {
        TypeAndId = (DataType.TypeUserDefine << 24) + fieldId;
        DataSize = dataSize;
        Value = null;
    }

    public UserField(byte[] data, ref int startIndex)
    {
        TypeAndId = XeBitConverter.ToInt32(data, startIndex);
        DataSize = XeBitConverter.ToInt32(data, startIndex + 4);
        Value = XeBitConverter.ToArrayT<T>(data, startIndex + 8, DataSize);
        startIndex += 8 + DataSize;
    }

    public byte[] GetBytes()
    {
        DataSize = GetSize() - 8;
        var bytes = new List<byte>();
        bytes.AddRange(XeBitConverter.GetBytes(TypeAndId));
        bytes.AddRange(XeBitConverter.GetBytes(DataSize));
        if (Value != null) bytes.AddRange(XeBitConverter.GetArrayTBytes(Value));
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = 8;
        if (Value != null) totalSize += Value.Length * Marshal.SizeOf(default(T));
        return totalSize;
    }
}