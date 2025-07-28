// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Parameter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class Parameter.
/// </summary>
[MessagePackObject]
public class Parameter
{
    /// <summary>
    ///     The default
    /// </summary>
    [IgnoreMember] private object _default;

    /// <summary>
    ///     The value
    /// </summary>
    [IgnoreMember] private object _value;

    /// <summary>
    ///     参数名
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; } = "";

    /// <summary>
    ///     参数支持的功能
    /// </summary>
    /// <value>The feature.</value>
    [Key("supportedFeatures")]
    //[MessagePackFormatter(typeof(MessagePackEnumFormatter<FeatureType>))]
    [JsonProperty("supportedFeatures", Required = Required.Always)]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<FeatureType>))]
    public FeatureType Feature { get; set; } = FeatureType.None;

    /// <summary>
    ///     显示名称
    /// </summary>
    /// <value>The display name.</value>
    [Key("displayName")]
    [JsonProperty("displayName", Required = Required.Always)]
    public string DisplayName { get; set; } = "";

    /// <summary>
    ///     参数描述
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    [JsonProperty("description", Required = Required.Default)]
    public string Description { get; set; } = "";

    /// <summary>
    ///     单位
    /// </summary>
    /// <value>The unit.</value>
    [Key("unit")]
    [JsonProperty("unit")]
    public string Unit { get; set; } = "";

    /// <summary>
    ///     参数分组
    /// </summary>
    /// <value>The category.</value>
    [Key("category")]
    [JsonProperty("category", Required = Required.Default)]
    public string Category { get; set; } = "常规设置";

    /// <summary>
    ///     参数的显示样式
    /// </summary>
    /// <value>The style.</value>
    [Key("style")]
    [JsonProperty("style", Required = Required.Default)]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<DisplayStyle>))]
    //[MessagePackFormatter(typeof(MessagePackEnumFormatter<DataType>))]
    public DisplayStyle Style { get; set; } = DisplayStyle.Default;

    /// <summary>
    ///     参数的数据格式
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    [JsonProperty("type", Required = Required.Default)]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<ParameterDataType>))]
    //[MessagePackFormatter(typeof(MessagePackEnumFormatter<DataType>))]
    public ParameterDataType Type { get; set; } = ParameterDataType.String;

    /// <summary>
    ///     关联参数集合
    /// </summary>
    /// <value>The children.</value>
    [Key("children")]
    [JsonProperty("children", Required = Required.Default)]
    public List<string> Children { get; set; } = new();

    /// <summary>
    ///     开/关关联参数的值的特性
    ///     当参数的值在此集合中时，打开显示关联参数，否则隐藏
    /// </summary>
    /// <value>The related value.</value>
    [Key("relatedValue")]
    [JsonProperty("relatedValue", Required = Required.Default)]
    public List<object> RelatedValue { get; set; } = new();

    /// <summary>
    ///     默认值
    /// </summary>
    /// <value>The default.</value>
    [Key("default")]
    [JsonProperty("default", Required = Required.Default)]
    public object Default
    {
        get => _default;
        set => _default = SetValue(value);
    }

    /// <summary>
    ///     最小值
    /// </summary>
    /// <value>The minimum.</value>
    [Key("minimum")]
    [JsonProperty("minimum", Required = Required.AllowNull)]
    public object Minimum { get; set; }

    /// <summary>
    ///     最大值
    /// </summary>
    /// <value>The maximum.</value>
    [Key("maximum")]
    [JsonProperty("maximum", Required = Required.AllowNull)]
    public object Maximum { get; set; }

    /// <summary>
    ///     步进
    /// </summary>
    /// <value>The step.</value>
    [Key("step")]
    [JsonProperty("step", Required = Required.Default)]
    public object Step { get; set; }

    /// <summary>
    ///     可选的值集合
    /// </summary>
    /// <value>The values.</value>
    [Key("values")]
    [JsonProperty("values", Required = Required.AllowNull)]
    public List<object> Values { get; set; } = new();

    /// <summary>
    ///     可选的值集合的显示值
    /// </summary>
    /// <value>The display values.</value>
    [Key("displayValues")]
    [JsonProperty("displayValues", Required = Required.AllowNull)]
    public List<string> DisplayValues { get; set; } = new();

    /// <summary>
    ///     是否显示参数
    ///     true-显示
    ///     false-隐藏
    /// </summary>
    /// <value><c>true</c> if browsable; otherwise, <c>false</c>.</value>
    [Key("browsable")]
    [JsonProperty("browsable", Required = Required.Default)]
    public bool Browsable { get; set; } = true;

    /// <summary>
    ///     是否为只能选择
    ///     true-只能选择，不能输入
    ///     false-可以输入
    /// </summary>
    /// <value><c>true</c> if [select only]; otherwise, <c>false</c>.</value>
    [Key("selectOnly")]
    [JsonProperty("selectOnly", Required = Required.Default)]
    public bool SelectOnly { get; set; }

    /// <summary>
    ///     是否只读
    ///     true-只读
    ///     false-可编辑
    /// </summary>
    /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
    [Key("readonly")]
    [JsonProperty("readonly", Required = Required.Default)]
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     是否安装参数
    /// </summary>
    /// <value><c>true</c> if this instance is installation; otherwise, <c>false</c>.</value>
    [Key("isInstallation")]
    [JsonProperty("isInstallation")]
    public bool IsInstallation { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is required.
    /// </summary>
    /// <value><c>true</c> if this instance is required; otherwise, <c>false</c>.</value>
    [Key("required")]
    [JsonProperty("required")]
    public bool IsRequired { get; set; }

    /// <summary>
    ///     是否是主设备
    ///     TODO : 以后是否会出现多个主设备的情况？
    /// </summary>
    /// <value><c>true</c> if this instance is primary device; otherwise, <c>false</c>.</value>
    [Key("isPrimaryDevice")]
    [JsonProperty("isPrimaryDevice")]
    public bool IsPrimaryDevice { get; set; }

    /// <summary>
    ///     配置的模块所支持的模块类别
    /// </summary>
    /// <value>The need module category.</value>
    [Key("needModuleCategory")]
    [JsonProperty("needModuleCategory")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleCategory>))]
    public ModuleCategory NeedModuleCategory { get; set; }

    /// <summary>
    ///     配置的模块所支持的功能
    /// </summary>
    /// <value>The need feature.</value>
    [Key("needFeature")]
    [JsonProperty("needFeature")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<FeatureType>))]
    public FeatureType NeedFeature { get; set; }

    /// <summary>
    ///     适用于运行参数，标识参数的拥有者ID（模块ID）集合，一个参数在驱动层面可能同属于不同的设备模块
    /// </summary>
    /// <value>The owners.</value>
    [Key("owners")]
    [JsonProperty("owners")]
    public List<string> Owners { get; set; }

    /// <summary>
    ///     子参数集合
    /// </summary>
    /// <value>The parameters.</value>
    [Key("parameters")]
    [MessagePackFormatter(typeof(MessagePackParameterListFormatter<List<object>>))]
    [JsonProperty("parameters", Required = Required.AllowNull)]
    [JsonConverter(typeof(JsonParameterListConverter<List<object>>))]
    //public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    public List<object> Parameters { get; set; } = new(); //Object:可以是Parameter也可以是List<Parameter>

    /// <summary>
    ///     序号
    /// </summary>
    /// <value>The order.</value>
    [Key("order")]
    [JsonProperty("order", Required = Required.Default)]
    public int Order { get; set; }

    /// <summary>
    ///     子参数模板，如果有这个模板，则子参数集合从模板生成
    /// </summary>
    /// <value>The template.</value>
    [Key("template")]
    [JsonProperty("template")]
    [JsonConverter(typeof(JsonParameterTemplateConverter<List<Parameter>>))]
    public List<Parameter> Template { get; set; }

    /// <summary>
    ///     参数当前值
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    [JsonProperty("value", Required = Required.Default)]
    public object Value
    {
        get => _value;
        set => _value = SetValue(value);
    }

    /// <summary>
    ///     是否是主要参数
    /// </summary>
    /// <value><c>true</c> if this instance is major parameter; otherwise, <c>false</c>.</value>
    [JsonIgnore]
    [IgnoreMember]
    public bool IsMajorParameter =>
        Name is ParameterNames.Frequency or ParameterNames.StartFrequency or ParameterNames.StopFrequency
            or ParameterNames.DfBandwidth or ParameterNames.IfBandwidth or ParameterNames.StepFrequency
            or ParameterNames.FilterBandwidth or ParameterNames.MscanPoints or ParameterNames.ScanSegments
            or ParameterNames.DdcChannels;

    /// <summary>
    ///     Clones this instance.
    /// </summary>
    /// <returns>Parameter.</returns>
    public Parameter Clone()
    {
        return MemberwiseClone() as Parameter;
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"Parameter:{Name},{DisplayName},{Value}";
    }

    /// <summary>
    ///     转换为简单参数
    /// </summary>
    /// <returns>SimpleParameter.</returns>
    public SimpleParameter ToSimple()
    {
        var simple = new SimpleParameter
        {
            Name = Name,
            Value = Value,
            Unit = Unit
        };
        if (Parameters == null || Parameters.Count == 0 || !Parameters.Any(p => p is Parameter)) return simple;
        simple.Parameters = Parameters.Where(p => p is Parameter).Select(p => (object)(p as Parameter)?.ToSimple())
            .ToList();
        return simple;
    }

    /// <summary>
    ///     Sets the value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>System.Object.</returns>
    private object SetValue(object value)
    {
        if (value is JArray ja) return ja.Select(item => item.ToObject(typeof(object))).ToArray();
        return value;
    }
}

