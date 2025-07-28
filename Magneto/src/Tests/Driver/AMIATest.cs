using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.AMIA;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class AmiaTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        RunningInfo.EdgeId = "142857";
        RunningInfo.UpdateGps(new SDataGps { Longitude = 104, Latitude = 30 });
        if (!HttpHelper.Instance.IsInitialized)
        {
            HttpHelper.Instance.Initialized("192.168.102.16", 12001);
            CloudClient.Instance.GetCloudTokenAsync("admin", "123456").ConfigureAwait(true).GetAwaiter().GetResult();
        }

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
        _driver = new Amia(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _driver?.Dispose();
        _driver = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private Amia _driver;
    private DemoReceiver _device;
    private Dictionary<string, object>[] _segments;
    private readonly AutoResetEvent _autoResetEvent = new(false);

    [Test(Description = "初始化测试")]
    [Order(1)]
    public void InitializeTest()
    {
        if (_driver == null) Assert.Fail("没有AMIA驱动");
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

    [Test(Description = "设置参数测试")]
    [Order(2)]
    public void SetScanSegmentsParameterTest()
    {
        Console.WriteLine("设置频段参数测试");
        if (_driver == null) Assert.Fail("没有AMIA驱动");
        try
        {
            var segments = new Dictionary<string, object>[2];
            var seg = new SegmentTemplate
            {
                StartFrequency = 86,
                StopFrequency = 108,
                StepFrequency = 25
            };
            var dic = seg.ToDictionary();
            segments[0] = dic;
            _segments = segments;
            _driver.SetParameter(ParameterNames.ScanSegments, segments);
            _driver.SetParameter("occupancyThreshold", 20);
            _driver.SetParameter(ParameterNames.ThresholdValue, 6);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}