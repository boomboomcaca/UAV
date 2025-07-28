using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Define;
using Core.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;
using MessagePack;

namespace Core.Storage;

public class ReportDataSave : IDisposable
{
    private readonly ConcurrentQueue<SDataScan> _dataCache = new();
    private readonly FileDataType _fileType;
    private readonly int _interval;

    /// <summary>
    ///     存储的根目录
    /// </summary>
    private readonly string _rootDir;

    private short[] _avg;
    private long _count;
    private ReportDataFile _dataFile;
    private string _edgeId;
    private List<short> _factor = new();
    private ReportDataHeader _header;
    private bool _isClosed;

    /// <summary>
    ///     电平分布
    /// </summary>
    private List<List<LevelDistribution>> _levelDist;

    private short[] _max;
    private short[] _min;

    /// <summary>
    ///     上次存储数据的时间
    /// </summary>
    private DateTime _preWriteDataTime = DateTime.MinValue;

    /// <summary>
    ///     文件存储的相对路径
    /// </summary>
    private string _relativePath;

    /// <summary>
    ///     文件存储目录
    ///     = <see cref="_rootDir" /> + <see cref="_relativePath" />
    /// </summary>
    private string _saveDir;

    private double _startFrequency;
    private double _stepFrequency;
    private double _stopFrequency;
    private long[] _sum;
    private TaskInfo _taskInfo;
    private int _total;
    private float[] _value;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="dir">存储根路径</param>
    /// <param name="interval">存储间隔 单位ms</param>
    /// <param name="fileType"></param>
    /// <param name="isCompress"></param>
    /// <param name="isSplit"></param>
    public ReportDataSave(string dir,
        int interval,
        FileDataType fileType,
        bool isCompress = false,
        bool isSplit = false)
    {
        _fileType = fileType;
        _rootDir = dir;
        _interval = interval;
        _isSplit = isSplit;
        _isCompress = isCompress;
    }

    public int SegmentIndex { get; private set; }

    public void Dispose()
    {
        Stop();
    }

    public event EventHandler<FileSavedNotification> FileSaveChanged;

    public void Initialized(int segmentIndex,
        double startFrequency,
        double stopFrequency,
        double stepFrequency,
        string edgeId,
        TaskInfo taskInfo)
    {
        _edgeId = edgeId;
        var timeFolderName = taskInfo.CreateTime.ToString("yyyyMMdd");
        var taskDir = Path.Combine(_rootDir, timeFolderName);
        if (!Directory.Exists(taskDir)) Directory.CreateDirectory(taskDir);
        var taskFolderName = taskInfo.Id.ToString();
        _saveDir = Path.Combine(taskDir, taskFolderName);
        if (!Directory.Exists(_saveDir)) Directory.CreateDirectory(_saveDir);
        _relativePath = Path.Combine(timeFolderName, taskFolderName).Replace('\\', '/');
        _taskInfo = taskInfo;
        SegmentIndex = segmentIndex;
        var factor = _taskInfo.Factors?.Find(item => item.SegmentOffset == segmentIndex);
        if (factor != null) _factor.AddRange(factor.Data);
        _startFrequency = startFrequency;
        _stopFrequency = stopFrequency;
        _stepFrequency = stepFrequency;
        _total = Magneto.Contract.Utils.GetTotalCount(startFrequency, stopFrequency, stepFrequency);
        _value = new float[_total];
        _max = Enumerable.Repeat(short.MinValue, _total).ToArray();
        _min = Enumerable.Repeat(short.MaxValue, _total).ToArray();
        _avg = new short[_total];
        _sum = new long[_total];
        _levelDist = new List<List<LevelDistribution>>();
        for (var i = 0; i < _total; i++) _levelDist.Add(new List<LevelDistribution>());
        _header = new ReportDataHeader
        {
            Interval = _interval,
            StartTime = Magneto.Contract.Utils.GetTimestamp(_taskInfo.BeginTime) / 1000000,
            MonitorInfo = new MonitorInfo
            {
                TaskId = _taskInfo.Id,
                Segment = new Dictionary<string, object>
                {
                    { ParameterNames.StartFrequency, startFrequency },
                    { ParameterNames.StopFrequency, stopFrequency },
                    { ParameterNames.StepFrequency, stepFrequency }
                },
                StartIndex = 0,
                Factors = _factor
            }
        };
    }

    public void Start()
    {
        _preWriteDataTime = DateTime.Now;
        _convertDataTask = Task.Run(() => ConvertDataAsync(_cts.Token).ConfigureAwait(false));
    }

