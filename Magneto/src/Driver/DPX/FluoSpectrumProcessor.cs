using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Magneto.Protocol.Data;

namespace DPX;

internal class FluoSpectrumProcessor
{
    public const int IntervalMilliseconds = 500;
    public const byte MaxCount = 100;
    private static readonly object _syncObj = new();
    private readonly Queue<float[]> _queue;
    private readonly Timer _timer;
    private SDataSpectrum _current;
    private byte[][] _fluoData;
    public EventHandler<SDataFluoSpec> ProcessCompleted;

    public FluoSpectrumProcessor()
    {
        _queue = new Queue<float[]>();
        _timer = new Timer(IntervalMilliseconds);
        _timer.Elapsed += Timer_Elapsed;
    }

    public short MinLevel { get; private set; } = -20;
    public short MaxLevel { get; private set; } = 80;
    public bool State { get; private set; }

    public void Start()
    {
        if (State) return;
        lock (_syncObj)
        {
            ResetData();
            _timer.Start();
            State = true;
        }
    }

    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="spectrum">频谱数据</param>
    public void AddData(SDataSpectrum spectrum)
    {
        if (spectrum?.Data?.Any() != true) return;
        var minLevel = (short)Math.Floor(spectrum.Data.Min() / 10f);
        var maxLevel = (short)Math.Ceiling(spectrum.Data.Max() / 10f);
        lock (_syncObj)
        {
            MinLevel = Math.Min(MinLevel, minLevel);
            MaxLevel = Math.Max(MaxLevel, maxLevel);
            var row = MaxLevel - MinLevel + 1;
            var data = Array.ConvertAll(spectrum.Data, item => item / 10f);
            if (data.Length == 0) return;
            _current = spectrum;
            _queue.Enqueue(data);
            if (_fluoData == null)
            {
                _fluoData = new byte[row][];
                for (var i = 0; i < row; i++) _fluoData[i] = new byte[data.Length];
            }

            for (var i = 0; i < data.Length; i++)
            {
                var level = (int)data[i];
                if (level < MinLevel || level > MaxLevel) continue;
                var rowIndex = MaxLevel - level;
                _fluoData[rowIndex][i]++;
            }

            if (_queue.Count > MaxCount)
            {
                var temp = _queue.Dequeue();
                for (var i = 0; i < temp.Length; i++)
                {
                    var level = (int)temp[i];
                    if (level < MinLevel || level > MaxLevel) continue;
                    var rowIndex = MaxLevel - level;
                    if (_fluoData[rowIndex][i] > 0) _fluoData[rowIndex][i]--;
                }
            }
        }
    }

    public SDataFluoSpec GetFluoData()
    {
        lock (_syncObj)
        {
            if (_fluoData == null) return null;
            var temp = new byte[_fluoData.Length][];
            Array.Copy(_fluoData, temp, _fluoData.Length);
            return new SDataFluoSpec
            {
                Frequency = _current.Frequency,
                Span = _current.Span,
                Data = temp,
                MaxLevel = MaxLevel,
                MinLevel = MinLevel
            };
        }
    }

    public void Stop()
    {
        if (!State) return;
        lock (_syncObj)
        {
            State = false;
            _timer.Stop();
            ResetData();
        }
    }

    public void Reset()
    {
        _timer.Stop();
        ResetData();
        _timer.Start();
    }

    private void ResetData()
    {
        lock (_syncObj)
        {
            _queue.Clear();
            _fluoData = null;
            _current = null;
        }
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        var data = GetFluoData();
        if (data == null) return;
        ProcessCompleted?.Invoke(this, data);
    }
}