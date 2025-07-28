using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.ITUM;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class ItumTest
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
        var specSwitch = new Parameter
        {
            Name = ParameterNames.SpectrumSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = true
        };
        var ituSwitch = new Parameter
        {
            Name = ParameterNames.ItuSwitch,
            Owners = new List<string> { devId.ToString() },
            Value = true
        };
        _module.Parameters = new List<Parameter>
        {
            freqPara,
            bwPara,
            specSwitch,
            ituSwitch
        };
        _driver = new Itum(_module.Id);
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
    private Itum _driver;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private DataPort _dataPort;
    private int _count;

    /// <summary>
    ///     平均收到数据的间隔
    /// </summary>
    private double _avgSpan;

    private double _maxSpan;
    private double _minSpan = double.MaxValue;
    private DateTime _lastTime = DateTime.Now;
    private double _frequency;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_driver == null) Assert.Fail("没有驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
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

    [Test]
    [Order(2)]
    public void StartTest()
    {
        Console.WriteLine("启动测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _lastTime = DateTime.Now;
            _driver.SetParameter(ParameterNames.SpectrumSwitch, true);
            // _driver.SetParameter(ParameterNames.ITUSwitch, true);
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private bool _isStartTest;
    private bool _isCatchItuData;

    [Test]
    [Order(3)]
    [TestCase(ParameterNames.Frequency, 101.7d)]
    [TestCase(ParameterNames.Frequency, 102.6d)]
    [TestCase(ParameterNames.Frequency, 106.1d)]
    public void SetParameterTest(string name, object value)
    {
        Console.WriteLine($"设置参数测试 name:{name},value:{value}");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            _isStartTest = false;
            _isCatchItuData = false;
            _count = 0;
            if (name.Equals(ParameterNames.Frequency)) _frequency = Convert.ToDouble(value);
            _lastTime = DateTime.Now;
            _driver.SetParameter(name, value);
            _isStartTest = true;
            _autoResetEvent.WaitOne(3000);
            _isStartTest = false;
            if (_count == 0) Assert.Fail($"参数下发失败 name:{name},value:{value}");
            Console.WriteLine(
                $"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan},获取ITU数据:{_isCatchItuData}");
            _count = 0;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        if (!_isStartTest) return;
        var spectrum = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        if (spectrum != null)
            if (Math.Abs(_frequency - spectrum.Frequency) < 1e-9)
            {
                _count++;
                var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
                _lastTime = DateTime.Now;
                _maxSpan = Math.Max(span, _maxSpan);
                _minSpan = Math.Min(span, _minSpan);
                _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
            }

        var itu = (SDataItu)data.Find(item => item is SDataItu);
        if (itu != null)
            if (Math.Abs(_frequency - itu.Frequency) < 1e-9)
                _isCatchItuData = true;
    }

    [Test]
    [Order(5)]
    public void StopTest()
    {
        Console.WriteLine("停止测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            _dataPort.DataArrived -= DataArrived;
            _driver.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}