using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(
    Name = "ADS_B_USB",
    Model = "ADS_B_USB",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.None,
    Version = "1.0.0",
    MaxInstance = 1,
    Capacity = EdgeCapacity.ADS_B,
    Description = "USB ADS-B接收器")]
public partial class AdsBUsb
{
    #region 安装参数

    [Name("com")]
    [PropertyOrder(0)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [DefaultValue("COM3")]
    [Description("设置ADS_B_USB的串口号，格式：COMx  。")]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; } = "COM3";

    #endregion 安装参数
}