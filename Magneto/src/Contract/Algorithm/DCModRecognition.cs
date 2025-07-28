using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     德辰调制识别算法类
/// </summary>
public sealed class DcModRecognition
{
    private const double Pi = Math.PI;
    private const double ThActiveRatio = 0.707d; // 0.9d;
    private const double ThActiveReRatio = 0.5d; // 0.9d * 0.707106781186547d;//0.707106781186547=1/Math.Sqrt(2);
    private const double ThPeakBottomRatio = 1.0d;
    private const double ThPeakTopRatio = 0.50118d; // 0.3d;
    private static readonly List<double> _vCumF1 = [];
    private static readonly List<double> _vCumF2 = [];
    private static readonly List<double> _vDeltaAf = [];
    private static readonly List<double> _vDeltaAp = [];
    private static readonly List<double> _vDeltaDa = [];
    private static readonly List<double> _vGammaMax = [];

    private static readonly List<double> _vMiuF42 = [];

    //private System.Threading.Thread fcThread = null;
    // <sample,<paramsName,value>>
    /// <summary>
    ///     不同采样率对应的特征参数门限键值对
    /// </summary>
    private static readonly Dictionary<string, Dictionary<string, double>> _dicConstThresholds = new();

    /// <summary>
    ///     实际测试过的支持常用带宽下的采样率列表：[25.6MHz，19.2MHz，12.8MHz，6.4MHz，3.2MHz，1.28MHz，640kHz，320kHz，128kHz，64kHz，32kHz，16kHz，8kHz]
    ///     25.6d, 19.2d, 12.8d, 6.4d, 3.2d, 1.28d, 0.64d, 0.32d, 0.128d, 0.064d, 0.032d, 0.016d, 0.008d
    ///     "tfps", "onpt", "otpe", "spf", "tpt", "opte", "zpsf", "zptt", "zpote", "zpzsf", "zpztt", "zpzos", "zpzze"
    /// </summary>
    private static readonly double[] _sampleRates = [51.2d, 12.8d, 6.4d, 3.2d, 1.28d, 0.8d, 0.32d, 0.128d, 0.064d];

    private static readonly string[] _sampleRatesStrs =
        ["fopt", "otpe", "spf", "tpt", "opte", "zpe", "zptt", "zpote", "zpzsf"];

    /// <summary>
    ///     π/4旋转因子，进行复数旋转，避免I,或Q分量大小比例失衡导致的特征参数计算NaN
    /// </summary>
    private static readonly Complex _rotationFactor = new(Math.Cos(Math.PI / 4), Math.Sin(Math.PI / 4));

    // static dictionary added
    /// <summary>
    ///     特征参数门限及其对应的门限值 51.2   51.2MHz
    /// </summary>
    private static readonly Dictionary<string, double> _fopt = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 12.80   12.8MHz
    /// </summary>
    private static readonly Dictionary<string, double> _otpe = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 6.40   6.4MHz
    /// </summary>
    private static readonly Dictionary<string, double> _spf = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 3.20   3.2MHz
    /// </summary>
    private static readonly Dictionary<string, double> _tpt = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 1.280   1.28MHz
    /// </summary>
    private static readonly Dictionary<string, double> _opte = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 0.800   0.8MHz
    /// </summary>
    private static readonly Dictionary<string, double> _zpe = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 0.320   320kHz
    /// </summary>
    private static readonly Dictionary<string, double> _zptt = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 0.128   128kHz
    /// </summary>
    private static readonly Dictionary<string, double> _zpote = new();

    /// <summary>
    ///     特征参数门限及其对应的门限值 0.064   64kHz
    /// </summary>
    private static readonly Dictionary<string, double> _zpzsf = new();

    private static DcModRecognition _modFeatures;

    /// <summary>
    ///     是否是测试 只供内部测试用,true:直接返回识别类型；false:加入频段筛选
    /// </summary>
    private readonly bool _isTest = true;

    /// <summary>
    ///     当前采样率与提供的采样率的差集，以此来寻找最接近的采样率门限列表
    /// </summary>
    private readonly double[] _minusSampleRate = new double[_sampleRates.Length];

    /// <summary>
    ///     识别率大于0的调制方式对应的相关业务信息
    /// </summary>
    private readonly Dictionary<ModulationType, string> _modulationInfo = new();

    /// <summary>
    ///     调制识别识别率
    /// </summary>
    private readonly SortedList<ModulationType, double> _modulationResult = new();

    /// <summary>
    ///     调制识别识别率统计
    /// </summary>
    private readonly SortedList<ModulationType, double> _modulationStatistics = new();

    private double[] _acn;
    private double _biasCumF1;
    private double _biasCumF2;
    private double _biasDeltaAf;
    private double _biasDeltaAp;
    private double _biasDeltaDa;
    private double _biasGammaMax;
    private double _biasMiuF42;
    private int _cntFreqNorm;

    /// <summary>
    ///     判断是否存在信号
    /// </summary>
    //private double signalExist = 0;
    private double _cumF1;

    private double _cumF2;

    /// <summary>
    ///     动态加载后的当前门限值字典序列
    /// </summary>
    private Dictionary<string, double> _currentThreshold;

    private Complex[] _dataFft;
    private int _dataLen;

    /// <summary>
    ///     零中心归一化瞬时幅度绝对值的标准偏差
    /// </summary>
    private double _deltaAa; // 1/N*(ΣXcn^2)/(1/N*(Σabs(Xcn)))^2 归一化瞬时幅度的平方和的均值与归一化瞬时幅度的绝对值和的均值的平方的比值

    /// <summary>
    ///     零中心归一化非弱信号段瞬时频率绝对值的标准偏差
    /// </summary>
    private double _deltaAf; // 1/N*(Σf^2)/(1/N*(Σabs(f)))^2  瞬时频率的平方和的均值与瞬时频率的绝对值之和的均值的平方的比值

    /// <summary>
    ///     零中心非弱信号段瞬时相位非线性分量的绝对值的标准偏差
    /// </summary>
    private double _deltaAp; // sqrt(1/N*(ΣX^2)-(1/NΣabs(X))^2)  平方和的均值与绝对值和的平均的平方的差开根号 *

    /// <summary>
    ///     零中心归一化非弱信号段瞬时幅度的标准偏差
    /// </summary>
    private double
        _deltaDa; // no weak signal segments Xcn(>threshold) 1/N*(ΣXcn^2)/(1/N*(ΣXcn))^2  非弱信号段的瞬时幅度平方和的均值与实际值的和的平均值的平方的比

    /// <summary>
    ///     零中心非弱信号段瞬时相位非线性分量的标准偏差
    /// </summary>
    private double _deltaDp; // sqrt(1/N*(ΣX^2)-(1/NΣX)^2)  平方和的均值与实际值和的平均的平方的差开根号

    private double[] _freqInst;
    private double[] _freqNorm;

    /// <summary>
    ///     中心频率，判断信号所在频段加以辅助性的频段内区分
    /// </summary>
    private double _frequency;

    /// <summary>
    ///     中频采样率
    /// </summary>
    private double _fs;

    /// <summary>
    ///     零中心归一化瞬时幅度谱密度最大值
    /// </summary>
    private double _gammaMax; // max(fft(X)^2/N) 平均功率谱密度的最大值

    /// <summary>
    ///     外部传入的IQ数据整合后对应的复数集合
    /// </summary>
    private Complex[] _localData;

    /// <summary>
    ///     零中心归一化瞬时幅度的紧致性E[Xcn^4]/E[Xcn^2]^2
    /// </summary>
    private double _miuA42; //  Xcn=X/mean(X)-1 ; Ns*ΣXcn^4/(ΣXcn^2)^2 归一化瞬时幅度四次方的和与归一化瞬时幅度平方和的平方的比值

    /// <summary>
    ///     零中心归一化瞬时频率的紧致性E[Fcn^4]/E[Fcn^2]^2
    /// </summary>
    private double _miuF42; // 1/N*(Σf^4)/(Σf^2)^2 瞬时频率四次方的和的均值与瞬时频率平方和的平方的比值 *

    private ModulationType _modType;
    private double _p;

    /// <summary>
    ///     主谱峰的个数
    /// </summary>
    private int _peakNum;

    private double[] _phiAll;
    private double[] _phiNl;

    /// <summary>
    ///     载波估计的时间间隔
    /// </summary>
    private long _prevFcEstimateTick = DateTime.Now.Ticks;

    ///////////////////////////////////统计结果与输出结果////////////////////////////////////////
    /// <summary>
    ///     估计码元速率时间间隔
    /// </summary>
    private long _prevSymbolEstimateTick = DateTime.Now.Ticks;

    private double _thActive;
    private double _thActiveRe;
    private double _thPeakBottom;
    private double _thPeakTop;

