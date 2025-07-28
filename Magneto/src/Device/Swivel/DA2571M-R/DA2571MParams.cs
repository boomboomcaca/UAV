using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

#pragma warning disable 1591
namespace Magneto.Device.DA2571M;

[DeviceDescription(Name = "DA2571M-R",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Swivel,
    MaxInstance = 1,
    Version = "1.1.1",
    Model = "DA2571M-R",
    FeatureType = FeatureType.AmpDF | FeatureType.FFDF,
    Description = "适用于比幅式单频测向的天线转台控制模块")]
public partial class Da2571M
{
    #region 设备参数

    private int _dfindMode = 1;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFDF)]
    [Name("dfindMode")]
    [DisplayName("测向模式")]
    [Description("设置测向模式，分别适用于测量常规和突发信号")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Gate",
        DisplayValues = "|常规信号|突发信号")]
    [DefaultValue(DFindMode.Normal)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Radio)]
    public DFindMode DFindMode
    {
        get => _dfindMode == 1 ? DFindMode.Normal : DFindMode.Gate;
        set
        {
            _dfindMode = value == DFindMode.Normal ? 1 : 2;
            if (TaskState == TaskState.Start) SendCommand($"*SPEED_{_dfindMode}");
        }
    }

    #endregion

    #region 安装属性

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [DisplayName("地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(2)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [DisplayName("端口")]
    [Description("设置连接到设备的网络控制端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(10000)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    #endregion
}