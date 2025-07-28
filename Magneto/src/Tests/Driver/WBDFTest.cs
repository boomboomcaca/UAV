using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.WBDF;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class WbdfTest
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
        var dfsc = new Parameter
        {
            Name = "dfSamplingCount",
            Owners = new List<string> { devId.ToString() },
            Value = 2048
        };
        _module.Parameters = new List<Parameter>
        {
            freqPara,
            bwPara,
            dfsc
        };
        _driver = new Wbdf(_module.Id);
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
    private Wbdf _driver;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private DataPort _dataPort;
    private int _countSpec;
    private int _countDfPan;

    /// <summary>
    ///     平均收到数据的间隔
    /// </summary>
    private double _avgSpanSpec;

    private double _avgSpanDfPan;
    private double _maxSpanSpec;
    private double _maxSpanDfPan;
    private double _minSpanSpec = double.MaxValue;
    private double _minSpanDfPan = double.MaxValue;
    private DateTime _lastTimeSpec = DateTime.Now;
    private DateTime _lastTimeDfPan = DateTime.Now;
    private double _frequency;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_driver == null) Assert.Fail("没有功能驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            // _device.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            // _device.SetParameter(ParameterNames.Port, 1720);
            var devModule = new ModuleInfo
            {
                Id = _device.Id,
                Category = ModuleCategory.Monitoring
            };
            _device.Initialized(devModule);
            _driver.SetParameter(ParameterNames.Dfinder, _device);
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
        if (_driver == null) Assert.Fail("没有功能驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _lastTimeDfPan = DateTime.Now;
            _lastTimeSpec = DateTime.Now;
            _driver.SetParameter(ParameterNames.SpectrumSwitch, true);
            _driver.SetParameter("dfSamplingCount", 2048);
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private bool _isStartTest;

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
            _countSpec = 0;
            if (name.Equals(ParameterNames.Frequency)) _frequency = Convert.ToDouble(value);
            _lastTimeSpec = DateTime.Now;
            _driver.SetParameter(name, value);
            _isStartTest = true;
            _autoResetEvent.WaitOne(3000);
            _isStartTest = false;
            if (_countSpec == 0) Assert.Fail($"参数下发失败 name:{name},value:{value}");
            Console.WriteLine($"获取数据个数:{_countSpec},平均间隔{_avgSpanSpec},最大间隔{_maxSpanSpec},最小间隔{_minSpanSpec}");
            _countSpec = 0;
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
        if (spectrum != null && Math.Abs(_frequency - spectrum.Frequency) < 1e-9)
        {
            _countSpec++;
            var span = DateTime.Now.Subtract(_lastTimeSpec).TotalMilliseconds;
            _lastTimeSpec = DateTime.Now;
            _maxSpanSpec = Math.Max(span, _maxSpanSpec);
            _minSpanSpec = Math.Min(span, _minSpanSpec);
            _avgSpanSpec = (_avgSpanSpec * (_countSpec - 1) + span) / _countSpec;
        }

        var dfpan = (SDataDfpan)data.Find(item => item is SDataDfpan);
        if (dfpan != null)
        {
            _countDfPan++;
            var span = DateTime.Now.Subtract(_lastTimeDfPan).TotalMilliseconds;
            _lastTimeDfPan = DateTime.Now;
            _maxSpanDfPan = Math.Max(span, _maxSpanDfPan);
            _minSpanDfPan = Math.Min(span, _minSpanDfPan);
            _avgSpanDfPan = (_avgSpanDfPan * (_countDfPan - 1) + span) / _countDfPan;
        }
    }

    [Test]
    [Order(4)]
    public void StopTest()
    {
        Console.WriteLine("停止测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            Console.WriteLine($"获取DfPan数据个数:{_countDfPan},平均间隔{_avgSpanDfPan},最大间隔{_maxSpanDfPan},最小间隔{_minSpanDfPan}");
            _dataPort.DataArrived -= DataArrived;
            _driver.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}