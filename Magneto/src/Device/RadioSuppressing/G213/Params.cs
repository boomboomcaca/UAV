using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.G213;

[DeviceDescription(Name = "G213",
    Manufacturer = "博亚时代",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    FeatureType = FeatureType.PCOMS,
    MaxInstance = 1,
    Model = "G213",
    Version = "1.0.0",
    Description = "博亚时代公众移动通信终端压制子系统")]
public partial class G213
{
    private float[] _powers;
    private RftxBandsTemplateEx[] _rftxBandExs;
    private RftxBandsTemplate[] _rftxBands;
    private RftxSegmentsTemplate[] _rftxSegments;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [Name(ParameterNames.Powers)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("功放功率")]
    [Description("每个功放下发的功率大小")]
    [ValueRange(1, 47)]
    [Unit(UnitNames.DBm)]
    [Style(DisplayStyle.Slider)]
    public float[] Powers
    {
        get => null;
        set
        {
            if (value == null) return;
            _powers = value;
        }
    }

    [Parameter(Template = typeof(RftxBandsTemplate), AbilitySupport = FeatureType.PCOMS)]
    [Name(ParameterNames.RftxBands)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("管制频段")]
    [Description("表示管制通道包含的基本信息，包含适用于频段范围")]
    [ParametersDefault(
        new[]
        {
            ParameterNames.PhysicalChannelNumber,
            ParameterNames.StartFrequency,
            ParameterNames.StopFrequency,
            ParameterNames.StepFrequency,
            ParameterNames.ChannelSubBands
        }, new object[]
        {
            0, 870d, 960d, 120d, "中国电信2G/4G(CDMA/LTEFDD);870;885|" +
                                 "中国电信4G(LTEFDD);871.7;876.7|中国移动2G(GSM900);930;937|" +
                                 "中国移动4G(LTEFDD);937;949|中国联通4G(LTEFDD);949;960"
        }, new object[]
        {
            1, 1805d, 1880d, 120d, "中国移动2G(GSM1800);1805;1820|中国联通2G(GSM);1840;1850|" +
                                   "中国联通4G(LTE;FDD);1850;1860|中国电信4G(LTEFDD);1860;1875"
        }, new object[] { 2, 1880d, 2025d, 960d, "中国移动4G(TDLTE);1880;1890|中国移动3G(TDSCDMA);2010;2025" },
        new object[] { 3, 2110d, 2170d, 960d, "中国电信2G(CDMA2000);2110;2115|中国联通3G(WCDMA);2130;2145" },
        new object[]
            { 4, 2300d, 2390d, 960d, "中国联通4G(TDLTE);2300;2320|中国移动4G(TDLTE);2320;2370|中国电信4G(TDLTE);2370;2390" },
        new object[] { 5, 2515d, 2675d, 960d, "中国移动5G(5GNR);2515;2675" },
        new object[] { 6, 3400d, 3600d, 1000d, "中国电信5G(5GNR);3400;3500|中国联通5G(5GNR);3500;3600" },
        new object[] { 7, 4800d, 4900d, 1000d, "中国移动5G(5GNR);4800;4900" })]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxBands
    {
        set
        {
            if (value == null) return;
            _rftxBands = new RftxBandsTemplate[value.Length];
            _rftxBandExs = new RftxBandsTemplateEx[value.Length];
            for (var i = 0; i < value.Length; i++)
            {
                var item = value[i];
                var template = (RftxBandsTemplate)item;
                _rftxBands[i] = template;
                _rftxBandExs[i] = RftxBandsTemplateEx.ToTemplateEx(i, template);
            }
        }
    }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.PCOMS,
        Template = typeof(RftxSegmentsTemplate))]
    [Name(ParameterNames.RftxSegments)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频管控")]
    [Description("表示包含特定通道多条待压制参数信息")]
    [Style(DisplayStyle.Default)]
    public Dictionary<string, object>[] RftxSegments
    {
        get => null;
        set
        {
            if (value == null)
            {
                _rftxSegments = null;
                return;
            }

            _rftxSegments = Array.ConvertAll(value, item => (RftxSegmentsTemplate)item);
        }
    }

    #region 安装参数

    [PropertyOrder(0)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("上位机端口")]
    [Description("设置上位机通过UDP进行广播信号的网络端口,厂家限制接受数据的上位机电脑IP只能设置为192.168.1.89，不能修改！！")]
    [ValueRange(1024, 65535)]
    [DefaultValue(7900)]
    public int SlavePort { get; set; } = 7900;

    [PropertyOrder(0)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("下位机0地址")]
    [Description("设置连接压制设备的下位机0的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [DefaultValue("192.168.1.90")]
    public string SlaveIp0 { get; set; } = "192.168.1.90";

    [PropertyOrder(1)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("下位机0端口")]
    [Description("设置连接到下位机0的网络控制端口")]
    [ValueRange(1024, 65535)]
    [DefaultValue(7900)]
    public int SlavePort0 { get; set; } = 7900;

    [PropertyOrder(2)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("下位机1地址")]
    [Description("设置连接压制设备的下位机1的（IPv4）网络地址，格式：xxx.xxx.xxx.xxx")]
    [DefaultValue("192.168.1.91")]
    public string SlaveIp1 { get; set; } = "192.168.1.91";

    [PropertyOrder(3)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("下位机1端口")]
    [Description("设置连接到下位机的网络控制端口")]
    [ValueRange(1024, 65535)]
    [DefaultValue(7900)]
    public int SlavePort1 { get; set; } = 7900;

    #endregion
}