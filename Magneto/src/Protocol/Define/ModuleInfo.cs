// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ModuleInfo.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class ModuleInfo.
/// </summary>
[MessagePackObject]
public class ModuleInfo
{
    /// <summary>
    ///     模块编号
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    [JsonProperty("id", Required = Required.Always)]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    [JsonProperty("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the station identifier.
    /// </summary>
    /// <value>The station identifier.</value>
    [Key("mfid")]
    [JsonProperty("mfid")]
    public string StationId { get; set; }

    /// <summary>
    ///     模板编号
    ///     弃用-云端保存在数据库中时自己维护了一个int型的自增长ID
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("templateID")]
    [JsonProperty("templateID", Required = Required.Always)]
    [JsonIgnore]
    [IgnoreMember]
    public Guid TemplateId { get; set; }

    /// <summary>
    ///     模块类型，内部使用
    /// </summary>
    /// <value>The class.</value>
    [JsonProperty("type")]
    [Key("type")]
    public string Class { get; set; }

    /// <summary>
    ///     Gets or sets the type of the module.
    /// </summary>
    /// <value>The type of the module.</value>
    [JsonProperty("moduleType")]
    [Key("moduleType")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleType>))]
    public ModuleType ModuleType { get; set; }

    /// <summary>
    ///     模块分类
    ///     01：监测扫描
    ///     02：测向定位
    ///     03：天线控制
    ///     04：控制器
    ///     05：GPS
    ///     06：电子罗盘
    ///     07：传感器
    ///     08：解码器
    /// </summary>
    /// <value>The category.</value>
    [JsonProperty("moduleCategory", Required = Required.AllowNull)]
    [Key("moduleCategory")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleCategory>))]
    public ModuleCategory Category { get; set; }

    /// <summary>
    ///     模块状态
    /// </summary>
    /// <value>The state.</value>
    [JsonProperty("moduleState", Required = Required.Default)]
    [Key("moduleState")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleState>))]
    public ModuleState State { get; set; } = ModuleState.Idle;

    /// <summary>
    ///     支持的功能类型
    /// </summary>
    /// <value>The feature.</value>
    [Key("supportedFeatures")]
    [JsonProperty("supportedFeatures", Required = Required.Default)]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<FeatureType>))]
    public FeatureType Feature { get; set; } = FeatureType.None;

    /// <summary>
    ///     显示名称
    /// </summary>
    /// <value>The display name.</value>
    [JsonProperty("displayName", Required = Required.Always)]
    [Key("displayName")]
    public string DisplayName { get; set; } = "";

    // /// <summary>
    // /// 主设备名称
    // /// </summary>
    // [JsonProperty("deviceName")]
    // [Key("deviceName")]
    // public string DeviceName { get; set; } = "";
    /// <summary>
    ///     描述信息
    /// </summary>
    /// <value>The description.</value>
    [JsonProperty("description", Required = Required.AllowNull)]
    [Key("description")]
    public string Description { get; set; } = "";

    /// <summary>
    ///     制造商
    /// </summary>
    /// <value>The manufacturer.</value>
    [JsonProperty("manufacturer", Required = Required.AllowNull)]
    [Key("manufacturer")]
    public string Manufacturer { get; set; } = "";

    /// <summary>
    ///     型号
    /// </summary>
    /// <value>The model.</value>
    [JsonProperty("model", Required = Required.AllowNull)]
    [Key("model")]
    public string Model { get; set; } = "";

    /// <summary>
    ///     Gets or sets the sn.
    /// </summary>
    /// <value>The sn.</value>
    [Key("sn")]
    [JsonProperty("sn")]
    public string Sn { get; set; }

    /// <summary>
    ///     最大实例数
    /// </summary>
    /// <value>The maximum instance.</value>
    [JsonProperty("maximumInstance")]
    [Key("maximumInstance")]
    public int MaxInstance { get; set; }

    /// <summary>
    ///     参数
    /// </summary>
    /// <value>The parameters.</value>
    [JsonProperty("parameters", Required = Required.AllowNull)]
    [Key("parameters")]
    public List<Parameter> Parameters { get; set; } = new();

    /// <summary>
    ///     参数脚本
    /// </summary>
    /// <value>The constraint script.</value>
    [JsonProperty("constraintScript")]
    [Key("constraintScript")]
    [JsonConverter(typeof(JsonObjectConverter<JObject>))]
    public JObject ConstraintScript { get; set; } = new();

    /// <summary>
    ///     模板版本
    /// </summary>
    /// <value>The version.</value>
    [JsonProperty("version", Required = Required.Always)]
    [Key("version")]
    public string Version { get; set; } = "0.0.0";

    /// <summary>
    ///     模块能力
    /// </summary>
    /// <value>The capability.</value>
    [JsonProperty("capability")]
    [Key("capability")]
    public string Capability { get; set; } = "";

    /// <summary>
    ///     是否独占 暂时不加
    /// </summary>
    /// <value><c>true</c> if this instance is monopolized; otherwise, <c>false</c>.</value>
    [JsonProperty("isMonopolized")]
    [Key("isMonopolized")]
    [IgnoreMember] //MessagePack
    [JsonIgnore] //Newtonsoft.Json
    public bool IsMonopolized { get; set; }

    /// <summary>
    ///     内部使用，模块引用计数
    /// </summary>
    /// <value>The reference count.</value>
    [IgnoreMember] //MessagePack
    [JsonIgnore] //Newtonsoft.Json
    public int RefCount { get; set; }

    /// <summary>
    ///     内部使用，模块上一次状态变化的时间
    /// </summary>
    /// <value>The last state time.</value>
    [IgnoreMember]
    [JsonIgnore]
    public DateTime LastStateTime { get; set; }

    /// <summary>
    ///     字典转换为ModuleInfo
    ///     由于从云端查询到的模块配置中的Parameter参数无法直接解析，因此需要在这里单独解析
    ///     -new:2020-9-24 黄渔将Parameter参数转换为json数组传过来，因此不需要做特殊处理了，因此这个方法弃用
    ///     -new:2022-8-8 现在返回的constraintScript字段也为string，需要修改为json对象
    /// </summary>
    /// <param name="dict">The dictionary.</param>
    /// <returns>The result of the conversion.</returns>
    // [Obsolete("2020-9-24 黄渔将Parameter参数转换为json数组传过来，因此不需要做特殊处理了，因此这个方法弃用")]
    public static explicit operator ModuleInfo(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        if (dict.ContainsKey("constraintScript"))
        {
            var str = dict["constraintScript"].ToString();
            try
            {
                if (str != null) dict["constraintScript"] = JsonConvert.DeserializeObject<JObject>(str);
            }
            catch
            {
                dict["constraintScript"] = new JObject();
            }
        }

        var json = JsonConvert.SerializeObject(dict);
        var template = JsonConvert.DeserializeObject<ModuleInfo>(json);
        return template;
    }

    /// <summary>
    ///     Clones this instance.
    /// </summary>
    /// <returns>ModuleInfo.</returns>
    public ModuleInfo Clone()
    {
        return MemberwiseClone() as ModuleInfo;
    }
}