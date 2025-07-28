//CopyRight(c) 2022 Nature.All rights reserved.文件名称:
//GuideDefine.h
//指控协议相关定义，详见 光电指控协议Vx.x.docx
//光电信息包类型

using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.Nvt.SDK;

public enum OptoCfgE
{
    OptoCmdNone = 0x00,

    /// <summary>
    ///     光电设备状态信息包
    /// </summary>
    OptoCmdStatus = 0x01,

    /// <summary>
    ///     光电设备方位、俯仰信息包
    /// </summary>
    OptoCmdHorver = 0x02,

    /// <summary>
    ///     设置光电目址信息包
    /// </summary>
    OptoCmdPosition = 0x03,

    /// <summary>
    ///     设置光电搜索跟踪信息包
    /// </summary>
    OptoCmdTrack = 0x04,

    /// <summary>
    ///     光电转发干扰查询
    /// </summary>
    OptoCmdInterfereQuery = 0x05,

    /// <summary>
    ///     光电转发干扰控制
    /// </summary>
    OptoCmdInterfereCtrl = 0x06,

    /// <summary>
    ///     光电转发干扰状态
    /// </summary>
    OptoCmdInterfereStatus = 0x07,

    /// <summary>
    ///     光电设备状态扩展信息包
    /// </summary>
    OptoCmdStatusExtend = 0x08,

    /// <summary>
    ///     光电镜头控制信息包
    /// </summary>
    OptoCmdLensCtrl = 0x09,

    /// <summary>
    ///     光电扫描扩展信息包
    /// </summary>
    OptoCmdScanExtend = 0x0A,

    /// <summary>
    ///     光电目标上报信息包
    /// </summary>
    OptoCmdNotifyTarget = 0x0B,

    /// <summary>
    ///     光电镜头状态扩展信息包
    /// </summary>
    OptoCmdLensExtend = 0x0C,

    /// <summary>
    ///     转台方向控制
    /// </summary>
    OptoCmdPtzctrl = 0x0D,

    /// <summary>
    ///     切换视频跟踪源
    /// </summary>
    OptoCmdSwitchsource = 0x0E,

    /// <summary>
    ///     跟踪状态信息(脱靶量信息包)
    /// </summary>
    OptoCmdTrackStatus = 0x0F,

    /// <summary>
    ///     人工锁定目标(将视频指定区域作为锁定目标)
    /// </summary>
    OptoCmdManuaLock = 0x10
}

/// <summary>
///     干扰器
/// </summary>
public enum AnonymousEnum
{
    /// <summary>
    ///     关闭 & 正常
    /// </summary>
    StateClose = 0x10,

    /// <summary>
    ///     开启 & 告警
    /// </summary>
    StateOpen = 0x11
}

public enum AnonymousEnum2
{
    /// <summary>
    ///     模块1(840-2600MHZ)
    /// </summary>
    ChannelVar1 = 0x01,

    /// <summary>
    ///     模块2(2600-6000MHZ)
    /// </summary>
    ChannelVar2 = 0x02,

    /// <summary>
    ///     模块3(1570-1620MHZ)
    /// </summary>
    ChannelVar3 = 0x03,

    /// <summary>
    ///     GPS/BD
    /// </summary>
    ChannelFix1 = 0x04,

    /// <summary>
    ///     350M
    /// </summary>
    ChannelFix2 = 0x10,

    /// <summary>
    ///     430M
    /// </summary>
    ChannelFix3 = 0x11,

    /// <summary>
    ///     850M
    /// </summary>
    ChannelFix4 = 0x12,

    /// <summary>
    ///     1200M
    /// </summary>
    ChannelFix5 = 0x21,

    /// <summary>
    ///     2400M
    /// </summary>
    ChannelFix6 = 0x31,

    /// <summary>
    ///     5800M
    /// </summary>
    ChannelFix7 = 0x32,

    /// <summary>
    ///     全部控制通道
    /// </summary>
    ChannelFix8 = 0xA1,

    /// <summary>
    ///     GPS+全部控制通道
    /// </summary>
    ChannelFix9 = 0xB1
}

public enum AnonymousEnum3
{
    /// <summary>
    ///     控制
    /// </summary>
    OrderCtrl = 0xA1,

    /// <summary>
    ///     查询
    /// </summary>
    OrderQuery = 0xB1,

    /// <summary>
    ///     状态
    /// </summary>
    OrderState = 0xB2
}

