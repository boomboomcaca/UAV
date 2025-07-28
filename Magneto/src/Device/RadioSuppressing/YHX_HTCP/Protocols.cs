using System;
using System.Linq;
using System.Text;

namespace Magneto.Device.YHX_HTCP;

public interface IGetBytes
{
    /// <summary>
    ///     取得该数据结构的二进制数组，发送给设备
    /// </summary>
    /// <returns></returns>
    byte[] GetBytes();
}

/// <summary>
///     公共方法与常量
/// </summary>
public class Common
{
    /// <summary>
    ///     系统状态自检
    /// </summary>
    public static readonly string Chk = "CHK";

    /// <summary>
    ///     系统设置（通信端口）等
    /// </summary>
    public static readonly string Set = "SET";

    /// <summary>
    ///     文本固化
    /// </summary>
    public static readonly string Tsd = "TSD";

    /// <summary>
    ///     频点发射
    /// </summary>
    public static readonly string Tra = "TRA";

    /// <summary>
    ///     云台控制（目前版本已取消）
    /// </summary>
    public static readonly string Rtt = "RTT";

    /// <summary>
    ///     心跳检测命令
    /// </summary>
    public static readonly string Htb = "HTB";

    /// <summary>
    ///     设备信息查询
    /// </summary>
    public static readonly string Inf = "INF";

    /// <summary>
    ///     开关机管理（仅针对Y8设备有效）
    /// </summary>
    public static readonly string Smm = "SMM";

    /// <summary>
    ///     固件升级（目前版本暂时取消了）
    /// </summary>
    public static readonly string Upd = "UPD";

    public static byte[] GetDataLength(int count)
    {
        var result = BitConverter.GetBytes((short)count);
        return result;
    }
}

/// <summary>
///     控制块 发送控制字段
/// </summary>
public class SendControlBits
{
    /// <summary>
    ///     数据块标志
    /// </summary>
    public readonly byte DataBlock;

    /// <summary>
    ///     参数块标志
    /// </summary>
    public readonly byte ParamsBlock;

    /// <summary>
    ///     接收确认
    /// </summary>
    public readonly byte RecvVerify;

    /// <summary>
    ///     重发标志
    /// </summary>
    public readonly byte Resend;

    /// <summary>
    ///     设置控制字段的各个位
    /// </summary>
    /// <param name="data">是否包含数据块</param>
    /// <param name="param">是否包含参数块</param>
    /// <param name="verify">是否需要设备回复确认</param>
    /// <param name="resend">重发还是第一次发，重发为1，第一次发送为0</param>
    public SendControlBits(byte data, byte param, byte verify, byte resend)
    {
        DataBlock = data;
        ParamsBlock = param;
        RecvVerify = verify;
        Resend = resend;
    }

    public SendControlBits()
    {
    }

    /// <summary>
    ///     发送控制块标志
    /// </summary>
    public byte DataBits
    {
        get
        {
            byte count = 0;
            if (DataBlock == 1) count += 1;
            if (ParamsBlock == 1) count += 2;
            if (RecvVerify == 1) count += 4;
            if (Resend == 1) count += 8;
            return count;
        }
    }
}

/// <summary>
///     完整的数据帧结构
/// </summary>
public class DataFrame : IGetBytes
{
    /// <summary>
    ///     校验和部分
    /// </summary>
    private byte _checkSum;

    /// <summary>
    ///     控制块部分
    /// </summary>
    public ControlBlock ControlData;

    /// <summary>
    ///     数据块部分
    /// </summary>
    public DataBlock Data;

    /// <summary>
    ///     参数块部分
    /// </summary>
    public ParamsBlock ParamsData;

    public DataFrame()
    {
        ParamsData = null;
        Data = null;
    }

    public byte[] GetBytes()
    {
        //控制块一定有，再加一字节的校验和
        var n = 8;
        var data = new byte[64 * 1024];
        // 控制块8字节
        var temp = ControlData.GetBytes();
        Array.Copy(temp, data, temp.Length);
        if (ParamsData != null)
        {
            temp = ParamsData.GetBytes();
            Array.Copy(temp, 0, data, n, temp.Length);
            n += ParamsData.GetBytes().Length;
        }

        if (Data != null)
        {
            temp = Data.GetBytes();
            Array.Copy(temp, 0, data, n, temp.Length);
            n += Data.GetBytes().Length;
        }

        _checkSum = Checksum(data);
        Array.Resize(ref data, n + 1);
        data[n] = _checkSum;
        return data;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(ControlData);
        if (ParamsData != null) sb.Append(ParamsData);
        if (Data != null) sb.Append(Data);
        return sb.ToString();
    }

    /// <summary>
    ///     逐字节累加取低位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private byte Checksum(byte[] data)
    {
        var sum = data.Aggregate(0, (current, t) => current + t);
        return BitConverter.GetBytes(sum)[0];
    }
}

/// <summary>
///     控制块
/// </summary>
public class ControlBlock : IGetBytes
{
    /// <summary>
    ///     控制块起始标志：#，十六进制0x23
    /// </summary>
    public const byte Header = (byte)'#';

    /// <summary>
    ///     保留字段 字节序号：6
    /// </summary>
    private readonly byte _reserved1 = 0;

    /// <summary>
    ///     保留字段 字节序号：7
    /// </summary>
    private readonly byte _reserved2 = 0;

    /// <summary>
    ///     报文长度 2~3 2字节【指令的总长度：控制块长度+参数块长度+数据块长度+校验位】
    /// </summary>
    public short DataLength;

