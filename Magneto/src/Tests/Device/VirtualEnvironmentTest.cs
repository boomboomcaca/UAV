using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.VirtualEnvironment;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class VirtualEnvironmentTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        _env = new VirtualEnvironment(_module.Id);
        _are = new AutoResetEvent(false);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _env?.Dispose();
        _env = null;
        _module = null;
    }

    private VirtualEnvironment _env;
    private ModuleInfo _module;
    private AutoResetEvent _are;
    private readonly string _edgeId = "142857";

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        if (_env == null) Assert.Fail("没有环控设备");
        if (_module == null) Assert.Fail("没有设备");
        var init = _env.Initialized(_module);
        Assert.AreEqual(init, true);
    }

    [Test]
    [Order(2)]
    [TestCase("WifiSwitch", SwitchState.On)]
    [TestCase("AirConditionSwitch", SwitchState.On)]
    [TestCase("ACSwitch1", SwitchState.On)]
    [TestCase("DCSwitch1", SwitchState.On)]
    public void SetParameterTest(string name, object value)
    {
        if (_env == null) Assert.Fail("没有环控设备");
        try
        {
            _env.SetParameter(name, value);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    [Timeout(15000)]
    public void SendDataTest()
    {
        if (_env == null) Assert.Fail("没有环控设备");
        var taskId = Guid.NewGuid();
        var port = new DataPort(taskId);
        _env.Start(port, _edgeId);
        port.DataArrived += DataArrived;
        _are.WaitOne();
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        _are.Set();
    }
}