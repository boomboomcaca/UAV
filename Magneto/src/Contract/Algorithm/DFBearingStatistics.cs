/*********************************************************************************************
 *
 * 文件名称:    ...Tracker800\Client\Source\DCComponent\WinFormsUI\DC.WinFormsUI.Chart\GaugeChart\DFBearingStatistics.cs
 *
 * 作    者:    jacberg
 *
 * 创作日期:    2018/09/05
 *
 * 修    改:    完善了最优值与置信区间边界值计算/增加了分区域优选数据（扇区内的概率与方差筛选）
 *
 * 备    注:	示向度数据时间片统计算法
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     数据最大时间窗为25s
///     每次计算更新时间窗内数据，移除超时数据
/// </summary>
public sealed class DfBearingStatistics : IDisposable
{
    /// <summary>
    ///     MaxTimeWin 缓存最大时间片长度的数据
    /// </summary>
    private const int MaxTimeWin = 25;

    /// <summary>
    ///     周期数据归一化的周期大小
    /// </summary>
    private const float Period = 360.0f;

    /// <summary>
    ///     2dB倍数，用于检测间发信号（发周期比停周期长）
    /// </summary>
    private const float SecondDb = 0.79432823472f;

    /// <summary>
    ///     平均线缓存数据
    /// </summary>
    private List<SdFindData> _averageLine = [];

    /// <summary>
    ///     是否正在计算,并向外输出数据
    /// </summary>
    private bool _calculating;

    /// <summary>
    ///     最大时间片窗口内的数据
    /// </summary>
    private List<SdFindData> _dataInWin = [];

    /// <summary>
    ///     角度分辨率小数位数，在舍入时用到，必须与分辨率对应
    /// </summary>
    private int _digits;

    //SortedList<float, float> _maxAmp, _maxQuality;
    /// <summary>
    ///     是否可运行计算线程
    /// </summary>
    private bool _isRunning = true;

    private float _resolution = 1f;

    private float _sectorValue = 45;

    private Thread _statisticsThread;

    private int _timeLength = 5;

    /// <summary>
    ///     无参构造函数
    /// </summary>
    public DfBearingStatistics()
    {
    }

    /// <summary>
    ///     构造时指定
    /// </summary>
    /// <param name="sectorVal"></param>
    /// <param name="qualityThreshold"></param>
    /// <param name="computeType"></param>
    public DfBearingStatistics(float sectorVal, float qualityThreshold = 10, string computeType = "PRO")
    {
        _sectorValue = sectorVal;
        QualityThreshold = qualityThreshold;
        ComputeType = computeType;
    }

    /// <summary>
    ///     是否正在计算
    /// </summary>
    public bool Calculating => _calculating;

    /// <summary>
    ///     获取或设置统计角度分辨率 默认1°
    /// </summary>
    public float Resolution
    {
        get => _resolution;
        set
        {
            if (Math.Abs(_resolution - value) > 0.01f) _resolution = value;
            // 0.01f存在数据精度带来的键重复问题，decimal可以解决此问题，考虑内存占用以及实际应用不做更改
            if (Math.Abs(_resolution - 0.01f) < 0.01f) _resolution = 0.02f;

            if (!_resolution.ToString(CultureInfo.InvariantCulture).Contains('.')) return;

            _digits = _resolution.ToString(CultureInfo.InvariantCulture).Length - _resolution
                .ToString(CultureInfo.InvariantCulture).IndexOf(".", StringComparison.Ordinal) - 1;
        }
    }

    /// <summary>
    ///     统计时间片长度（1-15s），默认5s,该参数后面由车速部分进行动态调整
    /// </summary>
    public int TimeLength
    {
        get => _timeLength;
        set
        {
            if (value == _timeLength) return;

            if (value is >= 1 and <= MaxTimeWin)
                _timeLength = value;
            else if (value < 1)
                _timeLength = 1;
            else
                _timeLength = MaxTimeWin;
        }
    }

    public bool IsStatisticsLevelAndQuality { get; set; } = true;

    /// <summary>
    ///     算法返回的实时值，不作任何处理，只是在事件触发时刻返回，可能并不与设备实时同步
    /// </summary>
    public float RealTimeValue { get; private set; } = float.MinValue;

    /// <summary>
    ///     获取当前最优值
    /// </summary>
    public float MaxProbability { get; private set; }

    public float SectorValue
    {
        get => _sectorValue;
        set
        {
            if (value <= 0 || value > 120) return;

            _sectorValue = value;
        }
    }

    public string ComputeType { get; } = "PRO";
    public float QualityThreshold { get; } = 10;
    public double Lng { get; private set; } = double.MaxValue;
    public double Lat { get; private set; } = double.MaxValue;

    public event EventHandler<BearingStatisticsArgs> ProbabilityChanged;

    public event EventHandler<BearingStatisticsArgs> AngleStatisticsChanged;

    public void AddData(SdFindData data)
    {
        //_needUpdateProBit = true;
        // 容错处理，防止外面传入＜0||＞360的角度值，正确范围应该是[0,360)
        data.Azimuth -= (float)Math.Floor(data.Azimuth / Period) * Period;
        // 保留一位小数
        data.Azimuth = (int)(data.Azimuth * 10) / 10.0f;
        RealTimeValue = data.Azimuth;
        Lng = data.Lng;
        Lat = data.Lat;
        //this._sectorValue = sectorValue;
        //this._computeType = computeType;
        lock (_dataInWin)
        {
            if (!(data.Quality >= QualityThreshold)) return;
            if (_dataInWin.Count > 512) _dataInWin.RemoveAt(0);
            _dataInWin.Add(data);
        }
    }

    private void StatisticsBearing()
    {
        while (_isRunning)
        {
            DateTime lastTime;
            List<SdFindData> realWin = null;
            lock (_dataInWin)
            {
                // 检查数据，超过时间窗则移除
                if (_dataInWin.Count > 0 && CheckDataInWin() > 0)
                {
                    // 从数据窗内截取最新的时间窗内的数据 
                    lastTime = _dataInWin[^1].TimeStamp;
                    // 处理间发信号
                    float sum = _dataInWin.Sum(t => t.Level);

                    var meanLevel = sum / _dataInWin.Count;
                    realWin = _dataInWin.FindAll(sdf =>
                            (lastTime - sdf.TimeStamp).TotalSeconds < TimeLength && sdf.Level > meanLevel * SecondDb)
                        .ToList();
                    var filteredIndex = new List<int>();
                    switch (ComputeType)
                    {
                        case "PRO":
                            filteredIndex = DivideDataInPro(realWin.ConvertAll(item => item.Azimuth), _sectorValue);
                            break;

                        case "STD":
                            filteredIndex = DivideDataInStd(realWin.ConvertAll(item => item.Azimuth), _sectorValue);
                            break;
                    }

                    if (filteredIndex.Count > 0)
                        realWin = (from data in realWin
                                   where filteredIndex.IndexOf(realWin.IndexOf(data)) > -1
                                   select data)
                            .ToList();
                }
            }

            if (realWin == null)
            {
                // 无数据
                Thread.Sleep(30);
                continue;
            }

            SortedList<float, float> maxAmp = null;
            SortedList<float, float> maxQuality = null;
            if (IsStatisticsLevelAndQuality) StatisticsLevelAndQuality(realWin, out maxAmp, out maxQuality);
            // 1、方差、质量筛选
            var angleCount = VarQualityFilter(realWin);
            var angleArgs = new BearingStatisticsArgs
            {
                AngleStatistics = angleCount
            };
            OnAngleCountChanged(angleArgs);
            // 2、时间片筛选 概率计算
            var args = TimeSlotFilter(angleCount);
            try
            {
                if (args != null && _calculating) // 向外部发送结果
                {
                    args.MaxAmpStatistics = maxAmp;
                    args.MaxQualityStatistics = maxQuality;
                    OnProbabilityChanged(args);
                }
            }
            catch
            {
                /* 防止外部事件处理异常导致线程终止 */
            }

            Thread.Sleep(30);
        }
    }

    /// <summary>
    ///     检查时间窗内数据
    /// </summary>
    /// <returns>返回时间窗内剩余数据量</returns>
    private int CheckDataInWin()
    {
        // 如果now=DateTime.Now,最大时间窗会保留0条数据，而且回放数据会受到影响，为避免此错误发生，最大窗口内至少保留最后一条数据
        var now = _dataInWin.Last().TimeStamp;
        var outTimeData = _dataInWin.FindAll(sdf => (now - sdf.TimeStamp).TotalSeconds > MaxTimeWin);
        var removeCount = outTimeData.Count;
        if (removeCount > 0) _dataInWin = _dataInWin.ExceptEx(outTimeData).ToList();

        return _dataInWin.Count;
    }

    private SortedList<float, int> VarQualityFilter(List<SdFindData> currentData)
    {
        //删除比例
        const float removeRatio = 0.45f;
        var removeCount = (int)(currentData.Count * removeRatio / 2); //删除的数量
        // 1、方差筛选
        if (currentData.Count > 15)
        {
            var reserveDataArray = currentData.GetRange(0, currentData.Count); //对所有数据进行方差筛选
            // 方差排序
            var avg = MeanAngle(reserveDataArray.ConvertAll(item => item.Azimuth))[0];
            reserveDataArray.Sort((df1, df2) =>
            {
                var err1 = Math.Abs(df1.Azimuth - avg);
                var err2 = Math.Abs(df2.Azimuth - avg);
                if (err1 > 180) err1 -= 360;

                if (err2 > 180) err2 -= 360;

                // 方差值最小的在最后面
                var index = Math.Abs(err2).CompareTo(Math.Abs(err1));
                index = index == 0 ? df1.Quality.CompareTo(df2.Quality) : index;
                index = index == 0 ? df1.TimeStamp.CompareTo(df2.TimeStamp) : index;
                return index;
            });

            var removeData = reserveDataArray.GetRange(0, removeCount);
            currentData = currentData.Except(removeData).ToList();
        }

        // 数据集进行按质量排序  
        currentData.Sort((df1, df2) =>
        {
            var index = df1.Quality.CompareTo(df2.Quality);
            return index == 0 ? df1.TimeStamp.CompareTo(df2.TimeStamp) : index;
        });

        // 2、 移除按质量排序的30%的测向数据
        var thirtyPercent = currentData.GetRange(0, removeCount);
        currentData = currentData.Except(thirtyPercent).ToList();

        var angleCount = new SortedList<float, int>();
        foreach (var t in currentData)
            if (!angleCount.TryAdd(t.Azimuth, 1))
                angleCount[t.Azimuth]++;

        return angleCount;
    }

    private BearingStatisticsArgs TimeSlotFilter(SortedList<float, int> angleCount)
    {
        var resolution = Resolution;
        var min = 0.0f;
        var max = 0.0f;
        var minKey = 0.0f;
        var maxKey = 0.0f;
        var realMaxProbability = 0f;

        var filterAngle = new SortedList<float, float>();
        for (float a = 0; a < 360; a += resolution) filterAngle.Add((float)Math.Round(a, _digits), 0);
        var keys = filterAngle.Keys.ToArray();

        // 统计角度对应的次数
        var angleKeys = angleCount.Keys;
        foreach (var t in angleKeys)
        {
            var index = (int)Math.Floor(t / resolution);
            if (t % resolution > resolution / 2) index++;

            if (index >= 360 / resolution) index = 0;

            var key = index * resolution;
            if (filterAngle.ContainsKey(key)) filterAngle[key] += angleCount[t];
        }

        try
        {
            var maxCount = 0;
            var sum = filterAngle.Values.Sum();
            if (sum != 0)
            {
                foreach (var t in keys)
                    filterAngle[t] /= sum;

                realMaxProbability = filterAngle.Values.Max();
                maxCount = filterAngle.Values.Count(item => Math.Abs(item - realMaxProbability) < 0.01f);
            }

            if (maxCount >= 3)
                try
                {
                    // 如果有连续相等的最大值 则按照顺序将分布中点的值凸出
                    var firstIndex = filterAngle.Values.ToList().FindIndex(item => Math.Abs(item - realMaxProbability) < 0.01f);
                    var lastIndex = filterAngle.Values.ToList().FindLastIndex(item => Math.Abs(item - realMaxProbability) < 0.01f);

                    var temp = filterAngle.Values.ToList().GetRange(firstIndex, lastIndex - firstIndex + 1);
                    var allSame = temp.All(item => Math.Abs(item - realMaxProbability) < 0.01f);

                    if (allSame)
                    {
                        var index = (firstIndex + lastIndex) / 2;
                        if (lastIndex - firstIndex <= 5)
                        {
                            filterAngle[keys[index]] += realMaxProbability / 2;
                            filterAngle[keys[index - 1]] += realMaxProbability / 5;
                            filterAngle[keys[index + 1]] += realMaxProbability / 5;
                        }
                        else
                        {
                            filterAngle[keys[index]] += realMaxProbability / 5;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("扇形凸出错误！\r\n{0}", ex);
                }

            // 经过扇形凸出之后有些概率超过1在归一化               
            realMaxProbability = filterAngle.Values.Max();
            if (realMaxProbability != 0)
            {
                foreach (var t in keys)
                    filterAngle[t] /= realMaxProbability;

                // 重新归一化
                realMaxProbability = filterAngle.Values.Max();
            }

            var maxValue = 0f;
            foreach (var t in keys)
                if (filterAngle[t] > maxValue)
                {
                    maxValue = filterAngle[t];
                    maxKey = t;
                }

            MaxProbability = maxKey;

            var mean = filterAngle.Values.Average();
            var std = 0f;

            for (var i = 0; i < filterAngle.Count; i++)
                std += (filterAngle.Values[i] - mean) * (filterAngle.Values[i] - mean);

            var count = filterAngle.Values.Count(item => item != 0);
            if (count != 0) std /= count;

            min = MaxProbability - 2 * std - 3;
            max = MaxProbability + 2 * std + 3;
            try
            {
                minKey =
                    filterAngle.FirstOrDefault(item => item.Value <= std && item.Value > 0 && item.Key >= min).Key -
                    resolution;
                maxKey = filterAngle.FirstOrDefault(item => item.Value is <= 1 and >= 0 && item.Key >= max).Key +
                         resolution;
            }
            catch
            {
                /* 可能找不到符合条件的元素 导致First为null */
            }

            min = min > minKey ? min : minKey;
            max = max < maxKey ? maxKey : max;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("时间片过滤错误！\r\n{0}", ex);
        }

        if (realMaxProbability == 0f) return null;
        {
            _averageLine.Add(new SdFindData
            {
                Azimuth = MaxProbability,
                TimeStamp = DateTime.Now
            });
            var sdf = _averageLine.Last();
            for (var i = 0; i < _averageLine.Count - 1; i++)
            {
                // 删除当前最优值±_sectorValue°以外的数据 保持扇区内的最优结果
                var err = Math.Abs(_averageLine[i].Azimuth - sdf.Azimuth);
                if (err > 180) err -= 360;

                if (!(Math.Abs(err) > _sectorValue)) continue;
                _averageLine.RemoveAt(i);
                i--;
            }

            _averageLine =
                _averageLine.FindAll(item => (sdf.TimeStamp - item.TimeStamp).TotalSeconds <= MaxTimeWin / 5d);
            var average = MeanAngle(_averageLine.ConvertAll(item => item.Azimuth))[0];
            _ = float.TryParse(average.ToString("F1"), out average);
            _ = float.TryParse(min.ToString("F1"), out min);
            _ = float.TryParse(max.ToString("F1"), out max);
            return new BearingStatisticsArgs
            {
                // 概率
                AngleProbability = filterAngle,
                // 平均线对应的值
                AverageValue = average,
                // 最优值
                MaxProbability = MaxProbability,
                // 实时值
                RealTimeValue = RealTimeValue,
                // 真实最大概率
                RealMaxProbability = realMaxProbability,
                // 经度
                Lng = Lng,
                // 纬度
                Lat = Lat,
                // 集中区间的上限
                Min = min,
                // 集中区间的下限
                Max = max
            };
        }

    }

    private void StatisticsLevelAndQuality(List<SdFindData> currentData, out SortedList<float, float> maxAmp,
        out SortedList<float, float> maxQuality)
    {
        var resolution = Resolution;
        maxAmp = new SortedList<float, float>();
        maxQuality = new SortedList<float, float>();
        for (float a = 0; a < 360; a += resolution)
        {
            maxAmp.Add((float)Math.Round(a, _digits), 0);
            maxQuality.Add((float)Math.Round(a, _digits), 0);
        }

        foreach (var item in currentData)
        {
            var index = (int)Math.Floor(item.Azimuth / resolution);
            if (item.Azimuth % resolution > resolution / 2) index++;

            if (index >= 360 / resolution) index = 0;

            var key = index * resolution;
            if (maxAmp.ContainsKey(key))
                if (maxAmp[key] < item.Level)
                    maxAmp[key] = item.Level;

            if (!maxQuality.ContainsKey(key)) continue;
            if (maxQuality[key] < item.Quality)
                maxQuality[key] = item.Quality;
        }
    }

    private void OnProbabilityChanged(BearingStatisticsArgs args)
    {
        ProbabilityChanged?.Invoke(this, args);
    }

    private void OnAngleCountChanged(BearingStatisticsArgs e)
    {
        AngleStatisticsChanged?.Invoke(this, e);
    }

    public static List<float> MeanAngle(IList<float> angles, bool bimodal = false)
    {
        //
        //angles:示向度角度数据\n
        //bimodal:是否是严格双峰数据，即数据在相差180°方向的两个方向摆动\n

        var n = angles.Count;
        double sv = 0;
        double cv = 0;
        if (bimodal)
            for (var i = 0; i < n; i++)
            {
                angles[i] *= 2;
                if (angles[i] >= 360) angles[i] -= 360;
            }

        for (var i = 0; i < n; i++)
        {
            sv += Math.Sin(angles[i] * Math.PI / 180);
            cv += Math.Cos(angles[i] * Math.PI / 180);
        }

        var y = sv / n;
        var x = cv / n;
        var r = (float)Math.Sqrt(x * x + y * y);
        var ca = x / r;
        var sa = y / r;
        var ave = (float)Math.Abs(Math.Atan(sa / ca));
        ave *= (float)(180 / Math.PI);
        if (sa > 0 && ca < 0)
            ave = 180 - ave;
        else if (sa < 0 && ca < 0)
            ave += 180;
        else if (sa < 0 && ca > 0) ave = 360 - ave;

        return [ave, r];
    }

    public static List<int> DivideDataInPro(List<float> data, float sectorVal)
    {
        var zoneNumbers = (int)(Period / sectorVal);
        var infoDict = new Dictionary<int, List<int>>();
        for (var i = 0; i < zoneNumbers; i++) infoDict.Add(i, []);
        for (var i = 0; i < data.Count; i++)
        {
            var y = (int)(data[i] / sectorVal);
            infoDict[y].Add(i); // 由于数据结构的缘故，暂时存储数据的索引
        }

        var weights1 = new List<int>();
        foreach (var item in infoDict.Values) weights1.Add(item.Count);

        var infoDict2 = new Dictionary<int, List<int>>();
        for (var i = 0; i < zoneNumbers + 1; i++) infoDict2.Add(i, []);

        for (var i = 0; i < data.Count; i++)
        {
            var y = (int)(NormalizeAngle(data[i] + sectorVal / 2) / sectorVal);
            infoDict2[y].Add(i);
        }

        var weights2 = new List<int>();
        foreach (var item in infoDict2.Values) weights2.Add(item.Count);

        var isOne = weights1.Max() > weights2.Max();
        var result = isOne ? infoDict[weights1.IndexOf(weights1.Max())] : infoDict2[weights2.IndexOf(weights2.Max())];

        return result;
    }

    public static List<int> DivideDataInStd(List<float> data, float sectorVal)
    {
        var zoneNumbers = (int)(Period / sectorVal);
        var infoDict = new Dictionary<int, List<int>>();
        for (var i = 0; i < zoneNumbers; i++) infoDict.Add(i, []);
        for (var i = 0; i < data.Count; i++)
        {
            var y = (int)(data[i] / sectorVal);
            infoDict[y].Add(i); // 由于数据结;构的缘故，暂时存储数据的索引
        }

        var weights = new List<float>();
        foreach (var item in infoDict.Values)
        {
            var realVal = new List<float>();
            foreach (var index in item) realVal.Add(data[index]);

            var val = CalcStdDev(realVal);
            weights.Add(float.IsNaN(val) ? float.MaxValue : val);
        }

        return infoDict[weights.IndexOf(weights.Min())];
    }

    /// <summary>
    ///     计算正北值
    /// </summary>
    /// <param name="latA"></param>
    /// <param name="lonA"></param>
    /// <param name="latB"></param>
    /// <param name="lonB"></param>
    public static double CalcBearing(double latA, double lonA, double latB, double lonB)
    {
        var radLatA = Deg2Rad(latA);
        var radLonA = Deg2Rad(lonA);
        var radLatB = Deg2Rad(latB);
        var radLonB = Deg2Rad(lonB);
        var dLon = radLonB - radLonA;
        var y = Math.Sin(dLon) * Math.Cos(radLatB);
        var x = Math.Cos(radLatA) * Math.Sin(radLatB) - Math.Sin(radLatA) * Math.Cos(radLatB) * Math.Cos(dLon);
        var bRng = Rad2Deg(Math.Atan2(y, x));
        return (bRng + 360) % 360;
    }

    public static float CalcStdDev(IList<float> angles)
    {
        var n = angles.Count;
        float sv = 0;
        float cv = 0;
        for (var i = 0; i < n; i++)
        {
            sv += (float)Math.Sin(angles[i] * (Math.PI / 180.0));
            cv += (float)Math.Cos(angles[i] * (Math.PI / 180.0));
        }

        sv /= n;
        cv /= n;
        var stdDev = (float)Math.Sqrt(-Math.Log(sv * sv + cv * cv));
        stdDev *= (float)(180 / Math.PI);
        return stdDev;
    }

    public static double Deg2Rad(double deg)
    {
        return deg * Math.PI / 180;
    }

    public static double Rad2Deg(double rad)
    {
        return rad * 180 / Math.PI;
    }

    /// <summary>
    ///     将角度范围归一化到[0,360)之间
    /// </summary>
    /// <param name="angle"></param>
    public static float NormalizeAngle(float angle)
    {
        return angle - (float)Math.Floor(angle / Period) * Period;
    }

    public void Start()
    {
        Clear();
        _calculating = true;
        if (_statisticsThread != null) return;
        _statisticsThread = new Thread(StatisticsBearing)
        {
            Name = "DFStatisticsThread",
            IsBackground = true
        };
        _statisticsThread.Start();
    }

    public void Stop()
    {
        _calculating = false;
    }

    public void Clear()
    {
        lock (_dataInWin)
        {
            _dataInWin.Clear();
        }
    }

    #region IDisposable Support

    private bool _disposedValue; // 要检测冗余调用

    private void Dispose(bool disposing)
    {
        if (_disposedValue) return;
        if (disposing)
        {
            // 释放托管状态(托管对象)。
        }

        // 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
        // 将大型字段设置为 null。

        _disposedValue = true;
    }

    /// <summary>
    ///     添加此代码以正确实现可处置模式。
    /// </summary>
    public void Dispose()
    {
        _isRunning = false;

        // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        Dispose(true);
        // 如果在以上内容中替代了终结器，则取消注释以下行。
        // GC.SuppressFinalize(this);
    }

    #endregion
}