using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EthAntController;

/// <summary>
///     网口天线控制器参数定义
/// </summary>
[DeviceDescription(Name = "网口天线控制器",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.AntennaControl,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "EthernetAntennaController",
    Version = "1.5.3",
    Description = "网口天线控制器，适用于打通通过网口进行控制的监测或测向天线"
)]
public partial class EthAntController
{
    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("地址")]
    [Description("设置连接到设备的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [PropertyOrder(0)]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设备的网络端口号。")]
    [ValueRange(1024, 65535, 0)]
    [PropertyOrder(1)]
    [DefaultValue(5555)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 5555;
}