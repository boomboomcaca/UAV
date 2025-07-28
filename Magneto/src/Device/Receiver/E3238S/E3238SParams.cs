using System.ComponentModel;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.E3238S;

[DeviceDescription(Name = "E3238S",
    Manufacturer = "Agilent",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.FScne,
    MaxInstance = 1,
    Version = "1.0.1",
    DeviceCapability = "20|5031|36",
    Model = "E3238S",
    Description = "E3238S黑鸟接收机")]
public partial class E3238S
{
    #region 数据开关

    [PropertyOrder(13)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取频谱数据")]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch { get; set; }

    #endregion

    #region 常规参数

    private double _frequency = 101.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20.0d, 5031.0d)]
    [DefaultValue(101.7d)]
    [Description("单频监测时的中心频率,单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            SendCommand($"centerFrequency:{_frequency} MHz");
            SendCommand($"handoffRx1.Frequency:{_frequency} MHz");
            lock (_locker)
            {
                _dataQueue.Clear();
            }

            //处理参数切换过快，返回过多无效数据，导致卡顿
            Thread.Sleep(100);
        }
    }

    private double _ifBandwidth = 2000d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [Description("中频带宽设置,单位 kHz")]
    [StandardValues(DisplayValues = "|500kHz|1MHz|2MHz|5MHz|9MHz|18MHz|36MHz",
        StandardValues = "|500|1000|2000|5000|9000|18000|36000",
        IsSelectOnly = true)]
    [DefaultValue(2000d)]
    [DisplayName("中频带宽")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get => _ifBandwidth;
        set
        {
            _ifBandwidth = value;
            if (_ifBandwidth.Equals(500d))
                SendCommand("rbw:550 Hz");
            else if (_ifBandwidth.Equals(1000d))
                SendCommand("rbw:1.09 KHz");
            else if (_ifBandwidth.Equals(2000d))
                SendCommand("rbw:2.18 KHz");
            else if (_ifBandwidth.Equals(5000d))
                SendCommand("rbw:4.35 KHz");
            else if (_ifBandwidth.Equals(9000d))
                SendCommand("rbw:8.70 KHz");
            else if (_ifBandwidth.Equals(18000d))
                SendCommand("rbw:17.4 KHz");
            else if (_ifBandwidth.Equals(36000d)) SendCommand("rbw:34.8 KHz");
            SendCommand($"spanFrequency:{_ifBandwidth} kHz");
            lock (_locker)
            {
                _dataQueue.Clear();
            }

            //处理参数切换过快，返回过多无效数据，导致卡顿
            Thread.Sleep(100);
        }
    }

    private double _startFrequency = 20.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(20.0d, 6000.0d)]
    [DefaultValue(20.0d)]
    [Description("设置扫描的起始频率，单位：MHz。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            SendCommand($"startFrequency:{_startFrequency} MHz");
            //多任务时，参数设置过快，可能设置不成功
            Thread.Sleep(100);
        }
    }

    private double _stopFrequency = 101.70d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(20.0d, 6000.0d)]
    [DefaultValue(101.70d)]
    [Description("设置扫描的终止频率，单位为MHz。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            SendCommand($"stopFrequency:{_stopFrequency} MHz");
            //多任务时，参数设置过快，可能设置不成功
            Thread.Sleep(100);
        }
    }

    private double _stepFrequency = 25d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    //[StandardValues(DisplayValues = "|371.09kHz|185.55kHz|92.77kHz|46.39kHz|23.19kHz|11.60kHz|5.80kHz|2.90kHz|1.44kHz",
    //                StandardValues = "|371.09375|185.546875|92.7734375|46.3867187|23.1933594|11.5966797875|5.7983398|2.8991699|1.449585",
    //                IsSelectOnly = true)]
    [StandardValues(DisplayValues = "|370kHz|190kHz|95kHz|45kHz|25kHz|11kHz|6kHz|3kHz",
        StandardValues = "|370|190|90|45|25|11|6|3",
        IsSelectOnly = true)]
    [DefaultValue(25d)]
    [Description("设置扫描步进。单位kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            if (_stepFrequency.Equals(370d))
                SendCommand("binSpacing:371.09375 kHz");
            else if (_stepFrequency.Equals(190d))
                SendCommand("binSpacing:185.546875 kHz");
            else if (_stepFrequency.Equals(90d))
                SendCommand("binSpacing:92.7734375 kHz");
            else if (_stepFrequency.Equals(45d))
                SendCommand("binSpacing:46.3867187 kHz");
            else if (_stepFrequency.Equals(25d))
                SendCommand("binSpacing:23.1933594 kHz");
            else if (_stepFrequency.Equals(11d))
                SendCommand("binSpacing:11.5966797875 kHz");
            else if (_stepFrequency.Equals(6d))
                SendCommand("binSpacing:5.7983398 kHz");
            else if (_stepFrequency.Equals(3d)) SendCommand("binSpacing:2.8991699 kHz");
            //多任务时，参数设置过快，可能设置不成功
            Thread.Sleep(100);
        }
    }

    private double _filterBandwidth = 24.0d;

    [PropertyOrder(5)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [Description("设置解调带宽,单位 kHz")]
    [StandardValues(
        DisplayValues =
            "|5.2kHz|10.4kHz|15.0kHz|20.0kHz|24.0kHz|28.0kHz|32.0kHz|40.0kHz|48.0kHz|54.0kHz|60.0kHz|66.0kHz|72.0kHz|84.0kHz|96.0kHz|108.0kHz|120.0kHz|144.0kHz|192.0kHz|240.0kHz|288.0kHz|336.0kHz|384.0kHz",
        StandardValues = "|5.2|10.4|15|20|24|28|32|40|48|54|60|66|72|84|96|108|120|144|192|240|288|336|384",
        IsSelectOnly = true)]
    [DefaultValue(24d)]
    [DisplayName("解调带宽")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            SendCommand($"handoffRx1.bandwidth:{_filterBandwidth} KHz");
        }
    }

    private float _attenuation;

    [PropertyOrder(6)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(DisplayValues = "|0dB|4dB|10dB|16dB|20dB|24dB|30dB|36dB|40dB|44dB|48dB|50dB|56dB|60dB|62dB",
        StandardValues = "|0|4|10|16|20|24|30|36|40|44|48|50|56|60|62",
        IsSelectOnly = true)]
    [DisplayName("衰减值")]
    [DefaultValue(0f)]
    [Description("设置衰减值,单位为dB,范围为[0,62]。")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value;
            SendCommand($"attenuation:{_attenuation}");
        }
    }

    private float _gain = -110f;

    [PropertyOrder(7)]
    [Name(ParameterNames.Gain)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(DisplayValues = "|自动|-100dB|-90dB|-80dB|-70dB|-60dB|-50dB|-40dB|-30dB|-20dB|-10dB|0dB",
        StandardValues = "|-110|-100|-90|-80|-70|-60|-50|-40|-30|-20|-10|0",
        IsSelectOnly = true)]
    [DisplayName("增益")]
    [DefaultValue(-110f)]
    [Description("设置增益,范围为[-100,0]。单位dB")]
    [Browsable(false)]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            if (_gain.Equals(-110f))
            {
                SendCommand("handoffRx1.agc:On");
            }
            else
            {
                SendCommand("handoffRx1.agc:Off");
                SendCommand($"handoffRx1.manualGain:{_gain}");
            }
        }
    }

    private Modulation _dem = Modulation.Fm;

    [PropertyOrder(8)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(DisplayValues = "|FM|AM|CW|LSB|USB",
        StandardValues = "|FM|AM|CW|LSB|USB",
        IsSelectOnly = true)]
    [DefaultValue(Modulation.Fm)]
    [Description("对应信号的调制模式，选择适当的解调模式才能解调出正常声音。")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _dem;
        set
        {
            _dem = value;
            switch (_dem)
            {
                case Modulation.Fm:
                    SendCommand("handoffRx1.detection:FM");
                    break;
                case Modulation.Am:
                    SendCommand("handoffRx1.detection:AM");
                    break;
                case Modulation.Cw:
                    SendCommand("handoffRx1.detection:CW");
                    break;
                case Modulation.Lsb:
                    SendCommand("handoffRx1.detection:LSB");
                    break;
                case Modulation.Usb:
                    SendCommand("handoffRx1.detection:USB");
                    break;
            }
        }
    }

    private float _squelchthreshold = -73f;

    [PropertyOrder(9)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [DefaultValue(-73f)]
    [ValueRange(-73, 107)]
    [Description("设置静噪门限的值，单位dBuV。")]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public float SquelchThreshold
    {
        get => _squelchthreshold;
        set
        {
            _squelchthreshold = value;
            SendCommand("handoffRx1.squelch:" + (int)(_squelchthreshold - 107));
        }
    }

    private DetectMode _detector = DetectMode.Fast;

    [PropertyOrder(10)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FFM)]
    [Category(PropertyCategoryNames.Misc)]
    [DefaultValue(DetectMode.Fast)]
    [DisplayName("检波方式")]
    [StandardValues(DisplayValues = "|峰值|均方根|快速",
        StandardValues = "|PEAK|RMS|Fast",
        IsSelectOnly = true)]
    [Description("设置计算电平数据时的处理方式。")]
    public DetectMode Detector
    {
        get => _detector;
        set
        {
            _detector = value;
            switch (_detector)
            {
                case DetectMode.Rms:
                    SendCommand("averageType:RMS");
                    break;
                case DetectMode.Pos:
                    SendCommand("averageType:Peak");
                    break;
                case DetectMode.Fast:
                    SendCommand("averageType:Off");
                    break;
            }
        }
    }

    //private int _averages = 1;
    //[PropertyOrder(11)]
    //[Parameter(AbilitySupport= SpecificAbility.SCAN | SpecificAbility.FixFQ)]
    //[Category(PropertyCategoryNames.Advanced)]
    //[DisplayName("检波系数")]
    //[StandardValues(DisplayValues="|1|2|3|4|7|8|15|16|31|32|63|64|127|128|255|256|511|512|1023|1024",
    //                StandardValues="|1|2|3|4|7|8|15|16|31|32|63|64|127|128|255|256|511|512|1023|1024",
    //                IsSelectOnly = true)]
    //[DefaultValue(1)]
    //[Description("设置非快速检波时检波系数")]
    //[Browsable(false)]
    //public int Averages
    //{
    //    get
    //    {
    //        return _averages;
    //    }
    //    set
    //    {
    //        _averages = value;
    //        SendCommand(string.Format("averages:{0}", _averages));
    //    }
    //}
    private int _searchRxadcInputRange;

    [PropertyOrder(12)]
    [Name("searchRxadcInputRange")]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FFM)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("ADC参考电平")]
    [StandardValues(DisplayValues = "|-36|-33|-30|-27|-24|-21|-18|-15|-12|-9|-6|-3|0|3|6|9|12",
        StandardValues = "|-36|-33|-30|-27|-24|-21|-18|-15|-12|-9|-6|-3|0|3|6|9|12",
        IsSelectOnly = true)]
    [DefaultValue(0)]
    [Description("设置ADC参考电平")]
    public int SearchRxadcInputRange
    {
        get => _searchRxadcInputRange;
        set
        {
            //设置ADC参考电平 根据ADC类型的不同，参考电平不同，此设备为E1439D/70
            _searchRxadcInputRange = value;
            SendCommand($"searchRx1.adcInputRange:{_searchRxadcInputRange}");
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(14)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机IP地址")]
    [DefaultValue("192.168.120.188")]
    [Description("E3238S接收机接收指令的IP地址。")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.120.188";

    [PropertyOrder(15)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("接收机TCP端口")]
    [DefaultValue(7011)]
    [Description("E3238S接收机接收指令通讯端口号。")]
    [ValueRange(1000, 60000)]
    [Style(DisplayStyle.Slider)]
    public int TcpPort { get; set; } = 7011;

    #endregion
}