using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.IQRETRI;

[DriverDescription(
    Name = "流盘分析",
    MaxInstance = 8,
    Category = ModuleCategory.Monitoring,
    Version = "1.3.0",
    MediaType = MediaType.Audio | MediaType.Iq | MediaType.Itu | MediaType.Level | MediaType.Spectrum,
    FeatureType = FeatureType.IQRETRI,
    Description = "流盘分析功能",
    IsMonopoly = false)]
public partial class Iqretri
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.IQRETRI)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFM)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.IQRETRI)]
    [Name("ssDevice")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("流盘设备")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.IoStorage,
        NeedFeature = FeatureType.IQRETRI)]
    [Description("提供流盘存储与读取的设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice SsDevice { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("antennaController")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("iqRecordSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信号解调数据采集开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启信号解调数据采集")]
    [DefaultValue(false)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Switch)]
    public bool IqRecordSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("iqCalibrationValue")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("IQ校准值")]
    [DefaultValue(1073741824.0d)]
    [Description("用于接收机IQ的校准值，这个值每台接收机必须根据实际情况计算并重配。")]
    [PropertyOrder(35)]
    [ValueRange(0, 107374182400)]
    [Style(DisplayStyle.Input)]
    public double IqCalibrationValue { get; set; } = 1073741824.0d;

    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Name("iqSaveLenConfig")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信号解调采集长度字典")]
    [DefaultValue("500|163356;10000|134055;20000|5362157;40000|5368710")]
    [Description("不同带宽下采集IQ数据的点数，带宽单位为kHz，带宽与点数之间用|分隔，不同带宽点数之间用英文分号分隔，格式为：带宽1|点数1;带宽2|点数2;带宽3|点数3")]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string IqSaveLenConfig { get; set; } = "500|163356;10000|134055;20000|5362157;40000|5368710";
}