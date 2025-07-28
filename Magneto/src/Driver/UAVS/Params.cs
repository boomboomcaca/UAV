using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UAVS;

[DriverDescription(FeatureType = FeatureType.UAVS,
    Name = "无人机管控",
    Category = ModuleCategory.RadioSuppressing,
    Version = "1.1.1",
    Model = "UAVS",
    MaxInstance = 1,
    Description = "联合监测设备实现引导或独立的无人机管制功能")]
public partial class Uavs
{
    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Suppressor)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("压制机")]
    [Module(NeedModule = ModuleCategory.RadioSuppressing,
        NeedFeature = FeatureType.UAVS,
        NeedEquip = true,
        IsPrimaryDevice = true
    )]
    [Description("设置无线电管制的设备")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Suppressor { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.SCAN
    )]
    [Description("设置提供扫描数据的主设置")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("antennaController")]
    [DisplayName("天线控制器")]
    [Category(PropertyCategoryNames.Installation)]
    [Module(
        NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true
    )]
    [Description("设置天线控制器")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.SwitchArray)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("开关矩阵")]
    [Module(NeedModule = ModuleCategory.SwitchArray,
        NeedEquip = true)]
    [Description("切换监测与管制的开关矩阵")]
    [PropertyOrder(3)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice SwitchArray { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("azimuth")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("模拟方位角")]
    [Description("设置无人机测向的示向度输出角度")]
    [ValueRange(0.0f, 360.0f)]
    [DefaultValue(10.0f)]
    [Unit(UnitNames.Degree)]
    [Style(DisplayStyle.Slider)]
    public float Azimuth { get; set; }

    [Parameter(AbilitySupport = FeatureType.UAVS)]
    [Name("snrThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信噪门限")]
    [ValueRange(-40, 120)]
    [DefaultValue(15)]
    [PropertyOrder(40)]
    [Unit(UnitNames.Db)]
    [Description("设置高于频谱底噪的门限值，用于筛选信号")]
    [Style(DisplayStyle.Slider)]
    public int SnrThreshold { get; set; } = 15;

    [Parameter(AbilitySupport = FeatureType.UAVS)]
    [Name(ParameterNames.IntegrationTime)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("积分次数")]
    [ValueRange(50, 1000)]
    [DefaultValue(300)]
    [PropertyOrder(41)]
    [Description("设置频谱积分次数，用于累计计算无人机的算法输入")]
    [Style(DisplayStyle.Slider)]
    public int IntegrationTimes { get; set; }

    [Parameter(AbilitySupport = FeatureType.UAVS)]
    [Name(ParameterNames.EnableSuppressGnss)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("是否开启卫星导航压制GNSS")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(false)]
    [PropertyOrder(42)]
    [Description("是否开启卫星导航压制GNSS，开启则压制卫星导航频段")]
    [Style(DisplayStyle.Switch)]
    public bool EnableSuppressGnss { get; set; }
}