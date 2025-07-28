using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.SignalDemod;

public delegate void StorageCompletedEventHandler(object obj, StorageCompletedEventArg e);

public sealed class IqForDtsAnalyze : IDisposable
{
    // IQ数缓存队列
    private readonly ConcurrentQueue<SDataIqToSave> _iqData = new();
    private bool _canSave;
    private CancellationTokenSource _cts;
    private bool _disposed;

    /// <summary>
    ///     节点名称
    /// </summary>
    private string _edgeId = "";

    /// <summary>
    ///     信号频率
    /// </summary>
    private double _frequency;

    /// <summary>
    ///     IQ校准值
    /// </summary>
    private double _iqCalibration;

    // 当前IQ数据存储文件全路径
    private string _iqFileName = "";

    private long _needDataLen;

    // 当前数据中心频率和带宽
    private double _preFreq, _preBand;

    // 当前已存储的IQ数据长度
    private long _savedDataLen;

    // 数据存储路径
    private string _savePath = string.Empty;

    /// <summary>
    ///     信号带宽
    /// </summary>
    private float _signalBw;

    // 存储数据线程
    private Task _task;

    /// <summary>
    ///     是否保存到文件
    /// </summary>
    public bool SaveToFile { get; set; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // 数据接收完成事件
    public event StorageCompletedEventHandler StorageCompleted;

    ~IqForDtsAnalyze()
    {
        Dispose(false);
    }

    /// <summary>
    /// </summary>
    /// <param name="iQToSave"></param>
    public void ReviceIqData(SDataIqToSave iQToSave)
    {
        if (_canSave) _iqData.Enqueue(iQToSave);
    }

    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="edgeId">节点名称</param>
    /// <param name="needDataLen"></param>
    /// <param name="iqCalibration">IQ校准值</param>
    /// <param name="iqSavePath">文件保存路径</param>
    public void Start(string edgeId, long needDataLen, double iqCalibration, string iqSavePath)
    {
        _savePath = iqSavePath;
        _edgeId = edgeId;
        _iqCalibration = iqCalibration;
        _needDataLen = needDataLen;
        _iqData.Clear();
        _savedDataLen = 0;
        _frequency = 0;
        _signalBw = 0;
        _canSave = true;
        _iqFileName = string.Empty;
        _preFreq = _preBand = 0;
        _cts = new CancellationTokenSource();
        _task = new Task(ThreadMethod, _cts.Token);
        _task.Start();
    }

    /// <summary>
    ///     停止
    /// </summary>
    public void Stop()
    {
        // 这样就可以让线程正常退出了
        _iqData.Clear();
        _preFreq = _preBand = 0;
        _canSave = false;
        Utils.CancelTask(_task, _cts);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing) _iqData.Clear();
        Utils.CancelTask(_task, _cts);
        _disposed = true;
    }

    private void ThreadMethod()
    {
        while (_canSave && _cts?.IsCancellationRequested == false)
            try
            {
                var b = _iqData.TryDequeue(out var data);
                if (!b || data == null)
                {
                    Thread.Sleep(5);
                    continue;
                }

                // 数据错误
                if (!SaveIq(data)) break;
                // 数据量够了
                if (_savedDataLen >= _needDataLen) break;
                Thread.Sleep(0);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                Thread.Sleep(0);
            }

        _canSave = false;
        //触发事件
        OnSaveComplete(new StorageCompletedEventArg(_edgeId, _frequency, _signalBw, _iqFileName));
    }

    private bool SaveIq(SDataIqToSave iqToSave)
    {
        //如果中心频率和中频带宽有改变
        if (Math.Abs(_preFreq - iqToSave.Frequency) > 1e-9 || Math.Abs(_preBand - iqToSave.IfBandWidth) > 1e-9)
        {
            if (!string.IsNullOrEmpty(_iqFileName)) return false;
            if (!Directory.Exists(_savePath)) Directory.CreateDirectory(_savePath);
            _iqFileName = Path.Combine(_savePath,
                $"{_edgeId}_{DateTime.Now:yyyyMMddHHmmssfff}_{iqToSave.Frequency}MHz_{iqToSave.IfBandWidth}kHz.txt");
            File.Create(_iqFileName).Close();
            _preFreq = iqToSave.Frequency;
            _preBand = iqToSave.IfBandWidth;
            WriteDataHead(_iqFileName, iqToSave);
            _frequency = iqToSave.Frequency;
            _signalBw = (float)iqToSave.IfBandWidth;
        }

        WriteIqData(_iqFileName, iqToSave);
        return true;
    }

