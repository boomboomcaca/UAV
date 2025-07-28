using System;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

[DeviceDescription(Name = "DDF550",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.DirectionFinding | ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.ITUM
                  | FeatureType.FFDF
                  | FeatureType.SCAN
                  | FeatureType.WBDF
                  | FeatureType.ScanDF,
    MaxInstance = 1,
    Model = "DDF550",
    DeviceCapability = "20|6000|80000",
    Version = "1.1.4",
    Description = "DDF550测向机")]
public partial class Ddf550
{
    #region 常规参数

    private double _frequency = 101.7d;

    [Name(ParameterNames.Frequency)]
    [PropertyOrder(0)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF)]
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
            // 中心频率
            SendCommand(CmdMeasuresettingsffm, "iFrequency", ((long)(_frequency * 1000000)).ToString());
            // 解调频率
            SendCommand(CmdDemodulationsettings, "iAfFrequency", ((long)(_frequency * 1000000)).ToString());
            var str = AntennaUsed();
            Console.WriteLine("当前打通的天线：" + str);
        }
    }

    [Name(ParameterNames.StartFrequency)]
    [PropertyOrder(1)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(88.0d)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88.0d;

    [Name(ParameterNames.StopFrequency)]
    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Description("设置扫描终止频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108.0d;

    [Name(ParameterNames.StepFrequency)]
    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1|0.625|0.5|0.3125|0.25|0.2|0.125|0.1",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz|625Hz|500Hz|312.5Hz|250Hz|200Hz|125Hz|100Hz")]
    [DefaultValue(200.0d)]
    [Description("设置频段扫描步进。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    public double StepFrequency { get; set; } = 200.0d;

    // private double _dfBandWidth = 100.0d;
    // [Name(ParameterNames.DfBandwidth)]
    // [PropertyOrder(4)]
    // [Parameter(AbilitySupport = FeatureType.FFDF)]
    // [Category(PropertyCategoryNames.Measurement)]
    // [DisplayName("测向带宽")]
    // [StandardValues(IsSelectOnly = true,
    //     StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1",
    //     DisplayValues = "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz")]
    // [DefaultValue(100.0d)]
    // [Description("测向带宽 默认单位 kHz")]
    // [Unit(UnitNames.KHz)]
    // [Style(DisplayStyle.Dropdown)]
    // public double DFBandWidth
    // {
    //     get { return _dfBandWidth; }
    //     set
    //     {
    //         _dfBandWidth = value;
    //         if (_taskState == TaskState.Start)
    //         {
    //             SetDFBandWidthAndSpectrumSpan();
    //         }
    //     }
    // }
    private double _dfBandwidth = 100.0d;

    [Name(ParameterNames.DfBandwidth)]
    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|80000|40000|20000|10000|5000|2000|1000|500|200|100",
        DisplayValues = "|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz")]
    [DefaultValue(100.0d)]
    [Description("测向带宽、频谱带宽、频谱跨距 默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    public double DfBandwidth
    {
        get => _dfBandwidth;
        set
        {
            _dfBandwidth = value;
            if (TaskState == TaskState.Start)
                //if (_curFeature == FeatureType.FFDF)
                //{
                //    return;
                //}
                SetSpectrumSpan(_dfBandwidth);
        }
    }

    private double _ifBandwidth = 100.0d;

    [Name(ParameterNames.IfBandwidth)]
    [PropertyOrder(5)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|80000|40000|20000|10000|5000|2000|1000|500|200|100",
        DisplayValues = "|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz")]
    [DefaultValue(100.0d)]
    [Description("中频带宽、频谱带宽、频谱跨距 默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            if (TaskState == TaskState.Start)
                //if (_curFeature == FeatureType.FFDF)
                //{
                //    return;
                //}
                SetSpectrumSpan(_ifBandwidth);
        }
    }

    private double _filterBandWidth = 120.0d;

    [Name(ParameterNames.FilterBandwidth)]
    [PropertyOrder(6)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|20000|15000|12500|10000|8000|5000|2000|1500|1250|1000|800|500|300|250|150|120|75|50|30|25|15|12|9|8.333|6|4.8|4|3.1|2.7|2.4|2.1|1.5|1|0.6|0.3|0.15|0.1",
        DisplayValues =
            "|20MHz|15MHz|12.5MHz|10MHz|8MHz|5MHz|2MHz|1.5MHz|1.25MHz|1MHz|800kHz|500kHz|300kHz|250kHz|150kHz|120kHz|75kHz|50kHz|30kHz|25kHz|15kHz|12kHz|9kHz|8.333kHz|6kHz|4.8kHz|4kHz|3.1kHz|2.7kHz|2.4kHz|2.1kHz|1.5kHz|1kHz|600Hz|300Hz|150Hz|100Hz")]
    [DefaultValue(120.0d)]
    [Description("滤波带宽 默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    public double FilterBandWidth
    {
        get => _filterBandWidth;
        set
        {
            _filterBandWidth = value;
            SendCommand(CmdDemodulationsettings, "eAfBandwidth",
                ((EAfBandWidth)(uint)(_filterBandWidth * 1000)).ToString());
        }
    }

    private double _resolutionBandwidth = 200.0d;

    [Name(ParameterNames.ResolutionBandwidth)]
    [PropertyOrder(7)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("分辨率带宽")]
    [Description("将固定大小的频谱带宽划分为若干个信道，返回每一个信道的测向结果。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125|2.5|2|1.25|1|0.625|0.5|0.3125|0.25|0.2|0.125|0.1|0.0625|0.05|0.03125|0.025|0.02|0.0125",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz|2.5kHz|2kHz|1.25kHz|1kHz|625Hz|500Hz|312.5Hz|250Hz|200Hz|125Hz|100Hz|62.5Hz|50Hz|31.25Hz|25Hz|20Hz|12.5Hz")]
    [DefaultValue(200.0d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    public double ResolutionBandwidth
    {
        get => _resolutionBandwidth;
        set
        {
            _resolutionBandwidth = value;
            if (TaskState == TaskState.Start) SetDfBandWidthAndSpectrumSpan();
        }
    }

    private RfMode _rfMode = RfMode.Normal;

    [Name(ParameterNames.RfMode)]
    [PropertyOrder(8)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("工作模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowNoise|LowDistort",
        DisplayValues = "|常规|低噪声|低失真")]
    [DefaultValue(RfMode.Normal)]
    [Description("控制射频模块的两种工作模式，常规模式，低噪声模式，低失真模式.")]
    [Style(DisplayStyle.Radio)]
    public RfMode RfMode
    {
        get => _rfMode;
        set
        {
            _rfMode = value;
            var mode = (ERfMode)(int)_rfMode;
            SetRfMode(mode);
        }
    }

    private bool _attCtrlType;

    [Parameter(AbilitySupport = FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.FFM
                                | FeatureType.DPX
                                | FeatureType.SCAN
                                | FeatureType.MScan
                                | FeatureType.MScne
                                | FeatureType.ScanDF
                                | FeatureType.ITUM
                                | FeatureType.MScanDf)]
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
            if (CurFeature == FeatureType.ScanDF) return;
            if (_attCtrlType)
            {
                SendCommand(CmdMeasuresettingsffm, "eAttSelect", nameof(EAttSelect.AttAuto));
            }
            else
            {
                SendCommand(CmdMeasuresettingsffm, "eAttSelect", nameof(EAttSelect.AttManual));
                SendCommand(CmdMeasuresettingsffm, "iAttValue", _attenuation.ToString());
            }
        }
    }

    private int _attenuation;

    [Name(ParameterNames.Attenuation)]
    [PropertyOrder(9)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF | FeatureType.WBDF |
                                FeatureType.SCAN | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|0|5|10|15|20|25|30|35|40", //[0,40]
        DisplayValues = "|0|5|10|15|20|25|30|35|40")]
    [ValueRange(0, 40, 5)]
    [DefaultValue(0)]
    [Description("射频衰减 默认单位 dB.")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public int Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            if (CurFeature == FeatureType.ScanDF) return;
            if (_attCtrlType)
            {
                SendCommand(CmdMeasuresettingsffm, "eAttSelect", nameof(EAttSelect.AttAuto));
            }
            else
            {
                SendCommand(CmdMeasuresettingsffm, "eAttSelect", nameof(EAttSelect.AttManual));
                SendCommand(CmdMeasuresettingsffm, "iAttValue", _attenuation.ToString());
            }
        }
    }

    private DFindMode _dfindMode = DFindMode.Normal;

    [Name(ParameterNames.DfindMode)]
    [PropertyOrder(10)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规|连续测向|突发信号")]
    [DefaultValue(DFindMode.Normal)]
    [Description("设置测向模式，主要完成常规信号、连续测向、突发信号进行定制化测向")]
    [Style(DisplayStyle.Radio)]
    public DFindMode DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            if (CurFeature == FeatureType.ScanDF) return;
            SendCommand(CmdMeasuresettingsffm, "eAvgMode", ConvertDFindMode(_dfindMode).ToString());
        }
    }

    private int _levelThreshold = -20;

    [Name(ParameterNames.LevelThreshold)]
    [PropertyOrder(11)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电平门限")]
    [ValueRange(-30, 130)]
    [DefaultValue(-20)]
    [Description("设置测向电平门限，仅当信号电平超过门限时才返回测向数据")]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            if (CurFeature == FeatureType.ScanDF) return;
            SendCommand(CmdMeasuresettingsffm, "iThreshold", _levelThreshold.ToString());
        }
    }

    [Name(ParameterNames.QualityThreshold)]
    [PropertyOrder(11)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("质量门限")]
    [ValueRange(0, 100)]
    [DefaultValue(0)]
    [Description("设置测向质量门限，仅当信号质量超过门限时才返回测向数据")]
    [Unit(UnitNames.Pct)]
    [Style(DisplayStyle.Slider)]
    public int QualityThreshold { get; set; }

    private Modulation _demMode = Modulation.Fm;

    /// <summary>
    ///     解调模式(没有开放电视解调(DDF550支持电视解调))
    /// </summary>
    [Name(ParameterNames.DemMode)]
    [PropertyOrder(12)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB",
        DisplayValues = "|FM|AM|PULSE|PM|IQ|ISB|CW|USB|LSB")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value;
            var mode = (EDemodulation)(int)_demMode;
            SendCommand(CmdDemodulationsettings, "eDemodulation", mode.ToString());
        }
    }

    private int _squelchThreshold = 10;

    [Name(ParameterNames.SquelchThreshold)]
    [PropertyOrder(13)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，当信号电平超过门限时，进行音频解调")]
    [ValueRange(-30, 130)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            if (_squelchSwitch) SendCommand(CmdDemodulationsettings, "iAfThreshold", _squelchThreshold.ToString());
        }
    }

    private int _gain = -100;

    /// <summary>
    ///     增益控制,对于DDF550来说,如果增益不是自动,则测试时间必须为默认值(auto)
    /// </summary>
    [Name(ParameterNames.Gain)]
    [PropertyOrder(14)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("增益控制")]
    [Description("是否自动调整设备增益。单位dBuV")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-100|-30|-20|-10|0|10|20|30|40|50|60|70|80|90|100|110|120|130",
        DisplayValues =
            "|自动|-30dBuV|-20dBuV|-10dBuV|0dBuV|10dBuV|20dBuV|30dBuV|40dBuV|50dBuV|60dBuV|70dBuV|80dBuV|90dBuV|100dBuV|110dBuV|120dBuV|130dBuV")]
    [DefaultValue(-100)]
    [Browsable(false)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            if (_gain.Equals(-100))
            {
                SendCommand(CmdDemodulationsettings, "eGainSelect", EGainControl.GainAuto.ToString());
            }
            else
            {
                SendCommand(CmdDemodulationsettings, "eGainSelect", EGainControl.GainManual.ToString());
                SendCommand(CmdDemodulationsettings, "iGainValue", _gain.ToString());
            }
        }
    }

    #endregion

    #region 高级参数

    private DetectMode _detector = DetectMode.Fast;

    [Name(ParameterNames.Detector)]
    [PropertyOrder(15)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [DefaultValue(DetectMode.Fast)]
    [Description("设置处理电平数据的检波方式（检波方式只影响电平数据，对频谱无影响）")]
    [Style(DisplayStyle.Radio)]
    public DetectMode Detector
    {
        get => _detector;
        set
        {
            _detector = value;
            SendCommand(CmdDemodulationsettings, "eLevelIndicator", ConvertDetect(_detector).ToString());
        }
    }

    private float _measureTime = -1f;

    [Name(ParameterNames.MeasureTime)]
    [PropertyOrder(16)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0.0005|0.001|0.01|0.02|0.05|0.1|0.2|0.5|1|2|5",
        DisplayValues =
            "|自动|0.0005s|0.001s|0.01s|0.02s|0.05s|0.1s|0.2s|0.5s|1s|2s|5s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [DefaultValue(-1f)]
    [Description("设置测量时间，单位：秒（s），测量时间影响监测测向的结果的准确性")]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float MeasureTime
    {
        get => _measureTime;
        set
        {
            _measureTime = value;
            var time = Math.Abs(_measureTime - -1) < 1e-9 ? 0 : (int)(_measureTime * 1000000);
            SendCommand(CmdRxsettings, "iMeasureTime", time.ToString());
        }
    }

    //影响固定频点测量的频谱
    private string _fftMode = "OFF";

    [Name(ParameterNames.FftMode)]
    [PropertyOrder(17)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
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
            SendCommand(CmdMeasuresettingsffm, "eIFPanMode", ConvertIfPanMode(_fftMode).ToString());
        }
    }

    private string _bandMeasureMode = "XDB";

    [Name(ParameterNames.BandMeasureMode)]
    [PropertyOrder(18)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
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
            SendCommand(CmdItu, "eBwMeasurementMode", ConvertMeasureMode(_bandMeasureMode).ToString());
        }
    }

    private float _xdB = 26.0f;

    [Name(ParameterNames.Xdb)]
    [PropertyOrder(19)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
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
            SendCommand(CmdItu, "iConfigBwXdB", ((int)(_xdB * 10)).ToString());
        }
    }

    private float _beta = 1.0f;

    [Name(ParameterNames.BetaValue)]
    [PropertyOrder(20)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM)]
    [Category(PropertyCategoryNames.Misc)]
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
            SendCommand(CmdItu, "iConfigBwBeta", ((int)(_beta * 10)).ToString());
        }
    }

    // 隐藏不用
    private string _dfSelectMode = "TIME";

    [Name("dfSelectMode")]
    [PropertyOrder(21)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("测向取值方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|TIME|CYCLES",
        DisplayValues = "|积分时间|积分个数")]
    [DefaultValue("TIME")]
    [Browsable(false)]
    [Description("设置测向取值方式,按照点数取值,或者按照积分时间进行取值")]
    [Style(DisplayStyle.Radio)]
    public string DfSelectMode
    {
        get => _dfSelectMode;
        set
        {
            _dfSelectMode = value;
            if (CurFeature == FeatureType.ScanDF) return;
            SendCommand(CmdMeasuresettingsffm, "eBlockAveragingSelect", ConvertDfSelectMode(_dfSelectMode).ToString());
            SendCommand(CmdMeasuresettingsffm, "iBlockAveragingTime", _integralTime.ToString());
            SendCommand(CmdMeasuresettingsffm, "iBlockAveragingCycles", _integralCycles.ToString());
        }
    }

    private int _integralTime = 100;

    [Name(ParameterNames.IntegrationTime)]
    [PropertyOrder(22)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10|20|30|50|100|200|300|500|1000|2000|3000|5000|10000",
        DisplayValues = "|10ms|20ms|30ms|50ms|100ms|200ms|300ms|500ms|1s|2s|3s|5s|10s")]
    [DefaultValue(100)]
    [Unit(UnitNames.Ms)]
    [Style(DisplayStyle.Slider)]
    public int IntegrationTime
    {
        get => _integralTime;
        set
        {
            _integralTime = value;
            if (CurFeature == FeatureType.ScanDF) return;
            SendCommand(CmdMeasuresettingsffm, "eBlockAveragingSelect",
                nameof(EBlockAveragingSelect.BlockAveragingSelectTime));
            SendCommand(CmdMeasuresettingsffm, "iBlockAveragingTime", _integralTime.ToString());
        }
    }

    // 隐藏不用
    private int _integralCycles = 1;

    [PropertyOrder(23)]
    [Name(ParameterNames.IntegrationCount)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分次数")]
    [Description("设置测向积分个数.")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10|20|30|50|100|200|300|500|1000|2000|3000|5000|10000",
        DisplayValues = "|10|20|30|50|100|200|300|500|1000|2000|3000|5000|10000")]
    [Browsable(false)]
    [DefaultValue(1)]
    [Unit("次")]
    [Style(DisplayStyle.Slider)]
    public int IntegrationCycles
    {
        get => _integralCycles;
        set
        {
            _integralCycles = value;
            SendCommand(CmdMeasuresettingsffm, "iBlockAveragingCycles", _integralCycles.ToString());
        }
    }

    #endregion

    #region 开关

    private bool _iqSwitch;

    [Name(ParameterNames.IqSwitch)]
    [PropertyOrder(24)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("IQ数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否获取IQ数据")]
    [DefaultValue(false)]
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
            if (TaskState == TaskState.Start) CheckIqSwitch(_iqSwitch);
        }
    }

    private bool _ituSwitch;

    [Name(ParameterNames.ItuSwitch)]
    [PropertyOrder(25)]
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
            if (value)
                _mediaType |= MediaType.Itu;
            else
                _mediaType &= ~MediaType.Itu;
            _ituSwitch = value;
            SendCommand(CmdItu, "bEnableMeasurement", _ituSwitch.ToString().ToLower());
            SendCommand(CmdItu, "bUseAutoBandwidthLimits", "true"); ////////////这里可能需要改
        }
    }

    private bool _levelSwitch;

    [Name(ParameterNames.LevelSwitch)]
    [PropertyOrder(26)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取电平数据")]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch
    {
        get => _levelSwitch;
        set
        {
            _levelSwitch = value;
            if (value)
                _mediaType |= MediaType.Level;
            else
                _mediaType &= ~MediaType.Level;
        }
    }

    private bool _spectrumSwitch = true;

    [Name(ParameterNames.SpectrumSwitch)]
    [PropertyOrder(27)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
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
            if (value)
                _mediaType |= MediaType.Spectrum;
            else
                _mediaType &= ~MediaType.Spectrum;
            _spectrumSwitch = value;
            if (TaskState == TaskState.Start)
                if (CurFeature == FeatureType.FFM)
                    CheckSpectrumSwitch(_spectrumSwitch);
        }
    }

    private bool _audioSwitch;

    [Name(ParameterNames.AudioSwitch)]
    [PropertyOrder(28)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
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
            if (value)
                _mediaType |= MediaType.Audio;
            else
                _mediaType &= ~MediaType.Audio;
            _audioSwitch = value;
            if (TaskState == TaskState.Start) CheckAudioSwitch(_audioSwitch);
        }
    }

    private bool _squelchSwitch;

    [Name(ParameterNames.SpectrumSwitch)]
    [PropertyOrder(29)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    [Style(DisplayStyle.Switch)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            SendCommand(CmdDemodulationsettings, "bUseAfThreshold", _squelchSwitch.ToString().ToLower());
            if (_squelchSwitch) SendCommand(CmdDemodulationsettings, "iAfThreshold", _squelchThreshold.ToString());
        }
    }

    [PropertyOrder(18)]
    [Name("dfindMethod")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向体制")]
    [Description("设置当前设备的测向体制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ci",
        DisplayValues = "|干涉仪")]
    [Browsable(false)]
    [ReadOnly(true)]
    [DefaultValue(DfindMethod.Ci)]
    [Style(DisplayStyle.Radio)]
    public DfindMethod DfindMethod { get; set; } = DfindMethod.Ci;

    #endregion

    #region 天线控制

    private Polarization _dfPolarization;

    [Name(ParameterNames.DfPolarization)]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("极化方式")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|垂直极化|水平极化",
        StandardValues = "|Vertical|Horizontal")]
    [Description("设置测向天线极化方式")]
    [DefaultValue(Polarization.Vertical)]
    [PropertyOrder(25)]
    [Style(DisplayStyle.Radio)]
    public Polarization DfPolarization
    {
        get => _dfPolarization;
        set
        {
            _dfPolarization = value;
            if (CurFeature == FeatureType.ScanDF) return;
            var pol = _dfPolarization == Polarization.Horizontal ? EAntPol.PolHorizontal : EAntPol.PolVertical;
            SendCommand(CmdMeasuresettingsffm, "eAntPol", pol.ToString());
        }
    }

    private bool _amplifier;

    [Name(ParameterNames.PreAmpSwitch)]
    [PropertyOrder(30)]
    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.ITUM
                                | FeatureType.FFDF
                                | FeatureType.SCAN
                                | FeatureType.WBDF
                                | FeatureType.ScanDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("天线放大器")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [Description("设置天线放大器开关")]
    [Style(DisplayStyle.Switch)]
    public bool Amplifier
    {
        get => _amplifier;
        set
        {
            _amplifier = value;
            if (CurFeature == FeatureType.ScanDF) return;
            var eState = _amplifier ? EState.StateOn : EState.StateOff;
            SendCommand(CmdMeasuresettingsffm, "eAntPreAmp", eState.ToString());
        }
    }

    #endregion

    #region 安装参数

    [Name(ParameterNames.IpAddress)]
    [PropertyOrder(31)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的网络地址，IPv4格式[x.x.x.x]")]
    [DefaultValue("127.0.0.1")]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [Name(ParameterNames.Port)]
    [PropertyOrder(32)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [DefaultValue(5555)]
    [Description("设置连接并控制设备的网络端口号")]
    [ValueRange(1000, 65535, 0)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 5555;

    [Name("angleOffset")]
    [PropertyOrder(33)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线夹角")]
    [DefaultValue(0)]
    [Browsable(false)]
    [Description("天线安装位置与正北方向的夹角")]
    [ValueRange(0, 360, 0.1)]
    [Unit(UnitNames.Degree)]
    [Style(DisplayStyle.Slider)]
    public int AngleOffset { get; set; }

    [PropertyOrder(40)]
    [Name("useGPS")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集GPS数据")]
    [Description("设置是否通过本接收机采集GPS信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(true)]
    [Style(DisplayStyle.Switch)]
    public bool UseGps { get; set; }

    [PropertyOrder(44)]
    [Name("useCompass")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("采集电子罗盘数据")]
    [Description("设置是否通过本接收机采集电子罗盘信息。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否"
    )]
    [DefaultValue(false)]
    [Browsable(true)]
    [Style(DisplayStyle.Switch)]
    public bool UseCompass { get; set; }

    [PropertyOrder(37)]
    [Name("dfAntennaPort")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("测向天线端口")]
    [Description("设置测向天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1",
        DisplayValues = "|外部控制|自动|V/UHF|HF/V/U/SHF"
    )]
    [DefaultValue(-1)]
    [Style(DisplayStyle.Radio)]
    public int DdfAntenna { get; set; } = -1;

    [PropertyOrder(38)]
    [Name("monitorAntennaPort")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("监测天线端口")]
    [Description("设置监测天线的连接端口")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1",
        DisplayValues = "|外部控制|自动|V/UHF|HF/V/U/SHF"
    )]
    [DefaultValue(1)]
    [Style(DisplayStyle.Radio)]
    public int MonitorAntenna { get; set; } = 1;

    #endregion
}