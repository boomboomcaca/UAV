using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye;

[DeviceDescription(Name = "RFeye",
    Manufacturer = "CRFS",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.AntennaControl,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.TDOA
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.FScne,
    MaxInstance = 1,
    Model = "RFeye",
    Version = "1.4.7",
    DeviceCapability = "20|6000|20000",
    Description = "RFeye接收机,型号20-6")]
public partial class RFeye
{
    #region 常规参数

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(99.6d)]
    [Description("中心频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 99.6d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz")]
    [DefaultValue(200.0d)]
    [Description("中频带宽、频谱带宽、频谱跨距，单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth { get; set; } = 200.0d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz")]
    [DefaultValue(200.0d)]
    [Description("滤波带宽、解调带宽，单位 kHz")]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 200.0d;

    [PropertyOrder(8)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.TDOA |
                                FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40|35|30|25|20|15|10|5|0|-1", //[0,40]
        DisplayValues = "|40|35|30|25|20|15|10|5|0|自动")]
    [DefaultValue(0)]
    [Description("设备衰减 默认单位 dB.")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Dropdown)]
    public int Attenuation { get; set; } = 0;

    [PropertyOrder(9)]
    [Name("timeNumSamples")]
    [Parameter(AbilitySupport = FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("IQ采样点数")]
    [Description("设置IQ采样点数。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|8192|4096|2048|1024|512|256",
        DisplayValues = "|8192|4096|2048|1024|512|256")]
    [DefaultValue(512)]
    [Style(DisplayStyle.Dropdown)]
    public int TimeNumSamples { get; set; } = 512;

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(87.0d)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Description("设置扫描终止频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进，单位kHz。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|500|200|100|50|25|12.5|6.25",
        DisplayValues = "|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz")]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25d;

    //[PropertyOrder(5)]
    //[Name(ParameterNames.ResolutionBandwidth)]
    //[Browsable(false)]
    //[Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    //[Category(PropertyCategoryNames.Scan)]
    //[DisplayName("分辨率带宽")]
    //[Description("设置扫描分辨率带宽，单位Hz。")]
    //[StandardValues(IsSelectOnly = true,
    //    StandardValues = "|1181250|590625|295312|147656|73828|36914|18457|7382|3691|1845|738|369|184|73|36|18",
    //    DisplayValues = "|1.181250MHz|590.625kHz|295.312kHz|147.656kHz|73.828kHz|36.914kHz|18.457kHz|7.382kHz|3.691kHz|1.845kHz|738Hz|369Hz|184Hz|73Hz|36Hz|18Hz")]
    //[DefaultValue(73828)]
    //[Unit(UnitNames.Hz)]
    //[Style(DisplayStyle.Bandwidth)]
    public int ResBandWidthHz { get; set; } = 73828;

    [PropertyOrder(6)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne,
        Template = typeof(MscanTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = null;

    [PropertyOrder(12)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("保持时间")]
    [Description("设置保持时间，单位：秒（s），保持特定时间进行信号搜索，等待信号出现")]
    [ValueRange(0.000f, 10.000f, 0.1f)]
    [DefaultValue(1f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float HoldTime { get; set; } = 1f;

    [PropertyOrder(13)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等")]
    [ValueRange(0.000f, 10.000f, 0.1f)]
    [DefaultValue(1f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime { get; set; } = 1f;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，当信号电平超过门限时，进行音频解调")]
    [ValueRange(-30, 130)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold { get; set; } = 10;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM",
        DisplayValues = "|AM|FM")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Radio)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Browsable(false)]
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

    #endregion

    #region 高级参数

    [PropertyOrder(14)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG",
        DisplayValues = "|快速|峰值|均值")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理频谱的检波方式")]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(15)]
    [Name("sweepNumLoops")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("统计次数")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|32|16|8|4|2",
        DisplayValues = "|32|16|8|4|2")]
    [DefaultValue(2)]
    [Description("用于实现峰值和均值检波, 注意若统计次数设置过多且频段扫描点数也过多时数据返回较缓慢.")]
    [Style(DisplayStyle.Dropdown)]
    public int SweepNumLoops { get; set; } = 2;

    [PropertyOrder(16)]
    [Name("syncMode")]
    [Parameter(AbilitySupport = FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("同步模式")]
    [Description("设置TDOA时钟同步模式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1",
        DisplayValues = "|GPSTime|UnixTime")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int SyncMode { get; set; } = 0;

    [PropertyOrder(17)]
    [Name("sweepRefLevel")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne |
                                FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("参考电平")]
    [Description("扫描时的参考电平，单位为dBm。")]
    [ValueRange(-50, 20)]
    [DefaultValue(-20)]
    [Unit(UnitNames.DBm)]
    [Style(DisplayStyle.Slider)]
    public int SweepRefLevel { get; set; } = -20;

    [PropertyOrder(18)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(3, 60)]
    [DefaultValue(26)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public int XdB { get; set; } = 26;

    #endregion

    #region 数据开关

    private bool _iqSwitch;

    [PropertyOrder(19)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否获取IQ数据")]
    [Style(DisplayStyle.Switch)]
    public bool IqSwitch
    {
        get => _iqSwitch;
        set
        {
            _iqSwitch = value;
            if (value)
                _media |= MediaType.Iq;
            else
                _media &= ~MediaType.Iq;
        }
    }

    private bool _ituSwitch;

    [PropertyOrder(20)]
    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否进行ITU测量，并获取数据")]
    [Style(DisplayStyle.Switch)]
    public bool ItuSwitch
    {
        get => _ituSwitch;
        set
        {
            _ituSwitch = value;
            if (value)
                _media |= MediaType.Itu;
            else
                _media &= ~MediaType.Itu;
        }
    }

    private bool _levelSwicth;

    [PropertyOrder(21)]
    [Name(ParameterNames.LevelSwitch)]
    [Parameter(AbilitySupport = FeatureType.TDOA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否获取电平数据。")]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch
    {
        get => _levelSwicth;
        set
        {
            _levelSwicth = value;
            if (value)
                _media |= MediaType.Level;
            else
                _media &= ~MediaType.Level;
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(22)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA | FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取频谱数据")]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch
    {
        get => _spectrumSwitch;
        set
        {
            _spectrumSwitch = value;
            if (value)
                _media |= MediaType.Spectrum;
            else
                _media &= ~MediaType.Spectrum;
        }
    }

    private bool _audioSwitch;

    [PropertyOrder(23)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否获取音频数据")]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch
    {
        get => _audioSwitch;
        set
        {
            _audioSwitch = value;
            if (value)
                _media |= MediaType.Audio;
            else
                _media &= ~MediaType.Audio;
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(24)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("RFEYE接收机IP地址。")]
    [DefaultValue("192.168.1.90")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.1.90";

    [PropertyOrder(25)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("RFEYE接收机TCP通讯端口号。")]
    [DefaultValue(9999)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 9999;

    [PropertyOrder(26)]
    [Name("gpsSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("GPS数据")]
    [Description("是否从RFeye获取GPS数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool GpsSwitch { get; set; } = false;

    #endregion
}