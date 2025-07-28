using Newtonsoft.Json;

namespace Magneto.Definition;

public class AppSettings
{
    [JsonProperty("ipAddress")] public string IpAddress { get; set; }

    [JsonProperty("port")] public int Port { get; set; }

    [JsonProperty("cloudIpAddress")] public string CloudIpAddress { get; set; }

    [JsonProperty("cloudPort")] public int CloudPort { get; set; }

    [JsonProperty("edgeID")] public string EdgeId { get; set; }

    /// <summary>
    ///     0:表示边缘端  1:表示环境控制
    /// </summary>
    [JsonProperty("type")]
    public int ServerType { get; set; }

    /// <summary>
    ///     边缘端对外显示IP地址的方式
    ///     0- 以云端返回的IP地址为准
    ///     1- 以本地配置的IP地址为准
    /// </summary>
    [JsonProperty("ipType")]
    public int IpType { get; set; }

    [JsonProperty("computerId")] public string ComputerId { get; set; }

    /// <summary>
    ///     webSocket超时时间 秒
    /// </summary>
    [JsonProperty("timeout")]
    public int Timeout { get; set; } = 30;

    /// <summary>
    ///     是否开启动态压帧
    /// </summary>
    [JsonProperty("frameDynamic")]
    public bool FrameDynamic { get; set; } = true;

    /// <summary>
    ///     固定压帧时间间隔 ms
    /// </summary>
    [JsonProperty("frameSpan")]
    public int FrameSpan { get; set; } = 30;

    /// <summary>
    ///     数据真实存储目录
    /// </summary>
    [JsonProperty("dataDir")]
    public string DataDir { get; set; }

    /// <summary>
    ///     云端账号
    /// </summary>
    [JsonProperty("cloudUser")]
    public string CloudUser { get; set; }

    /// <summary>
    ///     云端密码
    /// </summary>
    [JsonProperty("cloudPassword")]
    public string CloudPassword { get; set; }

    /// <summary>
    ///     音频识别服务器的地址
    /// </summary>
    [JsonProperty("audioRecogAddress")]
    public string AudioRecogAddress { get; set; }

    /// <summary>
    ///     音频识别服务器的端口
    /// </summary>
    [JsonProperty("audioRecogPort")]
    public int AudioRecogPort { get; set; }

    /// <summary>
    ///     音频识别服务器的Key
    /// </summary>
    [JsonProperty("audioRecogServerKey")]
    public string AudioRecogServerKey { get; set; }
}