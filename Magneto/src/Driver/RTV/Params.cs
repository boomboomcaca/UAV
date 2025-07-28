using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.RTV;

[DriverDescription(
    FeatureType = FeatureType.RTV,
    Name = "广播电视解调",
    Category = ModuleCategory.Decoder,
    Version = "1.5.0",
    Model = "RTV",
    MaxInstance = 1,
    IsMonopoly = true,
    Description = "广播电视解调功能")]
public partial class Rtv
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.RTV)]
    [Name("avReceiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("音视频接收设备")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.AVProcess)]
    [Description("录播盒，具备音视频数据的提取和播放的设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AvReceiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.RTV)]
    [Name("tvAnalysis")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("电视分析仪")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        NeedFeature = FeatureType.RTV)]
    [Description("具备电视节目搜索和播放的电视分析设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice TvAnalysis { get; set; }
}