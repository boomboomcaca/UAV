using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace Magneto.Contract.Algorithm.Interpolation;

/// <summary>
///     插值算法实现
/// </summary>
internal class InterpolationData
{
    private readonly List<AltitudePoint> _pointList = [];
    private double _formulaC;
    private double _formulaR;

    /// <summary>
    ///     插值后的数据
    /// </summary>
    private double[,] _interpolationDemData;

    /// <summary>
    ///     K的逆矩阵
    /// </summary>
    private Matrix<double> _kn;

    private int _size;

    public InterpolationData(List<AltitudePoint> pointList, int row, int column)
    {
        Row = row;
        Column = column;
        foreach (var p in pointList) _pointList.Add(new AltitudePoint(p.X, p.Y, p.AltitudeValue));
        GetRegressionPoints();
    }

    /// <summary>
    ///     行
    /// </summary>
    public int Row { get; }

    /// <summary>
    ///     列
    /// </summary>
    public int Column { get; }

    /// <summary>
    ///     测试随机点数据是否满足条件
    /// </summary>
    public bool IsRandomPointsOk()
    {
        var size = _pointList.Count;
        var k = new DenseMatrix(size, size);
        for (var m = 0; m < size; m++)
        for (var n = 0; n < size; n++)
            k[m, n] = CalCij(_pointList[m].X, _pointList[m].Y, _pointList[n].X, _pointList[n].Y);
        _kn = k.Inverse();
        for (var m = 0; m < size; m++)
        for (var n = 0; n < size; n++)
            if (double.IsNaN(_kn[m, n]))
                return false;
        return true;
    }

    /// <summary>
    ///     获取插值后的数据
    /// </summary>
    /// <param name="amplification">输出的数据比原始的数据放大的倍数</param>
    /// <returns>存有插值后的数组</returns>
    public double[,] GetInterpolationData(int amplification)
    {
        _size = _pointList.Count;
        _interpolationDemData = new double[(Row - 1) * amplification + 1, (Column - 1) * amplification + 1];
        for (var m = 0; m < _interpolationDemData.GetLength(0); m++)
        for (var n = 0; n < _interpolationDemData.GetLength(1); n++)
        {
            var realX = m / (double)amplification;
            var realY = n / (double)amplification;
            var d = new DenseVector(_size);
            for (var p = 0; p < _size; p++) d[p] = CalCij(_pointList[p].X, _pointList[p].Y, realX, realY);
            var namuta = _kn.LeftMultiply(d);
            for (var q = 0; q < _size; q++) _interpolationDemData[m, n] += namuta[q] * _pointList[q].AltitudeValue;
            //if (double.IsNaN(interpolationDEMData[m, n])) throw new Exception("数值异常，请重新生成随机点或选择其他DEM数据");
        }

        return _interpolationDemData;
    }

    /// <summary>
    ///     计算Cij
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="y1"></param>
    /// <param name="x2"></param>
    /// <param name="y2"></param>
    private double CalCij(double x1, double y1, double x2, double y2)
    {
        var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        if (distance == 0) return _formulaC;
        return _formulaC * Math.Exp(-distance / _formulaR);
    }

    /// <summary>
    ///     得到用于保存插值数据的byte[]
    /// </summary>
    /// <param name="length">byte[]的长度</param>
    /// <returns>byte[]数组，用于FileStream</returns>
    public byte[] GetByteInterpolationData(out int length)
    {
        string data = null;
        for (var m = 0; m < _interpolationDemData.GetLength(0); m++)
        {
            data += _interpolationDemData[m, 0];
            for (var n = 1; n < _interpolationDemData.GetLength(1); n++) data += "," + _interpolationDemData[m, n];
            data += "\n";
        }

        var result = Encoding.Default.GetBytes(data!);
        length = result.Length;
        return result;
    }

    private class RegressionPoint(double distance, double semivariogram)
    {
        public readonly double Distance = distance;
        public readonly double Semivariogram = semivariogram;
    }

    private class StartEnd(int start, int end)
    {
        public int Start { get; } = start;
        public int End { get; } = end;
    }

