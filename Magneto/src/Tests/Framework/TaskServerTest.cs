using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.CCC;
using Magneto.Core;
using Magneto.Core.Business;
using Magneto.Core.Configuration;
using Magneto.Protocol.Define;
using NUnit.Framework;
using StreamJsonRpc;

namespace Tests.Core;

[TestFixture]
public class TaskServerTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _server = new ControlServer(_client);
        Manager.Instance.Initialized();
        Thread.Sleep(10000);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _server?.Dispose();
        _server = null;
    }

    private readonly ClientInfo _client = new("127.0.0.1", 1);
    private ControlServer _server;
    private ModuleInfo _module;
    private Guid _taskId;

    [Test]
    [Order(1)]
    public async Task PresetAsync()
    {
        _module = DriverConfig.Instance.Drivers.FirstOrDefault();
        Guid moduleId;
        if (_module != null)
            moduleId = _module.Id;
        else
            moduleId = Guid.NewGuid();
        try
        {
            var result = await _server.PresetTaskAsync(moduleId, string.Empty, string.Empty).ConfigureAwait(false);
            _taskId = result.TaskId;
        }
        catch (LocalRpcException e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(2)]
    public void Start()
    {
        if (_taskId == default) Assert.Fail("没有获取到任务ID");
        try
        {
            _server.StartTask(_taskId);
        }
        catch (LocalRpcException e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(3)]
    public void SetParameter()
    {
        if (_taskId == default) Assert.Fail("没有获取到任务ID");
        try
        {
            var parameter = new Parameter
            {
                Name = "frequency",
                Value = "102.2"
            };
            var list = new List<Parameter>
            {
                parameter
            };
            _server.SetTaskParameters(_taskId, list);
        }
        catch (LocalRpcException e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(4)]
    public void Stop()
    {
        if (_taskId == default) Assert.Fail("没有获取到任务ID");
        try
        {
            _server.StopTask(_taskId);
        }
        catch (LocalRpcException e)
        {
            Assert.Fail(e.Message);
        }
    }
}