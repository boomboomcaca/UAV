using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver;

[DriverDescription(FeatureType = FeatureType.FDF,
    Name = "单频测向",
    MaxInstance = 8,
    Category = ModuleCategory.DirectionFinding,
    Version = "1.3.2",
    Model = "FDF",
    Description = "实现单频测向功能")]
public partial class Fdf
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name(ParameterNames.Dfinder)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.DirectionFinding,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFDF | FeatureType.SSE)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Dfinder { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FDF)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        NeedFeature = FeatureType.FFM)]
    [Description("提供监测数据的设备，可选项")]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("antennaController")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = false)]
    [Description("使用的天线控制器，实现天线的逻辑控制,可以为空")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }

    [Name("dfindMethod")] public DfindMethod DfindMethod { get; set; }
}