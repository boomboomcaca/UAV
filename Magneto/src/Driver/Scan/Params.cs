using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.Scan;

[DriverDescription(FeatureType = FeatureType.SCAN,
    Name = "频段扫描",
    Category = ModuleCategory.Monitoring,
    Version = "3.1.5",
    Model = "Scan",
    MaxInstance = 5,
    Description = "频段扫描功能")]
public partial class Scan
{
    private double _tolerance = 3d;

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.SCAN)]
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

    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("occupancyThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频段占用度统计阈值")]
    [Description("设置占用度过滤值,用来分析信号列表 单位 %")]
    [ValueRange(0d, 15d, 0.1)]
    [DefaultValue(1d)]
    [PropertyOrder(34)]
    [Unit(UnitNames.Pct)]
    [Style(DisplayStyle.Slider)]
    public double OccupancyThreshold { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
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
    [Browsable(false)]
    public bool AutoThreshold { get; set; } = true;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.ThresholdValue)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("手动门限值")]
    [Description("设置在手动门限下的手动门限值，为数组类型")]
    [ValueRange(-20, 100)]
    [DefaultValue(new double[] { 6 })]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    [Browsable(false)]
    public double[] ThresholdValue { get; set; } = { 6 };

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("tolerance")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("自动门限容差")]
    [Description("自动门限容差;")]
    [ValueRange(1, 30)]
    [DefaultValue(3)]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public double Tolerance
    {
        get => _tolerance;
        set
        {
            if (value <= 0) value = 1;
            _tolerance = value;
        }
    }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.OccupancySwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Resident]
    [DisplayName("占用度显示开关")]
    [Description("切换占用度是否显示")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool OccupancySwitch { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("signalsSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Resident]
    [DisplayName("信号列表显示开关")]
    [Description("切换信号列表是否显示")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public bool SignalsSwitch { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("signalsCount")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("信号列表最大个数")]
    [Description("每个频段发送到前端的信号列表的数据最大个数")]
    [ValueRange(1, 10000)]
    [DefaultValue(1000)]
    [Style(DisplayStyle.Switch)]
    [Browsable(false)]
    public int SignalsCount { get; set; }
}