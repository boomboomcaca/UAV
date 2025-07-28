using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Device.G33DDC.SDK;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.G33DDC;

public partial class G33Ddc
{
    private readonly object _lockMscanPoints = new();
    private int _mscanIndex;
    private MScanTemplate[] _mscans;
    private Task _mscanTask;
    private CancellationTokenSource _mscanTokenSource;
    private int _mscanTotalPoints;

    internal void StartMscan()
    {
        lock (_lockMscanPoints)
        {
            _mscans = _mscanPoints == null ? null : Array.ConvertAll(_mscanPoints, item => (MScanTemplate)item);
        }

        if (_mscans == null || _mscans.Length == 0) return;
        _mscanTotalPoints = _mscans.Length;
        _mscanIndex = -1;
        _mscanTokenSource = new CancellationTokenSource();
        _mscanTask = new Task(p => MScanProcessAsync(p).ConfigureAwait(false), _mscanTokenSource.Token);
        _mscanTask.Start();
    }

    internal void StopMscan()
    {
        try
        {
            if (_mscanTokenSource != null)
            {
                _mscanTokenSource.Cancel();
                _mscanTokenSource.Dispose();
            }
        }
        catch
        {
        }
        finally
        {
            _mscanTokenSource = null;
        }

        try
        {
            _mscanTask?.Dispose();
        }
        catch
        {
        }
    }

    private async Task MScanProcessAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        var startSign = true;
        while (!token.IsCancellationRequested)
            try
            {
                await Task.Delay(1).ConfigureAwait(false);
                if (token.IsCancellationRequested) break;
                if (startSign || CanMScanChangeFreq())
                {
                    _device.Stop();
                    ResetMScanSign();
                    _mscanIndex++;
                    if (_mscanIndex >= _mscanTotalPoints) _mscanIndex = 0;
                    var mscan = _mscans[_mscanIndex];
                    var freq = mscan.Frequency;
                    _device.SetFrequency(freq);
                    //Console.WriteLine($"开始驻留频点:{freq}");
                    var bandwidth = mscan.FilterBandwidth;
                    _device.SetIfBandwidth(bandwidth);
                    _mscanCurrentFreq = freq;
                    _mscanCurrentBw = bandwidth;
                    ////
                    _frequency = freq;
                    _ifBandwidth = bandwidth;
                    ////
                    _device.StartDdc1();
                    _device.SetDdc2(0, 0, 0, 0);
                    _device.SetDemBandwidth(0, bandwidth);
                    _device.SetAgcEnabled(0, true);
                    _device.SetAgcMode(0, AgcMode.AgcFast);
                    _device.StartDdc2(0);
                    if (_audioSwitch && CurFeature == FeatureType.MScne) _device.StartAudio(0, 50);
                    startSign = false;
                    if (_detector == DetectMode.Fast)
                    {
                    }
                }
            }
            catch
            {
            }
    }

    private void ResetMScanSign()
    {
        _mscanPreTime = DateTime.Now;
        _mscanComplete = false;
        _mscanHoldOk = false;
        _mscanPreHoldTime = DateTime.Now;
        _mscanPreDwellTime = DateTime.Now;
    }

    private bool CanMScanChangeFreq()
    {
        //if (!_dwellSwitch)
        //{
        //    return _detector == DetectMode.FAST || (_detector != DetectMode.FAST && span >= _measureTime);
        //}
        if (_mscanComplete) return true;
        return false;
    }

    private List<object> ProcessMScan(float rms)
    {
        List<object> list = new();
        if (_mscanComplete) return list;
        //if (_mscanHoldOk)
        //{
        //    return list;
        //}
        // 离散扫描数据组装
        var scan = new SDataScan
        {
            Total = _mscanTotalPoints,
            Offset = _mscanIndex,
            Data = new short[1]
        };
        _level = rms;
        scan.Data[0] = (short)(_level * 10);
        list.Add(scan);
        if (CurFeature == FeatureType.MScan)
        {
            // 离散模式下每个频点只收一次数据
            _mscanComplete = true;
            return list;
        }

        var overThreshold = !SquelchSwitch || SquelchThreshold <= _level;
        if (overThreshold && !_mscanHoldOk)
        {
            _mscanHoldOk = true;
            _mscanPreDwellTime = DateTime.Now;
        }
        else if (!_mscanHoldOk && DateTime.Now.Subtract(_mscanPreHoldTime).TotalMilliseconds > _holdTime)
        {
            _mscanHoldOk = true;
            _mscanComplete = true;
            return list;
        }

        return list;
    }

    private List<object> ProcessMScan(float[] data)
    {
        List<object> list = new();
        if (_mscanComplete) return list;
        if (!_mscanHoldOk) return list;
        if (_mscanHoldOk && DateTime.Now.Subtract(_mscanPreDwellTime).TotalMilliseconds > _dwellTime * 1000)
            _mscanComplete = true;
        //var spec = new SDataSpectrum()
        //{
        //    Frequency = _mscanCurrentFreq,
        //    Span = _mscanCurrentBw,
        //    Data = Array.ConvertAll(data, item => (short)((item + 107) * 10))
        //};
        //list.Add(spec);
        var spec = CacheSpectrum(data);
        if (spec?.Count > 0) list.AddRange(spec);
        return list;
    }

    #region 驻留相关

    private bool _mscanHoldOk;

    /// <summary>
    ///     一个频点结束了，可以进行下一个频点了
    /// </summary>
    private bool _mscanComplete;

    private DateTime _mscanPreHoldTime = DateTime.Now;
    private DateTime _mscanPreDwellTime = DateTime.Now;
    private DateTime _mscanPreTime = DateTime.Now;
    private double _mscanCurrentFreq;
    private double _mscanCurrentBw;

    #endregion
}