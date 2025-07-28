// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ISystem.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Threading.Tasks;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Magneto.Protocol.Interface;

/// <summary>
///     系统接口
/// </summary>
public interface ISystem
{
    /// <summary>
    ///     测试方法
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="age">The age.</param>
    /// <returns>Task&lt;HelloResponse&gt;.</returns>
    [JsonRpcMethod("edge.sayHello")]
    Task<HelloResponse> SayHelloAsync(string name, int age);

    /// <summary>
    ///     查询站点信息
    /// </summary>
    /// <returns>Task&lt;QueryStationResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryStation")]
    Task<QueryStationResponse> QueryStationAsync();

    /// <summary>
    ///     查询设备信息
    /// </summary>
    /// <returns>Task&lt;QueryDevicesResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryDevice")]
    Task<QueryDevicesResponse> QueryDeviceAsync();

    /// <summary>
    ///     查询功能信息
    /// </summary>
    /// <returns>Task&lt;QueryDriversResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryDriver")]
    Task<QueryDriversResponse> QueryDriverAsync();

    /// <summary>
    ///     查询所有信息
    /// </summary>
    /// <returns>Task&lt;AllInfoResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryAllinfo")]
    Task<AllInfoResponse> QueryAllInfoAsync();

    /// <summary>
    ///     查询当前运行的任务信息
    /// </summary>
    /// <returns>Task&lt;SDataTask&gt;.</returns>
    [JsonRpcMethod("edge.queryRunningTasks")]
    Task<SDataTask> QueryRunningTasksAsync();

    /// <summary>
    ///     重新启动应用
    /// </summary>
    /// <returns>Task.</returns>
    [JsonRpcMethod("edge.restartApp")]
    Task RestartAppAsync();

    /// <summary>
    ///     更新配置
    /// </summary>
    /// <returns>Task.</returns>
    [JsonRpcMethod("edge.updateConfig")]
    Task UpdateConfigAsync();

    /// <summary>
    ///     更新计划
    /// </summary>
    /// <returns>Task.</returns>
    [JsonRpcMethod("edge.updateCrontab")]
    Task UpdateCronTabAsync();

    /// <summary>
    ///     Rsync配置更新
    /// </summary>
    /// <param name="configType">Type of the configuration.</param>
    /// <returns>Task.</returns>
    [JsonRpcMethod("edge.updateRsync")]
    Task UpdateRsyncConfigAsync(RsyncConfigType configType);

    /// <summary>
    ///     带回复的心跳
    /// </summary>
    /// <returns>Task&lt;System.Int32&gt;.</returns>
    [JsonRpcMethod("heartbeat")]
    Task<int> HeartBeatWithResultAsync();

    /// <summary>
    ///     获取站点能力
    /// </summary>
    /// <returns>Task&lt;EdgeCapacity&gt;.</returns>
    [JsonRpcMethod("edge.getCapacity")]
    Task<EdgeCapacity> GetEdgeCapacityAsync();

    /// <summary>
    ///     查询边缘端路径下的文件
    ///     仅支持./data下的相对路径
    /// </summary>
    /// <param name="directory">The directory.</param>
    /// <returns>Task&lt;QueryDirectoryResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryDirectory")]
    Task<QueryDirectoryResponse> QueryDirectoryAsync(string directory);

    /// <summary>
    ///     Queries the features asynchronous.
    /// </summary>
    /// <returns>Task&lt;QueryAvailableFeaturesResponse&gt;.</returns>
    [JsonRpcMethod("edge.queryAvailableFeatures")]
    Task<QueryAvailableFeaturesResponse> QueryFeaturesAsync();

    /// <summary>
    ///     云端射电任务更新接口
    /// </summary>
    /// <param name="taskId">云端任务Id</param>
    /// <param name="moduleId">The module identifier.</param>
    /// <param name="pluginId">The plugin identifier.</param>
    /// <param name="pluginName">Name of the plugin.</param>
    /// <returns>Task&lt;FastEmtTaskResponse&gt;.</returns>
    [JsonRpcMethod("edge.updateFastemt")]
    Task<FastEmtTaskResponse> UpdateFastEmtAsync(string taskId, Guid moduleId, string pluginId = "",
        string pluginName = "");
}