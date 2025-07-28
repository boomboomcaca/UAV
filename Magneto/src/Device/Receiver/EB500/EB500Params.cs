using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EB500;

[DeviceDescription(
    Name = "EB500",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Model = "EB500",
    Version = "1.7.5",
    DeviceCapability = "20|6000|20000",
    Description = "EB500接收机")]
public partial class Eb500
{
    #region 常规参数

    private double _frequency = 101.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(101.7d)]
    [Description("中心频率，单位MHz")]
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

    private double _ifBandwidth = 200.0d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues = "|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、频谱跨距 单位 kHz")]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            SendCmd($"FREQ:SPAN {_ifBandwidth}kHz");
        }
    }

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(87.0d)]
    [Description("频段扫描起始频点 单位为MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Description("频段扫描终止频率 单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    /// <summary>
    ///     属于功能层参数 非设备参数 用于区分PSCAN/FSCAN
    /// </summary>
    [PropertyOrder(4)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|PSCAN|FSCAN")]
    [DefaultValue(ScanMode.Pscan)]
    [Description("扫描模式: 全景扫描或频率扫描")]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [StandardValues(StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz",
        IsSelectOnly = true)
    ]
    [DefaultValue(25.0d)]
    [Description("频段扫描步进 单位kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MscanTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = null;

    private bool _attCtrlType;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScne)]
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
                SendCmd("INP:ATT:AUTO:HOLD:TIME 0");
            }
            else
            {
                SendCmd("INP:ATT:AUTO OFF");
                SendCmd($"INP:ATT {_attenuation}");
            }
        }
    }

    private int _attenuation;

    [PropertyOrder(9)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [StandardValues(StandardValues = "|0|5|10|15|20|25|30|35|40",
        DisplayValues = "|0|5|10|15|20|25|30|35|40",
        IsSelectOnly = false)]
    [DefaultValue(0)]
    [ValueRange(0, 40)]
    [Description("设备衰减 默认单位 dB.")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public int Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (_attCtrlType)
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

    private double _filterBandwidth = 120.0d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [StandardValues(
        StandardValues =
            "|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|50|30|15|12|9|6|4.8|4|3.1|2.7|2.4|2.1|1.5",
        DisplayValues =
            "|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz",
        IsSelectOnly = true)]
    [DefaultValue(120.0d)]
    [Description("滤波带宽、解调带宽 默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            //当中频带宽小于9kHz时，解调模式CW/LSB/USB有效
            SetFilterBandwidth(_filterBandwidth);
        }
    }

    private RfMode _rfMode = RfMode.Normal;

    [PropertyOrder(8)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort",
        DisplayValues = "|常规|低失真")]
    [DefaultValue(RfMode.Normal)]
    [Description("控制射频模块的两种工作模式，常规模式，低失真模式.")]
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

    [PropertyOrder(13)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|2000|1000|500|200|100|50|20|10|1|0",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s")]
    [ValueRange(0f, 10000f)]
    [DefaultValue(1f)]
    [Description("设置等待时间，单位：秒（ms）,如果信号在驻留时间内消失，则保持时间开始计时。一旦保持时间过期，扫描将继续下一个频率，即使驻留时间尚未过期。如果信号在保持时间内超过了静噪门限，则保持时间被重置。")]
    [Unit(UnitNames.Ms)]
    [Style(DisplayStyle.Slider)]
    public double HoldTime
    {
        get;
        set;
        ////SendCmd($"SENSE:MSCAN:HOLD:TIME {_holdTime} ms");
        //if (_taskState == TaskState.Start && _dwellSwitch && _curFeature == FeatureType.MScan)
        //{
        //    StopTask();
        //    Thread.Sleep(10);
        //    StartTask();
        //}
    } = 1f;

    [PropertyOrder(13)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等(当频谱带宽和驻留时间都较小时，设备会出现不返回频谱数据的现象)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5|2|1|0.5|0.2|0.1|0.05|0.02|0.01|0",
        DisplayValues =
            "|5s|2s|1s|0.5s|0.2s|0.1s|0.05s|0.02s|0.01s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0.000f, 10.000f, 0.01)]
    [DefaultValue(1f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime
    {
        get;
        set;
        //if (_taskState == TaskState.Start && _dwellSwitch && _curFeature == FeatureType.MScan)
        //{
        //    StopTask();
        //    Thread.Sleep(10);
        //    StartTask();
        //}
    } = 1f;

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
    public int SquelchThreshold
    {
        get;
        set;
        //SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
    } = 10;

    private Modulation _demMode = Modulation.Fm;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|PULSE|PM|IQ|ISB|CW|USB|LSB",
        DisplayValues = "|AM|FM|PULSE|PM|IQ|ISB|CW|USB|LSB")]
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
        }
    }

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

    private float _measureTime;

    [PropertyOrder(14)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5|2|1|0.5|0.2|0.1|0.05|0.02|0.01|0.001|0.0005|0",
        DisplayValues =
            "|5s|2s|1s|0.5s|0.2s|0.1s|0.05s|0.02s|0.01s|0.001s|0.0005s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [DefaultValue(0f)]
    [Description("设置测量时间，单位：秒（s），测量时间影响监测测向的结果的准确性")]
    [Unit(UnitNames.Sec)]
    [ValueRange(0, 10, 0.001)]
    [Style(DisplayStyle.Slider)]
    public float MeasureTime
    {
        get => _measureTime;
        set
        {
            _measureTime = value;
            if (value == 0f)
                SendCmd("MEAS:TIME DEF");
            else
                SendCmd($"MEAS:TIME {_measureTime}s");
        }
    }

    private DetectMode _detector = DetectMode.Fast;

    [PropertyOrder(15)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne |
                                FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|PEAK|AVE|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理电平数据的检波方式（检波方式只影响电平数据，对频谱无影响）")]
    [Children($"|{ParameterNames.MeasureTime}", DetectMode.Avg, DetectMode.Pos, DetectMode.Rms)]
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

            if (string.IsNullOrEmpty(cmd)) return;
            SendCmd($"SENS:DET {cmd}");
        }
    }

    private string _fftMode = "OFF";

    [PropertyOrder(16)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("FFT模式")]
    [Description("设置中频频谱数据取值的方式（该参数对频谱数据有影响，对电平数据无影响）。")]
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

    private string _bandMeasureMode = "XDB";

    [PropertyOrder(17)]
    [Name(ParameterNames.BandMeasureMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
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

    private float _xdB = 26.0f;

    [PropertyOrder(18)]
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

    private float _beta = 1.0f;

    [PropertyOrder(19)]
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

    #endregion

    #region 数据开关

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
                _mediaType |= MediaType.Iq;
            else
                _mediaType &= ~MediaType.Iq;
            if (TaskState != TaskState.Start) return;
            SendMediaRequest();
        }
    }

    private bool _ituSwitch;

    [PropertyOrder(21)]
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
            if (!_ituOption && _ituSwitch)
                //SendMessage(MessageDomain.Task, MessageType.Information, "当前设备没有ITU选件，不能进行ITU测量!");
                return;
            if (value)
                _mediaType |= MediaType.Itu;
            else
                _mediaType &= ~MediaType.Itu;
            if (TaskState != TaskState.Start) return;
            SendMediaRequest();
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(22)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne)]
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
                _mediaType |= MediaType.Spectrum;
            else
                _mediaType &= ~MediaType.Spectrum;
            if (TaskState != TaskState.Start) return;
            SendMediaRequest();
        }
    }

    private bool _squelchSwitch;

    [PropertyOrder(23)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    [Style(DisplayStyle.Switch)]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            if (_squelchSwitch)
                SendCmd("OUTPut:SQUelch ON");
            else
                SendCmd("OUTPut:SQUelch OFF");
        }
    }

    private bool _audioSwitch;

    [PropertyOrder(24)]
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
            {
                SendCmd("OUTP:FILT:MODE NOTCH");
                _mediaType |= MediaType.Audio;
            }
            else
            {
                SendCmd("OUTP:FILT:MODE OFF");
                _mediaType &= ~MediaType.Audio;
            }

            if (TaskState != TaskState.Start) return;
            SendMediaRequest();
        }
    }

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
    // [Browsable(false)]
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
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(26)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [DefaultValue(5555)]
    [Description("设置连接并控制设备的网络端口号")]
    [ValueRange(1000, 60000)]
    [Style(DisplayStyle.Slider)]
    public int Port { get; set; } = 5555;

    #endregion
}