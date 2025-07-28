// #define WRITE_DATA
// #define OUTPUT_LEVEL
// #define OUTPUT_PHASE

#define ENHANCED_DFIND
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Newtonsoft.Json;

// 不访问实例数据，可标记为 static [MR3000A]csharp(CA1822)
#pragma warning disable CA1822
namespace Magneto.Device.MR3000A;

#region 数据到达事件

internal class DfRelatedDataArrivedEventArgs : EventArgs
{
    public DfRelatedDataArrivedEventArgs(object data)
    {
        Data = data;
    }

    public object Data { get; }
}

internal delegate void DfRelatedDataArrivedEventHandler(object sender, DfRelatedDataArrivedEventArgs e);

#endregion

#region 数据源

internal interface IDataSource
{
    bool Register(IDataFilter filter);
    bool UnRegister(IDataFilter filter);
    void SetParameter(string name, object value);
    void Receive(object data);
    void Clear();
    void ClearData();
}

internal class DataSource : IDataSource
{
    #region 构造函数

    public DataSource()
    {
        _filters = new List<IDataFilter>();
        _parameterPairs = new Dictionary<string, object>();
    }

    #endregion

    #region 成员变量

    private readonly object _lock = new();
    private readonly List<IDataFilter> _filters;

    /// <summary>
    ///     缓存测向相关参数
    /// </summary>
    private readonly IDictionary<string, object> _parameterPairs;

    #endregion

    #region IDataSource

    public bool Register(IDataFilter filter)
    {
        lock (_lock)
        {
            if (filter == null) return false;
            if (!_filters.Contains(filter))
            {
                foreach (var keyValue in _parameterPairs) filter.SetParameter(keyValue.Key, keyValue.Value);
                _filters.Add(filter);
                filter.Set();
            }

            return true;
        }
    }

    public bool UnRegister(IDataFilter filter)
    {
        lock (_lock)
        {
            if (filter == null) return true;
            if (!_filters.Contains(filter)) return false;
            filter.Reset();
            _filters.Remove(filter);
            return true;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            foreach (var filter in _filters) filter.Reset();
            _filters.Clear();
        }
    }

    public void SetParameter(string name, object value)
    {
        lock (_lock)
        {
            _parameterPairs[name] = value;
            foreach (var filter in _filters)
                try
                {
                    filter.SetParameter(name, value);
                }
                catch
                {
                }
        }
    }

    public void Receive(object data)
    {
        lock (_lock)
        {
            foreach (var filter in _filters)
                try
                {
                    filter.Receive(data);
                }
                catch
                {
                }
        }
    }

    public void ClearData()
    {
        lock (_lock)
        {
            foreach (var filter in _filters) filter.ClearData();
        }
    }

    #endregion
}

#endregion

#region 过滤器

internal interface IDataFilter
{
    void Set();
    void Reset();
    void SetParameter(string name, object value);
    void Receive(object data);
    void ClearData();
}

[Serializable]
internal abstract class DataFilterBase : IDataFilter, ICloneable
{
    #region 构造函数

    protected DataFilterBase(IDataSinker sinker)
    {
        Type = GetType();
        Sinker = sinker;
    }

    #endregion

    #region ICloneable

    public object Clone()
    {
        using var stream = new MemoryStream();
        var serializer = new JsonSerializer();
        serializer.Serialize(new StreamWriter(stream), this);
        stream.Flush();
        stream.Seek(0, SeekOrigin.Begin);
        return serializer.Deserialize(new StreamReader(stream), GetType())!;
    }

    #endregion

    #region Helper

    protected short GetAverageAngle(List<short> angles, bool usingComplex = true)
    {
        if (angles.Count == 0) return 0;
        if (usingComplex)
        {
            var array = from item in angles select item / 10.0d / 180.0d * Math.PI;
            var items = (from item in array select new Complex(Math.Cos(item), Math.Sin(item))).ToArray();
            var sum = items[0];
            for (var index = 1; index < items.Length; ++index) sum += items[index];
            var average = sum / items.Length;
            return (short)((average.Phase / Math.PI * 180 % 360 + 360) % 360 * 10);
        }
        else
        {
            var array = (from item in angles select (item % 3600 + 3600) % 3600).ToArray();
            var last = array[0];
            var sum = array[0];
            for (var index = 1; index < array.Length; ++index)
            {
                var diff = (array[index] - array[index - 1] + 1800) % 3600 - 1800;
                last += diff;
                sum += last;
            }

            var average = (short)(sum / array.Length % 3600);
            return average;
        }
    }

    #endregion

    #region 成员变量

    [NonSerialized] protected readonly double Epsilon = 1.0E-7d;

    [NonSerialized] protected IDataSinker Sinker;

    [NonSerialized] protected Type Type;

    #endregion

    #region IDataFilter

    public virtual void Set()
    {
        Sinker?.StartProcess();
    }

    public virtual void Reset()
    {
        if (Sinker != null)
        {
            Sinker.StopProcess();
            Sinker.ClearData();
        }
    }

    public virtual void SetParameter(string name, object value)
    {
        var property = Type.GetProperty(name);
        if (property != null)
            try
            {
                property.SetValue(this, value, null);
            }
            catch
            {
            }
    }

    public void ClearData()
    {
        Sinker?.ClearData();
    }

    public abstract void Receive(object data);

    #endregion
}

[Serializable]
[Obsolete("Using PhaseDifferenceFilterforDuplexChannel instead.")]
internal class RawFilterforDuplexChannel : DataFilterBase
{
    #region 构造函数

    public RawFilterforDuplexChannel(IDataSinker sinker)
        : base(sinker)
    {
        _syncCode = -1;
        _dataFlags = 0x0;
        IqCollection = null;
    }

    #endregion

    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    [NonSerialized] private int _dataFlags;

    [NonSerialized] private int _syncCode;

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    private double _frequency;

    public double Frequency
    {
        get => _frequency;
        set
        {
            lock (_stateLock)
            {
                if (Math.Abs(_frequency - value) > Epsilon)
                    lock (_stateLock)
                    {
                        _frequency = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private double _bandwidth;

    public double Bandwidth
    {
        get => _bandwidth;
        set
        {
            lock (_stateLock)
            {
                if (Math.Abs(_bandwidth - value) > Epsilon)
                    lock (_stateLock)
                    {
                        _bandwidth = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                if (_antennaIndex != value)
                    lock (_stateLock)
                    {
                        _antennaIndex = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private int _dfSamplingCount;

    public int DfSamplingCount
    {
        get => _dfSamplingCount;
        set
        {
            lock (_stateLock)
            {
                if (_dfSamplingCount != value)
                    lock (_stateLock)
                    {
                        if (IqCollection != null) IqCollection = new int[ChannelCount * GroupCount * value * 2];
                        _dfSamplingCount = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    public double SampleRate { get; private set; }
    public int Attenuation { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion

    #region 重写基类方法

    public override void Reset()
    {
        base.Reset();
        lock (_stateLock)
        {
            _syncCode = -1;
            _dataFlags = 0x0;
            IqCollection = null;
        }
    }

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfiq dfIq
                || Math.Abs(dfIq.Frequency / 1000000.0d - _frequency) > Epsilon
                || Math.Abs(dfIq.Bandwidth / 1000.0d - _bandwidth) > Epsilon
                || dfIq.AntennaIndex != _antennaIndex
                || dfIq.Count != _dfSamplingCount)
                return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection ??= new int[ChannelCount * GroupCount * _dfSamplingCount * 2];
            if (dfIq.SyncCode != _syncCode)
            {
                _dataFlags = 0x0;
                _syncCode = dfIq.SyncCode;
            }

            if (dfIq.ChannelIndex == -1)
            {
                Array.Copy(dfIq.DataColleciton, 0, IqCollection, dfIq.ChannelCount * dfIq.GroupOffset * dfIq.Count * 2,
                    dfIq.DataColleciton.Length);
                for (var index = 0; index < dfIq.ChannelCount; ++index)
                    _dataFlags |= 0x1 << (dfIq.ChannelCount * dfIq.GroupOffset + index);
            }
            else
            {
                Array.Copy(dfIq.DataColleciton, 0, IqCollection,
                    (dfIq.ChannelCount * dfIq.GroupOffset + dfIq.ChannelIndex) * dfIq.Count * 2,
                    dfIq.DataColleciton.Length);
                _dataFlags |= 0x1 << (dfIq.ChannelCount * dfIq.GroupOffset + dfIq.ChannelIndex);
            }

            if (_dataFlags == (int)(Math.Pow(2, ChannelCount * GroupCount) - 1))
            {
                Sinker.Receive(MemberwiseClone());
                IqCollection = null;
                _dataFlags = 0x0;
            }
        }
    }

    #endregion
}

[Serializable]
[Obsolete("Abstract level info from PhaseDifferenceFilterforDuplexChannel.")]
internal class LevelFilterforDuplexChannel : DataFilterBase
{
    #region 构造函数

    public LevelFilterforDuplexChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (Sinker != null && data is RawDfiq { GroupOffset: 0, ChannelIndex: 0 or -1 } dfIq)
        {
            if (Math.Abs(dfIq.Frequency / 1000000.0d - Frequency) > Epsilon ||
                Math.Abs(dfIq.Bandwidth / 1000.0d - Bandwidth) > Epsilon) return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection = new int[dfIq.Count * 2];
            Array.Copy(dfIq.DataColleciton, 0, IqCollection, 0, IqCollection.Length);
            Sinker.Receive(MemberwiseClone());
            IqCollection = null;
        }
    }

    #endregion

    #region 属性（参数）

    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public int Attenuation { get; private set; }
    public double SampleRate { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion
}

[Serializable]
[Obsolete("Abstract spectrum info from PhaseDifferenceFilterforDuplexChannel.")]
internal class SpectrumFilterforDuplexChannel : DataFilterBase
{
    #region 构造函数

    public SpectrumFilterforDuplexChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (Sinker != null && data is RawDfiq { GroupOffset: 0, ChannelIndex: 0 or -1 } dfIq)
        {
            if (Math.Abs(dfIq.Frequency / 1000000.0d - Frequency) > Epsilon ||
                Math.Abs(dfIq.Bandwidth / 1000.0d - Bandwidth) > Epsilon) return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection = new int[dfIq.Count * 2];
            Array.Copy(dfIq.DataColleciton, 0, IqCollection, 0, IqCollection.Length);
            Sinker.Receive(MemberwiseClone());
            IqCollection = null;
        }
    }

    #endregion

    #region 属性（参数）

    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public int Attenuation { get; private set; }
    public double SampleRate { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion
}

[Serializable]
internal class CharacterFilterforDuplexChannel : DataFilterBase
{
    #region 构造函数

    public CharacterFilterforDuplexChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfc dfc || dfc.AntennaIndex != _antennaIndex ||
                Math.Abs(dfc.Frequency / 1000000.0d - _frequency) > Epsilon ||
                Math.Abs(dfc.Bandwidth / 1000.0d - _bandwidth) > Epsilon) return;
            if (ValidCount != dfc.CharacterCount)
            {
                PhaseDiffs = null;
                _phaseDiffIntegs = null;
                Levels = null;
                _levelIntegs = null;
                _avgTimeCounter = 0;
            }

            ValidCount = dfc.CharacterCount;
            if (PhaseDiffs == null || PhaseDiffs.Length != dfc.Characters.Length)
            {
                PhaseDiffs = new short[dfc.Characters.Length];
                Levels = new short[dfc.Levels.Length];
            }

            if (_phaseDiffIntegs == null || _phaseDiffIntegs.Length != dfc.Characters.Length)
            {
                _phaseDiffIntegs = new List<short>[dfc.Characters.Length];
                for (var index = 0; index < _phaseDiffIntegs.Length; ++index)
                    _phaseDiffIntegs[index] = new List<short>();
                _levelIntegs = new List<short>[dfc.Levels.Length];
                for (var index = 0; index < _levelIntegs.Length; ++index) _levelIntegs[index] = new List<short>();
            }

            for (var index = 0; index < dfc.Characters.Length; ++index)
                _phaseDiffIntegs[index].Add(dfc.Characters[index]);
            for (var index = 0; index < dfc.Levels.Length; ++index) _levelIntegs?[index].Add(dfc.Levels[index]);
            if (++_avgTimeCounter >= _avgTimes)
            {
                for (var index = 0; index < _phaseDiffIntegs.Length; ++index)
                {
                    PhaseDiffs[index] = GetAverageAngle(_phaseDiffIntegs[index]);
                    _phaseDiffIntegs[index].Clear();
                }

                if (_levelIntegs != null)
                    for (var index = 0; index < _levelIntegs.Length; ++index)
                    {
                        if (Levels != null) Levels[index] = (short)_levelIntegs[index].Average(item => item);
                        _levelIntegs[index].Clear();
                    }

                IsAvgTimeComplete = true;
                _avgTimeCounter = 0;
                if (ValidCount > 1 && ValidCount == dfc.Spectra.Length)
                {
                    var midDfIndex = ValidCount / 2;
                    var halfDfWidth = (int)(_dfBandwidth / _bandwidth * ValidCount / 2);
                    var first = midDfIndex - halfDfWidth;
                    var last = midDfIndex + halfDfWidth;
                    first = first < 0 ? 0 : first;
                    last = last > ValidCount ? ValidCount : last;
                    if (first < last)
                    {
                        var max = -9999;
                        ValidDfIndex = first;
                        for (var index = first; index < last; ++index)
                            if (dfc.Spectra[index] > max)
                            {
                                max = dfc.Spectra[index];
                                ValidDfIndex = index;
                            }
                    }
                    else
                    {
                        ValidDfIndex = midDfIndex;
                    }
                }
            }
            else
            {
                IsAvgTimeComplete = false;
            }

            Level = dfc.Level;
            Spectrum = dfc.Spectra;
            Sinker.Receive(MemberwiseClone());
            if (IsAvgTimeComplete)
            {
                PhaseDiffs = null;
                _phaseDiffIntegs = null;
                Levels = null;
                _levelIntegs = null;
            }
        }
    }

    #endregion

    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    private int _avgTimeCounter;
    private List<short>[] _phaseDiffIntegs;
    private List<short>[] _levelIntegs;

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                _antennaIndex = value;
            }
        }
    }

    private double _frequency;

    public double Frequency
    {
        get => _frequency;
        set
        {
            lock (_stateLock)
            {
                _frequency = value;
            }
        }
    }

    private double _bandwidth;

    public double Bandwidth
    {
        get => _bandwidth;
        set
        {
            lock (_stateLock)
            {
                _bandwidth = value;
            }
        }
    }

    private double _dfBandwidth;

    public double DfBandwidth
    {
        get => _dfBandwidth;
        set
        {
            lock (_stateLock)
            {
                _dfBandwidth = value;
            }
        }
    }

    public int ValidDfIndex { get; private set; }
    private int _levelThreshold;

    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            lock (_stateLock)
            {
                _levelThreshold = value;
            }
        }
    }

    private int _qualityThrehold;

    public int QualityThreshold
    {
        get => _qualityThrehold;
        set
        {
            lock (_stateLock)
            {
                _qualityThrehold = value;
            }
        }
    }

    private int _avgTimes;

    public int AvgTimes
    {
        get => _avgTimes;
        set
        {
            lock (_stateLock)
            {
                if (value != _avgTimes)
                {
                    _avgTimes = value;
                    _avgTimeCounter = 0;
                    PhaseDiffs = null;
                    _phaseDiffIntegs = null;
                }
            }
        }
    }

    public bool IsAvgTimeComplete { get; private set; }

    public int ValidCount { get; private set; }

    public int Level { get; private set; }

    public short[] Levels { get; private set; }

    public short[] Spectrum { get; private set; }

    public short[] PhaseDiffs { get; private set; }

    #endregion
}

[Serializable]
internal class ScanDfCharacterFilterforDuplexChannel : DataFilterBase
{
    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    #endregion

    #region 构造函数

    public ScanDfCharacterFilterforDuplexChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfc dfc || dfc.AntennaIndex != _antennaIndex || (dfc.Frequency != _nextFrequency &&
                    Math.Abs(dfc.Frequency / 1000000.0d - _startFrequency) > Epsilon)) return;
            Offset = Utils.GetCurrIndex(dfc.Frequency / 1000000.0d, _startFrequency, _stepFrequency);
            Count = dfc.CharacterCount;
            PhaseDiffs = dfc.Characters;
            Spectrum = dfc.Spectra;
            Bandwidth = dfc.Bandwidth / 1000.0d;
            if (Offset + Count > Total) // 数据长度如果越界则直接丢弃
                return;
            _nextFrequency = dfc.Frequency + dfc.Spectra.Length * (int)(_stepFrequency * 1000.0d);
            if (_nextFrequency > _stopFrequency * 1000000.0d) _nextFrequency = (long)(_startFrequency * 1000000);
            Sinker.Receive(MemberwiseClone());
        }
    }

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                _antennaIndex = value;
            }
        }
    }

    private long _nextFrequency;
    private double _startFrequency;

    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            lock (_stateLock)
            {
                _startFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private double _stopFrequency;

    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            lock (_stateLock)
            {
                _stopFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private double _stepFrequency;

    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            lock (_stateLock)
            {
                _stepFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private int _levelThreshold;

    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            lock (_stateLock)
            {
                _levelThreshold = value;
            }
        }
    }

    private int _qualityThreshold;

    public int QualityThreshold
    {
        get => _qualityThreshold;
        set
        {
            lock (_stateLock)
            {
                _qualityThreshold = value;
            }
        }
    }

    public double Bandwidth { get; private set; }

    public int Total { get; private set; }

    public int Count { get; private set; }

    public int Offset { get; private set; }

    public short[] Spectrum { get; private set; }

    public short[] PhaseDiffs { get; private set; }

    #endregion
}

