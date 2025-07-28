using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.IFMCA;

public partial class Ifmca : DriverBase
{
    private readonly ConcurrentDictionary<int, SDataDdc> _ddcCache = new();
    private readonly List<int> _skipChannel = new();
    private short _factor;
    private double _frequency;
    private int _maxChannels;
    private int _preCount;
    private int _preIndex;
    private DateTime _preSendTime = DateTime.Now;

    public Ifmca(Guid driverId) : base(driverId)
    {
    }

    public override void Initialized(ModuleInfo module)
    {
        base.Initialized(module);
        CanPause = false;
        var parameter = module.Parameters.Find(item => item.Name == ParameterNames.DdcChannels);
        if (parameter != null)
        {
            _template = parameter.Template.ToDictionary(item => item.Name, item => item.Default);
            var fbw = parameter.Template.Find(item => item.Name == ParameterNames.FilterBandwidth);
            if (fbw?.Values?.Count > 0)
                _filterBandwidths = fbw.Values.ConvertAll(item => Convert.ToDouble(item)).ToArray();
        }

        var maxChannels = module.Parameters.Find(item => item.Name == "maxChanCount");
        if (maxChannels != null) _maxChannels = Convert.ToInt32(maxChannels.Value);
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _preSendTime = DateTime.Now;
        CanPause = false;
        (Receiver as DeviceBase)?.Start(FeatureType.IFMCA, this);
        return true;
    }

    public override bool Stop()
    {
        (Receiver as DeviceBase)?.Stop();
        if (_airMonitor != null)
        {
            Console.WriteLine("停止任务,退出航空监测模式");
            _airMonitor.Close();
            _airMonitor.SignalsChanged -= SignalsChanged;
            _airMonitor.AvicgFrequenciesChanged -= AvicgFrequenciesChanged;
            _airMonitor = null;
        }

        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency)
            if (AntennaController is IAntennaController antennaController
                && double.TryParse(value.ToString(), out var freq))
                antennaController.Frequency = freq;
        if (name == ParameterNames.DdcChannels && value is Dictionary<string, object>[] dic)
        {
            _skipChannel.Clear();
            var len = dic.Length;
            if (_preCount != len)
            {
                _preCount = len;
                if (_preIndex >= _preCount) _preIndex = 0;
            }

            for (var i = 0; i < dic.Length; i++)
                if (dic[i].ContainsKey(ParameterNames.LevelSwitch)
                    && bool.TryParse(dic[i][ParameterNames.LevelSwitch].ToString(), out var level)
                    && dic[i].ContainsKey(ParameterNames.SpectrumSwitch)
                    && bool.TryParse(dic[i][ParameterNames.SpectrumSwitch].ToString(), out var spec)
                    && dic[i].ContainsKey(ParameterNames.IfSwitch)
                    && bool.TryParse(dic[i][ParameterNames.IfSwitch].ToString(), out var ifs)
                    && ((!level && !spec) || !ifs))
                    _skipChannel.Add(i);
        }

        if (name == "autoChannelMode")
        {
            if (AutoChannelMode)
            {
                Console.WriteLine("进入航空监测模式...");
                _airMonitor = new AirMonitoring();
                _airMonitor.SignalsChanged += SignalsChanged;
                _airMonitor.AvicgFrequenciesChanged += AvicgFrequenciesChanged;
                _airMonitor.Start(Receiver as DeviceBase, AntennaController as AntennaControllerBase);
            }
            else if (_airMonitor != null)
            {
                Console.WriteLine("退出航空监测模式");
                _airMonitor.Close();
                _airMonitor.SignalsChanged -= SignalsChanged;
                _airMonitor.AvicgFrequenciesChanged -= AvicgFrequenciesChanged;
                _airMonitor = null;
            }
        }

