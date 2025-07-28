using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using NetTopologySuite.Geometries;
using SkiaSharp;

// using System.Drawing;
// using System.Drawing.Drawing2D;
// using System.Drawing.Imaging;

namespace Magneto.Contract.Algorithm;

public class FieldStrengthLocateHandle
{
    internal const string LibKrigingPath = "libkriging";

    private static Krig _krig;

    // private readonly Color[] _allColors;
    private readonly SKColor[] _allSkColors;

    /// <summary>
    ///     场强定位在地图上的刷新频率 10 秒
    /// </summary>
    private readonly int _constFsLocationTimeInterval = 5000;

    /// <summary>
    ///     缓存场强定位数据
    /// </summary>
    private readonly List<SFieldStrengthTrackData> _listSFieldStrengthTrackData = [];

    private readonly ManualResetEvent _manualResetEvent = new(true);
    private readonly object _syncObj = new();

    /// <summary>
    ///     插值网格大小，单位是°，在北纬四十度附近，0.001°约等于85米
    /// </summary>
    private double _dXy = 0.001;

    private Thread _fsLocationThread;
    private bool _isClear;

    /// <summary>
    ///     坐标边界X最大值
    /// </summary>
    private double _maxX;

    /// <summary>
    ///     坐标边界Y最大值
    /// </summary>
    private double _maxY;

    /// <summary>
    ///     坐标边界X最小值
    /// </summary>
    private double _minX;

    /// <summary>
    ///     坐标边界Y最小值
    /// </summary>
    private double _minY;

