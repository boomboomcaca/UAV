using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Core.Configuration;
using Core.Utils;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core;

/// <summary>
///     模块管理器
/// </summary>
internal sealed class ModuleManager
{
    private static readonly Lazy<ModuleManager> _lazy = new(() => new ModuleManager());

    /// <summary>
    ///     设备实例集合
    /// </summary>
    private readonly List<DeviceInstance> _devices = new();

    private readonly object _devicesLock = new();

    private readonly object _lockDevicesChange = new();
    private readonly object _lockModuleState = new();

    /// <summary>
    ///     当前已安装的设备配置信息
    /// </summary>
    private List<ModuleInfo> _installedDeviceModules = new();

    /// <summary>
    ///     当前已安装的模块配置信息
    /// </summary>
    private List<ModuleInfo> _installedDriverModules = new();

    /// <summary>
    ///     构造函数
    /// </summary>
    private ModuleManager()
    {
    }

    public static ModuleManager Instance => _lazy.Value;

    /// <summary>
    ///     站点能力
    /// </summary>
    public EdgeCapacity Capacity { get; private set; } = EdgeCapacity.None;

    /// <summary>
    ///     初始化
    /// </summary>
    public void Initialized()
    {
        _installedDeviceModules = DeviceConfig.Instance.Devices;
        _installedDriverModules = DriverConfig.Instance.Drivers;
        var disabledDev = _installedDeviceModules?.Where(item => item.State == ModuleState.Disabled);
        _installedDriverModules.ForEach(item =>
        {
            if (disabledDev != null && disabledDev.Any(dev => HasDevice(dev.Id.ToString(), item)))
                item.State = ModuleState.Disabled;
            if (item.State == ModuleState.Disabled) return;
            item.State = ModuleState.Fault;
        });
        if (_installedDeviceModules != null)
            foreach (var module in _installedDeviceModules)
            {
                if (module.State == ModuleState.Disabled)
                    // 已停用的设备不需要初始化
                    continue;
                // 先将设备的状态置为故障，再进行初始化
                module.State = ModuleState.Fault;
                module.LastStateTime = DateTime.Now;
                ThreadPool.QueueUserWorkItem(DeviceInitialize, module);
            }
    }

