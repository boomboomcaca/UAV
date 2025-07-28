using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Contract.BaseClass;

public abstract class ScanBase : DriverBase
{
    /// <summary>
    ///     线程同步锁
    /// </summary>
    private readonly object _objLock;

    // private int _sendCount = 0;
    /// <summary>
    ///     天线控制器
    /// </summary>
    protected IDevice AntennaController = null;

    protected readonly AutoResetEvent AutoResetEvent = new(false);
    protected readonly double Epsilon = 1.0E-7d;
    protected readonly object LockSegmentList = new();
    protected readonly List<ScanSegmentStatistics> SegmentList = new();
    private int _preSendDataIndex;
    private int _preSendDataOffset;
    private DateTime _preSendDataTime = DateTime.MinValue;

    private Thread _thdScan;

    protected List<SDataFactor> Factors = new();
    protected bool FactorSendOk;
    protected bool IsOver;

    /// <summary>
    ///     是否支持多频段
    /// </summary>
    protected bool IsSupportMultiSegments = false;

    protected bool PauseRequest;
    protected int SegmentIndex;

    protected ScanBase(Guid driverId) : base(driverId)
    {
        _objLock = new object();
        PauseRequest = false;
        CanPause = false;
    }

    protected virtual bool StartMultiSegments()
    {
        CanPause = false;
        IsOver = false;
        _preSendDataIndex = 0;
        _preSendDataOffset = 0;
        _preSendDataTime = DateTime.Now;
        UpdateScanSegments();
        _thdScan = new Thread(ScanSegmentsChangeProcess)
        {
            IsBackground = true
        };
        _thdScan.Start();
        return true;
    }

    protected virtual bool StartSingleSegments()
    {
        StartDevice();
        return true;
    }

    public override bool Pause()
    {
        lock (_objLock)
        {
            PauseRequest = true;
        }

        return base.Pause();
        // return false;
    }

    public override bool Stop()
    {
        base.Stop();
        IsOver = false;
        CanPause = false;
        lock (LockSegmentList)
        {
            SegmentList?.Clear();
        }

        AutoResetEvent.Set();
        //_thdScan?.Interrupt();
        // if (!_isSupportMultiSegments)
        {
            StopDevice();
        }
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        SetParameterInternal(name, value);
        if (IsSupportMultiSegments || name is not (ParameterNames.StartFrequency or ParameterNames.StopFrequency
                or ParameterNames.StepFrequency)) return;
        lock (LockSegmentList)
        {
            if (_segments == null)
            {
                _segments = new SegmentTemplate[1];
                _segments[0] = new SegmentTemplate();
            }

            if (name == ParameterNames.StartFrequency)
            {
                if (double.TryParse(value.ToString(), out var db)) _segments[0].StartFrequency = db;
            }
            else if (name == ParameterNames.StopFrequency)
            {
                if (double.TryParse(value.ToString(), out var db)) _segments[0].StopFrequency = db;
            }
            else if (name == ParameterNames.StepFrequency)
            {
                if (double.TryParse(value.ToString(), out var db)) _segments[0].StepFrequency = db;
            }
        }
    }

    /// <summary>
    ///     使用抽点的方式发送数据
    /// </summary>
    /// <param name="data"></param>
    protected virtual void SendDataWithSpan(List<object> data)
    {
        lock (LockSegmentList)
        {
            if (SegmentList == null
                || SegmentIndex < 0
                || SegmentList.Count <= SegmentIndex
                || (SegmentList[SegmentIndex].IsOver && SegmentList.Count > 1))
                return;
        }

        try
        {
            var scan = (SDataScan)data.Find(item => item is SDataScan);
            var dfScan = (SDataDfScan)data.Find(item => item is SDataDfScan);
            if (scan == null) return;
            var idx = SegmentIndex;
            var seg = SegmentList[idx];
            if (!seg.CheckSegment(scan.StartFrequency, scan.StopFrequency, scan.StepFrequency)) return;
            scan.SegmentOffset = idx;
            if (dfScan is not null) dfScan.SegmentOffset = idx;
            SegmentList[idx].AppendData(scan.Data, scan.Offset, scan.Data.Length);
            // 现在暂时不做缓存，后续需要与前端协商是发整包数据还是分包数据
            var isOver = SegmentList[idx].IsOver;
            // 不抽样。
            //SendDataForAppend();
            SendData(data);
            // 当前频段扫描完毕则继续扫描下一个频段
            if (!isOver || SegmentList.Count <= 1) return;
            IsOver = true;
            AutoResetEvent.Set();
        }
        catch
        {
            // 容错代码
        }
    }