    static FieldStrengthLocateHandle()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "Common", [LibKrigingPath]);
    }

    public FieldStrengthLocateHandle()
    {
        // _allColors = GetColors(100);
        _allSkColors = GetSkColors(100);
    }

    public bool IsRunning { get; private set; } = true;

    /// <summary>
    ///     克里金插值函数
    /// </summary>
    /// <param name="x">经度数组，单位°</param>
    /// <param name="y">纬度数组，单位°</param>
    /// <param name="z">经纬度对应的电平值数组，单位dB</param>
    /// <param name="count">坐标对的个数，也是电平值数据长度</param>
    /// <param name="edge">要进行插值的边界范围，[lng,lat,lng,lat...]</param>
    /// <param name="edgecount">要进行插值的边界坐标长度</param>
    /// <param name="dgxy">插值网格大小，即插值精度，根据面积范围动态计算</param>
    /// <param name="krig">传入指定结构体，由算法计算赋值后返回</param>
    /// <param name="result">插值结果，边界外为-9999</param>
    /// <param name="maxdb">插值后的最大电平值</param>
    /// <param name="maxlng">插值后的最大电平值最大值所在经度</param>
    /// <param name="maxlat">插值后的最大电平值最大值所在纬度</param>
    [DllImport(LibKrigingPath, EntryPoint = "get_interpolation_by_kriging",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void Krig_interpolation(double[] x,
        double[] y,
        double[] z,
        int count,
        double[] edge,
        int edgecount,
        double dgxy,
        ref Krig krig,
        double[] result,
        ref double maxdb,
        ref double maxlng,
        ref double maxlat);

    /// <summary>
    ///     等值线计算函数
    /// </summary>
    /// <param name="krig">克里金插值结果</param>
    /// <param name="result">为克里金插值结果中的场强向量，单位为 ，总长度为：krig.nRow*krig.nCol，按行存储</param>
    /// <param name="edge">边界</param>
    /// <param name="edgeLength">边界长度</param>
    /// <param name="maxdbinterval">场强间隔，每隔多少画一条线</param>
    /// <param name="dbcount">定义等值线级数</param>
    /// <param name="db">为等势线对应的不同dB值，限定范围为[0dB,100dB]</param>
    /// <param name="dbcountoutput">根据实际情况输出等值线级数，若实际级数大于定义的级数，则输出定义级数，反之，输出一个实际最大级数</param>
    /// <param name="lineIndex">为等势线的各dB下的各等值线的索引</param>
    /// <param name="nLineindex">坐标索引</param>
    /// <param name="eqpl">为等势线的各dB下的各等值线的经纬度值，存储格式为[x0,y0,x1,y1,x2,y2,….,xn,yn];</param>
    /// <param name="nEqpl">等值线经纬度长度</param>
    [DllImport(LibKrigingPath, EntryPoint = "get_equipotential_line_by_kriging",
        CallingConvention = CallingConvention.Cdecl)]
    private static extern void Pequipotential_line(ref Krig krig,
        double[] result,
        double[] edge,
        int edgeLength,
        double maxdbinterval,
        int dbcount,
        double[] db,
        ref int dbcountoutput,
        int[] lineIndex,
        ref int nLineindex,
        double[] eqpl,
        ref int nEqpl);

    // public double[] LoatePoisitionGps = new double[2] { 0, 0 };
    public event EventHandler<SsoaBitmapData> UpdateSsoaDataEvent;

    public void AddData(SFieldStrengthTrackData sBearingData)
    {
        lock (_syncObj)
        {
            _listSFieldStrengthTrackData.Add(sBearingData);
        }
    }

    public void Clear()
    {
        lock (_syncObj)
        {
            _listSFieldStrengthTrackData.Clear();
        }

        _isClear = true;
    }

    /// <summary>
    ///     开始起算线程
    /// </summary>
    public void Start()
    {
        if (_fsLocationThread == null)
        {
            _fsLocationThread = new Thread(Handling)
            {
                IsBackground = true,
                Name = "_fsLocationThread"
            };
            _fsLocationThread.Start();
            IsRunning = true;
        }

        _manualResetEvent.Set();
    }

    /// <summary>
    ///     停止绘制
    /// </summary>
    public void Stop()
    {
        IsRunning = false;
        _fsLocationThread = null;
        _manualResetEvent.Set();
    }

    /// <summary>
    ///     暂停计算
    /// </summary>
    public void Pause()
    {
        _manualResetEvent.Reset();
    }

    /// <summary>
    ///     处理数据插值并绘制定位结果热力图
    /// </summary>
    private void Handling()
    {
        while (IsRunning)
        {
            _manualResetEvent.WaitOne();
            try
            {
                if (!IsRunning) break;
                _isClear = false;
                List<SFieldStrengthTrackData> currentData = null;
                lock (_syncObj)
                {
                    if (_listSFieldStrengthTrackData.Count > 15)
                    {
                        //保持最大采样点为2000
                        if (_listSFieldStrengthTrackData.Count > 600)
                            _listSFieldStrengthTrackData.RemoveRange(0, 100);
                        currentData = [.._listSFieldStrengthTrackData];
                    }
                }

                //第一步：计算当前数据集中的最大最小经纬度，确定大致范围
                if (currentData != null)
                {
                    _minY = currentData[0].Latitude;
                    _maxY = currentData[0].Latitude;
                    _minX = currentData[0].Longitude;
                    _maxX = currentData[0].Longitude;
                    foreach (var t in currentData)
                    {
                        _minY = _minY < t.Latitude
                            ? _minY
                            : t.Latitude;
                        _maxY = _maxY > t.Latitude
                            ? _maxY
                            : t.Latitude;
                        _minX = _minX < t.Longitude
                            ? _minX
                            : t.Longitude;
                        _maxX = _maxX > t.Longitude
                            ? _maxX
                            : t.Longitude;
                    }

                    var w = _maxX - _minX;
                    var h = _maxY - _minY;
                    //处理当移动范围较小时候，不进行插值，至少应该比定义的网格大一倍
                    if (w < 0.0005 || h < 0.0005)
                    {
                        Thread.Sleep(100);
                        continue;
                    }

                    //第二步：根据行车轨迹动态计算边界
                    var points = new double[currentData.Count * 2];
                    var algorithm = new ConvexAlgorithm();
                    algorithm.FindConvexHull(currentData.Select(c => c.Longitude).ToArray(),
                        currentData.Select(c => c.Latitude).ToArray(),
                        currentData.Count, points, out var length);
                    //外接边界地理坐标集合
                    var coords = new Coordinate[length / 2 + 1];
                    //构建地理坐标
                    for (var i = 0; i < length / 2; i++)
                    {
                        var index = i * 2;
                        coords[i] = new Coordinate(points[index], points[index + 1]);
                    }

                    //确保边界闭合
                    coords[^1] = coords[0];
                    //将边界缓冲,
                    //第三步:将计算出的边界，进行地理缓冲分析，向外扩张0.002°
                    var linearRing =
                        new LinearRing(coords);
                    var polygon =
                        new Polygon(linearRing);
                    var bufferPolygon =
                        polygon.Buffer(0.001 * 2) as Polygon;
                    //获取缓冲后的坐标
                    var edge = new double[bufferPolygon.Coordinates.Length * 2];
                    var edgeGpsPoints = new List<PointLatLng>();
                    for (var k = 0; k < bufferPolygon.Coordinates.Length; k++)
                    {
                        edge[k * 2] = bufferPolygon.Coordinates[k].X;
                        edge[k * 2 + 1] = bufferPolygon.Coordinates[k].Y;
                        var p = new PointLatLng
                        {
                            Longitude = bufferPolygon.Coordinates[k].X,
                            Latitude = bufferPolygon.Coordinates[k].Y
                        };
                        edgeGpsPoints.Add(p);
                    }

                    //根据范围计算合理插值精度_dxy
                    const int maxKrigPoint = 5000; //设置一个最大插值点数，由于面积计算误差，实际插值点数比这个值大
                    var area = CalculateAreasLngLat(edgeGpsPoints);
                    _dXy = Math.Round(Math.Sqrt(area / maxKrigPoint), 6);
                    //设置理论插值结果大小，长宽都延长5倍
                    //估计不准，索性设置一个定值
                    //var theoryResult = new double[(Convert.ToInt32(w / (_dXy)) * 5) * (Convert.ToInt32(h / (_dXy)) * 5)]; //理论结果，预估大小
                    var theoryResult = new double[maxKrigPoint * 10]; //理论结果，预估大小
                    //将单精度转双精度
                    var weight = new double[currentData.Count];
                    for (var i = 0; i < currentData.Count; i++)
                        weight[i] = currentData.Select(c => c.FieldStrength).ToArray()[i];
                    //第四步:调用插值算法
                    double maxlng = 0;
                    double maxlat = 0;
                    double maxdb = 0;
                    //
                    _krig.CellSize = 0;
                    _krig.StartLat = 0;
                    _krig.StartLong = 0;
                    _krig.Col = 0;
                    _krig.Row = 0;
                    Krig_interpolation(currentData.Select(c => c.Longitude).ToArray(),
                        currentData.Select(c => c.Latitude).ToArray(),
                        weight, weight.Length, edge,
                        edge.Length / 2, _dXy, ref _krig, theoryResult, ref maxdb, ref maxlng, ref maxlat);
                    if (_krig.Col == 0 || _krig.Row == 0) continue;
                    //根据插值结果重新确定数组大小                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      并copy值过去
                    var interpolationResult = new double[_krig.Col * _krig.Row];
                    Array.Copy(theoryResult, 0, interpolationResult, 0, _krig.Col * _krig.Row); //复制
                    var rasterData = new double[_krig.Row, _krig.Col];
                    for (var i = 0; i < _krig.Row; i++)
                        for (var j = 0; j < _krig.Col; j++)
                        {
                            var value = interpolationResult[i * _krig.Col + j];
                            if (value + 9999 < 0.1) value = double.NaN;
                            rasterData[_krig.Row - 1 - i, j] = value;
                        }

                    //第五步:计算等值线
                    var maxLlPoint = _krig.Col * _krig.Row * 4;
                    var legendDb = new double[100];
                    var legendDbLength = 2;
                    var legendDbLengthout = 3;
                    var lineIndex = new int[maxLlPoint];
                    var lineIndexLength = 0;
                    var legendPosition = new double[maxLlPoint];
                    var legendPositionLength = 0;
                    Pequipotential_line(ref _krig, interpolationResult, edge, edge.Length / 2, 3, legendDbLength,
                        legendDb, ref legendDbLengthout, lineIndex,
                        ref lineIndexLength, legendPosition, ref legendPositionLength);
                    //重新确定数组大小,复制一份 
                    var legendDb1 = new double[legendDbLengthout];
                    var lineIndex1 = new int[lineIndexLength];
                    var legendPosition1 = new double[legendPositionLength];
                    // db值段
                    Array.Copy(legendDb, legendDb1, legendDbLengthout);
                    //索引数组
                    Array.Copy(lineIndex, lineIndex1, lineIndexLength);
                    //坐标数据
                    Array.Copy(legendPosition, legendPosition1, legendPositionLength);
                    //将索引转为需要的list数组
                    var onedblines = new List<List<int>>();
                    var one = new List<int>();
                    // int[] temp = new int[legendDbLength];
                    var temp = new List<int>();
                    //等值线集合,根据范围结果，拼装可以进行直接绘制的数据结果
                    //此处规则较复杂，参考函数说明，还有不明之处咨询监测技术部
                    for (var i = 0; i < lineIndex1.Length; i++)
                    {
                        if (lineIndex1[i] == 0)
                        {
                            if (one.Count > 0)
                            {
                                onedblines.Add(one);
                                one = [];
                            }

                            if (i > 0)
                            {
                                var t = lineIndex1[i - 1];
                                temp.Add(t);
                            }
                        }

                        var total = 0;
                        foreach (var t in temp)
                            total += t;

                        one.Add(lineIndex1[i] + total);
                    }

                    onedblines.Add(one);
                    var mulITlines = new List<List<double[]>>();
                    foreach (var t in onedblines)
                    {
                        var lineindexList = new List<double[]>();
                        for (var j = 0; j < t.Count; j++)
                        {
                            if (j + 1 >= t.Count) break;
                            var l = (t[j + 1] - t[j]) * 2; //长度*2
                            var tempDouble = new double[l];
                            Array.Copy(legendPosition1, t[j] * 2, tempDouble, 0, l); //起始索引也要*2
                            lineindexList.Add(tempDouble);
                        }

                        mulITlines.Add(lineindexList);
                    }

                    //清除等值线，同时会清除标记点DrawLabel，注意
                    foreach (var oneleveList in mulITlines)
                        foreach (var t in oneleveList)
                        {
                            var gpsArray = new PointLatLng[t.Length / 2];
                            for (var k = 0; k < t.Length; k += 2)
                            {
                                var p = new PointLatLng
                                {
                                    Longitude = t[k],
                                    Latitude = t[k + 1]
                                };
                                gpsArray[k / 2] = p;
                            }
                        }

                    var bitmap = GetSkBitmapFromArray(rasterData,
                        _krig.StartLong,
                        _krig.StartLat,
                        _krig.CellSize,
                        _krig.Row,
                        _krig.Col,
                        out var leftTop,
                        out var rightBottom);
                    if (!double.IsNaN(maxlng) && !double.IsNaN(maxlat))
                    {
                        // using var stream = new MemoryStream();
                        // bitmap.Save(stream, ImageFormat.Png);
                        // // TODO: wudepeng 这里需要等算法来了以后保存到本地查看图片是否是对的然后再发送到前端进行测试
                        // // bitmap.Save("test.png");
                        // var buffer = stream.ToArray();
                        // stream.Dispose();
                        using MemoryStream memStream = new();
                        using SKManagedWStream wstream = new(memStream);
                        bitmap.Encode(wstream, SKEncodedImageFormat.Png, 100);
                        var buffer = memStream.ToArray();
                        // File.WriteAllBytes("SkiaSharp.png", buffer);
                        var data = new SsoaBitmapData
                        {
                            LeftTopLatitude = leftTop.Latitude,
                            LeftTopLongitude = leftTop.Longitude,
                            RightBottomLatitude = rightBottom.Latitude,
                            RightBottomLongitude = rightBottom.Longitude,
                            MaxLatitude = maxlat,
                            MaxLongitude = maxlng,
                            Data = buffer
                        };
                        if (!_isClear) UpdateSsoaDataEvent?.Invoke(null, data);
                    }

                    bitmap.Dispose();
                }
            }
            catch
            {
                // 忽略异常
            }

            Thread.Sleep(_constFsLocationTimeInterval);
        }
    }

    /// <summary>
    ///     计算多边形面积(平方米)
    /// </summary>
    /// <param name="points"></param>
    private double CalculateAreasLngLat(List<PointLatLng> points)
    {
        if (points.Count < 3) return 0;
        var coordinates = new Coordinate[points.Count + 1];
        for (var i = 0; i < points.Count; i++) coordinates[i] = new Coordinate(points[i].Longitude, points[i].Latitude);
        coordinates[points.Count] = new Coordinate(points[0].Longitude, points[0].Latitude);
        var transformLine = new LinearRing(coordinates);
        var transformPolygon = new Polygon(transformLine);
        return transformPolygon.Area;
    }

    /// <summary>
    ///     获取边界数据
    /// </summary>
    /// <param name="rasterData"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="rows"></param>
    /// <param name="cols"></param>
    /// <param name="maxLocation"></param>
    private void UpdateHeatLayer(double[,] rasterData, double maxX, double maxY, double minX, double minY, int rows,
        int cols, out PointLatLng maxLocation)
    {
        var maxValue = double.MinValue;
        var minValue = double.MaxValue;
        var cenX = 0d;
        var cenY = 0d;
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
            {
                if (double.IsNaN(rasterData[i, j])) continue;
                if (rasterData[i, j] > maxValue)
                {
                    maxValue = rasterData[i, j];
                    cenX = j * (maxX - minX) / cols + minX;
                    cenY = i * (maxY - minY) / rows + minY;
                }
                else if (rasterData[i, j] < minValue)
                {
                    minValue = rasterData[i, j];
                }
            }

        maxLocation = new PointLatLng(cenY, cenX);
    }

    /// <summary>
    ///     克里金结构体
    /// </summary>
    public struct Krig
    {
        /// <summary>
        ///     列数
        /// </summary>
        public int Col;

        /// <summary>
        ///     行数
        /// </summary>
        public int Row;

        /// <summary>
        ///     起始经度
        /// </summary>
        public double StartLong;

        /// <summary>
        ///     起始纬度
        /// </summary>
        public double StartLat;

        /// <summary>
        ///     单元格大小
        /// </summary>
        public double CellSize;
    }

    #region SkiaSharp绘图

    private SKColor[] GetSkColors(int colorsCount)
    {
        var colorBlend = new SKColor[] { new(8, 8, 255, 200), new(238, 255, 62, 200), new(255, 0, 0, 200) };
        var bitmap = new SKBitmap(5, colorsCount);
        var allColors = new SKColor[bitmap.Height];
        var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(0, colorsCount),
            colorBlend,
            null,
            SKShaderTileMode.Clamp);
        var paint = new SKPaint
        {
            Shader = shader
        };
        var canvas = new SKCanvas(bitmap);
        canvas.DrawPaint(paint);
        for (var i = 0; i < allColors.Length; i++) allColors[i] = bitmap.GetPixel(0, i);
        // Console.WriteLine($"Color [A={allColors[i].Alpha}, R={allColors[i].Red}, G={allColors[i].Green}, B={allColors[i].Blue}]");
        bitmap.Dispose();
        return allColors;
    }

    public SKBitmap GetSkBitmapFromArray(
        double[,] rasterData,
        double leftBottomX,
        double leftBottomY,
        double cellsize,
        int rows,
        int cols,
        out PointLatLng leftTop,
        out PointLatLng rightBottom)
    {
        var maxX = leftBottomX + (cols - 1) * cellsize + cellsize / 2.0 + cellsize / 2.0;
        var minX = leftBottomX - cellsize / 2.0 + cellsize / 2.0;
        var maxY = leftBottomY + (rows - 1) * cellsize + cellsize / 2.0 - cellsize / 2.0;
        var minY = leftBottomY - cellsize / 2.0 - cellsize / 2.0;
        leftTop = new PointLatLng(maxY, minX);
        rightBottom = new PointLatLng(minY, maxX);
        var maxValue = double.MinValue;
        var minValue = double.MaxValue;
        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
            {
                if (double.IsNaN(rasterData[i, j])) continue;
                if (rasterData[i, j] > maxValue)
                    maxValue = rasterData[i, j];
                else if (rasterData[i, j] < minValue) minValue = rasterData[i, j];
            }

        var result = new SKBitmap(rasterData.GetLength(1), rasterData.GetLength(0));
        for (var i = 0; i < result.Height; i++) //纵轴
            for (var j = 0; j < result.Width; j++) //横轴
            {
                var value = rasterData[i, j];
                if (double.IsNaN(value))
                    result.SetPixel(j, i, SKColor.Empty);
                else
                    try
                    {
                        var colorIndex = (int)((value - minValue) / (maxValue - minValue) * _allSkColors.Length);
                        if (colorIndex < 0)
                            colorIndex = 0;
                        else if (colorIndex > _allSkColors.Length - 1) colorIndex = _allSkColors.Length - 1;
                        var c = _allSkColors[colorIndex];
                        result.SetPixel(j, i, c);
                    }
                    catch
                    {
                        // ignored
                    }
            }

        return result;
    }

    #endregion
}

