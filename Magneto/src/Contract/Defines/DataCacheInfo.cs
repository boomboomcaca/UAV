using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Defines;

public class DataCacheInfo(SDataType type, bool canDrop = false)
{
    private readonly ConcurrentQueue<SDataRaw> _dataCache = new();
    private readonly object _lockScan = new();
    private readonly ConcurrentDictionary<int, ScanDataMerge> _scanDataCache = new();
    private int _fpsCount;
    private DateTime _preFpsTime = DateTime.Now;
    private DateTime _preMaxDataTime = DateTime.Now;

    public void Clear()
    {
        _preFpsTime = DateTime.Now;
        _dataCache.Clear();
        _fpsCount = 0;
        lock (_lockScan)
        {
            _scanDataCache.Clear();
        }
    }

    public void AddData(SDataRaw data)
    {
        if (data.Type != type) return;
        if (type != SDataType.Scan)
        {
            if (canDrop && type != SDataType.Iq && type != SDataType.Audio) _dataCache.Clear();
            _dataCache.Enqueue(data);
        }
        else
        {
            ProcessScanData(data);
        }
    }

    public void AddData(IEnumerable<SDataRaw> data)
    {
        var sDataRaw = data as SDataRaw[] ?? data.ToArray();
        if (sDataRaw.Any() != true) return;
        foreach (var d in sDataRaw)
        {
            if (d == null) continue;
            AddData(d);
        }
    }

    public List<object> GetData()
    {
        List<SDataRaw> list = new();
        List<object> rt = new();
        if (type != SDataType.Scan)
        {
            while (_dataCache.TryDequeue(out var cache)) list.Add(cache);
            if (list.Count == 0) return null;
        }

        switch (type)
        {
            case SDataType.Level:
            {
                SDataLevel level = null;
                foreach (var raw in list.Select(t => t as SDataLevel))
                    if (raw != null && (level == null
                                        || Math.Abs(raw.Frequency - level.Frequency) > 1e-9
                                        || Math.Abs(raw.Bandwidth - level.Bandwidth) > 1e-9
                                        || raw.IsStrengthField != level.IsStrengthField))
                        level = raw;
                    else if (raw != null && level.Data < raw.Data) level.Data = raw.Data;
                rt.Add(level);
                return rt;
            }
            case SDataType.Spectrum:
            {
                SDataSpectrum spec = null;
                foreach (var t in list)
                {
                    var raw = t as SDataSpectrum;
                    if (spec == null
                        || raw?.Data == null
                        || spec.Data == null
                        || raw.Data.Length != spec.Data.Length
                        || Math.Abs(raw.Frequency - spec.Frequency) > 1e-9
                        || Math.Abs(raw.Span - spec.Span) > 1e-9)
                        spec = raw;
                    else
                        for (var d = 0; d < spec.Data.Length; d++)
                            if (spec.Data[d] < raw.Data[d])
                                spec.Data[d] = raw.Data[d];
                    if (DateTime.Now.Subtract(_preMaxDataTime).TotalMilliseconds > 1000)
                    {
                        _preMaxDataTime = DateTime.Now;
                        if (spec == null) continue;
                        spec.Maximum = raw.Maximum;
                        spec.Minimum = raw.Minimum;
                        spec.Mean = raw.Mean;
                        spec.Noise = raw.Noise;
                    }
                    else
                    {
                        if (spec == null) continue;
                        spec.Maximum = null;
                        spec.Minimum = null;
                        spec.Mean = null;
                        spec.Noise = null;
                        if (spec.DataMark[0] == 1) spec.DataMark[0] = 2;
                        if (spec.DataMark[1] == 1) spec.DataMark[1] = 2;
                        if (spec.DataMark[2] == 1) spec.DataMark[2] = 2;
                        if (spec.DataMark[3] == 1) spec.DataMark[3] = 2;
                    }
                }

                // spec.Data = Array.ConvertAll(spec.Data, item => (short)(item / 10f));
                rt.Add(spec);
                _fpsCount++;
                var span = DateTime.Now.Subtract(_preFpsTime).TotalMilliseconds;
                if (span >= 1000)
                {
                    var data = _fpsCount / (span / 1000);
                    SDataFps fps = new()
                    {
                        Data = data
                    };
                    rt.Add(fps);
                    _fpsCount = 0;
                    _preFpsTime = DateTime.Now;
                }

                return rt;
            }
            case SDataType.Scan:
                lock (_lockScan)
                {
                    foreach (var pair in _scanDataCache)
                    {
                        var data = pair.Value.GetData();
                        if (data != null)
                        {
                            if (pair.Key == _scanDataCache.Max(p => p.Key)
                                && data.Any(item => item.Offset + item.Data.Length == item.Total))
                                _fpsCount++;
                            rt.AddRange(data);
                        }
                    }

                    var span = DateTime.Now.Subtract(_preFpsTime).TotalMilliseconds;
                    if (span >= 1000)
                    {
                        var data = _fpsCount / (span / 1000);
                        SDataFps fps = new()
                        {
                            Data = data
                        };
                        rt.Add(fps);
                        _fpsCount = 0;
                        _preFpsTime = DateTime.Now;
                    }

                    return rt;
                }
            case SDataType.Audio:
            case SDataType.Iq: // 为了IQ连续，暂时全部发送
                rt.AddRange(list);
                return rt;
            default:
                rt.Add(list.Last());
                return rt;
        }
    }

