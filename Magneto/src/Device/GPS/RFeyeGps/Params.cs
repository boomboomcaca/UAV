using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeyeGps;

[DeviceDescription(Name = "RFeyeGPS",
    Manufacturer = "CRFS",
    DeviceCategory = ModuleCategory.Gps,
    MaxInstance = 1,
    Model = "RFeyeGPS",
    Version = "1.0.0",
    FeatureType = FeatureType.None,
    Capacity = EdgeCapacity.GPS,
    Description = "射频眼GPS模块通用")]
public partial class RFeyeGps
{
    [Name(ParameterNames.IpAddress)]
    [PropertyOrder(24)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("GPS模块IP地址。")]
    [DefaultValue("192.168.1.90")]
    public string Ip { get; set; } = "192.168.1.90";

    [Name(ParameterNames.Port)]
    [PropertyOrder(25)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("GPS模块端口号。")]
    [DefaultValue(9999)]
    public int Port { get; set; } = 9999;
}