using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Magneto.Contract.FFMpeg.PushStream;

internal static class Utils
{
    #region IntPtr 与 数组转换

    /// <summary>
    ///     ArrayToIntptr
    /// </summary>
    /// <param name="source"></param>
    public static IntPtr ArrayToIntptr(byte[] source)
    {
        if (source == null) return IntPtr.Zero;
        var da = source;
        var ptr = Marshal.AllocHGlobal(da.Length);
        Marshal.Copy(da, 0, ptr, da.Length);
        return ptr;
    }

    #endregion

    /// <summary>
    ///     获取字节内容块
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public static byte[] GetPart(byte[] buffer, ref int offset, int size)
    {
        var result = Array.Empty<byte>();
        if (buffer?.Length > 0)
            if (offset <= buffer.Length - 1)
            {
                int len;
                // 计算实际数组长度
                if (offset + size <= buffer.Length - 1)
                    len = size;
                else
                    len = buffer.Length - offset;
                result = new byte[len];
                Array.Copy(buffer, offset, result, 0, result.Length);
            }

        offset += result.Length;
        return result;
    }

    /// <summary>
    ///     获取字节内容块
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public static byte[] GetPart(ArrayList buffer, int offset, int size)
    {
        var result = Array.Empty<byte>();
        if (buffer?.Count > 0)
            if (offset <= buffer.Count - 1)
            {
                int len;
                // 计算实际数组长度
                if (offset + size <= buffer.Count - 1)
                    len = size;
                else
                    len = buffer.Count - offset;
                result = new byte[len];
                Array.Copy(buffer.ToArray(), offset, result, 0, result.Length);
            }

        return result;
    }

    /// <summary>
    ///     移除元素
    /// </summary>
    /// <param name="arraylist"></param>
    /// <param name="offset"></param>
    /// <param name="bufsize"></param>
    public static void Remove(ArrayList arraylist, int offset, int bufsize)
    {
        arraylist.RemoveRange(offset, bufsize);
    }

    #region IntPtr 与 目标结构转换

    /// <summary>
    ///     结构转换为IntPtr
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    public static IntPtr StructToIntPtr<T>(T info)
    {
        var size = Marshal.SizeOf(info);
        var intPtr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(info, intPtr, false);
        return intPtr;
    }

    /// <summary>
    ///     IntPtr 转换为结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="info"></param>
    public static T IntPtrToStruct<T>(IntPtr info)
    {
        return (T)Marshal.PtrToStructure(info, typeof(T));
    }

    #endregion
}