    /// <summary>
    ///     创建功能模块链
    /// </summary>
    /// <param name="id"></param>
    /// <param name="chain"></param>
    /// <param name="message"></param>
    public IDriver BuildDriverChain(Guid id, ref ModuleChain<IDriver> chain, out string message)
    {
        message = string.Empty;
        var module = _installedDriverModules.Find(i => i.Id == id);
        if (module == null)
        {
            message = $"配置错误，没有ID为{id}的功能模块！";
            return null;
        }

        if (module.State is ModuleState.Disabled or ModuleState.Fault)
        {
            message = $"功能模块错误,模块ID{id},模块状态{module.State}";
            return null;
        }

        IDriver driver;
        try
        {
            var type = TypesFactory.GetDriverType(module.Class);
            if (type == null)
            {
                message = $"未找到类型{module.Class}";
                return null;
            }

            driver = ModuleFactory.CreateInstance<IDriver>(type, module.Id);
            if (driver == null)
            {
                message = $"创建功能实例失败{type}";
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            message = $"创建功能模块失败，{ex}";
            return null;
        }

        if (!BuildDriverInstallParameter(module, driver, ref chain, out message))
        {
            // 防止创建模块失败以后会有遗留信息没有清理
            FreeDriverChain(chain);
            return null;
        }

        driver.Attach(MessageManager.Instance);
        chain.Instance = driver;
        chain.ModuleInfo = module;
        if (module.RefCount == 0 && module.State != ModuleState.Busy)
        {
            module.State = ModuleState.Busy;
            // send message
            SendMessage(module.Id, module.ModuleType, module.State, "");
        }

        module.RefCount++;
        return driver;
    }

    /// <summary>
    ///     设备重连成功以后需要重启任务
    /// </summary>
    /// <param name="module"></param>
    /// <param name="driver"></param>
    /// <param name="chain"></param>
    public void ReBuildDriver(ModuleInfo module, IDriver driver, ref ModuleChain<IDriver> chain)
    {
        if (!BuildDriverInstallParameter(module, driver, ref chain, out _, true)) return;
        if (module.State != ModuleState.Busy)
        {
            module.State = ModuleState.Busy;
            // send message
            SendMessage(module.Id, module.ModuleType, module.State, "");
        }
    }

    private bool BuildDriverInstallParameter(ModuleInfo module, IDriver driver, ref ModuleChain<IDriver> chain,
        out string message, bool isDeviceFault = false)
    {
        if (module.Parameters?.Count > 0)
        {
            var deviceChains = new List<ModuleChain<IDevice>>();
            var dic = new Dictionary<string, object>();
            message = "";
            var list = module.Parameters.Where(i => i.IsInstallation && i.NeedModuleCategory != ModuleCategory.None);
            foreach (var parameter in list)
            {
                if (parameter.Value == null) continue;
                if (parameter.Type == ParameterDataType.String)
                {
                    var devId = Guid.Parse(parameter.Value.ToString() ?? string.Empty);
                    var device = BuildDeviceChain(devId, module.Id, ref deviceChains, out message, isDeviceFault);
                    if (device == null) return false;
                    dic.Add(parameter.Name, device);
                }
                else if (parameter.Type == ParameterDataType.List)
                {
                    // List<Guid> devIds = (parameter.Value as List<string>)?.ConvertAll(Guid.Parse);
                    var devIds = (parameter.Value as object[])?.ToList()
                        .ConvertAll(item => Guid.Parse(item?.ToString() ?? string.Empty));
                    var devices = new List<IDevice>();
                    if (devIds != null)
                        foreach (var dev in devIds)
                        {
                            var device = BuildDeviceChain(dev, module.Id, ref deviceChains, out message, isDeviceFault);
                            // if (device == null)
                            // {
                            //     return null;
                            // }
                            devices.Add(device);
                        }

                    dic.Add(parameter.Name, devices.ToArray());
                }
            }

            var list1 = module.Parameters.Where(i => i.IsInstallation && i.NeedModuleCategory == ModuleCategory.None);
            foreach (var parameter in list1)
            {
                Console.WriteLine($"Name:{parameter.Name},Value:{parameter.Value}");
                driver.SetParameter(parameter.Name, parameter.Value);
            }

            if (dic.TryGetValue(ParameterNames.AntennaController, out var value))
            {
                var antennaController = value as IDevice;
                // 天线控制器相关设置
                BuildAntennaParameter(antennaController, module.Parameters);
            }

            chain.Devices = deviceChains;
            chain.Installations = dic;
            return true;
        }

        message = "此功能参数丢失";
        return false;
    }

    /// <summary>
    ///     创建设备模块链
    /// </summary>
    /// <param name="id"></param>
    /// <param name="driverId"></param>
    /// <param name="chains"></param>
    /// <param name="message"></param>
    /// <param name="isDeviceFault"></param>
    internal IDevice BuildDeviceChain(Guid id, Guid driverId, ref List<ModuleChain<IDevice>> chains, out string message,
        bool isDeviceFault = false)
    {
        message = string.Empty;
        var module = _installedDeviceModules.Find(i => i.Id == id);
        if (module == null)
        {
            message = $"配置错误，没有ID为{id}的设备模块!";
            return null;
        }

        if (module.State is ModuleState.Disabled or ModuleState.Offline or ModuleState.Fault)
        {
            message = $"设备模块错误,模块ID{id},模块状态{module.State}";
            return null;
        }

        var same = chains.Find(i => i.Instance.Id == id);
        if (same != null) return same.Instance;
        IDevice device = null;
        lock (_devicesLock)
        {
            var refers = _devices.FindAll(i => i.Id == id);
            if (refers.Count > 0)
            {
                // 如果在设备实例集合中已经存在多个实例，则查找被引用最少的实例
                var least = refers[0];
                foreach (var ins in refers)
                    if (ins.RefCount > least.RefCount)
                        least = ins;
                if (least.RefCount <= 0
                    || (refers.Count >= module.MaxInstance
                        && module.MaxInstance > 0))
                {
                    // 如果设备当前实例都被使用，则使用已创建的实例
                    least.RefCount++;
                    device = least.Instance;
                }
                // 如果设备支持多实例，实例数少于限制数则创建新设备实例
            }
        }

        if (device == null)
        {
            // 创建新实例
            try
            {
                device = DeviceInitialize(module);
            }
            catch
            {
                // ignored
            }

            if (device == null)
            {
                message = "设备初始化错误";
                return null;
            }

            try
            {
                device.Initialized(module);
            }
            catch (Exception ex)
            {
                message = $"设备初始化错误,{ex.Message}";
                (device as DeviceBase)?.Dispose();
                return null;
            }

            var instance = new DeviceInstance(module.Id, device);
            lock (_lockDevicesChange)
            {
                _devices.Add(instance);
            }
        }

        var devChain = new ModuleChain<IDevice>(device, module);
        chains.Add(devChain);
        lock (_lockModuleState)
        {
            if (module.State != ModuleState.Busy)
            {
                module.State = ModuleState.Busy;
                UpdateDriverState(module.Id.ToString(), ModuleState.DeviceBusy, driverId.ToString());
                // 发送日志
                SendMessage(module.Id, module.ModuleType, module.State, "");
            }

            if (!isDeviceFault) module.RefCount++;
        }

        return device;
    }

    internal void FreeDeviceChain(ModuleChain<IDevice> chain, bool isDeviceFault = false)
    {
        if (chain == null) return;
        lock (_lockModuleState)
        {
            if (!isDeviceFault) chain.ModuleInfo.RefCount--;
            if (chain.ModuleInfo.RefCount == 0
                && chain.ModuleInfo.State is ModuleState.Idle or ModuleState.Busy)
                if (chain.ModuleInfo.State != ModuleState.Idle)
                {
                    chain.ModuleInfo.State = ModuleState.Idle;
                    // 设备状态修改为normal并更新设备状态
                    SendMessage(chain.ModuleInfo.Id, chain.ModuleInfo.ModuleType, chain.ModuleInfo.State, "");
                    UpdateDriverState(chain.ModuleInfo.Id.ToString(), chain.ModuleInfo.State);
                }

            var instanceRef = _devices.Find(i => i.Instance == chain.Instance);
            if (instanceRef != null) instanceRef.RefCount--;
        }
    }

    public void FreeDriverChain(ModuleChain<IDriver> chain)
    {
        if (chain == null) return;
        if (chain.Devices != null)
            foreach (var dev in chain.Devices)
                FreeDeviceChain(dev);
        if (chain.Instance == null || chain.ModuleInfo == null) return;
        if (chain.Instance is IDataPort)
        {
            // 以后可能需要对这里进行操作，预留。
        }

        if (chain.Instance is IDisposable disposable) disposable.Dispose();
        lock (_lockModuleState)
        {
            chain.ModuleInfo.RefCount--;
            if (chain.ModuleInfo.RefCount == 0
                && chain.ModuleInfo.State is ModuleState.Idle or ModuleState.Busy)
            {
                var id = chain.ModuleInfo.Parameters.Find(item => item.IsInstallation && item.IsPrimaryDevice)?.Value
                    ?.ToString();
                var module = _installedDeviceModules.Find(item => item.Id.ToString().Equals(id));
                var state = ModuleState.Idle;
                if (module?.State == ModuleState.Busy) state = ModuleState.DeviceBusy;
                // 更新功能状态
                if (chain.ModuleInfo.State != state)
                {
                    chain.ModuleInfo.State = state;
                    SendMessage(chain.ModuleInfo.Id, chain.ModuleInfo.ModuleType, chain.ModuleInfo.State, "");
                }
            }
        }
    }

    /// <summary>
    ///     日志信息
    /// </summary>
    /// <param name="message"></param>
    public void OnMessage(SDataMessage message)
    {
        if (message.ErrorCode != (int)InternalMessageType.DeviceRestart) return;
        var id = Guid.Parse(message.Description);
        var module = _installedDeviceModules.Find(i => i.Id == id);
        if (module == null) return;
        // 更新模块状态
        if (module.State is ModuleState.Idle or ModuleState.Busy)
        {
            module.LastStateTime = DateTime.Now;
            module.State = ModuleState.Fault;
            SendMessage(module.Id, module.ModuleType, module.State, "");
            UpdateDriverState(module.Id.ToString(), module.State);
        }

        lock (_devicesLock)
        {
            var list = _devices.Where(i => i.Id == module.Id).ToList();
            list.ForEach(item =>
            {
                lock (_lockDevicesChange)
                {
                    _devices.Remove(item);
                }

                if (item.Instance is not IDisposable disposable) return;
                disposable.Dispose();
            });
        }

        // 重新初始化设备
        ThreadPool.QueueUserWorkItem(DeviceInitialize, module);
    }

    /// <summary>
    ///     更新设备状态
    /// </summary>
    public void UpdateModuleState()
    {
        _installedDeviceModules?.ForEach(dev => SendMessage(dev.Id, dev.ModuleType, dev.State, ""));
        _installedDriverModules?.ForEach(driver => SendMessage(driver.Id, driver.ModuleType, driver.State, ""));
    }

    /// <summary>
    ///     获取所有设备信息
    /// </summary>
    public List<ModuleInfo> GetDeviceState()
    {
        return _installedDeviceModules?.ConvertAll(i => i.Clone());
    }

    /// <summary>
    ///     异步初始化设备
    /// </summary>
    /// <param name="obj"></param>
    private void DeviceInitialize(object obj)
    {
        if (obj is not ModuleInfo module)
            // 日志信息
            return;
        var isOk = false;
        IDevice device = null;
        while (!isOk)
        {
            try
            {
                device = DeviceInitialize(module);
                UpdateEdgeCapacity(device.GetType());
            }
            catch
            {
                // ignored
            }

            if (device == null) return;
            var canResume = true;
            Exception excepction = null;
            try
            {
                isOk = device.Initialized(module);
            }
            catch (ArgumentException ex)
            {
                canResume = false;
                Trace.WriteLine($"设备{module.Id}初始化失败且不可恢复，异常信息：参数错误，{ex}");
                excepction = ex;
            }
            catch (NotSupportedException ex)
            {
                canResume = false;
                Trace.WriteLine($"设备{module.Id}初始化失败且不可恢复，异常信息：当前平台->{RuntimeInformation.RuntimeIdentifier}，{ex}");
                excepction = ex;
            }
            catch (NotImplementedException ex)
            {
                canResume = false;
                Trace.WriteLine($"设备{module.Id}初始化失败且不可恢复，异常信息：当前平台->{RuntimeInformation.RuntimeIdentifier}，{ex}");
                excepction = ex;
            }
            catch (DllNotFoundException ex)
            {
                canResume = false;
                Trace.WriteLine(
                    $"设备{module.Id}初始化失败且不可恢复，异常信息：当前平台->{RuntimeInformation.RuntimeIdentifier}，C/C++动态库缺失，{ex}");
                excepction = ex;
            }
            catch (BadImageFormatException ex)
            {
                canResume = false;
                Trace.WriteLine(
                    $"设备{module.Id}初始化失败且不可恢复，异常信息：当前平台->{RuntimeInformation.RuntimeIdentifier}，C/C++动态库加载错误，{ex}");
                excepction = ex;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"设备{module.Id}初始化异常，异常信息：{ex}");
                excepction = ex;
            }

            if (!isOk)
            {
                if (!canResume || (module.State != ModuleState.Offline &&
                                   DateTime.Now.Subtract(module.LastStateTime).TotalSeconds > 15))
                    DealWithDeviceError(module, excepction);
                if (device is IDisposable dis) dis.Dispose();
                if (!canResume) return;
                Thread.Sleep(1000);
            }
        }

        var instance = new DeviceInstance(module.Id, device);
        lock (_lockDevicesChange)
        {
            _devices.Add(instance);
        }

        // 后面应该不需要重新获取最新的配置了
        // 因为配置更改了以后边缘端会重启或重新加载配置
        // 不管怎么都会重新初始化
        if (module.State != ModuleState.Idle)
        {
            module.State = ModuleState.Idle;
            SendMessage(module.Id, module.ModuleType, module.State, "");
            UpdateDriverState(module.Id.ToString(), ModuleState.Idle);
        }
    }