        if (name == "maxChanCount") _maxChannels = Convert.ToInt32(value);
    }

    public override void OnData(List<object> data)
    {
        // SendData(data);
        if (data.Exists(item => item is SDataSpectrum)) CanPause = true;
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
            data.Insert(0, factor);
        }

        _airMonitor?.OnData(data);
        DataSampling1(data);
    }

    private void DataSampling1(List<object> data)
    {
        var sendData = new List<object>();
        var tmpIndex = _preIndex;
        while (_skipChannel.Contains(_preIndex))
        {
            _preIndex++;
            if (_preIndex >= _preCount) _preIndex = 0;
            if (_preIndex == tmpIndex)
                // 防止所有通道都关闭以后，在这个while循环中死循环，因此循环一轮以后就直接返回不处理了
                return;
        }

        foreach (var item in data)
        {
            if (item is not SDataDdc ddc)
            {
                sendData.Add(item);
                continue;
            }

            if (ddc.Data?.Find(p => p is SDataAudio) is SDataAudio)
                sendData.Add(ddc);
            else
                _ddcCache.AddOrUpdate(ddc.ChannelNumber, _ => ddc, (_, _) => ddc);
        }

        if (_ddcCache.TryGetValue(_preIndex, out var sd) && DateTime.Now.Subtract(_preSendTime).TotalMilliseconds > 30)
        {
            _preSendTime = DateTime.Now;
            sendData.Add(sd);
            _preIndex++;
            if (_preIndex >= _preCount) _preIndex = 0;
        }

        if (sendData.Count > 0) SendData(sendData);
    }

    /// <summary>
    ///     从集合中获取匹配的值
    /// </summary>
    /// <param name="array"></param>
    /// <param name="value"></param>
    /// <param name="isGreater">如果没有找到匹配的值，则:true:找比value大的最相近的值,false:找比value小的最相近的值</param>
    private static double FindNearestValue(double[] array, double value, bool isGreater = true)
    {
        if (array == null) return value;
        var index = Array.IndexOf(array, value);
        if (index >= 0) return value;
        Array.Sort(array);
        if (isGreater)
        {
            for (var i = 0; i < array.Length; i++)
                if (array[i] >= value)
                    return array[i];
            return array[^1];
        }

        for (var i = array.Length - 1; i >= 0; i--)
            if (array[i] <= value)
                return array[i];
        return array[0];
    }

    #region 航空监测

    private AirMonitoring _airMonitor;
    private Dictionary<string, object>[] _ddcChannels;

    /// <summary>
    ///     缓存当前下发的子通道频率
    ///     如果信号数量超过了子通道最大数量，需要将此集合中运行超过10次的点替换
    /// </summary>
    private readonly ConcurrentDictionary<double, int> _ddcFrequenciesCache = new();

    private Dictionary<string, object> _template;
    private double[] _filterBandwidths;

    #endregion

    #region 航空监测

    private DateTime _preTime = DateTime.Now;

    /// <summary>
    ///     信号提取以后需要重新下发子通道信息
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SignalsChanged(object sender, List<SignalsResult> e)
    {
        if (!AutoChannelMode) return;
        // 如果信号个数超过了子通道个数，多余的暂时不发
        // 添加一个更改标记，如果没有更改，则不重复下发，免得需要频繁调用设备
        var setSign = false;
        try
        {
            if (_ddcChannels == null || _ddcChannels.Length == 0)
            {
                var i = 0;
                _ddcFrequenciesCache.Clear();
                var dics = new List<Dictionary<string, object>>();
                foreach (var item in e)
                {
                    i++;
                    if (i > _maxChannels) Console.WriteLine($"频点：{i},{item.Frequency}超过最大DDC点数{_maxChannels},不再下发");
                    var dic = new Dictionary<string, object>(_template);
                    if (dic.ContainsKey(ParameterNames.IfSwitch)) dic[ParameterNames.IfSwitch] = true;
                    if (dic.ContainsKey(ParameterNames.AudioSwitch)) dic[ParameterNames.AudioSwitch] = true;
                    if (dic.ContainsKey(ParameterNames.SpectrumSwitch)) dic[ParameterNames.SpectrumSwitch] = true;
                    if (dic.ContainsKey(ParameterNames.LevelSwitch)) dic[ParameterNames.LevelSwitch] = true;
                    dic[ParameterNames.Frequency] = item.Frequency;
                    dic[ParameterNames.FilterBandwidth] = FindNearestValue(_filterBandwidths, item.Bandwidth);
                    Console.WriteLine($"新增频点:{i},{item.Frequency},{dic[ParameterNames.FilterBandwidth]}");
                    _ddcFrequenciesCache.AddOrUpdate(item.Frequency, _ => 1, (_, v) => v++);
                    dics.Add(dic);
                    setSign = true;
                }

                _ddcChannels = dics.ToArray();
            }
            else
            {
                var list = _ddcChannels.ToList();
                // 已经不存在的频点序号，需要替换或
                List<int> idx = new();
                // 新频点的序号，需要新增
                List<int> newFreqIdx = new();
                // 超时的频点，如果idx不够替换，则替换此频点
                // 优先替换时间最长的
                Dictionary<int, int> timeOutDics = new();
                for (var i = 0; i < list.Count; i++)
                {
                    var dic = list[i];
                    var freq = Convert.ToDouble(dic[ParameterNames.Frequency]);
                    var item = e.Find(p => Utils.IsNumberEquals(freq, p.Frequency));
                    if (item.Frequency > 0)
                    {
                        // 已有的频点直接赋值
                        var nbw = FindNearestValue(_filterBandwidths, item.Bandwidth);
                        var obw = Convert.ToDouble(dic[ParameterNames.FilterBandwidth]);
                        if (!Utils.IsNumberEquals(nbw, obw))
                        {
                            setSign = true;
                            dic[ParameterNames.FilterBandwidth] = nbw;
                            Console.WriteLine($"更新频点:{i},{item.Frequency},{nbw}");
                        }

                        if (_ddcFrequenciesCache.TryGetValue(item.Frequency, out var times) && times > 10)
                            timeOutDics.Add(i, times);
                        _ddcFrequenciesCache.AddOrUpdate(item.Frequency, _ => 1, (_, v) => v++);
                    }
                    else
                    {
                        // 频点不存在，则需要删除或替换
                        idx.Add(i);
                        _ddcFrequenciesCache.Remove(item.Frequency, out _);
                    }
                }

                // 对超时的点按照时间由大到小排序
                List<int> timeOutIdx = new();
                while (timeOutDics.Count > 0)
                {
                    var max = timeOutDics.MaxBy(item => item.Value);
                    timeOutIdx.Add(max.Key);
                    timeOutDics.Remove(max.Key);
                }

                for (var i = 0; i < e.Count; i++)
                    if (!list.Any(item =>
                            Utils.IsNumberEquals(e[i].Frequency, Convert.ToDouble(item[ParameterNames.Frequency]))))
                        newFreqIdx.Add(i);
                if (newFreqIdx.Count > 0)
                {
                    setSign = true;
                    foreach (var index in newFreqIdx)
                    {
                        var signal = e[index];
                        var nidx = -1;
                        if (idx.Count > 0)
                        {
                            nidx = idx[0];
                            idx.RemoveAt(0);
                        }
                        else if (list.Count > _maxChannels && timeOutIdx.Count > 0)
                        {
                            nidx = timeOutIdx[0];
                            timeOutIdx.RemoveAt(0);
                        }

                        if (nidx >= 0)
                        {
                            var oldFreq = list[nidx][ParameterNames.Frequency];
                            _ddcFrequenciesCache.Remove(Convert.ToDouble(oldFreq), out _);
                            list[nidx][ParameterNames.Frequency] = signal.Frequency;
                            list[nidx][ParameterNames.FilterBandwidth] =
                                FindNearestValue(_filterBandwidths, signal.Bandwidth);
                            _ddcFrequenciesCache.AddOrUpdate(signal.Frequency, _ => 1, (_, v) => v++);
                            Console.WriteLine(
                                $"替换频点:{nidx},原:{oldFreq},新:{signal.Frequency},{list[nidx][ParameterNames.FilterBandwidth]}");
                        }
                        else if (list.Count < _maxChannels)
                        {
                            // list元素数超过maxChannels以后不再新增
                            var dic = new Dictionary<string, object>(_template);
                            if (dic.ContainsKey(ParameterNames.IfSwitch)) dic[ParameterNames.IfSwitch] = true;
                            if (dic.ContainsKey(ParameterNames.AudioSwitch)) dic[ParameterNames.AudioSwitch] = true;
                            if (dic.ContainsKey(ParameterNames.SpectrumSwitch))
                                dic[ParameterNames.SpectrumSwitch] = true;
                            if (dic.ContainsKey(ParameterNames.LevelSwitch)) dic[ParameterNames.LevelSwitch] = true;
                            dic[ParameterNames.Frequency] = signal.Frequency;
                            dic[ParameterNames.FilterBandwidth] = FindNearestValue(_filterBandwidths, signal.Bandwidth);
                            Console.WriteLine($"新增频点:{signal.Frequency},{dic[ParameterNames.FilterBandwidth]}");
                            list.Add(dic);
                        }
                    }

                    if (idx.Count > 0)
                    {
                        setSign = true;
                        // 从小到大排序
                        idx.Sort();
                        // 先删除大的序号（否则序号会乱）
                        for (var i = idx.Count - 1; i >= 0; i--)
                        {
                            var index = idx[i];
                            var freq = list[index][ParameterNames.Frequency];
                            Console.WriteLine($"删除频点:{index},{freq}");
                            list.RemoveAt(index);
                        }
                    }
                }

                _ddcChannels = list.ToArray();
            }

            if (setSign)
            {
                _airMonitor.DdcFrequenciesModify(_ddcChannels);
                SetParameter(ParameterNames.DdcChannels, _ddcChannels);
            }

            if (DateTime.Now.Subtract(_preTime).TotalSeconds > 5)
            {
                // 5秒发送一次
                _preTime = DateTime.Now;
                var array = Array.Empty<double>();
                if (_ddcChannels?.Length > 0)
                    array = _ddcChannels.Select(item => Convert.ToDouble(item[ParameterNames.Frequency])).ToArray();
                var freqs = new SDataAvicgFrequencyChannels
                {
                    Total = _maxChannels,
                    Frequencies = array
                };
                SendMessageData(new List<object> { freqs });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void AvicgFrequenciesChanged(object sender, SDataAvicgFrequencies e)
    {
        SendMessageData(new List<object> { e });
    }

    #endregion
}