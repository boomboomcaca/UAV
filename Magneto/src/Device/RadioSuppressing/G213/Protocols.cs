using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Magneto.Device.G213;

internal class CommandFrame
{
    private const uint Header = 0xAA00FF00;
    private ushort _crc;
    private uint _frameLen;

    public CommandFrame(int deviceId, CommandType commandType, CommandBody body)
    {
        DeviceId = deviceId;
        CommandType = commandType;
        Body = body;
    }

    public CommandFrame(byte[] data, int offset)
    {
        BitConverter.ToUInt32(data, offset);
        offset += 4;
        _frameLen = BitConverter.ToUInt32(data, offset);
        offset += 4;
        DeviceId = BitConverter.ToInt32(data, offset);
        offset += 4;
        CommandType = (CommandType)BitConverter.ToUInt16(data, offset);
        offset += 2;
        _crc = BitConverter.ToUInt16(data, offset);
        offset += 2;
        switch (CommandType)
        {
            case CommandType.SendParameter:
            case CommandType.PowerControl:
            case CommandType.PerformCuring:
                break;
            case CommandType.QueryStatus:
                Body = new StatusResultBody(data, offset);
                break;
        }
    }

    public int DeviceId { get; }
    public CommandType CommandType { get; }
    public CommandBody Body { get; }

    public byte[] ToBytes()
    {
        var bodyBuffer = Body == null ? null : Body.ToBytes();
        var bodyLen = bodyBuffer == null ? 0 : bodyBuffer.Length;
        _frameLen = (uint)(4 + 4 + 4 + 2 + 2 + bodyLen);
        _crc = Crc16(bodyBuffer);
        var buffer = new byte[_frameLen];
        var offset = 0;
        var data = BitConverter.GetBytes(Header);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(_frameLen);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(DeviceId);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes((ushort)CommandType);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(_crc);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        if (bodyBuffer is { Length: > 0 }) Buffer.BlockCopy(bodyBuffer, 0, buffer, offset, bodyBuffer.Length);
        return buffer;
    }

    private static ushort Crc16(byte[] data)
    {
        if (data == null) return 0;
        var len = data.Length;
        if (len > 0)
        {
            ushort crc = 0xFFFF;
            for (var i = 0; i < len; i++)
            {
                crc = (ushort)(crc ^ data[i]);
                for (var j = 0; j < 8; j++) crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
            }

            return crc;
        }

        return 0;
    }
}

internal class CommandBody
{
    public virtual byte[] ToBytes()
    {
        return null;
    }
}

/// <summary>
///     下发干扰信号参数
/// </summary>
internal class ParameterBody : CommandBody
{
    /// <summary>
    ///     频段码
    /// </summary>
    public FrequencyBand FreqChannel { get; set; }

    /// <summary>
    ///     功放输出功率
    /// </summary>
    public short Power { get; set; }

    /// <summary>
    ///     信号数量
    /// </summary>
    public short SignalNumber { get; set; }

    /// <summary>
    ///     信号参数集合
    /// </summary>
    public List<SignalParameter> Parameters { get; set; }

    public override byte[] ToBytes()
    {
        var paramLen = Marshal.SizeOf(typeof(SignalParameter));
        var len = 2 + 2 + 2 + paramLen * 5;
        var buffer = new byte[len];
        var offset = 0;
        var band = (ushort)Math.Log((double)FreqChannel, 2);
        var data = BitConverter.GetBytes(band);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Power);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(SignalNumber);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        for (var i = 0; i < SignalNumber; i++)
        {
            data = Parameters[i].ToBytes();
            Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
            offset += data.Length;
        }

        return buffer;
    }
}

/// <summary>
///     干扰信号每个信号的参数
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
internal struct SignalParameter
{
    /// <summary>
    ///     信号类型
    ///     0：梳状谱干扰；
    ///     1：点频干扰(单 音)；
    ///     2：窄带噪声调频干扰；
    ///     3：宽带噪声干扰；
    ///     4：多音干扰；
    ///     5：线性调频；
    ///     6：协议压制
    ///     7: 窄带跳频
    /// </summary>
    [MarshalAs(UnmanagedType.I2)] public SignalType SignalType;

