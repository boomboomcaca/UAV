using System;
using System.Collections.Generic;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Core.Statistics;

public class IfmcaProcess(IAntennaController antennaController) : DataProcessBase(antennaController)
{
    private readonly Dictionary<int, IfmcaChannelInfo> _channelDic = new();
    private readonly object _lockChannels = new();
    private Dictionary<string, object>[] _channels;

    public override void OnData(List<object> data)
    {
        base.OnData(data);
        var ddc = (SDataDdc)data.Find(item => item is SDataDdc);
        if (ddc == null) return;
        IfmcaChannelInfo info = null;
        lock (_lockChannels)
        {
            if (_channelDic.TryGetValue(ddc.ChannelNumber, out var value)) info = value;
        }

        if (info == null) return;
        var spectrum = (SDataSpectrum)ddc.Data.Find(item => item is SDataSpectrum);
        if (spectrum != null
            && Math.Abs(info.Frequency - spectrum.Frequency) < 1e-9
            && Math.Abs(info.Bandwidth - spectrum.Span) < 1e-9)
        {
            info.SetData(spectrum.Data);
            if (info.MaximumSwitch)
            {
                spectrum.Maximum = new short[info.Maximum.Length];
                Array.Copy(info.Maximum, 0, spectrum.Maximum, 0, info.Maximum.Length);
            }

            if (info.MinimumSwitch)
            {
                spectrum.Minimum = new short[info.Minimum.Length];
                Array.Copy(info.Minimum, 0, spectrum.Minimum, 0, info.Minimum.Length);
            }

            if (info.MeanSwitch)
            {
                spectrum.Mean = new short[info.Mean.Length];
                Array.Copy(info.Mean, 0, spectrum.Mean, 0, info.Mean.Length);
            }
        }
    }

    public override void SetParameter(Parameter parameter)
    {
        base.SetParameter(parameter);
        if (parameter.Name == ParameterNames.DdcChannels)
        {
            var array = Array.Empty<Dictionary<string, object>>();
            if (parameter.Value is Dictionary<string, object>[] dic) array = (Dictionary<string, object>[])dic.Clone();
            lock (_lockChannels)
            {
                _channels ??= [];
                if (array.Length > 0)
                    for (var i = 0; i < array.Length; i++)
                        // TODO:待重构
                        if (i >= _channels.Length || IsChannelsChanged(_channels[i], array[i]))
                        {
                            if (!double.TryParse(array[i][ParameterNames.Frequency].ToString(), out var freq)) continue;
                            if (!double.TryParse(array[i][ParameterNames.FilterBandwidth].ToString(), out var bw))
                                continue;
                            var maximumSwitch = false;
                            var minimumSwitch = false;
                            var meanSwitch = false;
                            var noiseSwitch = false;
                            if (array[i].ContainsKey(ParameterNames.MaximumSwitch))
                                bool.TryParse(array[i][ParameterNames.MaximumSwitch].ToString(), out maximumSwitch);
                            if (array[i].ContainsKey(ParameterNames.MinimumSwitch))
                                bool.TryParse(array[i][ParameterNames.MinimumSwitch].ToString(), out minimumSwitch);
                            if (array[i].ContainsKey(ParameterNames.MeanSwitch))
                                bool.TryParse(array[i][ParameterNames.MeanSwitch].ToString(), out meanSwitch);
                            if (array[i].ContainsKey(ParameterNames.NoiseSwitch))
                                bool.TryParse(array[i][ParameterNames.NoiseSwitch].ToString(), out noiseSwitch);
                            _channelDic[i] = new IfmcaChannelInfo
                            {
                                Frequency = freq,
                                Bandwidth = bw,
                                MaximumSwitch = maximumSwitch,
                                MinimumSwitch = minimumSwitch,
                                MeanSwitch = meanSwitch,
                                NoiseSwitch = noiseSwitch
                            };
                        }
                        else if (IsChannelsSwitchChanged(_channels[i], array[i]))
                        {
                            var maximumSwitch = false;
                            var minimumSwitch = false;
                            var meanSwitch = false;
                            var noiseSwitch = false;
                            if (array[i].ContainsKey(ParameterNames.MaximumSwitch))
                                bool.TryParse(array[i][ParameterNames.MaximumSwitch].ToString(), out maximumSwitch);
                            if (array[i].ContainsKey(ParameterNames.MinimumSwitch))
                                bool.TryParse(array[i][ParameterNames.MinimumSwitch].ToString(), out minimumSwitch);
                            if (array[i].ContainsKey(ParameterNames.MeanSwitch))
                                bool.TryParse(array[i][ParameterNames.MeanSwitch].ToString(), out meanSwitch);
                            if (array[i].ContainsKey(ParameterNames.NoiseSwitch))
                                bool.TryParse(array[i][ParameterNames.NoiseSwitch].ToString(), out noiseSwitch);
                            _channelDic[i].MaximumSwitch = maximumSwitch;
                            _channelDic[i].MinimumSwitch = minimumSwitch;
                            _channelDic[i].MeanSwitch = meanSwitch;
                            _channelDic[i].NoiseSwitch = noiseSwitch;
                        }

                if (_channels.Length > array.Length)
                    for (var i = array.Length; i < _channels.Length; i++)
                        if (_channelDic.ContainsKey(i))
                            _channelDic.Remove(i);
                _channels = array;
            }
        }
    }

