using System;
using System.IO;
using Core.Utils;
using Magneto.Contract;
using Magneto.Protocol.Define;

namespace Core.Configuration;

/// <summary>
///     站点配置
/// </summary>
public sealed class StationConfig
{
    private static readonly Lazy<StationConfig> _lazy = new(() => new StationConfig());

    private StationConfig()
    {
        LoadConfig();
    }

    public static StationConfig Instance => _lazy.Value;
    public StationInfo Station { get; private set; }

    public void UpdateConfig(StationInfo station)
    {
        Station = station;
    }

    public void LoadConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigStation);
        if (!File.Exists(path))
            // File.Create(path).Close();
            // Test();
            return;
        var str = File.ReadAllText(path);
        if (string.IsNullOrEmpty(str)) return;
        Station = NewtonJsonSerialization.Deserialize<StationInfo>(str);
    }

    public void SaveConfig()
    {
        var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathConfig);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, PublicDefine.FileConfigStation);
        var json = NewtonJsonSerialization.Serialize(Station, true);
        Console.WriteLine(json);
        File.WriteAllText(path, json);
    }
}