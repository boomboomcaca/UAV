using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Magneto.Contract.Algorithm;

public class WbdfBearingStatistics : IDisposable
{
    // 最大时间片窗口内的数据
    private readonly List<SwbdFindData> _dataBuffer = [];

    // 最大数据缓存时间长度，默认为25s
    private readonly int _dataBufferMaxTimeLen = 25;

    // 数据缓存最少保留数据量
    private readonly int _dataBufferMinKeep = 0;

    // 根据角度分辨率计算出的角度数量
    private int _angleCount = 360;

    private float _angleResolution = 1;

    // 测向频率
    private double[] _freqs;

    /// <summary>
    ///     是否在计算，用于正常推出计算线程
    /// </summary>
    private bool _isCaculating;

    // 是否清除过数据， 清除数据后的第一次统计结果不正确，不触发事件
    private bool _isClearedData;

    // 标识是否正在清理数据
    private bool _isClearingData;

    // 统计线程是否运行
    private bool _isRunning = true;

    private int _sectorCount = 12;

    // 扇面分区大小，默认30°
    private int _sectorPeriod = 30;

    // 统计线程
    private Thread _statisticsThread;
    private int _timeResolution = 10000;

    /// <summary>
    ///     构造函数
    /// </summary>
    public WbdfBearingStatistics()
    {
    }

    /// <summary>
    ///     获取或设置示向度统计优化算法
    /// </summary>
    public StatisticsMethod BearingStatisticsMethod { get; set; } = StatisticsMethod.SectorFilter;

    /// <summary>
    ///     获取或设置角度分辨率，默认为1°
    /// </summary>
    public float AngleResolution
    {
        get => _angleResolution;
        set
        {
            _angleResolution = value;
            _angleCount = (int)Math.Round(360 / value);
        }
    }

    /// <summary>
    ///     获取或设置统计计算周期ms
    /// </summary>
    public int StatisticsPeriod { get; set; } = 500;

    /// <summary>
    ///     获取或设置数据筛选时间片长度ms
    /// </summary>
    public int TimeResolution
    {
        get => _timeResolution;
        set
        {
            if (value > _dataBufferMaxTimeLen * 1000)
                value = _dataBufferMaxTimeLen * 1000;
            _timeResolution = value;
        }
    }

    /// <summary>
    ///     获取或设置扇面分区数量， 最小3，最大24
    /// </summary>
    public int SectorCount
    {
        get => _sectorCount;
        set
        {
            if (_sectorCount > 24)
                _sectorCount = 24;
            if (_sectorCount < 3)
                _sectorCount = 3;
            while (360 % value >= 0) value--;
            _sectorCount = value;
            _sectorPeriod = 360 / value;
        }
    }

    /// <summary>
    ///     获取最大概率角度
    /// </summary>
    public float[] MaxProbabilityAngle { get; private set; }

    /// <summary>
    ///     获取或设置归一化比例
    /// </summary>
    public double NormalizeScale { get; set; } = 100;

    /// <summary>
    ///     概率变更事件
    /// </summary>
    public event EventHandler<WbdfBearingStatisticsArgs> ProbabilityChanged;

    /// <summary>
    ///     初始化频点
    /// </summary>
    /// <param name="freqs"></param>
    public void InitFreqs(double[] freqs)
    {
        _freqs = freqs;
        MaxProbabilityAngle = Enumerable.Repeat<float>(-1, freqs.Length).ToArray();
        _isClearedData = false;
    }

