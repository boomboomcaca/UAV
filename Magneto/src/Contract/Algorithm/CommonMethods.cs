/*********************************************************************************************
 *
 * 文件名称:    ...Tracker800\Client\Source\DCComponent\Commons\DC.Commons.Algorithm\Commonmethods\Commonmethods.cs
 *
 * 作    者:    jacberg
 *
 * 创作日期:    2017/08/03
 *
 * 修    改:    无
 *
 * 备    注:	常用方法，如矩阵运算、归一化、角度平均等
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Magneto.Contract.Algorithm;

/* 窗系数
 长度为10点的高斯窗（gausswin）         长度为10点的汉明窗（hamming）系数:
   0.00978586763122416                  0.0161943319838057
    0.0336329811597796                  0.0379796672399332
     0.084896519194904                  0.0931420725249417
     0.157388464297929                  0.1558704453441300
     0.214296167716164                  0.1968134829071900
     0.214296167716164                  0.1968134829071900
     0.157388464297929                  0.1558704453441300
     0.084896519194904                  0.0931420725249417
    0.0336329811597796                  0.0379796672399332
   0.00978586763122416                  0.0161943319838057
 * 长度为10点的海宁窗（hann）系数：（11阶）
                     0                 -0.0001953498949023,
    0.0259950618756691                 -0.00723647720991,
    0.0918168691481189                 -0.03141655530261,
    0.166666666666667                  0.02530641486632,
    0.215521402309545                  0.2815630677238,
    0.215521402309545                  0.4639577996346,
    0.166666666666667                  0.2815630677238,
    0.0918168691481189                 0.02530641486632,
    0.0259950618756691                 -0.03141655530261,
                     0                 -0.00723647720991,
    1.486719524832e-06,               （10阶 线性相位）
    0.0001338302266752,               1.651910959277e-06,
    0.00443184844212,                 0.000230489286013,
    0.05399096688119,                 0.009357262732207,
    0.2419707261693,                  0.1105301801342,
    0.3989422831225,                  0.3798804159366,
    0.2419707261693,                  0.3798804159366,
    0.05399096688119,                 0.1105301801342,
    0.00443184844212,                 0.009357262732207,
    0.0001338302266752,               0.000230489286013,
    1.486719524832e-06                1.651910959277e-06
 */

#region 滤波器系数

#region 3100-3150 22050 带阻

//    const double[] NUM = {
//     0.9772103785159,   -6.147234736808,    20.35395757193,   -44.04940502255,
//      68.41761191372,   -78.88466657238,    68.41761191372,   -44.04940502255,
//      20.35395757193,   -6.147234736808,   0.9772103785159
//};
//    const double[] DEN = {
//                   1,   -6.261591572236,    20.63698897752,   -44.45610941595,
//      68.73129964853,   -78.88167216887,    68.10041932736,   -43.64373479774,
//      20.07391165104,   -6.034838136292,   0.9549401238791
//};

#endregion

#region 2900-3200 22050 带阻

//  double[] num = { 0.8293825580592, -5.357849993815, 17.99096752295, -39.31616241739,
//                   61.37780276924, -70.90100546875, 61.37780276924, -39.31616241739,
//                   17.99096752295, -5.357849993815, 0.8293825580592 };
//  double[] den = { 1, -6.223667592474, 20.12646851801, -42.35166050287,
//                   63.65155350813, -70.77136644827, 58.95337076984, -36.32687672735,
//                   15.98460208707, -4.575459020202, 0.6803108174359 };

#endregion

#region 音频预加重滤波器

//
//
// 音频预加重滤波器一般为一阶的数字滤波器
//             -1
// H(Z)=1 - μz
//
// 式中，μ接近1
//

#endregion

#endregion

/// <summary>
///     该方法库包含直线求交点，交点聚类，置信椭圆，矩阵运算、范数，相关运算，信号处理，FFT，图像处理等方法
/// </summary>
public static class CommonMethods
{
    #region const variables

    private const double Pi = Math.PI;

    #endregion

    /// <summary>
    ///     当前计算的起始索引，避免重复计算
    /// </summary>
    private static int _currentIndex;

    /// <summary>
    ///     是否超过规定的示向线条数，移除第一列交点
    /// </summary>
    private static bool _isRemoveFirst;

    /// <summary>
    ///     存储交点的阵列 外层索引代表站个数，内层索引代表某个站与其他站的交点集合
    /// </summary>
    private static List<List<CrossPoint>> _crossPointsArray = [];

    /// <summary>
    ///     重新开始定位时将交点清空
    /// </summary>
    public static void ClearResource()
    {
        lock (_crossPointsArray)
        {
            _currentIndex = 0;
            _isRemoveFirst = false;
            _crossPointsArray = [];
        }
    }

