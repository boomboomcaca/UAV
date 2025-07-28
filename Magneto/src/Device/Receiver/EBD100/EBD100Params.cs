using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD100;

[DeviceDescription(Name = "EBD100",
    Manufacturer = "R&amp;S",
    DeviceCategory = ModuleCategory.DirectionFinding,
    FeatureType = FeatureType.FFDF,
    MaxInstance = 1,
    Version = "1.0.4",
    DeviceCapability = "20|3000|100",
    Model = "EBD100",
    Description = "EBD100测向机")]
public partial class Ebd100 : IAntennaController
{
    #region 常规参数

    private double _frequency = 101.7;

    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(20, 3000)]
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

    private string _dfBandWidth = "100";

    [PropertyOrder(1)]
    [Name(ParameterNames.DfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|1|2.5|8|15|100",
        DisplayValues = "|1kHz|2.5kHz|8kHz|15kHz|100kHz")]
    [DefaultValue("100")]
    [Description("默认单位 kHz")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public string DfBandWidth
    {
        get => _dfBandWidth;
        set
        {
            _dfBandWidth = value;
            SetBandWidth(SplitBandWidth(_dfBandWidth));
        }
    }

    private DFindMode _dfindMode = DFindMode.Feebleness;

    [Name(ParameterNames.DfindMode)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("测向模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|Normal|Feebleness|Gate",
        DisplayValues = "|常规信号|弱小信号|突发信号")]
    [DefaultValue("Continuous")]
    [Description("设置测向模式，主要完成常规信号、连续测向、突发信号进行定制化测向")]
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
    [Description("设置测向质量门限，仅当测向质量超过门限且侧向模式为非连续测向时才返回测向数据")]
    [ValueRange(0, 100)]
    [DefaultValue(0)]
    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            _qualityThreshold = value;
            SetSetSquelch(_qualityThreshold);
        }
    }

    public string BufIntegrationTime = "0.1";

    [PropertyOrder(3)]
    [Name(ParameterNames.IntegrationTime)]
    [Parameter(AbilitySupport = FeatureType.FFDF)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("积分时间")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0.1|0.2|0.5|1|2|5",
        DisplayValues = "|0.1s|0.2s|0.5s|1s|2s|5s")]
    [DefaultValue("0.1")]
    [Description("设置积分时间.测向模式为连续测向时积分时间不起作用")]
    [Unit(UnitNames.Sec)]
    public string IntegrationTime
    {
        get => BufIntegrationTime;
        set
        {
            BufIntegrationTime = value;
            SetInterTime(SplitInterTime(BufIntegrationTime));
        }
    }

    #endregion

    #region 数据开关

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
    [Browsable(false)]
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

    #endregion

    #region 安装参数

    [PropertyOrder(5)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口")]
    [Description("设置连接设备的串口端口号(COM1,COM2…),只需要输入数字即可")]
    [DefaultValue(1)]
    public int Port { get; set; } = 1;

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
    [DefaultValue("10.7")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Radio)]
    public string If { get; set; } = "10.7";

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
    [DefaultValue(true)]
    [Style(DisplayStyle.Switch)]
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

    #endregion

    #region 安装属性

    //天线安装信息模板,包含所有配置天线
    private AntennaInfo[] _antennaTemplates;

    /// <summary>
    ///     天线集合，集合中每一个元素都是一个字典类型，保存了天线信息的键值对
    /// </summary>
    [Parameter(IsInstallation = true, Template = typeof(AntennaInfo))]
    [Name("antennaTemplates")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线集合")]
    [Description("配置当前设置可用的所有天线。其中天线码直接设置为天线的名称，需要确保天线名称与设备内描述的一致")]
    [PropertyOrder(50)]
    public Dictionary<string, object>[] AntennaTemplates
    {
        get => null;
        set
        {
            if (value != null) _antennaTemplates = Array.ConvertAll(value, item => (AntennaInfo)item);
        }
    }

    #endregion

    #region IAntennaController

    public bool IsActive { get; set; }
    public AntennaSelectionMode AntennaSelectedType { get; set; }

    public Polarization PolarityType { get; set; }

    // 设置或获取当前选择的天线编号
    public Guid AntennaId { get; set; } = Guid.Empty;
    public List<AntennaInfo> Antennas { get; set; }

    /// <summary>
    ///     EBD100设备当做天线控制使用时，使用频率设置来打通低/高端天线，已供ESMB使用。
    /// </summary>
    private double _antennaFrequency;

    double IAntennaController.Frequency
    {
        get => _antennaFrequency;
        set
        {
            _antennaFrequency = value;
            SetFreqs(_antennaFrequency);
        }
    }

    public bool SendControlCode(string code)
    {
        return true;
    }

    #endregion
}