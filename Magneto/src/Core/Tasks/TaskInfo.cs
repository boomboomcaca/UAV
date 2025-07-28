using System;
using System.Collections.Generic;
using Core.Define;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Tasks;

/// <summary>
///     任务信息
/// </summary>
public class TaskInfo
{
    /// <summary>
    ///     任务ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     前端模块ID
    /// </summary>
    public string PluginId { get; set; }

    /// <summary>
    ///     前端模块名称
    /// </summary>
    public string PluginName { get; set; }

    /// <summary>
    ///     是否是计划任务
    /// </summary>
    public bool IsCrondTask { get; set; }

    /// <summary>
    ///     计划编号
    /// </summary>
    public string CrondId { get; set; }

    /// <summary>
    ///     其他信息
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    ///     任务名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     任务开启的功能类型
    /// </summary>
    public FeatureType Feature { get; set; }

    /// <summary>
    ///     任务优先级
    ///     0: 独占任务
    ///     int.MaxValue: 闲时任务
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    ///     任务等级
    /// </summary>
    public TaskLevel Level { get; set; }

    // /// <summary>
    // /// 当前任务的优先级
    // /// 如果任务存在争用，则每执行一次任务就优先级+1直到等于最大的优先级数值，再从最小的优先级开始循环
    // /// </summary>
    // public int CurrentPriority { get; set; }
    // /// <summary>
    // /// 可以重置优先级标记
    // /// </summary>
    // public bool CanResetPriority { get; set; }
    /// <summary>
    ///     任务承载的功能ID
    /// </summary>
    public Guid ModuleId { get; set; }

    /// <summary>
    ///     任务实际状态
    /// </summary>
    public TaskState State { get; set; }

    /// <summary>
    ///     任务请求状态
    /// </summary>
    public TaskState RequestState { get; set; }

    /// <summary>
    ///     功能信息，存放功能实例以及功能下属的设备实例
    /// </summary>
    internal ModuleChain<IDriver> ModuleChain { get; set; }

    internal MediaType MediaType { get; set; }

    /// <summary>
    ///     运行时参数
    /// </summary>
    internal Dictionary<string, Parameter> RunningParameters { get; set; }

    /// <summary>
    ///     任务的数据通道
    /// </summary>
    public DataPort DataPort { get; set; }

    /// <summary>
    ///     创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    ///     任务启动时间
    /// </summary>
    public DateTime BeginTime { get; set; }

    /// <summary>
    ///     任务停止时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    ///     任务上次的调度时间
    ///     排序用
    /// </summary>
    public DateTime LastActiveTime { get; set; }

    /// <summary>
    ///     给前端预留的访问地址
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    ///     是否已经初始化
    ///     此标记防止在多个任务按时间片频繁切换的时候模块重复初始化
    /// </summary>
    public bool IsInitialized { get; set; }

    /// <summary>
    ///     创建标记
    ///     如果是前端/云端的连接创建的，此时这里存放其SessionID
    ///     如果是计划任务，这里为 "Crond"
    /// </summary>
    public string Session { get; set; }

    /// <summary>
    ///     创建人，预留
    /// </summary>
    public string Creator { get; set; }

    /// <summary>
    ///     是否需要保存数据
    /// </summary>
    public bool IsDataSaveNeeded { get; set; }

    /// <summary>
    ///     是否需要保存IQ数据
    /// </summary>
    public bool IsIqDataSaveNeeded { get; set; }

    /// <summary>
    ///     是否需要保存报表数据
    /// </summary>
    public bool IsReportNeeded { get; set; }

    /// <summary>
    ///     任务所需的设备故障
    /// </summary>
    public bool IsDeviceFault { get; set; }

    public bool NeedHeart { get; set; } = true;
    public List<SDataFactor> Factors { get; set; }

    public TaskInfo Clone()
    {
        return MemberwiseClone() as TaskInfo;
    }
}