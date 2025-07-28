using System;
using System.Collections.Generic;
using Magneto.Core.Define;
using Magneto.Device.DC7530MOB;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Device;

public class Dc7530BTest
{
    private DataPort _dataPort;
    private Dc7530Mob _device;
    private ModuleInfo _module;

    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Decoder,
            ModuleType = ModuleType.Device,
            State = ModuleState.Idle
        };
        _device = new Dc7530Mob(_module.Id);
    }

    [Test]
    [Order(1)]
    public void InitializedTest()
    {
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "192.168.30.201");
            _device.SetParameter(ParameterNames.Port, 10002);
            var init = _device.Initialized(_module);
            Assert.AreEqual(init, true);
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
        try
        {
            _dataPort = new DataPort(Guid.NewGuid());
            _dataPort.DataArrived += DataArrived;
            _dataPort.MessageArrived += MessageArrived;
            _device.Start(FeatureType.BsDecoding, _dataPort);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    private void MessageArrived(SDataMessage message)
    {
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        Console.WriteLine(JsonConvert.SerializeObject(data));
    }
}