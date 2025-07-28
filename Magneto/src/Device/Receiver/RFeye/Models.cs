using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye;

internal enum FilterWindow
{
    Rectangle,
    Hanning,
    Blackman
}

internal struct ScanFreqInfo
{
    public readonly double Frequency;
    public readonly double IfBandWidth;
    public readonly Modulation Demodulation;

    public ScanFreqInfo(double freq, double ifBw, Modulation demodulation)
    {
        Frequency = freq;
        IfBandWidth = ifBw;
        Demodulation = demodulation;
    }
}

/// <summary>
///     设备返回的实时IQ数据的相关信息
/// </summary>
internal class TimeDataInfo
{
    /// <summary>
    ///     RF attenuation
    /// </summary>
    public int AgcAtten;

    /// <summary>
    ///     IQ数据中的I分量
    /// </summary>
    public double[] DataI;

    /// <summary>
    ///     IQ数据中的Q分量
    /// </summary>
    public double[] DataQ;

    /// <summary>
    ///     错误码
    /// </summary>
    public int ErrorCode;

    /// <summary>
    ///     时间戳（单位nano）
    /// </summary>
    public int Nano;

    /// <summary>
    ///     中心频率
    /// </summary>
    public double RealFrequency;

    /// <summary>
    ///     采样率
    /// </summary>
    public double Samplerate;

    /// <summary>
    ///     计算电平的参数
    /// </summary>
    public double Scale;

    /// <summary>
    ///     时间戳（单位s）
    /// </summary>
    public int Utim;

    /// <summary>
    ///     警告码
    /// </summary>
    public int WarningCode;
}

/// <summary>
///     设备返回的扫描数据相关信息
/// </summary>
internal class SweepDataInfo
{
    /// <summary>
    ///     原始频谱(未经过参考电平转换)
    /// </summary>
    public long[] Data;

    /// <summary>
    ///     分辨率带宽
    /// </summary>
    public int RealResBwHz;

    /// <summary>
    ///     起始频率
    /// </summary>
    public double RealStartFrequency;

    /// <summary>
    ///     结束频率
    /// </summary>
    public double RealStopFrequency;

    /// <summary>
    ///     参考电平
    /// </summary>
    public int RefLevel;
}

/// <summary>
///     设备返回的解调数据相关信息
/// </summary>
internal class DemodDataInfo
{
    /// <summary>
    ///     音频数据
    /// </summary>
    public byte[] DataAudio;

    /// <summary>
    ///     以RadioTuneFreqMHz为中心频率的20M带宽的频谱数据
    /// </summary>
    public long[] DataSpec;

    /// <summary>
    ///     中心频率小数据部分
    /// </summary>
    public int DdsFreqKHz;

    /// <summary>
    ///     中心频率整数部分
    /// </summary>
    public int RadioTuneFreqMHz;

    /// <summary>
    ///     采样率（40/DECI/DDEC）MHz
    /// </summary>
    public double Samplerate;
}

internal class Itu : IDisposable
{
    #region IDisposable

    public void Dispose()
    {
        if (_ituCalcTask == null) return;
        if (!_ituCalcTask.IsCompleted && _ituTokenSource != null)
            try
            {
                _ituTokenSource.Cancel();
            }
            catch
            {
            }
            finally
            {
                _ituTokenSource.Dispose();
            }

        _ituCalcTask.Dispose();
    }

    #endregion

    #region 导入函数