    private void Statistics()
    {
        while (_isRunning)
        {
            lock (_dataBuffer)
            {
                // 检查数据缓存，根据_dataBufferMaxTimeLen进行移除
                InvalidateDataBuffer();
            }

            // 判断是否已暂停
            if (!_isCaculating || _dataBuffer.Count < 1)
            {
                Thread.Sleep(StatisticsPeriod);
                continue;
            }

            // 提取最新的数据
            var dt = DateTime.Now;
            List<SwbdFindData> data;
            lock (_dataBuffer)
            {
                data = _dataBuffer.Where(d => (dt - d.TimeStamp).TotalMilliseconds < TimeResolution).ToList();
            }

            if (data.Count < 1)
                continue;
            // 一开始计算就缓存相应参数，以支持实时修改参数
            var pointCount = _freqs.Length;
            var angleCount = _angleCount;
            var angleRes = _angleResolution;
            double[][] probability = null;
            List<float> maxProbitAngle = null;
            switch (BearingStatisticsMethod)
            {
                case StatisticsMethod.Normal:
                    probability = NormalStatistics(data, pointCount, angleCount, angleRes, out maxProbitAngle);
                    break;
                case StatisticsMethod.SectorFilter:
                    probability = SectorFilterStatistics(data, pointCount, angleCount, angleRes, out maxProbitAngle);
                    break;
                case StatisticsMethod.VarianceFilter:
                    probability = VarianceFilterStatistics(data, pointCount, angleCount, angleRes, out maxProbitAngle);
                    break;
            }

            MaxProbabilityAngle = maxProbitAngle?.ToArray();
            // 二维数组调换行和列
            var angleProbits = new double[angleCount][];
            for (var i = 0; i < angleCount; i++)
            {
                var singleProbits = new double[pointCount];
                for (var j = 0; j < pointCount; j++)
                    if (probability != null)
                        singleProbits[j] = probability[j][i];
                angleProbits[i] = singleProbits;
            }

            if (!_isClearedData)
            {
                // 触发事件  事件有注册 && 线程在运行 && 参数未切换 
                if (ProbabilityChanged != null && _isRunning && _freqs.Length == pointCount)
                {
                    WbdfBearingStatisticsArgs e = new()
                    {
                        AngleProbability = angleProbits,
                        MaxProbabilityAngle = MaxProbabilityAngle
                    };
                    try
                    {
                        ProbabilityChanged(this, e);
                    }
                    catch
                    {
                        // 防止外部出错导致内部线程退出                            
                    }
                }
            }
            else
            {
                _isClearedData = false;
            }

            // 等待
            Thread.Sleep(StatisticsPeriod);
        }
    }

    /// <summary>
    ///     检查数据缓存，根据_dataBufferMaxTimeLen进行移除
    /// </summary>
    private void InvalidateDataBuffer()
    {
        // 超出时间数据范围
        var outTimeDataCount = 0;
        var nowTime = DateTime.Now;
        for (var i = 0; i < _dataBuffer.Count; i++)
        {
            var ts = nowTime - _dataBuffer[i].TimeStamp;
            if (!(ts.TotalSeconds < _dataBufferMaxTimeLen)) continue;
            outTimeDataCount = i;
            break;
        }

        // 校验剩余数据量
        if (_dataBuffer.Count - outTimeDataCount < _dataBufferMinKeep)
            outTimeDataCount = _dataBuffer.Count - _dataBufferMinKeep;
        // 移除超界数据
        if (outTimeDataCount > 0)
            _dataBuffer.RemoveRange(0, outTimeDataCount);
    }

    #region StatisticsMethod.Normal

    /// <summary>
    ///     常规方式统计，传统的概率计算
    /// </summary>
    /// <param name="data"></param>
    /// <param name="pointCount"></param>
    /// <param name="angleCount"></param>
    /// <param name="angleRes"></param>
    /// <param name="maxProbitAngles"></param>
    /// <returns></returns>
    private double[][] NormalStatistics(List<SwbdFindData> data, int pointCount, int angleCount, float angleRes,
        out List<float> maxProbitAngles)
    {
        // 计算概率
        var probability = StatisticProbability(data, pointCount, angleCount, angleRes, out maxProbitAngles);
        return probability;
    }

    #endregion StatisticsMethod.Normal

    private double[][] StatisticProbability(List<SwbdFindData> data, int pointCount, int angleCount, float angleRes,
        out List<float> maxProbitAngles)
    {
        List<double[]> probabilities = [];
        maxProbitAngles = [];
        for (var i = 0; i < pointCount; i++)
        {
            List<SdFindData> singlePointData = [];
            foreach (var item in data)
                singlePointData.Add(new SdFindData()
                {
                    Azimuth = item.Azimuth[i],
                    Lat = item.Lat,
                    Lng = item.Lng,
                    Quality = item.Quality[i],
                    TimeStamp = item.TimeStamp
                });
            // 计算概率
            float maxProbitAngle = -1;
            var probability = StatisticSingleProbability(singlePointData, angleCount, angleRes, ref maxProbitAngle);
            probabilities.Add(probability);
            maxProbitAngles.Add(maxProbitAngle);
        }

        return probabilities.ToArray();
    }