    static DcModRecognition()
    {
        //deltaDp biasDeltaAp biasCumF1 biasGammaMax    deltaAa biasCumF1   miuF42 peakNum deltaAf gammaMax    miuA42

        #region static load thresholds on samplerate /***12.8MHz,1.28MHz,320kHz,128kHz,64kHz priority***/

        #region initialize and load thresholds

        #region 51.2 51.2MHz

        {
            _fopt.Add("deltaDp", 1.1d);
            _fopt.Add("biasDeltaAp", 0.75d);
            _fopt.Add("biasCumF1", 0.6d);
            _fopt.Add("biasCumF2", 0.9d);
            _fopt.Add("biasGammaMax", 2.5d);
            _fopt.Add("deltaAa", 0.21d);
            _fopt.Add("biasDeltaAa", 0.45d);
            _fopt.Add("biasDeltaDa", 0.57d);
            _fopt.Add("biasDeltaDaCw", 0.64d);
            _fopt.Add("miuF42", 3.2d);
            _fopt.Add("peakNum", 2.0d);
            _fopt.Add("biasDeltaAf", 0.082d);
            _fopt.Add("deltaAf", 0.00724d);
            _fopt.Add("gammaMax", 6.0d);
            _fopt.Add("miuA42", 3.2d);
        }

        #endregion 51.2 51.2MHz

        #region 12.8 12.8MHz

        {
            _otpe.Add("deltaDp", 1.1d);
            _otpe.Add("biasDeltaAp", 0.6d);
            _otpe.Add("biasCumF1", 0.95d);
            _otpe.Add("biasCumF2", 0.16d);
            _otpe.Add("biasGammaMax", 2.0d);
            _otpe.Add("deltaAa", 0.21d);
            _otpe.Add("biasDeltaAa", 0.45d);
            _otpe.Add("biasDeltaDa", 0.45d);
            _otpe.Add("biasDeltaDaCw", 0.49d);
            _otpe.Add("miuF42", 2.05d);
            _otpe.Add("peakNum", 2.0d);
            _otpe.Add("biasDeltaAf", 0.0382d);
            _otpe.Add("deltaAf", 0.001515d);
            _otpe.Add("gammaMax", 11.0d);
            _otpe.Add("miuA42", 2.4d);
        }

        #endregion 12.8 12.8MHz

        #region 6.4 6.4MHz

        {
            _spf.Add("deltaDp", 1.3d);
            _spf.Add("biasDeltaAp", 0.6d);
            _spf.Add("biasCumF1", 0.60d);
            _spf.Add("biasCumF2", 0.95d);
            _spf.Add("biasGammaMax", 4.0d);
            _spf.Add("deltaAa", 0.212d);
            _spf.Add("biasDeltaAa", 0.46d);
            _spf.Add("biasDeltaDa", 0.46d);
            _spf.Add("biasDeltaDaCw", 0.40d);
            _spf.Add("miuF42", 80d);
            _spf.Add("peakNum", 2.0d);
            _spf.Add("biasDeltaAf", 0.006d);
            _spf.Add("deltaAf", 0.006d);
            _spf.Add("gammaMax", 10.0d);
            _spf.Add("miuA42", 2.5d);
        }

        #endregion 6.4 6.4MHz

        #region 3.2 3.2MHz

        {
            _tpt.Add("deltaDp", 1.3d);
            _tpt.Add("biasDeltaAp", 0.7d);
            _tpt.Add("biasCumF1", 0.60d);
            _tpt.Add("biasCumF2", 0.95d);
            _tpt.Add("biasGammaMax", 2.5d);
            _tpt.Add("deltaAa", 0.212d);
            _tpt.Add("biasDeltaAa", 0.46d);
            _tpt.Add("biasDeltaDa", 0.49d);
            _tpt.Add("biasDeltaDaCw", 0.49d);
            _tpt.Add("miuF42", 2.05d);
            _tpt.Add("peakNum", 2.0d);
            _tpt.Add("biasDeltaAf", 0.05d);
            _tpt.Add("deltaAf", 0.0025d);
            _tpt.Add("gammaMax", 11.0d);
            _tpt.Add("miuA42", 2.4d);
        }

        #endregion 3.2 3.2MHz

        #region 1.28 1.28MHz

        {
            _opte.Add("deltaDp", 1.3d);
            _opte.Add("biasDeltaAp", 0.6d);
            _opte.Add("biasCumF1", 0.16d);
            _opte.Add("biasCumF2", 0.95d);
            _opte.Add("biasGammaMax", 2.0d);
            _opte.Add("deltaAa", 0.21d);
            _opte.Add("biasDeltaAa", 0.45d);
            _opte.Add("biasDeltaDa", 0.45d);
            _opte.Add("biasDeltaDaCw", 0.30d);
            _opte.Add("miuF42", 2.05d);
            _opte.Add("peakNum", 2.0d);
            _opte.Add("biasDeltaAf", 0.0382d);
            _opte.Add("deltaAf", 0.001515d);
            _opte.Add("gammaMax", 11.0d);
            _opte.Add("miuA42", 2.4d);
        }

        #endregion 1.28 1.28MHz

        #region 0.80 800kHz

        {
            _zpe.Add("deltaDp", 1.2d);
            _zpe.Add("biasDeltaAp", 0.65d);
            _zpe.Add("biasCumF1", 0.40d);
            _zpe.Add("biasCumF2", 0.80d);
            _zpe.Add("biasGammaMax", 4.50d);
            _zpe.Add("deltaAa", 0.206d);
            _zpe.Add("biasDeltaAa", 0.44d);
            _zpe.Add("biasDeltaDa", 0.45d);
            _zpe.Add("biasDeltaDaCw", 0.30d);
            _zpe.Add("miuF42", 2.8);
            _zpe.Add("peakNum", 2);
            _zpe.Add("biasDeltaAf", 0.017d);
            _zpe.Add("deltaAf", 0.0025d);
            _zpe.Add("gammaMax", 25);
            _zpe.Add("miuA42", 2.5d);
        }

        #endregion

        #region 0.32 320kHz

        {
            _zptt.Add("deltaDp", 1.1d);
            _zptt.Add("biasDeltaAp", 0.6d);
            _zptt.Add("biasCumF1", 0.51d);
            _zptt.Add("biasCumF2", 0.95d);
            _zptt.Add("biasGammaMax", 3.0d);
            _zptt.Add("deltaAa", 0.195d);
            _zptt.Add("biasDeltaAa", 0.42d);
            _zptt.Add("biasDeltaDa", 0.46d);
            _zptt.Add("biasDeltaDaCw", 0.45d);
            _zptt.Add("miuF42", 2.2d);
            _zptt.Add("peakNum", 2);
            _zptt.Add("biasDeltaAf", 0.045d);
            _zptt.Add("deltaAf", 0.0023d);
            _zptt.Add("gammaMax", 11.0d);
            _zptt.Add("miuA42", 2.4d);
        }

        #endregion

        #region 0.128 128kHz

        {
            _zpote.Add("deltaDp", 1.1d);
            _zpote.Add("biasDeltaAp", 0.88d);
            _zpote.Add("biasCumF1", 0.51d);
            _zpote.Add("biasCumF2", 0.70d);
            _zpote.Add("biasGammaMax", 3.0d);
            _zpote.Add("deltaAa", 0.195d);
            _zpote.Add("biasDeltaAa", 0.432d);
            _zpote.Add("biasDeltaDa", 0.45d);
            _zpote.Add("biasDeltaDaCw", 0.45d);
            _zpote.Add("miuF42", 2.05d);
            _zpote.Add("peakNum", 2);
            _zpote.Add("biasDeltaAf", 0.062d);
            _zpote.Add("deltaAf", 4.0d);
            _zpote.Add("gammaMax", 11.0d);
            _zpote.Add("miuA42", 2.4d);
        }

        #endregion

        #region 0.064 64kHz

        {
            _zpzsf.Add("deltaDp", 1.6d);
            _zpzsf.Add("biasDeltaAp", 0.855d);
            _zpzsf.Add("biasCumF1", 0.51d);
            _zpzsf.Add("biasCumF2", 0.56d);
            _zpzsf.Add("biasGammaMax", 4.0d);
            _zpzsf.Add("deltaAa", 0.2125d);
            _zpzsf.Add("biasDeltaAa", 0.45d);
            _zpzsf.Add("biasDeltaDa", 0.45d);
            _zpzsf.Add("biasDeltaDaCw", 0.42d);
            _zpzsf.Add("miuF42", 3.0d);
            _zpzsf.Add("peakNum", 2);
            _zpzsf.Add("biasDeltaAf", 0.055d);
            _zpzsf.Add("deltaAf", 0.003d);
            _zpzsf.Add("gammaMax", 11.0d);
            _zpzsf.Add("miuA42", 2.4d);
        }

        #endregion

        _dicConstThresholds.TryAdd("fopt", _fopt);
        _dicConstThresholds.TryAdd("otpe", _otpe);
        _dicConstThresholds.TryAdd("spf", _spf);
        _dicConstThresholds.TryAdd("tpt", _tpt);
        _dicConstThresholds.TryAdd("opte", _opte);
        _dicConstThresholds.TryAdd("zpe", _zpe);
        _dicConstThresholds.TryAdd("zptt", _zptt);
        _dicConstThresholds.TryAdd("zpote", _zpote);
        _dicConstThresholds.TryAdd("zpzsf", _zpzsf);

        #endregion

        #endregion
    }