    private void DealWithDeviceError(ModuleInfo module, Exception ex)
    {
        var error = $"设备初始化错误 deviceID={module.Id}";
        SendMessage("device", error, ex);
        module.State = ModuleState.Offline;
        module.LastStateTime = DateTime.Now;
        // 设备离线时，功能仍然为故障，功能不存在离线状态
        UpdateDriverState(module.Id.ToString(), ModuleState.Fault);
        SendMessage(module.Id, module.ModuleType, module.State, "");
    }

    /// <summary>
    ///     设备初始化
    /// </summary>
    /// <param name="module"></param>
    private IDevice DeviceInitialize(ModuleInfo module)
    {
        IDevice device;
        try
        {
            var type = TypesFactory.GetDeviceType(module.Class);
            if (type == null)
                // 日志信息
                return null;
            device = ModuleFactory.CreateInstance<IDevice>(type, module.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"设备初始化失败{ex.Message}");
            return null;
        }

        if (device == null)
            // 日志信息
            return null;
        device.Attach(MessageManager.Instance);
        foreach (var para in module.Parameters.Where(i => i.IsInstallation))
            try
            {
                var value = para.Value;
                if (para.Value == null
                    && para.Template?.Count > 0
                    && para.Parameters?.Count > 0)
                    value = para.Parameters.Select(item => (Dictionary<string, object>)item).ToArray();
                device.SetParameter(para.Name, value);
            }
            catch
            {
                return null;
            }

        return device;
    }

