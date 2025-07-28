using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Contract.Algorithm;

#region 占用度相关

/// <summary>
///     占用度计算结构
/// </summary>
public class OccupancyStruct
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="freqNumbers"></param>
    public OccupancyStruct(int freqNumbers)
    {
        FreqNumbers = freqNumbers;
        ScanTimes = 0;
        UpperTimes = new int[FreqNumbers];
        //_scanData = new Queue<float[]>();
        ScanData = new ConcurrentQueue<KeyValuePair<DateTime, float[]>>();
        ThresholdData = new ConcurrentQueue<float[]>();
        NoiseData = new ConcurrentQueue<float[]>();
        Running = true;
        _maxValue = new float[freqNumbers];
        _maxValueOccMin = new float[freqNumbers];
        _noise = new double[freqNumbers];
        Snr = new float[freqNumbers];
        for (var i = 0; i < freqNumbers; i++)
        {
            _maxValue[i] = -999;
            _maxValueOccMin[i] = -999;
        }

        _cts = new CancellationTokenSource();
        _processTask = Task.Run(() => ProcessAsync(_cts.Token));
    }

    #endregion

    #region 变量/属性

    private readonly Task _processTask;
    private readonly CancellationTokenSource _cts;

    #region 统计

    /// <summary>
    ///     上一次统计时间
    /// </summary>
    private DateTime _preOccTime = DateTime.MinValue;

    /// <summary>
    ///     最大值
    /// </summary>
    private readonly float[] _maxValue;

    /// <summary>
    ///     统计间隔时间内的最大值
    /// </summary>
    private readonly float[] _maxValueOccMin;

    /// <summary>
    ///     背噪
    /// </summary>
    private readonly double[] _noise;

    /// <summary>
    ///     数据帧数
    /// </summary>
    private long _totalCount;

    private readonly Dictionary<int, PointScanStatInfo> _occPointInfo = new();

    #endregion

    public bool Running { get; set; }

    /// <summary>
    ///     获取/设置当前扫描的总点数
    /// </summary>
    public int FreqNumbers { get; set; }

    /// <summary>
    ///     获取/设置完整的扫描次数
    /// </summary>
    public int ScanTimes { get; set; }

    /// <summary>
    ///     获取/设置每个频率点对应的超过门限的次数
    /// </summary>
    public int[] UpperTimes { get; set; }

    /// <summary>
    ///     获取/设置扫描数据 key:时间　　value:float[](扫描数据)
    /// </summary>
    public ConcurrentQueue<KeyValuePair<DateTime, float[]>> ScanData { get; set; }

    /// <summary>
    ///     获取/设置门限数据
    /// </summary>
    public ConcurrentQueue<float[]> ThresholdData { get; set; }

    /// <summary>
    ///     噪声数据
    /// </summary>
    public ConcurrentQueue<float[]> NoiseData { get; private set; }

    /// <summary>
    ///     获取/设置当前占用度
    /// </summary>
    public double[] Occupancy { get; set; }

    /// <summary>
    ///     获取信号比对结果
    /// </summary>
    public SignalStationDescription SsDescription { get; } = null;

    /// <summary>
    ///     获取信噪比
    /// </summary>
    public float[] Snr { get; }

    /// <summary>
    ///     获取噪声谱
    /// </summary>
    public float[] Noise
    {
        get
        {
            if (_noise == null) return null;
            var result = new float[_noise.Length];
            for (var i = 0; i < _noise.Length; i++) result[i] = (float)_noise[i] / _totalCount;
            return result;
        }
    }

    #endregion

    #region 方法

    /// <summary>
    ///     清除
    /// </summary>
    public void Stop()
    {
        Running = false;
        _cts?.Cancel();
        try
        {
            _processTask?.Wait();
            _processTask?.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            _cts?.Dispose();
        }

        ScanData?.Clear();
        ScanData = null;
        ThresholdData?.Clear();
        ThresholdData = null;
        NoiseData?.Clear();
        NoiseData = null;
        FreqNumbers = 0;
        ScanTimes = 0;
        UpperTimes = null;
        _totalCount = 0;
    }

    /// <summary>
    ///     重置统计
    /// </summary>
    public void ResetStat()
    {
        ScanData.Clear();
        ThresholdData.Clear();
        NoiseData.Clear();
    }

    /// <summary>
    ///     添加数据
    /// </summary>
    /// <param name="scanData">扫描数据</param>
    /// <param name="thresholdData">门限</param>
    /// <param name="noise">噪声</param>
    /// <param name="measureTime">监测时间</param>
    public void AddDatas(float[] scanData, float[] thresholdData, float[] noise, DateTime measureTime)
    {
        ScanTimes++;
        ScanData?.Enqueue(new KeyValuePair<DateTime, float[]>(measureTime, scanData));
        ThresholdData?.Enqueue(thresholdData);
        NoiseData?.Enqueue(noise);
    }

    /// <summary>
    ///     计算占用度线程函数
    ///     这个线程里面计算，5秒钟才通知界面绘制一次
    ///     private DateTime _prevTime = DateTime.Now;
    /// </summary>
    private DateTime _prevTime = DateTime.MinValue;

    private async Task ProcessAsync(object obj)
    {
        var dicData = new KeyValuePair<DateTime, float[]>();
        float[] scanData = null;
        float[] threshold = null;
        float[] noise = null;
        var times = 0;
        //_preOccTime = DateTime.Now;
        _preOccTime = DateTime.MinValue;
        var token = (CancellationToken)obj;
        while (!token.IsCancellationRequested)
        {
            if (ScanData == null || ThresholdData == null) return;
            bool cal;
            var freqNumbers = FreqNumbers;
            if (ScanData.Count != ThresholdData.Count)
            {
                ScanData.Clear();
                ThresholdData.Clear();
                NoiseData.Clear();
            }

            if (!ScanData.IsEmpty && ScanData.TryDequeue(out dicData))
            {
                scanData = dicData.Value;
                if (!ThresholdData.IsEmpty) _ = ThresholdData.TryDequeue(out threshold);
                if (!NoiseData.IsEmpty) _ = NoiseData.TryDequeue(out noise);
                cal = true;
            }
            else
            {
                cal = false;
            }

            if (!cal)
            {
                await Task.Delay(10, token).ConfigureAwait(false);
                continue;
            }

            times++;
            _totalCount++;
            var occupancy = new List<double>();
            // 计算占用度
            for (var i = 0; i < freqNumbers; i++)
                try
                {
                    if (token.IsCancellationRequested) return;
                    //**************************jimbo 计算最大值及时间间隔最大值************************
                    //计算最大值
                    if (_maxValue[i] < scanData[i]) _maxValue[i] = scanData[i];
                    //计算时间间隔的最大值
                    if (_maxValueOccMin[i] < scanData[i]) _maxValueOccMin[i] = scanData[i];
                    if (noise != null && noise.Length > i) _noise[i] += noise[i];
                    //**********************************************************************************
                    // 超出自动门限以上3个dbuv视为信号
                    if (threshold != null && threshold.Length > i && scanData[i] > threshold[i])
                    {
                        UpperTimes[i]++;
                        PointScanStatInfo newPoint;
                        //***************jimbo 计算最大值和最大值时间********************
                        if (!_occPointInfo.ContainsKey(i))
                        {
                            newPoint = new PointScanStatInfo
                            {
                                Index = i,
                                MaxValue = (short)(scanData[i] * 10),
                                MaxValueTime = dicData.Key,
                                FirstFindTime = dicData.Key,
                                LastFindTime = dicData.Key,
                                //MaxValueTime = DateTime.Now,
                                //FirstFindTime = DateTime.Now,
                                //LastFindTime = DateTime.Now,
                                LevelTrack = new List<KeyValuePair<DateTime, short>>()
                            };
                            _occPointInfo.Add(i, newPoint);
                        }
                        else
                        {
                            newPoint = _occPointInfo[i];
                            if (newPoint.MaxValue < _maxValue[i])
                            {
                                newPoint.MaxValue = (short)(_maxValue[i] * 10);
                                newPoint.MaxValueTime = dicData.Key;
                                //newPoint.MaxValueTime = DateTime.Now;
                            }

                            newPoint.LastFindTime = dicData.Key;
                            //newPoint.LastFindTime = DateTime.Now;
                        }

                        if (SsDescription != null)
                        {
                            if (SsDescription.LstDescription[i] != "")
                            {
                                var stName = SsDescription.LstDescription[i];
                                newPoint.StationName = stName[..stName.IndexOf('(')];
                            }
                            else
                            {
                                newPoint.StationName = "";
                            }
                        }
                        else
                        {
                            newPoint.StationName = "";
                        }

                        //计算信噪比
                        var pointSnr = scanData[i] - threshold[i];
                        if (Snr[i] < pointSnr) Snr[i] = pointSnr;
                        //***************************************************************
                    }

                    var d = Math.Round((double)UpperTimes[i] / times * 100.0d, 1);
                    occupancy.Add(d);
                }
                catch
                {
                    // 不需要记录日志
                }

            Occupancy = occupancy.ToArray(); //jimbo赋值，以前这个属性变量没有使用
            var dtNow = dicData.Key;
            if (_prevTime == DateTime.MinValue) _prevTime = dicData.Key;
            if (_preOccTime == DateTime.MinValue) _preOccTime = dicData.Key;
            var diff1 = dtNow.Subtract(_prevTime);
            if (diff1.TotalSeconds > 5)
            {
                var arg = new OccupancyChangedEventArgs(occupancy, Snr);
                OnOccupancyChanged(null, arg);
                _prevTime = dtNow;
            }
        }
    }

    /// <summary>
    ///     StationInfomationChange 监测站信息发生变化,静态的不由运行状态改变的
    /// </summary>
    public event EventHandler<OccupancyChangedEventArgs> OccupancyChanged;

    private void OnOccupancyChanged(object sender, OccupancyChangedEventArgs e)
    {
        OccupancyChanged?.Invoke(sender, e);
    }

    #endregion
}

