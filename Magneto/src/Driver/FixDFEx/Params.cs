using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FixDFEx;

[DriverDescription(
    FeatureType = FeatureType.FDF,
    Name = "单频测向（比幅式）",
    MaxInstance = 0,
    Category = ModuleCategory.DirectionFinding,
    Version = "1.1.1",
    Model = "FixDFEx",
    Description = "适用于8GHz~16GHz比幅式单频测向")]
public partial class FixDfEx
{
    [Parameter(AbilitySupport = FeatureType.FDF)]
    [Name(ParameterNames.LevelThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("电平门限")]
    [Description("设置测向电平门限，当信号电平 超过门限时返回测向结果")]
    [ValueRange(-50, 140)]
    [DefaultValue(10)]
    [PropertyOrder(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int LevelThreshold { get; set; } = 10;

    [Parameter(AbilitySupport = FeatureType.FDF)]
    [Browsable(false)]
    [Name("divisionCount")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("分区数量")]
    [Description("设置方向图范围内等间距分区数量")]
    [StandardValues(
        IsSelectOnly = false,
        StandardValues = "|4|5|6|7|8|9",
        DisplayValues = "|4|5|6|7|8|9")]
    [DefaultValue(9)]
    [PropertyOrder(11)]
    [Style(DisplayStyle.Slider)]
    public int DivisionCount { get; set; } = 9;

    [Name("dfindMethod")]
    [Parameter(AbilitySupport = FeatureType.FDF)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向体制")]
    [Description("设置当前设备的测向体制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ci",
        DisplayValues = "|干涉仪")]
    [Browsable(false)]
    [DefaultValue(DfindMethod.Ci)]
    [Style(DisplayStyle.Radio)]
    public DfindMethod DfindMethod { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFM)]
    [PropertyOrder(12)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name("antennaSwivel")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("比幅天线")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.Swivel,
        IsPrimaryDevice = false,
        NeedFeature = FeatureType.FFDF)]
    [PropertyOrder(13)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaSwivel { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name(ParameterNames.AntennaController)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.AntennaControl,
        IsPrimaryDevice = false,
        NeedFeature = FeatureType.None)]
    [PropertyOrder(14)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name("deviation")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("较正角度")]
    [Description("示向度补偿角度")]
    [ValueRange(0, 360)]
    [DefaultValue(0)]
    [PropertyOrder(15)]
    [Unit(UnitNames.Degree)]
    [Style(DisplayStyle.Slider)]
    public int Deviation { get; set; } = 0;
}