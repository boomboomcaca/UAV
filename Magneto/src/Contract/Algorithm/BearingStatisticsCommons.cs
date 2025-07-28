using System;
using System.Collections;
using System.Collections.Generic;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     示向度数据统计方法
/// </summary>
public enum StatisticsMethod
{
    /// <summary>
    ///     常规方式，正常累计计算概率，以最大时间片内数据计算
    /// </summary>
    Normal,

    /// <summary>
    ///     方差筛选，根据示向度范围和质量按照设置的时间片内的数据进行筛选统计
    /// </summary>
    VarianceFilter,

    /// <summary>
    ///     扇面筛选，将360度按一定的角度进行扇面分区，然后将时间片内数据划分到各个扇面，取出数据最多的扇面进行统计
    /// </summary>
    SectorFilter
}

public class WbdfBearingStatisticsArgs : EventArgs
{
    /// <summary>
    ///     角度对应的概率
    /// </summary>
    public double[][] AngleProbability;

    /// <summary>
    ///     最优值
    /// </summary>
    public float[] MaxProbabilityAngle;
}

/// <summary>
///     宽带测向数据结构
/// </summary>
public struct SwbdFindData : IEqualityComparer<SwbdFindData>, IEqualityComparer
{
    /// <summary>
    ///     方位角 单位：°
    /// </summary>
    public float[] Azimuth;

    /// <summary>
    ///     测向质量
    /// </summary>
    public float[] Quality;

    /// <summary>
    ///     数据记录时戳
    /// </summary>
    public DateTime TimeStamp;

    /// <summary>
    ///     当前位置经度
    /// </summary>
    public double Lng;

    /// <summary>
    ///     当前位置纬度
    /// </summary>
    public double Lat;

    public override bool Equals(object obj)
    {
        if (obj is not SwbdFindData df) return false;
        if (Azimuth == df.Azimuth &&
            Quality == df.Quality &&
            Math.Abs(Lng - df.Lng) < 1e-13 &&
            Math.Abs(Lat - df.Lat) < 1e-13 &&
            TimeStamp == df.TimeStamp)
            return true;
        return false;
    }

    public bool Equals(SwbdFindData x, SwbdFindData y)
    {
        if (object.Equals(x, y))
            return true;

        return x.Equals(y);
    }

    public new bool Equals(object x, object y)
    {
        if (object.Equals(x, y)) return true;

        // if (object.Equals(x, null) || object.Equals(y, null))
        // {
        //     return false;
        // }

        return x != null && x.Equals(y);
    }

    public override int GetHashCode()
    {
        var result = Azimuth.GetHashCode();
        result ^= Quality.GetHashCode();
        result ^= Lng.GetHashCode();
        result ^= Lat.GetHashCode();
        result ^= TimeStamp.GetHashCode();
        return result;
    }

    public int GetHashCode(SwbdFindData obj)
    {
        return obj.GetHashCode();
    }

    public int GetHashCode(object obj)
    {
        return base.GetHashCode();
    }
}

public class BearingStatisticsArgs : EventArgs
{
    /// <summary>
    ///     cp 20191106
    /// </summary>
    public SortedList<float, int> AngleStatistics { get; set; } = new();

    /// <summary>
    ///     角度对应的概率
    /// </summary>
    public SortedList<float, float> AngleProbability { get; set; } = new();

    /// <summary>
    ///     最大幅度统计
    /// </summary>
    public SortedList<float, float> MaxAmpStatistics { get; set; } = new();

    /// <summary>
    ///     最大质量统计
    /// </summary>
    public SortedList<float, float> MaxQualityStatistics { get; set; } = new();

    /// <summary>
    ///     平均值线对应的值
    /// </summary>
    public float AverageValue { get; set; }

    /// <summary>
    ///     最优值
    /// </summary>
    public float MaxProbability { get; set; }

    /// <summary>
    ///     实时值
    /// </summary>
    public float RealTimeValue { get; set; }

    /// <summary>
    ///     真实最大概率
    /// </summary>
    public float RealMaxProbability { get; set; }

    /// <summary>
    ///     集中区间下限
    /// </summary>
    public float Min { get; set; }

    /// <summary>
    ///     集中区间上限
    /// </summary>
    public float Max { get; set; }

    /// <summary>
    ///     当前位置经度
    /// </summary>
    public double Lng { get; set; } = double.MaxValue;

    /// <summary>
    ///     当前位置纬度
    /// </summary>
    public double Lat { get; set; } = double.MaxValue;
}

public struct SdFindData(double frequency) : IEqualityComparer<SdFindData>, IEqualityComparer
{
    /// <summary>
    ///     测向频率
    /// </summary>
    public readonly double Frequency = frequency;

    /// <summary>
    ///     方位角 单位：°
    /// </summary>
    public float Azimuth { get; set; } = 0f;

    /// <summary>
    ///     测向电平 单位：dBμv
    /// </summary>
    public float Level { get; set; } = 0f;

    /// <summary>
    ///     测向质量
    /// </summary>
    public float Quality { get; set; } = 0f;

    /// <summary>
    ///     数据记录时戳
    /// </summary>
    public DateTime TimeStamp { get; set; } = DateTime.Now;

    /// <summary>
    ///     当前位置经度
    /// </summary>
    public double Lng { get; set; } = 0d;

    /// <summary>
    ///     当前位置纬度
    /// </summary>
    public double Lat { get; set; } = 0d;

