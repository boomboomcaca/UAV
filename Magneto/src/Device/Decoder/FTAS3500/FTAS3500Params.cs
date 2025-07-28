using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.FTAS3500;

[DeviceDescription(
    Name = "FTAS3500",
    DeviceCategory = ModuleCategory.Decoder,
    Manufacturer = "北京德辰科技有限公司",
    Version = "1.1.0",
    Model = "FTAS3500",
    FeatureType = FeatureType.ESE,
    MaxInstance = 1,
    Description = "适用信号解调解码的服务模块"
)]
public partial class Ftas3500
{
    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [DisplayName("地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [DisplayName("端口")]
    [Description("设置连接到设备的网络控制端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [ReadOnly(true)]
    [DefaultValue(10110)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 10110;

    [Parameter(IsInstallation = true)]
    [Name("locateUdpPort")]
    [DisplayName("本地UDP端口")]
    [Description("设置本地接收解析结果的UDP端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [ReadOnly(true)]
    [DefaultValue(10111)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Input)]
    public int LocalUdpPort { get; set; } = 10111;

    [Parameter(IsInstallation = true)]
    [Name("remoteUdpPort")]
    [DisplayName("远程UDP端口")]
    [Description("设置解码服务接收IQ数据的UDP服务端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [ReadOnly(true)]
    [DefaultValue(10112)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public int RemoteUdpPort { get; set; } = 10112;
}