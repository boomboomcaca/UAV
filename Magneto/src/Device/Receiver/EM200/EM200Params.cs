using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device;

[DeviceDescription(Name = "EM200",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.MScne
                  | FeatureType.FScne
                  | FeatureType.ITUM,
    MaxInstance = 1,
    Version = "1.4.5",
    Model = "EM200",
    DeviceCapability = "0.008|8000|40000",
    Description = "R&amp;S EM200监测接收机")]
public partial class Em200
{
    #region 常规参数

    private double _frequency = 101.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中心频率")]
    [ValueRange(0.008d, 8000.0d, 6)]
    [DefaultValue(101.7d)]
    [Description("单频监测时的中心频率,单位 MHz")]
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

    private double _ifBandwidth = 200d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues = "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(200d)]
    [Description("中频带宽、频谱跨距 默认单位 kHz")]
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

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("起始频率")]
    [ValueRange(0.008d, 8000.0d, 6)]
    [DefaultValue(88.0d)]
    [Description("设置扫描的起始频率，单位:MHz。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 88.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("终止频率")]
    [ValueRange(0.008d, 8000.0d, 6)]
    [DefaultValue(108.0d)]
    [Description("设置扫描的终止频率，单位为 MHz。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1|0.625|0.5|0.25|0.2|0.125|0.1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz|625Hz|500Hz|250Hz|200Hz|125Hz|100Hz")]
    [DefaultValue(25d)]
    [Description("设置扫描步进。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25d;

    [PropertyOrder(5)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描")]
    [DefaultValue(ScanMode.Pscan)]
    [Description("频点扫描模式更准确,全景扫描模式速度快。")]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    [PropertyOrder(6)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(
        AbilitySupport = FeatureType.MScan | FeatureType.MScne,
        Template = typeof(MScanTemplate))]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] Frequencys { get; set; }

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne, Template = typeof(SegmentTemplate))]
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

    private double _filterBandwidth = 150d;

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|40000|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|25|15|12|9|8.333|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1|0.6|0.3|0.15|0.1",
        DisplayValues =
            "|40MHz|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|25kHz|15kHz|12kHz|9kHz|8.333kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz|600Hz|300Hz|150Hz|100Hz")]
    [DefaultValue(150d)]
    [Description("设置信号滤波带宽。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            //当中频带宽小于9kHz时，解调模式CW/LSB/USB有效
            SetIfBandwidth(_filterBandwidth);
        }
    }

    private RfMode _rfMode = RfMode.Normal;

    [PropertyOrder(8)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
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
                                | FeatureType.FScne)]
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
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("衰减")]
    [Description("设置衰减，默认单位 dB")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40|35|30|25|20|15|10|5|0", //[0,40]
        DisplayValues = "|40|35|30|25|20|15|10|5|0")]
    [DefaultValue(0)]
    [Unit(UnitNames.Db)]
    [ValueRange(0, 40)]
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

    //TODO: 暂不暴露
    private float _gain;

    // [PropertyOrder(10)]
    // [Parameter(AbilitySupport = FeatureType.FFM)]
    // [Category(PropertyCategoryNames.Normal)]
    // [DisplayName("增益控制")]
    // [StandardValues(IsSelectOnly = true,
    //                 StandardValues = "|-100|-30|-20|-10|0|10|20|30|40|50|60|70|80|90|100|110|120|130",
    //                 DisplayValues = "|自动|-30dBuV|-20dBuV|-10dBuV|0dBuV|10dBuV|20dBuV|30dBuV|40dBuV|50dBuV|60dBuV|70dBuV|80dBuV|90dBuV|100dBuV|110dBuV|120dBuV|130dBuV"
    //                 )]
    // [DefaultValue(0f)]
    // [Description("设备增益控制。单位 dBuV")]
    public float Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            if (_gain.Equals(-100f))
            {
                SendCmd("SENS:GCON:MODE AUTO");
            }
            else
            {
                SendCmd("SENS:GCON:MODE MGC");
                SendCmd($"GCON {_gain}");
            }
        }
    }

    private Modulation _demodulation = Modulation.Fm;

    [PropertyOrder(11)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB",
        DisplayValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB")]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demodulation;
        set
        {
            _demodulation = value;
            SetDemodulation(_demodulation);
        }
    }

    private int _squelchThreshold = -20;

    [PropertyOrder(12)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [DefaultValue(-20)]
    [ValueRange(-30, 130)]
    [Description("设置静噪门限的值，单位dBuV。")]
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

    [PropertyOrder(12)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
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

    private double _holdTime = 1f;

    [PropertyOrder(13)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|10000|5000|2000|1000|500|200|100|50|20|10|1|0",
        DisplayValues = "|10s|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s")]
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

    private float _dwellTime = 1f;

    [PropertyOrder(14)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|10|8|6|5|4|3|2|1|0.5|0.2|0.1|0",
        DisplayValues =
            "|10s|8s|6s|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0f, 10f, 0.1)]
    [Unit(UnitNames.Sec)]
    [DefaultValue(1f)]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等(当带宽较小时频谱数据返回较慢，如果驻留时间设置较短，可能看不到频谱数据)")]
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

    #endregion

    #region 高级参数

    private float _measureTime = -1f;

    [PropertyOrder(14)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.FScne
                                | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|5|2|1|0.5|0.2|0.1|0.05|0.02|0.01|0.001|0|-1",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|0s|自动")]
    [DefaultValue(-1f)]
    [ValueRange(-1f, 5f)] //根据常规经验，屏蔽掉（5s,900s]以免客户长时间等待，造成没有数据返回的假象
    [Description("设置测量时间.")]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float MeasureTime
    {
        get => _measureTime;
        set
        {
            if (Math.Abs(value - _measureTime) < 1e-9) return;
            _measureTime = value;
            SendCmd(Math.Abs(value - -1f) < 1e-9 ? "MEAS:TIME DEF" : $"MEAS:TIME {_measureTime}s");
            // HoldTime = _measureTime * 1000;
        }
    }

    private DetectMode _detector = DetectMode.Pos;

    [PropertyOrder(15)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DefaultValue(DetectMode.Pos)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [Description("设置计算电平数据时的处理方式（该参数影响电平数据）")]
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

            if (cmd != string.Empty) SendCmd($"DET {cmd}");
        }
    }

    private string _fftMode = "OFF";

    [PropertyOrder(16)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("FFT模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|MIN|MAX|SCAL|OFF",
        DisplayValues = "|最小值|最大值|平均值|关闭")]
    [DefaultValue("OFF")]
    [Description("设置中频频谱数据的取值方式（该参数影响频谱数据）")]
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

    [PropertyOrder(17)]
    [Name(ParameterNames.BandMeasureMode)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("ITU测量模式")]
    [Description("ITU带宽测量模式，分为xdB带宽，百分比占用带宽。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|XDB|BETA",
        DisplayValues = "|XdB|β%"
    )]
    [DefaultValue("XDB")]
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

    private float _beta = 1f;

    [PropertyOrder(18)]
    [Name(ParameterNames.BetaValue)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("BETA")]
    [Description("百分比带宽，在它的频率下限之下和频率上限之上所发射的平均功率等于某一给定发射的总平均功率的规定百分数β/2.")]
    [DefaultValue(1f)]
    [Unit(UnitNames.Pct)]
    [ValueRange(0.1f, 99.9f)]
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

    private float _xdb = 26f;

    [PropertyOrder(19)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("XDB")]
    [DefaultValue(26f)]
    [ValueRange(0f, 100f)]
    [Unit(UnitNames.Db)]
    [Description("xdB带宽指在上、下限频率之外，任何离散频谱分量或连续的频谱功率密度至少比预定的0dB参考电平xdB.")]
    [Style(DisplayStyle.Slider)]
    public float XdB
    {
        get => _xdb;
        set
        {
            _xdb = value;
            SendCmd($"MEAS:BAND:XDB {_xdb}");
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
    [Description("设置是否获取IQ数据。")]
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
            if (TaskState == TaskState.Start) SendMediaRequest();
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
        DisplayValues = "|开|关"
    )]
    [DefaultValue(false)]
    [Description("设置是否进行ITU测量，并获取数据。")]
    [Style(DisplayStyle.Switch)]
    public bool ItuSwitch
    {
        get => _ituSwitch;
        set
        {
            _ituSwitch = value;
            if (!_ituOption && _ituSwitch) return;
            if (value)
                _mediaType |= MediaType.Itu;
            else
                _mediaType &= ~MediaType.Itu;
            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(22)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关"
    )]
    [DefaultValue(true)]
    [Description("频谱数据开关。")]
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
            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    private bool _audioSwitch = true;

    [PropertyOrder(24)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.MScne
                                | FeatureType.FScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取音频数据。")]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch
    {
        get => _audioSwitch;
        set
        {
            _audioSwitch = value;
            if (value)
            {
                _mediaType |= MediaType.Audio;
                //音频开启的时候开启自动消除干扰信号
                SendCmd("OUTP:FILT:MODE NOTCH");
            }
            else
            {
                SendCmd("OUTP:FILT:MODE OFF");
                _mediaType &= ~MediaType.Audio;
            }

            if (TaskState == TaskState.Start) SendMediaRequest();
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(26)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [DefaultValue("127.0.0.1")]
    [Description("EM200接收机接收指令的IP地址。")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(27)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [DefaultValue(5555)]
    [Description("EM200接收机接收指令通讯端口号。")]
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
        StandardValues = "|-1|0|1",
        DisplayValues = "|自动|VUHF1|VUHF2"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Radio)]
    public int MonitorAntenna { get; set; } = -1;

    #endregion
}