    /// <summary>
    ///     将求得的交点坐标存放到字典中
    /// </summary>
    /// <param name="data">包含台站经纬度，示向度信息的数组</param>
    /// <param name="latestBearings">指定纳入计算的最大示向线条数，若保留任何一个交点应将该值设置为 int.MaxValue</param>
    /// <returns>交点坐标字典</returns>
    public static List<CrossPoint> IntersectPoints(MobileStations[] data, int latestBearings)
    {
        //临时变量，判断系数矩阵是否可逆 
        var n = data.Length < latestBearings ? data.Length : latestBearings;
        //接收临时交点的临时值
        var crossPoint = new CrossPoint();
        //存储待返回的交点
        var result = new List<CrossPoint>();
        //包含经纬度，示向度信息的对象
        var stationInfo = new List<MobileStations>();
        //将正北示向度转换为直角坐标系的直线斜率（0~pi）
        foreach (var line in data)
        {
            //line.Bearing = line.Bearing - 90;
            //反压缩反算坐标，90-line.Bearing考虑了坐标旋转。
            var offBearing = Math.Atan(Math.Tan((90 - line.Bearing) * Math.PI / 180) *
                                       Math.Cos(line.Latitude * Math.PI / 180));
            //象限分布
            line.Bearing = (int)Math.Floor(line.Bearing / 90) switch
            {
                3 => offBearing + 2 * Math.PI,
                2 or 1 => offBearing + Math.PI,
                -2 or -3 => offBearing - Math.PI,
                -4 => offBearing - 2 * Math.PI,
                _ => offBearing
            };

            //这里传入的Bearing应该为弧度值。取值范围为（-π，π）
            //line.Bearing = System.Math.PI * line.Bearing / 180.0;
            stationInfo.Add(new MobileStations(line.Longitude, line.Latitude, line.Bearing));
        }

        try
        {
            //求两个点的交点坐标
            if (stationInfo.Count >= 2)
            {
                if (IsAllSamePoint(Array
                        .ConvertAll(stationInfo.ToArray(), ms => new CrossPoint(ms.Longitude, ms.Latitude)).ToList()))
                    return null;
                _isRemoveFirst = data.Length >= latestBearings;
                if (_isRemoveFirst) _crossPointsArray.RemoveAt(0);
                for (var i = 0; i < stationInfo.Count; i++)
                {
                    result = [];
                    for (var j = _currentIndex == 0 ? i + 1 : _currentIndex; j < stationInfo.Count; j++)
                    {
                        var temp = Math.Tan(stationInfo[j].Bearing) - Math.Tan(stationInfo[i].Bearing);
                        if (Math.Round(temp, 7) == 0) continue;
                        var coefficient = 1.0 / temp; //系数矩阵行列式的值                      
                        //逆矩阵与常数项乘积，X坐标                        
                        var b1 = stationInfo[i].Latitude - stationInfo[j].Latitude + Math.Tan(stationInfo[j].Bearing) *
                            stationInfo[j].Longitude - Math.Tan(stationInfo[i].Bearing) * stationInfo[i].Longitude;
                        crossPoint.CrossPointX = b1 * coefficient;
                        //逆矩阵与常数项乘积，Y坐标
                        var b2 = Math.Tan(stationInfo[i].Bearing) * Math.Tan(stationInfo[j].Bearing) *
                                 (stationInfo[j].Longitude - stationInfo[i].Longitude) + stationInfo[i].Latitude *
                                 Math.Tan(stationInfo[j].Bearing) -
                                 stationInfo[j].Latitude * Math.Tan(stationInfo[i].Bearing);
                        crossPoint.CrossPointY = b2 * coefficient;
                        // 排除监测车不动，导致交点始终在监测车所在位置，造成的错误结果 20170421 hufb add
                        if (Math.Abs(crossPoint.CrossPointX - stationInfo[i].Longitude) > 1e-9 &&
                            Math.Abs(crossPoint.CrossPointY - stationInfo[i].Latitude) > 1e-9)
                        {
                            result.Add(crossPoint);
                            crossPoint = new CrossPoint();
                        }
                    }

                    if (_crossPointsArray.Count > n - 1)
                    {
                        //CrossPointsArray[i] = result;
                        if (result.Count > 0) _crossPointsArray[i].AddRange(result);
                    }
                    else
                    {
                        _crossPointsArray.Add(result);
                    }
                }

                result = [];
                foreach (var item in _crossPointsArray)
                    if (item.Count > 0)
                        result.AddRange(item);
                // 去掉监测站与交点相同的坐标点，防止远近效应将定位结果收敛到射线交点处
                // 去掉监测站与交点相等的点
                for (var i = 0; i < stationInfo.Count; i++)
                for (var j = i + 1; j < result.Count; j++)
                    if (Math.Abs(stationInfo[i].Longitude - result[j].CrossPointX) < 1e-9 &&
                        Math.Abs(stationInfo[i].Latitude - result[j].CrossPointY) < 1e-9)
                    {
                        result.RemoveAt(j);
                        j--;
                    }

                if (result.Count < 3)
                    return result;
                if (result.Count > 20000)
                    lock (_crossPointsArray)
                    {
                        ClearResource();
                    }

                if (_isRemoveFirst)
                    _currentIndex = stationInfo.Count - 1;
                else
                    _currentIndex = stationInfo.Count;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Commonmethods IntersectPoints:\r\n{0}", ex);
        }

        return result;
    }

    /// <summary>
    ///     计算三角形内切圆的半径和圆心
    /// </summary>
    /// <param name="coordinates">计算内切圆所需的三个顶点坐标集合</param>
    /// <param name="centerX">三角形内切圆圆心横坐标</param>
    /// <param name="centerY">三角形内切圆圆心纵坐标</param>
    /// <param name="radius">三角形内切圆半径</param>
    public static void GetRadiusCenter(List<CrossPoint> coordinates, out double centerX, out double centerY,
        out double radius)
    {
        centerX = 0;
        centerY = 0;
        radius = 0;
        if (coordinates == null || coordinates.Count == 0) return;
        //hufb2015111014 判断Coordinates集合中是相同的点以及有没有点的值是NaN
        if (coordinates.Any(t => t.CrossPointX.Equals(double.NaN) || t.CrossPointY.Equals(double.NaN))) return;
        if (coordinates.Count != 3)
            //MessageBox.Show("三个交点才能构成三角形！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        //如果三个顶点为同一个点，说明三条示向线相交于一点，返回该点，且半径赋值为100
        if (IsSamePoint(coordinates))
        {
            double sumX = 0;
            double sumY = 0;
            var count = coordinates.Count;
            foreach (var cp in coordinates)
            {
                sumX += cp.CrossPointX;
                sumY += cp.CrossPointY;
            }

            centerX = sumX / count;
            centerY = sumY / count;
            radius = 3;
            return;
        }

        //求半径           
        var xyCoordinates = new List<MobileStations>();
        foreach (var data in coordinates) xyCoordinates.Add(new MobileStations(data.CrossPointX, data.CrossPointY));
        //X坐标差
        var x1 = xyCoordinates[0].Longitude - xyCoordinates[1].Longitude;
        var x2 = xyCoordinates[0].Longitude - xyCoordinates[2].Longitude;
        var x3 = xyCoordinates[1].Longitude - xyCoordinates[2].Longitude;
        //Y坐标差
        var y1 = xyCoordinates[0].Latitude - xyCoordinates[1].Latitude;
        var y2 = xyCoordinates[0].Latitude - xyCoordinates[2].Latitude;
        var y3 = xyCoordinates[1].Latitude - xyCoordinates[2].Latitude;
        //三角形边长a
        var a = Math.Sqrt(Square(x3) + Square(y3));
        //三角形边长b
        var b = Math.Sqrt(Square(x2) + Square(y2));
        //三角形边长c
        var c = Math.Sqrt(Square(x1) + Square(y1));
        //构成三角形的条件判断
        if (a + b < c || a + c < b || b + c < a || a - b > c || a - c > b || b - c > a) return;
        var p = (a + b + c) / 2.0;
        if (Math.Round(p, 7) == 0) return;
        //内切圆圆心横坐标
        centerX = a * xyCoordinates[0].Longitude / (2 * p) + b * xyCoordinates[1].Longitude / (2 * p) +
                  c * xyCoordinates[2].Longitude / (2 * p);
        centerX = Math.Round(centerX, 7);
        //内切圆圆心纵坐标
        centerY = a * xyCoordinates[0].Latitude / (2 * p) + b * xyCoordinates[1].Latitude / (2 * p) +
                  c * xyCoordinates[2].Latitude / (2 * p);
        centerY = Math.Round(centerY, 7);
        //三角形面积
        var s = Math.Sqrt(p * (p - a) * (p - b) * (p - c));
        //内切圆半径
        radius = Math.Round(s / p, 12);
        if (!radius.Equals(double.NaN) && radius != 0) return;
        radius = 3;
        Debug.WriteLine("radius is NaN");
    }

    /// <summary>
    ///     K-Means聚类算法实现代码
    /// </summary>
    /// <param name="points">data是输入的不带分类标号的数据</param>
    /// <param name="n">N是数据一共分多少类</param>
    /// <returns>每一类的中心</returns>
    public static List<CrossPoint> KMeans(List<CrossPoint> points, int n)
    {
        //if (points.Count < 15)
        //{
        //    Console.WriteLine("聚类算法必须至少15条数据!");
        //    return null;
        //}
        //将交点列表List<CrossPoint> points转换为double[,]数组
        var pointsCount = points.Count;
        var data = new double[pointsCount, 2]; //n行，2列矩阵，存放交点坐标:x,y
        for (var i = 0; i < pointsCount; i++)
        {
            data[i, 0] = points[i].CrossPointX;
            data[i, 1] = points[i].CrossPointY;
        }

        var times = 0; //统计循环次数
        const int iterTimes = 30; //控制迭代收敛次数
        var m = data.GetLength(0); //m是数据个数
        var d = data.GetLength(1); //d是数据维数
        var max = new double[d]; //每一维最大的数
        var min = new double[d]; //每一维最小的数 
        var center = new double[n, 2]; //待返回的各类的中心          
        var preCenter = new List<double[,]>(); //上次求得的中心位置
        var temp = new List<double[,]>();
        var rand = new Random(1); //修改为固定种子（1）生成，保证每次运行的时候初始化中心坐标只固定的2015-10-27-14：00
        //产生m-n之间的随机数
        // var rm = new Random(1);
        //int[] te = new int[12];
        //for (int i = 0; i < 12; i++)
        //{
        //    te[i] = rm.Next(n) + m;
        //}     
        if (points.Count < 200)
        {
            for (var i = 0; i < d; i++)
            {
                var tempColumn = GetOneColumn(data, i);
                max[i] = Max(tempColumn); //每一维最大的数
                min[i] = Min(tempColumn); //每一维最小的数
                for (var k = 0; k < n; k++)
                    center[k, i] = min[i] + (max[i] - min[i]) * rand.NextDouble(); //new                      
                //center[k, i] = max[i] + (min[i] - max[i]) * rand.NextDouble();//old
            }
        }
        else
        {
            //for (int i = 0; i < points.Count; i++)
            //{
            //    for (int j = i + 1; j < points.Count; j++)
            //    {
            //        if (points[i].Equals(points[j]))
            //        {
            //            points.RemoveAt(j);
            //            j--;
            //        }
            //    }
            //}
            //points = points.Where((x, i) => points.FindIndex(z => z.CrossPointX == x.CrossPointX && z.CrossPointY == x.CrossPointY) == i).ToList();               
            var initCenter = GetInitCenter(points, n);
            if (initCenter.Count >= 3)
                for (var i = 0; i < d; i++)
                for (var k = 0; k < n; k++)
                    if (k % 2 == 0)
                        center[k, i] = initCenter[k].CrossPointX;
                    else
                        center[k, i] = initCenter[k].CrossPointY;
            else
                for (var i = 0; i < d; i++)
                {
                    var tempColumn = GetOneColumn(data, i);
                    max[i] = Max(tempColumn); //每一维最大的数
                    min[i] = Min(tempColumn); //每一维最小的数
                    for (var k = 0; k < n; k++) center[k, i] = min[i] + (max[i] - min[i]) * rand.NextDouble(); //new
                    //center[k, i] = max[i] + (min[i] - max[i]) * rand.NextDouble();//old
                }
        }

        while (times <= iterTimes)
        {
            times++;
            //Array.Copy(center, copyCenter, center.Length);
            var copyCenter = CopyTwoDArray(center);
            preCenter.Add(copyCenter); //避免内存溢出，因为copyCenter里面本身存的就是上次的center，因此直接让他们两做判断条件
            if (preCenter.Count == 30) preCenter = [];
            for (var i = 0; i < n; i++)
            {
                temp.Add(new double[m, 2]);
                temp[i] = new double[m, 2];
                for (var j = 0; j < m; j++)
                {
                    temp[i][j, 0] = data[j, 0] - center[i, 0];
                    temp[i][j, 1] = data[j, 1] - center[i, 1];
                }
            }

            var weight = new double[m, n];
            for (var i = 0; i < m; i++)
            {
                var c = new double[n];
                for (var j = 0; j < n; j++)
                {
                    var tempRow = GetOneRow(temp[j], i);
                    c[j] = Norm(tempRow);
                }

                var index = MinPosition(c);
                var tempRow1 = GetOneRow(temp[index], i);
                weight[i, index] = Norm(tempRow1);
            }

            for (var i = 0; i < n; i++)
            for (var j = 0; j < d; j++)
            {
                center[i, j] = Sum(VectorsMultiply(GetOneColumn(weight, i), GetOneColumn(data, j))) /
                               Sum(GetOneColumn(weight, i));
                if (center[i, j].ToString(CultureInfo.InvariantCulture) == "非数字") center[i, j] = data[i, j];
            }

            //计算矩阵范数有CalcMatrixNorm改为使用GetMatrixNorm，精度更高，迭代次数少
            if (GetMatrixNorm(MatrixMinus(copyCenter, center)) <
                1e-6) //(CalcMatrixNorm(MatrixMinus(preCenter[preCenter.Count - 1], center)) < 1e-6)
                break;
        }

        return CPointsArrayToList(center);
    }

    private static List<CrossPoint> GetInitCenter(List<CrossPoint> points, int n)
    {
        var distances = new List<double>();
        var tempPoints = new List<CrossPoint>(points);
        var initCenter = new List<CrossPoint>();
        //计算两两对象之间的欧氏距离，存入集合中，并统计距离总和
        for (var i = 0; i < tempPoints.Count; i++)
        for (var j = i + 1; j < tempPoints.Count; j++)
        {
            var dist = CrossPoint.ComputDist(tempPoints[i], tempPoints[j]);
            distances.Add(dist);
        }

        //计算平均距离     
        var meanDist = distances.Average();
        //meanDist *= 1.2;//适当放大密集区域计算的圆半径2016030216
        while (initCenter.Count < n)
        {
            var density = new List<int>(); //<p,meanDist>
            foreach (var t in tempPoints)
            {
                var tempResult = 0;
                foreach (var t1 in tempPoints)
                {
                    var condition = meanDist - CrossPoint.ComputDist(t, t1);
                    var judge = condition >= 0 ? 1 : 0;
                    tempResult += judge; //密度
                }

                density.Add(tempResult);
            }

            try
            {
                if (density.Count > 0)
                {
                    //寻找最大值所在下标   
                    var index = MaxPosition(
                        density); //density.FindIndex(0, density.Count, obj => obj == density.Max());
                    initCenter.Add(tempPoints[index]);
                    var reference = tempPoints[index];
                    for (var i = 0; i < density.Count; i++)
                        if (CrossPoint.ComputDist(tempPoints[i], reference) < meanDist)
                        {
                            density.RemoveAt(i);
                            tempPoints.RemoveAt(i);
                        }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.WriteLine(e.Message + Environment.NewLine + "初始化优化的聚类中心时出错！");
            }
        }

        return initCenter;
    }

    public static bool ContainsKey()
    {
        const bool flag = false;
        return flag;
    }

    /// <summary>
    ///     周期类平均值计算方法，如角度、时间等
    /// </summary>
    /// <param name="data"></param>
    public static double AverageAngle(IList<double> data)
    {
        // 示向度平均值 循环求均值法 http://blog.csdn.net/xiahouzuoxin/article/details/38472845
        var sum = data[0];
        var last = data[0];
        for (var i = 1; i < data.Count; i++)
        {
            var az = data[i];
            var az1 = data[i - 1];
            var err = az - az1 + 180;
            var diff = err - Math.Floor(err / 360) * 360 - 180;
            last += diff;
            sum += last;
        }

        var tempSum = sum / data.Count;
        var average = tempSum - Math.Floor(tempSum / 360) * 360;
        return average;
    }

    public static double CalcStdDev(IList<double> angles)
    {
        var n = angles.Count;
        double sv = 0;
        double cv = 0;
        //% 坐标空间转换 角度转换为单位圆上位置，再转到笛卡尔空间
        for (var i = 0; i < n; i++)
        {
            sv += Math.Sin(angles[i] * (Math.PI / 180.0));
            cv += Math.Cos(angles[i] * (Math.PI / 180.0));
        }

        sv /= n;
        cv /= n;
        //% sqrt(log(1 / r ^ 2));
        var stdDev = Math.Sqrt(-Math.Log(sv * sv + cv * cv));
        stdDev *= 180 / Math.PI;
        return stdDev;
    }

    /// <summary>
    ///     计算离散度σ^2
    /// </summary>
    /// <param name="bearingCount">角度与频次</param>
    /// <param name="sigma">测向离散度</param>
    /// <param name="estimateAngle">估计的角度</param>
    public static void VarianceBearing(SortedList<int, int> bearingCount, out float sigma, out float estimateAngle)
    {
        sigma = 0.0f;
        estimateAngle = 0;
        // 查找频度最大的角度
        var maxCountAngle = bearingCount.First(item => item.Value == bearingCount.Values.Max()).Key;
        // 查找最大频度对应的频次
        var p = 0;
        var c = 0;
        foreach (var angle in bearingCount.Keys)
        {
            p += bearingCount[angle] * Square(angle - maxCountAngle);
            c += bearingCount[angle];
        }

        if (c != 0)
        {
            sigma = 1.0f * p / c;
            sigma = (float)Math.Sqrt(sigma);
        }

        var rangeMin = maxCountAngle - (2 * (int)sigma + 5);
        var rangeMax = maxCountAngle + 2 * (int)sigma + 5;
        var tempConfidence = new SortedList<int, int>();
        foreach (var item in bearingCount)
            if (item.Key >= rangeMin && item.Key <= rangeMax)
                tempConfidence.Add(item.Key, item.Value);
        p = 0;
        c = 0;
        foreach (var angle in tempConfidence.Keys)
        {
            p += tempConfidence[angle] * angle;
            c += tempConfidence[angle];
        }

        if (c == 0) return;
        estimateAngle = (float)p / c;
    }

    /// <summary>
    ///     将度数转换为弧度
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="angle"></param>
    public static T Deg2Rad<T>(double angle)
    {
        return (T)Convert.ChangeType(Math.PI * angle / 180, typeof(T));
    }

    /// <summary>
    ///     将弧度转换为度数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rad"></param>
    public static T Rad2Angle<T>(double rad)
    {
        return (T)Convert.ChangeType(rad * 180 / Math.PI, typeof(T));
    }

    /// <summary>
    ///     将double[,]数组转换成交点集合
    /// </summary>
    /// <param name="points">交点数组</param>
    public static List<CrossPoint> CPointsArrayToList(double[,] points)
    {
        var result = new List<CrossPoint>();
        //n行，2列矩阵，存放交点坐标:x,y
        for (var i = 0; i < points.GetLength(0); i++)
        {
            result.Add(new CrossPoint());
            result[i].CrossPointX = points[i, 0];
            result[i].CrossPointY = points[i, 1];
        }

        return result;
    }

    public static double[,] ListToArray(List<CrossPoint> points)
    {
        var m = points.Count;
        var result = new double[m, 2];
        for (var i = 0; i < m; i++)
        {
            result[i, 0] = points[i].CrossPointX;
            result[i, 1] = points[i].CrossPointY;
        }

        return result;
    }

    /// <summary>
    ///     打印矩阵
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="matrix"></param>
    public static void PrintMatrix<T>(T[,] matrix)
    {
        var r = matrix.GetLength(0);
        var c = matrix.GetLength(1);
        for (var i = 0; i < r; i++)
        {
            for (var j = 0; j < c; j++)
            {
                Console.Write(matrix[i, j].ToString());
                Console.Write("\t");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    ///     <para>根据双曲线参数生成双曲线点集，准备绘制曲线</para>
    ///     <para>需要的输入参数，监测站1，监测站2坐标，两监测站到发射源之间的距离差，估计的发射源坐标</para>
    ///     <para>返回参数集合（双曲线四部分），XRight，XLeft，YUp，YDown，横坐标与纵坐标两两组合即可绘制完整双曲线</para>
    /// </summary>
    /// <param name="cp1">监测站1</param>
    /// <param name="cp2">监测站2</param>
    /// <param name="distDiff">两监测站的距离差</param>
    /// <param name="crossPoint">TDOA返回的交点</param>
    /// <param name="points">绘制双曲线所需要的点集</param>
    /// <param name="rotateAngle">返回要旋转的角度</param>
    /// <param name="rotateCenter">旋转中心</param>
    /// <returns>判断双曲线点集生成是否成功</returns>
    public static bool ProduceHyperPoints(HeatPoints cp1, HeatPoints cp2, double distDiff, HeatPoints crossPoint,
        out List<double[]> points, out float rotateAngle, out HeatPoints rotateCenter)
    {
        //station =[20, 30; 80,30];% (20,30),(80,30),测试数据
        const int n = 4096;
        if (cp1.Lng > cp2.Lng) (cp1, cp2) = (cp2, cp1);

        var middle = new HeatPoints((cp1.Lng + cp2.Lng) / 2.0, (cp1.Lat + cp2.Lat) / 2.0);
        rotateCenter = middle;
        points = [];
        // 距离差为0时，绘制两个站点之间的中垂线
        if (distDiff == 0)
        {
            var x = new double[n];
            var y = new double[n];
            // 在地图上定义中垂线跨度为一度，1°长
            const double dLat = 1.0 / n;
            for (var i = 0; i < n; i++)
            {
                x[i] = middle.Lng;
                y[i] = middle.Lat - 0.5 + i * dLat;
            }

            points.Add(x);
            points.Add(y);
            //监测站构成的向量
            var vector1 = new HeatPoints(cp2.Lng - cp1.Lng, cp2.Lat - cp1.Lat);
            //旋转角度 
            _ = float.TryParse(
                (180 / Math.PI * Math.Atan(Math.Abs(vector1.Lat) / Math.Abs(vector1.Lng))).ToString(CultureInfo
                    .InvariantCulture),
                out rotateAngle);
            return true;
        }

        // 距离差不为0的情况
        var distStation = Math.Sqrt(Square(cp1.Lng - cp2.Lng) + Square(cp1.Lat - cp2.Lat));
        var c = distStation / 2.0;
        var a = distDiff / 2.0;
        var a2 = Square(a);
        double b2 = -1;
        var interval = (cp2.Lng - cp1.Lng) / n;
        if (c - a <= 0)
            // rotateAngle = 0.0f;
            Debug.WriteLine("无法构成双曲线");
        else
            b2 = Square(c) - Square(a); //b_2-c^2-a^2                
        //存储双曲线，的四部分
        var xRight = new double[n];
        var xLeft = new double[n];
        var yUp = new double[n];
        var yDown = new double[n];

        #region 计算要旋转的角度

        var vector = new HeatPoints(cp2.Lng - cp1.Lng, cp2.Lat - cp1.Lat); //监测站构成的向量        
        //旋转角度 
        _ = float.TryParse(
            (180 / Math.PI * Math.Atan(Math.Abs(vector.Lat) / Math.Abs(vector.Lng))).ToString(CultureInfo
                .InvariantCulture),
            out rotateAngle);

        #endregion

        #region 生成双曲线点集

        xRight[0] = a + middle.Lng;
        //生成双曲线坐标点集数据
        for (var i = 1; i < xRight.Length; i++) xRight[i] = xRight[i - 1] + interval;
        try
        {
            for (var j = 0; j < yUp.Length; j++)
                yUp[j] = Math.Sqrt(b2 * (Square(xRight[j] - middle.Lng) / a2 - 1)) +
                         middle.Lat; //sqrt((a^2/x^2-1)*b^2)                               
        }
        catch
        {
            Debug.WriteLine("算术运算错误，可能是对复数开方所导致！");
        }

        //从标准直角坐标系向middle平移
        for (var i = 0; i < xRight.Length; i++)
        {
            //XRight[i] += middle.Lng;
            xLeft[i] = 2 * middle.Lng - xRight[i];
            //YUp[i] += middle.Lat;
            yDown[i] = -1 * yUp[i];
            yDown[i] += 2 * middle.Lat;
        }

        #endregion

        #region 判断交点位置，返回交点所在那一侧的一条

        if (crossPoint == null)
        {
            points.Add(xRight);
            points.Add(xLeft);
        }
        else if (crossPoint.Lng > middle.Lng)
        {
            points.Add(xRight);
        }
        else
        {
            points.Add(xLeft);
        }

        #endregion

        points.Add(yUp);
        points.Add(yDown);
        /*
        using (StreamWriter sw = new StreamWriter(System.Environment.CurrentDirectory + "\\Hyperpola.txt", false))
        {
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points[i].Length; j++)
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(points[i][j]);
                }
                sw.WriteLine(points[i].ToString());
            }
        }*/
        return true;
    }

    ///// <summary>
    ///// 将数组的指定索引范围重新赋值
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="array"></param>
    ///// <param name="startIndex"></param>
    ///// <param name="endIndex"></param>
    ///// <param name="value"></param>
    //public static void SetArrayValue<T>(ref T[] array, int startIndex, int endIndex, T value)
    //{
    //    if (array == null)
    //        throw new ArgumentNullException("array");
    //    if (startIndex < 0)
    //        startIndex = 0;
    //    if (endIndex > array.Length)
    //        endIndex = array.Length;
    //    for (int i = startIndex; i < endIndex; i++)
    //    {
    //        array[i] = value;
    //    }
    //}         
    public static Complex Sqrt(double d)
    {
        if (d < 0)
            return new Complex(0, Math.Sqrt(-d));
        if (d == 0)
            return new Complex();
        return new Complex(Math.Sqrt(d), 0);
    }

    /// <summary>
    ///     计算复数的n次幂
    /// </summary>
    /// <param name="c"></param>
    /// <param name="index"></param>
    public static Complex Pow(Complex c, double index = 2)
    {
        var m = c.Magnitude;
        var phase = c.Phase;
        var result = Math.Pow(m, index) * new Complex(Math.Cos(phase * index), Math.Sin(phase * index));
        return result;
    }

    /// <summary>
    ///     返回指定数的平方
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="param"></param>
    public static T Square<T>(T param)
    {
        var temp = (double)Convert.ChangeType(param, typeof(double));
        return (T)Convert.ChangeType(Square(temp), typeof(T));
    }

    /// <summary>
    ///     返回数值的平方
    /// </summary>
    /// <param name="param"></param>
    public static Complex Square(Complex param)
    {
        return param * param;
    }

    /// <summary>
    ///     返回数值的立方
    /// </summary>
    /// <param name="param"></param>
    public static Complex Cube(Complex param)
    {
        return param * param * param;
    }

    /// <summary>
    ///     返回指定数字的平方
    /// </summary>
    /// <param name="param">指定数字</param>
    public static double Square(double param)
    {
        return param * param;
    }

    /// <summary>
    ///     返回指定数字的平方
    /// </summary>
    /// <param name="param">指定数字</param>
    public static float Square(float param)
    {
        return param * param;
    }

    /// <summary>
    ///     返回指定数字的平方
    /// </summary>
    /// <param name="param">指定数字</param>
    public static int Square(int param)
    {
        return param * param;
    }

    /// <summary>
    ///     计算两个向量的协方差矩阵
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static double[,] GetCovarianceMatrix(double[] vector1, double[] vector2)
    {
        if (vector1 == null || vector2 == null) return null;
        if (vector1.Length != vector2.Length) return null;
        var rows = vector1.Length;
        var meanX = Mean(vector1);
        var meanY = Mean(vector2);
        for (var i = 0; i < vector1.Length; i++) vector1[i] -= meanX;
        for (var i = 0; i < vector2.Length; i++) vector2[i] -= meanY;
        var xx00 = VectorsInProduct(vector1, vector1);
        var xx01 = VectorsInProduct(vector1, vector2);
        var xx10 = VectorsInProduct(vector2, vector1);
        var xx11 = VectorsInProduct(vector2, vector2);
        return new[,]
        {
            { xx00 / (rows - 1), xx01 / (rows - 1) },
            { xx10 / (rows - 1), xx11 / (rows - 1) }
        };
    }

    /// <summary>
    ///     获取二维数组每一列的均值，按顺序存储
    /// </summary>
    /// <param name="data"></param>
    internal static double[] Mean(double[,] data)
    {
        var column1 = GetOneColumn(data, 0);
        var column2 = GetOneColumn(data, 1);
        return [Mean(column1), Mean(column2)];
    }

    /// <summary>
    ///     求一组数的均值
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    public static double Mean(double[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return double.NaN;
        double result = 0;
        for (var i = 0; i < data.Length; i++) result += i;
        return result / data.Length;
    }

    /// <summary>
    ///     返回一组数的平均值
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    public static double Mean(IList<double> data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Count == 0) return double.NaN;
        var result = data.Sum();
        return result / data.Count;
    }

    /// <summary>
    ///     求一组数的均值
    /// </summary>
    /// <param name="data"></param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    public static float Mean(float[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return float.NaN;
        var result = data.Sum();
        return result / data.Length;
    }

    /**
         * Fast algorithm of PDF(probability density function) estimation
         * of a random sequence. The binning method and third-order cardinal
         * spline kernel function are used for reducing the computation
         * amount. Lambda is the coefficient of kernel bandwidth "kb", the
         * optimal "kb"( 2.11/Lx^0.2 * std(xn) ) is chosen when "lambda=1.0".
         * If you want to compute more point's probability, you can use a
         * smaller "lambda", such as 0.5, in this case you'd better make a
         * smoothing for the result.
         */
    public static List<double[]> Pdf(IList<double> xn, double lambda = 0.001d)
    {
        double tmpDbl;
        int i;
        var lx = xn.Count;
        var result = new List<double[]>();
        var kb = lambda * 2.107683 * StdSigma(xn.ToArray()) / Math.Pow(lx, 0.2);
        var r = new List<double>(new double[lx]);
        var pos = new List<int>(new int[lx]);
        var index = new List<int>(new int[lx]);
        // binning the data into blocks, each block width is "kb"
        for (i = 0; i < lx; ++i)
        {
            tmpDbl = xn[i] / kb;
            pos[i] = (int)tmpDbl;
            r[i] = tmpDbl - pos[i];
        }

        // compute the blocks index
        var tmpInt = 1 - Min(pos);
        for (i = 0; i < lx; ++i) index[i] = pos[i] + tmpInt;
        // weights for left, middle and right block
        var weightL = new double[lx];
        var weightC = new double[lx];
        var weightR = new double[lx];
        for (i = 0; i < lx; ++i)
        {
            tmpDbl = r[i];
            weightL[i] = (1 - tmpDbl) * (1 - tmpDbl) / 2.0;
            weightC[i] = 0.5 + tmpDbl * (1 - tmpDbl);
            weightR[i] = tmpDbl * tmpDbl / 2.0;
        }

        var lpx = Max(index) - Min(index) + 3;
        var px = new double[lpx];
        // compute probability density function
        for (i = 0; i < lx; ++i)
        {
            var k = index[i];
            px[k - 1] += weightL[i] / lx;
            px[k] += weightC[i] / lx;
            px[k + 1] += weightR[i] / lx;
        }

        var min = Min(xn);
        var max = Max(xn);
        var x = new double[lpx];
        for (var j = 0; j < px.Length; j++) x[j] = min + j * (max - min) / px.Length;
        result.Add(x);
        result.Add(px);
        return result;
    }

    /// <summary>
    ///     获取最大概率密度对应的项
    /// </summary>
    /// <param name="xn">原始数据项集合</param>
    /// <param name="lambda">平滑因子</param>
    public static double GetMaxPdfItem(IList<double> xn, double lambda = 0.001d)
    {
        var temp = Pdf(xn, lambda);
        var x = temp[0];
        var pdf = temp[1];
        var index = MaxPosition(pdf);
        var result = x[index];
        return result;
    }

    /// <summary>
    ///     求一组数的标准差
    /// </summary>
    /// <param name="data"></param>
    public static double StdSigma(IList<double> data)
    {
        var mean = Mean(data);
        var result = VarSigma(data, mean);
        return Math.Sqrt(result);
    }

    /// <summary>
    ///     求一组数的标准差
    /// </summary>
    /// <param name="data"></param>
    public static float StdSigma(float[] data)
    {
        var mean = Mean(data);
        var result = StdSigma(data, mean);
        return (float)Math.Sqrt(result);
    }

    /// <summary>
    ///     计算数组的方差
    /// </summary>
    /// <param name="data">输入数组</param>
    /// <param name="mean">输入数组的均值</param>
    internal static double VarSigma(IList<double> data, double mean)
    {
        if (data.Count == 0)
            return double.NaN;
        if (data.Count == 1) return 0.0d;
        double result = 0;
        foreach (var t in data)
            result += Square(mean - t);

        result /= data.Count - 1;
        return result;
    }

    /// <summary>
    ///     计算数组的标准差
    /// </summary>
    /// <param name="data">输入数组</param>
    /// <param name="mean">输入数组的均值</param>
    internal static float StdSigma(float[] data, float mean)
    {
        if (data.Length == 1)
            return 0.0f;
        if (data.Length == 0) return float.NaN;
        float result = 0;
        foreach (var t in data)
            result += Square(mean - t);

        result /= data.Length - 1;
        return (float)Math.Sqrt(result);
    }

    /// <summary>
    ///     将多个一维数组组合成二维数组
    /// </summary>
    /// <param name="arrayList">传入的多个一维数组</param>
    public static double[,] ToMultiDimensionArray(List<double[]> arrayList)
    {
        var mdArray = new double[arrayList.Count, arrayList[0].Length];
        var rowIndex = 0;
        arrayList.ForEach(x =>
        {
            Buffer.BlockCopy(
                x, // src
                0, // srcOffset
                mdArray, // dst
                rowIndex++ * mdArray.GetLength(1) * sizeof(double), // dstOffset
                x.Length * sizeof(double)); // count                
        });
        return mdArray;
    }

    /// <summary>
    ///     计算已知发射功率(mW)，接收灵敏度(dBm)以及工作频率（MHz）情况下的无线电波传输距离(KM)
    /// </summary>
    /// <param name="emitPower">发射功率(mW)</param>
    /// <param name="recvSens">接收灵敏度(dBm)</param>
    /// <param name="frequency">工作频率(MHz)</param>
    /// <returns>传输距离(KM)</returns>
    public static double TransDistance(double emitPower, double recvSens, double frequency)
    {
        var los = emitPower - recvSens;
        var distance = Math.Pow(10, (los - 32.44 - 20 * Math.Log10(frequency)) / 20.0d);
        return distance;
    }

    /// <summary>
    ///     返回根据天线高度计算得到的视距传输范围
    /// </summary>
    /// <param name="h">天线高度（m）</param>
    /// <returns>视距传输距离（Km）</returns>
    public static double LosTransRange(double h)
    {
        const int r = 6370;
        h /= 1000.0;
        var d = Math.Sqrt(h * h + 2 * r * h);
        return 2 * d;
    }

    /// <summary>
    ///     矩阵求逆
    /// </summary>
    /// <param name="matrix">输入矩阵,必须是方阵</param>
    public static double[,] InverseMatrix(double[,] matrix)
    {
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        int dimension;
        if (row == column)
            dimension = row;
        else
            throw new Exception("必须为方阵！");
        double tmp;
        var bTmp = new double[20];
        var cTmp = new double[20];
        int k;
        int k3;
        int j;
        int i;
        var kme = new int[20];
        var kmf = new int[20];
        var i2 = 0;
        var j2 = 0;
        for (k = 0; k < dimension; k++)
        {
            var tmp2 = 0.0d;
            for (i = k; i < dimension; i++)
            for (j = k; j < dimension; j++)
            {
                if (Math.Abs(matrix[i, j]) <= Math.Abs(tmp2)) continue;
                tmp2 = matrix[i, j];
                i2 = i;
                j2 = j;
            }

            if (i2 != k)
                for (j = 0; j < dimension; j++)
                {
                    tmp = matrix[i2, j];
                    matrix[i2, j] = matrix[k, j];
                    matrix[k, j] = tmp;
                }

            if (j2 != k)
                for (i = 0; i < dimension; i++)
                {
                    tmp = matrix[i, j2];
                    matrix[i, j2] = matrix[i, k];
                    matrix[i, k] = tmp;
                }

            kme[k] = i2;
            kmf[k] = j2;
            for (j = 0; j < dimension; j++)
            {
                if (j == k)
                {
                    bTmp[j] = 1.0 / tmp2;
                    cTmp[j] = 1.0;
                }
                else
                {
                    bTmp[j] = -matrix[k, j] / tmp2;
                    cTmp[j] = matrix[j, k];
                }

                matrix[k, j] = 0.0;
                matrix[j, k] = 0.0;
            }

            for (i = 0; i < dimension; i++)
            for (j = 0; j < dimension; j++)
                matrix[i, j] += cTmp[i] * bTmp[j];
        }

        for (k3 = 0; k3 < dimension; k3++)
        {
            k = dimension - k3 - 1;
            var k1 = kme[k];
            var k2 = kmf[k];
            if (k1 != k)
                for (i = 0; i < dimension; i++)
                {
                    tmp = matrix[i, k1];
                    matrix[i, k1] = matrix[i, k];
                    matrix[i, k] = tmp;
                }

            if (k2 != k)
                for (j = 0; j < dimension; j++)
                {
                    tmp = matrix[k2, j];
                    matrix[k2, j] = matrix[k, j];
                    matrix[k, j] = tmp;
                }
        }

        return matrix;
    }

    /// <summary>
    ///     求矩阵逆矩阵，必须为方阵
    /// </summary>
    /// <param name="matrix"></param>
    public static double[,] InverseMatrixExpand(double[,] matrix)
    {
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        var n = row == column ? row : 0;
        if (n == 0) throw new ArgumentException("求逆矩阵必须为方阵！！", nameof(matrix));
        var result = CopyTwoDArray(matrix);
        int i;
        const double eps = 10e-16;
        for (i = 0; i < n; i++)
        {
            if (result[i, i] <= eps || result[i, i] <= -1 * eps) return null;
            var q = 1.0 / result[i, i];
            result[i, i] = 1.0;
            int k;
            for (k = 0; k < n; k++) result[i, k] *= q;
            int j;
            for (j = 0; j < n; j++)
                if (i != j)
                {
                    q = result[j, i];
                    result[j, i] = 0.0;
                    for (k = 0; k < n; k++) result[j, k] -= q * result[i, k];
                }
        }

        return result;
    }

    /// <summary>
    ///     求矩阵matrix的逆矩阵，矩阵可能不存在逆矩阵，此时返回null
    /// </summary>
    /// <param name="matrix"></param>
    public static double[,] GetInverseMatrix(double[,] matrix)
    {
        var row = matrix.GetLength(0);
        // int column = matrix.GetLength(1);
        // 定义扩展矩阵
        var expandMatrix = new double[row, row * 2];
        // 定义得到的逆矩阵
        var newMatrix = new double[row, row];
        //初始化扩展矩阵
        InitExpandMatrix(matrix, expandMatrix);
        //调整扩展矩阵，若某一列全为0，则行列式的值等于0，不存在逆矩阵
        var canAdjust = AdjustMatrix(expandMatrix);
        //如果不存在逆矩阵，返回null  
        if (!canAdjust) return null;
        // 计算扩展矩阵
        CalculateExpandMatrix(expandMatrix);
        // 用计算过的扩展矩阵取后面的N*N矩阵，为所求
        GetNewMatrix(expandMatrix, newMatrix);
        return newMatrix;
    }

    /*初始化扩展矩阵*/
    private static void InitExpandMatrix(double[,] initMatrix, double[,] expandMatrix)
    {
        var row = expandMatrix.GetLength(0);
        var column = expandMatrix.GetLength(1);
        for (var i = 0; i < row; i++)
        for (var j = 0; j < column; j++)
            if (j < row)
            {
                //左边的N*N矩阵原样赋值  
                expandMatrix[i, j] = initMatrix[i, j];
            }
            else
            {
                //右边N*N赋值为单位矩阵  
                if (j == row + i) //如果为右边矩阵的对角线就赋值为1  
                    expandMatrix[i, j] = 1;
                else
                    expandMatrix[i, j] = 0;
            }
    }

    /// <summary>
    ///     调整扩展矩阵，若某一列全为0，则行列式的值等于0，不存在逆矩阵
    /// </summary>
    /// <param name="expandMatrix"></param>
    private static bool AdjustMatrix(double[,] expandMatrix)
    {
        var row = expandMatrix.GetLength(0);
        for (var i = 0; i < row; i++)
            if (expandMatrix[i, i] == 0)
            {
                //如果某行对角线数值为0  
                int j;
                /*搜索该列其他不为0的行，如果都为0，则返回false*/
                for (j = 0; j < row; j++)
                {
                    GetOneRow(expandMatrix, i);
                    GetOneRow(expandMatrix, j);
                    if (expandMatrix[j, i] != 0)
                        //如果有不为0的行，交换这两行  
                        break;
                }

                if (j >= row)
                {
                    //没有不为0的行  
#if DEBUG
                    Console.WriteLine("此矩阵没有逆矩阵");
#endif
                    return false;
                }
            }

        return true;
    }

    /// <summary>
    ///     计算扩展矩阵
    /// </summary>
    /// <param name="expandMatrix"></param>
    private static void CalculateExpandMatrix(double[,] expandMatrix)
    {
        var row = expandMatrix.GetLength(0);
        var column = expandMatrix.GetLength(1);
        for (var i = 0; i < row; i++)
        {
            var firstElement = expandMatrix[i, i];
            for (var j = 0; j < column; j++)
                // 将该行所有元素除以首元素  
                expandMatrix[i, j] /= firstElement;
            // 把其他行再该列的数值都化为0
            for (var m = 0; m < row; m++)
            {
                //遇到自己的行跳过  
                if (m == i) continue;
                var times = expandMatrix[m, i];
                for (var n = 0; n < column; n++) expandMatrix[m, n] -= expandMatrix[i, n] * times;
            }
        }
    }

    /// <summary>
    ///     用计算过的扩展矩阵取后面的N*N矩阵，为所求
    /// </summary>
    /// <param name="expandMatrix"></param>
    /// <param name="newMatrix"></param>
    private static void GetNewMatrix(double[,] expandMatrix, double[,] newMatrix)
    {
        var row = expandMatrix.GetLength(0);
        var column = expandMatrix.GetLength(1);
        for (var i = 0; i < row; i++)
        for (var j = 0; j < column; j++)
            if (j >= row)
                newMatrix[i, j - row] = expandMatrix[i, j];
    }

    internal struct PointLatLng(double lat, double lng)
    {
        public double Lng = lng;
        public double Lat = lat;
        public static readonly PointLatLng Empty = new();
    }

    #region 高阶累积量

    public static double C20(double[] s)
    {
        var n = s.Length;
        var temp = new double[n];
        for (var i = 0; i < n; i++) temp[i] = Square(s[i]);
        var result = Sum(temp) / n;
        return result;
    }

    public static double C21(double[] s)
    {
        var n = s.Length;
        var temp = new double[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Math.Abs(s[i]));
        return Sum(temp) / n;
    }

    public static double C40(double[] s)
    {
        var n = s.Length;
        var temp = new double[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Square(s[i]));
        var temp1 = Sum(temp) / n;
        return temp1 - 3 * Square(C20(s));
    }

    public static double C41(double[] s)
    {
        var n = s.Length;
        var temp = new double[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Square(s[i]));
        var temp1 = Sum(temp) / n;
        var result = temp1 - 3 * C20(s) * C21(s);
        return result;
    }

    public static double C42(double[] s)
    {
        var n = s.Length;
        var temp = new double[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Square(Math.Abs(s[i])));
        var temp1 = Sum(temp) / n;
        var result = temp1 - Square(Math.Abs(C20(s))) - 2 * Square(C21(s));
        return result;
    }

    public static double C60(double[] s)
    {
        var n = s.Length;
        var four = new double[n];
        var six = new double[n];
        for (var i = 0; i < n; i++) four[i] = Math.Pow(s[i], 4);
        for (var i = 0; i < n; i++) six[i] = Math.Pow(s[i], 6);
        var temp1 = Sum(six) / n;
        var result = temp1 - 15 * (Sum(four) / n) * C20(s) - 30 * Math.Pow(C20(s), 3);
        return result;
    }

    public static double C63(double[] s)
    {
        var n = s.Length;
        var six = new double[n];
        for (var i = 0; i < n; i++) six[i] = Math.Pow(Math.Abs(s[i]), 6);
        var temp1 = Sum(six) / n;
        var result = temp1 - 9 * C42(s) * C21(s) - 6 * Math.Pow(C21(s), 3);
        return result;
    }

    #endregion

    #region 高阶累积量

    public static Complex C20(Complex[] s)
    {
        var n = s.Length;
        var temp = new Complex[n];
        for (var i = 0; i < n; i++) temp[i] = Square(s[i]);
        var result = Sum(temp) / n;
        return result;
    }

    public static Complex C21(Complex[] s)
    {
        var n = s.Length;
        var temp = new Complex[n];
        for (var i = 0; i < n; i++) temp[i] = Square(s[i].Magnitude);
        var result = Sum(temp) / n;
        return result;
    }

    public static Complex C40(Complex[] s)
    {
        var n = s.Length;
        var temp = new Complex[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Square(s[i]));
        var temp1 = Sum(temp) / n;
        var result = temp1 - 3 * Square(C20(s));
        return result;
    }

    public static Complex C41(Complex[] s)
    {
        var n = s.Length;
        var temp = new Complex[n];
        for (var i = 0; i < n; i++) temp[i] = Cube(s[i]) * Complex.Conjugate(s[i]);
        var temp1 = Sum(temp) / n;
        var result = temp1 - 3 * C20(s) * C21(s);
        return result;
    }

    public static Complex C42(Complex[] s)
    {
        var n = s.Length;
        var temp = new Complex[n];
        for (var i = 0; i < n; i++) temp[i] = Square(Square(s[i].Magnitude));
        var temp1 = Sum(temp) / n;
        var result = temp1 - Square(C20(s).Magnitude) - 2 * Square(C21(s));
        return result;
    }

    public static Complex C60(Complex[] s)
    {
        var n = s.Length;
        var four = new Complex[n];
        var six = new Complex[n];
        for (var i = 0; i < n; i++) four[i] = Square(Square(s[i]));
        for (var i = 0; i < n; i++) six[i] = Square(Cube(s[i]));
        var temp1 = Sum(six) / n;
        var result = temp1 - 15 * (Sum(four) / n) * C20(s) - 30 * Cube(C20(s));
        return result;
    }

    public static Complex C63(Complex[] s)
    {
        var n = s.Length;
        var six = new Complex[n];
        for (var i = 0; i < n; i++) six[i] = Square(Cube(s[i].Magnitude));
        var temp1 = Sum(six) / n;
        var result = temp1 - 9 * C42(s) * C21(s) - 6 * Cube(C21(s));
        return result;
    }

    #endregion

    #region matrix computation

    /// <summary>
    ///     求两圆的交点之间的距离
    /// </summary>
    /// <param name="cic1">圆1</param>
    /// <param name="cic2">圆2</param>
    /// <returns>交点之间距离</returns>
    public static double CircleCrossDist(HeatPoints cic1, HeatPoints cic2)
    {
        var x0 = cic1.Lng;
        var y0 = cic1.Lat;
        var rad = cic1.Radius;
        var x1 = cic2.Lng;
        var y1 = cic2.Lat;
        var r = cic2.Radius;
        var d = Math.Sqrt(Square(x0 - x1) + Square(y0 - y1)); //两圆心距离
        var k1 = (y0 - y1) / (x0 - x1); //连接两圆心直线
        // double b1 = y1 - (k1 * x1);
        var k2 = -1 / k1; //公共弦方程直线
        var b2 = (Square(rad) - Square(r) - Square(x0) + Square(x1) - Square(y0) + Square(y1)) / (2 * (y1 - y0));
        var result = double.MinValue;
        if (Math.Abs(d - Math.Abs(r - rad)) < 1e-9 || Math.Abs(d - (r + rad)) < 1e-9) //相切时的交点
        {
            // double xx = -(b1 - b2) / (k1 - k2);
            // double yy = -((-b2 * k1) + (b1 * k2)) / (k1 - k2);
            // p = new double { xx, yy };
        }
        else if (Math.Abs(r - rad) < d && d < r + rad) //相交时的交点
        {
            var a = -b2 * k2 + x1 + k2 * y1;
            var b = Math.Sqrt(-1 * Square(b2)
                              + Square(r)
                              + Square(k2) * Square(r)
                              - 2 * b2 * k2 * x1
                              - Square(k2) * Square(x1)
                              + 2 * b2 * y1
                              + 2 * k2 * x1 * y1
                              - Square(y1));
            var c = 1 + Square(k2);
            //公共弦方程与其中一个圆的交点
            var xx1 = (a - b) / c;
            var yy1 = k2 * xx1 + b2;
            var xx2 = (a + b) / c;
            var yy2 = k2 * xx2 + b2;
            result = Math.Sqrt(Square(xx1 - xx2) + Square(yy1 - yy2));
        }

        return result;
    }

    /// <summary>
    ///     拷贝二维数组
    /// </summary>
    /// <param name="matrix"></param>
    private static double[,] CopyTwoDArray(double[,] matrix)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1);
        var result = new double[m, n];
        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
            result[i, j] = matrix[i, j];
        return result;
    }

    /// <summary>
    ///     返回两个向量的协方差，用于计算协方差矩阵
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns>失败返回-1</returns>
    public static double Cov(double[] v1, double[] v2)
    {
        if (v1 != null && v2 != null && v1.Length == v2.Length)
        {
            var m1 = Mean(v1);
            var m2 = Mean(v2);
            double cov = 0;
            for (var i = 0; i < v1.Length; i++) cov += (v1[i] - m1) * (v2[i] - m2);
            cov /= v1.Length - 1;
            return cov;
        }

        return -1.0d;
    }

    /// <summary>
    ///     返回矩阵的协方差矩阵，按输入矩阵的列向量作为信号元素进行计算，行向量的可以转置之后再计算
    /// </summary>
    /// <param name="matrix"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static double[,] GetCovarianceMatrix(double[,] matrix)
    {
        if (matrix == null || matrix.GetLength(0) == 0 || matrix.GetLength(1) == 0)
            throw new ArgumentNullException(nameof(matrix));
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        var result = new double[column, column];
        var vectors = new List<double[]>();
        for (var i = 0; i < column; i++)
        {
            var temp = new double[row];
            for (var j = 0; j < row; j++) temp[j] = matrix[j, i];
            vectors.Add(temp);
        }

        for (var i = 0; i < column; i++)
        for (var j = 0; j < row; j++)
            result[i, j] = Cov(vectors[i], vectors[j]);
        return result;
    }

    private static double Dist(double[] vector1, double[] vector2)
    {
        var sum = 0.0;
        var m = vector1.Length;
        var n = vector2.Length;
        if (m == n)
        {
            for (var i = 0; i < m; i++) sum += Square(vector1[i] - vector2[i]);
        }
        else
        {
            Debug.WriteLine("计算向量距离时，维数不一致！");
            return double.MaxValue;
        }

        return Math.Sqrt(sum);
    }

    /// <summary>
    ///     产生单位矩阵，可以指定幅度
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="n">方阵的维</param>
    /// <param name="x">初始值</param>
    public static T[,] Eye<T>(int n, T x)
    {
        var tmp = new T[n, n];
        for (var i = 0; i < n; ++i) tmp[i, i] = x;
        return tmp;
    }

    /// <summary>
    ///     一维数组寻找波峰、波谷位置索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns>波峰、波谷的位置索引集合</returns>
    public static List<List<int>> FindPeaksValleys<T>(IList<T> data)
    {
        // 一阶导判断增长方向，二阶导判断是否存在极值
        if (data == null) throw new ArgumentNullException(nameof(data));
        // 便于通用将data全部转换为double进行比较运算
        var tempData = Array.ConvertAll(data.ToArray(), item => (double)Convert.ChangeType(item, typeof(double)));
        var length = data.Count;
        IList<int> sign = new List<int>();
        for (var i = 1; i < length; i++)
        {
            /*相邻值做差： 小于0，赋-1;大于0，赋1;等于0，赋0*/
            var diff = tempData[i] - tempData[i - 1];
            if (diff > 0)
                sign.Add(1);
            else if (diff < 0)
                sign.Add(-1);
            else
                sign.Add(0);
        }

        //再对sign相邻位做差  
        //保存极大值和极小值的位置  
        var indexMax = new List<int>();
        var indexMin = new List<int>();
        for (var j = 1; j < sign.Count; j++)
        {
            var diff = sign[j] - sign[j - 1];
            if (diff < 0)
                indexMax.Add(j);
            else if (diff > 0) indexMin.Add(j);
        }

        var result = new List<List<int>>
        {
            indexMax,
            indexMin
        };
        return result;
    }

    /// <summary>
    ///     幂法实现求矩阵范数-2
    /// </summary>
    /// <param name="matrix"></param>
    public static double GetMatrixNorm(double[,] matrix)
    {
        //转换成方阵之后再求范数，确保准确性 
        var itrTimes = 0;
        var m = matrix.GetLength(0);
        var temp = MatrixMultiplyTrans(matrix);
        double[] dl;
        var uVector = new double[m];
        var vVector = new double[m];
        InitUnit(uVector); //初始化初始向量           
        do
        {
            itrTimes++;
            if (itrTimes > 30) break;
            dl = uVector;
            var max = MaxWithAbs(uVector);
            for (var i = 0; i < uVector.Length; i++) vVector[i] = uVector[i] / max;
            uVector = MatrixMultiplyVector(temp, vVector);
        } while (Dist(uVector, dl) > 1e-8);

        dl = uVector;
        var maxVl = MaxWithAbs(dl);
        //计算矩阵的特征向量
        var sum = 0.0;
        foreach (var d in dl) sum += d;
        for (var i = 0; i < dl.Length; i++) dl[i] /= sum;
        return Math.Sqrt(maxVl);
    }

    /// <summary>
    ///     仅供2*2行列式求特征根
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns name="result">
    ///     <para>按顺序返回，加入没有二次方程没有根，返回null</para>
    ///     <para>有两个相同的实根只返回一个值；</para>
    ///     <para>有两个不相同的实根则按大小顺序返回；</para>
    /// </returns>
    internal static List<double> GetMatrixNormBoth(double[,] matrix)
    {
        List<double> result;
        var a = matrix[0, 0];
        var b = matrix[0, 1];
        var c = matrix[1, 0];
        var d = matrix[1, 1];
        var delta = (a - d) * (a - d) + 4 * b * c;
        if (delta < 0.0d) return null;

        if (delta == 0.0d)
        {
            var root = (a + d) / 2.0d;
            result = [root];
        }
        else
        {
            result = [];
            var root1 = (a + d + Math.Sqrt(Square(a - d) + 4 * b * c)) / 2.0d;
            result.Add(root1);
            root1 = (a + d - Math.Sqrt(Square(a - d) + 4 * b * c)) / 2.0d;
            result.Add(root1);
        }

        return result;
    }

    /// <summary>
    ///     反幂法求矩阵最小特征值
    /// </summary>
    /// <param name="matrix"></param>
    internal static double GetMatrixNormMin(double[,] matrix)
    {
        //A为矩阵；ep为精度要求；N为最大迭代次数,本文限定30次
        //m 为绝对值最小的特征值；u为对应最小特征值的特征向量。
        const int n = 30;
        const double ep = 1e-6;
        //列数
        var nCol = matrix.GetLength(1);
        var uVector = new double[nCol];
        Ones(ref uVector);
        var k = 0;
        double m = 0;
        double m1 = 0;
        var invA = GetInverseMatrix(matrix);
        while (k <= n)
        {
            var vVector = MatrixMultiplyVector(invA, uVector);
            m = MaxWithAbs(vVector);
            uVector = VectorDivisionConst(vVector, m);
            if (Math.Abs(m - m1) < ep)
            {
#if DEBUG
                Debug.WriteLine("反幂法求矩阵最小特征值成功！值为{0:000}", 1 / m);
#endif
                break;
            }

            m1 = m;
            ++k;
        }

        if (double.IsNaN(m)) return 0;
        return 1 / m;
    }

    /// <summary>
    ///     从二维数组中获取第n列
    /// </summary>
    /// <param name="data"></param>
    /// <param name="n"></param>
    internal static double[] GetOneColumn(double[,] data, int n)
    {
        if (n > data.GetLength(1)) return null;
        var result = new double[data.GetLength(0)];
        for (var i = 0; i < data.GetLength(0); i++) result[i] = data[i, n];
        return result;
    }

    /// <summary>
    ///     区矩阵第n行
    /// </summary>
    /// <param name="data"></param>
    /// <param name="n"></param>
    internal static double[] GetOneRow(double[,] data, int n)
    {
        if (n > data.GetLength(0)) return null;
        var result = new double[data.GetLength(1)];
        for (var i = 0; i < data.GetLength(1); i++) result[i] = data[n, i];
        return result;
    }

    public static bool GradientHeatMap(double[] centerX, double[] centerY, double[] radius)
    {
        var flag = Math.Abs(radius[0] + radius[1]) >
                   Math.Sqrt(Square(centerX[0] - centerX[1]) + Square(centerY[0] - centerY[1]));
        //两圆相交
        return flag;
    }

    /// <summary>
    ///     初始化为单位向量
    /// </summary>
    /// <param name="data"></param>
    private static void InitUnit(double[] data)
    {
        for (var i = 0; i < data.Length; i++)
            if (data[i] == 0)
                data[i] = 1;
    }

    /// <summary>
    ///     全1向量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="vector"></param>
    public static void Ones<T>(ref T[] vector) where T : struct
    {
        for (var i = 0; i < vector.Length; i++) vector[i] = (T)Convert.ChangeType(1, typeof(T));
    }

    /// <summary>
    ///     全1矩阵
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="n"></param>
    public static T[,] Ones<T>(int n) where T : struct
    {
        var tmp = new T[n, n];
        for (var i = 0; i < n; ++i)
        for (var j = 0; j < n; j++)
            tmp[i, j] = (T)Convert.ChangeType(1, typeof(T));
        return tmp;
    }

    /// <summary>
    ///     判断是否有相同元素
    ///     <para>只要有相同元素就以第一个元素作为交点返回</para>
    /// </summary>
    /// <param name="coordinates">输入的三个交点坐标</param>
    /// <returns>返回判断的bool值</returns>
    public static bool IsSamePoint(List<CrossPoint> coordinates)
    {
        for (var i = 0; i < coordinates.Count - 1; i++)
        for (var j = i + 1; j < coordinates.Count; j++)
            if (Math.Abs(coordinates[i].CrossPointX - coordinates[j].CrossPointX) < 1e-9 &&
                Math.Abs(coordinates[i].CrossPointY - coordinates[j].CrossPointY) < 1e-9)
                return true;

        return false;
    }

    /// <summary>
    ///     判断是否所有元素相同
    ///     <para>只要有相同元素就以第一个元素作为交点返回</para>
    /// </summary>
    /// <param name="coordinates">输入的三个交点坐标</param>
    /// <returns>返回判断的bool值</returns>
    public static bool IsAllSamePoint(List<CrossPoint> coordinates)
    {
        for (var i = 0; i < coordinates.Count - 1; i++)
        for (var j = i + 1; j < coordinates.Count; j++)
            if (Math.Abs(coordinates[i].CrossPointX - coordinates[j].CrossPointX) < 1e-9 &&
                Math.Abs(coordinates[i].CrossPointY - coordinates[j].CrossPointY) < 1e-9)
            {
            }
            else
            {
                // 只要有一个不相同的元素就返回false
                return false;
            }

        return false;
    }

    /// <summary>
    ///     简单的计算校验和方法，按字节相加取第一字节
    /// </summary>
    /// <param name="data"></param>
    public static byte Checksum(byte[] data)
    {
        var sum = 0;
        foreach (var t in data)
            sum += t;

        return BitConverter.GetBytes(sum)[0];
    }

    /// <summary>
    ///     判断字符串是否包含汉字
    /// </summary>
    /// <param name="text"></param>
    public static bool IsChinese(string text)
    {
        var result = false;
        foreach (var t in text)
            if (t > 127)
            {
                result = true;
                break;
            }

        return result;
    }

    /// <summary>
    ///     将字符串截断指定字节数长度，英文ASCII，包含汉字用UTF-8
    /// </summary>
    /// <param name="str"></param>
    /// <param name="length"></param>
    public static string CutToLength(string str, int length)
    {
        var encoding = IsChinese(str) ? Encoding.UTF8 : Encoding.ASCII;
        var count = encoding.GetByteCount(str);
        if (length > count) return str;
        var data = encoding.GetBytes(str);
        var temp = new byte[length];
        Array.Copy(data, 0, temp, 0, temp.Length);
        return encoding.GetString(temp);
    }

    ////交换数组指定的两行，即进行行变换（具体为行交换）    
    //internal static void Swap(double[,] aa, double[,] bb)
    //{
    //    int n = aa.GetLength(1) == bb.GetLength(1) ? aa.GetLength(1) : 0;
    //    if (n == 0)
    //        return;
    //    int i;
    //    double temp;
    //    for (i = 0; i < n; i++)
    //    {
    //        temp = aa[i, n];
    //        aa[i, n] = bb[i, n];
    //        bb[i, n] = temp;
    //    }
    //}
    //public static double MatrixDet(double[,] matrix)
    //{
    //    if (matrix == null) return double.NaN;
    //    int row = matrix.GetLength(0);
    //    int column = matrix.GetLength(1);
    //    int dimension = row == column ? row : 0;
    //    if (dimension == 0)
    //        throw new ArgumentException("行列式必须为方阵才能进行求值运算！");
    //    int iterTimes = 0;  //记录行变换的次数（交换）  
    //    double det = 1.0d;
    //    double yin = 0.0d;
    //    for (int i = 0; i < dimension; i++)
    //    {
    //        if (matrix[i, i] == 0)
    //        {
    //            for (int j = i; j < dimension; j++)
    //            {
    //                if (matrix[j, i] != 0)
    //                {
    //                    Swap(matrix[i, dimension], matrix[j, dimension]);//交换两行    
    //                    iterTimes++;
    //                }
    //            }
    //        }
    //        for (int k = i + 1; k < dimension; k++)
    //        {
    //            yin = -1 * matrix[k, i] / matrix[i, i];
    //            for (int u = 0; u < dimension; u++)
    //            {
    //                matrix[k, u] = matrix[k, u] + matrix[i, u] * yin;
    //            }
    //        }
    //    }
    //    for (int i = 0; i < dimension; i++)  //求对角线的积 即 行列式的值  
    //        det = det * matrix[i, i];
    //    //行变换偶数次符号不变  
    //    if (iterTimes % 2 == 1)
    //        det = -det;
    //    return (det);
    //}
    internal static double MatrixDeterminant(double[,] matrix)
    {
        var n = matrix.GetLength(0);
        double dSum = 1;
        double dSign = 1;
        var matrixChange = new double[n];
        var matrixTemp = new double[n];
        for (var i = 0; i < n; i++) //化成三角阵
        {
            for (var j = i + 1; j < n; j++)
            {
                // 如果要乘的那行第一个数等于0，那就把它与下一列对换后继续
                if (matrix[i, i] == 0)
                {
                    for (var x = 0; x < n; x++)
                    {
                        matrixChange[x] = matrix[i + 1, x];
                        matrix[i + 1, x] = matrix[i, x];
                        matrix[i, x] = matrixChange[x];
                    }

                    dSign *= -1;
                }

                //if (matrix[j,i] != 0)
                matrixTemp[j] = -matrix[j, i] / matrix[i, i];
            }

            for (var k = i + 1; k < n; k++) //将两行乘系数相加，使变成0
            for (var m = i; m < n; m++)
                matrix[k, m] = matrixTemp[k] * matrix[i, m] + matrix[k, m];
            //if (matrix[k,m] < Math.Pow(10,-13))
            //    matrix[k,m] = 0;
        }

        // 三角阵对角线相乘得到结果
        for (var q = 0; q < n; q++) dSum = matrix[q, q] * dSum;
        return dSum * dSign;
    }

    /// <summary>
    ///     矩阵按列进行拼接为行向量
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="matrix"></param>
    /// <exception cref="ArgumentNullException"><paramref name="matrix" /> is <c>null</c>.</exception>
    public static T[] MatrixJointtoVector<T>(T[,] matrix)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        var result = new T[row * column];
        var index = 0;
        for (var i = 0; i < column; i++)
        for (var j = 0; j < row; j++)
            result[index++] = matrix[j, i];
        return result;
    }

    /// <summary>
    ///     矩阵相减
    /// </summary>
    /// <param name="matrix1">矩阵1</param>
    /// <param name="matrix2">被减矩阵</param>
    /// <returns>返回矩阵相减的差</returns>
    public static double[,] MatrixMinus(double[,] matrix1, double[,] matrix2)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var m1 = matrix2.GetLength(0);
        var n1 = matrix2.GetLength(1);
        var result = new double[m, n];
        if (m == m1 && n == n1)
            for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                result[i, j] = matrix1[i, j] - matrix2[i, j];
        return result;
    }

