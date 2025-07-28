using System;
using System.Collections.Generic;
using System.IO;
using Magneto.Protocol.Data;

namespace Magneto.Contract.SignalDemod;

public sealed class IqRecorder : IDisposable
{
    private bool _disposed;
    private IqDataProcess _iqDataProcess;
    private IqForDtsAnalyze _iqForDtsAnalyze;
    private volatile bool _running;
    private volatile bool _writeFile;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public event EventHandler<StorageCompletedEventArg> StorageCompleted;

    ~IqRecorder()
    {
        Dispose(false);
    }

    public void Start(string edgeId, Dictionary<double, long> iqDataLenConfig, double bw, double iqCalibration)
    {
        if (_running) Stop();
        _running = true;
        iqCalibration = iqCalibration.EqualTo(0, double.Epsilon) ? 1 : iqCalibration;
        var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSgldec);
        CheckAndCreateDirectory(savePath);
        _writeFile = false;
        _iqDataProcess = new IqDataProcess();
        _iqDataProcess.DataCanRead += IQDataProcess_DataCanRead;
        _iqForDtsAnalyze = new IqForDtsAnalyze();
        _iqForDtsAnalyze.StorageCompleted += IQForDTSAnalyze_StorageCompleted;
        var needDataLen = IqRecordHelper.GetIqDataLen(bw, iqDataLenConfig);
        _iqDataProcess.Start(needDataLen);
        _iqForDtsAnalyze.Start(edgeId, needDataLen, iqCalibration, savePath);
        _writeFile = true;
    }

    public void Stop()
    {
        _writeFile = false;
        if (_running) _iqForDtsAnalyze?.Stop();
        _running = false;
    }

    public void OnData(ref List<object> data)
    {
        if (_writeFile) //如果IQ数据存入文件或从文件读取
            if (data.Find(item => item is SDataIq) is SDataIq iqData)
                if (_writeFile) //写数据文件
                {
                    //IQ数据写入文件
                    _iqDataProcess.Revice(iqData);
                    //写入文件的IQ数据不发送
                    data.Remove(iqData);
                }
    }

    private void IQDataProcess_DataCanRead(object sender, EventArgs e)
    {
        _writeFile = false;
        while (true)
        {
            var iq = _iqDataProcess.ReadData();
            if (iq == null) break;
            var iqToSave = new SDataIqToSave
            {
                Attenuation = iq.Attenuation,
                DateTime = DateTime.Now,
                Frequency = iq.Frequency,
                IfBandWidth = iq.Bandwidth,
                SampleRate = iq.SamplingRate / 1e3
            };
            if (iq.Data16 != null)
            {
                var d = new short[iq.Data16.Length];
                iq.Data16.CopyTo(d, 0);
                iqToSave.Data16 = d;
            }
            else if (iq.Data32 != null)
            {
                var d = new int[iq.Data32.Length];
                iq.Data32.CopyTo(d, 0);
                iqToSave.Data32 = d;
            }

            _iqForDtsAnalyze.ReviceIqData(iqToSave);
        }
    }

    private void IQForDTSAnalyze_StorageCompleted(object obj, StorageCompletedEventArg e)
    {
        StorageCompleted?.Invoke(this, e);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _iqDataProcess?.Dispose();
            _iqForDtsAnalyze?.Dispose();
        }

        _disposed = true;
    }

    private static void CheckAndCreateDirectory(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName)) return;
        if (!Directory.Exists(folderName))
            try
            {
                Directory.CreateDirectory(folderName);
            }
            catch
            {
                // ignored
            }
    }
}