[Serializable]
internal class RawFilterforNineChannel : DataFilterBase
{
    #region 构造函数

    public RawFilterforNineChannel(IDataSinker sinker)
        : base(sinker)
    {
        _syncCode = -1;
        _dataFlags = 0x0;
        IqCollection = null;
    }

    #endregion

    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    [NonSerialized] private int _dataFlags;

    [NonSerialized] private int _syncCode;

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    public int EstimatedSseCount { get; set; }
    public int IntegratedSseTimes { get; set; }
    public int SseAutomaticMethod { get; set; }
    public float SseAutomaticCoe { get; set; }
    private double _frequency;

    public double Frequency
    {
        get => _frequency;
        set
        {
            lock (_stateLock)
            {
                if (Math.Abs(_frequency - value) > Epsilon)
                    lock (_stateLock)
                    {
                        _frequency = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private double _bandwidth;

    public double Bandwidth
    {
        get => _bandwidth;
        set
        {
            lock (_stateLock)
            {
                if (Math.Abs(_bandwidth - value) > Epsilon)
                    lock (_stateLock)
                    {
                        _bandwidth = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                if (_antennaIndex != value)
                    lock (_stateLock)
                    {
                        _antennaIndex = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    private int _dfSamplingCount;

    public int DfSamplingCount
    {
        get => _dfSamplingCount;
        set
        {
            lock (_stateLock)
            {
                if (_dfSamplingCount != value)
                    lock (_stateLock)
                    {
                        if (IqCollection != null) IqCollection = new int[ChannelCount * GroupCount * value * 2];
                        _dfSamplingCount = value;
                        _dataFlags = 0x0;
                    }
            }
        }
    }

    public int Attenuation { get; private set; }
    public double SampleRate { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion

    #region 重写基类方法

    public override void Reset()
    {
        base.Reset();
        lock (_stateLock)
        {
            _syncCode = -1;
            _dataFlags = 0x0;
            IqCollection = null;
        }
    }

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfiQforNine dfIq || Math.Abs(dfIq.Frequency / 1000000.0d - _frequency) > Epsilon ||
                Math.Abs(dfIq.Bandwidth / 1000.0d - _bandwidth) > Epsilon || dfIq.AntennaIndex != _antennaIndex ||
                dfIq.Count != _dfSamplingCount) return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection ??= new int[ChannelCount * GroupCount * _dfSamplingCount * 2];
            if (dfIq.SyncCode != _syncCode)
            {
                _dataFlags = 0x0;
                _syncCode = dfIq.SyncCode;
            }

            Array.Copy(dfIq.DataCollection, 0, IqCollection, dfIq.PacketOffset * dfIq.Count * 2,
                dfIq.DataCollection.Length);
            var length = dfIq.ChannelCount / dfIq.PacketCount + (dfIq.ChannelCount % dfIq.PacketCount == 0 ? 0 :
                dfIq.PacketOffset > 0 ? dfIq.ChannelCount % dfIq.PacketOffset : 0);
            for (var index = 0; index < length; ++index) _dataFlags |= 0x1 << (dfIq.PacketOffset + index);
            if (_dataFlags == (int)(Math.Pow(2, ChannelCount * GroupCount) - 1))
            {
                Sinker.Receive(MemberwiseClone());
                IqCollection = null;
                _dataFlags = 0x0;
            }
        }
    }

    #endregion
}

[Serializable]
internal class LevelFilterforNineChannel : DataFilterBase
{
    #region 构造函数

    public LevelFilterforNineChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        var dfIq = data as RawDfiQforNine;
        if (Sinker != null && dfIq?.PacketOffset == 0)
        {
            if (Math.Abs(dfIq.Frequency / 1000000.0d - Frequency) > Epsilon ||
                Math.Abs(dfIq.Bandwidth / 1000.0d - Bandwidth) > Epsilon) return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection = new int[dfIq.Count * 2];
            Array.Copy(dfIq.DataCollection, 0, IqCollection, 0, IqCollection.Length);
            Sinker.Receive(MemberwiseClone());
            IqCollection = null;
        }
    }

    #endregion

    #region 属性（参数）

    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public int Attenuation { get; private set; }
    public double SampleRate { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion
}

[Serializable]
internal class SpectrumFilterforNineChannel : DataFilterBase
{
    #region 构造函数

    public SpectrumFilterforNineChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        var dfIq = data as RawDfiQforNine;
        if (Sinker != null && dfIq?.PacketOffset == 0)
        {
            if (Math.Abs(dfIq.Frequency / 1000000.0d - Frequency) > Epsilon ||
                Math.Abs(dfIq.Bandwidth / 1000.0d - Bandwidth) > Epsilon) return;
            Attenuation = dfIq.Attenuation;
            SampleRate = dfIq.SampleRate / 1000.0d;
            IqCollection = new int[dfIq.Count * 2];
            Array.Copy(dfIq.DataCollection, 0, IqCollection, 0, IqCollection.Length);
            Sinker.Receive(MemberwiseClone());
            IqCollection = null;
        }
    }

    #endregion

    #region 属性（参数）

    public double Frequency { get; set; }
    public double Bandwidth { get; set; }
    public int Attenuation { get; private set; }
    public double SampleRate { get; private set; }
    public int[] IqCollection { get; private set; }

    #endregion
}

[Serializable]
internal class PhaseDifferenceFilterforNineChannel : DataFilterBase
{
    #region 构造函数

    public PhaseDifferenceFilterforNineChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfCforNine dfc || dfc.AntennaIndex != _antennaIndex ||
                Math.Abs(dfc.Frequency / 1000000.0d - _frequency) > Epsilon ||
                (!CalibratingRequest && Math.Abs(dfc.Bandwidth / 1000.0d - _bandwidth) > Epsilon)) return;
            if (CalibratingRequest && Math.Abs(dfc.Bandwidth - dfc.Frequency) <= Epsilon)
                Calibrating = true;
            else
                Calibrating = false;
            if (ValidCount != dfc.CharacterCount)
            {
                PhaseDiffs = null;
                _phaseDiffIntegs = null;
                _avgTimeCounter = 0;
            }

            ValidCount = dfc.CharacterCount;
            if (PhaseDiffs == null || PhaseDiffs.Length != dfc.Characters.Length)
                PhaseDiffs = new short[dfc.Characters.Length];
            if (_phaseDiffIntegs == null || _phaseDiffIntegs.Length != dfc.Characters.Length)
            {
                _phaseDiffIntegs = new List<short>[dfc.Characters.Length];
                for (var index = 0; index < _phaseDiffIntegs.Length; ++index)
                    _phaseDiffIntegs[index] = new List<short>();
            }

            for (var index = 0; index < dfc.Characters.Length; ++index)
                _phaseDiffIntegs[index].Add(dfc.Characters[index]);
            if (++_avgTimeCounter >= _avgTimes)
            {
                for (var index = 0; index < _phaseDiffIntegs.Length; ++index)
                {
                    PhaseDiffs[index] = GetAverageAngle(_phaseDiffIntegs[index]);
                    _phaseDiffIntegs[index].Clear();
                }

                IsAvgTimeComplete = true;
                _avgTimeCounter = 0;
                if (ValidCount > 1 && ValidCount == dfc.Spectra.Length)
                {
                    var midDfIndex = (int)Math.Ceiling(1.0d * ValidCount / 2);
                    var halfDfWidth = (int)(_dfBandwidth / _bandwidth * ValidCount / 2);
                    var first = midDfIndex - halfDfWidth;
                    var last = midDfIndex + halfDfWidth;
                    first = first < 0 ? 0 : first;
                    last = last > ValidCount ? ValidCount : last;
                    if (first < last)
                    {
                        var max = -9999;
                        ValidDfIndex = first;
                        for (var index = first; index < last; ++index)
                            if (dfc.Spectra[index] > max)
                            {
                                max = dfc.Spectra[index];
                                ValidDfIndex = index;
                            }
                    }
                    else
                    {
                        ValidDfIndex = midDfIndex;
                    }
                }
            }
            else
            {
                IsAvgTimeComplete = false;
            }

            Level = dfc.Level;
            Spectrum = dfc.Spectra;
            Sinker.Receive(MemberwiseClone());
            if (IsAvgTimeComplete)
            {
                PhaseDiffs = null;
                _phaseDiffIntegs = null;
            }
        }
    }

    #endregion

    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    private int _avgTimeCounter;
    private List<short>[] _phaseDiffIntegs;

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                _antennaIndex = value;
            }
        }
    }

    private double _frequency;

    public double Frequency
    {
        get => _frequency;
        set
        {
            lock (_stateLock)
            {
                _frequency = value;
            }
        }
    }

    private double _bandwidth;

    public double Bandwidth
    {
        get => _bandwidth;
        set
        {
            lock (_stateLock)
            {
                _bandwidth = value;
            }
        }
    }

    private double _dfBandwidth;

    public double DfBandwidth
    {
        get => _dfBandwidth;
        set
        {
            lock (_stateLock)
            {
                _dfBandwidth = value;
            }
        }
    }

    public int ValidDfIndex { get; private set; }
    private int _levelThreshold;

    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            lock (_stateLock)
            {
                _levelThreshold = value;
            }
        }
    }

    private int _qualityThrehold;

    public int QualityThreshold
    {
        get => _qualityThrehold;
        set
        {
            lock (_stateLock)
            {
                _qualityThrehold = value;
            }
        }
    }

    private int _avgTimes;

    public int AvgTimes
    {
        get => _avgTimes;
        set
        {
            lock (_stateLock)
            {
                if (value != _avgTimes)
                {
                    _avgTimes = value;
                    _avgTimeCounter = 0;
                    PhaseDiffs = null;
                    _phaseDiffIntegs = null;
                }
            }
        }
    }

    public bool IsAvgTimeComplete { get; private set; }

    public int ValidCount { get; private set; }

    public int Level { get; private set; }

    public short[] Spectrum { get; private set; }

    public short[] PhaseDiffs { get; private set; }

    public bool CalibratingRequest { get; private set; }

    private bool _calibrating;

    public bool Calibrating
    {
        get => _calibrating;
        set
        {
            lock (_stateLock)
            {
                if (_calibrating != value)
                {
                    _avgTimeCounter = 0;
                    PhaseDiffs = null;
                    _phaseDiffIntegs = null;
                }

                _calibrating = value;
            }
        }
    }

    #endregion
}

[Serializable]
internal class ScanDfPhaseDifferenceFilterforNineChannel : DataFilterBase
{
    #region 成员变量

    [NonSerialized] private readonly object _stateLock = new();

    #endregion

    #region 构造函数

    public ScanDfPhaseDifferenceFilterforNineChannel(IDataSinker sinker)
        : base(sinker)
    {
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        lock (_stateLock)
        {
            if (data is not RawDfCforNine dfc || dfc.AntennaIndex != _antennaIndex ||
                (dfc.Frequency != _nextFrequency &&
                 Math.Abs(dfc.Frequency / 1000000.0d - _startFrequency) > Epsilon)) return;
            Offset = Utils.GetCurrIndex(dfc.Frequency / 1000000.0d, _startFrequency, _stepFrequency);
            Count = dfc.CharacterCount;
            PhaseDiffs = dfc.Characters;
            Spectrum = dfc.Spectra;
            Bandwidth = dfc.Bandwidth / 1000.0d;
            if (Offset + Count > Total) // 数据长度如果越界则直接丢弃
                return;
            _nextFrequency = dfc.Frequency + dfc.Spectra.Length * (int)(_stepFrequency * 1000.0d);
            if (_nextFrequency > _stopFrequency * 1000000.0d) _nextFrequency = (long)(_startFrequency * 1000000);
            Sinker.Receive(MemberwiseClone());
        }
    }

