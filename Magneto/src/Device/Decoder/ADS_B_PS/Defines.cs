using System;
using System.Collections.Generic;

namespace Magneto.Device.ADS_B_PS;

[Serializable]
public class PingStationStatus
{
    /// <summary>
    ///     唯一的pingStation标识符
    /// </summary>
    public string PingStationGuid { get; set; }

    /// <summary>
    ///     主版本
    /// </summary>
    public int PingStationVersionMajor { get; set; }

    /// <summary>
    ///     次版本
    /// </summary>
    public int PingStationVersionMinor { get; set; }

    /// <summary>
    ///     修订版
    /// </summary>
    public int PingStationVersionBuild { get; set; }

    /// <summary>
    ///     时间数据包以pingStation ISO 8601格式收到
    /// </summary>
    public string TimeStamp { get; set; }

    /// <summary>
    ///     固定站的纬度以十进制度表示
    /// </summary>
    public double PingStationLatDd { get; set; }

    /// <summary>
    ///     固定站经度表示为十进制度数
    /// </summary>
    public double PingStationLonDd { get; set; }

    /// <summary>
    ///     GPS状态：0 = GPS不存在或正在运行，1 =未锁定，2 = 2D修正，3 = 3D修复，4 = DGPS修复
    /// </summary>
    public int GpsStatus { get; set; }

    /// <summary>
    ///     接收器的通信和健康状态：0 =正常运作，1 =过度的通信错误，2 =设备不传输，3-设备离线（自定义）
    /// </summary>
    public int ReceiverStatus { get; set; }
}

[Serializable]
public class Aircraft
{
    /// <summary>
    ///     ICAO of the aircraft 国际民航组织的飞机
    /// </summary>
    public string IcaoAddress { get; set; }

    /// <summary>
    ///     0 = 1090ES，1 = UAT
    /// </summary>
    public int TrafficSource { get; set; }

    /// <summary>
    ///     纬度以十进制度表示
    /// </summary>
    public double LatDd { get; set; }

    /// <summary>
    ///     经度以十进制度表示
    /// </summary>
    public double LonDd { get; set; }

    /// <summary>
    ///     几何高度或以毫米为单位的气压高度
    /// </summary>
    public double AltitudeMm { get; set; }

    /// <summary>
    ///     以度为单位的地面航向
    /// </summary>
    public int HeadingDe2 { get; set; }

    /// <summary>
    ///     以厘米/秒为单位的水平速度
    /// </summary>
    public ulong HorVelocityCms { get; set; }

    /// <summary>
    ///     以厘米/秒为单位的垂直速度正向上升
    /// </summary>
    public long VerVelocityCms { get; set; }

    /// <summary>
    ///     squawk代码
    /// </summary>
    public int Squawk { get; set; }

    /// <summary>
    ///     高度类型：0 = Pressure，1 = Geometric
    /// </summary>
    public int AltitudeType { get; set; }

    /// <summary>
    ///     呼号
    /// </summary>
    public string Callsign { get; set; }

    /// <summary>
    ///     发射器的分类类型：
    ///     0 =无航空器类型信息
    ///     1 =轻型（国际民航组织）小于15,500磅
    ///     2 =小型 - 15,500至75,000磅
    ///     3 =大型 - 75,000至300,000磅
    ///     4 =高涡大（例如B757）
    ///     5 =重型（ICAO） - > 300,000 lbs
    ///     6 =高度机动性> 5G加速度和高速度
    ///     7 =旋翼机
    ///     8 =（未分配）
    ///     9 =滑翔机/滑翔机
    ///     10 =比空气更轻
    ///     11 =跳伞者/潜水员
    ///     12 =超轻/滑翔机/滑翔伞13 =（未分配）
    ///     14 = 无人驾驶飞行器
    ///     15 =太空/超大气量车辆
    ///     16 =（未分配）
    ///     17 =地面车辆应急车辆
    ///     18 =地面车辆服务车辆
    ///     19 =障碍物（包括系留气球）
    ///     20 =群障碍物
    ///     21 =线障碍物
    ///     22-39 =（保留）
    /// </summary>
    public int EmitterType { get; set; }

    /// <summary>
    ///     自动递增数据包序列号
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    ///     唯一的pingStation标识符
    /// </summary>
    public string PingStationGuid { get; set; }

    /// <summary>
    ///     UTC时间标志
    /// </summary>
    public int UtcSync { get; set; }

    /// <summary>
    ///     时间数据包以pingStation ISO 8601格式接收：YYYY-MM-DDTHH：MM：SS：ffffffffZ
    /// </summary>
    public string TimeStamp { get; set; }

    public AircraftDetail Detail { get; set; }
}

[Serializable]
public class AircraftDetail
{
    /// <summary>
    ///     导航完整性类别（NIC）
    ///     0 = RC> = 37.04公里（20海里）未知的完整性
    ///     1 = RC小于37.04公里（20海里）RNP-10容纳半径
    ///     2 = RC小于14.816公里（8海里）RNP-4收容半径
    ///     3 = RC小于7.408公里（4海里）RNP-2容纳半径
    ///     4 = RC小于3.704 km（2 NM）RNP-1容纳半径
    ///     5 = RC小于1852 m（1 NM）RNP-0.5容积半径
    ///     6 = RC小于1111.2米（0.6海里）RNP-0.3收容半径
    ///     7 = RC小于370.4 m（0.2 NM）RNP-0.1容纳半径
    ///     8 = RC小于185.2米（0.1海里）RNP-0.05容积半径
    ///     9 = RC小于75m且VPL小于112m，例如SBAS，HPL，VPL
    ///     10 = RC小于25m且VPL小于37.5m，例如SBAS，HPL，VPL
    ///     11 = RC小于7.5m且VPL小于11m，例如GBAS，HPL，VPL
    ///     12 =（保留）（保留）
    ///     13 =（保留）（保留）
    ///     14 =（保留）（保留）
    ///     15 =（保留）（保留）
    /// </summary>
    public int NavIntegrity { get; set; }

