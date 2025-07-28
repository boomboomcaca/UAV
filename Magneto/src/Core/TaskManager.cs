using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Configuration;
using Core.Define;
using Core.Tasks;
using Magneto.Contract;
using Magneto.Contract.Storage;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Core;

public class TaskManager
{
    private static readonly Lazy<TaskManager> _lazy = new(() => new TaskManager());
    private readonly CancellationTokenSource _cts = new();
    private readonly AutoResetEvent _schedulerEvent = new(false);
    private readonly ConcurrentDictionary<Guid, TaskInstance> _taskCache = new();

    public TaskManager()
    {
        _ = Task.Run(() => MonitorTaskAsync(_cts.Token).ConfigureAwait(false), _cts.Token);
        RawDataStorage.Instance.FileModified += ManagerFileModified;
        RawIqDataStorage.Instance.FileModified += ManagerFileModified;
    }

    /// <summary>
    ///     任务管理器实例
    /// </summary>
    public static TaskManager Instance => _lazy.Value;

    public void Close()
    {
        try
        {
            _cts?.Cancel();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts?.Dispose();
        }
    }

    public Guid CreateTask(
        Guid driverId,
        bool crond,
        string createName,
        MediaType saveData,
        string crondId,
        string pluginId,
        string pluginName,
        string remark,
        int priority,
        bool needHeart)
    {
        CheckTaskCanStart(driverId, priority);
        var task = new TaskInstance();
        var id = task.CreateTask(driverId, crond, createName, saveData, crondId, pluginId, pluginName, remark, priority,
            needHeart);
        _taskCache.TryAdd(id, task);
        var count = _taskCache.Count(item => item.Value.TaskInfo.RequestState == TaskState.Start);
        _schedulerEvent.Set();
        Trace.WriteLine(
            $"{DateTime.Now:HH:mm:ss.fff}    CreateTask TaskID={id} Feature={task.Feature} TaskCount:{_taskCache.Count} Running:{count}");
        if (HaveCompetition(task.TaskInfo)) task.IsCompetition = true;
        // if (!canStart)
        // {
        //     var msg = new SDataTaskChangeInfo
        //     {
        //         ActiveTask = activeTaskID,
        //         SuspendedTasks = new() { id }
        //     };
        //     MessageManager.Instance.SendMessage(msg);
        // }
        return id;
    }

    /// <summary>
    ///     根据功能类型创建任务
    ///     计划任务使用
    /// </summary>
    /// <param name="feature">功能类型</param>
    /// <param name="moduleId">功能ID</param>
    /// <param name="saveData">保存数据</param>
    /// <param name="crondId">计划编号</param>
    /// <param name="pluginId">前端模块ID</param>
    /// <param name="planName"></param>
    /// <param name="remark">备注信息</param>
    /// <param name="priority"></param>
    /// <returns>任务编号</returns>
    /// <exception cref="NullReferenceException"></exception>
    public Guid CreateTask(FeatureType feature, string moduleId, MediaType saveData, string crondId, string pluginId,
        string planName, string remark, int priority)
    {
        var crondResult = new SDataCrondResult
        {
            CrondId = crondId,
            Result = true,
            Description = "计划任务创建"
        };
        Guid driverId;
        if (!string.IsNullOrEmpty(moduleId) && Guid.TryParse(moduleId, out var mid))
        {
            driverId = mid;
        }
        else
        {
            var driver = FindDriverByRule(feature);
            if (driver == null)
            {
                crondResult.Result = false;
                crondResult.Description = "计划任务创建失败，找不到合适的功能模块来执行任务";
                MessageManager.Instance.SendMessage(crondResult);
                throw new NullReferenceException("找不到合适的功能模块来执行任务");
            }

            driverId = driver.Id;
        }

        var taskId = CreateTask(driverId, true, "Crond", saveData, crondId, pluginId, $"计划任务-{planName}", remark,
            priority, false);
        crondResult.Result = true;
        crondResult.Description = "计划任务创建成功";
        MessageManager.Instance.SendMessage(crondResult);
        return taskId;
    }

