using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver;

[DriverDescription(
    FeatureType = FeatureType.EMDA,
    Name = "电磁环境分析",
    Category = ModuleCategory.Monitoring,
    Version = "1.4.1",
    Model = "EMDA",
    MaxInstance = 1,
    IsMonopoly = true,
    Description = "电磁环境分析功能")]
public partial class Emda
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.EMDA)]
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

    [Parameter(AbilitySupport = FeatureType.EMDA)]
    [Name(ParameterNames.MeasureThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量门限")]
    [Description("设置进行分析的测量门限")]
    [ValueRange(-40d, 120d)]
    [Unit(UnitNames.DBuV)]
    [DefaultValue(20d)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Slider)]
    [Browsable(false)]
    public double MeasureThreshold { get; set; } = 20d;
}