    public override bool Equals(object obj)
    {
        if (!(obj is SdFindData sDFindData)) return false;

        var df = sDFindData;
        return Math.Abs(Frequency - df.Frequency) < 1e-7 &&
               Math.Abs(Azimuth - df.Azimuth) < 1e-2 &&
               Math.Abs(Level - df.Level) < 1e-3 &&
               Math.Abs(Quality - df.Quality) < 1e-2 &&
               Math.Abs(Lng - df.Lng) < 1e-13 &&
               Math.Abs(Lat - df.Lat) < 1e-13 &&
               TimeStamp == df.TimeStamp;
    }

    public bool Equals(SdFindData x, SdFindData y)
    {
        if (object.Equals(x, y)) return true;

        // if (object.Equals(x, null) || object.Equals(y, null))
        // {
        //     return false;
        // }

        return x.Equals(y);
    }

    public override int GetHashCode()
    {
        var result = Frequency.GetHashCode();
        // result ^= this.Azimuth.GetHashCode();
        // result ^= this.Level.GetHashCode();
        // result ^= this.Quality.GetHashCode();
        // result ^= this.Lng.GetHashCode();
        // result ^= this.Lat.GetHashCode();
        // result ^= this.TimeStamp.GetHashCode();
        return result;
    }

    public int GetHashCode(SdFindData obj)
    {
        return obj.GetHashCode();
    }

    public new bool Equals(object x, object y)
    {
        if (x == y) return true;

        if (x == null || y == null) return false;

        if (x is SdFindData a
            && y is SdFindData b)
            return Equals(a, b);

        throw new ArgumentException("", nameof(x));
    }

    public int GetHashCode(object obj)
    {
        if (obj is SdFindData x) return GetHashCode(x);

        throw new ArgumentException("", nameof(obj));
    }
}

/// <summary>
///     重写IEmurable.Except()方法主要考虑到线程安全
/// </summary>
public static class EmurableExtend
{
    public static IEnumerable<T> ExceptEx<T>(this IList<T> first, IList<T> second)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));

        if (second == null) throw new ArgumentNullException(nameof(second));

        return ExceptIterator(first, second, null);
    }

    public static IEnumerable<T> ExceptEx<T>(this IList<T> first, IList<T> second, IEqualityComparer<T> compare)
    {
        if (first == null) throw new ArgumentNullException(nameof(first));

        if (second == null) throw new ArgumentNullException(nameof(second));

        return ExceptIterator(first, second, compare);
    }

    private static IEnumerable<T> ExceptIterator<T>(IList<T> first, IList<T> second, IEqualityComparer<T> comparer)
    {
        var set = new Set<T>(comparer);

        for (var i = 0; i < second.Count; i++)
        {
            var element = second[i];
            set.Add(element);
        }

        for (var i = 0; i < first.Count; i++)
        {
            var element = first[i];
            if (set.Add(element)) yield return element;
        }
    }

    internal class Set<TElement>
    {
        private readonly IEqualityComparer<TElement> _comparer;

        private int[] _buckets;

        private int _count;

        private int _freeList;

        private Slot[] _slots;

        public Set() : this(null)
        {
        }

        public Set(IEqualityComparer<TElement> comparer)
        {
            comparer ??= EqualityComparer<TElement>.Default;
            _comparer = comparer;
            _buckets = new int[7];
            _slots = new Slot[7];
            _freeList = -1;
        }

        public bool Add(TElement value)
        {
            return !Find(value, true);
        }

        public bool Contains(TElement value)
        {
            return Find(value, false);
        }

        public bool Remove(TElement value)
        {
            var num = InternalGetHashCode(value);
            var num2 = num % _buckets.Length;
            var num3 = -1;
            for (var i = _buckets[num2] - 1; i >= 0; i = _slots[i].Next)
            {
                if (_slots[i].HashCode == num && _comparer.Equals(_slots[i].Value, value))
                {
                    if (num3 < 0)
                        _buckets[num2] = _slots[i].Next + 1;
                    else
                        _slots[num3].Next = _slots[i].Next;
                    _slots[i].HashCode = -1;
                    _slots[i].Value = default;
                    _slots[i].Next = _freeList;
                    _freeList = i;
                    return true;
                }

                num3 = i;
            }

            return false;
        }

        private bool Find(TElement value, bool add)
        {
            var num = InternalGetHashCode(value);
            for (var i = _buckets[num % _buckets.Length] - 1; i >= 0; i = _slots[i].Next)
                if (_slots[i].HashCode == num && _comparer.Equals(_slots[i].Value, value))
                    return true;
            if (!add) return false;

            int num2;
            if (_freeList >= 0)
            {
                num2 = _freeList;
                _freeList = _slots[num2].Next;
            }
            else
            {
                if (_count == _slots.Length) Resize();
                num2 = _count;
                _count++;
            }

            var num3 = num % _buckets.Length;
            _slots[num2].HashCode = num;
            _slots[num2].Value = value;
            _slots[num2].Next = _buckets[num3] - 1;
            _buckets[num3] = num2 + 1;
            return false;
        }

        private void Resize()
        {
            var num = checked(_count * 2 + 1);
            var array = new int[num];
            var array2 = new Slot[num];
            Array.Copy(_slots, 0, array2, 0, _count);
            for (var i = 0; i < _count; i++)
            {
                var num2 = array2[i].HashCode % num;
                array2[i].Next = array[num2] - 1;
                array[num2] = i + 1;
            }

            _buckets = array;
            _slots = array2;
        }

        internal int InternalGetHashCode(TElement value)
        {
            if (value != null) return _comparer.GetHashCode(value) & int.MaxValue;
            return 0;
        }

        internal struct Slot
        {
            internal int HashCode;

            internal TElement Value;

            internal int Next;
        }
    }
}