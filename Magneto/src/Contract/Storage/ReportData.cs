using System;
using System.Collections.Generic;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Contract.Storage;

public sealed class TaskFileManagerInfo
{
    /// <summary>
    ///     任务锁
    /// </summary>
    public readonly object DataSyncLock = new();

    /// <summary>
    ///     首包文件标记
    ///     添加此标记的目的是删除首包以后需要发送delete消息至云端，并且需要外部任务清理所有文件
    /// </summary>
    public bool IsFirstFile = false;

    /// <summary>
    ///     任务状态标记
    /// </summary>
    public volatile bool IsRunning = false;

    /// <summary>
    ///     任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    ///     边缘端ID
    /// </summary>
    public string EdgeId { get; set; }

    /// <summary>
    ///     模块ID
    /// </summary>
    public string DriverId { get; set; }

    /// <summary>
    ///     前端插件ID
    /// </summary>
    public string PluginId { get; set; }

    /// <summary>
    ///     设备ID
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    ///     数据文件统一名称
    /// </summary>
    public string PublicFileName { get; set; }

    /// <summary>
    ///     任务类型
    /// </summary>
    public FeatureType Feature { get; set; }

    /// <summary>
    ///     任务创建时间
    /// </summary>
    public DateTime TaskCreatedTime { get; set; }

    /// <summary>
    ///     数据在数据文件中的偏移量
    /// </summary>
    public long DataStreamPosition { get; set; }

    /// <summary>
    ///     任务数据保存绝对路径
    /// </summary>
    public string TaskFolderPath { get; set; }

    /// <summary>
    ///     临时数据文件名称
    /// </summary>
    public string TempDataFileName { get; set; }

    /// <summary>
    ///     临时索引文件名称
    /// </summary>
    public string TempIndexFileName { get; set; }

    /// <summary>
    ///     文件头
    /// </summary>
    public byte[] Summary { get; set; }

    /// <summary>
    ///     当前数据索引
    ///     为帧数，每写一帧数据加1
    /// </summary>
    public long CurrentDataIndex { get; set; }

    /// <summary>
    ///     当前数据文件索引
    ///     为文件索引，每新增一个文件加1
    /// </summary>
    public ushort CurrentFileIndex { get; set; }

    /// <summary>
    ///     当前索引文件保存到的位置
    /// </summary>
    public long CurrentIndexPosition { get; set; }

    /// <summary>
    ///     参数信息
    /// </summary>
    public ParametersInfo ParametersInfo { get; set; }

    /// <summary>
    ///     缓存已保存的数据类型
    /// </summary>
    public List<string> DataTypeList { get; set; } = new();

    /// <summary>
    ///     缓存当前的参数信息
    /// </summary>
    public List<Parameter> Parameters { get; set; }

    /// <summary>
    ///     通知信息
    /// </summary>
    public FileSavedNotification FileNotificationInfo { get; set; }

    #region 缓存当前保存周期的一些数据，为清除数据文件做准备

    /// <summary>
    ///     存放一次存储周期的所有文件大小
    /// </summary>
    public long AllDataLength { get; set; }

    /// <summary>
    ///     当前保存周期的起始文件索引
    ///     如果清除了本周期，则文件索引需要还原
    /// </summary>
    public ushort CurrentCycleFileIndex { get; set; }

    /// <summary>
    ///     当前保存周期在索引文件中的起始索引位置
    ///     如果清除了本周期，则索引位置需要还原
    /// </summary>
    public long CurrentCycleIndexPosition { get; set; }

    /// <summary>
    ///     当前保存周期的起始数据索引
    ///     如果清除了本周期，则数据索引需要还原
    /// </summary>
    public long CurrentCycleDataIndex { get; set; }

    /// <summary>
    ///     标记当前周期的存储是否开启
    /// </summary>
    public bool CurrentCycleRunning { get; set; }

    #endregion
}

[Serializable]
[MessagePackObject]
public class ParametersInfo
{
    [Key("parameters")] public Dictionary<string, List<Parameter>> Parameters { get; set; }

    [Key("parameterMap")] public Dictionary<string, List<int>> ParameterMap { get; set; }

    [Key("storageTimeSegments")] public List<StorageTimeSegment> StorageTimeSegments { get; set; }

    [Key("factors")] public Dictionary<string, List<SDataFactor>> Factors { get; set; }
}

[Serializable]
[MessagePackObject]
public class StorageTimeSegment
{
    [Key("startTime")] public ulong StartTime { get; set; }

    [Key("stopTime")] public ulong StopTime { get; set; }
}