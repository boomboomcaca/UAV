// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="IEnvironment.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Magneto.Protocol.Interface;

/// <summary>
///     动环接口
/// </summary>
public interface IEnvironment
{
    /// <summary>
    ///     设置参数
    /// </summary>
    /// <param name="moduleId">The module identifier.</param>
    /// <param name="parameters">The parameters.</param>
    [JsonRpcMethod("edge.setSwitches")]
    public void SetSwitches(Guid moduleId, List<Parameter> parameters);
}