public class SsoaBitmapData
{
    /// <summary>
    ///     左上角经度
    /// </summary>
    public double LeftTopLongitude { get; set; }

    /// <summary>
    ///     左上角纬度
    /// </summary>
    public double LeftTopLatitude { get; set; }

    /// <summary>
    ///     右下角经度
    /// </summary>
    public double RightBottomLongitude { get; set; }

    /// <summary>
    ///     右下角纬度
    /// </summary>
    public double RightBottomLatitude { get; set; }

    /// <summary>
    ///     最大值经度
    /// </summary>
    public double MaxLongitude { get; set; }

    /// <summary>
    ///     最大值纬度
    /// </summary>
    public double MaxLatitude { get; set; }

    /// <summary>
    ///     图片数据
    /// </summary>
    public byte[] Data { get; set; }
}

/// <summary>
///     包装要绘制的场强跟踪数据
/// </summary>
public struct SFieldStrengthTrackData
{
    /// <summary>
    ///     经度
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    ///     纬度
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    ///     测量时间
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     场强值
    /// </summary>
    public float FieldStrength { get; set; }

    /// <summary>
    ///     绘制场强值所使用的颜色
    /// </summary>
    public SKColor Color { get; set; }
}

[Serializable]
public struct PointLatLng(double lat, double lng)
{
    public static readonly PointLatLng Empty = new();
    private double _lat = lat;
    private double _lng = lng;
    private bool _notEmpty = true;

