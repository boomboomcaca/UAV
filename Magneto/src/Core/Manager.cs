using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.Configuration;
using Core.Define;
using Core.PublicService;
using Core.Tasks;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core;

public sealed class Manager
{
    private static readonly Lazy<Manager> _lazy = new(() => new Manager());
    private readonly object _lockConnect = new();
    private bool _isClosing;

    /// <summary>
    ///     标记是否已经登录云端成功（获取配置可能还未成功
    /// </summary>
    private bool _isLogin;

    private bool _isStartConnect;

    /// <summary>
    ///     记录获取配置失败日志间隔60秒记录
    /// </summary>
    private DateTime _lastGetConfigTime = DateTime.MinValue;

    /// <summary>
    ///     记录登录失败日志时间隔60秒记录
    /// </summary>
    private DateTime _lastLoginFailLogTime = DateTime.MinValue;

    private ServerResourceMonitor _srm;

    /// <summary>
    ///     构造函数
    /// </summary>
    private Manager()
    {
        RunningInfo.CloudState = false;
        //Initialized();
    }

    public static Manager Instance => _lazy.Value;

    /// <summary>
    ///     初始化
    /// </summary>
    public void Initialized()
    {
        if (StationConfig.Instance.Station?.Tag?.ContainsKey("longitude") == true
            && StationConfig.Instance.Station?.Tag?.ContainsKey("latitude") == true
            && double.TryParse(StationConfig.Instance.Station.Tag["longitude"].ToString(), out var lng)
            && double.TryParse(StationConfig.Instance.Station.Tag["latitude"].ToString(), out var lat))
            RunningInfo.UpdateGps(lng, lat);
        if (RunningInfo.IpType == 0) RunningInfo.EdgeIp = StationConfig.Instance.Station?.GetEdgeIp();
        // if (StationConfig.Instance.Station != null)
        // {
        //     _edgeID = StationConfig.Instance.Station.EdgeID;
        // }
        if (StationConfig.Instance.Station != null)
        {
            RunningInfo.StationId = StationConfig.Instance.Station.StationId;
            RunningInfo.StationType = StationConfig.Instance.Station.Type;
        }

        ModuleManager.Instance.Initialized();
        // _ = Task.Run(() => SystemControl.ConfigRsync(RsyncConfigType.Update));
        _ = Task.Run(() => CrondTaskManager.Instance.InitializedAsync());
        ConnectToCloud();
        _srm = new ServerResourceMonitor();
        _srm.Initialized();
        if (RunningInfo.ServerType != 1) return;
        EnvironmentManager.Instance.Initialized();
    }

