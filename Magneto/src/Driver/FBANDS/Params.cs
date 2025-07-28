using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FBANDS;

[DriverDescription(FeatureType = FeatureType.FBANDS,
    Name = "全频段管控",
    Category = ModuleCategory.RadioSuppressing,
    Version = "1.1.0",
    Model = "FBANDS",
    MaxInstance = 5,
    Description = "联合监测设备实现引导或独立的全频段管制功能")]
public partial class Fbands
{
    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Suppressor)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("压制机")]
    [Module(NeedModule = ModuleCategory.RadioSuppressing,
        NeedFeature = FeatureType.FBANDS,
        NeedEquip = true,
        IsPrimaryDevice = true
    )]
    [Description("设置无线电管制的设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    [PropertyOrder(0)]
    public IDevice Suppressor { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedModule = ModuleCategory.Monitoring,
        NeedFeature = FeatureType.SCAN,
        NeedEquip = true)]
    [Description("提供监测数据的主设备")]
    [PropertyOrder(1)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.AntennaController)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [PropertyOrder(2)]
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

    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Name("occupancyThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频段占用度统计阈值")]
    [Description("设置占用度过滤值,用来分析信号列表 单位 %")]
    [ValueRange(1d, 100d)]
    [DefaultValue(1d)]
    [PropertyOrder(34)]
    [Unit(UnitNames.Pct)]
    [Style(DisplayStyle.Slider)]
    public double OccupancyThreshold { get; set; } = 1d;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Name(ParameterNames.AutoThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|自动|手动")]
    [Resident]
    [DisplayName("自动门限")]
    [Description("切换自动门限与手动门限")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AutoThreshold { get; set; } = true;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Name(ParameterNames.ThresholdValue)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("手动门限值")]
    [Description("设置在手动门限下的手动门限值，为数组类型")]
    [ValueRange(-20, 100)]
    [DefaultValue(new double[] { 6 })]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public double[] ThresholdValue { get; set; } = { 6 };

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.FBANDS)]
    [Name("tolerance")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("自动门限容差")]
    [Description("自动门限容差;")]
    [ValueRange(0, 30)]
    [DefaultValue(3)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public double Tolerance { get; set; } = 3d;
}