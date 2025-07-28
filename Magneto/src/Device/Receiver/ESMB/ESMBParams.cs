using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMB;

[DeviceDescription(Name = "ESMB",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.IFOUT
                  | FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.FScne
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Version = "1.6.5",
    DeviceCapability = "20|3000|1000",
    Model = "ESMB",
    Description = "R&amp;S ESMB 监测接收机(20MHz~3000MHz)")]
public partial class Esmb
{
    #region 常规参数

    private double _frequency = 89.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位MHz")]
    [ValueRange(20d, 3000d, 6)]
    [DefaultValue(89.7d)]
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

    private double _ifBandwidth = 150d;

    [PropertyOrder(6)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [Description("中频带宽设置。单位 kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|500|300|250|200|150|120|100|50|30|25|15|9|8|6|4|3|2.4|1.5|1",
        DisplayValues =
            "|1MHz|500kHz|300kHz|250kHz|200kHz|150kHz|120kHz|100kHz|50kHz|30kHz|25kHz|15kHz|9kHz|8kHz|6kHz|4kHz|3kHz|2.4kHz|1.5kHz|1kHz")]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            SendCmd($"SENS:FREQ:SPAN {_ifBandwidth} kHz");
        }
    }

    private double _filterBandwidth = 150d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("滤波带宽、解调带宽 默认单位 kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|300|250|150|120|100|30|15|9|8|6|4|3|2.4|1.5|1|0.6|0.3|0.15",
        DisplayValues =
            "|300kHz|250kHz|150kHz|120kHz|100kHz|30kHz|15kHz|9kHz|8kHz|6kHz|4kHz|3kHz|2.4kHz|1.5kHz|1kHz|600Hz|300Hz|150Hz")]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            SetFilterBandwidth(_filterBandwidth);
        }
    }

    [PropertyOrder(5)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MscanTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = null;

    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置扫描的起始频率，单位:MHz。")]
    [ValueRange(20.0, 3000.0, 6)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Slider)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 88.0;

    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置扫描的终止频率，单位为MHz。")]
    [ValueRange(20.0, 3000.0, 6)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Slider)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|150|125|75|60|50|15|7.5|4.5|4|3|2|1.5|1.2",
        DisplayValues = "|150kHz|125kHz|75kHz|60kHz|50kHz|15kHz|7.5kHz|4.5kHz|4kHz|3kHz|2kHz|1.5kHz|1.2kHz")]
    [DefaultValue(15d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 15d;

    [PropertyOrder(4)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描模式")]
    [Description("频点扫描模式更准确,全景扫描模式速度快。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描"
    )]
    [DefaultValue(ScanMode.Pscan)]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    private RfMode _rfMode = RfMode.Normal;

    [PropertyOrder(8)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.MScan |
                                FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频模式")]
    [Description("控制射频模块的三种工作模式，常规模式，低失真模式，低噪声模式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort|LowNoise",
        DisplayValues = "|常规|低失真|低噪声")]
    [DefaultValue(RfMode.Normal)]
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

    private bool _attCtrlType;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.MScne
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
                SendCmd("SENS:GCON:MODE MGC");
                SendCmd($"SENS:GCON:MGC {_attenuation}");
            }
        }
    }

    private float _attenuation;

    [PropertyOrder(9)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("衰减控制。单位dB。")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|110|100|90|80|70|60|50|40|30|20|10|0|-10|-20|-30",
        DisplayValues = "|110|100|90|80|70|60|50|40|30|20|10|0|-10|-20|-30")]
    [DefaultValue(0f)]
    [ValueRange(-30, 110)]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            //打开衰减控制
            SendCmd("INP:ATT:STAT ON");
            if (_attCtrlType)
            {
                SendCmd("INP:ATT:AUTO ON");
            }
            else
            {
                SendCmd("SENS:GCON:MODE MGC");
                SendCmd($"SENS:GCON:MGC {_attenuation}");
            }
        }
    }

    private Modulation _dem = Modulation.Fm;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB",
        DisplayValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB"
    )]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _dem;
        set
        {
            _dem = value;
            SetDemodulation(_dem);
        }
    }

    private bool _squelchSwitch = true;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关")]
    [Description("设置是否打开静噪门限开关, 只有在首先开启静噪开关，静噪门限才有效。")]
    [DefaultValue(false)]
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
                SendCmd("OUTP:SQU:STAT ON");
                SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
            }
            else
            {
                SendCmd("OUTP:SQU:STAT OFF");
            }
        }
    }

    private int _squelchThreshold = 10;

    [PropertyOrder(12)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，当信号电平超过门限时，进行音频解调, 单位dBuV. ")]
    [DefaultValue(10)]
    [ValueRange(-30, 100)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.DBuV)]
    public int SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
        }
    }

    private float _holdTime = 1000.0f;

    [PropertyOrder(13)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [Description("测量时间，单位：毫秒。 如在【驻留时间】内，电平低于门限，则继续等待，如果超过了【等待时间】，电平依然低于门限，则继续测量下个频点；如果电平超过门限，则继续测量，直到超过【驻留时间】。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000|2000|1000|500|200|100|50|20|10|1|0.5|0",
        DisplayValues =
            "|5s|2s|1s|0.5s|0.2s|0.1s|0.05s|0.02s|0.01s|0.001s|0.0005s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0f, 10000f, 0.5)]
    [DefaultValue(1000f)]
    [Unit(UnitNames.Ms)]
    [Style(DisplayStyle.Slider)]
    public float HoldTime
    {
        get => _holdTime;
        set
        {
            if (Math.Abs(_holdTime - value) < 1e-9) return;
            _holdTime = value;
            SendCmd($"SENSE:MScan:HOLD:TIME {_holdTime} ms");
        }
    }

    private float _dwellTime = 3.0f;

    [PropertyOrder(14)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间（秒）")]
    [Description("驻留时间，单位：秒。如电平值超过门限值，则在该频率上继续测量的时间长度。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5|4|3|2|1|0.5|0.2|0.1|0",
        DisplayValues =
            "|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0f, 10f, 0.1)]
    [DefaultValue(3f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime
    {
        get => _dwellTime;
        set
        {
            _dwellTime = value;
            SendCmd($"SENSE:MSCAN:DWELL {_dwellTime}s");
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

    private DetectMode _detector = DetectMode.Pos;

    [PropertyOrder(15)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.MScan |
                                FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVE|FAST|PEAK|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [Description("设置处理电平与频谱数的检波方式。")]
    [DefaultValue(DetectMode.Pos)]
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
                case DetectMode.Avg:
                    cmd = "PAV";
                    break;
                case DetectMode.Fast:
                    cmd = "FAST";
                    break;
                case DetectMode.Pos:
                    cmd = "POS";
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

    [PropertyOrder(17)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FScne | FeatureType.MScne)]
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

    private string _bandMeasureMode = "XDB";

    [PropertyOrder(18)]
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

    private float _xdB = 26f;

    [PropertyOrder(19)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XdB带宽")]
    [Description("XdB带宽指在上、下限频率之外，任何离散频谱分量或连续的频谱功率密度至少比预定的0dB参考电平低xdB.")]
    [DefaultValue(26f)]
    [ValueRange(0f, 100f)]
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

    private float _beta = 1f;

    [PropertyOrder(20)]
    [Name(ParameterNames.BetaValue)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("β%带宽")]
    [Description("百分比带宽，在它的频率下限之下和频率上限之上所发射的平均功率等于某一给定发射的总平均功率的规定百分数β/2.")]
    [DefaultValue(1f)]
    [ValueRange(0.1f, 99.9f)]
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

    private bool _ituSwitch;

    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [Description("设置是否进行ITU测量, 并获取数据.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
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
            if (TaskState != TaskState.Start) return;
            SendMeadiaRequest();
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(22)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("设置是否获取频谱数据.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
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
            SendMeadiaRequest();
        }
    }

    private bool _audioSwitch;

    [PropertyOrder(23)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.MScne | FeatureType.FScne | FeatureType.IFOUT)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否打开音频数据开关。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
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
            if (TaskState != TaskState.Start) return;
            SendMeadiaRequest();
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

    #endregion

    #region 安装参数

    [PropertyOrder(24)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("ESMB接收机接收指令的IP地址。")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(25)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("EMSB接收机接收指令通讯端口号。")]
    [DefaultValue(5555)]
    [ValueRange(1000, 60000)]
    [Style(DisplayStyle.Slider)]
    public int Port { get; set; } = 5555;

    #endregion
}