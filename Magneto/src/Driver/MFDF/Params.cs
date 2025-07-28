using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.MFDF;

[DriverDescription(
    Name = "离散测向",
    MaxInstance = 8,
    Category = ModuleCategory.DirectionFinding,
    Version = "1.0.2",
    MediaType = MediaType.Dfind | MediaType.Level | MediaType.Audio | MediaType.Spectrum,
    FeatureType = FeatureType.MFDF,
    Description = "离散测向功能",
    IsMonopoly = false)]
public partial class Mfdf
{
    private readonly object _lockMfdfPoints = new();
    private MfdfTemplate[] _mfdfFrequencies;
    private Dictionary<string, object>[] _mfdfPoints;

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.MFDF)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.DirectionFinding,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFDF)]
    [Description("提供测向数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.MFDF)]
    [Name("antennaController")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制，可为空")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }

    [Parameter(AbilitySupport = FeatureType.MFDF)]
    [Name(ParameterNames.HoldTime)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("等待时间")]
    [Description("等待时间，单位：秒。 如在【驻留时间】内，电平低于门限，则继续等待，如果超过了【等待时间】，电平依然低于门限，则继续测量下个频点；如果电平超过门限，则继续测量，直到超过【驻留时间】。")]
    [ValueRange(0, 10, 0.1)]
    [DefaultValue(0.1)]
    [PropertyOrder(10)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public double HoldTime { get; set; } = 0.1;

    [Parameter(AbilitySupport = FeatureType.MFDF)]
    [Name(ParameterNames.DwellTime)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("驻留时间")]
    [Description("驻留时间，单位：秒。如电平值超过门限值，则在该频率上继续测量的时间长度。")]
    [ValueRange(0, 10, 0.1)]
    [DefaultValue(0.1)]
    [PropertyOrder(10)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public double DwellTime { get; set; } = 0.1;

    // [Parameter(AbilitySupport = FeatureType.MFDF, Template = typeof(MfdfTemplate))]
    [Name(ParameterNames.MfdfPoints)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("离散测向频点")]
    [Description("设置离散测向频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    [PropertyOrder(1)]
    public Dictionary<string, object>[] MfdfPoints
    {
        get => null;
        set
        {
            lock (_lockMfdfPoints)
            {
                _mfdfPoints = value;
                if (_mfdfPoints == null)
                {
                    _total = 0;
                    _mfdfFrequencies = null;
                    return;
                }

                _mfdfFrequencies = Array.ConvertAll(value, item => (MfdfTemplate)item);
                _total = _mfdfPoints.Length;
            }
        }
    }
}

/// <summary>
///     离散测向模板
/// </summary>
public class MfdfTemplate
{
    [Parameter(AbilitySupport = FeatureType.MFDF)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("中心频率")]
    [Description("设置离散频点中心频率")]
    [ValueRange(20.0d, 6000.0d, 0.000001)]
    [DefaultValue(101.7d)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(1)]
    public double Frequency { get; set; }

    [Parameter(AbilitySupport = FeatureType.MFDF)]
    [Name(ParameterNames.DfBandwidth)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向带宽")]
    [Description("设置离散扫描中频滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|1000|500|250|200|120|100|50|25|12.5|6.25|3.125",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|1MHz|500kHz|250kHz|200kHz|120kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [Style(DisplayStyle.Bandwidth)]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(2)]
    public double DfBandwidth { get; set; }

    [Parameter(AbilitySupport = FeatureType.MFDF)]
    [Name(ParameterNames.MeasureThreshold)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测量门限")]
    [Description("获取或设置离散扫描进行占用度测量的门限值")]
    [ValueRange(-40, 120)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(20)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(10)]
    public int MeasureThreshold { get; set; } = 0;

    public static explicit operator MfdfTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new MfdfTemplate();
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
                        objValue = Guid.Parse(dict[name].ToString()!);
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