    #endregion

    #region 属性（参数）

    public float Aperture { get; set; }
    public float AngleOffset { get; set; }
    public int ChannelCount { get; set; }
    public int GroupCount { get; set; }
    public int AngleCount { get; set; }
    private int _antennaIndex;

    public int AntennaIndex
    {
        get => _antennaIndex;
        set
        {
            lock (_stateLock)
            {
                _antennaIndex = value;
            }
        }
    }

    private long _nextFrequency;
    private double _startFrequency;

    public double StartFrequency
    {
        get => _startFrequency;
        set
        {
            lock (_stateLock)
            {
                _startFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private double _stopFrequency;

    public double StopFrequency
    {
        get => _stopFrequency;
        set
        {
            lock (_stateLock)
            {
                _stopFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private double _stepFrequency;

    public double StepFrequency
    {
        get => _stepFrequency;
        set
        {
            lock (_stateLock)
            {
                _stepFrequency = value;
                _nextFrequency = (long)(_startFrequency * 1000000);
                if (_stopFrequency > _startFrequency && _stepFrequency > Epsilon)
                    Total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency);
            }
        }
    }

    private int _levelThreshold;

    public int LevelThreshold
    {
        get => _levelThreshold;
        set
        {
            lock (_stateLock)
            {
                _levelThreshold = value;
            }
        }
    }

    private int _qualityThrehold;

    public int QualityThreshold
    {
        get => _qualityThrehold;
        set
        {
            lock (_stateLock)
            {
                _qualityThrehold = value;
            }
        }
    }

    public double Bandwidth { get; private set; }

    public int Total { get; private set; }

    public int Count { get; private set; }

    public int Offset { get; private set; }

    public short[] Spectrum { get; private set; }

    public short[] PhaseDiffs { get; private set; }

    #endregion
}

#endregion

#region 接收器

internal interface IDataSinker
{
    event EventHandler<DfRelatedDataArrivedEventArgs> DataArrived;
    void Receive(object data);
    void ClearData();
    void StartProcess();
    void StopProcess();
}

internal abstract class DataSinkerBase : IDataSinker
{
    #region 构造函数

    protected DataSinkerBase()
    {
        MaximumBufferedPhaseDiff = 100;
        _dataQueue = new MQueue<object>();
        BufferedPhaseDiffs = new Dictionary<int, IDictionary<int, float[]>>();
        MaximumQueueLength = -1;
    }

    #endregion

    #region 成员变量

    protected int MaximumQueueLength;
    protected int MaximumBufferedPhaseDiff;
    protected readonly IDictionary<int, IDictionary<int, float[]>> BufferedPhaseDiffs;
    private readonly MQueue<object> _dataQueue;
    private Thread _dataProcess;
    private readonly object _receivingLock = new();
    private bool _receivingData;

    private bool ReceivingData
    {
        get
        {
            lock (_receivingLock)
            {
                return _receivingData;
            }
        }
        set
        {
            lock (_receivingLock)
            {
                _receivingData = value;
            }
        }
    }

    private readonly object _stopThreadLock = new();
    private bool _stopThread;

    private bool StopThread
    {
        get
        {
            lock (_stopThreadLock)
            {
                return _stopThread;
            }
        }
        set
        {
            lock (_stopThreadLock)
            {
                _stopThread = value;
            }
        }
    }

    #endregion

    #region 事件与事件触发

    public event EventHandler<DfRelatedDataArrivedEventArgs> DataArrived;

    protected void RaiseDfRelatedDataArrived(object data)
    {
        DataArrived?.Invoke(this, new DfRelatedDataArrivedEventArgs(data));
    }

    #endregion

    #region 成员方法

    public virtual void Receive(object data)
    {
        if (ReceivingData)
        {
            if (MaximumQueueLength != -1 && _dataQueue.Count >= MaximumQueueLength) _dataQueue.Clear();
            _dataQueue.EnQueue(data);
        }
    }

    public void ClearData()
    {
        _dataQueue.Clear();
        BufferedPhaseDiffs.Clear();
    }

    public void StartProcess()
    {
        if (_dataProcess?.IsAlive != true)
        {
            _dataProcess = new Thread(Process)
            {
                IsBackground = true,
                Name = $"dfiq_data_sinker({GetHashCode()})"
            };
            StopThread = false;
            ReceivingData = true;
            _dataProcess.Start();
        }
    }

    public void StopProcess()
    {
        if (_dataProcess?.IsAlive == true)
        {
            ReceivingData = false;
            StopThread = true;
        }

        _dataProcess?.Join(2000);
    }

    private void Process()
    {
        while (!StopThread)
            try
            {
                var data = _dataQueue.DeQueue(100);
                if (data == null)
                {
                    Thread.Sleep(1);
                    continue;
                }

                var obj = ToData(data);
                if (obj != null)
                    RaiseDfRelatedDataArrived(obj);
                else
                    Thread.Sleep(1);
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException) return;
            }
    }

    protected abstract object ToData(object obj);

    #endregion
}

[Obsolete("Using DFindPhaseSinker instead.")]
internal class DFindRawSinkerforDuplexChannel : DataSinkerBase, IDisposable
{
#if WRITE_DATA
        #region 成员变量
		private FileStream _stream;
        #endregion
        #region 构造函数
		public DFindRawSinkerforDuplexChannel()
		{
			_stream = new FileStream("dfind_iq.dat", FileMode.Create, FileAccess.Write);
		}
        #endregion
#endif

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is RawFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as RawFilterforDuplexChannel;
#if WRITE_DATA
			var buffer = new byte[raw.IQCollection.Length * 4];
			Buffer.BlockCopy(raw.IQCollection, 0, buffer, 0, buffer.Length);
			_stream.Write(buffer, 0, buffer.Length);
			_stream.Flush();
#endif
        var p = Utilities.Log2N(raw!.DfSamplingCount);
        var length = 1 << p;
        var windowValue = new float[length];
        Utilities.Window(ref windowValue, WindowType.Hanning);
        var peakIndex = 0;
        var max = 0.0d;
#if (OUTPUT_LEVEL || OUTPUT_PHASE) && ENHANCED_DFIND
			var antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 4 }, { 0, 7 }, { 2, 0 }, { 5, 0 }, { 8, 0 }, { 8, 3 }, { 8, 6 }, { 8, 1 } };
#else
#endif
#if OUTPUT_LEVEL
			var antennaLevels = new float[18];
			Console.WriteLine("**************LEVELS*******************");
#endif
        var sourcePhases = new float[9];
        var ch1TimeDomainData = new float[length * 2];
        var ch2TimeDomainData = new float[length * 2];
        for (var index = 0; index < raw.GroupCount; ++index)
        {
            Array.Copy(raw.IqCollection, raw.ChannelCount * index * raw.DfSamplingCount * 2, ch1TimeDomainData, 0,
                length * 2);
            var ch1FrequencyDomainData = Utilities.GetWindowData(ch1TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch1FrequencyDomainData);
            Array.Copy(raw.IqCollection, (raw.ChannelCount * index + 1) * raw.DfSamplingCount * 2, ch2TimeDomainData, 0,
                length * 2);
            var ch2FrequencyDomainData = Utilities.GetWindowData(ch2TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch2FrequencyDomainData);
#if OUTPUT_LEVEL
				antennaLevels[2 * index] = Utilities.GetLevel(ch1TimeDomainData);
				antennaLevels[2 * index + 1] = Utilities.GetLevel(ch2TimeDomainData);
				Console.WriteLine("AntElem({0}, {1}) => {2}, {3}", antennaIndex[index, 0], antennaIndex[index, 1], antennaLevels[2 * index], antennaLevels[2 * index + 1]);
#endif
            for (var iqIndex = 0; index == 0 && iqIndex < length; ++iqIndex)
            {
                var level = ch1FrequencyDomainData[index].Magnitude;
                if (level > max)
                {
                    max = level;
                    peakIndex = iqIndex;
                }
            }

            var diff = ch2FrequencyDomainData[peakIndex] / ch1FrequencyDomainData[peakIndex];
            sourcePhases[index] += (float)(diff.Phase / Math.PI * 180);
        }
#if ENHANCED_DFIND
        var sourcePhasesTemp = new float[raw.GroupCount];
        var delta = sourcePhases[0] + sourcePhases[5] - sourcePhases[8];
        sourcePhasesTemp[0] = sourcePhases[8] - sourcePhases[5];
        sourcePhasesTemp[1] = delta - sourcePhases[3];
        sourcePhasesTemp[2] = sourcePhases[6] - sourcePhases[5];
        sourcePhasesTemp[3] = sourcePhases[1] - delta;
        sourcePhasesTemp[4] = delta - sourcePhases[4];
        sourcePhasesTemp[5] = sourcePhases[7] - sourcePhases[5];
        sourcePhasesTemp[6] = sourcePhases[2] - delta;
        sourcePhasesTemp[7] = delta - sourcePhases[5];
        sourcePhasesTemp[8] = sourcePhasesTemp[7] - sourcePhasesTemp[0];
#if OUTPUT_PHASE
			Console.WriteLine("**************PHASES*******************");
#endif
        for (var index = 0; index < raw.GroupCount; ++index)
        {
#if OUTPUT_PHASE
				Console.WriteLine("AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], sourcePhases[index]);
#endif
            sourcePhases[index] = sourcePhasesTemp[index];
        }
#endif
        var instantPhases = DdfUtilities.GetTransformedPhaseDiffs(sourcePhases);
        if (instantPhases == null || instantPhases.Length != raw.GroupCount * (raw.GroupCount - 1)) return null;
        var samplePhases = DdfUtilities.GetPhaseDiffsInTheory(raw.Frequency, raw.Aperture, raw.AngleCount);
        var ddf = DdfUtilities.GetDdfCorrelation(instantPhases, samplePhases, raw.AngleCount);
        if (ddf != null)
        {
            ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
            return new SDataDfind
                { Frequency = raw.Frequency, BandWidth = raw.Bandwidth, Azimuth = ddf.Azimuth, Quality = ddf.Quality };
        }

        return null;
    }

    #endregion

    public void Dispose()
    {
#if WRITE_DATA
			_stream.Close();
#endif
    }
}

[Obsolete("Using WBDFPhaseSinker instead.")]
internal class WbdfRawSinkerforDuplexChannel : DataSinkerBase
{
    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is RawFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as RawFilterforDuplexChannel;
        var exp = Utilities.Log2N(raw!.DfSamplingCount);
        var length = 1 << exp;
        var windowValue = new float[length];
        var windowCoe = Utilities.Window(ref windowValue, WindowType.Hanning);
        var sourceWbPhases = new float[9 * length];
        var ch1TimeDomainData = new float[length * 2];
        var ch2TimeDomainData = new float[length * 2];
        Complex[] ch1FrequencyDomainData = null;
        for (var index = 0; index < raw.GroupCount; ++index)
        {
            Array.Copy(raw.IqCollection, raw.ChannelCount * index * raw.DfSamplingCount * 2, ch1TimeDomainData, 0,
                length * 2);
            ch1FrequencyDomainData = Utilities.GetWindowData(ch1TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch1FrequencyDomainData);
            Array.Copy(raw.IqCollection, (raw.ChannelCount * index + 1) * raw.DfSamplingCount * 2, ch2TimeDomainData, 0,
                length * 2);
            var ch2FrequencyDomainData = Utilities.GetWindowData(ch2TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch2FrequencyDomainData);
            for (var iqIndex = 0; iqIndex < length; ++iqIndex)
            {
                var diff = ch2FrequencyDomainData[iqIndex] / ch1FrequencyDomainData[iqIndex];
                sourceWbPhases[index * length + iqIndex] += (float)(diff.Phase / Math.PI * 180);
            }
        }

        var efficientLength = (int)(length * raw.Bandwidth / raw.SampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        var wbdf = new SDataDfpan
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Azimuths = new float[efficientLength],
            Qualities = new float[efficientLength]
        };
        for (var index = 0; index < wbdf.Azimuths.Length; ++index)
        {
            wbdf.Azimuths[index] = -1.0f;
            wbdf.Qualities[index] = -1.0f;
        }

        var wbdfSpectrum = new SDataSpectrum
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Data = new short[efficientLength]
        };
        for (var index = 0; index < efficientLength; ++index)
        {
            wbdfSpectrum.Data[index] =
                (short)((20 * Math.Log10(ch1FrequencyDomainData![(efficientIndex + index) % length].Magnitude) -
                    20 * Math.Log10(length) + windowCoe + raw.Attenuation / 10.0f) * 10);
            var frequency = raw.Frequency - raw.Bandwidth / 1000.0d / 2.0d +
                            raw.Bandwidth / 1000.0d / efficientLength * index;
            var samplePhases = DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            var sourcePhases = new float[raw.GroupCount];
            for (var groupIndex = 0; groupIndex < raw.GroupCount; ++groupIndex)
            {
                var subIndex = groupIndex * length;
                sourcePhases[groupIndex] = sourceWbPhases[subIndex + (efficientIndex + index) % length];
            }
#if ENHANCED_DFIND
            var sourcePhasesTemp = new float[raw.GroupCount];
            var delta = sourcePhases[0] + sourcePhases[5] - sourcePhases[8];
            sourcePhasesTemp[0] = sourcePhases[8] - sourcePhases[5];
            sourcePhasesTemp[1] = delta - sourcePhases[3];
            sourcePhasesTemp[2] = sourcePhases[6] - sourcePhases[5];
            sourcePhasesTemp[3] = sourcePhases[1] - delta;
            sourcePhasesTemp[4] = delta - sourcePhases[4];
            sourcePhasesTemp[5] = sourcePhases[7] - sourcePhases[5];
            sourcePhasesTemp[6] = sourcePhases[2] - delta;
            sourcePhasesTemp[7] = delta - sourcePhases[5];
            sourcePhasesTemp[8] = sourcePhasesTemp[7] - sourcePhasesTemp[0];
            for (var groupIndex = 0; groupIndex < raw.GroupCount; ++groupIndex)
                sourcePhases[groupIndex] = sourcePhasesTemp[groupIndex];
#endif
            var instantPhases = DdfUtilities.GetTransformedPhaseDiffs(sourcePhases);
            if (instantPhases == null || instantPhases.Length != raw.GroupCount * (raw.GroupCount - 1)) return null;
            var ddf = DdfUtilities.GetDdfCorrelation(instantPhases, samplePhases, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                wbdf.Azimuths[index] = ddf.Azimuth;
                wbdf.Qualities[index] = ddf.Quality;
            }
        }

        return new List<object>(new object[] { wbdf, wbdfSpectrum });
    }

    #endregion
}

