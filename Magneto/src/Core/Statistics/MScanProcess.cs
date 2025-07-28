using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Statistics;

public class MScanProcess(IAntennaController antennaController) : DataProcessBase(antennaController)
{
    private readonly ConcurrentDictionary<int, FrequencyInfo> _frequencies = new();
    private readonly object _lockPoints = new();

    /// <summary>
    ///     占用度统计阈值，提取信号用，单位 %，范围 0~100
    /// </summary>
    private readonly double _occupancyThreshold = 0;

    private Task _calTask;
    private CancellationTokenSource _cts;
    private int _freqNumber;
    private DateTime _prevTime = DateTime.Now;
    private short[] _readtimeData;
    private ConcurrentQueue<short[]> _scanDataCache = new();
    private int _scanTimes;
    private bool _switch;
    private int[] _thresholds;
    private int[] _upperTimes;

    public override void SetParameter(Parameter parameter)
    {
        base.SetParameter(parameter);
        if (parameter.Name == ParameterNames.MscanPoints
            && parameter.Value is Dictionary<string, object>[] value)
            lock (_lockPoints)
            {
                _freqNumber = value.Length;
                _upperTimes = new int[_freqNumber];
                _scanTimes = 0;
                _readtimeData = new short[_freqNumber];
                _thresholds = new int[_freqNumber];
                _frequencies.Clear();
                for (var i = 0; i < _freqNumber; i++)
                {
                    if (value[i].ContainsKey(ParameterNames.MeasureThreshold)
                        && int.TryParse(value[i][ParameterNames.MeasureThreshold].ToString(), out var threshold))
                        _thresholds[i] = threshold;
                    if (value[i].ContainsKey(ParameterNames.Frequency)
                        && double.TryParse(value[i][ParameterNames.Frequency].ToString(), out var freq))
                    {
                        var modulation = Modulation.None;
                        if (value[i].ContainsKey(ParameterNames.DemMode))
                            modulation =
                                Magneto.Contract.Utils.ConvertStringToEnum<Modulation>(value[i][ParameterNames.DemMode]
                                    ?.ToString());
                        var bandwidth = 0d;
                        if (value[i].ContainsKey(ParameterNames.FilterBandwidth)
                            && double.TryParse(value[i][ParameterNames.FilterBandwidth].ToString(), out var fbw))
                            bandwidth = fbw;
                        else if (value[i].ContainsKey(ParameterNames.IfBandwidth)
                                 && double.TryParse(value[i][ParameterNames.IfBandwidth].ToString(), out var ibw))
                            bandwidth = ibw;
                        var info = new FrequencyInfo(freq, bandwidth, modulation);
                        _frequencies.TryAdd(i, info);
                    }
                }
            }
        // if (parameter.Name == "measureSwitch" && bool.TryParse(parameter.Value.ToString(), out bool swi))
        // {
        //     _switch = swi;
        //     if (!_switch)
        //     {
        //         _scanDataCache?.Clear();
        //     }
        //     _prevTime = DateTime.Now;
        // }
    }

    public override void Start()
    {
        base.Start();
        try
        {
            _calTask?.Dispose();
        }
        catch
        {
            // ignored
        }

        _scanDataCache = new ConcurrentQueue<short[]>();
        _cts = new CancellationTokenSource();
        _calTask = Task.Run(() => CalThreadAsync(_cts.Token).ConfigureAwait(false));
        // 现在离散扫描改为占用度计算常开了
        _switch = true;
        _prevTime = DateTime.Now;
    }

    public override void Stop()
    {
        base.Stop();
        _scanDataCache?.Clear();
        _cts?.Cancel();
        try
        {
            _calTask?.Dispose();
        }
        catch
        {
            // ignored
        }

        _calTask = null;
        //
    }

