using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.UAVD;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class UavdTest
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
        _module.Parameters = new List<Parameter>
        {
            startPara,
            stopPara,
            stepPara
        };
        _driver = new Uavd(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _driver?.Stop();
        _driver?.Dispose();
        _driver = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private Uavd _driver;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private double _startFrequency;
    private double _stopFrequency;
    private double _stepFrequency;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_driver == null) Assert.Fail("没有UAVD驱动");
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

    [Test]
    [Order(2)]
    public void SetParameterTest()
    {
        Console.WriteLine("设置参数测试");
        if (_driver == null) Assert.Fail("没有UAVD驱动");
        try
        {
            _startFrequency = 86;
            _stopFrequency = 108;
            _stepFrequency = 25;
            _driver.SetParameter(ParameterNames.StartFrequency, 86);
            _driver.SetParameter(ParameterNames.StopFrequency, 108);
            _driver.SetParameter(ParameterNames.StepFrequency, 25);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private DataPort _dataPort;

    [Test]
    [Order(4)]
    public void StartTest()
    {
        Console.WriteLine("启动无人机任务");
        if (_driver == null) Assert.Fail("没有UAVD驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _lastTime = DateTime.Now;
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

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

    private void DataArrived(Guid taskId, List<object> data)
    {
        var scan = (SDataScan)data.Find(item => item is SDataScan);
        if (scan != null)
        {
            if (Math.Abs(scan.StartFrequency - _startFrequency) > 1e-9
                || Math.Abs(scan.StopFrequency - _stopFrequency) > 1e-9
                || Math.Abs(scan.StepFrequency - _stepFrequency) > 1e-9)
            {
                _errMsg = "返回数据的频段个数与下发的参数不匹配";
                _recvDataError = true;
                _autoResetEvent.Set();
            }

            _count++;
            var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            _maxSpan = Math.Max(span, _maxSpan);
            _minSpan = Math.Min(span, _minSpan);
            _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
            if (_count >= 100) _autoResetEvent.Set();
        }
    }

    [Test]
    [Order(5)]
    public void StopTemplateTest()
    {
        _autoResetEvent.WaitOne(20000);
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan}");
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("无数据返回");
        if (_driver == null) Assert.Fail("没有UAVD驱动");
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