[Obsolete("Level is wrapped in DFindPhaseSinker or WBDFPhaseSinker, using the above classes instead.")]
internal class LevelSinkerforDuplexChannel : DataSinkerBase
{
    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is LevelFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as LevelFilterforDuplexChannel;
        if (raw?.IqCollection != null)
        {
            var iq = Array.ConvertAll(raw.IqCollection, item => (float)item);
            var level = Utilities.GetLevel(iq);
            level += raw.Attenuation / 10.0f;
            return new SDataLevel { Frequency = raw.Frequency, Bandwidth = raw.Bandwidth, Data = level };
        }

        return null;
    }

    #endregion
}

[Obsolete("Spectrum is wrapped in DFindPhaseSinker or WBDFPhaseSinker, using the above classes instead.")]
internal class SpectrumSinkerforDuplexChannel : DataSinkerBase
{
    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is SpectrumFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as SpectrumFilterforDuplexChannel;
        var iq = Array.ConvertAll(raw?.IqCollection!, item => (float)item);
        var exp = Utilities.Log2N(iq.Length / 2);
        var length = 1 << exp;
        var windowValue = new float[length];
        var coe = Utilities.Window(ref windowValue, WindowType.Hanning);
        var spectrum = Utilities.GetWindowData(iq, windowValue, length);
        Utilities.Fft(ref spectrum);
        var efficientLength = (int)(length * 1.0 * raw!.Bandwidth / raw.SampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        coe = (float)(-20 * Math.Log10(length) + raw.Attenuation / 10d);
        var spectrumEx = new float[length];
        for (var index = 0; index < length; ++index)
            spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
        var validSpectrum = new short[efficientLength];
        for (var index = 0; index < validSpectrum.Length; ++index)
            validSpectrum[index] = (short)((spectrumEx[(efficientIndex + index) % length] + coe) * 10);
        return new SDataSpectrum { Frequency = raw.Frequency, Span = raw.Bandwidth, Data = validSpectrum };
    }

    #endregion
}

internal class DFindCharacterSinkerforMonoDuplexChannel : DataSinkerBase
{
    #region 构造函数

    public DFindCharacterSinkerforMonoDuplexChannel(IDictionary<int, IDFindCalibration> dfindCalibrators = null,
        int dfAntennaRef = 2)
    {
        _dfindCalibrators = dfindCalibrators;
        _dfAntennaRef = dfAntennaRef;
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly int _dfAntennaRef;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is CharacterFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as CharacterFilterforDuplexChannel;
        var result = new List<object>();
        if (raw != null)
        {
            var level = new SDataLevel
                { Frequency = raw.Frequency, Bandwidth = raw.Bandwidth, Data = raw.Level / 10.0f };
            var spectrum = new SDataSpectrum { Frequency = raw.Frequency, Span = raw.Bandwidth, Data = raw.Spectrum };
            // for (var index = 0; index < spectrum.Data.Length; ++index)
            // {
            //     spectrum.Data[index] = raw.Spectrum[index] / 10.0f;
            // }
            result.AddRange(new object[] { level, spectrum });
            if ( /*raw.ValidCount != 1 || */level.Data < raw.LevelThreshold || !raw.IsAvgTimeComplete) return result;
        }

        // 单通道，尚不具备理论测向的条件
        if (raw is { ChannelCount: 1 } && _dfindCalibrators != null &&
            _dfindCalibrators.TryGetValue(raw.AntennaIndex, out var calibrator))
        {
            var sourceLevels = Array.ConvertAll(raw.Levels.Where((_, index) => index % 2 == 0).ToArray(),
                item => item / 10.0f);
            var sourceMinLevel = sourceLevels.Min();
            var sourceMaxLevel = sourceLevels.Max();
            for (var index = 0; index < sourceLevels.Length; ++index)
            {
                sourceLevels[index] -= sourceMinLevel;
                sourceLevels[index] /= sourceMaxLevel - sourceMinLevel;
                sourceLevels[index] *= 360;
            }

            var destinationLevelDiffs = DdfUtilities.GetTransformedLevelDiffs(sourceLevels);
            var destinationMinLevel = destinationLevelDiffs.Min();
            var destinationMaxLevel = destinationLevelDiffs.Max();
            for (var index = 0; index < destinationLevelDiffs.Length; ++index)
            {
                destinationLevelDiffs[index] -= destinationMinLevel;
                destinationLevelDiffs[index] /= destinationMaxLevel - destinationMinLevel;
                destinationLevelDiffs[index] *= 360;
            }

            var sampleLevelDiffs = calibrator.ReadData(raw.Frequency);
            var levelDdf =
                DdfUtilities.GetDdfCorrelationWithLevelDiffs(destinationLevelDiffs, sampleLevelDiffs, raw.AngleCount);
            if (levelDdf != null)
            {
                levelDdf.Azimuth = ((levelDdf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                var dfind = new SDataDfind
                {
                    Frequency = raw.Frequency,
                    BandWidth = raw.Bandwidth,
                    Azimuth = levelDdf.Azimuth,
                    Quality = levelDdf.Quality
                };
                if (dfind.Quality < raw.QualityThreshold) dfind.Azimuth = -1;
                result.Add(dfind);
            }

            return result;
        }

        var sourcePhaseDiffs = new float[raw!.GroupCount];
        if (raw.ValidCount == 1)
            for (var index = 0; index < raw.GroupCount; ++index)
            {
                sourcePhaseDiffs[index] = raw.PhaseDiffs[index] / 10.0f;
                sourcePhaseDiffs[index] = (sourcePhaseDiffs[index] % 360 + 360) % 360;
            }
        else
            for (var index = 0; index < raw.GroupCount; ++index)
            {
                sourcePhaseDiffs[index] = raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDfIndex] / 10.0f;
                sourcePhaseDiffs[index] = (sourcePhaseDiffs[index] % 360 + 360) % 360;
            }
#if OUTPUT_PHASE
			Console.WriteLine("**************PHASES*******************");
			int[,] antennaIndex = null;
			if (_dfAntennaRef == 0)
			{
				antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 0, 6 }, { 0, 7 }, { 0, 8 }, { 8, 1 } };
			}
			else if (_dfAntennaRef == 1)
			{
				antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 0, 6 }, { 0, 7 }, { 0, 8 }, { 8, 0 } };
			}
			else if (_dfAntennaRef == 2)
			{
				antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 4 }, { 0, 7 }, { 2, 0 }, { 5, 0 }, { 8, 0 }, { 8, 3 }, { 8, 6 }, { 8, 1 } };
			}
			else if (_dfAntennaRef == 5)
			{
				antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 1, 2 }, { 1, 3 }, { 2, 3 }, { 2, 4 }, { 3, 4 } };
			}
			for (var index = 0; index < raw.GroupCount; ++index)
			{
				Console.WriteLine("AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], sourcePhaseDiffs[index]);
			}
#endif
        if (_dfAntennaRef != 0)
        {
            var tempSourcePhaseDiffs = new float[raw.GroupCount];
            if (_dfAntennaRef == 1)
            {
                tempSourcePhaseDiffs[0] = sourcePhaseDiffs[0] - sourcePhaseDiffs[1];
                tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - sourcePhaseDiffs[2];
                tempSourcePhaseDiffs[2] = sourcePhaseDiffs[2] - sourcePhaseDiffs[3];
                tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
                tempSourcePhaseDiffs[4] = sourcePhaseDiffs[4] - sourcePhaseDiffs[5];
                tempSourcePhaseDiffs[5] = sourcePhaseDiffs[5] - sourcePhaseDiffs[6];
                tempSourcePhaseDiffs[6] = sourcePhaseDiffs[6] - sourcePhaseDiffs[7];
                tempSourcePhaseDiffs[7] = sourcePhaseDiffs[7] - sourcePhaseDiffs[0];
                tempSourcePhaseDiffs[8] = sourcePhaseDiffs[0] - sourcePhaseDiffs[7];
            }
            else if (_dfAntennaRef == 2)
            {
                var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[5] - sourcePhaseDiffs[8];
                tempSourcePhaseDiffs[0] = sourcePhaseDiffs[8] - sourcePhaseDiffs[5];
                tempSourcePhaseDiffs[1] = delta - sourcePhaseDiffs[3];
                tempSourcePhaseDiffs[2] = sourcePhaseDiffs[6] - sourcePhaseDiffs[5];
                tempSourcePhaseDiffs[3] = sourcePhaseDiffs[1] - delta;
                tempSourcePhaseDiffs[4] = delta - sourcePhaseDiffs[4];
                tempSourcePhaseDiffs[5] = sourcePhaseDiffs[7] - sourcePhaseDiffs[5];
                tempSourcePhaseDiffs[6] = sourcePhaseDiffs[2] - delta;
                tempSourcePhaseDiffs[7] = delta - sourcePhaseDiffs[5];
                tempSourcePhaseDiffs[8] = tempSourcePhaseDiffs[7] - tempSourcePhaseDiffs[0];
            }
            else if (_dfAntennaRef == 5)
            {
                var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[1] - sourcePhaseDiffs[3];
                tempSourcePhaseDiffs[0] = sourcePhaseDiffs[4] - sourcePhaseDiffs[2];
                tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - delta;
                tempSourcePhaseDiffs[2] = sourcePhaseDiffs[4] - delta;
                tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - delta;
                tempSourcePhaseDiffs[4] = sourcePhaseDiffs[1] - delta - sourcePhaseDiffs[4] + sourcePhaseDiffs[2];
                tempSourcePhaseDiffs[5] = sourcePhaseDiffs[2] - delta;
                tempSourcePhaseDiffs[6] = sourcePhaseDiffs[4] - sourcePhaseDiffs[1];
                tempSourcePhaseDiffs[7] = sourcePhaseDiffs[0] - delta;
                tempSourcePhaseDiffs[8] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
            }

            for (var index = 0; index < raw.GroupCount; ++index) sourcePhaseDiffs[index] = tempSourcePhaseDiffs[index];
        }

        var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
        if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
            BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
        if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
            BufferedPhaseDiffs[raw.AntennaIndex].Clear();
        var tempFrequency = (int)(raw.Frequency * 1000000);
        if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
        {
            if (_dfindCalibrators != null)
                BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                    _dfindCalibrators[raw.AntennaIndex].ReadData(raw.Frequency);
            else
                BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                    DdfUtilities.GetPhaseDiffsInTheory(raw.Frequency, raw.Aperture, raw.AngleCount);
        }

        var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
        var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
        if (ddf != null)
        {
            ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
            var dfind = new SDataDfind
                { Frequency = raw.Frequency, BandWidth = raw.Bandwidth, Azimuth = ddf.Azimuth, Quality = ddf.Quality };
            if (dfind.Quality < raw.QualityThreshold) dfind.Azimuth = -1;
            result.Add(dfind);
        }

        return result;
    }

    #endregion
}

internal class WbdfCharacterSinkerforDuplexChannel : DataSinkerBase
{
    #region 构造函数

    public WbdfCharacterSinkerforDuplexChannel(IDictionary<int, IDFindCalibration> dfindCalibrators = null,
        int dfAntennaRef = 2)
    {
        MaximumBufferedPhaseDiff = 2048 * 100;
        MaximumQueueLength = 150;
        _dfindCalibrators = dfindCalibrators;
        _dfAntennaRef = dfAntennaRef;
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly int _dfAntennaRef;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is CharacterFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as CharacterFilterforDuplexChannel;
        var result = new List<object>();
        var spectrum = new SDataSpectrum { Frequency = raw!.Frequency, Span = raw.Bandwidth, Data = raw.Spectrum };
        // for (var index = 0; index < spectrum.Data.Length; ++index)
        // {
        //     spectrum.Data[index] = raw.Spectrum[index] / 10.0f;
        // }
        result.Add(spectrum);
        if (!raw.IsAvgTimeComplete) return result;
        var length = raw.ValidCount;
        var groupCount = raw.GroupCount;
        var wbdf = new SDataDfpan
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Azimuths = new float[length],
            Qualities = new float[length]
        };
        for (var index = 0; index < wbdf.Azimuths.Length; ++index)
        {
            wbdf.Azimuths[index] = -1.0f;
            wbdf.Qualities[index] = -1.0f;
        }

        var sourceWbPhaseDiffs = new float[length * groupCount];
        for (var ptIndex = 0; ptIndex < length; ++ptIndex)
        {
            if (spectrum.Data[ptIndex] < raw.LevelThreshold * 10) continue;
            var sourcePhaseDiffs = new float[groupCount];
            for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex)
            {
                sourceWbPhaseDiffs[groupIndex * length + ptIndex] =
                    raw.PhaseDiffs[groupIndex * length + ptIndex] / 10.0f;
                sourceWbPhaseDiffs[groupIndex * length + ptIndex] =
                    (sourceWbPhaseDiffs[groupIndex * length + ptIndex] % 360 + 360) % 360;
                sourcePhaseDiffs[groupIndex] = sourceWbPhaseDiffs[groupIndex * length + ptIndex];
            }

            if (_dfAntennaRef != 0)
            {
                var tempSourcePhaseDiffs = new float[9];
                if (_dfAntennaRef == 1)
                {
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[0] - sourcePhaseDiffs[1];
                    tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[2] - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
                    tempSourcePhaseDiffs[4] = sourcePhaseDiffs[4] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[5] - sourcePhaseDiffs[6];
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[6] - sourcePhaseDiffs[7];
                    tempSourcePhaseDiffs[7] = sourcePhaseDiffs[7] - sourcePhaseDiffs[0];
                    tempSourcePhaseDiffs[8] = sourcePhaseDiffs[0] - sourcePhaseDiffs[7];
                }
                else if (_dfAntennaRef == 2)
                {
                    var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[5] - sourcePhaseDiffs[8];
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[8] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[1] = delta - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[6] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[1] - delta;
                    tempSourcePhaseDiffs[4] = delta - sourcePhaseDiffs[4];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[7] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[2] - delta;
                    tempSourcePhaseDiffs[7] = delta - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[8] = tempSourcePhaseDiffs[7] - tempSourcePhaseDiffs[0];
                }
                else if (_dfAntennaRef == 5)
                {
                    var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[1] - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[4] - sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - delta;
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[4] - delta;
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - delta;
                    tempSourcePhaseDiffs[4] = sourcePhaseDiffs[1] - delta - sourcePhaseDiffs[4] + sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[2] - delta;
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[4] - sourcePhaseDiffs[1];
                    tempSourcePhaseDiffs[7] = sourcePhaseDiffs[0] - delta;
                    tempSourcePhaseDiffs[8] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
                }

                for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex)
                    sourcePhaseDiffs[groupIndex] = tempSourcePhaseDiffs[groupIndex];
            }

            var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
            var frequency = raw.Frequency - raw.Bandwidth / 1000.0d / 2.0d + raw.Bandwidth / 1000.0d / length * ptIndex;
            if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
                BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
            if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
                BufferedPhaseDiffs[raw.AntennaIndex].Clear();
            var tempFrequency = (int)(frequency * 1000000);
            if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
            {
                if (_dfindCalibrators != null)
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        _dfindCalibrators[raw.AntennaIndex].ReadData(frequency);
                else
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            }

            var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
            var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                if (ddf.Quality < raw.QualityThreshold) ddf.Azimuth = -1;
                wbdf.Azimuths[ptIndex] = ddf.Azimuth;
                wbdf.Qualities[ptIndex] = ddf.Quality;
            }
        }

        result.Add(wbdf);
        return result;
    }

    #endregion
}

