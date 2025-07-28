using System.Collections.Generic;
using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using Newtonsoft.Json;

namespace Core.Define;

/// <summary>
///     解析命令函参数，支持解析如下两种格式
/// </summary>
public class RuleInfo
{
    [JsonProperty("duration")] public List<long> Duration { get; set; }

    [JsonProperty("executors")] public List<string> Executors { get; set; }

    [JsonProperty("executeModules")] public List<string> ExecuteModules { get; set; }

    [JsonProperty("feature")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<FeatureType>))]
    public FeatureType Feature { get; set; }

    [JsonProperty("dataStorage")]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<MediaType>))]
    public MediaType DataStorage { get; set; }

    [JsonProperty("parameters", Required = Required.Always)]
    public Dictionary<string, object> Parameters { get; set; }

    [JsonProperty("priority")] public int Priority { get; set; }

    [JsonIgnore] public string TaskId { get; set; }

    /// <summary>
    ///     在TaskID出现前临时使用的ID
    /// </summary>
    [JsonIgnore]
    public string TempId { get; set; }

    [JsonIgnore] public TaskState State { get; set; } = TaskState.Stop;

    [JsonIgnore] public string Message { get; set; }
}