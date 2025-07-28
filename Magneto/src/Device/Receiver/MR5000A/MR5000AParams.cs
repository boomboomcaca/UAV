using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.MR5000A;

[DeviceDescription(
    Name = "MR5000A",
    DeviceCategory = ModuleCategory.Monitoring,
    Manufacturer = "成都阿莱夫信息技术有限公司",
    Version = "1.1.6",
    Model = "MR5000A",
    FeatureType = FeatureType.FFM
                  | FeatureType.DPX
                  | FeatureType.ITUM
                  | FeatureType.FFDF
                  | FeatureType.WBDF
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne
                  | FeatureType.IFMCA
                  | FeatureType.ScanDF
                  | FeatureType.AmpDF
                  | FeatureType.TDOA
                  | FeatureType.SSE
                  | FeatureType.IQRETRI,
    MaxInstance = 1,
    DeviceCapability = "20|8000|160000",
    Description = "应用于德辰科技自研MR5000A系列接收机")]
public partial class Mr5000A
{
    #region 天线控制

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF | FeatureType.SSE)]
    [Name("dfPolarization")]
    [DisplayName("极化方式")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|垂直极化|水平极化",
        StandardValues = "|Vertical|Horizontal")]
    [Description("设置测向天线极化方式")]
    [DefaultValue(Polarization.Vertical)]
    [PropertyOrder(27)]
    [Style(DisplayStyle.Radio)]
    public Polarization DfPolarization { get; set; }

    #endregion

    #region 射频控制

    private double _frequency;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.WBDF
                                | FeatureType.IFMCA | FeatureType.AmpDF | FeatureType.TDOA | FeatureType.SSE |
                                FeatureType.IQRETRI)]
    [Name(ParameterNames.Frequency)]
    [DisplayName("中心频率")]
    [Description("设置监测或测向时被测信号的中心频率，单位：MHz")]
    [Category(PropertyCategoryNames.RadioControl)]
    [ValueRange(20.0d, 8000.0d, 6)]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(0)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get
        {
            lock (_parameterLock)
            {
                return _frequency;
            }
        }
        set
        {
            lock (_parameterLock)
            {
                _frequency = value;
                SendCommand($"FREQ {value} MHz");
            }
        }
    }

    private double _frequencyOffset;

    [Parameter(AbilitySupport = FeatureType.IQRETRI | FeatureType.FFM)]
    [Name(ParameterNames.FrequencyOffset)]
    [DisplayName("回放频率")]
    [Description("设置流盘回放时待分析频点相对于采集是中心频点的偏移量，单位：MHz")]
    [Category(PropertyCategoryNames.RadioControl)]
    [ValueRange(-40.0d, 40.0d, 6)]
    [DefaultValue(0.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    public double FrequencyOffset
    {
        get => _frequencyOffset;
        set
        {
            _frequencyOffset = -value;
            SendCommand($"FREQ:OFFS {_frequencyOffset} MHz");
        }
    }

    private double _ifBandwidth;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.DPX
                                | FeatureType.ITUM
                                | FeatureType.FScne
                                | FeatureType.IFMCA
                                | FeatureType.AmpDF
                                | FeatureType.TDOA
                                | FeatureType.IQRETRI)]
    [Name(ParameterNames.IfBandwidth)]
    [DisplayName("中频带宽")]
    [Description("中频带宽、频谱跨距，单位：MHz")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|160000|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues =
            "|160MHz|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(500.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            lock (_parameterLock)
            {
                _ifBandwidth = value;
                SendCommand($"FREQ:SPAN {value} kHz");
                var shift = Math.Pow(2, BufRbwShift % 8) * 100;
                SendCommand($"BAND:RES {value / shift} kHz");
                // 设置IQ相关的采样带宽和采样率，采样率取1.28倍带宽
                var samplingRate =
                    GetProperSamplingRateByBandwidthString(value.ToString(CultureInfo.InvariantCulture), value * 1.28d);
                if (double.IsNaN(samplingRate)) samplingRate = value * 1.28;
                SendCommand($"MEAS:IQ:SRAT {samplingRate} kHz");
                SendCommand($"MEAS:IQ:BAND {value} kHz");
            }
        }
    }

    public int BufRbwShift;

    [Parameter(AbilitySupport = FeatureType.FFM
                                | FeatureType.DPX
                                | FeatureType.ITUM
                                | FeatureType.FScne
                                | FeatureType.MScan
                                | FeatureType.AmpDF
                                | FeatureType.FFDF
                                | FeatureType.WBDF
                                | FeatureType.SSE)]
    [Name("rbwShift")]
    [DisplayName("分辨率档位")]
    [Description("设置频谱分辨率档位，档位越高，分辨率越高，频谱越细腻")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2|3|4|5|6|7",
        DisplayValues = "|一档|二档|三档|四档|五档|六档|七档")]
    [DefaultValue(6)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Dropdown)]
    public int RbwShift
    {
        get => BufRbwShift;
        set
        {
            lock (_parameterLock)
            {
                BufRbwShift = value;
                SendCommand($"FREQ:SPAN {_ifBandwidth} kHz");
                var shift = Math.Pow(2, value % 8) * 100;
                SendCommand($"BAND:RES {_ifBandwidth / shift} kHz");
                // 设置IQ相关的采样带宽和采样率，采样率默认值取1.28倍带宽
                // 注：在IQ采集模式下，采样带宽并不等于频谱带宽，需要在脚本约束里面体现出来
                var samplingRate =
                    GetProperSamplingRateByBandwidthString(_ifBandwidth.ToString(CultureInfo.InvariantCulture),
                        _ifBandwidth * 1.28d);
                if (double.IsNaN(samplingRate)) samplingRate = _ifBandwidth * 1.28d;
                SendCommand($"MEAS:IQ:SRAT {samplingRate} kHz");
                SendCommand($"MEAS:IQ:BAND {_ifBandwidth} kHz");
            }
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.SSE)]
    [Name(ParameterNames.DfBandwidth)]
    [DisplayName("测向带宽")]
    [Description("测向带宽，单位：MHz")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|160000|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues =
            "|160MHz|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(500.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Bandwidth)]
    public double DfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            lock (_parameterLock)
            {
                _ifBandwidth = value;
                SendCommand($"FREQ:SPAN {value} kHz");
                var shift = Math.Pow(2, BufRbwShift % 8) * 100;
                SendCommand($"BAND:RES {value / shift} kHz");
                // 设置IQ相关的采样带宽和采样率，采样率取1.28倍带宽
                var samplingRate =
                    GetProperSamplingRateByBandwidthString(value.ToString(CultureInfo.InvariantCulture), value * 1.28d);
                if (double.IsNaN(samplingRate)) samplingRate = value * 1.28;
                SendCommand($"MEAS:IQ:SRAT {samplingRate} kHz");
                SendCommand($"MEAS:IQ:BAND {value} kHz");
            }
        }
    }

    private RfMode _rfMode;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.WBDF | FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne
                                | FeatureType.MScne | FeatureType.IFMCA | FeatureType.ScanDF | FeatureType.AmpDF |
                                FeatureType.TDOA | FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.RfMode)]
    [DisplayName("工作模式")]
    [Description("设置接收机射频工作模式")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|LowNoise|LowDistort",
        DisplayValues = "|常规|低噪声|低失真")]
    [DefaultValue(RfMode.Normal)]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Radio)]
    public RfMode RfMode
    {
        get => _rfMode;
        set
        {
            _rfMode = value;
            SendCommand($"ATT:RF:MODE {value}");
        }
    }

    private bool _autoAttenuation;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.WBDF | FeatureType.SCAN | FeatureType.MScan
                                | FeatureType.FScne | FeatureType.MScne | FeatureType.ScanDF | FeatureType.AmpDF |
                                FeatureType.TDOA | FeatureType.SSE)]
    [Name(ParameterNames.AttCtrlType)]
    [DisplayName("自动衰减控制")]
    [Description("设置衰减控制的方式")]
    [Category(PropertyCategoryNames.RadioControl)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开启|关闭")]
    [DefaultValue(true)]
    [PropertyOrder(7)]
    [Children($"|{ParameterNames.RfAttenuation}|{ParameterNames.IfAttenuation}", false)]
    [Style(DisplayStyle.Switch)]
    public bool AutoAttenuation
    {
        get => _autoAttenuation;
        set
        {
            _autoAttenuation = value;
            if (_autoAttenuation)
            {
                SendCommand("ATT:AUT ON");
            }
            else
            {
                SendCommand("ATT:AUT OFF");
                SendCommand($"ATT:RF {_rfAttenuation}");
                SendCommand($"ATT:IF {_ifAttenuation}");
            }
        }
    }

    private int _rfAttenuation;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.WBDF | FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne
                                | FeatureType.MScne | FeatureType.IFMCA | FeatureType.ScanDF | FeatureType.AmpDF |
                                FeatureType.TDOA | FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.RfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("射频衰减")]
    [Description("设置射频衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|30|24|18|12|6|0",
        DisplayValues = "|30|24|18|12|6|0")]
    [ValueRange(0, 30)]
    [DefaultValue(0)]
    [Unit(UnitNames.Db)]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Slider)]
    public int RfAttenuation
    {
        get => _rfAttenuation;
        set
        {
            _rfAttenuation = value;
            if (value % 2 != 0) // 射频衰减步进为2
                _rfAttenuation--;
            // _autoAttenuation = _rfAttenuation == 0 && _ifAttenuation == 0;
            // if (_autoAttenuation)
            // {
            //     SendCommand("ATT:AUT ON");
            // }
            // else
            {
                SendCommand($"ATT:RF {_rfAttenuation}");
            }
        }
    }

    private int _ifAttenuation;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FFDF |
                                FeatureType.WBDF | FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne
                                | FeatureType.MScne | FeatureType.IFMCA | FeatureType.ScanDF | FeatureType.AmpDF |
                                FeatureType.TDOA | FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.IfAttenuation)]
    [Category(PropertyCategoryNames.RadioControl)]
    [DisplayName("中频衰减")]
    [Description("设置中频衰减")]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|20|16|12|8|4|0",
        DisplayValues = "|20|16|12|8|4|0")]
    [ValueRange(0, 20)]
    [DefaultValue(0)]
    [Unit(UnitNames.Db)]
    [PropertyOrder(9)]
    [Style(DisplayStyle.Slider)]
    public int IfAttenuation
    {
        get => _ifAttenuation;
        set
        {
            _ifAttenuation = value;
            // _autoAttenuation = _rfAttenuation == 0 && _ifAttenuation == 0;
            // if (_autoAttenuation)
            // {
            //     SendCommand("ATT:AUT ON");
            // }
            // else
            {
                SendCommand($"ATT:IF {_ifAttenuation}");
            }
        }
    }

    #endregion

    #region 测向控制

    private double _resolutionBandwidth;

    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [DisplayName("测向分辨率")]
    [Description("测向分辨率，单位：kHz")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|160000|80000|40000|20000|10000|5000|2000|1000|500|200|100|50|20|10|5|2|1",
        DisplayValues =
            "|160MHz|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|20kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Bandwidth)]
    public double ResolutionBandwidth
    {
        get => _resolutionBandwidth;
        set
        {
            _resolutionBandwidth = value;
            SendCommand($"BAND {value} kHz");
        }
    }

    private int _dfSamplingCount;

    [Parameter(AbilitySupport = FeatureType.WBDF)]
    [Name("dfSamplingCount")]
    [DisplayName("采样点数")]
    [Description("采样点数，用于控制测向分辨率")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2048|1024|512|256|128",
        DisplayValues = "|2048|1024|512|256|128")]
    [DefaultValue(1024)]
    [PropertyOrder(10)]
    [Style(DisplayStyle.Dropdown)]
    public int DfSamplingCount
    {
        get
        {
            lock (_parameterLock)
            {
                return _dfSamplingCount;
            }
        }
        set
        {
            lock (_parameterLock)
            {
                _dfSamplingCount = value;
                SendCommand($"MEAS:DFIN:COUN {value}");
            }
        }
    }

    private DFindMode _dfindMode;

    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Name(ParameterNames.DfindMode)]
    [DisplayName("测向模式")]
    [Description("设置测向模式")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号")]
    [DefaultValue(DFindMode.Normal)]
    [PropertyOrder(11)]
    [Style(DisplayStyle.Radio)]
    public DFindMode DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            if (value == DFindMode.Gate)
            {
                SendCommand("HARD:COMP:ENAB OFF");
                SendCommand("MEAS:DFIN:PAN OFF");
            }
            else
            {
                SendCommand($"HARD:COMP:ENAB {(EnableCompass ? "ON" : "OFF")}");
                SendCommand("MEAS:DFIN:PAN ON");
            }

            SendCommand($"MEAS:DFIN:MODE {value}");
        }
    }

    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("estimatedSSECount")]
    [DisplayName("示向度估计数量")]
    [Description("设置空间谱测向时预估的示向度数量")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|9|8|7|6|5|4|3|2|1|-1",
        DisplayValues = "|9|8|7|6|5|4|3|2|1|自动")]
    [DefaultValue(-1)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Dropdown)]
    public int EstimatedSseCount { get; set; }

    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("sseAutomaticMethod")]
    [DisplayName("估测方法")]
    [Description("设置空间谱自动信号估测方法")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|4|3|2|1",
        DisplayValues = "|4|3|2|1")]
    [DefaultValue(2)]
    [PropertyOrder(13)]
    [Style(DisplayStyle.Radio)]
    public int SseAutomaticMethod { get; set; }

    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("sseAutomaticCoe")]
    [DisplayName("估测系数")]
    [Description("设置空间谱自动信号估测系数")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(0.0f, 10.0f)]
    [DefaultValue(1.0f)]
    [PropertyOrder(14)]
    [Style(DisplayStyle.Slider)]
    public float SseAutomaticCoe { get; set; }

    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("integratedSSETimes")]
    [DisplayName("谱估计积分次数")]
    [Description("设置空间谱测向时预估的示向度积分次数")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(1, 100)]
    [DefaultValue(10)]
    [PropertyOrder(15)]
    [Style(DisplayStyle.Slider)]
    public int IntegratedSseTimes { get; set; }

    private int _integrationGear;

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.SSE)]
    [Name("integrationGear")]
    [DisplayName("采样等级")]
    [Description("设置测向采样等级；即在相同采样率的条件，通过调整不同的采样点数达到不同的采样等级，积分总共分五档，最低档采集128个点，最高档采集2048个点")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|4|3|2|1|0",
        DisplayValues = "|五级|四级|三级|二级|一级")]
    [DefaultValue(2)]
    [PropertyOrder(16)]
    [Style(DisplayStyle.Dropdown)]
    public int IntegrationGear
    {
        get
        {
            lock (_parameterLock)
            {
                return _integrationGear;
            }
        }
        set
        {
            lock (_parameterLock)
            {
                _dfSamplingCount = 128 << value;
                if (_dfSamplingCount > 2048)
                {
                    _dfSamplingCount = 2048;
                    _integrationGear = 4;
                }
                else if (_dfSamplingCount < 128)
                {
                    _dfSamplingCount = 128;
                    _integrationGear = 0;
                }
                else
                {
                    _integrationGear = value;
                }

                SendCommand($"MEAS:DFIN:COUN {_dfSamplingCount}");
            }
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Name("avgTimes")]
    [DisplayName("积分次数")]
    [Description("设置测向积分次数")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|200|150|100|50|10|5|0",
        DisplayValues = "|200|150|100|50|10|5|0")] //695333
    [DefaultValue(5)]
    [ValueRange(0, 200)]
    [PropertyOrder(17)]
    [Style(DisplayStyle.Slider)]
    public int AvgTimes { get; set; }

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Name(ParameterNames.IntegrationTime)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间，单位：μs")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000000|1000000|500000|200000|100000|50000|20000|10000|5000|2000|1000|500|200|100|0",
        DisplayValues = "|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|5ms|2ms|1ms|500μs|200μs|100μs|0μs")]
    // [ValueRange(0, 2000000)]
    [DefaultValue(500)]
    [Unit(UnitNames.Us)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Slider)]
    public int IntegrationTime { get; set; }

    private int _levelThreshold;

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Name(ParameterNames.LevelThreshold)]
    [DisplayName("电平门限")]
    [Description("设置测向电平门限，当信号电平超过门限时返回测向结果")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(-20, 120)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Slider)]
    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            SendCommand($"MEAS:DFIN:THR {value}");
        }
    }

    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.WBDF | FeatureType.ScanDF)]
    [Name(ParameterNames.QualityThreshold)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，当测向质量超过门限时返回测向结果")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [ValueRange(0, 100)]
    [DefaultValue(40)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Slider)]
    public int QualityThreshold { get; set; }

    private bool _beamForming;

    [Parameter(AbilitySupport = FeatureType.SSE)]
    [Name("beamForming")]
    [DisplayName("波束合成")]
    [Browsable(false)]
    [Description("设置是否启用波束合成")]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [StandardValues(IsSelectOnly = false,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(21)]
    [Style(DisplayStyle.Switch)]
    public bool BeamForming
    {
        get => _beamForming;
        set
        {
            _beamForming = value;
            SendCommand($"MEAS:DFIN:BEAM {(value ? "ON" : "OFF")}");
        }
    }

    [PropertyOrder(18)]
    [Name("dfindMethod")]
    [Parameter(AbilitySupport = FeatureType.FFDF | FeatureType.SSE)]
    [Category(PropertyCategoryNames.DirectionFinding)]
    [DisplayName("测向体制")]
    [Description("设置当前设备的测向体制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|ci|sse",
        DisplayValues = "|干涉仪|空间谱")]
    [Browsable(false)]
    [DefaultValue(DfindMethod.Ci)]
    [Style(DisplayStyle.Radio)]
    public DfindMethod DfindMethod { get; set; }

    #endregion

    #region 扫描参数

    private double _startFrequency;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne | FeatureType.ScanDF)]
    [Name(ParameterNames.StartFrequency)]
    [DisplayName("起始频率")]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Category(PropertyCategoryNames.Scan)]
    [ValueRange(20.0d, 8000.0d, 6)]
    [DefaultValue(87.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(0)]
    [Browsable(false)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            if (TaskState == TaskState.Start) // 适用于运行时修改参数
            {
                _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
                SendCommand($"FREQ:START {value} MHz");
            }
        }
    }

    private double _stopFrequency;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne | FeatureType.ScanDF)]
    [Name(ParameterNames.StopFrequency)]
    [DisplayName("结束频率")]
    [Description("设置扫描终止频率，单位MHz")]
    [Category(PropertyCategoryNames.Scan)]
    [ValueRange(20.0d, 8000.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit(UnitNames.MHz)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            if (TaskState == TaskState.Start) // 适用于运行时修改参数
            {
                _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
                SendCommand($"FREQ:STOP {value} MHz");
            }
        }
    }

    private double _stepFrequency = 25.0d;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne | FeatureType.ScanDF)]
    [Name(ParameterNames.StepFrequency)]
    [DisplayName("扫描步进")]
    [Description("设置频段扫描步进")]
    [Category(PropertyCategoryNames.Scan)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|400|200|100|50|25|12.5|6.25|3.125",
        DisplayValues = "|400kHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            if (TaskState == TaskState.Start) // 适用于运行时修改参数
            {
                _scanDataLength = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
                SendCommand($"FREQ:STEP {value} kHz");
            }
        }
    }

    private ScanMode _scanMode;

    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name(ParameterNames.ScanMode)]
    [DisplayName("扫描模式")]
    [Description("扫描模式: 全景扫描或频点扫描")]
    [Category(PropertyCategoryNames.Scan)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|PSCAN|FSCAN",
        DisplayValues = "|全景扫描|频点扫描")]
    [DefaultValue(ScanMode.Pscan)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Radio)]
    public ScanMode ScanMode
    {
        get => _scanMode;
        set
        {
            _scanMode = value;
            if (TaskState == TaskState.Start)
            {
                if (value == ScanMode.Pscan)
                {
                    SendCommand("FREQ:MODE PSC");
                    WaitForModeSwitchFeedback();
                    SendCommand("FREQ:PSC:MODE NORM");
                    if (CalibPScanNoise) SendCommand("SYST:RESP:IF ON");
                }
                else if (value == ScanMode.Fscan)
                {
                    SendCommand("FREQ:MODE SWE");
                    WaitForModeSwitchFeedback();
                    SendCommand("SYST:RESP:IF ON");
                }
            }
        }
    }

    public bool FastPScan;

    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Name("fastPScan")]
    [DisplayName("快速扫描")]
    [Description("设置是否快速全景扫描")]
    [Category(PropertyCategoryNames.Scan)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [DefaultValue(false)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Dropdown)]
    public bool PScanSpan
    {
        get => FastPScan;
        set
        {
            FastPScan = value;
            SendCommand($"MEAS:PSC:IFSP {(value ? 160000 : 20000)} kHz");
        }
    }

    private SegmentTemplate[] _segments;

    [Parameter(AbilitySupport = FeatureType.SCAN, Template = typeof(SegmentTemplate))]
    [Name(ParameterNames.ScanSegments)]
    [Category(PropertyCategoryNames.Scan)]
    [DisplayName("频段信息")]
    [Description("频段信息，存放频段扫描的频段信息")]
    [Browsable(false)]
    [PropertyOrder(3)]
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

    public class SegmentTemplate
    {
        [PropertyOrder(3)]
        [Parameter(AbilitySupport = FeatureType.SCAN
                                    | FeatureType.NSIC
                                    | FeatureType.ESE
                                    | FeatureType.EMDC
                                    | FeatureType.EMDA)]
        [Name("startFrequency")]
        [Category(PropertyCategoryNames.RadioControl)]
        [Resident]
        [DisplayName("起始频率")]
        [ValueRange(20.0d, 8000.0d)]
        [DefaultValue(87.0d)]
        [Unit(UnitNames.MHz)]
        [Description("设置频段扫描起始频点，单位为MHz")]
        public double StartFrequency { get; set; } = 87.0d;

        [PropertyOrder(4)]
        [Parameter(AbilitySupport = FeatureType.SCAN
                                    | FeatureType.NSIC
                                    | FeatureType.ESE
                                    | FeatureType.EMDC
                                    | FeatureType.EMDA)]
        [Name("stopFrequency")]
        [Category(PropertyCategoryNames.RadioControl)]
        [Resident]
        [DisplayName("终止频率")]
        [ValueRange(20.0d, 8000.0d)]
        [DefaultValue(108.0d)]
        [Unit(UnitNames.MHz)]
        [Description("设置扫描终止频率，单位MHz")]
        public double StopFrequency { get; set; } = 108.0d;

        [PropertyOrder(5)]
        [Parameter(AbilitySupport = FeatureType.SCAN
                                    | FeatureType.NSIC
                                    | FeatureType.ESE
                                    | FeatureType.EMDC
                                    | FeatureType.EMDA)]
        [Name("stepFrequency")]
        [Category(PropertyCategoryNames.Scan)]
        [Resident]
        [DisplayName("扫描步进")]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|500|200|100|50|25|12.5|6.25|3.125",
            DisplayValues = "|500kHz|200kHz|100kHz|50kHz|25kHz|12.5kHz|6.25kHz|3.125kHz")]
        [ValueRange(0.1d, 500.0d)]
        [DefaultValue(25.0d)]
        [Unit(UnitNames.KHz)]
        [Description("设置频段扫描步进，单位kHz")]
        public double StepFrequency { get; set; } = 25.0d;

        public static explicit operator SegmentTemplate(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            var template = new SegmentTemplate();
            var type = template.GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    if (dict.TryGetValue(name, out var value)) property.SetValue(template, value, null);
                }
            }
            catch
            {
                // 容错代码
            }

            return template;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dic = new Dictionary<string, object>();
            var type = GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute)
                        continue;
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    var value = property.GetValue(this);
                    dic.Add(name, value);
                }
            }
            catch
            {
                // 容错代码
            }

            return dic;
        }
    }

    #endregion

    #region 解调解码

    private double _filterBandwidth;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.SCAN |
                                FeatureType.FScne | FeatureType.FFDF | FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.FilterBandwidth)]
    [DisplayName("滤波带宽")]
    [Description("滤波带宽或解调带宽")]
    [Category(PropertyCategoryNames.Demodulation)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|160000|80000|40000|20000|10000|5000|2000|1000|500|300|200|150|120|100|50|20|15|12|10|5|2|1",
        DisplayValues =
            "|160MHz|80MHz|40MHz|20MHz|10MHz|5MHz|2MHz|1MHz|500kHz|300kHz|200kHz|150kHz|120kHz|100kHz|50kHz|20kHz|15kHz|12kHz|10kHz|5kHz|2kHz|1kHz")]
    [DefaultValue(120.0d)]
    [Unit(UnitNames.KHz)]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            SendCommand($"BAND {value} kHz");
        }
    }

    private Modulation _demMode;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.FFDF |
                                FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.DemMode)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("解调模式")]
    [Description("设置信号音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PM|DMR|dPMR|PDT|NXDN|CW|LSB|USB|PULSE|IQ|TETRA",
        DisplayValues = "|FM|AM|PM|DMR|dPMR|PDT|NXDN|CW|LSB|USB|PULSE|IQ|TETRA")]
    [DefaultValue(Modulation.Fm)]
    [PropertyOrder(12)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value;
            if (value == Modulation.Tetra)
                SendCommand("DEM IQ");
            else if (value == Modulation.Pdt)
                SendCommand("DEM DMR");
            else
                SendCommand($"DEM {value}");
        }
    }

    private int _squelchThreshold;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.IQRETRI)]
    [Name(ParameterNames.SquelchThreshold)]
    [Category(PropertyCategoryNames.Demodulation)]
    [DisplayName("静噪门限")]
    [Description("设置静噪门限，当信号电平超过门限时进行音频解调")]
    [ValueRange(-40, 120)]
    [Unit(UnitNames.DBuV)]
    [DefaultValue(10)]
    [PropertyOrder(13)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold
    {
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            SendCommand($"MEAS:THR {value}");
        }
    }

    #endregion

    #region 测量参数

    // [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScan)]
    // [Name(ParameterNames.HoldTime)]
    // [Browsable(false)]
    // [DisplayName("等待时间")]
    // [Description("设置频率等待时间，单位：s，等待电平超过静噪门限的超时时间，后用于分析信号强度、频谱、音频解调的时间")]
    // [Category(PropertyCategoryNames.Measurement)]
    // [ValueRange(0.0f, 300.0f)]
    // [DefaultValue(5.0f)]
    // [Unit("s")]
    // [PropertyOrder(14)]
    public float HoldTime { get; set; }
    private float _dwellTime;

    [Parameter(AbilitySupport = FeatureType.FScne | FeatureType.MScne)]
    [Name(ParameterNames.DwellTime)]
    [DisplayName("驻留时间")]
    [Description("设置频率驻留时间，单位：s，当频率电平超过静噪门限后用于分析信号强度、频谱、音频解调的时间")]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10|7|5|4|3|2|1|0.5|0.2|0.1|0",
        DisplayValues =
            "|10s|7s|5s|4s|3s|2s|1s|0.5s|0.2s|0.1s|0s")] //参数范围【0.0005s，900s】根据常规经验，屏蔽掉（5s,900s】以免客户长时间等待，造成没有数据返回的假象
    [ValueRange(0.0f, 10.0f, 0.1)]
    [DefaultValue(5.0f)]
    [Unit(UnitNames.Sec)]
    [PropertyOrder(15)]
    [Style(DisplayStyle.Slider)]
    public float DwellTime
    {
        get => _dwellTime;
        set
        {
            _dwellTime = value;
            SendCommand($"MEAS:DWEL {value} s");
        }
    }

    private DetectMode _detectMode;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.SCAN |
                                FeatureType.MScan
                                | FeatureType.FScne | FeatureType.MScne | FeatureType.IQRETRI)]
    [Name(ParameterNames.Detector)]
    [DisplayName("检波方式")]
    [Description("设备接收机检波方式，此参数配合测量时间，影响电平、频谱等数据的输出结果")]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [DefaultValue(DetectMode.Pos)]
    [PropertyOrder(16)]
    [Children($"|{ParameterNames.MeasureTime}", DetectMode.Avg, DetectMode.Pos, DetectMode.Rms)]
    [Style(DisplayStyle.Radio)]
    public DetectMode DetectMode
    {
        get => _detectMode;
        set
        {
            _detectMode = value;
            var cmd = string.Empty;
            var modeOn = string.Empty;
            switch (_detectMode)
            {
                case DetectMode.Fast:
                    cmd = "FAST";
                    modeOn = "ON";
                    break;
                case DetectMode.Pos:
                    cmd = "POS";
                    modeOn = "OFF";
                    break;
                case DetectMode.Avg:
                    cmd = "AVG";
                    modeOn = "OFF";
                    break;
                case DetectMode.Rms:
                    cmd = "RMS";
                    modeOn = "OFF";
                    break;
            }

            if (!string.IsNullOrEmpty(cmd))
            {
                SendCommand($"MEAS:DET {cmd}");
                SendCommand($"MEAS:TIME:AUT {modeOn}");
            }
        }
    }

    private float _measureTime;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.SCAN |
                                FeatureType.MScan
                                | FeatureType.FScne | FeatureType.MScne | FeatureType.IQRETRI)]
    [Name(ParameterNames.MeasureTime)]
    [DisplayName("测量时间")]
    [Description("设置接收机测量时间，此参数配合检波方式，影响电平、频谱等数据的输出结果")]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|5000000|2000000|1000000|500000|200000|100000|50000|20000|10000|1000|500",
        DisplayValues = "|5s|2s|1s|500ms|200ms|100ms|50ms|20ms|10ms|1ms|500us")]
    [ValueRange(500f, 5000000f, 500f)]
    [DefaultValue(500)]
    [Unit(UnitNames.Us)]
    [PropertyOrder(17)]
    [Style(DisplayStyle.Slider)]
    public float MeasureTime
    {
        get => _measureTime;
        set
        {
            _measureTime = value;
            SendCommand($"MEAS:TIME {value / 1000.0f} ms");
            SendCommand($"MEAS:HOLD {value / 1000.0f} ms");
        }
    }

    private float _xdB;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.IQRETRI)]
    [Name(ParameterNames.Xdb)]
    [DisplayName("XdB宽带")]
    [Description("设置ITU测量中XdB值 单位：dB")]
    [Category(PropertyCategoryNames.Measurement)]
    [ValueRange(0.0f, 100.0f)]
    [DefaultValue(26.0f)]
    [Unit(UnitNames.Db)]
    [PropertyOrder(18)]
    [Style(DisplayStyle.Slider)]
    public float XdB
    {
        get => _xdB;
        set
        {
            _xdB = value;
            SendCommand($"MEAS:BAND:XDB {value}");
        }
    }

    private float _beta;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.IQRETRI)]
    [Name(ParameterNames.BetaValue)]
    [DisplayName("β宽带")]
    [Description("设置ITU测量中β值，单位：%")]
    [Category(PropertyCategoryNames.Measurement)]
    [ValueRange(0.0f, 100f)]
    [DefaultValue(1.0f)]
    [Unit(UnitNames.Pct)]
    [PropertyOrder(19)]
    [Style(DisplayStyle.Slider)]
    public float Beta
    {
        get => _beta;
        set
        {
            _beta = value;
            SendCommand($"MEAS:BAND:BETA {value / 100}");
        }
    }

    private int _iqSamplingCount;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.TDOA)]
    [Name("iqSamplingCount")]
    [DisplayName("IQ采样点数")]
    [Description("设置IQ采样点数，值通常是以2为底的幂")]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|16384|8192|4096|2048",
        DisplayValues = "|16384|8192|4096|2048")]
    [DefaultValue(4096)]
    [PropertyOrder(20)]
    [Style(DisplayStyle.Radio)]
    public int IqSamplingCount
    {
        get => _iqSamplingCount;
        set
        {
            _iqSamplingCount = value;
            SendCommand($"IQ:COUN {value}");
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
    //         if (value)
    //         {
    //             SendCommand("MEAS:SQU ON");
    //             SendCommand($"MEAS:DWEL {_dwellTime} s");
    //         }
    //         else
    //         {
    //             SendCommand("MEAS:SQU OFF");
    //         }
    //     }
    // }
    private bool _levelSwitch;

    [Parameter(AbilitySupport = FeatureType.TDOA | FeatureType.SSE)]
    [Name(ParameterNames.LevelSwitch)]
    [DisplayName("电平数据")]
    [Description("设置接收机是否输出电平")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(21)]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch
    {
        get => _levelSwitch;
        set
        {
            _levelSwitch = value;
            if (value)
            {
                AcceptDataRequest(DataType.Level);
                _dfDataSource.Register(_levelRawFilterforNineChannel);
            }
            else
            {
                RejectDataRequest(DataType.Level);
                _dfDataSource.UnRegister(_levelRawFilterforNineChannel);
            }
        }
    }

    private bool _spectrumSwitch;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.DPX | FeatureType.ITUM | FeatureType.FScne |
                                FeatureType.AmpDF | FeatureType.TDOA | FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.SpectrumSwitch)]
    [DisplayName("频谱数据")]
    [Description("设置是否获取频谱数据")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(22)]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch
    {
        get
        {
            lock (_parameterLock)
            {
                return _spectrumSwitch;
            }
        }
        set
        {
            lock (_parameterLock)
            {
                _spectrumSwitch = value;
                if (value)
                {
                    AcceptDataRequest(DataType.Spectrum);
                    _dfDataSource.Register(_spectrumRawFilterforNineChannel);
                }
                else
                {
                    RejectDataRequest(DataType.Spectrum);
                    _dfDataSource.UnRegister(_spectrumRawFilterforNineChannel);
                }
            }
        }
    }

    private bool _audioSwitch;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.FScne | FeatureType.FFDF |
                                FeatureType.SSE | FeatureType.IQRETRI)]
    [Name(ParameterNames.AudioSwitch)]
    [DisplayName("音频数据")]
    [Description("设置是否获取音频数据")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [PropertyOrder(23)]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch
    {
        get => _audioSwitch;
        set
        {
            _audioSwitch = value;
            if (value)
                AcceptDataRequest(DataType.Audio);
            else
                RejectDataRequest(DataType.Audio);
        }
    }

    private bool _ituSwitch;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.ITUM | FeatureType.IQRETRI)]
    [Name(ParameterNames.ItuSwitch)]
    [DisplayName("ITU数据")]
    [Description("设置是否进行ITU测量，并获取数据")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(24)]
    [Style(DisplayStyle.Switch)]
    public bool ItuSwitch
    {
        get => _ituSwitch;
        set
        {
            _ituSwitch = value;
            if (value)
                AcceptDataRequest(DataType.Itu);
            else
                RejectDataRequest(DataType.Itu);
        }
    }

    private bool _iqSwitch;

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.IQRETRI)]
    [Name(ParameterNames.IqSwitch)]
    [DisplayName("IQ数据")]
    [Description("设置是否获取IQ数据")]
    [Category(PropertyCategoryNames.DataSwitch)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(25)]
    [Style(DisplayStyle.Switch)]
    public bool IqSwitch
    {
        get => _iqSwitch;
        set
        {
            _iqSwitch = value;
            if (value)
                AcceptDataRequest(DataType.Iq);
            else
                RejectDataRequest(DataType.Iq);
        }
    }

    private bool _continuousIq;

    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("连续IQ")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [Description("设置是否获取连续IQ")]
    [DefaultValue(false)]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Switch)]
    public bool ContinuousIq
    {
        get => _continuousIq;
        set
        {
            _continuousIq = value;
            SendCommand($"IQ:COUN {(value ? -1 : _normSamplingRateIqCount)}");
        }
    }

    private string _ssMode = "none";

    [Parameter(AbilitySupport = FeatureType.IQRETRI)]
    [Name("ssMode")]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("流盘模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|none|record|playback",
        DisplayValues = "|常规模式|记录数据|回放数据")]
    [Description("设置流盘模式")]
    [DefaultValue("none")]
    [PropertyOrder(26)]
    [Style(DisplayStyle.Radio)]
    public string SsMode
    {
        get => _ssMode;
        set
        {
            _ssMode = value;
            if (value.Equals("record", StringComparison.OrdinalIgnoreCase))
            {
                // SendCommand("MEAS:HGE:READ OFF");
                // SendCommand("MEAS:HGE:WRIT ON");
            }
            else if (value.Equals("playback", StringComparison.OrdinalIgnoreCase))
            {
                // SendCommand("MEAS:HGE:READ ON");
                // SendCommand("MEAS:HGE:WRIT OFF");
            }
            // SendCommand("MEAS:HGE:READ OFF");
            // SendCommand("MEAS:HGE:WRIT OFF");
        }
    }

    #endregion

    #region 离散扫描/离散搜索

    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne, Template = typeof(MScanTemplate))]
    [Name(ParameterNames.MscanPoints)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描频点参数")]
    [PropertyOrder(6)]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MScanPoints { get; set; }

    [DefaultProperty("")]
    private class MScanTemplate
    {
        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Name("frequency")]
        [DisplayName("中心频率")]
        [Description("设置离散频点中心频率，单位：MHz")]
        [Category(PropertyCategoryNames.RadioControl)]
        [ValueRange(20.0d, 8000.0d, 6)]
        [DefaultValue(101.7d)]
        [Unit(UnitNames.MHz)]
        [PropertyOrder(1)]
        [Style(DisplayStyle.Input)]
        public double Frequency { get; set; }

        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Name("filterBandwidth")]
        [DisplayName("滤波带宽")]
        [Description("设置离散扫描中频滤波带宽")]
        [Category(PropertyCategoryNames.Demodulation)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|500|300|250|150|120|50|30|15|12|9|6|2.4|1.5",
            DisplayValues = "|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|2.4kHz|1.5kHz")]
        [DefaultValue(120.0d)]
        [Unit(UnitNames.MHz)]
        [PropertyOrder(2)]
        [Style(DisplayStyle.Bandwidth)]
        public double FilterBandwidth { get; set; }

        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Name("demMode")]
        [Category(PropertyCategoryNames.Demodulation)]
        [DisplayName("解调模式")]
        [Description("设置离散信号音频解调模式")]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ",
            DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE|IQ")]
        [DefaultValue(Modulation.Fm)]
        [PropertyOrder(4)]
        [Style(DisplayStyle.Dropdown)]
        public Modulation DemMode { get; set; }

        [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
        [Name("measureThreshold")]
        [DisplayName("测量门限")]
        [Description("获取或设置离散扫描进行占用度测量的门限值")]
        [Category(PropertyCategoryNames.DriverSpecified)]
        [ValueRange(-40, 120)]
        [DefaultValue(20)]
        [Unit(UnitNames.DBuV)]
        [PropertyOrder(10)]
        [Style(DisplayStyle.Slider)]
        public int MeasureThreshold { get; set; }

        public static explicit operator MScanTemplate(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            var template = new MScanTemplate();
            var type = template.GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    if (dict.ContainsKey(name))
                    {
                        object objValue = null;
                        if (property.PropertyType.IsEnum)
                            objValue = Utils.ConvertStringToEnum(dict[name].ToString(), property.PropertyType);
                        else if (property.PropertyType == typeof(Guid))
                            objValue = Guid.Parse(dict[name].ToString()!);
                        else if (property.PropertyType.IsValueType)
                            objValue = Convert.ChangeType(dict[name], property.PropertyType);
                        else
                            objValue = dict[name]; //Convert.ChangeType(value, prop.PropertyType);
                        property.SetValue(template, objValue, null);
                    }
                }
            }
            catch
            {
                // 容错代码
            }

            return template;
        }
    }

    #endregion

    #region DDC

    [Parameter(AbilitySupport = FeatureType.IFMCA)]
    [Name("maxChanCount")]
    [DisplayName("通道数量")]
    [Description("获取中频多路支持的最大子通道数据量")]
    [Category(PropertyCategoryNames.Misc)]
    [Browsable(false)]
    [ReadOnly(true)]
    [DefaultValue(64)]
    [PropertyOrder(7)]
    [ValueRange(64, 64)]
    [Style(DisplayStyle.Input)]
    public int MaxChanCount { get; set; }

    private Dictionary<string, object>[] _ddcChannels;

    [Parameter(AbilitySupport = FeatureType.IFMCA, Template = typeof(IfmcaTemplate))]
    [Name(ParameterNames.DdcChannels)]
    [DisplayName("中频信道")]
    [Description("设置中频多路参数")]
    [Category(PropertyCategoryNames.Misc)]
    [PropertyOrder(7)]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] DdcChannels // 参数变更，包括数量变更，需要重新更新参数
    {
        get => _ddcChannels;
        set
        {
            _ddcChannels = value;
            SetDdc();
        }
    }

    [DefaultProperty("")]
    private class IfmcaTemplate
    {
        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("frequency")]
        [DisplayName("中心频率")]
        [Description("设置离散频点中心频率，单位：MHz")]
        [Category(PropertyCategoryNames.RadioControl)]
        [ValueRange(20.0d, 8000.0d, 6)]
        [DefaultValue(101.7d)]
        [Unit(UnitNames.MHz)]
        [PropertyOrder(1)]
        [Style(DisplayStyle.Input)]
        public double Frequency { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("filterBandwidth")]
        [DisplayName("滤波带宽")]
        [Description("设置中频子通道滤波带宽")]
        [Category(PropertyCategoryNames.Demodulation)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|500|300|250|150|120|50|30|15|12|9|6|2.4|1.5",
            DisplayValues = "|500kHz|300kHz|250kHz|150kHz|120kHz|50kHz|30kHz|15kHz|12kHz|9kHz|6kHz|2.4kHz|1.5kHz")]
        [DefaultValue(150.0d)]
        [Unit(UnitNames.KHz)]
        [PropertyOrder(2)]
        [Style(DisplayStyle.Bandwidth)]
        public double FilterBandwidth { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("demMode")]
        [DisplayName("解调模式")]
        [Description("设置信号音频解调模式")]
        [Category(PropertyCategoryNames.Demodulation)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE",
            DisplayValues = "|FM|AM|PM|DMR|dPMR|CW|LSB|USB|PULSE")]
        [DefaultValue(Modulation.Fm)]
        [PropertyOrder(3)]
        [Style(DisplayStyle.Dropdown)]
        public Modulation DemMode { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name(ParameterNames.Xdb)]
        [DisplayName("XdB宽带")]
        [Description("设置ITU测量中XdB值 单位：dB")]
        [Category(PropertyCategoryNames.Measurement)]
        [ValueRange(0.0f, 100.0f)]
        [DefaultValue(26.0f)]
        [PropertyOrder(4)]
        [Style(DisplayStyle.Slider)]
        public float XdB { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name(ParameterNames.BetaValue)]
        [DisplayName("β宽带")]
        [Description("设置ITU测量中β值，单位：%")]
        [Category(PropertyCategoryNames.Measurement)]
        [ValueRange(0.0f, 100f)]
        [DefaultValue(1.0f)]
        [PropertyOrder(5)]
        [Style(DisplayStyle.Slider)]
        public float Beta { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("ifSwitch")]
        [DisplayName("中频输出")]
        [Description("设置是否输出子通道中频数据")]
        [Category(PropertyCategoryNames.DataSwitch)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|True|False",
            DisplayValues = "|开|关")]
        [DefaultValue(false)]
        [PropertyOrder(4)]
        [Style(DisplayStyle.Switch)]
        public bool IfSwitch { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("levelSwitch")]
        [DisplayName("电平数据")]
        [Description("设置子通道是否输出电平数据")]
        [Category(PropertyCategoryNames.DataSwitch)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|true|false",
            DisplayValues = "|开|关")]
        [DefaultValue(false)]
        [PropertyOrder(5)]
        [Style(DisplayStyle.Switch)]
        public bool LevelSwitch { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name(ParameterNames.SpectrumSwitch)]
        [DisplayName("频谱数据")]
        [Description("设置子通道是否输出频谱数据")]
        [Category(PropertyCategoryNames.DataSwitch)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|true|false",
            DisplayValues = "|开|关")]
        [DefaultValue(true)]
        [PropertyOrder(6)]
        [Style(DisplayStyle.Switch)]
        public bool SpectrumSwitch { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("audioSwitch")]
        [DisplayName("音频数据")]
        [Description("设置子通道是否输出音频数据")]
        [Category(PropertyCategoryNames.DataSwitch)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|true|false",
            DisplayValues = "|开|关")]
        [DefaultValue(false)]
        [PropertyOrder(7)]
        [Style(DisplayStyle.Switch)]
        public bool AudioSwitch { get; set; }

        [Parameter(AbilitySupport = FeatureType.IFMCA)]
        [Name("iqSwitch")]
        [DisplayName("IQ数据")]
        [Description("设置子通道是否输出IQ数据")]
        [Category(PropertyCategoryNames.DataSwitch)]
        [StandardValues(IsSelectOnly = true,
            StandardValues = "|true|false",
            DisplayValues = "|开|关")]
        [DefaultValue(false)]
        [PropertyOrder(8)]
        [Style(DisplayStyle.Switch)]
        public bool IqSwitch { get; }

        public static explicit operator IfmcaTemplate(Dictionary<string, object> dict)
        {
            if (dict == null) return null;
            var template = new IfmcaTemplate();
            var type = template.GetType();
            try
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var name =
                        Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                            ? property.Name
                            : nameAttribute.Name;
                    if (dict.ContainsKey(name))
                    {
                        object objValue = null;
                        if (property.PropertyType.IsEnum)
                            objValue = Utils.ConvertStringToEnum(dict[name].ToString(), property.PropertyType);
                        else if (property.PropertyType == typeof(Guid))
                            objValue = Guid.Parse(dict[name].ToString()!);
                        else if (property.PropertyType.IsValueType)
                            objValue = Convert.ChangeType(dict[name], property.PropertyType);
                        else
                            objValue = dict[name];
                        property.SetValue(template, objValue, null);
                    }
                }
            }
            catch
            {
                // 容错代码
            }

            return template;
        }
    }

    #endregion

    #region 安装属性

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.IpAddress)]
    [DisplayName("地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [Category(PropertyCategoryNames.Installation)]
    [DefaultValue("127.0.0.1")]
    [PropertyOrder(28)]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [DisplayName("端口")]
    [Description("设置连接到设置的网络控制端口")]
    [Category(PropertyCategoryNames.Installation)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(5025)]
    [PropertyOrder(29)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 5025;

    [Parameter(IsInstallation = true)]
    [Name("dfChannelCount")]
    [DisplayName("测向通道数")]
    [Description("设置测向通道数")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|双通道",
        StandardValues = "|2")]
    [DefaultValue(2)]
    [PropertyOrder(30)]
    [Style(DisplayStyle.Radio)]
    public int DfChannelCount { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("isTheoryDFind")]
    [DisplayName("理论测向")]
    [Description("设置是否采用理论测向，适用于双通道")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [DefaultValue(true)]
    [PropertyOrder(31)]
    [Style(DisplayStyle.Switch)]
    public bool IsTheoryDFind { get; set; } = true;

    [Parameter(IsInstallation = true)]
    [Name("dfAntennaRef")]
    [DisplayName("天线参考数")]
    [Description("设置测向天线参考数，应用于不同的天线打通方式，仅适用于双通道测向")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|原始|单参考|多参考|五单元",
        StandardValues = "|0|1|2|5")]
    [DefaultValue(2)]
    [PropertyOrder(32)]
    [Style(DisplayStyle.Radio)]
    public int DfAntennaRef { get; set; }

    private string _dfDataFolder = ".";

    [Parameter(IsInstallation = true)]
    [Name("dfDataFolder")]
    [DisplayName("因子文件目录")]
    [Description("设置包含有测向因子文件（*.vcy, antennas.json）的目录")]
    [Category(PropertyCategoryNames.Configuration)]
    [DefaultValue(".")]
    [PropertyOrder(33)]
    [ValueRange(double.NaN, double.NaN, 255)]
    [Style(DisplayStyle.Input)]
    public string DfDataFolder
    {
        get => _dfDataFolder;
        set
        {
            _dfDataFolder = value;
            if (_dfDataFolder == ".")
                // 在控制台下，"."定位到程序根目录
                // 但是在windows服务下，"."定位到"C:\Windows\system32\"文件夹下
                // 因此需要对目录进行处理
                _dfDataFolder = AppDomain.CurrentDomain.BaseDirectory;
        }
    }

    [Parameter(IsInstallation = true)]
    [Name("enableGPS")]
    [DisplayName("启用GPS")]
    [Description("设置接收机是否返回GPS数据")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [DefaultValue(false)]
    [PropertyOrder(34)]
    [Style(DisplayStyle.Switch)]
    public bool EnableGps { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("enableCompass")]
    [DisplayName("启用罗盘")]
    [Description("设置接收机是否返回电子罗盘数据")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [DefaultValue(false)]
    [PropertyOrder(35)]
    [Style(DisplayStyle.Switch)]
    public bool EnableCompass { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("compasssIntallingAngle")]
    [DisplayName("罗盘安装夹角")]
    [Description("设置电子罗盘安装夹角,单位：度")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(0.0f, 360.0f)]
    [DefaultValue(0.0f)]
    [PropertyOrder(36)]
    [Style(DisplayStyle.Slider)]
    public float CompassInstallingAngle { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("invalidScanAndSpectrumCount")]
    [DisplayName("无效频谱次数")]
    [Description("设置无效的扫描或频谱次数，模块将在任务启动时自动过滤次数无效的扫描或频谱数据")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(0, 20)]
    [DefaultValue(1)]
    [PropertyOrder(37)]
    [Style(DisplayStyle.Slider)]
    public int InvalidScanAndSpectrumCount { get; set; } = 1;

    [Parameter(IsInstallation = true)]
    [Name("levelCallibrationFromIQ")]
    [DisplayName("测量电平校准值")]
    [Description("设置测量电平修正值，通常为负数，用于修正本地通过IQ计算的电平值；例如：IQ算出的电平为100，若设置为-50，则展示的电平将变为50")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(-300, 300)]
    [DefaultValue(0)]
    [PropertyOrder(38)]
    [Style(DisplayStyle.Slider)]
    public int LevelCalibrationFromIq { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("levelCalibrationFromDF")]
    [DisplayName("测向电平校准值")]
    [Description("设置测向电平修正值，通常为负数，用于修改测向电平过高，与实际不相符的情况，同时也会影响到“电平门限”的设置")]
    [Category(PropertyCategoryNames.Configuration)]
    [ValueRange(-300, 300)]
    [DefaultValue(0)]
    [PropertyOrder(39)]
    [Style(DisplayStyle.Slider)]
    public int LevelCalibrationFromDf { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("enableRFAmplifier")]
    [DisplayName("大增益开关")]
    [Description("设置是否打开大增益开关")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(40)]
    [Style(DisplayStyle.Switch)]
    public bool EnableRfAmplifier { get; set; }

    [Parameter(IsInstallation = true)]
    [Name("iqWidth")]
    [DisplayName("IQ位宽")]
    [Description("设置IQ采样的位数，分为32位和16位")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|32|16",
        DisplayValues = "|32位|16位")]
    [DefaultValue(32)]
    [PropertyOrder(42)]
    [Style(DisplayStyle.Radio)]
    public int IqWidth { get; set; } = 32;

    [Parameter(IsInstallation = true)]
    [Name("calibPScanNoise")]
    [DisplayName("PScan校底")]
    [Description("设置是否打开PScan底噪校准功能")]
    [Category(PropertyCategoryNames.Configuration)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(false)]
    [PropertyOrder(43)]
    [Style(DisplayStyle.Switch)]
    public bool CalibPScanNoise { get; set; }

    #endregion
}