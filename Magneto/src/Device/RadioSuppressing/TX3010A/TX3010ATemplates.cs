using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.TX3010A;

public class RftxSegmentsTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。")]
    [ValueRange(1, 9)]
    [DefaultValue(1)]
    [Style(DisplayStyle.Slider)]
    public int PhysicalChannelNumber { get; set; } = 1;

    [PropertyOrder(1)]
    [Name(ParameterNames.LogicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道号")]
    [Description("表示当前管制使用的逻辑通道编号")]
    [ValueRange(1, 1)]
    [DefaultValue(1)]
    [ReadOnly(true)]
    [Style(DisplayStyle.Slider)]
    public int LogicalChannelNumber { get; set; } = 1;

    [PropertyOrder(2)]
    [Name(ParameterNames.RftxSwitch)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关"
    )]
    [Description("表示当前压制使能")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool RftxSwitch { get; set; } = false;

    [PropertyOrder(3)]
    [Name(ParameterNames.RftxFrequencyMode)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("压制模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3",
        DisplayValues = "|定频|多音|扫频|梳状谱"
    )]
    [Description("表示频率压制模式，分为：0 - 定频，1 - 多音， 2 - 扫频， 3 - 梳状谱")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int RftxFrequencyMode { get; set; } = 0;

    [PropertyOrder(4)]
    [Name(ParameterNames.Modulation)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|cw|qpsk",
        DisplayValues = "|CW|QPSK"
    )]
    [Description("表示当前压制使用的调制方式，如CW, QPSK")]
    [DefaultValue(Modulation.Cw)]
    [Style(DisplayStyle.Radio)]
    public Modulation Modulation { get; set; } = Modulation.Cw;

    [PropertyOrder(5)]
    [Name(ParameterNames.Bandwidth)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制带宽")]
    [Description("设置管制信道信号带宽，单位：kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|5000|10000|20000",
        DisplayValues = "|2MHz|5MHz|10MHz|20MHz"
    )]
    [DefaultValue(2000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Radio)]
    public double Bandwidth { get; set; } = 2000d;

    [PropertyOrder(6)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("驻留时间")]
    [Description("表示跳频或扫描模式下，待压制信号的的发射时长，单位：μs")]
    [ValueRange(1f, 1000000f)]
    [DefaultValue(10f)]
    [Unit(UnitNames.Us)]
    [Style(DisplayStyle.Slider)]
    public float HoldTime { get; set; } = 10f;

    [PropertyOrder(7)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("中心频率")]
    [Description("表示定频压制模式下的发射信号中心频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [DefaultValue(100d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 100d;

    [PropertyOrder(7)]
    [Name(ParameterNames.Frequencies)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("跳频频点")]
    [Description("表示跳频压制模式下的离散频表，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double[] Frequencies { get; set; }

    [PropertyOrder(7)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(8)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(9)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫频步进")]
    [Description("表示扫频压制模式下的步进，单位：kHz")]
    [ValueRange(1d, 100d, 3)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(25d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 25d;

    public static explicit operator RftxSegmentsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxSegmentsTemplate();
        var type = template.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            if (!dict.ContainsKey(name)) continue;
            try
            {
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
            catch
            {
            }
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                    continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}

public class RftxBandsTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。")]
    [ValueRange(1, 9)]
    [DefaultValue(1)]
    [Style(DisplayStyle.Slider)]
    public int PhysicalChannelNumber { get; set; } = 1;

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("梳状谱频率间隔")]
    [Description("表示梳状谱模式下的频率间隔，单位：kHz")]
    [ValueRange(10d, 10000d)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(120d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 120d;

    [PropertyOrder(5)]
    [Name(ParameterNames.LogicalChannelCount)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道数")]
    [Description("配置压制机对应物理通道可用的逻辑通道数量")]
    [ValueRange(1, 1)]
    [DefaultValue(1)]
    [ReadOnly(true)]
    [Style(DisplayStyle.Slider)]
    public int LogicalChannelCount { get; set; } = 1;

    [Parameter]
    [Name(ParameterNames.ChannelSubBands)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("通道子频段信息")]
    [Description(
        "通道子频段信息，为空使用整个通道信息，格式：名称1;起始频率1(MHz);结束频率1(MHz)|名称2;起始频率2(MHz);结束频率2(MHz)，eg：中国电信2G/4G(CDMA/LTEFDD);870;885|中国移动4G(LTEFDD);937;949")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public string ChannelSubBands { get; set; }

    [Parameter]
    [Name("maxSignalCount")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("最大信号数量")]
    [Description("当前通道可配信号的最大数量")]
    [PropertyOrder(7)]
    [ValueRange(1, 32)]
    [DefaultValue(8)]
    [Style(DisplayStyle.Input)]
    public int MaxSignalCount { get; set; } = 8;

    public static explicit operator RftxBandsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxBandsTemplate();
        var type = template.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            if (!dict.ContainsKey(name)) continue;
            try
            {
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
            catch
            {
            }
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                    continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}

/// <summary>
///     这个与王凡沟通过，
///     如果只启用两路，则必定为启用通道1，2；不会出现启用1，3；2，3的情况
///     如果只启用一路，则必定为启用通道1，不会出现启用2或启用3的情况
/// </summary>
public class DeviceConfigTemplate
{
    [Parameter(IsInstallation = true)]
    [Name("index")]
    [DisplayName("子系统编号")]
    [Description("设置当前设备属于哪个子系统，范围1-3")]
    [Category(PropertyCategoryNames.Installation)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3",
        DisplayValues = "|子系统1|子系统2|子系统3"
    )]
    [DefaultValue(1)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Radio)]
    public int Index { get; set; } = 1;

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [DisplayName("设备地址")]
    [Description("设置连接的功放控制板的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("192.168.30.50")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; } = "192.168.30.50";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [DisplayName("设备端口")]
    [Description("设置连接到功放控制板的端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(8000)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 8000;

    [Parameter(IsInstallation = true)]
    [Name("deviceType")]
    [DisplayName("设备类型")]
    [Description("设置当前设备的类型")]
    [Category(PropertyCategoryNames.Installation)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1",
        DisplayValues = "|控制板|信号源"
    )]
    [DefaultValue(0)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Radio)]
    public int DeviceType { get; set; } = 0;

    [Parameter(IsInstallation = true)]
    [Name("channelNumber")]
    [DisplayName("物理通道号")]
    [Description("设置当前通道在整个系统的通道序号（仅对信号源有效，不能重复）")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1, 9)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3|4|5|6|7|8|9",
        DisplayValues = "|通道1|通道2|通道3|通道4|通道5|通道6|通道7|通道8|通道9"
    )]
    [DefaultValue(1)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Dropdown)]
    public int ChannelNumber { get; set; } = 1;

    [Parameter(IsInstallation = true)]
    [Name("deviceChannelNumber")]
    [DisplayName("设备真实通道号")]
    [Description("当前通道的在设备中的真实通道号（仅对信号源有效，不能重复）")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1, 3)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3",
        DisplayValues = "|PA1|PA2|PA3"
    )]
    [DefaultValue(1)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Radio)]
    public int DeviceChannelNumber { get; set; } = 1;

    public static explicit operator DeviceConfigTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new DeviceConfigTemplate();
        var type = template.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            if (!dict.ContainsKey(name)) continue;
            try
            {
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
            catch
            {
            }
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute) continue;
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            try
            {
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
            catch
            {
                // 容错代码
            }
        }

        return dic;
    }
}