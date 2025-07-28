using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;

namespace Core.Define;

[MessagePackObject]
public class ReportDataHeader
{
    /// <summary>
    ///     采集间隔 单位ms
    /// </summary>
    [Key("interval")]
    public int Interval { get; set; } = 1000;

    /// <summary>
    ///     任务开始时间 自1970年开始 单位ms
    /// </summary>
    [Key("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     监测信息
    /// </summary>
    [Key("monitorInfo")]
    public MonitorInfo MonitorInfo { get; set; }
}

[MessagePackObject]
public class MonitorInfo
{
    /// <summary>
    ///     任务ID
    /// </summary>
    [Key("taskId")]
    public Guid TaskId { get; set; }

    /// <summary>
    ///     频段信息
    /// </summary>
    [Key("segment")]
    public Dictionary<string, object> Segment { get; set; }

    /// <summary>
    ///     当前文件的起始行号
    /// </summary>
    [Key("startIndex")]
    public long StartIndex { get; set; }

    /// <summary>
    ///     其他参数信息集合（暂定）
    /// </summary>
    [Key("parameters")]
    public List<Parameter> Parameters { get; set; }

    /// <summary>
    ///     天线因子数据
    /// </summary>
    [Key("factors")]
    public List<short> Factors { get; set; }
}

/// <summary>
///     电平分平数据
/// </summary>
[Serializable]
public struct LevelDistribution
{
    /// <summary>
    ///     索引
    /// </summary>
    public byte Index;

    /// <summary>
    ///     电平值
    /// </summary>
    public short Value;

    /// <summary>
    ///     出现次数
    /// </summary>
    public ushort Count;
}