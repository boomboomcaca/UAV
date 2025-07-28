using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC1000PMS;

[DeviceDescription(
    Name = "DC1000PMS",
    DeviceCategory = ModuleCategory.Control,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.1.0",
    Model = "DC1000PMS",
    Description = "DC1000PMS电源与环境控制模块",
    MaxInstance = 1)]
public partial class Dc1000Pms
{
    #region 常规参数

    private SwitchState _wifiSwitch = SwitchState.Off;

    [Parameter]
    [Name("wifiSwitch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("WIFI开关")]
    [Description("设置WIFI开关(WIFI模块存在时有效)")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Radio)]
    public SwitchState WifiSwitch
    {
        get => _wifiSwitch;
        set
        {
            _wifiSwitch = value;
            _modifywifiswitch = true;
        }
    }

    private SwitchState _airconditionSwitch = SwitchState.Off;

    [Parameter]
    [Name("airConditionSwitch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("空调开关")]
    [Description("设置空调开关(空调模块存在时有效)")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AirConditionSwitch
    {
        get => _airconditionSwitch;
        set
        {
            _airconditionSwitch = value;
            _modifyairconditionswitch = true;
        }
    }

    private SwitchState _ac1Switch = SwitchState.On;

    [Parameter]
    [Name("ac1Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关1")]
    [Description("设置交流开关1")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(21)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac1Switch
    {
        get => _ac1Switch;
        set
        {
            _ac1Switch = value;
            _operationAcChannel[0] = true;
            UpdatePowerSwitchSetting(0, true);
        }
    }

    private SwitchState _ac2Switch = SwitchState.On;

    [Parameter]
    [Name("ac2Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关2")]
    [Description("设置交流开关2")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(22)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac2Switch
    {
        get => _ac2Switch;
        set
        {
            _ac2Switch = value;
            _operationAcChannel[1] = true;
            UpdatePowerSwitchSetting(1, true);
        }
    }

    private SwitchState _ac3Switch = SwitchState.On;

    [Parameter]
    [Name("ac3Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关3")]
    [Description("设置交流开关3")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(23)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac3Switch
    {
        get => _ac3Switch;
        set
        {
            _ac3Switch = value;
            _operationAcChannel[2] = true;
            UpdatePowerSwitchSetting(2, true);
        }
    }

    private SwitchState _ac4Switch = SwitchState.On;

    [Parameter]
    [Name("ac4Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关4")]
    [Description("设置交流开关4")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(24)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac4Switch
    {
        get => _ac4Switch;
        set
        {
            _ac4Switch = value;
            _operationAcChannel[3] = true;
            UpdatePowerSwitchSetting(3, true);
        }
    }

    private SwitchState _ac5Switch = SwitchState.On;

    [Parameter]
    [Name("ac5Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关5")]
    [Description("设置交流开关5")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(25)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac5Switch
    {
        get => _ac5Switch;
        set
        {
            _ac5Switch = value;
            _operationAcChannel[4] = true;
            UpdatePowerSwitchSetting(4, true);
        }
    }

    private SwitchState _ac6Switch = SwitchState.On;

    [Parameter]
    [Name("ac6Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关6")]
    [Description("设置交流开关6")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac6Switch
    {
        get => _ac6Switch;
        set
        {
            _ac6Switch = value;
            _operationAcChannel[5] = true;
            UpdatePowerSwitchSetting(5, true);
        }
    }

    private SwitchState _ac7Switch = SwitchState.On;

    [Parameter]
    [Name("ac7Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关7")]
    [Description("设置交流开关7")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(27)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac7Switch
    {
        get => _ac7Switch;
        set
        {
            _ac7Switch = value;
            _operationAcChannel[6] = true;
            UpdatePowerSwitchSetting(6, true);
        }
    }

    private SwitchState _ac8Switch = SwitchState.On;

    [Parameter]
    [Name("ac8Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流开关8")]
    [Description("设置交流开关8")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(28)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Ac8Switch
    {
        get => _ac8Switch;
        set
        {
            _ac8Switch = value;
            _operationAcChannel[7] = true;
            UpdatePowerSwitchSetting(7, true);
        }
    }

    private SwitchState _dc1Switch = SwitchState.On;

    [Parameter]
    [Name("dc1Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关1")]
    [Description("设置直流开关1")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(29)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc1Switch
    {
        get => _dc1Switch;
        set
        {
            _dc1Switch = value;
            _operationDcChannel[0] = true;
            UpdatePowerSwitchSetting(0, false);
        }
    }

    private SwitchState _dc2Switch = SwitchState.On;

    [Parameter]
    [Name("dc2Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关2")]
    [Description("设置直流开关2")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(30)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc2Switch
    {
        get => _dc2Switch;
        set
        {
            _dc2Switch = value;
            _operationDcChannel[1] = true;
            UpdatePowerSwitchSetting(1, false);
        }
    }

    private SwitchState _dc3Switch = SwitchState.On;

    [Parameter]
    [Name("dc3Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关3")]
    [Description("设置直流开关3")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(31)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc3Switch
    {
        get => _dc3Switch;
        set
        {
            _dc3Switch = value;
            _operationDcChannel[2] = true;
            UpdatePowerSwitchSetting(2, false);
        }
    }

    private SwitchState _dc4Switch = SwitchState.On;

    [Parameter]
    [Name("dc4Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关4")]
    [Description("设置直流开关4")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(32)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc4Switch
    {
        get => _dc4Switch;
        set
        {
            _dc4Switch = value;
            _operationDcChannel[3] = true;
            UpdatePowerSwitchSetting(3, false);
        }
    }

    private SwitchState _dc5Switch = SwitchState.On;

    [Parameter]
    [Name("dc5Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关5")]
    [Description("设置直流开关5")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(33)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc5Switch
    {
        get => _dc5Switch;
        set
        {
            _dc5Switch = value;
            _operationDcChannel[4] = true;
            UpdatePowerSwitchSetting(4, false);
        }
    }

    private SwitchState _dc6Switch = SwitchState.On;

    [Parameter]
    [Name("dc6Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关6")]
    [Description("设置直流开关6")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc6Switch
    {
        get => _dc6Switch;
        set
        {
            _dc6Switch = value;
            _operationDcChannel[5] = true;
            UpdatePowerSwitchSetting(5, false);
        }
    }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置设备的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; }

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设置设备的端口号")]
    [DefaultValue(8088)]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    #endregion

    #region 配置参数（开关使能）

    [Parameter(IsInstallation = true)]
    [Name("wifiSwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用WiFi")]
    [Description("设置是否启用WiFi")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Switch)]
    public bool WifiSwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("airConditionSwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用空调")]
    [Description("设置是否启用空调")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Switch)]
    public bool AirConditionSwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac1SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流1")]
    [Description("设置是否启用交流1")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Switch)]
    public bool Ac1SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac2SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流2")]
    [Description("设置是否启用交流2")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Switch)]
    public bool Ac2SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac3SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流3")]
    [Description("设置是否启用交流3")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Switch)]
    public bool Ac3SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac4SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流4")]
    [Description("设置是否启用交流4")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Switch)]
    public bool Ac4SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac5SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流5")]
    [Description("设置是否启用交流5")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(14)]
    [Style(DisplayStyle.Switch)]
    public bool Ac5SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac6SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流6")]
    [Description("设置是否启用交流6")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Switch)]
    public bool Ac6SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac7SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流7")]
    [Description("设置是否启用交流7")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Switch)]
    public bool Ac7SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac8SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用交流8")]
    [Description("设置是否启用交流8")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Switch)]
    public bool Ac8SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc1SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流1")]
    [Description("设置是否启用直流1")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(22)]
    [Style(DisplayStyle.Switch)]
    public bool Dc1SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc2SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流2")]
    [Description("设置是否启用直流2")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(24)]
    [Style(DisplayStyle.Switch)]
    public bool Dc2SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc3SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流3")]
    [Description("设置是否启用直流3")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Switch)]
    public bool Dc3SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc4SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流4")]
    [Description("设置是否启用直流4")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(28)]
    [Style(DisplayStyle.Switch)]
    public bool Dc4SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc5SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流5")]
    [Description("设置是否启用直流5")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(30)]
    [Style(DisplayStyle.Switch)]
    public bool Dc5SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc6SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流6")]
    [Description("设置是否启用直流6")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(32)]
    [Style(DisplayStyle.Switch)]
    public bool Dc6SwitchEnabled { get; set; }

    #endregion

    #region 配置参数（开关名称）

    [Parameter(IsInstallation = true)]
    [Name("wifiSwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("WiFi")]
    [Description("设备WiFi开关名称")]
    [DefaultValue("WiFi")]
    [PropertyOrder(3)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string WifiSwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("airConditionSwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("空调")]
    [Description("设置空调开关名称")]
    [DefaultValue("空调")]
    [PropertyOrder(5)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AirConditionSwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac1SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流1名称")]
    [Description("设置交流1开关的名称")]
    [DefaultValue("交流1")]
    [PropertyOrder(7)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac1SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac2SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流2名称")]
    [Description("设置交流2开关的名称")]
    [DefaultValue("交流2")]
    [PropertyOrder(9)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac2SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac3SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流3名称")]
    [Description("设置交流3开关的名称")]
    [DefaultValue("交流3")]
    [PropertyOrder(11)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac3SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac4SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流4名称")]
    [Description("设置交流4开关的名称")]
    [DefaultValue("交流4")]
    [PropertyOrder(13)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac4SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac5SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流5名称")]
    [Description("设置交流5开关的名称")]
    [DefaultValue("交流5")]
    [PropertyOrder(15)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac5SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac6SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流6名称")]
    [Description("设置交流6开关的名称")]
    [DefaultValue("交流6")]
    [PropertyOrder(17)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac6SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac7SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流7名称")]
    [Description("设置交流7开关的名称")]
    [DefaultValue("交流7")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac7SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("ac8SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("交流8名称")]
    [Description("设置交流8开关的名称")]
    [DefaultValue("交流8")]
    [PropertyOrder(21)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ac8SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc1SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流1名称")]
    [Description("设置直流1开关的名称")]
    [DefaultValue("直流1")]
    [PropertyOrder(23)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc1SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc2SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流2名称")]
    [Description("设置直流2开关的名称")]
    [DefaultValue("直流2")]
    [PropertyOrder(25)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc2SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc3SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流3名称")]
    [Description("设置直流3开关的名称")]
    [DefaultValue("直流3")]
    [PropertyOrder(27)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc3SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc4SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流4名称")]
    [Description("设置直流4开关的名称")]
    [DefaultValue("直流4")]
    [PropertyOrder(29)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc4SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc5SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流5名称")]
    [Description("设置直流5开关的名称")]
    [DefaultValue("直流5")]
    [PropertyOrder(31)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc5SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc6SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流6名称")]
    [Description("设置直流6开关的名称")]
    [DefaultValue("直流6")]
    [PropertyOrder(33)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc6SwitchName { get; set; }

    #endregion
}