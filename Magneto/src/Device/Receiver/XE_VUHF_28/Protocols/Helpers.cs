using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF_28.Protocols;

/*/////////////////////////////数据协议/////////////////////////////////////////////
* Header of Message
I + L + Content of Message
I : message identifier over 4 bytes.
L : size of the message encoded over 4 bytes. This is the size of the content of message, without the header size.
* Content of Message
I + L + V
I: Type(1byte) + UserIdentifier(3bytes),  Identifier of the parameter over 4 bytes (includes the parameter type).
L: Size(4bytes), size of the parameter encoded over 4 bytes. This is the size of V, without the I+L size.
V: value of the parameter.
* The data of an ILV message can contains another ILV message. As a matter of fact, an ILV message is a group of encapsulated parameters.
//////////////////////////////////////////////////////////////////////////////////*/
/*/////////////////////////////注意事项/////////////////////////////////////////////
 * 注意：
 1. 在测试过程中发现如果发送WBAT无法识别的FieldID，该软件会直接挂掉，具体表现为XE的IP可以ping通，
     但是Socket连接不上，此时发送telnet 指令到26端口可唤醒WBAT软件
 2. 所有指令中关于UdpPort端口的FieldID应该为0x202C，开发手册上部分为0x202B，如果照0x202B发送则
     如1所言WBAT软件会挂掉，监测软件中表现为设备连接异常
//////////////////////////////////////////////////////////////////////////////////*/
internal static class XeBitConverter
{
    public static short ToInt16(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 2);
        return BitConverter.ToInt16(value, startIndex);
    }

    public static ushort ToUInt16(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 2);
        return BitConverter.ToUInt16(value, startIndex);
    }

    public static int ToInt32(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 4);
        return BitConverter.ToInt32(value, startIndex);
    }

    public static uint ToUInt32(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 4);
        return BitConverter.ToUInt32(value, startIndex);
    }

    public static long ToInt64(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 8);
        return BitConverter.ToInt64(value, startIndex);
    }

    public static ulong ToUInt64(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 8);
        return BitConverter.ToUInt64(value, startIndex);
    }

    public static float ToFloat(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 4);
        return BitConverter.ToSingle(value, startIndex);
    }

    public static double ToDouble(byte[] value, int startIndex)
    {
        Array.Reverse(value, startIndex, 8);
        return BitConverter.ToDouble(value, startIndex);
    }

    public static T ToValue<T>(byte[] bytes, int startIndex) where T : struct, IConvertible
    {
        var type = typeof(T);
        if (!type.IsValidNumeric()) return default;
        object value;
        try
        {
            if (type == typeof(sbyte))
                value = (sbyte)bytes[startIndex];
            else if (type == typeof(byte))
                value = bytes[startIndex];
            else if (type == typeof(char))
                value = Convert.ToChar(bytes[startIndex]);
            else if (type == typeof(bool))
                value = Convert.ToBoolean(bytes[startIndex]);
            else if (type == typeof(short))
                value = ToInt16(bytes, startIndex);
            else if (type == typeof(ushort))
                value = ToUInt16(bytes, startIndex);
            else if (type == typeof(int))
                value = ToInt32(bytes, startIndex);
            else if (type == typeof(uint))
                value = ToUInt32(bytes, startIndex);
            else if (type == typeof(long))
                value = ToInt64(bytes, startIndex);
            else if (type == typeof(ulong))
                value = ToUInt64(bytes, startIndex);
            else if (type == typeof(float))
                value = ToFloat(bytes, startIndex);
            else if (type == typeof(double))
                value = ToDouble(bytes, startIndex);
            else
                value = default(T);
        }
        catch
        {
            value = default(T);
        }

        return (T)value;
    }

    public static byte[] GetBytes(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(ushort value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(ulong value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(float value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetBytes(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        return bytes;
    }

    public static byte[] GetTBytes<T>(T value) where T : struct, IConvertible
    {
        var type = typeof(T);
        if (!type.IsValidNumeric()) return null;
        byte[] bytes;
        try
        {
            if (value is sbyte tSByte)
                bytes = new[] { (byte)tSByte };
            else if (value is byte tByte)
                bytes = new[] { tByte };
            else if (value is char tChar)
                bytes = BitConverter.GetBytes(tChar);
            else if (value is bool tBool)
                bytes = BitConverter.GetBytes(tBool);
            else if (value is short tShort)
                bytes = BitConverter.GetBytes(tShort);
            else if (value is ushort tUShort)
                bytes = BitConverter.GetBytes(tUShort);
            else if (value is int tInt)
                bytes = BitConverter.GetBytes(tInt);
            else if (value is uint tUInt)
                bytes = BitConverter.GetBytes(tUInt);
            else if (value is long tLong)
                bytes = BitConverter.GetBytes(tLong);
            else if (value is ulong tULong)
                bytes = BitConverter.GetBytes(tULong);
            else if (value is float tFloat)
                bytes = BitConverter.GetBytes(tFloat);
            else if (value is double tDouble)
                bytes = BitConverter.GetBytes(tDouble);
            else
                return null;
            Array.Reverse(bytes);
        }
        catch
        {
            return null;
        }

        return bytes;
    }

    public static T[] ToArrayT<T>(byte[] value, int startIndex, int length) where T : struct, IConvertible
    {
        var elemsize = Marshal.SizeOf(default(T));
        var elemnumber = length / elemsize;
        var result = new T[elemnumber];
        for (var i = 0; i < elemnumber && startIndex < value.Length; ++i)
        {
            var t = ToValue<T>(value, startIndex);
            result[i] = t;
            startIndex += elemsize;
        }

        return result;
    }

    public static byte[] GetArrayTBytes<T>(T[] value) where T : struct, IConvertible
    {
        if (!typeof(T).IsValidNumeric()) return null;
        var bytes = new List<byte>();
        foreach (var item in value)
        {
            var tempBytes = GetTBytes(item);
            if (tempBytes != null) bytes.AddRange(tempBytes);
        }

        return bytes.ToArray();
    }

    internal static bool IsValidNumeric(this Type value)
    {
        return value == typeof(byte) ||
               value == typeof(short) ||
               value == typeof(int) ||
               value == typeof(long) ||
               value == typeof(sbyte) ||
               value == typeof(ushort) ||
               value == typeof(uint) ||
               value == typeof(ulong) ||
               value == typeof(double) ||
               value == typeof(float) ||
               value == typeof(bool) ||
               value == typeof(char);
    }
}

/// <summary>
///     主要为XE设备中关于参数转换的一些辅助函数
/// </summary>
internal static class XeAssister
{
    //解调模式转换
    //[0x200B] Type of modulation :
    //0 : A3E, 1 : F3E, 2 : H3E- , 3 : H3E+ , 4 : J3E- , 5 : J3E+ , 6 : A0, 
    //7 : F1B, 8 : A1A, 9 : N0N, 10 : R3E- , 11 : R3E+ , 12 : G3E
    public static Modulation GetDemoduMode(uint type)
    {
        var mode = Modulation.Fm;
        switch (type)
        {
            case 0:
                mode = Modulation.Am;
                break;
            case 1:
                mode = Modulation.Fm;
                break;
            case 4:
                mode = Modulation.Lsb;
                break;
            case 5:
                mode = Modulation.Usb;
                break;
            case 8:
                mode = Modulation.Cw;
                break;
            case 12:
                mode = Modulation.Pm;
                break;
        }

        return mode;
    }

    public static uint GetDemoduMode(Modulation mode)
    {
        uint type = 1;
        switch (mode)
        {
            case Modulation.Am:
                type = 0;
                break;
            case Modulation.Fm:
                type = 1;
                break;
            case Modulation.Lsb:
                type = 4;
                break;
            case Modulation.Usb:
                type = 5;
                break;
            case Modulation.Cw:
                type = 8;
                break;
            case Modulation.Pm:
                type = 12;
                break;
        }

        return type;
    }

    public static ushort GetQualityMark(int qualityThreshold)
    {
        ushort qualityMark;
        if (qualityThreshold == 100)
            qualityMark = 9;
        else
            qualityMark = (ushort)(qualityThreshold / 10);
        return qualityMark;
    }
}