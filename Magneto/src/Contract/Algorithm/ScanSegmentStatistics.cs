using System;
using System.Collections.Generic;
using System.Linq;
using Magneto.Protocol.Define;

namespace Magneto.Contract.Algorithm;

public class ScanSegmentStatistics
{
    private const double Epsilon = 1.0E-7d;
    private readonly int[] _dataCount;
    private readonly bool _isDataAppend;
    private readonly bool[] _resetSign;
    private readonly TheoryThreshold _theoryThreshold;

    /// <summary>
    ///     缓存抽点临时数据
    /// </summary>
    private readonly short[] _tmpData;

    private readonly long[] _totalData;

    private bool _isReset;
    private bool _isThresholdUpdated;

    /// <summary>
    ///     当前抽点时间片的扫描起始位置
    /// </summary>
    private int _startOffset;

    /// <summary>
    ///     当前抽点时间片的扫描结束位置
    /// </summary>
    private int _stopOffset;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="index">频段序号</param>
    /// <param name="startFrequency">起始频率 MHz</param>
    /// <param name="stopFrequency">结束频率 MHz</param>
    /// <param name="stepFrequency">频率步进 KHz</param>
    /// <param name="isDataAppend"></param>
    public ScanSegmentStatistics(int index, double startFrequency, double stopFrequency, double stepFrequency,
        bool isDataAppend = false)
    {
        _isDataAppend = isDataAppend;
        Index = index;
        StartFrequency = startFrequency;
        StopFrequency = stopFrequency;
        StepFrequency = stepFrequency;
        IsOver = false;
        Total = Utils.GetTotalCount(startFrequency, stopFrequency, stepFrequency);
        _theoryThreshold = new TheoryThreshold();
        var count = 25;
        if (Total <= count) count = Total;
        _theoryThreshold.SegmentNumber = Total / count;
        Threshold = new short[Total];
        _tmpData = Enumerable.Repeat(short.MinValue, Total).ToArray();
        Data = Enumerable.Repeat(short.MinValue, Total).ToArray();
        Max = Enumerable.Repeat(short.MinValue, Total).ToArray(); //new float[Total];
        Min = Enumerable.Repeat(short.MaxValue, Total).ToArray(); // new float[Total];
        Mean = new short[Total];
        _dataCount = new int[Total];
        _totalData = new long[Total];
        _resetSign = new bool[Total];
    }

    public ScanSegmentStatistics(int index, int total, bool isDataAppend = false)
    {
        _isDataAppend = isDataAppend;
        Index = index;
        IsOver = false;
        Total = total;
        _theoryThreshold = new TheoryThreshold();
        var count = 25;
        if (Total <= count) count = Total;
        _theoryThreshold.SegmentNumber = Total / count;
        Threshold = new short[Total];
        _tmpData = Enumerable.Repeat(short.MinValue, Total).ToArray();
        Data = Enumerable.Repeat(short.MinValue, Total).ToArray();
        Max = Enumerable.Repeat(short.MinValue, Total).ToArray(); //new float[Total];
        Min = Enumerable.Repeat(short.MaxValue, Total).ToArray(); // new float[Total];
        Mean = new short[Total];
        _dataCount = new int[Total];
        _totalData = new long[Total];
        _resetSign = new bool[Total];
    }

    public double StartFrequency { get; }
    public double StopFrequency { get; }
    public double StepFrequency { get; }

    /// <summary>
    ///     频段序号
    /// </summary>
    public int Index { get; }

    public bool IsOver { get; set; }

    /// <summary>
    ///     缓存实时数据
    /// </summary>
    public short[] Data { get; }

    /// <summary>
    ///     最大值
    /// </summary>
    public short[] Max { get; }

    /// <summary>
    ///     最小值
    /// </summary>
    public short[] Min { get; }

    /// <summary>
    ///     平均值
    /// </summary>
    public short[] Mean { get; }

    /// <summary>
    ///     本频段的数据长度
    /// </summary>
    public int Total { get; }

    /// <summary>
    ///     当前扫描完的位置
    /// </summary>
    public int ScanIndex { get; set; }

    /// <summary>
    ///     当前扫描到的位置
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    ///     获取/设置当前帧扫描数据的门限值
    /// </summary>
    public short[] Threshold { get; set; }