    /// <summary>
    ///     矩阵相减
    /// </summary>
    /// <param name="matrix1">矩阵1</param>
    /// <param name="matrix2">被减矩阵</param>
    /// <returns>返回矩阵相减的差</returns>
    public static Complex[,] MatrixMinus(Complex[,] matrix1, Complex[,] matrix2)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var m1 = matrix2.GetLength(0);
        var n1 = matrix2.GetLength(1);
        var result = new Complex[m, n];
        if (m == m1 && n == n1)
            for (var i = 0; i < m; i++)
            for (var j = 0; j < n; j++)
                result[i, j] = matrix1[i, j] - matrix2[i, j];
        return result;
    }

    /// <summary>
    ///     矩阵转置
    /// </summary>
    /// <param name="matrix1">要求转置的矩阵</param>
    public static Complex[,] MatrixTrans(Complex[,] matrix1)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var result = new Complex[n, m];
        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
            result[j, i] = Complex.Conjugate(matrix1[i, j]);
        return result;
    }

    /// <summary>
    ///     矩阵转置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="matrix"></param>
    public static T[,] MatrixTrans<T>(T[,] matrix)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1);
        var result = new T[n, m];
        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
            result[j, i] = matrix[i, j];
        return result;
    }

    /// <summary>
    ///     矩阵与自身的转置相乘
    /// </summary>
    /// <param name="matrix1">待计算矩阵</param>
    /// <returns>matrix*matri’n*n矩阵</returns>
    public static double[,] MatrixMultiplyTrans(double[,] matrix1)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1); //m*n
        var temp = MatrixTrans(matrix1); //n*m
        var result = new double[m, m];
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
            //对b的每列进行遍历
        for (var j = 0; j < m; j++)
            //if(matrix1[i,j]!=0),//如果可以出现0值，则可以用该语句进行优化
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
        for (var k = 0; k < n; k++)
            result[i, j] += matrix1[i, k] * temp[k, j];
        return result;
    }

    /// <summary>
    ///     矩阵与自身的转置相乘
    /// </summary>
    /// <param name="matrix">待计算矩阵</param>
    /// <returns>matrix*matri’n*n矩阵</returns>
    public static Complex[,] MatrixMultiplyTrans(Complex[,] matrix)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1); //m*n
        var temp = MatrixTrans(matrix); //n*m
        var result = new Complex[m, m];
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
            //对b的每列进行遍历
        for (var j = 0; j < m; j++)
            //if(matrix1[i,j]!=0),//如果可以出现0值，则可以用该语句进行优化
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
        for (var k = 0; k < n; k++)
            result[i, j] += matrix[i, k] * temp[k, j];
        return result;
    }

    /// <summary>
    ///     矩阵相乘
    /// </summary>
    /// <param name="matrix1">矩阵1</param>
    /// <param name="matrix2">矩阵2</param>
    /// <returns>matrix*matri’n*n矩阵</returns>
    public static double[,] MatrixMultiplyMatrix(double[,] matrix1, double[,] matrix2)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var p = matrix2.GetLength(0);
        var q = matrix2.GetLength(1);
        if (n != p) return null;
        var result = new double[m, q];
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
            //对b的每列进行遍历
        for (var j = 0; j < q; j++)
            //if(matrix1[i,j]!=0),//如果可以出现0值，则可以用该语句进行优化
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
        for (var k = 0; k < n; k++)
            result[i, j] += matrix1[i, k] * matrix2[k, j];
        return result;
    }

    /// <summary>
    ///     矩阵相乘
    /// </summary>
    /// <param name="matrix1">矩阵1</param>
    /// <param name="matrix2">矩阵2</param>
    /// <returns>matrix*matri’n*n矩阵</returns>
    public static Complex[,] MatrixMultiplyMatrix(Complex[,] matrix1, Complex[,] matrix2)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var p = matrix2.GetLength(0);
        var q = matrix2.GetLength(1);
        if (n != p) return null;
        var result = new Complex[m, q];
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
            //对b的每列进行遍历
        for (var j = 0; j < q; j++)
            //if(matrix1[i,j]!=0),//如果可以出现0值，则可以用该语句进行优化
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
        for (var k = 0; k < n; k++)
            result[i, j] += matrix1[i, k] * matrix2[k, j];
        return result;
    }

    /// <summary>
    ///     矩阵相乘
    /// </summary>
    /// <param name="matrix1">矩阵1</param>
    /// <param name="matrix2">矩阵2</param>
    /// <returns>matrix*matri’n*n矩阵</returns>
    public static Complex[,] MatrixMultiplyMatrix(Complex[,] matrix1, double[,] matrix2)
    {
        var m = matrix1.GetLength(0);
        var n = matrix1.GetLength(1);
        var p = matrix2.GetLength(0);
        var q = matrix2.GetLength(1);
        if (n != p) return null;
        var result = new Complex[m, q];
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
            //对b的每列进行遍历
        for (var j = 0; j < q; j++)
            //if(matrix1[i,j]!=0),//如果可以出现0值，则可以用该语句进行优化
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
        for (var k = 0; k < n; k++)
            result[i, j] += matrix1[i, k] * matrix2[k, j];
        return result;
    }

    /// <summary>
    ///     矩阵乘以常数
    /// </summary>
    /// <param name="matrix">要求转置的矩阵</param>
    /// <param name="cons">常数</param>
    public static double[,] MatrixMultiplyConst(double[,] matrix, double cons)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1);
        var result = new double[n, m];
        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
            result[j, i] = matrix[i, j] * cons;
        return result;
    }

    /// <summary>
    ///     矩阵乘以常数
    /// </summary>
    /// <param name="matrix">要求转置的矩阵</param>
    /// <param name="cons">常数</param>
    public static Complex[,] MatrixMultiplyConst(Complex[,] matrix, double cons)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1);
        var result = new Complex[n, m];
        for (var i = 0; i < m; i++)
        for (var j = 0; j < n; j++)
            result[j, i] = matrix[i, j] * cons;
        return result;
    }

    /// <summary>
    ///     矩阵与向量相乘，需满足矩阵的列等于向量的行数
    /// </summary>
    /// <param name="matrix">待乘矩阵</param>
    /// <param name="vector">被乘的向量</param>
    /// <returns>matrix*vector</returns>
    public static double[] MatrixMultiplyVector(double[,] matrix, double[] vector)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1); //m*n
        var result = new double[m];
        if (n != vector.Length) return result;
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
        {
            //c为每一个点的值
            double middleValue = 0;
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
            for (var k = 0; k < n; k++) middleValue += matrix[i, k] * vector[k];
            result[i] = middleValue;
        }

        return result;
    }

    /// <summary>
    ///     矩阵与向量相乘，需满足矩阵的列等于向量的行数
    /// </summary>
    /// <param name="matrix">待乘矩阵</param>
    /// <param name="vector">被乘的向量</param>
    /// <returns>matrix*vector</returns>
    public static Complex[] MatrixMultiplyVector(Complex[,] matrix, double[] vector)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1); //m*n
        var result = new Complex[m];
        if (n != vector.Length) return result;
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
        {
            //c为每一个点的值
            Complex middleValue = 0;
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
            for (var k = 0; k < n; k++) middleValue += matrix[i, k] * vector[k];
            result[i] = middleValue;
        }

        return result;
    }

    /// <summary>
    ///     矩阵与向量相乘，需满足矩阵的列等于向量的行数
    /// </summary>
    /// <param name="matrix">待乘矩阵</param>
    /// <param name="vector">被乘的向量</param>
    /// <returns>matrix*vector</returns>
    public static Complex[] MatrixMultiplyVector(Complex[,] matrix, Complex[] vector)
    {
        var m = matrix.GetLength(0);
        var n = matrix.GetLength(1); //m*n
        var result = new Complex[m];
        if (n != vector.Length) return result;
        //对a的每行进行遍历
        for (var i = 0; i < m; i++)
        {
            //c为每一个点的值
            Complex middleValue = 0;
            //第i行j列的值为a的第i行上的n个数和b的第j列上的n个数对应相乘之和，其中n为a的列数，也是b的行数，a的列数和b的行数相等
            for (var k = 0; k < n; k++) middleValue += matrix[i, k] * vector[k];
            result[i] = middleValue;
        }

        return result;
    }

    private static double Max(double[] data)
    {
        var max = data[0];
        foreach (var t in data)
            if (t > max)
                max = t;

        return max;
    }

    /// <summary>
    ///     返回数组中绝对值最大的值
    /// </summary>
    /// <param name="data"></param>
    private static double MaxWithAbs(double[] data)
    {
        var max = data[0];
        foreach (var t in data)
            if (Math.Abs(t) > Math.Abs(max))
                max = t;

        return max;
    }

    private static double Min(double[] data)
    {
        var min = data[0];
        foreach (var t in data)
            if (t < min)
                min = t;

        return min;
    }

    public static T Min<T>(IList<T> data) where T : IComparable
    {
        var min = data[0];
        foreach (var t in data)
            if (t != null)
                if (t.CompareTo(min) < 0)
                    min = t;

        return min;
    }

    public static T Max<T>(IList<T> data) where T : IComparable
    {
        var max = data[0];
        foreach (var t in data)
            if (t != null)
                if (t.CompareTo(max) > 0)
                    max = t;

        return max;
    }

    /// <summary>
    ///     返回最大值索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public static int MaxPosition<T>(T[] data) where T : IComparable
    {
        var position = 0;
        var max = data[0];
        for (var i = 0; i < data.Length; i++)
            if (data[i] is IComparable)
                if (data[i].CompareTo(max) > 0)
                {
                    max = data[i];
                    position = i;
                }

        return position;
    }

    private static int MaxPosition<T>(List<T> data) where T : IComparable
    {
        var position = 0;
        var max = data[0];
        for (var i = 0; i < data.Count; i++)
            if (data[i] is IComparable)
                if (data[i].CompareTo(max) > 0)
                {
                    max = data[i];
                    position = i;
                }

        return position;
    }

    /// <summary>
    ///     返回数组中绝对值最大的值的索引
    /// </summary>
    /// <param name="data"></param>
    public static int MaxPositionWithAbs(double[] data)
    {
        var index = 0;
        var max = data[0];
        for (var i = 0; i < data.Length; i++)
            if (Math.Abs(data[i]) > Math.Abs(max))
            {
                max = data[i];
                index = i;
            }

        return index;
    }

    /// <summary>
    ///     返回数组中绝对值最大的值的索引
    /// </summary>
    /// <param name="data"></param>
    public static int MaxPositionWithAbs(Complex[] data)
    {
        var index = 0;
        var max = data[0].Magnitude;
        for (var i = 0; i < data.Length; i++)
            if (data[i].Magnitude > max)
            {
                max = data[i].Magnitude;
                index = i;
            }

        return index;
    }

    /// <summary>
    ///     返回最小值索引
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public static int MinPosition<T>(T[] data) where T : IComparable
    {
        var position = 0;
        var max = data[0];
        for (var i = 0; i < data.Length; i++)
            if (data[i] is IComparable)
                if (data[i].CompareTo(max) < 0)
                {
                    max = data[i];
                    position = i;
                }

        return position;
    }

    /// <summary>
    ///     返回数组中绝对值最小的值的索引
    /// </summary>
    /// <param name="data"></param>
    public static int MinPositionWithAbs(double[] data)
    {
        var index = 0;
        var max = data[0];
        for (var i = 0; i < data.Length; i++)
            if (Math.Abs(data[i]) < Math.Abs(max))
            {
                max = data[i];
                index = i;
            }

        return index;
    }

    public static double Mod(double x, double y)
    {
        if (y == 0)
            return x;
        if (Math.Abs(x - y) < 1e-9)
            return 0;
        return x - Math.Floor(x / y) * y;
    }

    private static int MinPosition(double[] data)
    {
        var position = 0;
        var min = data[0];
        for (var i = 0; i < data.Length; i++)
            if (data[i] < min)
            {
                min = data[i];
                position = i;
            }

        return position;
    }

    /// <summary>
    ///     求向量的范数，norm（a），norm（a，2）
    /// </summary>
    /// <param name="vector1">输入向量数组</param>
    /// <returns>求得的范数</returns>
    public static double Norm(double[] vector1)
    {
        var sum = 0.0;
        var m = vector1.Length;
        for (var i = 0; i < m; i++) sum += vector1[i] * vector1[i];
        return Math.Sqrt(sum);
    }

    /// <summary>
    ///     返回向量的内积的均方根，即向量的二范数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="vector"></param>
    public static T Norm<T>(IList<T> vector) where T : struct
    {
        var v = Array.ConvertAll(vector.ToArray(), item => (double)Convert.ChangeType(item, typeof(double)));
        var norm = Norm(v);
        return (T)Convert.ChangeType(norm, typeof(T));
    }

    /// <summary>
    ///     求复数的范数即复数对应的模值
    /// </summary>
    /// <param name="data">复数数组</param>
    public static double[] Norm(Complex[] data)
    {
        var result = new double[data.Length];
        for (var i = 0; i < data.Length; i++) result[i] = Norm(data[i]);
        return result;
    }

    /// <summary>
    ///     返回复数的二范数，也即极坐标下矢量的模值
    /// </summary>
    /// <param name="data"></param>
    public static double Norm(Complex data)
    {
        if (double.IsInfinity(data.Real) || double.IsInfinity(data.Imaginary)) return double.PositiveInfinity;
        return Math.Sqrt(Square(data.Real) + Square(data.Imaginary));
    }

    /// <summary>
    ///     返回一个复数实部和虚部的二范数，也即极坐标下矢量的模值
    /// </summary>
    /// <param name="real"></param>
    /// <param name="image"></param>
    public static double Norm(double real, double image)
    {
        return Math.Sqrt(Square(real) + Square(image));
    }

    /// <summary>
    ///     归一化到[-1,1]
    /// </summary>
    /// <param name="data">待归一化数据</param>
    public static void Normalize(ref double[] data)
    {
        var min = data.Min();
        var max = data.Max();
        var len = data.Length;
        var den = max - min;
        if (den == 0) return;
        for (var i = 0; i < len; i++) data[i] = 2 * (data[i] - min) / den - 1;
    }

    /// <summary>
    ///     归一化到[-1,1]
    /// </summary>
    /// <param name="data">待归一化数据</param>
    public static void Normalize(ref float[] data)
    {
        var min = data.Min();
        var max = data.Max();
        var len = data.Length;
        var den = max - min;
        if (den == 0f) return;
        for (var i = 0; i < len; i++) data[i] = 2 * (data[i] - min) / den - 1;
    }

    /// <summary>
    ///     归一化到[-1,1]
    /// </summary>
    /// <param name="data">待归一化数据</param>
    public static void Normalize(ref int[] data)
    {
        var min = data.Min();
        var max = data.Max();
        var len = data.Length;
        var den = 1.0f * (max - min);
        if (den == 0) return;
        for (var i = 0; i < len; i++) data[i] = (int)(2 * (data[i] - min) / den - 1);
    }

    /// <summary>
    ///     将角度值归一化处理
    /// </summary>
    /// <typeparam name="T">值类型泛型</typeparam>
    /// <param name="angle"></param>
    public static T NormalizeAngle<T>(T angle) where T : struct
    {
        var temp = (double)Convert.ChangeType(angle, typeof(double));
        temp -= Math.Floor(temp / 360.0) * 360;
        return (T)Convert.ChangeType(temp, typeof(T));
    }

    /// <summary>
    ///     矩阵按最大值归一化
    /// </summary>
    /// <param name="matrix">待归一化的矩阵</param>
    public static void NormalizeMatrix(double[,] matrix)
    {
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        var max = Math.Abs(matrix[0, 0]);
        for (var i = 0; i < row; i++)
        for (var j = 0; j < column; j++)
            if (Math.Abs(matrix[i, j]) > max)
                max = Math.Abs(matrix[i, j]);
        // 此处不考虑全0矩阵，因此max不可能为0，不再做异常判断
        for (var i = 0; i < row; i++)
        for (var j = 0; j < column; j++)
            matrix[i, j] /= max;
    }

    /// <summary>
    ///     返回数组对象的和
    /// </summary>
    /// <param name="data"></param>
    public static double Sum(double[] data)
    {
        var sum = 0.0d;
        foreach (var t in data)
            sum += t;

        return sum;
    }

    /// <summary>
    ///     返回数组对象的和
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public static T Sum<T>(T[] data)
    {
        var sum = 0.0d;
        foreach (var t in data)
            sum += (double)Convert.ChangeType(t, typeof(double));

        return (T)Convert.ChangeType(sum, typeof(T));
    }

    public static Complex Sum(Complex[] data)
    {
        var len = data.Length;
        var sum = new Complex();
        for (var i = 0; i < len; i++) sum += data[i];
        return sum;
    }

    /// <summary>
    ///     向量相加
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static double[] VectorAddVector(double[] vector1, double[] vector2)
    {
        if (vector1 == null || vector2 == null) throw new ArgumentNullException(nameof(vector1));
        var len = vector1.Length;
        var len1 = vector2.Length;
        len = len == len1 ? len : 0;
        if (len == 0) throw new ArgumentException("两向量长度不相等！");
        var result = new double[len];
        for (var i = 0; i < len; i++) result[i] = vector1[i] + vector2[i];
        return result;
    }

    /// <summary>
    ///     计算向量角
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns>返回向量夹角</returns>
    public static double VectorAngle(double[] vector1, double[] vector2)
    {
        /*
         *
         *       |A·B|
         *   ---------------
         *   sqrt(||A||)·sqrt(||B||)
         *
         *   ||A||=sqrt(a1^2+a2^2+...+an^2)
         * */
        var vecDot = VectorsInProduct(vector1, vector2);
        double sumI2 = 0;
        double sumQ2 = 0;
        for (var i = 0; i < vector1.Length; i++)
        {
            sumI2 += vector1[i] * vector1[i];
            sumQ2 += vector2[i] * vector2[i];
        }

        var angle = 180 / Pi * Math.Acos(vecDot / (Math.Sqrt(sumI2) * Math.Sqrt(sumQ2)));
        return angle;
    }

    public static int VectorAngle(int[] vector1, int[] vector2)
    {
        /*
         *
         *       |A·B|
         *   ---------------
         *   sqrt(||A||)·sqrt(||B||)
         *
         *   ||A||=sqrt(a1^2+a2^2+...+an^2)
         * */
        var vecDot = VectorsInProduct(vector1, vector2);
        var sumI2 = 0;
        var sumQ2 = 0;
        for (var i = 0; i < vector1.Length; i++)
        {
            sumI2 += vector1[i] * vector1[i];
            sumQ2 += vector2[i] * vector2[i];
        }

        return (int)(180 / Pi * Math.Acos(vecDot / (Math.Sqrt(sumI2) * Math.Sqrt(sumQ2))));
    }

    public static short VectorAngle(short[] vector1, short[] vector2)
    {
        /*
         *
         *       |A·B|
         *   ---------------
         *   sqrt(||A||)·sqrt(||B||)
         *
         *   ||A||=sqrt(a1^2+a2^2+...+an^2)
         * */
        var vecDot = VectorsInProduct(vector1, vector2);
        long sumI2 = 0;
        long sumQ2 = 0;
        for (var i = 0; i < vector1.Length; i++)
        {
            sumI2 += vector1[i] * vector1[i];
            sumQ2 += vector2[i] * vector2[i];
        }

        return (short)(180 / Pi * Math.Acos(vecDot / (Math.Sqrt(sumI2) * Math.Sqrt(sumQ2))));
    }

    /// <summary>
    ///     计算向量角
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    /// <returns>返回向量夹角</returns>
    public static double VectorAngle<T>(IList<T> vector1, IList<T> vector2) where T : struct
    {
        /*
         *
         *       |A·B|
         *   ---------------
         *   sqrt(||A||)·sqrt(||B||)
         *
         *   ||A||=sqrt(a1^2+a2^2+...+an^2)
         * */
        var vecDot = VectorsInProduct(vector1, vector2);
        var vectorDot = (double)Convert.ChangeType(vecDot, typeof(double));
        var sumI2 = (double)Convert.ChangeType(Norm(vector1), typeof(double));
        var sumQ2 = (double)Convert.ChangeType(Norm(vector2), typeof(double));
        var angle = 180 / Pi * Math.Acos(vectorDot / (sumI2 * sumQ2));
        return angle;
    }

    /// <summary>
    ///     向量相减
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static double[] VectorMinusVector(double[] vector1, double[] vector2)
    {
        if (vector1 == null || vector2 == null) throw new ArgumentNullException(nameof(vector1));
        var len = vector1.Length;
        var len1 = vector2.Length;
        len = len == len1 ? len : 0;
        if (len == 0) throw new ArgumentException("两向量长度不相等！");
        var result = new double[len];
        for (var i = 0; i < len; i++) result[i] = vector1[i] - vector2[i];
        return result;
    }

    /// <summary>
    ///     向量除以指定数
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="num"></param>
    public static double[] VectorDivisionConst(double[] vector, double num)
    {
        if (vector == null) return null;
        if (num == 0) throw new DivideByZeroException();
        var len = vector.Length;
        var result = new double[len];
        for (var i = 0; i < len; i++) result[i] = vector[i] / num;
        return result;
    }

    /// <summary>
    ///     向量乘以指定数
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="num"></param>
    public static double[] VectorMultiplyConst(double[] vector, double num)
    {
        if (vector == null) return null;
        var len = vector.Length;
        var result = new double[len];
        for (var i = 0; i < len; i++) result[i] = vector[i] * num;
        return result;
    }

    /// <summary>
    ///     向量除以指定数
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="num"></param>
    public static float[] VectorDivisionConst(float[] vector, float num)
    {
        if (vector == null) return null;
        if (num == 0) throw new DivideByZeroException();
        var len = vector.Length;
        var result = new float[len];
        for (var i = 0; i < len; i++) result[i] = vector[i] / num;
        return result;
    }

    /// <summary>
    ///     两个向量做乘法
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static double[] VectorsMultiply(double[] vector1, double[] vector2)
    {
        var m = vector1.Length;
        var n = vector2.Length;
        var result = new double[m];
        if (m == n)
            for (var i = 0; i < n; i++)
                result[i] = vector1[i] * vector2[i];
        return result;
    }

    /// <summary>
    ///     vector-vector tranpose multiplication: a * b^H.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static Complex[,] VectorsMultiplyTranpose(Complex[] a, Complex[] b)
    {
        var rows = a.Length;
        var columns = b.Length;
        var tmp = new Complex[rows, columns];
        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < columns; ++j)
            tmp[i, j] = Complex.Conjugate(a[i]) * Complex.Conjugate(b[j]);
        return tmp;
    }

    /// <summary>
    ///     vector-vector tranpose multiplication: a * b^H.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static double[,] VectorsMultiplyTranpose(double[] a, double[] b)
    {
        var rows = a.Length;
        var columns = b.Length;
        var tempA = Array.ConvertAll(a, d => new Complex(d, 0.0d));
        var tempB = Array.ConvertAll(b, d => new Complex(d, 0.0d));
        var tmp = new Complex[rows, columns];
        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < columns; ++j)
            tmp[i, j] = tempA[i] * Complex.Conjugate(tempB[j]);
        var result = new double[rows, columns];
        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < columns; ++j)
            result[i, j] = tmp[i, j].Real;
        return result;
    }

    /// <summary>
    ///     vector-vector tranpose multiplication: a * b^H.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static Complex[,] VectorsMultiplyTranpose(double[] a, Complex[] b)
    {
        var tempA = Array.ConvertAll(a, d => new Complex(d, 0.0d));
        return VectorsMultiplyTranpose(tempA, b);
    }

    /// <summary>
    ///     向量内积
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static double VectorsInProduct(double[] vector1, double[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length) return double.MinValue;
        var result = 0.0d;
        for (var i = 0; i < vector1.Length; i++) result += vector1[i] * vector2[i];
        return result;
    }

    /// <summary>
    ///     计算向量内积
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static int VectorsInProduct(int[] vector1, int[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length) return int.MinValue;
        var result = 0;
        for (var i = 0; i < vector1.Length; i++) result += vector1[i] * vector2[i];
        return result;
    }

    /// <summary>
    ///     计算向量内积
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static float VectorsInProduct(float[] vector1, float[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length) return float.MinValue;
        var result = 0.0f;
        for (var i = 0; i < vector1.Length; i++) result += vector1[i] * vector2[i];
        return result;
    }

    /// <summary>
    ///     计算向量内积
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static short VectorsInProduct(short[] vector1, short[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length) return short.MinValue;
        var result = 0;
        for (var i = 0; i < vector1.Length; i++) result += vector1[i] * vector2[i];
        return (short)result;
    }

    /// <summary>
    ///     计算向量内积
    /// </summary>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static Complex VectorsInProduct(Complex[] vector1, Complex[] vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Length != vector2.Length) return Complex.Zero;
        Complex result = 0;
        for (var i = 0; i < vector1.Length; i++) result += vector1[i] * vector2[i];
        return result;
    }

    /// <summary>
    ///     向量内积
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="vector1"></param>
    /// <param name="vector2"></param>
    public static T VectorsInProduct<T>(IList<T> vector1, IList<T> vector2)
    {
        if (vector1 == null || vector2 == null || vector1.Count != vector2.Count) return default;
        var v1 = Array.ConvertAll(vector1.ToArray(), item => (double)Convert.ChangeType(item, typeof(double)));
        var v2 = Array.ConvertAll(vector2.ToArray(), item => (double)Convert.ChangeType(item, typeof(double)));
        double dot = 0;
        for (var i = 0; i < vector1.Count; i++) dot += v1[i] * v2[i];
        return (T)Convert.ChangeType(dot, typeof(T));
    }

    /// <summary>
    ///     vector-vector tranpose multiplication: a * b^H.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public static Complex[,] MultiTr(Complex[] a, Complex[] b)
    {
        var rows = a.Length;
        var columns = b.Length;
        var tmp = new Complex[rows, columns];
        for (var i = 0; i < rows; ++i)
        for (var j = 0; j < columns; ++j)
            tmp[i, j] = a[i] * Complex.Conjugate(b[j]);
        return tmp;
    }

    #endregion
}

