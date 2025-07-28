using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Device.G33DDC.SDK;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

[DeviceDescription(Name = "G33DDC",
    Manufacturer = "WiNRADiO",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.ITUM
                  | FeatureType.IFMCA,
    MaxInstance = 1,
    Version = "1.5.5",
    DeviceCapability = "0.009|50|4000",
    Model = "G33DDC",
    Description = "WiNRADiO G33DDC宽带短波接收机")]
public partial class G33Ddc
{
    #region 常规参数

    private double _frequency = 10.0d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.AmpDF
                                | FeatureType.TDOA
                                | FeatureType.IFMCA
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [ValueRange(0.009d, 50.0d, 6)]
    [DefaultValue(10.0d)]
    [Description("中心频率，默认单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            _device?.SetFrequency(_frequency);
        }
    }

    private double _filterBandwidth = 64d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FScne
                                | FeatureType.MScne
                                | FeatureType.AmpDF
                                | FeatureType.TDOA
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|64|50|40|32|24|20",
        DisplayValues = "|64kHz|50kHz|40kHz|32kHz|24kHz|20kHz")]
    [DefaultValue(64d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、滤波带宽、解调带宽 默认单位 kHz")]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            _device?.SetDemBandwidth(0, _filterBandwidth);
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
        StandardValues =
            "|4000|3200|2500|2000|1600|1250|1000|800|640|500|400|320|250|200|160|125|100|80|64|50|40|32|24|20",
        DisplayValues =
            "|4MHz|3.2MHz|2.5MHz|2MHz|1.6MHz|1.25MHz|1MHz|800kHz|640kHz|500kHz|400kHz|320kHz|250kHz|200kHz|160kHz|125kHz|100kHz|80kHz|64kHz|50kHz|40kHz|32kHz|24kHz|20kHz")]
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
            _device?.SetIfBandwidth(_ifBandwidth, CurFeature == FeatureType.IFMCA);
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
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("衰减值")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|3|6|9|12|15|18|21",
        DisplayValues = "|0|3|6|9|12|15|18|21")]
    [DefaultValue(0)]
    [ValueRange(0, 21)]
    [Unit(UnitNames.Db)]
    [Description("设备衰减 默认单位 dB.")]
    [Style(DisplayStyle.Slider)]
    public int Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            _device?.SetAttenuator((uint)_attenuation);
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

    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("起始频率")]
    [ValueRange(0.009d, 50d, 6)]
    [DefaultValue(0.009d)]
    [Unit(UnitNames.MHz)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 0.009d;

    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("终止频率")]
    [ValueRange(0.009d, 50d, 6)]
    [DefaultValue(50d)]
    [Unit(UnitNames.MHz)]
    [Description("设置扫描终止频率，单位MHz")]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 50d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|98|48.8|24.4|12.2|6.1|3.1|1.5",
        DisplayValues = "|98kHz|48.8kHz|24.4kHz|12.2kHz|6.1kHz|3.1kHz|1.5kHz")]
    [DefaultValue(6.1d)]
    [Unit(UnitNames.KHz)]
    [Description("设置频段扫描步进。")]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 6.1d;

    private IfmcaTemplate[] _ddcChannels;

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
        get => null;
        set
        {
            _ddcChannels = value == null ? null : Array.ConvertAll(value, item => (IfmcaTemplate)item);
            UpdateDdcChannels();
        }
    }

    [PropertyOrder(8)]
    [Name("maxChanCount")]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.RadioControl)]
    [ValueRange(3, 3)]
    [ReadOnly(true)]
    [DisplayName("子通道数量")]
    [Description("中频多路支持的最大子通道数据量")]
    [DefaultValue(3)]
    [Style(DisplayStyle.Input)]
    public int MaxChanCount { get; set; } = 3;

    //TODO: 确认范围
    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，单位 dBuV, 当信号电平超过门限时，进行音频解调")]
    [ValueRange(-30, 130)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold { get; set; } = 10;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    [Style(DisplayStyle.Switch)]
    public bool SquelchSwitch { get; set; } = true;

    private uint _demMode = 2;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.TDOA | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3|4|6|7",
        DisplayValues = "|CW|AM|FM|LSB|USB|DSB|ISB")]
    [DefaultValue(2)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public uint DemMode
    {
        get => _demMode;
        set
        {
            // 解调模式删除DRM与AMS
            _demMode = value;
            var mode = (DemodulatorMode)_demMode;
            if (CurFeature == FeatureType.FFM && TaskState == TaskState.Start) _device?.SetDemMode(0, mode);
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

    private Dictionary<string, object>[] _mscanPoints;

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MScanTemplate))]
    [Name(ParameterNames.MscanPoints)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描频点参数")]
    [PropertyOrder(1)]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MScanPoints
    {
        get => _mscanPoints;
        set
        {
            lock (_lockMscanPoints)
            {
                _mscanPoints = value;
            }
        }
    }

    #endregion

    #region 高级参数

    private double _holdTime = 1000d;

    [PropertyOrder(13)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = true,
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
        DisplayValues = "|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")]
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
        }
    }

    private int _samplingCount = 1024;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA)]
    [Name("samplingCount")]
    [DisplayName("采样点数")]
    [Description("采样点数，用于控制采集数据的点数")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|32768|16384|8192|4096|2048|1024",
        DisplayValues = "|32768|16384|8192|4096|2048|1024")]
    [DefaultValue(1024)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Dropdown)]
    public int SamplingCount
    {
        get => _samplingCount;
        set
        {
            _samplingCount = value;
            _device?.SetSamplingCount((uint)_samplingCount);
        }
    }

    private int _measureTime;

    [PropertyOrder(15)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.IFMCA
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|2000|1000|500|200|100|0",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|0s")]
    [ValueRange(0, 5000, 100)]
    [DefaultValue(0)]
    [Unit(UnitNames.Ms)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    [Style(DisplayStyle.Slider)]
    public int MeasureTime
    {
        get => _measureTime;
        set
        {
            if (value.Equals(_measureTime)) return;
            _measureTime = value;
            //if (_detector != DetectMode.FAST)
            //{
            //    _device?.SetMeasureTime((uint)_measureTime);
            //}
        }
    }

    private DetectMode _detector = DetectMode.Fast;

    [PropertyOrder(14)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置计算电平的处理方式（影响电平数据）")]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector
    {
        get => _detector;
        set
        {
            if (_detector == value) return;
            _detector = value;
            _device?.SetDetector(_detector);
            if (TaskState == TaskState.Start && CurFeature is FeatureType.FFM or FeatureType.MScan or FeatureType.TDOA)
            {
                if (_detector != DetectMode.Fast)
                    StartDetector();
                else
                    StopDetector();
            }
        }
    }

    private int _agcMode;

    [PropertyOrder(14)]
    [Name("agcMode")]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("自动增益设置")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3",
        DisplayValues = "|OFF|SLOW|MEDIUM|FAST")]
    [DefaultValue(3)]
    [Description("为信道设置固定增益。 ")]
    [Children("|gain", 0)]
    [Style(DisplayStyle.Radio)]
    public int GainMode
    {
        get => _agcMode;
        set
        {
            // 注意，在FSCAN时，必须设置为自动增益，否则计算不正确
            _agcMode = value;
            if (CurFeature == FeatureType.SCAN && ScanMode == ScanMode.Fscan) _agcMode = 3;
            var mode = (AgcMode)_agcMode;
            _device?.SetAgcMode(0, mode);
            if (mode == AgcMode.AgcOff) _device?.SetGain(0, _gain);
        }
    }

    private int _gain;

    [PropertyOrder(14)]
    [Name("gain")]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.TDOA
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("手动增益设置")]
    [DefaultValue(0)]
    [ValueRange(0, 150)]
    [Description("为信道设置固定增益。仅在自动增益关闭时有效")]
    [Style(DisplayStyle.Slider)]
    public int Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            var mode = (AgcMode)_agcMode;
            if (mode == AgcMode.AgcOff) _device.SetGain(0, _gain);
        }
    }

    [PropertyOrder(21)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM)]
    [Name(ParameterNames.Xdb)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [ValueRange(3, 60)]
    [DefaultValue(26)]
    [Unit(UnitNames.Db)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Style(DisplayStyle.Slider)]
    public int XdB { get; set; } = 26;

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM)]
    [Name(ParameterNames.BetaValue)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [ValueRange(0, 50)]
    [DefaultValue(1)]
    [Unit(UnitNames.Pct)]
    [Description("设置ITU测量中beta值，单位：%")]
    [Style(DisplayStyle.Slider)]
    public int BetaValue { get; set; } = 1;

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
    public bool IqSwitch { get; set; }

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
    public bool SpectrumSwitch { get; set; } = true;

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
            if (TaskState == TaskState.Start)
            {
                if (!_audioSwitch)
                    _device.StopAudio(0);
                else
                    _device.StartAudio(0);
            }
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
    [Style(DisplayStyle.Switch)]
    public bool ItuSwitch
    {
        get => _ituSwitch;
        set
        {
            if (_ituSwitch == value) return;
            _ituSwitch = value;
            if (TaskState == TaskState.Start)
            {
                if (_ituSwitch)
                    StartItu();
                else
                    StopItu();
            }
        }
    }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [Name("preamplifier")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("放大器开关")]
    [Description("设置是否启用前置放大器")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(44)]
    [Style(DisplayStyle.Switch)]
    public bool EnabledPreamplifier { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("adcDithering")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("ADC抖动")]
    [Description("设置是否启用ADC抖动")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(44)]
    [Style(DisplayStyle.Switch)]
    public bool EnabledAdcDithering { get; set; } = true;

    #endregion
}