    private bool IsChannelsChanged(Dictionary<string, object> preChannel, Dictionary<string, object> currChannel)
    {
        foreach (var item in preChannel)
        {
            if (item.Key is ParameterNames.MaximumSwitch or ParameterNames.MinimumSwitch or ParameterNames.MeanSwitch
                or ParameterNames.NoiseSwitch or ParameterNames.UnitSelection) continue;
            if (!currChannel.ContainsKey(item.Key)) continue;
            if (!Equals(item.Value, currChannel[item.Key])) return true;
        }

        return false;
    }

    private bool IsChannelsSwitchChanged(Dictionary<string, object> preChannel, Dictionary<string, object> currChannel)
    {
        foreach (var item in preChannel)
            if (item.Key is ParameterNames.MaximumSwitch or ParameterNames.MinimumSwitch or ParameterNames.MeanSwitch
                or ParameterNames.NoiseSwitch or ParameterNames.UnitSelection)
            {
                if (!currChannel.ContainsKey(item.Key)) continue;
                if (!Equals(item.Value, currChannel[item.Key])) return true;
            }

        return false;
    }

    private class IfmcaChannelInfo
    {
        public double Frequency { get; set; }
        public double Bandwidth { get; set; }
        public bool MaximumSwitch { get; set; }
        public bool MinimumSwitch { get; set; }
        public bool UnitSwitch { get; set; }
        public bool MeanSwitch { get; set; }
        public bool NoiseSwitch { get; set; }
        public short[] Maximum { get; set; }
        public short[] Minimum { get; set; }
        public short[] Mean { get; set; }
        public short[] Noise { get; set; }
        public int Count { get; set; }

        public void SetData(short[] data)
        {
            if (data == null || data.Length == 0) return;
            if (MaximumSwitch && Maximum == null)
            {
                Maximum = new short[data.Length];
                Array.Copy(data, Maximum, data.Length);
            }

            if (MinimumSwitch && Minimum == null)
            {
                Minimum = new short[data.Length];
                Array.Copy(data, Minimum, data.Length);
            }

            if (MeanSwitch)
            {
                Count++;
                if (Mean == null)
                {
                    Mean = new short[data.Length];
                    Array.Copy(data, Mean, data.Length);
                }
            }

            if (NoiseSwitch && Noise == null)
            {
                Noise = new short[data.Length];
                Array.Copy(data, Noise, data.Length);
            }

            for (var i = 0; i < data.Length; i++)
            {
                if (MaximumSwitch && Maximum != null) Maximum[i] = Math.Max(Maximum[i], data[i]);
                if (MinimumSwitch && Minimum != null) Minimum[i] = Math.Min(Minimum[i], data[i]);
                if (MeanSwitch && Mean != null) Mean[i] = (short)((Mean[i] * (Count - 1) + data[i]) / (float)Count);
            }
        }
    }
}