    public override void OnData(List<object> data)
    {
        if (!_switch) return;
        foreach (var item in data)
            if (item is SDataScan scan)
            {
                if (scan.Total != _freqNumber || scan.Offset + scan.Data.Length > _freqNumber) continue;
                Buffer.BlockCopy(scan.Data, 0, _readtimeData, scan.Offset * sizeof(short),
                    scan.Data.Length * sizeof(short));
                if (scan.Offset + scan.Data.Length == _freqNumber)
                {
                    var array = new short[_freqNumber];
                    Buffer.BlockCopy(_readtimeData, 0, array, 0, _freqNumber * sizeof(short));
                    _scanDataCache.Enqueue(array);
                }
            }
    }

    private async Task CalThreadAsync(object obj)
    {
        if (obj is not CancellationToken token) return;
        while (!token.IsCancellationRequested)
            try
            {
                if (_scanDataCache.IsEmpty)
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                    continue;
                }

                if (!_scanDataCache.TryDequeue(out var scanData))
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                    continue;
                }

                var freqNumbers = _freqNumber;
                if (scanData != null && scanData.Length != _freqNumber)
                {
                    _scanDataCache.Clear();
                    await Task.Delay(10, token).ConfigureAwait(false);
                    continue;
                }

                if (scanData != null && scanData.Length == freqNumbers)
                {
                    _scanTimes++;
                    var occupancy = new List<double>();
                    // 计算占用度
                    for (var i = 0; i < freqNumbers; i++)
                    {
                        var level = scanData[i] / 10f;
                        if (level > _thresholds[i]) _upperTimes[i]++;
                        var d = Math.Round((double)_upperTimes[i] / _scanTimes * 100.0d, 1);
                        occupancy.Add(d);
                        if (_frequencies.TryGetValue(i, out var info)) info.SetData(level);
                    }

                    var dtNow = DateTime.Now;
                    var diff1 = dtNow.Subtract(_prevTime);
                    if (diff1.TotalSeconds > 2)
                    {
                        var data = new SegmentOccupancyData
                        {
                            SegmentIndex = 0,
                            Occupancy = occupancy.ConvertAll(item => (short)(item * 10)).ToArray()
                        };
                        var occ = new SDataOccupancy
                        {
                            Data = new SegmentOccupancyData[1]
                        };
                        occ.Data[0] = data;
                        var signals = ExtractSignals(occupancy);
                        SendData([occ, signals]);
                        _prevTime = dtNow;
                    }
                }
                else
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
            }
    }

    private SDataMScanSignals ExtractSignals(List<double> occupancy)
    {
        List<MScanSignalsResult> list = [];
        for (var i = 0; i < occupancy.Count; i++)
        {
            if (occupancy[i] < _occupancyThreshold) continue;
            if (_frequencies.TryGetValue(i, out var info))
            {
                info.UpdateTime();
                MScanSignalsResult res = new()
                {
                    FrequencyIndex = i,
                    Frequency = info.Frequency,
                    Bandwidth = info.Bandwidth,
                    Level = info.Level,
                    MaxLevel = info.Max,
                    MinLevel = info.Min,
                    Modulation = info.Modulation,
                    FirstTime = info.FirstTime,
                    LastTime = info.LastTime,
                    Occupancy = (float)occupancy[i]
                };
                list.Add(res);
            }
        }

        SDataMScanSignals signals = new()
        {
            Data = list
        };
        return signals;
    }

    private class FrequencyInfo(double frequency, double bandwidth, Modulation modulation)
    {
        private DateTime _firstTime = DateTime.MinValue;
        private DateTime _lastTime;

        public double Frequency { get; } = frequency;
        public double Bandwidth { get; } = bandwidth;
        public float Level { get; private set; }
        public float Max { get; private set; } = float.MinValue;
        public float Min { get; private set; } = float.MaxValue;
        public Modulation Modulation { get; } = modulation;
        public ulong FirstTime => Magneto.Contract.Utils.GetTimestamp(_firstTime);
        public ulong LastTime => Magneto.Contract.Utils.GetTimestamp(_lastTime);

        public void SetData(float level)
        {
            Level = level;
            if (level > Max) Max = level;
            if (level < Min) Min = level;
        }

        public void UpdateTime()
        {
            if (_firstTime == DateTime.MinValue) _firstTime = Magneto.Contract.Utils.GetNowTime();
            _lastTime = Magneto.Contract.Utils.GetNowTime();
        }
    }
}