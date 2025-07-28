using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DemoReceiver;

[DeviceDescription(Name = "演示接收机",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.DirectionFinding,
    Manufacturer = "Aleph",
    Version = "1.10.1.0",
    Model = "DemoReceiver",
    Description = "演示接收机，内部测试使用",
    DeviceCapability = "0.3|8000|40000",
    MaxInstance = 1,
    FeatureType = FeatureType.Ffdf
                  | FeatureType.Ffm
                  | FeatureType.Scan
                  | FeatureType.Tdoa
                  | FeatureType.Wbdf
                  | FeatureType.MScan
                  | FeatureType.Ifmca
                  | FeatureType.Sse
                  | FeatureType.ScanDf
                  | FeatureType.Itum
                  | FeatureType.MScanDf
                  | FeatureType.Dpx)]
public partial class DemoReceiver
{
    #region 常规设置

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Tdoa
                                | FeatureType.Wbdf
                                | FeatureType.Sse
                                | FeatureType.Ifmca
                                | FeatureType.Itum)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [Description("设置监测或测向时被测信号的中心频率，单位：MHz")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    [PropertyOrder(0)]
    public double Frequency { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name("frequencyOffset")]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("频率偏移")]
    [Description("设置监测或测向时被测信号的频率偏移，单位：MHz")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    [PropertyOrder(0)]
    public double FrequencyOffset { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Itum
                                | FeatureType.Tdoa
                                | FeatureType.Sse
                                | FeatureType.Ifmca)]
    [Name(ParameterNames.IfBandwidth)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("中频带宽")]
    [Description("中频带宽、频谱跨距")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|150|120|100|50|20|10|5",
        DisplayValues = "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|10kHz|5kHz")]
    [DefaultValue(500.0d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    [PropertyOrder(1)]
    public double IfBandwidth { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.Sse
                                | FeatureType.ScanDf
                                | FeatureType.Itum
                                | FeatureType.MScanDf)]
    [Name(ParameterNames.RfMode)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("工作模式")]
    [Description("设置接收机射频工作模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowNoise|LowDistort",
        DisplayValues = "|常规|低噪声|低失真")]
    [DefaultValue(RfMode.Normal)]
    [Style(DisplayStyle.Radio)]
    [PropertyOrder(2)]
    public RfMode RfMode { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.MScne
                                | FeatureType.ScanDf
                                | FeatureType.Itum
                                | FeatureType.MScanDf)]
    [Name(ParameterNames.AttCtrlType)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("自动衰减控制")]
    [Description("设置衰减控制的方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [Children($"|{ParameterNames.RfAttenuation}|{ParameterNames.IfAttenuation}", false)]
    [Style(DisplayStyle.Switch)]
    [DefaultValue(true)]
    [PropertyOrder(3)]
    public bool AttCtrlType { get; set; }

    private int _rfAttenuation;

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.Ffm
                                | FeatureType.MScne
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.ScanDf
                                | FeatureType.Itum
                                | FeatureType.MScanDf)]
    [Name(ParameterNames.RfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("射频衰减")]
    [Description("设置射频衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|30|24|18|12|6|0",
        DisplayValues = "|30|24|18|12|6|0")]
    [ValueRange(0, 30, 6)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Db)]
    [DefaultValue(0)]
    [PropertyOrder(4)]
    public int RfAttenuation
    {
        get => _rfAttenuation;
        set
        {
            if (value % 2 != 0) return;
            _rfAttenuation = value;
            AttCtrlType = _rfAttenuation == 0 && _ifAttenuation == 0;
        }
    }

    private int _ifAttenuation;

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.MScne
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.Itum)]
    [Name(ParameterNames.IfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频衰减")]
    [Description("设置中频衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|20|16|12|8|4|0",
        DisplayValues = "|20|16|12|8|4|0")]
    [ValueRange(0, 20)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Db)]
    [DefaultValue(0)]
    [PropertyOrder(6)]
    public int IfAttenuation
    {
        get => _ifAttenuation;
        set
        {
            _ifAttenuation = value;
            AttCtrlType = _rfAttenuation == 0 && _ifAttenuation == 0;
        }
    }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.Ffdf
                                | FeatureType.Sse
                                | FeatureType.Itum)]
    [Name(ParameterNames.FilterBandwidth)]
    [Category(PropertyCategoryNames.Demodulation)]
    [Resident]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|1000|500|250|200|120|100|50|25|12.5|6.25|3.125",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|1MHz|500kHz|250kHz|200kHz|120kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(120.0d)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    [Style(DisplayStyle.Bandwidth)]
    [Unit(UnitNames.KHz)]
    public double FilterBandwidth { get; set; } = 120.0d;

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.Sse)]
    [Name(ParameterNames.DfBandwidth)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|150|120|100|50|20|10|5",
        DisplayValues = "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|10kHz|5kHz")]
    [Style(DisplayStyle.Bandwidth)]
    [DefaultValue(120.0d)]
    [Description("测向带宽")]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(3)]
    public double DfBandwidth { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(87.0d)]
    [Browsable(true)]
    [Style(DisplayStyle.Input)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(0.3d, 8000.0d, 6)]
    [DefaultValue(108.0d)]
    [Browsable(true)]
    [Style(DisplayStyle.Input)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.Scan | FeatureType.ScanDf)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|1000|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|5MHz|1MHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进，单位kHz")]
    [Browsable(true)]
    [Style(DisplayStyle.Dropdown)]
    public double StepFrequency { get; set; } = 25.0d;

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Scan, Template = typeof(SegmentTemplate))]
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

    /// <summary>
    ///     属于功能层参数 非设备参数 用于区分PSCAN/FSCAN
    /// </summary>
    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.Scan)]
    [Name(ParameterNames.ScanMode)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|PSCAN|FSCAN")]
    [DefaultValue(ScanMode.Pscan)]
    [Description("扫描模式: 全景扫描或频率扫描")]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    [Parameter(AbilitySupport = FeatureType.Ffdf)]
    [Name(ParameterNames.DfindMode)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号")]
    [DefaultValue(DFindMode.Normal)]
    [Resident]
    [Description("设置测向模式")]
    [Style(DisplayStyle.Radio)]
    [PropertyOrder(8)]
    public DFindMode DFindMode { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf)]
    [Name("dfSamplingCount")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("采样点数")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2048|1024|512|256|128",
        DisplayValues = "|2048|1024|512|256|128")]
    [DefaultValue(1024)]
    [Description("采样点数，用于控制测向分辨率")]
    [Style(DisplayStyle.Slider)]
    [Unit("个")]
    [PropertyOrder(4)]
    public int DfSamplingCount { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.ScanDf)]
    [Name("avgTimes")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("积分次数")]
    [Description("设置测向积分次数")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|200|150|100|50|10|5|0",
        DisplayValues = "|200|150|100|50|10|5|0")] //695333
    [DefaultValue(5)]
    [Style(DisplayStyle.Slider)]
    [Unit("次")]
    [ValueRange(0, 200)]
    [PropertyOrder(9)]
    public int AvgTimes { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf | FeatureType.Wbdf | FeatureType.ScanDf)]
    [Name(ParameterNames.IntegrationTime)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间，单位：μs")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|10000000|5000000|2000000|1000000|500000|200000|100000|50000|20000|10000|5000|2000|1000|500|200|100|0",
        DisplayValues = "|10s|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|5ms|2ms|1ms|500μs|200μs|100μs|0")]
    // [ValueRange(0, 10000000, 100)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(500)]
    [Unit(UnitNames.Us)]
    [PropertyOrder(18)]
    public int IntegrationTime { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Wbdf
                                | FeatureType.Sse
                                | FeatureType.ScanDf)]
    [Name(ParameterNames.LevelThreshold)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("电平门限")]
    [Description("设置测向电平门限，当信号电平超过门限时返回测向结果")]
    [Style(DisplayStyle.Slider)]
    [ValueRange(-40, 120)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(10)]
    public int LevelThreshold { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.ScanDf
                                | FeatureType.Wbdf)]
    [Name(ParameterNames.QualityThreshold)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，当测向质量超过门限时返回测向结果")]
    [Style(DisplayStyle.Slider)]
    [ValueRange(0, 100)]
    [DefaultValue(40)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(11)]
    public int QualityThreshold { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Itum)]
    [Name(ParameterNames.DemMode)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Description("设置信号音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ|TETRA",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ|TETRA")]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    [PropertyOrder(12)]
    public Modulation DemMode { get; set; }

    [PropertyOrder(15)]
    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Itum
                                | FeatureType.MScne)]
    [Name(ParameterNames.SquelchSwitch)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Style(DisplayStyle.Switch)]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    [DefaultValue(true)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    public bool SquelchSwitch { get; set; } = false;

    [PropertyOrder(15)]
    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Itum
                                | FeatureType.MScne)]
    [Name(ParameterNames.SquelchThreshold)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，当信号电平超过门限时，进行音频解调")]
    [Style(DisplayStyle.Slider)]
    [ValueRange(-40, 100)]
    [Unit(UnitNames.DBuV)]
    [DefaultValue(10)]
    public int SquelchThreshold { get; set; } = -10;

    [PropertyOrder(17)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|5|2|1|0.5|0.2|0.1|0.05|0.02|0.01|0.001|0",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s")]
    [Description("设置等待时间，单位：秒（s），保持特定时间进行信号搜索，等待信号出现")]
    [ValueRange(0.0f, 300.0f, 0.001f)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(0.0f)]
    [Unit(UnitNames.Sec)]
    public double HoldTime { get; set; } = 0.0f;

    [PropertyOrder(18)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne | FeatureType.MScanDf)]
    [Category(PropertyCategoryNames.Scan)]
    [Name(ParameterNames.DwellTime)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|10|7|5|4|3|2|1|0.5|0.2|0.1|0",
        DisplayValues =
            "|10s|7s|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0.0f, 300.0f, 0.1f)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(0.0f)]
    [Unit(UnitNames.Sec)]
    public float DwellTime { get; set; } = 0.0f;

    [PropertyOrder(18)]
    [Name("dfindMethod")]
    [Parameter(AbilitySupport = FeatureType.Ffdf | FeatureType.Sse)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向体制")]
    [Description("设置当前设备的测向体制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ci|sse",
        DisplayValues = "|干涉仪|空间谱")]
    [Browsable(false)]
    [Style(DisplayStyle.Radio)]
    [DefaultValue(DfindMethod.Ci)]
    public DfindMethod DfindMethod { get; set; }

    [PropertyOrder(39)]
    [Name(ParameterNames.DfPolarization)]
    [Parameter(AbilitySupport = FeatureType.Ffdf | FeatureType.Wbdf)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [Description("设置测向极化方式，设备将结合当前频率自动选择合适的天线用于测向")]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化")]
    [DefaultValue(Polarization.Vertical)]
    [Style(DisplayStyle.Radio)]
    public Polarization Polar { get; set; } = Polarization.Vertical;

    #endregion

    #region 高级设置

    [PropertyOrder(19)]
    [Parameter(AbilitySupport = FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Scan
                                | FeatureType.MScan
                                | FeatureType.Itum)]
    [Name(ParameterNames.MeasureTime)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|1000000|500000|200000|100000|50000|20000|10000|1000",
        DisplayValues = "|1s|0.5s|0.2s|0.1s|0.05s|0.02s|0.01s|0.001s")]
    [ValueRange(1000.0f, 1000000.0f, 1000f)]
    [DefaultValue(10000.0f)]
    [Description("设置测量时间，测量时间影响监测测向的结果的准确性")]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Us)]
    public float MeasureTime { get; set; } = 10000.0f;

    [PropertyOrder(20)]
    [Parameter(AbilitySupport = FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Ffdf
                                | FeatureType.Scan
                                | FeatureType.MScan
                                | FeatureType.Itum
                                | FeatureType.MScanDf)]
    [Name(ParameterNames.Detector)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [Children($"|{ParameterNames.MeasureTime}", DetectMode.Avg, DetectMode.Pos, DetectMode.Rms)]
    [Style(DisplayStyle.Radio)]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理电平与频谱的检波方式")]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(21)]
    [Parameter(AbilitySupport = FeatureType.Ffm | FeatureType.Dpx | FeatureType.Itum)]
    [Name(ParameterNames.Xdb)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 120.0f)]
    [DefaultValue(26.0f)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Db)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    public float XdB { get; set; } = 26.0f;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.Ffm | FeatureType.Dpx | FeatureType.Itum)]
    [Name(ParameterNames.BetaValue)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [ValueRange(0.0f, 50.0f)]
    [DefaultValue(0.5f)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Pct)]
    [Description("设置ITU测量中XdB值，单位：%")]
    public float BetaValue { get; set; } = 0.5f;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.Ffdf | FeatureType.Sse)]
    [Name("simAzimuth")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("虚拟示向度")]
    [ValueRange(0.0, 360)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(0)]
    [Unit(UnitNames.Degree)]
    [Description("设置单频测向的模拟示向度，单位°")]
    public int SimAzimuth { get; set; } = 0;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.Ffm | FeatureType.Itum)]
    [Name("simPulse")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("开启脉冲")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(false)]
    [Unit(UnitNames.Degree)]
    [Description("设置单频测向的模拟示向度，单位°")]
    public bool SimPulse { get; set; } = false;

    private Dictionary<string, object>[] _mscanPoints;
    private readonly object _lockMscanPoints = new();

    [Parameter(AbilitySupport = FeatureType.MScan, Template = typeof(MScanTemplate))]
    [Name(ParameterNames.MscanPoints)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    [PropertyOrder(1)]
    public Dictionary<string, object>[] MScanPoints
    {
        get
        {
            lock (_lockMscanPoints)
            {
                return _mscanPoints;
            }
        }
        set
        {
            lock (_lockMscanPoints)
            {
                _index = 0;
                _mscanPoints = value;
            }
        }
    }

    private Dictionary<string, object>[] _mscandfPoints;

    [Parameter(AbilitySupport = FeatureType.MScanDf, Template = typeof(MScanDfTemplate))]
    [Name(ParameterNames.MscanDfPoints)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("离散测向频点")]
    [Description("设置离散测向频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    [PropertyOrder(1)]
    public Dictionary<string, object>[] MScanDfPoints
    {
        get => _mscandfPoints;
        set
        {
            _mscandfPoints = value;
            _index = 0;
        }
    }

    #endregion

    #region 数据开关

    // private bool _dwellSwitch = false;
    // [Parameter(AbilitySupport = FeatureType.MScan)]
    // [Name(ParameterNames.DwellSwitch)]
    // [Category(PropertyCategoryNames.DataSwitch)]
    // [DisplayName("驻留开关")]
    // [Description("切换离散扫描与驻留离散扫描")]
    // [StandardValues(IsSelectOnly = true,
    //     StandardValues = "|true|false",
    //     DisplayValues = "|开|关")]
    // [DefaultValue(false)]
    // [Style(DisplayStyle.Switch)]
    // [PropertyOrder(10)]
    // public bool DwellSwitch
    // {
    //     get
    //     {
    //         return _dwellSwitch;
    //     }
    //     set
    //     {
    //         _dwellSwitch = value;
    //     }
    // }
    private bool _iqSwitch;

    [PropertyOrder(24)]
    [Parameter(AbilitySupport = FeatureType.Ffm)]
    [Name(ParameterNames.IqSwitch)]
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

    [PropertyOrder(25)]
    [Parameter(AbilitySupport = FeatureType.Ffm | FeatureType.Itum)]
    [Name(ParameterNames.ItuSwitch)]
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

    private bool _audioSwitch;

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Sse
                                | FeatureType.Itum
                                | FeatureType.MScan)]
    [Name(ParameterNames.AudioSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取音频数据")]
    [PropertyOrder(22)]
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

    private bool _levelSwitch;

    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Tdoa)]
    [Name(ParameterNames.LevelSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置接收机是否输出电平")]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch
    {
        get => _levelSwitch;
        set
        {
            _levelSwitch = value;
            if (value)
                _media |= MediaType.Level;
            else
                _media &= ~MediaType.Level;
        }
    }

    private bool _spectrumSwitch;

    [PropertyOrder(27)]
    [Parameter(AbilitySupport = FeatureType.Ffdf
                                | FeatureType.Ffm
                                | FeatureType.Dpx
                                | FeatureType.Tdoa
                                | FeatureType.Sse
                                | FeatureType.Itum
                                | FeatureType.MScan)]
    [Name(ParameterNames.SpectrumSwitch)]
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

    #endregion

    #region 中频多路参数

    private Dictionary<string, object>[] _ifMultiChannels;
    private IfMultiChannelTemplate[] _ifMultiChannelsArray;

    [PropertyOrder(7)]
    [Parameter(AbilitySupport = FeatureType.Ifmca, Template = typeof(IfMultiChannelTemplate))]
    [Category(PropertyCategoryNames.Misc)]
    [Name(ParameterNames.DdcChannels)]
    [DisplayName("测量信道")]
    [Description("设置中频多路参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] IfMultiChannels
    {
        get => _ifMultiChannels;
        set
        {
            lock (_lockChannel)
            {
                _ifMultiChannels = value;
                if (value != null)
                {
                    _ifMultiChannelsArray = Array.ConvertAll(value, item => (IfMultiChannelTemplate)item);
                    _mchEvent?.Set();
                }
                else
                {
                    _ifMultiChannelsArray = null;
                }
            }
        }
    }

    [PropertyOrder(8)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.Misc)]
    [Name("maxChanCount")]
    [ReadOnly(true)]
    [Browsable(false)]
    [DisplayName("窄带数量")]
    [Description("最多支持的窄带通道数。")]
    [ValueRange(32, 32)]
    [DefaultValue(32)]
    [Style(DisplayStyle.Input)]
    public int MaxChanCount { get; set; }

    [PropertyOrder(8)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.Misc)]
    [ReadOnly(true)]
    [Name("maxAudioCount")]
    [Browsable(false)]
    [DisplayName("音频数量")]
    [Description("设置窄带最多支持的音频通道数。")]
    [ValueRange(32, 32)]
    [DefaultValue(32)]
    [Style(DisplayStyle.Input)]
    public int MaxAudioCount { get; set; }

    #endregion

    #region 空间谱模拟参数

    private int _sseAzimuthCount = 1;

    [PropertyOrder(23)]
    [Parameter(AbilitySupport = FeatureType.Sse)]
    [Name("estimatedSSECount")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("示向度估计数量")]
    [Description("设置空间谱测向时预估的示向度数量")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|9|8|7|6|5|4|3|2|1|-1",
        DisplayValues = "|9|8|7|6|5|4|3|2|1|自动")]
    [DefaultValue(1)]
    [Style(DisplayStyle.Slider)]
    public int SseAzimuthCount
    {
        get => _sseAzimuthCount;
        set
        {
            if (value == 0)
                _sseAzimuthCount = 1;
            else if (value == -1)
                _sseAzimuthCount = 1;
            else
                _sseAzimuthCount = value;
        }
    }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设备的IP地址")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 16)]
    [Style(DisplayStyle.Input)]
    [PropertyOrder(26)]
    public string IpAddress { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设备连接的端口号")]
    [ValueRange(1000, 60000)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(1720)]
    [PropertyOrder(27)]
    public int Port { get; set; } = 1720;

    [Parameter(IsInstallation = true)]
    [Name("tdoaDataPath")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("文件路径")]
    [DefaultValue("")]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    [Description("TDOA数据文件路径")]
    public string TdoaDataPath { get; set; } = "";

    [Parameter(IsInstallation = true)]
    [Name("asRealDeviceSocket")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("真实设备模拟")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("是否模拟真实设备的网络状况")]
    [Style(DisplayStyle.Switch)]
    public bool AsRealDevice { get; set; } = false;

    [Parameter(IsInstallation = true)]
    [Name("address")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装经纬度")]
    [Description("如果没有统一的地理位置信息，可以在此手工设置,格式：经度,纬度")]
    [DefaultValue("104.061,30.63202")]
    [Style(DisplayStyle.Input)]
    public string Address { get; set; }

    #endregion
}

/// <summary>
///     离散扫描模板
/// </summary>
public class MScanTemplate
{
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [Description("设置离散频点中心频率")]
    [ValueRange(20.0d, 6000.0d, 0.000001)]
    [DefaultValue(101.7d)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(1)]
    public double Frequency { get; set; }

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.FilterBandwidth)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("滤波带宽")]
    [Description("设置离散扫描中频滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|500|300|250|150|120|50|30|15|12|9|6|2.4|1.5",
        DisplayValues = "|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|2.4kHz|1.5kHz")]
    [Style(DisplayStyle.Bandwidth)]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(2)]
    public double FilterBandwidth { get; set; }

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.DemMode)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Description("设置离散信号音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ")]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    [PropertyOrder(4)]
    public Modulation DemMode { get; set; }

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.MeasureThreshold)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("测量门限")]
    [Description("获取或设置离散扫描进行占用度测量的门限值")]
    [ValueRange(-40, 120)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(20)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(10)]
    public int MeasureThreshold { get; set; } = 0;
}