    private void ProcessScanData(SDataRaw data)
    {
        if (data is not SDataScan scan) return;
        lock (_lockScan)
        {
            _scanDataCache.AddOrUpdate(scan.SegmentOffset,
                _ =>
                {
                    var merge = new ScanDataMerge(scan.Total, scan.SegmentOffset, scan.StartFrequency,
                        scan.StopFrequency, scan.StepFrequency);
                    merge.AddData(scan);
                    return merge;
                },
                (_, v) =>
                {
                    v.AddData(scan);
                    return v;
                });
        }
    }
}

internal class ScanDataMerge
{
    /// <summary>
    ///     第一帧数据
    /// </summary>
    private readonly short[] _data;

    private readonly short[] _max;
    private readonly short[] _mean;
    private readonly short[] _min;
    private readonly int _scanSegmentOffset;
    private readonly short[] _threshold;
    private readonly int _total;

    /// <summary>
    ///     第一帧收完
    /// </summary>
    private bool _firstOver;

    private bool _firstSign;
    private int _fullPackageCount;
    private bool _hasData;
    private byte[] _mark;

    /// <summary>
    ///     第二帧数据
    /// </summary>
    private short[] _nextData;

    private bool _nextHasData;
    private bool _nextOver;
    private int _nextStartIndex;
    private int _nextStopIndex;
    private bool _preMaxSign;
    private bool _preMeanSign;
    private bool _preMinSign;
    private bool _preThdSign;
    private int _startIndex;
    private int _stopIndex;

    public ScanDataMerge(int total, int scanOffset, double start, double stop, double step)
    {
        _scanSegmentOffset = scanOffset;
        StartFrequency = start;
        StopFrequency = stop;
        StepFrequency = step;
        _total = total;
        _data = Enumerable.Repeat<short>(-9999, _total).ToArray();
        _nextData = Enumerable.Repeat<short>(-9999, _total).ToArray();
        _max = new short[_total];
        _min = new short[_total];
        _mean = new short[_total];
        _threshold = new short[_total];
        _startIndex = _total;
        _stopIndex = 0;
        _nextStartIndex = _total;
        _nextStopIndex = 0;
        _mark = new byte[4];
        _fullPackageCount = 999;
        _firstSign = true;
    }

    public double StartFrequency { get; }
    public double StopFrequency { get; }
    public double StepFrequency { get; }

    public void AddData(SDataScan scan)
    {
        if (!_firstOver)
        {
            for (var i = 0; i < scan.Data.Length; i++)
            {
                var index = scan.Offset + i;
                if (_data[index] < scan.Data[i]) _data[index] = scan.Data[i];
            }

            if (scan.Offset + scan.Data.Length == scan.Total) _firstOver = true;
            if (_startIndex > scan.Offset) _startIndex = scan.Offset;
            if (_stopIndex < scan.Offset + scan.Data.Length - 1) _stopIndex = scan.Offset + scan.Data.Length - 1;
            _hasData = true;
        }
        else
        {
            for (var i = 0; i < scan.Data.Length; i++)
            {
                var index = scan.Offset + i;
                if (_nextData[index] < scan.Data[i]) _nextData[index] = scan.Data[i];
            }

            _nextOver = scan.Offset + scan.Data.Length == scan.Total;
            if (_nextStartIndex > scan.Offset) _nextStartIndex = scan.Offset;
            if (_nextStopIndex < scan.Offset + scan.Data.Length - 1)
                _nextStopIndex = scan.Offset + scan.Data.Length - 1;
            _nextHasData = true;
            if (_firstOver && _nextOver)
            {
                for (var i = 0; i < _total; i++)
                    if (_data[i] < _nextData[i])
                        _data[i] = _nextData[i];
                _nextOver = false;
                _nextStartIndex = _total;
                _nextStopIndex = 0;
                _nextHasData = false;
                _nextData = Enumerable.Repeat<short>(-9999, _total).ToArray();
            }
        }

        if (scan.Maximum != null)
        {
            if (!_preMaxSign)
            {
                _preMaxSign = true;
                _firstSign = true;
                _fullPackageCount = 999;
            }

            Buffer.BlockCopy(scan.Maximum, 0, _max, scan.Offset * sizeof(short), scan.Data.Length * sizeof(short));
        }
        else
        {
            _preMaxSign = false;
        }

        if (scan.Minimum != null)
        {
            if (!_preMinSign)
            {
                _preMinSign = true;
                _firstSign = true;
                _fullPackageCount = 999;
            }

            Buffer.BlockCopy(scan.Minimum, 0, _min, scan.Offset * sizeof(short), scan.Data.Length * sizeof(short));
        }
        else
        {
            _preMinSign = false;
        }

        if (scan.Mean != null)
        {
            if (!_preMeanSign)
            {
                _preMeanSign = true;
                _firstSign = true;
                _fullPackageCount = 999;
            }

            Buffer.BlockCopy(scan.Mean, 0, _mean, scan.Offset * sizeof(short), scan.Data.Length * sizeof(short));
        }
        else
        {
            _preMeanSign = false;
        }

        if (scan.Threshold != null)
        {
            if (!_preThdSign)
            {
                _preThdSign = true;
                _firstSign = true;
                _fullPackageCount = 999;
            }

            Buffer.BlockCopy(scan.Threshold, 0, _threshold, scan.Offset * sizeof(short),
                scan.Data.Length * sizeof(short));
        }
        else
        {
            _preThdSign = false;
        }

        _mark = scan.DataMark;
    }

