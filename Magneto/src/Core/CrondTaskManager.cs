using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Configuration;
using Core.Define;
using Magneto.Contract;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using Quartz.Spi;

namespace Core;

/// <summary>
///     后面考虑时间结束后清理资源
/// </summary>
public sealed class CrondTaskManager
{
    private static readonly Lazy<CrondTaskManager> _lazy = new(() => new CrondTaskManager());

    /// <summary>
    ///     计划缓存，统一管理计划
    /// </summary>
    private readonly ConcurrentDictionary<string, BatchedCrondInfo> _planCache = new();

    private List<BatchedCrondInfo> _tasks;

    private CrondTaskManager()
    {
    }

    public static CrondTaskManager Instance => _lazy.Value;

    public async Task InitializedAsync()
    {
        // 初始化时运行本地已经存在的计划
        // 连接云端以后再更新计划
        // string str = await CloudClient.Instance.GetCrondTasksAsync(StationConfig.Instance.Station.EdgeID, GetLastUpdateTime(localTasks)).ConfigureAwait(false);
        // if (string.IsNullOrEmpty(str))
        // {
        //     localTasks = null;
        //     return;
        // }
        // var cloudTasks = JsonConvert.DeserializeObject<BatchedCrondInfo[]>(str);
        // if (cloudTasks == null)
        // {
        //     localTasks = null;
        //     return;
        // }
        _tasks = GetLocalTasks();
        var now = DateTime.Now;
        //有效计划
        if (_tasks == null || _tasks.Count == 0) return;
        _tasks = _tasks.Where(t => t.Status.Equals("0") && DateTime.Parse(t.ExpireTime) > now).ToList();
        // SaveData(_tasks);
        foreach (var task in _tasks)
        {
            if (!task.Rules.Exists(item => item.Executors?.Exists(e => e == RunningInfo.EdgeId) == true)) continue;
            _planCache.TryAdd(task.BatchId, task);
            await CreateCrondTaskAsync(task).ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     计划任务改变
    /// </summary>
    public async Task OnCrondTaskChangedAsync()
    {
        //1.如果云端为空，则清理所有Job
        //2.如果云端不为空，则比较status
        //  如果status为0，判断是否已经存在Job，不存在则创建，如果已经存在并且为暂停状态，则恢复
        //  如果status为1，判断是否还存在Job，存在则删除Job、StopJob并清除所有涉及的Task
        //  如果status为2，删除所有的Job和StopJob，清除BatchID涉及的所有Task
        //  如果status为3，则暂停计划
        //3.如果云端删除了某个计划任务，同样需要清理对应的计划
        var localTasks = GetLocalTasks();
        var toSavedList = new List<BatchedCrondInfo>();
        // string str = await CloudClient.Instance.GetCrondTasksAsync(StationConfig.Instance.Station.EdgeID, GetLastUpdateTime(localTasks)).ConfigureAwait(false);
        var str = await CloudClient.Instance.GetCrondTasksAsync(StationConfig.Instance.Station.EdgeId, null)
            .ConfigureAwait(false);
        if (string.IsNullOrEmpty(str)) return;
        var cloudTasks = JsonConvert.DeserializeObject<BatchedCrondInfo[]>(str);
        if (cloudTasks == null)
        {
            var taskIds = await TriggerServier.GetTaskIDsAsync("").ConfigureAwait(false); //要先获取taskid再删除job，下面代码需要加锁
            await TriggerServier.ClearAllJobsAsync().ConfigureAwait(false);
            taskIds.AddRange(await TriggerServier.GetTaskIDsAsync("").ConfigureAwait(false));
            _planCache.Clear();
            StopTasks(taskIds);
            return;
        }

        foreach (var item in cloudTasks)
            //正常
            if (item.Status == "0")
            {
                toSavedList.Add(item);
                if (!await TriggerServier.ExistsGroupAsync(item.BatchId).ConfigureAwait(false))
                {
                    _planCache.AddOrUpdate(item.BatchId, _ => item, (_, _) => item);
                    await CreateCrondTaskAsync(item).ConfigureAwait(false); //新增加
                }
                else
                {
                    if (_planCache.TryGetValue(item.BatchId, out var info)) info.Status = item.Status;
                    //修改现有属性
                    //FIXME 是不是会存在修改Rule规则或duration、DataStorage等属性的情况，待商榷
                    // 判断当前计划是否暂停中，如果在暂停则恢复计划
                    await ResumeCrondAsync(item).ConfigureAwait(false);
                }
            }
            //过期 || 撤销
            else if (item.Status is "1" or "2")
            {
                _planCache.Remove(item.BatchId, out _);
                await DeleteCrondServiceAsync(item).ConfigureAwait(false);
            }
            else if (item.Status == "3")
            {
                // 暂停计划
                if (_planCache.TryGetValue(item.BatchId, out var info)) info.Status = item.Status;
                await PauseCrondAsync(item).ConfigureAwait(false);
            }
            else
            {
                // TODO dosomething interesting
                // 放错
                _planCache.Remove(item.BatchId, out _);
            }

        if (localTasks != null)
            foreach (var item in localTasks)
            {
                if (cloudTasks.ToList().Exists(p => p.BatchId == item.BatchId)) continue;
                // 如果云端查询的计划中不包含本地计划，则将本地的对应计划停止
                _planCache.Remove(item.BatchId, out _);
                await DeleteCrondServiceAsync(item).ConfigureAwait(false);
            }

        SaveData(toSavedList);
    }

    /// <summary>
    ///     更新计划信息
    /// </summary>
    /// <param name="batchId"></param>
    /// <param name="taskId"></param>
    /// <param name="state"></param>
    /// <param name="msg"></param>
    public void UpdatePlanInfo(string batchId, string taskId, TaskState state, string msg)
    {
        if (!_planCache.TryGetValue(batchId, out var info)) return;
        var rule = info.Rules.Find(item => item.TaskId == taskId);
        if (rule == null) return;
        rule.State = state;
        rule.Message = msg;
    }

    /// <summary>
    ///     临时更新计划信息
    /// </summary>
    /// <param name="batchId"></param>
    /// <param name="tempId"></param>
    /// <param name="taskId"></param>
    /// <param name="state"></param>
    /// <param name="msg"></param>
    public void UpdatePlanInfo(string batchId, string tempId, string taskId, TaskState state, string msg)
    {
        if (!_planCache.TryGetValue(batchId, out var info)) return;
        var rule = info.Rules.Find(item => item.TempId == tempId);
        if (rule == null) return;
        rule.TaskId = taskId;
        rule.State = state;
        rule.Message = msg;
    }

    /// <summary>
    ///     根据功能查询
    /// </summary>
    /// <param name="feature"></param>
    /// <param name="tempId">需要排除的tempID</param>
    public RuleInfo GetPlanFeature(FeatureType feature, string tempId)
    {
        foreach (var pair in _planCache)
        {
            if (pair.Value.Status != "0") continue;
            var rule = pair.Value.Rules.Find(item => item.Feature == feature);
            if (rule == null) continue;
            if (!string.IsNullOrEmpty(tempId) && rule.TempId == tempId) continue;
            if (rule.State == TaskState.Stop) continue;
            return rule;
        }

        return null;
    }

    public List<BatchedCrondInfo> GetPlanList()
    {
        return _planCache.Select(item => item.Value).ToList();
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    /// <param name="taskIds"></param>
    private static void StopTasks(List<string> taskIds)
    {
        if (taskIds == null) return;
        foreach (var taskId in taskIds)
            try
            {
                TaskManager.Instance.StopTask(new Guid(taskId));
            }
            catch
            {
                //TODO 记录日志
            }
    }

    /// <summary>
    ///     删除计划
    /// </summary>
    /// <param name="info"></param>
    public async Task DeleteCrondServiceAsync(BatchedCrondInfo info)
    {
        if (info == null) return;
        var groupName = info.BatchId;
        var taskIds = await TriggerServier.GetTaskIDsAsync(groupName).ConfigureAwait(false); //要先获取taskid再删除job，下面代码需要加锁
        if (await TriggerServier.ExistsGroupAsync(groupName).ConfigureAwait(false))
            await TriggerServier.ClearJobsWhileGroupEndsWithAsync(groupName).ConfigureAwait(false);
        taskIds.AddRange(await TriggerServier.GetTaskIDsAsync(groupName).ConfigureAwait(false));
        StopTasks(taskIds);
    }

    public async Task PauseCrondAsync(BatchedCrondInfo info)
    {
        if (info == null) return;
        var groupName = info.BatchId;
        if (await TriggerServier.ExistsGroupAsync(groupName).ConfigureAwait(false))
            await TriggerServier.PauseJobGroupAsync(groupName).ConfigureAwait(false);
        var taskIds = await TriggerServier.GetTaskIDsAsync(groupName).ConfigureAwait(false); //要先获取taskid再删除job，下面代码需要加锁
        StopTasks(taskIds);
    }

    public async Task ResumeCrondAsync(BatchedCrondInfo info)
    {
        if (info == null) return;
        var groupName = info.BatchId;
        if (await TriggerServier.ExistsGroupAsync(groupName).ConfigureAwait(false))
            await TriggerServier.ResumeJobGroupAsync(groupName).ConfigureAwait(false);
    }

    /// <summary>
    ///     创建计划
    /// </summary>
    /// <param name="info"></param>
    private static async Task CreateCrondTaskAsync(BatchedCrondInfo info)
    {
        if (info.Crons == null || info.Crons.Count == 0) return;
        if (info.Rules == null || info.Rules.Count == 0) return;
        var scheduler = await TriggerServier.GetSchedulerAsync().ConfigureAwait(false);
        // if (!DateTimeOffset.TryParse(info.EffectiveTime, out DateTimeOffset startTime))
        // {
        //     startTime = DateTime.Now;
        // }
        if (!DateTimeOffset.TryParse(info.ExpireTime, out var expireTime)) expireTime = DateTime.MaxValue;
        if (!DateTimeOffset.TryParse(info.EffectiveTime, out var startTime)) startTime = DateTime.Now;
        var job = JobBuilder.Create<StartJob>()
            .WithIdentity($"StartJob_{info.Name}", info.BatchId)
            .RequestRecovery(false)
            .SetJobData(new JobDataMap
            {
                new("TaskName", info.Name),
                new("BatchID", info.BatchId)
            }) //属性自动赋值
            .Build();
        IList<ITrigger> lstTrigger = new List<ITrigger>();
        for (var i = 0; i < info.Crons.Count; i++)
        {
            var cron = info.Crons[i];
            // cron = "0 0/1 0/1 * * ? *";
            var isIdleTask = string.IsNullOrEmpty(cron);
            if (isIdleTask)
                // 闲时任务的cron表达式为空字符串
                cron = "0 0 0 * * ? *";
            if (!CronExpression.IsValidExpression(cron)) continue;
            foreach (var item in info.Rules)
            {
                var index = item.Executors.IndexOf(RunningInfo.EdgeId);
                if (index < 0) continue;
                var moduleId = string.Empty;
                if (item.ExecuteModules != null && item.ExecuteModules.Count > index)
                    moduleId = item.ExecuteModules[index];
                long duration;
                if (isIdleTask)
                {
                    duration = 86400;
                }
                else
                {
                    if (item.Duration == null || item.Duration.Count < i + 1) continue;
                    duration = item.Duration[i];
                }

                var identityKey = new StringBuilder();
                identityKey.Append("StartTrigger_");
                identityKey.Append(item.Feature);
                if (lstTrigger.Count != 0)
                {
                    identityKey.Append('_');
                    identityKey.Append(lstTrigger.Count);
                }

                // var startTime = DateTime.Now.Subtract(TimeSpan.FromHours(1));
                IOperableTrigger trigger = new CronTriggerImpl(identityKey.ToString(), info.BatchId, cron)
                {
                    StartTimeUtc = startTime,
                    EndTimeUtc = expireTime,
                    MisfireInstruction = MisfireInstruction.CronTrigger.DoNothing
                };
                var priority = isIdleTask ? 999 : item.Priority;
                var id = Guid.NewGuid().ToString();
                item.TempId = id;
                trigger.JobDataMap.Add("IsIdle", isIdleTask);
                trigger.JobDataMap.Add("TempID", id);
                trigger.JobDataMap.Add("Priority", priority);
                trigger.JobDataMap.Add("Duration", duration);
                trigger.JobDataMap.Add("Feature", item.Feature);
                trigger.JobDataMap.Add("DataStorage", item.DataStorage);
                trigger.JobDataMap.Add("Parameters", item.Parameters);
                trigger.JobDataMap.Add("ModuleId", moduleId);
                trigger.JobDataMap.Add("ExpireTime", expireTime);
                if (isIdleTask) trigger.MisfireInstruction = MisfireInstruction.CronTrigger.FireOnceNow;
                trigger.GetFireTimeAfter(DateTimeOffset.UtcNow);
                if (TriggerUtils.ComputeEndTimeToAllowParticularNumberOfFirings(trigger, null, 1) != null)
                    lstTrigger.Add(trigger);
                //TODO cron表达式正确但是不会被触发
            }
        }

        if (lstTrigger.Count == 0)
            //TODO do something
            return;
        try
        {
            await scheduler.Start().ConfigureAwait(false);
            await scheduler.ScheduleJob(job, new ReadOnlyCollection<ITrigger>(lstTrigger), true).ConfigureAwait(false);
            Trace.WriteLine($"Create批号:{info.BatchId}");
        }
        catch
        {
            //TODO do something
        }
    }

    /// <summary>
    ///     保存计划任务到本地
    /// </summary>
    /// <param name="tasks"></param>
    private static void SaveData(List<BatchedCrondInfo> tasks)
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigCrondtask);
        File.WriteAllText(path, JsonConvert.SerializeObject(tasks, Formatting.Indented));
    }

    /// <summary>
    ///     读取本地计划任务
    /// </summary>
    private static List<BatchedCrondInfo> GetLocalTasks()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) return null;
        var path = Path.Combine(dir, PublicDefine.FileConfigCrondtask);
        if (!File.Exists(path)) return null;
        try
        {
            var str = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<BatchedCrondInfo>>(str);
        }
        catch (Exception e)
        {
            Trace.WriteLine(e);
            return null;
        }
    }

    private class CrondTaskInfoEqualityComparer : IEqualityComparer<BatchedCrondInfo>
    {
        public bool Equals(BatchedCrondInfo t1, BatchedCrondInfo t2)
        {
            if (t1 == null || t2 == null) return false;
            return t1.BatchId.Equals(t2.BatchId);
        }

        public int GetHashCode(BatchedCrondInfo t)
        {
            return t.BatchId.GetHashCode();
        }
    }
}