    private double[] StatisticSingleProbability(List<SdFindData> data, int angleCount, float angleRes,
        ref float maxProbitAngle)
    {
        if (!data.Any(d => d.Azimuth is >= 0 and < 360))
            return Enumerable.Repeat(double.NaN, angleCount).ToArray();
        // 出现次数
        var timeCount = new double[angleCount];
        // 统计次数
        foreach (var item in data)
        {
            var angleIndex = (int)((item.Azimuth + angleRes / 2) / angleRes);
            angleIndex = angleIndex * angleRes >= 360 ? 0 : angleIndex;
            timeCount[angleIndex]++;
        }

        // 计算概率
        for (var i = 0; i < timeCount.Length; i++) timeCount[i] /= data.Count;
        // 归一化，同时查找最优值
        maxProbitAngle = -1;
        var maxProbit = timeCount.Max();
        var scale = NormalizeScale / maxProbit;
        for (var i = 0; i < timeCount.Length; i++)
        {
            if (maxProbitAngle < 0 && maxProbit > 0 && Math.Abs(timeCount[i] - maxProbit) < 1e-9)
                maxProbitAngle = i * angleRes;
            timeCount[i] *= scale;
        }

        return timeCount;
    }

    #region StatisticsMethod.VarianceFilter

    private double[][] VarianceFilterStatistics(List<SwbdFindData> data, int pointCount, int angleCount,
        float angleRes, out List<float> maxProbitAngles)
    {
        var probabilities = new List<double[]>();
        maxProbitAngles = [];
        for (var i = 0; i < pointCount; i++)
        {
            var singlePointData = new List<SdFindData>();
            foreach (var item in data)
                singlePointData.Add(new SdFindData()
                {
                    Azimuth = item.Azimuth[i],
                    Lat = item.Lat,
                    Lng = item.Lng,
                    Quality = item.Quality[i],
                    TimeStamp = item.TimeStamp
                });
            // 1、方差筛选
            VarianceFilter(ref singlePointData);
            // 2、 移除按质量排序的30%的测向数据
            QualityFilter(ref singlePointData);
            // 计算概率
            float maxProbitAngle = -1;
            var probability = StatisticSingleProbability(singlePointData, angleCount, angleRes, ref maxProbitAngle);
            probabilities.Add(probability);
            maxProbitAngles.Add(maxProbitAngle);
        }

        return probabilities.ToArray();
    }

    /// <summary>
    ///     方差筛选
    /// </summary>
    /// <param name="data"></param>
    private void VarianceFilter(ref List<SdFindData> data)
    {
        if (data.Count <= 15) return;
        var reserveDataArray = data.GetRange(0, data.Count); //对所有数据进行方差筛选
        // 方差排序
        var avg = MeanAngle(reserveDataArray.ConvertAll(item => item.Azimuth))[0];
        reserveDataArray.Sort((df1, df2) =>
        {
            var err1 = Math.Abs(df1.Azimuth - avg);
            var err2 = Math.Abs(df2.Azimuth - avg);
            if (err1 > 180)
                err1 -= 360;
            if (err2 > 180)
                err2 -= 360;
            // 方差值最小的在最后面
            var index = Math.Abs(err2).CompareTo(Math.Abs(err1));
            index = index == 0 ? df1.Quality.CompareTo(df2.Quality) : index;
            index = index == 0 ? df1.TimeStamp.CompareTo(df2.TimeStamp) : index;
            return index;
        });
        //删除比例
        const float removeRatio = 0.45f;
        var removeData = reserveDataArray.GetRange(0, (int)(removeRatio * data.Count));
        data = data.Except(removeData).ToList();
    }

