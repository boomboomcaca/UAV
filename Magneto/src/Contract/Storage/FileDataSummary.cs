using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Contract.Storage;

/// <summary>
///     数据文件的概要信息
/// </summary>
[MessagePackObject]
public class FileDataSummary
{
    /// <summary>
    ///     数据类型
    /// </summary>
    [Key("dataType")]
    [JsonProperty("dataType", Required = Required.Always)]
    [JsonConverter(typeof(JsonEnumAsStringFormatter<FileDataType>))]
    public FileDataType DataType { get; set; }

    /// <summary>
    ///     边缘端编号
    /// </summary>
    [Key("edgeId")]
    [JsonProperty("edgeId", Required = Required.Always)]
    public string EdgeId { get; set; }

    /// <summary>
    ///     驱动编号
    /// </summary>
    [Key("driverId")]
    [JsonProperty("driverId", Required = Required.Always)]
    public string DriverId { get; set; }

    /// <summary>
    ///     设备编号
    /// </summary>
    [Key("deviceId")]
    [JsonProperty("deviceId", Required = Required.Always)]
    public string DeviceId { get; set; }

    /// <summary>
    ///     任务编号
    /// </summary>
    [Key("taskId")]
    [JsonProperty("taskId", Required = Required.Always)]
    public string TaskId { get; set; }

    /// <summary>
    ///     模块编号
    /// </summary>
    [Key("pluginId")]
    [JsonProperty("pluginId", Required = Required.Always)]
    public string PluginId { get; set; } //默认值为调试需要，避免云端报错

    /// <summary>
    ///     附加信息
    /// </summary>
    [Key("tag")]
    [JsonProperty("tag", Required = Required.Always)]
    public string Tag { get; set; }
}