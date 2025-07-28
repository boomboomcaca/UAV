using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EthSPDTSwitchArray;

[DeviceDescription(Name = "网口单刀双掷开关",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.SwitchArray,
    FeatureType = FeatureType.None,
    MaxInstance = 1,
    Model = "EthSPDTSwitchArray",
    Version = "1.0.0",
    Description = "适用于监测管制二选一打通方式的开关切换阵列"
)]
public partial class EthSpdtSwitchArray
{
    private string _rmdCode;
    private bool _rmsSwitch;
    private string _rsdCode;

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("地址")]
    [Description("设置连接到设备的网络地址。IPv4格式：XXX.XXX.XXX.XXX")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设备的网络端口号。")]
    [ValueRange(1024, 65535)]
    [DefaultValue(5555)]
    [PropertyOrder(1)]
    public int Port { get; set; } = 5555;

    [Parameter(IsInstallation = true)]
    [Name("rmdCode")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("监测控制码")]
    [Description("设置监测设备开关打通码，以“|”分隔，如:0x01|0x02|0x03")]
    [PropertyOrder(2)]
    [DefaultValue("0x01")]
    public string RmdCode
    {
        get => _rmdCode;
        set
        {
            _rmdCode = value;
            var switchIndex = AddSwitch(SwitchUsage.RadioMonitoring, nameof(RmdCode), value);
            _switchUsageTable[SwitchUsage.RadioMonitoring] = switchIndex;
        }
    }

    [Parameter(IsInstallation = true)]
    [Name("rsdCode")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("管制控制码")]
    [Description("设置管制设备开关打通码，以“|”分隔，如:0x01|0x02|0x03")]
    [PropertyOrder(2)]
    [DefaultValue("0x02")]
    public string RsdCode
    {
        get => _rsdCode;
        set
        {
            _rsdCode = value;
            var switchIndex = AddSwitch(SwitchUsage.RadioSuppressing, nameof(RsdCode), value);
            _switchUsageTable[SwitchUsage.RadioSuppressing] = switchIndex;
        }
    }

    [Parameter]
    [Name(ParameterNames.RmsSwitch)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("管制开关")]
    [Description("设置是否打通无线电管制设备的开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(2)]
    public bool RmsSwitch
    {
        get => _rmsSwitch;
        set
        {
            _rmsSwitch = value;
            if (value)
                RaiseSwitchChangeNotification(_switchUsageTable[SwitchUsage.RadioSuppressing]);
            else
                RaiseSwitchChangeNotification(_switchUsageTable[SwitchUsage.RadioMonitoring]);
        }
    }
}