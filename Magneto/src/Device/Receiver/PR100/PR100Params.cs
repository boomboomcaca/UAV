using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.PR100;

[DeviceDescription(Name = "PR100",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM | FeatureType.SCAN,
    MaxInstance = 1,
    Version = "1.5.4",
    DeviceCapability = "20|3600|9600",
    Model = "PR100",
    Description = "PR100接收机 支持20MHz~3600MHz")]
public partial class Pr100
{
    private bool _attCtrlType;
    private int _attenuation;
    private Modulation _demMode = Modulation.Fm;
    private DetectMode _detector = DetectMode.Fast;
    private string _fftMode = "OFF";
    private double _filterBandwidth = 120.0d;
    private double _frequency = 101.7d;
    private double _ifBandwidth = 256d;
    private SegmentTemplate[] _segments;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("单频监测时的中心频率,单位MHz")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [DefaultValue(101.7d)]
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

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.MScan | FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [Description("设置中频带宽，单位kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|9600|4800|2400|1200|800|600|400|300|256|150|100|50|25|10",
        DisplayValues =
            "|9.6MHz|4.8MHz|2.4MHz|1.2MHz|800kHz|600kHz|400kHz|300kHz|256kHz|150kHz|100kHz|50kHz|25kHz|10kHz"
    )]
    [DefaultValue(256d)]
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
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置扫描的起始频率，单位:MHz。")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency { get; set; } = 87.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置扫描的终止频率，单位为MHz。")]
    [ValueRange(20.0d, 3600.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency { get; set; } = 108.0d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|100|50|25|12.5|6.25|3.125|2.5|1.25|0.625|0.5|0.25|0.125",
        DisplayValues = "|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|2.5kHz|1.25kHz|625Hz|500Hz|250Hz|125Hz"
    )]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描模式")]
    [Description("频点扫描模式更准确,全景扫描模式速度快。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描")]
    [DefaultValue(ScanMode.Pscan)]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    [PropertyOrder(6)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MscanTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = null;

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

    [PropertyOrder(7)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [Description("滤波带宽、解调带宽 默认单位 kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10000|5000|2000|1500|1250|1000|800|500|300|250|150|120|50|30|15|12|9|6|2.4|1.5|0.6|0.3|0.15",
        DisplayValues =
            "|10MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|2.4kHz|1.5kHz|600Hz|300Hz|150Hz"
    )]
    [DefaultValue(120.0d)]
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
                                | FeatureType.MScan
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
                SendCmd("GCONtrol:MODE AUTO");
            }
            else
            {
                SendCmd("GCONtrol:MODE MGC");
                SendCmd($"GCONtrol:{_attenuation}");
            }
        }
    }

    [PropertyOrder(9)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.SCAN | FeatureType.MScan | FeatureType.MScne | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("衰减控制。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|110|100|90|80|70|60|50|40|30|20|10|0|-10|-20|-30",
        DisplayValues = "|110dB|100dB|90dB|80dB|70dB|60dB|50dB|40dB|30dB|20dB|10dB|0dB|-10dB|-20dB|-30dB")]
    [DefaultValue(0)]
    [ValueRange(-30, 110)]
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
                SendCmd("GCONtrol:MODE AUTO");
            }
            else
            {
                SendCmd("GCONtrol:MODE MGC");
                SendCmd($"GCONtrol:{_attenuation}");
            }
        }
    }

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne)]
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
        get => _demMode;
        set
        {
            _demMode = value;
            SendCmd($"SENS:DEM {_demMode}");
        }
    }

    [PropertyOrder(2)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("等待时间（秒）")]
    [Description("等待时间，单位：秒。 如在【驻留时间】内，电平低于门限，则继续等待，如果超过了【等待时间】，电平依然低于门限，则继续测量下个频点；如果电平超过门限，则继续测量，直到超过【驻留时间】。")]
    [ValueRange(0, 10, 0.1)]
    [DefaultValue(0.0f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float HoldTime { get; set; } = 0.0f;

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Name(ParameterNames.DwellTime)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间（秒）")]
    [Description("驻留时间，单位：秒。如电平值超过门限值，则在该频率上继续测量的时间长度。")]
    [ValueRange(0, 10, 0.1)]
    [DefaultValue(0.0f)]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime { get; set; } = 0.0f;

    [PropertyOrder(16)]
    [Name(ParameterNames.FftMode)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
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

    [PropertyOrder(14)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
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

    #region IDataSwitch Implement

    private bool _audioSwitch;

    [PropertyOrder(20)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("是否监听音频。")]
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
            {
                _mediaType |= MediaType.Audio;
                //音频开启的时候开启自动消除干扰信号
                SendCmd("OUTP:FILT:MODE NOTCH");
                var audioMode = SendSyncCmd("SYST:AUD:REM:MODE?\r\n");
                while (!audioMode.Contains("2"))
                {
                    SendCmd("SYST:AUD:REM:MODE 2");
                    audioMode = SendSyncCmd("SYST:AUD:REM:MODE?\r\n");
                }
            }
            else
            {
                SendCmd("OUTP:FILT:MODE OFF");
                _mediaType &= ~MediaType.Audio;
            }

            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    private bool _iqSwitch;

    [PropertyOrder(21)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [Description("IQ数据开关。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
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
                _mediaType |= MediaType.Iq;
            else
                _mediaType &= ~MediaType.Iq;
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(24)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关。")]
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
                _mediaType |= MediaType.Spectrum;
            else
                _mediaType &= ~MediaType.Spectrum;
            if (TaskState == TaskState.Start) InitUdpPath();
        }
    }

    private float _squelchThreshold = -20f;

    [PropertyOrder(25)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，单位dBuV。")]
    [DefaultValue(-20f)]
    [ValueRange(-30f, 130f)]
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

    private bool _squelchSwitch;

    [PropertyOrder(23)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
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
                SendCmd("OUTP:SQU:STAT ON");
                SendCmd($"OUTP:SQU:THR {_squelchThreshold} dBuV");
            }
            else
            {
                SendCmd("OUTP:SQU:STAT OFF");
            }
        }
    }

    #endregion IDataSwitch Implement

    #region 安装参数

    [PropertyOrder(28)]
    [Name(ParameterNames.IpAddress)]
    [Category(PropertyCategoryNames.Installation)]
    [Parameter(IsInstallation = true)]
    [DisplayName("IP地址")]
    [Description("PR100接收机接收指令的IP地址。")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(29)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [Parameter(IsInstallation = true)]
    [DisplayName("TCP端口")]
    [Description("PR100接收机接收指令通讯端口号。")]
    [DefaultValue(5555)]
    [ValueRange(1024, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 5555;

    #endregion 安装参数
}