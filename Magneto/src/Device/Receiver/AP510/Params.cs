using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.AP510;

[DeviceDescription(Name = "AP510",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.DirectionFinding
                     | ModuleCategory.Monitoring
                     | ModuleCategory.AntennaControl,
    FeatureType = FeatureType.FFM | FeatureType.ITUM
                                  | FeatureType.FFDF
                                  | FeatureType.WBDF
                                  | FeatureType.SCAN
                                  | FeatureType.MScan
                                  | FeatureType.MScne,
    MaxInstance = 1,
    Model = "AP510",
    DeviceCapability = "20|3600|20000",
    Version = "1.2.5",
    Description = "AP510接收机")]
public partial class Ap510
{
    #region 常规参数

    //private string _freQuency = "101.7"; // 设备参数

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 3600.0d)]
    [DefaultValue(101.7d)]
    [Description("单频监测时的中心频率,单位MHz")]
    [Unit(UnitNames.MHz)]
    public double Frequency
    {
        get;
        set;
        //_freQuency = _frequency.ToString();
    } = 101.7d;

    //private string _banDwidth = "200"; // 设备参数

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|250|200|150|125|100|80|60|50|40|30|25|20|15|12.5|10|8|6|5|4|3|2.5|2|1.5|1.2|1|0.8|0.6",
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|250kHz|200kHz|150kHz|125kHz|100kHz|80kHz|60kHz|50kHz|40kHz|30kHz|25kHz|20kHz|15kHz|12.5kHz|10kHz|8kHz|6kHz|5kHz|4kHz|3kHz|2.5kHz|2kHz|1.5kHz|1.2kHz|1kHz|800Hz|600Hz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Description("指定用于测量频谱、电平、ITU 参数、调制识别和数字解码的带宽。不同型号的设备除了最大带宽有 10M、20M 或 40M 的区别外，可选值列表中的其他值都是相同的。")]
    public double IfBandwidth
    {
        get;
        set;
        //_banDwidth = _ifBandwidth.ToString(CultureInfo.InvariantCulture);
    } = 200.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|250|200|150|125|100|80|60|50|40|30|25|20|15|12.5|10|8|6|5|4|3|2.5|2|1.5|1.2|1|0.8|0.6",
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|250kHz|200kHz|150kHz|125kHz|100kHz|80kHz|60kHz|50kHz|40kHz|30kHz|25kHz|20kHz|15kHz|12.5kHz|10kHz|8kHz|6kHz|5kHz|4kHz|3kHz|2.5kHz|2kHz|1.5kHz|1.2kHz|1kHz|800Hz|600Hz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Description("指定用于测量频谱、电平、ITU 参数、调制识别和数字解码的带宽。不同型号的设备除了最大带宽有 10M、20M 或 40M 的区别外，可选值列表中的其他值都是相同的。")]
    public double DfBandWidth
    {
        get;
        set;
        //_banDwidth = _dfBandWidth.ToString();
    } = 200.0d;

    //private string _debw = "200"; // 设备参数

    [PropertyOrder(9)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|500|400|300|250|200|150|125|100|80|60|50|40|30|25|20|15|12.5|10|8|6|5|4|3|2.5|2|1.5|1.2",
        DisplayValues =
            "|500kHz|400kHz|300kHz|250kHz|200kHz|150kHz|125kHz|100kHz|80kHz|60kHz|50kHz|40kHz|30kHz|25kHz|20kHz|15kHz|12.5kHz|10kHz|8kHz|6kHz|5kHz|4kHz|3kHz|2.5kHz|2kHz|1.5kHz|1.2kHz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Description("指定用于音频解调的带宽，范围：1.2kHz ~ 500kHz。单独指定解调带宽意味着可以在以更宽的带宽查看频谱的同时，只对其中较小的带宽进行音频解调。")]
    public double FilterBandwidth
    {
        get;
        set;
        //_debw = _filterBandwidth.ToString();
    } = 200.0d;

    //private string _beGin = "88"; // 设备参数

    [PropertyOrder(3)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [Description("设置扫描的起始频率，单位为MHz。")]
    [ValueRange(20.0d, 3600.0d)]
    [DefaultValue(88.0d)]
    [Unit(UnitNames.MHz)]
    public double StartFrequency
    {
        get;
        set;
        //_beGin = _startFrequency.ToString();
    } = 88.0d;

    //private string _end = "108"; // 设备参数

    [PropertyOrder(4)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [Description("设置扫描的终止频率,单位MHz。")]
    [ValueRange(20.0d, 3600.0d)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    public double StopFrequency
    {
        get;
        set;
        //_end = _stopFrequency.ToString();
    } = 108.0d;

    //private string _step = "25"; // 设备参数

    [PropertyOrder(5)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置扫描步进，单位 kHz。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|3.125|6.25|12.5|25|50|100|200|400",
        DisplayValues = "|3.125kHz|6.25kHz|12.5kHz|25kHz|50kHz|100kHz|200kHz|400kHz")]
    [ValueRange(0.001d, 10.0d)]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    public double StepFrequency
    {
        get;
        set;
        //_step = _stepFrequency.ToString();
    } = 25d;

    [Parameter(AbilitySupport = FeatureType.SCAN)] // 用于区分PSCAN/FSCAN，非设备参数
    [PropertyOrder(6)]
    [Name(ParameterNames.ScanMode)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描类型")]
    [Description("设置频段扫描类型：全景扫描，频点扫描")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描")]
    [DefaultValue(ScanMode.Pscan)]
    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;

    //private string _scaNmode = "快速模式"; // 设备参数

    [PropertyOrder(6)]
    [Name("scanType")]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|快速模式|精度模式",
        DisplayValues = "|快速模式|精度模式")]
    [DefaultValue("快速模式")]
    [Description("指定PScan时的模式。各种模式的含义同“频谱模式”。")]
    public string ScanType
    {
        get;
        set;
        //_scaNmode = _scanType;
    } = "快速模式";

    [PropertyOrder(7)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(DiscreteFrequencyTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描的频点参数")]
    public Dictionary<string, object>[] MscanPoints { get; set; }

    [PropertyOrder(10)]
    [Name(ParameterNames.RfMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.SCAN |
                                FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowDistort|LowNoise",
        DisplayValues = "|常规模式|低失真模式|低噪声模式")]
    [DefaultValue(RfMode.Normal)]
    [Description("控制射频模块的三种工作模式：常规模式，低失真模式，低噪声模式")]
    public RfMode RfMode { get; set; } = RfMode.Normal;

    //private string _atTenuation = "自动"; // 设备参数

    [PropertyOrder(11)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF |
                                FeatureType.SCAN | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0|5|10|15|20|25|30|35|40|45|50",
        DisplayValues = "|自动|0|5|10|15|20|25|30|35|40|45|50")]
    [DefaultValue(-1.0f)]
    [Description("调整设备衰减")]
    public float Attenuation
    {
        get;
        set;
        //if (value == -1)
        //    _atTenuation = "自动";
        //else
        //    _atTenuation = _attenuation.ToString();
    } = -1.0f;

    [PropertyOrder(12)]
    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [Description("对于不同类型的信号，设置不同的测向模式")]
    [DefaultValue(DFindMode.Normal)]
    public DFindMode DFindMode { get; set; } = DFindMode.Normal;

    //private string _thReshold = "-10"; // 设备参数

    [PropertyOrder(13)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电平门限")]
    [Description("设置测向或离散扫描的电平门限，单位dB。")]
    [ValueRange(-100.0f, 100.0f)]
    [DefaultValue(-10.0f)]
    public float LevelThreshold
    {
        get;
        set;
        //_thReshold = _levelThreshold.ToString();
    } = -10.0f;

    //private string _deModulation = "FM"; // 设备参数

    [PropertyOrder(15)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DefaultValue(Modulation.Fm)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|CW|LSB|USB|IQ|PULSE",
        DisplayValues = "|FM|AM|PM|CW|LSB|USB|IQ|PULSE")]
    [DisplayName("解调模式")]
    [Description("设置解调模式")]
    public Modulation DemMode
    {
        get;
        set;
        //if (value == Modulation.PULSE)
        //    _deModulation = "PLUSE";
        //else
        //    _deModulation = _demodulation.ToString();
    } = Modulation.Fm;

    //private string _sqUelch = "-10"; // 设备参数

    [PropertyOrder(16)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [ValueRange(-100.0f, 120.0f)]
    [DefaultValue(-10.0f)]
    [Description("只有在当前测量电平值超过这个门限，音频数据才会输出。")]
    public float SquelchThreshold
    {
        get;
        set;
        //_sqUelch = _squelch.ToString();
    } = -10.0f;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Name(ParameterNames.Xdb)]
    [DisplayName("XdB宽带")]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Category(PropertyCategoryNames.Measurement)]
    [ValueRange(1f, 100.0f)]
    [DefaultValue(26.0f)]
    [Unit(UnitNames.Db)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Slider)]
    public float XdB { get; set; } = 26;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Name(ParameterNames.BetaValue)]
    [DisplayName("β宽带")]
    [Description("设置ITU测量中β值，单位：%")]
    [Category(PropertyCategoryNames.Measurement)]
    [ValueRange(0.1f, 99.9f, 0.1f)]
    [DefaultValue(1.0f)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Slider)]
    public float Beta { get; set; } = 1;

    //private string _holDtime = "0"; // 设备参数

    [PropertyOrder(18)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("保持时间")]
    [ValueRange(0.0f, 600.0f)]
    [DefaultValue(0.0f)]
    [Description(@"保持时间，单位：秒。 如在【驻留时间】内，电平低于门限，则继续等待，如果超过了【等待时间】，
电平依然低于门限，则继续测量下个频点；如果电平超过门限，则继续测量，直到超过【驻留时间】。")]
    public float Holdtime
    {
        get;
        set;
        //_holDtime = _holdtime.ToString();
    }

    //private string _dweLltime = "0"; // 设备参数

    [PropertyOrder(19)]
    [Name(ParameterNames.DwellTime)]
    [Parameter(AbilitySupport = FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("驻留时间")]
    [ValueRange(0.0f, 600.0f)]
    [DefaultValue(0.0f)]
    [Description("驻留时间，单位：秒。如电平值超过门限值，则在该频率上继续测量的时间长度。")]
    public float DwellTime
    {
        get;
        set;
        //_dweLltime = _dwelltime.ToString();
    }

    #endregion

    #region 高级参数

    [PropertyOrder(21)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AVG|FAST|POS|RMS",
        DisplayValues = "|平均|快速|峰值|均方根")]
    [Description("设置计算电平数据（包括 FScan）时的处理方式。")]
    public DetectMode Detector { get; set; } = DetectMode.Avg;

    //private string _strFftPoints = "2048";

    [PropertyOrder(25)]
    [Name("fftPoints")]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱点数")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|64|128|512|1024|2048|4096|8192|16384",
        DisplayValues = "|64|128|512|1024|2048|4096|8192|16384")]
    [DefaultValue(2048)]
    [Description("IQ数据开关")]
    public int FftPoints
    {
        get;
        set;
        //_strFftPoints = _fftPoints.ToString();
    } = 2048;

    #endregion

    #region 数据开关

    //private string _iqData = "false"; // 设备参数

    [PropertyOrder(25)]
    [Name(ParameterNames.IqSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("IQ数据开关")]
    public bool IqSwitch
    {
        get;
        set;
        //_iqData = _iqSwitch.ToString();
    }

    //private string _reCognise = "true"; // 设备参数(模式识别)
    //private string _ituMeasure = "true"; // 设备参数

    [PropertyOrder(26)]
    [Name(ParameterNames.ItuSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("ITU数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("ITU数据开关")]
    public bool ItuSwitch
    {
        get;
        set;
        //_reCognise = _ituSwitch.ToString(); // 模式识别和ITU共用一个开关
        //_ituMeasure = _ituSwitch.ToString();
    }

    [PropertyOrder(27)]
    [Name(ParameterNames.LevelSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("电平数据开关")]
    public bool LevelSwitch { get; set; } = true;

    //private string _speCtrum = "true"; // 设备参数

    [PropertyOrder(28)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("频谱数据开关")]
    public bool SpectrumSwitch
    {
        get;
        set;
        //_speCtrum = _spectrumSwitch.ToString();
    } = true;

    private bool _audioSwitch;

    [PropertyOrder(30)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.MScne)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("是否监听音频\r\n注：当前设备音频资源的使用权由后发起者获得")]
    public bool AudioSwitch
    {
        get => _audioSwitch;
        set
        {
            _audioSwitch = value;
            // 加载音频，成功与否由音频引用计数决定
            LoadAudioCapturer(value);
        }
    }

    #endregion

    #region 设备参数

    //private string _chbw = "200"; // 设备参数

    [PropertyOrder(33)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [Parameter(AbilitySupport = FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("信道带宽")]
    [Description("将固定大小的频谱带宽划分为若干个信道，返回每一个信道的测向结果。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|3.125|6.25|12.5|25|50|100|200|400",
        DisplayValues = "|3.125kHz|6.25kHz|12.5kHz|25kHz|50kHz|100kHz|200kHz|400kHz")]
    [DefaultValue(200.0d)]
    public double ResolutionBandwidth
    {
        get;
        set;
        //_chbw = _resolutionBandwidth.ToString(CultureInfo.InvariantCulture);
    } = 200.0d;

    [PropertyOrder(33)]
    [Name("channelBandwidth")]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("信道带宽")]
    [Description("信道带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|250|200|150|125|100|80|60|50|40|30|25|20|15|12.5|10|8|6|5|4|3|2.5|2|1.5|1.2|1|0.8|0.6",
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|250kHz|200kHz|150kHz|125kHz|100kHz|80kHz|60kHz|50kHz|40kHz|30kHz|25kHz|20kHz|15kHz|12.5kHz|10kHz|8kHz|6kHz|5kHz|4kHz|3kHz|2.5kHz|2kHz|1.5kHz|1.2kHz|1kHz|800Hz|600Hz")]
    [DefaultValue(3.0d)]
    [Unit(UnitNames.KHz)]
    public double ChannelBandwidth
    {
        get;
        set;
        //_banDwidth = _channelBandwidth.ToString(CultureInfo.InvariantCulture);
    } = 3.0d;

    //private string _dfaNtenna = "无源"; // 设备参数

    [PropertyOrder(34)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向天线阵")]
    [StandardValues(
        StandardValues = "|无源|有源",
        DisplayValues = "|无源|有源")]
    [DefaultValue("无源")]
    [Description("测向天线阵(300M以下)，设置有源或无源")]
    public string DfAntenna
    {
        get;
        set;
        //_dfaNtenna = _dfAntenna;
    } = "无源";

    //private string _speCtrummode = "快速模式"; // 设备参数

    [PropertyOrder(35)]
    [Name("spectrumMode")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|快速模式|精度模式",
        DisplayValues = "|快速模式|精度模式")]
    [DefaultValue("快速模式")]
    [Description("频谱模式：指定计算频谱时的模式。\r\n快速模式：直接根据原始 IQ 数据进行 FFT 计算，没有进行附加处理；"
                 + "\r\n精度模式：以更小的分辨率进行数据采集，明显改善邻道分辨率，减少干扰。")]
    public string SpectrumMode
    {
        get;
        set;
        //_speCtrummode = _spectrumMode;
    } = "快速模式";

    //private string _strAvgCount = "1"; // 设备参数

    [PropertyOrder(36)]
    [Name(ParameterNames.IntegrationCount)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("平均次数")]
    [StandardValues(StandardValues = "|1|2|4|5|8|10|15|20",
        DisplayValues = "|1|2|4|5|8|10|15|20")]
    [DefaultValue(1)]
    [Description("频谱平均次数")]
    public int AvgCount
    {
        get;
        set;
        //_strAvgCount = _avgCount.ToString();
    } = 1;

    //private string _deCode = "无"; // 设备参数
    private string _decode = "无";

    [PropertyOrder(37)]
    [Name("decode")]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("数字解码")]
    [Description("用于单频测量功能，当执行亚音频解码时，要求频谱带宽必须为 25kHz 或者 12.5kHz。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|无|模拟亚音频|数字亚音频-正极性|数字亚音频-负极性",
        DisplayValues = "|无|模拟亚音频|数字亚音频-正极性|数字亚音频-负极性")]
    [DefaultValue("无")]
    public string Decode
    {
        get => _decode;
        set
        {
            if (IfBandwidth.CompareWith(25) == 0 || IfBandwidth.CompareWith(12.5) == 0)
                _decode = value;
            else
                _decode = "无";
            //_deCode = _decode;
        }
    }

    //private string _poLarize = "垂直极化"; // 设备参数

    [PropertyOrder(38)]
    [Name(ParameterNames.DfPolarization)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [StandardValues(
        StandardValues = "|垂直极化|水平极化",
        DisplayValues = "|垂直极化|水平极化")]
    [DefaultValue("垂直极化")]
    [Description("设置天线极化方式")]
    public string DfPolarization
    {
        get;
        set;
        //_poLarize = _polarization;
    } = "垂直极化";

    #endregion

    #region 安装参数

    [PropertyOrder(31)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("127.0.0.1")]
    [DisplayName("地址")]
    [Description("网络连接地址，IPV4格式[x.x.x.x]")]
    public string Ip { get; set; } = "192.168.1.200";

    [PropertyOrder(32)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [DefaultValue(9221)]
    [Description("设备使用的端口号")]
    public int Port { get; set; } = 9221;

    [PropertyOrder(33)]
    [Name("audioPort")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("音频端口")]
    [DefaultValue(9222)]
    [Description("设备音频数据返回时需要连接的UDP端口号。注：此端口号相对设备服务没有被占用")]
    public int AudioPort { get; set; } = 9222;

    #endregion
}