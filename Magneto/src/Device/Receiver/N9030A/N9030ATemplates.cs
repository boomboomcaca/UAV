using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9030A;

#region 离散频点模板

public class DiscreteFrequency
{
    [Parameter]
    [PropertyOrder(0)]
    [Name(ParameterNames.Frequency)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("中心频率")]
    [DefaultValue(101.7d)]
    [ValueRange(0d, 26500d)]
    [Description("中心频率，单位MHz")]
    [Unit(UnitNames.MHz)]
    public double Frequency { get; set; } = 101.7d;

    [Parameter]
    [PropertyOrder(1)]
    [Name(ParameterNames.IfBandwidth)]
    [Category(PropertyCategoryNames.Measurement)]
    [DisplayName("频谱带宽")]
    [StandardValues(
        DisplayValues =
            "|20MHz|10MHz|5MHz|2.5MHz|2MHz|1.25MHz|1MHz|800kHz|600kHz|500kHz|400kHz|300kHz|200kHz|150kHz|120kHz|100kHz|80kHz|50kHz|30kHz|25kHz|12.5kHz|8kHz|6kHz|3kHz|1kHz",
        StandardValues =
            "|20000|10000|5000|2500|2000|1250|1000|800|600|500|400|300|200|150|120|100|80|50|30|25|12.5|8|6|3|1",
        IsSelectOnly = true)]
    [DefaultValue(200d)]
    [Description("设置信号的频谱带宽。单位：kHz。")]
    [Unit(UnitNames.KHz)]
    public double IfBandWidth { get; set; } = 200d;

    [Parameter]
    [PropertyOrder(2)]
    [Name(ParameterNames.Attenuation)]
    [Category(PropertyCategoryNames.Measurement)]
    [StandardValues(DisplayValues = "|自动|5dB|10dB|15dB|20dB|30dB|40dB|50dB|60dB|70dB",
        StandardValues = "|-1|5|10|15|20|30|40|50|60|70", IsSelectOnly = true)]
    [DisplayName("衰减值")]
    [DefaultValue(-1.0f)]
    [Description("设置衰减值，单位为dB。范围为6~70。")]
    [Unit(UnitNames.Db)]
    public float Attenuation { get; set; } = -1.0f;
    //private string _preamp = "FALSE";
    //[Parameter]
    //[Category(PropertyCategoryNames.Normal)]
    //[DisplayName("前端放大器")]
    //[DefaultValue("FALSE")]
    //[Description("控制前端放大器开关。")]
    //[StandardValues("低频段|LOW;全频段|FULL;关|FALSE")]
    //public string Preamp
    //{
    //    get { return _preamp; }
    //    set { _preamp = value; }
    //}
}

#endregion