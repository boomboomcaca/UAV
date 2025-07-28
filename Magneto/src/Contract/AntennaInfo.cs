using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract;

/// <summary>
///     天线信息
/// </summary>
[MessagePackObject]
public class AntennaInfo
{
    /// <summary>
    ///     天线编号
    /// </summary>
    [Key("id")]
    [JsonProperty("id")]
    [Parameter(IsInstallation = true)]
    [Name("id")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("编号")]
    [Description("天线编号")]
    [PropertyOrder(0)]
    public Guid Id { get; set; } = Guid.Empty;

    /// <summary>
    ///     天线名称
    /// </summary>
    [Key("displayName")]
    [JsonProperty("displayName")]
    [Parameter(IsInstallation = true)]
    [Name("displayName")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("名称")]
    [Description("设置天线名称")]
    [PropertyOrder(1)]
    [DefaultValue("天线一")]
    public string Name { get; set; } = "天线一";

    [Key("model")]
    [JsonProperty("model")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<AntennaModel>))]
    [Parameter(IsInstallation = true)]
    [Name("model")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("型号")]
    [Description("选择天线型号")]
    [StandardValues(
        StandardValues =
            "|Default|DH8911|MA800F|MA802P|TS2021|ADD195_071|HE309|HF902V|HF902H|HE314A1|HF214|HF907OM|ADD071|ADD075|ADD119|ADD157_H|ADD157_V|ADD175|ADD195|ADD196|ADD197_H|ADD197_V|ADD295|HE010|HE016H|HE016V|HE500|HE600|HK014|HK033|HK309|HL033|HL040",
        DisplayValues =
            "|Default|DH8911|MA800F|MA802P|TS2021|ADD195_071|HE309|HF902V|HF902H|HE314A1|HF214|HF907OM|ADD071|ADD075|ADD119|ADD157_H|ADD157_V|ADD175|ADD195|ADD196|ADD197_H|ADD197_V|ADD295|HE010|HE016H|HE016V|HE500|HE600|HK014|HK033|HK309|HL033|HL040"
    )]
    [PropertyOrder(2)]
    [DefaultValue(AntennaModel.Default)]
    public AntennaModel Model { get; set; } = AntennaModel.Default;

    [Key("type")]
    [JsonProperty("type")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<AntennaType>))]
    [Parameter(IsInstallation = true)]
    [Name("type")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("类型")]
    [Description("设备天线类型，监测或测向")]
    [StandardValues(
        StandardValues = "|Monitoring|DirectionFinding",
        DisplayValues = "|监测|测向"
    )]
    [PropertyOrder(3)]
    [DefaultValue(AntennaType.Monitoring)]
    public AntennaType AntennaType { get; set; } = AntennaType.Monitoring;

    [Key("startFrequency")]
    [JsonProperty("startFrequency")]
    [Parameter(IsInstallation = true)]
    [Name("startFrequency")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("起始频率")]
    [Description("设置天线监测测向频率下限")]
    [Unit("MHz")]
    [ValueRange(0, 50000)]
    [PropertyOrder(4)]
    [DefaultValue(20)]
    public double StartFrequency { get; set; } = 20d;

    [Key("stopFrequency")]
    [JsonProperty("stopFrequency")]
    [Parameter(IsInstallation = true)]
    [Name("stopFrequency")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("结束频率")]
    [Description("设置天线监测或测向频率上限")]
    [Unit("MHz")]
    [ValueRange(0, 50000)]
    [PropertyOrder(5)]
    [DefaultValue(8000)]
    public double StopFrequency { get; set; } = 8000d;

    [Key("polarization")]
    [JsonProperty("polarization")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<Polarization>))]
    [Parameter(IsInstallation = true)]
    [Name("polarization")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("极化方式")]
    [Description("设置天线的极化方式")]
    [StandardValues(
        StandardValues = "|Vertical|Horizontal",
        DisplayValues = "|垂直极化|水平极化"
    )]
    [PropertyOrder(6)]
    [DefaultValue(Polarization.Vertical)]
    public Polarization Polarization { get; set; } = Polarization.Vertical;

    [Key("isActive")]
    [JsonProperty("isActive")]
    [Parameter(IsInstallation = true)]
    [Name("isActive")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("有源天线")]
    [Description("当前天线为有源天线或无源天线")]
    [StandardValues(
        StandardValues = "|1|2|3",
        DisplayValues = "|无源|有源|同时存在"
    )]
    [PropertyOrder(5)]
    [DefaultValue(1)]
    public int IsActive { get; set; } = 1;

    [Key("height")]
    [JsonProperty("height")]
    [Parameter(IsInstallation = true)]
    [Name("height")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("高度")]
    [Unit("m")]
    [Description("设置天线的高度")]
    [PropertyOrder(7)]
    [DefaultValue(0f)]
    public float Height { get; set; }

    [Key("gain")]
    [JsonProperty("gain")]
    [Parameter(IsInstallation = true)]
    [Name("gain")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("增益")]
    [Description("设置天线增益")]
    [Unit("dB")]
    [DefaultValue(0)]
    [ValueRange(-50, 50)]
    [PropertyOrder(8)]
    public int Gain { get; set; }

    [Key("passiveCode")]
    [JsonProperty("passiveCode")]
    [Parameter(IsInstallation = true)]
    [Name("passiveCode")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("无源控制码")]
    [Description("设置天线开关打通码，以“|”分隔，如:0x01|0x02|0x03")]
    [PropertyOrder(9)]
    [DefaultValue("0x01")]
    public string PassiveCode { get; set; } = "0x01";

    [Key("activeCode")]
    [JsonProperty("activeCode")]
    [Parameter(IsInstallation = true)]
    [Name("activeCode")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("有源控制码")]
    [Description("设置天线开关打通码，以“|”分隔，如:0x01|0x02|0x03")]
    [PropertyOrder(9)]
    [DefaultValue("0x01")]
    public string ActiveCode { get; set; } = "0x01";

    // [Key("factors")]
    // [JsonProperty("factors")]
    // [Parameter(IsInstallation = true)]
    // [Name("factors")]
    // [Category(PropertyCategoryNames.Installation)]
    // [DisplayName("天线因子")]
    // [Description("设置天线因子数据")]
    // [PropertyOrder(10)]
    // [DefaultValue(new double[] { 101.7, 10, 102.6, 20, 110, 30, 120, 10, 130, 40 })]
    // public double[] Factors { get; set; } = new double[] { 101.7, 10, 102.6, 20, 110, 30, 120, 10, 130, 40 };
    [Key("tag")]
    [JsonProperty("tag")]
    [Parameter(IsInstallation = true)]
    [Name("tag")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("附属值")]
    [Description("设置天线附属值")]
    [PropertyOrder(11)]
    [DefaultValue("")]
    public string Tag { get; set; } = "";

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        foreach (var property in GetType().GetProperties())
            if (Attribute.GetCustomAttribute(property, typeof(ParameterAttribute)) is ParameterAttribute)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }

        return dic;
    }

    public static explicit operator AntennaInfo(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var json = JsonConvert.SerializeObject(dict);
        var template = JsonConvert.DeserializeObject<AntennaInfo>(json);
        return template;
    }

    #region 辅助方法

    /// <summary>
    ///     转换为平台需要的因子数据
    /// </summary>
    /// <param name="isAcive">是否为有源天线</param>
    public SDataFactor GetFactor(bool isAcive)
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathAntfactor);
        var path = Path.Combine(dir, $"{Model}.json");
        List<FrequencyFactorPair> factors = null;
        if (File.Exists(path))
            try
            {
                var json = File.ReadAllText(path);
                var cache = JsonConvert.DeserializeObject<FactorCache>(json);
                if (isAcive)
                    factors = cache.Active;
                else
                    factors = cache.Passive;
            }
            catch
            {
                factors = null;
            }

        if (factors == null || factors.Count == 0)
            // 如果不存在，则因子全为0
            factors =
            [
                new()
                {
                    Frequency = StartFrequency,
                    Factor = 0
                },

                new()
                {
                    Frequency = StopFrequency,
                    Factor = 0
                }
            ];
        var step = (long)(factors[0].Frequency * 1000000);
        foreach (var factor in factors) step = CalculateGcd((long)(factor.Frequency * 1000000), step);
        var start = (long)(factors[0].Frequency * 1000000);
        var stop = (long)(factors[^1].Frequency * 1000000);
        var index = 0;
        var previousValue = factors[0];
        var currentValue = factors[0];
        var data = new short[(stop - start) / step + 1];
        for (long i = 0; i < data.Length; ++i)
        {
            var frequency = start + i * step;
            if (frequency > (long)(currentValue.Frequency * 1000000))
            {
                previousValue = currentValue;
                if (++index < factors.Count) currentValue = factors[index];
            }

            if (frequency <= (long)(previousValue.Frequency * 1000000))
            {
                data[i] = (short)(previousValue.Factor * 10); // Math.Round(previousValue.Factor, 1);
            }
            else
            {
                var fa = previousValue.Factor + (currentValue.Factor - previousValue.Factor) *
                    (frequency - (long)(previousValue.Frequency * 1000000)) /
                    ((long)(currentValue.Frequency * 1000000) - (long)(previousValue.Frequency * 1000000));
                data[i] = (short)(fa * 10); // Math.Round(fa, 1);
            }
        }

        return new SDataFactor
        {
            // 起始频率/结束频率，单位：MHz
            // 频率步进，单位：kHz
            StartFrequency = factors[0].Frequency,
            StopFrequency = factors[^1].Frequency,
            StepFrequency = step / 1000.0d,
            Data = data,
            Total = data.Length
        };
    }

    /// <summary>
    ///     计算两个数的最大公约数
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private static long CalculateGcd(long x, long y)
    {
        if (y == 0) throw new DivideByZeroException();
        var mod = x % y;
        while (mod != 0)
        {
            x = y;
            y = mod;
            mod = x % y;
        }

        return y;
    }

    private struct FactorCache
    {
        [JsonProperty("passive")] public List<FrequencyFactorPair> Passive { get; set; }
        [JsonProperty("active")] public List<FrequencyFactorPair> Active { get; set; }
    }

    private struct FrequencyFactorPair
    {
        /// <summary>
        ///     天线频率
        /// </summary>
        [JsonProperty("frequency")]
        public double Frequency { get; set; }

        /// <summary>
        ///     因子数据
        /// </summary>
        [JsonProperty("factor")]
        public double Factor { get; set; }
    }

    #endregion
}