/// <summary>
///     光电设备状态信息 OPTO_CMD_STATUS
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagOptoDeviceStateT
{
    /// <summary>
    ///     视频源 0-可见光 1-热成像
    /// </summary>
    public uint Channel;

    /// <summary>
    ///     故障编码
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Fault;

    /// <summary>
    ///     0:空闲 1:搜索 2:跟踪
    /// </summary>
    public uint Mode;

    /// <summary>
    ///     光电编号
    /// </summary>
    public uint Number;

    /// <summary>
    ///     0:异常    1:正常
    /// </summary>
    public uint State;

    public ulong Timestamp;
}

/// <summary>
///     目标信息
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagObjectInfoT
{
    /// <summary>
    ///     目标高度(像素)
    /// </summary>
    public int Height;

    /// <summary>
    ///     方位角
    /// </summary>
    public double HorAngle;

    /// <summary>
    ///     目标编号
    /// </summary>
    public uint Index;

    /// <summary>
    ///     相似度
    /// </summary>
    public uint Likeness;

    /// <summary>
    ///     目标类型
    /// </summary>
    public uint Type;

    /// <summary>
    ///     俯仰角
    /// </summary>
    public double VerAngle;

    /// <summary>
    ///     目标宽度(像素: 基于1920x1080)
    /// </summary>
    public int Width;
}

/// <summary>
///     空闲外带数据
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagIdleDataT
{
    /// <summary>
    ///     持续时间
    /// </summary>
    public uint Time;
}

/// <summary>
///     搜索外带数据
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagSearchDataT
{
    /// <summary>
    ///     目标数量
    /// </summary>
    public uint Count;

    /// <summary>
    ///     object_list数据长度
    /// </summary>
    public uint Datalen;

    /// <summary>
    ///     持续时间
    /// </summary>
    public uint Time;
    //tagObjectInfo_t object_list[0];
}

/// <summary>
///     跟踪状态数据包
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagTrackingDataT
{
    /// <summary>
    ///     目标状态1-锁定 0 -丢失
    /// </summary>
    public ushort ObjState;

    public IntPtr TagObjectInfoT;

    /// <summary>
    ///     持续时间
    /// </summary>
    public uint Time;

    /// <summary>
    ///     add by zzt: 2020-11-1扩展两个字段用作用户ID回传
    /// </summary>
    public ushort UserId;
}

/// <summary>
///     光电设备状态扩展信息 OPTO_CMD_STATUS_EXTEND
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagOptoDeviceStateExT
{
    /// <summary>
    ///     外带数据长度
    /// </summary>
    public uint Datalen;

    /// <summary>
    ///     0:空闲 1:搜索 2:跟踪 3: 开始引导 4 : 引导结束(到位)
    /// </summary>
    public uint Mode;

    /// <summary>
    ///     光电编号
    /// </summary>
    public uint Number;

    public ulong Timestamp;
}

/// <summary>
///     目标扩展信息 2.10.1
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagObjectExInfoT
{
    /// <summary>
    ///     方向 0: 静止;  1: 左;   2: 右;3: 上;    4: 下;   5：左上;6：右上;  7: 左下; 8: 右下;
    /// </summary>
    public byte Direct;

    /// <summary>
    ///     目标距离
    /// </summary>
    public uint Distance;

    /// <summary>
    ///     目标高度(像素)
    /// </summary>
    public uint Height;

    /// <summary>
    ///     方位角
    /// </summary>
    public double HorAngle;

    /// <summary>
    ///     水平速度 像素/s
    /// </summary>
    public ushort HorSpeed;

    /// <summary>
    ///     目标编号
    /// </summary>
    public uint Index;

    /// <summary>
    ///     相似度
    /// </summary>
    public uint Likeness;

    /// <summary>
    ///     目标物理高度
    /// </summary>
    public uint PhyHeight;

    /// <summary>
    ///     水平速度 m/s     * 100
    /// </summary>
    public ushort PhyHorSpeed;

    /// <summary>
    ///     垂直速度 m/s * 100
    /// </summary>
    public ushort PhyVerSpeed;

    /// <summary>
    ///     目标物理宽度       实际宽度 * 100,单位 m
    /// </summary>
    public uint PhyWidth;

    /// <summary>
    ///     保留
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Temp;

    /// <summary>
    ///     目标类型
    /// </summary>
    public uint Type;

    /// <summary>
    ///     俯仰角
    /// </summary>
    public double VerAngle;

    /// <summary>
    ///     垂直速度 像素/s
    /// </summary>
    public ushort VerSpeed;

    /// <summary>
    ///     目标宽度(像素: 基于1920x1080)
    /// </summary>
    public uint Width;
}

