using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF;

/// <summary>
///     离散扫描模板类
/// </summary>
internal class DiscreteFrequencyTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20d, 3000d)]
    [DefaultValue(102.6d)]
    [Description("中心频率，默认单位MHz")]
    public double Frequency { get; set; } = 102.6d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [DefaultValue(150d)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    public double FilterBandwidth { get; set; } = 150.0d;
}

/// <summary>
///     中频多路分析子通道类模板
/// </summary>
internal class IfMultiChannelTemplate
{
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("设置中频多路子通道中心频率，单位MHz")]
    [ValueRange(20.0d, 3000.0d)]
    [DefaultValue(102.6d)]
    public double Frequency { get; set; } = 102.6d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [ReadOnly(true)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [DefaultValue(150d)]
    [Description("设置中频多路子通道的滤波带宽。")]
    public double FilterBandwidth { get; set; } = 150.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|LSB|USB|CW|PM",
        DisplayValues = "|AM|FM|LSB|USB|CW|PM")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.AudioSwitch)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    public bool AudioSwitch { get; set; } = true;

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.IfSwitch)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频输出")]
    [Description("设置是否输出子通道中频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(4)]
    public bool IfSwitch { get; set; } = false;
}

public class SegmentTemplate
{
    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(20d, 3000d)]
    [DefaultValue(88.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 88.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(20d, 3000d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("stepFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|50|25|12.5|6.25|3.125|1.5625",
        DisplayValues = "|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|1.5625kHz")]
    [DefaultValue(25d)]
    [ValueRange(1.5625d, 50.0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进，单位kHz")]
    public double StepFrequency { get; set; } = 25.0d;

    public static explicit operator SegmentTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new SegmentTemplate();
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
                if (dict.TryGetValue(name, out var value)) property.SetValue(template, value, null);
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