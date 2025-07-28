using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.VirtualCompass;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class VirtualCompassTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        _compass = new VirtualCompass(_module.Id);
        _are = new AutoResetEvent(false);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _compass?.Dispose();
        _compass = null;
        _module = null;
    }

    private VirtualCompass _compass;
    private ModuleInfo _module;
    private AutoResetEvent _are;

    [Test]
    [Order(2)]
    public void InitializeTest()
    {
        if (_compass == null) Assert.Fail("没有罗盘");
        if (_module == null) Assert.Fail("没有设备");
        var init = _compass.Initialized(_module);
        Assert.AreEqual(init, true);
    }

    [Test]
    [Order(1)]
    [TestCase("IsMove", true)]
    [TestCase("Degree", 30f)]
    [TestCase("Step", 1f)]
    public void SetParameterTest(string name, object value)
    {
        if (_compass == null) Assert.Fail("没有罗盘");
        try
        {
            _compass.SetParameter(name, value);
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
        if (_compass == null) Assert.Fail("没有罗盘");
        var taskId = Guid.NewGuid();
        var port = new DataPort(taskId);
        _compass.Attach(port);
        port.DataArrived += DataArrived;
        _are.WaitOne();
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        _are.Set();
    }
}