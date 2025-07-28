using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SGLDEC;

[DriverDescription(
    FeatureType = FeatureType.SGLDEC,
    Name = "信号解调",
    Category = ModuleCategory.Monitoring,
    Version = "1.1.0",
    Model = "SGLDEC",
    MaxInstance = 1,
    IsMonopoly = true,
    Description = "信号解调功能")]
public partial class Sgldec
{
    [Name("decoder")]
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.SGLDEC)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("信号解调设备")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.SGLDEC)]
    [Description("提供信号解调设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Decoder { get; set; }
}