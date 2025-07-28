using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract.Interface;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.SSE;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class SseTest
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
        var bandwidthPara = new Parameter
        {
            Name = ParameterNames.IfBandwidth,
            Owners = new List<string> { devId.ToString() },
            Value = 200
        };
        var dfBandwidthPara = new Parameter
        {
            Name = ParameterNames.DfBandwidth,
            Owners = new List<string> { devId.ToString() },
            Value = 200
        };
        _module.Parameters = new List<Parameter>
        {
            freqPara,
            bandwidthPara,
            dfBandwidthPara
        };
        _driver = new Sse(_module.Id);
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
    private Sse _driver;
    private DemoReceiver _device;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private double _freq;
    private double _bandwidth;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_driver == null) Assert.Fail("没有ESE驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            _device.SetParameter(ParameterNames.Port, 1720);
            _device.SetParameter("sseAzimuthCount", 3);
            _device.SetParameter("simAzimuth", 30);
            var devModule = new ModuleInfo
            {
                Id = _device.Id,
                Category = ModuleCategory.Monitoring
            };
            _device.Initialized(devModule);
            _driver.SetParameter(ParameterNames.Receiver, _device);
            _driver.SetParameter("azimuthCount", 3);
            _driver.SetParameter("receivers", new IDevice[] { _device });
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
        if (_driver == null) Assert.Fail("没有SSE驱动");
        try
        {
            _freq = 110;
            _bandwidth = 500;
            _driver.SetParameter(ParameterNames.Frequency, _freq);
            _driver.SetParameter(ParameterNames.IfBandwidth, _bandwidth);
            _driver.SetParameter(ParameterNames.DfBandwidth, _bandwidth);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private DataPort _dataPort;

    [Test]
    [Order(3)]
    public void StartTemplateTest()
    {
        Console.WriteLine("启动空间谱测试");
        if (_driver == null) Assert.Fail("没有SSE驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private int _count;

    private void DataArrived(Guid taskId, List<object> data)
    {
        var sse = (SDataSse)data.Find(item => item is SDataSse);
        if (sse != null)
        {
            if (Math.Abs(sse.Frequency - _freq) > 1e-9
                || Math.Abs(sse.Bandwidth - _bandwidth) > 1e-9)
                Assert.Fail("返回数据与参数不匹配");
            _count++;
            Console.WriteLine($"接收到空间谱数据{_count}");
            if (_count >= 100) _autoResetEvent.Set();
        }
    }

    [Test]
    [Order(4)]
    public void StopTemplateTest()
    {
        _autoResetEvent.WaitOne(20000);
        if (_count == 0) Assert.Fail("无数据返回");
        if (_driver == null) Assert.Fail("没有SSE驱动");
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