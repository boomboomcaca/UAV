using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.YHX_HTCP;

[DeviceDescription(
    Name = "YHX-HTCP",
    Manufacturer = "成都阿莱夫信息技术有限公司",
    DeviceCategory = ModuleCategory.RadioSuppressing,
    Model = "YHX-HTCP",
    Version = "1.0.0",
    MaxInstance = 1,
    FeatureType = FeatureType.UAVS | FeatureType.PCOMS | FeatureType.SATELS,
    Description = "北京泰华YHX-HTCP系列管制设备")]
public partial class YhxHtcp
{
    private float[] _powers;
    private RftxBandsTemplateEx[] _rftxBandExs;
    private RftxBandsTemplate[] _rftxBands;
    private RftxSegmentsTemplate[] _rftxSegments;
    private string _solidText = string.Empty;

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS,
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

    [PropertyOrder(3)]
    [Parameter(Template = typeof(RftxBandsTemplate),
        AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS)]
    [Name(ParameterNames.RftxBands)]
    [Category(PropertyCategoryNames.Configuration)]
    [DisplayName("管制频段")]
    [Description("表示管制通道包含的基本信息")]
    [Style(DisplayStyle.Default)]
    [ParametersDefault(
        new[]
        {
            ParameterNames.PhysicalChannelNumber,
            ParameterNames.StartFrequency,
            ParameterNames.StopFrequency,
            ParameterNames.LogicalChannelCount,
            ParameterNames.ChannelSubBands,
            ParameterNames.ChannelMaxPower
        }, new object[] { 0, 100d, 500d, 1, "1;100;300|2;300;500", 200 },
        new object[] { 1, 500d, 1000d, 1, "1;500;750|2;750;1000", 200 },
        new object[] { 2, 1000d, 1700d, 1, "1;1000;1250|2;1250;1500|3;1500;1700", 100 })]
    public Dictionary<string, object>[] RftxBands
    {
        get => null;
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
    [Parameter(AbilitySupport = FeatureType.SATELS | FeatureType.UAVS | FeatureType.PCOMS)]
    [Name(ParameterNames.Powers)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("功放功率")]
    [Description("每个功放下发的功率大小，第三段（1000MHz-1700MHz）的发射功率范围为5W-100W，其余段为5W-200W。")]
    [ValueRange(5, 200)]
    [Unit(UnitNames.Watt)]
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

    [Parameter(AbilitySupport = FeatureType.PCOMS)]
    [DisplayName("设备初始化时间")]
    [Description("该参数表示公众移动通信频段管制设备需要多少时间才能初始化成功，在此期间避免客户端频繁操作。")]
    [DefaultValue(3.0f)]
    [Browsable(false)]
    public float InitTime { get; set; } = 3.0f;

    [PropertyOrder(2)]
    [Parameter(AbilitySupport = FeatureType.UAVS | FeatureType.SATELS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("是否固化文本")]
    [Description("是：将设置好的文本固化到设备里；否：加载上一次固化过的文本；文本大小最大支持2KB。")]
    [StandardValues("|true|false", "|是|否")]
    [DefaultValue(false)]
    public bool IsSolidText { get; set; }

    [PropertyOrder(3)]
    [Parameter(AbilitySupport = FeatureType.UAVS | FeatureType.SATELS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("固化文本")]
    [Description("将要固化的文本固化到设备里面。【先将文本固化，再进行数字调制发射，目前所列的数字调制方式均支持】。")]
    [Browsable(false)]
    [DefaultValue("非法信号，请勿轻信！")]
    public string SolidText
    {
        get => _solidText;
        set
        {
            if (IsSolidText)
                if (!_solidText.Equals(value))
                {
                    _solidText = value;
                    WriteText();
                    Thread.Sleep(2000);
                    DeviceSelfCheck();
                }
        }
    }

    #region 安装参数

    [PropertyOrder(0)]
    [Name(ParameterNames.IpAddress)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("设备IP")]
    [Description("设置设备的网络地址。【高危设置，谨慎修改！设备没有Reset功能，如果忘记请使用端口地址扫描工具寻找】。")]
    [DefaultValue("192.168.8.8")]
    public string Ip { get; set; } = "192.168.8.8";

    [PropertyOrder(1)]
    [Name(ParameterNames.Port)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("端口号")]
    [Description("设置设备的端口号。【高危设置，谨慎修改！设备没有Reset功能，如果忘记请使用端口地址扫描工具寻找】")]
    [DefaultValue(30000)]
    public int Port { get; set; } = 30000;

    private int _sleepTime = 5;

    [PropertyOrder(2)]
    [Name("sleepTime")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("休眠时间")]
    [Description("指定设备多少时间不工作以后直接进入休眠状态，此时可以正常控制设备，只是无法正常执行发射任务。单位：分钟")]
    [ValueRange(2, 30)]
    [DefaultValue(5)]
    public int SleepTime
    {
        get => _sleepTime;
        set
        {
            try
            {
                var temp = value.ToString();
                var index = temp.IndexOf(".", StringComparison.Ordinal);
                if (index > 0) temp = temp.Substring(0, index);
                _sleepTime = int.Parse(temp);
            }
            catch
            {
                _sleepTime = 5;
            }
        }
    }

    [PropertyOrder(3)]
    [Name("isY8")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("是否是Y8设备")]
    [Description("是否是Y8公众移动通信频段管制设备，此设备比较特殊。")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(5)]
    public bool IsY8 { get; set; } = false;

    [PropertyOrder(3)]
    [Name("isRadio")]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("是否是广播段压制设备")]
    [Description("是否是广播段压制设备。")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|是|否",
        StandardValues = "|true|false")]
    [DefaultValue(5)]
    public bool IsRadio { get; set; } = false;

    #endregion
}