using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ESE;

[DriverDescription(
    FeatureType = FeatureType.ESE,
    Name = "考试保障",
    Category = ModuleCategory.Monitoring,
    Version = "2.8.0",
    Model = "ESE",
    IsMonopoly = true,
    MaxInstance = 1,
    Description = "考试保障功能")]
public partial class Ese
{
    private readonly object _ifbwLock = new();
    private double _ifBandwidthEse;

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.ESE)]
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

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.ESE)]
    [Name("analyzer")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("分析仪")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        NeedFeature = FeatureType.FFM)]
    [Description("进行单频信号分析的设备，输出IQ")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Analyzer { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.ESE)]
    [Name("decoder")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("解码器")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        NeedFeature = FeatureType.ESE)]
    [Description("进行信号解调解码的设备或服务")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Decoder { get; set; }

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

    [Parameter(AbilitySupport = FeatureType.ESE)]
    [Name("ifBandwidthESE")]
    [DisplayName("中频带宽")]
    [Description("中频带宽、频谱跨距，单位：kHz")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|500|300|250|200|150|120|100|50|20|10|5|2|1",
        DisplayValues = "|500kHz|300kHz|250kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(250.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidthEse
    {
        get
        {
            lock (_ifbwLock)
            {
                return _ifBandwidthEse;
            }
        }
        set
        {
            lock (_ifbwLock)
            {
                _ifBandwidthEse = value;
            }
        }
    }

    /// <summary>
    ///     功能切换，true:信号比对,false:模板采集
    /// </summary>
    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
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

    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
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
    public bool AutoThreshold { get; set; }

    [PropertyOrder(7)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
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

    [PropertyOrder(8)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
    [Name("decodeThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [Resident]
    [DisplayName("解码阈值")]
    [Description("设置解码的阈值，如果解码时获取到的信号电平小于此阈值则跳过对此信号的解码")]
    [ValueRange(-20, 80)]
    [DefaultValue(6)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public double DecodeThreshold { get; set; }

    [PropertyOrder(9)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
    [Name("templateID")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("模板ID")]
    [Description("新信号比对的模板ID")]
    [DefaultValue(6)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string TemplateId { get; set; } = "";

    [PropertyOrder(11)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
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

    [PropertyOrder(12)]
    [Parameter(AbilitySupport = FeatureType.ESE)]
    [Name("priorityFrequency")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("优先解码频点")]
    [Description("信号解码时优先解析的频点")]
    [ValueRange(0.3, 26500, 6)]
    [DefaultValue(101.7)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double PriorityFrequency { get; set; } = 101.7;
}