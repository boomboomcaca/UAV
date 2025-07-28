using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "虚拟AIS",
    Model = "VirtualAIS",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.BsDecoding,
    Version = "1.2.0",
    MaxInstance = 1,
    Capacity = EdgeCapacity.ADS_B | EdgeCapacity.AIS | EdgeCapacity.BsDecoding,
    Description = "虚拟AIS接收器")]
public partial class VirtualAis
{
    private int _adsInterval = 500;
    private int _aisInterval = 500;

    [PropertyOrder(0)]
    [Name("aisSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("AIS模拟")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|true|false")]
    [Description("是否开启AIS数据模拟")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AisSwitch { get; set; } = true;

    [PropertyOrder(1)]
    [Name("adsSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("ADS_B模拟")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|true|false")]
    [Description("是否开启ADS_B数据模拟")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AdsSwitch { get; set; } = true;

    [PropertyOrder(1)]
    [Name("bsSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("基站信号模拟")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|true|false")]
    [Description("是否开启基站信号数据模拟")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool BsSwitch { get; set; } = true;

    [PropertyOrder(2)]
    [Name("aisInterval")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("AIS发送间隔")]
    [Description("消息发送间隔,单位 毫秒")]
    [ValueRange(100, 10000, 0)]
    [DefaultValue(500)]
    [Style(DisplayStyle.Input)]
    public int AisInterval
    {
        get => _aisInterval;
        set
        {
            if (value < 100)
                _aisInterval = 100;
            else if (value > 10000)
                _aisInterval = 10000;
            else
                _aisInterval = value;
        }
    }

    [PropertyOrder(3)]
    [Name("adsInterval")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("ADS发送间隔")]
    [Description("消息发送间隔,单位 毫秒")]
    [ValueRange(100, 10000, 0)]
    [DefaultValue(500)]
    [Style(DisplayStyle.Input)]
    public int AdsInterval
    {
        get => _adsInterval;
        set
        {
            if (value < 100)
                _adsInterval = 100;
            else if (value > 10000)
                _adsInterval = 10000;
            else
                _adsInterval = value;
        }
    }
}