    public void StartTask(Guid taskId, string account = "")
    {
        var task = GetTask(taskId);
        if (task?.TaskInfo == null)
        {
            var exception = new LocalRpcException($"未找到ID为{taskId}的任务")
            {
                ErrorCode = ErrorCode.ErrorCodeTaskNotFound
            };
            throw exception;
        }

        // task.CheckStartSaveData();
        task.TaskInfo.RequestState = TaskState.Start;
        task.TaskInfo.Creator = account;
        _schedulerEvent.Set();
        var desc = "任务启动";
        var detail = $"taskID={task.TaskInfo.Id}";
        if (!string.IsNullOrEmpty(task.TaskInfo.CrondId))
        {
            desc = "计划" + desc;
            detail += $" crondID={task.TaskInfo.CrondId}";
            // 数据订阅放到任务调度启动方法中去
            // taskInfo.DataPort.DataArrived += OnDataArrived;
            var crondResult = new SDataCrondResult
            {
                CrondId = task.TaskInfo.CrondId,
                Result = true,
                Description = "计划任务启动"
            };
            MessageManager.Instance.SendMessage(crondResult);
        }

        var message = new SDataMessage
        {
            LogType = LogType.Message,
            ErrorCode = 0,
            Description = desc,
            Detail = detail
        };
        MessageManager.Instance.Log(message);
        var dic = new Dictionary<string, object>
        {
            { "code", "300001" },
            { "level", "info" },
            { "edgeId", RunningInfo.EdgeId },
            // {"parameter1",task.TaskInfo.ID},
            { "parameter1", task.TaskInfo.Name }
        };
        _ = Task.Run(() => CloudClient.Instance.AddBusinessLogAsync(dic));
        Trace.WriteLine(
            $"{DateTime.Now:HH:mm:sss.fff}    {desc} Feature={task.Feature} TaskID={task.TaskInfo.Id} CrondID={task.TaskInfo.CrondId}");
    }

    public void StopTask(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task?.TaskInfo == null) return;
        // 未找到任务不再抛出异常到前端了
        // var exception = new LocalRpcException($"未找到ID为{taskID}的任务")
        // {
        //     ErrorCode = ErrorCode.ErrorCode_Task_NotFound
        // };
        // throw exception;
        task.TaskInfo.RequestState = TaskState.Stop;
        var desc = "任务停止";
        var detail = $"taskID={task.TaskInfo.Id}";
        if (!string.IsNullOrEmpty(task.TaskInfo.CrondId))
        {
            desc = "计划" + desc;
            detail += $" crondID={task.TaskInfo.CrondId}";
            var crondResult = new SDataCrondResult
            {
                CrondId = task.TaskInfo.CrondId,
                Result = true,
                Description = "计划任务停止"
            };
            MessageManager.Instance.SendMessage(crondResult);
        }