    /// <summary>
    ///     异步连接云端
    /// </summary>
    /// <param name="loseConnect">true: 与云端中断连接;false: 新连接</param>
    public void ConnectToCloud(bool loseConnect = false)
    {
        lock (_lockConnect)
        {
            // 冗余代码
            // 防止客户端错误的连接了两个System服务，造成连接中断时调用两次这个方法
            if (loseConnect)
            {
                if (!_isStartConnect)
                {
                    RunningInfo.CloudState = false;
                    _isLogin = false;
                    _isStartConnect = true;
                }
                else
                {
                    return;
                }
            }
        }

        // 异步连接线程
        _ = Task.Run(async () =>
        {
            // 第一次中断连接的时候等待5秒再重连
            var timeSpan = loseConnect ? 5000 : 1000;
            while (!_isClosing)
            {
                await Task.Delay(timeSpan).ConfigureAwait(false);
                try
                {
                    if (_isClosing) break;
                    timeSpan = 5000;
                    if (!await EdgeLoginAsync().ConfigureAwait(false)) continue;
                    RunningInfo.CloudState = true;
                    lock (_lockConnect)
                    {
                        _isStartConnect = false;
                    }
                    // 登录成功以后向云端发送一次当前设备与功能的状态
                    Console.ForegroundColor = ConsoleColor.Green;
                    Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} 登录成功，更新设备状态到云端");
                    Console.ResetColor();
                    MessageManager.Instance.Log("Login", LogType.Message, "登录云端成功");
                    if (RunningInfo.CloudClientState)
                    {
                        // 更新时间
                        _ = Task.Run(MessageManager.Instance.GetCloudTimeAsync);
                        // var client = MessageManager.Instance.GetCloudRpc();
                        // if (client != null)
                        // {
                        //     AudioRecognition.Instance.Attach(client.RpcServer);
                        // }
                        DeviceConfig.Instance.Devices.ForEach(dev =>
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
                        DriverConfig.Instance.Drivers.ForEach(driver =>
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
                        if (RunningInfo.ServerType == 0)
                            // 现在是登录云端成功才会执行计划任务的初始化，是否要直接执行？
                            // await CrondTaskManager.Instance.InitializedAsync().ConfigureAwait(false);
                            await CrondTaskManager.Instance.OnCrondTaskChangedAsync().ConfigureAwait(false);
                    }

                    break;
                }
                catch
                {
                    // 容错代码
                }
            }
        });
    }

    /// <summary>
    ///     关闭服务
    /// </summary>
    public void Close()
    {
        _isClosing = true;
        if (RunningInfo.ServerType == 1) EnvironmentManager.Instance.Close();
        _srm?.Close();
        MessageManager.Instance.Close();
    }

    public async Task<bool> EdgeLoginAsync()
    {
        // string id = StationConfig.Instance.Station.EdgeID.ToString();
        var id = RunningInfo.EdgeId;
        if (_isLogin) return await CheckCloudConfigAsync().ConfigureAwait(false);
        var result = await CloudClient.Instance.EdgeLoginAsync(RunningInfo.CloudTokenUser,
            RunningInfo.CloudTokenPassword, id, RunningInfo.Port, RunningInfo.ServerType).ConfigureAwait(false);
        if (!result.Result)
        {
            if (!(DateTime.Now.Subtract(_lastLoginFailLogTime).TotalSeconds > 60)) return false;
            _lastLoginFailLogTime = DateTime.Now;
            MessageManager.Instance.Log("Login", LogType.Warning, result.IpAddress);

            return false;
        }

        if (RunningInfo.IpType == 0) RunningInfo.EdgeIp = result.IpAddress;
        if (RunningInfo.ServerType == 1) RunningInfo.EdgeId = result.Id;
        _isLogin = true;

        return await CheckCloudConfigAsync().ConfigureAwait(false);
    }

    public async Task<bool> CheckCloudConfigAsync()
    {
        var oldVersion = "";
        if (StationConfig.Instance.Station != null) oldVersion = StationConfig.Instance.Station.Version;
        var id = RunningInfo.EdgeId;
        var version = await CloudClient.Instance.GetVersionAsync(id).ConfigureAwait(false);
        //StationInfo station = _register.GetEdge(id);
        if (version == null) return false;
        if (version.Equals(oldVersion)) return true;
        try
        {
            var isEdge = RunningInfo.ServerType == 0;
            var config = await CloudClient.Instance.GetEdgeConfigAsync(id, isEdge).ConfigureAwait(false);
            if (config == null) return false;
            var devices = config.Modules.Where(item => item.ModuleType == ModuleType.Device).ToList();
            var drivers = config.Modules.Where(item => item.ModuleType == ModuleType.Driver).ToList();
            config.Station.UpdateEdgeIp(RunningInfo.EdgeIp);
            StationConfig.Instance.UpdateConfig(config.Station);
            StationConfig.Instance.SaveConfig();
            DeviceConfig.Instance.UpdateConfig(devices);
            DeviceConfig.Instance.SaveConfig();
            DriverConfig.Instance.UpdateConfig(drivers);
            DriverConfig.Instance.SaveConfig();
            SystemControl.Restart();
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"获取站点配置失败:{ex.Message}");
            // 容错代码
            if (!(DateTime.Now.Subtract(_lastGetConfigTime).TotalSeconds > 60)) return false;
            _lastGetConfigTime = DateTime.Now;
            MessageManager.Instance.Log("GetConfig", LogType.Warning, ex.Message);

            return false;
        }
    }

    /// <summary>
    ///     更新GPS信息
    /// </summary>
    /// <param name="gps"></param>
    public void UpdateGps(SDataGps gps)
    {
        RunningInfo.UpdateGps(gps);
    }

    /// <summary>
    ///     更新罗盘信息
    /// </summary>
    /// <param name="compass"></param>
    public void UpdateCompass(SDataCompass compass)
    {
        RunningInfo.UpdateCompass(compass);
    }

    public List<ModuleInfo> GetDeviceState()
    {
        return ModuleManager.Instance.GetDeviceState();
    }

    public List<TaskInfo> GetTaskList()
    {
        return TaskManager.Instance.GetTaskList();
    }

    public List<BatchedCrondInfo> GetPlanList()
    {
        return CrondTaskManager.Instance.GetPlanList();
    }

    public EdgeCapacity GetEdgeCapacity()
    {
        return ModuleManager.Instance.Capacity;
    }
}