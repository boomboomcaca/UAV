using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CCC;
using Core.Configuration;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Magneto.Protocol.Interface;
using StreamJsonRpc;

namespace Core.Business;

public partial class ControlServer : ISystem, IDisposable
{
    private readonly ClientInfo _client;

    public ControlServer(ClientInfo client)
    {
        client.ActiveTime = DateTime.Now;
        _client = client;
        MessageManager.Instance.AddClient(client);
        if (_client.Map == Maps.MapAtomic || (_client.Map == Maps.MapControl && RunningInfo.CloudClientState))
        {
            DeviceConfig.Instance.Devices?.ForEach(dev =>
            {
                var stateChange = new SDataStateChange
                {
                    Id = dev.Id,
                    ModuleType = dev.ModuleType,
                    State = dev.State,
                    Content = ""
                };
                MessageManager.Instance.SendMessage(stateChange);
            });
            DriverConfig.Instance.Drivers?.ForEach(driver =>
            {
                var stateChange = new SDataStateChange
                {
                    Id = driver.Id,
                    ModuleType = driver.ModuleType,
                    State = driver.State,
                    Content = ""
                };
                MessageManager.Instance.SendMessage(stateChange);
            });
            var runningTask = TaskManager.Instance.GetRunningTaskList();
            if (runningTask?.Tasks?.Count > 0) MessageManager.Instance.SendMessage(runningTask);
        }
    }

    public void Dispose()
    {
        TaskManager.Instance.ClearTaskByName(_client.SessionId);
        MessageManager.Instance.RemoveClient(_client.SessionId);
        // if (_client.Map != Maps.MapControl && _client.Map != Maps.MapEnvironment)
        if (_client.Map != Maps.MapControl) return;
        // 由于单机版的系统或者docker部署的系统，云端的连接过来以后获取到的IP地址可能不是云端本身的IP地址（可能是内网的IP地址）
        // 因此根据IP地址无法正确的识别云端
        // 因此这里将map是否是control来作为云端的判定依据
        // if (_client.IpAddress != RunningInfo.CloudIpAddress)
        // {
        //     return;
        // }
        Manager.Instance.ConnectToCloud(true);
    }

    public Task<HelloResponse> SayHelloAsync(string name, int age)
    {
        return Task.FromResult(new HelloResponse { Message = $"您好， {name}!,{age}" });
    }

    public Task<AllInfoResponse> QueryAllInfoAsync()
    {
        var allInfo = new AllInfoResponse
        {
            StationInfo = StationConfig.Instance.Station,
            DeviceInfo = DeviceConfig.Instance.Devices,
            DriverInfo = DriverConfig.Instance.Drivers
        };
        return Task.FromResult(allInfo);
    }

    public Task<QueryDevicesResponse> QueryDeviceAsync()
    {
        return Task.FromResult(new QueryDevicesResponse { Devices = DeviceConfig.Instance.Devices });
    }

    public Task<QueryDriversResponse> QueryDriverAsync()
    {
        return Task.FromResult(new QueryDriversResponse
        {
            Drivers = DriverConfig.Instance.Drivers
        });
    }

    public Task<QueryStationResponse> QueryStationAsync()
    {
        var station = StationConfig.Instance.Station;
        var response = new QueryStationResponse
        {
            Station = station
        };
        return Task.FromResult(response);
    }

    public Task<SDataTask> QueryRunningTasksAsync()
    {
        var list = TaskManager.Instance.GetRunningTaskList();
        return Task.FromResult(list);
    }

    public Task RestartAppAsync()
    {
        Trace.WriteLine("云端发来边缘端重启命令，重新启动边缘端");
        return Task.Run(SystemControl.Restart);
    }

    public Task UpdateConfigAsync()
    {
        Trace.WriteLine("云端发来配置更改的通知，重新获取配置");
        return Task.Run(Manager.Instance.CheckCloudConfigAsync);
    }

    public Task UpdateCronTabAsync()
    {
        Trace.WriteLine("云端发来计划任务更新通知，重新获取计划任务");
        return Task.Run(CrondTaskManager.Instance.OnCrondTaskChangedAsync);
    }

    public Task UpdateRsyncConfigAsync(RsyncConfigType configType)
    {
        Trace.WriteLine($"云端发来Rsync配置更新通知，通知内容:{configType}");
        // return Task.Run(() => SystemControl.ConfigRsync(configType));
        throw new LocalRpcException("此接口已经弃用")
        {
            ErrorCode = ErrorCode.ErrorCodeInterfaceDeprecated
        };
    }

    public Task<int> HeartBeatWithResultAsync()
    {
        // Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff}云端发来心跳...{_client.SessionId}");
        _client.ActiveTime = DateTime.Now;
        return Task.FromResult(1);
    }

