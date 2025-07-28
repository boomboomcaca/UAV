using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC1020PMS;

[DeviceDescription(
    Name = "DC1020PMS",
    DeviceCategory = ModuleCategory.Control,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.1.0",
    Model = "DC1020PMS",
    Description = "DC1020PMS电源与环境控制模块",
    MaxInstance = 1)]
public partial class Dc1020Pms
{
    #region 常规参数

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
            UpdateSwitchStatus(0, true);
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
            UpdateSwitchStatus(1, true);
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
            UpdateSwitchStatus(2, true);
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
            UpdateSwitchStatus(3, true);
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
    [PropertyOrder(25)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc1Switch
    {
        get => _dc1Switch;
        set
        {
            _dc1Switch = value;
            _operationDcChannel[0] = true;
            UpdateSwitchStatus(0, false);
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
    [PropertyOrder(26)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc2Switch
    {
        get => _dc2Switch;
        set
        {
            _dc2Switch = value;
            _operationDcChannel[1] = true;
            UpdateSwitchStatus(1, false);
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
    [PropertyOrder(27)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc3Switch
    {
        get => _dc3Switch;
        set
        {
            _dc3Switch = value;
            _operationDcChannel[2] = true;
            UpdateSwitchStatus(2, false);
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
    [PropertyOrder(28)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc4Switch
    {
        get => _dc4Switch;
        set
        {
            _dc4Switch = value;
            _operationDcChannel[3] = true;
            UpdateSwitchStatus(3, false);
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
    [PropertyOrder(29)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc5Switch
    {
        get => _dc5Switch;
        set
        {
            _dc5Switch = value;
            _operationDcChannel[4] = true;
            UpdateSwitchStatus(4, false);
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
    [PropertyOrder(30)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc6Switch
    {
        get => _dc6Switch;
        set
        {
            _dc6Switch = value;
            _operationDcChannel[5] = true;
            UpdateSwitchStatus(5, false);
        }
    }

    private SwitchState _dc7Switch = SwitchState.On;

    [Parameter]
    [Name("dc7Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关7")]
    [Description("设置直流开关7")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(31)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc7Switch
    {
        get => _dc7Switch;
        set
        {
            _dc7Switch = value;
            _operationDcChannel[6] = true;
            UpdateSwitchStatus(6, false);
        }
    }

    private SwitchState _dc8Switch = SwitchState.On;

    [Parameter]
    [Name("dc8Switch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流开关8")]
    [Description("设置直流开关8")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.On)]
    [PropertyOrder(32)]
    [Style(DisplayStyle.Radio)]
    public SwitchState Dc8Switch
    {
        get => _dc8Switch;
        set
        {
            _dc8Switch = value;
            _operationDcChannel[7] = true;
            UpdateSwitchStatus(7, false);
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

    #region 配置参数（使能）

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
    [Name("dc1SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流1")]
    [Description("设置是否启用直流1")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(14)]
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
    [PropertyOrder(16)]
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
    [PropertyOrder(18)]
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
    [PropertyOrder(20)]
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
    [PropertyOrder(22)]
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
    [PropertyOrder(24)]
    [Style(DisplayStyle.Switch)]
    public bool Dc6SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc7SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流7")]
    [Description("设置是否启用直流7")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Switch)]
    public bool Dc7SwitchEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc8SwitchEnabled")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("启用直流8")]
    [Description("设置是否启用直流8")]
    [StandardValues(
        IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(28)]
    [Style(DisplayStyle.Switch)]
    public bool Dc8SwitchEnabled { get; set; }

    #endregion

    #region 配置参数（开关名称）

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
    [Name("dc1SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流1名称")]
    [Description("设置直流1开关的名称")]
    [DefaultValue("直流1")]
    [PropertyOrder(15)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc1SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc2SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流2名称")]
    [Description("设置直流2开关的名称")]
    [DefaultValue("直流2")]
    [PropertyOrder(17)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc2SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc3SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流3名称")]
    [Description("设置直流3开关的名称")]
    [DefaultValue("直流3")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc3SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc4SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流4名称")]
    [Description("设置直流4开关的名称")]
    [DefaultValue("直流4")]
    [PropertyOrder(21)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc4SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc5SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流5名称")]
    [Description("设置直流5开关的名称")]
    [DefaultValue("直流5")]
    [PropertyOrder(23)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc5SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc6SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流6名称")]
    [Description("设置直流6开关的名称")]
    [DefaultValue("直流6")]
    [PropertyOrder(25)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc6SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc7SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流7名称")]
    [Description("设置直流7开关的名称")]
    [DefaultValue("直流7")]
    [PropertyOrder(27)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc7SwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dc8SwitchName")]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("直流8名称")]
    [Description("设置直流8开关的名称")]
    [DefaultValue("直流8")]
    [PropertyOrder(29)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Dc8SwitchName { get; set; }

    #endregion
}