/// <summary>
///     扩展方法集
/// </summary>
public static class AdditiveMethods
{
    private const int SmoothingLen = 11;

    /// <summary>
    ///     获取矩阵中某单元格内的元素
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="r"></param>
    /// <param name="c"></param>
    public static T GetCell<T>(this T[,] array, int r = 0, int c = 0)
    {
        // int row = array.GetLength(0);
        // int column = array.GetLength(1);
        T cell;
        try
        {
            cell = array[r, c];
        }
        catch (Exception)
        {
            throw new ArgumentOutOfRangeException(nameof(array));
        }

        return cell;
    }

    /// <summary>
    ///     获取数组一行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="n">第几行,从零开始</param>
    public static T[] GetRow<T>(this T[,] array, int n = 0)
    {
        var row = array.GetLength(0);
        var column = array.GetLength(1);
        var getRow = new T[column];
        if (row > 0 && column > 0 && n < row)
            for (var j = 0; j < column; j++)
                getRow[j] = array[n, j];
        else
            throw new ArgumentOutOfRangeException(nameof(array), "read row index OutOfRangeException");
        return getRow;
    }

    /// <summary>
    ///     获取数组一列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="n">第几列,从零开始</param>
    public static T[] GetColumn<T>(this T[,] array, int n = 0)
    {
        var row = array.GetLength(0);
        var column = array.GetLength(1);
        var getColumn = new T[row];
        if (row > 0 && column > 0 && n < column)
            for (var j = 0; j < row; j++)
                getColumn[j] = array[j, n];
        else
            throw new ArgumentOutOfRangeException(nameof(array), "read column index OutOfRangeException");
        return getColumn;
    }

