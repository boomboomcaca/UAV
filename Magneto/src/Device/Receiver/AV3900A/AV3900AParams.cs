using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.AV3900A;

[DeviceDescription(Name = "AV3900A",
    Manufacturer = "北京德辰科技有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.TDOA,
    MaxInstance = 1,
    Version = "1.3.5",
    Model = "AV3900A",
    DeviceCapability = "20|6000|20000",
    Description = "中电科第41所 AV3900A无线电监测接收机(频率范围20MHz~6GHz，最大分析带宽20MHz)")]
public partial class Av3900A
{
    private SegmentTemplate[] _segments;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位MHz")]
    [ValueRange(20, 6000, 6)]
    [Style(DisplayStyle.Input)]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    public double Frequency { get; set; } = 101.7d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [Description("频谱带宽设置(最大20MHz)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz"
    )]
    [DefaultValue(1000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth { get; set; } = 1000d;

    [PropertyOrder(9)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("滤波带宽、解调带宽。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|800|500|300|250|150|120|75|50|20|15|12|10|5|2|1|0.5",
        DisplayValues =
            "|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|20kHz|15kHz|12kHz|10kHz|5kHz|2kHz|1kHz|500Hz"
    )]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 150d;

    [PropertyOrder(11)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("在 0~30 之间线性可调 单位dB。")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|30|25|20|15|10|5|0",
        DisplayValues = "|30dB|25dB|20dB|15dB|10dB|5dB|0dB"
    )]
    [ValueRange(0, 30)]
    [DefaultValue(0f)]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Attenuation { get; set; }

    [PropertyOrder(3)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置频段扫描的起始频率，单位:MHz。")]
    [ValueRange(20.0, 6000.0, 6)]
    [DefaultValue(88.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 88.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描扫描步进.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|6.25|3.125|2.5|1.25|1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|6.25kHz|3.125kHz|2.5kHz|1.25kHz|1kHz")]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置频段扫描的终止频率，单位为MHz。")]
    [ValueRange(20.0, 6000.0, 6)]
    [Style(DisplayStyle.Input)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(24)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Scan)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [Description("设置计算电平数据时的处理方式。")]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(23)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000000|2000000|1000000|500000|200000|100000|50000|20000|10000|1000|500",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|500us")]
    [ValueRange(500, 900000000)]
    [DefaultValue(1000)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    [Unit(UnitNames.Us)]
    [Style(DisplayStyle.Slider)]
    public int MeasureTime { get; set; } = 1000;

    [PropertyOrder(16)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM",
        DisplayValues = "|FM|AM"
    )]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Radio)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [PropertyOrder(34)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch { get; set; }

    [PropertyOrder(29)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [Description("IQ数据开关。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IqSwitch { get; set; }

    [PropertyOrder(32)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] ScanSegments
    {
        get => null;
        set
        {
            if (value == null) return;
            _segments = Array.ConvertAll(value, item => (SegmentTemplate)item);
        }
    }

    #region 安装参数

    [PropertyOrder(35)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("DDF255接收机接收指令的IP地址。")]
    [DefaultValue("172.141.11.202")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "172.141.11.202";

    [PropertyOrder(38)]
    [Name("monitorAntenna")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("监测天线端口")]
    [Description("设置监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1",
        DisplayValues = "|ANTENNA1|ANTENNA2"
    )]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int MonitorAntenna { get; set; } = 0;

    [PropertyOrder(39)]
    [Name("preamp")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("前置放大器")]
    [Description("是否开启前置放大器")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1",
        DisplayValues = "|关闭|开启"
    )]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int Preamp { get; set; } = 0;

    [PropertyOrder(40)]
    [Name("useGPS")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集GPS数据")]
    [Description("设置是否通过接收机采集GPS信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(true)]
    [Style(DisplayStyle.Switch)]
    public bool UseGps { get; set; }

    [PropertyOrder(40)]
    [Name("TdoaTriggerInterval")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("TDOA采集间隔")]
    [Description("设置TDOA采集间隔，单位为毫秒。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|500|200|100|50|20",
        DisplayValues = "|1s|500ms|200ms|100ms|50ms|20ms"
    )]
    [DefaultValue(1000)]
    [Browsable(true)]
    [Style(DisplayStyle.Dropdown)]
    public int TdoaTriggerInterval { get; set; } = 1000;

    [Parameter(IsInstallation = true)]
    [Name("levelCallibrationFromIQ")]
    [DisplayName("测量电平校准值")]
    [Description("设置测量电平修正值，通常为负数，用于修正本地通过IQ计算的电平值；例如：IQ算出的电平为100，若设置为-50，则展示的电平将变为50")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(-300, 300)]
    [DefaultValue(0)]
    [PropertyOrder(38)]
    [Style(DisplayStyle.Slider)]
    public int LevelCalibrationFromIq { get; set; }

    #endregion
}