    private DcModRecognition()
    {
    }

    /// <summary>
    ///     估计的载波频率
    /// </summary>
    public double Fc { get; private set; }

    public double SymbolRate { get; private set; }

    /// <summary>
    ///     状态变量重置
    /// </summary>
    private void Reset()
    {
        _deltaDp = 0.0d;
        _deltaAp = 0.0d;
        _gammaMax = 0.0d;
        _miuA42 = 0.0d;
        _deltaAa = 0.0d;
        _deltaDa = 0.0d;
        _deltaAf = 0.0d;
        _miuF42 = 0.0d;
        _cumF1 = 0.0d;
        _cumF2 = 0.0d;
        _peakNum = 0;
        _thActive = 0.0d;
        _thActiveRe = 0.0d;
        _cntFreqNorm = 0;
        _thPeakBottom = 0.0d;
        _thPeakTop = 0.0d;
        _biasCumF1 = 0.0d;
        _biasCumF2 = 0.0d;
        _biasDeltaAf = 0.0d;
        _biasDeltaAp = 0.0d;
        _biasDeltaDa = 0.0d;
        _biasGammaMax = 0.0d;
        _biasMiuF42 = 0.0d;
    }

    /// <summary>
    ///     返回自动调制识别相关信息集合，[0]:[识别类型 识别率],[1]:[识别类型，可能的业务信息]，采样率必须为MHz单位
    /// </summary>
    /// <param name="iData">输入中频信号同相分量</param>
    /// <param name="qData">输入中频信号正交分量</param>
    /// <param name="sampleRate">中频采样率，必须为MHz单位</param>
    /// <param name="frequency">当前信号中心频率</param>
    public List<object> StatisticsTimes(short[] iData, short[] qData, double sampleRate, double frequency)
    {
        var result = new List<object>();
        try
        {
            var tempMod = ModulationType.None;
            _modFeatures.Reset();
            _modFeatures.InitializeData(iData, qData, sampleRate, frequency);
            _modFeatures.Extract();
            _modFeatures.ClassifierDecisionTreeDic();
            // just for test
            //ft.ClassifierDecisionTree();
            //ft.PrintFeature();
            var dicMod = from d in _modulationResult
                         orderby d.Value descending
                         select d.Key;
            var modulationTypes = dicMod as ModulationType[] ?? dicMod.ToArray();
            if (modulationTypes.Any()) tempMod = modulationTypes.ToList()[0];
            // 当调制方式为数字调制且时间间隔2s以上时才估计码元速率
            if (TimeSpan.FromTicks(DateTime.Now.Ticks - _prevSymbolEstimateTick).TotalSeconds > 2.0 &&
                tempMod != ModulationType.None && IsDigitalModulation(tempMod))
            {
                _prevSymbolEstimateTick = DateTime.Now.Ticks;
                var c = new Complex[iData.Length];
                for (var i = 0; i < c.Length; i++) c[i] = new Complex(iData[i], qData[i]);
                SymbolRate = SignalProcess.SymbolRateEstimate(c, sampleRate);
                //double fc = SignalProcess.FcEstimate(iData, qData, iqData.SampleRate);
            }
        }
        catch
        {
            // 由于傅里叶点数不满足2的整数幂导致运算错误【只有切换中频带宽导致采样率变化，数据错误可能】，暂时不做补零措施
            //System.Diagnostics.Debug.WriteLine(ex);
        }

        try
        {
            // 估计载波频率进行参数P的计算，区分LSB USB
            if (TimeSpan.FromTicks(DateTime.Now.Ticks - _prevFcEstimateTick).TotalSeconds > 3.0)
            {
                _prevFcEstimateTick = DateTime.Now.Ticks;
                Fc = SignalProcess.FcEstimate(_localData, _fs);
            }
        }
        catch
        {
            // 忽略异常
        }

        var demodulation = _modFeatures.GetModType();
        if (_modulationStatistics.ContainsKey(demodulation) && demodulation != ModulationType.None)
            _modulationStatistics[demodulation]++;
        // 统计每种调制方式的识别百分比，即信号识别率
        var sumTimes = _modulationStatistics.Values.Sum();
        if (sumTimes != 0)
        {
            var keys = _modulationStatistics.Keys.ToList();
            foreach (var item in keys)
            {
                _modulationResult[item] = _modulationStatistics[item] / sumTimes;
                // 当统计的信号识别率大于0时才加入相关可能的业务信息内容
                if (!_modulationInfo.ContainsKey(item) && _modulationResult[item] > 0)
                    _modulationInfo.Add(item, _modFeatures.GetBusinessInfo(item));
                // 当信号识别率等于0时将之前的业务信息删除
                if (_modulationInfo.ContainsKey(item) && !string.IsNullOrEmpty(_modulationInfo[item]) &&
                    _modulationResult[item] == 0) _modulationInfo.Remove(item);
            }
        }

        result.Add(_modulationResult);
        result.Add(_modulationInfo);
        result.Add(Fc);
        result.Add(SymbolRate);
        return result;
    }

    /// <summary>
    ///     统计识别率，采样率必须为MHz单位
    /// </summary>
    /// <param name="iData">同相分量</param>
    /// <param name="qData">正交分量</param>
    /// <param name="sampleRate">采样率，必须为MHz单位</param>
    /// <param name="frequency">中心频率</param>
    public List<object> StatisticsTimes(int[] iData, int[] qData, double sampleRate, double frequency)
    {
        var result = new List<object>();
        try
        {
            var tempMod = ModulationType.None;
            _modFeatures.Reset();
            _modFeatures.InitializeData(iData, qData, sampleRate, frequency);
            _modFeatures.Extract();
            _modFeatures.ClassifierDecisionTreeDic();
            // just for test
            //ft.ClassifierDecisionTree();
            //ft.PrintFeature();
            var dicMod = from d in _modulationResult
                         orderby d.Value descending
                         select d.Key;
            var modulationTypes = dicMod as ModulationType[] ?? dicMod.ToArray();
            if (modulationTypes.Any()) tempMod = modulationTypes.ToList()[0];
            // 当调制方式为数字调制且时间间隔2s以上时才估计码元速率
            if (TimeSpan.FromTicks(DateTime.Now.Ticks - _prevSymbolEstimateTick).TotalSeconds > 1.0 &&
                tempMod != ModulationType.None && IsDigitalModulation(tempMod))
            {
                _prevSymbolEstimateTick = DateTime.Now.Ticks;
                var c = new Complex[iData.Length];
                for (var i = 0; i < c.Length; i++) c[i] = new Complex(iData[i], qData[i]);
                SymbolRate = SignalProcess.SymbolRateEstimate(c, sampleRate);
            }
        }
        catch
        {
            // 由于傅里叶点数不满足2的整数幂导致运算错误【只有切换中频带宽导致采样率变化，数据错误可能】，暂时不做补零措施
            //System.Diagnostics.Debug.WriteLine(ex);
        }

        try
        {
            // 估计载波频率进行参数P的计算，区分LSB USB
            if (TimeSpan.FromTicks(DateTime.Now.Ticks - _prevFcEstimateTick).TotalSeconds > 1.5)
            {
                _prevFcEstimateTick = DateTime.Now.Ticks;
                //if ((modType & (ModulationType.AM | ModulationType.AM_SCDSB | ModulationType.AM_SSB_LSB | ModulationType.AM_SSB_USB)) > 0)
                //    fc = SignalProcess.FcEstimateTime(localData, fs);
                //else
                Fc = SignalProcess.FcEstimate(_localData, _fs);
            }
        }
        catch
        {
            // 忽略异常
        }

        var demodulation = _modFeatures.GetModType();
        if (_modulationStatistics.ContainsKey(demodulation) && demodulation != ModulationType.None)
            _modulationStatistics[demodulation]++;
        // 统计每种调制方式的识别百分比，即信号识别率
        var sumTimes = _modulationStatistics.Values.Sum();
        if (sumTimes != 0)
        {
            var keys = _modulationStatistics.Keys.ToList();
            foreach (var item in keys)
            {
                _modulationResult[item] = _modulationStatistics[item] / sumTimes;
                // 当统计的信号识别率大于0时才加入相关可能的业务信息内容
                if (!_modulationInfo.ContainsKey(item) && _modulationResult[item] > 0)
                    _modulationInfo.Add(item, _modFeatures.GetBusinessInfo(item));
                // 当信号识别率等于0时将之前的业务信息删除
                if (_modulationInfo.ContainsKey(item) && !string.IsNullOrEmpty(_modulationInfo[item]) &&
                    _modulationResult[item] == 0) _modulationInfo.Remove(item);
            }
        }

        result.Add(_modulationResult);
        result.Add(_modulationInfo);
        result.Add(Fc);
        result.Add(SymbolRate);
        return result;
    }

    /// <summary>
    ///     返回单实例进行运算
    /// </summary>
    public static DcModRecognition GetSingleton()
    {
        return _modFeatures ??= new DcModRecognition();
    }

