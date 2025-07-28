using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SSOA;

[DriverDescription(
    FeatureType = FeatureType.SSOA,
    Name = "场强定位",
    Category = ModuleCategory.DirectionFinding,
    Version = "2.9.0",
    Model = "SSOA",
    IsMonopoly = true,
    MaxInstance = 1,
    Description = "单车场强定位功能")]
public partial class Ssoa
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.SSOA)]
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

    [Parameter(AbilitySupport = FeatureType.SSOA)]
    [Name("resetSSOA")]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("重置统计")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|重置|关闭")]
    [Description("重置场强定位统计")]
    [DefaultValue(false)]
    [PropertyOrder(33)]
    [Style(DisplayStyle.Switch)]
    public bool ResetSsoa { get; set; }

    [Parameter(AbilitySupport = FeatureType.SSOA)]
    [Name("saveLevelData")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("保存数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [Description("开启保存电平数据")]
    [DefaultValue(false)]
    [PropertyOrder(33)]
    [Style(DisplayStyle.Switch)]
    public bool SaveLevelData { get; set; }
}