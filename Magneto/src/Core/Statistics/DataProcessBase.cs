using System;
using System.Collections.Generic;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Statistics;

public class DataProcessBase : IDataProcess
{
    protected readonly IAntennaController AntennaController;

    private long[] _total;
    protected double Bandwidth;

    /// <summary>
    ///     当前的数据个数
    /// </summary>
    protected int Count;

    /// <summary>
    ///     中心频率
    /// </summary>
    protected double Frequency;

    /// <summary>
    ///     最大值数据
    /// </summary>
    protected short[] Maximum;

    /// <summary>
    ///     平均值数据
    /// </summary>
    protected short[] Mean;

    /// <summary>
    ///     最小值数据
    /// </summary>
    protected short[] Minimum;

    public DataProcessBase(IAntennaController antennaController)
    {
        if (antennaController is { } controller) AntennaController = controller;
    }

    public event EventHandler<List<object>> DataProcessComplete;

    public virtual void OnData(List<object> data)
    {
        var spectrum = (SDataSpectrum)data.Find(item => item is SDataSpectrum);
        if (spectrum != null)
            if (MaximumSwitch || MinimumSwitch || MeanSwitch)
            {
                if (Math.Abs(Frequency - spectrum.Frequency) > 1e-9
                    || Math.Abs(Bandwidth - spectrum.Span) > 1e-9)
                {
                    Maximum = null;
                    Minimum = null;
                    Mean = null;
                    _total = null;
                    Count = 0;
                    Frequency = spectrum.Frequency;
                    Bandwidth = spectrum.Span;
                }

                GetRealtimeStat(spectrum.Data);
                if (MaximumSwitch)
                {
                    if (Maximum != null)
                    {
                        spectrum.Maximum = new short[Maximum.Length];
                        Array.Copy(Maximum, 0, spectrum.Maximum, 0, Maximum.Length);
                    }

                    spectrum.DataMark[0] = 1;
                }

                if (MinimumSwitch)
                {
                    if (Minimum != null)
                    {
                        spectrum.Minimum = new short[Minimum.Length];
                        Array.Copy(Minimum, 0, spectrum.Minimum, 0, Minimum.Length);
                    }

                    spectrum.DataMark[1] = 1;
                }

                if (MeanSwitch)
                {
                    if (Mean != null)
                    {
                        spectrum.Mean = new short[Mean.Length];
                        Array.Copy(Mean, 0, spectrum.Mean, 0, Mean.Length);
                    }

                    spectrum.DataMark[2] = 1;
                }
            }
    }

    public virtual void SetParameter(Parameter parameter)
    {
        if (parameter.Name == ParameterNames.MaximumSwitch
            && bool.TryParse(parameter.Value.ToString(), out var max))
            MaximumSwitch = max;
        else if (parameter.Name == ParameterNames.MinimumSwitch
                 && bool.TryParse(parameter.Value.ToString(), out var min))
            MinimumSwitch = min;
        else if (parameter.Name == ParameterNames.MeanSwitch
                 && bool.TryParse(parameter.Value.ToString(), out var mean))
            MeanSwitch = mean;
        else if (parameter.Name == ParameterNames.NoiseSwitch
                 && bool.TryParse(parameter.Value.ToString(), out var noise))
            NoiseSwitch = noise;
    }

    public virtual void Start()
    {
    }

    public virtual void Stop()
    {
    }

    protected void SendData(List<object> data)
    {
        DataProcessComplete?.Invoke(this, data);
    }

    private void GetRealtimeStat(short[] data)
    {
        if (data == null || data.Length == 0) return;
        if (MaximumSwitch && (Maximum == null || Maximum.Length != data.Length))
        {
            Maximum = new short[data.Length];
            Array.Copy(data, Maximum, data.Length);
        }

        if (MinimumSwitch && (Minimum == null || Minimum.Length != data.Length))
        {
            Minimum = new short[data.Length];
            Array.Copy(data, Minimum, data.Length);
        }

        if (MeanSwitch)
        {
            if (Mean == null || Mean.Length != data.Length)
            {
                Mean = new short[data.Length];
                _total = new long[data.Length];
                Count = 0;
                Array.Copy(data, Mean, data.Length);
            }

            if (_total == null)
            {
                Count = 0;
                _total = new long[data.Length];
            }

            Count++;
        }

        for (var i = 0; i < data.Length; i++)
        {
            if (MaximumSwitch && Maximum != null)
                // _maximum[i] = Math.Max(_maximum[i], data[i]);
                if (data[i] > Maximum[i])
                    Maximum[i] = data[i];
            if (MinimumSwitch && Minimum != null)
                // _minimum[i] = Math.Min(_minimum[i], data[i]);
                if (data[i] < Minimum[i])
                    Minimum[i] = data[i];
            if (MeanSwitch && Mean != null)
            {
                _total[i] += data[i];
                // _mean[i] = (short)(((_mean[i] * (_count - 1)) + data[i]) / (float)_count);
                Mean[i] = (short)(_total[i] / (float)Count);
            }
        }
    }

    #region 开关

    /// <summary>
    ///     最大值开关
    /// </summary>
    protected bool MaximumSwitch;

    /// <summary>
    ///     最小值开关
    /// </summary>
    protected bool MinimumSwitch;

    /// <summary>
    ///     平均值开关
    /// </summary>
    protected bool MeanSwitch;

    /// <summary>
    ///     噪声开关
    /// </summary>
    protected bool NoiseSwitch;

    #endregion
}