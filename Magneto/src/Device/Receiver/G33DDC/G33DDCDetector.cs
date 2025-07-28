using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

public partial class G33Ddc
{
    private struct LevelData
    {
        public readonly float Level;
        public readonly float Pos;
        public readonly float Rms;

        public LevelData(float level, float pos, float rms)
        {
            Level = level;
            Pos = pos;
            Rms = rms;
        }
    }

    #region 频谱检波方式处理

    private Task _detectorTask;
    private CancellationTokenSource _detectorCts;
    private readonly ConcurrentQueue<float[]> _spectrumsCache = new();
    private readonly ConcurrentQueue<LevelData> _levelsCache = new();
    private DateTime _preDetectorTime = DateTime.Now;

    private void StartDetector()
    {
        _detectorCts = new CancellationTokenSource();
        _preDetectorTime = DateTime.Now;
        _detectorTask = new Task(p => ProcessDetectorAsync(p).ConfigureAwait(false), _detectorCts.Token);
        _detectorTask.Start();
    }

    private void StopDetector()
    {
        try
        {
            _detectorCts?.Cancel();
        }
        catch
        {
        }

        try
        {
            _detectorTask?.Dispose();
        }
        catch
        {
        }

        _spectrumsCache?.Clear();
    }

    private List<object> CacheSpectrum(float[] data)
    {
        List<object> list = new();
        if (_detector == DetectMode.Fast)
        {
            var spec = new SDataSpectrum
            {
                Frequency = _frequency,
                Span = _ifBandwidth,
                Data = Array.ConvertAll(data, item => (short)((item + 107) * 10))
            };
            list.Add(spec);
            return list;
        }

        _spectrumsCache.Enqueue(data);
        return null;
    }

    private List<object> CacheLevel(float level, float pos, float rms)
    {
        List<object> list = new();
        if (_detector == DetectMode.Fast)
        {
            _level = level + LevelDataCalibration;
            var data = new SDataLevel
            {
                Frequency = _frequency,
                Bandwidth = _ifBandwidth,
                Data = _level
            };
            list.Add(data);
            return list;
        }

        _levelsCache.Enqueue(new LevelData(level, pos, rms));
        return null;
    }

    private async Task ProcessDetectorAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(1, token).ConfigureAwait(false);
                if (_spectrumsCache.IsEmpty) continue;
                if (DateTime.Now.Subtract(_preDetectorTime).TotalMilliseconds > _measureTime)
                {
                    List<float[]> specs = new();
                    List<LevelData> levels = new();
                    while (_spectrumsCache.TryDequeue(out var bf)) specs.Add(bf);
                    while (_levelsCache.TryDequeue(out var lv)) levels.Add(lv);
                    _preDetectorTime = DateTime.Now;
                    List<object> data = new();
                    if (specs.Count > 0)
                    {
                        var spec = ProcessSpectrum(specs);
                        if (spec != null) data.Add(spec);
                    }

                    if (levels.Count > 0)
                    {
                        var level = ProcessLevel(levels);
                        if (level != null) data.Add(level);
                    }

                    if (data.Count > 0) SendData(data);
                }
            }
            catch
            {
            }
    }

    private SDataLevel ProcessLevel(List<LevelData> levels)
    {
        var count = levels.Count;
        if (count == 0) return null;
        var level = 0f;
        switch (_detector)
        {
            case DetectMode.Pos:
                level = levels.Max(item => item.Pos);
                break;
            case DetectMode.Avg:
                level = levels.Sum(item => item.Level) / count;
                break;
            case DetectMode.Rms:
                // 计算均方根的方法可能是错的
                level = 10 * (float)Math.Log10(levels.Sum(item => Math.Pow(10, item.Rms / 10)) / count);
                break;
            case DetectMode.Fast:
            default:
                return null;
        }

        _level = level + LevelDataCalibration;
        var data = new SDataLevel
        {
            Frequency = _frequency,
            Bandwidth = _ifBandwidth,
            Data = _level
        };
        return data;
    }

    /// <summary>
    ///     处理频谱数据
    /// </summary>
    /// <param name="specs"></param>
    private SDataSpectrum ProcessSpectrum(List<float[]> specs)
    {
        var count = specs.Count;
        var len = specs.Last().Length;
        var sums = new float[len];
        var max = new float[len];
        Array.Fill(max, float.MinValue);
        var rms = new double[len];
        var total = 0;
        for (var i = 0; i < count; i++)
        {
            if (specs[i].Length != len) continue;
            total++;
            for (var j = 0; j < len; j++)
            {
                var vl = specs[i][j];
                sums[j] += vl;
                if (max[j] < vl) max[j] = vl;
                // rms
                var x2 = Math.Pow(10, vl / 10);
                rms[j] += x2;
            }
        }

        var spec = new SDataSpectrum
        {
            Frequency = _frequency,
            Span = _ifBandwidth
        };
        switch (_detector)
        {
            case DetectMode.Pos:
                spec.Data = Array.ConvertAll(max, item => (short)((item + 107) * 10));
                break;
            case DetectMode.Avg:
                spec.Data = Array.ConvertAll(sums, item => (short)((item / total + 107) * 10));
                break;
            case DetectMode.Rms:
                spec.Data = Array.ConvertAll(rms, item => (short)((10 * Math.Log10(item / total) + 107) * 10));
                break;
            case DetectMode.Fast:
            default:
                return null;
        }

        return spec;
    }

    #endregion
}