    /// <summary>
    ///     清除调制识别率集合相关数据
    /// </summary>
    public void ClearModulationStatistics()
    {
        var type = typeof(ModulationType);
        foreach (ModulationType demod in Enum.GetValues(type))
        {
            _modulationStatistics[demod] = 0;
            _modulationResult[demod] = 0;
        }

        _modFeatures?.Clear();
    }

    /// <summary>
    ///     清除缓存的静态集合数据
    /// </summary>
    public void Clear()
    {
        _vCumF1?.Clear();
        _vMiuF42?.Clear();
        _vGammaMax?.Clear();
        _vDeltaAp?.Clear();
        _vDeltaAf?.Clear();
        _vCumF2?.Clear();
        _vDeltaDa?.Clear();
    }

    /// <summary>
    ///     初始化，更新一些数据，初始化一些变量
    /// </summary>
    /// <param name="iData">I/Q数据同相分量</param>
    /// <param name="qData">I/Q数据正交分量</param>
    /// <param name="sampleRate">IQ采样率，单位：MHz</param>
    /// <param name="freq">信号所在中心频率</param>
    private void InitializeData(short[] iData, short[] qData, double sampleRate, double freq)
    {
        if (iData == null || qData == null || iData.Length != qData.Length || iData.Length == 0 ||
            qData.Length == 0) return;
        var n = (int)Math.Pow(2, Math.Ceiling(Math.Log(iData.Length, 2)));
        if (n != iData.Length)
        {
            Array.Resize(ref iData, n);
            Array.Resize(ref qData, n);
        }

        // 初始化变量
        _frequency = freq;
        _dataLen = iData.Length;
        _acn = new double[_dataLen];
        _phiNl = new double[_dataLen];
        _phiAll = new double[_dataLen];
        _dataFft = new Complex[_dataLen];
        _localData = new Complex[_dataLen];
        _freqInst = new double[_dataLen - 1];
        _freqNorm = new double[_dataLen - 1];
        double angle = CommonMethods.VectorAngle(iData, qData) - 90;
        if (Math.Abs(angle) > 1 && Math.Abs(angle) <= 30)
        {
            var ida = Array.ConvertAll(iData, item => (double)item);
            var qda = Array.ConvertAll(qData, item => (double)item);
            SignalProcess.CorrectIqPhase(ida, qda, out var nq);
            qData = Array.ConvertAll(nq, item => (short)item);
        }

        for (var i = 0; i < _dataLen; i++)
        {
            _localData[i] = new Complex(iData[i], qData[i]);
            _localData[i] *= _rotationFactor;
            _dataFft[i] = _localData[i];
        }

        // 当采样率改变时重新加载门限列表
        if (Math.Abs(_fs - sampleRate) < 1e-9) return;
        _fs = sampleRate;
        // dynamically load threshold based on samplerate
        for (var i = 0; i < _sampleRates.Length; i++) _minusSampleRate[i] = Math.Abs(_sampleRates[i] - sampleRate);
        var strKey = _sampleRatesStrs[CommonMethods.MinPosition(_minusSampleRate)];
        _currentThreshold = _dicConstThresholds[strKey];
    }

    private void InitializeData(int[] iData, int[] qData, double sampleRate, double freq)
    {
        if (iData == null
            || qData == null
            || iData.Length != qData.Length
            || iData.Length == 0
            || qData.Length == 0)
            return;
        var n = (int)Math.Pow(2, Math.Ceiling(Math.Log(iData.Length, 2)));
        if (n != iData.Length)
        {
            Array.Resize(ref iData, n);
            Array.Resize(ref qData, n);
        }

        // 初始化变量
        _frequency = freq;
        _dataLen = iData.Length;
        _acn = new double[_dataLen];
        _phiNl = new double[_dataLen];
        _phiAll = new double[_dataLen];
        _dataFft = new Complex[_dataLen];
        _localData = new Complex[_dataLen];
        _freqInst = new double[_dataLen - 1];
        _freqNorm = new double[_dataLen - 1];
        double angle = CommonMethods.VectorAngle(iData, qData) - 90;
        if (Math.Abs(angle) > 1 && Math.Abs(angle) <= 30)
        {
            var ida = Array.ConvertAll(iData, item => (double)item);
            var qda = Array.ConvertAll(qData, item => (double)item);
            SignalProcess.CorrectIqPhase(ida, qda, out var nq);
            qData = Array.ConvertAll(nq, item => (int)item);
        }

        for (var i = 0; i < _dataLen; i++)
        {
            _localData[i] = new Complex(iData[i], qData[i]);
            _localData[i] *= _rotationFactor;
            _dataFft[i] = _localData[i];
        }

        // 当采样率改变时重新加载门限列表
        if (Math.Abs(_fs - sampleRate) < 1e-9) return;
        _fs = sampleRate;
        // dynamically load threshold based on samplerate
        for (var i = 0; i < _sampleRates.Length; i++) _minusSampleRate[i] = Math.Abs(_sampleRates[i] - sampleRate);
        var strKey = _sampleRatesStrs[CommonMethods.MinPosition(_minusSampleRate)];
        _currentThreshold = _dicConstThresholds[strKey];
    }

