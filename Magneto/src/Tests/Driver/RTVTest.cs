using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.CA300B;
using Magneto.Device.HCNetDVR;
using Magneto.Driver.RTV;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class RtvTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _moduleCa300B = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Decoder,
            ModuleType = ModuleType.Device,
            State = ModuleState.Idle
        };
        _ca300B = new Ca300B(_moduleCa300B.Id);
        _moduleDvr = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Decoder,
            ModuleType = ModuleType.Device,
            State = ModuleState.Idle
        };
        _dvr = new HcNetDvr(_moduleDvr.Id);
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Decoder,
            ModuleType = ModuleType.Driver,
            State = ModuleState.Idle,
            Parameters = new List<Parameter>
            {
                new()
                {
                    Name = "playProgram",
                    Owners = new List<string> { _moduleCa300B.Id.ToString(), _moduleDvr.Id.ToString() }
                }
            }
        };
        _driver = new Rtv(_module.Id);
        _driver.SetParameter("avReceiver", _dvr);
        _driver.SetParameter("tvAnalysis", _ca300B);
    }

    private Rtv _driver;
    private ModuleInfo _module;
    private ModuleInfo _moduleDvr;
    private ModuleInfo _moduleCa300B;
    private Ca300B _ca300B;
    private HcNetDvr _dvr;
    private DataPort _dataPort;
    private readonly AutoResetEvent _event = new(false);

    [Test]
    public void InitializedTest()
    {
        try
        {
            _dvr.SetParameter(ParameterNames.IpAddress, "192.168.6.64");
            _dvr.SetParameter(ParameterNames.Port, 8000);
            _dvr.SetParameter("userName", "admin");
            _dvr.SetParameter("password", "decentest123");
            _dvr.Initialized(_moduleDvr);
            _ca300B.SetParameter(ParameterNames.IpAddress, "192.168.6.33");
            _ca300B.Initialized(_moduleCa300B);
            _driver.Initialized(_module);
            _dataPort = new DataPort(Guid.NewGuid());
            _dataPort.DataArrived += DataArrived;
            _driver.Start(_dataPort, MediaType.None);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
    }

    [Test]
    public void RealPlayTest()
    {
        _driver.SetParameter("playProgram", "DTMB|482|1111|SCTV1");
        _event.WaitOne();
    }
}