/// <summary>
///     中频多路分析子通道类模板
/// </summary>
internal class IfMultiChannelTemplate
{
    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [Name(ParameterNames.Frequency)]
    [Description("设置中频多路子通道中心频率，单位MHz")]
    [ValueRange(0.3d, 8000.0d, 0.000001)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(102.6d)]
    public double Frequency { get; set; } = 102.6d;

    [PropertyOrder(1)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.Demodulation)]
    [ReadOnly(true)]
    [Name(ParameterNames.FilterBandwidth)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [Unit(UnitNames.KHz)]
    [DefaultValue(150d)]
    [Description("设置中频多路子通道的滤波带宽。")]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth { get; set; } = 150.0d;

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Name(ParameterNames.DemMode)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|LSB|USB|CW|PM",
        DisplayValues = "|AM|FM|LSB|USB|CW|PM")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.AudioSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch { get; set; } = true;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.LevelSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取电平数据")]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch { get; set; } = false;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.SpectrumSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取频谱数据")]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch { get; set; } = false;

    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name("ifSwitch")]
    [DisplayName("中频输出")]
    [Description("设置是否输出子通道中频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool IfSwitch { get; set; } = false;

    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Name(ParameterNames.UnitSelection)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("单位选择")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|dBμV|dBμV/m|dBm")]
    [Description("单位选择")]
    [DefaultValue(0)]
    [PropertyOrder(35)]
    [Style(DisplayStyle.Radio)]
    public int UnitSelection { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Name(ParameterNames.MaximumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最大值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最大值")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(30)]
    public bool MaximumSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Name(ParameterNames.MinimumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最小值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最小值")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(31)]
    public bool MinimumSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Name(ParameterNames.MeanSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱平均值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的平均值")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(32)]
    public bool MeanSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.Ifmca)]
    [Name(ParameterNames.NoiseSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱噪声显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的噪声")]
    [DefaultValue(false)]
    [Browsable(false)]
    [Style(DisplayStyle.Switch)]
    [PropertyOrder(33)]
    public bool NoiseSwitch { get; set; }

    public static explicit operator IfMultiChannelTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new IfMultiChannelTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.ContainsKey(name))
                {
                    object objValue;
                    if (property.PropertyType.IsEnum)
                        objValue = Utils.ConvertStringToEnum(dict[name].ToString(), property.PropertyType);
                    else if (property.PropertyType == typeof(Guid))
                        objValue = Guid.Parse(dict[name].ToString() ?? string.Empty);
                    else if (property.PropertyType.IsValueType)
                        objValue = Convert.ChangeType(dict[name], property.PropertyType);
                    else
                        objValue = dict[name]; //Convert.ChangeType(value, prop.PropertyType);
                    property.SetValue(template, objValue, null);
                }
            }
        }
        catch
        {
            // 容错代码
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                    continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}

/// <summary>
///     频率表扫描测向模板类
/// </summary>
public class MScanDfTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScanDf)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("被测频率")]
    [ValueRange(0.3, 8000, 6)]
    [Style(DisplayStyle.Input)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(101.7d)]
    [Description("被测频率，默认单位MHz")]
    public double Frequency { get; set; } = 101.7;

    [PropertyOrder(2)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScanDf)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|1000|500|250|200|120|100|50|25|12.5|6.25|3.125",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|1MHz|500kHz|250kHz|200kHz|120kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(120.0d)]
    [Style(DisplayStyle.Bandwidth)]
    [Unit(UnitNames.KHz)]
    [Description("默认单位 kHz")]
    public double DfBandWidth { get; set; } = 120.0d;

    [PropertyOrder(13)]
    [Name(ParameterNames.QualityThreshold)]
    [Parameter(AbilitySupport = FeatureType.MScanDf)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，仅当测向质量超过门限时才返回测向数据")]
    [Unit(UnitNames.Pct)]
    [ValueRange(0, 100)]
    [Style(DisplayStyle.Slider)]
    [DefaultValue(10)]
    public int QualityThreshold { get; set; } = 10;
}

public class SegmentTemplate
{
    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(0.3d, 8000.0d)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(0.3d, 8000.0d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.Scan
                                | FeatureType.Nsic
                                | FeatureType.Ese
                                | FeatureType.Emdc
                                | FeatureType.Emda)]
    [Name("stepFrequency")]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|1000|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|5MHz|1MHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [ValueRange(3.125d, 5000.0d)]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进，单位kHz")]
    public double StepFrequency { get; set; } = 25.0d;

    public static explicit operator SegmentTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new SegmentTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.TryGetValue(name, out var value)) property.SetValue(template, value, null);
            }
        }
        catch
        {
            // 容错代码
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                    continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}