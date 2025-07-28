using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct EvdoBcch
{
    //public byte ColorCode;
    //public uint SectorID24;
    //public ushort SectorSignature;
    //public ushort AccessSignature;
    //public byte MSG_ID;
    //public byte MIN_P_REV;
    //public byte MaximumRevision;
    //public byte MinimumRevision;
    //public ushort PILOT_PN;
    //public ushort PACKET_ZONE_ID;
    //public ushort NID;
    //public ushort SID;
    ////public  uint SYS_TIME_High32;
    ////public uint SYS_TIME_Low32;
    //public ushort CDMA_FREQ;
    //public float BASE_LAT;
    //public float BASE_LONG;
    //public ushort MCC;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    //public byte[] sector_id;//[32]; //每个为4比特
    //public byte sector_id_flag;
    //public byte SubnetMask;
    //public ushort RouteUpdateRadius;
    //public byte LeapSeconds;
    //public ushort LocalTimeOffset;
    //public byte ReverseLinkSilenceDuration;
    //public byte ReverseLinkSilencePeriod;
    //public ushort ChannelCount;
    //public ushort NeighborCount;
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    //public ushort[] channel;//[32];
    //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    //public ushort[] NeighborPilotPN;//[32];
    //                                ////////以下参数为非系统参数，为测量所得
    //public int rssi;
    //public int rxPacket;
    //public uint centerfrequency;
    //public ushort BASE_ID;
    public readonly sbyte ColorCode; //色码
    public readonly uint SectorID24; //扇区ID第24位
    public readonly ushort SectorSignature;
    public readonly ushort AccessSignature;
    public readonly sbyte MSG_ID;
    public readonly sbyte MIN_P_REV;
    public readonly sbyte MaximumRevision;
    public readonly sbyte MinimumRevision;
    public readonly ushort PILOT_PN; // 导频码
    public readonly ushort PACKET_ZONE_ID;
    public readonly ushort NID; //network Identification Number 网络ID
    public readonly ushort SID; //System Identification Number  系统ID
    public readonly ushort CDMA_FREQ; // CDMA频率号
    public readonly float BASE_LAT; //纬度
    public readonly float BASE_LONG; //经度
    public readonly ushort MCC; //国家码

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public readonly sbyte[] sector_id; //扇区ID 每个为4比特

    public readonly sbyte sector_id_flag;
    public readonly sbyte SubnetMask;
    public readonly ushort RouteUpdateRadius;
    public readonly sbyte LeapSeconds;
    public readonly ushort LocalTimeOffset;
    public readonly sbyte ReverseLinkSilenceDuration;
    public readonly sbyte ReverseLinkSilencePeriod;
    public readonly ushort ChannelCount;
    public readonly ushort NeighborCount;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public readonly ushort[] channel;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public readonly ushort[] NeighborPilotPN;

    public readonly int rssi; //接收电平
    public readonly int rxPacket;
    public readonly uint centerfrequency; //中心频率 (in Hz)
    public readonly ushort BASE_ID; //basestation ID， 基站ID
}