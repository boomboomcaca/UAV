using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.ScanDF;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class ScanDfTest
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
        var startPara = new Parameter
        {
            Name = ParameterNames.StartFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 88
        };
        var stopPara = new Parameter
        {
            Name = ParameterNames.StopFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 108
        };
        var stepPara = new Parameter
        {
            Name = ParameterNames.StepFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 25
        };
        var dfsc = new Parameter
        {
            Name = "dfSamplingCount",
            Owners = new List<string> { devId.ToString() },
            Value = 2048
        };
        _module.Parameters = new List<Parameter>
        {
            startPara,
            stopPara,
            stepPara,
            dfsc
        };
        _driver = new ScanDf(_module.Id);
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
    private ScanDf _driver;
    private DemoReceiver _device;
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
            _driver.SetParameter(ParameterNames.StartFrequency, 88);
            _driver.SetParameter(ParameterNames.StopFrequency, 108);
            _driver.SetParameter(ParameterNames.StepFrequency, 25);
            _driver.SetParameter("dfSamplingCount", 2048);
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        var spectrum = (SDataScan)data.Find(item => item is SDataScan);
        if (spectrum != null)
        {
            _countSpec++;
            var span = DateTime.Now.Subtract(_lastTimeSpec).TotalMilliseconds;
            _lastTimeSpec = DateTime.Now;
            _maxSpanSpec = Math.Max(span, _maxSpanSpec);
            _minSpanSpec = Math.Min(span, _minSpanSpec);
            _avgSpanSpec = (_avgSpanSpec * (_countSpec - 1) + span) / _countSpec;
        }

        var dfpan = (SDataDfScan)data.Find(item => item is SDataDfScan);
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
        Thread.Sleep(30000);
        Console.WriteLine("停止测试");
        if (_driver == null) Assert.Fail("没有驱动");
        try
        {
            Console.WriteLine($"获取Scan数据个数:{_countSpec},平均间隔{_avgSpanSpec},最大间隔{_maxSpanSpec},最小间隔{_minSpanSpec}");
            Console.WriteLine(
                $"获取DfScan数据个数:{_countDfPan},平均间隔{_avgSpanDfPan},最大间隔{_maxSpanDfPan},最小间隔{_minSpanDfPan}");
            _dataPort.DataArrived -= DataArrived;
            _driver.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}