internal class ScanCharacterSinkerforDuplexChannel : DataSinkerBase
{
    #region 构造函数

    public ScanCharacterSinkerforDuplexChannel(IDictionary<int, IDFindCalibration> dfindCalibrators = null,
        int dfAntennaRef = 2)
    {
        MaximumBufferedPhaseDiff = 2048 * 100;
        MaximumQueueLength = 150;
        _dfindCalibrators = dfindCalibrators;
        _dfAntennaRef = dfAntennaRef;
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly int _dfAntennaRef;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is ScanDfCharacterFilterforDuplexChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as ScanDfCharacterFilterforDuplexChannel;
        var result = new List<object>();
        var length = raw!.Count;
        var scan = new SDataScan
        {
            StartFrequency = raw.StartFrequency,
            StopFrequency = raw.StopFrequency,
            StepFrequency = raw.StepFrequency,
            Offset = raw.Offset,
            Total = raw.Total,
            Data = new short[length]
        };
        var scanDf = new SDataDfScan
        {
            StartFrequency = raw.StartFrequency,
            StopFrequency = raw.StopFrequency,
            StepFrequency = raw.StepFrequency,
            Offset = raw.Offset,
            Count = length,
            Indices = new int[length],
            Azimuths = new float[length],
            Qualities = new float[length]
        };
        for (var index = 0; index < length; ++index)
        {
            scan.Data[index] = raw.Spectrum[index];
            scanDf.Indices[index] = raw.Offset + index;
            scanDf.Azimuths[index] = -1.0f;
            scanDf.Qualities[index] = -1.0f;
        }

        var groupCount = raw.GroupCount;
        var sourceWbPhaseDiffs = new float[length * groupCount];
        // float[] calibratedValues = null;
        for (var ptIndex = 0; ptIndex < length; ++ptIndex)
        {
            if (scan.Data[ptIndex] < raw.LevelThreshold * 10) continue;
            var sourcePhaseDiffs = new float[groupCount];
            for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex)
            {
                sourceWbPhaseDiffs[groupIndex * length + ptIndex] =
                    raw.PhaseDiffs[groupIndex * length + ptIndex] / 10.0f;
                sourceWbPhaseDiffs[groupIndex * length + ptIndex] =
                    (sourceWbPhaseDiffs[groupIndex * length + ptIndex] % 360 + 360) % 360;
                sourcePhaseDiffs[groupIndex] =
                    sourceWbPhaseDiffs
                        [groupIndex * length + ptIndex] /*- (float)(calibratedValues[groupIndex] / Math.PI * 180)*/;
            }

            if (_dfAntennaRef != 0)
            {
                var tempSourcePhaseDiffs = new float[9];
                if (_dfAntennaRef == 1)
                {
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[0] - sourcePhaseDiffs[1];
                    tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[2] - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
                    tempSourcePhaseDiffs[4] = sourcePhaseDiffs[4] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[5] - sourcePhaseDiffs[6];
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[6] - sourcePhaseDiffs[7];
                    tempSourcePhaseDiffs[7] = sourcePhaseDiffs[7] - sourcePhaseDiffs[0];
                    tempSourcePhaseDiffs[8] = sourcePhaseDiffs[0] - sourcePhaseDiffs[7];
                }
                else if (_dfAntennaRef == 2)
                {
                    var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[5] - sourcePhaseDiffs[8];
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[8] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[1] = delta - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[6] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[1] - delta;
                    tempSourcePhaseDiffs[4] = delta - sourcePhaseDiffs[4];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[7] - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[2] - delta;
                    tempSourcePhaseDiffs[7] = delta - sourcePhaseDiffs[5];
                    tempSourcePhaseDiffs[8] = tempSourcePhaseDiffs[7] - tempSourcePhaseDiffs[0];
                }
                else if (_dfAntennaRef == 5)
                {
                    var delta = sourcePhaseDiffs[0] + sourcePhaseDiffs[1] - sourcePhaseDiffs[3];
                    tempSourcePhaseDiffs[0] = sourcePhaseDiffs[4] - sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[1] = sourcePhaseDiffs[1] - delta;
                    tempSourcePhaseDiffs[2] = sourcePhaseDiffs[4] - delta;
                    tempSourcePhaseDiffs[3] = sourcePhaseDiffs[3] - delta;
                    tempSourcePhaseDiffs[4] = sourcePhaseDiffs[1] - delta - sourcePhaseDiffs[4] + sourcePhaseDiffs[2];
                    tempSourcePhaseDiffs[5] = sourcePhaseDiffs[2] - delta;
                    tempSourcePhaseDiffs[6] = sourcePhaseDiffs[4] - sourcePhaseDiffs[1];
                    tempSourcePhaseDiffs[7] = sourcePhaseDiffs[0] - delta;
                    tempSourcePhaseDiffs[8] = sourcePhaseDiffs[3] - sourcePhaseDiffs[4];
                }

                for (var groupIndex = 0; groupIndex < groupCount; ++groupIndex)
                    sourcePhaseDiffs[groupIndex] = tempSourcePhaseDiffs[groupIndex];
            }

            var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
            var frequency = raw.StartFrequency + raw.StepFrequency / 1000.0d * ptIndex;
            if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
                BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
            if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
                BufferedPhaseDiffs[raw.AntennaIndex].Clear();
            var tempFrequency = (int)(frequency * 1000000);
            if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
            {
                if (_dfindCalibrators != null)
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        _dfindCalibrators[raw.AntennaIndex].ReadData(frequency);
                else
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            }

            var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
            var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                if (ddf.Quality < raw.QualityThreshold) ddf.Azimuth = -1;
                scanDf.Azimuths[ptIndex] = ddf.Azimuth;
                scanDf.Qualities[ptIndex] = ddf.Quality;
            }
        }

        result.AddRange(new object[] { scan, scanDf });
        return result;
    }

    #endregion
}

[Obsolete("Using DFindPhaseSinkerforNine instead.")]
internal class DFindRawSinkerforNineChannel : DataSinkerBase, IDisposable
{
#if WRITE_DATA
        #region 成员变量
		private FileStream _stream;
        #endregion
        #region 构造函数
		public DFindRawSinkerforNineChannel()
		{
			_stream = new FileStream("dfind_iq.dat", FileMode.Create, FileAccess.Write);
		}
        #endregion
#endif

    #region 成员变量

    private readonly IChannelCalibration _channelCalibrator;

    #endregion

    #region 构造函数

    public DFindRawSinkerforNineChannel(IChannelCalibration channelCalibrator = null)
    {
        _channelCalibrator = channelCalibrator;
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is RawFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
#if WRITE_DATA
			var buffer = new byte[raw.IQCollection.Length * 4];
			Buffer.BlockCopy(raw.IQCollection, 0, buffer, 0, buffer.Length);
			_stream.Write(buffer, 0, buffer.Length);
			_stream.Flush();
#endif
        if (obj is RawFilterforNineChannel raw)
        {
            var p = Utilities.Log2N(raw.DfSamplingCount);
            var length = 1 << p;
            var windowValue = new float[length];
            Utilities.Window(ref windowValue, WindowType.Hanning);
            var peakIndex = 0;
            var max = 0.0d;
#if (OUTPUT_LEVEL || OUTPUT_PHASE)
			var antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 0, 6 }, { 0, 7 }, { 0, 8 }, { 8, 1 } };
#endif
#if OUTPUT_LEVEL
			var antennaLevels = new float[18];
			Console.WriteLine("**************LEVELS*******************");
#endif
            var sourcePhases = new float[9];
            var ch1TimeDomainData = new float[length * 2];
            var ch2TimeDomainData = new float[length * 2];
            Array.Copy(raw.IqCollection, 0, ch1TimeDomainData, 0, length * 2);
            var ch1FrequencyDomainData = Utilities.GetWindowData(ch1TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch1FrequencyDomainData);
            for (var index = 0; index < length; ++index)
            {
                var level = ch1FrequencyDomainData[index].Magnitude;
                if (level > max)
                {
                    max = level;
                    peakIndex = index;
                }
            }

            for (var index = 0; index < raw.ChannelCount - 1; ++index)
            {
                Array.Copy(raw.IqCollection, (index + 1) * raw.DfSamplingCount * 2, ch2TimeDomainData, 0, length * 2);
                var ch2FrequencyDomainData = Utilities.GetWindowData(ch2TimeDomainData, windowValue, length);
                Utilities.Fft(ref ch2FrequencyDomainData);
#if OUTPUT_LEVEL
				antennaLevels[2 * index] = Utilities.GetLevel(ch1TimeDomainData);
				antennaLevels[2 * index + 1] = Utilities.GetLevel(ch2TimeDomainData);
				Console.WriteLine("AntElem({0}, {1}) => {2}, {3}", antennaIndex[index, 0], antennaIndex[index, 1], antennaLevels[2 * index], antennaLevels[2 * index + 1]);
#endif
                var diff = ch2FrequencyDomainData[peakIndex] / ch1FrequencyDomainData[peakIndex];
                sourcePhases[index] += (float)(diff.Phase / Math.PI * 180);
            }

            sourcePhases[raw.ChannelCount - 1] = sourcePhases[raw.ChannelCount - 2] - sourcePhases[0];
            float[] calibratedValues = null;
            calibratedValues = _channelCalibrator != null
                ? _channelCalibrator.ReadData(raw.Frequency, raw.Bandwidth)
                : new float[raw.ChannelCount];
            if (calibratedValues.Length != 9) Array.Resize(ref calibratedValues, raw.ChannelCount);
            for (var index = 0; index < raw.ChannelCount; ++index)
                sourcePhases[index] -= (float)(calibratedValues[index] / Math.PI * 180);
#if OUTPUT_PHASE
			Console.WriteLine("**************PHASES*******************");
			for (var index = 0; index < 9; ++index)
			{
				Console.WriteLine("AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], sourcePhases[index]);
			}
#endif
            var instantPhases = DdfUtilities.GetTransformedPhaseDiffs(sourcePhases);
            if (instantPhases is not { Length: 36 }) return null;
            var samplePhases = DdfUtilities.GetPhaseDiffsInTheory(raw.Frequency, raw.Aperture, raw.AngleCount);
            var ddf = DdfUtilities.GetDdfCorrelation(instantPhases, samplePhases, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                return new List<object>
                {
                    new SDataDfind
                    {
                        Frequency = raw.Frequency, BandWidth = raw.Bandwidth, Azimuth = ddf.Azimuth,
                        Quality = ddf.Quality
                    }
                };
            }
        }

        return null;
    }

    #endregion

    public void Dispose()
    {
#if WRITE_DATA
			_stream.Close();
#endif
    }
}

[Obsolete("Using WBDFPhaseSinkerforNine instead.")]
internal class WbdfRawSinkerforNineChannel : DataSinkerBase
{
    #region 成员变量

    private readonly IChannelCalibration _channelCalibrator;

    #endregion

    #region 构造函数

    public WbdfRawSinkerforNineChannel(IChannelCalibration channelCalibrator = null)
    {
        MaximumQueueLength = 100;
        _channelCalibrator = channelCalibrator;
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is RawFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as RawFilterforNineChannel;
        var exp = Utilities.Log2N(raw!.DfSamplingCount);
        var length = 1 << exp;
        var windowValue = new float[length];
        var windowCoe = Utilities.Window(ref windowValue, WindowType.Hanning);
        var sourceWbPhases = new float[9 * length];
        var ch1TimeDomainData = new float[length * 2];
        var ch2TimeDomainData = new float[length * 2];
        Array.Copy(raw.IqCollection, 0, ch1TimeDomainData, 0, length * 2);
        var ch1FrequencyDomainData = Utilities.GetWindowData(ch1TimeDomainData, windowValue, length);
        Utilities.Fft(ref ch1FrequencyDomainData);
        for (var index = 0; index < raw.ChannelCount - 1; ++index)
        {
            Array.Copy(raw.IqCollection, (index + 1) * raw.DfSamplingCount * 2, ch2TimeDomainData, 0, length * 2);
            var ch2FrequencyDomainData = Utilities.GetWindowData(ch2TimeDomainData, windowValue, length);
            Utilities.Fft(ref ch2FrequencyDomainData);
            for (var iqIndex = 0; iqIndex < length; ++iqIndex)
            {
                var diff = ch2FrequencyDomainData[iqIndex] / ch1FrequencyDomainData[iqIndex];
                sourceWbPhases[index * length + iqIndex] += (float)(diff.Phase / Math.PI * 180);
            }
        }

        for (var iqIndex = 0; iqIndex < length; ++iqIndex)
            sourceWbPhases[(raw.ChannelCount - 1) * length + iqIndex] =
                sourceWbPhases[(raw.ChannelCount - 2) * length + iqIndex] - sourceWbPhases[iqIndex];
        var efficientLength = (int)(length * raw.Bandwidth / raw.SampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        var wbdf = new SDataDfpan
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Azimuths = new float[efficientLength],
            Qualities = new float[efficientLength]
        };
        for (var index = 0; index < wbdf.Azimuths.Length; ++index)
        {
            wbdf.Azimuths[index] = -1.0f;
            wbdf.Qualities[index] = -1.0f;
        }

        var wbdfSpectrum = new SDataSpectrum
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Data = new short[efficientLength]
        };
        var calibratedValues = new float[raw.ChannelCount];
        for (var index = 0; index < efficientLength; ++index)
        {
            wbdfSpectrum.Data[index] =
                (short)((20 * Math.Log10(ch1FrequencyDomainData[(efficientIndex + index) % length].Magnitude) -
                    20 * Math.Log10(length) + windowCoe + raw.Attenuation / 10.0f) * 10);
            var frequency = raw.Frequency - raw.Bandwidth / 1000.0d / 2.0d +
                            raw.Bandwidth / 1000.0d / efficientLength * index;
            var samplePhases = DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            if (_channelCalibrator != null)
            {
                calibratedValues = _channelCalibrator.ReadData(frequency, raw.Bandwidth);
                if (calibratedValues.Length != raw.ChannelCount) Array.Resize(ref calibratedValues, raw.ChannelCount);
            }

            var sourcePhases = new float[raw.ChannelCount];
            for (var channelIndex = 0; channelIndex < raw.ChannelCount; ++channelIndex)
            {
                var subIndex = channelIndex * length;
                sourcePhases[channelIndex] = sourceWbPhases[subIndex + (efficientIndex + index) % length] -
                                             (float)(calibratedValues[channelIndex] / Math.PI * 180);
            }

            var instantPhases = DdfUtilities.GetTransformedPhaseDiffs(sourcePhases);
            if (instantPhases is not { Length: 36 }) return null;
            var ddf = DdfUtilities.GetDdfCorrelation(instantPhases, samplePhases, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                wbdf.Azimuths[index] = ddf.Azimuth;
                wbdf.Qualities[index] = ddf.Quality;
            }
        }