    /// <summary>
    ///     returns true if coordinates wasn't assigned
    /// </summary>
    public bool IsEmpty => !_notEmpty;

    public double Latitude
    {
        get => _lat;
        set
        {
            _lat = value;
            _notEmpty = true;
        }
    }

    public double Longitude
    {
        get => _lng;
        set
        {
            _lng = value;
            _notEmpty = true;
        }
    }

    public static PointLatLng operator +(PointLatLng pt, SizeLatLng sz)
    {
        return Add(pt, sz);
    }

    public static PointLatLng operator -(PointLatLng pt, SizeLatLng sz)
    {
        return Subtract(pt, sz);
    }

    public static bool operator ==(PointLatLng left, PointLatLng right)
    {
        return Math.Abs(left.Longitude - right.Longitude) < 1e-9 && Math.Abs(left.Latitude - right.Latitude) < 1e-9;
    }

    public static bool operator !=(PointLatLng left, PointLatLng right)
    {
        return !(left == right);
    }

    public static PointLatLng Add(PointLatLng pt, SizeLatLng sz)
    {
        return new PointLatLng(pt.Latitude - sz.HeightLat, pt.Longitude + sz.WidthLng);
    }

    public static PointLatLng Subtract(PointLatLng pt, SizeLatLng sz)
    {
        return new PointLatLng(pt.Latitude + sz.HeightLat, pt.Longitude - sz.WidthLng);
    }