/// <summary>
///     光电目标上报信息包 OPTO_CMD_NOTIFY_TARGET
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagSearchDataExT
{
    /// <summary>
    ///     目标数量
    /// </summary>
    public uint Count;

    /// <summary>
    ///     object_list数据长度
    /// </summary>
    public uint Datalen;

    /// <summary>
    ///     持续时间
    /// </summary>
    public uint Time;
    //tagObjectExInfo_t object_list[0];
}

/// <summary>
///     光电设备方位、俯仰信息 OPTO_CMD_HORVER
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagOptoHvInfoT
{
    /// <summary>
    ///     镜头倍数
    /// </summary>
    public byte ByLensPos;

    public byte ByTemp;

    /// <summary>
    ///     目标距离
    /// </summary>
    public double Distance;

    /// <summary>
    ///     水平角度
    /// </summary>
    public double HorAngle;

    /// <summary>
    ///     水平角速度
    /// </summary>
    public double HorAngularVelocity;

    /// <summary>
    ///     光电编号
    /// </summary>
    public uint Number;

    public ulong Timestamp;

    /// <summary>
    ///     俯仰角度
    /// </summary>
    public double VerAngle;

    /// <summary>
    ///     俯仰角速度
    /// </summary>
    public double VerAngularVelocity;

    /// <summary>
    ///     目标高度
    /// </summary>
    public ushort WHeight;
}

/// <summary>
///     设置光电目址信息 OPTO_CMD_POSITION
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagTargetPositionT
{
    /// <summary>
    ///     目标距离
    /// </summary>
    public double Distance;

    /// <summary>
    ///     目标高度
    /// </summary>
    public double Height;

    /// <summary>
    ///     目标方位
    /// </summary>
    public double HorAngle;

    /// <summary>
    ///     目标纬度
    /// </summary>
    public double Latitude;

    /// <summary>
    ///     目标经度
    /// </summary>
    public double Longitude;

    /// <summary>
    ///     引导模式，0-方位俯仰距离，1-经纬高
    /// </summary>
    public byte Mode;

    /// <summary>
    ///     光电编号
    /// </summary>
    public uint OptoNumber;

    /// <summary>
    ///     系统编号
    /// </summary>
    public uint SystemNumber;

    public ulong Timestamp;

    /// <summary>
    ///     远离还是抵近：0-抵近  1--远离
    /// </summary>
    public byte Type;

    /// <summary>
    ///     用户关联ID 2020-11-01
    /// </summary>
    public ushort UserId;

    /// <summary>
    ///     目标俯仰
    /// </summary>
    public double VerAngle;
}

/// <summary>
///     控制光电镜头: 2.8  OPTO_CMD_LENS_CTRL
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagCtrlLensT
{
    public byte Channel;
    public uint Cmd;
    public uint FocusPos;
    public uint LensPos;
    public uint OptoNumber;
    public uint SystemNumber;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Temp;

    public ulong Timestamp;
    public uint ZoomPos;
}

/// <summary>
///     扇扫扩展信息包 OPTO_CMD_SCAN_EXTEND
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagScanExT
{
    public uint Cmd;
    public int HorStart;
    public int HorStop;
    public uint OptoNumber;

    /// <summary>
    ///     扫描速度
    /// </summary>
    public int ScanSpeed;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Temp;

    public ulong Timestamp;
    public int VerStart;
    public int VerStop;
}

/// <summary>
///     设置光电搜索跟踪 OPTO_CMD_TRACK
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagTraceCfgT
{
    public uint Cmd;
    public uint HorStart;
    public uint HorStop;

    /// <summary>
    ///     目标编号
    /// </summary>
    public uint ObjNo;

    public uint OptoNumber;
    public ulong Timestamp;
    public uint VerStart;
    public uint VerStop;
}

