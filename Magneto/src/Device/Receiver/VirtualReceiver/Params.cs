using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualReceiver;

[DeviceDescription(Name = "自动测试接收机",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring,
    Manufacturer = "Aleph",
    Version = "2.6.6",
    Model = "VirtualReceiver",
    DeviceCapability = "0.3|8000|40000",
    Description = "自动测试接收机，产品组自动测试使用",
    MaxInstance = 1,
    FeatureType = FeatureType.FFDF
                  | FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.SCAN
                  | FeatureType.TDOA
                  | FeatureType.WBDF
                  | FeatureType.MScan
                  | FeatureType.IFMCA
                  | FeatureType.SSE)]
public partial class VirtualReceiver
{
    #region 空间谱模拟参数

    [PropertyOrder(23)]
    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("sseAzimuthCount")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("模拟示向度个数")]
    [ValueRange(0, 10)]
    [DefaultValue(1)]
    [Unit("")]
    [Description("设置空间谱模拟的示向度个数")]
    public int SseAzimuthCount { get; set; }

    #endregion

    #region 常规设置

    private double _frequency;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.WBDF
                                | FeatureType.SSE
                                | FeatureType.IFMCA)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [Description("设置监测或测向时被测信号的中心频率，单位：MHz")]
    [ValueRange(0.3d, 8000.0d)]
    [DefaultValue(101.7d)]
    [Unit("MHz")]
    [Resident]
    [PropertyOrder(0)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            SetSingleParameter();
        }
    }

    private double _ifBandwidth;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.WBDF
                                | FeatureType.SSE
                                | FeatureType.IFMCA)]
    [Name(ParameterNames.IfBandwidth)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("中频带宽")]
    [Description("中频带宽、频谱跨距")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|150|120|100|50|20|10|5|2|1",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(500.0d)]
    // [Unit("kHz")]
    [Resident]
    [PropertyOrder(1)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            SetSingleParameter();
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.SSE
                                | FeatureType.MScan)]
    [Name(ParameterNames.RfMode)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("工作模式")]
    [Description("设置接收机射频工作模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowNoise|LowDistort",
        DisplayValues = "|常规模式|低噪声模式|低失真模式")]
    [DefaultValue(RfMode.Normal)]
    [PropertyOrder(2)]
    public RfMode RfMode { get; set; }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan)]
    [Name(ParameterNames.AttCtrlType)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("自动衰减控制")]
    [Description("设置衰减控制的方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(true)]
    [PropertyOrder(3)]
    public bool AttCtrlType { get; set; }

    private int _rfAttenuation;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN)]
    [Name(ParameterNames.RfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("射频衰减")]
    [Description("设置射频衰减")]
    [ValueRange(0, 30)]
    [Unit("dB")]
    [DefaultValue(0)]
    [PropertyOrder(4)]
    public int RfAttenuation
    {
        get => _rfAttenuation;
        set
        {
            _rfAttenuation = value;
            if (value % 2 == 0) return;
            _rfAttenuation--;
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN)]
    [Name(ParameterNames.IfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频衰减")]
    [Description("设置中频衰减")]
    [ValueRange(0, 20)]
    [Unit("dB")]
    [DefaultValue(0)]
    [PropertyOrder(6)]
    public int IfAttenuation { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.FFDF
                                | FeatureType.SSE)]
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
    // [Unit("kHz")]
    public double FilterBandwidth { get; set; } = 120.0d;

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.SSE)]
    [Name(ParameterNames.DfBandwidth)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|150|120|100|50|20|10|5|2|1",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [Resident]
    [DefaultValue(120.0d)]
    [Description("测向带宽")]
    // [Unit("kHz")]
    [PropertyOrder(3)]
    public double DfBandWidth { get; set; }

    private double _startFrequency = 87.0d;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 8000.0d)]
    [DefaultValue(87.0d)]
    [Unit("MHz")]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            SetSingleParameter();
        }
    }

    private double _stopFrequency = 108.0d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.RadioControl)]
    [Resident]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 8000.0d)]
    [DefaultValue(108.0d)]
    [Unit("MHz")]
    [Description("设置扫描终止频率，单位MHz")]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            SetSingleParameter();
        }
    }

    private double _stepFrequency = 25.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|1000|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|5MHz|1MHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [ValueRange(3.125d, 5000.0d)]
    [DefaultValue(25.0d)]
    // [Unit("kHz")]
    [Description("设置频段扫描步进，单位kHz")]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            SetSingleParameter();
        }
    }

    /// <summary>
    ///     属于功能层参数 非设备参数 用于区分PSCAN/FSCAN
    /// </summary>
    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.ScanMode)]
    [Category(PropertyCategoryNames.Scan)]
    [Resident]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|PSCAN|FSCAN")]
    [DefaultValue(ScanMode.Pscan)]
    [Description("扫描模式: 全景扫描或频率扫描")]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Name(ParameterNames.DfindMode)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号")]
    [DefaultValue(DFindMode.Normal)]
    [Resident]
    [Description("设置测向模式")]
    [PropertyOrder(8)]
    public DFindMode DFindMode { get; set; }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF)]
    [Name("dfSamplingCount")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("采样点数")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2048|1024|512|256|128",
        DisplayValues = "|2048|1024|512|256|128")]
    [DefaultValue(1024)]
    [Description("采样点数，用于控制测向分辨率")]
    [Resident]
    [Unit("个")]
    [PropertyOrder(4)]
    public int DfSamplingCount { get; set; }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF)]
    [Name("avgTimes")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("积分次数")]
    [Description("设置测向积分次数")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|0|2|5|10|15|20|25|30|35|40|45|50",
        DisplayValues = "|0|2|5|10|15|20|25|30|35|40|45|50")]
    [DefaultValue(5)]
    [Unit("次")]
    [Resident]
    [ValueRange(0, 200)]
    [PropertyOrder(9)]
    public int AvgTimes { get; set; }

    private int _levelThreshold;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.SCAN
                                | FeatureType.SSE)]
    [Name(ParameterNames.LevelThreshold)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("电平门限")]
    [Description("设置测向电平门限，当信号电平超过门限时返回测向结果")]
    [ValueRange(-40, 120)]
    [DefaultValue(10)]
    [Unit("dBμV")]
    [PropertyOrder(10)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            SetSingleParameter();
        }
    }

    private int _qualityThreshold;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF)]
    [Name(ParameterNames.QualityThreshold)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，当测向质量超过门限时返回测向结果")]
    [ValueRange(0, 100)]
    [DefaultValue(40)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(11)]
    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            _qualityThreshold = value;
            SetSingleParameter();
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM)]
    [Name(ParameterNames.DemMode)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Description("设置信号音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ")]
    [DefaultValue(Modulation.Fm)]
    [PropertyOrder(12)]
    public Modulation DemMode { get; set; }

    [PropertyOrder(15)]
    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM)]
    [Name(ParameterNames.SquelchSwitch)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    public bool SquelchSwitch { get; set; }

    [PropertyOrder(15)]
    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM)]
    [Name(ParameterNames.SquelchThreshold)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，当信号电平超过门限时，进行音频解调")]
    [ValueRange(-40, 100)]
    [Unit("dBμV")]
    [DefaultValue(-10)]
    public int SquelchThreshold { get; set; } = -10;

    [PropertyOrder(17)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [Name("holdTime")]
    [DisplayName("保持时间")]
    [Description("设置保持时间，单位：秒（s），保持特定时间进行信号搜索，等待信号出现")]
    [ValueRange(0.0f, 300.0f)]
    [DefaultValue(0.0f)]
    public float HoldTime { get; set; }

    [PropertyOrder(18)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [Name("dwellTime")]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等")]
    [ValueRange(0.0f, 300.0f)]
    [DefaultValue(0.0f)]
    public float DwellTime { get; set; }

    #endregion

    #region 高级设置

    [PropertyOrder(19)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan)]
    [Name("measureTime")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|10000|20000|50000|100000|200000|500000|1000000",
        DisplayValues = "|0.001s|0.01s|0.02s|0.05s|0.1s|0.2s|0.5s|1s")]
    [ValueRange(0.0f, 1000000.0f)]
    [DefaultValue(10000.0f)]
    [Description("设置测量时间，测量时间影响监测测向的结果的准确性")]
    public float MeasureTime { get; set; } = 10000.0f;

    [PropertyOrder(20)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FFDF
                                | FeatureType.SCAN
                                | FeatureType.MScan)]
    [Name(ParameterNames.Detector)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理电平与频谱的检波方式")]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(21)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Name(ParameterNames.Xdb)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 120.0f)]
    [DefaultValue(26.0f)]
    [Unit("dB")]
    [Description("设置ITU测量中XdB值 单位：dB")]
    public float XdB { get; set; } = 26.0f;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Name(ParameterNames.BetaValue)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [ValueRange(0.0f, 50.0f)]
    [DefaultValue(0.5f)]
    [Unit("%")]
    [Description("设置ITU测量中XdB值，单位：%")]
    public float BetaValue { get; set; } = 0.5f;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.SSE)]
    [Name("simAzimuth")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("虚拟示向度")]
    [ValueRange(0.0, 360)]
    [DefaultValue(0)]
    [Unit("°")]
    [Description("设置单频测向的模拟示向度，单位°")]
    public int SimAzimuth { get; set; }

    [Parameter(AbilitySupport = FeatureType.MScan
                                | FeatureType.MScne, Template = typeof(MScanTemplate))]
    [Name(ParameterNames.MscanPoints)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描频点参数")]
    [PropertyOrder(1)]
    public Dictionary<string, object>[] MScanPoints { get; set; }

    #endregion

    #region 数据开关

    private bool _iqSwitch;

    [PropertyOrder(24)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Name(ParameterNames.IqSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否获取IQ数据")]
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
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Name(ParameterNames.ItuSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否进行ITU测量，并获取数据")]
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

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM | FeatureType.ITUM
                                | FeatureType.SSE)]
    [Name(ParameterNames.AudioSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取音频数据")]
    [PropertyOrder(22)]
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

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.TDOA)]
    [Name(ParameterNames.LevelSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置接收机是否输出电平")]
    [PropertyOrder(20)]
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
    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.SSE)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取频谱数据")]
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
    [Parameter(AbilitySupport = FeatureType.IFMCA, Template = typeof(IfMultiChannelTemplate))]
    [Category(PropertyCategoryNames.Misc)]
    [Name(ParameterNames.DdcChannels)]
    [DisplayName("测量信道")]
    [Description("设置中频多路参数")]
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
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Misc)]
    [Name("maxChanCount")]
    [ReadOnly(true)]
    [DisplayName("窄带数量")]
    [Description("最多支持的窄带通道数。")]
    [ValueRange(32, 32)]
    [Browsable(false)]
    [DefaultValue(32)]
    public int MaxChanCount { get; set; }

    [PropertyOrder(8)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Misc)]
    [ReadOnly(true)]
    [Name("maxAudioCount")]
    [DisplayName("音频数量")]
    [Description("设置窄带最多支持的音频通道数。")]
    [Browsable(false)]
    [ValueRange(32, 32)]
    [DefaultValue(32)]
    public int MaxAudioCount { get; set; }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设备的IP地址")]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(26)]
    public string IpAddress { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name("cmdPort")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("命令端口号")]
    [Description("设备连接的端口号")]
    [DefaultValue(5055)]
    [PropertyOrder(27)]
    public int CmdPort { get; set; } = 5055;

    [Parameter(IsInstallation = true)]
    [Name("dataPort")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("数据端口号")]
    [Description("设备发送数据的本地端口号")]
    [DefaultValue(8088)]
    [PropertyOrder(27)]
    public new int DataPort { get; set; } = 8088;

    [Parameter(IsInstallation = true)]
    [Name("tdoaDataPath")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("文件路径")]
    [DefaultValue("")]
    [Description("TDOA数据文件路径")]
    public string TdoaDataPath { get; set; } = "";

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
    [ValueRange(20.0d, 6000.0d)]
    [DefaultValue(101.7d)]
    [Unit("MHz")]
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
    [DefaultValue(120.0d)]
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
    [PropertyOrder(4)]
    public Modulation DemMode { get; set; }
}

