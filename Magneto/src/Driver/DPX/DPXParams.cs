using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace DPX;

[DriverDescription(
    Name = "荧光谱",
    MaxInstance = 8,
    Category = ModuleCategory.Monitoring,
    Version = "2.4.0",
    MediaType = MediaType.Audio | MediaType.Iq | MediaType.Itu | MediaType.Level | MediaType.Spectrum | MediaType.Tdoa,
    FeatureType = FeatureType.DPX,
    Description = "荧光谱功能",
    IsMonopoly = false)]
public partial class Dpx
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.DPX)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFM)]
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