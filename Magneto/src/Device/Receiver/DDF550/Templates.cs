using System;
using System.Collections.Generic;
using System.ComponentModel;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Magneto.Device.DDF550;

internal class AntennaTemplate
{
    /// <summary>
    ///     天线名称
    /// </summary>
    [PropertyOrder(0)]
    [Name("antennaName")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线名称")]
    [DefaultValue("ADD153SR")]
    [StandardValues(
        DisplayValues =
            "|HE010E|ADD011SR|ADD119|ADD050SR|ADD153SR|ADD157|ADD170|ADD070|ADD070M|ADD216|ADD253|ADD253_VAR1x|ADD078SR",
        StandardValues =
            "|HE010E|ADD011SR|ADD119|ADD050SR|ADD153SR|ADD157|ADD170|ADD070|ADD070M|ADD216|ADD253|ADD253_VAR1x|ADD078SR",
        IsSelectOnly = true
    )]
    [Description("天线名称")]
    public string AntennaName { get; set; }

    /// <summary>
    ///     起始频率 MHz
    /// </summary>
    [PropertyOrder(1)]
    [Name(ParameterNames.StartFrequency)]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("起始频率")]
    [DefaultValue(88.0d)]
    [Description("设置天线的起始频率，单位MHz")]
    public double StartFrequency { get; set; } = 88.0d;

    /// <summary>
    ///     结束频率 MHz
    /// </summary>
    [PropertyOrder(2)]
    [Name(ParameterNames.StopFrequency)]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("结束频率")]
    [DefaultValue(108.0d)]
    [Description("设置天线的结束频率，单位MHz")]
    public double StopFrequency { get; set; } = 108.0d;

    /// <summary>
    ///     罗盘名称
    /// </summary>
    [PropertyOrder(3)]
    [Name("compassName")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("罗盘名称")]
    [DefaultValue("GH150@ADD153SR")]
    [Description("设置天线上的罗盘名称")]
    public string CompassName { get; set; }

    /// <summary>
    ///     正北安装偏角
    /// </summary>
    [PropertyOrder(4)]
    [Name("northCorrection")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("安装偏角")]
    [DefaultValue(0)]
    [Description("天线安装的水平偏角")]
    public int NorthCorrection { get; set; }

    /// <summary>
    ///     滚动矫正
    /// </summary>
    [PropertyOrder(5)]
    [Name("rollCorrection")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("滚动矫正")]
    [DefaultValue(0)]
    [Description("天线安装时左/右偏移的偏移角")]
    public int RollCorrection { get; set; }

    /// <summary>
    ///     俯仰矫正
    /// </summary>
    [PropertyOrder(6)]
    [Name("pitchCorrection")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("俯仰矫正")]
    [DefaultValue(0)]
    [Description("天线安装上下偏移的偏移角")]
    public int PitchCorrection { get; set; }

    /// <summary>
    ///     VHF/UHF/SHF接线方式
    /// </summary>
    [PropertyOrder(7)]
    [Name("rfInput")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("V/U/SHF连接器组")]
    [DefaultValue("RF_INPUT_VUSHF1")]
    [StandardValues(
        DisplayValues = "|V/UHF|HF/V/U/SHF",
        StandardValues = "|RF_INPUT_VUSHF1|RF_INPUT_VUSHF2",
        IsSelectOnly = true
    )]
    [Description("VHF/UHF/SHF的接线方式")]
    public string RfInput { get; set; }

    /// <summary>
    ///     HF接线方式
    /// </summary>
    [PropertyOrder(8)]
    [Name("hfInput")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("HF连接器组")]
    [DefaultValue("HF_INPUT_HF1")]
    [StandardValues(
        DisplayValues = "|HF|HF/V/U/SHF",
        StandardValues = "|HF_INPUT_HF1|HF_INPUT_HF2",
        IsSelectOnly = true
    )]
    [Description("HF接线方式")]
    public string HfInput { get; set; }

    /// <summary>
    ///     VHF/UHF天线接口
    /// </summary>
    [PropertyOrder(9)]
    [Name("rfRxPath")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("VHF/UHF天线接口")]
    [DefaultValue("RX_PATH_DF1")]
    [StandardValues(
        DisplayValues = "|1|2|3",
        StandardValues = "|RX_PATH_DF1|RX_PATH_DF2|RX_PATH_DF3",
        IsSelectOnly = true
    )]
    [Description("VHF/UHF天线接口")]
    public string RfRxPath { get; set; }

    /// <summary>
    ///     HF天线接口
    /// </summary>
    [PropertyOrder(10)]
    [Name("hfRxPath")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("HF天线接口")]
    [DefaultValue("RX_PATH_DF1")]
    [StandardValues(
        DisplayValues = "|1|2|3",
        StandardValues = "|RX_PATH_DF1|RX_PATH_DF2|RX_PATH_DF3",
        IsSelectOnly = true
    )]
    [Description("HF天线接口")]
    public string HfRxPath { get; set; }

    /// <summary>
    ///     Antenna control signal to output via AUX (X17), Pin 28 to 35 (if AUX control mode is ANTENNA)
    /// </summary>
    [PropertyOrder(11)]
    [Name("ctrlPort")]
    [Parameter]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("天线控制信号")]
    [DefaultValue(16)]
    [Description("Antenna control signal to output via AUX (X17), Pin 28 to 35 (if AUX control mode is ANTENNA)")]
    [Browsable(false)]
    public int CtrlPort { get; set; }

    /// <summary>
    ///     是否读取GPS数据
    /// </summary>
    [PropertyOrder(12)]
    [Parameter]
    [Name("gpsRead")]
    [Category(PropertyCategoryNames.Installation)]
    [DisplayName("是否读取GPS")]
    [StandardValues(
        StandardValues = "|false|true",
        DisplayValues = "|否|是",
        IsSelectOnly = true
    )]
    [DefaultValue(false)]
    [Description("是否读取GPS")]
    public bool GpsRead { get; set; }

    /// <summary>
    ///     运算符重载，装天线配置的字典类型转换成对应的天线模板对象
    /// </summary>
    /// <param name="dict">天线配置字典表</param>
    /// <returns>类型为天线码模板类的实例</returns>
    public static explicit operator AntennaTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new AntennaTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (!dict.ContainsKey(name)) continue;
                var value = Utils.GetRealValue(property.PropertyType, dict[name]);
                property.SetValue(template, value, null);
            }
        }
        catch
        {
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