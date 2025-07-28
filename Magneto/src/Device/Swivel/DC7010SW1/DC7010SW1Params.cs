using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.DC7010SW1;

[DeviceDescription(Name = "DC7010SW1",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Swivel,
    MaxInstance = 1,
    Version = "1.0.1",
    Model = "DC7010SW1",
    FeatureType = FeatureType.AmpDF | FeatureType.FFDF,
    Description = "支持同一方向方位角连续转动，不支持俯仰角转动")]
public partial class Dc7010Sw1
{
    #region 设备参数

    [PropertyOrder(2)]
    [Name("movement")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电机动作")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|不控制电机-1|停止转动0|收天线1|比幅测向2|转动天线3|步进控制4",
        StandardValues = "|-1|0|1|2|3|4")]
    [DefaultValue(0)]
    public int Movement { get; set; }

    [PropertyOrder(3)]
    [Name("measureCount")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("比幅次数")]
    [ValueRange(1, 30)]
    [DefaultValue(1)]
    [Description("比幅测向次数(电机转动360°为一次")]
    public int MeasureCount { get; set; } = 1;

    [PropertyOrder(5)]
    [Name("azimuthAngle")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("方位角")]
    [ValueRange(-180f, 180f)]
    [DefaultValue(0f)]
    [Description("设置云台俯仰角")]
    public float AzimuthAngle { get; set; }

    #endregion

    #region 安装参数

    [PropertyOrder(0)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("地址")]
    [Description("设置连接DC7010SW1的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [DefaultValue("192.168.1.108")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.1.108";

    [PropertyOrder(1)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到DC7010SW1的网络控制端口")]
    [ValueRange(1024, 65535)]
    [DefaultValue(2208)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 2208;

    #endregion
}