using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMD;

[DeviceDescription(Name = "ESMD",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.AmpDF
                  | FeatureType.TDOA
                  | FeatureType.IFMCA,
    MaxInstance = 1,
    Version = "1.8.6",
    DeviceCapability = "20|3600|20000",
    Model = "ESMD",
    Description = "R&amp;S ESMD接收机，最大中频带宽20MHz，频率最高支持到3.6GHz")]
[DeviceDescription(Name = "ESMD(80)",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.AmpDF
                  | FeatureType.TDOA
                  | FeatureType.IFMCA,
    MaxInstance = 1,
    Version = "1.8.6",
    DeviceCapability = "20|3600|80000",
    Model = "ESMD",
    Description = "R&amp;S ESMD接收机，最大中频带宽80MHz，频率最高支持到3.6GHz")]
[DeviceDescription(Name = "ESMD_SHF",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.AmpDF
                  | FeatureType.TDOA
                  | FeatureType.IFMCA,
    MaxInstance = 1,
    Version = "1.8.5",
    DeviceCapability = "20|26500|20000",
    Model = "ESMD",
    Description = "R&amp;S ESMD接收机，最大中频带宽20MHz，频率最高支持到26.5GHz")]
[DeviceDescription(Name = "ESMD_SHF(80)",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.AmpDF
                  | FeatureType.TDOA
                  | FeatureType.IFMCA,
    MaxInstance = 1,
    Version = "1.8.5",
    Model = "ESMD",
    DeviceCapability = "20|26500|80000",
    Description = "R&amp;S ESMD接收机，最大中频带宽80MHz，频率最高支持到26.5GHz")]
public partial class Esmd
{
    #region 常规参数

    private double _frequency = 101.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.AmpDF
                                | FeatureType.TDOA
                                | FeatureType.IFMCA
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [ValueRange(20.0d, 26500.0d, 6, regexPattern: "_SHF")]
    [DefaultValue(101.7d)]
    [Description("中心频率，默认单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            SendCmd($"SENS:FREQ {_frequency} MHz");
        }
    }