    /// <summary>
    ///     自动门限噪声
    /// </summary>
    public short[] AutoNoise
    {
        get
        {
            var data = Array.ConvertAll(Data, item => item / 10f);
            var noise = _theoryThreshold.CalThreshold(data, StartFrequency, StopFrequency, (float)StepFrequency);
            return Array.ConvertAll(noise, item => (short)(item * 10));
        }
    }

    public bool AutoThreshold { get; set; } = true;

    /// <summary>
    ///     重置统计数据
    /// </summary>
    public void ResetStat()
    {
        for (var i = 0; i < Total; i++)
        {
            Max[i] = short.MinValue;
            Min[i] = short.MaxValue;
            Mean[i] = 0;
            _dataCount[i] = 0;
            _totalData[i] = 0;
        }
    }

    /// <summary>
    ///     向频段中添加数据
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="offset">本组数据在频段中的位置</param>
    /// <param name="length">本组数据的长度</param>
    public void AddData(short[] data, int offset, int length)
    {
        // Array.Copy(data, 0, Data, offset, length);
        Buffer.BlockCopy(data, 0, Data, offset * sizeof(short), length * sizeof(short));
        for (var i = 0; i < length; i++)
        {
            var index = offset + i;
            if (_isDataAppend) continue;
            _dataCount[index]++;
            _totalData[index] += data[i];
            // Max[index] = Math.Max(Max[index], data[i]);
            if (data[i] > Max[index]) Max[index] = data[i];
            // Min[index] = Math.Min(Min[index], data[i]);
            if (data[i] < Min[index]) Min[index] = data[i];
            // Mean[index] = (short)(((Mean[index] * (_dataCount[index] - 1)) + data[i]) / (float)_dataCount[index]);
            Mean[index] = (short)(_totalData[index] / (float)_dataCount[index]);
        }

        IsOver = offset + length == Total;
    }

    /// <summary>
    ///     向频段中以抽点的方式添加数据
    ///     抽点的逻辑是最大值抽点
    ///     每次取出数据以后需要重置这些数据，否则数据会变成最大值保持的数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    public void AppendData(short[] data, int offset, int length)
    {
        if (offset + length > Total) length = Total - offset;
        if (_isReset)
        {
            _startOffset = _stopOffset == Total - 1 ? 0 : offset;
            _stopOffset = 0;
            _isReset = false;
        }

        // Array.Copy(data, 0, Data, offset, length);
        Buffer.BlockCopy(data, 0, Data, offset * sizeof(short), length * sizeof(short));
        for (var i = 0; i < length; i++)
        {
            var index = offset + i;
            if (_resetSign[index])
            {
                _tmpData[index] = short.MinValue;
                // Mean[index] = 0;
                // _dataCount[index] = 0;
                _resetSign[index] = false;
            }

            _tmpData[index] = _tmpData[index] > data[i] ? _tmpData[index] : data[i];
            if (_isDataAppend) continue;
            _dataCount[index]++;
            _totalData[index] += data[i];
            Max[index] = Max[index] > data[i] ? Max[index] : data[i];
            Min[index] = Min[index] < data[i] ? Min[index] : data[i];
            // Mean[index] = (short)(((Mean[index] * (_dataCount[index] - 1)) + data[i]) / (float)_dataCount[index]);
            Mean[index] = (short)(_totalData[index] / (float)_dataCount[index]);
        }

        if (_stopOffset <= offset + length - 1) _stopOffset = offset + length - 1;
        // Console.WriteLine($"Seg:{Index}:{offset},{length},||||{_startOffset},{_stopOffset},total{Total}");
        IsOver = offset + length == Total;
    }

    /// <summary>
    ///     重置抽点时间片
    ///     每次取出数据以后需要重置这些数据，否则数据会变成最大值保持的数据
    ///     <param name="offset"></param>
    ///     <param name="length"></param>
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    public void ResetAppend(int offset, int length)
    {
        for (var i = 0; i < length; i++)
        {
            var index = i + offset;
            // Data[index] = float.MinValue;
            _resetSign[index] = true;
        }

        _startOffset = -1;
        _isReset = true;
    }

    /// <summary>
    ///     校验频段信息
    /// </summary>
    /// <param name="startFrequency"></param>
    /// <param name="stopFrequency"></param>
    /// <param name="stepFrequency"></param>
    public bool CheckSegment(double startFrequency, double stopFrequency, double stepFrequency)
    {
        if (Math.Abs(startFrequency - StartFrequency) > Epsilon) return false;
        if (Math.Abs(stopFrequency - StopFrequency) > Epsilon) return false;
        if (Math.Abs(stepFrequency - StepFrequency) > Epsilon) return false;
        return true;
    }

