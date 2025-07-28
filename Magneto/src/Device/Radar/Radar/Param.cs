using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.Radar;

[DeviceDescription(Name = "雷达",
    DeviceCategory = ModuleCategory.Radar,
    Manufacturer = "Aleph",
    Version = "1.10.1.0",
    Model = "Radar",
    Description = "雷达",
    MaxInstance = 1,
    FeatureType = FeatureType.UavDef)]
public partial class Radar
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [Name("ipAddress")]
    [DisplayName("设备地址")]
    [Description("设置图像识别设备的摄像头的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [Name("port")]
    [DisplayName("设备端口号")]
    [Description("设置图像识别设备的摄像头的端口号")]
    [DefaultValue(16002)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("")]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    #endregion
}