    public void Stop()
    {
        if (_isClosed) return;
        _cts?.Cancel();
        _dataCache.Clear();
        Thread.Sleep(100);
        SaveData(true);
        _cts?.Dispose();
        _convertDataTask?.Dispose();
        _isClosed = true;
    }

    /// <summary>
    ///     获取当前正在写入的文件名
    /// </summary>
    public string GetNowWritingDataFileName()
    {
        return _dataFile?.FileFullName;
    }

    public void SetData(List<object> data)
    {
        foreach (var obj in data)
        {
            if (obj is SDataFactor factor)
            {
                if (factor.SegmentOffset != SegmentIndex) continue;
                _factor = factor.Data.ToList();
                _header.MonitorInfo.Factors = _factor;
                continue;
            }

            if (obj is not SDataScan scan) continue;
            if (scan.SegmentOffset != SegmentIndex) continue;
            _dataCache.Enqueue(scan);
        }
    }

    private async Task ConvertDataAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_dataCache.IsEmpty)
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            if (!_dataCache.TryDequeue(out var scan))
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                continue;
            }

            try
            {
                if (scan.Offset + scan.Data.Length <= _total)
                {
                    var scanData = Array.ConvertAll(scan.Data, item => item / 10f);
                    Array.Copy(scanData, 0, _value, scan.Offset, scan.Data.Length);
                }

                if (scan.Offset + scan.Data.Length == _total) ConvertData(_value);
                SaveData();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }

    private void SaveData(bool isClose = false)
    {
        // 如果有跨天则月报模式下需要分包
        var dayChange = _dataFile?.FileType == FileDataType.Report && DateTime.Now.Day != _preWriteDataTime.Day;
        if (!isClose && !dayChange && !IsTimeChanged()) return;
        _dataFile ??= new ReportDataFile(_fileType, _isCompress);
        var fileLen = _dataFile.FileSize;
        if (fileLen == 0)
        {
            var fileName =
                $"{_edgeId}-{DateTime.Now:yyyyMMddHHmmssfff}-{_startFrequency}MHz-{_stopFrequency}MHz-{_stepFrequency}kHz";
            _dataFile.CreateFile(_saveDir, fileName, GetHeaderData());
            var name = _dataFile.FileName;
            var startTime = _dataFile.StartTime;
            fileLen = _dataFile.FileSize;
            var count = _dataFile.RecordCount;
            NotifyFileSavedCreate(name, startTime, count, fileLen);
        }

        var data = SerializeData();
        _dataFile.WriteData(data, DateTime.Now);
        if ((_isSplit && fileLen > _splitSize)
            || (_isSplit
                && _dataFile.FileType == FileDataType.SignalCensus
                && DateTime.Now.Subtract(_dataFile.StartTime).TotalMinutes > 15)
            || (_dataFile.FileType == FileDataType.Report
                && DateTime.Now.Day != _dataFile.StartTime.Day)
            || isClose)
        {
            // 获取最新的文件大小
            _dataFile.CloseFile();
            fileLen = _dataFile.FileSize;
            var startTime = _dataFile.StartTime;
            var stopTime = _dataFile.StopTime;
            var count = _dataFile.RecordCount;
            var name = _dataFile.FileName;
            NotifyFileSavedComplete(name, startTime, stopTime, count, fileLen);
            _dataFile = null;
        }

        ResetData();
        _preWriteDataTime = DateTime.Now;
    }

    /// <summary>
    ///     返回时间是否变化
    ///     日报月报的保存需要整15分钟保存：
    ///     15 30 45 00
    /// </summary>
    /// <returns></returns>
    private bool IsTimeChanged()
    {
        if (_fileType != FileDataType.Report)
            return DateTime.Now.Subtract(_preWriteDataTime).TotalMilliseconds >= _interval;
        var time = DateTime.Now;
        if (time.Minute == _preWriteDataTime.Minute) return false;
        if (time.Minute != 15 && time.Minute != 30 && time.Minute != 45 && time.Minute != 00) return false;
        Console.Write($"日报时间变化,pre:{_preWriteDataTime:HH:mm:ss.fff},now:{time:HH:mm:ss.fff}");
        return true;
    }

    private void ConvertData(float[] data)
    {
        _count++;
        for (var i = 0; i < data.Length; i++)
        {
            var value = (short)data[i];
            if (value < -128) value = -128;
            if (value > 127) value = 127;
            _sum[i] += value;
            _max[i] = Math.Max(_max[i], value);
            _min[i] = Math.Min(_min[i], value);
            _avg[i] = (short)((double)_sum[i] / _count);
            try
            {
                var level = _levelDist[i].Find(item => item.Value == value);
                if (level.Count == 0) //没有这个幅度
                {
                    var newLevel = new LevelDistribution
                    {
                        Index = (byte)_levelDist[i].Count,
                        Count = 1,
                        Value = value
                    };
                    _levelDist[i].Add(newLevel);
                }
                else
                {
                    //这里将计数限定在ushort的最大值
                    if (level.Count < 65535)
                    {
                        //找到了这个幅度
                        level.Count++;
                        _levelDist[i][level.Index] = level;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
    }

    /// <summary>
    ///     统计完一次以后需要将数据初始化重新统计
    /// </summary>
    private void ResetData()
    {
        _count = 0;
        _max = Enumerable.Repeat(short.MinValue, _total).ToArray();
        _min = Enumerable.Repeat(short.MaxValue, _total).ToArray();
        _avg = new short[_total];
        _sum = new long[_total];
        for (var i = 0; i < _total; i++) _levelDist[i].Clear();
    }

    /// <summary>
    ///     获取文件头字节数组
    /// </summary>
    private byte[] GetHeaderData()
    {
        return MessagePackSerializer.Serialize(_header);
    }

    /// <summary>
    ///     序列化数据
    /// </summary>
    private byte[] SerializeData()
    {
        var dataLen = 0;
        foreach (var list in _levelDist) dataLen += 1 + list.Count * (1 + 2);
        var buffer = new byte[dataLen];
        var offset = 0;
        foreach (var t in _levelDist)
        {
            // 1. 幅度个数
            buffer[offset] = (byte)t.Count;
            offset++;
            for (var j = 0; j < t.Count; j++)
            {
                // 1. 幅度 值
                var levelVal = t[j].Value;
                if (levelVal < 0) levelVal = (short)(127 - levelVal);
                buffer[offset] = (byte)levelVal;
                offset++;
                var tmp = BitConverter.GetBytes(t[j].Count);
                Array.Copy(tmp, 0, buffer, offset, tmp.Length);
                offset += tmp.Length;
            }
        }

        return buffer;
    }

    private void NotifyFileSavedCreate(string fileName,
        DateTime startTime,
        long count,
        long size)
    {
        var msg = new FileSavedNotification
        {
            NotificationType = FileNotificationType.Created,
            TaskId = _taskInfo.Id.ToString(),
            DriverId = _taskInfo.ModuleId.ToString(),
            PluginId = _taskInfo.PluginId,
            RootPath = _rootDir.Replace('\\', '/'),
            RelativePath = _relativePath,
            ComputerId = RunningInfo.ComputerId,
            FileName = fileName,
            Parameters = "",
            DataType = _fileType,
            BeginRecordTime = Magneto.Contract.Utils.GetTimestamp(startTime),
            EndRecordTime = null,
            LastModifiedTime = Magneto.Contract.Utils.GetTimestamp(startTime),
            RecordCount = count,
            Size = size
        };
        Trace.WriteLine($"发送创建文件消息:{msg.FileName},Type:{msg.DataType}");
        FileSaveChanged?.Invoke(this, msg);
    }

    private void NotifyFileSavedComplete(string fileName,
        DateTime startTime,
        DateTime stopTime,
        long count,
        long size)
    {
        var msg = new FileSavedNotification
        {
            NotificationType = FileNotificationType.Modified,
            TaskId = _taskInfo.Id.ToString(),
            DriverId = _taskInfo.ModuleId.ToString(),
            PluginId = _taskInfo.PluginId,
            RootPath = _rootDir.Replace('\\', '/'),
            RelativePath = _relativePath,
            ComputerId = RunningInfo.ComputerId,
            FileName = fileName,
            Parameters = "",
            DataType = _fileType,
            BeginRecordTime = Magneto.Contract.Utils.GetTimestamp(startTime),
            EndRecordTime = Magneto.Contract.Utils.GetTimestamp(stopTime),
            LastModifiedTime = Magneto.Contract.Utils.GetTimestamp(stopTime),
            RecordCount = count,
            Size = size
        };
        Trace.WriteLine($"发送修改文件消息:{msg.FileName},Type:{msg.DataType}");
        FileSaveChanged?.Invoke(this, msg);
    }

    #region task

    private readonly CancellationTokenSource _cts = new();
    private Task _convertDataTask;

    #endregion

    #region 分包或压缩相关

    private readonly long _splitSize = 200 * 1024 * 1024;
    private readonly bool _isSplit;
    private readonly bool _isCompress;

    #endregion
}