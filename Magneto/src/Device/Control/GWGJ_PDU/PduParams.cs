using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.GWGJ_PDU;

[DeviceDescription(Name = "GWGJ_PDU",
    DeviceCategory = ModuleCategory.Control,
    Manufacturer = "深圳特普瑞斯科技有限公司",
    Version = "1.3.0",
    Model = "XY-G10-G",
    Description = "智能PDU插座设备",
    MaxInstance = 1,
    FeatureType = FeatureType.None)]
public partial class Pdu
{
    #region 运行参数

    [Parameter]
    [Name("acSwitch1")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流1开关")]
    [Description("设置交流1开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch1 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch2")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流2开关")]
    [Description("设置交流2开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch2 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch3")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流3开关")]
    [Description("设置交流3开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch3 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch4")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流4开关")]
    [Description("设置交流4开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch4 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch5")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流5开关")]
    [Description("设置交流5开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch5 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch6")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流6开关")]
    [Description("设置交流6开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(9)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch6 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch7")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流7开关")]
    [Description("设置交流7开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch7 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("acSwitch8")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("交流8开关")]
    [Description("设置交流8开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(11)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AcSwitch8 { get; set; } = SwitchState.Off;

    #endregion

    #region 网络配置

    [Parameter(IsInstallation = true)]
    [Name("ipAddress")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置设备的IP地址")]
    [DefaultValue("192.168.0.163")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; } = "192.168.0.100";

    [Parameter(IsInstallation = true)]
    [Name("port")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设置设备的端口号")]
    [DefaultValue(4600)]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 4600;

    [Parameter(IsInstallation = true)]
    [Name("loginUser")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("登录账号")]
    [Description("设置设备的登录账号")]
    [DefaultValue("admin")]
    [PropertyOrder(1)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string LoginUser { get; set; } = "admin";

    [Parameter(IsInstallation = true)]
    [Name("loginPwd")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("登录密码")]
    [Description("设置设备的登录密码")]
    [DefaultValue("admin")]
    [PropertyOrder(1)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string LoginPwd { get; set; } = "admin";

    #endregion

    #region 模块启用

    [Parameter(IsInstallation = true)]
    [Name("temperatureAvailable")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("温度模块")]
    [Description("是否配置温度模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Switch)]
    public bool TemperatureAvailable { get; set; } = false;

    [Parameter(IsInstallation = true)]
    [Name("acSwitch1Enabled")]
    [Category("配置项")]
    [DisplayName("交流1设备")]
    [Description("是否配置设备交流通道第一路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch1Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch2Enabled")]
    [Category("配置项")]
    [DisplayName("交流2设备")]
    [Description("是否配置设备交流通道第二路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(17)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch2Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch3Enabled")]
    [Category("配置项")]
    [DisplayName("交流3设备")]
    [Description("是否配置设备交流通道第三路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch3Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch4Enabled")]
    [Category("配置项")]
    [DisplayName("交流4设备")]
    [Description("是否配置设备交流通道第四路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch4Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch5Enabled")]
    [Category("配置项")]
    [DisplayName("交流5设备")]
    [Description("是否配置设备交流通道第五路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch5Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch6Enabled")]
    [Category("配置项")]
    [DisplayName("交流6设备")]
    [Description("是否配置设备交流通道第六路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch6Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch7Enabled")]
    [Category("配置项")]
    [DisplayName("交流7设备")]
    [Description("是否配置设备交流通道第七路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch7Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch8Enabled")]
    [Category("配置项")]
    [DisplayName("交流8设备")]
    [Description("是否配置设备交流通道第八路开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(true)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch8Enabled { get; set; }

    #endregion

    #region 开关名称

    [Parameter(IsInstallation = true)]
    [Name("acSwitch1Name")]
    [Category("配置项")]
    [DisplayName("交流1设备")]
    [Description("设备交流通道第一路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流1设备")]
    [PropertyOrder(16)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch1Name { get; set; } = "交流1设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch2Name")]
    [Category("配置项")]
    [DisplayName("交流2设备")]
    [Description("设备交流通道第二路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流2设备")]
    [PropertyOrder(17)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch2Name { get; set; } = "交流2设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch3Name")]
    [Category("配置项")]
    [DisplayName("交流3设备")]
    [Description("设备交流通道第三路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流3设备")]
    [PropertyOrder(18)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch3Name { get; set; } = "交流3设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch4Name")]
    [Category("配置项")]
    [DisplayName("交流4设备")]
    [Description("设备交流通道第四路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流4设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch4Name { get; set; } = "交流4设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch5Name")]
    [Category("配置项")]
    [DisplayName("交流5设备")]
    [Description("设备交流通道第五路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流5设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch5Name { get; set; } = "交流5设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch6Name")]
    [Category("配置项")]
    [DisplayName("交流6设备")]
    [Description("设备交流通道第六路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流6设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch6Name { get; set; } = "交流6设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch7Name")]
    [Category("配置项")]
    [DisplayName("交流7设备")]
    [Description("设备交流通道第七路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流7设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch7Name { get; set; } = "交流7设备";

    [Parameter(IsInstallation = true)]
    [Name("acSwitch8Name")]
    [Category("配置项")]
    [DisplayName("交流8设备")]
    [Description("设备交流通道第八路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流8设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch8Name { get; set; } = "交流8设备";

    #endregion

    #region 配置

    [Parameter(IsInstallation = true)]
    [Name("voltageMax")]
    [Category("配置项")]
    [DisplayName("电压上限")]
    [Description("电压的上限值")]
    [DefaultValue(240)]
    [ValueRange(200, 260)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Slider)]
    public int VoltageMax { get; set; } = 240;

    [Parameter(IsInstallation = true)]
    [Name("currentMax")]
    [Category("配置项")]
    [DisplayName("电流上限")]
    [Description("电流的上限值")]
    [DefaultValue(12)]
    [ValueRange(0, 16)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Slider)]
    public int CurrentMax { get; set; } = 12;

    [Parameter(IsInstallation = true)]
    [Name("temperatureMax")]
    [Category("配置项")]
    [DisplayName("温度上限")]
    [Description("温度的上限值")]
    [DefaultValue(50)]
    [ValueRange(-40, 100)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Slider)]
    public int TemperatureMax { get; set; } = 50;

    #endregion
}