using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.VirtualGps;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class VirtualGpsTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        _gps = new VirtualGps(_module.Id);
        _are = new AutoResetEvent(false);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _gps?.Dispose();
        _gps = null;
        _module = null;
    }

    private VirtualGps _gps;
    private ModuleInfo _module;
    private AutoResetEvent _are;

    [Test]
    [Order(2)]
    public void InitializeTest()
    {
        if (_gps == null) Assert.Fail("没有GPS");
        if (_module == null) Assert.Fail("没有设备");
        var init = _gps.Initialized(_module);
        Assert.AreEqual(init, true);
    }

    [Test]
    [Order(1)]
    [TestCase("Longitude", 104.2d)]
    [TestCase("Latitude", 30.8d)]
    [TestCase("UpdatingData", true)]
    [TestCase("CycleTime", 1000)]
    public void SetParameterTest(string name, object value)
    {
        if (_gps == null) Assert.Fail("没有GPS");
        try
        {
            _gps.SetParameter(name, value);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    [Timeout(5000)]
    public void SendDataTest()
    {
        if (_gps == null) Assert.Fail("没有GPS");
        var taskId = Guid.NewGuid();
        var port = new DataPort(taskId);
        _gps.Attach(port);
        port.DataArrived += DataArrived;
        _are.WaitOne();
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        _are.Set();
    }
}