/// <summary>
///     开始任务
/// </summary>
public class StartJob : IJob
{
    /// <summary>
    ///     是否闲时任务
    /// </summary>
    public bool IsIdle { set; get; }

    public int Priority { set; get; }
    public double Duration { set; get; }
    public string TaskName { get; set; }
    public string BatchId { get; set; }
    public string TempId { get; set; }
    public string ModuleId { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    public FeatureType Feature { get; set; }

    public MediaType DataStorage { get; set; }

    // 闲时任务时需要一直持续到有效期结束
    public DateTimeOffset ExpireTime { get; set; }
    public Task Execute(IJobExecutionContext context)
    {
        //todo 判断有效性
        return Task.Run(async () =>
        {
            Guid id;
            var saveType = DataStorage;
            if (Feature == FeatureType.None) return;
            // if (saveType == MediaType.NONE)
            // {
            //     return;
            // }
            if (Feature == FeatureType.Avicg)
            {
                var rule = CrondTaskManager.Instance.GetPlanFeature(FeatureType.Avicg, TempId);
                if (rule != null)
                {
                    CrondTaskManager.Instance.UpdatePlanInfo(BatchId, TempId, "", TaskState.Stop, "已有航空监测在运行中");
                    Console.WriteLine("已有航空监测在运行中");
                    await Task.Delay(10000, context.CancellationToken).ConfigureAwait(false);
                    await context.JobInstance.Execute(context).ConfigureAwait(false);
                    return;
                }
            }

            if (!RunningInfo.CloudClientState)
            {
                CrondTaskManager.Instance.UpdatePlanInfo(BatchId, TempId, "", TaskState.Stop, "云端未连接");
                Console.WriteLine("云端未连接...重新执行脚本");
                await Task.Delay(5000, context.CancellationToken).ConfigureAwait(false);
                await context.JobInstance.Execute(context).ConfigureAwait(false);
                return;
            }

            Console.WriteLine("云端状态正常，开始执行计划脚本...");
            PresetTask(out var feature, out var dic);
            var remark = "";
            var needSaveData = false;
            if (dic.Count > 0)
            {
                needSaveData = true;
                dic.Add("needSaveData", true);
                remark = JsonConvert.SerializeObject(dic);
            }

            if (feature == FeatureType.Amia) Priority = 100;
            try
            {
                id = TaskManager.Instance.CreateTask(feature, ModuleId, saveType, BatchId, "", TaskName, remark,
                    Priority);
                CrondTaskManager.Instance.UpdatePlanInfo(BatchId, TempId, id.ToString(), TaskState.New, "任务创建成功");
            }
            catch (Exception ex)
            {
                var str = $"创建任务失败{DateTime.Now:yyyy-MM-dd HH:mm:ss} {ex.Message}";
                CrondTaskManager.Instance.UpdatePlanInfo(BatchId, TempId, "", TaskState.Stop, str);
                Console.WriteLine(str);
                // TODO : 这里先暂时这样处理吧，没办法了，等以后重构的时候再优化 -_-
                if (ex is NullReferenceException && ex.Message == "找不到合适的功能模块来执行任务") return;
                await Task.Delay(10000, context.CancellationToken).ConfigureAwait(false);
                await context.JobInstance.Execute(context).ConfigureAwait(false);
                return;
            }

            try
            {
                var task = TaskManager.Instance.GetTask(id);
                // 处理参数
                var parameters = new List<Parameter>();
                foreach (var pair in Parameters)
                {
                    var info = new Parameter
                    {
                        Name = pair.Key
                    };
                    if (pair.Key is ParameterNames.ScanSegments or ParameterNames.MscanPoints
                        or ParameterNames.DdcChannels or ParameterNames.Antennas)
                    {
                        var str = pair.Value.ToString();
                        var dArray = JsonConvert.DeserializeObject<Dictionary<string, object>[]>(str!);
                        var array = new Dictionary<string, object>[dArray.Length];
                        for (var i = 0; i < array.Length; i++)
                        {
                            var tmp = new Dictionary<string, object>();
                            foreach (var dp in dArray[i])
                            {
                                if (dp.Key == "segmentType") continue;
                                tmp.Add(dp.Key, dp.Value);
                            }

                            array[i] = tmp;
                        }

                        info.Parameters = [];
                        info.Parameters.AddRange(array);
                    }
                    else
                    {
                        info.Value = pair.Value;
                    }

                    parameters.Add(info);
                }

                var antPara = parameters.Find(item => item.Name == ParameterNames.Polarization);
                if (antPara != null)
                {
                    var freq = 0d;
                    var freqPara = parameters.Find(item =>
                        item.Name is ParameterNames.Frequency or ParameterNames.StartFrequency);
                    if (freqPara != null && double.TryParse(freqPara.Value.ToString(), out var db)) freq = db;
                    var polarization = Magneto.Contract.Utils.ConvertStringToEnum<Polarization>(antPara.Value.ToString());
                    var antId = task?.GetAntennaIdByPolarization(freq, polarization);
                    var para = new Parameter
                    {
                        Name = ParameterNames.AntennaId,
                        Value = antId
                    };
                    parameters.Add(para);
                }

                if (Feature == FeatureType.Avicg)
                {
                    var para = new Parameter
                    {
                        Name = "autoChannelMode",
                        Value = true
                    };
                    parameters.Add(para);
                }
                else if (Feature == FeatureType.Amia)
                {
                    var para = new Parameter
                    {
                        Name = "saveStatData",
                        Value = true
                    };
                    parameters.Add(para);
                    var seg = new Parameter
                    {
                        Name = ParameterNames.ScanSegments,
                        Parameters = []
                    };
                    var array = new Dictionary<string, object>[1];
                    var list = parameters.Where(item =>
                        item.Name is ParameterNames.StartFrequency or ParameterNames.StopFrequency
                            or ParameterNames.StepFrequency).ToList();
                    Dictionary<string, object> tmp = new();
                    foreach (var item in list)
                    {
                        tmp.Add(item.Name, item.Value);
                        parameters.Remove(item);
                    }

                    array[0] = tmp;
                    seg.Parameters.AddRange(array);
                    parameters.Add(seg);
                }
                else if (Feature is FeatureType.Report or FeatureType.SignalCensus)
                {
                    // 月报扫描与信号普查添加统计开关
                    var max = new Parameter
                    {
                        Name = ParameterNames.MaximumSwitch,
                        Value = true
                    };
                    var min = new Parameter
                    {
                        Name = ParameterNames.MinimumSwitch,
                        Value = true
                    };
                    var avg = new Parameter
                    {
                        Name = ParameterNames.MeanSwitch,
                        Value = true
                    };
                    parameters.Add(max);
                    parameters.Add(min);
                    parameters.Add(avg);
                }

                if (!needSaveData)
                {
                    var para = new Parameter
                    {
                        Name = ParameterNames.RawSwitch,
                        Value = 15
                    };
                    parameters.Add(para);
                }

                task?.SetParameters(parameters);
                TaskManager.Instance.StartTask(id);
                CrondTaskManager.Instance.UpdatePlanInfo(BatchId, id.ToString(), TaskState.Start, "计划启动");
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"启动任务失败...{ex.Message}");
                TaskManager.Instance.StopTask(id);
                id = Guid.Empty;
            }

            if (Equals(id, Guid.Empty))
            {
                //TODO 记录日志并返回，当前任务下发失败
                Trace.WriteLine($"启动任务失败{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await Task.Delay(10000, context.CancellationToken).ConfigureAwait(false);
                await context.JobInstance.Execute(context).ConfigureAwait(false);
                return;
            }

            if (TaskHelper.GetTaskLevel(Priority) == TaskLevel.Level3)
            {
                if (ExpireTime == DateTimeOffset.MaxValue) ExpireTime = DateTimeOffset.Now.AddDays(30);
                // 闲时任务
                Duration = (long)ExpireTime.ToLocalTime().AddMinutes(-10).Subtract(DateTime.Now).TotalSeconds;
            }

            Trace.WriteLine(
                $"Create批号:{BatchId}_任务{id},{Priority},duration:{Duration},DateTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            //创建任务
            await TriggerServier.CreateStopTaskTriggerAsync(id, BatchId, TimeSpan.FromSeconds(Duration))
                .ConfigureAwait(false);
        });
    }

    private void PresetTask(out FeatureType feature, out Dictionary<string, object> dic)
    {
        feature = Feature;
        dic = new Dictionary<string, object>();
        if (Feature is FeatureType.Report or FeatureType.SignalCensus)
        {
            feature = FeatureType.Scan;
            FileDataType type;
            if (Feature == FeatureType.SignalCensus)
            {
                Console.WriteLine("准备启动信号普查任务");
                dic.Add("interval", Parameters.TryGetValue("interval", out var parameter) ? parameter : 1000);
                dic.Add("compress", true);
                type = FileDataType.SignalCensus;
            }
            else
            {
                Console.WriteLine("准备启动月报扫描任务");
                dic.Add("compress", true);
                dic.Add("interval", 15 * 60 * 1000);
                type = FileDataType.Report;
            }

            dic.Add("fileType", type);
        }
        else if (Feature == FeatureType.Avicg)
        {
            feature = FeatureType.Ifmca;
        }
        else if (TaskName.Contains("月报扫描"))
        {
            // 临时
            Console.WriteLine("准备启动月报扫描任务");
            dic.Add("compress", true);
            dic.TryAdd("interval", 15 * 60 * 1000);
            dic.TryAdd("fileType", FileDataType.Report);
        }
        else if (TaskName.Contains("信号普查"))
        {
            // 临时
            Console.WriteLine("准备启动信号普查任务");
            dic.Add("compress", true);
            dic.TryAdd("interval", 1000);
            dic.TryAdd("fileType", FileDataType.SignalCensus);
        }
    }
}

/// <summary>
///     停止任务
/// </summary>
public class StopJob : IJob
{
    public string TaskId { get; set; }
    public string BatchId { get; set; }
    public Task Execute(IJobExecutionContext context)
    {
        return Task.Run(async () =>
        {
            try
            {
                Trace.WriteLine($"开始停止计划:{TaskId},{BatchId}");
                TaskManager.Instance.StopTask(new Guid(TaskId));
                Trace.WriteLine($"Delete批号:{BatchId}_任务{TaskId},DateTime:{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await (await TriggerServier.GetSchedulerAsync().ConfigureAwait(false))
                    .DeleteJob(new JobKey($"StopJob_{TaskId}", BatchId)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //TODO 记录日志
                Trace.WriteLine($"{TaskId} 删除失败:{ex.Message}");
                //需要重新创建一个触发器
            }
        });
    }
}

public static class TriggerServier
{
    public static async Task<IScheduler> GetSchedulerAsync()
    {
        var factory = new StdSchedulerFactory();
        var scheduler = await factory.GetScheduler().ConfigureAwait(false);
        return await Task.FromResult(scheduler).ConfigureAwait(false);
    }

    public static async Task ClearAllJobsAsync()
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJobs(tempInfos).ConfigureAwait(false);
        if (deleteResult) await ShutDownSchedulerAsync().ConfigureAwait(false); //关闭
    }

    public static async Task<bool> ClearJobsWhileGroupEqualsAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJobs(tempInfos).ConfigureAwait(false);
        return deleteResult;
    }

    public static async Task<bool> ClearJobsWhileGroupContainsAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupContains(groupName)).ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJobs(tempInfos).ConfigureAwait(false);
        return deleteResult;
    }

