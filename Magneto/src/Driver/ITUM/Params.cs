using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ITUM;

[DriverDescription(
    Name = "ITU测量",
    MaxInstance = 8,
    Category = ModuleCategory.Monitoring,
    Version = "1.4.0",
    MediaType = MediaType.Audio | MediaType.Iq | MediaType.Itu | MediaType.Level | MediaType.Spectrum,
    FeatureType = FeatureType.ITUM,
    Description = "ITU测量功能",
    IsMonopoly = false)]
public partial class Itum
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.ITUM)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.ITUM)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("antennaController")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }
}