using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CCC;
using Core.Define;
using Core.PublicService;
using Core.Statistics;
using Core.Storage;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Contract.Storage;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using StreamJsonRpc;

namespace Core.Tasks;

public delegate void DataArrivedHandler(Guid taskId, List<object> data);

public sealed class TaskInstance : IComparable<TaskInstance>, IComparable
{
    /// <summary>
    ///     防止startTask的时候重复下发参数造成延时比较久
    ///     在setTaskParameters中设置此集合
    ///     在startTask中将此集合清空
    /// </summary>
    private readonly List<string> _changedParameters = [];

    private readonly List<ClientInfo> _clientList = [];
    private readonly double _epsilon = 1.0E-7d;
    private readonly object _lockClientList = new();
    private readonly object _lockDeviceFault = new();
    private readonly object _lockRunningPara = new();
    private DataStatistics _dataStat;

    /// <summary>
    ///     创建任务以后第一次参数下发不再验证参数是否相等
    /// </summary>
    private bool _firstSetParameterSign;

    private TaskIqDataSave _iqDataSave;
    private bool _isCompetition;
    private DateTime _lastErrorTime = DateTime.MinValue;

    /// <summary>
    ///     上次有连接过来的时间，如果30秒没有任何连接则关闭任务
    /// </summary>
    private DateTime _preClientTime = DateTime.Now;

    private TaskRawDataSave _rawDataSave;
    private bool _sign;
    private UnitProcess _unitStat;
    public TaskInfo TaskInfo { get; private set; }
    public FeatureType Feature => TaskInfo?.Feature ?? FeatureType.None;

    /// <summary>
    ///     获取/设置 当前是否存在竞争（主要指设备竞争）
    /// </summary>
    public bool IsCompetition
    {
        get => _isCompetition;
        set
        {
            _isCompetition = value;
            if (TaskInfo?.ModuleChain?.Instance is DriverBase driver) driver.IsCompetition = value;
        }
    }

    public event DataArrivedHandler DataArrived;

