using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Driver.PCOMS;

public class FreqBandTemplate
{
    [PropertyOrder(0)]
    [Name("duplexMode")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("网络制式")]
    [Description("网络制式")]
    public DuplexMode DuplexMode { get; set; } = DuplexMode.None;

    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置频段起始频率，单位：MHz")]
    [ValueRange(20.0d, 26500.0d)]
    [DefaultValue(758d)]
    [Unit(UnitNames.MHz)]
    public double StartFrequency { get; set; } = 758d;

    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("结束频率")]
    [Description("设置频段结束频率，单位：MHz")]
    [ValueRange(20.0d, 26500.0d)]
    [DefaultValue(788d)]
    [Unit(UnitNames.MHz)]
    public double StopFrequency { get; set; } = 788d;

    public static explicit operator FreqBandTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new FreqBandTemplate();
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