    /// <summary>
    ///     参数段长度 4~5 2字节【参数快总长度包括“”“”】
    /// </summary>
    public short ParamsLength;

    /// <summary>
    ///     发送控制字段 1字节
    /// </summary>
    public byte SendControl;

    public byte[] GetBytes()
    {
        // 固定8个字节
        var data = new byte[8];
        // 0 起始位：#
        var temp = new byte[] { 0X23 };
        Array.Copy(temp, data, temp.Length);
        // 发送控制字段 1
        data[1] = SendControl;
        // 报文长度 2-3
        temp = BitConverter.GetBytes(DataLength).Reverse().ToArray();
        Array.Copy(temp, 0, data, 2, temp.Length);
        // 参数快长度 4-5
        temp = BitConverter.GetBytes(ParamsLength).Reverse().ToArray();
        Array.Copy(temp, 0, data, 4, temp.Length);
        // 保留位6,7
        data[6] = _reserved1;
        data[7] = _reserved2;
        return data;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Header={Header}").Append($"SendControl={SendControl}")
            .Append($"DiagramLength={DataLength}")
            .Append($"ParamsLength={ParamsLength}").Append($"Reserved1={_reserved1}")
            .Append($"Reserved2={_reserved2}");
        return sb.ToString();
    }
}

/// <summary>
///     参数块
/// </summary>
public class ParamsBlock : IGetBytes
{
    /// <summary>
    ///     协议功能代码
    ///     <para>CHK:系统自检 CHK:?</para>
    ///     <para>SET:系统设置</para>
    ///     <para>TSD:文本固化 数据块不超过2K</para>
    ///     <para>TRA:频点发射</para>
    ///     <para>RTT:云台控制</para>
    ///     <para>INF:设备信息查询</para>
    ///     <para>SMM:开关机管理</para>
    ///     <para>UPD:固件升级（此功能预留） 代码部分不超过1K</para>
    /// </summary>
    public string Function;

    /// <summary>
    ///     参数列表，形如：名称=值；如FREL=101.7MHz;代表扫频发射是下限频率=101.7MHz
    ///     <para>每个参数之间用分号;相隔，一个参数有多个值时，多个值之间用逗号,隔开</para>
    /// </summary>
    public string Params;

    public byte[] GetBytes()
    {
        return Encoding.ASCII.GetBytes(ToString() ?? string.Empty);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('<').Append(Function).Append(':').Append(Params).Append('>');
        return sb.ToString();
    }
}

/// <summary>
///     数据块
/// </summary>
public class DataBlock : IGetBytes
{
    /// <summary>
    ///     数据内容 以字符串的形式提供，固化文本不超过2K，固件升级一般1K
    /// </summary>
    public byte[] DataContent;

    /// <summary>
    ///     数据块类型，位置数据块1-2字节
    ///     <para>频点参数0x11；调制文本：0x12；固件更新数据：0x13</para>
    /// </summary>
    public short DataType;

    /// <summary>
    ///     数据长度 包含起始位[]
    /// </summary>
    private short DataLength { get; set; }

    //public override string ToString()
    //{
    //    var sb = new StringBuilder();
    //    sb.Append('[').
    //        Append(DataType).
    //        Append(DataContent).
    //        Append(']');
    //    return sb.ToString();
    //}
    public byte[] GetBytes()
    {
        // 因为数据块可能传输固化的文本字符串，也可能传输同步和跳频的频点，
        // 两种获取byte[]的方式不同，因此在此区别对待，若是这两者以外的对象则返回null
        //DataLength = (short)(System.Text.ASCIIEncoding.ASCII.GetByteCount(strContent) + 6);
        var data = new byte[4096];
        // 0 起始位：[
        var temp = new byte[] { 0X5B };
        Array.Copy(temp, data, temp.Length);
        // 1-2
        temp = BitConverter.GetBytes(DataType).Reverse().ToArray();
        Array.Copy(temp, 0, data, 1, temp.Length);
        // 数据 5-m-1
        temp = DataContent ?? Array.Empty<byte>();
        // 将数据放到数据位相应位置 5~m-1
        Array.Copy(temp, 0, data, 5, temp.Length);
        DataLength = (short)(temp.Length + 6);
        // 数据位长度 3-4
        temp = BitConverter.GetBytes(DataLength).Reverse().ToArray();
        Array.Copy(temp, 0, data, 3, temp.Length);
        // m 结束位：]
        temp = new byte[] { 0X5D };
        Array.Copy(temp, 0, data, DataLength - 1, temp.Length);
        Array.Resize(ref data, DataLength);
        return data;
    }
}

/// <summary>
///     命令发送状态信息
/// </summary>
public class GeneralInfo
{
    /// <summary>
    ///     状态信息内容，如发送成功，发送失败
    /// </summary>
    public string Info;

    /// <summary>
    ///     命令发送状态信息，发送名称
    /// </summary>
    public string Name = string.Empty;

    public override string ToString()
    {
        return $"执行任务={Name}，执行状态信息={Info}";
    }
}

/// <summary>
///     模块状态
/// </summary>
public class GeneralStatus
{
    /// <summary>
    ///     消息头，包含该消息种类，如设备自检等
    /// </summary>
    public readonly string Name = string.Empty;

    /// <summary>
    ///     消息内容
    /// </summary>
    public readonly string Status = string.Empty;

    /// <summary>
    ///     模块ID
    /// </summary>
    public int Id;

    public override string ToString()
    {
        return $"[Name={Name},ID={Id},Status={Status}]";
    }
}