    /// <summary>
    ///     将数组的指定索引范围重新赋值，赋值范围[startIndex, endIndex]，即包含边界
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="startIndex">起始索引，越界时自动修正</param>
    /// <param name="endIndex">结束索引，越界时自动修正</param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"><paramref name="array" /> is <c>null</c>.</exception>
    public static void SetArrayValue<T>(this T[] array, int startIndex, int endIndex, T value)
    {
        try
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (startIndex < 0)
                startIndex = 0;
            else if (startIndex > array.Length - 1) startIndex = array.Length - 1;
            if (endIndex > array.Length - 1)
                endIndex = array.Length - 1;
            else if (endIndex < 0) endIndex = 0;
            for (var i = startIndex; i <= endIndex; i++) array[i] = value;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    ///     加窗
    ///     返回平滑窗口系数
    /// </summary>
    /// <param name="data"></param>
    public static void Smoothing(this double[] data)
    {
        // 平滑窗口宽度
        // var windowsWidth = new double[SmoothingLen];
        var tempArray = new double[data.Length + SmoothingLen];
        var tempValue = new double[data.Length];
        // 按照前偶后奇的原则为扫描数据首尾添加数据
        for (var i = 0; i <= SmoothingLen / 2 - 1; i++) tempArray[i] = data[0];
        Array.Copy(data, 0, tempArray, SmoothingLen / 2, data.Length);
        for (var i = data.Length + SmoothingLen / 2; i <= tempArray.Length - 1; i++) tempArray[i] = data[^1];
        // 计算窗平均值
        for (var i = 0; i <= data.Length - 1; i++)
        {
            double sum = 0;
            for (var j = 0; j <= SmoothingLen - 1; j++) sum += tempArray[i + j];
            sum /= SmoothingLen;
            // 把门限值加上容差值
            tempValue[i] = sum;
        }

        Array.Copy(tempValue, data, tempValue.Length);
        //return data;
    }

    /// <summary>
    ///     加窗
    ///     返回平滑窗口系数
    /// </summary>
    /// <param name="data"></param>
    public static void Smoothing(this short[] data)
    {
        // 平滑窗口宽度
        // var windowsWidth = new short[SmoothingLen];
        var tempArray = new short[data.Length + SmoothingLen];
        // var tempValue = new short[data.Length];
        // 按照前偶后奇的原则为扫描数据首尾添加数据
        for (var i = 0; i <= SmoothingLen / 2 - 1; i++) tempArray[i] = data[0];
        Array.Copy(data, 0, tempArray, SmoothingLen / 2, data.Length);
        for (var i = data.Length + SmoothingLen / 2; i <= tempArray.Length - 1; i++) tempArray[i] = data[^1];
        // 计算窗平均值
        for (var i = 0; i <= data.Length - 1; i++)
        {
            short sum = 0;
            for (var j = 0; j <= SmoothingLen - 1; j++) sum += tempArray[i + j];
            sum /= SmoothingLen;
            // 把门限值加上容差值
            data[i] = sum;
        }
    }
}