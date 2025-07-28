using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.FixDF;

// [DriverDescription(FeatureType = FeatureType.FFDF,
//    Name = "单频测向",
//    MaxInstance = 8,
//    Category = ModuleCategory.DirectionFinding,
//    Version = "2.4.0",
//    Model = "FixDF",
//    Description = "实现单频测向功能")]
public partial class FixDf
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.FFDF)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.DirectionFinding,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.FFDF)]
    [Description("提供监测数据的主设备")]
    public IDevice Receiver { get; set; }
}