/// <summary>
///     Class SimpleParameter.
/// </summary>
[MessagePackObject]
public class SimpleParameter
{
    /// <summary>
    ///     参数名
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; } = "";

    /// <summary>
    ///     子参数集合
    /// </summary>
    /// <value>The parameters.</value>
    [Key("parameters")]
    [MessagePackFormatter(typeof(MessagePackParameterListFormatter<List<object>>))]
    [JsonProperty("parameters", Required = Required.AllowNull)]
    [JsonConverter(typeof(JsonParameterListConverter<List<object>>))]
    //public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    public List<object> Parameters { get; set; } = new(); //Object:可以是Parameter也可以是List<Parameter>

    /// <summary>
    ///     参数当前值
    /// </summary>
    /// <value>The value.</value>
    [Key("value")]
    [JsonProperty("value", Required = Required.Default)]
    public object Value { get; set; }

    /// <summary>
    ///     单位
    /// </summary>
    /// <value>The unit.</value>
    [Key("unit")]
    [JsonProperty("unit")]
    public string Unit { get; set; } = "";

    /// <summary>
    ///     Clones this instance.
    /// </summary>
    /// <returns>Parameter.</returns>
    public Parameter Clone()
    {
        var clone = MemberwiseClone();
        if (clone is Parameter parameter) return parameter;

        return null;
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"Parameter:{Name},{Value}";
    }
}