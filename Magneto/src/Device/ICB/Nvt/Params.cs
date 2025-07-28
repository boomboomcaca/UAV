using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.Nvt;

[DeviceDescription(
    Name = "图像识别设备",
    Manufacturer = "Aleph",
    DeviceCategory = ModuleCategory.ICB,
    Version = "1.0.1",
    Model = "Nvt",
    Description = "图象识别设备，包括电机，摄像头，识别、跟踪算法等一套完整设备",
    MaxInstance = 1,
    FeatureType = FeatureType.UavDef)]
public partial class Nvt
{
    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备地址")]
    [Description("设置图像识别设备的摄像头的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Input)]
    public string DeviceIp { get; set; }

    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备端口号")]
    [Description("设置图像识别设备的摄像头的端口号")]
    [DefaultValue(6002)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public int DevicePort { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("")]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("用户名")]
    [Description("图像识别设备的登录用户名。")]
    [DefaultValue("system")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string UserName { get; set; }

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("密码")]
    [Description("图像识别设备的登录密码。")]
    [DefaultValue("system")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Password { get; set; }

    #endregion

    #region 控制参数

    private byte _direction;

    [Parameter(AbilitySupport = FeatureType.UAVD)]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("云台控制按钮")]
    [Description("手工云台台控制按钮")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|11|12|13|14|15|16|21|22|23|24|25|26|27|28",
        DisplayValues = "|停止|焦距变大|焦距变小|焦点前调|焦点后调|光圈扩大|光圈缩小|上|下|坐|右|右上|右下|左上|左下")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Default)]
    public byte Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            ControlPtz(value, value.Equals(0) ? 0 : (uint)1, (uint)Speed);
        }
    }

    [Parameter(AbilitySupport = FeatureType.UAVD)]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("云台转动速度")]
    [Description("设置云台的转动速度")]
    [DefaultValue(100)]
    [Style(DisplayStyle.Default)]
    public int Speed { get; set; }

    [Parameter(AbilitySupport = FeatureType.UAVD)]
    [Category(PropertyCategoryNames.Command)]
    [DisplayName("开关控制")]
    [Description("从高到低按位表示开关（1开，0关）或模式切换（1是前一个，0是后一个），顺序是：激光开关，跟踪开关，可见光/热成像，跟踪框开关，识别框开关")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Default)]
    public short OnOff { get; set; }

    #endregion
}