    /// <summary>
    ///     导航精度类别（NACv）
    ///     0 =未知或> = 10 m / s未知> =每秒50英尺（15.24米）
    ///     1 = 小于10米/秒小于50英尺（15.24米）每秒
    ///     2 = 小于3米/秒小于15英尺（4.57米）每秒
    ///     3 = 小于1米/秒小于5英尺（1.52米）每秒
    ///     4 = 小于0.3米/秒小于1.5英尺（0.46米）每秒
    ///     5 =（保留）（保留）
    ///     6 =（保留）（保留）
    ///     7 =（保留）（保留）
    /// </summary>
    public int NavAccuracy { get; set; }

    /// <summary>
    ///     垂直速度源：0 =压力，1 =几何
    /// </summary>
    public int VerVelocitySrc { get; set; }

    /// <summary>
    ///     紧急状态
    ///     0 =没有紧急情况
    ///     1 =一般紧急情况
    ///     2 =救生员/医疗
    ///     3 =最低燃油
    ///     4 =没有通信
    ///     5 =非法干扰
    ///     6 =飞机坠毁
    /// </summary>
    public int EmergencyStatus { get; set; }

    /// <summary>
    ///     (1090ES特定字段)监视状态
    ///     0 =无条件
    ///     1 =永久警报
    ///     2 =临时警报
    ///     3 = SPI
    /// </summary>
    public int SurveilStatus { get; set; }

    /// <summary>
    ///     (1090ES特定字段)压力高度与gnss高度之差（mm）
    /// </summary>
    public long BaroaltDiffMm { get; set; }

    /// <summary>
    ///     系统完整性等级（SIL）
    /// </summary>
    public int SysIntegrityLevel { get; set; }

    /// <summary>
    ///     空中或地面：0 =机载亚音速条件，1 =机载超音速情况，3 =地面情况
    /// </summary>
    public int AirGroundState { get; set; }

    /// <summary>
    ///     从标题跟踪角度：0 =数据不可用，1 =真实的轨道角度，2 =磁性标题，3 =真正的标题
    /// </summary>
    public int SvHeadingType { get; set; }

    /// <summary>
    ///     垂直速率信息：0 =压力，1 =几何
    /// </summary>
    public int VerticalVelType { get; set; }

    /// <summary>
    ///     所报告的状态矢量对于预期用途具有足够的位置精度（NACp）
    ///     0 = EPU> = 18.52公里（10海里）
    ///     /// 1 = EPU小于18.52公里（10海里）
    ///     2 = EPU小于7.408公里（4海里）
    ///     3 = EPU小于3.704公里（2海里）
    ///     4 = EPU小于1852米（1NM）
    ///     5 = EPU小于926米（0.5海里）
    ///     6 = EPU小于555.6m（0.3NM）
    ///     7 = EPU小于185.2m（0.1NM）
    ///     8 = EPU小于92.6m（0.05NM）
    ///     9 = EPU小于30m且VEPU小于45
    ///     10 = EPU小于10m且VEPU小于15
    ///     11 = EPU小于3m，VEPU小于4m
    ///     12 =（保留）
    ///     13 =（保留）
    ///     14 =（保留）
    ///     15 =（保留）
    /// </summary>
    public int NavPostionAccuracy { get; set; }

    /// <summary>
    ///     正在传输的最不准确的速度分量
    ///     0 =未知或> = 10 m / s未知或> = 50英尺（15.24米）每秒
    ///     1 = 小于10米/秒小于50英尺（15.24米）每秒
    ///     2 = 小于3米/秒小于15英尺（4.57米）每秒
    ///     3 = 小于1米/秒小于5英尺（1.52米）每秒
    ///     4 = 小于0.3米/秒小于1.5英尺（0.46米）每秒
    ///     5 =（保留）（保留）
    ///     6 =（保留）（保留）
    ///     7 =（保留）（保留）
    /// </summary>
    public int NavVelocityAccuracy { get; set; }

    /// <summary>
    ///     检查气压计（NICbaro）：0 =气压高度未被交叉检查，1 =气压高度已被交叉检查
    /// </summary>
    public int NavIntegrityBaro { get; set; }

    /// <summary>
    ///     飞机安装了TCAS（ACAS）计算机，并且该计算机已启动并以可产生解决方案咨询（RA）警报的模式运行
    /// </summary>
    public int TcasAcasOperating { get; set; }

    /// <summary>
    ///     TCAS II或ACAS计算机目前正在发布解决方案咨询
    /// </summary>
    public int TcasAcasAdvisory { get; set; }

    /// <summary>
    ///     Ident开关被激活
    /// </summary>
    public int IdentSwActive { get; set; }

    /// <summary>
    ///     真北或磁北：0 =真北，1 =磁北
    /// </summary>
    public int MagHeading { get; set; }

    /// <summary>
    ///     代表地面站是UTC耦合的：0 =地面站不是UTC耦合的，1 =地面站与UTC耦合
    /// </summary>
    //[JsonProperty(PropertyName = "utcCoupledCondition")]
    public int UtcCoupledCondition { get; set; }
}

internal class DataStatus
{
    public PingStationStatus Status { get; set; }
}

/// <summary>
///     设备航空态势数据类，用于Json解析
/// </summary>
internal class DataAircraft
{
    public List<Aircraft> Aircraft { get; set; }
}