using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7520MOB;

[DeviceDescription(Name = "DC7520MOB",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    Version = "1.1.0",
    Model = "DC7520MOB",
    MaxInstance = 1,
    FeatureType = FeatureType.BsDecoding,
    Description = "基站解码器，支持所有移动、联通和电信制式。")]
public partial class Dc7520Mob
{
    #region 安装参数

    [PropertyOrder(0)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通信方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|TCP|UDP|串口",
        DisplayValues = "|TCP|UDP|串口")]
    [DefaultValue("TCP")]
    [Description("设置与设备的通信方式，当选择TCP/UDP时，对应的IP和端口必须设置正确；选择串口时，对应的串口号和波特率必须设置正确。")]
    [Style(DisplayStyle.Radio)]
    public string NetType { get; set; } = "TCP";

    #region 网口

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx。（注：当“通信方式”选择为TCP/UDP时生效！）")]
    [PropertyOrder(1)]
    [DefaultValue("192.168.151.94")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.151.94";

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设置的网络控制端口。（注：当“通信方式”选择为TCP/UDP时生效！）")]
    [PropertyOrder(2)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(10000)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 10000;

    #endregion

    #region 串口

    [PropertyOrder(3)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [DefaultValue("COM3")]
    [Description("设置DC7520MOB的串口号，格式：COMx  。（注：当“通信方式”选择为串口时生效！）")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; } = "COM3";

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [DefaultValue(115200)]
    [Description("设置DC7520MOB的串口波特率。（注：当“通信方式”选择为串口时生效！）")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200",
        DisplayValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200")]
    [Style(DisplayStyle.Dropdown)]
    public int Baudrate { get; set; } = 115200;

    #endregion

    [PropertyOrder(5)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("GSM/LTE查询个数")]
    [DefaultValue(20)]
    [Description("设置每次查询GSM/LTE制式基站的个数。")]
    [ValueRange(1, 100, 0)]
    [Style(DisplayStyle.Input)]
    public int Count { get; set; } = 20;

    [PropertyOrder(6)]
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
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("超时时间")]
    [DefaultValue(45)]
    [Description("设置搜索超时时间，单位 秒。")]
    [ValueRange(1, 100, 0)]
    [Style(DisplayStyle.Input)]
    public int Timeout
    {
        get => _timeout;
        set
        {
            _timeout = value * 1000;
            if (_client != null)
                _client.Timeout = _timeout;
        }
    }

    //通道1没有使用，不会产生TD_SCDMA
    [PropertyOrder(10)]
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

    #endregion
}