// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="FunctionInfo.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Define;

/// <summary>
///     功能信息类
/// </summary>
[MessagePackObject]
public class FunctionInfo
{
    /// <summary>
    ///     功能ID
    /// </summary>
    /// <value>The function identifier.</value>
    [Key("functionId")]
    [JsonProperty("functionId")]
    public Guid FunctionId { get; set; }

    /// <summary>
    ///     模板ID
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("templateId")]
    [JsonProperty("templateId")]
    public Guid TemplateId { get; set; }

    /// <summary>
    ///     功能名称
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    ///     功能类型
    /// </summary>
    /// <value>The feature.</value>
    [Key("feature")]
    [JsonProperty("feature")]
    public FeatureType Feature { get; set; }

    /// <summary>
    ///     功能类名
    /// </summary>
    /// <value>The class.</value>
    [Key("className")]
    [JsonProperty("className")]
    public string Class { get; set; }

    /// <summary>
    ///     功能是否可用
    /// </summary>
    /// <value><c>true</c> if applicable; otherwise, <c>false</c>.</value>
    [Key("applicable")]
    [JsonProperty("applicable")]
    public bool Applicable { get; set; }

    /// <summary>
    ///     主设备ID
    /// </summary>
    /// <value>The primary device.</value>
    [Key("primaryDevice")]
    [JsonProperty("primaryDevice")]
    public Guid PrimaryDevice { get; set; }

    /// <summary>
    ///     可选设备集合
    /// </summary>
    /// <value>The secondary devices.</value>
    [Key("secondaryDevices")]
    [JsonProperty("secondaryDevices")]
    public List<Guid> SecondaryDevices { get; set; } = new();

    /// <summary>
    ///     功能参数
    /// </summary>
    /// <value>The parameters.</value>
    [JsonProperty("parameters", Required = Required.AllowNull)]
    [Key("parameters")]
    public List<Parameter> Parameters { get; set; } = new();
}