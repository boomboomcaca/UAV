using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace NSIC;

public class TemplateProcess : IDataProcess
{
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
            for (var i = 0; i < _scanSegments.Length; i++)
            {
                var scanLen = _scanSegments[i].Total;
                Array.Copy(_scanSegments[i].Data, 0, level, offset, scanLen);
                offset += scanLen;
            }

            _dataQueue.EnQueue(Array.ConvertAll(level, item => item / 10f));
        }
    }

    public void SetParameter(Parameter parameter)
    {
        if (parameter.Name == ParameterNames.ScanSegments)
        {
            var dic = (Dictionary<string, object>[])parameter.Value;
            if (dic?.Length > 0)
                lock (_lockSegmentList)
                {
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

                    _segCalculators.Clear();
                    foreach (var item in _scanSegments)
                    {
                        var sse = new SingleSegStatistics();
                        sse.Init(item.Total, _isAutoThreshold, _threshold);
                        _segCalculators.Add(sse);
                    }

                    _dataQueue.Clear();
                    _totalCount = 0;
                }
        }
        else if (parameter.Name == "autoThreshold")
        {
            _isAutoThreshold = Convert.ToBoolean(parameter.Value);
        }
        else if (parameter.Name == "thresholdValue")
        {
            _threshold = Convert.ToSingle(parameter.Value);
        }
    }

    public void Start()
    {
        _totalCount = 0;
        _segCalculators.Clear();
        lock (_lockSegmentList)
        {
            foreach (var item in _scanSegments)
            {
                var sse = new SingleSegStatistics();
                sse.Init(item.Total, _isAutoThreshold, _threshold);
                _segCalculators.Add(sse);
            }
        }

        _isRunning = true;
        _thdProcess = new Thread(Process)
        {
            IsBackground = true
        };
        _thdProcess.Start();
    }

    public void Stop()
    {
        _isRunning = false;
        _dataQueue.Clear();
        // 最大值  均值  门限  最大值-门限
        var segTmpInfo = new List<Tuple<float[], float[], float[], float[]>>();
        for (var i = 0; i < _segCalculators.Count; i++)
        {
            if (ExtractTimeGap > 10) _segCalculators[i].Extract();
            segTmpInfo.Add(_segCalculators[i].GetStatsInfo());
        }

        if (segTmpInfo.Count > 0)
        {
            var data = new List<object>
            {
                segTmpInfo
            };
            DataProcessComplete?.Invoke(null, data);
        }
    }

    private void Process()
    {
        while (_isRunning)
        {
            if (_dataQueue.Count == 0) Thread.Sleep(1);
            try
            {
                var data = _dataQueue.DeQueue(100);
                if (data == null) continue;
                var startIndex = 0;
                lock (_lockSegmentList)
                {
                    var sign = true;
                    for (var i = 0; i < _scanSegments.Length; i++)
                    {
                        var len = _scanSegments[i].Total;
                        var spec = new float[len];
                        Array.Copy(data, startIndex, spec, 0, len);
                        if (!_segCalculators[i].Statistics(spec)) sign = false;
                        startIndex += len;
                    }

                    if (!sign) continue;
                    _totalCount++;
                    if (_totalCount > 10 && ExtractTimeGap > 10)
                        if ((DateTime.Now - _preExtractTime).TotalMilliseconds >= ExtractTimeGap)
                        {
                            // 进行计算，提取信号
                            for (var i = 0; i < _segCalculators.Count; i++) _segCalculators[i].Extract();
                            _preExtractTime = DateTime.Now;
                        }
                }
            }
            catch
            {
                //////////////
            }
        }
    }

    /// <summary>
    ///     内部类
    ///     单个频段统计处理类
    /// </summary>
    private class SingleSegStatistics
    {
        private AutoThreshold _at;
        private bool _autoThreshold;

        /// <summary>
        ///     数据帧数统计
        /// </summary>
        private float _dataTimes;

        /// <summary>
        ///     UInt32.MaxValue = 4294967295 ≈ 每秒5000帧*3600秒*24小时*10天
        ///     UInt64.MaxValue = 18446744073709551615 ≈ 每秒5000帧*3600秒*24小时*365天*116988483年
        ///     整个过程超过门限统计
        /// </summary>
        private ulong[] _exceedTimes;

        /// <summary>
        ///     最终的门限值
        /// </summary>
        private float[] _finalThreshold;

        private float[] _maxLevel;

        /// <summary>
        ///     每个 _extractTimeGap 内最大值统计，用于计算占用度提取信号
        /// </summary>
        private float[] _maxLevelOfExtract;

        /// <summary>
        ///     信号信息，起始就是存的信噪比
        /// </summary>
        private float[] _signals;

        private float[] _threshold;

        /// <summary>
        ///     整个过程电平和
        /// </summary>
        private float[] _totalLevel;

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="pointCount">点数</param>
        /// <param name="autoThreshold">是否自动门限</param>
        /// <param name="thrMargin">门限值</param>
        internal void Init(int pointCount, bool autoThreshold, float thrMargin)
        {
            _exceedTimes = new ulong[pointCount];
            _dataTimes = 0;
            _signals = new float[pointCount];
            _totalLevel = new float[pointCount];
            _maxLevel = Enumerable.Repeat(-9999f, pointCount).ToArray();
            _maxLevelOfExtract = new float[pointCount];
            Array.Copy(_maxLevel, _maxLevelOfExtract, pointCount);
            _autoThreshold = autoThreshold;
            if (_autoThreshold)
                _at = new AutoThreshold
                {
                    ThreadsholdMargion = thrMargin
                };
            else
                _threshold = Enumerable.Repeat(thrMargin, pointCount).ToArray();
        }

        internal bool Statistics(float[] spectrum)
        {
            var threshold = _autoThreshold ? _at.GetThreshold(spectrum) : _threshold;
            if (spectrum.Min() <= -9999)
                // 防止出现极小值
                return false;
            for (var i = 0; i < spectrum.Length; i++)
            {
                var val = spectrum[i];
                var thr = threshold[i];
                _totalLevel[i] += val;
                _maxLevel[i] = (float)Math.Round(Math.Max(_maxLevel[i], val), 2);
                _maxLevelOfExtract[i] = Math.Max(_maxLevelOfExtract[i], val);
                if (val > thr) _exceedTimes[i]++;
            }

            _dataTimes++;
            return true;
        }

        /// <summary>
        ///     提取一次信号
        /// </summary>
        internal void Extract()
        {
            // 使用最大频谱提取信号
            var thr = _autoThreshold ? _at.GetThreshold(_maxLevelOfExtract) : _threshold;
            _finalThreshold = thr.Select(item => (float)Math.Round(item, 2)).ToArray();
            for (var i = 0; i < _maxLevelOfExtract.Length; i++)
            {
                var snr = _maxLevelOfExtract[i] - thr[i];
                if (snr > 1 && snr > _signals[i]) _signals[i] = (float)Math.Round(snr, 2);
            }

            _maxLevelOfExtract = Enumerable.Repeat(-9999f, _maxLevelOfExtract.Length).ToArray();
        }

        /// <summary>
        ///     获取最终的提取结果
        ///     最大值 均值 门限值 最大值-门限
        /// </summary>
        internal Tuple<float[], float[], float[], float[]> GetStatsInfo()
        {
            // 计算均值
            var ave = new float[_maxLevel.Length];
            for (var i = 0; i < ave.Length; i++) ave[i] = (float)Math.Round(_totalLevel[i] / _dataTimes, 2);
            return new Tuple<float[], float[], float[], float[]>(_maxLevel, ave, _finalThreshold, _signals);
        }
    }

    #region 属性和字段

    private long _totalCount;

    /// <summary>
    ///     上一次信号提取时间
    /// </summary>
    private DateTime _preExtractTime = DateTime.MinValue;

    /// <summary>
    ///     每个频段额信号提取处理类实例
    /// </summary>
    private readonly List<SingleSegStatistics> _segCalculators = new();

    private ScanSegmentStatistics[] _scanSegments;

    /// <summary>
    ///     true:自动门限
    /// </summary>
    private bool _isAutoThreshold;

    private bool _isRunning;

    /// <summary>
    ///     门限值 or 自动门限容差
    /// </summary>
    private float _threshold;

    private Thread _thdProcess;
    private readonly MQueue<float[]> _dataQueue = new();
    private readonly object _lockSegmentList = new();

    /// <summary>
    ///     信号提取间隔ms
    /// </summary>
    public int ExtractTimeGap { get; set; } = 1000;

    public event EventHandler<List<object>> DataProcessComplete;

    #endregion
}