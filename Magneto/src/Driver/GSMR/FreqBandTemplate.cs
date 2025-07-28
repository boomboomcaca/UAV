using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Driver.GSMR;

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
                if (!dict.ContainsKey(name)) continue;
                var value = GetRealValue(property.PropertyType, dict[name]);
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

    private static object GetRealValue(Type propType, object obj)
    {
        if (propType.IsEnum) return Utils.ConvertStringToEnum(obj?.ToString(), propType);

        if (propType == typeof(Guid))
        {
            var b = Guid.TryParse(obj?.ToString(), out var value);
            if (!b) return Guid.Empty;
            return value;
        }

        if (typeof(IConvertible).IsAssignableFrom(propType))
        {
            var value = Convert.ChangeType(obj, propType);
            return value;
        }

        if (typeof(IList).IsAssignableFrom(propType))
        {
            var elementType = propType.IsArray ? propType.GetElementType() : propType.GetGenericArguments()[0];
            var source = obj as IList;
            if (source == null) return null;
            var count = source.Count;
            var enu = source.GetEnumerator();
            if (propType.IsArray)
            {
                if (elementType != null)
                {
                    var target = Array.CreateInstance(elementType, count);
                    for (var i = 0; i < count; i++)
                    {
                        enu.MoveNext();
                        target.SetValue(GetRealValue(elementType, enu.Current), i);
                    }

                    return target;
                }
            }
            else
            {
                var target = Activator.CreateInstance(propType);
                var list = target as IList;
                for (var i = 0; i < count; i++)
                {
                    enu.MoveNext();
                    list?.Add(GetRealValue(elementType, enu.Current));
                }

                return target;
            }
        }

        if (typeof(IDictionary).IsAssignableFrom(propType))
        {
            var source = obj as IDictionary;
            if (source == null) return null;
            var count = source.Count;
            var enu = source.GetEnumerator();
            var types = propType.GetGenericArguments();
            var target = Activator.CreateInstance(propType);
            var dic = target as IDictionary;
            for (var i = 0; i < count; i++)
            {
                enu.MoveNext();
                dic?.Add(GetRealValue(types[0], enu.Key), GetRealValue(types[1], enu.Value));
            }

            return target;
        }

        return obj;
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