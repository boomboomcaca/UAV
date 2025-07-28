using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF;

[DeviceDescription(Name = "XE_VUHF",
    Manufacturer = "Thales",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring | ModuleCategory.AntennaControl,
    FeatureType = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.ScanDF | FeatureType.FFDF |
                  FeatureType.IFMCA | FeatureType.WBDF,
    MaxInstance = 1,
    Model = "XE_VUHF",
    DeviceCapability = "20|3000|1000",
    Version = "1.0.3",
    Description = "XE超短波接收机，适用于版本号为21和25的接收机")]
public partial class XeVuhf
{
    #region 常规参数

    private double _frequency = 102.6d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.IFMCA | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20d, 3000.0d)]
    [DefaultValue(102.6d)]
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

    private double _ifBandWidth = 150d;

    [PropertyOrder(9)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [DefaultValue(150d)]
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

    private double _dfBandWidth = 150d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|600|300|200|150|100|50|25|12.5|8|6.25|4|3.125|1",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8kHz|6.25kHz|4kHz|3.125kHz|1kHz")]
    [DefaultValue(150d)]
    [Description("设置被测信号的测向带宽，取值为中心频率前后频率上下限之和")]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetDfBandwidth((uint)(value * 1000));
        }
    }

    private double _channelBandWidth = 25d;

    [PropertyOrder(9)]
    [Name("channelBandwidth")]
    [Parameter(AbilitySupport = FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("信道带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|100|50|25|12.5|6.25",
        DisplayValues = "|100kHz|50kHz|25kHz|12.5kHz|6.25kHz")]
    [DefaultValue(25d)]
    [Description("宽带测向时，设置每个信道的带宽。")]
    public double ChannelBandWidth
    {
        get => _channelBandWidth;
        set
        {
            _channelBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
                _cmdCollector.SetResolution((uint)(value * 1000));
        }
    }

    private double _startFrequency = 88.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(20d, 3000d)]
    [DefaultValue(88.0d)]
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

    private double _stopFrequency = 108.0d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(20d, 3000d)]
    [DefaultValue(108.0d)]
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

    private double _stepFrequency = 25d;

    [PropertyOrder(5)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进，单位kHz。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|100|50|25|12.5|6.25|3.125|1.5625",
        DisplayValues = "|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|1.5625kHz")]
    [DefaultValue(25d)]
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

    [PropertyOrder(5)]
    [Name("chan")]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("通道号")]
    [Description("设置频段扫描数据来源通道。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2",
        DisplayValues = "|1|2")]
    [DefaultValue(1)]
    [Browsable(false)]
    public int Chan { get; set; } = 1;

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
    [Description("快速模式具有较高的测向时效，灵敏模式则具有较高的灵敏度但时效较低。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2",
        DisplayValues = "|快速|常规|灵敏")]
    [DefaultValue(1)]
    [Browsable(false)]
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
    private int _levelThreshold = 10;

    [PropertyOrder(15)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.ScanDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电平门限")]
    [Description("设置测向门限，当信号电平超过门限时返回测向结果，单位dBuV")]
    [ValueRange(-67, 127)]
    [DefaultValue(10)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetLevelThreshold(value - 107);
        }
    }

    private int _qualityThreshold;

    [PropertyOrder(16)]
    [Name(ParameterNames.QualityThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，仅当测向质量超过门限时才返回测向数据")]
    [ValueRange(0, 100)]
    [DefaultValue(0)]
    [Unit(UnitNames.Pct)]
    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            _qualityThreshold = value;
            if (TaskState == TaskState.Start && _cmdCollector != null)
            {
                var qualityMark = (ushort)(value == 100 ? 9 : value / 10);
                _cmdCollector.SetQualityThreshold(qualityMark);
            }
        }
    }

    private int _attenuation = -1;

    [PropertyOrder(17)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan | FeatureType.SCAN |
                                FeatureType.FFDF | FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan |
                                FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("设置衰减值，单位:dB")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|-1|0|5|10|15|20|25|30|35|40|45|50",
        DisplayValues = "|自动|0|5|10|15|20|25|30|35|40|45|50")]
    [ValueRange(-1, 51)]
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

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan)]
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

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan)]
    [Name(ParameterNames.SquelchSwitch)]
    [PropertyOrder(21)]
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

    #endregion

    #region 高级参数

    [PropertyOrder(11)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|PEAK|AVE",
        DisplayValues = "|快速|峰值|均值")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理频谱的检波方式")]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(11)]
    [Name("sweepNumLoops")]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("统计次数")]
    [ValueRange(1, 10)]
    [DefaultValue(3)]
    [Description("用于实现峰值和均值检波，注意若统计次数设置过多且频段扫描点数也过多时数据返回较缓慢。")]
    public int SweepNumLoops { get; set; } = 3;

    //1/98，短波为1/66
    private bool _ampliConfig;

    [PropertyOrder(11)]
    [Name("ampliConfig")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FFDF |
                                FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("放大器")]
    [Description("放大器开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|false|true",
        DisplayValues = "|关|开")]
    [DefaultValue(false)]
    public bool AmpliConfig
    {
        get => _ampliConfig;
        set
        {
            _ampliConfig = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetAmpli(value);
        } //TODO:
    }

    //TODO：考虑其它功能是否需要暴露
    private bool _fmFilter;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.MScan)]
    [Name("fmFilter")]
    [PropertyOrder(13)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("FM陷波")]
    [Description("陷波器打开:抑制88-108广播频段。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool FmFilter
    {
        get => _fmFilter;
        set
        {
            _fmFilter = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetFmFilter(value);
        }
    }

    private bool _sensitivity;

    [Parameter(AbilitySupport = FeatureType.ScanDF | FeatureType.IFMCA)]
    [Name("sensitivity")]
    [PropertyOrder(12)]
    [Category(PropertyCategoryNames.Misc)]
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
    [Category(PropertyCategoryNames.Misc)]
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
    //暂不使用
    private float _integralTime;

    [PropertyOrder(24)]
    [Name("integrationTime")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|10|20|50|100|200|400|600|800|1000",
        DisplayValues = "|0|10ms|20ms|50ms|100ms|200ms|400ms|600ms|800ms|1s")]
    [Browsable(false)]
    [DefaultValue(0)]
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