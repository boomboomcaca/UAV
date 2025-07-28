using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.FixDF;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class FixDfTest
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
        var freqPara = new Parameter
        {
            Name = ParameterNames.Frequency,
            Owners = new List<string> { devId.ToString() },
            Value = 101.7
        };
        var bwPara = new Parameter
        {
            Name = ParameterNames.IfBandwidth,
            Owners = new List<string> { devId.ToString() },
            Value = 150
        };
        var dfbwPara = new Parameter
        {
            Name = ParameterNames.DfBandwidth,
            Owners = new List<string> { devId.ToString() },
            Value = 120
        };
        _module.Parameters = new List<Parameter>
        {
            freqPara,
            bwPara,
            dfbwPara
        };
        _df = new FixDf(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _df?.Dispose();
        _df = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private FixDf _df;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        if (_df == null) Assert.Fail("没有FixDF驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            _device.SetParameter(ParameterNames.Port, 1720);
            _device.SetParameter("simAzimuth", 50);
            var devModule = new ModuleInfo
            {
                Id = _device.Id,
                Category = ModuleCategory.Monitoring
            };
            _device.Initialized(devModule);
            _df.SetParameter(ParameterNames.Receiver, _device);
            _df.Initialized(_module);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    [Order(2)]
    [TestCase("Frequency", 101.7d)]
    public void SetParameterTest(string name, object value)
    {
        Console.WriteLine("设置参数测试");
        if (_df == null) Assert.Fail("没有FixDF驱动");
        try
        {
            _df.SetParameter(name, value);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private DataPort _dataPort;

    [Test]
    [Order(3)]
    public void StartTest()
    {
        if (_df == null) Assert.Fail("没有FixDF驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _startTime = DateTime.Now;
            _lastTime = DateTime.Now;
            _df.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private int _count;

    /// <summary>
    ///     平均收到数据的间隔
    /// </summary>
    private double _avgSpan;

    private double _maxSpan;
    private double _minSpan = double.MaxValue;
    private double _azimuth;
    private double _maxAzi;
    private double _minAzi = double.MaxValue;
    private DateTime _lastTime = DateTime.Now;
    private DateTime _startTime = DateTime.Now;

    private void DataArrived(Guid taskId, List<object> data)
    {
        var dfind = (SDataDfind)data.Find(item => item is SDataDfind);
        if (dfind != null)
        {
            if (dfind.Azimuth < 0) return;
            _count++;
            var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            _maxSpan = Math.Max(span, _maxSpan);
            _minSpan = Math.Min(span, _minSpan);
            _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
            _azimuth = dfind.OptimalAzimuth;
            _maxAzi = Math.Max(_maxAzi, dfind.Azimuth);
            _minAzi = Math.Min(_minAzi, dfind.Azimuth);
            // if (_count >= 100)
            if (DateTime.Now.Subtract(_startTime).TotalSeconds > 5) _autoResetEvent.Set();
        }
    }

    [Test]
    [Order(4)]
    public void StopTest()
    {
        _autoResetEvent.WaitOne(20000);
        if (_count == 0) Assert.Fail("示向度无数据返回");
        if (_df == null) Assert.Fail("没有FixDF驱动");
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan}");
        Console.WriteLine($"示向度最优值:{_azimuth},最大值{_maxAzi},最小值{_minAzi}");
        try
        {
            _dataPort.DataArrived -= DataArrived;
            _df.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}