    /// <summary>
    ///     写文件头数据
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="iqToSave"></param>
    private void WriteDataHead(string fileName, SDataIqToSave iqToSave)
    {
        var sb = new StringBuilder();
        sb.AppendLine("InputZoom\tTRUE");
        sb.AppendFormat("InputCenter\t{0}\r\n", Math.Round(iqToSave.Frequency * 1000000, 1));
        sb.AppendLine("InputRange\t1");
        sb.AppendLine("InputReflmped\t50.0");
        sb.AppendLine("XStart\t0.0");
        sb.AppendFormat("XDelta\t{0}\r\n", 1 / (iqToSave.SampleRate * 1000000));
        sb.AppendLine("XDomain\t2");
        sb.AppendLine("XUnit\tSec");
        sb.AppendLine("YUnit\tV");
        var startFreq = iqToSave.Frequency + iqToSave.IfBandWidth / 2000;
        sb.AppendFormat("FreqValidMax\t{0}\r\n", Math.Round(startFreq * 1000000, 1));
        var stopFreq = iqToSave.Frequency - iqToSave.IfBandWidth / 2000;
        sb.AppendFormat("FreqValidMin\t{0}\r\n", Math.Round(stopFreq * 1000000, 1));
        // Nov 2017
        string dtStr;
        try
        {
            dtStr = iqToSave.DateTime.ToString("Wed MMM dd HH:mm:ss.fff yyyy",
                CultureInfo.CreateSpecificCulture("en-GB"));
        }
        catch (CultureNotFoundException)
        {
            dtStr = iqToSave.DateTime.ToString("Wed MMM dd HH:mm:ss.fff yyyy");
        }

        sb.AppendFormat("TimeString\t{0}\r\n", dtStr);
        sb.AppendLine("Y\t");
        File.WriteAllText(fileName, sb.ToString());
    }

    private void WriteIqData(string fileName, SDataIqToSave iqToSave)
    {
        if (iqToSave.Data16 == null && iqToSave.Data32 == null) return;
        var sb = new StringBuilder();
        if (iqToSave.Data16 != null)
        {
            for (var i = 0; i < iqToSave.Data16.Length; i += 2)
                sb.AppendFormat("{0}\t{1}\r\n", iqToSave.Data16[i] / _iqCalibration,
                    iqToSave.Data16[i + 1] / _iqCalibration);
            _savedDataLen += iqToSave.Data16.Length;
        }
        else if (iqToSave.Data32 != null)
        {
            for (var i = 0; i < iqToSave.Data32.Length; i += 2)
                sb.AppendFormat("{0}\t{1}\r\n", iqToSave.Data32[i] / _iqCalibration,
                    iqToSave.Data32[i + 1] / _iqCalibration);
            _savedDataLen += iqToSave.Data32.Length;
        }

        File.AppendAllText(fileName, sb.ToString());
    }

    private void OnSaveComplete(StorageCompletedEventArg e)
    {
        StorageCompleted?.Invoke(null, e);
    }
}

public class SDataIqToSave
{
    public DateTime DateTime { get; set; }

    /// <summary>
    ///     中心频率，单位 MHz
    /// </summary>
    public double Frequency { get; set; }

    /// <summary>
    ///     中频带宽，单位 kHz
    /// </summary>
    public double IfBandWidth { get; set; }

    /// <summary>
    ///     采样率，单位 MHz
    /// </summary>
    public double SampleRate { get; set; }

    /// <summary>
    ///     衰减，单位dBuV
    /// </summary>
    public float Attenuation { get; set; }

    /// <summary>
    ///     IQ数据，I分量和Q分量依次存储,适用于16位采样
    /// </summary>
    public short[] Data16 { get; set; }

    /// <summary>
    ///     IQ数据，I分量和Q分量依次存储,适用于32位采样
    /// </summary>
    public int[] Data32 { get; set; }
}

/// <summary>
///     数据存储完成事件
/// </summary>
public class StorageCompletedEventArg : EventArgs
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="edgeId">节点名称</param>
    /// <param name="frequency">频率(MHz)</param>
    /// <param name="bandWidth">信号带宽(kHz)</param>
    /// <param name="strFile">存储数据的文件名</param>
    public StorageCompletedEventArg(string edgeId, double frequency, float bandWidth, string strFile = "")
    {
        EdgeId = edgeId;
        Frequency = frequency;
        BandWidth = bandWidth;
        DataFile = strFile;
    }

    /// <summary>
    ///     获取节点名称
    /// </summary>
    public string EdgeId { get; }

    /// <summary>
    ///     获取信号频率(MHz)
    /// </summary>
    public double Frequency { get; }

    /// <summary>
    ///     获取带宽(kHz)
    /// </summary>
    public float BandWidth { get; }

    /// <summary>
    ///     获取保存的数据文件
    /// </summary>
    public string DataFile { get; }
}