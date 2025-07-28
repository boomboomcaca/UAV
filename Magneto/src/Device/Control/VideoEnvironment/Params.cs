using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "视频监控设备",
    DeviceCategory = ModuleCategory.Control,
    Manufacturer = "Aleph",
    Version = "1.2.0",
    Model = "VideoEnvironment",
    Description = "视频监控设备,前端使用",
    MaxInstance = 1,
    FeatureType = FeatureType.None)]
public partial class VideoEnvironment
{
    [Parameter(IsInstallation = true)]
    [Name("user")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("用户名")]
    [Description("设置视频监控的摄像头的访问用户")]
    [DefaultValue("admin")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string User { get; set; } = "admin";

    [Parameter(IsInstallation = true)]
    [Name("password")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("密码")]
    [Description("设置视频监控的摄像头的访问密码")]
    [DefaultValue("dc123456")]
    [PropertyOrder(1)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Password { get; set; } = "dc123456";

    [Parameter(IsInstallation = true)]
    [Name("host")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("监控地址")]
    [Description("设置视频监控的摄像头的IP地址")]
    [DefaultValue("192.168.1.64")]
    [PropertyOrder(2)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Host { get; set; } = "192.168.1.64";

    [Parameter(IsInstallation = true)]
    [Name("path")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("监控路径")]
    [Description("设置视频监控的摄像头的Uri路径")]
    [DefaultValue("/ISAPI/Streaming/channels/102/httpPreview")]
    [PropertyOrder(3)]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string Path { get; set; } = "/ISAPI/Streaming/channels/102/httpPreview";

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("摄像头地址")]
    [Description("设置摄像头的安装位置")]
    [DefaultValue("")]
    [PropertyOrder(4)]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }
}