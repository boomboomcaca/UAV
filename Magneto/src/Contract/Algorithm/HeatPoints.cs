using System;

namespace Magneto.Contract.Algorithm;

public class HeatPoints
{
    private double _zoom;

    public HeatPoints()
    {
    }

    public HeatPoints(double lng, double lat, double zoom, double radius)
    {
        Lng = lng;
        Lat = lat;
        Radius = radius;
        Count = 1; //第一个点判断时，自身就是第一个数据
        Zoom = zoom;
    }

    public HeatPoints(double lng, double lat, int count)
    {
        Lng = lng;
        Lat = lat;
        Count = count;
    }

    public HeatPoints(double lng, double lat)
    {
        Lng = lng;
        Lat = lat;
    }

    /// <summary>
    ///     经度
    /// </summary>
    public double Lng { get; set; }

    /// <summary>
    ///     纬度
    /// </summary>
    public double Lat { get; set; }

    /// <summary>
    ///     半径
    /// </summary>
    public double Radius { get; set; }

    public double Zoom
    {
        get => _zoom;
        set
        {
            if (_zoom is >= 0 and <= 18) _zoom = value;
        }
    }

    /// <summary>
    ///     统计与当前圆相交或者重叠的圆的个数，有必要的时候应该定义为long
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    ///     将HeatPoints数组转换为N*2数组，横坐标第一列，纵坐标第二列
    /// </summary>
    /// <param name="data"></param>
    public static double[,] ConvertToArray(HeatPoints[] data)
    {
        double[,] result = null;
        if (data != null)
        {
            var row = data.Length;
            result = new double[row, 2];
            for (var i = 0; i < row; i++)
            {
                result[i, 0] = data[i].Lng;
                result[i, 1] = data[i].Lat;
            }
        }

        return result;
    }

    /// <summary>
    ///     判断是否为相等的对象
    /// </summary>
    /// <param name="obj">被比较的对象</param>
    /// <returns>相等返回true</returns>
    public override bool Equals(object obj)
    {
        if (obj is not HeatPoints cp) return false;
        return Math.Abs(Lng - cp.Lng) < 1e-9 &&
               Math.Abs(Lat - cp.Lat) < 1e-9 &&
               Math.Abs(Radius - cp.Radius) < 1e-9 &&
               Math.Abs(Zoom - cp.Zoom) < 1e-9;
    }

    public override int GetHashCode()
    {
        var result = 0;
        result ^= Lng.GetHashCode();
        result ^= Lat.GetHashCode();
        result ^= Radius.GetHashCode();
        result ^= Count.GetHashCode();
        result ^= Zoom.GetHashCode();
        return result;
    }
}