using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Protocol.Data;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     信号提取算法
/// </summary>
public class SignalExtraction : IDisposable
{
    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="startFrequency">起始频率MKz</param>
    /// <param name="stopFrequency">终止频率MHz</param>
    /// <param name="step">步进kHz</param>
    /// <param name="calThreshold">True:计算门限 False：不计算门限</param>
    /// <param name="thresholdMargin">门限阈值</param>
    /// <param name="snrThreshold">信噪比门限</param>
    public SignalExtraction(double startFrequency, double stopFrequency, float step, bool calThreshold = false,
        int thresholdMargin = 0, int snrThreshold = 8)
    {
        SegmentId = Guid.NewGuid();
        StartFrequency = startFrequency;
        StopFrequency = stopFrequency;
        StepFrequency = step;
        _snrThreshold = snrThreshold;
        PointCount = Utils.GetTotalCount(startFrequency, stopFrequency, step);
        FrequencyList = new double[PointCount];
        for (var i = 0; i < PointCount; i++)
            FrequencyList[i] = Math.Round((StartFrequency * 1000 + i * StepFrequency) / 1000, 6);
        if (calThreshold) //自动门限
        {
            _theoryThreshold = new TheoryThreshold
            {
                ThresholdMargin = thresholdMargin
            };
            var count = 25;
            if (PointCount <= count) count = PointCount;
            _theoryThreshold.SegmentNumber = PointCount / count;
        }
    }

    /// <summary>
    ///     提取信号的占用度门限
    /// </summary>
    public float OccThreshold
    {
        set => _occThreshold = value;
    }

    public void Dispose()
    {
        _signalBuffer = null;
        GC.SuppressFinalize(this);
        Snr = null;
    }

    /// <summary>
    ///     设置门限
    /// </summary>
    /// <param name="isAuto"></param>
    /// <param name="lineThresholdVal"></param>
    public void SetThreshold(bool isAuto, float lineThresholdVal)
    {
        _isAutoThreshold = isAuto;
        if (isAuto)
            for (var i = 0; i < AutoThreshold.Length; i++)
                AutoThreshold[i] = lineThresholdVal;
    }

    public void SetTempleted(float[] data)
    {
        _isAutoThreshold = false;
        AutoThreshold = new float[data.Length];
        for (var i = 0; i < data.Length; i++) AutoThreshold[i] = data[i];
    }

    /// <summary>
    ///     初始化
    /// </summary>
    /// <param name="factor">天线因子</param>
    public void Init(float[] factor)
    {
        if (factor == null) return;
        PointCount = factor.Length;
        FrequencyList = new double[PointCount];
        ListScanData = new float[PointCount];
        Snr = new float[PointCount];
        AutoThreshold = new float[PointCount];
        _occ = new float[PointCount];
        _occThrCout = new int[PointCount];
        _maxVal = new float[PointCount]; // Enumerable.Repeat<float>(-9999f, PointCount).ToArray();
        Array.Fill(_maxVal, -9999f);
        for (var i = 0; i < PointCount; i++)
            FrequencyList[i] = Math.Round((StartFrequency * 1000 + i * StepFrequency) / 1000, 6);
    }

    /// <summary>
    ///     添加扫描数据
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="destIndex"></param>
    /// <param name="data"></param>
    /// <returns>true:成功 false:数据超长</returns>
    public bool AddScanData(int startIndex, int destIndex, float[] data)
    {
        if (destIndex + data.Length > PointCount) return false;

        if (destIndex + data.Length == PointCount)
        {
            IsOver = true;
            CalculationData();
        }
        else
        {
            IsOver = false;
        }

        Array.Copy(data, startIndex, ListScanData, destIndex, data.Length);
        for (var i = 0; i < data.Length; i++)
            if (_maxVal[i + destIndex] < data[i])
                _maxVal[i + destIndex] = data[i];
        return true;
    }

    /// <summary>
    ///     获取信号
    /// </summary>
    public List<SignalsResult> GetSignal()
    {
        //计算占用度
        var count = (float)_curSplitCount;
        for (var i = 0; i < _occ.Length; i++)
        {
            _occ[i] = _occThrCout[i] / count * 100;
            if (_occ[i] >= _occThreshold) _signalBuffer.Add(i);
        }

        var lstResult = UniteSignal(_signalBuffer.ToArray(), _occ, Snr);
        Reset();
        return lstResult;
    }

    /// <summary>
    ///     计算数据
    /// </summary>
    private void CalculationData()
    {
        if (IsOver)
        {
            //计算门限
            if (_isAutoThreshold)
            {
                var bNoise = _theoryThreshold.CalThreshold(ListScanData.ToArray(), StartFrequency, StopFrequency,
                    (float)StepFrequency);
                for (var i = 0; i < bNoise.Length; i++)
                    AutoThreshold[i] = bNoise[i] + 3;
            }

            for (var i = 0; i < FrequencyList.Length; i++)
                if (ListScanData[i] > AutoThreshold[i]) //如果大于门限
                {
                    //计算信噪比
                    var pointSnr = ListScanData[i] - AutoThreshold[i];
                    if (Snr[i] < pointSnr) Snr[i] = pointSnr;
                    if (Snr[i] > _snrThreshold) //表示有信号
                        _occThrCout[i]++;
                }

            _curSplitCount++;
        }
    }

