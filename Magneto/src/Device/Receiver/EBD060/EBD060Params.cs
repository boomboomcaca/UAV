using System;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD060;

[DeviceDescription(Name = "EBD060",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFDF,
    MaxInstance = 1,
    Version = "1.0.3",
    DeviceCapability = "20|3000|200",
    Model = "EBD060",
    Description = "EBD060测向接收机(不支持多任务、只支持单频测向)")]
public partial class Ebd060
{
    #region 常规参数

    private double _frequency = 101.7d;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [Description("测向中心频率，单位 MHz")]
    [ValueRange(20, 3000)]
    [DefaultValue(101.7d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency
    {
        get => _frequency;
        set
        {
            _frequency = value;
            // 设置频率时，需要同时设备带宽
            var cmd = $"f {_frequency:0.000000}mhz {_dfBandWidth:0.000}khz";
            SendCmd(cmd);
        }
    }

    private double _dfBandWidth = 200d;

    [PropertyOrder(1)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [Description("设备测向时的信号带宽，单位 kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0.6|1.2|2.4|4.8|6|7.5|12|15|30|60|120|200",
        DisplayValues = "|600Hz|1.2kHz|2.4kHz|4.8kHz|6kHz|7.5kHz|12kHz|15kHz|30kHz|60kHz|120kHz|200kHz"
    )]
    [DefaultValue(200d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            // 设置带宽时，需要同时设置频率
            var cmd = $"f {_frequency:0.000000}MHz {_dfBandWidth:0.000}KHz";
            SendCmd(cmd);
        }
    }

    private double _filterBandwidth = 200d;

    [PropertyOrder(2)]
    [Name(ParameterNames.FilterBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [Description("设置音频解调带宽，单位：kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0.6|1.2|2.4|4.8|6|7.5|12|15|30|200",
        DisplayValues = "|60Hz|1.2kHz|2.4kHz|4.8kHz|6kHz|7.5kHz|12kHz|15kHz|30kHz|200kHz")]
    [DefaultValue(200d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double FilterBandwidth
    {
        get => _filterBandwidth;
        set
        {
            _filterBandwidth = value;
            var cmd = $"afbw {value:0.000}kHz";
            SendCmd(cmd);
        }
    }

    private float _gain;

    [PropertyOrder(3)]
    [Name(ParameterNames.Gain)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("增益控制")]
    [Description("增益/衰减控制")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|-2|-1|0|1|2|3|4|5|6|7|8|9|10",
        DisplayValues = "|快速自动|自动|M0|M1|M2|M3|M4|M5|M6|M7|M8|M9|M10"
    )]
    [Style(DisplayStyle.Slider)]
    public float Gain
    {
        get => _gain;
        set
        {
            _gain = value;
            var cmd = string.Empty;
            if (Math.Abs(value - -2) < 1e-9)
                cmd = "gain auto";
            else if (Math.Abs(value - -1) < 1e-9)
                cmd = "gain auto1";
            else
                cmd = $"gain M{value}";
            SendCmd(cmd);
        }
    }

    private DFindMode _dfindMode = DFindMode.Feebleness;

    [PropertyOrder(4)]
    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [Description("设置当前的测向模式。【注：连续测向模式下测向门限无效。】")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号"
    )]
    [DefaultValue(DFindMode.Feebleness)]
    public DFindMode DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            var cmd = string.Empty;
            switch (value)
            {
                case DFindMode.Normal:
                    cmd = "amode normal";
                    break;
                case DFindMode.Feebleness:
                    cmd = "amode cont";
                    break;
                case DFindMode.Gate:
                    cmd = "amode gate";
                    break;
            }

            SendCmd(cmd);
        }
    }

    private float _levelThreshold = -10.0f;

    [PropertyOrder(5)]
    [Name(ParameterNames.LevelThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("电平门限")]
    [Description("设置测向的电平门限，仅当信号电平超过门限时才返回测向数据。【注：连续测向模式下测向门限无效。在此设备中测向门限也是静躁门限。】")]
    [ValueRange(-30, 99)]
    [DefaultValue(-10.0f)]
    public float LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            _levelThreshold = value;
            var cmd = $"th {value}db";
            SendCmd(cmd);
        }
    }

    private int _quality = 1;

    [PropertyOrder(6)]
    [Name(ParameterNames.QualityThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限")]
    [ValueRange(0, 99)]
    [DefaultValue(1)]
    [Browsable(false)]
    public int QualityThreshold
    {
        get => _quality;
        set
        {
            _quality = value;
            var cmd = $"qu {value}";
            SendCmd(cmd);
        }
    }

    private Modulation _demMode = Modulation.Fm;

    [PropertyOrder(7)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [Description("设置音频解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|LSB|USB|CW",
        DisplayValues = "|FM|AM|LSB|USB|CW"
    )]
    [DefaultValue(Modulation.Fm)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode
    {
        get => _demMode;
        set
        {
            _demMode = value;
            var cmd = $"dem {value}";
            SendCmd(cmd);
        }
    }

    private double _bfo = 10.0d;

    [PropertyOrder(8)]
    [Name("bfo")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("BFO频率")]
    [Description("设置数字解调的BFO频率，用于LSB,USB,CW解调，单位：Hz")]
    [ValueRange(0, 9990)]
    [DefaultValue(10)]
    [Unit(UnitNames.Hz)]
    public double Bfo
    {
        get => _bfo;
        set
        {
            _bfo = value;
            if (_demMode is Modulation.Cw or Modulation.Usb or Modulation.Lsb)
            {
                var cmd = $"bfo {value}hz";
                SendCmd(cmd);
            }
        }
    }

    public float BufIntegrationTime = 100.0f;

    [PropertyOrder(0)]
    [Name(ParameterNames.IntegrationTime)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [Description("设置测向积分时间，单位 毫秒(ms)")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|100|200|500|1000|2000|5000",
        DisplayValues = "|100ms|200ms|500ms|1000ms|2000ms|5000ms"
    )]
    [DefaultValue(100.0f)]
    [Unit(UnitNames.Ms)]
    public float IntegrationTime
    {
        get => BufIntegrationTime;
        set
        {
            BufIntegrationTime = value;
            var cmd = $"rmode M0{value}ms M1 M2";
            SendCmd(cmd);
        }
    }

    #endregion

    #region 数据开关

    private bool _preamp;

    [PropertyOrder(1)]
    [Name("preamplifier")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("前置放大")]
    [Description("设置是否开启前置放大器")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关"
    )]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool Preamplifier
    {
        get => _preamp;
        set
        {
            _preamp = value;
            var sw = value ? "on" : "off";
            var cmd = $"pa {sw}";
            SendCmd(cmd);
        }
    }

    [PropertyOrder(0)]
    [Name(ParameterNames.AudioSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("音频开关")]
    [Description("是否监听音频")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关"
    )]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool AudioSwitch { get; set; }

    [PropertyOrder(1)]
    [Name(ParameterNames.SpectrumSwitch)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.DataSwitch)]
    [DisplayName("频谱数据")]
    [Description("频谱数据开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|开|关"
    )]
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
    public bool SpectrumSwitch { get; set; }

    #endregion

    #region 安装参数

    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("地址")]
    [Description("网络连接地址，IPV4格式[x.x.x.x]")]
    [DefaultValue("127.0.0.1")]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "127.0.0.1";

    [PropertyOrder(0)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设备绑定的TCP端口号")]
    [DefaultValue(1293)]
    public int Port { get; set; } = 1293;

    [PropertyOrder(0)]
    [Name("north")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("北偏角校正值")]
    [Description("北偏角校正值.如-15|30,符号 | 左端表示低端天线与正北方向的夹角，右端表示高端天线与正北方向的夹角。")]
    [DefaultValue("0|0")]
    public string North { get; set; } = "0|0";

    #endregion
}