    private void SendMessage(string module, string description, Exception ex)
    {
        MessageManager.Instance.Error(module, description, ex);
    }

    private void LogMessage(LogType logType, string description, InternalMessageType messageType, string content)
    {
        var info = new SDataMessage
        {
            LogType = logType,
            ErrorCode = (int)messageType,
            Description = description,
            Detail = content
        };
        MessageManager.Instance.Log(info);
    }

    private void SendMessage(Guid moduleId, ModuleType moduleType, ModuleState state, string content)
    {
        // if (moduleType != ModuleType.Device)
        // {
        //     return;
        // }
        var stateChange = new SDataStateChange
        {
            Id = moduleId,
            ModuleType = moduleType,
            State = state,
            Content = content
        };
        MessageManager.Instance.SendMessage(stateChange);
        var logType = state is ModuleState.Fault or ModuleState.Offline ? LogType.Warning : LogType.Message;
        var messageType = moduleType == ModuleType.Device
            ? InternalMessageType.DeviceStateChange
            : InternalMessageType.DriverStateChange;
        LogMessage(logType, moduleId.ToString(), messageType, state.ToString());
    }

    private void BuildAntennaParameter(IDevice device, List<Parameter> parameters)
    {
        var paraAntennas = parameters.Find(item => item.Name == ParameterNames.Antennas);
        paraAntennas.Value = paraAntennas.Parameters.Select(item => (Dictionary<string, object>)item).ToArray();
        device.SetParameter(paraAntennas.Name, paraAntennas.Value);
        var paraSelectMode = parameters.Find(item => item.Name == ParameterNames.AntennaSelectionMode);
        Magneto.Contract.Utils.ConvertStringToEnum<AntennaSelectionMode>(paraSelectMode.Value.ToString());
        device.SetParameter(ParameterNames.AntennaSelectionMode, paraSelectMode.Value);
    }

