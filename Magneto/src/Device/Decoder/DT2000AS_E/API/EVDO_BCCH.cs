using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

[StructLayout(LayoutKind.Sequential)]
public struct EvdoBcch
{
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