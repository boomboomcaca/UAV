using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.TX3010A;

[DeviceDescription(Name = "TX3010A",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    Manufacturer = "Aleph",
    Version = "1.2.1",
    Model = "TX3010A",
    Description = "国产无线电管制设备",
    MaxInstance = 1,
    FeatureType = FeatureType.UAVS
                  | FeatureType.SATELS
                  | FeatureType.PCOMS)]
public partial class Tx3010A
{
    #region 运行参数

    private RftxSegmentsTemplate[] _rftxSegments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS,
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
        AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS)]
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
            UpdateFrequencyRanges(_rftxBands);
        }
    }

    private float[] _powers;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS)]
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

    #endregion

    #region 安装参数

    private DeviceConfigTemplate[] _deviceConfig;

    [PropertyOrder(3)]
    [Parameter(IsInstallation = true, Template = typeof(DeviceConfigTemplate))]
    [Name("deviceConfig")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("设备配置")]
    [Description("配置设备的基本信息，支持添加多个管制设备")]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] DeviceConfig
    {
        get => null;
        set
        {
            if (value == null) return;
            _deviceConfig = Array.ConvertAll(value, item => (DeviceConfigTemplate)item);
        }
    }

    #endregion
}