    /// <summary>
    ///     更新模块状态
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="state">状态</param>
    /// <param name="skipDriverId">要跳过的模块ID</param>
    private void UpdateDriverState(string deviceId, ModuleState state, string skipDriverId = "")
    {
        // 这里暂时只判断主设备
        var list = _installedDriverModules.Where(item => HasDevice(deviceId, item, true));
        var moduleInfos = list as ModuleInfo[] ?? list.ToArray();
        if (moduleInfos.Any())
            foreach (var item in moduleInfos)
            {
                if (item.Id.ToString() == skipDriverId) continue;
                if (item.State == ModuleState.Disabled)
                    // 被禁用的功能不更新状态
                    continue;
                if (item.State != state)
                {
                    item.State = state;
                    // Console.WriteLine($"Change State:{item.ID}:{item.State}");
                    SendMessage(item.Id, item.ModuleType, item.State, "");
                }
            }
    }

    /// <summary>
    ///     判断某个设备是否属于某个功能模块
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="driver"></param>
    /// <param name="isPrimary">是否判断主设备，默认不判断</param>
    private bool HasDevice(string deviceId, ModuleInfo driver, bool isPrimary = false)
    {
        if (driver.Parameters == null) return false;
        foreach (var para in driver.Parameters)
        {
            if (!para.IsInstallation
                || para.NeedModuleCategory == ModuleCategory.None)
                continue;
            if (isPrimary && !para.IsPrimaryDevice) continue;
            var value = para.Value?.ToString();
            if (deviceId == value) return true;
        }

        return false;
    }

