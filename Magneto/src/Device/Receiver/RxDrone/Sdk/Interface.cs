using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;

namespace Magneto.Device.RxDrone.Sdk;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct DjiFlightInfoStr
{
    /// <summary>
    ///     数据包类型
    /// </summary>
    public ushort PacketType;

    /// <summary>
    ///     序列号
    /// </summary>
    public ushort SeqNum;

    /// <summary>
    ///     状态信息
    /// </summary>
    public ushort StateInfo;

    /// <summary>
    ///     无人机序列号
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
    public string DroneSerialNum;

    /// <summary>
    ///     无人机经度
    /// </summary>
    public double DroneLongitude;

    /// <summary>
    ///     无人机纬度
    /// </summary>
    public double DroneLatitude;

    /// <summary>
    ///     海拔
    /// </summary>
    public float Altitude;

    /// <summary>
    ///     高度
    /// </summary>
    public float Height;

    /// <summary>
    ///     北向速度
    /// </summary>
    public float NorthSpeed;

    /// <summary>
    ///     东向速度
    /// </summary>
    public float EastSpeed;

    /// <summary>
    ///     上升速度
    /// </summary>
    public float UpSpeed;

    /// <summary>
    ///     俯仰角
    /// </summary>
    public short PitchAngle;

    /// <summary>
    ///     横滚角
    /// </summary>
    public short RollAngle;

    /// <summary>
    ///     偏航角
    /// </summary>
    public short YawAngle;

    /// <summary>
    ///     GPS时间
    /// </summary>
    public ulong GpsTime;

    /// <summary>
    ///     飞行员经度
    /// </summary>
    public double PilotLongitude;

    /// <summary>
    ///     飞行员纬度
    /// </summary>
    public double PilotLatitude;

    /// <summary>
    ///     家庭经度
    /// </summary>
    public double HomeLongitude;

    /// <summary>
    ///     家庭纬度
    /// </summary>
    public double HomeLatitude;

    /// <summary>
    ///     产品类型
    /// </summary>
    public byte ProductType;

    /// <summary>
    ///     产品类型字符串
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string ProductTypeStr;

    /// <summary>
    ///     UUID长度
    /// </summary>
    public byte UuidLength;

    /// <summary>
    ///     UUID
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
    public byte[] Uuid;

    /// <summary>
    ///     许可证
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] License;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
    public string WifiSsid;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
    public string WifiVendor;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] WifiDestinationAddress;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] WifiSourceAddress;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
    public byte[] WifiBssIdAddress;

    /// <summary>
    ///     电子指纹
    /// </summary>
    public ulong EFingerPrint;

    /// <summary>
    ///     频率
    /// </summary>
    public long Freq;

    /// <summary>
    ///     信号电平
    /// </summary>
    public float Rssi;

    /// <summary>
    ///     同步头的相关值
    /// </summary>
    public float SyncCorr;

    /// <summary>
    ///     带宽
    /// </summary>
    public float Bandwidth;

    /// <summary>
    ///     是否通过CRC校验
    /// </summary>
    public char DecodeFlag;

    public char WifiFlag;
    public ushort DeviceNum;

    public ushort AntennaCode;

    public int Offset10ms;

    ///// the following is for open drone id
    public byte ProtocalVersion;

    public byte
        UAType; //[0]Not Declared [1]Aeroplane,[2]Helicopter,[3]Gyroplane,[4]Hybrid Lift,[5]Ornithopter,[6]Glider[7]Kite [8]Free Balloon

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public byte[] DroneId;

    public ushort Direction;
    public float Speed;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 176)]
    public byte[] DjiByte176;

    public float Likelihood;
    public uint Hashcode;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public ushort[] Reserved;
}

public delegate void DroneCbFn(DjiFlightInfoStr message);

public delegate void SpectrumCbFn(float[] spectrum, long startFreq, int length, float resolution);

public static class GlobalMembers
{
    public const string LibPath = "ERadiodll";

    public static void Init()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "RxDrone", [LibPath]);
    }
}

internal static class DefineConstants
{
    public const int SpectrumScanSampleRate = 20000000;
}