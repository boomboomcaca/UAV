using System;

namespace Magneto.Device.DC1020PMS;

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