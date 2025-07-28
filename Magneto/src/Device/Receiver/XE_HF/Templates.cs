using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_HF;

/// <summary>
///     离散扫描模板类
/// </summary>
public class DiscreteFrequencyTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(10.6d)]
    [Description("中心频率，默认单位MHz")]
    public double Frequency { get; set; } = 10.6d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|300|100|50|25|20|12.5|8|6.25|4|3|2|1|0.6|0.3|0.1",
        DisplayValues = "|300kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|8kHz|6.25kHz|4kHz|3kHz|2kHz|1kHz|600Hz|300Hz|100Hz")]
    [DefaultValue(25d)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    public double FilterBandwidth { get; set; } = 25.0d;

    public static explicit operator DiscreteFrequencyTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new DiscreteFrequencyTemplate();
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

/// <summary>
///     中频多路分析子通道类模板
/// </summary>
public class IfMultiChannelTemplate
{
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("设置中频多路子通道中心频率，单位MHz")]
    [ValueRange(1.0d, 30.0d)]
    [DefaultValue(10.6d)]
    public double Frequency { get; set; } = 10.6d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [ReadOnly(true)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|300|100|50|25|20|12.5|8|6.25|4|3|2|1|0.6|0.3|0.1",
        DisplayValues = "|300kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|8kHz|6.25kHz|4kHz|3kHz|2kHz|1kHz|600Hz|300Hz|100Hz")]
    [DefaultValue(25d)]
    [Description("设置中频多路子通道的滤波带宽。")]
    public double FilterBandwidth { get; set; } = 25.0d;

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

    public static explicit operator IfMultiChannelTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new IfMultiChannelTemplate();
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

public class SegmentTemplate
{
    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(8.8d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 8.8d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(10.8d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 10.8d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("stepFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|3.125|1.5625|0.78125|0.390625",
        DisplayValues = "|3.125kHz|1.5625kHz|781.25Hz|390.625Hz")]
    [DefaultValue(1.5625d)]
    [ValueRange(3.125d, 390.625d)]
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