        return new List<object> { wbdf, wbdfSpectrum };
    }

    #endregion
}

internal class LevelSinkerforNineChannel : DataSinkerBase
{
    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is LevelFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as LevelFilterforNineChannel;
        if (raw?.IqCollection != null)
        {
            var iq = Array.ConvertAll(raw.IqCollection, item => (float)item);
            var level = Utilities.GetLevel(iq);
            level += raw.Attenuation / 10.0f;
            return new List<object>
                { new SDataLevel { Frequency = raw.Frequency, Bandwidth = raw.Bandwidth, Data = level } };
        }

        return null;
    }

    #endregion
}

internal class SpectrumSinkerforNineChannel : DataSinkerBase
{
    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is SpectrumFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as SpectrumFilterforNineChannel;
        var iq = Array.ConvertAll(raw?.IqCollection!, item => (float)item);
        var exp = Utilities.Log2N(iq.Length / 2);
        var length = 1 << exp;
        var windowValue = new float[length];
        var coe = Utilities.Window(ref windowValue, WindowType.Hanning);
        var spectrum = Utilities.GetWindowData(iq, windowValue, length);
        Utilities.Fft(ref spectrum);
        var efficientLength = (int)(length * 1.0 * raw!.Bandwidth / raw.SampleRate + 0.5);
        var efficientIndex = length - efficientLength / 2;
        coe = (float)(-20 * Math.Log10(length) + raw.Attenuation / 10d);
        var spectrumEx = new float[length];
        for (var index = 0; index < length; ++index)
            spectrumEx[index] = (float)(20 * Math.Log10(spectrum[index].Magnitude));
        var validSpectrum = new short[efficientLength];
        for (var index = 0; index < validSpectrum.Length; ++index)
            validSpectrum[index] = (short)((spectrumEx[(efficientIndex + index) % length] + coe) * 10);
        return new List<object>
            { new SDataSpectrum { Frequency = raw.Frequency, Span = raw.Bandwidth, Data = validSpectrum } };
    }

    #endregion
}

internal class SseRawSinkerforNineChannel : DataSinkerBase, IDisposable
{
#if WRITE_DATA
		private FileStream _stream = new FileStream("iq.dat", FileMode.Create);
		private int _counter = 10;
#endif

    #region 成员变量

    private readonly IChannelCalibration _channelCalibrator;
    private readonly IDictionary<int, float[]> _channelPhaseDiffDic;
    private readonly int[] _sortedValidIfBandwidth;
    private int _countOfAngles;
    private readonly Dictionary<int, int> _countOfAnglesDic = new();

    #endregion

    #region 构造函数

    public SseRawSinkerforNineChannel(IChannelCalibration channelCalibrator = null,
        IDictionary<int, float[]> channelPhaseDiffDic = null)
    {
        _channelCalibrator = channelCalibrator;
        _channelPhaseDiffDic = channelPhaseDiffDic;
        if (channelPhaseDiffDic?.Count > 0)
        {
            _sortedValidIfBandwidth = channelPhaseDiffDic.Keys.ToArray();
            Array.Sort(_sortedValidIfBandwidth, (x, y) => x.CompareTo(y));
        }
    }

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is RawFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as RawFilterforNineChannel;
#if WRITE_DATA
			if (_counter > 0)
			{
				var buffer = new byte[raw.IQCollection.Length * 4];
				Buffer.BlockCopy(raw.IQCollection, 0, buffer, 0, buffer.Length);
				_stream.Write(buffer, 0, buffer.Length);
				_counter--;
			}
			else if (_stream != null)
			{
				_stream.Close();
				_stream = null;
			}
#endif
        var diffs = GetChannelPhaseDiffs(raw!.Bandwidth);
        if (diffs == null)
            diffs = new float[raw.ChannelCount];
        else
            Array.Resize(ref diffs, raw.ChannelCount);
        var frequencyByHz = (long)(raw.Frequency * 1000000);
        var results = new float[720];
        float[] calibratedValues = null;
        if (_channelCalibrator != null)
        {
            if (!_channelCalibrator.Calibrating)
            {
                calibratedValues = _channelCalibrator.ReadData(raw.Frequency, raw.Bandwidth);
                if (calibratedValues == null) return null;
            }

            for (var index = 0; index < diffs.Length; ++index)
                calibratedValues![index] += (float)(diffs[index] / 180.0d * Math.PI);
        }
        else
        {
            calibratedValues = new float[raw.ChannelCount];
        }

        if (calibratedValues!.Length != 9) Array.Resize(ref calibratedValues, raw.ChannelCount);
        try
        {
            if (raw.EstimatedSseCount == -1)
            {
                var countOfAngles = DllInvoker.EstimateAngleCount(raw.IqCollection, raw.DfSamplingCount,
                    calibratedValues, false, raw.SseAutomaticCoe, raw.SseAutomaticMethod);
                if (_countOfAnglesDic.ContainsKey(countOfAngles))
                    _countOfAnglesDic[countOfAngles]++;
                else
                    _countOfAnglesDic[countOfAngles] = 1;
                var total = _countOfAnglesDic.Sum(item => item.Value);
                if (total > raw.IntegratedSseTimes)
                {
                    var maxValue = _countOfAnglesDic.Max(item => item.Value);
                    _countOfAngles = _countOfAnglesDic.First(item => item.Value == maxValue).Key;
                    if (_countOfAngles > 9) _countOfAngles = 9;
                    _countOfAnglesDic.Clear();
                }
            }
            else
            {
                _countOfAngles = raw.EstimatedSseCount;
            }

            _ = DllInvoker.GetSSE(results, _countOfAngles < 0 ? 0 : _countOfAngles, frequencyByHz, raw.Aperture,
                raw.IqCollection, raw.DfSamplingCount, calibratedValues);
        }
        catch
        {
            return null;
        }

        var angleOffsetIndex = (int)(raw.AngleOffset / (360.0f / results.Length));
        if (angleOffsetIndex < 0) angleOffsetIndex += results.Length;
        var sse = new SDataSse
        {
            Frequency = raw.Frequency,
            Bandwidth = raw.Bandwidth,
            Data = new float[results.Length],
            AzimuthCount = _countOfAngles < 0 ? 0 : _countOfAngles
        };
        for (var index = results.Length - 1; index >= 0; --index)
            sse.Data[(index + angleOffsetIndex) % results.Length] = results[index] * 100;
        return new List<object> { sse };
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
#if WRITE_DATA
			_stream.Close();
#endif
    }

    #endregion

    #region Helper

    private float[] GetChannelPhaseDiffs(double bandwidth)
    {
        if (_channelPhaseDiffDic != null)
            foreach (var t in _sortedValidIfBandwidth)
                if ((int)Math.Round(bandwidth) <= t)
                    return _channelPhaseDiffDic[t];

        return null;
    }

    #endregion
}

internal class DFindPhaseDifferenceSinkerforNineChannel : DataSinkerBase
{
    #region 构造函数

    public DFindPhaseDifferenceSinkerforNineChannel(IDictionary<int, IDFindCalibration> dfindCalibrators = null,
        IChannelCalibration channelCalibrator = null, IBandCalibration bandCalibrator = null)
    {
        _dfindCalibrators = dfindCalibrators;
        _channelCalibrator = channelCalibrator;
        _bandCalibrator = bandCalibrator;
        _bandPhaseDiffsInCenter = _bandCalibrator?.ReadData(_bandCalibrator.Total / 2);
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly IChannelCalibration _channelCalibrator;
    private readonly IBandCalibration _bandCalibrator;
    private readonly float[] _bandPhaseDiffsInCenter;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is PhaseDifferenceFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as PhaseDifferenceFilterforNineChannel;
        var result = new List<object>();
        var level = new SDataLevel { Frequency = raw!.Frequency, Bandwidth = raw.Bandwidth, Data = raw.Level / 10.0f };
        var spectrum = new SDataSpectrum { Frequency = raw.Frequency, Span = raw.Bandwidth, Data = raw.Spectrum };
        // for (var index = 0; index < spectrum.Data.Length; ++index)
        // {
        //     spectrum.Data[index] = raw.Spectrum[index] / 10.0f;
        // }
        result.AddRange(new object[] { level, spectrum });
#if OUTPUT_PHASE
			var antennaIndex =
 new int[9, 2] { { 0, 1 }, { 0, 2 }, { 0, 3 }, { 0, 4 }, { 0, 5 }, { 0, 6 }, { 0, 7 }, { 0, 8 }, { 8, 1 } };
			Console.WriteLine("**************PHASES*******************");
			if (raw.ValidCount == 1)
			{
				for (var index = 0; index < raw.ChannelCount; ++index)
				{
					Console.WriteLine("AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], raw.PhaseDiffs[index] / 10.0f);
				}
			}
			else
			{
				for(var index = 0; index < raw.ChannelCount; ++index)
				{
					Console.WriteLine("AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDFIndex] / 10.0f);
				}
			}
#endif
        float[] calibratedValues = null;
        if (_channelCalibrator != null)
        {
            if (_channelCalibrator.Calibrating)
            {
                if (raw.Calibrating && raw.IsAvgTimeComplete)
                {
                    var phaseDiffs = new float[raw.ChannelCount];
                    if (raw.ValidCount == 1)
                        for (var index = 0; index < raw.ChannelCount; ++index)
                            phaseDiffs[index] =
                                (float)((raw.PhaseDiffs[index] / 10.0f % 360 + 360) % 360 / 180.0d * Math.PI);
                    else
                        for (var index = 0; index < raw.ChannelCount; ++index)
                            phaseDiffs[index] =
                                (float)((raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDfIndex] / 10.0f % 360 +
                                         360) % 360 / 180.0d * Math.PI);
                    _channelCalibrator.WriteData(raw.Frequency, raw.Bandwidth, phaseDiffs);
                }

                return null;
            }

            calibratedValues = _channelCalibrator.ReadData(raw.Frequency, raw.Bandwidth);
            if (calibratedValues == null) return null;
            if (calibratedValues.Length != raw.ChannelCount) Array.Resize(ref calibratedValues, raw.ChannelCount);
        }
        else
        {
            calibratedValues = new float[raw.ChannelCount];
        }

        if (raw.Calibrating || raw.CalibratingRequest) return null;
        if ( /*raw.ValidCount != 1 || */level.Data < raw.LevelThreshold || !raw.IsAvgTimeComplete) return result;
        var sourcePhaseDiffs = new float[raw.ChannelCount];
        if (raw.ValidCount == 1)
        {
            for (var index = 0; index < raw.ChannelCount; ++index)
            {
                sourcePhaseDiffs[index] =
                    raw.PhaseDiffs[index] / 10.0f - (float)(calibratedValues[index] / Math.PI * 180);
                sourcePhaseDiffs[index] = (sourcePhaseDiffs[index] % 360 + 360) % 360;
            }
        }
        else
        {
            var bandPhaseDiffsForValidIndex =
                _bandCalibrator.ReadData((int)(1.0d * raw.ValidDfIndex / raw.ValidCount * _bandCalibrator.Total));
            for (var index = 0; index < raw.ChannelCount; ++index)
            {
                sourcePhaseDiffs[index] = raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDfIndex] / 10.0f -
                                          (float)((bandPhaseDiffsForValidIndex[index] - _bandPhaseDiffsInCenter[index] +
                                                   calibratedValues[index]) / Math.PI * 180);
                // sourcePhaseDiffs[index] = raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDFIndex] / 10.0f - (float)(calibratedValues[index] / Math.PI * 180);
                sourcePhaseDiffs[index] = (sourcePhaseDiffs[index] % 360 + 360) % 360;
            }
        }
#if OUTPUT_PHASE
			for (var index = 0; index < raw.ChannelCount; ++index)
			{
				Console.WriteLine("Refined AntPhase({0}, {1}) => {2}", antennaIndex[index, 0], antennaIndex[index, 1], sourcePhaseDiffs[index]);
			}
#endif
        var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
        if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
            BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
        if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
            BufferedPhaseDiffs[raw.AntennaIndex].Clear();
        var tempFrequency = (int)(raw.Frequency * 1000000);
        if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
        {
            if (_dfindCalibrators != null)
                BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                    _dfindCalibrators[raw.AntennaIndex].ReadData(raw.Frequency);
            else
                BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                    DdfUtilities.GetPhaseDiffsInTheory(raw.Frequency, raw.Aperture, raw.AngleCount);
        }

        var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
        var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
        if (ddf != null)
        {
            ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
            var dfind = new SDataDfind
                { Frequency = raw.Frequency, BandWidth = raw.Bandwidth, Azimuth = ddf.Azimuth, Quality = ddf.Quality };
            if (dfind.Quality < raw.QualityThreshold) dfind.Azimuth = -1;
            result.Add(dfind);
        }

        return result;
    }

    #endregion
}

internal class WbdfPhaseDifferenceSinkerforNineChannel : DataSinkerBase
{
    #region 构造函数

