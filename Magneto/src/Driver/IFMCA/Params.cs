using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.IFMCA;

[DriverDescription(
    FeatureType = FeatureType.IFMCA,
    Name = "中频多路",
    Category = ModuleCategory.Monitoring,
    Version = "2.8.0",
    Model = "IFMCA",
    IsMonopoly = true,
    MaxInstance = 0,
    Description = "IFMCA功能")]
public partial class Ifmca
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.IFMCA)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.IFMCA)]
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

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name("autoChannelMode")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("自动子通道模式")]
    [Description("自动提取信号并设置子通道的模式，为航空监测使用")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Browsable(false)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Slider)]
    public bool AutoChannelMode { get; set; }
}