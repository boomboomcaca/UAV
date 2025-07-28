using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.VirtualGps;

public partial class VirtualGps : DeviceBase
{
    private bool _isRunning;

    public VirtualGps(Guid deviceId) : base(deviceId)
    {
    }

    public override void Dispose()
    {
        _isRunning = false;
        base.Dispose();
    }

    /// <summary>
    ///     初始化设备模块
    /// </summary>
    /// <param name="device">模块信息</param>
    /// <returns>成功返回True，否则返回False</returns>
    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (!result) return false;
        InitStatusAndMembers();
        InitGpsSender();
        _isRunning = true;
        return true;
    }

    /// <summary>
    ///     初始化成员和状态
    /// </summary>
    private void InitStatusAndMembers()
    {
        // 初始化最新的GPS为安装的经纬度信息
        _latestGps = new SDataGps { Latitude = Latitude, Longitude = Longitude };
        Stream stream = null;
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GPS.dat");
            if (File.Exists(path)) // 优先从应用程序根目录获取GPS数据
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            else
                Console.WriteLine($"在Path:[{path}]中找不到GPS.dat");
            // 从数据流中获取GPS数据集合，如果stream为空，则返回结果也为空
            _embeddedGpsCollection = GetEmbeddedGpsCollection(stream);
        }
        finally
        {
            stream?.Close();
        }
    }

    /// <summary>
    ///     初始化GPS数据发送线程
    /// </summary>
    private void InitGpsSender()
    {
        if (_gpsSender?.IsAlive == true) return;
        _gpsSender = new Thread(SendGps)
        {
            Name = "virtual_gps_sender",
            IsBackground = true
        };
        _gpsSender.Start();
    }

    /// <summary>
    ///     发送GPS数据
    /// </summary>
    private void SendGps()
    {
        try
        {
            if (!UpdatingData)
                SendConstantGps();
            else if (_embeddedGpsCollection != null)
                SendEmbeddedGps();
            else
                SendMockedGps();
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     发送固定数据
    /// </summary>
    private void SendConstantGps()
    {
        while (_isRunning)
        {
            SendMessageData(new List<object> { _latestGps });
            Thread.Sleep(_cycleTime);
        }
    }

    /// <summary>
    ///     发送内嵌的数据
    /// </summary>
    private void SendEmbeddedGps()
    {
        var count = _embeddedGpsCollection.Count();
        var maxRange = count / 3;
        while (_isRunning)
        {
            var counter = 0;
            // 获取伪随机数据种子
            var seed = GetRandomSeed();
            // 获取任意数据开始索引位置
            var starter = new Random(seed).Next(maxRange);
            foreach (var gps in _embeddedGpsCollection)
            {
                // 跳过直到计数点开始位置
                if (counter++ < starter) continue;
                _latestGps = gps;
                SendMessageData(new List<object> { _latestGps });
                Thread.Sleep(_cycleTime);
            }
        }
    }

    /// <summary>
    ///     发送模拟的GPS数据
    /// </summary>
    private void SendMockedGps()
    {
        var gpsCount = 0;
        while (_isRunning)
        {
            if (gpsCount < 20)
            {
                _latestGps.Latitude = Latitude + 0.001 * (gpsCount % 20);
                _latestGps.Longitude = Longitude;
            }
            else if (gpsCount is >= 20 and < 40)
            {
                _latestGps.Latitude = Latitude + 0.001 * 19;
                _latestGps.Longitude = Longitude + 0.001 * (gpsCount % 20);
            }
            else if (gpsCount is >= 40 and < 60)
            {
                _latestGps.Latitude = Latitude + 0.001 * (19 - gpsCount % 20);
                _latestGps.Longitude = Longitude + 0.001 * 19;
            }
            else if (gpsCount is >= 60 and < 80)
            {
                _latestGps.Latitude = Latitude;
                _latestGps.Longitude = Longitude + 0.001 * (19 - gpsCount % 20);
            }
            else if (gpsCount >= 80)
            {
                gpsCount = 0;
            }

            gpsCount++;
            SendMessageData(new List<object> { _latestGps });
            Thread.Sleep(_cycleTime);
        }
    }

    #region 成员变量

    /// <summary>
    ///     GPS数据发送线程
    /// </summary>
    private Thread _gpsSender;

    /// <summary>
    ///     缓存最新的GPS
    /// </summary>
    private SDataGps _latestGps;

    /// <summary>
    ///     缓存内嵌的GPS数据集合
    /// </summary>
    private IEnumerable<SDataGps> _embeddedGpsCollection;

    #endregion

    #region 辅助方法

    /// <summary>
    ///     从数据流获取GPS
    /// </summary>
    /// <param name="stream"></param>
    private IEnumerable<SDataGps> GetEmbeddedGpsCollection(Stream stream)
    {
        if (stream == null) return null;
        var embeddedGps = new List<SDataGps>();
        using (var streamReader = new StreamReader(stream))
        {
            var content = streamReader.ReadToEnd();
            if (!string.IsNullOrEmpty(content))
            {
                const double epsilon = 1.0E-6d; // 定义一个极小值，用作浮点数与零进行比较
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var values = line.Split(new[] { ',' });
                    if (values is not { Length: 2 }) continue;
                    // 非正确的数值或解析结果不是有效的GPS数据，则直接跳过
                    if (!double.TryParse(values[0], out var longitude) || !double.TryParse(values[1], out var latitude)
                                                                       || Math.Abs(longitude - -1) <= epsilon ||
                                                                       Math.Abs(latitude - -1) <= epsilon)
                        continue;
                    embeddedGps.Add(new SDataGps { Longitude = longitude, Latitude = latitude });
                }
            }
        }

        return embeddedGps.Count == 0 ? null : embeddedGps;
    }

    /// <summary>
    ///     获取随机数种子
    /// </summary>
    private int GetRandomSeed()
    {
        var tick = DateTime.Now.Ticks;
        return (int)(tick & 0xffffffffL) | (int)(tick >> 32) | Guid.NewGuid().GetHashCode();
    }

    #endregion
}