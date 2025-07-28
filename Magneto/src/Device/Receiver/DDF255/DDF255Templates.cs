using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

/// <summary>
///     中频多路模板类
/// </summary>
public class IfmcaTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("设置中频多路分析功能子通道中心频率，单位：MHz")]
    [ValueRange(20.0d, 26500.0d, 6)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 101.7d;

    [PropertyOrder(1)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("设置中频子通道滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|800|500|250|150|120|75|50|30|15|12|9|6|4.8|4",
        DisplayValues = "|1MHz|800kHz|500kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz")]
    [DefaultValue(150.0d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 150d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("设置信号音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE")]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [PropertyOrder(3)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，单位dBuV。")]
    [DefaultValue(-20f)]
    [ValueRange(-30, 130)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public float SquelchThreshold { get; set; } = -20f;

    [PropertyOrder(4)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关"
    )]
    [Description("设置是否打开静噪门限。")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool SquelchSwitch { get; set; }

    [PropertyOrder(5)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [Description("IQ数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IqSwitch { get; set; }

    [PropertyOrder(6)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch { get; set; } = true;

    [PropertyOrder(7)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(false)]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch { get; set; } = false;

    [Name(ParameterNames.LevelSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [Description("设置当前子通道是否输出电平")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch { get; set; } = true;

    [PropertyOrder(9)]
    [Name(ParameterNames.IfSwitch)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频输出")]
    [Description("设置是否输出子通道中频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool IfSwitch { get; set; } = true;

    public static explicit operator IfmcaTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new IfmcaTemplate();
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

/// <summary>
///     离散扫描模板类
/// </summary>
public class MScanTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位 MHz")]
    [ValueRange(20, 26500, 6)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(89.7d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 89.7d;

    [PropertyOrder(9)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("指定中频带宽、滤波带宽、解调带宽。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|80000|40000|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|15|12|9|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1",
        DisplayValues =
            "|80MHz|40MHz|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz")]
    [StandardValues(RegexPattern = @"\(20\)", IsSelectOnly = true,
        StandardValues =
            "|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|15|12|9|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1",
        DisplayValues =
            "|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz")]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 150d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|A0|IQ|A1|CW|LSB|USB",
        DisplayValues = "|FM|AM|PULSE|A0|IQ|A1|CW|LSB|USB"
    )]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [Parameter]
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
    [ValueRange(20.0d, 26500.0d)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(87.0d)]
    [Unit("MHz")]
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
    [ValueRange(20.0d, 26500.0d)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(108.0d)]
    [Unit("MHz")]
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
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz")]
    [ValueRange(0.1d, 500.0d)]
    [DefaultValue(25.0d)]
    [Unit("kHz")]
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