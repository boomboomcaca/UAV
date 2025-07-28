// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="DataTypeAttribute.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;

namespace Magneto.Protocol.Define;

/// <summary>
///     Class DataTypeAttribute.
///     Implements the <see cref="Attribute" />
/// </summary>
/// <seealso cref="Attribute" />
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)]
public class DataTypeAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeAttribute" /> class.
    /// </summary>
    public DataTypeAttribute()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DataTypeAttribute" /> class.
    /// </summary>
    /// <param name="sendDirectly">if set to <c>true</c> [send directly].</param>
    /// <param name="needSampling">if set to <c>true</c> [need sampling].</param>
    /// <param name="canDrop">if set to <c>true</c> [can drop].</param>
    public DataTypeAttribute(bool sendDirectly, bool needSampling, bool canDrop)
    {
        SendDirectly = sendDirectly;
        NeedSampling = needSampling;
        CanDrop = canDrop;
    }

    /// <summary>
    ///     数据是否直接发送
    /// </summary>
    /// <value><c>true</c> if [send directly]; otherwise, <c>false</c>.</value>
    public bool SendDirectly { get; set; }

    /// <summary>
    ///     数据是否需要采样
    /// </summary>
    /// <value><c>true</c> if [need sampling]; otherwise, <c>false</c>.</value>
    public bool NeedSampling { get; set; }

    /// <summary>
    ///     数据是否可以丢弃
    /// </summary>
    /// <value><c>true</c> if this instance can drop; otherwise, <c>false</c>.</value>
    public bool CanDrop { get; set; }
}