/// <summary>
///     镜头信息包 OPTO_CMD_LENS_EXTEND
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagLensInfoT
{
    /// <summary>
    ///     热像水平fov * 100
    /// </summary>
    public uint FlirHfov;

    /// <summary>
    ///     热像物理焦距 * 100
    /// </summary>
    public uint FlirPhyFocus;

    /// <summary>
    ///     热像垂直fov * 100
    /// </summary>
    public uint FlirVfov;

    /// <summary>
    ///     热像倍数
    /// </summary>
    public uint FlirZoomBs;

    /// <summary>
    ///     水平fov * 100
    /// </summary>
    public uint HorFov;

    /// <summary>
    ///     光电编号
    /// </summary>
    public uint Number;

    /// <summary>
    ///     物理焦距 * 100
    /// </summary>
    public uint PhyFocus;

    public ulong Timestamp;

    /// <summary>
    ///     垂直fov * 100
    /// </summary>
    public uint VerFov;

    /// <summary>
    ///     可见光倍数
    /// </summary>
    public uint ZoomBs;
}

/// <summary>
///     转台控制信息包 OPTO_CMD_PTZCTRL
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagPtzCtlT
{
    public uint Cmd;
    public uint OptoNumber;

    /// <summary>
    ///     速度0-254
    /// </summary>
    public uint Speed;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Temp;

    public ulong Timestamp;

    /// <summary>
    ///     运动方式
    /// </summary>
    public uint Type;
}

/// <summary>
///     视频跟踪源切换 OPTO_CMD_SWITCHSOURCE
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagVideoSourceT
{
    /// <summary>
    ///     视频源通道 0-可见光 1-热成像
    /// </summary>
    public uint Channel;

    public uint OptoNumber;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Temp;

    public ulong Timestamp;
}

/// <summary>
///     光电转发干扰状态 OPTO_CMD_INTERFERE_STATUS
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagDisturberT
{
    /// <summary>
    ///     通道标识
    /// </summary>
    public byte Channel;

    /// <summary>
    ///     校验
    /// </summary>
    public byte Check;

    /// <summary>
    ///     指令标识
    /// </summary>
    public byte Cmd;

    /// <summary>
    ///     目的设备
    /// </summary>
    public byte Dev;

    /// <summary>
    ///     结束位
    /// </summary>
    public ushort End;

    /// <summary>
    ///     结束频率
    /// </summary>
    public ushort EndFq;

    /// <summary>
    ///     预留
    /// </summary>
    public byte Reserved;

    /// <summary>
    ///     起始位
    /// </summary>
    public ushort Start;

    /// <summary>
    ///     开始频率
    /// </summary>
    public ushort StartFq;

    /// <summary>
    ///     开启状态
    /// </summary>
    public byte State;

    /// <summary>
    ///     温度告警
    /// </summary>
    public byte TempAlarm;

    /// <summary>
    ///     驻波告警
    /// </summary>
    public byte WaveAlarm;
}

/// <summary>
///     跟踪脱靶量信息:OPTO_CMD_TRACK_STATUS
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagTrackStatusT
{
    /// <summary>
    ///     视频源通道 0-可见光 1-热成像
    /// </summary>
    public uint Channel;

    /// <summary>
    ///     脱靶量水平偏移，左负右正,  单位:像素
    /// </summary>
    public int IhOffset;

    /// <summary>
    ///     脱靶量俯仰偏移，下负上正
    /// </summary>
    public int IvOffset;

    public uint OptoNumber;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Temp;

    public ulong Timestamp;
}

//数据包头
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagPacketHeadT
{
    /// <summary>
    ///     命令类型
    /// </summary>
    public uint DwCmdType;

    /// <summary>
    ///     包长度, 共（20+N）Byte，
    /// </summary>
    public uint DwPacketLen;

    /// <summary>
    ///     起始位
    /// </summary>
    public uint DwStartCode;

    /// <summary>
    ///     版本号
    /// </summary>
    public uint DwVersion;

    /// <summary>
    ///     时间戳
    /// </summary>
    public ulong U64TimeStamp;
}

//数据包尾
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagPacketTailT
{
    /// <summary>
    ///     错误校验
    /// </summary>
    public uint DwCheckCode;

    /// <summary>
    ///     序列号
    /// </summary>
    public uint DwSeq;

    /// <summary>
    ///     停止位
    /// </summary>
    public uint DwStopCode;
}

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define PACKET_EX_LEN (sizeof(tagPacketHead_t) + sizeof(tagPacketTail_t))
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public class TagPacketT
{
    /// <summary>
    ///     信息内容缓冲长度
    /// </summary>
    public int BufferLen;

    /// <summary>
    ///     信息内容长度
    /// </summary>
    public int DataLen;

    /// <summary>
    ///     信息内容缓冲
    /// </summary>
    public IntPtr PBuffer;

    public IntPtr StHead;
    public IntPtr StTail;
}