    protected abstract void StartDevice();
    protected abstract void StopDevice();

    protected virtual void UpdateAntennaControllerFrequency(double frequency)
    {
        if (AntennaController is IAntennaController antennaController) antennaController.Frequency = frequency;
    }

    /// <summary>
    ///     内部设置参数方法
    ///     添加这个方法的目的是方法<see cref="SetParameter" />可能会设置startFrequency等参数，
    ///     而这些参数在多频段时需要过滤
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected virtual void SetParameterInternal(string name, object value)
    {
        base.SetParameter(name, value);
        // if (name == ParameterNames.ScanMode)
        // {
        //     lock (_lockSegmentList)
        //     {
        //         _segmentList.Clear();
        //     }
        //     UpdateScanSegments();
        // }
        if (!AntennaChanged) return;
        Factors = GetFactors();
        FactorSendOk = false;
        AntennaChanged = false;
    }

    /// <summary>
    ///     抽点发送
    /// </summary>
    protected virtual void SendDataForAppend()
    {
        if (CanPause && IsCompetition) return;
        //_sendCount++;
        // if (DateTime.Now.Subtract(_preSendDataTime).TotalMilliseconds < 30)
        var span = PublicDefine.DataSpan;
        if (IsCompetition)
            // 如果设备有争用，则不再进行抽点
            span = 0;
        if (DateTime.Now.Subtract(_preSendDataTime).TotalMilliseconds < span) return;
        // var span = DateTime.Now.Subtract(_preSendDataTime).TotalMilliseconds;
        // Console.WriteLine($"Span:{span}");
        //_sendCount = 0;
        _preSendDataTime = DateTime.Now;
        short[] data;
        ScanSegmentStatistics segment;
        int offset;
        int length;
        var count = 0;
        do
        {
            if (_preSendDataIndex >= SegmentList.Count)
            {
                _preSendDataIndex = 0;
                _preSendDataOffset = 0;
            }

            segment = SegmentList[_preSendDataIndex];
            // int length = ((_maxSendLength + _preSendDataOffset) > segment.Total) ? (segment.Total - _preSendDataOffset) : _maxSendLength;
            data = segment.GetData(out offset, out length);
            if (data == null)
            {
                if (count > SegmentList.Count * 2)
                    // 尝试频段数量*2次还获取不到数据就返回等待下一次获取数据
                    return;
                _preSendDataIndex++;
            }

            count++;
        } while (data == null);

        var scan = new SDataScan
        {
            SegmentOffset = _preSendDataIndex,
            StartFrequency = segment.StartFrequency,
            StopFrequency = segment.StopFrequency,
            StepFrequency = segment.StepFrequency,
            Offset = offset,
            Total = segment.Total,
            IsStrengthField = false,
            DataMark = new byte[4],
            Data = data
        };
        // Console.WriteLine($"     scan offset:{scan.Offset},len:{scan.Data.Length},total:{scan.Total}");
        if (MaximumSwitch)
        {
            scan.Maximum = segment.GetMaxData(scan.Offset, scan.Data.Length);
            scan.DataMark[0] = 1;
        }

        if (MinimumSwitch)
        {
            scan.Minimum = segment.GetMinData(scan.Offset, scan.Data.Length);
            scan.DataMark[1] = 1;
        }

        if (MeanSwitch)
        {
            scan.Mean = segment.GetMeanData(scan.Offset, scan.Data.Length);
            scan.DataMark[2] = 1;
        }

        if (ThresholdSwitch)
        {
            scan.Threshold = segment.GetThresholdData(scan.Offset, scan.Data.Length);
            if (scan.Threshold != null) scan.DataMark[3] = 1;
            else scan.DataMark[3] = 2;
        }

        _preSendDataOffset = offset + length;
        var list = new List<object> { scan };
        if (!FactorSendOk && Factors.Count > 0)
        {
            SendData(Factors.ConvertAll(item => (object)item));
            FactorSendOk = true;
        }

        SendData(list);
        var isOver = _preSendDataOffset >= segment.Total;
        // 如果已经扫描完一整包，并且有设备冲突，则不发送后续数据了，等待暂停
        if (isOver && SegmentList.Count == _preSendDataIndex + 1 && IsCompetition && PauseRequest) CanPause = true;
        if (_preSendDataOffset >= segment.Total)
        {
            _preSendDataIndex++;
            _preSendDataOffset = 0;
        }

        if (_preSendDataIndex < SegmentList.Count) return;
        _preSendDataIndex = 0;
        _preSendDataOffset = 0;
    }

