using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT2000AS;

[DeviceDescription(Name = "DT2000AS",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    Version = "1.1.1",
    Model = "DT2000AS",
    MaxInstance = 1,
    FeatureType = FeatureType.BsDecoding,
    Description = "DT2000AS USB全制式基站扫频仪，支持2/3/4/5G基站信息解码。")]
public partial class Dt2000As
{
    [PropertyOrder(0)]
    [Name("scanInterval")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("查询间隔")]
    [ValueRange(10, 10000)]
    [DefaultValue(200)]
    [Description("查询时间间隔，单位 毫秒")]
    [Style(DisplayStyle.Input)]
    public int ScanInterval { get; set; } = 200;

    [PropertyOrder(10)]
    [Name("isSearchTD_SCDMA")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("TD_SCDMA")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索TD_SCDMA类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool TdScdma { get; set; } = true;

    [PropertyOrder(11)]
    [Name("isSearchLTE")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("LTE")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索LTE类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Lte { get; set; } = true;

    [PropertyOrder(12)]
    [Name("isSearchGSM")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("GSM")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索GSM类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Gsm { get; set; } = true;

    [PropertyOrder(13)]
    [Name("isSearchWCDMA")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("WCDMA")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索WCDMA类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Wcdma { get; set; } = true;

    [PropertyOrder(14)]
    [Name("isSearchCDMA_1X")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("CDMA_1X")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索CDMA_1X类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Cdma1X { get; set; } = true;

    [PropertyOrder(15)]
    [Name("isSearchEVDO")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("EVDO")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索EVDO类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Evdo { get; set; } = true;

    [PropertyOrder(16)]
    [Name("isSearchNR")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("NR")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|搜索|不搜索")]
    [DefaultValue(true)]
    [Description("设置是否搜索5G NR类型基站。")]
    [Style(DisplayStyle.Switch)]
    public bool Nr { get; set; } = true;
}