    public static List<float> MeanAngle(IList<float> angles, bool bimodal = false)
    {
        //angles:示向度角度数据\n
        //bimodal:是否是严格双峰数据，即数据在相差180°方向的两个方向摆动\n
        var n = angles.Count;
        double sv = 0;
        double cv = 0;
        if (bimodal)
            for (var i = 0; i < n; i++)
            {
                angles[i] *= 2;
                if (angles[i] >= 360)
                    angles[i] -= 360;
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
        else if (sa < 0 && ca > 0)
            ave = 360 - ave;
        return [ave, r];
    }

    /// <summary>
    ///     质量筛选
    /// </summary>
    /// <param name="data"></param>
    private void QualityFilter(ref List<SdFindData> data)
    {
        data.Sort((df1, df2) =>
        {
            var index = df1.Quality.CompareTo(df2.Quality);
            return index == 0 ? df1.TimeStamp.CompareTo(df2.TimeStamp) : index;
        });
        // 移除按质量排序的30%的测向数据
        const float removeRatio = 0.3f;
        var thirtyPercent = data.GetRange(0, (int)(removeRatio * data.Count));
        data = data.Except(thirtyPercent).ToList();
    }

    #endregion StatisticsMethod.VarianceFilter

    #region StatisticsMethod.SectorFilter

    private double[][] SectorFilterStatistics(List<SwbdFindData> data, int pointCount, int angleCount, float angleRes,
        out List<float> maxProbitAngles)
    {
        List<double[]> probabilities = [];
        maxProbitAngles = [];
        for (var i = 0; i < pointCount; i++)
        {
            List<SdFindData> singlePointData = [];
            foreach (var item in data)
                singlePointData.Add(new SdFindData()
                {
                    Azimuth = item.Azimuth[i],
                    Lat = item.Lat,
                    Lng = item.Lng,
                    Quality = item.Quality[i],
                    TimeStamp = item.TimeStamp
                });
            // 计算概率
            // 1、按照扇面分区，返回数据量最多的扇面内的数据
            SectorFilter(ref singlePointData);
            // 2、方差筛选
            VarianceFilter(ref singlePointData);
            // 3、 移除按质量排序的30%的测向数据
            QualityFilter(ref singlePointData);
            // 4、计算概率
            float maxProbitAngle = -1;
            var probability = StatisticSingleProbability(singlePointData, angleCount, angleRes, ref maxProbitAngle);
            // MD probability=null怎么处理
            probabilities.Add(probability);
            maxProbitAngles.Add(maxProbitAngle);
            Thread.Sleep(0); // 提升性能？
        }

        return probabilities.ToArray();
    }

    private void SectorFilter(ref List<SdFindData> data)
    {
        // 扇面内最大数据数
        var maxCount = 0;
        // 数据最多的扇面内的数据
        var maxCountOfSectorData = new List<SdFindData>();
        for (var i = 0; i < SectorCount; i++)
        {
            float sectorMin = i * _sectorPeriod;
            float sectorMax = (i + 1) * _sectorPeriod;
            var tmp = data.Where(d => d.Azimuth >= sectorMin && d.Azimuth < sectorMax).ToList();
            if (tmp.Count <= maxCount) continue;
            maxCount = tmp.Count;
            maxCountOfSectorData = tmp;
        }

        data = maxCountOfSectorData;
    }

    #endregion StatisticsMethod.SectorFilter

    #region 外部调用方法 启动、暂停、停止、Dispose

    /// <summary>
    ///     开始统计
    /// </summary>
    public void StartStatistics()
    {
        if (_statisticsThread == null)
        {
            _statisticsThread = new Thread(Statistics)
            {
                Name = "statisticsThread",
                IsBackground = true
            };
            _statisticsThread.Start();
        }

        _isCaculating = true;
    }

    /// <summary>
    ///     暂停统计
    ///     统计线程继续运行
    /// </summary>
    public void PauseStatistics()
    {
        _isCaculating = false;
    }

    /// <summary>
    ///     停止统计
    ///     统计线程继续运行,但是会清除数据
    /// </summary>
    public void StopStatistics()
    {
        _isCaculating = false;
        lock (_dataBuffer)
        {
            _dataBuffer.Clear();
        }
        // _isRunning = false;
    }

    /// <summary>
    ///     添加测向数据
    /// </summary>
    /// <param name="bearingData"></param>
    public void AddData(SwbdFindData bearingData)
    {
        if (_isClearingData || _freqs == null)
            return;
        lock (_dataBuffer)
        {
            if (bearingData.Azimuth.Length == _freqs.Length) _dataBuffer.Add(bearingData);
        }
    }

    /// <summary>
    ///     清除数据
    ///     清理后 生效时间≤StatisticsPeriod * 2（ms）
    /// </summary>
    public void ClearData()
    {
        _isClearingData = true;
        lock (_dataBuffer)
        {
            // 标识有清理过数据
            _isClearedData = true;
            _dataBuffer.Clear();
        }

        if (MaxProbabilityAngle != null)
            MaxProbabilityAngle = Enumerable.Repeat<float>(-1, _freqs.Length).ToArray();
        _isClearingData = false;
    }

    /// <summary>
    ///     防止未调用Stop方法终止计算
    /// </summary>
    public void Dispose()
    {
        _isCaculating = false;
        _isRunning = false;
    }

    #endregion 外部调用方法 启动、暂停、停止、Dispose
}