using System;

#pragma warning disable 1591
namespace Magneto.Contract.Algorithm;

/// <summary>
///     记录示向线的交点，不包含反向延长线上的交点与射线的交点
/// </summary>
public class CrossPoint
{
    /// <summary>
    ///     所在圆心经度
    /// </summary>
    private double _crossPointX;

    /// <summary>
    ///     所在圆心纬度
    /// </summary>
    private double _crossPointY;

    public CrossPoint()
    {
    }

    public CrossPoint(CrossPoint cp)
    {
        _crossPointX = cp._crossPointX;
        _crossPointY = cp._crossPointY;
    }

    public CrossPoint(double crossx, double crossy)
    {
        _crossPointX = crossx;
        _crossPointY = crossy;
    }

    public double CrossPointX
    {
        get => _crossPointX;
        set => _crossPointX = Math.Round(value, 6);
    }

    public double CrossPointY
    {
        get => _crossPointY;
        set => _crossPointY = Math.Round(value, 6);
    }

    public static double[,] ConvertToArray(CrossPoint[] data)
    {
        double[,] result = null;
        if (data != null)
        {
            var row = data.Length;
            result = new double[row, 2];
            for (var i = 0; i < row; i++)
            {
                result[i, 0] = data[i].CrossPointX;
                result[i, 1] = data[i].CrossPointY;
            }
        }

        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj is not CrossPoint cp) return false;
        return Math.Abs(CrossPointX - cp.CrossPointX) < 1e-9 && Math.Abs(CrossPointY - cp.CrossPointY) < 1e-9;
    }

    public override int GetHashCode()
    {
        // int result = base.GetHashCode();
        // result ^= this.CrossPointX.GetHashCode();
        // result ^= this.CrossPointY.GetHashCode();
        // return result;
        return CrossPointX.GetHashCode() ^ CrossPointY.GetHashCode();
    }

    /// <summary>
    ///     计算两个点之间的距离
    /// </summary>
    /// <param name="cp1">点1</param>
    /// <param name="cp2">点2</param>
    /// <returns>两点之间的距离</returns>
    public static double ComputDist(CrossPoint cp1, CrossPoint cp2)
    {
        return cp1.Equals(cp2)
            ? 0
            : Math.Sqrt(CommonMethods.Square(cp1.CrossPointX - cp2.CrossPointX) +
                        CommonMethods.Square(cp1.CrossPointY - cp2.CrossPointY));
    }
}