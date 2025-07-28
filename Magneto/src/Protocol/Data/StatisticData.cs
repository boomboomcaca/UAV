// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="StatisticData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Protocol.Data;

/*
    统计数据
*/
/// <summary>
///     数据更新速率
/// </summary>
public class SDataFps : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFps" /> class.
    /// </summary>
    public SDataFps()
    {
        Type = SDataType.Fps;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public double Data { get; set; }
}

/// <summary>
///     原始数据存储大小数据
/// </summary>
public class SDataRawDataLen : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataRawDataLen" /> class.
    /// </summary>
    public SDataRawDataLen()
    {
        Type = SDataType.RawDataLength;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public double Data { get; set; }

    /// <summary>
    ///     Gets or sets the unit.
    /// </summary>
    /// <value>The unit.</value>
    [Key("unit")]
    public string Unit { get; set; }

    /// <summary>
    ///     Gets or sets the disk space.
    /// </summary>
    /// <value>The disk space.</value>
    [Key("diskSpace")]
    public double DiskSpace { get; set; }

    /// <summary>
    ///     Gets or sets the disk unit.
    /// </summary>
    /// <value>The disk unit.</value>
    [Key("diskUnit")]
    public string DiskUnit { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance can save.
    /// </summary>
    /// <value><c>true</c> if this instance can save; otherwise, <c>false</c>.</value>
    [Key("canSave")]
    public bool CanSave { get; set; } = false;
}

/// <summary>
///     离散测向统计信号列表
/// </summary>
public class SDataMfdfSignal : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataMfdfSignal" /> class.
    /// </summary>
    public SDataMfdfSignal()
    {
        Type = SDataType.MfdfSignal;
    }

    /// <summary>
    ///     序号
    /// </summary>
    /// <value>The index.</value>
    [Key("index")]
    public int Index { get; set; }

    /// <summary>
    ///     频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     最优值 °
    /// </summary>
    /// <value>The optimal azimuth.</value>
    [Key("optimalAzimuth")]
    public float OptimalAzimuth { get; set; }

    /// <summary>
    ///     瞬时值 °
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public float Azimuth { get; set; }

    /// <summary>
    ///     电平值 dBμV
    /// </summary>
    /// <value>The level.</value>
    [Key("level")]
    public float Level { get; set; }

    /// <summary>
    ///     质量 %
    /// </summary>
    /// <value>The quality.</value>
    [Key("quality")]
    public float Quality { get; set; }
}

/// <summary>
///     单频测向示向度概率分布统计信息
/// </summary>
public class SDataDfindProbDist : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfindProbDist" /> class.
    /// </summary>
    public SDataDfindProbDist()
    {
        Type = SDataType.DfindProbDist;
    }

    /// <summary>
    ///     单频测向示向度概率分布统计信息
    ///     长度360
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}