using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EM550;

/// <summary>
///     离散扫描模板类
/// </summary>
internal class MScanTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [DefaultValue(88.7d)]
    [Unit(UnitNames.MHz)]
    [Description("单频监测时的中心频率,单位MHz")]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 88.7d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10000|5000|2000|1500|1250|1000|800|500|300|250|150|120|50|30|15|12|9|6|2.4|1.5|0.6|0.3|0.15",
        DisplayValues =
            "|10MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|2.4kHz|1.5kHz|600Hz|300Hz|150Hz"
    )]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 150d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.MScan)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB",
        DisplayValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB")]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.MeasureThreshold)]
    [DisplayName("测量门限")]
    [Description("获取或设置离散扫描进行占用度测量的门限值")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [ValueRange(-40, 120)]
    [DefaultValue(20)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Slider)]
    public int MeasureThreshold { get; set; }

    public static explicit operator MScanTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new MScanTemplate();
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
    [Parameter(AbilitySupport = FeatureType.SCAN
                                | FeatureType.NSIC
                                | FeatureType.ESE
                                | FeatureType.EMDC
                                | FeatureType.EMDA)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 3600.0d)]
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
    [ValueRange(20.0d, 3600.0d)]
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
        StandardValues = "|100|50|25|12.5|6.25|3.125|2.5|1.25",
        DisplayValues = "|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|2.5kHz|1.25kHz"
    )]
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