    /// <summary>
    ///     创建任务
    /// </summary>
    /// <param name="driverId">要开启任务的功能ID</param>
    /// <param name="crond">是否计划任务</param>
    /// <param name="createName">创建者</param>
    /// <param name="saveData">保存数据</param>
    /// <param name="crondId">计划编号</param>
    /// <param name="pluginId">前端模块ID</param>
    /// <param name="pluginName">前端模块名称</param>
    /// <param name="remark">备注信息</param>
    /// <param name="priority">优先级 0-独占任务;int.MaxValue-闲时任务</param>
    /// <param name="needHeart">是否需要心跳</param>
    /// <exception cref="LocalRpcException"></exception>
    public Guid CreateTask(Guid driverId,
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
        var driverChain = new ModuleChain<IDriver>();
        var driver = ModuleManager.Instance.BuildDriverChain(driverId, ref driverChain, out var msg);
        if (driver == null)
        {
            // TODO: 需要定义Exception
            var exception = new LocalRpcException(msg)
            {
                ErrorCode = ErrorCode.ErrorCodeTaskCreateFail
            };
            throw exception;
        }

        var taskId = Guid.NewGuid();
        var info = new TaskInfo
        {
            Id = taskId,
            Feature = driverChain.ModuleInfo.Feature,
            Priority = priority,
            // CurrentPriority = priority,
            ModuleId = driverId,
            ModuleChain = driverChain,
            State = TaskState.New,
            Session = createName
        };
        info.Name = Magneto.Contract.Utils.GetNameByDescription(info.Feature);
        info.DataPort = new DataPort(taskId);
        info.CreateTime = DateTime.Now;
        info.IsInitialized = false;
        info.RequestState = TaskState.New;
        info.MediaType = saveData;
        info.IsCrondTask = crond;
        info.CrondId = crondId;
        info.PluginId = pluginId;
        info.PluginName = pluginName;
        info.Remark = remark;
        info.NeedHeart = needHeart;
        info.Level = TaskHelper.GetTaskLevel(priority);
        info.DataPort.DataArrived += OnDataArrived;
        var ip = RunningInfo.EdgeIp; //StationConfig.Instance.Station.GetEdgeIp();
        var port = RunningInfo.Port;
        info.Uri = $"ws://{ip}:{port}{Maps.MapTask}/{taskId}";
        TaskInfo = info;
        _preClientTime = DateTime.Now;
        // 给任务信息类添加默认的运行参数
        // 第二个条件可以不用
        if (!info.IsInitialized)
        {
            // 查询功能中的运行参数
            var runningParams = driverChain.ModuleInfo.Parameters.Where(item => !item.IsInstallation);
            // 防止修改某个地方的参数造成配置中的参数也修改了，因此这里需要转换参数副本
            // 运行参数的目的是当前端不设置参数直接运行任务时为设备设置默认的参数以运行
            info.RunningParameters = runningParams.ToDictionary(item => item.Name, item => item.Clone());
            foreach (var pair in info.RunningParameters)
                if (info.Feature != FeatureType.BsDecoding && pair.Value.Template?.Count > 0
                                                           && (pair.Value.Parameters == null
                                                               || pair.Value.Parameters.Count == 0))
                {
                    // 防错代码
                    // 设计上，当模板不为空的时候子参数一定不为空
                    // 但是现在在某些情况下前端配置的设备参数中有几率会出现模板不为空但是子参数为空的情况
                    // 这时候需要从模板中取出参数名与参数值重新生成字典并保存到子参数中
                    var dic = pair.Value.Template.ToDictionary(item => item.Name, item => item.Value);
                    pair.Value.Parameters = [dic];
                }

            if (info.ModuleChain.Installations?.Count > 0)
                // 设置功能的安装参数
                foreach (var pair in info.ModuleChain.Installations)
                    info.ModuleChain.Instance.SetParameter(pair.Key, pair.Value);
            IAntennaController controller = null;
            if (info.ModuleChain.Devices?.Count > 0)
                controller = (IAntennaController)info.ModuleChain.Devices
                    .Find(item => item.Instance is IAntennaController)?.Instance;
            _dataStat = new DataStatistics(TaskInfo.Feature, TaskInfo.Id, controller);
            _dataStat.StatisticsDataArrived += DataStatDataArrived;
            // 如果任务存在单位切换开关则初始化单位切换处理类
            if (info.RunningParameters.ContainsKey(ParameterNames.UnitSelection)) _unitStat = new UnitProcess();
            // 初始化功能
            info.ModuleChain.Instance.Initialized(info.ModuleChain.ModuleInfo);
            info.IsInitialized = true;
        }

        _firstSetParameterSign = true;
        _rawDataSave = new TaskRawDataSave(TaskInfo);
        if (TaskInfo.Feature == FeatureType.Ffm) _iqDataSave = new TaskIqDataSave(TaskInfo);
        return taskId;
    }

    public bool StartTask()
    {
        // 设置运行时参数
        if (TaskInfo.RunningParameters?.Count > 0)
            lock (_lockRunningPara)
            {
                foreach (var pair in TaskInfo.RunningParameters)
                {
                    if (TaskInfo.Feature == FeatureType.Rtv) continue;
                    if (pair.Value.IsInstallation) continue;
                    if (_changedParameters.Contains(pair.Key)) continue;
                    var value = pair.Value.Value;
                    if (value == null
                        && pair.Value.Parameters != null
                        && pair.Value.Template != null
                        && pair.Value.Parameters.Count > 0
                        && pair.Value.Template.Count > 0
                        && pair.Value.Name != ParameterNames.DdcChannels)
                    {
                        // 如果模板不为空，说明这个参数为复杂参数，需要从子参数中取出相关的值转换为字典并设置下去
                        // 主要的应用场景为以下几个：
                        //     1. 频段扫描的频段参数scanSegments
                        //     2. 中频多路的子通道参数
                        //     3. 天线集合 antennas
                        value = pair.Value.Parameters.Select(item => (Dictionary<string, object>)item).ToArray();
                        pair.Value.Value = value;
                    }

                    if (value == null && pair.Value.Template?.Count > 0) continue;
#if DEBUG
                    Console.WriteLine($"启动时下发参数:{pair.Key},{value}");
#endif
                    TaskInfo.ModuleChain.Instance.SetParameter(pair.Key, value);
                    _dataStat?.ChangeParameter(pair.Value);
                }

                _changedParameters.Clear();
                if (!_rawDataSave.Running) _rawDataSave.CheckStartSaveData();
                if (_iqDataSave?.Running == false) _iqDataSave.CheckStartSaveData();
            }

        // 功能启动
        try
        {
            _firstSetParameterSign = false;
            ReportDataStorage.Instance.StartSaveData(TaskInfo);
            // _taskInfo.DataPort.DataArrived += OnDataArrived;
            TaskInfo.ModuleChain.Instance.Start(TaskInfo.DataPort, TaskInfo.MediaType);
            TaskInfo.LastActiveTime = DateTime.Now;
            if (TaskInfo.BeginTime == DateTime.MinValue) TaskInfo.BeginTime = TaskInfo.LastActiveTime;
            TaskInfo.State = TaskState.Start;
            _dataStat?.Start();
            // _preClientTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"任务{TaskInfo.Id}启动失败,{ex.Message}");
            if (DateTime.Now.Subtract(_lastErrorTime).TotalSeconds > 60)
            {
                MessageManager.Instance.Error("Task", ex.Message, ex);
                _lastErrorTime = DateTime.Now;
            }

            return false;
        }

        UpdateTaskInfo();
        // Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    功能{task.Feature}启动 TaskID={task.ID} DriverID={task.ModuleId} CrondID={task.CrondID}");
        return true;
    }

