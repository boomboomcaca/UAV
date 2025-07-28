using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualEnvironment;

[DeviceDescription(
    Name = "虚拟环境监控设备",
    DeviceCategory = ModuleCategory.Control,
    Manufacturer = "Aleph",
    Version = "2.3.0",
    Model = "VirtualEnvironment",
    Description = "虚拟环境监控设备，内部测试使用",
    MaxInstance = 1,
    FeatureType = FeatureType.None)]
public sealed partial class VirtualEnvironment
{
    #region 运行参数

    [Parameter]
    [Name("wifiSwitch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("WIFI开关")]
    [Description("设置WIFI开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Radio)]
    public SwitchState WifiSwitch { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("airConditionSwitch")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("空调开关")]
    [Description("设置空调开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Radio)]
    public SwitchState AirConditionSwitch { get; set; } = SwitchState.Off;

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
    [Name("dcSwitch1")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流1开关")]
    [Description("设置直流1开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch1 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("dcSwitch2")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流2开关")]
    [Description("设置直流2开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(11)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch2 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("dcSwitch3")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流3开关")]
    [Description("设置直流3开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch3 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("dcSwitch4")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流4开关")]
    [Description("设置直流4开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(13)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch4 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("dcSwitch5")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流5开关")]
    [Description("设置直流5开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(14)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch5 { get; set; } = SwitchState.Off;

    [Parameter]
    [Name("dcSwitch6")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("直流6开关")]
    [Description("设置直流6开关")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|On|Off")]
    [DefaultValue(SwitchState.Off)]
    [PropertyOrder(15)]
    [Style(DisplayStyle.Radio)]
    public SwitchState DcSwitch6 { get; set; } = SwitchState.Off;

    #endregion

    #region 安装参数

    #region 网络配置

    [Parameter(IsInstallation = true)]
    [Name("ipAddress")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置设备的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(0)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("port")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设置设备的端口号")]
    [DefaultValue(5025)]
    [PropertyOrder(1)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; }

    #endregion

    #region 模块启用

    [Parameter(IsInstallation = true)]
    [Name("wifiSwitchEnabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("WIFI模块")]
    [Description("是否配置WIFI模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Switch)]
    public bool WifiEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("airConditionSwitchEnabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("空调模块")]
    [Description("是否配置空调模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Switch)]
    public bool AirConditionEnabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch1Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流1模块")]
    [Description("是否配置交流1模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch1Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch2Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流2模块")]
    [Description("是否配置交流2模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch2Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch3Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流3模块")]
    [Description("是否配置交流3模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch3Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch4Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流4模块")]
    [Description("是否配置交流4模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch4Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch5Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流5模块")]
    [Description("是否配置交流5模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch5Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch6Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("交流6模块")]
    [Description("是否配置交流6模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(9)]
    [Style(DisplayStyle.Switch)]
    public bool AcSwitch6Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch1Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流1模块")]
    [Description("是否配置直流1模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch1Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch2Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流2模块")]
    [Description("是否配置直流2模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(11)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch2Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch3Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流3模块")]
    [Description("是否配置直流3模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch3Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch4Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流4模块")]
    [Description("是否配置直流4模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(13)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch4Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch5Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流5模块")]
    [Description("是否配置直流5模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(14)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch5Enabled { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch6Enabled")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("直流6模块")]
    [Description("是否配置直流6模块")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(15)]
    [Style(DisplayStyle.Switch)]
    public bool DcSwitch6Enabled { get; set; }

    #endregion

    #region 开关名称

    [Parameter(IsInstallation = true)]
    [Name("wifiSwitchName")]
    [Category("配置项")]
    [DisplayName("WIFI开关")]
    [Description("WIFI设备的开关")]
    [DefaultValue("WIFI开关")]
    [PropertyOrder(16)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string WifiSwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("airConditionSwitchName")]
    [Category("配置项")]
    [DisplayName("空调开关")]
    [Description("空调设备的开关")]
    [DefaultValue("空调开关")]
    [PropertyOrder(16)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AirConditionSwitchName { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch1Name")]
    [Category("配置项")]
    [DisplayName("交流1设备")]
    [Description("设备交流通道第一路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流1设备")]
    [PropertyOrder(16)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch1Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch2Name")]
    [Category("配置项")]
    [DisplayName("交流2设备")]
    [Description("设备交流通道第二路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流2设备")]
    [PropertyOrder(17)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch2Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch3Name")]
    [Category("配置项")]
    [DisplayName("交流3设备")]
    [Description("设备交流通道第三路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流3设备")]
    [PropertyOrder(18)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch3Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch4Name")]
    [Category("配置项")]
    [DisplayName("交流4设备")]
    [Description("设备交流通道第四路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流4设备")]
    [PropertyOrder(19)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch4Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch5Name")]
    [Category("配置项")]
    [DisplayName("交流5设备")]
    [Description("设备交流通道第五路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流5设备")]
    [PropertyOrder(20)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch5Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("acSwitch6Name")]
    [Category("配置项")]
    [DisplayName("交流6设备")]
    [Description("设备交流通道第六路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("交流6设备")]
    [PropertyOrder(21)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string AcSwitch6Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch1Name")]
    [Category("配置项")]
    [DisplayName("直流1设备")]
    [Description("设备直流通道第一路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流1设备")]
    [PropertyOrder(22)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch1Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch2Name")]
    [Category("配置项")]
    [DisplayName("直流2设备")]
    [Description("设备直流通道第二路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流2设备")]
    [PropertyOrder(23)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch2Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch3Name")]
    [Category("配置项")]
    [DisplayName("直流3设备")]
    [Description("设备直流通道第三路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流3设备")]
    [PropertyOrder(24)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch3Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch4Name")]
    [Category("配置项")]
    [DisplayName("直流4设备")]
    [Description("设备直流通道第四路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流4设备")]
    [PropertyOrder(25)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch4Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch5Name")]
    [Category("配置项")]
    [DisplayName("直流5设备")]
    [Description("设备直流通道第五路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流5设备")]
    [PropertyOrder(26)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch5Name { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("dcSwitch6Name")]
    [Category("配置项")]
    [DisplayName("直流6设备")]
    [Description("设备直流通道第六路开关对应的设备名称，如“计算机”，“测向机”等")]
    [DefaultValue("直流6设备")]
    [PropertyOrder(27)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string DcSwitch6Name { get; set; }

    #endregion

    #endregion
}