using System.Collections.Generic;
using Newtonsoft.Json;

namespace Core.Define;

/// <summary>
///     计划任务信息
/// </summary>
public class BatchedCrondInfo
{
    [JsonProperty("id")] public string BatchId { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("creator")] public string Creator { get; set; }

    [JsonProperty("createTime")] public string CreatedTime { get; set; }

    [JsonProperty("updateTime")] public string UpdateTime { get; set; }

    [JsonProperty("effectiveTime")] public string EffectiveTime { get; set; }

    [JsonProperty("expireTime")] public string ExpireTime { get; set; }

    /// <summary>
    ///     0-正常，1-过期，2-撤销
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("cron")] public List<string> Crons { get; set; }

    [JsonProperty("rule")] public List<RuleInfo> Rules { get; set; }
}