    public WbdfPhaseDifferenceSinkerforNineChannel(IDictionary<int, IDFindCalibration> dfindCalibrators = null,
        IChannelCalibration channelCalibrator = null, IBandCalibration bandCalibrator = null)
    {
        MaximumBufferedPhaseDiff = 2048 * 100;
        MaximumQueueLength = 150;
        _dfindCalibrators = dfindCalibrators;
        _channelCalibrator = channelCalibrator;
        _bandCalibrator = bandCalibrator;
        _bandPhaseDiffsInCenter = _bandCalibrator?.ReadData(_bandCalibrator.Total / 2);
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly IChannelCalibration _channelCalibrator;
    private readonly IBandCalibration _bandCalibrator;
    private readonly float[] _bandPhaseDiffsInCenter;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is PhaseDifferenceFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as PhaseDifferenceFilterforNineChannel;
        var result = new List<object>();
        var spectrum = new SDataSpectrum { Frequency = raw!.Frequency, Span = raw.Bandwidth, Data = raw.Spectrum };
        // for (var index = 0; index < spectrum.Data.Length; ++index)
        // {
        //     spectrum.Data[index] = raw.Spectrum[index] / 10.0f;
        // }
        result.Add(spectrum);
        var length = raw.ValidCount;
        var channelCount = raw.ChannelCount;
        var wbdf = new SDataDfpan
        {
            Frequency = raw.Frequency,
            Span = raw.Bandwidth,
            Azimuths = new float[length],
            Qualities = new float[length]
        };
        for (var index = 0; index < wbdf.Azimuths.Length; ++index)
        {
            wbdf.Azimuths[index] = -1.0f;
            wbdf.Qualities[index] = -1.0f;
        }

        var sourceWbPhaseDiffs = new float[length * channelCount];
        float[] calibratedValues = null;
        if (_channelCalibrator != null)
        {
            if (_channelCalibrator.Calibrating)
            {
                if (raw.Calibrating && raw.IsAvgTimeComplete)
                {
                    var phaseDiffs = new float[raw.ChannelCount];
                    for (var index = 0; index < raw.ChannelCount; ++index)
                        phaseDiffs[index] =
                            (float)((raw.PhaseDiffs[index * raw.ValidCount + raw.ValidDfIndex] / 10.0f % 360 + 360) %
                                360 / 180.0d * Math.PI);
                    _channelCalibrator.WriteData(raw.Frequency, raw.Bandwidth, phaseDiffs);
                }

                return null;
            }

            calibratedValues = _channelCalibrator.ReadData(raw.Frequency, raw.Bandwidth);
            if (calibratedValues == null) return null;
            if (calibratedValues.Length != raw.ChannelCount) Array.Resize(ref calibratedValues, raw.ChannelCount);
        }
        else
        {
            calibratedValues = new float[raw.ChannelCount];
        }

        if (raw.Calibrating || raw.CalibratingRequest) return null;
        if (!raw.IsAvgTimeComplete) return result;
        for (var ptIndex = 0; ptIndex < length; ++ptIndex)
        {
            if (spectrum.Data[ptIndex] < raw.LevelThreshold * 10) continue;
            // var frequency = raw.Frequency - raw.Bandwidth / 1000.0d / 2.0d + raw.Bandwidth / 1000.0d / length * ptIndex;
            var frequency = (long)(raw.Frequency * 1000000.0d - raw.Bandwidth * 1000.0d / 2 +
                                   raw.Bandwidth * 1000.0d / (length - 1) * ptIndex) / 1000000.0d;
            var sourcePhaseDiffs = new float[channelCount];
            for (var channelIndex = 0; channelIndex < channelCount; ++channelIndex)
            {
                sourceWbPhaseDiffs[channelIndex * length + ptIndex] =
                    raw.PhaseDiffs[channelIndex * length + ptIndex] / 10.0f;
                // sourceWBPhaseDiffs[channelIndex * length + ptIndex] = (sourceWBPhaseDiffs[channelIndex * length + ptIndex] % 360 + 360) % 360;
                var bandPhaseDiffsForValidIndex =
                    _bandCalibrator.ReadData((int)(1.0d * ptIndex / length * _bandCalibrator.Total));
                // sourcePhaseDiffs[channelIndex] = sourceWBPhaseDiffs[channelIndex * length + ptIndex] - (float)(calibratedValues[channelIndex] / Math.PI * 180);
                sourcePhaseDiffs[channelIndex] = sourceWbPhaseDiffs[channelIndex * length + ptIndex] -
                                                 (float)((bandPhaseDiffsForValidIndex[channelIndex] -
                                                          _bandPhaseDiffsInCenter[channelIndex] +
                                                          calibratedValues[channelIndex]) / Math.PI * 180);
                sourcePhaseDiffs[channelIndex] = (sourcePhaseDiffs[channelIndex] % 360 + 360) % 360;
            }
#if OUTPUT_PHASE
				Console.WriteLine("*************************WBDF PHASE****************************");
				for (var index = 0; index < channelCount; ++index)
				{
					Console.WriteLine("FREQ: {2}, WBDF AntPhase({0}) => {1}", index, sourcePhaseDiffs[index], frequency);
				}
#endif
            var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
            if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
                BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
            if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
                BufferedPhaseDiffs[raw.AntennaIndex].Clear();
            var tempFrequency = (int)(frequency * 1000000);
            if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
            {
                if (_dfindCalibrators != null)
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        _dfindCalibrators[raw.AntennaIndex].ReadData(frequency);
                else
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            }

            var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
            var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                if (ddf.Quality < raw.QualityThreshold) ddf.Azimuth = -1;
                wbdf.Azimuths[ptIndex] = ddf.Azimuth;
                wbdf.Qualities[ptIndex] = ddf.Quality;
            }
        }

        result.Add(wbdf);
        return result;
    }

    #endregion
}

internal class ScanDfPhaseDifferenceSinkerforNineChannel : DataSinkerBase
{
    #region 构造函数

    public ScanDfPhaseDifferenceSinkerforNineChannel(IDictionary<int, IDFindCalibration> dfindCalibrators,
        IChannelCalibration channelCalibrator = null, IBandCalibration bandwidthCalibrator = null)
    {
        MaximumBufferedPhaseDiff = 2048 * 100;
        MaximumQueueLength = 250;
        _dfindCalibrators = dfindCalibrators;
        _channelCalibrator = channelCalibrator;
        _bandCalibrator = bandwidthCalibrator;
        _bandPhaseDiffsInCenter = _bandCalibrator?.ReadData(_bandCalibrator.Total / 2);
    }

    #endregion

    #region 成员变量

    private readonly IDictionary<int, IDFindCalibration> _dfindCalibrators;
    private readonly IChannelCalibration _channelCalibrator;
    private readonly IBandCalibration _bandCalibrator;
    private readonly float[] _bandPhaseDiffsInCenter;
    private int _lengthInBand;

    #endregion

    #region 重写基类方法

    public override void Receive(object data)
    {
        if (data is ScanDfPhaseDifferenceFilterforNineChannel) base.Receive(data);
    }

    protected override object ToData(object obj)
    {
        var raw = obj as ScanDfPhaseDifferenceFilterforNineChannel;
        var result = new List<object>();
        var length = raw!.Count;
        // 第一包数据的长度为所有包中最大值
        if (raw.Offset == 0) _lengthInBand = length;
        var scan = new SDataScan
        {
            StartFrequency = raw.StartFrequency,
            StopFrequency = raw.StopFrequency,
            StepFrequency = raw.StepFrequency,
            Offset = raw.Offset,
            Total = raw.Total,
            Data = new short[length]
        };
        var scanDf = new SDataDfScan
        {
            StartFrequency = raw.StartFrequency,
            StopFrequency = raw.StopFrequency,
            StepFrequency = raw.StepFrequency,
            Offset = raw.Offset,
            Count = length,
            Indices = new int[length],
            Azimuths = new float[length],
            Qualities = new float[length]
        };
        for (var index = 0; index < length; ++index)
        {
            scan.Data[index] = raw.Spectrum[index];
            scanDf.Indices[index] = raw.Offset + index;
            scanDf.Azimuths[index] = -1.0f;
            scanDf.Qualities[index] = -1.0f;
        }

        var channelCount = raw.ChannelCount;
        var sourceWbPhaseDiffs = new float[length * channelCount];
        // 获取当前包的中心频率，用于计算包内的带内误差
        var centerFrequency =
            (raw.StartFrequency * 1000000 + raw.StepFrequency * 1000 * (raw.Offset + _lengthInBand / 2d)) / 1000000.0d;
        float[] calibratedValues = null;
        if (_channelCalibrator != null)
        {
            calibratedValues = _channelCalibrator.ReadData(centerFrequency, raw.Bandwidth);
            if (calibratedValues.Length != raw.ChannelCount) Array.Resize(ref calibratedValues, raw.ChannelCount);
        }

        for (var ptIndex = 0; ptIndex < length; ++ptIndex)
        {
            if (scan.Data[ptIndex] < raw.LevelThreshold * 10) continue;
            // var frequency = raw.StartFrequency + raw.StepFrequency / 1000.0d * ptIndex;
            var frequency = (raw.StartFrequency * 1000000 + raw.StepFrequency * 1000 * (raw.Offset + ptIndex)) /
                            1000000.0d;
            var sourcePhaseDiffs = new float[channelCount];
            for (var channelIndex = 0; channelIndex < channelCount; ++channelIndex)
            {
                sourceWbPhaseDiffs[channelIndex * length + ptIndex] =
                    raw.PhaseDiffs[channelIndex * length + ptIndex] / 10.0f;
                sourceWbPhaseDiffs[channelIndex * length + ptIndex] =
                    (sourceWbPhaseDiffs[channelIndex * length + ptIndex] % 360 + 360) % 360;
                var bandPhaseDiffsForValidIndex =
                    _bandCalibrator.ReadData((int)(1.0d * ptIndex / _lengthInBand * _bandCalibrator.Total));
                // sourcePhaseDiffs[channelIndex] = sourceWBPhaseDiffs[channelIndex * length + ptIndex] - (float)(calibratedValues[channelIndex] / Math.PI * 180);
                sourcePhaseDiffs[channelIndex] = sourceWbPhaseDiffs[channelIndex * length + ptIndex] -
                                                 (float)((bandPhaseDiffsForValidIndex[channelIndex] -
                                                          _bandPhaseDiffsInCenter[channelIndex] +
                                                          calibratedValues![channelIndex]) / Math.PI * 180);
                sourcePhaseDiffs[channelIndex] = (sourcePhaseDiffs[channelIndex] % 360 + 360) % 360;
            }
#if OUTPUT_PHASE
				Console.WriteLine("*************************SCAN DF PHASE****************************");
				for (var index = 0; index < channelCount; ++index)
				{
					Console.WriteLine("Center Freq: {2}, ScanDF AntPhase({0}) => {1}", index, sourcePhaseDiffs[index], frequency);
				}
#endif
            var destinationPhaseDiffs = DdfUtilities.GetTransformedPhaseDiffs(sourcePhaseDiffs);
            if (!BufferedPhaseDiffs.ContainsKey(raw.AntennaIndex))
                BufferedPhaseDiffs[raw.AntennaIndex] = new Dictionary<int, float[]>();
            if (BufferedPhaseDiffs[raw.AntennaIndex].Count > MaximumBufferedPhaseDiff)
                BufferedPhaseDiffs[raw.AntennaIndex].Clear();
            var tempFrequency = (int)(frequency * 1000000);
            if (!BufferedPhaseDiffs[raw.AntennaIndex].ContainsKey(tempFrequency))
            {
                if (_dfindCalibrators != null)
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        _dfindCalibrators[raw.AntennaIndex].ReadData(frequency);
                else
                    BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency] =
                        DdfUtilities.GetPhaseDiffsInTheory(frequency, raw.Aperture, raw.AngleCount);
            }

            var samplePhaseDiffs = BufferedPhaseDiffs[raw.AntennaIndex][tempFrequency];
            var ddf = DdfUtilities.GetDdfCorrelation(destinationPhaseDiffs, samplePhaseDiffs, raw.AngleCount);
            if (ddf != null)
            {
                ddf.Azimuth = ((ddf.Azimuth + raw.AngleOffset) % 360 + 360) % 360;
                if (ddf.Quality < raw.QualityThreshold) ddf.Azimuth = -1;
                scanDf.Azimuths[ptIndex] = ddf.Azimuth;
                scanDf.Qualities[ptIndex] = ddf.Quality;
            }
        }

        result.AddRange(new object[] { scan, scanDf });
        return result;
    }

    #endregion
}

#endregion

#region Helper Classes

internal enum WindowType
{
    Rectangle,
    Hanning,
    Blackman
}

internal class DdfCorrelation
{
    public float Azimuth { get; set; }
    public float Quality { get; set; }
    public int Angles { get; set; }
    public int AngleIndex { get; set; }
    public float[] Correlations { get; set; }
}

internal static class Utilities
{
    public static int Log2N(int value)
    {
        var n = 0;
        while ((value >>= 1) > 0) n++;
        return n;
    }

    public static float Window(ref float[] data, WindowType windowType)
    {
        const float pi = (float)Math.PI;
        var coe = 0.0f;
        var length = data.Length;
        switch (windowType)
        {
            case WindowType.Rectangle:
                for (var i = 0; i < length; ++i) data[i] = 1;
                coe = -1;
                break;
            case WindowType.Hanning:
            {
                var pi2L = pi * 2.0f / length;
                for (var i = 0; i < length; ++i) data[i] = (float)(1 - Math.Cos(pi2L * i)) / 2;
            }
                coe = 5.08f;
                break;
            case WindowType.Blackman:
            {
                var pi2L = pi * 2.0f / length;
                var pi4L = pi * 4.0f / length;
                for (var i = 0; i < length; ++i)
                    data[i] = (float)(0.42f - 0.5f * Math.Cos(pi2L * i) + 0.08f * Math.Cos(pi4L * i));
            }
                coe = 6.60f;
                break;
        }

        return coe;
    }

    public static Complex[] GetWindowData(float[] data, float[] windowValue, int length)
    {
        if (data == null || data.Length != length * 2
                         || windowValue == null || windowValue.Length != length)
            return null;
        var outputData = new Complex[length];
        for (var index = 0; index < length; ++index)
        {
            outputData[index] = new Complex(data[2 * index], data[2 * index + 1]);
            outputData[index] *= windowValue[index];
        }

        return outputData;
    }

    public static void Fft(ref Complex[] x)
    {
        var log2N = Log2N(x.Length);
        int i;
        int l;
        Complex t;
        var n = 1 << log2N;
        var nv2 = n >> 1;
        var nm1 = n - 1;
        var j = 0;
        for (i = 0; i < nm1; i++)
        {
            if (i < j)
            {
                t = x[j];
                x[j] = x[i];
                x[i] = t;
            }

            var k = nv2;
            while (k <= j)
            {
                j -= k;
                k >>= 1;
            }

            j += k;
        }

        var pi = (float)Math.PI;
        for (l = 1; l <= log2N; l++)
        {
            var le = 1 << l;
            var le1 = le >> 1;
            var ain = pi / le1;
            var u = new Complex(1, 0);
            var w = new Complex(Math.Cos(ain), -Math.Sin(ain));
            for (j = 0; j < le1; j++)
            {
                for (i = j; i < n; i += le)
                {
                    var ip = i + le1;
                    t = x[ip] * u;
                    x[ip] = x[i] - t;
                    x[i] += t;
                }

                u *= w;
            }
        }
    }

    public static float GetLevel(float[] data)
    {
        if (data == null || data.Length % 2 != 0) return 0.0f;
        const double epsilon = 1.0E-7d;
        var length = data.Length / 2;
        var sum = 0.0d;
        var increment = 0;
        for (var index = 0; index < length; ++index)
        {
            var real = data[2 * index];
            var image = data[2 * index + 1];
            if (Math.Abs(real - 0.0f) > epsilon || Math.Abs(image - 0.0f) > epsilon)
            {
                sum += Math.Log10(real * real + image * image);
                increment++;
            }
        }

        if (increment == 0) return 0.0f;
        return (float)(10 * sum / increment);
    }
}

