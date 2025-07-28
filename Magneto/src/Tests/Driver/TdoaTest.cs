using System;
using Magneto.Core.Define;
using Magneto.Driver.TDOA;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class TdoaTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        _tdoa = new Tdoa(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _tdoa?.Dispose();
        _tdoa = null;
        _module = null;
    }

    private ModuleInfo _module;
    private Tdoa _tdoa;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        if (_tdoa == null) Assert.Fail("没有TDOA驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            _tdoa.Initialized(_module);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    [Order(2)]
    [TestCase("Frequency", 101.7d)]
    public void SetParameterTest(string name, object value)
    {
        if (_tdoa == null) Assert.Fail("没有TDOA驱动");
        try
        {
            _tdoa.SetParameter(name, value);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    [TestCase(MediaType.Audio)]
    [TestCase(MediaType.Dfind)]
    [TestCase(MediaType.Iq)]
    [TestCase(MediaType.Itu)]
    [TestCase(MediaType.Level)]
    [TestCase(MediaType.None)]
    [TestCase(MediaType.Scan)]
    [TestCase(MediaType.Spectrum)]
    [TestCase(MediaType.Tdoa)]
    public void StartTest(MediaType media)
    {
        if (_tdoa == null) Assert.Fail("没有TDOA驱动");
        try
        {
            var taskId = Guid.NewGuid();
            var port = new DataPort(taskId);
            _tdoa.Start(port, media);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(4)]
    public void StopTest()
    {
        if (_tdoa == null) Assert.Fail("没有TDOA驱动");
        try
        {
            _tdoa.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}