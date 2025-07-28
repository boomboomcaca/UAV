using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD195;

[DeviceDescription(Name = "EBD195",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFDF,
    MaxInstance = 1,
    Version = "1.1.8",
    Model = "EBD195",
    DeviceCapability = "20|8000|100",
    Description = "R&amp;S EBD195测向机")]
public partial class Ebd195
{
    #region 常规参数

    private double _frequency = 101.7;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20, 8000, 6)]
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
            SetFreqs(_frequency);
        }
    }

    private double _dfBandWidth = 15;

    [PropertyOrder(1)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2.5|8|15|100",
        DisplayValues = "|1kHz|2.5kHz|8kHz|15kHz|100kHz")]
    [DefaultValue(15d)]
    [Description("默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            SetBoundWidth(SplitBoundWidth(_dfBandWidth));
        }
    }

    private DFindMode _dfindMode = DFindMode.Normal;

    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号")]
    [DefaultValue("Normal")]
    [Description("设置测向模式，主要完成常规信号、连续测向、突发信号进行定制化测向")]
    [Style(DisplayStyle.Radio)]
    public DFindMode DFindMode
    {
        get => _dfindMode;
        set
        {
            _dfindMode = value;
            switch (_dfindMode)
            {
                case DFindMode.Normal:
                    SetDdfMode(0); //门限测向
                    break;
                case DFindMode.Feebleness:
                    SetDdfMode(1); //门限测向
                    break;
                case DFindMode.Gate:
                    SetDdfMode(2); //门限测向
                    break;
            }
        }
    }

    private int _qualityThreshold = 10;

    [PropertyOrder(2)]
    [Name(ParameterNames.QualityThreshold)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("质量门限")]
    [Description("设置测向质量门限，仅当测向质量超过门限时才返回测向数据")]
    [ValueRange(0, 100)]
    [DefaultValue(0)]
    [Style(DisplayStyle.Slider)]
    [Unit(UnitNames.Pct)]
    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            _qualityThreshold = value;
            SetSetSquelch(_qualityThreshold);
        }
    }

    public string BufIntegrationTime = "0.2";

    [PropertyOrder(3)]
    [Name("integrationTime")]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("积分时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0.1|0.2|0.5|1|2|5",
        DisplayValues = "|0.1s|0.2s|0.5s|1s|2s|5s")]
    [DefaultValue("0.2")]
    [Description("设置积分时间.")]
    [Unit(UnitNames.Sec)]
    [Style(DisplayStyle.Slider)]
    public string IntegrationTime
    {
        get => BufIntegrationTime;
        set
        {
            BufIntegrationTime = value;
            SetInterTime(SplitInterTime(BufIntegrationTime));
        }
    }

    private bool _levelSwitch;

    [PropertyOrder(4)]
    [Name(ParameterNames.LevelSwitch)]
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
                _media |= MediaType.Level;
            else
                _media &= ~MediaType.Level;
        }
    }

    #endregion 常规参数

    #region 安装参数

    [PropertyOrder(0)]
    [Name("netType")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("通信方式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|TCP|串口",
        DisplayValues = "|TCP|串口")]
    [DefaultValue("TCP")]
    [Description("设置与设备的通信方式，当选择TCP时，对应的IP和端口必须设置正确；选择串口时，对应的串口号和波特率必须设置正确。")]
    [Style(DisplayStyle.Radio)]
    public string NetType { get; set; } = "TCP";

    #region 网口

    [Parameter(IsInstallation = true)]
    [Name("ip")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("IP地址")]
    [Description("设置连接设备的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx。（注：当“通信方式”选择为TCP时生效！）")]
    [PropertyOrder(1)]
    [DefaultValue("192.168.200.201")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Ip { get; set; } = "192.168.200.201";

    [Parameter(IsInstallation = true)]
    [Name(ParameterNames.Port)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接到设置的网络端口。（注：当“通信方式”选择为TCP时生效！）")]
    [PropertyOrder(2)]
    [ValueRange(1024, 65535, 0)]
    [DefaultValue(4002)]
    [Style(DisplayStyle.Input)]
    public int Port { get; set; } = 4002;

    #endregion

    #region 串口

    [PropertyOrder(3)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("串口号")]
    [DefaultValue("COM3")]
    [Description("设置DC7520MOB的串口号，格式：COMx  。（注：当“通信方式”选择为串口时生效！）")]
    [ValueRange(double.NaN, double.NaN, 100)]
    [Style(DisplayStyle.Input)]
    public string Com { get; set; } = "COM3";

    [PropertyOrder(4)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("波特率")]
    [DefaultValue(115200)]
    [Description("设置DC7520MOB的串口波特率。（注：当“通信方式”选择为串口时生效！）")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200",
        DisplayValues = "|600|1200|2400|4800|9600|19200|38400|76800|115200")]
    [Style(DisplayStyle.Dropdown)]
    public int Baudrate { get; set; } = 115200;

    #endregion

    // Intermediate Frequency 太长了
    [PropertyOrder(6)]
    [Name("if")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("中频输入")]
    [Description("设置中频输入")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|10.7|21.4",
        DisplayValues = "|10.7MHz|21.4MHz")]
    [DefaultValue(10.7)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Radio)]
    public double If { get; set; } = 10.7;

    [PropertyOrder(7)]
    [Name("haveCompass")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("电子罗盘")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|有|无")]
    [DefaultValue(false)]
    [Description("测向机是否自带电子罗盘")]
    [Style(DisplayStyle.Switch)]
    public bool HaveCompass { get; set; }

    [Name("reportingDirection")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("公布方位")]
    [Description("是否通过消息对外公布地理方位")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|是|否")]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Switch)]
    [DefaultValue(true)]
    public bool ReportingDirection { get; set; }

    [Name("extraAngle")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("补偿角度")]
    [Description("设置角度补偿值，有效值范围-180~180，主要用于工程安装时的角度纠偏")]
    [ValueRange(-180, 180)]
    [PropertyOrder(9)]
    [DefaultValue(0)]
    [Style(DisplayStyle.Slider)]
    public int ExtraAngle { get; set; }

    #endregion 安装参数
}