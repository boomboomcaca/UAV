using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9030A;

[DeviceDescription(Name = "N9030A",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.AmpDF
                  | FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan,
    MaxInstance = 1,
    Version = "1.0.6",
    Model = "N9030A",
    DeviceCapability = "0|26500|20000",
    Description = "安捷伦频谱仪设备")]
[DeviceDescription(Name = "N9020A",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.AmpDF
                  | FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan,
    MaxInstance = 1,
    Version = "1.0.6",
    DeviceCapability = "0|26500|20000",
    Model = "N9020A",
    Description = "安捷伦频谱仪设备")]
[DeviceDescription(Name = "N9030B",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.Monitoring,
    FeatureType = FeatureType.AmpDF
                  | FeatureType.FFM
                  | FeatureType.SCAN
                  | FeatureType.MScan | FeatureType.FASTEMT,
    MaxInstance = 1,
    Version = "1.0.0",
    Model = "N9030B",
    DeviceCapability = "10|13600|25000",
    Description = "是德频谱仪设备")]
public partial class N9030A
{
    #region 常规参数

    private double _frequency = 101.7d;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率(MHz)")]
    [DefaultValue(101.7d)]
    [ValueRange(0d, 26500d)]
    [ValueRange(10d, 13600.0d, Key = "N9030B")]
    [Description("中心频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get
        {
            var str = SendCommand("FREQ:CENT?");
            if (!string.IsNullOrEmpty(str))
                _frequency = double.Parse(str) / 1000000;
            return _frequency;
        }
        set
        {
            if (value is >= 0 and <= 26500)
            {
                //使能mark1
                SendCommand("CALC:MARK1:MODE POS");
                //将mark1与轨迹1绑定
                SendCommand("CALC:MARK1:TRAC 1");
                //设置工作频率
                SendCommand("FREQ:CENT " + value + "MHz");
                //设置mark1频率，以便读取中心频率的电平
                SendCommand("CALC:MARK1:X " + value + "MHz");
                _frequency = value; //客户端收取天线因子时调整
            }
        }
    }

    private double _ifBandwidth = 200d;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中频带宽")]
    [StandardValues(
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|200kHz|150kHz|120kHz|100kHz|80kHz|50kHz|30kHz|25kHz|12.5kHz|8kHz|6kHz|3kHz|1kHz",
        StandardValues =
            "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|200|150|120|100|80|50|30|25|12.5|8|6|3|1",
        IsSelectOnly = true)]
    [StandardValues(
        DisplayValues =
            "|25MHz|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|200kHz|150kHz|120kHz|100kHz|80kHz|50kHz|30kHz|25kHz|12.5kHz|8kHz|6kHz|3kHz|1kHz",
        StandardValues =
            "|25000|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|200|150|120|100|80|50|30|25|12.5|8|6|3|1",
        IsSelectOnly = true, Key = "N9030B")]
    [DefaultValue(200d)]
    [Description("设置信号的中频带宽。单位：kHz。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandwidth
    {
        get
        {
            var str = SendCommand("FREQ:SPAN?");
            if (!string.IsNullOrEmpty(str))
                _ifBandwidth = double.Parse(str) / 1000;
            return _ifBandwidth;
        }
        set
        {
            SendCommand($"FREQ:SPAN {value}kHz");
            _ifBandwidth = value;
        }
    }

    private double _startFrequency = 88d;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("起始频率(MHz)")]
    [ValueRange(0d, 26500d)]
    [ValueRange(10d, 13600.0d, Key = "N9030B")]
    [DefaultValue(88d)]
    [Description("设置频段扫描的起始频率。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            _startFrequency = value;
            SendCommand("FREQ:STAR " + _startFrequency + " MHz");
        }
    }

    private double _stopFrequency = 108d;

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("终止频率(MHz)")]
    [ValueRange(10d, 13600.0d, Key = "N9030B")]
    [ValueRange(0d, 26500d)]
    [DefaultValue(108d)]
    [Description("设置扫描终止频率,单位MHz。")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    [Browsable(false)]
    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            _stopFrequency = value;
            SendCommand("FREQ:STOP " + _stopFrequency + " MHz");
        }
    }

