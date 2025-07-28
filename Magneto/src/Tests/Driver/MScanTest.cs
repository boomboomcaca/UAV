using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Core.Statistics;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.MScan;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class MScanTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        var devId = Guid.NewGuid();
        _device = new DemoReceiver(devId);
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        var freqs = new Parameter
        {
            Name = ParameterNames.MscanPoints,
            Owners = new List<string> { devId.ToString() },
            Value = null
        };
        var squelchSwitch = new Parameter
        {
            Name = ParameterNames.SquelchSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = false
        };
        var spectrumSwitch = new Parameter
        {
            Name = ParameterNames.SpectrumSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = false
        };
        var audioSwitch = new Parameter
        {
            Name = ParameterNames.AudioSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = false
        };
        var squelchThreshold = new Parameter
        {
            Name = ParameterNames.SquelchThreshold,
            Owners = new List<string> { devId.ToString() },
            Value = 0
        };
        var holdTime = new Parameter
        {
            Name = ParameterNames.HoldTime,
            Owners = new List<string> { devId.ToString() },
            Value = 0
        };
        var dwellTime = new Parameter
        {
            Name = ParameterNames.DwellTime,
            Owners = new List<string> { devId.ToString() },
            Value = 0
        };
        _module.Parameters = new List<Parameter>
        {
            freqs, squelchSwitch, squelchThreshold, holdTime, dwellTime, spectrumSwitch, audioSwitch
        };
        _driver = new MScan(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        if (_dataPort != null) _dataPort.DataArrived -= DataArrived;
        _driver?.Dispose();
        _driver = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private MScan _driver;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private DataPort _dataPort;

    /// <summary>
    ///     获取数据的个数
    /// </summary>
    private int _count;

    /// <summary>
    ///     错误信息
    /// </summary>
    private string _errMsg = string.Empty;

    /// <summary>
    ///     平均收到数据的间隔
    /// </summary>
    private double _avgSpan;

    private double _maxSpan;
    private double _minSpan = double.MaxValue;
    private bool _recvDataError;
    private DateTime _lastTime = DateTime.Now;
    private readonly MScanProcess _process = new(null);

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_driver == null) Assert.Fail("没有驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            _device.SetParameter(ParameterNames.Port, 1720);
            var devModule = new ModuleInfo
            {
                Id = _device.Id,
                Category = ModuleCategory.Monitoring
            };
            _device.Initialized(devModule);
            _driver.SetParameter(ParameterNames.Receiver, _device);
            _driver.Initialized(_module);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    private Dictionary<string, object>[] _freqs;

    [Test]
    [Order(2)]
    public void SetParameterTest()
    {
        Console.WriteLine("设置参数测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            var freqs = new List<Dictionary<string, object>>();
            var dic1 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 101.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 40 }
            };
            freqs.Add(dic1);
            var dic2 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 102.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 60 }
            };
            freqs.Add(dic2);
            var dic3 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 103.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 50 }
            };
            freqs.Add(dic3);
            _freqs = freqs.ToArray();
            _max = new short[_freqs.Length];
            _min = Enumerable.Repeat(short.MaxValue, _freqs.Length).ToArray();
            _avg = new short[_freqs.Length];
            _occ = new float[_freqs.Length];
            _counts = new int[_freqs.Length];
            _driver.SetParameter(ParameterNames.MscanPoints, _freqs);
            _driver.SetParameter("measureSwitch", true);
            var para = new Parameter
            {
                Name = ParameterNames.MscanPoints,
                Value = _freqs
            };
            _process.SetParameter(para);
            para = new Parameter
            {
                Name = "measureSwitch",
                Value = true
            };
            _process.SetParameter(para);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    public void StartTest()
    {
        Console.WriteLine("启动任务测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _process.Start();
            _process.DataProcessComplate += DataProcessComplate;
            _driver.Start(_dataPort, MediaType.None);
            _lastTime = DateTime.Now;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private void DataProcessComplate(object sender, List<object> e)
    {
        var occ = (SDataOccupancy)e.Find(item => item is SDataOccupancy);
        if (occ != null) _occ = occ.Data[0].Occupancy.Select(item => (float)item).ToArray();
    }

    private short[] _max;
    private short[] _min;
    private short[] _avg;
    private float[] _occ;
    private int[] _counts;

    private void DataArrived(Guid taskId, List<object> data)
    {
        if (_freqs == null || _freqs.Length == 0)
        {
            _errMsg = "频点信息为空！";
            _recvDataError = true;
            _autoResetEvent.Set();
        }

        var scan = (SDataScan)data.Find(item => item is SDataScan);
        if (scan != null)
        {
            if (_freqs != null && scan.Offset >= _freqs.Length)
            {
                _errMsg = "返回数据有误";
                _recvDataError = true;
                return;
            }

            _max[scan.Offset] = Math.Max(_max[scan.Offset], scan.Data[0]);
            _min[scan.Offset] = Math.Min(_min[scan.Offset], scan.Data[0]);
            _counts[scan.Offset]++;
            _avg[scan.Offset] = (short)((_avg[scan.Offset] * (_counts[scan.Offset] - 1) + scan.Data[0]) /
                                        (float)_counts[scan.Offset]);
            _count++;
            var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            _maxSpan = Math.Max(span, _maxSpan);
            _minSpan = Math.Min(span, _minSpan);
            _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
        }

        _process?.OnData(data);
    }

    [Test]
    [Order(4)]
    public void StopTest()
    {
        if (_driver == null) Assert.Fail("没有MScan驱动");
        _autoResetEvent.WaitOne(20000);
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("无数据返回");
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan}");
        var str = ArrayToString(_avg);
        Console.WriteLine($"平均值:{str}");
        str = ArrayToString(_occ);
        Console.WriteLine($"占用度{str}");
        try
        {
            _dataPort.DataArrived -= DataArrived;
            _driver.Stop();
            _process.DataProcessComplate -= DataProcessComplate;
            _process.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(5)]
    public void StartMScneTest()
    {
        Console.WriteLine("启动驻留离散扫描测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            var freqs = new List<Dictionary<string, object>>();
            var dic1 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 101.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 40 }
            };
            freqs.Add(dic1);
            var dic2 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 102.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 60 }
            };
            freqs.Add(dic2);
            var dic3 = new Dictionary<string, object>
            {
                { ParameterNames.Frequency, 103.7d },
                { ParameterNames.FilterBandwidth, 120d },
                { ParameterNames.DemMode, "FM" },
                { ParameterNames.MeasureThreshold, 50 }
            };
            freqs.Add(dic3);
            _freqs = freqs.ToArray();
            _driver.SetParameter(ParameterNames.MscanPoints, _freqs);
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrivedMScne;
            _driver.SetParameter("dwellSwitch", true);
            _driver.SetParameter(ParameterNames.SquelchSwitch, true);
            _driver.SetParameter(ParameterNames.SquelchThreshold, 0);
            _driver.SetParameter(ParameterNames.HoldTime, 5);
            _driver.SetParameter(ParameterNames.DwellTime, 5);
            _driver.SetParameter(ParameterNames.SpectrumSwitch, true);
            _driver.SetParameter(ParameterNames.AudioSwitch, true);
            _driver.Start(_dataPort, MediaType.None);
            _lastTime = DateTime.Now;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private bool _hasSpec;

    private void DataArrivedMScne(Guid taskId, List<object> data)
    {
        if (_freqs == null || _freqs.Length == 0)
        {
            _errMsg = "频点信息为空！";
            _recvDataError = true;
            _autoResetEvent.Set();
        }

        var scan = (SDataScan)data.Find(item => item is SDataScan);
        if (scan != null)
        {
            if (_freqs != null && scan.Offset >= _freqs.Length)
            {
                _errMsg = "返回数据有误";
                _recvDataError = true;
                return;
            }

            _count++;
            var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            _maxSpan = Math.Max(span, _maxSpan);
            _minSpan = Math.Min(span, _minSpan);
            _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
        }

        var spec = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        if (spec != null) _hasSpec = true;
        _process?.OnData(data);
    }

    [Test]
    [Order(6)]
    public void StopMScneTest()
    {
        if (_driver == null) Assert.Fail("没有驱动");
        _autoResetEvent.WaitOne(20000);
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("无数据返回");
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan},频谱数据:{_hasSpec}");
        try
        {
            _dataPort.DataArrived -= DataArrivedMScne;
            _driver.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private string ArrayToString(Array array)
    {
        var str = "";
        foreach (var item in array) str += item + ",";
        return str.TrimEnd(',');
    }
}