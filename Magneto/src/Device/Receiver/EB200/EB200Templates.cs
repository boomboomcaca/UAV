using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB200;

/// <summary>
///     离散扫描模板类
/// </summary>
internal class MscanTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [DefaultValue(101.7d)]
    [Description("中心频率 单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 101.7d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [Description("解调带宽设置。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|500|200|150|120|100|50|30|25|15|9|6|2.4|1.5",
        DisplayValues = "|1MHz|500kHz|200kHz|150kHz|120kHz|100kHz|50kHz|30kHz|25kHz|15kHz|9kHz|6kHz|2.4kHz|1.5kHz")]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 120.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|IQ|CW|USB|LSB",
        DisplayValues = "|AM|FM|IQ|CW|USB|LSB")]
    [DefaultValue(Modulation.Fm)]
    [Description("信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.MeasureThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量门限")]
    [Description("获取或设置离散扫描进行占用度测量的门限值")]
    [ValueRange(-40, 120)]
    [DefaultValue(20)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Slider)]
    public int MeasureThreshold { get; set; } = 0;

    public static explicit operator MscanTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new MscanTemplate();
        var type = template.GetType();
        try
        {
            foreach (var property in type.GetProperties())
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.ContainsKey(name))
                {
                    object objValue = null;
                    if (property.PropertyType.IsEnum)
                        objValue = Utils.ConvertStringToEnum(dict[name].ToString(), property.PropertyType);
                    else if (property.PropertyType == typeof(Guid))
                        objValue = Guid.Parse(dict[name].ToString() ?? string.Empty);
                    else if (property.PropertyType.IsValueType)
                        objValue = Convert.ChangeType(dict[name], property.PropertyType);
                    else
                        objValue = dict[name]; //Convert.ChangeType(value, prop.PropertyType);
                    property.SetValue(template, objValue, null);
                }
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
            foreach (var property in type.GetProperties())
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
    [Parameter(AbilitySupport = FeatureType.SCAN
                                | FeatureType.NSIC
                                | FeatureType.ESE
                                | FeatureType.EMDC
                                | FeatureType.EMDA)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 8000.0d)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.SCAN
                                | FeatureType.NSIC
                                | FeatureType.ESE
                                | FeatureType.EMDC
                                | FeatureType.EMDA)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 8000.0d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.SCAN
                                | FeatureType.NSIC
                                | FeatureType.ESE
                                | FeatureType.EMDC
                                | FeatureType.EMDA)]
    [Name("stepFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|150|125|75|60|50|15|7.5|4.5|4|3|2|1.5|1.2",
        DisplayValues = "|150kHz|125kHz|75kHz|60kHz|50kHz|15kHz|7.5kHz|4.5kHz|4kHz|3kHz|2kHz|1.5kHz|1.2kHz")]
    [ValueRange(0.1d, 500.0d)]
    [DefaultValue(25.0d)]
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