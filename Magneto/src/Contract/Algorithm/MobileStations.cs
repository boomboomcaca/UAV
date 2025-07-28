using System;

#pragma warning disable 1591
namespace Magneto.Contract.Algorithm;

/// <summary>
///     一个移动站点的信息
/// </summary>
public class MobileStations : IEquatable<MobileStations>, IComparable
{
    public MobileStations()
    {
    }

    public MobileStations(double longitude, double latitude)
    {
        Longitude = longitude;
        Latitude = latitude;
        Bearing = 0;
        Weight = 0;
    }

    public MobileStations(double longitude, double latitude, double bearing)
    {
        Longitude = longitude;
        Latitude = latitude;
        Bearing = bearing;
        Weight = 0;
    }

    /// <summary>
    ///     移动测向数据构造函数
    /// </summary>
    /// <param name="longitude">经度</param>
    /// <param name="latitude">纬度</param>
    /// <param name="bearing">示向度，单位：度</param>
    /// <param name="weight">该条测向数据权重（测向电平与测向质量的某种运算结果）</param>
    public MobileStations(double longitude, double latitude, double bearing, double weight)
    {
        Longitude = longitude;
        Latitude = latitude;
        Bearing = bearing;
        Weight = weight;
    }

    /// <summary>
    ///     测向站的经度坐标
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     测向站的纬度坐标
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     测向站的示向度数据
    /// </summary>
    public double Bearing { get; set; }

    /// <summary>
    ///     测向数据权重（测向电平与测向质量的某种运算结果）
    /// </summary>
    public double Weight { get; set; }

    public int CompareTo(object obj)
    {
        if (obj == null) return -1;
        // 按照Weight降序排序
        MobileStations other;
        if (obj is MobileStations stations)
            other = stations;
        else
            return -1;
        if (Weight > other.Weight)
            return -1;
        if (Math.Abs(Weight - other.Weight) < 1e-9)
            return 0;
        return 1;
    }

    public bool Equals(MobileStations other)
    {
        if (other == null) return false;
        return Math.Abs(Longitude - other.Longitude) < 1e-9 &&
               Math.Abs(Latitude - other.Latitude) < 1e-9 &&
               Math.Abs(Bearing - other.Bearing) < 1e-9 &&
               Math.Abs(Weight - other.Weight) < 1e-9;
    }

    public override bool Equals(object obj)
    {
        if (obj is not MobileStations ms) return false;
        return Equals(ms);
    }

    public override int GetHashCode()
    {
        var result = 0;
        result ^= Longitude.GetHashCode();
        result ^= Latitude.GetHashCode();
        result ^= Bearing.GetHashCode();
        result ^= Weight.GetHashCode();
        return result;
    }
}