using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Data;

namespace Magneto.Contract.Algorithm;

public delegate void RecognizeResultHandler(SDataRecognize recognize);

/// <summary>
///     调制识别
/// </summary>
public sealed class Recognize
{
    private readonly ConcurrentQueue<SDataIq> _dataCache = new();
    private readonly int _maxPoint = 4096;
    private CancellationTokenSource _cts;
    private DateTime _lastRecognizeTime = DateTime.Now;
    private Task _processTask;
    private DcModRecognition _recognize;
    private bool _running;

    public event RecognizeResultHandler RecognizeResultArrived;

    public void Start()
    {
        _recognize = DcModRecognition.GetSingleton();
        _cts = new CancellationTokenSource();
        _processTask = Task.Run(() => ProcessAsync(_cts.Token));
        _running = true;
    }

    public void Stop()
    {
        _running = false;
        try
        {
            Clear();
            _cts?.Cancel();
            _processTask?.Dispose();
        }
        catch
        {
            // ignored
        }
    }

    public void Clear()
    {
        _dataCache.Clear();
        _recognize?.ClearModulationStatistics();
    }

    public void SetIqData(SDataIq iqData)
    {
        if (!_running) return;
        _dataCache.Enqueue(iqData);
    }

    private async Task ProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                if (_dataCache.IsEmpty) await Task.Delay(5).ConfigureAwait(false);
                if (!_dataCache.TryDequeue(out var iq)) await Task.Delay(1).ConfigureAwait(false);
                ProcessData(iq);
            }
            catch
            {
                // ignored
            }
    }

    private void ProcessData(SDataIq iqData)
    {
        if (iqData == null) return;
        if (DateTime.Now.Subtract(_lastRecognizeTime).TotalMilliseconds < 1000) return;
        _lastRecognizeTime = DateTime.Now;
        List<object> result = null;
        if (iqData.Data16?.Length > 0)
        {
            var len = iqData.Data16.Length / 2;
            len = len > _maxPoint ? _maxPoint : len;
            var iData = new short[len];
            var qData = new short[len];
            for (var i = 0; i < len; i++)
            {
                iData[i] = iqData.Data16[i * 2];
                qData[i] = iqData.Data16[i * 2 + 1];
            }

            result = _recognize.StatisticsTimes(iData, qData, iqData.SamplingRate / 1000, iqData.Frequency);
        }
        else if (iqData.Data32?.Length > 0)
        {
            var len = iqData.Data32.Length / 2;
            len = len > _maxPoint ? _maxPoint : len;
            var iData = new int[len];
            var qData = new int[len];
            for (var i = 0; i < len; i++)
            {
                iData[i] = iqData.Data32[i * 2];
                qData[i] = iqData.Data32[i * 2 + 1];
            }

            result = _recognize.StatisticsTimes(iData, qData, iqData.SamplingRate / 1000, iqData.Frequency);
        }

        if (result == null) return;
        var recognize = new SDataRecognize
        {
            RecognizeList = []
        };
        var per = (SortedList<ModulationType, double>)result[0];
        var desc = (Dictionary<ModulationType, string>)result[1];
        foreach (var pair in per)
        {
            if (pair.Value == 0) continue;
            string name;
            var t = pair.Key.ToString();
            var index = t.IndexOfAny(['1', '2', '3', '4', '6', '8']);
            if (index > 0)
            {
                name = t[index..];
                name = string.Concat(name, t[..index]);
            }
            else
            {
                name = t;
            }

            var description = desc.TryGetValue(pair.Key, out var value) ? value : string.Empty;
            var item = new RecognizeItem
            {
                Name = name,
                Percent = Math.Round(pair.Value * 100, 2),
                Description = description
            };
            recognize.RecognizeList.Add(item);
        }

        if (recognize.RecognizeList?.Count > 0) RecognizeResultArrived?.Invoke(recognize);
    }
}