using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.G213;

public class RftxBandsTemplate
{
    [Parameter]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("设置管制信道所属通道编号")]
    [StandardValues(IsSelectOnly = false,
        DisplayValues = "|通道1|通道2|通道3|通道4|通道5|通道6|通道7|通道8",
        StandardValues = "|0|1|2|3|4|5|6|7")]
    [ValueRange(0, 5)]
    [DefaultValue(0)]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Dropdown)]
    public int PhysicalChannelNumber { get; set; }

    [Parameter]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("设置管制信道频段起始频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(88.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; }

    [Parameter]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("设置管制信道频段结束频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; }

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("频率间隔")]
    [Description("表示扫频压制模式下的频率间隔，单位：kHz")]
    [ValueRange(10d, 10000d)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(120d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 100d;

    [Parameter]
    [Name(ParameterNames.ChannelSubBands)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("通道子频段信息")]
    [Description(
        "通道子频段信息，起始频率和结束频率单位为MHz，格式：名称1;起始频率1;结束频率1|名称2;起始频率2;结束频率2，eg：中国电信2G/4G(CDMA/LTEFDD);870;885|中国移动4G(LTEFDD);937;949")]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Input)]
    public string ChannelSubBands { get; set; }

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
            if (dict.TryGetValue(name, out var value1))
                try
                {
                    var value = Utils.GetRealValue(property.PropertyType, value1);
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

public class RftxSegmentsTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。")]
    [ValueRange(1, 9)]
    [DefaultValue(1)]
    [Style(DisplayStyle.Slider)]
    public int PhysicalChannelNumber { get; set; } = 1;

    [PropertyOrder(1)]
    [Name(ParameterNames.LogicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
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
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
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
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("压制模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3|4|5|6|7",
        DisplayValues = "|定频|多音|窄带跳频|梳状谱|窄带噪声调频|宽带噪声|线性调频|协议压制"
    )]
    [Description("表示频率压制模式，分为：0-定频，1-多音， 2-窄带跳频， 3-梳状谱， 4-窄带噪声调频， 5-宽带噪声， 6-线性调频， 7-协议压制")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int RftxFrequencyMode { get; set; } = 0;

    [PropertyOrder(4)]
    [Name(ParameterNames.Modulation)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
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
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制带宽")]
    [Description("设置管制信道信号带宽，单位：kHz")]
    [ValueRange(1, 200000, 0.01)]
    [DefaultValue(1000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Input)]
    public double Bandwidth { get; set; } = 1000d;

    [PropertyOrder(6)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
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
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("中心频率")]
    [Description("表示定频压制模式下的发射信号中心频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [DefaultValue(100d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 100d;

    [PropertyOrder(8)]
    [Name(ParameterNames.Frequencies)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("跳频频点")]
    [Description("表示跳频压制模式下的离散频表，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double[] Frequencies { get; set; }

    [PropertyOrder(9)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(10)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(11)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫频步进")]
    [Description("表示扫频压制模式下的步进，单位：kHz")]
    [ValueRange(1d, 100d, 3)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(25d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 25d;

    [PropertyOrder(12)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("数字幅度衰减量")]
    [ValueRange(0, 64)]
    [DefaultValue(0)]
    [Browsable(true)]
    [Description("设置数字幅度衰减量，单位dB")]
    public int Attenuation { get; set; }

    [PropertyOrder(13)]
    [Name("cycle")]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫描周期")]
    [ValueRange(1, 1000000)]
    [DefaultValue(4)]
    [Browsable(false)]
    [Description("仅线性调频有效，单位 μs")]
    public int Cycle { get; set; } = 1000;

    [PropertyOrder(14)]
    [Name("combBandwidth")]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("梳状谱齿宽度")]
    [ValueRange(1, 10000)]
    [DefaultValue(1000)]
    [Browsable(false)]
    [Unit(UnitNames.KHz)]
    [Description("仅梳状谱有效，单位为kHz，分辨率：10Hz 0 - 为多音干扰")]
    public int CombBandwidth { get; set; } = 1000;

    [PropertyOrder(15)]
    [Name("protocolSuppressType")]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("协议压制类型")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3|4|5",
        DisplayValues = "|类型1|类型2|类型3|类型4|类型5|类型6")]
    [DefaultValue(0)]
    [Description("仅对协议压制有效")]
    public int ProtocolSuppressType { get; set; }

    [PropertyOrder(16)]
    [Name("hoppingSpeed")]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("跳频速率")]
    [ValueRange(20, 1000)]
    [DefaultValue(20)]
    [Browsable(false)]
    [Description("仅跳频有效")]
    public int HoppingSpeed { get; set; } = 20;

    [PropertyOrder(17)]
    [Name("subBandIndex")]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("子频段索引")]
    [DefaultValue(-1)]
    [Browsable(false)]
    [Description("子频段索引，管制频段中通道频段列表索引，-1为所有子频段")]
    public int SubBandIndex { get; set; } = -1;

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