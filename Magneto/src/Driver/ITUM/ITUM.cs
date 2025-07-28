using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ITUM;

public partial class Itum : DriverBase
{
    private readonly Dictionary<string, ItuStatData> _ituStat = new();
    private short _factor;
    private double _frequency;
    private bool _isParameterChanged;

    private float _level;
    //private SDataCapture _preCapture = null;
    //private SDataSpectrum _preSpectrum = null;

    public Itum(Guid functionId) : base(functionId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        SetParameter(ParameterNames.ItuSwitch, true);
        (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        // System.Threading.Tasks.Task.Run(DataProcess);
        _isParameterChanged = false;
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        _isParameterChanged = false;
        (Receiver as DeviceBase)?.Stop();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        if (name == ParameterNames.ItuSwitch) value = true;
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency)
        {
            if (AntennaController is IAntennaController antennaController
                && double.TryParse(value.ToString(), out var freq))
                antennaController.Frequency = freq;
            ClearData();
        }

        if (name == ParameterNames.SquelchThreshold && float.TryParse(value.ToString(), out var fl))
        {
            _threshold = fl;
            _overCount = 0;
            _totalCount = 0;
        }

        _isParameterChanged = true;
    }

    public override void OnData(List<object> data)
    {
        if (data.Exists(item => item is SDataLevel)) CanPause = true;
        var spectrum = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        if (spectrum != null && !Utils.IsNumberEquals(_frequency, spectrum.Frequency)
                             && AntennaController is IAntennaController antennaController)
        {
            _frequency = spectrum.Frequency;
            _factor = antennaController.GetFactor(_frequency);
            var factor = new SDataFactor
            {
                Data = new short[1]
            };
            factor.Data[0] = _factor;
            data.Add(factor);
            _prevSsp = spectrum;
        }

        if (_isParameterChanged)
        {
            _isParameterChanged = false;
            _ituStat.Clear();
        }

        var level = (SDataLevel)data.Find(item => item is SDataLevel);
        if (level != null)
        {
            _level = level.Data;
            _totalCount++;
            if (_level > _threshold) _overCount++;
            _occupancy = _overCount / (double)_totalCount * 100;
            ProcessPulse();
        }

        var itu = (SDataItu)data.Find(item => item is SDataItu);
        if (itu != null)
        {
            itu.FieldStrength = _level + _factor / 10f;
            var misc = itu.Misc ?? new Dictionary<string, object>();
            misc.TryAdd(ParameterNames.ItuLevel, _level);
            misc[ParameterNames.ItuStrength] = itu.FieldStrength;
            misc.TryAdd(ParameterNames.ItuFrequency, itu.Frequency);
            var fpd = itu.FieldStrength * itu.FieldStrength / 120d;
            misc.Add(ParameterNames.ItuFpd, fpd);
            misc.Add(ParameterNames.ItuOccupancy, _occupancy);
            misc.Add(ParameterNames.ItuPumRise, _pumRise);
            misc.Add(ParameterNames.ItuPumFall, _pumFall);
            misc.Add(ParameterNames.ItuPumWidth, _pumWidth);
            if (misc.ContainsKey(ParameterNames.ItuBeta) &&
                float.TryParse(misc[ParameterNames.ItuBeta].ToString(), out var beta))
            {
                var une = ErCalculation(_level, _prevSsp, beta);
                misc.Add(ParameterNames.ItuUne, une);
            }
            else if (misc.ContainsKey(ParameterNames.ItuXdb) &&
                     float.TryParse(misc[ParameterNames.ItuXdb].ToString(), out var xdb))
            {
                var une = ErCalculation(_level, _prevSsp, xdb);
                misc.Add(ParameterNames.ItuUne, une);
            }

            foreach (var pair in misc)
            {
                if (pair.Value == null) continue;
                var obj = pair.Value;
                var value = 0d;
                if (obj is ItuStatData s)
                    value = s.Value;
                else if (double.TryParse(obj.ToString(), out var db))
                    value = db;
                else
                    continue;
                var round = 2;
                if (pair.Key == ParameterNames.ItuFrequency) round = 6;
                UpdateItuStat(pair.Key, value, round);
            }

            misc = _ituStat.ToDictionary(p => p.Key, p => (object)p.Value.ToDictionary());
            itu.Misc = misc;
        }

        SendData(data);
    }