    /// <summary>
    ///     特征参数提取
    /// </summary>
    private void Extract()
    {
        var cnt = 0;
        double temp = 0;
        double tempSum1 = 0;
        double tempSum2 = 0;
        double tempSum3 = 0;
        double phiMean = 0;
        double freqMean = 0;
        Complex tempComplex1;
        Complex tempComplex2;
        Complex tempComplex3;
        int i;
        for (i = 0; i < _dataLen; i++)
        {
            _phiAll[i] = _localData[i].Phase;
            phiMean += _phiAll[i];
            temp += _localData[i].Magnitude * _localData[i].Magnitude;
            // localData[i].Real * localData[i].Real + localData[i].Imaginary * localData[i].Imaginary;
        }

        phiMean /= _dataLen;
        for (i = 0; i < _dataLen; i++) _phiNl[i] = _phiAll[i] - phiMean;
        temp /= _dataLen;
        _thActive = Math.Sqrt(temp) *
                    ThActiveRatio; // localData.Select(item => item.Magnitude * item.Magnitude).Max() * thActiveRatio;//
        _thActiveRe =
            Math.Sqrt(temp) *
            ThActiveReRatio; // localData.Select(item => item.Magnitude * item.Magnitude).Max() * thActiveReRatio;// 
        for (i = 0; i < _dataLen - 1; i++)
        {
            if (_phiNl[i + 1] - _phiNl[i] > Pi)
                _freqInst[i] = _phiNl[i + 1] - _phiNl[i] - 2 * Pi;
            else if (_phiNl[i + 1] - _phiNl[i] < -Pi)
                _freqInst[i] = _phiNl[i + 1] - _phiNl[i] + 2 * Pi;
            else
                _freqInst[i] = _phiNl[i + 1] - _phiNl[i];
            _freqInst[i] = _freqInst[i] / 2 / Pi;
        }

        temp = 0;
        for (i = 0; i < _dataLen; i++)
            if (Math.Abs(_localData[i].Real) > _thActiveRe && Math.Abs(_localData[i].Imaginary) > _thActiveRe)
            {
                tempSum1 += _phiNl[i] * _phiNl[i];
                tempSum2 += Math.Abs(_phiNl[i]);
                tempSum3 += _phiNl[i];
                cnt++;
                if (i > 0)
                {
                    _freqNorm[_cntFreqNorm] = _freqInst[i - 1];
                    temp += Math.Abs(_freqInst[i - 1]);
                    freqMean += _freqInst[i - 1];
                    _cntFreqNorm++;
                }
            }

        _deltaAp = Math.Sqrt(tempSum1 / cnt - Math.Pow(tempSum2 / cnt, 2));
        _deltaDp = Math.Sqrt(tempSum1 / cnt - Math.Pow(tempSum3 / cnt, 2));
        tempSum1 = 0;
        tempSum2 = 0;
        tempSum3 = 0;
        temp /= _cntFreqNorm;
        freqMean /= _cntFreqNorm;
        if (_cntFreqNorm > 10)
        {
            // Σfcn^4/Ns/(Σfcn^2)^2
            for (i = 0; i < _cntFreqNorm; i++)
            {
                // fn=fc/rb=(fi-mf)/rb,mf=1/N*Σfi,rb为码元速率
                _freqNorm[i] = (_freqNorm[i] - freqMean) * temp;
                tempSum1 += Math.Pow(_freqNorm[i], 2);
                tempSum2 += Math.Abs(_freqNorm[i]);
                tempSum3 += Math.Pow(_freqNorm[i], 4);
            }

            _deltaAf = Math.Sqrt(tempSum1 / _cntFreqNorm - Math.Pow(tempSum2 / _cntFreqNorm, 2));
        }

        // E(fcn^4)/E(fcn^2)^2
        _miuF42 = tempSum3 * _cntFreqNorm / Math.Pow(tempSum1, 2);
        double dataMean = 0;
        for (i = 0; i < _dataLen; i++) dataMean += _localData[i].Magnitude;
        dataMean /= _dataLen;
        tempSum1 = 0;
        tempSum2 = 0;
        tempSum3 = 0;
        double tempSum4 = 0;
        double tempSum5 = 0;
        cnt = 0;
        var invDataLen = 1.0d / _dataLen;
        var acnComplex = new Complex[_dataLen];
        for (i = 0; i < _dataLen; i++)
        {
            // 归一化去除直流分量与信道增益 Xcn = X / Xmean-1;
            _acn[i] = _localData[i].Magnitude / dataMean - 1;
            acnComplex[i] = new Complex(_acn[i], 0);
            tempSum1 += Math.Pow(_acn[i], 2);
            tempSum2 += Math.Pow(_acn[i], 4);
            tempSum3 += Math.Abs(_acn[i]);
            if (_localData[i].Magnitude > _thActive)
            {
                cnt++;
                tempSum4 += _acn[i] * _acn[i];
                tempSum5 += _acn[i];
            }
        }

        // E(xcn^4)/E(xcn^2)^2
        _miuA42 = tempSum2 * _dataLen / Math.Pow(tempSum1, 2);
        _deltaAa = Math.Sqrt(tempSum1 * invDataLen - Math.Pow(tempSum3 * invDataLen, 2));
        _deltaDa = Math.Sqrt(tempSum4 / cnt - Math.Pow(tempSum5 / cnt, 2));
        GeneralMethods.Fft(acnComplex, _dataLen);
        //FFT(dataLen, acnComplex);
        temp = 0;
        for (i = 0; i < _dataLen; i++)
        {
            var acnAbs = Math.Pow(acnComplex[i].Magnitude, 2);
            if (acnAbs > temp) temp = acnAbs;
        }

        _gammaMax = temp * invDataLen;
        var tempComplex9 = new Complex();
        var tempComplex10 = new Complex();
        var tempComplex11 = new Complex();
        var tempComplex12 = new Complex();
        var tempComplex13 = new Complex();
        for (i = 0; i < _dataLen; i++)
        {
            // new Complex(data[i].Real, -data[i].Imaginary);
            tempComplex1 = Complex.Conjugate(_localData[i]);
            tempComplex2 = _localData[i] * _localData[i]; // x^2
            tempComplex3 = tempComplex2 * _localData[i]; // x^3
            var tempComplex4 = tempComplex3 * _localData[i]; // x^4
            var tempComplex5 = tempComplex3 * tempComplex1;
            var tempComplex6 = tempComplex1 * tempComplex1;
            var tempComplex7 = tempComplex2 * tempComplex6;
            var tempComplex8 = _localData[i] * tempComplex1;
            tempComplex9 += new Complex(tempComplex2.Real, tempComplex2.Imaginary); // sum(x^2)
            tempComplex10 += new Complex(tempComplex5.Real, tempComplex5.Imaginary); // sum(x^3*conj(x))
            tempComplex11 += new Complex(tempComplex4.Real, tempComplex4.Imaginary); // sum(x^4)
            tempComplex12 += new Complex(tempComplex7.Real, tempComplex7.Imaginary); // sum(x^2*conj(x)^2)
            tempComplex13 += new Complex(tempComplex8.Real, tempComplex8.Imaginary); // sum(x.conj(x)) 
        }

        tempComplex1 = tempComplex11 * invDataLen;
        tempComplex2 = tempComplex9 * tempComplex9 * invDataLen * invDataLen;
        tempComplex2 = new Complex(tempComplex2.Real * 3, tempComplex2.Imaginary * 3);
        var c40 = tempComplex1 - tempComplex2;
        tempComplex1 = tempComplex10 * invDataLen;
        tempComplex2 = tempComplex13 * tempComplex9 * invDataLen * invDataLen;
        tempComplex2 *= 3; // Compute mistake before:Record
        var c41 = tempComplex1 - tempComplex2;
        tempComplex1 = tempComplex12 * invDataLen;
        tempComplex2 = new Complex(Math.Pow((tempComplex9 * invDataLen).Magnitude, 2), 0);
        tempComplex3 = new Complex(2 * Math.Pow(tempComplex13.Real * invDataLen, 2), 0);
        var c42 = new Complex(tempComplex1.Real - tempComplex2.Real - tempComplex3.Real, 0);
        _cumF1 = c40.Magnitude / Math.Abs(c42.Real);
        _cumF2 = c41.Magnitude / Math.Abs(c42.Real);
        // [1,1]=>M=2,BPSK;[1,0]=>M=4,QPSK;[0,0]=>M=8,8PSK或更高
        // var vectorFm = new List<double>() { _cumF2, _cumF1 };
        GeneralMethods.Fft(_dataFft, _dataLen);
        //FFT(dataLen, dataFft);
        //FFTShift(dataLen, dataFft);
        _dataFft = SignalProcess.FftShift(_dataFft);
        var dataFftAbs = new double[_dataLen];
        for (i = 0; i < _dataFft.Length; i++) dataFftAbs[i] = _dataFft[i].Magnitude;
        // TODO: "功率谱平坦型，区分噪声与信号,存在算法准确性问题"
        //double E1 = 0.0d;
        //double E2 = 0.0d;
        //double E3 = 0.0d;
        //int nonweakCnt = 0;
        //double[] ft2 = new double[dataFftAbs.Length];
        //for (i = 0; i < dataFftAbs.Length; i++)
        //{
        //    ft2[i] = dataFftAbs[i] * dataFftAbs[i];
        //    E1 += ft2[i];
        //}
        //E1 /= ft2.Length;
        //for (i = 0; i < dataFftAbs.Length; i++)
        //{
        //    if (ft2[i] > E1)
        //    {
        //        E2 += ft2[i];
        //        nonweakCnt++;
        //    }
        //}
        //E2 /= nonweakCnt;
        //nonweakCnt = 0;
        //for (i = 0; i < dataFftAbs.Length; i++)
        //{
        //    if (ft2[i] > E2)
        //    {
        //        E3 += ft2[i];
        //        nonweakCnt++;
        //    }
        //}
        //E3 /= nonweakCnt;
        //signalExist = E3 / E1 - 1;
        ////////////////////////////////////////////
        // 参数P的估计，进行LSB、USB与FM\FSK,PSK,QAM等的区分 
        double pl = 0;
        double pu = 0;
        if (Fc != 0)
        {
            var fcn = (int)(Fc * _dataLen / 2 / _fs - 1);
            var fftX1 = new double[fcn];
            var fftX2 = new double[fcn];
            for (i = 0; i < fcn; i++)
            {
                fftX1[i] = dataFftAbs[i] * 2 / _dataLen;
                if (fcn + 1 + i >= dataFftAbs.Length) break;
                fftX2[i] = dataFftAbs[fcn + 1 + i] * 2 / _dataLen;
            }

            for (i = 0; i < fcn; i++)
            {
                pl += Math.Pow(fftX1[i], 2);
                pu += Math.Pow(fftX2[i], 2);
            }

            _p = (pl - pu) / (pl + pu);
        }

        // 循环两次避免邻近的谱峰导致数目误差偏大
        var indexMax = CommonMethods.FindPeaksValleys(dataFftAbs)[0];
        var peaks = new double[indexMax.Count];
        for (i = 0; i < indexMax.Count; i++) peaks[i] = dataFftAbs[indexMax[i]];
        temp = 0;
        double dataFftAbsMax = 0;
        if (peaks.Length > dataFftAbs.Length / 8)
        {
            indexMax = CommonMethods.FindPeaksValleys(peaks)[0];
            var secondPeaks = new double[indexMax.Count];
            for (var k = 0; k < indexMax.Count; k++) secondPeaks[k] = peaks[indexMax[k]];
            indexMax = CommonMethods.FindPeaksValleys(secondPeaks)[0];
            peaks = new double[indexMax.Count];
            for (i = 0; i < indexMax.Count; i++) peaks[i] = secondPeaks[indexMax[i]];
        }

        for (i = 0; i < indexMax.Count; i++)
        {
            if (peaks[i] > dataFftAbsMax) dataFftAbsMax = peaks[i];
            temp += peaks[i];
        }

        temp /= indexMax.Count;
        _thPeakBottom = temp * ThPeakBottomRatio;
        _thPeakTop = dataFftAbsMax * ThPeakTopRatio;
        for (i = 1; i < indexMax.Count - 1; i++)
            // 谱峰凸显条件：X[k]/((X[k-2]+X[k+2]+X[k-3]+X[k+3])/4)>delta delta ≈ 2.75
            if (peaks[i] > _thPeakBottom && peaks[i] > _thPeakTop)
                if (peaks[i] > peaks[i - 1] && peaks[i] > peaks[i + 1])
                    _peakNum++;

        #region 特征参数的二次估计量

        ////////////////////////////////////////////////////
        if (_vMiuF42.Count > 20) _vMiuF42.RemoveRange(0, _vMiuF42.Count - 20);
        if (!double.IsNaN(_miuF42)) _vMiuF42.Add(_miuF42);
        _biasMiuF42 = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vMiuF42)) - CommonMethods.StdSigma(_vMiuF42))
            .Real;
        ///////////////////////////////////////////////////
        if (_vCumF1.Count > 20) _vCumF1.RemoveRange(0, _vCumF1.Count - 20);
        if (!double.IsNaN(_cumF1)) _vCumF1.Add(_cumF1);
        _biasCumF1 = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vCumF1)) - CommonMethods.StdSigma(_vCumF1)).Real;
        ///////////////////////////////////////////////////
        if (_vCumF2.Count > 20) _vCumF2.RemoveRange(0, _vCumF2.Count - 20);
        if (!double.IsNaN(_cumF2)) _vCumF2.Add(_cumF2);
        _biasCumF2 = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vCumF2)) - CommonMethods.StdSigma(_vCumF2)).Real;
        //////////////////////////////////////////////////
        if (_vGammaMax.Count > 20) _vGammaMax.RemoveRange(0, _vGammaMax.Count - 20);
        if (!double.IsNaN(_gammaMax)) _vGammaMax.Add(_gammaMax);
        _biasGammaMax = CommonMethods
            .Sqrt(Math.Abs(CommonMethods.Mean(_vGammaMax)) - CommonMethods.StdSigma(_vGammaMax)).Real;
        /////////////////////////////////////////////////
        if (_vDeltaAp.Count > 20) _vDeltaAp.RemoveRange(0, _vDeltaAp.Count - 20);
        if (!double.IsNaN(_deltaAp)) _vDeltaAp.Add(_deltaAp);
        _biasDeltaAp = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vDeltaAp)) - CommonMethods.StdSigma(_vDeltaAp))
            .Real;
        ////////////////////////////////////////////////
        if (_vDeltaAf.Count > 20) _vDeltaAf.RemoveRange(0, _vDeltaAf.Count - 20);
        if (!double.IsNaN(_deltaAf)) _vDeltaAf.Add(_deltaAf);
        _biasDeltaAf = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vDeltaAf)) - CommonMethods.StdSigma(_vDeltaAf))
            .Real;
        //////////////////////////////////////////////
        if (_vDeltaDa.Count > 20) _vDeltaDa.RemoveRange(0, _vDeltaDa.Count - 20);
        if (!double.IsNaN(_deltaDa)) _vDeltaDa.Add(_deltaDa);
        _biasDeltaDa = CommonMethods.Sqrt(Math.Abs(CommonMethods.Mean(_vDeltaDa)) - CommonMethods.StdSigma(_vDeltaDa))
            .Real;
        //////////////////////////////////////////////

        #endregion 特征参数的二次估计量

        var u = CommonMethods.Mean(_acn);
        var sigma = CommonMethods.StdSigma(_acn);
        _ = u * u / (sigma * sigma);
    }

    /// <summary>
    ///     决策树分类
    /// </summary>
    public void ClassifierDecisionTree()
    {
        if (_deltaDp > ConstThresholds.DeltaDp)
        {
            //if (deltaAp > ConstThresholds.deltaAp)   
            if (_biasDeltaAp > ConstThresholds.DeltaAp && _biasCumF1 < ConstThresholds.BiasCumF1)
            {
                //if (gammaMax > ConstThresholds.gammaMax1)
                if (_biasGammaMax > ConstThresholds.BiasGammamax)
                {
                    if (_deltaAa > ConstThresholds.DeltaAa)
                        _modType = ModulationType.Qam16;
                    else
                        //if (cumF1 > ConstThresholds.cumF1)
                        _modType = _biasCumF1 > ConstThresholds.BiasCumF2 ? ModulationType.Qpsk : ModulationType.Psk8;
                } // deltaAf 进一步区分2FSK与4FSK
                else if
                    (_miuF42 > ConstThresholds
                        .MiuF42 /*  && deltaAf < ConstThresholds.deltaAf2&& stdF42 < ConstThresholds.stdMiuF42*/)
                {
                    _modType = ModulationType.Fm;
                }
                else if (_peakNum > ConstThresholds.PeakNum && _deltaAf < ConstThresholds.DeltaAf1)
                {
                    _modType = ModulationType.Fsk4;
                }
                else
                {
                    _modType = ModulationType.Fsk2;
                }
            }
            else
            {
                _modType = ModulationType.Bpsk;
            }
        }
        else if (_gammaMax > ConstThresholds.GammaMax2)
        {
            _modType = _miuA42 > ConstThresholds.MiuA42
                ? ModulationType.Am
                :
                // deltaAa进一步区分2ASK与4ASK
                ModulationType.Ask2;
        }
        else
        {
            _modType = ModulationType.Cw;
        }
    }

    //#if test
    public void ClassifierDecisionTreeDic()
    {
        if (_currentThreshold?.ContainsKey("deltaDp") != true) return;
        if (_deltaDp > _currentThreshold["deltaDp"])
        {
            // 适当时候可以去掉后面的约束条件“biasCumF1 < currentThreshold["biasCumF1"]”，因为该条件并不任何采样率都成立；或者biasCumF1>=0.95 && biadCumF1<=1 2018.01.29.11.24
            // 发现biasCumF2对于信号BPSK有独特的区分作用，考虑加入该参数判断
            if (_biasDeltaAp > _currentThreshold["biasDeltaAp"] && _biasCumF2 < _currentThreshold["biasCumF2"])
            {
                if (_biasGammaMax > _currentThreshold["biasGammaMax"])
                {
                    if (_biasDeltaDa > _currentThreshold["biasDeltaDa"])
                        _modType = ModulationType.Qam16;
                    else if (_biasCumF1 > _currentThreshold["biasCumF1"])
                        _modType = ModulationType.Qpsk;
                    else
                        _modType = ModulationType.Psk8;
                } // deltaAf 进一步区分2FSK与4FSK
                //else if (miuF42 > currentThreshold["miuF42"]) /*&& deltaAf < currentThreshold["deltaAf"]) ConstThresholds.deltaAf2)/*&& stdF42 < ConstThresholds.stdMiuF42)*/
                //{
                //    modType = ModulationType.FM;
                //} // deltaAf 区分2FSK 4FSK 8FSK
                else if (_peakNum > _currentThreshold["peakNum"] && _deltaAf < _currentThreshold["deltaAf"])
                {
                    _modType = ModulationType.Fsk;
                }
                else
                {
                    _modType = ModulationType.Fm;
                }
            }
            else
            {
                _modType = ModulationType.Bpsk;
            }
        }
        else
        {
            //if (gammaMax > currentThreshold["gammaMax"])
            if (_biasDeltaDa > _currentThreshold["biasDeltaDaCw"])
            {
                if (_miuA42 > _currentThreshold["miuA42"])
                    _modType = ModulationType.Am;
                // deltaAa进一步区分2ASK与4ASK
                else if (_deltaAa < 0.5)
                    _modType = ModulationType.Ask2;
                else
                    _modType = ModulationType.Ask4;
            }
            else
            {
                _modType = ModulationType.Cw;
            }
        }
    }

    /// <summary>
    ///     返回识别的类型
    /// </summary>
    public ModulationType GetModType()
    {
        if (!_isTest)
        {
            // var rand = new Random(Environment.TickCount);

            #region 按频率表划分

            if (IsValueRange(26, 33, _frequency) || IsValueRange(47, 54, _frequency) ||
                IsValueRange(136, 174, _frequency) || IsValueRange(400, 470, _frequency))
            {
                if (_modType != ModulationType.Fm && _modType != ModulationType.Fsk4)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.FM;
                    //else
                    //    modType = ModulationType.FSK4;
                }
            }
            // FM
            else if (IsValueRange(87, 108, _frequency))
            {
                if (_modType != ModulationType.Am && _modType != ModulationType.Fm)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.FM;
                    //else
                    //    modType = ModulationType.AM;
                }
            }
            // Tetra(380~400,410~430,806~825,851~870) 7/4πQPSK 36kb/s
            else if (IsValueRange(361, 368, _frequency) || IsValueRange(380, 400, _frequency) ||
                     IsValueRange(410, 430, _frequency) || IsValueRange(806, 825, _frequency) ||
                     IsValueRange(851, 870, _frequency))
            {
                if (_modType != ModulationType.Qpsk)
                {
                    //modType = ModulationType.QPSK;
                }
            }
            // PAL（Phase Alteration Line，逐行倒相，电视标准）
            else if (IsValueRange(171, 219, _frequency) || IsValueRange(474, 562, _frequency) ||
                     IsValueRange(610, 802, _frequency))
            {
                if (_modType != ModulationType.AmScdsb && _modType != ModulationType.Qpsk &&
                    _modType != ModulationType.Qam)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.AM_SCDSB;
                    //else if (rand.Next() % 3 == 0)
                    //    modType = ModulationType.QPSK;
                    //else
                    //    modType = ModulationType.QAM;
                }
            }
            // GSM900
            else if (IsValueRange(930, 960, _frequency))
            {
                if (_modType != ModulationType.Psk8 && _modType != ModulationType.Msk)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.PSK8;
                    //else
                    //    modType = ModulationType.MSK;
                }
            }
            // EVDO (CDMA2000 1xEV-DO Data Only)
            else if (IsValueRange(870, 880, _frequency))
            {
                if (_modType != ModulationType.Qam)
                {
                    //modType = ModulationType.QAM;
                }
            }
            // WCDMA
            else if (IsValueRange(1880, 1920, _frequency) || IsValueRange(2010, 2025, _frequency) ||
                     IsValueRange(2110, 2170, _frequency))
            {
                if (_modType != ModulationType.Bpsk && _modType != ModulationType.Qpsk)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.BPSK;
                    //else
                    //    modType = ModulationType.QPSK;
                }
            }
            // TD-LTE,FDD-LTE,WI-FI(2.4G,5G)
            else if (IsValueRange(1885, 1915, _frequency) || IsValueRange(2300, 2400, _frequency) ||
                     IsValueRange(2500, 2690, _frequency) || IsValueRange(1755, 1785, _frequency) ||
                     IsValueRange(1805, 1880, _frequency) ||
                     IsValueRange(1995, 1980, _frequency) || IsValueRange(2110, 2170, _frequency) ||
                     IsValueRange(2400, 2485, _frequency) ||
                     IsValueRange(5725, 5850, _frequency))
            {
                if (_modType != ModulationType.Bpsk && _modType != ModulationType.Qpsk &&
                    _modType != ModulationType.Qam16 && _modType != ModulationType.Qam64)
                {
                    //if (rand.Next() % 2 == 0)
                    //    modType = ModulationType.BPSK;
                    //else if (rand.Next() % 3 == 0)
                    //    modType = ModulationType.QPSK;
                    //else if (rand.Next() % 5 == 0)
                    //    modType = ModulationType.QAM16;
                    //else
                    //    modType = ModulationType.QAM64;
                }
            }
        }

        #endregion

        return _modType;
    }

    /// <summary>
    ///     判断一个数是否在某区间
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="min">最小边缘</param>
    /// <param name="max">最大边缘</param>
    /// <param name="data">待判断的数</param>
    /// <param name="isCloseInterval">是否闭区间</param>
    /// <returns>在区间内：true；不在区间内：false</returns>
    private bool IsValueRange<T>(T min, T max, T data, bool isCloseInterval = true) where T : struct, IComparable
    {
        return isCloseInterval
            ? min.CompareTo(data) <= 0 && max.CompareTo(data) >= 0
            : min.CompareTo(data) < 0 && max.CompareTo(data) > 0;
    }

    /// <summary>
    ///     输出调制技术对应的可能处于哪些业务段的信息
    /// </summary>
    /// <param name="modtype"></param>
    public string GetBusinessInfo(ModulationType modtype)
    {
        string str;
        //% 2FSK:
        //% AM:短波调幅广播（3.5MHz~29.7MHz）
        //% SSB - AM:ATC,应急语音（2.85MHz~24.89MHz）
        //% AM:调幅广播，航空通信（118MHz~137MHz），长波调幅广播（120kHz~300kHz，9kHz带宽），中波调幅广播（525kHz~1605kHz，9kHz带宽）
        //% FM:可能为调频广播及数据广播（88MHz - 108MHz）
        //% FM:可能为电视伴音信号（48.5MHz~92MHz；167MHz~223MHz；223MHz~443MHz；443MHz~870MHz，视频加伴音8MHz带宽），查看距离 - 8MHz位置附近是否有宽带信号
        //% 2FSK:
        //% 4FSK: 可能为数字对讲机Tetra业务
        //% BPSK:可能为卫星业务通信
        //% QPSK:可能为数字蜂窝移动通信系统（2G）业务，IS - 95CDMA或者DAMPS或PDC通信业务
        //% QPSK:可能为CDMA2000,WCDMA宽带蜂窝移动通信系统（3G）下行传输业务
        //% 7/4πQPSK: 可能为Tetra数字集群通信系统（380MHz~400MHz，410~430MHz，806MHz~825MHz，851MHz~870MHz）
        //% 8PSK:
        //% 16QAM: 可能为数字电视业务通信
        //% 16QAM: 可能为WiFi数据传输业务，TD - SCDMA
        if (modtype == ModulationType.Am)
        {
            if (_frequency is >= 0.12d and < 0.3d)
                str = "长波调幅广播（120kHz~300kHz，9kHz带宽）；";
            else if (_frequency is >= 0.525d and <= 1.605d)
                str = "中波调幅广播（525kHz~1605kHz，9kHz带宽）；";
            else if (_frequency is >= 3.5d and <= 29.7d)
                str = "短波调幅广播（3.5MHz~29.7MHz）；";
            else if (_frequency is >= 87.0d and <= 108.0d)
                str = "调频/调幅广播段（88MHz~108MHz）；";
            else if (_frequency is >= 117.9d and <= 137.0d)
                str = "航空通信（118MHz~137MHz）；";
            else if (_frequency is >= 48.5d and <= 92.0d or >= 167.0d and <= 223.0d or >= 223.0d and <= 443.0d
                     or > 443.0d and <= 870.0d)
                str = "模拟电视业务（可能是AM调制或者残留边带的AM调制信号，即VSB调制）。";
            else
                str = "未知业务应用；";
            //str = "可能为长波调幅广播（120kHz~300kHz，9kHz带宽），中波调幅广播（525kHz~1605kHz，9kHz带宽）；短波调幅广播（3.5MHz~29.7MHz）；调频/调幅广播段（88MHz~108MHz）；航空通信（118MHz~137MHz）；";
        }
        else if (modtype == ModulationType.Fm)
        {
            if (_frequency is >= 87.0d and <= 108.0d)
                str = "调频广播及数据广播（88MHz - 108MHz）；";
            else if (_frequency is >= 48.5d and <= 92.0d or >= 167.0d and <= 223.0d or >= 223.0d and <= 443.0d
                     or > 443.0d and <= 870.0d)
                str =
                    "对讲机业务应用或者电视伴音信号（48.5MHz~92MHz；167MHz~223MHz；223MHz~443MHz；443MHz~870MHz，视频加伴音8MHz带宽），查看距离 - 8MHz位置附近是否有宽带信号加以验证；";
            else
                str = "未知业务类型的调频信号";
        }
        else if (modtype == ModulationType.Fsk2)
        {
            str = "数字广播、数字语音通信等业务或者现代铁路信息化传输应用业务；";
        }
        else if (modtype is ModulationType.Fsk4 or ModulationType.Fsk)
        {
            str = "数字对讲机业务（V段：136MHz~174MHz，U段：400MHz-470MHz，公众对讲机：409MHz~410MHz）；";
        }
        else if (modtype == ModulationType.Bpsk)
        {
            str = "卫星数据通信业务或CDMA2000上行链路业务或WCDMA上行链路数据传输业务；";
        }
        else if (modtype == ModulationType.Qpsk)
        {
            str = "数字蜂窝移动通信系统（2G）业务，IS-95 CDMA或者DAMPS或PDC通信业务；";
            str += "或者为CDMA2000,WCDMA宽带蜂窝移动通信系统（3G）下行传输业务；";
            str += "或者为Tetra数字集群通信系统（380MHz~400MHz，410MHz~430MHz，806MHz~825MHz，851MHz~870MHz）业务（7/4πQPSK）；";
        }
        else if (modtype == ModulationType.Psk8)
        {
            str = _frequency is >= 885.0d and <= 890.0d or >= 930.0d and <= 935.0d
                ? "GSM演进方案EDGE业务,应用旋转的3π/8的8PSK技术；"
                : "未知业务类型的8PSK信号；";
        }
        else if (modtype is ModulationType.Qam or ModulationType.Qam16)
        {
            str = "数字电视数据传输业务，或者Wi-Fi数据传输业务，TD-SCDMA；";
        }
        else if (modtype == ModulationType.Cw)
        {
            str = _frequency is >= 420.0d and <= 470.0d ? "对讲机业务（没有调制音频的单载波信号）；" : "等幅电报通信方式；";
        }
        else
        {
            str = "相关业务信息待完善...";
        }

        return str;
    }

    /// <summary>
    ///     是否是数字调制方式
    /// </summary>
    /// <param name="mod">可以使枚举对象或者字符串</param>
    public static bool IsDigitalModulation(object mod)
    {
        var flag = true;
        var modulation = mod.ToString()?.ToUpper();
        if (modulation != null)
            flag &= modulation.Contains('Q') || modulation.Contains("DM") || modulation.Contains("SK");
        return flag;
    }

    /// <summary>
    ///     打印特征值
    /// </summary>
    /// <param name="isPrint"></param>
    public string PrintFeature(bool isPrint = false)
    {
        var sb = new StringBuilder();
        sb.Append("\n****** print Feature ******").Append('\n')
            .AppendFormat("deltaDp = {0}", _deltaDp)
            .Append('\n')
            .AppendFormat("deltaAp = {0}", _deltaAp)
            .Append('\n')
            .AppendFormat("gammaMax = {0}", _gammaMax)
            .Append('\n')
            .AppendFormat("miuA42 = {0}", _miuA42)
            .Append('\n')
            .AppendFormat("deltaAa = {0}", _deltaAa)
            .Append('\n')
            .AppendFormat("deltaA = {0}", _deltaDa)
            .Append('\n')
            .AppendFormat("deltaAf = {0}", _deltaAf)
            .Append('\n')
            .AppendFormat("miuF42 = {0}", _miuF42)
            .Append('\n')
            .AppendFormat("cumF1 = {0}", _cumF1)
            .Append('\n')
            .AppendFormat("cumF2 = {0}", _cumF2)
            .Append('\n')
            .AppendFormat("peakNum = {0}", _peakNum)
            .Append('\n')
            .AppendFormat("biasMiuF42 = {0}", _biasMiuF42)
            .Append('\n')
            .AppendFormat("biasGammaMax = {0}", _biasGammaMax)
            .Append('\n')
            .AppendFormat("biasCumF1 = {0}", _biasCumF1)
            .Append('\n')
            .AppendFormat("biasDeltaAf = {0}", _biasDeltaAf)
            .Append('\n')
            //.AppendFormat("signalExist = {0}", signalExist)
            //.Append("\n")
            .AppendFormat("P = {0}", _p)
            .Append('\n')
            .AppendFormat("ModType = {0}", GetModType());
        var str = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
            _deltaDp, _deltaAp, _gammaMax, _miuA42, _deltaAa, _deltaDa, _deltaAf, _miuF42, _cumF1, _cumF2, _peakNum,
            _biasMiuF42,
            _biasGammaMax, _biasCumF1, _biasDeltaAf, 0 //signalExist
            , _p, GetModType());
        if (isPrint) Console.WriteLine(str);
        return str;
        //Console.WriteLine(sb.ToString());
    }
}

