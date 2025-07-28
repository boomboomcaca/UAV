using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.SerialAntController;

/// <summary>
///     串口天线控制器参数定义
/// </summary>
[DeviceDescription(Name = "串口天线控制器",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.AntennaControl,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "SerialPortAntennaController",
    Version = "1.5.3",
    Description = "串口天线控制器，适用于打通通过串口进行控制的监测或测向天线")]
public partial class SerialAntController
{
    [Name("com")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [Description("设置天线控制器占用的串口编号")]
    [PropertyOrder(0)]
    [DefaultValue("COM1")]
    [ValueRange(double.NaN, double.NaN, 5)]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; } = "COM1";

    [Name("baudRate")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [Description("设置与串口通信的波特率")]
    [PropertyOrder(1)]
    [DefaultValue(9600)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200",
        DisplayValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200")]
    [Style(DisplayStyle.Dropdown)]
    public int BaudRate { get; set; } = 9600;
}