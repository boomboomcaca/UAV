using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMC;

[DeviceDescription(Name = "ESMC",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.IFOUT
                  | FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan
                  | FeatureType.FScne
                  | FeatureType.MScne,
    MaxInstance = 1,
    Version = "1.1.4",
    DeviceCapability = "0.5|3000|2000",
    Model = "ESMC",
    Description = "EMSC接收机")]
public partial class Esmc
{
    #region 其他

    /// <summary>
    ///     工作模式
    /// </summary>
    private WorkMode CurWorkMode
    {
        set
        {
            SendCommand("FREQ:MODE " + value);
            if (value == WorkMode.Cw)
            {
                SendCommand("TRAC:FEED:CONT MTRAC,NEV");
                SendCommand("TRAC:FEED:CONT IFPAN,ALW");
            }
            else
            {
                SendCommand("TRAC:FEED:CONT IFPAN,NEV");
                SendCommand("TRAC:FEED:CONT MTRAC,ALW");
            }
        }
    }

    #endregion

    #region 常规参数

    // HR 中心频率
    private double _frequency = 101.7;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(0.5d, 3000.0d, 6)]
    [DefaultValue(101.7d)]
    [Description("中心频率，默认单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            SendCommand($"SENS:FREQ {_frequency} MHz");
        }
    }

    // HR 滤波带宽
    private double _ifBandWidth = 100.0d;

    [PropertyOrder(7)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("滤波带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2.5|8|15|100|2000",
        DisplayValues = "|2.5kHz|8kHz|15kHz|100KHz|2MHz")]
    [DefaultValue(100.0d)]
    [Description("中频带宽、滤波带宽、解调带宽 默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandWidth
    {
        get => _ifBandWidth;
        set
        {
            _ifBandWidth = value;
            SendCommand($"SENS:BAND {_ifBandWidth} KHz");
        }
    }

    // HR 射频衰减
    private string _attenuation = "AUTO";

    [PropertyOrder(9)]
    [Name(ParameterNames.Attenuation)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("射频衰减")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AUTO|ON|OFF",
        DisplayValues = "|自动|开|关")]
    [DefaultValue("AUTO")]
    [Description("设备衰减 默认单位 dB")]
    [Style(DisplayStyle.Radio)]
    public string Attenuation
    {
        get => _attenuation;
        set
        {
            _attenuation = value.ToUpper();
            switch (_attenuation)
            {
                case "AUTO":
                    SendCommand("INP:ATT:AUTO ON");
                    break;
                case "ON":
                    SendCommand("INP:ATT:STAT OFF");
                    break;
                case "OFF":
                    SendCommand("INP:ATT:STAT OFF");
                    break;
            }
        }
    }

    private ScanMode _scanMode = ScanMode.Fscan;

    // HR 起始频率
    private double _startFrequency = 88.0d;

    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率")]
    [ValueRange(0.5d, 3000.0d, 6)]
    [DefaultValue(88.0d)]
    [Description("设置频段扫描起始频点，单位为MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            SendCommand($"SENS:FREQ:STAR {_startFrequency}MHz");
        }
    }

    // HR 终止频率
    private double _stopFrequency = 108.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率")]
    [ValueRange(0.5d, 3000.0d, 6)]
    [DefaultValue(108.0d)]
    [Description("设置扫描终止频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            SendCommand($"SENS:FREQ:STOP {_stopFrequency}MHz");
        }
    }

    // HR 扫描步进
    private double _stepFrequency = 25.0d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|1000|500|200|100|50|25|20|12.5|10|8.333|6.25|5|3.125",
        DisplayValues =
            "|2MHz|1MHz|500kHz|200kHz|100kHz|50kHz|25kHz|20kHz|12.5kHz|10kHz|8.333kHz|6.25kHz|5kHz|3.125kHz")]
    [DefaultValue(25.0d)]
    [Description("设置频段扫描步进。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            _stepFrequency = value;
            SendCommand(_scanMode == ScanMode.Fscan
                ? $"SENS:SWE:STEP {_stepFrequency}KHz"
                : $"SENS:BAND:STEP {_stepFrequency}KHz");
        }
    }

    [PropertyOrder(5)]
    [Name(ParameterNames.MscanPoints)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne,
        Template = typeof(DiscreteFrequencyTemplate))]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描频点")]
    [Description("设置离散扫描的频点参数")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = null;

    // HR 静噪门限
    private int _squelchThreshold = 10;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪门限")]
    [Description("设置门限值，单位 dBuV, 当信号电平超过门限时，进行音频解调")]
    [ValueRange(-10, 120)]
    [DefaultValue(10)]
    [Unit(UnitNames.DBuV)]
    [Style(DisplayStyle.Slider)]
    public int SquelchThreshold
    {
        // signal-controlled, adjustable −10 dBμV to 80 dBμV(max. 110 dBμV,120 dBμV with tuner 0)
        get => _squelchThreshold;
        set
        {
            _squelchThreshold = value;
            SendCommand($"OUTP:SQU:THR {_squelchThreshold} dBuV");
        }
    }

    // HR 静噪开关
    private bool _squelchSwitch = true;

    [PropertyOrder(11)]
    [Name(ParameterNames.SquelchSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("静噪开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否打开静噪开关，只有在首先开启静噪开关的时，静噪门限才有效")]
    [Children($"|{ParameterNames.SquelchThreshold}", true)]
    [Style(DisplayStyle.Switch)]
    public bool SquelchSwitch
    {
        get => _squelchSwitch;
        set
        {
            _squelchSwitch = value;
            SendCommand(_squelchSwitch ? "OUTP:SQU:STAT ON" : "OUTP:SQU:STAT OFF");
        }
    }

    // HR  解调模式
    private Modulation _demMode = Modulation.Fm;

    [PropertyOrder(10)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|LSB|USB|LOG|A1",
        DisplayValues = "|FM|AM|PULSE|LSB|USB|LOG|A1")]
    [DefaultValue(Modulation.Fm)]
    [Description("设置信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value; // ESMC支持的解调模式：FM,AM,PULSE,LSB,USB,LOG,A1
            if (value is Modulation.Usb or Modulation.Lsb)
                // TODO
                SendCommand("SENS:FREQ:AFC OFF");
            SendCommand("SENS:DEM " + _demMode);
        }
    }

    #endregion

    #region 高级参数

    // HR测量时间(单频测量)
    private float _measureTime = -1; // 单位s  

    [PropertyOrder(15)]
    [Name(ParameterNames.MeasureTime)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.SCAN | FeatureType.MScan | FeatureType.FScne | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("测量时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-1|0.001|0.002|0.004|0.005|0.01|0.02|0.03|0.06|0.1|0.2|0.4|0.5|1|2|5",
        DisplayValues = "|自动|1ms|2ms|4ms|5ms|10ms|20ms|30ms|60ms|100ms|200ms|400ms|500ms|1s|2s|5s")]
    [ValueRange(0, 5, 0.001)]
    [DefaultValue(-1)]
    [Description("设置测量时间，测量时间影响测量数据结果的准确性。")]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public float MeasureTime
    {
        get => _measureTime;
        set
        {
            _measureTime = value;
            //HR单位：秒
            SendCommand(Math.Abs(value - -1) < 1e-9 ? "MEAS:TIME DEF" : $"MEAS:TIME {_measureTime}");
        }
    }

    // HR检波方式 影响电平
    private DetectMode _detector = DetectMode.Pos;

    [PropertyOrder(14)]
    [Name(ParameterNames.Detector)]
    [Parameter(AbilitySupport =
        FeatureType.FFM | FeatureType.SCAN | FeatureType.FScne | FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("检波方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FAST|POS|AVG|RMS",
        DisplayValues = "|快速|峰值|均值|均方根")]
    [DefaultValue(DetectMode.Pos)]
    [Description("设置计算电平的处理方式（影响电平数据）")]
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
                case DetectMode.Fast:
                    cmd = "FAST";
                    break;
                case DetectMode.Pos:
                    cmd = "POS";
                    break;
                case DetectMode.Avg:
                    cmd = "PAV";
                    break;
                case DetectMode.Rms:
                    cmd = "RMS";
                    break;
            }

            if (cmd != string.Empty) SendCommand($"SENS:DET:FUNC {cmd}");
        }
    }

    #endregion

    #region 数据开关

    // HR 音频开关
    private bool _audioSwitch;

    [PropertyOrder(24)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.FScne | FeatureType.MScne)]
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
            _audioSwitch = value;
            //if (_Wav != null)
            //{
            //    _Wav.Stop();
            //}
            //if (_audioRecorder != null)
            //{
            //    _audioRecorder.Stop();
            //}
            //if (_Wav != null)
            //{
            //    _Wav.Start();
            //}
            //if (_audioRecorder != null)
            //{
            //    _audioRecorder.Start();
            //}
            SendCommand(value ? "SYSTEM:SPE:STAT ON" : "SYSTEM:SPE:STAT OFF");
        }
    }

    #endregion

    #region 安装参数

    [PropertyOrder(25)]
    [Name("gpib_code")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("GPIB板号")]
    [Description("设置设备的GPIB板卡号。")]
    [DefaultValue(0)]
    [ValueRange(double.NaN, double.NaN, 0)]
    [Style(DisplayStyle.Input)]
    public int GpibCode { get; set; }

    [PropertyOrder(26)]
    [Name("gpib_address")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备主地址")]
    [DefaultValue(24)]
    [Description("设置设备的GPIB地址。")]
    [ValueRange(double.NaN, double.NaN, 0)]
    [Style(DisplayStyle.Input)]
    public int GpibAddress { get; set; } = 24;

    [PropertyOrder(27)]
    [Name("gpib_address2")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备次地址")]
    [DefaultValue(0)]
    [Description("设置设备的GPIB第2地址。")]
    [ValueRange(double.NaN, double.NaN, 0)]
    [Style(DisplayStyle.Input)]
    public int GpibAddress2 { get; set; }

    #endregion
}