using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class DemoReceiverTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring,
            ModuleType = ModuleType.Device,
            State = ModuleState.Idle
        };
        // _socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // var endPoint = new IPEndPoint(IPAddress.Any, _port);
        // try
        // {
        //     _socketServer.Bind(endPoint);
        //     _socketServer.Listen(100);
        // }
        // catch
        // {
        //     Console.WriteLine("socket服务端启动失败");
        // }
        _receiver = new DemoReceiver(_module.Id);
        _are = new AutoResetEvent(false);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _receiver?.Dispose();
        _socketServer?.Dispose();
        _socketServer = null;
        _receiver = null;
        _module = null;
    }

    private DemoReceiver _receiver;
    private ModuleInfo _module;
    private Socket _socketServer;
    private AutoResetEvent _are;
    private FeatureType _onTestingFeature = FeatureType.None;
    private bool _sign;

    [Test]
    [Order(1)]
    public void InitializeTest()
    {
        if (_receiver == null) Assert.Fail("没有接收机");
        if (_module == null) Assert.Fail("没有设备");
        try
        {
            // _receiver.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            // _receiver.SetParameter(ParameterNames.Port, _port);
            var init = _receiver.Initialized(_module);
            Assert.AreEqual(init, true);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(2)]
    [TestCase(FeatureType.FFDF)]
    [TestCase(FeatureType.FFM)]
    [TestCase(FeatureType.IFMCA)]
    [TestCase(FeatureType.MScan)]
    [TestCase(FeatureType.SCAN)]
    [TestCase(FeatureType.SSE)]
    // [TestCase(FeatureType.TDOA)]
    public void StartTest(FeatureType feature)
    {
        if (_receiver == null) Assert.Fail("没有接收机");
        var taskId = Guid.NewGuid();
        var port = new DataPort(taskId);
        port.DataArrived += DataArrived;
        try
        {
            _onTestingFeature = feature;
            switch (feature)
            {
                case FeatureType.FFDF:
                case FeatureType.FFM:
                case FeatureType.SSE:
                case FeatureType.TDOA:
                    _receiver.SetParameter(ParameterNames.Frequency, 101.7d);
                    _receiver.SetParameter(ParameterNames.SpectrumSwitch, true);
                    break;
                case FeatureType.IFMCA:
                    _receiver.SetParameter(ParameterNames.Frequency, 101.7d);
                    _receiver.SetParameter("maxChanCount", 32);
                    _receiver.SetParameter("maxAudioCount", 32);
                    var ddcs = new Dictionary<string, object>[2];
                    for (var i = 0; i < ddcs.Length; i++)
                    {
                        var freq = 100 + i * 10;
                        ddcs[i] = new Dictionary<string, object>
                        {
                            { ParameterNames.Frequency, freq },
                            { ParameterNames.FilterBandwidth, 150 },
                            { ParameterNames.DemMode, "fm" },
                            { ParameterNames.AudioSwitch, false },
                            { ParameterNames.LevelSwitch, true },
                            { ParameterNames.SpectrumSwitch, true },
                            { "ifSwitch", true }
                        };
                    }

                    _receiver.SetParameter(ParameterNames.DdcChannels, ddcs);
                    break;
                case FeatureType.MScan:
                    var points = new Dictionary<string, object>[2];
                    for (var i = 0; i < points.Length; i++)
                    {
                        var freq = 100 + i * 10;
                        points[i] = new Dictionary<string, object>
                        {
                            { ParameterNames.Frequency, freq },
                            { ParameterNames.FilterBandwidth, 150 },
                            { ParameterNames.DemMode, "fm" },
                            { ParameterNames.MeasureThreshold, 10 }
                        };
                    }

                    _receiver.SetParameter(ParameterNames.MscanPoints, points);
                    break;
                case FeatureType.SCAN:
                    _receiver.SetParameter(ParameterNames.StartFrequency, 86);
                    _receiver.SetParameter(ParameterNames.StopFrequency, 108);
                    _receiver.SetParameter(ParameterNames.StepFrequency, 25);
                    break;
                default:
                    Assert.Fail($"单元测试未完善此功能:{feature}");
                    return;
            }

            _sign = false;
            _are.Reset();
            _receiver.Start(feature, port);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }

        _are.WaitOne(5000);
        _receiver.Stop();
        port.DataArrived -= DataArrived;
        if (!_sign) Assert.Fail($"功能{feature}测试未通过");
    }

    private void DataArrived(Guid taskId, List<object> data)
    {
        switch (_onTestingFeature)
        {
            case FeatureType.FFDF:
                var df = data.Find(item => item is SDataDfind);
                if (df != null)
                {
                    _sign = true;
                    _are.Set();
                }

                break;
            case FeatureType.FFM:
                var spectrum = data.Find(item => item is SDataSpectrum);
                if (spectrum != null)
                {
                    _sign = true;
                    _are.Set();
                }

                break;
            case FeatureType.SSE:
                var sse = data.Find(item => item is SDataSse);
                if (sse != null)
                {
                    _sign = true;
                    _are.Set();
                }

                break;
            case FeatureType.TDOA:
                break;
            case FeatureType.IFMCA:
                var ddc = data.Find(item => item is SDataDdc);
                if (ddc != null)
                {
                    _sign = true;
                    _are.Set();
                }

                break;
            case FeatureType.MScan:
            case FeatureType.SCAN:
                var scan = data.Find(item => item is SDataScan);
                if (scan != null)
                {
                    _sign = true;
                    _are.Set();
                }

                break;
            default:
                Assert.Fail($"单元测试未完善此功能:{_onTestingFeature}");
                return;
        }
    }
}