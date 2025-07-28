using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoRadioSuppressor;

[DeviceDescription(Name = "虚拟压制设备",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    Manufacturer = "Aleph",
    Version = "1.1.0",
    Model = "DemoRadioSuppressor",
    Description = "虚拟压制设备",
    MaxInstance = 1,
    FeatureType = FeatureType.UAVS
                  | FeatureType.FBANDS
                  | FeatureType.SATELS
                  | FeatureType.UAVD
                  | FeatureType.UavDef
                  | FeatureType.PCOMS)]
public partial class DemoRadioSuppressor
{
    #region 运行参数

    private RftxSegmentsTemplate[] _rftxSegments;

    [PropertyOrder(3)]
    [Parameter(
        AbilitySupport = FeatureType.UavDef | FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS |
                         FeatureType.FBANDS,
        Template = typeof(RftxSegmentsTemplate))]
    [Name(ParameterNames.RftxSegments)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频管控")]
    [Description("表示包含特定通道多条待压制参数信息")]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxSegments
    {
        get => null;
        set
        {
            if (value == null)
            {
                _rftxSegments = null;
                return;
            }

            _rftxSegments = Array.ConvertAll(value, item => (RftxSegmentsTemplate)item);
        }
    }

    private RftxBandsTemplate[] _rftxBands;

    [PropertyOrder(3)]
    [Parameter(Template = typeof(RftxBandsTemplate),
        AbilitySupport = FeatureType.UavDef | FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS |
                         FeatureType.FBANDS)]
    [Name(ParameterNames.RftxBands)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("管制频段")]
    [Description("表示管制通道包含的基本信息")]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxBands
    {
        get => null;
        set
        {
            if (value == null) return;
            _rftxBands = Array.ConvertAll(value, item => (RftxBandsTemplate)item);
        }
    }

    private float[] _powers;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport =
        FeatureType.UavDef | FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS | FeatureType.FBANDS)]
    [Name(ParameterNames.Powers)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("功放功率")]
    [Description("每个功放下发的功率大小")]
    [ValueRange(30, 50)]
    [Unit(UnitNames.DBm)]
    [Style(DisplayStyle.Slider)]
    public float[] Powers
    {
        get => null;
        set
        {
            if (value == null) return;
            _powers = value;
        }
    }

    [Parameter(AbilitySupport =
        FeatureType.UavDef | FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS | FeatureType.FBANDS)]
    [Name("omniSuppressing")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("全向压制")]
    [Description("打开或关闭全向压制")]
    public bool OmniSuppressing { get; set; }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设备的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(26)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设备连接的端口号")]
    [DefaultValue(11720)]
    [PropertyOrder(27)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 11720;

    [Parameter(IsInstallation = true)]
    [Name("asRealDeviceSocket")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("真实设备模拟")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("是否模拟真实设备的网络状况")]
    [Style(DisplayStyle.Switch)]
    public bool AsRealDevice { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("")]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    #endregion
}