    public override bool Equals(object obj)
    {
        if (obj is not PointLatLng tf) return false;
        return Math.Abs(tf.Longitude - Longitude) < 1e-9 && Math.Abs(tf.Latitude - Latitude) < 1e-9 &&
               tf.GetType() == GetType();
    }

    public void Offset(PointLatLng pos)
    {
        Offset(pos.Latitude, pos.Longitude);
    }

    public void Offset(double lat, double lng)
    {
        Longitude += lng;
        Latitude -= lat;
    }

    public override int GetHashCode()
    {
        return Longitude.GetHashCode() ^ Latitude.GetHashCode();
    }

    public override string ToString()
    {
        return string.Format(CultureInfo.CurrentCulture, "{{Lat={0}, Lng={1}}}", Latitude, Longitude);
    }
}

public struct SizeLatLng(double heightLat, double widthLng)
{
    public static readonly SizeLatLng Empty;

    public SizeLatLng(SizeLatLng size) : this(size.HeightLat, size.WidthLng)
    {
    }

    public SizeLatLng(PointLatLng pt) : this(pt.Latitude, pt.Longitude)
    {
    }

    public static SizeLatLng operator +(SizeLatLng sz1, SizeLatLng sz2)
    {
        return Add(sz1, sz2);
    }

    public static SizeLatLng operator -(SizeLatLng sz1, SizeLatLng sz2)
    {
        return Subtract(sz1, sz2);
    }

