using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.MScan;

[DriverDescription(
    Name = "离散扫描",
    MaxInstance = 8,
    Category = ModuleCategory.Monitoring,
    Version = "2.9.1",
    MediaType = MediaType.Scan,
    FeatureType = FeatureType.MScan,
    Description = "离散扫描功能",
    IsMonopoly = false)]
public partial class MScan
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.MScan)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.MScan)]
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
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }

    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Name("measureSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量开关")]
    [Description("离散扫描进行占用度测量的开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Browsable(false)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Switch)]
    public bool MeasureSwitch { get; set; }
}