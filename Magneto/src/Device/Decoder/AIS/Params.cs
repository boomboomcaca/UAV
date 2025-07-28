using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "AIS",
    Model = "AIS",
    Manufacturer = "飞通通信",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.None,
    Version = "1.1.0",
    MaxInstance = 1,
    Capacity = EdgeCapacity.AIS,
    Description = "AIS船舶信息设备")]
public partial class Ais
{
    [PropertyOrder(0)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("AIS设备默认IP地址。")]
    [DefaultValue("192.168.1.200")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.1.200";

    [PropertyOrder(1)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口号")]
    [Description("AIS设备默认端口号。")]
    [DefaultValue(4196)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 4196;

    [PropertyOrder(2)]
    [Name("interval")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("发送间隔")]
    [Description("消息发送间隔,单位 毫秒")]
    [DefaultValue(5000)]
    [ValueRange(5000, 240000, 0)]
    [Style(DisplayStyle.Input)]
    public int Interval { get; set; } = 5000;
}