    /// <summary>
    ///     更新设备能力
    /// </summary>
    /// <param name="type"></param>
    private void UpdateEdgeCapacity(Type type)
    {
        try
        {
            var devDescriptions = type?.GetCustomAttributes(typeof(DeviceDescriptionAttribute), true);
            if (devDescriptions == null || devDescriptions.Length == 0) return;
            if (devDescriptions[0] is DeviceDescriptionAttribute desc)
            {
                var capacity = desc.Capacity;
                Capacity |= capacity;
            }
        }
        catch
        {
            // ignored
        }
    }
}

/// <summary>
///     模块实例类
/// </summary>
internal class DeviceInstance(Guid id, IDevice instance, int refCount = 0)
{
    /// <summary>
    ///     模块ID
    /// </summary>
    public Guid Id { get; set; } = id;

    /// <summary>
    ///     模块实例
    /// </summary>
    public IDevice Instance { get; set; } = instance;

    /// <summary>
    ///     引用计数
    /// </summary>
    public int RefCount { get; set; } = refCount;
}

/// <summary>
///     模块链表
///     待重构！
/// </summary>
/// <typeparam name="T">T为IDevice或IDriver</typeparam>
internal class ModuleChain<T> where T : class
{
    internal ModuleChain(T instance, ModuleInfo moduleInfo)
    {
        Instance = instance;
        ModuleInfo = moduleInfo;
    }

    internal ModuleChain()
    {
    }

    /// <summary>
    ///     模块实例
    /// </summary>
    public T Instance { get; set; }

    /// <summary>
    ///     模块信息
    /// </summary>
    public ModuleInfo ModuleInfo { get; set; }

    public Dictionary<string, object> Installations { get; set; }

    /// <summary>
    ///     模块的子级设备实例集合
    /// </summary>
    public List<ModuleChain<IDevice>> Devices { get; set; }
}