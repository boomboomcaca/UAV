using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.ESE;

public class ComparisonProcess : IDataProcess
{
    public event EventHandler<List<object>> DataProcessComplate;

    public void OnData(List<object> data)
    {
        var scan = (SDataScan)data.Find(i => i is SDataScan);
        if (scan == null || scan.SegmentOffset < 0) return;
        var index = scan.Offset;
        var segmentIndex = scan.SegmentOffset;
        var len = scan.Data.Length;
        var segment = _scanSegments[scan.SegmentOffset];
        segment.AddData(scan.Data, index, len);
        if (!_isRunning) return;
        if (segmentIndex == _scanSegments.Length - 1
            && index + len == segment.Total)
        {
            // 完整的扫描完所有频段了
            var count = _scanSegments.Sum(i => i.Total);
            var level = new short[count];
            var offset = 0;
            foreach (var t in _scanSegments)
            {
                var scanLen = t.Total;
                Array.Copy(t.Data, 0, level, offset, scanLen);
                offset += scanLen;
            }

            _dataQueue.Enqueue(Array.ConvertAll(level, item => item / 10f));
        }
    }

    public void SetParameter(Parameter parameter)
    {
        if (parameter.Name == ParameterNames.ScanSegments)
        {
            var dic = (Dictionary<string, object>[])parameter.Value;
            if (dic?.Length > 0)
            {
                _scanSegments = new ScanSegmentStatistics[dic.Length];
                var total = 0;
                for (var i = 0; i < _scanSegments.Length; i++)
                {
                    var startFreq = Convert.ToDouble(dic[i][ParameterNames.StartFrequency]);
                    var stopFreq = Convert.ToDouble(dic[i][ParameterNames.StopFrequency]);
                    var stepFreq = Convert.ToDouble(dic[i][ParameterNames.StepFrequency]);
                    _scanSegments[i] = new ScanSegmentStatistics(i, startFreq, stopFreq, stepFreq)
                    {
                        ScanIndex = 0,
                        Offset = 0
                    };
                    total += _scanSegments[i].Total;
                }

                _dataQueue.Clear();
                _overThrStats = new long[total];
            }
        }
        else if (parameter.Name == "templateID")
        {
            _templateId = parameter.Value.ToString();
        }
        else if (parameter.Name == "thresholdValue")
        {
            _threshold = Convert.ToInt32(parameter.Value);
            _isThresholdChanged = true;
        }
    }

    public void Start()
    {
        _segCalculators = new List<SegStatisticsForComparison>();
        _signals = new List<Dictionary<int, CompareSignalInfo>>();
        _weakSignals = new List<Dictionary<int, CompareSignalInfo>>();
        _ = Task.Run(() => StartAsync(_templateId));
    }

    public void Stop()
    {
        _isRunning = false;
        _dataQueue.Clear();
        _compareTemplates.Clear();
        _cts?.Cancel();
        try
        {
            _processTask?.Dispose();
        }
        catch
        {
        }
        finally
        {
            _cts?.Dispose();
        }
    }

    private async Task StartAsync(string templateId)
    {
        var cloud = await CloudClient.Instance.GetEseTemplateDataAsync(templateId);
        if (string.IsNullOrEmpty(cloud.TemplateId)) throw new Exception("在云端未查询到模板信息");
        var parameters = cloud.Parameters;
        DataProcessComplate?.Invoke(null, new List<object> { parameters });
        var list = cloud.Data;
        var templateInfos = new List<TemplateInfo>();
        foreach (var tmp in list)
        {
            var data = new TemplateInfo
            {
                StartFrequency = tmp.StartFrequency,
                StopFrequency = tmp.StopFrequency,
                StepFrequency = tmp.StepFrequency,
                MaxLevel = tmp.Maximum,
                AveLevel = tmp.Average,
                Threshold = tmp.Threshold,
                Signals = tmp.Signals
            };
            data.Frequencies = new double[data.Signals.Length];
            for (var i = 0; i < data.Frequencies.Length; i++)
                data.Frequencies[i] = data.StartFrequency + i * data.StepFrequency / 1000;
            _compareTemplates.Add(data);
        }

        foreach (var item in _compareTemplates)
        {
            var startFreq = item.StartFrequency;
            var stopFreq = item.StopFrequency;
            var step = item.StepFrequency;
            var sse = new SegStatisticsForComparison(startFreq, stopFreq, step);
            // float[] tmpThr = item.Item3.MaxLevel.Select(l => l + ThresholdValue).ToArray();
            sse.Init(item.Frequencies.Length, _threshold, item.AveLevel, item.MaxLevel);
            _segCalculators.Add(sse);
        }

        _signals.Clear();
        _weakSignals.Clear();
        _preCompareTime = DateTime.Now;
        _preGetWeakTime = DateTime.Now;
        _isRunning = true;
        _cts = new CancellationTokenSource();
        _processTask = new Task(p => ProcessAsync(p).ConfigureAwait(false), _cts.Token);
        _processTask.Start();
    }

