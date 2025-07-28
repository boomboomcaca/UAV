using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualSignalDemodulator;

[DeviceDescription(Name = "信号解调器",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Decoder,
    Version = "1.1.0",
    Model = "VirtualSignalDemodulator",
    MaxInstance = 1,
    FeatureType = FeatureType.SGLDEC,
    Description = "信号解调设备，调用信号精确分析服务。")]
public partial class SignalDemodulator
{
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [Parameter(IsInstallation = true)]
    [DisplayName("信号分析服务IP地址")]
    [Browsable(false)]
    [DefaultValue("127.0.0.1")]
    [Description("信号分析服务IP地址")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IpAddress { get; set; } = "127.0.0.1";

    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [Parameter(IsInstallation = true)]
    [DisplayName("信号分析服务端口")]
    [Browsable(false)]
    [DefaultValue(22001)]
    [Description("信号分析服务端口")]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 22001;

    [Name("exactModeSwitch")]
    [Category(PropertyCategoryNames.Installation)]
    [Parameter(IsInstallation = true)]
    [DisplayName("精确模式开关")]
    [Browsable(false)]
    [DefaultValue(true)]
    [Description("是否开启精确模式")]
    public bool ExactModeSwitch { get; set; } = true;

    [Name("iqFileName")]
    [Category(PropertyCategoryNames.Measurement)]
    [Parameter(AbilitySupport = FeatureType.SGLDEC)]
    [DisplayName("IQ数据文件名称")]
    [Browsable(false)]
    [Description("用于信号解调的IQ数据文件名称")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string IqFileName { get; set; } = string.Empty;

    [Name("analysisMode")]
    [Category(PropertyCategoryNames.Measurement)]
    [Parameter(AbilitySupport = FeatureType.SGLDEC)]
    [DisplayName("分析模式")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|快速|精确",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [Description("信号解调分析模式")]
    [Style(DisplayStyle.Switch)]
    public bool AnalysisMode { get; set; }

    [Name("calculateSymbolRate")]
    [Category(PropertyCategoryNames.Measurement)]
    [Parameter(AbilitySupport = FeatureType.SGLDEC)]
    [DisplayName("计算符号速率")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|计算|不计算",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [Description("是否计算符号速率")]
    public bool CalculateSymbolRate { get; set; }
}