/// <summary>
///     调制类型
/// </summary>
public enum ModulationType
{
    /// <summary>
    ///     未知调制方式
    /// </summary>
    None,

    /// <summary>
    ///     调幅
    /// </summary>
    Am,

    /// <summary>
    ///     抑制载波双边带调幅
    /// </summary>
    AmScdsb = Am << 1,

    /// <summary>
    ///     单边带 下边带调幅
    /// </summary>
    AmSsbLsb = AmScdsb << 1,

    /// <summary>
    ///     单边带 上边带调幅
    /// </summary>
    AmSsbUsb = AmSsbLsb << 1,

    /// <summary>
    ///     调频
    /// </summary>
    Fm = AmSsbUsb << 1,

    /// <summary>
    ///     连续波调制
    /// </summary>
    Cw = Fm << 1,

    /// <summary>
    ///     二幅移键控
    /// </summary>
    Ask2 = Cw << 1,

    /// <summary>
    ///     四幅移键控
    /// </summary>
    Ask4 = Ask2 << 1,

    /// <summary>
    ///     二相移键控
    /// </summary>
    Bpsk = Ask4 << 1,

    /// <summary>
    ///     最小频移键控
    /// </summary>
    Msk = Bpsk << 1,

    /// <summary>
    ///     四相移键控
    /// </summary>
    Qpsk = Msk << 1,

