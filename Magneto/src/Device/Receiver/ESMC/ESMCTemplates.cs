using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.ESMC;

#region 离散扫描模板

internal class DiscreteFrequencyTemplate
{
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [ValueRange(0.5d, 3000.0d, 6)]
    [DefaultValue(101.7d)]
    [Description("中心频率 单位MHz")]
    [Unit(UnitNames.MHz)]
    [Style(DisplayStyle.Input)]
    public double Frequency { get; set; } = 101.7d;

    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调带宽")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|2.5|8|15|100|2000",
        DisplayValues = "|2.5 kHz|8 kHz|15 kHz|100 kHz|2 MHz")]
    [DefaultValue(100.0d)]
    [Description("中频带宽、滤波带宽、解调带宽。")]
    [Unit(UnitNames.KHz)]
    [Style(DisplayStyle.Bandwidth)]
    public double IfBandWidth
    {
        //  2.5f,8,15,100,2000
        get;
        set;
    } = 120.0d;

    [PropertyOrder(2)]
    [Name(ParameterNames.DemMode)]
    [Parameter(AbilitySupport = FeatureType.MScan | FeatureType.MScne)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("解调模式")]
    [StandardValues(IsSelectOnly = true,
        StandardValues = "|FM|AM|PULSE|LSB|USB",
        DisplayValues = "|FM|AM|PULSE|LSB|USB")]
    [DefaultValue(Modulation.Fm)]
    [Description("信号的解调模式")]
    [Style(DisplayStyle.Dropdown)]
    public Modulation DemMode { get; set; } = Modulation.Fm;
}

#endregion