    protected virtual void UpdateScanSegments()
    {
        if (_segments == null) return;
        lock (LockSegmentList)
        {
            var isChanged = false;
            for (var i = 0; i < _segments.Length; i++)
            {
                var startFreq = _segments[i].StartFrequency;
                var stopFreq = _segments[i].StopFrequency;
                var stepFreq = _segments[i].StepFrequency;
                if (startFreq >= stopFreq) continue;
                if (SegmentList.Count <= i)
                {
                    var seg = new ScanSegmentStatistics(i, startFreq, stopFreq, stepFreq, true)
                    {
                        ScanIndex = 0,
                        Offset = 0
                    };
                    SegmentList.Add(seg);
                    isChanged = true;
                }
                else if (IsScanSegmentsChanged(_segments[i], SegmentList[i]))
                {
                    var seg = new ScanSegmentStatistics(i, startFreq, stopFreq, stepFreq, true)
                    {
                        ScanIndex = 0,
                        Offset = 0
                    };
                    SegmentList[i] = seg;
                    isChanged = true;
                }
            }

            if (SegmentList.Count > _segments.Length)
            {
                isChanged = true;
                for (var i = _segments.Length; i < SegmentList.Count;) SegmentList.RemoveAt(i);
            }

            if (isChanged)
            {
                Factors = GetFactors();
                FactorSendOk = false;
                // SendData(_factors.ConvertAll(item => (object)item));
            }
        }

        SegmentIndex = -1;
        AutoResetEvent.Set();
    }

    protected virtual bool CanChangeSegments()
    {
        return !CanPause;
    }

    protected virtual void ScanSegmentsChangeProcess()
    {
        while (IsTaskRunning)
        {
            AutoResetEvent.WaitOne();
            try
            {
                if (!IsTaskRunning) break;
                StopDevice();
                if (!IsTaskRunning) break;
                if (!CanChangeSegments()) continue;
                lock (LockSegmentList)
                {
                    SegmentIndex++;
                    if (SegmentList?.Any() != true) break;
                    if (SegmentIndex >= SegmentList.Count || SegmentIndex < 0) SegmentIndex = 0;
                    UpdateAntennaControllerFrequency(SegmentList[SegmentIndex].StartFrequency);
                    var dic = _segments?[SegmentIndex]?.ToDictionary();
                    if (dic?.Count > 0)
                        foreach (var pair in dic)
                            SetParameterInternal(pair.Key, pair.Value);
                    SegmentList[SegmentIndex].IsOver = false;
                }

                IsOver = false;
                CanPause = false;
                StartDevice();
            }
            catch (Exception ex)
            {
#if DEBUG
                Trace.WriteLine(ex.ToString());
#endif
            }
        }
    }

    private bool IsScanSegmentsChanged(SegmentTemplate seg, ScanSegmentStatistics segment)
    {
        var startFreq = seg.StartFrequency;
        var stopFreq = seg.StopFrequency;
        var stepFreq = seg.StepFrequency;
        if (Math.Abs(segment.StartFrequency - startFreq) > Epsilon) return true;
        if (Math.Abs(segment.StopFrequency - stopFreq) > Epsilon) return true;
        if (Math.Abs(segment.StepFrequency - stepFreq) > Epsilon) return true;
        return false;
    }

