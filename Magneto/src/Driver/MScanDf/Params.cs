using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.MScanDf;

[DriverDescription(
    Name = "离散扫描测向",
    MaxInstance = 8,
    Category = ModuleCategory.DirectionFinding,
    Version = "1.3.0",
    MediaType = MediaType.Dfind,
    FeatureType = FeatureType.MScanDf,
    Description = "离散扫描测向功能",
    IsMonopoly = false)]
public partial class MScanDf
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.MScanDf)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.DirectionFinding,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.MScanDf)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }
}