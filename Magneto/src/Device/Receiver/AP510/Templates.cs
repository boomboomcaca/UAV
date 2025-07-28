using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

public partial class Ap510
{
    /// <summary>
    ///     离散扫描模板类
    /// </summary>
    internal class DiscreteFrequencyTemplate
    {
        [PropertyOrder(0)]
        [Name(ParameterNames.Frequency)]
        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Category(PropertyCategoryNames.Measurement)]
        [DisplayName("中心频率")]
        [ValueRange(20.0d, 3600.0d)]
        [DefaultValue(101.7d)]
        [Unit(UnitNames.MHz)]
        [Description("单频监测时的中心频率,单位MHz")]
        public double Frequency { get; set; } = 101.7d;

        [PropertyOrder(1)]
        [Name(ParameterNames.FilterBandwidth)]
        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Category(PropertyCategoryNames.Measurement)]
        [DisplayName("滤波带宽")]
        [StandardValues(IsSelectOnly = true,
            StandardValues =
                "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|250|200|150|125|100|80|60|50|40|30|25|20|15|12.5|10|8|6|5|4|3|2.5|2|1.5|1.2|1|0.8|0.6",
            DisplayValues =
                "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|250kHz|200kHz|150kHz|125kHz|100kHz|80kHz|60kHz|50kHz|40kHz|30kHz|25kHz|20kHz|15kHz|12.5kHz|10kHz|8kHz|6kHz|5kHz|4kHz|3kHz|2.5kHz|2kHz|1.5kHz|1.2kHz|1kHz|800Hz|600Hz")]
        [DefaultValue(150d)]
        [Unit(UnitNames.KHz)]
        [Description("滤波带宽")]
        public double FilterBandwidth { get; set; } = 150d;

        [PropertyOrder(2)]
        [Name(ParameterNames.DemMode)]
        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Category(PropertyCategoryNames.Measurement)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|FM|AM|PM|CW|LSB|USB|IQ|PULSE",
            DisplayValues = "|FM|AM|PM|CW|LSB|USB|IQ|PULSE")]
        [DisplayName("解调模式")]
        [DefaultValue(Modulation.Fm)]
        [Description("设置解调模式")]
        public Modulation DemMode { get; set; } = Modulation.Fm;

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
}