using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ScanDF;

[DriverDescription(FeatureType = FeatureType.ScanDF,
    Name = "扫描测向",
    MaxInstance = 0,
    Category = ModuleCategory.DirectionFinding,
    Version = "2.6.0",
    Model = "ScanDF",
    Description = "扫描测向功能模块")]
public partial class ScanDf
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.ScanDF)]
    [Name(ParameterNames.Dfinder)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.DirectionFinding,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.ScanDF)]
    [Description("提供测向数据的设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice DFinder { get; set; }

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
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }
}