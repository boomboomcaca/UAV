using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.Http;
using Magneto.Core.Define;
using Magneto.Device.DemoReceiver;
using Magneto.Driver.ESE;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using NUnit.Framework;

namespace Tests.Driver;

[TestFixture]
public class EseTest
{
    [OneTimeSetUp]
    public void Setup()
    {
        if (!HttpHelper.Instance.IsInitialized)
        {
            HttpHelper.Instance.Initialized("192.168.102.191", 10000);
            CloudClient.Instance.GetCloudTokenAsync("admin", "123456").ConfigureAwait(true).GetAwaiter().GetResult();
        }

        AppDomain.CurrentDomain.UnhandledException += UnhandledException;
        Initialized();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _ese?.Stop();
        _ese?.Dispose();
        _ese = null;
        _device?.Dispose();
        _device = null;
        _module = null;
    }

    private ModuleInfo _module;
    private Ese _ese;
    private DemoReceiver _device;
    private Dictionary<string, object>[] _segments;
    private readonly AutoResetEvent _autoResetEvent = new(false);
    private bool _isAppException;
    private string _appExeMsg = "";
    private readonly string _templateId = "c7b1b5a0-1852-11ec-b02b-053ab2c868a1";

    private void Initialized()
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
        _ese = new Ese(_module.Id);
    }

    private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        _isAppException = true;
        _appExeMsg = e.ExceptionObject.ToString();
        _autoResetEvent.Set();
    }

    [Test(Description = "初始化测试")]
    [Order(1)]
    public void InitializeTest()
    {
        Console.WriteLine("初始化测试");
        if (_ese == null) Assert.Fail("没有ESE驱动");
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
            _ese.SetParameter(ParameterNames.Receiver, _device);
            _ese.SetParameter("decodeReceiver", _device);
            _ese.Initialized(_module);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    [Order(2)]
    public void SetScanSegmentsParameterTest()
    {
        Console.WriteLine("设置频段参数测试");
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            var segments = new Dictionary<string, object>[1];
            var seg = new SegmentTemplate
            {
                StartFrequency = 86,
                StopFrequency = 108,
                StepFrequency = 25
            };
            var dic = seg.ToDictionary();
            segments[0] = dic;
            _segments = segments;
            _ese.SetParameter(ParameterNames.ScanSegments, segments);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    /// <summary>
    ///     模板采集
    /// </summary>
    [Test]
    [Order(3)]
    public void SetTemplateParameterTest()
    {
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            _ese.SetParameter("functionSwitch", false);
            _ese.SetParameter("templateID", _templateId);
            _ese.SetParameter("setSimData", true);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private DataPort _dataPort;

    [Test]
    [Order(4)]
    public void StartTemplateTest()
    {
        Console.WriteLine("启动模板采集测试");
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += TemplateDataArrived;
            _ese.Start(_dataPort, MediaType.None);
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

    private void TemplateDataArrived(Guid taskId, List<object> data)
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
            if (_segments != null && _segments.Length <= segIndex)
            {
                _errMsg = "返回数据的频段个数与下发的参数不匹配";
                _recvDataError = true;
                _autoResetEvent.Set();
            }

            var seg = _segments?[segIndex];
            double.TryParse(seg?[ParameterNames.StartFrequency].ToString(), out var startFreq);
            double.TryParse(seg?[ParameterNames.StopFrequency].ToString(), out var stopFreq);
            double.TryParse(seg?[ParameterNames.StepFrequency].ToString(), out var stepFreq);
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

    [Test]
    [Order(5)]
    public void StopTemplateTest()
    {
        _autoResetEvent.WaitOne(20000);
        Console.WriteLine($"获取数据个数:{_count},平均间隔{_avgSpan},最大间隔{_maxSpan},最小间隔{_minSpan}");
        if (_isAppException) Assert.Fail(_appExeMsg);
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("模板采集无数据返回");
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            _dataPort.DataArrived -= TemplateDataArrived;
            _ese.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    /// <summary>
    ///     信号比对
    /// </summary>
    [Test]
    [Order(6)]
    public void SetCompareParameterTest()
    {
        Initialized();
        InitializeTest();
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            _ese.SetParameter("functionSwitch", true);
            _ese.SetParameter("setSimData", true);
            _ese.SetParameter("templateID", _templateId);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    [Order(7)]
    public void StartCompareTest()
    {
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            var taskId = Guid.NewGuid();
            _dataPort = new DataPort(taskId);
            _dataPort.DataArrived += CompareDataArrived;
            _count = 0;
            _avgSpan = 0;
            _maxSpan = 0;
            _minSpan = double.MaxValue;
            _errMsg = string.Empty;
            _recvDataError = false;
            _ese.Start(_dataPort, MediaType.None);
            _ese.SetParameter("setSimData", true);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    private readonly Dictionary<double, FrequencyResult> _signalDic = new();

    private void CompareDataArrived(Guid taskId, List<object> data)
    {
        var result = (SDataNsicResult)data.Find(item => item is SDataNsicResult);
        if (result?.Data?.Length > 0)
            foreach (var signal in result.Data)
                // if (_segments == null || index >= _segments.Length)
                // {
                //     _errMsg = "频段信息有误！";
                //     _recvDataError = true;
                //     _autoResetEvent.Set();
                // }
            foreach (var freq in signal.Results)
                _signalDic[freq.Frequency] = freq;
        var ese = (SDataEseResult)data.Find(item => item is SDataEseResult);
        if (ese != null)
        {
            _count++;
            if (!_signalDic.ContainsKey(ese.Frequency))
            {
                _errMsg = "解析数据有误，中心频率不存在";
                _recvDataError = true;
                _autoResetEvent.Set();
            }

            _autoResetEvent.Set();
        }
    }

    [Test]
    [Order(8)]
    public void StopCompareTest()
    {
        _autoResetEvent.WaitOne(20000);
        Console.WriteLine("停止比对测试");
        if (_isAppException) Assert.Fail(_appExeMsg);
        if (_recvDataError) Assert.Fail(_errMsg);
        if (_count == 0) Assert.Fail("解码无数据返回");
        if (_ese == null) Assert.Fail("没有ESE驱动");
        try
        {
            _dataPort.DataArrived -= CompareDataArrived;
            _ese.Stop();
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }

        Assert.Pass();
    }
}