    private async Task ProcessAsync(object obj)
    {
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(1, token);
            if (_dataQueue.IsEmpty) continue;
            if (!_dataQueue.TryDequeue(out var data)) continue;
            var startIndex = 0;
            try
            {
                for (var i = 0; i < _scanSegments.Length; i++)
                {
                    var len = _scanSegments[i].Total;
                    var spec = new float[len];
                    Array.Copy(data, startIndex, spec, 0, len);
                    _segCalculators[i].Statistics(spec);
                    startIndex += len;
                }
            }
            catch
            {
                // ignored
            }

            if (_isThresholdChanged)
            {
                _isThresholdChanged = false;
                _dataQueue.Clear();
                _signals.Clear();
                _segCalculators?.ForEach(i => i.SetThreshold(_threshold));
                _preCompareTime = DateTime.Now;
                continue;
            }

            if (_compareTimeGap > 10)
                if ((DateTime.Now - _preCompareTime).TotalMilliseconds >= _compareTimeGap)
                    try
                    {
                        // 进行比较
                        if (_combineSignal)
                            try
                            {
                                CompareSignalWidthCombine();
                            }
                            catch
                            {
                                // 信号数量过多 超过10000个，不再处理，没想好怎么把这异常丢出去
                            }
                        else
                            CompareSignal();

                        // 拷贝集合，触发事件
                        var signalInfos = new List<IList<CompareSignalInfo>>();
                        foreach (var item in _signals)
                        {
                            IList<CompareSignalInfo> s = item.Values.ToList()
                                .ConvertAll(i => (CompareSignalInfo)i.Clone());
                            signalInfos.Add(s);
                        }

                        // 触发事件
                        DataProcessComplate?.Invoke(null, new List<object> { signalInfos });
                        _preCompareTime = DateTime.Now;
                    }
                    catch
                    {
                        // ignored
                    }
        }
    }

    private void CompareSignal()
    {
        var updateTime = Utils.GetNowTime();
        for (var i = 0; i < _segCalculators.Count; i++)
        {
            var signals = _segCalculators[i].Compare(_threshold);
            var preSignals = new Dictionary<int, CompareSignalInfo>();
            if (_signals.Count > i)
                preSignals = _signals[i];
            else
                _signals.Add(preSignals);
            // 比较
            foreach (var t in signals)
            {
                var freqIndex = t.FreqIndex;
                var freq = _compareTemplates[i].Frequencies[freqIndex];
                var csi = new CompareSignalInfo();
                if (!preSignals.ContainsKey(freqIndex))
                {
                    csi.FirstCaptureTime = updateTime;
                    preSignals.Add(freqIndex, csi);
                    csi.Frequency = freq;
                }

                csi = preSignals[freqIndex];
                csi.Bandwidth = t.Band;
                csi.MaxLevel = t.MaxLevel;
                csi.AveLevel = t.AvgLevel;
                csi.LastCaptureTime = updateTime;
                csi.IsLunching = true;
                // 可疑信号
                csi.CompareResult = _compareTemplates[i].Signals[freqIndex] > 0
                    ? "可疑信号"
                    :
                    // 新信号
                    "新增信号";
            }

            // 设置未再出现的信号
            UpdateSignalLunching(preSignals, updateTime);
        }
    }

    private void CompareSignalWidthCombine()
    {
        var updateTime = Utils.GetNowTime().ToUniversalTime();
        for (var i = 0; i < _segCalculators.Count; i++)
        {
            var signals = _segCalculators[i].CompareWidthCombineNew(_threshold, true);
            var preSignals = new Dictionary<int, CompareSignalInfo>();
            if (_signals.Count > i)
                preSignals = _signals[i];
            else
                _signals.Add(preSignals);
            // 比较
            foreach (var t in signals)
            {
                var freqIndex = t.FreqIndex;
                var freq = _compareTemplates[i].Frequencies[freqIndex];
                var csi = new CompareSignalInfo();
                if (!preSignals.ContainsKey(freqIndex))
                {
                    csi.FirstCaptureTime = updateTime;
                    preSignals.Add(freqIndex, csi);
                    csi.Frequency = freq;
                }

                csi = preSignals[freqIndex];
                csi.Bandwidth = t.Band;
                csi.MaxLevel = t.MaxLevel;
                csi.AveLevel = t.AvgLevel;
                csi.IsLunching = t.IsLunching;
                if (csi.IsLunching) csi.LastCaptureTime = updateTime;
                // 可疑信号
                csi.CompareResult = _compareTemplates[i].Signals[freqIndex] > 0
                    ? "可疑信号"
                    :
                    // 新信号
                    "新增信号";
            }

            _signals[i] = preSignals;
        }
    }

    /// <summary>
    ///     更新未再出现的信号（比较之前的信号，未再出现的信号）
    /// </summary>
    /// <param name="signals"></param>
    /// <param name="time"></param>
    protected void UpdateSignalLunching(Dictionary<int, CompareSignalInfo> signals, DateTime time)
    {
        for (var j = 0; j < signals.Count; j++)
        {
            var item = signals.ElementAt(j);
            if (item.Value.LastCaptureTime != time) item.Value.IsLunching = false;
        }
    }

    #region 属性和字段

    /// <summary>
    ///     上一次比对时间
    /// </summary>
    private DateTime _preCompareTime = DateTime.MinValue;

    /// <summary>
    ///     上一次获取小信号的时间
    /// </summary>
    private DateTime _preGetWeakTime = DateTime.MinValue;

    /// <summary>
    ///     每个频段信号比对处理类实例
    /// </summary>
    private List<SegStatisticsForComparison> _segCalculators;

    /// <summary>
    ///     参与比对的模板信息
    /// </summary>
    private readonly List<TemplateInfo> _compareTemplates = new();

    /// <summary>
    ///     正常信号列表 <Key： 频点索引， Value:信号集合>  List存储表示每个段单独一个Dictionary
    /// </summary>
    private List<Dictionary<int, CompareSignalInfo>> _signals;

    /// <summary>
    ///     小信号列表 <Key： 频点索引， Value:信号集合>  List存储表示每个段单独一个Dictionary
    /// </summary>
    private List<Dictionary<int, CompareSignalInfo>> _weakSignals;

    private ScanSegmentStatistics[] _scanSegments;

    /// <summary>
    ///     是否进行信号合并
    /// </summary>
    private readonly bool _combineSignal = true;

    /// <summary>
    ///     比对提取间隔ms
    ///     每隔此时间取最大电平值进行一次比对计算
    /// </summary>
    private readonly int _compareTimeGap = 2000;

    private string _templateId;

    /// <summary>
    ///     获取或设置阈值门限
    /// </summary>
    private int _threshold = 10;

    private bool _isRunning;
    private CancellationTokenSource _cts;
    private Task _processTask;
    private readonly ConcurrentQueue<float[]> _dataQueue = new();
    private long[] _overThrStats;

    /// <summary>
    ///     标记门限阈值发生了变化，需要重新比对信号，原来的信号需要清理
    /// </summary>
    private bool _isThresholdChanged;

    #endregion

    #region 内部类

    /// <summary>
    ///     单段比对统计
    /// </summary>
    protected class SegStatisticsForComparison
    {
        private readonly object _lockSignals = new();
        private readonly List<CompareWithCombineResult> _signals = new();
        private AutoThreshold _at;

        /// <summary>
        ///     数据帧数统计
        /// </summary>
        private float _dataTimes;

        private float[] _maxLevel;

        /// <summary>
        ///     每个 _extractTimeGap 内最大值统计，用于计算占用度提取信号
        /// </summary>
        private float[] _maxLevelOfCompare;

        /// <summary>
        ///     超过门限的帧数统计
        /// </summary>
        private int[] _overTimes;

        /// <summary>
        ///     阈值门限
        /// </summary>
        private int _thrVal;

        /// <summary>
        ///     用于计算小信号的统计次数
        /// </summary>
        private float _timesForWeakStats;

        /// <summary>
        ///     模板平均值数据
        /// </summary>
        private float[] _tmpAve;

        /// <summary>
        ///     模板最大值数据
        /// </summary>
        private float[] _tmpMax;

        /// <summary>
        ///     整个过程电平和
        /// </summary>
        private float[] _totalLevel;

        /// <summary>
        ///     小信号统计   电平>模板均值 && 电平< 模板最大值 + 阈值门限
        /// </summary>
        private int[] _weakSignal;

        public SegStatisticsForComparison(double startFreq, double stopFreq, double stepFreq)
        {
            _startFreq = startFreq;
            _stopFreq = stopFreq;
            _stepFreq = stepFreq;
            _totalCount = Utils.GetTotalCount(startFreq, stopFreq, stepFreq);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="pointCount"></param>
        /// <param name="thrVal">阈值门限</param>
        /// <param name="tmpAve"></param>
        /// <param name="tmpMax">阈值谱</param>
        internal void Init(int pointCount, int thrVal, float[] tmpAve, float[] tmpMax)
        {
            _thrVal = thrVal;
            _tmpAve = tmpAve;
            _tmpMax = tmpMax;
            _dataTimes = 0;
            _weakSignal = new int[pointCount];
            _overTimes = new int[pointCount];
            _at = new AutoThreshold();
            _totalLevel = new float[pointCount];
            _maxLevel = Enumerable.Repeat(-9999f, pointCount).ToArray();
            _maxLevelOfCompare = new float[pointCount];
            Array.Copy(_maxLevel, _maxLevelOfCompare, pointCount);
        }

        internal void SetThreshold(int thrVal)
        {
            _thrVal = thrVal;
            ClearData();
        }

        internal void ClearData()
        {
            lock (_lockSignals)
            {
                _signals.Clear();
                // 重置比对最大值统计缓存
                _maxLevelOfCompare = Enumerable.Repeat(-9999f, _maxLevelOfCompare.Length).ToArray();
            }
        }

        internal void Statistics(float[] spectrum)
        {
            var autoThreshold = _at.GetThreshold(spectrum);
            for (var i = 0; i < spectrum.Length; i++)
            {
                var val = spectrum[i];
                var tmpMax = _tmpMax[i];
                var tmpThr = tmpMax + _thrVal;
                _totalLevel[i] += val;
                _maxLevel[i] = Math.Max(_maxLevel[i], val);
                _maxLevelOfCompare[i] = Math.Max(_maxLevelOfCompare[i], val);
                if (val > tmpMax && val < tmpThr) _weakSignal[i]++;
                if (spectrum[i] > autoThreshold[i]) _overTimes[i]++;
            }

            _dataTimes++;
            _timesForWeakStats++;
        }

        /// <summary>
        ///     执行比对
        ///     频点索引，电平最大值，均值电平
        /// </summary>
        /// <param name="thrVal"></param>
        /// <returns>频点索引，中心频率MHz，带宽kHz，电平最大值，均值电平</returns>
        internal List<CompareResult> Compare(int thrVal)
        {
            var signals = new List<CompareResult>();
            for (var i = 0; i < _maxLevelOfCompare.Length; i++)
                if (_maxLevelOfCompare[i] >= _tmpMax[i] + thrVal)
                {
                    var ave = _totalLevel[i] / _dataTimes;
                    var freq = _startFreq + i * _stepFreq / 1000;
                    signals.Add(new CompareResult
                    {
                        FreqIndex = i,
                        Freq = freq,
                        Band = _stepFreq,
                        MaxLevel = _maxLevel[i],
                        AvgLevel = ave
                    });
                }

            // 重置比对最大值统计缓存
            _maxLevelOfCompare = Enumerable.Repeat(-9999f, _maxLevelOfCompare.Length).ToArray();
            return signals;
        }

        private List<CompareWithCombineResult> ExtractSignals(float[] maxLevels)
        {
            var signals = new List<CompareWithCombineResult>();
            // 信号合并时使用
            var signalStart = -1;
            var signalEnd = -1;
            CompareWithCombineResult si = null;
            for (var i = 0; i < _totalCount; i++)
                if (maxLevels[i] > _tmpMax[i] + _thrVal)
                {
                    if (signalStart == -1)
                    {
                        si = new CompareWithCombineResult();
                        signalStart = i;
                    }

                    signalEnd = i;
                }
                else if (signalStart > -1)
                {
                    if (si == null) continue;
                    si.Band = (signalEnd - signalStart + 1) * _stepFreq;
                    var freqIndex = signalStart + (signalEnd - signalStart + 1) / 2;
                    si.FreqIndex = freqIndex;
                    si.Freq = _startFreq + freqIndex * _stepFreq / 1000;
                    var ave = _totalLevel[freqIndex] / _dataTimes;
                    si.AvgLevel = (float)Math.Round(ave, 1);
                    si.MaxLevel = _maxLevel[freqIndex];
                    signals.Add(si);
                    signalStart = -1;
                    signalEnd = -1;
                }

            return signals;
        }

        /// <summary>
        ///     更新信号
        /// </summary>
        /// <param name="signals"></param>
        private void UnionSignals(List<CompareWithCombineResult> signals)
        {
            foreach (var s in signals)
                if (!ContainsSignal(_signals, s, true))
                {
                    s.IsLunching = true;
                    _signals.Add(s);
                }

            foreach (var s in _signals)
                if (!ContainsSignal(signals, s, false))
                    s.IsLunching = false;
        }

        /// <summary>
        ///     已监测到的信号是否包含当前信号
        /// </summary>
        /// <param name="signals"></param>
        /// <param name="signal"></param>
        /// <param name="updateSign">是否更新集合数据</param>
        private bool ContainsSignal(List<CompareWithCombineResult> signals, CompareWithCombineResult signal,
            bool updateSign)
        {
            var result = -1;
            foreach (var s in signals)
            {
                var gap = s.Band / 2000;
                if (Math.Abs(signal.Freq - s.Freq) < gap)
                {
                    result = 0;
                    signal.AvgLevel = s.AvgLevel;
                    signal.MaxLevel = Math.Max(signal.MaxLevel, s.MaxLevel);
                    var stop1 = s.Freq + gap;
                    var stop2 = signal.Freq + signal.Band / 2;
                    if (updateSign)
                    {
                        s.IsLunching = true;
                        if (stop1 > stop2) s.Band = signal.Band;
                    }

                    break;
                }
            }

            return result > -1;
        }

        internal List<CompareWithCombineResult> CompareWidthCombineNew(int thrVal, bool reset)
        {
            var avgLevel = new float[_totalLevel.Length];
            var thr = new double[_totalLevel.Length];
            for (var i = 0; i < avgLevel.Length; i++)
            {
                avgLevel[i] = _totalLevel[i] / _dataTimes;
                thr[i] = _tmpMax[i] + thrVal;
            }

            var maxLevel = new float[_maxLevelOfCompare.Length];
            Array.Copy(_maxLevelOfCompare, maxLevel, _maxLevelOfCompare.Length);
            if (reset)
                // 重置比对最大值统计缓存
                _maxLevelOfCompare = Enumerable.Repeat(-9999f, _maxLevelOfCompare.Length).ToArray();
            var segSignals = ExtractSignals(maxLevel);
            lock (_lockSignals)
            {
                UnionSignals(segSignals);
            }

            return _signals;
        }

        // /// <summary>
        // /// 调用算法执行比对
        // /// </summary>
        // /// <param name="thrVal">阈值 dBμV</param>
        // /// <param name="reset">是否重置信号提取，重新开始下一次提取</param>
        // /// <returns>频点索引，中心频率MHz，带宽kHz，电平最大值，均值电平</returns>
        // internal List<CompareWithCombineResult> CompareWidthCombine(int thrVal, bool reset)
        // {
        //     var avgLevel = new float[_totalLevel.Length];
        //     var thr = new double[_totalLevel.Length];
        //     for (int i = 0; i < avgLevel.Length; i++)
        //     {
        //         avgLevel[i] = _totalLevel[i] / _dataTimes;
        //         thr[i] = _tmpMax[i] + thrVal;
        //     }
        //     var maxLevel = new double[_maxLevelOfCompare.Length];
        //     Array.Copy(_maxLevelOfCompare, maxLevel, _maxLevelOfCompare.Length);
        //     if (reset)
        //     {
        //         // 重置比对最大值统计缓存
        //         _maxLevelOfCompare = Enumerable.Repeat<float>(-9999f, _maxLevelOfCompare.Length).ToArray();
        //     }
        //     int signalCount = 0;
        //     var signalId = new int[10000];
        //     var isLunching = new int[10000];
        //     var signalInfo = new double[30000];
        //     SignalDetAndFusionBasedonENT_IsInHisotry(_startFreq, _stopFreq, _stepFreq / 1000, maxLevel.Length, maxLevel, thr, _preSignalCount, _preSignalId, _preSignalInfo, ref signalCount, signalId, isLunching, signalInfo);
        //     _preSignalCount = signalCount;
        //     var signals = new List<CompareWithCombineResult>();
        //     if (signalCount > 0)
        //     {
        //         if (signalCount > 10000)
        //         {
        //             throw new Exception("信号数量超过10000请重新设置参数");
        //         }
        //         int sCount = signalCount <= 100 ? signalCount : 100;
        //         for (int i = 0; i < sCount; i++)
        //         {
        //             double freq = signalInfo[i * 3];
        //             double band = Math.Round(signalInfo[(i * 3) + 1] * 1000, 1);
        //             var freqIndex = (int)Math.Round((freq - _startFreq) * 1000 / _stepFreq);
        //             signals.Add(new CompareWithCombineResult()
        //             {
        //                 FreqIndex = freqIndex,
        //                 Freq = freq,
        //                 Band = band,
        //                 MaxLevel = (float)_maxLevel[freqIndex],
        //                 AvgLevel = avgLevel[freqIndex],
        //                 IsLunching = isLunching[i] == 0
        //             });
        //         }
        //         _preSignalId = new int[_preSignalCount];
        //         Array.Copy(signalId, _preSignalId, signalCount);
        //         _preSignalInfo = new double[signalCount * 3];
        //         Array.Copy(signalInfo, _preSignalInfo, signalCount * 3);
        //     }
        //     return signals;
        // }
        /// <summary>
        ///     获取统计值信息
        /// </summary>
        /// <param name="maxLevel">最大值</param>
        /// <param name="avgLevel">均值</param>
        /// <param name="thr">门限</param>
        /// <param name="thrVal">阈值</param>
        /// <param name="reset">是否重新开始统计</param>
        internal void GetStatsData(ref double[] maxLevel, ref double[] avgLevel, ref double[] thr, int thrVal,
            bool reset)
        {
            avgLevel = new double[_totalLevel.Length];
            thr = new double[_totalLevel.Length];
            for (var i = 0; i < avgLevel.Length; i++)
            {
                avgLevel[i] = _totalLevel[i] / _dataTimes;
                thr[i] = _tmpMax[i] + thrVal;
            }

            maxLevel = new double[_maxLevelOfCompare.Length];
            Array.Copy(_maxLevelOfCompare, maxLevel, _maxLevelOfCompare.Length);
            if (reset)
                // 重置比对最大值统计缓存
                _maxLevelOfCompare = Enumerable.Repeat(-9999f, _maxLevelOfCompare.Length).ToArray();
        }

        /// <summary>
        ///     获取小信号
        /// </summary>
        /// <param name="occThr"></param>
        /// <returns>频点索引，中心频率MHz，带宽kHz，电平最大值，均值电平</returns>
        internal List<Tuple<int, float, float>> GetWeakSignal(float occThr = 0.75f)
        {
            var weakSingal = new List<Tuple<int, float, float>>();
            for (var i = 0; i < _weakSignal.Length; i++)
            {
                var occ = _weakSignal[i] / _timesForWeakStats;
                if (occ > occThr)
                {
                    var ave = _totalLevel[i] / _dataTimes;
                    weakSingal.Add(new Tuple<int, float, float>(i, _maxLevel[i], ave));
                }
            }

            // 重置小信号计数器
            _weakSignal = new int[_weakSignal.Length];
            // 重置小信号统计计数次数
            _timesForWeakStats = 0;
            return weakSingal;
        }

        internal List<CompareResult> GetWeakSignalWidthCombine(float occThr = 0.75f)
        {
            var weakSignals = new List<CompareResult>();
            for (var i = 0; i < _weakSignal.Length; i++)
            {
                var occ = _weakSignal[i] / _timesForWeakStats;
                if (occ > occThr)
                {
                    var ave = _totalLevel[i] / _dataTimes;
                    var freq = _startFreq + i * _stepFreq / 1000;
                    weakSignals.Add(new CompareResult
                    {
                        FreqIndex = i,
                        Freq = freq,
                        Band = _stepFreq,
                        MaxLevel = _maxLevel[i],
                        AvgLevel = ave
                    });
                }
            }

            // 重置小信号计数器
            _weakSignal = new int[_weakSignal.Length];
            // 重置小信号统计计数次数
            _timesForWeakStats = 0;
            return weakSignals;
        }

        #region 信号提取相关

        /// <summary>
        ///     基于扫描数据的信号提取，带合并（包括频域和时域）
        ///     此算法由监测系统部提供
        /// </summary>
        /// <param name="startFreq">起始频率MHz 算法内部未限制</param>
        /// <param name="stopFreq">结束频率MHz 算法内部未限制</param>
        /// <param name="stepFreq">扫描步进MHz</param>
        /// <param name="pointCount">点数</param>
        /// <param name="spectrum">频谱数组</param>
        /// <param name="thr">门限数组 外部需要事先+容差</param>
        /// <param name="preSignalCount">上一次提取的信号数量 用于时域融合</param>
        /// <param name="preSingalID">上一次提取的信号ID 用于时域融合</param>
        /// <param name="preSignalInfo">上一次提取的信号信息，[fc1, bw1, dbuv1, fc2, bw2, dbuv2,…] 频率和带宽 用于时域融合</param>
        /// <param name="signalCount">本次提取的信号数量  时域上为增量输出</param>
        /// <param name="singalID">本次提取的信号ID  时域上为增量输出</param>
        /// <param name="isLunching"></param>
        /// <param name="signalInfo">本次提取的信号信息  [fc1, bw1, dbuv1, fc2, bw2, dbuv2,…] 时域上为增量输出</param>
        // [DllImport("signaldetandfusionbasedonent.dll", EntryPoint = "SignalDetAndFusionBasedonENT_IsInHisotry", CallingConvention = CallingConvention.StdCall)]
        // private static extern void SignalDetAndFusionBasedonENT_IsInHisotry(double startFreq, double stopFreq, double stepFreq, int pointCount,
        //  double[] spectrum, double[] thr, int preSignalCount, int[] preSingalID, double[] preSignalInfo,
        //  ref int signalCount, int[] singalID, int[] isLunching, double[] signalInfo);
        // private int _preSignalCount = 0;
        // private int[] _preSignalId = new int[0];
        // private double[] _preSignalInfo = new double[0];
        private readonly double _startFreq;

        private readonly double _stopFreq;
        private readonly double _stepFreq;
        private readonly int _totalCount;

        #endregion 信号提取相关
    }

    internal class CompareResult
    {
        /// <summary>
        ///     均值电平
        /// </summary>
        public float AvgLevel = -9999;

        /// <summary>
        ///     带宽kHz
        /// </summary>
        public double Band;

        /// <summary>
        ///     中心频率MHz
        /// </summary>
        public double Freq;

        /// <summary>
        ///     频点索引
        /// </summary>
        public int FreqIndex;

        /// <summary>
        ///     电平最大值
        /// </summary>
        public float MaxLevel = -9999;
    }

    internal class CompareWithCombineResult : CompareResult
    {
        public bool IsLunching;
    }

    #endregion
}