    /// <summary>
    ///     π/4-QPSK
    /// </summary>
    Qpskpi4 = Qpsk << 1,

    /// <summary>
    ///     八相移键控
    /// </summary>
    Psk8 = Qpskpi4 << 1,

    /// <summary>
    ///     QAM调制
    /// </summary>
    Qam = Psk8 << 1,

    /// <summary>
    ///     4阶正交调幅
    /// </summary>
    Qam4 = Qam << 1,

    /// <summary>
    ///     16阶正交调幅
    /// </summary>
    Qam16 = Qam4 << 1,

    /// <summary>
    ///     32阶正交调幅
    /// </summary>
    Qam32 = Qam16 << 1,

    /// <summary>
    ///     64阶正交调幅
    /// </summary>
    Qam64 = Qam32 << 1,

    /// <summary>
    ///     128阶正交调幅
    /// </summary>
    Qam128 = Qam64 << 1,

    /// <summary>
    ///     256阶正交调幅
    /// </summary>
    Qam256 = Qam128 << 1,

    /// <summary>
    ///     脉幅调制
    /// </summary>
    Pam = Qam256 << 1,

    /// <summary>
    ///     二频移键控
    /// </summary>
    Fsk2 = Pam << 1,

    /// <summary>
    ///     四频移键控
    /// </summary>
    Fsk4 = Fsk2 << 1,

