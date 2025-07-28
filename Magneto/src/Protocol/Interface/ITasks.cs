// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ITasks.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Magneto.Protocol.Interface;

/// <summary>
///     Interface ITask
/// </summary>
public interface ITask
{
    /// <summary>
    ///     传入基础参数，预设置任务
    /// </summary>
    /// <param name="moduleId">驱动编号</param>
    /// <param name="pluginId">调用端编号</param>
    /// <param name="pluginName">调用端名称</param>
    /// <param name="priority">任务优先级</param>
    /// <param name="needHeart">是否需要心跳</param>
    /// <returns>返回包含任务编号在内的结果</returns>
    [JsonRpcMethod("edge.presetTask")]
    Task<PresetTaskResponse> PresetTaskAsync(Guid moduleId, string pluginId, string pluginName = "", int priority = 1,
        bool needHeart = true);

    /// <summary>
    ///     启动任务
    /// </summary>
    /// <param name="id">任务编号</param>
    /// <param name="account">The account.</param>
    [JsonRpcMethod("edge.startTask")]
    void StartTask(Guid id, string account = "");

    /// <summary>
    ///     停止任务
    /// </summary>
    /// <param name="id">任务编号</param>
    [JsonRpcMethod("edge.stopTask")]
    void StopTask(Guid id);

    /// <summary>
    ///     修改任务参数
    /// </summary>
    /// <param name="id">任务编号</param>
    /// <param name="parameters">任务参数列表</param>
    [JsonRpcMethod("edge.setTaskParameters")]
    void SetTaskParameters(Guid id, List<Parameter> parameters);
}