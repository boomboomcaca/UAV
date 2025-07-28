// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="StationInfo.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class StationInfo.
/// </summary>
[MessagePackObject]
public class StationInfo
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("id")]
    [JsonProperty("id")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     站点编号
    /// </summary>
    /// <value>The station identifier.</value>
    [Key("mfid")]
    [JsonProperty("mfid")]
    public string StationId { get; set; }

    /// <summary>
    ///     站点名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>
    ///     站点大类
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    [JsonProperty("type")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<StationType>))]
    public StationType Type { get; protected set; }

    /// <summary>
    ///     站点类型
    ///     固定站：01：一类固定监测执行站 02：二类固定监测执行站 03：三类固定监测执行站 04：四类固定监测执行站
    ///     移动站：01：陆地一类移动监测执行站 02：陆地二类移动监测执行站 03：陆地三类移动监测执行站 04：水上移动监測执行站 05：空中移动监測执行站
    /// </summary>
    /// <value>The category.</value>
    [Key("category")]
    [JsonProperty("category")]
    public int Category { get; set; }

    /// <summary>
    ///     监测站地址，即监测站归属地地址
    /// </summary>
    /// <value>The address.</value>
    [Key("address")]
    [JsonProperty("address")]
    public string Address { get; set; } = "";

    /// <summary>
    ///     监测站其他附加信息，由键值对组成
    /// </summary>
    /// <value>The tag.</value>
    [Key("tag")]
    [JsonProperty("tag")]
    public Dictionary<string, object> Tag { get; set; }

    /// <summary>
    ///     Gets or sets the remarks.
    /// </summary>
    /// <value>The remarks.</value>
    [Key("remarks")]
    [JsonProperty("remarks")]
    public string Remarks { get; set; }

    /// <summary>
    ///     Gets or sets the version.
    /// </summary>
    /// <value>The version.</value>
    [Key("version")]
    [JsonProperty("version")]
    public string Version { get; set; }

    /// <summary>
    ///     将字典转换为站点信息的方法
    /// </summary>
    /// <param name="dict">The dictionary.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator StationInfo(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var strTag = "";
        if (dict.ContainsKey("tag"))
        {
            strTag = dict["tag"].ToString();
            dict.Remove("tag");
        }

        var json = JsonConvert.SerializeObject(dict);
        var template = JsonConvert.DeserializeObject<StationInfo>(json);
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var name = property.Name;
                if (Attribute.GetCustomAttribute(property, typeof(JsonPropertyAttribute)) is JsonPropertyAttribute
                    attribute) name = attribute.PropertyName;
                if (name != null && dict.ContainsKey(name)) dict.Remove(name);
            }

            template.Tag = dict;
            if (!string.IsNullOrEmpty(strTag))
            {
                var dics = JsonConvert.DeserializeObject<Dictionary<string, object>>(strTag);
                foreach (var pair in dics) template.Tag.Add(pair.Key, pair.Value);
            }

            return template;
        }
        catch
        {
            return template;
        }
    }

    /// <summary>
    ///     Updates the edge ip.
    /// </summary>
    /// <param name="ip">The ip.</param>
    public void UpdateEdgeIp(string ip)
    {
        Tag ??= new Dictionary<string, object>();
        Tag["ip"] = ip;
    }

    /// <summary>
    ///     Gets the edge ip.
    /// </summary>
    /// <returns>System.String.</returns>
    public string GetEdgeIp()
    {
        if (Tag?.TryGetValue("ip", out var value) is true) return value.ToString();
        return string.Empty;
    }
}

/// <summary>
///     向云端注册时使用
/// </summary>
public class StationRegisterInfo
{
    /// <summary>
    ///     Gets or sets the station.
    /// </summary>
    /// <value>The station.</value>
    public StationInfo Station { get; set; }

    /// <summary>
    ///     Gets or sets the modules.
    /// </summary>
    /// <value>The modules.</value>
    public ModuleInfo[] Modules { get; set; }
}