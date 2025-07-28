using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1000AS;

[DeviceDescription(Name = "DT1000AS",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    FeatureType = FeatureType.BsDecoding,
    MaxInstance = 1,
    Version = "1.0.0",
    Model = "DT1000AS",
    Description = "伪基站识别模块")]
public partial class Dt1000As
{
    [PropertyOrder(0)]
    [Name("isGsm900Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("GSM900M")]
    [DefaultValue(true)]
    [Description("是否搜索GSM900M")]
    public bool Gsm900Search { get; set; } = true;

    [PropertyOrder(1)]
    [Name("isGSM1800Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("GSM1800M")]
    [DefaultValue(true)]
    [Description("是否搜索GSM1800M")]
    public bool Gsm1800Search { get; set; } = true;

    [PropertyOrder(2)]
    [Name("isCMCC900Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("移动900M")]
    [DefaultValue(true)]
    [Description("是否搜索移动900M")]
    public bool Cmcc900Search { get; set; } = true;

    [PropertyOrder(3)]
    [Name("isCUCC900Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("联通900M")]
    [DefaultValue(true)]
    [Description("是否搜索联通900M")]
    public bool Cucc900Search { get; set; } = true;

    [PropertyOrder(4)]
    [Name("isCMCC1800Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("移动1800M")]
    [DefaultValue(true)]
    [Description("是否搜索移动1800M")]
    public bool Cmcc1800Search { get; set; } = true;

    [PropertyOrder(5)]
    [Name("isCUCC1800Search")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("联通1800M")]
    [DefaultValue(true)]
    [Description("是否搜索联通1800M")]
    public bool Cucc1800Search { get; set; } = true;

    [PropertyOrder(6)]
    [Name("isGSMRSearch")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("GSMR")]
    [DefaultValue(true)]
    [Description("是否搜索GSMR")]
    public bool GsmrSearch { get; set; } = true;

    [PropertyOrder(7)]
    [Name("minRssiScan")]
    [Parameter(AbilitySupport = FeatureType.BsDecoding)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("最小搜索电平")]
    [DefaultValue(-90)]
    [Description("最小搜索电平，单位dBm")]
    public int MinRssiScan { get; set; } = -90;

    #region 安装属性

    [Name("com")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [Description("设置天线控制器占用的串口编号")]
    [PropertyOrder(8)]
    [DefaultValue("COM1")]
    public string Com { get; set; } = "COM1";

    [Name("baudRate")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [Description("设置与串口通信的波特率")]
    [PropertyOrder(9)]
    [DefaultValue(115200)]
    public int BaudRate { get; set; } = 115200;

    #endregion
}