/// <summary>
///     中频多路分析子通道类模板
/// </summary>
internal class IfMultiChannelTemplate
{
    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [Name(ParameterNames.Frequency)]
    [Description("设置中频多路子通道中心频率，单位MHz")]
    [ValueRange(20.0d, 3000.0d)]
    [Unit("MHz")]
    [DefaultValue(102.6d)]
    public double Frequency { get; set; } = 102.6d;

    [PropertyOrder(1)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Demodulation)]
    [ReadOnly(true)]
    [Name(ParameterNames.FilterBandwidth)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [DefaultValue(150d)]
    [Description("设置中频多路子通道的滤波带宽。")]
    public double FilterBandwidth { get; set; } = 150.0d;

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Name(ParameterNames.DemMode)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|LSB|USB|CW|PM",
        DisplayValues = "|AM|FM|LSB|USB|CW|PM")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    public Modulation DemMode { get; set; } = Modulation.Fm;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.AudioSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    public bool AudioSwitch { get; set; } = true;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.LevelSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取电平数据")]
    public bool LevelSwitch { get; set; } = false;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name(ParameterNames.SpectrumSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取频谱数据")]
    public bool SpectrumSwitch { get; set; } = false;

    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [Name("ifSwitch")]
    [DisplayName("中频输出")]
    [Description("设置是否输出子通道中频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool IfSwitch { get; set; } = false;

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.UnitSelection)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("单位选择")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|dBμV|dBμV/m|dBm")]
    [Description("单位选择")]
    [DefaultValue(0)]
    [PropertyOrder(35)]
    public int UnitSelection { get; set; }

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.MaximumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最大值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最大值")]
    [DefaultValue(false)]
    [PropertyOrder(30)]
    public bool MaximumSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.MinimumSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱最小值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的最小值")]
    [DefaultValue(false)]
    [PropertyOrder(31)]
    public bool MinimumSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.MeanSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱平均值显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的平均值")]
    [DefaultValue(false)]
    [PropertyOrder(32)]
    public bool MeanSwitch { get; set; }

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name(ParameterNames.NoiseSwitch)]
    [Category(PropertyCategoryNames.DriverSpecified)]
    [DisplayName("频谱噪声显示")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否显示频谱数据的噪声")]
    [DefaultValue(false)]
    [Browsable(false)]
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
                    object objValue = null;
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