    /// <summary>
    ///     获取所有天线因子
    /// </summary>
    protected virtual List<SDataFactor> GetFactors()
    {
        var factors = new List<SDataFactor>();
        var segmentIndex = 0;
        if (AntennaController is not IAntennaController antennaController) return factors;
        foreach (var segment in SegmentList)
        {
            // 无论是频率自动还是极化手动，都依赖先设置天线的频率，因此有必要先设置频率，另外，如果是手动天线方式，则此处即便设置了频率，也并不影响天线的打通
            UpdateAntennaControllerFrequency(segment.StartFrequency);
            var startFreq = segment.StartFrequency;
            var stopFreq = segment.StopFrequency;
            var stepFreq = segment.StepFrequency;
            var count = segment.Total;
            Exception exception = null;
            var data = antennaController.GetFactor(startFreq, stopFreq, stepFreq, count, ref exception);
            if (exception != null)
            {
                Trace.WriteLine($"查询天线因子失败:{exception.Message}");
                var msg = new SDataMessage
                {
                    LogType = LogType.Error,
                    ErrorCode = (int)InternalMessageType.Error,
                    Description = exception.Message,
                    Detail = exception.ToString()
                };
                SendMessage(msg);
            }

            if (data != null && data.Length == count)
            {
                var factor = new SDataFactor
                {
                    SegmentOffset = segmentIndex,
                    StartFrequency = startFreq,
                    StopFrequency = stopFreq,
                    StepFrequency = stepFreq,
                    Total = data.Length,
                    Data = data
                };
                factors.Add(factor);
            }

            //lx 此处是否还要记录日志？原则上不会走到此分支
            segmentIndex++;
        }

        return factors;
    }

    #region 功能参数

    private SegmentTemplate[] _segments;

    [Name(ParameterNames.ScanSegments)]
    public Dictionary<string, object>[] ScanSegments
    {
        get => null;
        set
        {
            if (value == null) return;
            _segments = Array.ConvertAll(value, item => (SegmentTemplate)item);
            UpdateScanSegments();
        }
    }

    #endregion
}

internal class SegmentTemplate
{
    [Name(ParameterNames.StartFrequency)] public double StartFrequency { get; set; } = 87.0d;

    [Name(ParameterNames.StopFrequency)] public double StopFrequency { get; set; } = 108.0d;

    [Name(ParameterNames.StepFrequency)] public double StepFrequency { get; set; } = 25.0d;

    public Dictionary<string, object> Parameters { get; set; } = new();

    public static explicit operator SegmentTemplate(Dictionary<string, object> dict)
    {
        if (dict == null) return null;
        var template = new SegmentTemplate();
        var type = template.GetType();
        try
        {
            var properties = type.GetProperties();
            PropertyInfo prop = null;
            Dictionary<string, object> other = new();
            foreach (var pair in dict) other.Add(pair.Key, pair.Value);
            foreach (var property in properties)
            {
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                if (dict.TryGetValue(name, out var value))
                {
                    property.SetValue(template, value, null);
                    other.Remove(name);
                }

                if (name.Equals("Parameters", StringComparison.OrdinalIgnoreCase)) prop = property;
            }

            prop?.SetValue(template, other);
        }
        catch
        {
            // 容错代码
        }

        return template;
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dic = new Dictionary<string, object>();
        var type = GetType();
        try
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if (Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute) continue;
                var name =
                    Attribute.GetCustomAttribute(property, typeof(NameAttribute)) is not NameAttribute nameAttribute
                        ? property.Name
                        : nameAttribute.Name;
                var value = property.GetValue(this);
                dic.Add(name, value);
            }

            if (Parameters?.Count > 0)
                foreach (var pair in Parameters)
                    dic.TryAdd(pair.Key, pair.Value);
        }
        catch
        {
            // 容错代码
        }

        return dic;
    }
}