    public static bool operator ==(SizeLatLng sz1, SizeLatLng sz2)
    {
        return Math.Abs(sz1.WidthLng - sz2.WidthLng) < 1e-9 && Math.Abs(sz1.HeightLat - sz2.HeightLat) < 1e-9;
    }

    public static bool operator !=(SizeLatLng sz1, SizeLatLng sz2)
    {
        return !(sz1 == sz2);
    }

    public static explicit operator PointLatLng(SizeLatLng size)
    {
        return new PointLatLng(size.HeightLat, size.WidthLng);
    }

    public bool IsEmpty => WidthLng == 0d && HeightLat == 0d;
    public double WidthLng { get; set; } = widthLng;
    public double HeightLat { get; set; } = heightLat;

    public static SizeLatLng Add(SizeLatLng sz1, SizeLatLng sz2)
    {
        return new SizeLatLng(sz1.HeightLat + sz2.HeightLat, sz1.WidthLng + sz2.WidthLng);
    }

    public static SizeLatLng Subtract(SizeLatLng sz1, SizeLatLng sz2)
    {
        return new SizeLatLng(sz1.HeightLat - sz2.HeightLat, sz1.WidthLng - sz2.WidthLng);
    }

    public override bool Equals(object obj)
    {
        if (obj is not SizeLatLng ef) return false;
        return Math.Abs(ef.WidthLng - WidthLng) < 1e-9 && Math.Abs(ef.HeightLat - HeightLat) < 1e-9 &&
               ef.GetType() == GetType();
    }

