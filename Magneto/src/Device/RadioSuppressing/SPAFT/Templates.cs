using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.SPAFT;

public class RftxSegmentsTemplate
{
    [Parameter]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("设置管制信道所属通道编号")]
    [StandardValues(IsSelectOnly = false,
        DisplayValues = "|通道1|通道2|通道3|通道4|通道5|通道6",
        StandardValues = "|0|1|2|3|4|5|")]
    [ValueRange(0, 5)]
    [DefaultValue(0)]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Dropdown)]
    public int PhysicalChannelNumber { get; set; } = 0;

    [Parameter]
    [Name(ParameterNames.LogicalChannelNumber)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道号")]
    [Description("设置管制信道所属通道下的逻辑通道号，即载波编号")]
    [StandardValues(IsSelectOnly = false,
        DisplayValues = "|载波1|载波2|载波3|载波4|载波5|载波6|载波7|载波8|载波9|载波10",
        StandardValues = "|0|1|2|3|4|5|6|7|8|9")]
    [ValueRange(0, 9)]
    [DefaultValue(0)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Dropdown)]
    public int LogicalChannelNumber { get; set; } = 0;

    [Parameter]
    [Name(ParameterNames.RftxSwitch)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("射频开关")]
    [Description("设置是否使能信道发射")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|开|关",
        StandardValues = "|true|false")]
    [DefaultValue(false)]
    [PropertyOrder(2)]
    [Style(DisplayStyle.Switch)]
    public bool RftxSwitch { get; set; } = false;

    [Parameter]
    [Name(ParameterNames.RftxFrequencyMode)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("压制模式")]
    [Description("设置管制信道所属压制模式，包含：定频、跳频、扫频和梳状谱")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|定频|跳频|扫频|梳状谱",
        StandardValues = "|0|1|2|3")]
    [DefaultValue(0)]
    [PropertyOrder(3)]
    [Style(DisplayStyle.Radio)]
    public int RftxFrequencyMode { get; set; } = 0;

    [Parameter]
    [Name(ParameterNames.Modulation)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制模式")]
    [Description("设置管制信道所属压制信号的调制模式")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|CW|AM|FM|ASK|2FSK|4FSK|BPSK|QPSK|8PSK|16QAM|线性调频",
        StandardValues = "|CW|AM|FM|ASK|_2FSK|_4FSK|BPSK|QPSK|_8PSK|_16QAM|LFM")]
    [DefaultValue(Modulation.Cw)]
    [PropertyOrder(4)]
    [Style(DisplayStyle.Dropdown)]
    public Modulation Modulation { get; set; } = Modulation.Cw;

    [Parameter]
    [Name(ParameterNames.ModulationSource)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制源")]
    [Description("设置FM调制的的调制源")]
    [StandardValues(IsSelectOnly = true,
        DisplayValues = "|1kH单音|警示语音|噪声",
        StandardValues = "|0|1|2")]
    [DefaultValue(0)]
    [PropertyOrder(5)]
    [Style(DisplayStyle.Radio)]
    public int ModulationSource { get; set; } = 0;

    [Parameter]
    [Name(ParameterNames.Bandwidth)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制带宽")]
    [Description("设置管制信道信号带宽，单位：kHz")]
    [ValueRange(1.0d, 80000.0d, 0)]
    [DefaultValue(10.0d)]
    [Unit("kHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double Bandwidth { get; set; } = 100.0d;

    [Parameter]
    [Name(ParameterNames.Baudrate)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("调制速率")]
    [Description("设置管制信道信号，单位：kbps")]
    [ValueRange(2, 2000, 0)]
    [DefaultValue(2)]
    [Unit("kbps")]
    [PropertyOrder(7)]
    [Style(DisplayStyle.Input)]
    public double Baudrate { get; set; } = 200d;

    [Parameter]
    [Name(ParameterNames.HoldTime)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("驻留时间")]
    [Description("设置跳频或扫描模式下，压制信道的驻留时长，单位：μs")]
    [ValueRange(1, 10000000)]
    [StandardValues(IsSelectOnly = false,
        DisplayValues = "|10s|5s|2s|1s|500ms|200ms|100ms",
        StandardValues = "|10000000|50000000|20000000|1000000|500000|200000|100000")]
    [DefaultValue(5000000)]
    [Unit("μs")]
    [PropertyOrder(8)]
    [Style(DisplayStyle.Dropdown)]
    public float HoldTime { get; set; } = 50000f;

    [Parameter]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("中心频率")]
    [Description("设置管制信道信号中心频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(101.7d)]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 101.7d;

    [Parameter]
    [Name(ParameterNames.Frequencies)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("跳频频点")]
    [Description("设置管制信道跳频频率表，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(new[] { 101.7d, 102.6d })]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double[] Frequencies { get; set; } = { 101.7d, 102.6d };

    [Parameter]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("设置管制信道频段起始频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(88.0d)]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; } = 88.0d;

    [Parameter]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("设置管制信道频段结束频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; } = 108.0d;

    [Parameter]
    [Name(ParameterNames.StepFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("扫频步进")]
    [Description("设置管制信道频段扫频步进，单位：kHz")]
    [ValueRange(1.0d, 100000.0d, 3)]
    [DefaultValue(25.0d)]
    [Unit("kHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double StepFrequency { get; set; } = 25.0d;

    [Browsable(false)]
    public double MaxFrequency =>
        RftxFrequencyMode switch
        {
            0 => Frequency,
            1 => Frequencies.Max(),
            _ => StopFrequency
        };

    [Browsable(false)]
    public double MinFrequency =>
        RftxFrequencyMode switch
        {
            0 => Frequency,
            1 => Frequencies.Min(),
            _ => StartFrequency
        };

    [Browsable(false)]
    public double ModulationIndex =>
        Modulation switch
        {
            Modulation.Cw => 0,
            Modulation.Am => 1,
            Modulation.Fm => 2,
            Modulation.Ask => 3,
            Modulation._2FSK => 4,
            Modulation._4FSK => 5,
            Modulation.Bpsk => 6,
            Modulation.Qpsk => 7,
            Modulation._8PSK => 9,
            Modulation._16QAM => 10,
            Modulation.Dpsk => 11,
            Modulation.Lfm => 2,
            _ => 0
        };

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
            if (dict.TryGetValue(name, out var value1))
                try
                {
                    var value = Utils.GetRealValue(property.PropertyType, value1);
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
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute) continue;
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            try
            {
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
            catch
            {
                // 容错代码
            }
        }

        return dic;
    }
}

public class RftxBandsTemplate
{
    [Parameter]
    [Name(ParameterNames.PhysicalChannelNumber)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("物理通道号")]
    [Description("设置管制信道所属通道编号")]
    [StandardValues(IsSelectOnly = false,
        DisplayValues = "|通道1|通道2|通道3|通道4|通道5|通道6",
        StandardValues = "|0|1|2|3|4|5|")]
    [ValueRange(0, 5)]
    [DefaultValue(0)]
    [PropertyOrder(0)]
    [Style(DisplayStyle.Dropdown)]
    public int PhysicalChannelNumber { get; set; }

    [Parameter]
    [Name(ParameterNames.StartFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("起始频率")]
    [Description("设置管制信道频段起始频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(88.0d)]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double StartFrequency { get; set; }

    [Parameter]
    [Name(ParameterNames.StopFrequency)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("结束频率")]
    [Description("设置管制信道频段结束频率，单位：MHz")]
    [ValueRange(20.0d, 6000.0d, 6)]
    [DefaultValue(108.0d)]
    [Unit("MHz")]
    [PropertyOrder(6)]
    [Style(DisplayStyle.Input)]
    public double StopFrequency { get; set; }

    [Parameter]
    [Name(ParameterNames.LogicalChannelCount)]
    [Category(PropertyCategoryNames.Misc)]
    [DisplayName("逻辑通道数")]
    [Description("设置管制信道所属通道下的逻辑通道数量")]
    [ValueRange(8, 10)]
    [DefaultValue(8)]
    [PropertyOrder(1)]
    [Style(DisplayStyle.Slider)]
    public int LogicalChannelCount { get; set; } = 8;

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
            if (dict.TryGetValue(name, out var value1))
                try
                {
                    var value = Utils.GetRealValue(property.PropertyType, value1);
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
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is not ParameterAttribute) continue;
            var name = Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                ? property.Name
                : nameAttribute.Name;
            try
            {
                var value = property.GetValue(this);
                dic.Add(name, value);
            }
            catch
            {
                // 容错代码
            }
        }

        return dic;
    }
}