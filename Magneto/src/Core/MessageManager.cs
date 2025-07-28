using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using CCC;
using Core.Configuration;
using Magneto.Contract;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Magneto.Protocol.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Core;

public sealed class MessageManager : IDataPort
{
    private const string LogTemplate =
        "level={Level:w3} ts={Ts} module={Md} message=\"{Message:lj}\" error={Ex}{NewLine}";

    private static readonly Lazy<MessageManager> _lazy = new(() => new MessageManager());
    private readonly List<ClientInfo> _clientList = new();

    #region task

    private readonly CancellationTokenSource _cts;

    #endregion

    private readonly object _lockClient = new();
    private readonly string _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathLog);
    private readonly ConcurrentQueue<object> _messageQueue = new();
    private DateTime _lastClearTimeoutTime = DateTime.MinValue;

    private MessageManager()
    {
        Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel
            //.Debug()
            .Information() // 设置最低日志等级，如果设置为Debug，则会频繁记录日志：level=dbg ts= module= message="Batch acquisition of 0 triggers" error=
            .WriteTo
            .File(Path.Combine(_logDirectory, "Log-.log"), LogEventLevel.Debug, LogTemplate,
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
        // ThreadPool.QueueUserWorkItem(SendMessageThread);
        _cts = new CancellationTokenSource();
        //var sendMsgTask = new Task(() => SendMessageAsync(_cts.Token).ConfigureAwait(false), _cts.Token);
        _ = Task.Run(() => SendMessageAsync(_cts.Token).ConfigureAwait(false));
    }

    public static MessageManager Instance => _lazy.Value;
    public Guid TaskId => Guid.Empty;

    /// <summary>
    ///     通过IDataPort返回的数据需要通过消息通道发送到云端
    ///     比如：GPS数据，Compass数据，动环数据等
    /// </summary>
    /// <param name="data"></param>
    public void OnData(List<object> data)
    {
        _messageQueue.Enqueue(data);
        var gps = (SDataGps)data.Find(item => item is SDataGps);
        if (gps != null) Manager.Instance.UpdateGps(gps);
        var compass = (SDataCompass)data.Find(item => item is SDataCompass);
        if (compass != null) Manager.Instance.UpdateCompass(compass);
    }

    /// <summary>
    ///     通过IDataPort返回的信息
    /// </summary>
    /// <param name="message"></param>
    public void OnMessage(SDataMessage message)
    {
        if (message.ErrorCode == (int)InternalMessageType.DeviceRestart)
        {
            // 通知模块管理器进行设备重启操作
            ModuleManager.Instance.OnMessage(message);
            if (RunningInfo.ServerType == 0)
                // 通知任务进行停止操作
                TaskManager.Instance.DeviceStateChange(message);
            else
                // 通知动环进行停止操作
                EnvironmentManager.Instance.DeviceStateChange(message);
        }
        else if (message.ErrorCode == (int)InternalMessageType.Error)
        {
            Log(message);
        }
    }

    public void Initialized()
    {
    }

    public void Close()
    {
        try
        {
            _cts?.Cancel();
        }
        finally
        {
            _cts?.Dispose();
        }

        Trace.WriteLine("停止消息管理器...");
    }

    /// <summary>
    ///     通过消息通道向云端发送消息
    /// </summary>
    /// <param name="obj"></param>
    public void SendMessage(object obj)
    {
        _messageQueue.Enqueue(obj);
        if (obj is SDataStateChange { ModuleType: ModuleType.Device, State: ModuleState.Idle } stateChange)
            TaskManager.Instance.RestartTaskByDevice(stateChange.Id);
    }

    /// <summary>
    ///     记录日志
    /// </summary>
    /// <param name="message"></param>
    public void Log(SDataMessage message)
    {
        var module = "edge";
        string msg;
        // var log = new SDataBusinessLog();
        var dic = new Dictionary<string, object>
        {
            { "edgeId", RunningInfo.EdgeId }
        };
        switch (message.ErrorCode)
        {
            case (int)InternalMessageType.DeviceRestart:
                module = "device";
                msg = $"设备故障重启 deviceID={message.Description}";
                dic.Add("code", "200005");
                dic.Add("level", "warn");
                dic.Add("parameter1", message.Detail);
                break;
            case (int)InternalMessageType.DeviceStateChange:
                module = "device";
                msg = $"设备状态改变 state={message.Detail} deviceID={message.Description}";
                var deviceId = Guid.Parse(message.Description);
                var deviceName = DeviceConfig.Instance.GetDeviceName(deviceId);
                if (string.IsNullOrEmpty(deviceName)) break;
                if (Enum.TryParse<ModuleState>(message.Detail, out var state))
                    switch (state)
                    {
                        case ModuleState.Busy:
                            dic.Add("code", "200004");
                            dic.Add("level", "info");
                            dic.Add("parameter1", deviceName);
                            break;
                        case ModuleState.Fault:
                            dic.Add("code", "200005");
                            dic.Add("level", "warn");
                            dic.Add("parameter1", deviceName);
                            break;
                        case ModuleState.Idle:
                            dic.Add("code", "200003");
                            dic.Add("level", "info");
                            dic.Add("parameter1", deviceName);
                            break;
                        case ModuleState.Offline:
                            dic.Add("code", "200002");
                            dic.Add("level", "warn");
                            dic.Add("parameter1", deviceName);
                            break;
                        default:
                            dic.Add("code", "200003");
                            dic.Add("level", "info");
                            dic.Add("parameter1", deviceName);
                            break;
                    }

                break;
            default:
                msg = $"{message.Description},{message.Detail}";
                break;
        }

        if (dic.ContainsKey("code") && RunningInfo.CloudState)
            try
            {
                // 向云端记录运行日志
                _ = Task.Run(() => CloudClient.Instance.AddBusinessLogAsync(dic).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                var err = new SDataMessage
                {
                    LogType = LogType.Error,
                    ErrorCode = (int)InternalMessageType.Error,
                    Description = ex.Message,
                    Detail = ex.ToString()
                };
                OnMessage(err);
            }

        Log(module, message.LogType, msg);
    }

    /// <summary>
    ///     记录日志
    /// </summary>
    /// <param name="module">模块</param>
    /// <param name="level">级别</param>
    /// <param name="message">详细信息</param>
    public void Log(string module, LogType level, string message)
    {
        // TODO: LOKI 日志有bug，会将`]}}`转义为`]}`
        var defaultLogger = Serilog.Log.ForContext(new LokiEnricher(module));
        switch (level)
        {
            case LogType.None:
            case LogType.Message:
                defaultLogger.Information(message);
                break;
            case LogType.Warning:
                defaultLogger.Warning(message);
                break;
            case LogType.Error:
                defaultLogger.Error(message);
                break;
            default:
                defaultLogger.Information(message);
                break;
        }
    }

    /// <summary>
    ///     记录错误
    /// </summary>
    /// <param name="module">模块</param>
    /// <param name="message">详细信息</param>
    /// <param name="ex">异常</param>
    public void Error(string module, string message, Exception ex)
    {
        var defaultLogger = Serilog.Log.ForContext(new LokiEnricher(module, ex));
        defaultLogger.Error(message);
    }

    public void AddClient(ClientInfo client)
    {
        lock (_lockClient)
        {
            Trace.WriteLine($"添加连接:{client.IpAddress}:{client.Port}");
            _clientList.Add(client);
            if (client.Map == Maps.MapControl) RunningInfo.CloudClientState = true;
        }
    }

    /// <summary>
    ///     获取云端连接
    /// </summary>
    public ClientInfo GetCloudRpc()
    {
        lock (_lockClient)
        {
            return _clientList.Find(item => item.Map == Maps.MapControl);
        }
    }

    public void RemoveClient(string sessionId)
    {
        lock (_lockClient)
        {
            var client = _clientList.Find(item => item.SessionId == sessionId);
            if (client != null)
            {
                _clientList.Remove(client);
                if (client.Map == Maps.MapControl) RunningInfo.CloudClientState = false;
            }
        }
    }

    private async Task SendMessageAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(100, token).ConfigureAwait(false);
            if (token.IsCancellationRequested) return;
            // 判断连接的心跳
            try
            {
                var ids = new List<string>();
                lock (_lockClient)
                {
                    _clientList.ForEach(client =>
                    {
                        if (DateTime.Now.Subtract(client.ActiveTime).TotalSeconds < RunningInfo.Timeout) return;
                        try
                        {
                            Trace.WriteLine(
                                $"连接{client.SessionId},{client.IpAddress}:{client.Port}心跳超时(超时时间:{RunningInfo.Timeout})，断开连接");
                            ids.Add(client.SessionId);
                            // if (client.Socket.State == WebSocketState.Open)
                            {
                                _ = Task.Run(() => client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                    "Heartbeat Timeout", CancellationToken.None), token);
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    });
                }

                if (ids.Count > 0) ids.ForEach(RemoveClient);
            }
            catch
            {
                // ignored
            }

            try
            {
                // 其他信息
                var list = new List<object>();
                while (!_messageQueue.IsEmpty)
                {
                    _messageQueue.TryDequeue(out var obj);
                    if (obj == null) continue;
                    if (obj is List<object> arr)
                        list.AddRange(arr);
                    else
                        list.Add(obj);
                }

                if (list.Count == 0) continue;
                // if (!RunningInfo.CloudState)
                // {
                //     // Trace.WriteLine("还未登录，不发送数据");
                //     continue;
                // }
                var data = new SharedData
                {
                    EdgeId = RunningInfo.EdgeId,
                    Timestamp = Magneto.Contract.Utils.GetNowTimestamp(),
                    DataCollection = list
                };
                lock (_lockClient)
                {
                    _clientList.ForEach(client =>
                    {
                        if (client.Map == Maps.MapControl && !RunningInfo.CloudState)
                            // 还未登录，不发送数据
                            return;
                        if (client.Socket?.State != WebSocketState.Open) return;
                        // 这个方法无法识别KeyAttribute特性 因此需要转为Dictionary
                        _ = Task.Run(async () =>
                            await client.RpcServer?.NotifyWithParameterObjectAsync(MethodDefine.MessageHandlerNotify,
                                data.ToDictionary())!, token);
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("发送信息Error" + ex);
            }

            ClearTimeoutData();
        }

        Console.WriteLine("消息线程已退出...");
    }

    /// <summary>
    ///     清理超时的日志或原始数据（暂定超时7天删除）
    ///     每天调用一次
    /// </summary>
    private void ClearTimeoutData()
    {
        if (DateTime.Now.Subtract(_lastClearTimeoutTime).TotalDays < 1) return;
        _lastClearTimeoutTime = DateTime.Now;
        try
        {
            // 更新时间
            _ = Task.Run(GetCloudTimeAsync);
            // 清理日志
            var dir = new DirectoryInfo(_logDirectory);
            var files = dir.GetFiles();
            foreach (var file in files)
                try
                {
                    var create = file.CreationTime;
                    if (DateTime.Now.Subtract(create).TotalDays < 7) continue;
                    Trace.WriteLine($"清理日志文件:{file.FullName}");
                    file.Delete();
                }
                catch
                {
                    // 报错不管
                }
        }
        catch
        {
            // 报错不管
        }

        try
        {
            // 清理原始数据
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSavedata);
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return;
            var dirs = dir.GetDirectories();
            foreach (var item in dirs)
                try
                {
                    var create = item.CreationTime;
                    if (DateTime.Now.Subtract(create).TotalDays < 7) continue;
                    Trace.WriteLine($"清理数据文件:{item.FullName}");
                    item.Delete(true);
                }
                catch
                {
                    // 报错不管
                }
        }
        catch
        {
            // 报错不管
        }

        try
        {
            // 清理原始数据data2
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data2", "avicg");
            var dir = new DirectoryInfo(path);
            if (!dir.Exists) return;
            var dirs = dir.GetDirectories();
            foreach (var item in dirs)
                try
                {
                    var create = item.CreationTime;
                    if (DateTime.Now.Subtract(create).TotalDays < 7) continue;
                    Trace.WriteLine($"清理avicg数据文件:{item.FullName}");
                    item.Delete(true);
                }
                catch
                {
                    // 报错不管
                }
        }
        catch
        {
            // 报错不管
        }
    }

    public async Task GetCloudTimeAsync()
    {
        var timestamp = await CloudClient.Instance.GetCloudDateTimeAsync();
        if (timestamp <= 0) return;
        var time = new DateTime(1970, 1, 1).AddMilliseconds(timestamp).ToLocalTime();
        Trace.WriteLine($"查询到系统时间:{time:yyyy-MM-dd HH:mm:ss.fff}");
        RunningInfo.TimeCompensation = (long)(time - DateTime.Now).TotalMilliseconds;
    }

    internal class LokiEnricher : ILogEventEnricher
    {
        private readonly string _ex = "\"\"";
        private readonly string _module;

        public LokiEnricher(string module, Exception ex = null)
        {
            _module = module;
            if (ex != null)
                _ex =
                    $"\"{ex.Message}\" source=\"{ex.Source}\" stacktrace=\"{ex.StackTrace?.Replace('\n', '\0').Replace('\r', '\0')}\"";
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Ts",
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Md", _module));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Ex", _ex));
        }
    }
}