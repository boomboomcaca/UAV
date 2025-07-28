using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.UavDef.NSIC;

/*
    1. 数据准备阶段
        1.1 根据模板ID从云端获取参数以及模板信息
        1.2 根据参数集合中的参数[scanSegments]获取到频段信息[startFrequency MHz,stopFrequency MHz,stepFrequency kHz]
        1.3 根据频段信息获取每个频段的数据长度length=((stopFrequency-startFrequency)/stepFrequency)*1000,根据这个来确定数据的点数
        1.4 数据缓存 方法 OnData 行号113
            1.4.1 根据类型scan筛选数据
            1.4.2 根据scan数据中的segmentOffset字段获取当前的频段索引（从0开始），根据offset字段获取当前扫描数据的第一包在当前频段的位置，以此将数据中的data进行缓存
            1.4.3 判断一整包数据是否缓存完毕
        1.5 一整包数据缓存完毕以后，将缓存的数据取出并放入缓存数据的队列
    2. 数据比对阶段 方法 Process 行号 262
        2.1 数据初步处理（每个频段单独处理）
            2.1.1 计算自动门限
            2.2.2 计算最大值
            2.2.3 计算数据总和
            2.2.4 累加计数
        2.2 数据比对并合并（每个频段单独处理）
            2.2.1 判断每个频点的数据最大值与[模板最大值+阈值门限]的大小（maxLevel[i]>tmpMax[i]+thresholdValue）
            2.2.2 [2.2.1]计算的上升沿算信号开始，下降沿算信号结束，开始与结束的序号之间判断为有信号，两个序号的间隔为带宽，序号中间为中心频率
        2.3 信号更新（与之前的比对结果进行比较）
            2.2.1 如果新的信号在之前的结果中不存在则添加
            2.2.2 如果新的信号已存在则查看带宽是否变化，如果变了则更新
            2.2.3 更新平均值与最大值
            2.2.4 更新最后更新时间
    3. 信息发送
*/
public class ComparisonProcess : IDataProcess
{
    public event EventHandler<List<object>> DataProcessComplete;

