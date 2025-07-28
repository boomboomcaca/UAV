using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF200M;

[DeviceDescription(Name = "DDF200M",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM | FeatureType.FFDF | FeatureType.WBDF,
    MaxInstance = 1,
    Version = "1.0.1",
    DeviceCapability = "118|450|20000",
    Description = "R&amp;S DDF200M监测测向接收机")]
public partial class Ddf200M
{
    private float _attenuation;
    private bool _audioSwitch;
    private double _channelBandWidth = 200d;
    private Modulation _dem = Modulation.Fm;
    private DetectMode _detector = DetectMode.Fast;
    private double _dfBandWidth = 150d;
    private float _dFindAverageTime = 0.1f;
    private DFindMode _dfindMode = DFindMode.Normal;
    private string _fftMode = "OFF";
    private double _frequency = 120d;
    private double _ifbandwidth = 150d;
    private bool _iqSwitch;
    private bool _ituSwitch;
    private bool _levelSwitch;
    private float _levelThreshold = -20.0f;
    private int _measureTime = -1; //单位us
    private Polarization _polarityType = Polarization.Vertical;
    private RfMode _rfmod = RfMode.Normal;
    private double _spectrumspan = 1000d;
    private bool _spectrumSwitch;
    private bool _squelchSwitch;
    private float _squelchThreshold;

    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位MHz")]
    [ValueRange(118.0, 450.0)]
    [DefaultValue(120d)]
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
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.WBDF /*| FeatureType.FScne | FeatureType.MScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱带宽")]
    [Description("频谱带宽设置")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|5|10|20|50|100|200|500|1000|2000|5000|10000|20000",
        DisplayValues = "|1kHz|2kHz|5kHz|10kHz|20kHz|50kHz|100kHz|200kHz|500kHz|1MHz|2MHz|5MHz|10MHz|20MHz"
    )]
    [DefaultValue(1000d)]
    public double SpectrumSpan
    {
        get => _spectrumspan;
        set
        {
            _spectrumspan = value;
            //AUTO ON是为了保证从测向功能切换到监测功能的时候能够顺利设置Span
            SendCmd("CALC:IFP:STEP:AUTO ON");
            SendCmd($"FREQ:SPAN {_spectrumspan}kHz");
            //WBDF在任务启动的时候再重新设置
        }
    }

    [PropertyOrder(9)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FFDF /* | FeatureType.SCAN | FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [Description("指定中频带宽、滤波带宽、解调带宽。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|1|1.5|2.1|2.4|2.7|3.1|4|4.8|6|9|12|15|25|30|50|75|120|150|250|300|500|800|1000|1250|1500|2000|5000|8000|10000|12500|15000|20000",
        DisplayValues =
            "|1kHz|1.5kHz|2.1kHz|2.4kHz|2.7kHz|3.1kHz|4kHz|4.8kHz|6kHz|9kHz|12kHz|15kHz|25kHz|30kHz|50kHz|75kHz|120kHz|150kHz|250kHz|300kHz|500kHz|800kHz|1MHz|1.25MHz|1.5MHz|2MHz|5MHz|8MHz|10MHz|12.5MHz|15MHz|20MHz"
    )]
    [DefaultValue(150d)]
    public double IfBandWidth
    {
        get => _ifbandwidth;
        set
        {
            _ifbandwidth = value;
            SendCmd($"SENS:BAND {_ifbandwidth}kHz");
        }
    }

    [PropertyOrder(11)]
    [Parameter(AbilitySupport =
        FeatureType.FFM |
        FeatureType.WBDF /*| FeatureType.SCAN | FeatureType.FScne | SpecificAbility.MSCAN | FeatureType.MScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("在 0~40 之间线性可调 单位dB。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|10|20|30|40",
        DisplayValues = "|自动|0dB|10dB|20dB|30dB|40dB"
    )]
    [DefaultValue(0f)]
    public float Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (_attenuation.Equals(-1f))
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
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.FFDF |
        FeatureType.WBDF /* | FeatureType.SCAN | FeatureType.FScne | SpecificAbility.MSCAN | FeatureType.MScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频模式")]
    [Description("控制射频模块的两种工作模式，常规模式，低失真模式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort",
        DisplayValues = "|常规模式|低失真模式"
    )]
    [DefaultValue(RfMode.Normal)]
    public RfMode RfMode
    {
        get => _rfmod;
        set
        {
            _rfmod = value;
            if (_rfmod.Equals(RfMode.Normal))
                SendCmd("INP:ATT:MODE NORM");
            else if (_rfmod.Equals(RfMode.LowDistort)) SendCmd("INP:ATT:MODE LOWD");
        }
    }

    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描类型")]
    [Description("频点扫描模式更准确|全景扫描模式速度快。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描"
    )]
    [DefaultValue(ScanMode.Fscan)]
    public ScanMode ScanMode { get; set; } = ScanMode.Fscan;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置频段扫描的起始频率，单位:MHz。")]
    [ValueRange(20.0, 3600.0)]
    [DefaultValue(88.0d)]
    public double StartFrequency { get; set; } = 88.0d;

    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描扫描步进.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|1.25|2|2.5|3.125|5|6.25|8.333|10|12.5|20|25|50|100|200|500|1000|2000"
        , DisplayValues = "|1kHz|1.25kHz|2kHz|2.5kHz|3.125kHz|5kHz|6.25kHz|8.333kHz|10kHz|12.5kHz|20kHz|25kHz|50kHz|100kHz|200kHz|500kHz|1MHz|2MHz")]
    [DefaultValue(25d)]
    public double StepFrequency
    {
        get;
        set;
        //// 频点扫描支持[1Hz,100MHz]任意的扫描步进，而全景扫描只支持以上步进为了使“频点扫描”和“全景扫描”使用统一的“扫描步进参数”，使扫描步进不可以任意输入
        //SendCmd(string.Format("SENS:SWE:STEP {0}MHz", _stepFrequency));
        //SendCmd(string.Format("SENS:PSC:STEP {0}MHz", _stepFrequency));
    } = 25d;

    [PropertyOrder(4)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置频段扫描的终止频率，单位为MHz。")]
    [ValueRange(20.0, 3600.0)]
    [DefaultValue(108.0d)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(21)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("保持时间")]
    [Description("设置保持时间，单位：秒（s）,如果信号在驻留时间内消失，则保持时间开始计时。一旦保持时间过期，扫描将继续下一个频率，即使驻留时间尚未过期。如果信号在保持时间内超过了静噪门限，则保持时间被重置")]
    [ValueRange(0, 10)]
    [DefaultValue(0.0f)]
    public float HoldTime { get; set; }

    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等(当带宽较小时频谱数据返回较慢，如果驻留时间设置较短，可能看不到频谱数据)")]
    [ValueRange(0, 10)]
    [DefaultValue(0.0f)]
    public float DwellTime { get; set; }

    [PropertyOrder(24)]
    [Parameter(AbilitySupport =
        FeatureType.FFM /*| FeatureType.SCAN | SpecificAbility.MSCAN | FeatureType.MScne | FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.Misc)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [Description("设置计算电平数据时的处理方式。")]
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
    [Parameter(AbilitySupport = FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("信道带宽")]
    [DefaultValue(200d)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|1.25|2|2.5|3.125|5|6.25|8.333|10|12.5|20|25|50|100|200|500|1000|2000",
        DisplayValues =
            "|1kHz|1.25kHz|2kHz|2.5kHz|3.125kHz|5kHz|6.25kHz|8.333kHz|10kHz|12.5kHz|20kHz|25kHz|50kHz|100kHz|200kHz|500kHz|1MHz|2MHz"
    )]
    [Description("将固定大小的频谱带宽划分为若干个信道，返回每一个信道的测向结果。")]
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
    [Parameter(AbilitySupport =
        FeatureType.FFM /* | FeatureType.SCAN | SpecificAbility.MSCAN | FeatureType.FScne | FeatureType.MScne*/)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|100|200|400|500|800|1000|2000|4000|5000|8000|10000",
        DisplayValues = "|自动|100ms|200ms|400ms|500ms|800ms|1s|2s|4s|5s|8s|10s")]
    [DefaultValue(-1)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    public int MeasureTime
    {
        get => _measureTime;
        set
        {
            _measureTime = value;
            if (value == -1)
                SendCmd("MEAS:TIME DEF");
            else
                SendCmd($"MEAS:TIME {_measureTime}ms");
        }
    }

    [PropertyOrder(23)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("积分时间")]
    [DefaultValue(0.1)]
    [ValueRange(0.1, 10)]
    [Description("测向值是一个平均值，测向时间是指求这个平均值的时间。单位(秒)")]
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
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [Description("正常：电平门限值有效，用于常规信号测向；/r/n门限：电平门限值有效，并且积累测量结果，用于突发信号测向；/r/n连续：电平门限值无效，用于弱小信号测向。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Gate|Continuous",
        DisplayValues = "|正常|门限|连续")]
    [DefaultValue(DFindMode.Normal)]
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
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DefaultValue(-20.0f)]
    [ValueRange(-50f, 130f)]
    [DisplayName("电平门限")]
    [Description("单位：dbuV；测向机在Normal或Gate模式下，测向电平门限起效。")]
    public float LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            SendCmd($"MEAS:DF:THR {_levelThreshold}");
        }
    }

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [Description("设置测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|1.25|2|2.5|3.125|5|6.25|8.333|10|12.5|20|25|50|100|200|500|1000|2000",
        DisplayValues =
            "|1kHz|1.25kHz|2kHz|2.5kHz|3.125kHz|5kHz|6.25kHz|8.333kHz|10kHz|12.5kHz|20kHz|25kHz|50kHz|100kHz|200kHz|500kHz|1MHz|2MHz"
    )]
    [DefaultValue(200d)]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            SetDfBandwidth(value);
        }
    }

    [PropertyOrder(17)]
    [Parameter(AbilitySupport = FeatureType.FFM /* | SpecificAbility.MSCAN | FeatureType.SCAN*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("FFT模式")]
    [Description("设置中频数据取值的方式。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|MIN|MAX|SCAL|OFF",
        DisplayValues = "|最小值|最大值|平均值|关闭"
    )]
    [DefaultValue("OFF")]
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
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FFDF /*| FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|LSB|USB",
        DisplayValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|LSB|USB"
    )]
    [DefaultValue(Modulation.Fm)]
    public Modulation Demodulation
    {
        get => _dem;
        set
        {
            _dem = value;
            SendCmd($"SENS:DEM {_dem}");
        }
    }

    [PropertyOrder(19)]
    [Parameter(AbilitySupport = FeatureType.FFM /* | FeatureType.MScne | FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，单位dBuV。")]
    [DefaultValue(0f)]
    [ValueRange(0f, 100f)]
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
    [Parameter(AbilitySupport = FeatureType.FFM /*| FeatureType.MScne | FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("静噪门限开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关"
    )]
    [Description("设置是否打开静噪门限。")]
    [DefaultValue(false)]
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
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FFDF /*| FeatureType.MScne | FeatureType.FScne*/)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
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
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    [PropertyOrder(29)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [Description("IQ数据开关。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    public bool IqSwitch
    {
        get => _iqSwitch;
        set
        {
            _iqSwitch = value;
            if (_iqSwitch)
                SendCmd("SYSTem:IF:REMote:MODe SHORT");
            else
                SendCmd("SYSTem:IF:REMote:MODe OFF");
            if (value)
                _media |= MediaType.Iq;
            else
                _media &= ~MediaType.Iq;
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    [PropertyOrder(30)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [Description("ITU数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    [PropertyOrder(31)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [Description("电平数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    [PropertyOrder(32)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FFDF /*| FeatureType.FScne | FeatureType.MScne*/)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    [PropertyOrder(35)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("DDF255接收机接收指令的IP地址。")]
    [DefaultValue("127.0.0.1")]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(36)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("RS接收机接收指令通讯端口号。")]
    [DefaultValue(5555)]
    public int TcpPort { get; set; } = 5555;

    [PropertyOrder(37)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向天线端口")]
    [Description("设置测向天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|1|2",
        DisplayValues = "|Auto|VUHF1|VUHF2|VUHF3"
    )]
    [DefaultValue(-1)]
    public int DdfAntenna { get; set; } = 2;

    [PropertyOrder(38)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("监测天线端口")]
    [Description("设置监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|1|2",
        DisplayValues = "|Auto|VUHF1|VUHF2|VUHF3"
    )]
    [DefaultValue(-1)]
    public int MonitorAntenna { get; set; } = 2;

    [PropertyOrder(39)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [Description("设置测向极化方式，设备将结合当前频率自动选择合适的天线用于测向")]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化")]
    [DefaultValue(Polarization.Vertical)]
    public Polarization Polar
    {
        get => _polarityType;
        set
        {
            _polarityType = value;
            if (value == Polarization.Vertical)
                SendCmd("ROUT:POL VERT");
            else
                SendCmd("ROUT:POL HOR");
        }
    }

    [PropertyOrder(40)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集GPS数据")]
    [Description("设置是否通过DDF255接收机采集GPS信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(false)]
    public bool UseGps { get; set; }

    [PropertyOrder(44)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集电子罗盘数据")]
    [Description("设置是否通过DDF200M接收机采集电子罗盘信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(false)]
    public bool UseCompass { get; set; }
}