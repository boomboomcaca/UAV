using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.EMDC;

[DriverDescription(FeatureType = FeatureType.EMDC,
    Name = "电磁环境采集",
    Category = ModuleCategory.Monitoring,
    Version = "2.1.0",
    Model = "EMDC",
    MaxInstance = 1,
    IsMonopoly = true,
    Description = "电磁环境采集功能")]
public partial class Emdc
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.EMDC)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.SCAN)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.AntennaController)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }

    [Parameter(AbilitySupport = FeatureType.EMDC)]
    [Name("occupancyThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频段占用度统计阈值")]
    [Description("设置占用度过滤值,用来分析信号列表 单位 %")]
    [ValueRange(1d, 100d)]
    [Unit(UnitNames.Pct)]
    [DefaultValue(1d)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Slider)]
    public double OccupancyThreshold { get; set; } = 1d;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.EMDC)]
    [Name(ParameterNames.ThresholdValue)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("自动门限容差")]
    [Description("如果是手动门限则本参数为门限值;如果是自动门限则本参数为自动门限容差;")]
    [ValueRange(1, 30)]
    [Unit(UnitNames.DBuV)]
    [DefaultValue(6)]
    [Style(DisplayStyle.Slider)]
    public double ThresholdValue { get; set; } = 6f;
}