    /// <summary>
    ///     由系统调度暂停
    /// </summary>
    /// <returns>true:任务可以暂停 false:任务不能暂停</returns>
    internal bool PauseTask()
    {
        try
        {
            if (TaskInfo.State != TaskState.Start) return true;
            var bCanPause = TaskInfo.ModuleChain.Instance.Pause();
            if (bCanPause)
            {
                // _taskInfo.DataPort.DataArrived -= OnDataArrived;
                TaskInfo.ModuleChain.Instance.Stop();
                TaskInfo.LastActiveTime = DateTime.Now;
                TaskInfo.State = TaskState.Pause;
            }

            return bCanPause;
        }
        catch
        {
            // ignored
        }

        return false;
    }

    /// <summary>
    ///     停止任务
    /// </summary>
    public void StopTask()
    {
        TaskInfo.EndTime = DateTime.Now;
        TaskInfo.LastActiveTime = DateTime.Now;
        _dataStat?.Stop();
        ReportDataStorage.Instance.Stop(TaskInfo.Id);
        _rawDataSave?.CheckStopSaveData();
        _iqDataSave?.CheckStopSaveData();
        if (TaskInfo.State != TaskState.Start) return;
        TaskInfo.State = TaskState.Stop;
        TaskInfo.DataPort.DataArrived -= OnDataArrived;
        TaskInfo.ModuleChain.Instance.Stop();
        // Debug.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    功能{task.Feature}停止 TaskID={task.ID} DriverID={task.ModuleId} TaskCount:{_taskDic.Count}");
    }

    /// <summary>
    ///     卸载任务
    /// </summary>
    public void ReleaseTask()
    {
        StopTask();
        TaskInfo.State = TaskState.Stop;
        UpdateTaskInfo();
        ModuleManager.Instance.FreeDriverChain(TaskInfo.ModuleChain);
        if (_dataStat != null) _dataStat.StatisticsDataArrived -= DataStatDataArrived;
        _ = ClearClientAsync();
    }

    /// <summary>
    ///     因为设备故障造成的中断任务
    /// </summary>
    public void InterruptTask()
    {
        if (TaskInfo.RequestState != TaskState.Start) return;
        lock (_lockDeviceFault)
        {
            // _taskInfo.DataPort.DataArrived -= OnDataArrived;
            TaskInfo.ModuleChain.Instance.Stop();
            TaskInfo.ModuleChain.Devices.ForEach(item => ModuleManager.Instance.FreeDeviceChain(item, true));
            TaskInfo.LastActiveTime = DateTime.Now;
            TaskInfo.State = TaskState.Pause;
            TaskInfo.RequestState = TaskState.Pause;
            TaskInfo.IsDeviceFault = true;
        }
    }

    /// <summary>
    ///     设备恢复以后重启任务
    /// </summary>
    public void RestartTask()
    {
        lock (_lockDeviceFault)
        {
            if (!TaskInfo.IsDeviceFault) return;
            if (TaskInfo.RequestState == TaskState.Stop) return;
            var chain = TaskInfo.ModuleChain;
            ModuleManager.Instance.ReBuildDriver(TaskInfo.ModuleChain.ModuleInfo, TaskInfo.ModuleChain.Instance,
                ref chain);
            TaskInfo.ModuleChain = chain;
            TaskInfo.RequestState = TaskState.Start;
            TaskInfo.IsDeviceFault = false;
        }
    }

