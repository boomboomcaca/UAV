using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Core.Configuration;

public sealed class DriverConfig
{
    private static readonly Lazy<DriverConfig> _lazy = new(() => new DriverConfig());

    private DriverConfig()
    {
        LoadConfig();
        //string json = MessagePack.MessagePackSerializer.SerializeToJson<List<DriverInfo>>(_driverList);
    }

    public static DriverConfig Instance => _lazy.Value;
    public List<ModuleInfo> Drivers { get; private set; } = new();

    /// <summary>
    ///     加载配置
    /// </summary>
    public void LoadConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigDriver);
        if (!File.Exists(path))
            // GetDefaultConfig();
            return;
        var str = File.ReadAllText(path);
        if (string.IsNullOrEmpty(str)) return;
        Drivers = NewtonJsonSerialization.Deserialize<List<ModuleInfo>>(str);
    }

    public void UpdateConfig(List<ModuleInfo> drivers)
    {
        Drivers = drivers;
        foreach (var driver in Drivers)
        {
            if (driver.State != ModuleState.Disabled)
                // 云端有bug，有时候取回的模块状态会是忙碌的状态，因此这里需要将功能模块的状态修改为空闲
                driver.State = ModuleState.Idle;
            var ids = driver.Parameters
                .Where(item => item.IsInstallation && item.NeedModuleCategory != ModuleCategory.None)
                .Select(item => item.Value?.ToString()).ToList();
            if (ids.Count > 0)
            {
                var modules = DeviceConfig.Instance.Devices.Where(item => ids.Contains(item.Id.ToString()));
                foreach (var module in modules)
                    driver.Parameters.ForEach(parameter => Magneto.Contract.Utils.UpdateParameterOwners(ref parameter, module));
            }
        }

        SaveConfig();
    }

    public void SaveConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigDriver);
        var json = NewtonJsonSerialization.Serialize(Drivers, true);
        Console.WriteLine(json);
        File.WriteAllText(path, json);
    }

    public ModuleInfo FindDriverById(Guid driverId)
    {
        if (Drivers == null || Drivers.Count == 0) return null;
        var module = Drivers.Find(item => item.Id == driverId);
        return module;
    }
}