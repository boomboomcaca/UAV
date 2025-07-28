using System;
using System.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Contract;

/// <summary>
///     边缘端运行时数据缓存
/// </summary>
public static class RunningInfo
{
    public static string EdgeId { get; set; }

    /// <summary>
    ///     边缘端对外IP地址
    /// </summary>
    public static string EdgeIp { get; set; }

    /// <summary>
    ///     边缘端本地IP地址
    /// </summary>
    public static string LocalIpAddress { get; set; }

    /// <summary>
    ///     边缘端端口号
    /// </summary>
    public static int Port { get; set; }

    /// <summary>
    ///     云端IP地址
    /// </summary>
    public static string CloudIpAddress { get; set; }

    /// <summary>
    ///     云端端口
    /// </summary>
    public static int CloudPort { get; set; }

    /// <summary>
    ///     音频识别服务的地址
    /// </summary>
    public static string AudioRecognitionAddress { get; set; }

    /// <summary>
    ///     音频识别服务的端口
    /// </summary>
    public static int AudioRecognitionPort { get; set; }

    /// <summary>
    ///     音频识别服务的端口
    /// </summary>
    public static string AudioRecognitionServerKey { get; set; }

    /// <summary>
    ///     边缘端类型 0-边缘端主服务；1-环境监控服务
    /// </summary>
    public static int ServerType { get; set; }

    /// <summary>
    ///     边缘端对外显示IP地址的方式
    ///     0- 以云端返回的IP地址为准
    ///     1- 以本地配置的IP地址为准
    /// </summary>
    public static int IpType { get; set; }

    /// <summary>
    ///     云端状态
    /// </summary>
    public static bool CloudState { get; set; }

    /// <summary>
    ///     云端连接状态
    /// </summary>
    public static bool CloudClientState { get; set; }

    /// <summary>
    ///     机器ID
    /// </summary>
    public static string ComputerId { get; set; }

    /// <summary>
    ///     原始数据的真实存储路径
    /// </summary>
    public static string DataDir { get; set; }

    /// <summary>
    ///     webSocket超时时间 秒
    /// </summary>
    public static int Timeout { get; set; } = 15;

    /// <summary>
    ///     是否开启动态压帧
    /// </summary>
    public static bool FrameDynamic { get; set; } = true;

    /// <summary>
    ///     视频文件存放路径
    /// </summary>
    public static string VideoDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathVideo);

    /// <summary>
    ///     时间补偿值 毫秒
    /// </summary>
    public static long TimeCompensation { get; set; }

    /// <summary>
    ///     站点编号
    /// </summary>
    public static string StationId { get; set; }

    /// <summary>
    ///     站点大类
    /// </summary>
    public static StationType StationType { get; set; }

    /// <summary>
    ///     GPS缓存
    /// </summary>
    public static SDataGps BufGpsData =>
        // lock (_lockGps)
        new()
        {
            Longitude = _gpsData.Longitude,
            Latitude = _gpsData.Latitude,
            Altitude = _gpsData.Altitude,
            Heading = _gpsData.Heading,
            Speed = _gpsData.Speed,
            Satellites = _gpsData.Satellites
        };

    /// <summary>
    ///     罗盘缓存
    /// </summary>
    public static SDataCompass BufCompassData =>
        // lock (_lockCompass)
        new()
        {
            Heading = _compassData.Heading,
            Rolling = _compassData.Rolling,
            Pitch = _compassData.Pitch
        };

    /// <summary>
    ///     云端token用户名
    /// </summary>
    public static string CloudTokenUser { get; set; } = "dc_admin";

    /// <summary>
    ///     云端token密码
    /// </summary>
    public static string CloudTokenPassword { get; set; } = "456789";

    /// <summary>
    ///     更新GPS信息
    /// </summary>
    /// <param name="gps">GPS数据</param>
    public static void UpdateGps(SDataGps gps)
    {
        if (gps == null) return;
        // lock (_lockGps)
        {
            _gpsData.Longitude = gps.Longitude;
            _gpsData.Latitude = gps.Latitude;
            _gpsData.Altitude = gps.Altitude;
            _gpsData.Heading = gps.Heading;
            _gpsData.Speed = gps.Speed;
            _gpsData.Satellites = gps.Satellites;
        }
    }

    /// <summary>
    ///     更新GPS信息
    /// </summary>
    /// <param name="longitude">经度</param>
    /// <param name="latitude">纬度</param>
    public static void UpdateGps(double longitude, double latitude)
    {
        // lock (_lockGps)
        {
            _gpsData.Longitude = longitude;
            _gpsData.Latitude = latitude;
        }
    }

    /// <summary>
    ///     更新罗盘信息
    /// </summary>
    /// <param name="compass">罗盘数据</param>
    public static void UpdateCompass(SDataCompass compass)
    {
        if (compass == null) return;
        // lock (_lockCompass)
        {
            _compassData.Heading = compass.Heading;
            _compassData.Rolling = compass.Rolling;
            _compassData.Pitch = compass.Pitch;
        }
    }

    /// <summary>
    ///     获取空闲的磁盘空间
    /// </summary>
    public static (double freeHdd, string unit, bool canSave) GetFreeHdd()
    {
        var free = HddTotal - HddUsed; //GB 预留0.1G的空闲
        var unit = "GB";
        if (free <= 0) return new ValueTuple<double, string, bool>(0, "GB", false);
        var canSave = true;
        if (free < 1 && free * 1024 > 1)
        {
            free *= 1024;
            unit = "MB";
        }
        else if (free * 1024 < 1 && free * 1024 * 1024 > 1)
        {
            free *= 1024 * 1024;
            unit = "KB";
            canSave = false;
        }
        else if (free * 1024 * 1024 < 1)
        {
            free *= 1024 * 1024 * 1024;
            unit = "B";
            canSave = false;
        }

        free = Math.Round(free, 2);
        return new ValueTuple<double, string, bool>(free, unit, canSave);
    }

    #region 服务资源监控

    /// <summary>
    ///     CPU使用率 %
    /// </summary>
    public static double CpuUseage { get; set; }

    /// <summary>
    ///     内存使用量 GB
    /// </summary>
    public static double MemoryUsed { get; set; }

    /// <summary>
    ///     内存总量 GB
    /// </summary>
    public static double MemoryTotal { get; set; }

    /// <summary>
    ///     硬盘总量 GB
    /// </summary>
    public static double HddTotal { get; set; }

    /// <summary>
    ///     硬盘使用量 GB
    /// </summary>
    public static double HddUsed { get; set; }

    #endregion

    #region 私有变量

    // private static readonly object _lockGps = new();
    // private static readonly object _lockCompass = new();
    private static readonly SDataGps _gpsData = new();
    private static readonly SDataCompass _compassData = new();

    #endregion
}