using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualCompass;

[DeviceDescription(
    Name = "虚拟罗盘",
    DeviceCategory = ModuleCategory.Compass,
    Manufacturer = "Aleph",
    Version = "2.2.0",
    Model = "VirtualCompass",
    Description = "虚拟罗盘，内部测试使用",
    MaxInstance = 1,
    Capacity = EdgeCapacity.Compass,
    FeatureType = FeatureType.None)]
public partial class VirtualCompass
{
    #region 安装参数

    [Name("degree")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("初始角度")]
    [Description("设置罗盘相对于正北方向的角度")]
    [ValueRange(0.0f, 360.0f)]
    [PropertyOrder(0)]
    [DefaultValue(0.0f)]
    [Style(DisplayStyle.Slider)]
    public float Degree { get; set; }

    [Name("isMove")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("是否转动")]
    [Description("设置该罗盘是否随着时间按照指定步进增加。")]
    [PropertyOrder(1)]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IsMove { get; set; }

    [Name("step")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("转动步进")]
    [Description("当罗盘配置为转动罗盘时的步进设置。")]
    [PropertyOrder(1)]
    [ValueRange(0, 360)]
    [DefaultValue(2f)]
    [Style(DisplayStyle.Slider)]
    public float Step { get; set; } = 2f;

    #endregion
}