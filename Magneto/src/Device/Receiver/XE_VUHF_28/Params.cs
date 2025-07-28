using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF_28;

[DeviceDescription(Name = "XE_VUHF_28",
    Manufacturer = "Thales",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring | ModuleCategory.AntennaControl,
    FeatureType = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.MScan | FeatureType.ScanDF |
                  FeatureType.FFDF | FeatureType.IFMCA | FeatureType.WBDF,
    MaxInstance = 1,
    Model = "XE_VUHF_28",
    Version = "1.3.2",
    DeviceCapability = "20|3000|40000",
    Description = "XE超短波接收机,适用于版本号为28的接收机")]
public partial class XeVuhf28
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
    [Unit(UnitNames.MHz)]
    [Description("中心频率，单位MHz")]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetCenterFrequency(value);
        }
    }

    private double _ifBandWidth = 500d;

    [PropertyOrder(9)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues = "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(500d)]
    [Unit(UnitNames.KHz)]
    [Description("中频带宽、滤波带宽、解调带宽")]
    public double IfBandWidth
    {
        get => _ifBandWidth;
        set
        {
            _ifBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetIfBandwidth(value);
        }
    }

    private double _filterBandWidth = 150d;

    [PropertyOrder(9)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10000|5000|2000|1000|600|300|200|150|100|50|25|12.5|8.333|6.25|4|3.125|1|0.6|0.3",
        DisplayValues =
            "|10MHz|5MHz|2MHz|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|8.333kHz|6.25kHz|4kHz|3.125kHz|1kHz|600Hz|300Hz")]
    [DefaultValue(150d)]
    [Unit(UnitNames.KHz)]
    [Description("滤波带宽、解调带宽")]
    public double FilterBandwidth
    {
        get => _filterBandWidth;
        set
        {
            _filterBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetFilterBandwidth(value);
        }
    }

    private double _dfBandWidth = 150d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|40000|20000|10000|5000|2000|1000|600|300|200|150|100|50|25|12.5|6.25|3.125",
        DisplayValues =
            "|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|600kHz|300kHz|200kHz|150kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(150d)]
    [Description("设置被测信号的测向带宽，取值为中心频率前后频率上下限之和")]
    [Unit(UnitNames.KHz)]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetDfBandwidth(value);
        }
    }

    private double _resolutionBandwidth = 25d;

    [PropertyOrder(2)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向分辨率")]
    [Description("设置测向分辨率")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|50|25|12.5|6.25|3.125|1.5625|0.78125",
        DisplayValues = "|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|1.5625kHz|0.78125kHz")]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    public double ResolutionBandwidth
    {
        get => _resolutionBandwidth;
        set
        {
            _resolutionBandwidth = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetResolution(value);
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
    [Unit(UnitNames.MHz)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetStartFrequency(value);
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
    [Unit(UnitNames.MHz)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetStopFrequency(value);
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
        StandardValues = "|50|25|12.5|6.25|3.125|1.5625",
        DisplayValues = "|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz|1.5625kHz")]
    [DefaultValue(25d)]
    [Browsable(false)]
    [Unit(UnitNames.KHz)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetResolution(value);
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
        Template = typeof(MScanTemplate))]
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
    public int MaxChanCount { get; set; } = 4;

    [PropertyOrder(8)]
    [Name("maxAudioCount")]
    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Category(PropertyCategoryNames.Measurement)]
    [ReadOnly(true)]
    [DisplayName("音频数量")]
    [Description("设置窄带最多支持的音频通道数。")]
    [DefaultValue(2)]
    public int MaxAudioCount { get; set; } = 2;

    #endregion

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
    [Unit(UnitNames.DBuV)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetLevelThreshold(value);
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
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetQualityThreshold(value);
        }
    }

    private int _attenuation = -1;

    [PropertyOrder(17)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN | FeatureType.FFDF |
                                FeatureType.MScan | FeatureType.ScanDF | FeatureType.IFMCA | FeatureType.MScan |
                                FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [Description("设置衰减值，单位:dB")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|-1|0|5|10|15|20|25|30|35|40|45",
        DisplayValues = "|自动|0|5|10|15|20|25|30|35|40|45")]
    [ValueRange(-1, 46)]
    [DefaultValue(-1)]
    [Unit(UnitNames.Db)]
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
    [Unit(UnitNames.DBuV)]
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

    #endregion

    #region 高级参数

    [PropertyOrder(11)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG",
        DisplayValues = "|快速|峰值|均值")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理频谱的检波方式")]
    public DetectMode Detector { get; set; } = DetectMode.Fast;

    [PropertyOrder(11)]
    [Name("sweepNumLoops")]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("统计次数")]
    [ValueRange(1, 10)]
    [DefaultValue(3)]
    [Description("用于实现峰值和均值检波，注意若统计次数设置过多且频段扫描点数也过多时数据返回较缓慢。")]
    public int SweepNumLoops { get; set; } = 3;

    private float _xdB = 26.0f;

    [PropertyOrder(25)]
    [Name(ParameterNames.Xdb)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("XdB带宽")]
    [ValueRange(0.0f, 200.0f)]
    [DefaultValue(26.0f)]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Unit(UnitNames.Db)]
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
    [Unit(UnitNames.Pct)]
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
    [Name(ParameterNames.IntegrationTime)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|10|20|50|100|200|400|600|800|1000",
        DisplayValues = "|0|10ms|20ms|50ms|100ms|200ms|400ms|600ms|800ms|1s")]
    [Browsable(false)]
    [DefaultValue(0)]
    [Unit(UnitNames.Ms)]
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

    [PropertyOrder(17)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("等待时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5|4|3|2|1",
        DisplayValues = "|5s|4s|3s|2s|1s")]
    [Description("设置等待时间，单位：秒（s），保持特定时间进行信号搜索，等待信号出现")]
    [ValueRange(1f, 300.0f)]
    [DefaultValue(1f)]
    [Unit(UnitNames.Sec)]
    public double HoldTime { get; set; } = 1f;

    [PropertyOrder(18)]
    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Category(PropertyCategoryNames.Scan)]
    [Name(ParameterNames.DwellTime)]
    [DisplayName("驻留时间")]
    [Description("设置驻留时间，单位：秒（s），当信号电平超过门限值时，在该频率上驻留特定时间分析信号强度、频谱、音频等")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10|7|5|4|3|2|1|0.5|0.2|0.1",
        DisplayValues =
            "|10s|7s|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0.1f, 300.0f)]
    [DefaultValue(0.1f)]
    [Unit(UnitNames.Sec)]
    public float DwellTime { get; set; } = 0.1f;

    //1/98，短波为1/66
    private bool _ampliConfig;

    [PropertyOrder(37)]
    [Name(ParameterNames.PreAmpSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.ScanDF | FeatureType.MScan | FeatureType.IFMCA | FeatureType.WBDF)]
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
        }
    }

    #endregion

    #region 数据开关

    [Parameter(AbilitySupport = FeatureType.MScan)]
    [Name(ParameterNames.DwellSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("驻留开关")]
    [Description("切换离散扫描与驻留离散扫描")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(10)]
    public bool DwellSwitch { get; set; }

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
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetItuSwitch(value);
        }
    }

    private bool _spectrumSwitch = true;

    [PropertyOrder(28)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.MScan)]
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

    private bool _levelSwitch;

    [Parameter(AbilitySupport = FeatureType.FFDF)]
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

    private bool _audioSwitch = true;

    [PropertyOrder(29)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.MScan)]
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

    private bool _iqSwitch;

    [PropertyOrder(29)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [Description("设置是否获取IQ数据。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
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
            if (TaskState == TaskState.Start && _cmdCollector != null) _cmdCollector.SetIqSwitch(value);
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
    [DefaultValue("192.168.0.1")]
    public string Ip { get; set; } = "192.168.0.1";

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

    [Parameter(IsInstallation = true)]
    [Name("fmFilter")]
    [PropertyOrder(38)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("FM陷波")]
    [Description("陷波器打开:抑制88-108广播频段。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    public bool FmFilter { get; set; }

    [Name("detectionMode")]
    [Parameter(IsInstallation = true)]
    [PropertyOrder(39)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("侦测模式")]
    [Description("侦测模式，1：积分前检测 N:积分后检测")]
    [ValueRange(0, 5)]
    [DefaultValue(1)]
    [Browsable(false)]
    public ushort DetectionMode { get; set; } = 1;

    [Parameter(IsInstallation = true)]
    [PropertyOrder(40)]
    [Name("xeIntTime")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("NT模式积分时间")]
    [Description("NT模式积分时间，单位：ms，侦测模式大于1时生效")]
    [ValueRange(0, 65536)]
    [DefaultValue(10000)]
    [Browsable(false)]
    public int XeIntTime { get; set; } = 10000;

    #endregion
}