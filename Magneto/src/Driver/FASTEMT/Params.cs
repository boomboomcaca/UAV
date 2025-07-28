using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FASTEMT;

[DriverDescription(FeatureType = FeatureType.FASTEMT,
    Name = "射电天文电测",
    Category = ModuleCategory.Monitoring,
    Version = "1.0.0",
    Model = "FASTEMT",
    MaxInstance = 1,
    Description = "射电天文电测功能")]
public partial class Fastemt
{
    /// <summary>
    ///     依赖的频谱仪设备
    /// </summary>
    private IFastIcb _icb;

    /// <summary>
    ///     依赖的频谱仪设备
    /// </summary>
    private IFastSpectrumScan _spectrumAnalyser;

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FASTEMT)]
    [Name(ParameterNames.Receiver)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FASTEMT)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice SpectrumAnalyser
    {
        get => (IDevice)_spectrumAnalyser;
        set => _spectrumAnalyser = (IFastSpectrumScan)value;
    }

    [Parameter(IsInstallation = true)]
    [Name("icb")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("控制箱")]
    [Module(NeedModule = ModuleCategory.Icb,
        NeedFeature = FeatureType.FASTEMT,
        NeedEquip = true)]
    [Description("使用的控制箱，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Icb
    {
        get => (IDevice)_icb;
        set => _icb = (IFastIcb)value;
    }
}