    /// <summary>
    ///     更新ITU统计数据
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="round">保留的小数位数</param>
    private void UpdateItuStat(string name, double value, int round)
    {
        if (Math.Abs(value - double.MinValue) < 1e-9) return;
        if (!_ituStat.ContainsKey(name))
        {
            var nu = Utils.ConvertNameAndUnit(name);
            _ituStat.Add(name, new ItuStatData(nu.Item1, nu.Item2));
        }

        var obj = _ituStat[name];
        if (obj == null)
        {
            var nu = Utils.ConvertNameAndUnit(name);
            _ituStat[name] = new ItuStatData(nu.Item1, nu.Item2);
        }

        value = Math.Round(value, round);
        _ituStat[name].Count++;
        _ituStat[name].Value = value;
        var max = _ituStat[name].Max;
        var min = _ituStat[name].Min;
        var avg = _ituStat[name].Avg;
        var rms = _ituStat[name].Rms;
        var cnt = _ituStat[name].Count;
        // max
        _ituStat[name].Max = Math.Max(value, max);
        // min
        _ituStat[name].Min = Math.Min(value, min);
        // avg
        var avgNew = (avg * (cnt - 1) + value) / cnt;
        _ituStat[name].Avg = Math.Round(avgNew, round);
        // rms
        var rmsNew = Math.Sqrt((rms * rms * (cnt - 1) + value * value) / cnt);
        _ituStat[name].Rms = Math.Round(rmsNew, round);
    }

    #region 占用度/脉冲测量计算

    private float _threshold;
    private int _overCount;
    private int _totalCount;
    private double _occupancy;
    private DateTime _preLevelTime = DateTime.Now;

    /// <summary>
    ///     开始上升时间
    /// </summary>
    private DateTime _pumStartRiseTime = DateTime.Now;

    private DateTime _pumEndRiseTime = DateTime.Now;

    /// <summary>
    ///     开始下降时间
    /// </summary>
    private DateTime _pumStartFallTime = DateTime.Now;

    private DateTime _pumEndFallTime = DateTime.Now;
    private float _preLevel;

    #endregion

    #region 无用发射

    private SDataSpectrum _prevSsp;

    /// <summary>
    ///     无用发射计算
    /// </summary>
    /// <param name="level">电平</param>
    /// <param name="data">频谱数据</param>
    /// <param name="bandWidth">ITU测量带宽(可传入Beta带宽)</param>
    private float? ErCalculation(float level, SDataSpectrum data, float bandWidth)
    {
        //计算信号百分比
        var percent = (float)(bandWidth / data.Span);
        //在频谱中有信号的点数
        var signalPointCount = (int)(data.Data.Length * percent);
        var emptyPointCount = data.Data.Length - signalPointCount;
        int signalStartIndex;
        int signalEndIndex;
        if (emptyPointCount == 0)
        {
            signalStartIndex = 0;
            signalEndIndex = data.Data.Length;
        }
        else
        {
            signalStartIndex = emptyPointCount / 2;
            signalEndIndex = signalStartIndex + signalPointCount;
        }

        double signalSumLevel = 0;
        double emptySumLevel = 0;
        for (var i = 0; i < data.Data.Length; i++)
            if (i < signalStartIndex || i > signalEndIndex)
                emptySumLevel += data.Data[i];
            else
                signalSumLevel += data.Data[i];
        if (emptySumLevel == 0) return null;
        var v = Math.Abs((float)(signalSumLevel / emptySumLevel));
        var result = (float)(level - 107 - 10 * Math.Log10(v));
        if (float.IsInfinity(result) || float.IsNaN(result)) return null;
        return result;
    }

    #endregion

    #region 脉冲测量

    private double _pumRise;
    private double _pumFall;
    private double _pumWidth;

    private void ProcessPulse()
    {
        if (_preLevel < _threshold)
            // 疑似上升
            _pumStartRiseTime = _preLevel < _level ? _preLevelTime : DateTime.Now;

        if (_preLevel > _threshold)
            // 疑似下降
            _pumStartFallTime = _preLevel > _level ? _preLevelTime : DateTime.Now;

        if (_level > _threshold && _level <= _preLevel)
        {
            // 上升完毕
            _pumEndRiseTime = _preLevelTime;
            _pumRise = _pumEndRiseTime.Subtract(_pumStartRiseTime).TotalMilliseconds;
        }

        if (_level < _threshold && _level >= _preLevel)
        {
            // 下降完毕
            _pumEndFallTime = _preLevelTime;
            _pumFall = _pumEndFallTime.Subtract(_pumStartFallTime).TotalMilliseconds;
            _pumWidth = _pumEndFallTime.Subtract(_pumEndRiseTime).TotalMilliseconds;
        }

        _preLevel = _level;
        _preLevelTime = DateTime.Now;
    }

    #endregion
}