using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7530MOB_5G;

[DeviceDescription(Name = "DC7530MOB_5G",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    Version = "1.1.0",
    Model = "DC7530MOB_5G",
    MaxInstance = 1,
    FeatureType = FeatureType.BsDecoding,
    Description = "基站解码器，支持移动、联通、电信制式的解码。")]
public partial class Dc7530Mob5G
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx。")]
    [PropertyOrder(1)]
    [DefaultValue("192.168.30.201")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.30.201";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设置的网络控制端口。")]
    [PropertyOrder(2)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(10002)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 10002;

    [PropertyOrder(5)]
    [Name("count")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("GSM/LTE查询个数")]
    [DefaultValue(20)]
    [Description("设置每次查询GSM/LTE制式基站的个数。固定站推荐设置为 20，监测车推荐设置为 4。")]
    [ValueRange(1, 100, 0)]
    [Style(DisplayStyle.Input)]
    public int Count { get; set; } = 20;

    [PropertyOrder(6)]
    [Name("useChannel1")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通道1")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开通|关闭")]
    [DefaultValue(true)]
    [Description("设置通道1是否使用。")]
    [Style(DisplayStyle.Switch)]
    public bool UseChannel1 { get; set; } = true;

    [PropertyOrder(7)]
    [Name("useChannel2")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通道2")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开通|关闭")]
    [DefaultValue(true)]
    [Description("设置通道2是否使用。")]
    [Style(DisplayStyle.Switch)]
    public bool UseChannel2 { get; set; } = true;

    [PropertyOrder(8)]
    [Name("useChannel3")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通道3")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开通|关闭")]
    [DefaultValue(true)]
    [Description("设置通道3是否使用。")]
    [Style(DisplayStyle.Switch)]
    public bool UseChannel3 { get; set; } = true;

    #endregion

    #region 常规参数

    private int _timeout = 45000;

    [PropertyOrder(9)]
    [Name("timeout")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("超时时间")]
    [DefaultValue(45000)]
    [Description("设置搜索超时时间，单位毫秒。")]
    [ValueRange(1000, 100000, 0)]
    [Style(DisplayStyle.Input)]
    public int Timeout
    {
        get => _timeout;
        set
        {
            _timeout = value;
            if (_client != null)
                _client.Timeout = _timeout;
        }
    }

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

    #endregion
}