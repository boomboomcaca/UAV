using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF255;

[DeviceDescription(Name = "DDF255",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFM
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.SCAN
                  | FeatureType.FFDF
                  | FeatureType.WBDF
                  | FeatureType.FScne
                  | FeatureType.AmpDF
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Version = "2.1.0",
    DeviceCapability = "20|26500|80000",
    Model = "DDF255",
    Description = "R&amp;S DDF255监测测向接收机(20MHz~26.5GHz/80MHz中频;含DDC选件)")]
[DeviceDescription(Name = "DDF255(20)",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFM
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.SCAN
                  | FeatureType.FFDF
                  | FeatureType.WBDF
                  | FeatureType.FScne
                  | FeatureType.AmpDF
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Version = "2.1.0",
    DeviceCapability = "20|26500|20000",
    Model = "DDF255",
    Description = "R&amp;S DDF255监测测向接收机(20MHz~26.5GHz/20MHz中频;含DDC选件)")]
[DeviceDescription(Name = "DDF255_VUHF(20)",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring | ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFM
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.SCAN
                  | FeatureType.FFDF
                  | FeatureType.WBDF
                  | FeatureType.FScne
                  | FeatureType.AmpDF
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Version = "2.1.0",
    DeviceCapability = "20|3600|20000",
    Model = "DDF255",
    Description = "R&amp;S DDF255监测测向接收机(20MHz~3.6GHz/20MHz中频;含DDC选件)")]
public partial class Ddf255
{
    private bool _attCtrlType;
    private float _attenuation;
    private bool _audioSwitch;

    private string _bandMeasureMode = "XDB";

    //TODO:精确到小数点后一位
    private float _beta = 1.0f;
    private double _channelBandWidth = 200d;
    private Dictionary<string, object>[] _ddcChannels;
    private Modulation _dem = Modulation.Fm;
    private DetectMode _detector = DetectMode.Fast;
    private double _dfBandwidth = 1000d;
    private float _dFindAverageTime = 0.1f;
    private DFindMode _dfindMode = DFindMode.Normal;
    private float _dwellTime;
    private string _fftMode = "OFF";
    private double _filterBandwidth = 150d;
    private double _frequency = 101.7d;
    private double _holdTime = 1f;
    private double _ifBandwidth = 1000d;
    private bool _iqSwitch;
    private bool _ituSwitch;
    private bool _levelSwitch;
    private float _levelThreshold = -20.0f;

    private int _measureTime;

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
    // [Browsable(false)]
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
    //         if (value)
    //         {
    //             SendCmd("OUTP:SQU ON");
    //             SendCmd($"SENSE:MSCAN:DWELL {_dwellTime}s");
    //             SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime}ms");
    //             SendCmd("MSCan:CONTrol:ON \"STOP:SIGN\"");
    //         }
    //         else
    //         {
    //             SendCmd("MSCan:CONTrol:OFF \"STOP:SIGN\"");
    //             SendCmd("SENSE:MSCAN:DWELL 0 ms");
    //             SendCmd("SENSE:MSCAN:HOLD:TIME 0 ms");
    //         }
    //     }
    // }
    private Polarization _polarityType = Polarization.Vertical;
    private double _resolutionBandwidth = 200d;
    private RfMode _rfmod = RfMode.Normal;
    private SegmentTemplate[] _segments;
    private bool _spectrumSwitch;
    private bool _squelchSwitch;

    private float _squelchThreshold = -20f;

