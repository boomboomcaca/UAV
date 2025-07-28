// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ReportData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Protocol.Data;

/// <summary>
///     向前端推送的示向度最优值数据
/// </summary>
public class SDataDfStatOptimal : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfStatOptimal" /> class.
    /// </summary>
    public SDataDfStatOptimal()
    {
        Type = SDataType.DfStatOptimal;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public short[] Data { get; set; }
}

/// <summary>
///     报表数据基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SDataReportBase<T> : SDataRaw
{
    /// <summary>
    ///     任务ID
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public Guid TaskId { get; set; }

    /// <summary>
    ///     数据体
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public T Data { get; set; }
}

/// <summary>
///     干涉仪报表数据
/// </summary>
public class SDataDfStatCi : SDataReportBase<StatCiInfo>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfStatCi" /> class.
    /// </summary>
    public SDataDfStatCi()
    {
        Type = SDataType.DfStatCi;
    }
}

/// <summary>
///     空间谱报表数据
/// </summary>
public class SDataDfStatSse : SDataReportBase<StatSseInfo>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfStatSse" /> class.
    /// </summary>
    public SDataDfStatSse()
    {
        Type = SDataType.DfStatSse;
    }
}

/// <summary>
///     离散测向报表数据
/// </summary>
public class SDataMfdfStat : SDataReportBase<StatMfdfInfo>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataMfdfStat" /> class.
    /// </summary>
    public SDataMfdfStat()
    {
        Type = SDataType.MfdfStat;
    }
}

/// <summary>
///     频段扫描报表数据
/// </summary>
public class SDataScanStat : SDataReportBase<List<StatScanInfo>>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataScanStat" /> class.
    /// </summary>
    public SDataScanStat()
    {
        Type = SDataType.ScanStat;
    }
}

/// <summary>
///     频段扫描报表数据
/// </summary>
public class SDataFfmStat : SDataReportBase<StatFfmInfo>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFfmStat" /> class.
    /// </summary>
    public SDataFfmStat()
    {
        Type = SDataType.FfmStat;
    }
}

/// <summary>
///     干涉仪报表详细信息
/// </summary>
[MessagePackObject]
public struct StatCiInfo
{
    /// <summary>
    ///     正北角度
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public short[] Azimuth { get; set; }

    /// <summary>
    ///     Gets or sets the maximum level.
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public short[] MaxLevel { get; set; }

    /// <summary>
    ///     Gets or sets the maximum quality.
    /// </summary>
    /// <value>The maximum quality.</value>
    [Key("maxQuality")]
    public short[] MaxQuality { get; set; }

    /// <summary>
    ///     Gets or sets the count.
    /// </summary>
    /// <value>The count.</value>
    [Key("count")]
    public ushort[] Count { get; set; }
}

/// <summary>
///     空间谱报表详细信息
/// </summary>
[MessagePackObject]
public struct StatSseInfo
{
    /// <summary>
    ///     正北角度
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public short[] Azimuth { get; set; }

    /// <summary>
    ///     可信度 单位0.1%
    /// </summary>
    /// <value>The maximum quality.</value>
    [Key("maxQuality")]
    public short[] MaxQuality { get; set; }
}

/// <summary>
///     离散测向报表详细信息
/// </summary>
[MessagePackObject]
public struct StatMfdfInfo
{
    /// <summary>
    ///     频率 单位MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double[] Frequency { get; set; }

    /// <summary>
    ///     测向电平 单位0.1dBμV
    /// </summary>
    /// <value>The level.</value>
    [Key("level")]
    public short[] Level { get; set; }

    /// <summary>
    ///     正北角度 单位0.1°
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public short[] Azimuth { get; set; }

    /// <summary>
    ///     最优质量 单位0.1%
    /// </summary>
    /// <value>The quality.</value>
    [Key("quality")]
    public short[] Quality { get; set; }
}

/// <summary>
///     单频测量报表数据
/// </summary>
[MessagePackObject]
public struct StatFfmInfo
{
    /// <summary>
    ///     电平最大值 dBμV
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public double MaxLevel { get; set; }

    /// <summary>
    ///     电平平均值 dBμV
    /// </summary>
    /// <value>The ave level.</value>
    [Key("aveLevel")]
    public double AveLevel { get; set; }

    /// <summary>
    ///     场强最大值 dBμV/m
    /// </summary>
    /// <value>The maximum field strength.</value>
    [Key("maxFs")]
    public double MaxFieldStrength { get; set; }

    /// <summary>
    ///     场强平均值 dBμV/m
    /// </summary>
    /// <value>The ave field strength.</value>
    [Key("aveFs")]
    public double AveFieldStrength { get; set; }
}

/// <summary>
///     频段扫描报表详细信息
/// </summary>
[MessagePackObject]
public struct StatScanInfo
{
    /// <summary>
    ///     频段序号
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     频率 单位MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double[] Frequency { get; set; }

    /// <summary>
    ///     场强最大值 单位0.1dBμV/m
    /// </summary>
    /// <value>The maximum.</value>
    [Key("maximum")]
    public short[] Maximum { get; set; }

    /// <summary>
    ///     场强最小值 单位0.1dBμV/m
    /// </summary>
    /// <value>The minimum.</value>
    [Key("minimum")]
    public short[] Minimum { get; set; }

    /// <summary>
    ///     场强平均值 单位0.1dBμV/m
    /// </summary>
    /// <value>The average.</value>
    [Key("average")]
    public short[] Average { get; set; }

    /// <summary>
    ///     占用度 单位0.1%
    /// </summary>
    /// <value>The occupancy.</value>
    [Key("occupancy")]
    public short[] Occupancy { get; set; }
}