    public override int GetHashCode()
    {
        if (IsEmpty) return 0;
        return WidthLng.GetHashCode() ^ HeightLat.GetHashCode();
    }

    public PointLatLng ToPointLatLng()
    {
        return (PointLatLng)this;
    }

    public override string ToString()
    {
        return "{WidthLng=" + WidthLng.ToString(CultureInfo.CurrentCulture) + ", HeightLng=" +
               HeightLat.ToString(CultureInfo.CurrentCulture) + "}";
    }

    static SizeLatLng()
    {
        Empty = new SizeLatLng();
    }
}

/// <summary>
///     多边形凸包算法实现
/// </summary>
internal class ConvexAlgorithm
{
    private List<SKPoint> _nodes;
    public SKPoint[] SorNodes;

    public ConvexAlgorithm(List<SKPoint> points)
    {
        _nodes = points;
    }

    public ConvexAlgorithm()
    {
    }

    public Stack<SKPoint> SortedNodes { get; private set; }

    private double DistanceOfNodes(SKPoint p0, SKPoint p1)
    {
        if (p0.IsEmpty || p1.IsEmpty) return 0.0;
        return Math.Sqrt((p1.X - p0.X) * (p1.X - p0.X) + (p1.Y - p0.Y) * (p1.Y - p0.Y));
    }

    #region 计算多边形凸包

    /// <summary>
    ///     计算多边形凸包
    /// </summary>
    /// <param name="x">经度</param>
    /// <param name="y">纬度</param>
    /// <param name="inputlength">点的个数</param>
    /// <param name="edge">凸包边界坐标集合（x=i,y=i+1）</param>
    /// <param name="routputlength">凸包边界坐标个数*2</param>
    public void FindConvexHull(
        double[] x,
        double[] y,
        int inputlength,
        double[] edge,
        out int routputlength)
    {
        _nodes = [];
        for (var i = 0; i < inputlength; i++)
        {
            var p = new SKPoint((float)x[i], (float)y[i]);
            _nodes.Add(p);
        }

        GetNodesByAngle();
        var arr = SortedNodes.ToArray();
        routputlength = arr.Length * 2;
        for (var i = 0; i < arr.Length; i++)
        {
            edge[2 * i] = arr[i].X;
            edge[2 * i + 1] = arr[i].Y;
        }
    }

