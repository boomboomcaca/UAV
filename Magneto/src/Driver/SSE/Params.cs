using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SSE;

// [DriverDescription(
//     FeatureType = FeatureType.SSE,
//     Name = "空间谱测向",
//     Category = ModuleCategory.DirectionFinding,
//     Version = "1.5.0",
//     Model = "SSE",
//     MaxInstance = 0,
//     Description = "空间谱测向功能")]
public partial class Sse
{
    #region 安装属性

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [PropertyOrder(1)]
    [Name(ParameterNames.Receiver)]
    [DisplayName("测向机")]
    [Module(
        NeedFeature = FeatureType.SSE,
        NeedModule = ModuleCategory.DirectionFinding,
        NeedEquip = true,
        IsPrimaryDevice = true)]
    [Description("提供测向数据的主设备")]
    public IDevice DFinder { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [PropertyOrder(2)]
    [Name("receivers")]
    [DisplayName("接收机")]
    [Module(NeedFeature = FeatureType.IFOUT, // 具备中频输出的接收机
        NeedModule = ModuleCategory.Monitoring,
        NeedEquip = false, // 缺省可以不安装才能兼具一体式与分离式应用
        IsPrimaryDevice = false)]
    [Description("提供中频数据的接收机集合。如果不配置任何接收机，则当前功能为一体式单/多通道测向，配置接收机数量大于零，则对应为分离式单/多通道测向")]
    public IDevice[] Receivers { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.AntennaController)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    public IDevice AntennaController { get; set; }

    #endregion
}