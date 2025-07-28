using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ADS_B_PS;

[DeviceDescription(
    Name = "ADS_B_PS",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.None,
    Version = "1.0.0",
    Model = "ADS_B_PS",
    MaxInstance = 0,
    Capacity = EdgeCapacity.ADS_B,
    Description = "ADS-B网口接收器")]
public partial class AdsBPs
{
    #region 安装参数

    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备IP地址")]
    [Description("设置设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx。")]
    [PropertyOrder(0)]
    [DefaultValue("172.18.106.60")]
    public string DeviceIp { get; set; } = "172.18.106.60";

    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备TCP端口")]
    [Description("设备TCP监听端口。")]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535)]
    [DefaultValue(20000)]
    public int DevicePort { get; set; } = 20000;

    [Name("localIP")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("本地IP地址")]
    [Description("本地接收数据的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx，应与设备配置的IP保持一致。")]
    [PropertyOrder(2)]
    [DefaultValue("172.18.106.7")]
    public string LocalIp { get; set; }

    [Name("localPort")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("本地端口")]
    [Description("本地接收数据的端口（UDP方式接收），应与设备配置的端口保持一致。")]
    [PropertyOrder(3)]
    [ValueRange(1024, 65535)]
    [DefaultValue(30000)]
    public int LocalPort { get; set; } = 30000;

    [Name("tcpAlive")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("心跳检测")]
    [Description(
        "设置ADS-B设备的心跳检测方式，TCP为发送心跳包方式（选择TCP时，设备必须在配置界面勾选TCP复选框，否则设备可能无法初始化成功！)，Ping为发送ping包检查网路连通性的方式，默认选择Ping!")]
    [PropertyOrder(4)]
    [StandardValues(
        StandardValues = "|false|true",
        DisplayValues = "|Ping|TCP")]
    [DefaultValue(false)]
    public bool TcpAlive { get; set; }

    #endregion
}