    /// <summary>
    ///     八频移键控
    /// </summary>
    Fsk8 = Fsk4 << 1,

    /// <summary>
    ///     ASK大类
    /// </summary>
    Ask = Fsk8 << 1,

    /// <summary>
    ///     FSK大类
    /// </summary>
    Fsk = Ask << 1,

    /// <summary>
    ///     PSK大类
    /// </summary>
    Psk = Fsk << 1
}

/// <summary>
///     一堆常量 定义门限的边界
/// </summary>
public static class ConstThresholds
{
    /// <summary>
    ///     零中心非弱信号段瞬时相位非线性分量的标准偏差
    /// </summary>
    public const double DeltaDp = 1.1d;

    /// <summary>
    ///     零中心非弱信号段瞬时相位非线性分量的绝对值的标准差
    /// </summary>
    public const double DeltaAp = 0.6d;

    /// <summary>
    ///     零中心归一化瞬时幅度谱密度的最大值
    /// </summary>
    public const double GammaMax1 = 15d; // 3d;

    public const double GammaMax2 = 11d; // 5d;

    /// <summary>
    ///     零中心归一化瞬时幅度的紧致性
    /// </summary>
    public const double MiuA42 = 2.4d;

    /// <summary>
    ///     零中心归一化瞬时幅度绝对值的标准偏差
    /// </summary>
    public const double DeltaAa = 0.195d; // 0.175d;// 0.25d;// 0.15d;

    public const double DeltaDa = 0d;
    public const double DeltaAf = 1d;

    /// <summary>
    ///     进一步区分2FSK与4FSK的附加条件，提高准确性
    /// </summary>
    public const double DeltaAf1 = 0.00275; // 0.0015d;// 0.02d;

    /// <summary>
    ///     增加FM与MFSK的划分
    /// </summary>
    public const double DeltaAf2 = 0.0015d;

    /// <summary>
    ///     零中心归一化瞬时频率的紧致性
    /// </summary>
    public const double MiuF42 = 2.05d; // 2.03;// 1.9d;

    /// <summary>
    ///     高阶累积量门限1
    /// </summary>
    public const double CumF1 = 0.6d; // 0.125d;// 0.5d;

    /// <summary>
    ///     高阶累积量门限2
    /// </summary>
    public const double CumF2 = 0d;

    /// <summary>
    ///     主谱峰个数，功率谱中δ脉冲数目
    /// </summary>
    public const int PeakNum = 2;

    public const double BiasCumF1 = 0.95d;

    /// <summary>
    ///     高阶累积量的偏差
    /// </summary>
    public const double BiasCumF2 = 0.51d;

    /// <summary>
    ///     判断瞬时频率紧致性的偏差
    /// </summary>
    public const double StdMiuF42 = 1.0d;

    /// <summary>
    ///     gamma_max的偏差
    /// </summary>
    public const double BiasGammamax = 2.0d; // 1.2d;
}

/// <summary>
///     通用方法 暂定
/// </summary>
public static class GeneralMethods
{
    private const double Pi = Math.PI;

    public static void Fft(Complex[] f, int length)
    {
        if (f == null || length <= 1) return;
        int i;
        int j;
        int mBuf;
        int m;
        Complex t;
        /*-------计算分解的级数M=log2(length)-------*/
        for (i = length, m = 1; (i /= 2) != 1; m++)
        {
        }

        /*-------按照倒位序重新排列原信号-------*/
        for (i = 1, j = length / 2; i <= length - 2; i++)
        {
            if (i < j)
            {
                t = f[j];
                f[j] = f[i];
                f[i] = t;
            }

            var k = length / 2;
            // 防止k = j = 0 added hufb
            while (k <= j && k != 0)
            {
                j -= k;
                k /= 2;
            }

            j += k;
        }

        /*-------FFT算法-------*/
        for (mBuf = 1; mBuf <= m; mBuf++)
        {
            var la = (int)Math.Pow(2, mBuf);
            var lb = la / 2;
            /*-------碟形运算-------*/
            int l;
            for (l = 1; l <= lb; l++)
            {
                var r = (int)((l - 1) * Math.Pow(2, m - mBuf));
                int n;
                for (n = l - 1; n < length - 1; n += la) //遍历每个分组，分组总数为length/la
                {
                    var lc = n + lb;
                    t = f[lc] * new Complex(Math.Cos(2 * Pi * r / length), -Math.Sin(2 * Pi * r / length));
                    f[lc] = f[n] - t;
                    f[n] += t;
                }
            }
        }
    }
}