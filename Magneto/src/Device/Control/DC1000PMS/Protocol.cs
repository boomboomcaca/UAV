using System;

namespace Magneto.Device.DC1000PMS;

internal class PowerSupplyInfo
{
    /// <summary>
    ///     是否为交流电
    /// </summary>
    public bool Ac;

    /// <summary>
    ///     电流（单位：安培），float.MaxValue为无效值
    /// </summary>
    public float Current;

    /// <summary>
    ///     电源名称，默认为空
    /// </summary>
    public string Name;

    /// <summary>
    ///     电压（单位：伏特），float.MaxValue为无效值
    /// </summary>
    public float Voltage;

    /// <summary>
    ///     默认构造函数
    /// </summary>
    public PowerSupplyInfo()
    {
        Name = string.Empty;
        Ac = true;
        Voltage = float.MaxValue;
        Current = float.MaxValue;
    }
}

[Flags]
internal enum SecurityAlert
{
    /// <summary>
    ///     无报警
    /// </summary>
    None = 0x0,

    /// <summary>
    ///     烟雾报警
    /// </summary>
    Smoke = 0x1,

    /// <summary>
    ///     火灾报警（未使用）
    /// </summary>
    Fire = 0x2,

    /// <summary>
    ///     浸水报警
    /// </summary>
    Flooding = 0x4,

    /// <summary>
    ///     红外（活动物）报警
    /// </summary>
    Infrared = 0x8,

    /// <summary>
    ///     门磁报警
    /// </summary>
    GateAccess = 0x10,

    /// <summary>
    ///     高温报警
    /// </summary>
    Overtemperature = 0x20,

    /// <summary>
    ///     电流过载
    /// </summary>
    CurrentOverload = 0x40,

    /// <summary>
    ///     电压过载
    /// </summary>
    VoltageOverload = 0x80
}

internal class SwitchStatusInfo
{
    /// <summary>
    ///     开关显示名称
    /// </summary>
    public string DisplayName;

    /// <summary>
    ///     开关名称
    /// </summary>
    public string Name;

    /// <summary>
    ///     状态是否为已开启，true为开启，false为关闭
    /// </summary>
    public bool On;

    /// <summary>
    ///     默认构造函数
    /// </summary>
    public SwitchStatusInfo()
    {
        Name = "Unknown";
        DisplayName = "未知";
        On = false;
    }

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="name">开关名称</param>
    /// <param name="displayName">开关显示名称（中文）</param>
    /// <param name="on">开关状态是否为已开启</param>
    public SwitchStatusInfo(string name, string displayName, bool on = false)
    {
        Name = name;
        DisplayName = displayName;
        On = on;
    }
}

/// <summary>
///     电压电流信息
/// </summary>
internal struct VoltageCurrentInfo
{
    /// <summary>
    ///     电压 (默认值为-1，表示无效值)
    /// </summary>
    public float Voltage { get; set; }

    /// <summary>
    ///     电流（默认值为-1，表示无效值）
    /// </summary>
    public float Current { get; set; }
}

/// <summary>
///     安防报警信息
/// </summary>
[Flags]
internal enum Security
{
    /// <summary>
    ///     无报警信息
    /// </summary>
    None = 0x00,

    /// <summary>
    ///     烟雾报警
    /// </summary>
    Smoke = 0x01,

    /// <summary>
    ///     人体红外报警
    /// </summary>
    Inferade = 0x02,

    /// <summary>
    ///     浸水报警
    /// </summary>
    Flooding = 0x04,

    /// <summary>
    ///     门禁报警
    /// </summary>
    AccessControl = 0x08
}

[Flags]
internal enum CmdFlag
{
    /// <summary>
    ///     交流通道开关数据
    /// </summary>
    AcChannelStatus = 16843009,

    /// <summary>
    ///     直流通道开关数据
    /// </summary>
    DcChannelStatus = 16908545,

    /// <summary>
    ///     交流通道1电压电流数据
    /// </summary>
    AcChannel1Power = 20971777,

    /// <summary>
    ///     交流通道2电压电流数据
    /// </summary>
    AcChannel2Power = 37748993,

    /// <summary>
    ///     交流通道3电压电流数据
    /// </summary>
    AcChannel3Power = 54526209,

    /// <summary>
    ///     交流通道4电压电流数据
    /// </summary>
    AcChannel4Power = 71303425,

    /// <summary>
    ///     交流通道5电压电流数据
    /// </summary>
    AcChannel5Power = 88080641,

    /// <summary>
    ///     交流通道6电压电流数据
    /// </summary>
    AcChannel6Power = 104857857,

    /// <summary>
    ///     交流通道7电压电流数据
    /// </summary>
    AcChannel7Power = 121635073,

    /// <summary>
    ///     交流通道8电压电流数据
    /// </summary>
    AcChannel8Power = 138412289,

    /// <summary>
    ///     直流通道1电压电流数据
    /// </summary>
    DcChannel1Power = 22020353,

    /// <summary>
    ///     直流通道2电压电流数据
    /// </summary>
    DcChannel2Power = 38797569,

    /// <summary>
    ///     直流通道3电压电流数据
    /// </summary>
    DcChannel3Power = 55574785,

    /// <summary>
    ///     直流通道4电压电流数据
    /// </summary>
    DcChannel4Power = 72352001,

    /// <summary>
    ///     直流通道5电压电流数据
    /// </summary>
    DcChannel5Power = 89129217,

    /// <summary>
    ///     直流通道6电压电流数据
    /// </summary>
    DcChannel6Power = 105906433,

    /// <summary>
    ///     WiFi开关数据
    /// </summary>
    WiFiStatus = 17105153,

    /// <summary>
    ///     空调开关数据
    /// </summary>
    AirconditionStatus = 17039617,

    /// <summary>
    ///     温湿度信息
    /// </summary>
    TemperatureHumidyInfo = 50594049,

    /// <summary>
    ///     安防报警信息
    /// </summary>
    SecurityAlarmInfo = 19923201
}