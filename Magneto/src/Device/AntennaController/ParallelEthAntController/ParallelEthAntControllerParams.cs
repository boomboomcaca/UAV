using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(Name = "并联天线控制器(网口)",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.AntennaControl,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "ParallelEthAntController",
    Version = "1.0.3",
    Description = "适用于两个网口天线控制器并联使用的场景"
)]
public partial class ParallelEthAntController
{
    #region 安装参数

    [Name("antController1IP")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器1 IP地址")]
    [Description("设置连接到天线控制器1的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [PropertyOrder(0)]
    [DefaultValue("127.0.0.1")]
    [Style(DisplayStyle.Input)]
    public string AntController1Ip { get; set; }

    [Name("port1")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器1 端口")]
    [Description("设置连接到天线控制器1的网络端口号。")]
    [ValueRange(1024, 65535)]
    [PropertyOrder(1)]
    [DefaultValue(10000)]
    [Style(DisplayStyle.Input)]
    public int Port1 { get; set; }

    [PropertyOrder(2)]
    [Name("antController2IP")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器2 IP地址")]
    [Description("设置连接到天线控制器2的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [DefaultValue("127.0.0.1")]
    [Style(DisplayStyle.Input)]
    public string AntController2Ip { get; set; }

    [Name("port2")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器2 端口")]
    [Description("设置连接到天线控制器2的网络端口号。")]
    [ValueRange(1024, 65535)]
    [PropertyOrder(3)]
    [DefaultValue(4196)]
    [Style(DisplayStyle.Input)]
    public int Port2 { get; set; }

    #endregion
}