    [Parameter(AbilitySupport = FeatureType.SCAN | FeatureType.AmpDF)]
    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("扫描步进")]
    [StandardValues(DisplayValues = "|10kHz|12.5kHz|25kHz|50kHz|100kHz|150kHz|300kHz|500kHz|1MHz",
        StandardValues = "|10|12.5|25|50|100|150|300|500|1000", IsSelectOnly = true)]
    [DefaultValue(25d)]
    [Description("设置扫描步进，单位kHz。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Dropdown)]
    [Browsable(false)]
    public double StepFrequency { get; set; } = 25d;

    [Parameter(AbilitySupport = FeatureType.MScan, Template = typeof(DiscreteFrequency))]
    [PropertyOrder(5)]
    [Name(ParameterNames.MscanPoints)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("离散频点")]
    [Description("设置离散扫描的频点参数。")]
    [Browsable(false)]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] MscanPoints { get; set; } = Array.Empty<Dictionary<string, object>>();

    private float _referenceLevel = 120f;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(5)]
    [Name("referenceLevel")]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("参考电平(dBm)")]
    [DefaultValue(-120f)]
    [ValueRange(-130, 20.0d, Key = "N9030B")]
    [Description("参考电平，单位dBm")]
    [Unit(UnitNames.DBm)]
    [Style(DisplayStyle.Input)]
    public float ReferenceLevel
    {
        get
        {
            var str = SendCommand("DISP:WIND:TRAC:Y:RLEV?");
            if (!string.IsNullOrEmpty(str))
                _referenceLevel = float.Parse(str);
            return _referenceLevel;
        }
        set
        {
            if (value is >= -130 and <= 20)
            {
                SendCommand("DISP:WIND:TRAC:Y:RLEV " + value + "dBm");
                _referenceLevel = value; //客户端收取天线因子时调整
            }
        }
    }

    private float _attenuation = -1.0f;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(6)]
    [Name(ParameterNames.Attenuation)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(DisplayValues = "|自动|5dB|10dB|15dB|20dB|30dB|40dB|50dB|60dB|70dB",
        StandardValues = "|-1|5|10|15|20|30|40|50|60|70", IsSelectOnly = true)]
    [StandardValues(
        DisplayValues =
            "|自动|2dB|4dB|6dB|8dB|10dB|12dB|14dB|16dB|18dB|20dB|22dB|24dB|26dB|28dB|30dB|32dB|34dB|36dB|38dB|40dB",
        StandardValues = "|-1|2|4|6|8|10|12|14|16|18|20|22|24|26|28|30|32|34|36|38|40", IsSelectOnly = true,
        Key = "N9030B")]
    [DisplayName("衰减值")]
    [DefaultValue(-1.0f)]
    [Description("设置衰减值，单位为dB。")]
    [Unit(UnitNames.Db)]
    [Style(DisplayStyle.Slider)]
    public float Attenuation
    {
        get
        {
            var str = SendCommand("POW:ATT?");
            if (!string.IsNullOrEmpty(str))
                _attenuation = float.Parse(str);
            return _attenuation;
        }
        set
        {
            if (Math.Abs(value - -1.0f) < 1e-9)
            {
                SendCommand("POW:ATT:AUTO ON");
            }
            else
            {
                SendCommand("POW:ATT:AUTO OFF");
                SendCommand("POW:ATT " + value);
            }

            _attenuation = value;
        }
    }

    #endregion

    #region 高级参数

    private string _detector = "NORM";

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.MScan | FeatureType.SCAN)]
    [PropertyOrder(7)]
    [Name(ParameterNames.Detector)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("检波方式")]
    [Description("检波方式。")]
    [StandardValues(DisplayValues = "|常规|平均值|正峰值|负峰值|采样",
        StandardValues = "|NORM|AVER|POS|NEG|SAMP", IsSelectOnly = true)]
    [DefaultValue("NORM")]
    [Style(DisplayStyle.Radio)]
    public string Detector
    {
        get
        {
            _detector = SendCommand("DET:TRAC1?");
            return _detector;
        }
        set
        {
            SendCommand("DET:TRAC1 " + value);
            _detector = value;
        }
    }

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.MScan | FeatureType.SCAN)]
    [Category(PropertyCategoryNames.Misc)]
    [PropertyOrder(8)]
    [Name("preamp")]
    [DisplayName("前端放大器")]
    [DefaultValue("FALSE")]
    [Description("控制前端放大器开关。")]
    [StandardValues(DisplayValues = "|低频段|全频段|关", StandardValues = "|LOW|FULL|FALSE", IsSelectOnly = true)]
    public string Preamp
    {
        get
        {
            var str = SendCommand("POW:GAIN?");
            if (str == "0") return "FALSE";

            str = SendCommand("POW:GAIN:BAND?");
            return str;
        }
        set
        {
            if (value.Equals("FALSE"))
            {
                SendCommand("POW:GAIN 0");
            }
            else
            {
                SendCommand("POW:GAIN 1");
                SendCommand("POW:GAIN:BAND " + value);
            }
        }
    }

    private double _resolutionBandwidth;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(9)]
    [Name(ParameterNames.ResolutionBandwidth)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("分辨率带宽")]
    [StandardValues(DisplayValues = "|自动|5kHz|10kHz|25kHz|50kHz|100kHz|300kHz|500kHz|1MHz|2MHz|5MHz|8MHz",
        StandardValues = "|0|5|10|25|50|100|300|500|1000|2000|5000|8000", IsSelectOnly = true)]
    [StandardValues(DisplayValues = "|自动|1kHz|3kHz|10kHz|30kHz|100kHz|300kHz|1MHz|3MHz|10MHz",
        StandardValues = "|0|1|3|10|30|100|300|1000|3000|10000", IsSelectOnly = true, Key = "N9030B")]
    [DefaultValue(0d)]
    [Description("设置分辨率带宽滤波器的值。单位：kHz。")]
    [Unit(UnitNames.KHz)]
    public double ResolutionBandwidth
    {
        get
        {
            var str = SendCommand("BAND:RES?");
            if (!string.IsNullOrEmpty(str))
                _resolutionBandwidth = double.Parse(str) / 1000;
            return _resolutionBandwidth;
        }
        set
        {
            if (value.Equals(0))
            {
                SendCommand("BAND:RES:AUTO ON");
            }
            else
            {
                SendCommand("BAND:RES:AUTO OFF");
                SendCommand($"BAND:RES {value}kHz");
                _resolutionBandwidth = value;
            }
        }
    }

    private double _videoBandwidth;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(9)]
    [Name("videoBandwidth")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("视频带宽")]
    [StandardValues(DisplayValues = "|自动|1kHz|3kHz|10kHz|30kHz|100kHz|300kHz|1MHz|3MHz|50MHz",
        StandardValues = "|0|1|3|10|30|100|300|1000|3000|50000", IsSelectOnly = true)]
    [DefaultValue(0d)]
    [Description("设置视频带宽滤波器的值。单位：kHz。")]
    [Unit(UnitNames.KHz)]
    public double VideoBandwidth
    {
        get
        {
            var str = SendCommand("BAND:VID?");
            if (!string.IsNullOrEmpty(str))
                _videoBandwidth = double.Parse(str) / 1000;
            return _videoBandwidth;
        }
        set
        {
            if (value.Equals(0))
            {
                SendCommand("BAND:VID:AUTO ON");
            }
            else
            {
                SendCommand("BAND:VID:AUTO OFF");
                SendCommand($"BAND:VID {value}kHz");
                _videoBandwidth = value;
            }
        }
    }

    private bool _preAmpSwitch = true;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN)]
    [PropertyOrder(9)]
    [Name(ParameterNames.PreAmpSwitch)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("前置预放")]
    [Description("前置预放开关。")]
    [StandardValues(DisplayValues = "|开|关", StandardValues = "|true|false", IsSelectOnly = true)]
    [DefaultValue(true)]
    public bool PreAmpSwitch
    {
        get => _preAmpSwitch;
        set
        {
            SendCommand(value ? "POW:GAIN 1" : "POW:GAIN 0");
            _preAmpSwitch = value;
        }
    }

    [PropertyOrder(10)]
    [Name(ParameterNames.IntegrationTime)]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间，单位 秒(s)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0.1|0.2|0.5|1|2|5|10",
        DisplayValues = "|100ms|200ms|500ms|1s|2s|5s|10s"
    )]
    [DefaultValue(10.0f)]
    [Unit(UnitNames.Sec)]
    public float IntegrationTime { get; set; }

    [PropertyOrder(10)]
    [Name("repeatTimes")]
    [Parameter(AbilitySupport = FeatureType.None)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("重复次数")]
    [Description("设置重复次数")]
    [ValueRange(1, 5)]
    [DefaultValue(5)]
    public int RepeatTimes { get; set; }

    private int _scanTime;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.MScan | FeatureType.SCAN)]
    [PropertyOrder(10)]
    [Name("scanTime")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫描时间")]
    [StandardValues(DisplayValues = "|自动|1ms|5ms|10ms|20ms|50ms|100ms|250ms|500ms|1s|3s|5s|10s|20s|50s",
        StandardValues = "|0|1|5|10|20|50|100|250|500|1000|3000|5000|10000|20000|50000", IsSelectOnly = true)]
    [DefaultValue(0)]
    [Description("扫描时间设置。")]
    [Unit(UnitNames.Ms)]
    public int ScanTime
    {
        get
        {
            var str = SendCommand("SWE:TIME?");
            if (!string.IsNullOrEmpty(str))
                _scanTime = int.Parse(str) * 1000;
            return _scanTime;
        }
        set
        {
            if (value.Equals(0))
            {
                SendCommand("SWE:TIME:AUTO ON");
            }
            else
            {
                SendCommand("SWE:TIME:AUTO OFF");
                SendCommand("SWE:TIME " + value + "ms");
                _scanTime = value;
            }
        }
    }

    private int _points = 601;

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM)]
    [PropertyOrder(11)]
    [Name("points")]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("频谱点数")]
    [Description("设置返回的频谱数据的点数。")]
    [ValueRange(1, 40001)]
    [DefaultValue(601)]
    public int Points
    {
        get
        {
            var points = int.Parse(SendCommand("SWE:POIN?"));
            return points;
        }
        set
        {
            SendCommand("SENS:SWE:POIN " + value);
            _points = value;
        }
    }

    private string _unit = "DBUV";

    [Parameter(AbilitySupport = FeatureType.AmpDF | FeatureType.FFM | FeatureType.SCAN | FeatureType.MScan)]
    [PropertyOrder(12)]
    [Name("dbuv")]
    [Category(PropertyCategoryNames.Misc)]
    [Browsable(false)]
    [ReadOnly(true)]
    [DisplayName("单位")]
    [Description("设置电平幅度单位，比如dBm,dBpW等。")]
    [StandardValues(DisplayValues = "|dBm|dBmV|dBmA|dBμV|dBμA|dBpW",
        StandardValues = "|DBM|DBMV|DBMA|DBUV|DBUA|DBPW", IsSelectOnly = true)]
    [DefaultValue("DBUV")]
    public string Unit
    {
        get
        {
            var strUnit = SendCommand("UNIT:POW?");
            _unit = strUnit;
            return _unit;
        }
        set
        {
            SendCommand("UNIT:POW " + value);
            _unit = value;
        }
    }

    #endregion

    #region 数据开关

    [Parameter(AbilitySupport = FeatureType.FFM | FeatureType.AmpDF)]
    [PropertyOrder(13)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关。")]
    [StandardValues(DisplayValues = "|开|关", StandardValues = "|true|false", IsSelectOnly = true)]
    [DefaultValue(true)]
    public bool SpectrumSwitch { get; set; } = true;

    [Name(ParameterNames.LevelSwitch)]
    [PropertyOrder(26)]
    [Parameter(AbilitySupport = FeatureType.FFM)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("电平数据")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关")]
    [DefaultValue(true)]
    [Description("设置是否获取电平数据")]
    [Style(DisplayStyle.Switch)]
    public bool LevelSwitch { get; set; } = true;

    #endregion

    #region 安装参数

    /// <summary>
    ///     网络端口
    /// </summary>
    private const int Netport = 5025;

    [Parameter(IsInstallation = true)]
    [PropertyOrder(14)]
    [Name("ip")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("网络地址")]
    [Description("配置设备的网络地址。")]
    [DefaultValue("127.0.0.1")]
    public string Ip { get; set; } = "127.0.0.1";

    #endregion
}