    public short[] GetMaxData(int offset, int length)
    {
        var data = new short[length];
        // Array.Copy(Max, offset, data, 0, length);
        Buffer.BlockCopy(Max, offset * sizeof(short), data, 0, length * sizeof(short));
        return data;
    }

    public short[] GetMinData(int offset, int length)
    {
        var data = new short[length];
        // Array.Copy(Min, offset, data, 0, length);
        Buffer.BlockCopy(Min, offset * sizeof(short), data, 0, length * sizeof(short));
        return data;
    }

    public short[] GetMeanData(int offset, int length)
    {
        var data = new short[length];
        // Array.Copy(Mean, offset, data, 0, length);
        Buffer.BlockCopy(Mean, offset * sizeof(short), data, 0, length * sizeof(short));
        return data;
    }

    public short[] GetThresholdData(int offset, int length)
    {
        if (!_isThresholdUpdated && AutoThreshold) return null;
        //_isThresholdUpdated = false;
        var data = new short[length];
        // Array.Copy(Threshold, offset, data, 0, length);
        Buffer.BlockCopy(Threshold, offset * sizeof(short), data, 0, length * sizeof(short));
        return data;
    }

    public short[] GetData(out int offset, out int length)
    {
        length = _stopOffset - _startOffset + 1;
        offset = _startOffset;
        if (_startOffset < 0) return null;
        var data = new short[length];
        // Array.Copy(_tmpData, offset, data, 0, length);
        Buffer.BlockCopy(_tmpData, offset * sizeof(short), data, 0, length * sizeof(short));
        for (var i = 0; i < length; i++)
        {
            var index = i + offset;
            if (data[i] == short.MinValue) data[i] = 0;
            if (!_isDataAppend) continue;
            _dataCount[index]++;
            _totalData[index] += data[i];
            // Max[index] = Math.Max(Max[index], data[i]);
            if (data[i] > Max[index]) Max[index] = data[i];
            // Min[index] = Math.Min(Min[index], data[i]);
            if (data[i] < Min[index]) Min[index] = data[i];
            // Mean[index] = (short)(((Mean[index] * (_dataCount[index] - 1)) + data[i]) / (float)_dataCount[index]);
            Mean[index] = (short)(_totalData[index] / (float)_dataCount[index]);
        }

        // 取出来以后就将这部分数据清空
        ResetAppend(offset, length);
        // Console.WriteLine($"        Seg:{Index}:{offset},{length},total{Total}");
        return data;
    }

    /// <summary>
    ///     计算其理论门限
    /// </summary>
    /// <param name="threshold">自动门限容差</param>
    public void CalTheoryThreshold(float threshold)
    {
        AutoThreshold = true;
        var data = Array.ConvertAll(Data, item => item / 10f);
        var bNoise = _theoryThreshold.CalThreshold(data, StartFrequency, StopFrequency, (float)StepFrequency);
        for (var i = 0; i < bNoise.Length; i++) Threshold[i] = (short)((bNoise[i] + threshold) * 10);
    }

    /// <summary>
    ///     以手动门限来设置其门限值
    /// </summary>
    /// <param name="threshold"></param>
    public void SetThreshold(float threshold)
    {
        AutoThreshold = false;
        for (var i = 0; i < Threshold.Length; i++) Threshold[i] = (short)(threshold * 10);
    }

    /// <summary>
    ///     设置数组式门限值
    /// </summary>
    /// <param name="threshold"></param>
    /// <param name="autoThreshold"></param>
    /// <param name="thdUpdateSign">更新标记，刚初始化时为false，如果是自动门限则不发送门限信息</param>
    public void SetThreshold(float[] threshold, bool autoThreshold, bool thdUpdateSign)
    {
        AutoThreshold = autoThreshold;
        _isThresholdUpdated = thdUpdateSign;
        if (threshold == null) return;
        for (var i = 0; i < Threshold.Length; i++)
            if (threshold.Length > i)
                Threshold[i] = (short)(threshold[i] * 10);
            else
                Threshold[i] = (short)(threshold[^1] * 10);
    }

    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>
        {
            { ParameterNames.StartFrequency, StartFrequency },
            { ParameterNames.StopFrequency, StopFrequency },
            { ParameterNames.StepFrequency, StepFrequency }
        };
    }
}