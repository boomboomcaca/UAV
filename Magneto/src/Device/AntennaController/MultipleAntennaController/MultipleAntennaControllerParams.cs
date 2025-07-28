using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(Name = "串联天线控制器(网口)",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.AntennaControl,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "MultipleAntennaController",
    Version = "1.1.3",
    Description = "适用于两个网口天线控制器串联使用的场景"
)]
public partial class MultipleAntennaController
{
    #region 安装参数

    [Name("ip1")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器1 IP地址")]
    [Description("设置连接到天线控制器1的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [PropertyOrder(0)]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AntController1Ip { get; set; }

    [Name("port1")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器1 端口")]
    [Description("设置连接到天线控制器1的网络端口号。")]
    [ValueRange(1024, 65535, 0)]
    [PropertyOrder(1)]
    [DefaultValue(10000)]
    [Style(DisplayStyle.Input)]
    public int Port1 { get; set; }

    [Name("ip2")]
    [PropertyOrder(2)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器2 IP地址")]
    [Description("设置连接到天线控制器2的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AntController2Ip { get; set; }

    [Name("port2")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器2 端口")]
    [Description("设置连接到天线控制器2的网络端口号。")]
    [ValueRange(1024, 65535, 0)]
    [PropertyOrder(3)]
    [DefaultValue(4196)]
    [Style(DisplayStyle.Input)]
    public int Port2 { get; set; }

    #endregion
}