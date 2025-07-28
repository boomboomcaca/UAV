// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ModuleInfoForUpload.cs" company="锟缴讹拷锟斤拷锟斤拷锟斤拷锟斤拷息锟斤拷锟斤拷锟斤拷锟睫癸拷司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class ModuleInfoForUpload.
/// </summary>
[MessagePackObject]
public class ModuleInfoForUpload
{
    /// <summary>
    ///     Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    [JsonProperty("name", Required = Required.Always)]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the remark.
    /// </summary>
    /// <value>The remark.</value>
    [Key("remark")]
    [JsonProperty("remark", Required = Required.Always)]
    public string Remark { get; set; }

    /// <summary>
    ///     Gets or sets the template.
    /// </summary>
    /// <value>The template.</value>
    [Key("template")]
    [JsonProperty("template", Required = Required.Always)]
    public ModuleInfo Template { get; set; }
}