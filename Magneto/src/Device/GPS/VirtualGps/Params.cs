using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualGps;

[DeviceDescription(
    Name = "虚拟GPS",
    DeviceCategory = ModuleCategory.Gps,
    Manufacturer = "Aleph",
    Version = "2.2.0",
    Model = "VirtualGps",
    Description = "虚拟GPS，内部测试使用",
    MaxInstance = 1,
    Capacity = EdgeCapacity.GPS,
    FeatureType = FeatureType.None)]
public partial class VirtualGps
{
    #region 安装参数

    [Name("longitude")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("经度")]
    [Description("设置GPS初始经度，作为无GPS数据输出时的默认经度")]
    [PropertyOrder(0)]
    [DefaultValue(104.063611d)]
    [ValueRange(-180, 180, 6)]
    [Style(DisplayStyle.Input)]
    public double Longitude { get; set; } = 104.063611d;

    [Name("latitude")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("纬度")]
    [Description("设置GPS初始纬度，作为无GPS数据输出时的默认纬度")]
    [PropertyOrder(1)]
    [DefaultValue(30.547778d)]
    [ValueRange(-90, 90, 6)]
    [Style(DisplayStyle.Input)]
    public double Latitude { get; set; } = 30.547778d;

    [Name("updatingData")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("更新数据")]
    [Description("设置是否启用GPS数据更新，如果不启用，则使用安装的初始经纬度")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [PropertyOrder(2)]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool UpdatingData { get; set; }

    private int _cycleTime = 1000;

    [Name("cycleTime")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("间隔时间")]
    [Description("设置GPS数据更新的周期，单位：毫秒")]
    [ValueRange(50, 20000, 0)]
    [PropertyOrder(3)]
    [DefaultValue(1000)]
    [Style(DisplayStyle.Input)]
    public int CycleTime
    {
        get => _cycleTime;
        set
        {
            if (value == 0) value = 1000;
            if (value < 50) value = 50;
            _cycleTime = value;
        }
    }

    #endregion
}