    /// <summary>
    ///     设置参数
    /// </summary>
    /// <param name="parameters">参数集合</param>
    public void SetParameters(List<Parameter> parameters)
    {
        var isParameterChanged = false;
        foreach (var parameter in parameters)
        {
            if (parameter.IsInstallation) continue;
            var name = parameter.Name;
            var value = parameter.Value;
            if (value == null)
                if (parameter.Parameters?.Count > 0)
                {
                    value = parameter.Parameters.Select(item => (Dictionary<string, object>)item).ToArray();
                    parameter.Value = value;
                }

            // 现在中频多路功能在刚启动时可能不会下发子通道参数，因此这里如果下发的子参数为null时，也需要将运行参数缓存进行更新，防止将上次的历史数据下发下去
            // continue;
            if (TaskInfo.RunningParameters.TryGetValue(name, out var runningParameter))
            {
                if ((runningParameter.Template == null || runningParameter.Template.Count == 0) && value == null)
                    // 现在前端下发的参数会出现值为null的情况，这种情况需要过滤掉，同时存在子参数（如scanSegments）的不能过滤
                    continue;
                if (!IsParameterChanged(runningParameter, value, runningParameter.Value) &&
                    !_firstSetParameterSign) continue;
                if (name != ParameterNames.AntennaId)
                    // 天线ID不再校验以下信息
                    CheckParameter(runningParameter, value);
            }
            else
            {
                var tmp = TaskInfo.ModuleChain.ModuleInfo.Parameters?.FirstOrDefault(item => item.Name == name);
                if (tmp?.IsInstallation == true) continue;
                var para = new Parameter
                {
                    Name = name,
                    Value = value
                };
                lock (_lockRunningPara)
                {
                    TaskInfo.RunningParameters.Add(name, para);
                }
            }

            if (name.Equals(ParameterNames.RecByThreshold) && bool.TryParse(value.ToString(), out var recs))
                _overSaveDataSwitch = recs;
            else if (TaskInfo.Feature == FeatureType.Ffm && name.Equals(ParameterNames.SquelchThreshold) &&
                     double.TryParse(value.ToString(), out var mt))
                _saveDataThreshold = mt;
            else if (TaskInfo.Feature == FeatureType.Fdf && name.Equals(ParameterNames.LevelThreshold) &&
                     double.TryParse(value.ToString(), out var lt)) _saveDataThreshold = lt;
            isParameterChanged = true;
            TaskInfo.RunningParameters[name].Value = value;
            // 子参数集合不能为null，只能为空集合
            TaskInfo.RunningParameters[name].Parameters = parameter.Parameters ?? [];
            if (TaskInfo.State != TaskState.Pause && !(TaskInfo.State == TaskState.New && _isCompetition))
            {
                if (!_changedParameters.Contains(name)) _changedParameters.Add(name);
                // 任务暂停时不能下发参数，同时状态为new时也不能下发，否则刚创建的任务万一有设备争用，会把当前正在运行的任务参数给冲了
#if DEBUG
                Console.WriteLine($"下发参数:{name},{value}");
#endif
                // 如果任务已经挂起了则不能再下发参数了，否则会影响正在运行的其他任务
                TaskInfo.ModuleChain.Instance.SetParameter(name, value);
            }

            _dataStat?.ChangeParameter(parameter);
            _unitStat?.SetParameter(parameter);
            // 部分参数修改以后需要重新发送数据，之前缓存的数据需要清理
            if (name is ParameterNames.Frequency
                or ParameterNames.ScanSegments
                or ParameterNames.MscanPoints
                or ParameterNames.DwellSwitch
                or ParameterNames.StartFrequency
                or ParameterNames.StopFrequency
                or ParameterNames.StepFrequency
                or ParameterNames.RmsSwitch)
                _clientList?.ForEach(p => p.ClearDataSign = true);
        }

        if (isParameterChanged && TaskInfo.State == TaskState.Start)
        {
            Console.WriteLine("参数修改，重新保存");
            _rawDataSave?.CheckStartSaveData(false);
            _iqDataSave?.CheckStartSaveData(false);
            // UpdateTaskInfo();
            if (TaskInfo?.IsDataSaveNeeded == true) RawDataStorage.Instance.UpdateFactor(TaskInfo.Id, TaskInfo.Factors);
            if (TaskInfo?.IsIqDataSaveNeeded == true)
                RawIqDataStorage.Instance.UpdateFactor(TaskInfo.Id, TaskInfo.Factors);
        }
    }