        var message = new SDataMessage
        {
            LogType = LogType.Message,
            ErrorCode = 0,
            Description = desc,
            Detail = detail
        };
        MessageManager.Instance.Log(message);
        var dic = new Dictionary<string, object>
        {
            { "code", "300003" },
            { "level", "info" },
            { "edgeId", RunningInfo.EdgeId },
            // {"parameter1",task.TaskInfo.ID},
            { "parameter1", task.TaskInfo.Name }
        };
        _ = Task.Run(() => CloudClient.Instance.AddBusinessLogAsync(dic));
        Trace.WriteLine(
            $"{DateTime.Now:HH:mm:sss.fff}    {desc} Feature={task.Feature} TaskID={taskId} CrondID={task.TaskInfo.CrondId}");
        _schedulerEvent.Set();
    }

    /// <summary>
    ///     根据连接的SessionID停止任务
    ///     当前端/云端的连接中断以后，防止内存中有遗留的垃圾信息，因此需要将其清理
    /// </summary>
    /// <param name="sessionId"></param>
    public void ClearTaskByName(string sessionId)
    {
        foreach (var pair in _taskCache)
            if (pair.Value.TaskInfo.Session == sessionId)
                pair.Value.TaskInfo.RequestState = TaskState.Stop;
        _schedulerEvent.Set();
    }

    public TaskInstance GetTask(Guid taskId)
    {
        return _taskCache.GetValueOrDefault(taskId);
    }

    /// <summary>
    ///     查询某个任务是否存在
    /// </summary>
    /// <param name="taskId">任务ID</param>
    public bool IsTaskIdExist(Guid taskId)
    {
        return _taskCache.ContainsKey(taskId);
    }

    /// <summary>
    ///     获取所有任务信息
    /// </summary>
    public List<TaskInfo> GetTaskList()
    {
        return _taskCache?.Select(i => i.Value.TaskInfo.Clone()).ToList();
    }

    /// <summary>
    ///     获取当前运行的任务集合
    /// </summary>
    internal SDataTask GetRunningTaskList()
    {
        var list = _taskCache.Where(i => i.Value.TaskInfo.RequestState is TaskState.Start or TaskState.New)
            .Select(i => i.Value);
        return new SDataTask
        {
            Tasks = list.Select(i => new RunningTaskInfo
            {
                Id = i.TaskInfo.Id,
                DeviceId = i.TaskInfo.ModuleChain.Devices.FirstOrDefault()?.ModuleInfo.Id ?? Guid.Empty,
                PluginId = i.TaskInfo.PluginId,
                ModuleId = i.TaskInfo.ModuleId,
                CrondId = i.TaskInfo.CrondId,
                Name = Magneto.Contract.Utils.ConvertEnumToString(i.Feature),
                Uri = i.TaskInfo.Uri,
                MajorParameters = i.TaskInfo.RunningParameters.Where(p => p.Value.IsMajorParameter)
                        .Select(p => p.Value.ToSimple()).ToList(),
                Parameters = i.TaskInfo.RunningParameters.Select(p => p.Value.ToSimple()).ToList(),
                StartTime = Magneto.Contract.Utils.GetTimestamp(i.TaskInfo.CreateTime),
                StopTime = null,
                WorkTime = 0,
                Account = i.TaskInfo.Creator,
                AntennaDescription = i.GetAntennaInfo(),
                Factor = i.TaskInfo.Factors ?? new List<SDataFactor>()
            })
                .ToList()
        };
    }

    internal void DeviceStateChange(SDataMessage message)
    {
        if (message.ErrorCode != (int)InternalMessageType.DeviceRestart) return;
        var str = message.Description;
        if (!Guid.TryParse(str, out var deviceId)) return;
        StopTaskByDevice(deviceId);
    }

    internal void RestartTaskByDevice(Guid deviceId)
    {
        var task = FindTaskInfoBy(deviceId);
        if (task?.Count > 0) task.ForEach(item => item.RestartTask());
    }

    private void StopTaskByDevice(Guid deviceId)
    {
        var task = FindTaskInfoBy(deviceId);
        if (task?.Count > 0) task.ForEach(item => item.InterruptTask());
    }

    /// <summary>
    ///     销毁任务
    /// </summary>
    /// <param name="taskId"></param>
    private void ReleaseTask(Guid taskId)
    {
        _taskCache.TryRemove(taskId, out var info);
        if (info != null && info.TaskInfo.IsCrondTask)
            CrondTaskManager.Instance.UpdatePlanInfo(info.TaskInfo.CrondId, info.TaskInfo.Id.ToString(), TaskState.Stop,
                "计划执行完毕");
        info?.ReleaseTask();
    }

    private void ManagerFileModified(object sender, FileSavedNotification e)
    {
        MessageManager.Instance.SendMessage(e);
    }

    /// <summary>
    ///     这个方法有时间的话最好重构一下，祖传代码太多冗余了
    /// </summary>
    /// <param name="obj"></param>
    private Task MonitorTaskAsync(object obj)
    {
        if (obj is not CancellationToken token) return Task.CompletedTask;
        var errCnt = 0;
        var timeout = 100;
        while (!token.IsCancellationRequested)
        {
            // 这里进行任务调度
            _schedulerEvent.WaitOne(timeout);
            try
            {
                // 清理任务
                // 1. 已经创建但是超时300秒没有执行的任务
                // 2. 已经开始执行，但是超过30秒没有连接接收数据的任务
                var invalidTasks = _taskCache.Where(i => (i.Value.TaskInfo.RequestState == TaskState.New
                                                          && i.Value.TaskInfo.State == TaskState.New
                                                          && DateTime.Now.Subtract(i.Value.TaskInfo.CreateTime)
                                                              .TotalSeconds > 300
                                                          && string.IsNullOrEmpty(i.Value.TaskInfo.CrondId))
                                                         || i.Value.CanStopTask())
                    .Select(i => i.Value)
                    .ToList();
                if (invalidTasks?.Count > 0)
                {
                    invalidTasks.ForEach(task => ReleaseTask(task.TaskInfo.Id));
                    var count = _taskCache.Count(item => item.Value.TaskInfo.RequestState == TaskState.Start);
                    Trace.WriteLine(
                        $"{DateTime.Now:HH:mm:ss.fff}    CleanTimeOutTask TaskCount:{_taskCache.Count} Running:{count}");
                }

                if (_taskCache.Count < 2)
                    foreach (var pair in _taskCache)
                        pair.Value.IsCompetition = false;
                // 待调度任务集合
                var waitingTasks = _taskCache.Where(i => (i.Value.TaskInfo.RequestState != i.Value.TaskInfo.State
                                                          && i.Value.TaskInfo.ModuleChain.ModuleInfo.State !=
                                                          ModuleState.Fault)
                                                         || i.Value.TaskInfo.RequestState == TaskState.Stop)
                    .Select(i => i.Value)
                    .ToList();
                waitingTasks.Sort();
                // 当前的活跃任务集合
                var tasks = _taskCache.Where(i => i.Value.TaskInfo.State == TaskState.Start
                                                  || i.Value.TaskInfo.State != i.Value.TaskInfo.RequestState)
                    .Select(i => i.Value)
                    .ToList();
                while (waitingTasks.Count > 0)
                {
                    var task = waitingTasks[0];
                    waitingTasks.Remove(task);
                    switch (task.TaskInfo.RequestState)
                    {
                        case TaskState.Start:
                            {
                                var device = Guid.Empty;
                                if (task.TaskInfo.ModuleChain?.Devices is { Count: > 0 })
                                    device = task.TaskInfo.ModuleChain.Devices[0].Instance.Id;
                                var competitors = tasks.Where(item => item.TaskInfo.Id != task.TaskInfo.Id
                                                                      && (item.TaskInfo.State == TaskState.Start ||
                                                                          item.TaskInfo.RequestState == TaskState.Start)
                                                                      && HaveCompetition(item.TaskInfo, task.TaskInfo))
                                    .ToArray();
                                var pause = false;
                                // var max = GetMaxPriority(task.TaskInfo);
                                var stopped = new List<Guid>();
                                var suspended = new List<Guid>();
                                var isCompetitor = competitors.Any();
                                var isTimeShare = false; //时间分片标记
                                if (isCompetitor)
                                {
                                    foreach (var p in competitors) p.IsCompetition = true;
                                    task.IsCompetition = true;
                                    // 这条注释作废：这里的条件可以为等于，不会违反“相同优先级，不可被打断，先到先得”的约束，原因是在创建任务时就已经进行了限制
                                    // 如果争用任务列表中的所有任务都小于或等于当前任务，则将争用的任务全部暂停
                                    // 如果当前任务为独占，则将争用任务中的Level1级任务停止，其他任务暂停
                                    // 如果当前任务为Level1，则将争用任务中的同优先级任务暂停，其他Level1任务停止，其他任务暂停
                                    // if (competitors.All(item => task.TaskInfo.CurrentPriority.CompareTo(item.TaskInfo.CurrentPriority) < 0))
                                    if (competitors.All(item =>
                                            task.TaskInfo.Priority.CompareTo(item.TaskInfo.Priority) <= 0))
                                        switch (task.TaskInfo.Level)
                                        {
                                            case TaskLevel.Level0:
                                                pause = true;
                                                foreach (var item in competitors)
                                                    if (item.TaskInfo.Level == TaskLevel.Level1)
                                                    {
                                                        stopped.Add(item.TaskInfo.Id);
                                                        ReleaseTask(item.TaskInfo.Id);
                                                    }
                                                    else
                                                    {
                                                        // 已经暂定了的任务不再重复暂停
                                                        if (item.TaskInfo.State == TaskState.Start)
                                                        {
                                                            suspended.Add(item.TaskInfo.Id);
                                                            pause &= item.PauseTask();
                                                        }
                                                        // if (item.TaskInfo.State == TaskState.New)
                                                        // {
                                                        //     item.TaskInfo.State = TaskState.Pause;
                                                        // }
                                                    }

                                                break;
                                            case TaskLevel.Level1:
                                                pause = true;
                                                foreach (var item in competitors)
                                                    // 优先级相同则分时
                                                    if (item.TaskInfo.Priority == task.TaskInfo.Priority)
                                                    {
                                                        isTimeShare = true;
                                                        pause &= item.PauseTask();
                                                    }
                                                    else if (item.TaskInfo.Level == TaskLevel.Level1)
                                                    {
                                                        // 同级别的低优先级停止
                                                        stopped.Add(item.TaskInfo.Id);
                                                        ReleaseTask(item.TaskInfo.Id);
                                                    }
                                                    else
                                                    {
                                                        // 其他任务暂停
                                                        // 已经暂定了的任务不再重复暂停
                                                        if (item.TaskInfo.State == TaskState.Start)
                                                        {
                                                            suspended.Add(item.TaskInfo.Id);
                                                            pause &= item.PauseTask();
                                                        }
                                                        // if (item.TaskInfo.State == TaskState.New)
                                                        // {
                                                        //     item.TaskInfo.State = TaskState.Pause;
                                                        // }
                                                    }

                                                break;
                                            case TaskLevel.Level2:
                                                pause = true;
                                                foreach (var item in competitors)
                                                    // 优先级相同则分时
                                                    if (item.TaskInfo.Priority == task.TaskInfo.Priority)
                                                    {
                                                        isTimeShare = true;
                                                        pause &= item.PauseTask();
                                                    }
                                                    // 已经暂定了的任务不再重复暂停
                                                    else if (item.TaskInfo.State == TaskState.Start)
                                                    {
                                                        suspended.Add(item.TaskInfo.Id);
                                                        pause &= item.PauseTask();
                                                    }
                                                    else if (item.TaskInfo.State == TaskState.New)
                                                    {
                                                        // 刚创建的任务不能启动，也需要记录
                                                        suspended.Add(item.TaskInfo.Id);
                                                    }

                                                break;
                                            case TaskLevel.Level3:
                                                pause = true;
                                                foreach (var item in competitors)
                                                {
                                                    isTimeShare = true;
                                                    pause &= item.PauseTask();
                                                }

                                                break;
                                        }
                                    // if (competitors.All(item => task.TaskInfo.CurrentPriority.CompareTo(item.TaskInfo.CurrentPriority) <= 0))
                                    // {
                                    //     pause = competitors.All(t => t.PauseTask());
                                    // }
                                    // else if (task.TaskInfo.Priority != int.MaxValue)
                                    // {
                                    //     foreach (var item in competitors)
                                    //     {
                                    //         if (item.TaskInfo.Priority > 0)
                                    //         {
                                    //             item.TaskInfo.CurrentPriority++;
                                    //         }
                                    //     }
                                    // }
                                }
                                else
                                {
                                    task.IsCompetition = false;
                                    pause = true;
                                    if (task.TaskInfo.State == TaskState.Pause)
                                    {
                                        // 这里暂时不做处理，这样每个任务启动时都会发送消息SDataTaskChangeInfo至云端，等后面看效果
                                    }
                                }

                                if (pause)
                                {
                                    if (!task.StartTask()) ReleaseTask(task.TaskInfo.Id);
                                    // if (task.TaskInfo.Priority > 0 && task.TaskInfo.Priority < int.MaxValue)
                                    // {
                                    //     task.TaskInfo.CurrentPriority++;
                                    //     if (task.TaskInfo.CurrentPriority > max)
                                    //     {
                                    //         task.TaskInfo.CanResetPriority = true;
                                    //     }
                                    // }
                                    var sign = isCompetitor && isTimeShare;
                                    if (!sign)
                                    {
                                        // 闲时任务之间互相时间片切换时不发送消息，否则消息太多
                                        var msg = new SDataTaskChangeInfo
                                        {
                                            ActiveTask = task.TaskInfo.Id,
                                            DeviceId = device,
                                            StoppedTasks = stopped,
                                            SuspendedTasks = suspended
                                        };
                                        MessageManager.Instance.SendMessage(msg);
                                        Trace.WriteLine(
                                            $"任务[{msg.ActiveTask}]启动，任务[{string.Join(",", msg.StoppedTasks)}]停止，任务[{string.Join(",", msg.SuspendedTasks)}]暂停");
                                    }
                                }

                                // ResetPriority(task.TaskInfo);
                                // 移除待调度任务集合中的所有争用的任务
                                waitingTasks.RemoveAll(item => HaveCompetition(item.TaskInfo, task.TaskInfo));
                            }
                            break;
                        case TaskState.Stop:
                            ReleaseTask(task.TaskInfo.Id);
                            var count = _taskCache.Count(item => item.Value.TaskInfo.RequestState == TaskState.Start);
                            Trace.WriteLine(
                                $"{DateTime.Now:HH:mm:ss.fff}    DeleteTask Feature={task.Feature} TaskID={task.TaskInfo.Id} DriverID={task.TaskInfo.ModuleId} TaskCount:{_taskCache.Count} Running:{count}");
                            break;
                    }
                }

                // 若待启动及已启动的任务所使用设备存在竞争，则设置调度操作等时间为 100ms
                // 如果待调度队列中有多个不同的设备，则等待50ms继续调度，否则直接阻塞线程直到有新的任务到达
                // 最新修改：由于需要利用这个线程清理不用的任务，因此不再阻塞线程
                timeout = 100; //(devices.Select(item => item.Id).Distinct().Count() < devices.Count()) ? 100 : -1;
                tasks.Clear();
                waitingTasks.Clear();
            }
            catch (Exception ex)
            {
                if (errCnt % 600 == 0) Trace.WriteLine($"任务调度错误!{ex.Message}");
                errCnt++;
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     判断两个任务是否存在设备竞争关系
    /// </summary>
    /// <param name="task1"></param>
    /// <param name="task2"></param>
    internal static bool HaveCompetition(TaskInfo task1, TaskInfo task2)
    {
        return task1.ModuleChain.Devices.Select(item => item.Instance)
            .Intersect(task2.ModuleChain.Devices.Select(item => item.Instance))
            .Any();
    }

    /// <summary>
    ///     判断任务集合中是否有跟本任务有争用的任务
    /// </summary>
    /// <param name="task"></param>
    internal bool HaveCompetition(TaskInfo task)
    {
        foreach (var pair in _taskCache)
        {
            if (pair.Key == task.Id) continue;
            if (HaveCompetition(pair.Value.TaskInfo, task)) return true;
        }

        return false;
    }

    /// <summary>
    ///     查询某个功能是否已经有任务在运行
    /// </summary>
    /// <param name="driverId">功能ID</param>
    private bool IsDriverRunning(Guid driverId)
    {
        foreach (var pair in _taskCache)
            if (pair.Value != null && pair.Value.TaskInfo.ModuleId == driverId
                                   && pair.Value.TaskInfo.RequestState == TaskState.Start)
                return true;
        return false;
    }

    private List<TaskInstance> FindTaskInfoBy(Guid deviceId)
    {
        var list = new List<TaskInstance>();
        foreach (var pair in _taskCache)
            if (pair.Value?.TaskInfo?.ModuleChain?.Devices.Any(d => d.ModuleInfo.Id == deviceId) == true)
                list.Add(pair.Value);
        return list;
    }

    /*
        根据功能类型以一定的规则查询空闲的功能信息
        这里以后可能会改
        云端下发的计划任务只会下发功能类型，不会指定特定的功能模块
        因此这里需要根据功能类型自己找到合适的功能模块来执行功能
        目前的条件为：
            1. 空闲的功能
            2. 支持所有的保存数据？
        如果以上的所有条件全不符合或者不全符合则直接取第一个功能
    */
    private ModuleInfo FindDriverByRule(FeatureType feature)
    {
        var drivers = DriverConfig.Instance.Drivers.Where(d => (d.Feature & feature) > 0
                                                               && d.State != ModuleState.Disabled
                                                               && d.State != ModuleState.Fault);
        var moduleInfos = drivers as ModuleInfo[] ?? drivers.ToArray();
        if (moduleInfos.Any() != true) return null;
        // TODO : 后面的迭代需要完善规则
        // 取功能的规则是？
        // 目前暂时取符合条件的功能集合中空闲的第一个功能，如果全都在任务中则直接取第一个
        var idleDrivers = moduleInfos.Where(d => !IsDriverRunning(d.Id));
        var enumerable = idleDrivers as ModuleInfo[] ?? idleDrivers.ToArray();
        return enumerable.Any() != true ? moduleInfos.First() : enumerable.First();
    }

    #region 优先级相关

    ///// <summary>
    /////     获取本任务有争用的所有任务的优先级总和
    ///// </summary>
    ///// <param name="task"></param>
    //private int GetMaxPriority(TaskInfo task)
    //{
    //    var competitionTasks = _taskCache.Values.Where(item => item.TaskInfo.RequestState != TaskState.Stop
    //                                                           && HaveCompetition(task, item.TaskInfo)
    //                                                           && item.TaskInfo.Priority != int.MaxValue);
    //    var taskInstances = competitionTasks as TaskInstance[] ?? competitionTasks.ToArray();
    //    if (taskInstances.Any())
    //    {
    //        var max = taskInstances.Max(item => item.TaskInfo.Priority);
    //        var sb = new StringBuilder("    ");
    //        foreach (var item in taskInstances)
    //            sb.Append(item.TaskInfo.Id.ToString()).Append(',').Append(item.TaskInfo.Priority).Append(',')
    //                .Append(item.TaskInfo.Priority).Append("    ");
    //        Console.WriteLine(sb.ToString());
    //        return max;
    //    }

    //    return 0;
    //}

    // /// <summary>
    // /// 重置所有任务的优先级
    // /// </summary>
    // /// <param name="task"></param>
    // private void ResetPriority(TaskInfo task)
    // {
    //     var competitionTasks = _taskCache.Values.Where(item => item.TaskInfo.RequestState != TaskState.Stop
    //                                                         && HaveCompetition(task, item.TaskInfo)
    //                                                         && item.TaskInfo.Priority != int.MaxValue);
    //     if (competitionTasks.Any() && competitionTasks.All(item => item.TaskInfo.CanResetPriority))
    //     {
    //         foreach (var item in competitionTasks)
    //         {
    //             // Console.WriteLine($"重置优先级:{item.TaskInfo.CurrentPriority}=>{item.TaskInfo.Priority}");
    //             item.TaskInfo.Priority = item.TaskInfo.Priority;
    //             // item.TaskInfo.CanResetPriority = false;
    //         }
    //     }
    // }
    /// <summary>
    ///     创建任务时判断任务是否可以创建
    /// </summary>
    /// <param name="moduleId"></param>
    /// <param name="priority"></param>
    /// <returns>true: 可以创建;false: 可以创建但是不能启动</returns>
    private void CheckTaskCanStart(Guid moduleId, int priority)
    {
        var module = DriverConfig.Instance.FindDriverById(moduleId);
        if (module == null)
        {
            var exception = new LocalRpcException($"未找到模块ID为{moduleId}的功能")
            {
                ErrorCode = ErrorCode.ErrorCodeModuleNotFound
            };
            throw exception;
        }

        var parameters = module.Parameters?.Where(item => item.IsInstallation && item.IsPrimaryDevice).ToList();
        if (parameters != null)
            foreach (var item in parameters)
            {
                if (item.Value == null || !Guid.TryParse(item.Value.ToString(), out var id)) continue;
                var device = DeviceConfig.Instance.GetDeviceName(id);
                var list = FindTaskInfoBy(id);
                if (list == null || list.Count == 0) continue;
                var level = TaskHelper.GetTaskLevel(priority);
                switch (level)
                {
                    case TaskLevel.Level0:
                        {
                            var task = list.Find(taskInstance =>
                                taskInstance.TaskInfo.Priority <= priority &&
                                taskInstance.TaskInfo.RequestState != TaskState.Stop);
                            if (task is not null)
                            {
                                var exception =
                                    new LocalRpcException($"功能所依赖的设备{device}已有同级别或更高级别的任务{task.TaskInfo.Feature}在运行!")
                                    {
                                        ErrorCode = ErrorCode.ErrorCodeTaskCreateFail
                                    };
                                throw exception;
                            }
                        }
                        break;
                    case TaskLevel.Level1:
                        {
                            var task = list.Find(taskInstance =>
                                taskInstance.TaskInfo.Priority < priority &&
                                taskInstance.TaskInfo.RequestState != TaskState.Stop);
                            if (task is not null)
                            {
                                var exception =
                                    new LocalRpcException($"功能所依赖的设备{device}已有更高级别的任务{task.TaskInfo.Feature}在运行!")
                                    {
                                        ErrorCode = ErrorCode.ErrorCodeTaskCreateFail
                                    };
                                throw exception;
                            }
                        }
                        break;
                    case TaskLevel.Level2:
                        {
                            // var task = list.Find(item => item.TaskInfo.Priority == priority && item.TaskInfo.RequestState != TaskState.Stop);
                            // if (task is not null)
                            // {
                            //     var exception = new LocalRpcException($"功能所依赖的设备{device}已有同级别的任务{task.TaskInfo.Feature}在运行!")
                            //     {
                            //         ErrorCode = ErrorCode.ErrorCode_Task_CreateFail
                            //     };
                            //     throw exception;
                            // }
                            var tmp = list.Where(taskInstance =>
                                taskInstance.TaskInfo.Priority < priority &&
                                taskInstance.TaskInfo.RequestState != TaskState.Stop);
                            var taskInstances = tmp as TaskInstance[] ?? tmp.ToArray();
                            if (taskInstances.Length > 0)
                            {
                                var maxPriority = taskInstances.Max(taskInstance => taskInstance.TaskInfo.Priority);
                                var maxTask = taskInstances.FirstOrDefault(taskInstance =>
                                    taskInstance.TaskInfo.Priority == maxPriority);
                                if (maxTask is not null)
                                    // if (GetTaskLevel(maxPriority) == TaskLevel.Level1)
                                    // {
                                    //     var exception = new LocalRpcException($"功能所依赖的设备{device}已有更高别的任务{maxTask.TaskInfo.Feature}在运行!")
                                    //     {
                                    //         ErrorCode = ErrorCode.ErrorCode_Task_CreateFail
                                    //     };
                                    //     throw exception;
                                    // }
                                    return;
                            }
                        }
                        break;
                    case TaskLevel.Level3:
                        break;
                }
            }
    }

    #endregion
}