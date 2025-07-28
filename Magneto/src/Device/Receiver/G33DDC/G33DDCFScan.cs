using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Device.G33DDC.SDK;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

public partial class G33Ddc
{
    private double[] _fscanFrequencies;
    private int _fscanIndex;
    private DateTime _fscanPreTime = DateTime.Now;
    private Task _fscanTask;
    private CancellationTokenSource _fscanTokenSource;
    private int _total;

    private void StartScan()
    {
        if (ScanMode == ScanMode.Pscan)
        {
            var step = (uint)(StepFrequency * 1000);
            // 这里可以将0-50MHz全部打通然后只取其中一部分
            //_device.SetSegments(start, stop, step);
            _device.SetSegments(0, 50000000, step);
            _device.StartIf();
        }
        else
        {
            _total = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
            _fscanFrequencies = new double[_total];
            _fscanIndex = -1;
            for (var i = 0; i < _total; i++) _fscanFrequencies[i] = StartFrequency + StepFrequency * i / 1000;
            if (!_device.PowerOn()) Trace.WriteLine("任务启动失败，设备启动失败");
            SetFscan();
            _fscanTokenSource = new CancellationTokenSource();
            _fscanTask = new Task(p => FScanProcessAsync(p).ConfigureAwait(false), _fscanTokenSource.Token);
            _fscanTask.Start();
        }
    }

    private void StopFscan()
    {
        ResetFscan();
        try
        {
            if (_fscanTokenSource != null)
            {
                _fscanTokenSource.Cancel();
                _fscanTokenSource.Dispose();
            }
        }
        catch
        {
        }
        finally
        {
            _fscanTokenSource = null;
        }

        try
        {
            _fscanTask?.Dispose();
        }
        catch
        {
        }
    }

    private async Task FScanProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var startSign = true;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Yield();
                if (token.IsCancellationRequested) break;
                var span = DateTime.Now.Subtract(_fscanPreTime).TotalMilliseconds;
                if ((startSign || _canFscanChangeFrequency)
                    && (_detector == DetectMode.Fast || (_detector != DetectMode.Fast && span >= _measureTime)))
                {
                    if (_fscanIndex >= 0)
                    {
                        var level = GetLevelData(_detector);
                        if (float.IsNaN(level)) continue;
                        var scan = new SDataScan
                        {
                            Offset = _fscanIndex,
                            Total = _total,
                            StartFrequency = StartFrequency,
                            StopFrequency = StopFrequency,
                            StepFrequency = StepFrequency,
                            Data = new short[1]
                        };
                        scan.Data[0] = (short)(level * 10);
                        SendData(new List<object> { scan });
                    }

                    _fscanIndex++;
                    if (_fscanIndex >= _total) _fscanIndex = 0;
                    var freq = _fscanFrequencies[_fscanIndex];
                    _device.Stop();
                    _device.SetFrequency(freq);
                    var bandwidth = StepFrequency;
                    _device.SetIfBandwidth(bandwidth);
                    _device.StartDdc1();
                    _device.SetDdc2(0, 0, 0, 0);
                    _device.SetDemBandwidth(0, bandwidth);
                    _device.SetAgcMode(0, AgcMode.AgcFast);
                    _device.StartDdc2(0);
                    _fscanPreTime = DateTime.Now;
                    startSign = false;
                    var time = _measureTime;
                    if (_detector == DetectMode.Fast) time = 0;
                    ClearFScan(time, freq);
                }
            }
            catch
            {
            }
    }

    private void ProcessFscan(float level)
    {
        if (!_isInFScanMode || _canFscanChangeFrequency) return;
        _levelCount++;
        if (_levelMax < level) _levelMax = level;
        if (_levelMin > level) _levelMin = level;
        _fscanLevel = level;
        _levelMean = (_levelMean * (_levelCount - 1) + level) / _levelCount;
        var span = DateTime.Now.Subtract(_preScanTime).TotalMilliseconds;
        if (span > _interval)
        {
            //Console.WriteLine($"频率:{_frequency},gain:{gain},电平:{_fscanLevel},max:{_levelMax},mean:{_levelMean},peak:{_peak},rms:{_rms},span:{span}");
            _preScanTime = DateTime.Now;
            _canFscanChangeFrequency = true;
        }
    }

    #region FScan

    private bool _canFscanChangeFrequency;
    private int _levelCount;
    private float _levelMax = float.MinValue;
    private float _levelMin = float.MaxValue;
    private float _levelMean;
    private float _peak;
    private float _rms;
    private float _fscanLevel;
    private readonly int _interval = 0;
    private DateTime _preScanTime = DateTime.Now;
    private bool _isInFScanMode;
    private double _fscanFrequency;

    internal void SetFscan()
    {
        _isInFScanMode = true;
    }

    internal void ResetFscan()
    {
        _isInFScanMode = false;
    }

    internal void ClearFScan(int interval, double frequency)
    {
        _fscanFrequency = frequency;
        _canFscanChangeFrequency = false;
        _levelCount = 0;
        _levelMax = float.MinValue;
        _levelMin = float.MaxValue;
        _levelMean = 0f;
        _rms = 0;
        _peak = 0;
        _preScanTime = DateTime.Now;
    }

    internal float GetLevelData(DetectMode mode)
    {
        if (_device?.GetLevel(0, out var peak, out var rms) == true)
        {
            _peak = peak;
            _rms = rms;
        }

        return mode switch
        {
            DetectMode.Fast => _fscanLevel,
            DetectMode.Pos => _peak,
            DetectMode.Avg => _levelMean,
            DetectMode.Rms => _rms,
            _ => float.NaN
        };
    }

    #endregion
}