    /// <summary>
    ///     缓存数据
    /// </summary>
    /// <param name="data"></param>
    public void OnData(List<object> data)
    {
        var scan = (SDataScan)data.Find(i => i is SDataScan);
        if (scan == null || scan.SegmentOffset < 0) return;
        var index = scan.Offset;
        var segmentIndex = scan.SegmentOffset;
        var len = scan.Data.Length;
        var segment = _scanSegments[scan.SegmentOffset];
        segment.AddData(scan.Data, index, len);
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

            _dataQueue.EnQueue(Array.ConvertAll(level, item => item / 10f));
        }
    }

    public void SetParameter(Parameter parameter)
    {
        switch (parameter.Name)
        {
            case ParameterNames.ScanSegments:
            {
                var dic = (Dictionary<string, object>[])parameter.Value;
                if (!(dic?.Length > 0)) return;
                _scanSegments = new ScanSegmentStatistics[dic.Length];
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
                }

                break;
            }
            case "templateID":
                _templateId = parameter.Value.ToString();
                break;
            case "thresholdValue":
                _threshold = Convert.ToInt32(parameter.Value);
                _isThresholdChanged = true;
                break;
        }
    }

    /// <summary>
    ///     启动
    /// </summary>
    public void Start()
    {
        _segCalculators = [];
        _signals = [];
        _ = Task.Run(() => StartAsync(_templateId));
    }

    public void Stop()
    {
        _isRunning = false;
        _dataQueue.Clear();
        _compareTemplates.Clear();
    }

    /// <summary>
    ///     通过模板ID获取参数信息
    /// </summary>
    /// <param name="templateId"></param>
    private async Task StartAsync(string templateId)
    {
        var cloud = await CloudClient.Instance.GetNsicTemplateDataAsync(templateId);
        if (string.IsNullOrEmpty(cloud.TemplateId)) throw new Exception("在云端未查询到模板信息");
        var parameters = cloud.Parameters;
        DataProcessComplete?.Invoke(null, [parameters]);
        var list = cloud.Data;

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
            sse.Init(item.Frequencies.Length, _threshold, item.MaxLevel);
            _segCalculators.Add(sse);
        }

        _signals.Clear();
        _preCompareTime = DateTime.Now;
        _isRunning = true;
        _thdProcess = new Thread(Process)
        {
            IsBackground = true
        };
        _thdProcess.Start();
    }

    /// <summary>
    ///     信号比对线程
    /// </summary>
    private void Process()
    {
        while (_isRunning)
        {
            if (_dataQueue.Count == 0) Thread.Sleep(1);
            var data = _dataQueue.DeQueue(100);
            if (data == null) continue;
            var startIndex = 0;
            try
            {
                // 每个频段单独处理
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
                            // 合并信号
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
                        DataProcessComplete?.Invoke(null, [signalInfos]);
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
    private static void UpdateSignalLunching(Dictionary<int, CompareSignalInfo> signals, DateTime time)
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
    ///     每个频段信号比对处理类实例
    /// </summary>
    private List<SegStatisticsForComparison> _segCalculators;

    /// <summary>
    ///     参与比对的模板信息
    /// </summary>
    private readonly List<TemplateInfo> _compareTemplates = [];

    /// <summary>
    ///     正常信号列表 Key： 频点索引， Value:信号集合  List存储表示每个段单独一个Dictionary
    /// </summary>
    private List<Dictionary<int, CompareSignalInfo>> _signals;

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
    private Thread _thdProcess;
    private readonly MQueue<float[]> _dataQueue = new();

    /// <summary>
    ///     标记门限阈值发生了变化，需要重新比对信号，原来的信号需要清理
    /// </summary>
    private bool _isThresholdChanged;

    #endregion

    #region 内部类

    /// <summary>
    ///     单段比对统计
    /// </summary>
    private class SegStatisticsForComparison(double startFreq, double stopFreq, double stepFreq)
    {
        private readonly object _lockSignals = new();
        private readonly List<CompareWithCombineResult> _signals = [];
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
        ///     模板最大值数据
        /// </summary>
        private float[] _tmpMax;

        /// <summary>
        ///     整个过程电平和
        /// </summary>
        private float[] _totalLevel;

        /// <summary>
        ///     小信号统计   电平>模板均值 && 电平 模板最大值 + 阈值门限
        /// </summary>
        private int[] _weakSignal;

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="pointCount"></param>
        /// <param name="thrVal">阈值门限</param>
        /// <param name="tmpMax">阈值谱</param>
        internal void Init(int pointCount, int thrVal, float[] tmpMax)
        {
            _thrVal = thrVal;
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

        private void ClearData()
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
                    var freq = startFreq + i * stepFreq / 1000;
                    signals.Add(new CompareResult
                    {
                        FreqIndex = i,
                        Freq = freq,
                        Band = stepFreq,
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
                    si.Band = (signalEnd - signalStart + 1) * stepFreq;
                    var freqIndex = signalStart + (signalEnd - signalStart + 1) / 2;
                    si.FreqIndex = freqIndex;
                    si.Freq = startFreq + freqIndex * stepFreq / 1000;
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
        private static bool ContainsSignal(List<CompareWithCombineResult> signals, CompareWithCombineResult signal,
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

        /// <summary>
        ///     比对信号并合并
        /// </summary>
        /// <param name="thrVal"></param>
        /// <param name="reset"></param>
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

        #region 信号提取相关

        private readonly int _totalCount = Utils.GetTotalCount(startFreq, stopFreq, stepFreq);

        #endregion 信号提取相关
    }

    private class CompareResult
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

    private class CompareWithCombineResult : CompareResult
    {
        public bool IsLunching;
    }

    #endregion
}