    #region 计算拟合曲线中的a，r   "f(x)=a*(1-e^(-x/r))"

    /// <summary>
    ///     求得用于显示的点集
    /// </summary>
    private List<RegressionPoint> _showPoints;

    /// <summary>
    ///     求得用于拟合计算的点集
    /// </summary>
    private List<RegressionPoint> _calPoints;

    private void GetRegressionPoints()
    {
        var initialPoints = new List<RegressionPoint>();
        var length = _pointList.Count;
        for (var m = 0; m < length; m++)
        for (var n = 0; n < length; n++)
            if (m != n)
            {
                var distance = Math.Sqrt(Math.Pow(_pointList[m].X - _pointList[n].X, 2) +
                                         Math.Pow(_pointList[m].Y - _pointList[n].Y, 2));
                var semivariogram = 0.5 * Math.Pow(_pointList[m].AltitudeValue - _pointList[n].AltitudeValue, 2);
                initialPoints.Add(new RegressionPoint(distance, semivariogram));
            }

        var maxDistance = initialPoints[0].Distance;
        var minDistance = initialPoints[0].Distance;
        foreach (var point in initialPoints)
        {
            if (point.Distance > maxDistance) maxDistance = point.Distance;
            if (point.Distance < minDistance) minDistance = point.Distance;
        }

        // maxDistance *= 10000;
        // minDistance *= 10000;
        _showPoints = [];
        _calPoints = [];
        for (var n = (int)minDistance; n <= Math.Min(100, maxDistance); n++)
        {
            var n1 = n;
            var tempPointsList = initialPoints.Where(x => (int)x.Distance == n1);
            var regressionPoints = tempPointsList as RegressionPoint[] ?? tempPointsList.ToArray();
            if (regressionPoints.Any())
            {
                double value = 0;
                foreach (var point in regressionPoints) value += point.Semivariogram;
                value /= regressionPoints.Length;
                _showPoints.Add(new RegressionPoint(n, value));
            }
        }

        for (var n = 0; n < _showPoints.Count / 10; n++)
        {
            double x = 0;
            double y = 0;
            for (var m = n * 10; m < (n + 1) * 10; m++)
            {
                x += _showPoints[m].Distance;
                y += _showPoints[m].Semivariogram;
            }

            _calPoints.Add(new RegressionPoint(x / 10, y / 10));
        }

        FitPoints(_calPoints);
    }

    /// <summary>
    ///     高斯牛顿法
    /// </summary>
    /// <param name="calPoints"></param>
    private void FitPoints(List<RegressionPoint> calPoints)
    {
        var model = new PowerModel();
        var solver = new GaussNewtonSolver(0.001, 0.001, 10000, new DenseVector([50.0, 1.5]));
        var solverIterations = new List<Vector<double>>();
        var x = new double[calPoints.Count];
        var y = new double[calPoints.Count];
        for (var n = 0; n < calPoints.Count; n++)
        {
            //缩小数值，方便计算，之后会还原
            x[n] = calPoints[n].Distance / 5;
            y[n] = calPoints[n].Semivariogram / 1000;
        }

        Vector<double> dataX = new DenseVector(x);
        Vector<double> dataY = new DenseVector(y);
        solver.Estimate(model, calPoints.Count, dataX, dataY, ref solverIterations);
        _formulaC = solverIterations.Last()[0] * 5;
        _formulaR = solverIterations.Last()[1] * 1000;
    }

    #endregion
}

/// <summary>
///     关于高程点的类
/// </summary>
internal class AltitudePoint(double x, double y, double altitudeValue)
{
    /// <summary>
    ///     X坐标
    /// </summary>
    public double X { get; } = x;

    /// <summary>
    ///     Y坐标
    /// </summary>
    public double Y { get; } = y;

    /// <summary>
    ///     高程值
    /// </summary>
    public double AltitudeValue { get; } = altitudeValue;

    public override string ToString()
    {
        return X + "," + Y + "," + AltitudeValue;
    }
}