using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "ADS_B_2020",
    Model = "ADS_B_2020",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.None,
    Version = "1.1.0",
    MaxInstance = 1,
    Capacity = EdgeCapacity.ADS_B,
    Description = "ADS-B接收器")]
public partial class AdsB2020
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备IP地址")]
    [Description("设置设备的（IPv4）网络地址，格式:xxx.xxx.xxx.xxx。")]
    [PropertyOrder(0)]
    [DefaultValue("192.168.1.27")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DeviceIp { get; set; } = "192.168.1.27";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设备TCP监听端口。")]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(10001)]
    [Style(DisplayStyle.Input)]
    public int DevicePort { get; set; } = 10001;

    [PropertyOrder(2)]
    [Name("interval")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("发送间隔")]
    [Description("消息发送间隔,单位 毫秒")]
    [DefaultValue(500)]
    [ValueRange(500, 120000)]
    [Style(DisplayStyle.Slider)]
    public int Interval { get; set; } = 500;

    #endregion 安装参数
}