using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9917A;

[DeviceDescription(Name = "N9917A",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.AmpDF
                  | FeatureType.FFM
                  | FeatureType.SCAN,
    MaxInstance = 1,
    Version = "1.0.3",
    DeviceCapability = "0|18000|20000",
    Model = "N9917A",
    Description = "安捷伦频谱仪设备")]
[DeviceDescription(Name = "N9918A",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.AmpDF
                  | FeatureType.FFM
                  | FeatureType.SCAN,
    MaxInstance = 1,
    Version = "1.0.3",
    DeviceCapability = "0|18000|20000",
    Model = "N9918A",
    Description = "安捷伦频谱仪设备")]
public partial class N9917A
{
    #region 常规参数

    private double _frequency = 89.7d;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("设置中心频率,单位MHz。")]
    [ValueRange(0.0d, 18000.0d)]
    [DefaultValue(89.7d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            ////连续扫描模式
            //SendCmd("INIT:CONT 1");
            SendCmd($"FREQ:CENT {_frequency}e6"); //, _getLevel.Equals("PEAK") ? 50 : 1500);
            //创建一个marker
            SendCmd("CALC:MARK1 NORM");
            //if (!_getLevel.Equals("PEAK"))
            //{
            //    //SendCmd(string.Format("CALC:MARK1:X {0}e6", value));//将marker移到中心频率
            //    GetLevel = "PEAK";
            //}
            //SendCmd(string.Format("INIT:CONT {0}", _spectrumSwitch ? 0 : 1));//将扫描模式还原
        }
    }

    private double _ifBandwidth = 200d;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱带宽")]
    [Description("设置信号的频谱带宽。单位：kHz。")]
    [StandardValues(
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|200kHz|150kHz|120kHz|100kHz|80kHz|50kHz|30kHz|25kHz|12.5kHz|8kHz|6kHz",
        StandardValues =
            "|20000|10000|5000|2500|2000|1200|1000|800|600|500|400|300|200|150||120|100|80|50|30|25|12.5|8|6",
        IsSelectOnly = true)]
    [DefaultValue(200d)]
    [Unit(UnitNames.KHz)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            SendCmd($"FREQ:SPAN {_ifBandwidth}kHz");
        }
    }

    private double _startFrequency = 88d;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率(MHz)")]
    [Description("设置频段扫描的起始频率, 单位MHz")]
    [ValueRange(0.0d, 18000.0d)]
    [DefaultValue(88d)]
    [Unit(UnitNames.MHz)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            SendCmd($"FREQ:STAR {_startFrequency}e6");
        }
    }

    private double _stopFrequency = 108d;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率(MHz)")]
    [Description("设置频段扫描的终止频率,单位MHz。")]
    [ValueRange(0.0d, 18000.0d)]
    [DefaultValue(108d)]
    [Unit(UnitNames.MHz)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            SendCmd($"FREQ:STOP {_stopFrequency}e6");
        }
    }

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描的扫描步进，单位kHz。")]
    [StandardValues(DisplayValues = "|6.25kHz|12.5kHz|25kHz|50kHz|100kHz|150kHz|300kHz|500kHz",
        StandardValues = "|6.25|12.5|25|50|100|150|300|500", IsSelectOnly = true)]
    [DefaultValue(25d)]
    [Unit(UnitNames.KHz)]
    public double StepFrequency { get; set; } = 25d;

    public ScanMode ScanMode { get; set; } = ScanMode.Pscan;
    public RfMode RfMode { get; set; } = RfMode.Normal;
    private float _attenuation = float.NaN;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(5)]
    [Name(ParameterNames.Attenuation)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(DisplayValues = "|自动|0dB|5dB|10dB|15dB|20dB|25dB|30dB",
        StandardValues = "|-1|0|5|10|15|20|25|30", IsSelectOnly = true)]
    [DisplayName("衰减值")]
    [DefaultValue(-1)]
    [Description("设置衰减值,单位为dB,范围为[0,30]。")]
    [Unit(UnitNames.Db)]
    public float Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (value.Equals(-1))
            {
                SendCmd("POW:ATT:AUTO ON");
            }
            else
            {
                SendCmd("POW:ATT:AUTO OFF");
                SendCmd($"POW:ATT {_attenuation}");
                //设置窗口自动调整
                SendCmd("DISP:WIND:TRAC1:Y:AUTO");
            }
        }
    }

    private float _gain;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(6)]
    [Name(ParameterNames.Gain)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("增益")]
    [Description("设置外部增益，单位dB。")]
    [ValueRange(-100, 100)]
    [DefaultValue(0f)]
    [Unit(UnitNames.Db)]
    public float Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            SendCmd($"POW:EXTG {_gain}");
            //设置窗口自动调整
            SendCmd("DISP:WIND:TRAC1:Y:AUTO");
        }
    }

    #endregion

    #region 高级参数

    private string _detector = "AVER";

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(7)]
    [Name(ParameterNames.Detector)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("检波方式")]
    [StandardValues(DisplayValues = "|自动|常规|平均值|正峰值|负峰值|采样",
        StandardValues = "|AUTO|NORM|AVER|POS|NEG|SAMP", IsSelectOnly = true)]
    [Description("设置计算电平数据时的处理方式。")]
    [DefaultValue("AVER")]
    public string Detector
    {
        get => _detector;
        set
        {
            _detector = value;
            SendCmd("DET:FUNC " + _detector);
        }
    }

    private string _preamp = "OFF";

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(8)]
    [Name("preamp")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("前端放大器")]
    [Description("控制前端放大器开关。")]
    [StandardValues(DisplayValues = "|开|关", StandardValues = "|ON|OFF", IsSelectOnly = true)]
    [DefaultValue("OFF")]
    public string Preamp
    {
        get
        {
            SendCmd("POW:GAIN?");
            var preState = RecvResult('\n');
            if (preState.Equals("0"))
                _preamp = "OFF";
            else
                _preamp = "ON";
            return _preamp;
        }
        set
        {
            SendCmd($"POW:GAIN {value}");
            _preamp = value;
        }
    }

    private string _sweepType = "AUTO";

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(9)]
    [Name("sweepType")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫描类型")]
    [StandardValues(DisplayValues = "|自动|FFT扫描|步进扫描",
        StandardValues = "|AUTO|FFT|STEP", IsSelectOnly = true)]
    [Description("设置扫描类型。")]
    [DefaultValue("AUTO")]
    public string SweepType
    {
        get => _sweepType;
        set
        {
            _sweepType = value;
            SendCmd("SWE:TYPE " + _sweepType);
        }
    }

    private int _points = 401;

    [Parameter(AbilitySupport = FeatureType.FFM)]
    [PropertyOrder(10)]
    [Name("points")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("频谱点数")]
    [Description("设置返回的频谱数据的点数。")]
    [StandardValues(DisplayValues = "|1001|801|601|401|201|101",
        StandardValues = "|1001|801|601|401|201|101", IsSelectOnly = true)]
    [DefaultValue(401)]
    public int Points
    {
        get => _points;
        set
        {
            _points = value;
            SendCmd($"SWE:POIN {_points}");
        }
    }

    private double _resolutionBandwidth = 50d;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(11)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("分辨率带宽")]
    [Description("设置分辨率带宽的值。单位：kHz。")]
    [StandardValues(DisplayValues = "|自动|1kHz|5kHz|10kHz|30kHz|50kHz|100kHz|300kHz|1MHz|3MHz",
        StandardValues = "|0|1|5|10|30|50|100|300|1000|3000")]
    [ValueRange(0d, 3000d)]
    [DefaultValue(0d)]
    [Unit(UnitNames.KHz)]
    public double ResolutionBandwidth
    {
        get => _resolutionBandwidth;
        set
        {
            _resolutionBandwidth = value;
            if (_resolutionBandwidth.Equals(0))
            {
                SendCmd("BAND:AUTO ON");
            }
            else
            {
                SendCmd("BAND:AUTO OFF");
                SendCmd($":BAND:RES {_resolutionBandwidth}kHz");
            }
        }
    }

    private string _getLevel = "PEAK";

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(12)]
    [Name("getLevel")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("电平提取方式")]
    [StandardValues(DisplayValues = "|峰值|均值",
        StandardValues = "|PEAK|CENT", IsSelectOnly = true)]
    [Description("电平提取方式,目前支持两种形式(建议仪器出现频偏时使用峰值方式)。")]
    [DefaultValue("PEAK")]
    public string GetLevel
    {
        get => _getLevel;
        set
        {
            _getLevel = value;
            if (!_getLevel.Equals("PEAK"))
            {
                //SendCmd(string.Format("CALC:MARK1:X {0}e6", _frequency));//将marker移到中心频率
                // 设置取平均电平的指令。
                SendCmd(":TRAC1:TYPE AVG"); // 频谱数据类型 AVG为平均类型
                SendCmd("CME:AVER:ENAB 1"); // 打开平均设置
                SendCmd("AVER:COUN {5}"); // 平均次数
                SendCmd("AVER:TYPE AUTO"); // 数据平均类型，一般为Power 功率平均。
            }
        }
    }

    #endregion

    #region 数据开关

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(12)]
    [Name(ParameterNames.LevelSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [Description("电平数据开关")]
    [StandardValues(DisplayValues = "|开|关", StandardValues = "|true|false", IsSelectOnly = true)]
    [Browsable(false)]
    [DefaultValue(true)]
    public bool LevelSwitch { get; set; } = true;

    private bool _spectrumSwitch = true;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(13)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关。")]
    [StandardValues(DisplayValues = "|开|关", StandardValues = "|true|false", IsSelectOnly = true)]
    [DefaultValue(true)]
    public bool SpectrumSwitch
    {
        get => _spectrumSwitch;
        set
        {
            lock (_lockData)
            {
                _spectrumSwitch = value;
                if (_spectrumSwitch)
                    //单扫描模式
                    SendCmd("INIT:CONT 0");
                else
                    //连续扫描模式
                    SendCmd("INIT:CONT 1");
            }
        }
    }

    #endregion

    #region 安装参数

    [Parameter(IsInstallation = true)]
    [PropertyOrder(14)]
    [Name("ip")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("N9917A频谱仪接收指令的IP地址。")]
    [DefaultValue("127.0.0.1")]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [PropertyOrder(15)]
    [Name("tcpPort")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("TCP端口")]
    [Description("N9917A频谱仪接收指令通讯端口号。")]
    [DefaultValue(5025)]
    public int TcpPort { get; set; } = 5025;

    #endregion
}