    /// <summary>
    ///     TODO: 实现ITU算法库
    ///     测量ITU
    /// </summary>
    /// <param name="arrIq">IQ数组</param>
    /// <param name="length">数据长度</param>
    /// <param name="fs">信号采样率（单位Hz）</param>
    /// <param name="xdB">xdB带宽选择（范围[3,60]，默认26dB）</param>
    /// <param name="beta">beta带宽选择（暂定固定输入1）</param>
    /// <param name="fcc">接收机当前调谐频率（单位Hz）</param>
    /// <param name="detaF"></param>
    /// <param name="ituResult">ITU测量结果</param>
    [DllImport("ITU.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool MeasureITU(short[] arrIq, int length, int fs, int xdB, int beta, double fcc, int detaF,
        ref ItuResult ituResult);

    #endregion

    #region 内部类

    private struct OriginalData
    {
        /// <summary>
        ///     ID为本次测量状态的唯一标识，任何参数的改变都会导致该值的改变（递增）
        ///     可根据比较该值来判断当前计算的最新结果是否为当前的测量状态下的，以此来丢弃旧数据
        /// </summary>
        public long Id;

        /// <summary>
        ///     接收机当前调谐频率（单位Hz）
        /// </summary>
        public readonly double CenterFreq;

        /// <summary>
        ///     信号采样率（单位Hz）
        /// </summary>
        public readonly int SampleRate;

        /// <summary>
        ///     xdB带宽选择（范围[3,60]，默认26dB）
        /// </summary>
        public readonly int XdB;

        /// <summary>
        ///     beta带宽选择（暂定固定输入1）
        /// </summary>
        public readonly int Beta;

        /// <summary>
        ///     频率测量容限（单位：Hz,暂定固定输入5000）
        /// </summary>
        public readonly int Tolerance;

        /// <summary>
        ///     待计算的IQ数据
        /// </summary>
        public readonly short[] DataIq;

        public OriginalData(OriginalData other)
        {
            Id = other.Id;
            CenterFreq = other.CenterFreq;
            SampleRate = other.SampleRate;
            XdB = other.XdB;
            Beta = other.Beta;
            Tolerance = other.Tolerance;
            DataIq = new short[other.DataIq.Length]; //由调用者保证此处不为null,不用再做判断
            Array.Copy(other.DataIq, DataIq, other.DataIq.Length);
        }

        public OriginalData(long id, double centerFreq, int samplerate, int xdb, int beta, int tolerance,
            short[] dataIq)
        {
            Id = id;
            CenterFreq = centerFreq;
            SampleRate = samplerate;
            XdB = xdb;
            Beta = beta;
            Tolerance = tolerance;
            DataIq = new short[dataIq.Length];
            Array.Copy(dataIq, DataIq, dataIq.Length);
        }
    }

    #endregion

    #region 成员变量

    /// <summary>
    ///     当前ITU测量结果对应的采集状态
    /// </summary>
    private long _id;

    /// <summary>
    ///     当前最新计算出的ITU测量结果
    /// </summary>
    private ItuResult _ituResult;

    /// <summary>
    ///     当前正在计算的原始数据
    /// </summary>
    private OriginalData _dataCurr;

    /// <summary>
    ///     待处理的原始数据
    /// </summary>
    private OriginalData _dataNext;

    /// <summary>
    ///     对_dataNext和_ituResult进行锁保护
    /// </summary>
    private readonly object _dataLock = new();

    /// <summary>
    ///     ITU计算线程
    /// </summary>
    private Task _ituCalcTask;

    private CancellationTokenSource _ituTokenSource;

    #endregion

    #region 成员函数

    /// <summary>
    ///     启动ITU计算线程
    /// </summary>
    public void Initialize()
    {
        if (_ituCalcTask?.IsCompleted == false && _ituTokenSource != null)
            try
            {
                _ituTokenSource.Cancel();
            }
            catch
            {
            }
            finally
            {
                _ituTokenSource.Dispose();
            }

        _ituTokenSource = new CancellationTokenSource();
        _ituCalcTask = new Task(ItuMeasureProc, _ituTokenSource.Token);
        _ituCalcTask.Start();
    }

    /// <summary>
    ///     ITU测量
    /// </summary>
    /// <param name="id">当前设备参数状态标识</param>
    /// <param name="centerFreq"></param>
    /// <param name="samplerate"></param>
    /// <param name="xdb"></param>
    /// <param name="beta"></param>
    /// <param name="tolerance"></param>
    /// <param name="dataIq"></param>
    /// <param name="ituResult"></param>
    public bool MeasureItu(long id, double centerFreq, int samplerate, int xdb, int beta, int tolerance, short[] dataIq,
        ref ItuResult ituResult)
    {
        var result = false;
        lock (_dataLock)
        {
            //id相同，表示当前计算的最新结果即为当前测量状态下的，拷贝数据并返回
            if (id == _id)
            {
                ituResult = _ituResult;
                result = true;
            }

            _dataNext = new OriginalData(id, centerFreq, samplerate, xdb, beta, tolerance, dataIq);
        }

        return result;
    }

    /// <summary>
    ///     ITU计算线程
    /// </summary>
    private void ItuMeasureProc()
    {
        while (!_ituTokenSource.IsCancellationRequested)
        {
            //表示_dataCurr为有效数据可进行运算
            var isValid = false;
            lock (_dataLock)
            {
                //_dataNext.ID != 0表示当前有待处理的数据，则将该数据赋值给处理缓存
                if (_dataNext.Id != 0)
                {
                    _dataCurr = new OriginalData(_dataNext);
                    _dataNext.Id = 0;
                    isValid = true;
                }
            }

            if (!isValid)
            {
                //当前没有待处理的数据，释放时间片，避免占用系统资源
                Thread.Sleep(1);
                continue;
            }

            var ituResultTemp = new ItuResult();
            if (MeasureITU(_dataCurr.DataIq, _dataCurr.DataIq.Length, _dataCurr.SampleRate, _dataCurr.XdB,
                    _dataCurr.Beta, _dataCurr.CenterFreq, _dataCurr.Tolerance, ref ituResultTemp))
                lock (_dataLock)
                {
                    //保存最新计算结果
                    _ituResult = ituResultTemp;
                    _id = _dataCurr.Id;
                }

            Thread.Sleep(3000);
        }
    }

    #endregion
}

/// <summary>
///     ITU.dll动态库ITU计算结果输出结构体
/// </summary>
internal struct ItuResult
{
    /// <summary>
    ///     功率谱密度上的β带宽（单位：Hz）
    /// </summary>
    public double FBetaBwPsd;

    /// <summary>
    ///     功率谱密度上的xdb带宽（单位：Hz）
    /// </summary>
    public double FXdBBwPsd;

    /// <summary>
    ///     中心频率（单位：Hz）
    /// </summary>
    public double FFreq;

    /// <summary>
    ///     AM调制度，取值范围0~1，如调制度为50%则为0.5 (0 %-100 %)
    /// </summary>
    public double FAmMod;

    /// <summary>
    ///     FM最大频偏(单位：Hz)
    /// </summary>
    public double FFmMod;

    /// <summary>
    ///     FM正向调制深度(单位：Hz)
    /// </summary>
    public double FFmPos;

    /// <summary>
    ///     FM负向调制深度(单位：Hz)
    /// </summary>
    public double FFmNeg;

    /// <summary>
    ///     PM最大调制相偏(单位：rad)
    /// </summary>
    public double FPmMod;
}