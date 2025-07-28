using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.CA300B;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;
using NUnit.Framework;

namespace Tests.Device;

public class Ca300BTest
{
    private DataPort _dataPort;
    private Ca300B _device;
    private AutoResetEvent _event;
    private AutoResetEvent _eventCancelSearch;
    private bool _isCanceled;
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
        _device = new Ca300B(_module.Id);
        _event = new AutoResetEvent(false);
        _eventCancelSearch = new AutoResetEvent(false);
    }

    [Test]
    [Order(1)]
    public void InitializedTest()
    {
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "192.168.6.33");
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
            _device.Start(FeatureType.RTV, _dataPort);
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
        if (data.Find(item => item is SDataSearchProgress) is SDataSearchProgress searchProgress)
        {
            Console.WriteLine($"当前搜索进度:{searchProgress.Progress}%");
            if (searchProgress.Progress > 1) _eventCancelSearch.Set();
            if (Math.Abs(searchProgress.Progress - 100) < 1e-9)
            {
                Console.WriteLine("搜索完毕");
                _event.Set();
            }
        }

        if (data.Find(item => item is SDataVideoChannel) is SDataVideoChannel channel)
        {
            if (_isCanceled) Console.WriteLine("取消搜索");
            Console.WriteLine($"搜索到节目{channel.Programs?.Count}个");
        }
    }

    [Test]
    [Order(3)]
    public void SearchTest()
    {
        try
        {
            // DTMB="474,562,8000|610,858,8000"
            // DVBT="474,562,8000|610,858,8000"
            // ANATV="49.75,65.75,8000|77.25,85.25,8000|168.25,216.25,8000|471.25,559.25,8000|607.25,855.25,8000"
            var list = new List<Dictionary<string, object>>();
            var dic1 = new Dictionary<string, object>
            {
                { "startFrequency", 482d },
                { "stopFrequency", 482d },
                { "standard", new List<string> { "DTMB" } }
            };
            list.Add(dic1);
            // var dic2 = new Dictionary<string, object>
            // {
            //     { "startFrequency", 610d },
            //     { "stopFrequency", 858d },
            //     { "standard", new List<string>() { "DVBT", "DTMB" } }
            // };
            // list.Add(dic2);
            // var dic3 = new Dictionary<string, object>
            // {
            //     { "startFrequency", 49.75 },
            //     { "stopFrequency", 65.75 },
            //     { "standard", new List<string>() { "ANATV" } }
            // };
            // list.Add(dic3);
            // var dic4 = new Dictionary<string, object>
            // {
            //     { "startFrequency", 77.25 },
            //     { "stopFrequency", 85.25 },
            //     { "standard", new List<string>() { "ANATV" } }
            // };
            // list.Add(dic4);
            // var dic5 = new Dictionary<string, object>
            // {
            //     { "startFrequency", 168.25 },
            //     { "stopFrequency", 216.25 },
            //     { "standard", new List<string>() { "ANATV" } }
            // };
            // list.Add(dic5);
            // var dic6 = new Dictionary<string, object>
            // {
            //     { "startFrequency",471.25 },
            //     { "stopFrequency", 559.25 },
            //     { "standard", new List<string>() { "ANATV" } }
            // };
            // list.Add(dic6);
            // var dic7 = new Dictionary<string, object>
            // {
            //     { "startFrequency",607.25 },
            //     { "stopFrequency", 855.25 },
            //     { "standard", new List<string>() { "ANATV" } }
            // };
            // list.Add(dic7);
            MessagePackSerializer.SerializeToJson(list);
            var buffer = MessagePackSerializer.Serialize(list);
            var para = MessagePackSerializer.Deserialize<Dictionary<string, object>[]>(buffer);
            _device.SetParameter("searchProgram", para);
            _event.WaitOne();
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    // [Test]
    [Order(3)]
    public void CancelSearchTest()
    {
        _eventCancelSearch.WaitOne();
        _isCanceled = true;
        _device.SetParameter("cancelSearch", true);
    }

    [Test]
    [Order(4)]
    public void PlayProgramTest()
    {
        _device.SetParameter("playProgram", "DTMB|538|8");
        _event.WaitOne();
    }
}