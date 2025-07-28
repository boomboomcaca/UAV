using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.YHX_HTCP;

public class RftxSegmentsTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。")]
    [ValueRange(0, 2)]
    [DefaultValue(0)]
    [Style(DisplayStyle.Slider)]
    public int PhysicalChannelNumber { get; set; }

    [PropertyOrder(1)]
    [Name(ParameterNames.LogicalChannelNumber)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道号")]
    [Description("表示当前管制使用的逻辑通道编号")]
    [ValueRange(1, 1)]
    [DefaultValue(1)]
    [ReadOnly(true)]
    [Style(DisplayStyle.Slider)]
    public int LogicalChannelNumber { get; set; } = 1;

    [PropertyOrder(2)]
    [Name(ParameterNames.RftxSwitch)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频开关")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|True|False",
        DisplayValues = "|开|关"
    )]
    [Description("表示当前压制使能")]
    [DefaultValue(false)]
    [Style(DisplayStyle.Switch)]
    public bool RftxSwitch { get; set; }

    [PropertyOrder(3)]
    [Name(ParameterNames.RftxFrequencyMode)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("压制模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|0|1|2|3|4",
        DisplayValues = "|定频|跳频|扫频|梳状谱|多音"
    )]
    [Description("表示频率压制模式，分为：0 - 定频，1 - 跳频， 2 - 扫频， 3 - 梳状谱， 4 - 多音")]
    [DefaultValue(0)]
    [Style(DisplayStyle.Radio)]
    public int RftxFrequencyMode { get; set; }

    [PropertyOrder(4)]
    [Name(ParameterNames.Modulation)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|AM|FM|CW|BPSK|QPSK|DPSK|ASK|_2FSK|_4FSK|GMSK",
        DisplayValues = "|AM|FM|CW|BPSK|QPSK|DPSK|ASK|2FSK|4FSK|GMSK"
    )]
    [Description("表示当前压制使用的调制方式，如CW, QPSK")]
    [DefaultValue(Modulation.Cw)]
    [Style(DisplayStyle.Radio)]
    public Modulation Modulation { get; set; } = Modulation.Cw;

    [PropertyOrder(4)]
    [Name("modulation1")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制参数1")]
    [Description("同步和跳频发射时采用该参数进行单独的频点的调制参数1设置。\r\n调制参数1（模拟带宽、AM调制度，2FSK/4FSK/ASK/BPSK/QPSK/DPSK/GMSK符号率）")]
    [DefaultValue(30.0d)]
    public double Modulation1 { get; set; } = 30.0d;

    [PropertyOrder(4)]
    [Name("modulation2")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制参数2")]
    [Description("同步和跳频发射时采用该参数进行单独的频点的调制参数2设置。\r\n调制参数2（2FSK频偏、GSMK带宽、4FSK频偏1）")]
    [DefaultValue(0.03d)]
    [ValueRange(0.001d, 10.0d)]
    public double Modulation2 { get; set; } = 0.03d;

    [PropertyOrder(4)]
    [Name("modulation3")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制参数3")]
    [Description("同步和跳频发射时采用该参数进行单独的频点的调制参数3设置。\r\n调制参数3：4字节（4FSK频偏2）")]
    [DefaultValue(0.06d)]
    [ValueRange(0.001d, 10.0d)]
    public double Modulation3 { get; set; } = 0.06d;

    [PropertyOrder(5)]
    [Name(ParameterNames.Bandwidth)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制带宽")]
    [Description("设置管制信道信号带宽，单位：kHz")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2000|5000|10000|20000",
        DisplayValues = "|2MHz|5MHz|10MHz|20MHz"
    )]
    [DefaultValue(2000d)]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Radio)]
    public double Bandwidth { get; set; } = 2000d;

    [PropertyOrder(6)]
    [Name(ParameterNames.HoldTime)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("驻留时间")]
    [Description("表示跳频或扫描模式下，待压制信号的的发射时长，单位：μs")]
    [ValueRange(100f, 1_000_000f)]
    [DefaultValue(100f)]
    [Unit(UnitNames.Us)]
    [Style(DisplayStyle.Slider)]
    public float HoldTime { get; set; } = 100f;

    [PropertyOrder(7)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("中心频率")]
    [Description("表示定频压制模式下的发射信号中心频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [DefaultValue(100d)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 100d;

    [PropertyOrder(7)]
    [Name(ParameterNames.Frequencies)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("跳频频点")]
    [Description("表示跳频压制模式下的离散频表，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double[] Frequencies { get; set; }

    [PropertyOrder(7)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(8)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(9)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫频步进")]
    [Description("表示扫频压制模式下的步进，单位：kHz")]
    [ValueRange(1d, 100d, 3)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(25d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 25d;

    [PropertyOrder(10)]
    [Name("isLoadAudio")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("是否加载音频")]
    [Description("是否加载音频文件。是：加载选择的音频序号对应的音频文件；否：不加载音频文件，接收端没有音频输出。")]
    [DefaultValue(false)]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|true|false",
        DisplayValues = "|是|否")]
    [Style(DisplayStyle.Switch)]
    public bool IsLoadAudio { get; set; }

    [PropertyOrder(11)]
    [Name("audioNo")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("音频序号")]
    [Description("要加载的音频对应的序号值，选择的音频将自动被调制到AM，FM对应的频点上发射出去。如果不设置该值，则值默认为1。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|38|39|40|41|42|43|44|45|46|47|48|49|50|51|52|53|54|55|56|57|58|59|60|61|62|63|64|65|66|67|68|69|70|71|72|73|74|75|76|77|78|79|80|81|82|83|84|85|86|87|88|89|90|91|92|93|94|95|96|97|98|99|100",
        DisplayValues =
            "|1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|38|39|40|41|42|43|44|45|46|47|48|49|50|51|52|53|54|55|56|57|58|59|60|61|62|63|64|65|66|67|68|69|70|71|72|73|74|75|76|77|78|79|80|81|82|83|84|85|86|87|88|89|90|91|92|93|94|95|96|97|98|99|100"
    )]
    [DefaultValue(1)]
    [Style(DisplayStyle.Dropdown)]
    public int AudioNo { get; set; } = 1;

    [PropertyOrder(12)]
    [Name("subToneType")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("亚音类型")]
    [Description("同步发射时对对讲机压制所选择的亚音类型。包括无亚音、模拟亚音、数字亚音正序和数字亚音反序。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|E|A|I|N",
        DisplayValues = "|无亚音|模拟亚音|数字亚音正序|数字亚音反序")]
    [DefaultValue(Satp.E)]
    [Style(DisplayStyle.Radio)]
    public Satp SubToneType { get; set; } = Satp.E;

    [PropertyOrder(13)]
    [Name("subToneNo")]
    [Parameter(AbilitySupport = FeatureType.UAVS
                                | FeatureType.SATELS
                                | FeatureType.PCOMS
                                | FeatureType.FBANDS)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("亚音频率")]
    [Description("同步发射时对对讲机压制需要设置的亚音，模拟亚音对应相应的频率值，数字亚音正序和反序则对应一个八进制序列，数字亚音正序和反序需要用户自行确定。")]
    [StandardValues(IsSelectOnly = true,
        StandardValues =
            "|1|2|3|4|5|6|7|8|9|10|11|12|13|14|15|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|38|39|40|41|42|43|44|45|46|47|48|49|50",
        DisplayValues =
            "|67Hz|69.3Hz|71.9Hz|74.4Hz|77.0Hz|79.7Hz|82.5Hz|85.4Hz|88.5Hz|91.5Hz|94.8Hz|97.4Hz|100.0Hz|103.5Hz|107.2Hz|110.9Hz|114.8Hz|118.8Hz|123Hz|127.3Hz|131.8Hz|136.5Hz|141.3Hz|146.2Hz|151.4Hz|156.7Hz|159.8Hz|162.2Hz|165.5Hz|167.9Hz|171.3Hz|173.8Hz|177.3Hz|179.9Hz|183.5Hz|186.2Hz|189.9Hz|192.8Hz|196.6Hz|199.5Hz|203.5Hz|206.5Hz|210.7Hz|218.1Hz|225.7Hz|229.1Hz|233.6Hz|241.8Hz|250.3Hz|254.1Hz")]
    [DefaultValue(1)]
    public int SubToneNo { get; set; }

    public static explicit operator RftxSegmentsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxSegmentsTemplate();
        var type = template.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            if (!dict.ContainsKey(name)) continue;
            try
            {
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
            catch
            {
            }
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

    public RftxSegmentsTemplate Clone()
    {
        return new RftxSegmentsTemplate
        {
            AudioNo = AudioNo,
            IsLoadAudio = IsLoadAudio,
            Bandwidth = Bandwidth,
            Frequencies = Frequencies,
            Frequency = Frequency,
            HoldTime = HoldTime,
            LogicalChannelNumber = LogicalChannelNumber,
            Modulation = Modulation,
            Modulation1 = Modulation1,
            Modulation2 = Modulation2,
            Modulation3 = Modulation3,
            PhysicalChannelNumber = PhysicalChannelNumber,
            RftxFrequencyMode = RftxFrequencyMode,
            RftxSwitch = RftxSwitch,
            StartFrequency = StartFrequency,
            StepFrequency = StepFrequency,
            StopFrequency = StopFrequency,
            SubToneNo = SubToneNo,
            SubToneType = SubToneType
        };
    }
}

public class RftxBandsTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。")]
    [ValueRange(1, 9)]
    [DefaultValue(1)]
    [Style(DisplayStyle.Slider)]
    public int PhysicalChannelNumber { get; set; } = 1;

    [PropertyOrder(2)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("表示扫频压制模式下的起始频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(88d)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88d;

    [PropertyOrder(3)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("表示扫频压制模式下的结束频率，单位：MHz")]
    [ValueRange(20d, 26500d, 6)]
    [Unit(UnitNames.MHz)]
    [DefaultValue(108d)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108d;

    [PropertyOrder(4)]
    [Name(ParameterNames.StepFrequency)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("梳状谱频率间隔")]
    [Description("表示梳状谱模式下的频率间隔，单位：kHz")]
    [ValueRange(10d, 10000d)]
    [Unit(UnitNames.KHz)]
    [DefaultValue(120d)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 120d;

    [PropertyOrder(5)]
    [Name(ParameterNames.LogicalChannelCount)]
    [Parameter(IsInstallation = true)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道数")]
    [Description("配置压制机对应物理通道可用的逻辑通道数量")]
    [ValueRange(1, 1)]
    [DefaultValue(1)]
    [ReadOnly(true)]
    [Style(DisplayStyle.Slider)]
    public int LogicalChannelCount { get; set; } = 1;

    [Parameter]
    [Name(ParameterNames.ChannelSubBands)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("通道子频段信息")]
    [Description(
        "通道子频段信息，为空使用整个通道信息，格式：名称1;起始频率1(MHz);结束频率1(MHz)|名称2;起始频率2(MHz);结束频率2(MHz)，eg：中国电信2G/4G(CDMA/LTEFDD);870;885|中国移动4G(LTEFDD);937;949")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public string ChannelSubBands { get; set; }

    [Parameter]
    [Name(ParameterNames.ChannelMaxPower)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("频段最大功率")]
    [Description("频段最大功率")]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Input)]
    public double ChannelMaxPower { get; set; }

    public static explicit operator RftxBandsTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new RftxBandsTemplate();
        var type = template.GetType();
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            if (!dict.ContainsKey(name)) continue;
            try
            {
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
            catch
            {
            }
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