    /// <summary>
    ///     中心频率，单位10Hz
    /// </summary>
    [MarshalAs(UnmanagedType.U4)] public uint Frequency;

    /// <summary>
    ///     带宽，仅对窄带噪声调频干扰、宽带噪声干扰、线性调频、窄带跳频有效，单位10Hz
    /// </summary>
    [MarshalAs(UnmanagedType.U4)] public uint Bandwidth;

    /// <summary>
    ///     数字幅度衰减量，单位dB
    /// </summary>
    [MarshalAs(UnmanagedType.U2)] public ushort Attenuation;

    /// <summary>
    ///     频点个数，仅梳状谱、多音、跳频有效
    /// </summary>
    [MarshalAs(UnmanagedType.U2)] public ushort FrequencyNumber;

    /// <summary>
    ///     梳状谱每个齿宽度，单位10Hz 仅梳状谱有效， 0-为多音干扰
    /// </summary>
    [MarshalAs(UnmanagedType.U4)] public uint CombBandwidth;

    /// <summary>
    ///     频率间隔，单位10Hz 仅梳状谱、多音有效
    /// </summary>
    [MarshalAs(UnmanagedType.U4)] public uint CombStep;

    /// <summary>
    ///     线性调频扫描周期，单位μs 仅线性调频有效
    /// </summary>
    [MarshalAs(UnmanagedType.U4)] public uint LFMCycle;

    /// <summary>
    ///     协议压制类型
    ///     0-5:类型1~类型6
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] public int ProtocolSuppressType;

    /// <summary>
    ///     跳频速率 仅跳频信号有效
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] public int HoppingSpeed;

    /// <summary>
    ///     预留
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] public readonly int Reverse1;

    /// <summary>
    ///     预留
    /// </summary>
    [MarshalAs(UnmanagedType.I4)] public readonly int Reverse2;

    public byte[] ToBytes()
    {
        var len = Marshal.SizeOf(this);
        var buffer = new byte[len];
        var offset = 0;
        var data = BitConverter.GetBytes((short)SignalType);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Frequency);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Bandwidth);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Attenuation);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(FrequencyNumber);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(CombBandwidth);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(CombStep);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(LFMCycle);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(ProtocolSuppressType);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(HoppingSpeed);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Reverse1);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        data = BitConverter.GetBytes(Reverse2);
        if (!BitConverter.IsLittleEndian) Array.Reverse(data);
        Buffer.BlockCopy(data, 0, buffer, offset, data.Length);
        offset += data.Length;
        return buffer;
    }
}

internal class PowerControlBody : CommandBody
{
    public FrequencyBand Switch { get; set; }

    public override byte[] ToBytes()
    {
        var buffer = BitConverter.GetBytes((ushort)Switch);
        if (!BitConverter.IsLittleEndian) Array.Reverse(buffer);
        return buffer;
    }
}

/// <summary>
///     状态查询结果
///     ...?
/// </summary>
internal class StatusResultBody : CommandBody
{
    public StatusResultBody(byte[] data, int offset)
    {
        DeviceId = BitConverter.ToInt16(data, offset);
        offset += 2;
        PowerState = BitConverter.ToInt16(data, offset);
        offset += 2;
        Vswr = BitConverter.ToInt16(data, offset);
        offset += 2;
        Temp = BitConverter.ToInt16(data, offset);
        offset += 2;
        Current = BitConverter.ToInt16(data, offset);
        offset += 2;
        Volt = BitConverter.ToInt16(data, offset);
        offset += 2;
    }

    public short DeviceId { get; set; }

    /// <summary>
    ///     功放开关状态
    /// </summary>
    public short PowerState { get; set; }

    /// <summary>
    ///     功放驻波告警
    /// </summary>
    public short Vswr { get; set; }

    /// <summary>
    ///     功放温度告警
    /// </summary>
    public short Temp { get; set; }

    /// <summary>
    ///     功放电流告警
    /// </summary>
    public short Current { get; set; }

    /// <summary>
    ///     功放电压告警
    /// </summary>
    public short Volt { get; set; }
}