    public static async Task ClearJobsWhileGroupEndsWithAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEndsWith(groupName)).ConfigureAwait(false);
        await scheduler.DeleteJobs(tempInfos).ConfigureAwait(false);
    }

    public static async Task<bool> ClearJobsWhileGroupStartsWithAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupStartsWith(groupName))
            .ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJobs(tempInfos).ConfigureAwait(false);
        return deleteResult;
    }

    public static async Task<bool> ClearJobAsync(string jobName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJob(new JobKey(jobName)).ConfigureAwait(false);
        return deleteResult;
    }

    public static async Task<bool> ClearJobAsync(string name, string group)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var deleteResult = await scheduler.DeleteJob(new JobKey(name, group)).ConfigureAwait(false);
        return deleteResult;
    }

    public static async Task PauseJobGroupAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        await scheduler.PauseJobs(GroupMatcher<JobKey>.GroupEndsWith(groupName)).ConfigureAwait(false);
        var tempInfos = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEndsWith(groupName))
            .ConfigureAwait(false);
        List<string> list = [];
        foreach (var item in tempInfos)
        {
            var state = await scheduler.GetTriggerState(item);
            list.Add($"{item.Name},{state}");
        }

        Console.WriteLine($"计划{groupName}当前状态:{string.Join(";", list)}");
    }

    public static async Task ResumeJobGroupAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        await scheduler.ResumeJobs(GroupMatcher<JobKey>.GroupEndsWith(groupName)).ConfigureAwait(false);
        var tempInfos = await scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEndsWith(groupName))
            .ConfigureAwait(false);
        List<string> list = [];
        foreach (var item in tempInfos)
        {
            var state = await scheduler.GetTriggerState(item);
            list.Add($"{item.Name},{state}");
        }

        Console.WriteLine($"计划{groupName}当前状态:{string.Join(";", list)}");
    }

    public static async Task ShutDownSchedulerAsync()
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        if (scheduler.IsShutdown) return;
        await scheduler.Shutdown().ConfigureAwait(false);
    }

    public static async Task<List<JobKey>> GetJobsWhileGroupEqualsAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        var retJobKeys = new List<JobKey>();
        var tempInfos = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).ConfigureAwait(false);
        foreach (var item in tempInfos) retJobKeys.Add(item);
        return retJobKeys;
    }

    public static async Task<List<string>> GetTaskIDsAsync(string groupName)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        IReadOnlyCollection<JobKey> tempList;
        if (string.IsNullOrEmpty(groupName))
            tempList = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false);
        else
            tempList = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).ConfigureAwait(false);
        var tasklst = tempList.Where(task => task.Name.StartsWith("StopJob_") && task.Name.Length >= 8)
            .Select(c => c.Name[8..]);
        return [..tasklst];
    }

    public static async Task<bool> ExistsGroupAsync(string groupName)
    {
        // IScheduler scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        return (await GetJobsWhileGroupEqualsAsync(groupName).ConfigureAwait(false))?.Count > 0;
    }

    public static async Task<bool> ExistsJobKeyAsync(JobKey jobKey, CancellationToken token = default)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        return await scheduler.CheckExists(jobKey, token).ConfigureAwait(false);
    }

    public static async Task<bool> ExistsTriggerKeyAsync(TriggerKey triggerKey, CancellationToken token = default)
    {
        var scheduler = await GetSchedulerAsync().ConfigureAwait(false);
        return await scheduler.CheckExists(triggerKey, token).ConfigureAwait(false);
    }

    public static async Task CreateStopTaskTriggerAsync(Guid taskId, string batchId, TimeSpan duration)
    {
        Trace.WriteLine($"创建停止计划:{taskId}，{batchId}，{duration.TotalSeconds}秒以后");
        var job = JobBuilder.Create<StopJob>()
            .WithIdentity($"StopJob_{taskId}", batchId)
            .Build();
        var trigger = TriggerBuilder.Create()
            .StartAt(DateTime.Now.Add(duration)) //指定时间开始,且只执行一次
            .WithIdentity($"StopTrigger_{taskId}", batchId)
            .UsingJobData("BatchID", batchId)
            .UsingJobData("TaskID", taskId.ToString())
            .Build();
        try
        {
            await (await GetSchedulerAsync().ConfigureAwait(false)).ScheduleJob(job, trigger).ConfigureAwait(false);
        }
        catch
        {
            //TODO do something ;
        }
    }
}