    /// <summary>
    ///     将有信号的频点合并为信号
    /// </summary>
    /// <param name="signalBuffer"></param>
    /// <param name="occ"></param>
    /// <param name="snr"></param>
    private List<SignalsResult> UniteSignal(int[] signalBuffer, float[] occ, float[] snr)
    {
        var singalUnite = new List<SignalsResult>();
        //对有信号的频点进行排序
        var pointIndex = signalBuffer.ToArray();
        Array.Sort(pointIndex);
        //提取信号
        for (var i = 0; i < pointIndex.Length; i++)
        {
            int freqIndex;
            var s = i;
            var e = i + 1;
            //判断一个信号的起始索引和结束索引(连续超过门限的频点为一个信号)
            while (e < pointIndex.Length && pointIndex[e] - pointIndex[s] == 1)
            {
                s++;
                e++;
            }

            //计算信号中心频率
            var snrMaxIndex = pointIndex[i];
            var occMaxIndex = pointIndex[i];
            for (var v = i; v < e; v++)
            {
                //计算信噪比最高的频率索引
                if (snr[snrMaxIndex] < snr[pointIndex[v]]) snrMaxIndex = pointIndex[v];
                //计算占用度最高的频率索引
                if (occ[occMaxIndex] < occ[pointIndex[v]]) occMaxIndex = pointIndex[v];
            }

            //如果计算出的占用度最大频点和信噪比最大频点不同，并且两个点的占用度相差 > 1 %，则以占用度大的为中心频率
            if (snrMaxIndex != occMaxIndex && occ[occMaxIndex] - occ[snrMaxIndex] > 1)
                freqIndex = occMaxIndex;
            else
                freqIndex = snrMaxIndex;
            //计算估测带宽
            var bw = (e - i) * (float)StepFrequency;
            // bool findSkip = false;
            // //公众移动通讯频段不提取
            // if (_skipMobile)
            // {
            //     foreach (KeyValuePair<double, double> mItem in GeneralClass.MobileSegment)
            //     {
            //         if (FrequencyList[freqIndex] >= mItem.Key && FrequencyList[freqIndex] <= mItem.Value)
            //         {
            //             findSkip = true;
            //             break;
            //         }
            //     }
            //     if (findSkip) continue;
            // }
            //生成信号信息
            SignalsResult signal = new()
            {
                Frequency = FrequencyList[freqIndex],
                Bandwidth = bw,
                MaxLevel = ListScanData[freqIndex],
                LastTime = Utils.GetNowTimestamp(),
                FirstTime = Utils.GetNowTimestamp(), //背噪 = 实时值 - 信噪比
                Name = "",
                Result = "新信号"
            };
            singalUnite.Add(signal);
            i = e - 1;
        }

        return singalUnite;
    }

    public override string ToString()
    {
        return $"{StartFrequency}MHz-{StopFrequency}MHz-{StepFrequency}kHz";
    }

    /// <summary>
    ///     重置缓存变量
    /// </summary>
    public void Reset()
    {
        _curSplitCount = 0;
        _signalBuffer.Clear();
        Snr = new float[PointCount];
        _occ = new float[PointCount];
        _occThrCout = new int[PointCount];
        _maxVal = new float[PointCount]; // Enumerable.Repeat<float>(-9999f, PointCount).ToArray();
        Array.Fill(_maxVal, -9999f);
    }

    #region 变量定义

    private int _curSplitCount;

    /// <summary>
    ///     自动门限
    /// </summary>
    private readonly TheoryThreshold _theoryThreshold;

    private readonly float _snrThreshold; //信噪比门限
    private float _occThreshold; //占用度门限
    private float[] _maxVal; //最大值
    private bool _isAutoThreshold = true;
    private float[] _occ;

    /// <summary>
    ///     每个频点超过门限的次数
    /// </summary>
    private int[] _occThrCout;

    private List<int> _signalBuffer = new(); //有信号的频点

    #endregion

    #region 属性

    /// <summary>
    ///     获取频段ID
    /// </summary>
    public Guid SegmentId { get; }

    /// <summary>
    ///     起始频率MHz
    /// </summary>
    public double StartFrequency { get; }

    /// <summary>
    ///     步进kHz
    /// </summary>
    public double StepFrequency { get; }

    /// <summary>
    ///     终止频率MHz
    /// </summary>
    public double StopFrequency { get; }

    /// <summary>
    ///     频点数
    /// </summary>
    public int PointCount { get; private set; }

    /// <summary>
    ///     频率列表
    /// </summary>
    public double[] FrequencyList { get; private set; }

    /// <summary>
    ///     获取/设置最新帧的扫描数据
    /// </summary>
    public float[] ListScanData { get; set; }

    public float[] Snr { get; private set; }

    /// <summary>
    ///     获取当前自动门限
    /// </summary>
    public float[] AutoThreshold { get; private set; }

    /// <summary>
    ///     是否完整一帧
    /// </summary>
    public bool IsOver { get; private set; }

    /// <summary>
    ///     跳过公众移动通讯频段
    /// </summary>
    public bool SkipMobile { get; set; } = true;

    #endregion
}