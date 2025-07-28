using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CCC;
using Core.Configuration;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Magneto.Protocol.Interface;
using StreamJsonRpc;

namespace Core.Business;

public partial class ControlServer : ITask
{
    public Task<PresetTaskResponse> PresetTaskAsync(Guid moduleId, string pluginId, string pluginName = "",
        int priority = 1, bool needHeart = true)
    {
        pluginId ??= "";
        pluginName ??= "";
        var result = new PresetTaskResponse();
        var value = string.Empty;
        var module = DriverConfig.Instance.FindDriverById(moduleId);
        if (module == null)
        {
            value = $"未找到ID为{moduleId}的模块";
            var str = $"{DateTime.Now:HH:mm:ss.fff} ,{value}";
            Trace.WriteLine(str);
            var exception = new LocalRpcException(value)
            {
                ErrorCode = ErrorCode.ErrorCodeModuleNotFound
            };
            throw exception;
        }

        try
        {
            var taskId = TaskManager.Instance.CreateTask(moduleId, false, _client.SessionId, MediaType.None, "",
                pluginId, pluginName, "", priority, needHeart);
            result.IpAddress = RunningInfo.LocalIpAddress;
            result.TaskId = taskId;
            var ip = RunningInfo.EdgeIp;
            if (string.IsNullOrEmpty(ip)) ip = _client.ServerIp;
            result.Uri = $"ws://{ip}:{_client.ServerPort}{Maps.MapTask}/{taskId}";
        }
        catch (Exception ex)
        {
            value = $"创建任务失败！{ex.Message}";
            var str = $"{DateTime.Now:HH:mm:ss.fff}    Feature:{module.Feature} False, {value}";
            Trace.WriteLine(str);
            throw new LocalRpcException(value)
            {
                ErrorCode = ErrorCode.ErrorCodeTaskCreateFail
            };
#if !DEBUG
                throw;
#endif
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
        return Task.FromResult(result);
    }

    public void SetTaskParameters(Guid id, List<Parameter> parameters)
    {
        var task = TaskManager.Instance.GetTask(id);
        // var message = "任务ID不存在！";
        // var exception = new LocalRpcException(message)
        // {
        //     ErrorCode = ErrorCode.ErrorCode_Task_NotFound
        // };
        // throw exception;
        // var log = new SDataBusinessLog()
        // {
        //     LogType = BusinessLogType.Task,
        //     Message = $"任务[{id}]设置参数",
        //     Code = 110002,
        //     Level = BusinessLogLevel.Info
        // };
        // MessageManager.Instance.SendMessage(log);
        task?.SetParameters(parameters);
    }

    public void StartTask(Guid id, string account = "")
    {
        try
        {
            TaskManager.Instance.StartTask(id, account);
            var message = new SDataMessage
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
    }

    public void StopTask(Guid id)
    {
        TaskManager.Instance.StopTask(id);
        var client = Server.GetClient(_client.ServerPort, id.ToString());
        if (client != null
            && client.IpAddress != _client.IpAddress
            && client.Port != _client.Port
            && client.SessionId != _client.SessionId)
            Server.ReleaseClient(client.ServerPort, client.SessionId);
        var message = new SDataMessage
        {
            LogType = LogType.Message,
            ErrorCode = (int)InternalMessageType.Information,
            Description = "Stop Task"
        };
        MessageManager.Instance.Log(message);
    }
}