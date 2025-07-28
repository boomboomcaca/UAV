using System;
using System.Collections.Generic;

namespace Magneto.Device.Nvt;

public enum UavCtrlProCmd
{
    UavCtrlProCmdNone = 0,

    /// 光电设备状态信息包
    UavCtrlProCmdStatus = 0x01,

    /// 光电设备方位、俯仰信息包
    UavCtrlProCmdPosition = 0x02,

    /// 设置光电目址信息包
    UavCtrlProCmdTargetAddr = 0x03,

    /// 设置光电搜索跟踪信息包
    UavCtrlProCmdTrack = 0x04,

    /// 光电转发干扰查询
    UavCtrlProCmdInterfereQuery = 0x05,

    /// 光电转发干扰控制
    UavCtrlProCmdInterfereCtrl = 0x06,

    /// 光电转发干扰状态
    UavCtrlProCmdInterfereStatus = 0x07,

    /// 光电设备状态扩展信息包
    UavCtrlProCmdStatusExtend = 0x08,

    /// 光电镜头控制信息包
    UavCtrlProCmdCameraLens = 0x09,

    /// 光电扫描扩展信息包
    UavCtrlSearchExtend = 0x0A,

    /// 光电目标上报信息包
    UavCtrlNotifyTarget = 0x0B,

    /// 光电镜头状态扩展信息包
    UavCtrlLensState = 0x0C,

    /// 转台方位控制信息包
    UavCtrlPtz = 0x0D,

    /// 切换视频跟踪源
    UavCtrlTrackSrc = 0x0E,

    /// 脱靶量上报信息包
    UavProDeviationInfo = 0x0F,

    /// 手动锁定目标
    UavProManualTrack = 0x10,

    /// 光电外围设备控制
    UavProFdoCtrl = 0x11,

    /// AI 参数控制
    UavCtrlAiParams = 0x12,

    /// 聚焦模式切换
    UavCtrlFocusMode = 0x13,

    /// 配置光电根据目标大小自动变倍参数
    UavCtrlTrackLenseCfg = 0x14,

    /// 光电系统状态扩展信息包
    UavCtrlStateExtend = 0x15
}

public enum TracksDirection
{
    /// 抵近
    TracksDirectionClosein = 0,
    /// 远离
    TracksDirectionFar = 1 
}

public enum GuideType
{
    /// 方位角
    GuideTypeAzimuth = 0,

    /// 经纬度
    GuideTypeLngLat = 1 
}

public enum StateType
{
    /// 关闭
    CloseState = 0,

    /// 开启
    OpenState = 1 
}

/// <summary>
///     @brief 协议头
/// </summary>
public abstract class CCameraUavProtocol
{
    /// <summary>
    ///     信息序列号 7
    ///     第一条 1,每次累加
    /// </summary>
    private static uint _messageIndex;

    /// <summary>
    ///     协议号 2
    /// </summary>
    private readonly uint _proNumber = 9002;

    /// <summary>
    ///     起始位 1
    /// </summary>
    private readonly byte[] _startBit = [0x88, 0x89, 0x80, 0x8A];

    /// <summary>
    ///     停止位
    /// </summary>
    private readonly byte[] _stopBit = [0x89, 0x80, 0x8A, 0x8B];

    /// <summary>
    ///     错误校验 8
    /// </summary>
    private uint _verify;

    /// <summary>
    ///     命令字 4
    /// </summary>
    public uint Cmd;

    /// <summary>
    ///     包长度 3
    ///     =命令字+时间戳+信息内容+信息序列号+错误校验
    /// </summary>
    public uint PacketLen;

    /// <summary>
    ///     时间戳 5
    /// </summary>
    public ulong Timestamp;

    public byte[] ToBytes(byte[] subClass)
    {
        List<byte> re = [];
        re.AddRange(_startBit);
        re.AddRange(BitConverter.GetBytes(_proNumber));
        PacketLen = 20 + (uint)subClass.Length;
        re.AddRange(BitConverter.GetBytes(PacketLen));
        re.AddRange(BitConverter.GetBytes(Cmd));
        re.AddRange(BitConverter.GetBytes(Timestamp));
        re.AddRange(subClass);
        _messageIndex++;
        re.AddRange(BitConverter.GetBytes(_messageIndex));
        for (var i = 8; i < re.Count; i++) _verify += re[i];
        re.AddRange(BitConverter.GetBytes(_verify));
        re.AddRange(_stopBit);
        return re.ToArray();
    }
}

/// <summary>
///     设置光电目址信息包
/// </summary>
public class CTargetDirectionProtocol : CCameraUavProtocol
{
    /// <summary>
    ///     目标距离 7
    /// </summary>
    public readonly double Distance = 0;

    /// <summary>
    ///     0-	方位、俯仰、距离 1 - 经度、纬度、高度
    /// </summary>
    public byte GuideType = 0;

    /// <summary>
    ///     目标高度 6
    /// </summary>
    public double Height = 0;

    /// <summary>
    ///     水平角度 7
    /// </summary>
    public readonly double HorAngle = 0;

    /// <summary>
    ///     目标纬度 5
    /// </summary>
    public double Lat = 0;

    /// <summary>
    ///     目标经度 4
    /// </summary>
    public double Lng = 0;

    /// <summary>
    ///     光电编号 1
    /// </summary>
    public readonly uint Number = 0;

    /// <summary>
    ///     系统编号 2
    /// </summary>
    public readonly uint SysNumber = 0;

    /// <summary>
    ///     0 --抵近 1--远离
    /// </summary>
    public readonly byte TargetDirection = 0;

    /// <summary>
    ///     系统下发 时间戳 3
    /// </summary>
    public new ulong Timestamp = 0;

    /// <summary>
    ///     用户Id
    /// </summary>
    public readonly ushort UserData = 0;

    /// <summary>
    ///     俯仰角度 8
    /// </summary>
    public readonly double VerAngle = 0;

    public byte[] ToBytes()
    {
        var re = new List<byte>();
        re.AddRange(BitConverter.GetBytes(Number));
        re.AddRange(BitConverter.GetBytes(SysNumber));
        var timestamp = (ulong)DateTime.Now.ToFileTimeUtc();
        re.AddRange(BitConverter.GetBytes(timestamp));
        re.AddRange(BitConverter.GetBytes(Lng));
        re.AddRange(BitConverter.GetBytes(Lat));
        re.AddRange(BitConverter.GetBytes(Height));
        re.AddRange(BitConverter.GetBytes(Distance));
        re.AddRange(BitConverter.GetBytes(HorAngle));
        re.AddRange(BitConverter.GetBytes(VerAngle));
        re.AddRange(BitConverter.GetBytes(UserData));
        re.Add(GuideType);
        re.Add(TargetDirection);
        Cmd = (uint)UavCtrlProCmd.UavCtrlProCmdTargetAddr;
        PacketLen = (uint)re.Count;
        base.Timestamp = timestamp;
        //byte[] byteArray = ToBytes(re.ToArray());
        //string hexString = string.Join(" ", byteArray.Select(b => "0x" + b.ToString("X2")));
        return ToBytes(re.ToArray());
    }
}