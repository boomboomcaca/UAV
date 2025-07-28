using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.YaesuG1000DXC;

[DeviceDescription(Name = "YaesuG1000DXC",
    Manufacturer = "八重洲",
    DeviceCategory = ModuleCategory.Swivel,
    MaxInstance = 1,
    Version = "1.0.1",
    Model = "YaesuG1000DXC",
    FeatureType = FeatureType.AmpDF | FeatureType.FFDF,
    Description = "八重洲YaesuG1000DXC转台(仅支持水平0°~360°转动)")]
public partial class YaesuG1000Dxc
{
    #region 设备参数

    [PropertyOrder(0)]
    [Name("movement")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电机动作")]
    [Description("设置电机要执行的操作。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|1|2|3|4",
        DisplayValues = "|不控制电机|停止转动|收天线|比幅测向|转动天线|步进控制"
    )]
    [DefaultValue(0)]
    public int Movement { get; set; }

    [PropertyOrder(1)]
    [Name("azimuthAngle")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("方位角")]
    [Description("设置方位角度数，将天线转到指定的角度。")]
    [DefaultValue(0)]
    [ValueRange(0, 360)]
    public float AzimuthAngle { get; set; } = 100;

    [PropertyOrder(2)]
    [Name("measureCount")]
    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("比幅次数")]
    [ValueRange(1, 30)]
    [DefaultValue(1)]
    [Description("比幅测向的次数（电机来回转动360度为一次）。")]
    public int MeasureCount { get; set; } = 1;

    #endregion

    #region 安装参数

    [PropertyOrder(3)]
    [Name("serialPortNum")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("连接设备的串口号")]
    [DefaultValue("COM4")]
    [Description("工控机连接YeasuG1000DXC串口控制器的串口号")]
    public string SerialPortNum { get; set; } = "COM4";

    [PropertyOrder(4)]
    [Name("baudRate")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|9600|4800|2400|1200",
        DisplayValues = "|9600|4800|2400|1200"
    )]
    [DefaultValue(9600)]
    [Description("传输波特率")]
    public int BaudRate { get; set; } = 9600;

    [PropertyOrder(5)]
    [Name("offSet")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向角度偏差")]
    [DefaultValue(6)]
    [ValueRange(0, 10)]
    [Description("比幅测向时，电机转动角度的固有偏差。单位°")]
    public int OffSet { get; set; } = 6;

    #endregion
}