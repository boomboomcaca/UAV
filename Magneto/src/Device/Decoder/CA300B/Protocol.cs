using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Magneto.Device.CA300B;

/// <summary>
///     节目信息头
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct ProgramInfoHead
{
    /// <summary>
    ///     数据头标识
    /// </summary>
    public readonly byte Head;

    /// <summary>
    ///     数据类型
    /// </summary>
    public readonly byte DataType;

    /// <summary>
    ///     频点
    /// </summary>
    public readonly ushort freq;

    /// <summary>
    ///     当前频点节目数
    /// </summary>
    public readonly byte ProgramCnt;
}

/// <summary>
///     节目信息
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct ProgramInfo
{
    /// <summary>
    ///     节目ID
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
    public readonly byte[] ProgramId;

    /// <summary>
    ///     节目是否加密
    /// </summary>
    public readonly byte ProgramEncrypt;

    /// <summary>
    ///     节目名称长度
    /// </summary>
    public readonly byte ProgramNameLen;
}

public enum DtmbPlayErrorCode
{
    [Description("节目播放成功")] PlaySuccess,
    [Description("搜索节目中心频点错误")] FreqError,
    [Description("播放节目频道ID号错误")] NumError,
    [Description("搜索节目频点没有锁定")] FreqUnLock,
    [Description("恢复出厂设置成功")] ResetSuccess
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct ProgramPlayRtnInfo
{
    /// <summary>
    ///     数据包头
    /// </summary>
    public readonly byte PackageHead;

    /// <summary>
    ///     数据类型
    /// </summary>
    public readonly byte DataType;

    /// <summary>
    ///     频点
    /// </summary>
    public readonly ushort Freq;

    /// <summary>
    ///     数据
    /// </summary>
    public readonly byte Data;
}

public static class Ca300BProtocol
{
    public const string Dvbt8M = "at+reset;";
    public const string Dvbt6M = "at+country;";

    /// <summary>
    ///     打通指定通道
    /// </summary>
    public static readonly string OpenChannelCmd = "*PATH:{0}\r\n";

    /// <summary>
    ///     模拟电视搜索指令头
    /// </summary>
    public static byte[] AnatvHead = { 0x55, 0xAA, 0x07, 0x51 };

    /// <summary>
    ///     模拟电视播放指令头
    /// </summary>
    public static readonly byte[] AnatvPlayHead = { 0x55, 0xAA, 0x06, 0x50 };

    /// <summary>
    ///     模拟电视设置音量指令头
    /// </summary>
    //public static byte[] ANATVVolumeHead = new byte[] { 0xAB, 0xCD, 0x04, 0xFF };
    public static byte[] AnatvVolumeAddCmd = { 0x55, 0xAA, 0x03, 0x44, 0x01, 0x46 };

    public static byte[] AnatvVolumeMinusCmd = { 0x55, 0xAA, 0x03, 0x45, 0x01, 0x47 };

    /// <summary>
    ///     结尾标识\r\n
    /// </summary>
    public static byte[] End = { 0x0D, 0x0A };

    /// <summary>
    ///     DTMB 搜索指令头
    /// </summary>
    public static readonly string Dtmbhead = "at+search=";

    /// <summary>
    ///     DTMB 播放指令头
    /// </summary>
    public static readonly string DtmbPlayHead = "at+play=";

    /// <summary>
    ///     DTMB 音量设置指令头
    /// </summary>
    public static string DtmbVolumeHead = "at+vol=";

    /// <summary>
    ///     将字节数组拷贝到结构变量中
    /// </summary>
    /// <param name="btSrc">字节数组</param>
    /// <param name="iOffset">字节数组的起始位置</param>
    /// <param name="tDstn">目的结构类型</param>
    /// <param name="obj">目的结构变量的引用</param>
    /// <returns>拷贝的字节数</returns>
    public static int CopyBytes2Struct(byte[] btSrc, int iOffset, Type tDstn, ref object obj)
    {
        if (btSrc.Length - iOffset < Marshal.SizeOf(tDstn)) //源数据缓冲区长度小于需要拷贝的结构长度
            return 0;
        var buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf(tDstn));
        Marshal.Copy(btSrc, iOffset, buffer, Marshal.SizeOf(tDstn));
        obj = Marshal.PtrToStructure(new IntPtr(buffer.ToInt64()), tDstn);
        Marshal.FreeCoTaskMem(buffer);
        return Marshal.SizeOf(tDstn);
    }

    /// <summary>
    ///     获取枚举类的描述信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumItemName"></param>
    public static string GetDescriptionByName<T>(this T enumItemName)
    {
        var fi = enumItemName.GetType().GetField(enumItemName.ToString() ?? string.Empty);
        var attributes = (DescriptionAttribute[])fi?.GetCustomAttributes(
            typeof(DescriptionAttribute), false);
        if (attributes?.Length > 0) return attributes[0].Description;
        return enumItemName.ToString();
    }
}