    public List<SDataScan> GetData()
    {
        if (!_hasData) return null;
        List<SDataScan> list = new();
        list.AddRange(GetData(_data));
        Buffer.BlockCopy(_nextData, 0, _data, 0, _total * sizeof(short));
        _nextData = Enumerable.Repeat<short>(-9999, _total).ToArray();
        _firstOver = _nextOver;
        _startIndex = _nextStartIndex;
        _stopIndex = _nextStopIndex;
        _hasData = _nextHasData;
        _nextStartIndex = _total;
        _nextStopIndex = 0;
        _nextHasData = false;
        return list;
    }

    public List<SDataScan> GetData(short[] data)
    {
        List<SDataScan> list = new();
        var start = Array.IndexOf(data, (short)-9999);
        var stop = Array.LastIndexOf(data, (short)-9999);
        // Console.WriteLine($"start:{start},stop:{stop},bstart:{_startIndex},bstop:{_stopIndex},ALL:{string.Join(",", data)}");
        if (start < 0) PackageScanData(data, list, _total, 0);
        if (start >= 0 && stop < _total - 1)
        {
            var len = _total - stop - 1;
            var offset = stop + 1;
            PackageScanData(data, list, len, offset);
        }

        if (start == 0 && stop == _total - 1 && _startIndex <= _stopIndex)
        {
            var len = _stopIndex - _startIndex + 1;
            var offset = _startIndex;
            PackageScanData(data, list, len, offset);
        }

        if (start > 0)
        {
            var len = start;
            PackageScanData(data, list, len, 0);
        }

        return list;
    }

    private void PackageScanData(short[] data, List<SDataScan> list, int len, int offset)
    {
        var scan = new SDataScan
        {
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            SegmentOffset = _scanSegmentOffset,
            Total = _total,
            DataMark = _mark,
            Offset = offset,
            Data = new short[len],
            Maximum = new short[len],
            Minimum = new short[len],
            Mean = new short[len],
            Threshold = new short[len]
        };
        Buffer.BlockCopy(data, offset * sizeof(short), scan.Data, 0, len * sizeof(short));
        Buffer.BlockCopy(_max, offset * sizeof(short), scan.Maximum, 0, len * sizeof(short));
        Buffer.BlockCopy(_min, offset * sizeof(short), scan.Minimum, 0, len * sizeof(short));
        Buffer.BlockCopy(_mean, offset * sizeof(short), scan.Mean, 0, len * sizeof(short));
        Buffer.BlockCopy(_threshold, offset * sizeof(short), scan.Threshold, 0, len * sizeof(short));
        if (_fullPackageCount > 5 || _firstSign)
        {
            if (scan.Offset + scan.Data.Length == scan.Total)
            {
                if (_fullPackageCount < 100) _firstSign = false;
                _fullPackageCount = 0;
            }

            if (scan.DataMark[0] == 0) scan.Maximum = null;
            if (scan.DataMark[1] == 0) scan.Minimum = null;
            if (scan.DataMark[2] == 0) scan.Mean = null;
            if (scan.DataMark[3] == 0 || scan.DataMark[3] == 2) scan.Threshold = null;
        }
        else
        {
            if (scan.Offset + scan.Data.Length == scan.Total) _fullPackageCount++;
            scan.Maximum = null;
            scan.Minimum = null;
            scan.Mean = null;
            scan.Threshold = null;
            if (scan.DataMark[0] == 1) scan.DataMark[0] = 2;
            if (scan.DataMark[1] == 1) scan.DataMark[1] = 2;
            if (scan.DataMark[2] == 1) scan.DataMark[2] = 2;
            if (scan.DataMark[3] == 1) scan.DataMark[3] = 2;
        }

        list.Add(scan);
    }
}