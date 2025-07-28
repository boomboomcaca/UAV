using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.RxDrone;

[DeviceDescription(
    Name = "无人机识别设备（云哨）",
    Manufacturer = "Aleph",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.UavDecoder,
    Version = "1.10.1.0",
    Model = "RxDrone",
    Description = "基于云哨对大疆无人机的识别。",
    MaxInstance = 1,
    FeatureType = FeatureType.UavDef)]
public partial class RxDrone
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name("ipAddress")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("设备地址")]
    [Description("设置无人机识别设备的IP地址。")]
    [DefaultValue("192.168.2.1")]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("104.061,30.63202")]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("isDemo")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("演示模式")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IsDemo { get; set; }

    #endregion

    #region 设备参数

    private const int Gain = 70;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.UavDef | FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(87.0d)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.UavDef | FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(108.0d)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.UavDef | FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|1000|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|5MHz|1MHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进，单位kHz")]
    [Browsable(false)]
    [Style(DisplayStyle.Dropdown)]
    public double StepFrequency { get; set; } = 25.0d;

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.UavDef | FeatureType.Scan, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] ScanSegments
    {
        get => null;
        set
        {
            if (value == null) return;
            _segments = Array.ConvertAll(value, item => (SegmentTemplate)item);
        }
    }

    #endregion
}

public class SegmentTemplate
{
    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.UavDef
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(0.3d, 8000.0d)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Browsable(false)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.UavDef
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(0.3d, 8000.0d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Browsable(false)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.UavDef
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("stepFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|1000|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|5MHz|1MHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [ValueRange(3.125d, 5000.0d)]
    [DefaultValue(25.0d)]
    [Browsable(false)]
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