internal static class DdfUtilities
{
    #region 类成员变量

    private static readonly double[] _cosArray;

    #endregion

    #region 静态构造函数

    static DdfUtilities()
    {
        _cosArray = new double[3600];
        var sinArray = new double[3600];
        for (var index = 0; index < 3600; ++index)
        {
            _cosArray[index] = Math.Cos(index / 1800.0d * Math.PI);
            sinArray[index] = Math.Sin(index / 1800.0d * Math.PI);
        }
    }

    #endregion

    #region Helper

    private static float[] GetPhaseDiffsByOffsetInTheory(double frequency, float aperture, int angles, int angleOffset)
    {
        var samples = new float[36];
        var angleStep = 360.0f / angles;
        var angle = angleStep * angleOffset;
        const double radValue = Math.PI / 180.0d;
        const double coe = Math.PI / 150.0d;
        samples[0] = (float)(coe * Math.Sin(20 * radValue) * Math.Sin((angle - 60) * radValue));
        samples[1] = (float)(coe * Math.Sin(40 * radValue) * Math.Sin((angle - 80) * radValue));
        samples[2] = (float)(coe * Math.Sin(60 * radValue) * Math.Sin((angle - 100) * radValue));
        samples[3] = (float)(coe * Math.Sin(80 * radValue) * Math.Sin((angle - 120) * radValue));
        samples[4] = (float)(coe * Math.Sin(80 * radValue) * Math.Sin((angle - 140) * radValue));
        samples[5] = (float)(coe * Math.Sin(60 * radValue) * Math.Sin((angle - 160) * radValue));
        samples[6] = (float)(-1 * coe * Math.Sin(40 * radValue) * Math.Sin(angle * radValue));
        samples[7] = (float)(coe * Math.Sin(20 * radValue) * Math.Sin((angle - 100) * radValue));
        samples[8] = (float)(coe * Math.Sin(40 * radValue) * Math.Sin((angle - 120) * radValue));
        samples[9] = (float)(coe * Math.Sin(60 * radValue) * Math.Sin((angle - 140) * radValue));
        samples[10] = (float)(coe * Math.Sin(80 * radValue) * Math.Sin((angle - 160) * radValue));
        samples[11] = (float)(-1 * coe * Math.Sin(80 * radValue) * Math.Sin(angle * radValue));
        samples[12] = (float)(-1 * coe * Math.Sin(60 * radValue) * Math.Sin((angle - 20) * radValue));
        samples[13] = (float)(coe * Math.Sin(20 * radValue) * Math.Sin((angle - 140) * radValue));
        samples[14] = (float)(coe * Math.Sin(40 * radValue) * Math.Sin((angle - 160) * radValue));
        samples[15] = (float)(-1 * coe * Math.Sin(60 * radValue) * Math.Sin(angle * radValue));
        samples[16] = (float)(-1 * coe * Math.Sin(80 * radValue) * Math.Sin((angle - 20) * radValue));
        samples[17] = (float)(-1 * coe * Math.Sin(80 * radValue) * Math.Sin((angle - 40) * radValue));
        samples[18] = (float)(-1 * coe * Math.Sin(20 * radValue) * Math.Sin(angle * radValue));
        samples[19] = (float)(-1 * coe * Math.Sin(40 * radValue) * Math.Sin((angle - 20) * radValue));
        samples[20] = (float)(-1 * coe * Math.Sin(60 * radValue) * Math.Sin((angle - 40) * radValue));
        samples[21] = (float)(-1 * coe * Math.Sin(80 * radValue) * Math.Sin((angle - 60) * radValue));
        samples[22] = (float)(-1 * coe * Math.Sin(20 * radValue) * Math.Sin((angle - 40) * radValue));
        samples[23] = (float)(-1 * coe * Math.Sin(40 * radValue) * Math.Sin((angle - 60) * radValue));
        samples[24] = (float)(-1 * coe * Math.Sin(60 * radValue) * Math.Sin((angle - 80) * radValue));
        samples[25] = (float)(-1 * coe * Math.Sin(20 * radValue) * Math.Sin((angle - 80) * radValue));
        samples[26] = (float)(-1 * coe * Math.Sin(40 * radValue) * Math.Sin((angle - 100) * radValue));
        samples[27] = (float)(-1 * coe * Math.Sin(20 * radValue) * Math.Sin((angle - 120) * radValue));
        samples[28] = (float)(coe * Math.Sin(20 * radValue) * Math.Sin((angle - 20) * radValue));
        samples[29] = (float)(coe * Math.Sin(40 * radValue) * Math.Sin((angle - 40) * radValue));
        samples[30] = (float)(coe * Math.Sin(60 * radValue) * Math.Sin((angle - 60) * radValue));
        samples[31] = (float)(coe * Math.Sin(80 * radValue) * Math.Sin((angle - 80) * radValue));
        samples[32] = (float)(coe * Math.Sin(80 * radValue) * Math.Sin((angle - 100) * radValue));
        samples[33] = (float)(coe * Math.Sin(60 * radValue) * Math.Sin((angle - 120) * radValue));
        samples[34] = (float)(coe * Math.Sin(40 * radValue) * Math.Sin((angle - 140) * radValue));
        samples[35] = (float)(coe * Math.Sin(20 * radValue) * Math.Sin((angle - 160) * radValue));
        for (var index = 0; index < 36; ++index)
        {
            samples[index] = (float)(samples[index] * frequency * aperture / radValue % 360);
            if (samples[index] > 180)
                samples[index] -= 360;
            else if (samples[index] < -180) samples[index] += 360;
        }

        return samples;
    }

    #endregion

    #region 类方法

    public static float[] GetTransformedPhaseDiffs(float[] sourcePhaseDiffs)
    {
        if (sourcePhaseDiffs is not { Length: 9 }) return null;
        for (var index = 0; index < sourcePhaseDiffs.Length; ++index)
            sourcePhaseDiffs[index] = (sourcePhaseDiffs[index] % 360 + 360) % 360;
        var destinationPhases = new float[36];
        destinationPhases[0] = sourcePhaseDiffs[1] - sourcePhaseDiffs[0];
        destinationPhases[1] = sourcePhaseDiffs[2] - sourcePhaseDiffs[0];
        destinationPhases[2] = sourcePhaseDiffs[3] - sourcePhaseDiffs[0];
        destinationPhases[3] = sourcePhaseDiffs[4] - sourcePhaseDiffs[0];
        destinationPhases[4] = sourcePhaseDiffs[5] - sourcePhaseDiffs[0];
        destinationPhases[5] = sourcePhaseDiffs[6] - sourcePhaseDiffs[0];
        destinationPhases[6] = sourcePhaseDiffs[7] - sourcePhaseDiffs[0];
        destinationPhases[7] = sourcePhaseDiffs[2] - sourcePhaseDiffs[1];
        destinationPhases[8] = sourcePhaseDiffs[3] - sourcePhaseDiffs[1];
        destinationPhases[9] = sourcePhaseDiffs[4] - sourcePhaseDiffs[1];
        destinationPhases[10] = sourcePhaseDiffs[5] - sourcePhaseDiffs[1];
        destinationPhases[11] = sourcePhaseDiffs[6] - sourcePhaseDiffs[1];
        destinationPhases[12] = sourcePhaseDiffs[7] - sourcePhaseDiffs[1];
        destinationPhases[13] = sourcePhaseDiffs[3] - sourcePhaseDiffs[2];
        destinationPhases[14] = sourcePhaseDiffs[4] - sourcePhaseDiffs[2];
        destinationPhases[15] = sourcePhaseDiffs[5] - sourcePhaseDiffs[2];
        destinationPhases[16] = sourcePhaseDiffs[6] - sourcePhaseDiffs[2];
        destinationPhases[17] = sourcePhaseDiffs[7] - sourcePhaseDiffs[2];
        destinationPhases[18] = sourcePhaseDiffs[4] - sourcePhaseDiffs[3];
        destinationPhases[19] = sourcePhaseDiffs[5] - sourcePhaseDiffs[3];
        destinationPhases[20] = sourcePhaseDiffs[6] - sourcePhaseDiffs[3];
        destinationPhases[21] = sourcePhaseDiffs[7] - sourcePhaseDiffs[3];
        destinationPhases[22] = sourcePhaseDiffs[5] - sourcePhaseDiffs[4];
        destinationPhases[23] = sourcePhaseDiffs[6] - sourcePhaseDiffs[4];
        destinationPhases[24] = sourcePhaseDiffs[7] - sourcePhaseDiffs[4];
        destinationPhases[25] = sourcePhaseDiffs[6] - sourcePhaseDiffs[5];
        destinationPhases[26] = sourcePhaseDiffs[7] - sourcePhaseDiffs[5];
        destinationPhases[27] = sourcePhaseDiffs[7] - sourcePhaseDiffs[6];
        destinationPhases[28] = sourcePhaseDiffs[7] - sourcePhaseDiffs[8];
        destinationPhases[29] = sourcePhaseDiffs[1] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[30] = sourcePhaseDiffs[2] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[31] = sourcePhaseDiffs[3] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[32] = sourcePhaseDiffs[4] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[33] = sourcePhaseDiffs[5] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[34] = sourcePhaseDiffs[6] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        destinationPhases[35] = sourcePhaseDiffs[7] + sourcePhaseDiffs[7] - sourcePhaseDiffs[0] - sourcePhaseDiffs[8];
        return destinationPhases;
    }

    public static float[] GetPhaseDiffsInTheory(double frequency, float aperture, int angles)
    {
        var phaseDiffs = new float[36 * angles];
        for (var index = 0; index < angles; ++index)
        {
            var currentSamples = GetPhaseDiffsByOffsetInTheory(frequency, aperture, angles, index);
            Array.Copy(currentSamples, 0, phaseDiffs, 36 * index, 36);
        }

        return phaseDiffs;
    }

    public static DdfCorrelation GetDdfCorrelation(float[] values, float[] samples, int angles)
    {
        if (values is not { Length: 36 }
            || samples == null || samples.Length != 36 * angles)
            return null;
        var correlationResult = new double[angles];
        const int phaseCount = 36;
        var angleStep = 360.0f / angles;
        var max = -9999.0d;
        var index = 0;
        var insertedCorrelation = new double[5];
        for (var angleIndex = 0; angleIndex < angles; ++angleIndex)
        {
            var startOffset = angleIndex * phaseCount;
            for (var phaseIndex = 0; phaseIndex < phaseCount; ++phaseIndex)
            {
                // correlationResult[angleIndex] += Math.Cos((samples[startOffset + phaseIndex] - values[phaseIndex]) * Math.PI / 180.0);
                var temp = (int)(((samples[startOffset + phaseIndex] - values[phaseIndex]) % 360 + 360) % 360 * 10);
                correlationResult[angleIndex] += _cosArray[temp];
            }

            if (correlationResult[angleIndex] < 0.0f) correlationResult[angleIndex] = -correlationResult[angleIndex];
            if (correlationResult[angleIndex] > max)
            {
                max = correlationResult[angleIndex];
                index = angleIndex;
            }
        }

        insertedCorrelation[0] = correlationResult[index];
        insertedCorrelation[1] = correlationResult[(index - 2 + angles) % angles];
        insertedCorrelation[2] = correlationResult[(index - 1 + angles) % angles];
        insertedCorrelation[3] = correlationResult[(index + 1) % angles];
        insertedCorrelation[4] = correlationResult[(index + 2) % angles];
        var deltaCorrelation = 0.7f *
                               (2 * (insertedCorrelation[4] - insertedCorrelation[1]) + insertedCorrelation[3] -
                                insertedCorrelation[2]) /
                               (2 * insertedCorrelation[0] + insertedCorrelation[3] + insertedCorrelation[2] -
                                2 * (insertedCorrelation[4] + insertedCorrelation[1]));
        for (var correlationIndex = 0; correlationIndex < angles; ++correlationIndex)
            correlationResult[correlationIndex] /= phaseCount;
        var azimuth = (float)(((index + deltaCorrelation) * angleStep + 360) % 360);
        var quality = (float)(correlationResult[index] * 100);
        return new DdfCorrelation
        {
            Azimuth = azimuth,
            Quality = quality,
            Angles = angles,
            AngleIndex = index /*, Correlations = Array.ConvertAll(correlationResult, item => (float)item) */
        };
    }

    public static float[] GetTransformedLevelDiffs(float[] sourceLevels)
    {
        if (sourceLevels is not { Length: 9 }) return null;
        var destinationLevels = new float[36];
        Buffer.BlockCopy(sourceLevels, 0, destinationLevels, 0, sizeof(float) * sourceLevels.Length);
        return destinationLevels;
    }

    public static DdfCorrelation GetDdfCorrelationWithLevelDiffs(float[] values, float[] samples, int angles,
        int lc = 36)
    {
        if (values is not { Length: 36 }
            || samples == null || samples.Length != 36 * angles)
            return null;
        var correlationResult = new float[angles];
        var levelCount = lc;
        var angleStep = 360.0f / angles;
        var min = float.MaxValue;
        var index = 0;
        for (var angleIndex = 0; angleIndex < angles; ++angleIndex)
        {
            var startOffset = angleIndex * levelCount;
            for (var levelIndex = 0; levelIndex < levelCount; ++levelIndex)
                correlationResult[angleIndex] += Math.Abs(samples[startOffset + levelIndex] - values[levelIndex]);
            if (correlationResult[angleIndex] < min)
            {
                min = correlationResult[angleIndex];
                index = angleIndex;
            }
        }

        var previousIndex = (index - 1 + angles) % angles;
        var nextIndex = (index + 1) % angles;
        var deltaCorrelation = (correlationResult[nextIndex] - correlationResult[previousIndex]) /
                               (4 * correlationResult[index] -
                                2 * (correlationResult[nextIndex] + correlationResult[previousIndex]));
        var correlationMax = correlationResult.Max();
        for (var tempIndex = 0; tempIndex < correlationResult.Length; ++tempIndex)
        {
            correlationResult[tempIndex] /= correlationMax;
            correlationResult[tempIndex] = (1 - correlationResult[tempIndex]) * 100;
        }

        var azimuth = ((index + deltaCorrelation) * angleStep + 360) % 360;
        var quality = correlationResult[index] * (1 - Math.Abs(deltaCorrelation));
        return new DdfCorrelation
        {
            Azimuth = azimuth,
            Quality = quality,
            Angles = angles,
            AngleIndex = index,
            Correlations = correlationResult
        };
    }

    #endregion
}

#endregion