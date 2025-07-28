using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Core.Define;
using Magneto.Device.HCNetDVR;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Device;

[TestFixture]
public class HcNetDvrTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        if (!HttpHelper.Instance.IsInitialized)
        {
            HttpHelper.Instance.Initialized("192.168.102.191", 19101);
            CloudClient.Instance.GetCloudTokenAsync("dc_admin", "456789").ConfigureAwait(true).GetAwaiter().GetResult();
        }

        RunningInfo.EdgeId = "142857";
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Decoder,
            ModuleType = ModuleType.Device,
            State = ModuleState.Idle
        };
        _device = new HcNetDvr(_module.Id);
        _event = new AutoResetEvent(false);
    }

    private HcNetDvr _device;
    private ModuleInfo _module;
    private DataPort _dataPort;
    private AutoResetEvent _event;

    [Test]
    [Order(0)]
    public void InitializedTest()
    {
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "192.168.6.64");
            _device.SetParameter(ParameterNames.Port, 8000);
            _device.SetParameter("userName", "admin");
            _device.SetParameter("password", "decentest123");
            var init = _device.Initialized(_module);
            Assert.AreEqual(init, true);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    [Order(1)]
    public void StartTest()
    {
        try
        {
            _dataPort = new DataPort(Guid.NewGuid());
            _dataPort.DataArrived += DataArrived;
            _dataPort.MessageArrived += MessageArrived;
            _device.Start(FeatureType.AVProcess, _dataPort);
            // _event.WaitOne(10000);
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
        if (data.Find(item => item is SDataDvrFileInfo) is SDataDvrFileInfo files)
        {
            Console.WriteLine($"查询到{files.Count}条录像");
            _dvrFiles = files.Files;
            _event.Set();
        }

        if (data.Find(item => item is SDataPlayResult) is SDataPlayResult result)
        {
            Console.WriteLine(result.Uri);
            if (result.OperateType is OperateType.RecordStart or OperateType.RecordStop)
            {
                _dvrOk = result.Result;
                _errMsg = result.Uri;
                _event.Set();
            }
        }

        if (data.Find(item => item is SDataPlaybackProgress) is SDataPlaybackProgress progress)
        {
            Console.WriteLine($"当前回放进度{progress.Progress}");
            if (progress.Progress == 100) _event.Set();
        }
    }

    private List<DvrFileInfo> _dvrFiles;
    private bool _dvrOk;

    private string _errMsg;

    // [Test]
    [Order(2)]
    public void StartDvrTest()
    {
        // _event.WaitOne(1000);
        _device.SetParameter("playProgram", "DTMB|538|8");
        _device.SetParameter("dvrRecord", true);
        _event.WaitOne();
        if (!_dvrOk)
        {
            Assert.Fail($"开始录像失败{_errMsg}");
            return;
        }

        _event.WaitOne(60000);
        _device.SetParameter("dvrRecord", false);
        _event.WaitOne();
        if (!_dvrOk) Assert.Fail($"停止录像失败{_errMsg}");
    }

    [Test]
    [Order(3)]
    public void QueryPlaybackTest()
    {
        // _event.WaitOne(1000);
        _device.SetParameter("queryRecord", "true");
        _event.WaitOne();
        if (_dvrFiles == null || _dvrFiles.Count == 0) Assert.Fail("查询录像失败");
    }

    [Test]
    [Order(4)]
    public void PlaybackTest()
    {
        if (_dvrFiles == null || _dvrFiles.Count == 0) Assert.Fail("查询录像失败");
        var file = _dvrFiles[^1].FileName;
        var start = _dvrFiles[^1].StartTime;
        var stop = _dvrFiles[^1].StopTime;
        file = $"{start}|{stop}";
        Console.WriteLine($"开始回放{file}");
        _device.SetParameter("playBackFileName", file);
        _event.WaitOne();
    }

    [Test]
    [Order(5)]
    public void StopTest()
    {
        try
        {
            _device?.Stop();
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}