using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoRadioSuppressor;

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
        DisplayValues = "|定频|跳频|扫频|梳状谱"
    )]
    [Description("表示频率压制模式，分为：0 - 定频，1 - 跳频， 2 - 扫频， 3 - 梳状谱")]
    [DefaultValue(0)]
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
    public Modulation Modulation { get; set; } = Modulation.Cw;

    // [PropertyOrder(5)]
    // [Name(ParameterNames.ModulationSource)]
    // [Parameter(AbilitySupport = FeatureType.UAVS
    //                             | FeatureType.SATELS
    //                             | FeatureType.PCOMS
    //                             | FeatureType.FBANDS)]
    // [Category(PropertyCategoryNames.Misc)]
    // [DisplayName("调制源")]
    // [StandardValues(IsSelectOnly = true,
    //             StandardValues = "|0|1|2",
    //             DisplayValues = "|1kHz单音|网络语音|噪声"
    //             )]
    // [Description("表示在使用FM或AM调制方式进行压制时，调制信号的来源，分为：0 - 1kHz单音，1 - 网络语音， 2 - 噪声")]
    // [DefaultValue(0)]
    // public int ModulationSource { get; set; } = 0;
    [PropertyOrder(5)]
    [Name(ParameterNames.Bandwidth)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制带宽")]
    [Description("表示在使用FM或AM调制方式进行压制时，调制信号的来源，分为：0 - 1kHz单音，1 - 网络语音， 2 - 噪声")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|5000|10000|20000",
        DisplayValues = "|2MHz|5MHz|10MHz|20MHz"
    )]
    [DefaultValue(2000d)]
    [Unit(UnitNames.KHz)]
    public double Bandwidth { get; set; } = 2000d;

    // [PropertyOrder(7)]
    // [Name(ParameterNames.Baudrate)]
    // [Parameter(AbilitySupport = FeatureType.UAVS
    //                             | FeatureType.SATELS
    //                             | FeatureType.PCOMS
    //                             | FeatureType.FBANDS)]
    // [Category(PropertyCategoryNames.Misc)]
    // [DisplayName("调制速率")]
    // [Description("表示调制信号的符号率，码元率等，单位：kpbs")]
    // [StandardValues(IsSelectOnly = true,
    //             StandardValues = "|9.6",
    //             DisplayValues = "|9.6"
    //             )]
    // [DefaultValue(9.6d)]
    // [Unit(UnitNames.Kbps)]
    // public double Baudrate { get; set; } = 9.6;
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
    [DefaultValue(100f)]
    [Unit(UnitNames.Us)]
    public float HoldTime { get; set; } = 100f;

    [PropertyOrder(7)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("中心频率")]
    [Description("表示定频压制模式下的发射信号中心频率，单位：MHz")]
    [ValueRange(20d, 26500d)]
    [DefaultValue(100d)]
    [Unit(UnitNames.MHz)]
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
    [ValueRange(20d, 26500d)]
    [Unit(UnitNames.MHz)]
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
    [ValueRange(20d, 26500d)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
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
    [ValueRange(20d, 26500d)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
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
    [ValueRange(1d, 100d)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(25d)]
    public double StepFrequency { get; set; } = 25d;

    public static explicit operator RftxSegmentsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxSegmentsTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (!dict.ContainsKey(name)) continue;
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine(e.ToString());
#endif
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
    public int PhysicalChannelNumber { get; set; } = 1;

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(4)]
    [Name(ParameterNames.LogicalChannelCount)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道数")]
    [Description("配置压制机对应物理通道可用的逻辑通道数量")]
    [ValueRange(1, 1)]
    [DefaultValue(1)]
    public int LogicalChannelCount { get; set; } = 1;

    public static explicit operator RftxBandsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxBandsTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (!dict.ContainsKey(name)) continue;
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
        }
        catch
        {
            // 容错代码
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