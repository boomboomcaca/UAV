using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Device.XE_HF.Protocols;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_HF;

[DeviceDescription(Name = "XE_HF",
    Manufacturer = "Thales",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring | ModuleCategory.AntennaControl,
    FeatureType = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FFDF,
    MaxInstance = 1,
    Model = "XE_HF",
    DeviceCapability = "1|30|300",
    Version = "1.0.3",
    Description = "XE短波接收机")]
public partial class XeHf
{
    #region 常规参数

    private double _frequency = 10.6d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(10.6d)]
    [Description("中心频率，单位MHz")]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetCenterFrequency((uint)(value * 1000000));
        }
    }

    private double _ifBandWidth = 25d;

    [PropertyOrder(9)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|300|100|50|25|20|12.5|8|6.25|4|3|2|1|0.6|0.3|0.1",
        DisplayValues = "|300kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|8kHz|6.25kHz|4kHz|3kHz|2kHz|1kHz|600Hz|300Hz|100Hz")]
    [DefaultValue(25d)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    public double IfBandWidth
    {
        get => _ifBandWidth;
        set
        {
            _ifBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetBandwidth((uint)(value * 1000));
        }
    }

    //TODO:仅用于中频多路使用，且固定为40MHz
    [PropertyOrder(9)]
    [Name("spectrumSpan")]
    [Parameter(AbilitySupport = FeatureType.IFMCA | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000",
        DisplayValues = "|40MHz")]
    [DefaultValue(40000d)]
    [Description("频谱带宽")]
    public double SpectrumSpan { get; set; } = 40000d;

    private double _dfBandWidth = 25d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|300|100|50|25|20|12.5|8|6.25|4|3|2|1|0.6|0.3|0.1",
        DisplayValues = "|300kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|8kHz|6.25kHz|4kHz|3kHz|2kHz|1kHz|600Hz|300Hz|100Hz")]
    [DefaultValue(25d)]
    [Description("设置被测信号的测向带宽，取值为中心频率前后频率上下限之和")]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetBandwidth((uint)(value * 1000));
        }
    }

    private double _startFrequency = 8.8d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(8.8d)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetStartFrequency((uint)(value * 1000000));
        }
    }

    private double _stopFrequency = 10.8d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(1d, 30d)]
    [DefaultValue(10.8d)]
    [Description("设置扫描终止频率，单位MHz")]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetStopFrequency((uint)(value * 1000000));
        }
    }

    private double _stepFrequency = 1.5625d;

    [PropertyOrder(5)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进，单位kHz。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|3.125|1.5625|0.78125|0.390625",
        DisplayValues = "|3.125kHz|1.5625kHz|781.25Hz|390.625Hz")]
    [DefaultValue(1.5625d)]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetResolution(value * 1000);
        }
    }

    private SegmentTemplate[] _segments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    public Dictionary<string, object>[] ScanSegments
    {
        get => null;
        set
        {
            if (value == null) return;
            _segments = Array.ConvertAll(value, item => (SegmentTemplate)item);
        }
    }

    [PropertyOrder(6)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan,
        Template = typeof(DiscreteFrequencyTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    public Dictionary<string, object>[] MscanPoints { get; set; }

    #region 中频多路

    private Dictionary<string, object>[] _ddcChannels;

    [PropertyOrder(7)]
    [Name(ParameterNames.DdcChannels)]
    [Parameter(AbilitySupport = FeatureType.IFMCA, Template = typeof(IfMultiChannelTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测量信道")]
    [Description("设置中频多路参数")]
    public Dictionary<string, object>[] DdcChannels
    {
        get => _ddcChannels;
        set
        {
            _ddcChannels = value;
            // 通道改变，需要重新设置参数
            //TODO:
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetIfMultiChannels(value);
        }
    }

    [PropertyOrder(8)]
    [Name("maxChanCount")]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [ReadOnly(true)]
    [DisplayName("窄带数量")]
    [Description("设置最多支持的窄带通道数。")]
    [DefaultValue(4)]
    public int MaxChanCount { get; set; }

    [PropertyOrder(8)]
    [Name("maxAudioCount")]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [ReadOnly(true)]
    [DisplayName("音频数量")]
    [Description("设置窄带最多支持的音频通道数。")]
    [DefaultValue(2)]
    public int MaxAudioCount { get; set; }

    #endregion

    private int _dfindMode = 1;

    [PropertyOrder(14)]
    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [Description("连续测向模式则既无音频也无频谱数据输出以达到硬件提供的最佳测向灵敏度(灵敏度模式)和高速特性(快速模式)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2",
        DisplayValues = "|常规模式|连续测向")]
    [DefaultValue(1)]
    public int DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetDfMode((XedFindMode)value);
        }
    }

    //TODO：设备里边对应的单位是dbm,此处单位是dbuv,使用时注意转换
    private int _levelThreshold = -67;

    [PropertyOrder(15)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电平门限")]
    [Description("设置测向门限，当信号电平超过门限时返回测向结果，单位dBuV")]
    [ValueRange(-67, 127)]
    [DefaultValue(-67)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetLevelThreshold(value - 107);
        }
    }

    private int _attenuation = -1;

    [PropertyOrder(17)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FFDF |
                                FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("设置衰减值，可手动输入，最大可到95dB.")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|-1|0|5|10|15|20|25|30|35|40|50|60|70|80|90",
        DisplayValues = "|自动|0|5|10|15|20|25|30|35|40|50|60|70|80|90")]
    [ValueRange(-1, 95)]
    [DefaultValue(-1)]
    public int Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetAttenuation(_attenuation);
        }
    }

    //TODO:和设备的解调模式不对应需要转换
    private Modulation _demMode = Modulation.Fm;

    [PropertyOrder(20)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|LSB|USB|CW|PM",
        DisplayValues = "|AM|FM|LSB|USB|CW|PM")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetDemodulation(XeAssister.GetDemoduMode(value));
        }
    }

    //TODO：设备里边对应的单位是dbm,此处单位是dbuv,使用时注意转换
    private int _squelchThreshold = -20;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [PropertyOrder(21)]
    [Name(ParameterNames.SquelchThreshold)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限的值，单位dBuv。")]
    [ValueRange(-67, 127)]
    [DefaultValue(-20f)]
    public int SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetSquelchThreshold(value - 107);
        }
    }

    private bool _squelchSwitch;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [PropertyOrder(21)]
    [Name(ParameterNames.SquelchSwitch)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪开关")]
    [Description("静噪开关打开时静噪门限才生效，否则音频数据将会一直输出不受静噪门限的限制。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetSquelchSwitch(value);
        }
    }

    [PropertyOrder(10)]
    [Name(ParameterNames.ScanMode)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描模式")]
    [Description("常规对应于快速模式,灵敏对应于灵敏模式且速度最慢.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2",
        DisplayValues = "|常规|灵敏")]
    [DefaultValue(1)]
    [Browsable(false)] //不再对外公布
    public int ScanMode
    {
        get;
        set;
        //频段扫描运行时不修改参数
    } = 1;

    #endregion

    #region 高级参数

    //TODO:确认是否需要，LG319上连接真实设备并无此参数，此处先设置为隐藏参数，后期若需要再启用
    //短波为1/66
    private bool _ampliConfig;

    [PropertyOrder(11)]
    [Name("ampliConfig")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FFDF |
                                FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("放大器")]
    [Description("放大器开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|false|true",
        DisplayValues = "|关|开")]
    [DefaultValue(false)]
    [Browsable(false)]
    public bool AmpliConfig
    {
        get => _ampliConfig;
        set
        {
            _ampliConfig = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetAmpli(value);
        } //TODO:
    }

    private bool _sensitivity;

    [Name("sensitivity")]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan)]
    [PropertyOrder(12)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("灵敏度模式")]
    [Description("设置接收机灵敏度模式，灵敏模式时接收机内部会积分处理，切换时会有短暂的停顿为正常现象。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|灵敏|快速")]
    [DefaultValue(false)]
    public bool Sensitivity
    {
        get => _sensitivity;
        set
        {
            _sensitivity = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetSensitivity(value);
        }
    }

    private ushort _detectionMode = 1;

    [Name("detectionMode")]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FFDF | FeatureType.FFM | FeatureType.ITUM)]
    [PropertyOrder(12)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("侦测周期")]
    [Description("")]
    [ValueRange(0, 5)]
    [DefaultValue(1)]
    [Browsable(false)] //测试参数不对外暴露
    public ushort DetectionMode
    {
        get => _detectionMode;
        set
        {
            _detectionMode = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetDetectionMode(value);
        }
    }

    private int _intTime = 10000;

    [Name("xeIntTime")]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FFDF | FeatureType.FFM | FeatureType.ITUM)]
    [PropertyOrder(12)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("")]
    [ValueRange(0, 65536)]
    [DefaultValue(10000)]
    [Browsable(false)] //测试参数不对外暴露
    public int XeIntTime
    {
        get => _intTime;
        set
        {
            _intTime = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetIntegrationTime((ushort)value);
        }
    }

    private float _xdB = 26.0f;

    [PropertyOrder(25)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 200.0f)]
    [DefaultValue(26.0f)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    public float XdB
    {
        get => _xdB;
        set
        {
            _xdB = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetXdBAndBeta(value, _beta);
        }
    }

    private float _beta = 1f;

    [PropertyOrder(26)]
    [Name(ParameterNames.BetaValue)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("β%带宽")]
    [ValueRange(0.0f, 100.0f)]
    [DefaultValue(1f)]
    [Description("设置ITU测量中β值，单位：%")]
    public float Beta
    {
        get => _beta;
        set
        {
            _beta = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetXdBAndBeta(_xdB, value);
        }
    }

    //TODO:该参数为软件实现，并非真正意义上的积分时间，只是返回该时间段内测向质量最大的示向度
    //该做法建立在实践后测向质量越高测向结果越准确的基础上
    private float _integralTime;

    [PropertyOrder(24)]
    [Name("integrationTime")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|10|20|50|100|200|400|600|800|1000",
        DisplayValues = "|不积分|10ms|20ms|50ms|100ms|200ms|400ms|600ms|800ms|1s")]
    [DefaultValue(0)]
    [Browsable(false)]
    public float IntegrationTime
    {
        get => _integralTime;
        set
        {
            _integralTime = value;
            if (TaskState == TaskState.Start)
            {
                if (value == 0)
                    _watcherIntegrationTime.Reset();
                else
                    _watcherIntegrationTime.Start();
            }
        }
    }

    #endregion

    #region 数据开关

    private bool _ituSwitch;

    [PropertyOrder(27)]
    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [Description("设置是否进行ITU测量，并获取数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
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

    private bool _spectrumSwitch = true;

    [PropertyOrder(28)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("设置是否获取频谱数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
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
        }
    }

    private bool _audioSwitch = true;

    [PropertyOrder(29)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [Description("设置是否获取音频数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
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
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetAudioSwitch(value);
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(30)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("Thales设备IP地址。")]
    [DefaultValue("22.250.11.2")]
    public string Ip { get; set; } = "22.250.11.2";

    [PropertyOrder(31)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("Thales设备TCP通讯端口号。")]
    [DefaultValue(49000)]
    public int Port { get; set; } = 49000;

    [PropertyOrder(35)]
    [Name("gpsSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("GPS数据")]
    [Description("是否从XE获取GPS数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool GpsSwitch { get; set; }

    [PropertyOrder(36)]
    [Name("compassSwitch")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("罗盘数据")]
    [Description("是否从XE获取罗盘数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool CompassSwitch { get; set; }

    #endregion
}