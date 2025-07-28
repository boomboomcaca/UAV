using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.Scan;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class ScanTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        var devId = Guid.NewGuid();
        _device = new DemoReceiver(devId);
        _module = new ModuleInfo
        {
            Id = Guid.NewGuid(),
            Category = ModuleCategory.Monitoring
        };
        var startPara = new Parameter
        {
            Name = ParameterNames.StartFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 88
        };
        var stopPara = new Parameter
        {
            Name = ParameterNames.StopFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 108
        };
        var stepPara = new Parameter
        {
            Name = ParameterNames.StepFrequency,
            Owners = new List<string> { devId.ToString() },
            Value = 25
        };
        _module.Parameters = new List<Parameter>
        {
            startPara,
            stopPara,
            stepPara
        };
        _scan = new Scan(_module.Id);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _scan?.Dispose();
        _scan = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private Scan _scan;
    private DemoReceiver _device;
    private Dictionary<string, object>[] _segments;
    private readonly AutoResetEvent _autoResetEvent = new(false);

    [Test(Description = "初始化测试")]
    [Order(1)]
    public void InitializeTest()
    {
        if (_scan == null) Assert.Fail("没有Scan驱动");
        if (_module == null) Assert.Fail("没有模块");
        try
        {
            _device.SetParameter(ParameterNames.IpAddress, "127.0.0.1");
            _device.SetParameter(ParameterNames.Port, 1720);
            var devModule = new ModuleInfo
            {
                Id = _device.Id,
                Category = ModuleCategory.Monitoring
            };
            _device.Initialized(devModule);
            _scan.SetParameter(ParameterNames.Receiver, _device);
            _scan.Initialized(_module);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test(Description = "设置参数测试")]
    [Order(2)]
    public void SetScanSegmentsParameterTest()
    {
        Console.WriteLine("设置频段参数测试");
        if (_scan == null) Assert.Fail("没有Scan驱动");
        try
        {
            var segments = new Dictionary<string, object>[2];
            var seg = new SegmentTemplate
            {
                StartFrequency = 86,
                StopFrequency = 108,
                StepFrequency = 25
            };
            var dic = seg.ToDictionary();
            segments[0] = dic;
            var seg2 = new SegmentTemplate
            {
                StartFrequency = 200,
                StopFrequency = 210,
                StepFrequency = 25
            };
            var dic2 = seg2.ToDictionary();
            segments[1] = dic2;
            _segments = segments;
            _scan.SetParameter(ParameterNames.ScanSegments, segments);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private DataPort _dataPort;

    [Test(Description = "启动任务")]
    [Order(3)]
    public void StartTest()
    {
        if (_scan == null) Assert.Fail("没有Scan驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += DataArrived;
            _scan.Start(_dataPort, MediaType.None);
            _lastTime = DateTime.Now;
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    /// <summary>
    ///     获取数据的个数
    /// </summary>
    private int _count;

    /// <summary>
    ///     错误信息
    /// </summary>
    private string _errMsg = string.Empty;

    /// <summary>
    ///     平均收到数据的间隔
    /// </summary>
    private double _avgSpan;

    private double _maxSpan;
    private double _minSpan = double.MaxValue;
    private bool _recvDataError;
    private DateTime _lastTime = DateTime.Now;

    private void DataArrived(Guid taskId, List<object> data)
    {
        if (_segments == null || _segments.Length == 0)
        {
            _errMsg = "频段信息为空！";
            _recvDataError = true;
            _autoResetEvent.Set();
        }

        var scan = (SDataScan)data.Find(item => item is SDataScan);
        if (scan != null)
        {
            var segIndex = scan.SegmentOffset;
            if (_segments!.Length <= segIndex)
            {
                _errMsg = "返回数据的频段个数与下发的参数不匹配";
                _recvDataError = true;
                _autoResetEvent.Set();
            }

            var seg = _segments[segIndex];
            _ = double.TryParse(seg[ParameterNames.StartFrequency].ToString(), out var startFreq);
            _ = double.TryParse(seg[ParameterNames.StopFrequency].ToString(), out var stopFreq);
            _ = double.TryParse(seg[ParameterNames.StepFrequency].ToString(), out var stepFreq);
            if (Math.Abs(scan.StartFrequency - startFreq) > 1e-9
                || Math.Abs(scan.StopFrequency - stopFreq) > 1e-9
                || Math.Abs(scan.StepFrequency - stepFreq) > 1e-9)
            {
                _errMsg = "返回数据与参数不匹配";
                _recvDataError = true;
                _autoResetEvent.Set();
            }

            _count++;
            var span = DateTime.Now.Subtract(_lastTime).TotalMilliseconds;
            _lastTime = DateTime.Now;
            _maxSpan = Math.Max(span, _maxSpan);
            _minSpan = Math.Min(span, _minSpan);
            _avgSpan = (_avgSpan * (_count - 1) + span) / _count;
            if (_count >= 100) _autoResetEvent.Set();
        }
    }

    [Test(Description = "停止任务")]
    [Order(4)]
    public void StopTest()
    {
        _autoResetEvent.WaitOne(20000);
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan}");
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("模板采集无数据返回");
        if (_scan == null) Assert.Fail("没有Scan驱动");
        try
        {
            _dataPort.DataArrived -= DataArrived;
            _scan.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}