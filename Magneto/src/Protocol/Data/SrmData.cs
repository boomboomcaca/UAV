// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="SrmData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Protocol.Data;

/// <summary>
///     服务资源监视数据 CPU
/// </summary>
public class SDataSrmCpu : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSrmCpu" /> class.
    /// </summary>
    public SDataSrmCpu()
    {
        Type = SDataType.SrmCpu;
    }

    /// <summary>
    ///     Gets or sets the useage.
    /// </summary>
    /// <value>The useage.</value>
    [Key("useage")]
    public double Useage { get; set; }
}

/// <summary>
///     服务资源监视数据 内存
/// </summary>
public class SDataSrmMemory : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSrmMemory" /> class.
    /// </summary>
    public SDataSrmMemory()
    {
        Type = SDataType.SrmMemory;
    }

    /// <summary>
    ///     Gets or sets the used.
    /// </summary>
    /// <value>The used.</value>
    [Key("used")]
    public double Used { get; set; }

    /// <summary>
    ///     Gets or sets the total.
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public double Total { get; set; }
}

/// <summary>
///     服务资源监视数据 磁盘
/// </summary>
public class SDataSrmHdd : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSrmHdd" /> class.
    /// </summary>
    public SDataSrmHdd()
    {
        Type = SDataType.SrmHdd;
    }

    /// <summary>
    ///     磁盘使用量 GB
    /// </summary>
    /// <value>The used.</value>
    [Key("used")]
    public double Used { get; set; }

    /// <summary>
    ///     磁盘总量 GB
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public double Total { get; set; }
}