    //TODO:精确到小数点后一位
    private float _xdB = 26.0f;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF |
                                FeatureType.IFMCA | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位MHz")]
    [ValueRange(20, 26500d, 6)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            SendCmd($"SENS:FREQ {_frequency}MHz");
            // 将设备的解调频率和中心频率设置为一致。
            SendCmd($"SENS:FREQ:DEM {_frequency}MHz");
        }
    }

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne |
                                FeatureType.IFMCA | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [Description("频谱带宽设置(当配置R&amp;S DDF255-WB选件，可达80MHz)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10",
        DisplayValues = "|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz")]
    [StandardValues(RegexPattern = @"\(20\)", IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz")]
    [DefaultValue(1000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            //AUTO ON是为了保证从测向功能切换到监测功能的时候能够顺利设置Span
            SendCmd("CALC:IFP:STEP:AUTO ON");
            SendCmd($"FREQ:SPAN {_ifBandwidth}kHz");
            //WBDF在任务启动的时候再重新设置
        }
    }

    [PropertyOrder(1)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [Description("频谱带宽设置(当配置R&amp;S DDF255-WB选件，可达80MHz)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10",
        DisplayValues = "|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz")]
    [StandardValues(RegexPattern = @"\(20\)", IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz")]
    [DefaultValue(1000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double DfBandwidth
    {
        get => _dfBandwidth;
        set
        {
            _dfBandwidth = value;
            //AUTO ON是为了保证从测向功能切换到监测功能的时候能够顺利设置Span
            SendCmd("CALC:IFP:STEP:AUTO ON");
            SendCmd($"FREQ:SPAN {_dfBandwidth}kHz");
            //WBDF在任务启动的时候再重新设置
        }
    }

    [PropertyOrder(9)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne |
                                FeatureType.FFDF | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("指定中频带宽、滤波带宽、解调带宽。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|80000|40000|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|15|12|9|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1",
        DisplayValues =
            "|80MHz|40MHz|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz")]
    [StandardValues(RegexPattern = @"\(20\)", IsSelectOnly = true,
        StandardValues =
            "|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|15|12|9|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1",
        DisplayValues =
            "|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz")]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            SendCmd($"SENS:BAND {_filterBandwidth}kHz");
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.WBDF
                                | FeatureType.FScne
                                | FeatureType.IFMCA
                                | FeatureType.MScne
                                | FeatureType.AmpDF)]
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

    [PropertyOrder(11)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.WBDF
                                | FeatureType.FScne
                                | FeatureType.IFMCA
                                | FeatureType.MScne
                                | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("衰减")]
    [Description("在 0~40 之间线性可调 单位dB。")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|40|30|20|10|0", //[0,40]
        DisplayValues = "|40|30|20|10|0")]
    [DefaultValue(0f)]
    [ValueRange(0, 40)]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
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

    [PropertyOrder(10)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.FFDF
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.WBDF
                                | FeatureType.IFMCA
                                | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频模式")]
    [Description("控制射频模块的三种工作模式，常规模式，低失真模式，低噪声模式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort|LowNoise",
        DisplayValues = "|常规|低失真|低噪声"
    )]
    [DefaultValue(RfMode.Normal)]
    [Style(DisplayStyle.Radio)]
    public RfMode RfMode
    {
        get => _rfmod;
        set
        {
            _rfmod = value;
            if (_rfmod.Equals(RfMode.Normal))
                SendCmd("INP:ATT:MODE NORM");
            else if (_rfmod.Equals(RfMode.LowNoise))
                SendCmd("INP:ATT:MODE LOWN");
            else if (_rfmod.Equals(RfMode.LowDistort)) SendCmd("INP:ATT:MODE LOWD");
        }
    }

    [PropertyOrder(6)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描类型")]
    [Description("频点扫描模式更准确|全景扫描模式速度快。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描"
    )]
    [DefaultValue(ScanMode.Fscan)]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Fscan;

    [PropertyOrder(3)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置频段扫描的起始频率，单位:MHz。")]
    [ValueRange(20.0, 26500.0, 6)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描扫描步进.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz")]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency
    {
        get;
        set;
        //// 频点扫描支持[1Hz,100MHz]任意的扫描步进，而全景扫描只支持以上步进为了使“频点扫描”和“全景扫描”使用统一的“扫描步进参数”，使扫描步进不可以任意输入
        //SendCmd(string.Format("SENS:SWE:STEP {0}MHz", _stepFrequency));
        //SendCmd(string.Format("SENS:PSC:STEP {0}MHz", _stepFrequency));
    } = 25d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置频段扫描的终止频率，单位为MHz。")]
    [ValueRange(20.0, 26500.0, 6)]
    [ValueRange(20.0d, 3600d, 6, regexPattern: "_VUHF")]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(13)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|5000|2000|1000|500|200|100|50|20|10|1|0",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s")]
    [ValueRange(0f, 10000f)]
    [DefaultValue(1f)]
    [Description("设置等待时间，单位：秒（ms）,如果信号在驻留时间内消失，则保持时间开始计时。一旦保持时间过期，扫描将继续下一个频率，即使驻留时间尚未过期。如果信号在保持时间内超过了静噪门限，则保持时间被重置。")]
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

    [PropertyOrder(22)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|10|7|5|2|1|0.5|0.2|0.1|0.05|0.02|0.01|0",
        DisplayValues =
            "|10s|7s|5s|2s|1s|0.5s|0.2s|0.1s|0.05s|0.02s|0.01s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：微秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等(当带宽较小时频谱数据返回较慢，如果驻留时间设置较短，可能看不到频谱数据)")]
    [ValueRange(0, 10, 0.01)]
    [Unit(UnitNames.Sec)]
    [DefaultValue(0.0f)]
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

    [Name(ParameterNames.MscanPoints)]
    [PropertyOrder(7)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MScanTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描的频点参数")]
    [Style(DisplayStyle.Default)]
    [Browsable(false)]
    public Dictionary<string, object>[] MscanPoints { get; set; }

    [Parameter(AbilitySupport = FeatureType.IFMCA, Template = typeof(IfmcaTemplate))]
    [Name(ParameterNames.DdcChannels)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频信道")]
    [Description("设置中频多路参数")]
    [PropertyOrder(8)]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
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
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("子通道数量")]
    [Description("中频多路支持的最大子通道数据量")]
    [ValueRange(4, 4)]
    [DefaultValue(4)]
    [ReadOnly(true)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    public int MaxChanCount { get; set; } = 4;

    [PropertyOrder(24)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.MScan |
                                FeatureType.MScne | FeatureType.FScne | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Scan)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVE|FAST|PEAK|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [Description("设置计算电平数据时的处理方式。")]
    [Children($"|{ParameterNames.MeasureTime}", DetectMode.Avg, DetectMode.Pos, DetectMode.Rms)]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector
    {
        get => _detector;
        set
        {
            _detector = value;
            var cmd = string.Empty;
            if (_detector.Equals(DetectMode.Avg))
                cmd = "PAV";
            else if (_detector.Equals(DetectMode.Fast))
                cmd = "FAST";
            else if (_detector.Equals(DetectMode.Pos))
                cmd = "POS";
            else if (_detector.Equals(DetectMode.Rms)) cmd = "RMS";
            SendCmd($"DET {cmd}");
        }
    }

    [PropertyOrder(9)]
    [Name("channelBandWidth")]
    [Parameter(AbilitySupport = FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向分辨率")]
    [DefaultValue(200d)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz")]
    [Description("将固定大小的频谱带宽划分为若干个信道，返回每一个信道的测向结果。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double ChannelBandWidth
    {
        get => _channelBandWidth;
        set
        {
            _channelBandWidth = value;
            SendCmd("CALC:IFP:STEP:AUTO OFF");
            SendCmd($"CALCulate:IFPan:STEP {_channelBandWidth} kHz");
        }
    }

    [PropertyOrder(23)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.MScan |
                                FeatureType.FScne | FeatureType.MScne | FeatureType.IFMCA | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues =
            "|9000000|5000000|2000000|1000000|500000|400000|200000|100000|60000|30000|20000|10000|5000|4000|2000|1000|0",
        DisplayValues = "|9s|5s|2s|1s|500ms|400ms|200ms|100ms|60ms|30ms|20ms|10ms|5ms|4ms|2ms|1ms|0ms")]
    [ValueRange(0, 900000000, 1000)]
    [DefaultValue(0)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    [Unit(UnitNames.Us)]
    [Style(DisplayStyle.Slider)]
    public int MeasureTime
    {
        get => _measureTime;
        set
        {
            if (_measureTime == value) return;
            _measureTime = value;
            SendCmd(value == 0 ? "MEAS:TIME DEF" : $"MEAS:TIME {_measureTime}us");
        }
    }

    [PropertyOrder(18)]
    [Name(ParameterNames.BandMeasureMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("ITU测量模式")]
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

    [PropertyOrder(26)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 100.0f)]
    [DefaultValue(26.0f)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Unit(UnitNames.Db)]
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

    [PropertyOrder(27)]
    [Name(ParameterNames.BetaValue)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [ValueRange(0.1f, 99.9f)]
    [DefaultValue(1.0f)]
    [Description("设置ITU测量中β值，单位：%")]
    [Unit(UnitNames.Pct)]
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

    [PropertyOrder(23)]
    [Name(ParameterNames.IntegrationTime)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("积分时间")]
    [DefaultValue(0.1)]
    [ValueRange(0.1, 10, 0.1)]
    [Description("测向值是一个平均值，测向时间是指求这个平均值的时间。单位(秒)")]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DFindAverageTime
    {
        get => _dFindAverageTime;
        set
        {
            _dFindAverageTime = value;
            SendCmd($"MEAS:DF:TIME {_dFindAverageTime}");
        }
    }

    [PropertyOrder(13)]
    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [Description("正常：电平门限值有效，用于常规信号测向；/r/n门限：电平门限值有效，并且积累测量结果，用于突发信号测向；/r/n连续：电平门限值无效，用于弱小信号测向。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Gate|Continuous",
        DisplayValues = "|正常|门限|连续")]
    [DefaultValue(DFindMode.Normal)]
    [Style(DisplayStyle.Radio)]
    public DFindMode DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            switch (_dfindMode)
            {
                case DFindMode.Feebleness:
                    SendCmd("MEAS:DF:MODE CONT");
                    break;
                case DFindMode.Gate:
                    SendCmd("MEAS:DF:MODE GATE");
                    break;
                case DFindMode.Normal:
                    SendCmd("MEAS:DF:MODE NORM");
                    break;
            }
        }
    }

    [PropertyOrder(14)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DefaultValue(-20.0f)]
    [ValueRange(-50f, 130f)]
    [DisplayName("电平门限")]
    [Description("单位：dbuV；测向机在Normal或Gate模式下，测向电平门限起效。")]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public float LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            SendCmd($"MEAS:DF:THR {_levelThreshold}");
        }
    }

    [PropertyOrder(15)]
    [Name(ParameterNames.QualityThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("质量门限")]
    [ValueRange(0, 100)]
    [DefaultValue(0)]
    [Description("设置测向质量门限，仅当信号质量超过门限时才返回测向数据")]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Pct)]
    public int QualityThreshold { get; set; }

    [PropertyOrder(2)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向分辨率")]
    [Description("设置测向分辨率")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz")]
    [DefaultValue(200d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double ResolutionBandwidth
    {
        get => _resolutionBandwidth;
        set
        {
            _resolutionBandwidth = value;
            //SetDFBandwidth(value);
            SendCmd($"CALC:IFPAN:STEP {_resolutionBandwidth}KHz");
        }
    }

    [PropertyOrder(17)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan | FeatureType.SCAN |
                                FeatureType.IFMCA | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("FFT模式")]
    [Description("设置中频数据取值的方式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|MIN|MAX|SCAL|OFF",
        DisplayValues = "|最小值|最大值|平均值|关闭"
    )]
    [DefaultValue("OFF")]
    [Style(DisplayStyle.Radio)]
    public string FftMode
    {
        get => _fftMode;
        set
        {
            _fftMode = value;
            SendCmd($"CALC:IFP:AVER:TYPE {_fftMode}");
        }
    }

    [PropertyOrder(16)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.FFDF | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE"
    )]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _dem;
        set
        {
            _dem = value;
            SendCmd($"SENS:DEM {_dem}");
        }
    }

    [PropertyOrder(19)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，单位dBuV。")]
    [DefaultValue(-20f)]
    [ValueRange(-30, 130)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public float SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
        }
    }

    [PropertyOrder(33)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("静噪门限开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关"
    )]
    [Description("设置是否打开静噪门限。")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            if (_squelchSwitch)
            {
                SendCmd("OUTP:SQU:STAT ON");
                SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
            }
            else
            {
                SendCmd("OUTP:SQU:STAT OFF");
            }
        }
    }

    [PropertyOrder(34)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FFDF |
                                FeatureType.FScne | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
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
            if (TaskState == TaskState.Start) InitPath();
        }
    }

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
    public bool IqSwitch
    {
        get => _iqSwitch;
        set
        {
            _iqSwitch = value;
            SendCmd(_iqSwitch ? "SYSTem:IF:REMote:MODe SHORT" : "SYSTem:IF:REMote:MODe OFF");
            if (value)
                _media |= MediaType.Iq;
            else
                _media &= ~MediaType.Iq;
            if (TaskState == TaskState.Start) InitPath();
        }
    }

    [PropertyOrder(30)]
    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [Description("ITU数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start) InitPath();
        }
    }

    [PropertyOrder(31)]
    [Name(ParameterNames.LevelSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [Description("电平数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start) InitPath();
        }
    }

    [PropertyOrder(32)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.FScne |
                                FeatureType.MScne | FeatureType.AmpDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState != TaskState.Start) return;
            InitPath();
        }
    }

    [PropertyOrder(39)]
    [Name(ParameterNames.DfPolarization)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [Description("设置测向极化方式，设备将结合当前频率自动选择合适的天线用于测向")]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化")]
    [DefaultValue(Polarization.Vertical)]
    [Style(DisplayStyle.Radio)]
    public Polarization Polar
    {
        get => _polarityType;
        set
        {
            _polarityType = value;
            SendCmd(value == Polarization.Vertical ? "ROUT:POL VERT" : "ROUT:POL HOR");
        }
    }

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

    [PropertyOrder(18)]
    [Name("dfindMethod")]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.SSE)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向体制")]
    [Description("设置当前设备的测向体制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ci",
        DisplayValues = "|干涉仪")]
    [Browsable(false)]
    [DefaultValue(DfindMethod.Ci)]
    [Style(DisplayStyle.Radio)]
    public DfindMethod DfindMethod { get; set; } = DfindMethod.Ci;

    #region 安装参数

    [PropertyOrder(35)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("DDF255接收机接收指令的IP地址。")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(36)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("RS接收机接收指令通讯端口号。")]
    [DefaultValue(5555)]
    [ValueRange(1000, 60000)]
    [Style(DisplayStyle.Slider)]
    public int Port { get; set; } = 5555;

    [PropertyOrder(37)]
    [Name("dfAntenna")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向天线端口")]
    [Description("设置测向天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1|2",
        DisplayValues = "|外部控制|自动|VUHF1|VUHF2|VUHF3"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Dropdown)]
    public int DfAntenna { get; set; } = 2;

    [PropertyOrder(38)]
    [Name("monitorAntenna")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("监测天线端口")]
    [Description("设置监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1|2",
        DisplayValues = "|外部控制|自动|VUHF1|VUHF2|VUHF3"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Dropdown)]
    public int MonitorAntenna { get; set; } = 2;

    [PropertyOrder(38)]
    [Name("hfAntenna")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("短波天线端口")]
    [Description("设置短波监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1|2",
        DisplayValues = "|外部控制|自动|HF1|HF2|HF3"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Dropdown)]
    public int HfAntenna { get; set; } = 2;

    [PropertyOrder(39)]
    [Name("angleCompensation")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向天线角度补偿")]
    [Description(
        "设置测向天线的角度补偿，格式如下：起始频率,结束频率,补偿值|起始频率,结束频率,补偿值；比如：20,1000,-10|1000,3000,20；如果没有逗号与竖线，代表整体补偿，比如：-10；注意，请输入英文半角符号！")]
    [DefaultValue("0")]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string AngleCompensation { get; set; } = "0";

    [PropertyOrder(40)]
    [Name("useGPS")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集GPS数据")]
    [Description("设置是否通过DDF255接收机采集GPS信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(true)]
    [Style(DisplayStyle.Switch)]
    public bool UseGps { get; set; }

    private int _useCompass;
    private string _compassName = "";

    [PropertyOrder(44)]
    [Name("useCompass")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集电子罗盘数据")]
    [Description("设置是否通过DDF255接收机采集电子罗盘信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|不采集|GH150@ADD197_V|GH150@ADD197_VAR1x_V"
    )]
    [DefaultValue(0)]
    [Browsable(true)]
    [Style(DisplayStyle.Radio)]
    public int UseCompass
    {
        get => _useCompass;
        set
        {
            _useCompass = value;
            _compassName = "";
            if (_useCompass == 1)
                _compassName = "GH150@ADD197_V";
            else if (_useCompass == 2) _compassName = "GH150@ADD197_VAR1x_V";
        }
    }

    [PropertyOrder(40)]
    [Name("firmwareVersion")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("硬件固件版本")]
    [Description("设置DDF255接收机的固件版本，影响PScan的数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Default|OffsetFrequencySwaped|OldSubscribe|Old",
        DisplayValues = "|默认|偏移频率反转|旧(订阅)|旧固件"
    )]
    [DefaultValue(FirmwareVersion.Default)]
    [Browsable(true)]
    [Style(DisplayStyle.Dropdown)]
    public FirmwareVersion FirmwareVersion { get; set; } = FirmwareVersion.Default;

    #endregion
}