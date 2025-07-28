using System;
using System.Collections.Generic;
using System.IO;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Core.Configuration;

/// <summary>
///     设备配置
/// </summary>
public class DeviceConfig
{
    private static readonly Lazy<DeviceConfig> _lazy = new(() => new DeviceConfig());

    public DeviceConfig()
    {
        LoadConfig();
    }

    public static DeviceConfig Instance => _lazy.Value;
    public List<ModuleInfo> Devices { get; private set; } = new();

    public void LoadConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigDevice);
        if (!File.Exists(path))
            // GetDefaultConfig();
            return;
        var str = File.ReadAllText(path);
        if (string.IsNullOrEmpty(str)) return;
        Devices = NewtonJsonSerialization.Deserialize<List<ModuleInfo>>(str);
    }

    public void UpdateConfig(List<ModuleInfo> devices)
    {
        Devices = devices;
    }

    public void SaveConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigDevice);
        var json = NewtonJsonSerialization.Serialize(Devices, true);
        Console.WriteLine(json);
        File.WriteAllText(path, json);
    }

    public string GetDeviceName(Guid deviceId)
    {
        var info = Devices.Find(item => item.Id == deviceId);
        if (info == null) return string.Empty;
        return info.DisplayName;
    }
}