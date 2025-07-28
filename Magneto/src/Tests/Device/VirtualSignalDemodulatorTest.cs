using System;
using System.Collections.Generic;
using Magneto.Core.Define;
using Magneto.Device.VirtualSignalDemodulator;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class VirtualSignalDemodulatorTest
{
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
        _device = new SignalDemodulator(_module.Id);
    }

    private SignalDemodulator _device;
    private DataPort _dataPort;
    private ModuleInfo _module;

    [Test]
    [Order(1)]
    public void InitializedTest()
    {
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "192.168.102.81");
            _device.SetParameter(ParameterNames.Port, 22001);
            _device.SetParameter("iqFileName",
                @"E:\00_WORK\Magneto\0506\bin\Debug\AnyCPU\sgldec\Edge_Tao_002-20220610141237041-101.7MHz-500kHz.txt");
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
            _device.Start(FeatureType.SGLDEC, _dataPort);
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