    public void GetNodesByAngle()
    {
        var listNode = new LinkedList<SKPoint>();
        var p0 = GetMinYPoint();
        var node = new LinkedListNode<SKPoint>(_nodes[0]);
        listNode.AddFirst(node);
        for (var i = 1; i < _nodes.Count; i++)
            if (node != null)
            {
                var direct = IsClockDirection(p0, node.Value, _nodes[i]);
                if (direct == 1)
                {
                    listNode.AddLast(_nodes[i]);
                    node = listNode.Last;
                    //node.Value = nodes[i];
                }
                else if (direct == -10)
                {
                    if (listNode.Last != null) listNode.Last.Value = _nodes[i];
                    //node = list_node.Last
                    //node.Value = nodes[i];
                }
                else if (direct == 10)
                {
                }
                else if (direct == -1)
                {
                    var temp = node.Previous;
                    while (temp != null && IsClockDirection(p0, temp.Value, _nodes[i]) == -1) temp = temp.Previous;
                    if (temp == null)
                    {
                        listNode.AddFirst(_nodes[i]);
                        continue;
                    }

                    if (IsClockDirection(p0, temp.Value, _nodes[i]) == -10)
                        temp.Value = _nodes[i];
                    else if (IsClockDirection(p0, temp.Value, _nodes[i]) != 10)
                        listNode.AddAfter(temp, _nodes[i]);
                }
            }

        SorNodes = listNode.ToArray();
        SortedNodes = new Stack<SKPoint>();
        SortedNodes.Push(p0);
        SortedNodes.Push(SorNodes[0]);
        SortedNodes.Push(SorNodes[1]);
        for (var i = 2; i < SorNodes.Length; i++)
        {
            var p2 = SorNodes[i];
            var p1 = SortedNodes.Pop();
            var p0Sec = SortedNodes.Pop();
            SortedNodes.Push(p0Sec);
            SortedNodes.Push(p1);
            if (IsClockDirection1(p0Sec, p1, p2) == 1)
            {
                SortedNodes.Push(p2);
                continue;
            }

            while (IsClockDirection1(p0Sec, p1, p2) != 1)
            {
                SortedNodes.Pop();
                p1 = SortedNodes.Pop();
                p0Sec = SortedNodes.Pop();
                SortedNodes.Push(p0Sec);
                SortedNodes.Push(p1);
            }

            SortedNodes.Push(p2);
        }
    }

    private int IsClockDirection1(SKPoint p0, SKPoint p1, SKPoint p2)
    {
        var p0P1 = new SKPoint(p1.X - p0.X, p1.Y - p0.Y);
        var p0P2 = new SKPoint(p2.X - p0.X, p2.Y - p0.Y);
        return p0P1.X * p0P2.Y - p0P2.X * p0P1.Y > 0 ? 1 : -1;
    }

    private SKPoint GetMinYPoint()
    {
        double miny = _nodes.Min(r => r.Y);
        var pminYs = _nodes.Where(r => Math.Abs(r.Y - miny) < 1e-9);
        var skPoints = pminYs as SKPoint[] ?? pminYs.ToArray();
        var ps = skPoints.ToArray();
        if (skPoints.Length > 1)
        {
            var succNode = skPoints.Single(r => Math.Abs(r.X - skPoints.Min(t => t.X)) < 1e-9);
            _nodes.Remove(succNode);
            return succNode;
        }

        _nodes.Remove(ps[0]);
        return ps[0];
    }

    private int IsClockDirection(SKPoint p0, SKPoint p1, SKPoint p2)
    {
        var p0P1 = new SKPoint(p1.X - p0.X, p1.Y - p0.Y);
        var p0P2 = new SKPoint(p2.X - p0.X, p2.Y - p0.Y);
        if (p0P1.X * p0P2.Y - p0P2.X * p0P1.Y != 0) return p0P1.X * p0P2.Y - p0P2.X * p0P1.Y > 0 ? 1 : -1;
        return DistanceOfNodes(p0, p1) > DistanceOfNodes(p0, p2) ? 10 : -10;
    }

    #endregion
}