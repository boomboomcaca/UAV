using System;
using Magneto.Contract.Interface;
using Magneto.Device.DemoAntennaController;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class DemoAntennaControllerTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.AntennaControl
        };
        _controller = new DemoAntennaController(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _controller?.Dispose();
        _controller = null;
        _module = null;
    }

    private DemoAntennaController _controller;
    private ModuleInfo _module;

    [Test]
    [Order(1)]
    [TestCase(500000)]
    [TestCase(1000000)]
    public void GetFactorTest(double freq)
    {
        if (_controller == null) Assert.Fail("没有天线控制器");
        _controller.GetFactor(freq);
    }

    [Test]
    [Order(2)]
    public void InitializeTest()
    {
        if (_controller == null) Assert.Fail("没有天线控制器");
        if (_module == null) Assert.Fail("没有设备");
        try
        {
            var init = _controller.Initialized(_module);
            Assert.AreEqual(init, true);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    [TestCase("testCode")]
    public void SendControlCodeTest(string code)
    {
        if (_controller == null) Assert.Fail("没有天线控制器");
        try
        {
            var result = _controller.SendControlCode(code);
            Assert.AreEqual(result, true);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(4)]
    [TestCase("PolarityType", Polarization.Horizontal)]
    [TestCase("PolarityType", Polarization.Vertical)]
    public void SetParameterTest(string name, object value)
    {
        if (_controller == null) Assert.Fail("没有天线控制器");
        try
        {
            _controller.SetParameter(name, value);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}