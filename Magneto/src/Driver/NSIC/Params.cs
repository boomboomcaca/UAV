using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace NSIC;

[DriverDescription(
    FeatureType = FeatureType.Nsic,
    Name = "新信号截获",
    Category = ModuleCategory.Monitoring,
    Version = "1.10.1.0",
    Model = "NSIC",
    IsMonopoly = true,
    MaxInstance = 1,
    Description = "新信号截获功能")]
public partial class Nsic
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.Nsic)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.Scan)]
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

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Nsic)]
    [Name("functionSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|信号比对|模板采集")]
    [Resident]
    [DisplayName("功能切换")]
    [Description("切换采集模板与信号比对")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool FunctionSwitch { get; set; }

    [PropertyOrder(11)]
    [Parameter(AbilitySupport = FeatureType.Nsic)]
    [Name("setSimData")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [Resident]
    [DisplayName("模拟信号")]
    [Description("是否产生模拟信号")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool SetSimData { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Nsic)]
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
    public bool AutoThreshold { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Nsic)]
    [Name(ParameterNames.ThresholdValue)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("门限")]
    [Description("如果是手动门限则本参数为门限值;如果是自动门限则本参数为门限容差;如果为信号比对功能则本参数为截获阈值")]
    [ValueRange(0, 60)]
    [DefaultValue(6)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public double ThresholdValue { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Nsic)]
    [Name("templateID")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("模板ID")]
    [Description("新信号比对的模板ID")]
    [DefaultValue(6)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string TemplateId { get; set; } = "";
}