    private double _filterBandwidth = 120.0d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.FScne
                                | FeatureType.MScne
                                | FeatureType.AmpDF
                                | FeatureType.TDOA
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|25|15|12|9|8.333|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1|0.6|0.3|0.15|0.1",
        DisplayValues =
            "|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|25kHz|15kHz|12kHz|9kHz|8.333kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz|600Hz|300Hz|150Hz|100Hz")]
    [StandardValues(RegexPattern = @"\(80\)", IsSelectOnly = true,
        StandardValues =
            "|80000|40000|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|25|15|12|9|8.333|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1|0.6|0.3|0.15|0.1",
        DisplayValues =
            "|80MHz|40MHz|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|25kHz|15kHz|12kHz|9kHz|8.333kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz|600Hz|300Hz|150Hz|100Hz")]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、滤波带宽、解调带宽 默认单位 kHz")]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            SetFilterBandwidth(value);
        }
    }

    private double _ifBandwidth = 200.0d;

    [PropertyOrder(6)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.AmpDF
                                | FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [StandardValues(RegexPattern = @"\(80\)", IsSelectOnly = true,
        StandardValues = "|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues =
            "|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、频谱跨距 默认单位 kHz")]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            SendCmd($"FREQ:SPAN {value}kHz");
        }
    }

    private bool _attCtrlType;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.AmpDF
                                | FeatureType.IFOUT)]
    [Name(ParameterNames.AttCtrlType)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("自动衰减控制")]
    [Description("设置衰减控制的方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [Children($"|{ParameterNames.Attenuation}", false)]
    [Style(DisplayStyle.Switch)]
    [DefaultValue(true)]
    [PropertyOrder(3)]
    public bool AttCtrlType
    {
        get => _attCtrlType;
        set
        {
            _attCtrlType = value;
            if (_attCtrlType)
            {
                SendCmd("INP:ATT:AUTO ON");
            }
            else
            {
                SendCmd("INP:ATT:AUTO OFF");
                SendCmd($"INP:ATT {_attenuation}");
            }
        }
    }

    private int _attenuation = -1;

    [PropertyOrder(9)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.AmpDF
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|40|35|30|25|20|15|10|5|0", //[0,40]
        DisplayValues = "|40|35|30|25|20|15|10|5|0")]
    [DefaultValue(-1)]
    [ValueRange(0, 40)]
    [Unit(UnitNames.Db)]
    [Description("设备衰减 默认单位 dB.")]
    [Style(DisplayStyle.Slider)]
    public int Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (_attenuation.Equals(-1))
            {
                SendCmd("INP:ATT:AUTO ON");
                SendCmd("INP:ATT:AUTO:HOLD:TIME 0");
            }
            else
            {
                SendCmd("INP:ATT:AUTO OFF");
                SendCmd($"INP:ATT {_attenuation}");
            }
        }
    }

    /// <summary>
    ///     属于功能层参数 非设备参数 用于区分PSCAN/FSCAN
    /// </summary>
    [PropertyOrder(4)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|PSCAN|FSCAN")]
    [DefaultValue(ScanMode.Pscan)]
    [Description("扫描模式: 全景扫描或频率扫描")]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    private RfMode _rfMode = RfMode.Normal;

    [PropertyOrder(8)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.AmpDF
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("射频模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort|LowNoise",
        DisplayValues = "|常规|低失真|低噪声")]
    [DefaultValue(RfMode.Normal)]
    [Description("控制射频模块的三种工作模式，常规模式，低失真模式，低噪声模式.")]
    [Style(DisplayStyle.Radio)]
    public RfMode RfMode
    {
        get => _rfMode;
        set
        {
            _rfMode = value;
            SendCmd($"INP:ATT:MODE {_rfMode.ToString().ToUpper()}");
        }
    }

    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [ValueRange(20.0d, 26500.0d, 6, regexPattern: "_SHF")]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [ValueRange(20.0d, 26500.0d, 6, regexPattern: "_SHF")]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进。")]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne,
        Template = typeof(MScanTemplate))]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    [Style(DisplayStyle.Default)]
    [Browsable(false)]
    public Dictionary<string, object>[] MScanPoints { get; set; }

    private Dictionary<string, object>[] _ddcChannels;

    [Name(ParameterNames.DdcChannels)]
    [Parameter(AbilitySupport = FeatureType.IFMCA, Template = typeof(IfmcaTemplate))]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频信道")]
    [Description("设置中频多路参数")]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Default)]
    [Browsable(false)]
    public Dictionary<string, object>[] DdcChannels
    {
        get => _ddcChannels.ToArray();
        set
        {
            _ddcChannels = value;
            if (TaskState == TaskState.Start) SetIfmch(value);
        }
    }

    [PropertyOrder(8)]
    [Name("maxChanCount")]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("子通道数量")]
    [Description("中频多路支持的最大子通道数据量")]
    [DefaultValue(4)]
    [ValueRange(4, 4)]
    [ReadOnly(true)]
    [Style(DisplayStyle.Input)]
    public int MaxChanCount { get; set; } = 4;

    private double _holdTime = 1000d;

    [PropertyOrder(13)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|5000|2000|1000|500|200|100|50|20|10|1|0",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s")]
    [ValueRange(0f, 5000d)]
    [DefaultValue(1000d)]
    [Description("设置保持时间，单位：毫秒（ms）,如果信号在驻留时间内消失，则保持时间开始计时。一旦保持时间过期，扫描将继续下一个频率，即使驻留时间尚未过期。如果信号在保持时间内超过了静噪门限，则保持时间被重置。")]
    [Unit(UnitNames.Ms)]
    [Style(DisplayStyle.Slider)]
    public double HoldTime
    {
        get => _holdTime;
        set
        {
            if (Math.Abs(_holdTime - value) < 1e-9) return;
            _holdTime = value;
            SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime} ms");
        }
    }

    private float _dwellTime;

    [PropertyOrder(13)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等(当带宽较小时频谱数据返回较慢，如果驻留时间设置较短，可能看不到频谱数据)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5|4|3|2|1|0.5|0.2|0.1|0",
        DisplayValues =
            "|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0.000f, 10.000f, 0.1)]
    [DefaultValue(1f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime
    {
        get => _dwellTime;
        set
        {
            if (Math.Abs(_dwellTime - value) < 1e-9) return;
            _dwellTime = value;
            SendCmd($"SENSE:MSCAN:DWELL {_dwellTime}s");
        }
    }

    //TODO: 确认范围
    private int _squelchThreshold = 10;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，单位 dBuV, 当信号电平超过门限时，进行音频解调")]
    [ValueRange(-30, 130)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
        }
    }

    private bool _squelchSwitch = true;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    [Style(DisplayStyle.Switch)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            if (_squelchSwitch)
            {
                SendCmd("OUTPut:SQUelch ON");
                SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
            }
            else
            {
                SendCmd("OUTPut:SQUelch OFF");
            }
        }
    }

    private Modulation _demMode = Modulation.Fm;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|PULSE|PM|IQ|ISB|CW|USB|LSB|TV",
        DisplayValues = "|AM|FM|PULSE|PM|IQ|ISB|CW|USB|LSB|TV")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value;
            SetDemodulation(_demMode);
            if (TaskState == TaskState.Start) StartGetTvBmp();
        }
    }

    private int _updateCount;

    /// <summary>
    ///     UMS300中增加的特殊标记
    ///     在TDOA功能中，UMS300每次发送的TDOA数据包中的时间戳不会变化，
    ///     只有接收到了特殊指令以后才会更新时间戳
    ///     不对外开放
    /// </summary>
    [PropertyOrder(13)]
    [Name("updateCount")]
    [Parameter(AbilitySupport = FeatureType.TDOA)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("TDOA更新")]
    [DefaultValue(0)]
    [Browsable(false)]
    [Description("TDOA更新时间戳")]
    [Style(DisplayStyle.Default)]
    public int UpdateCount
    {
        get => _updateCount;
        set
        {
            _updateCount = value;
            SendCmd("CALC:IFPAN:STEP:AUTO ON");
        }
    }

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    // [ParametersDefault(
    //     new[] { ParameterNames.StartFrequency, ParameterNames.StopFrequency, ParameterNames.StepFrequency },
    //     new object[] { 88d, 108d, 25d },
    //     new object[] { 1000d, 2000d, 25d },
    //     new object[] { 2000d, 3000d, 25d }
    // )]
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

    private int _measureTime;

    [PropertyOrder(15)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.IFMCA | FeatureType.TDOA |
                                FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000000|2000000|1000000|500000|200000|100000|50000|20000|10000|1000|500|0|-1",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|500us|0s|自动")]
    [DefaultValue(0)]
    [Unit(UnitNames.Us)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    [Style(DisplayStyle.Slider)]
    public int MeasureTime
    {
        get => _measureTime;
        set
        {
            if (value == _measureTime) return;
            _measureTime = value;
            SendCmd(value == -1 ? "MEAS:TIME DEF" : $"MEAS:TIME {_measureTime}us");
        }
    }

    private DetectMode _detector = DetectMode.Pos;

    [PropertyOrder(14)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA | FeatureType.SCAN |
                                FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [DefaultValue(DetectMode.Pos)]
    [Description("设置计算电平的处理方式（影响电平数据）")]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector
    {
        get => _detector;
        set
        {
            _detector = value;
            var cmd = string.Empty;
            switch (_detector)
            {
                case DetectMode.Fast:
                    cmd = "FAST";
                    break;
                case DetectMode.Pos:
                    cmd = "POS";
                    break;
                case DetectMode.Avg:
                    cmd = "PAV";
                    break;
                case DetectMode.Rms:
                    cmd = "RMS";
                    break;
            }

            if (cmd != string.Empty) SendCmd($"SENS:DET {cmd}");
        }
    }

    private string _fftMode = "OFF";

    [PropertyOrder(16)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("FFT模式")]
    [Description("设置中频频谱数据取值的方式(影响频谱数据)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|MIN|MAX|SCALar|OFF",
        DisplayValues = "|最小值|最大值|平均值|关闭")]
    [DefaultValue("OFF")]
    [Style(DisplayStyle.Radio)]
    public string FftMode
    {
        get => _fftMode;
        set
        {
            _fftMode = value;
            SendCmd($"CALC:IFPAN:AVER:TYPE {_fftMode}");
        }
    }

    //TODO:
    private string _bandMeasureMode = "XDB";

    [PropertyOrder(17)]
    [Name(ParameterNames.BandMeasureMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("带宽测量模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|XDB|BETA",
        DisplayValues = "|XdB|β%")]
    [DefaultValue("XDB")]
    [Description("ITU带宽测量模式，分为xdB带宽，百分比占用带宽。")]
    [Style(DisplayStyle.Radio)]
    public string BandMeasureMode
    {
        get => _bandMeasureMode;
        set
        {
            _bandMeasureMode = value;
            SendCmd($"MEAS:BAND:MODE {_bandMeasureMode}");
        }
    }

    //TODO:精确到小数点后一位
    private float _xdB = 26.0f;

    [PropertyOrder(18)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 100.0f)]
    [DefaultValue(26.0f)]
    [Unit(UnitNames.Db)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Style(DisplayStyle.Slider)]
    public float XdB
    {
        get => _xdB;
        set
        {
            _xdB = value;
            SendCmd($"MEAS:BAND:XDB {_xdB}");
        }
    }

    //TODO:精确到小数点后一位
    private float _beta = 1.0f;

    [PropertyOrder(19)]
    [Name(ParameterNames.BetaValue)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [ValueRange(0.1f, 99.9f)]
    [DefaultValue(1.0f)]
    [Unit(UnitNames.Pct)]
    [Description("设置ITU测量中β值，单位：%")]
    [Style(DisplayStyle.Slider)]
    public float Beta
    {
        get => _beta;
        set
        {
            _beta = value;
            SendCmd($"MEAS:BAND:BETA {_beta}");
        }
    }

    [Parameter(AbilitySupport = FeatureType.AmpDF)]
    [Name("iqSamplingCount")]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("IQ采样点数")]
    [Description("设置IQ采样点数，值通常是以2为底的幂")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|16384|8192|4096|2048",
        DisplayValues = "|16384|8192|4096|2048")]
    [DefaultValue(4096)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Radio)]
    public int IqSamplingCount { get; set; }

    private bool _iqMode;

    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Name("iqMode")]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("IQ采集模式")]
    [Description("设置IQ的采集模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|连续|默认")]
    [DefaultValue(false)]
    [PropertyOrder(43)]
    [Style(DisplayStyle.Switch)]
    public bool IqMode
    {
        get => _iqMode;
        set
        {
            _iqMode = value;
            if (TaskState == TaskState.Start) SendMediaRequest();
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
    // [PropertyOrder(10)]
    // [Style(DisplayStyle.Switch)]
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

    [PropertyOrder(20)]
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
            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    private bool _ituSwitch;

    [PropertyOrder(21)]
    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM)]
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
            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(22)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FScne
                                | FeatureType.MScne)]
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
            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    private bool _audioSwitch;

    [PropertyOrder(24)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FScne
                                | FeatureType.MScne)]
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
            {
                SendCmd("OUTP:FILT:MODE NOTCH");
                _media |= MediaType.Audio;
            }
            else
            {
                SendCmd("OUTP:FILT:MODE OFF");
                _media &= ~MediaType.Audio;
            }

            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(25)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的网络地址，IPv4格式[x.x.x.x]")]
    [DefaultValue("127.0.0.1")]
    [Style(DisplayStyle.Input)]
    [ValueRange(double.NaN, double.NaN, 100)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(26)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [DefaultValue(5555)]
    [Description("设置连接并控制设备的网络端口号")]
    [Style(DisplayStyle.Slider)]
    [ValueRange(1000, 60000)]
    public int Port { get; set; } = 5555;

    [PropertyOrder(38)]
    [Name("monitorAntenna")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("监测天线端口")]
    [Description("设置监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|1|2",
        DisplayValues = "|自动|VUHF1|VUHF2|VUHF3"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Radio)]
    public int MonitorAntenna { get; set; } = -1;

    [Parameter(IsInstallation = true)]
    [Name("iqWidth")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IQ宽度")]
    [Description("设置IQ采样的位数，分为32位和16位")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|32|16",
        DisplayValues = "|32位|16位")]
    [DefaultValue(32)]
    [PropertyOrder(43)]
    [Style(DisplayStyle.Radio)]
    public int IqWidth { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("levelCalibrationFromIQ")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测量电平校准值")]
    [Description("设置测量电平修正值，通常为负数，用于修正本地通过IQ计算的电平值；例如：IQ算出的电平为100，若设置为-50，则展示的电平将变为50")]
    [ValueRange(-300, 300)]
    [DefaultValue(-80)]
    [PropertyOrder(39)]
    [Style(DisplayStyle.Slider)]
    public int LevelCalibrationFromIq { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("enableLNA")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("低噪放开关")]
    [Description("设置是否启用低噪放大器")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(44)]
    [Style(DisplayStyle.Switch)]
    public bool EnableLna { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("lna")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("放大器值")]
    [Description("设置低噪放的取值，单位为dB")]
    [ValueRange(-80, 80)]
    [DefaultValue(0)]
    [PropertyOrder(45)]
    [Style(DisplayStyle.Slider)]
    public float Lna { get; set; }

    #endregion
}