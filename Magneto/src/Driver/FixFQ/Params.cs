using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace FixFQ;

[DriverDescription(
    Name = "单频测量",
    MaxInstance = 8,
    Category = ModuleCategory.Monitoring,
    Version = "1.10.1.0",
    MediaType = MediaType.Audio | MediaType.Iq | MediaType.Itu | MediaType.Level | MediaType.Spectrum | MediaType.Tdoa,
    FeatureType = FeatureType.Ffm,
    Description = "单频测量功能",
    IsMonopoly = false)]
public partial class FixFq
{
    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.Ffm)]
    [Name("receiver")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Monitoring,
        IsPrimaryDevice = true,
        NeedFeature = FeatureType.Ffm)]
    [Description("提供监测数据的主设备")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice Receiver { get; set; }

    [Parameter(IsInstallation = true, AbilitySupport = FeatureType.Ffm)]
    [Name("decoder")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("解码器")]
    [Module(NeedEquip = true,
        NeedModule = ModuleCategory.Decoder,
        NeedFeature = FeatureType.Ese)]
    [Description("进行信号解调解码的设备或服务")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public object Decoder { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("antennaController")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制器")]
    [Module(NeedModule = ModuleCategory.AntennaControl,
        NeedFeature = FeatureType.None,
        NeedEquip = true)]
    [Description("使用的天线控制器，实现天线的逻辑控制")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public IDevice AntennaController { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("iqSaveLenConfig")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信号解调采集长度字典")]
    [DefaultValue("500|163356;10000|134055;20000|5362157;40000|5368710")]
    [Description("不同带宽下采集IQ数据的点数，带宽单位为kHz，带宽与点数之间用|分隔，不同带宽点数之间用英文分号分隔，格式为：带宽1|点数1;带宽2|点数2;带宽3|点数3")]
    [Browsable(false)]
    [ValueRange(double.NaN, double.NaN, 60)]
    [Style(DisplayStyle.Input)]
    public string IqSaveLenConfig { get; set; } = "500|163356;10000|134055;20000|5362157;40000|5368710";

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("mrSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("调制识别开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启调制识别统计")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(34)]
    public bool MrSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("asrSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("语音识别开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启语音识别")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(34)]
    public bool AsrSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("capture")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信号捕获")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|停止捕获|开始捕获|捕获成功")]
    [Description("设置信号捕获的模式")]
    [DefaultValue(0)]
    [PropertyOrder(35)]
    [Browsable(false)]
    [Style(DisplayStyle.Radio)]
    public int Capture { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("captureThreshold")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量门限")]
    [Description("设置信号捕获的测量门限")]
    [ValueRange(-30, 120)]
    [PropertyOrder(34)]
    [Unit(UnitNames.DBuV)]
    [Browsable(false)]
    [Style(DisplayStyle.Slider)]
    public int[] CaptureThreshold { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("measureSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量开关")]
    [Description("设置进行占用度测量的门限开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [PropertyOrder(34)]
    [DefaultValue(false)]
    [Browsable(false)]
    [Style(DisplayStyle.Switch)]
    public bool MeasureSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("iqRawSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("IQ数据存储开关")]
    [Description("设置IQ数据存储的开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|关|开|取消")]
    [PropertyOrder(34)]
    [DefaultValue(0)]
    [Browsable(false)]
    [Style(DisplayStyle.Switch)]
    public int IqRawSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name(ParameterNames.MeasureThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量门限")]
    [Description("设置进行占用度测量的门限")]
    [ValueRange(-30, 120)]
    [PropertyOrder(34)]
    [DefaultValue(0)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    [Browsable(false)]
    public int MeasureThreshold { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("iqRecordSwitch")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("信号解调数据采集开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("是否开启信号解调数据采集")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(34)]
    [Browsable(false)]
    public bool IqRecordSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("iqCalibrationValue")]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("IQ校准值")]
    [DefaultValue(1073741824.0d)]
    [Description("用于接收机IQ的校准值，这个值每台接收机必须根据实际情况计算并重配。")]
    [ValueRange(0, 107374182400)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    [PropertyOrder(35)]
    public double IqCalibrationValue { get; set; } = 1073741824.0d;
}