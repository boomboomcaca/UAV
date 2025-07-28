using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UAVD;

[DriverDescription(
    Name = "无人机侦测",
    MaxInstance = 0,
    Category = ModuleCategory.Monitoring,
    Version = "2.5.1",
    FeatureType = FeatureType.UAVD,
    Description = "无人机侦测",
    IsMonopoly = false)]
public partial class Uavd
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UAVD)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(
        NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.SCAN
    )]
    [Description("设置提供扫描数据的主设置")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UAVD)]
    [Name("antennaController")]
    [DisplayName("天线控制器")]
    [Category(PropertyCategoryNames.Installation)]
    [Module(
        NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true
    )]
    [Description("设置天线控制器")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public new IDevice AntennaController
    {
        get => base.AntennaController;
        set => base.AntennaController = value;
    }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.UAVD)]
    [Name("azimuth")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("模拟方位角")]
    [Description("设置无人机测向的示向度输出角度")]
    [ValueRange(0.0f, 360.0f)]
    [DefaultValue(10.0f)]
    [Unit(UnitNames.Degree)]
    [Style(DisplayStyle.Slider)]
    public float Azimuth { get; set; }

    [Parameter(AbilitySupport = FeatureType.UAVD)]
    [Name("snrThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信噪门限")]
    [ValueRange(-40, 120)]
    [DefaultValue(15)]
    [PropertyOrder(40)]
    [Unit(UnitNames.Db)]
    [Description("设置高于频谱底噪的门限值，用于筛选信号")]
    [Style(DisplayStyle.Slider)]
    public int SnrThreshold { get; set; } = 15;

    [Parameter(AbilitySupport = FeatureType.UAVD)]
    [Name(ParameterNames.IntegrationTime)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("积分次数")]
    [ValueRange(50, 1000)]
    [DefaultValue(300)]
    [PropertyOrder(41)]
    [Description("设置频谱积分次数，用于累计计算无人机的算法输入")]
    [Style(DisplayStyle.Slider)]
    public int IntegrationTimes { get; set; }
}