/// <summary>
///     StationInfomationChange 监测站信息发生变化事件参数
/// </summary>
public class OccupancyChangedEventArgs(List<double> occupancy, float[] snr) : EventArgs
{
    /// <summary>
    ///     占用度数据 单位 %
    /// </summary>
    public List<double> Occupancy { get; set; } = occupancy;

    /// <summary>
    ///     获取信噪比
    /// </summary>
    public float[] Snr { get; } = snr;
}

public class SignalStationDescription(List<Color> lstColor, List<string> lstDescription)
{
    public List<Color> LstColor { get; set; } = lstColor;
    public List<string> LstDescription { get; set; } = lstDescription;
}

public class PointScanStatInfo
{
    /// <summary>
    ///     频点在总频段中的序号
    /// </summary>
    public int Index { get; set; }

    public short AvgValue { get; set; }
    public DateTime FirstFindTime { get; set; }
    public DateTime LastFindTime { get; set; }
    public List<KeyValuePair<DateTime, short>> LevelTrack { get; set; }
    public short MaxValue { get; set; }
    public DateTime MaxValueTime { get; set; }
    public short NoiseValue { get; set; }
    public short OccValue { get; set; }
    public string StationName { get; set; }

    public PointScanStatInfo Clone()
    {
        return MemberwiseClone() as PointScanStatInfo;
    }
}

#endregion