    public Task<QueryDirectoryResponse> QueryDirectoryAsync(string directory)
    {
        try
        {
            var folders = directory.Split('/');
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            foreach (var folder in folders) dir = Path.Join(dir, folder);
            if (!Directory.Exists(dir)) throw new Exception($"路径{directory}不存在！");
            var list = new List<string>();
            var info = new DirectoryInfo(dir);
            foreach (var file in info.GetFiles()) list.Add(file.Name);
            return Task.FromResult(new QueryDirectoryResponse { Directory = directory, FileList = list });
        }
        catch (Exception ex)
        {
            var exception = new LocalRpcException($"查询路径失败:{ex.Message}")
            {
                ErrorCode = ErrorCode.ErrorCodeModuleNotFound
            };
            throw exception;
        }
    }

    public Task<QueryAvailableFeaturesResponse> QueryFeaturesAsync()
    {
        var res = new QueryAvailableFeaturesResponse();
        var list = new List<AvailableFeature>();
        foreach (var item in DriverConfig.Instance.Drivers)
        {
            if (item.State != ModuleState.Idle) continue;
            // if (list.Find(p => p.Feature == item.Feature) != null)
            // {
            //     continue;
            // }
            var feat = new AvailableFeature
            {
                Feature = item.Feature,
                FeatureId = item.Id
            };
            list.Add(feat);
        }

        res.Features = list;
        return Task.FromResult(res);
    }

    /// <summary>
    ///     获取站点能力
    /// </summary>
    [JsonRpcMethod("edge.getCapacity")]
    public Task<EdgeCapacity> GetEdgeCapacityAsync()
    {
        return Task.FromResult(Manager.Instance.GetEdgeCapacity());
    }

    /// <summary>
    ///     云端射电任务更新接口
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="moduleId"></param>
    /// <param name="pluginId"></param>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public Task<FastEmtTaskResponse> UpdateFastEmtAsync(string taskId, Guid moduleId, string pluginId = "",
        string pluginName = "")
    {
        var result = new FastEmtTaskResponse();
        if (string.IsNullOrEmpty(taskId)) return Task.FromResult(result);

        #region 执行任务

        var value = string.Empty;
        var module = DriverConfig.Instance.FindDriverById(moduleId);
        if (module == null)
        {
            value = $"未找到ID为{moduleId}的模块";
            Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} ,{value}");
            var exception = new LocalRpcException(value)
            {
                ErrorCode = ErrorCode.ErrorCodeModuleNotFound
            };
            throw exception;
        }

        try
        {
            //判断任务是否正在运行
            var taskList = TaskManager.Instance.GetTaskList();
            foreach (var task in taskList)
                if (task.ModuleId == moduleId)
                {
                    Console.WriteLine($"检测到有射电天文电测任务{task.Id}正在执行，正在停止...");
                    TaskManager.Instance.StopTask(task.Id);
                }

            //独占任务
            var priority = 0;
            var resultTaskId = TaskManager.Instance.CreateTask(moduleId, false, _client.SessionId, MediaType.None, "",
                pluginId ?? "", pluginName ?? "", "", priority, false);
            result.IpAddress = RunningInfo.LocalIpAddress;
            result.TaskId = resultTaskId;
            var ip = RunningInfo.EdgeIp;
            if (string.IsNullOrEmpty(ip)) ip = _client.ServerIp;
            result.Uri = $"ws://{ip}:{_client.ServerPort}{Maps.MapTask}/{resultTaskId}";
        }
        catch (Exception ex)
        {
            value = $"创建任务失败！{ex.Message}";
            Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff}    Feature:{module.Feature} False, {value}");
            var exception = new LocalRpcException(value)
            {
                ErrorCode = ErrorCode.ErrorCodeTaskCreateFail
            };
            throw exception;
        }

        var detail =
            $"{DateTime.Now:HH:mm:ss.fff}    Feature:{module.Feature},Result:{value},TaskId:{result.TaskId},Uri:{result.Uri}";
        Trace.WriteLine(detail);
        var message = new SDataMessage
        {
            LogType = LogType.Message,
            ErrorCode = (int)InternalMessageType.Information,
            Description = "Create Task",
            Detail = detail
        };
        MessageManager.Instance.Log(message);
        try
        {
            var task = TaskManager.Instance.GetTask(result.TaskId);
            var parameters = new List<Parameter>
            {
                new()
                {
                    Name = "taskId",
                    Value = taskId
                }
            };
            task.SetParameters(parameters);
            TaskManager.Instance.StartTask(result.TaskId);
            message = new SDataMessage
            {
                LogType = LogType.Message,
                ErrorCode = (int)InternalMessageType.Information,
                Description = "Start Task"
            };
            MessageManager.Instance.Log(message);
        }
        catch (Exception ex)
        {
            var exc = new LocalRpcException(ex.Message)
            {
                ErrorCode = ErrorCode.ErrorCodeTaskStartFail
            };
            throw exc;
        }

        #endregion

        return Task.FromResult(result);
    }
}