    /// <summary>
    ///     添加连接到任务
    /// </summary>
    /// <param name="client"></param>
    public void AddClient(ClientInfo client)
    {
        lock (_lockClientList)
        {
            _clientList.Add(client);
            _preClientTime = DateTime.Now;
        }
    }

    /// <summary>
    ///     从任务中删除连接
    /// </summary>
    /// <param name="client"></param>
    public void DeleteClient(ClientInfo client)
    {
        lock (_lockClientList)
        {
            _clientList.Remove(client);
            _preClientTime = DateTime.Now;
        }
    }

    /// <summary>
    ///     获取此任务实例是否可后台停止
    /// </summary>
    /// <returns>true:可停止；false：不可停止</returns>
    public bool CanStopTask()
    {
        lock (_lockClientList)
        {
            //射电天文电测任务不清理
            if (!TaskInfo.IsCrondTask && _clientList.Count == 0 &&
                DateTime.Now.Subtract(_preClientTime).TotalSeconds > 10 && TaskInfo.Feature != FeatureType.Fastemt)
            {
                Trace.WriteLine($"任务{TaskInfo.Feature}:{TaskInfo.Id}超时10秒没有连接，需要清理");
                return true;
            }

            if (_clientList.Count > 0 && TaskInfo.NeedHeart)
                _clientList.ForEach(client =>
                {
                    if (DateTime.Now.Subtract(client.ActiveTime).TotalSeconds <= RunningInfo.Timeout)
                    {
                        _sign = true;
                        return;
                    }

                    if (_sign)
                    {
                        Trace.WriteLine(
                            $"任务{TaskInfo.Feature}:{TaskInfo.Id}的连接[{client.SessionId}]超时{RunningInfo.Timeout}秒没有心跳，需要清理本连接");
                        _sign = false;
                    }

                    try
                    {
                        // client.Socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        client.Socket.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }
                });
            if (TaskInfo.IsDeviceFault && DateTime.Now.Subtract(TaskInfo.LastActiveTime).TotalSeconds > 15)
            {
                Trace.WriteLine($"任务{TaskInfo.Feature}:{TaskInfo.Id}超时15秒设备未重连成功，需要清理");
                return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     清理所有连接
    /// </summary>
    public async Task ClearClientAsync()
    {
        try
        {
            if (_clientList?.Count > 0)
                foreach (var client in _clientList) await client.Socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
            _clientList?.Clear();
        }
        catch
        {
            // 容错代码
        }
    }

    public string GetAntennaInfo()
    {
        // 由于有的接收机也可以是天线控制器，因此这里可能会查到多个天线控制器，需要进行处理
        if (TaskInfo.ModuleChain?.Devices?.FirstOrDefault(item => item.Instance is IAntennaController)?.Instance is not
            IAntennaController) return string.Empty;
        var sid = TaskInfo.ModuleChain.ModuleInfo.Parameters.Find(item => item.Name == ParameterNames.Antennas);
        if (sid?.Owners == null || sid.Owners.Count == 0 || !Guid.TryParse(sid.Owners[0], out var antCtrolId))
            return string.Empty;
        var dev = TaskInfo.ModuleChain?.Devices?.Find(item => item.Instance.Id == antCtrolId);
        if (dev?.Instance is not IAntennaController controller) return string.Empty;
        var antenna = controller.Antennas.Find(item => item.Id == controller.AntennaId);
        if (antenna == null) return string.Empty;
        var polarization = antenna.Polarization == Polarization.Horizontal ? "水平极化" : "垂直极化";
        var type = antenna.AntennaType == AntennaType.Monitoring ? "监测天线" : "测向天线";
        return $"{type}-{polarization}-{antenna.StartFrequency}MHz-{antenna.StopFrequency}MHz";
    }

    /// <summary>
    ///     根据极化方式获取天线ID
    /// </summary>
    /// <param name="frequency">频率</param>
    /// <param name="polarization">极化方式</param>
    public string GetAntennaIdByPolarization(double frequency, Polarization polarization)
    {
        if (TaskInfo.ModuleChain?.Devices?.FirstOrDefault(item => item.Instance is IAntennaController)?.Instance is not
            IAntennaController) return Guid.Empty.ToString();
        var sid = TaskInfo.ModuleChain.ModuleInfo.Parameters.Find(item => item.Name == ParameterNames.Antennas);
        if (sid?.Owners == null || sid.Owners.Count == 0 || !Guid.TryParse(sid.Owners[0], out var antCtrolId))
            return Guid.Empty.ToString();
        object dev = TaskInfo.ModuleChain?.Devices?.Find(item => item.Instance.Id == antCtrolId);
        if (dev is not IAntennaController controller) return Guid.Empty.ToString();
        var antennas = controller.Antennas.Where(item => item.Id != Guid.Empty && item.Polarization == polarization)
            .ToList();
        if (antennas.Any() != true) return Guid.Empty.ToString();
        var antenna = antennas.Find(item => item.StartFrequency <= frequency && item.StopFrequency >= frequency);
        if (antenna == null) return antennas[0].Id.ToString();
        return antenna.Id.ToString();
    }

    #region 天线因子处理

    ///// <summary>
    /////     判断当前任务开启的功能是否有天线因子
    ///// </summary>
    //private bool IsHasFactor()
    //{
    //    var res = TaskInfo.ModuleChain.Devices.Find(item => item.Instance is IAntennaController) != null;
    //    return TaskInfo.Feature switch
    //    {
    //        FeatureType.FFDF or FeatureType.FDF or FeatureType.WBDF
    //            or FeatureType.SCAN_DF or FeatureType.SSE or FeatureType.MFDF
    //            or FeatureType.M_SCAN_DF or FeatureType.AMP_DF => false,
    //        _ => res
    //    };
    //}

    #endregion

    #region 私有方法

    /// <summary>
    ///     更新任务信息到云端
    ///     现在这个方法只在任务启动与任务停止时下发，其他时候不再发送
    /// </summary>
    private void UpdateTaskInfo()
    {
        // if (IsHasFactor())
        // {
        //     // 暂时这样处理，防止云端出现并发而造成部分数据无法存储
        //     if (_taskInfo.Factors == null || _taskInfo.Factors.Count == 0)
        //     {
        //         // Console.WriteLine("没有天线因子...");
        //         return;
        //     }
        //     // else
        //     // {
        //     //     Console.WriteLine($"天线因子为:{_taskInfo.Factors[0].Data[0]}...");
        //     // }
        // }
        var factors = TaskInfo.Factors ?? [];
        if (TaskInfo.Feature != FeatureType.Ffm
            && TaskInfo.Feature != FeatureType.Itum
            && TaskInfo.Feature != FeatureType.Fdf)
            factors = null;
        var task = new RunningTaskInfo
        {
            Id = TaskInfo.Id,
            DeviceId = TaskInfo.ModuleChain.Devices.FirstOrDefault()!.ModuleInfo.Id,
            PluginId = TaskInfo.PluginId,
            ModuleId = TaskInfo.ModuleId,
            CrondId = TaskInfo.CrondId,
            Name = Magneto.Contract.Utils.ConvertEnumToString(TaskInfo.Feature), //_taskInfo.Name,
            Uri = TaskInfo.Uri,
            MajorParameters = TaskInfo.RunningParameters.Where(p => p.Value.IsMajorParameter)
                .Select(p => p.Value.ToSimple()).ToList(),
            StartTime = Magneto.Contract.Utils.GetTimestamp(TaskInfo.CreateTime.AddMilliseconds(RunningInfo.TimeCompensation)),
            StopTime = TaskInfo.State != TaskState.Stop
                ? null
                : Magneto.Contract.Utils.GetTimestamp(TaskInfo.EndTime.AddMilliseconds(RunningInfo.TimeCompensation)),
            WorkTime = TaskInfo.State != TaskState.Stop
                ? 0
                : (int)TaskInfo.EndTime.Subtract(TaskInfo.CreateTime).TotalSeconds,
            Account = TaskInfo.Creator,
            Parameters = TaskInfo.RunningParameters.Values.Select(p => p.ToSimple()).ToList(),
            AntennaDescription = GetAntennaInfo(),
            Factor = factors
        };
        if (TaskInfo.EndTime != DateTime.MinValue && TaskInfo.State != TaskState.Stop)
        {
            task.StopTime = Magneto.Contract.Utils.GetTimestamp(TaskInfo.EndTime.AddMilliseconds(RunningInfo.TimeCompensation));
            task.WorkTime = 0;
        }

        var info = new SDataTask
        {
            Tasks = [task]
        };
        MessageManager.Instance.SendMessage(info);
    }

    private void OnDataArrived(Guid taskId, List<object> data)
    {
        if (TaskInfo.Id != taskId) return;
        if (TaskInfo?.IsReportNeeded == true) ReportDataStorage.Instance.SetData(TaskInfo.Id, data);
        if (data.Find(item => item is SDataLevel) is SDataLevel lvl && _overSaveDataSwitch)
            _isDataOverThreshold = lvl.Data >= (float)_saveDataThreshold;
        // 先存储再统计
        if (CanDataSave()) _rawDataSave?.SaveData(ref data);
        if (TaskInfo?.IsIqDataSaveNeeded == true) _iqDataSave?.SaveData(ref data);
        // 进行数据统计分析与处理
        _dataStat?.RealtimeDataProcess(data);
        if (data.Count == 0) return;
        _unitStat?.OnData(data);
        if (data.Count == 0) return;
        DataArrived?.Invoke(taskId, data);
        if (data.Exists(item => item is SDataFactor))
            if (TaskInfo != null)
            {
                (TaskInfo.Factors ??= []).Clear();
                foreach (var item in data)
                    if (item is SDataFactor factor)
                        // Console.WriteLine($"检测到天线因子数据:{factor.SegmentOffset},{factor.StartFrequency},{factor.StopFrequency},{factor.StepFrequency},{factor.Data.Length}");
                        // factor.Data = factor.Data.Select(i => Math.Round(i, 1)).ToArray();
                        TaskInfo.Factors.Add(factor);
                // Thread.Sleep(100);
                // UpdateTaskInfo();
                if (TaskInfo?.IsDataSaveNeeded == true)
                    RawDataStorage.Instance.UpdateFactor(TaskInfo.Id, TaskInfo.Factors);
                if (TaskInfo?.IsIqDataSaveNeeded == true)
                    RawIqDataStorage.Instance.UpdateFactor(TaskInfo.Id, TaskInfo.Factors);
            }
    }

    private void DataStatDataArrived(List<object> data)
    {
        DataArrived?.Invoke(TaskInfo.Id, data);
    }

    private bool IsParameterChanged(Parameter parameter, object value, object preValue)
    {
        if (TaskInfo?.Feature == FeatureType.Rtv)
            // TODO : 现在暂定rtv全都要下发下去，后期可能需要优化或重构这里
            return true;
        // 设置为工业控制的参数，即使值不发生变化，也发送到功能和设备，以便于控制。
        if (parameter.Style.Equals(DisplayStyle.Ics)) return true;
        if (parameter.Category == PropertyCategoryNames.Command)
            // TODO : 现在暂时这么做
            // 这个分组内的参数都为命令，如果参数值相等的话一样需要下发（其他参数如果值相等则不会重复下发）
            // 以后需要修改Parameter类，添加一个是否为命令的属性来标记当前参数是命令还是普通参数
            return true;
        // if (parameter.Name == ParameterNames.SaveDataSwitch && int.TryParse(value.ToString(), out int saveDataSwitch))
        // {
        //     _taskInfo.MediaType = (MediaType)saveDataSwitch;
        // }
        if (value == null || preValue == null) return true;

        if (value is string) return !value.ToString()!.Equals(preValue.ToString());

        if (value is Dictionary<string, object> dic1
            && preValue is Dictionary<string, object> dic2)
        {
            if (dic1.Count != dic2.Count) return true;
            foreach (var pair in dic1)
            {
                if (!dic2.ContainsKey(pair.Key)) return true;
                if (IsParameterChanged(parameter, pair.Value, dic2[pair.Key])) return true;
            }

            return false;
        }

        var type = value.GetType();
        if (type.IsValueType)
        {
            if (double.TryParse(value.ToString(), out var db1) && double.TryParse(preValue.ToString(), out var db2))
                return Math.Abs(db1 - db2) > _epsilon;
            return !value.Equals(preValue);
        }

        if (type.IsArray)
            if (value is Array arr1
                && preValue is Array arr2)
            {
                if (arr1.Length != arr2.Length) return true;
                for (var i = 0; i < arr1.Length; i++)
                    if (IsParameterChanged(parameter, arr1.GetValue(i), arr2.GetValue(i)))
                        return true;
                return false;
            }

        return true;
    }

    /// <summary>
    ///     对前端设置的参数进行校验，如果参数不符合规则，则抛出异常
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <param name="value">要设置的参数值</param>
    private static void CheckParameter(Parameter parameter, object value)
    {
        if (parameter.Values == null || parameter.Values.Count == 0) return;
        if (value == null) return;
        switch (parameter.Type)
        {
            case ParameterDataType.Bool:
                if (!bool.TryParse(value.ToString(), out _))
                {
                    var msg = $"输入数据的格式不正确,参数名:{parameter.Name},目标格式:{parameter.Type}";
                    ThrowException(msg, ErrorCode.ErrorCodeParameterSetFail);
                }

                // var bls = parameter.Values.ConvertAll(Convert.ToBoolean);
                // if (!bls.Contains(bl))
                // {
                //     var msg = $"设置的参数在值集合中不存在,参数名:{parameter.Name},参数值:{value}";
                //     ThrowException(msg, ErrorCode.ErrorCode_Parameter_SetFail);
                // }
                break;
            case ParameterDataType.Number:
                if (!double.TryParse(value.ToString(), out _))
                {
                    var msg = $"输入数据的格式不正确,参数名:{parameter.Name},目标格式:{parameter.Type}";
                    ThrowException(msg, ErrorCode.ErrorCodeParameterSetFail);
                }

                // var dbs = parameter.Values.ConvertAll(Convert.ToDouble);
                // if (!dbs.Exists(item => Math.Abs(db - item) <= _epsilon))
                // {
                //     var msg = $"设置的参数在值集合中不存在,参数名:{parameter.Name},参数值:{value}";
                //     ThrowException(msg, ErrorCode.ErrorCode_Parameter_SetFail);
                // }
                break;
            case ParameterDataType.String:
                // var strs = parameter.Values.ConvertAll(item => item.ToString());
                // if (!strs.Contains(value.ToString()))
                // {
                //     var msg = $"设置的参数在值集合中不存在,参数名:{parameter.Name},参数值:{value}";
                //     ThrowException(msg, ErrorCode.ErrorCode_Parameter_SetFail);
                // }
                break;
            case ParameterDataType.List:
            default:
                break;
        }
    }

    private static void ThrowException(string message, int errorCode)
    {
        var exception = new LocalRpcException(message)
        {
            ErrorCode = errorCode
        };
        throw exception;
    }

    #endregion

    #region IComparable<Task> 成员

    /// <summary>
    ///     比较器实现
    /// </summary>
    /// <param name="other"></param>
    public int CompareTo(TaskInstance other)
    {
        // 任务类型权限
        var r0 = -TaskInfo.Priority.CompareTo(other.TaskInfo.Priority);
        // 请求状态权限 停止 > 新建 > 暂停 > 运行
        var r1 = TaskInfo.RequestState.CompareTo(other.TaskInfo.RequestState);
        // 任务实际运行状态 没有运行的任务高于在执行的任务 才能保证都能运行
        var r2 = TaskInfo.State.CompareTo(other.TaskInfo.State);
        // 任务最后激活调度时间
        // 更近的时间大于更远的时间，但其实更远的时间优先级更高，故加一个负号
        var r3 = -TaskInfo.LastActiveTime.CompareTo(other.TaskInfo.LastActiveTime);
        var r4 = 0;
        if (TaskInfo.RequestState == TaskState.Stop) r4 = 1;
        // 以4个量来确定任务的调度优秀级
        // r0 权值为8 r1权值为4 r2权值为2 r3权值为1
        var priority = -((r4 << 4) + (r0 << 3) + (r1 << 2) + (r2 << 1) + r3);
        return priority;
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        if (obj is TaskInstance x) return CompareTo(x);
        throw new ArgumentException("", nameof(obj));
    }

    #endregion

    #region 原始数据存储

    /// <summary>
    ///     超过门限才存储原始数据开关
    /// </summary>
    private bool _overSaveDataSwitch;

    /// <summary>
    ///     门限
    /// </summary>
    private double _saveDataThreshold;

    /// <summary>
    ///     值是否超过门限
    /// </summary>
    private bool _isDataOverThreshold = true;

    public bool CanDataSave()
    {
        if (TaskInfo == null) return false;
        if (!TaskInfo.IsDataSaveNeeded) return false;
        if (!_overSaveDataSwitch) return true;
        return _isDataOverThreshold;
    }

    #endregion
}