using System;
using System.Collections.Generic;
using SkiaSharp;

// using System.Drawing;
namespace Magneto.Contract.Algorithm.Interpolation;

internal class DemData
{
    public DemData(double[,] data)
    {
        DemCellSize = 10;
        Row = data.GetLength(0);
        Column = data.GetLength(1);
        AltitudeData = data;
        AnalyseData();
    }

    /// <summary>
    ///     高程数据
    /// </summary>
    public double[,] AltitudeData { get; }

    /// <summary>
    ///     行
    /// </summary>
    public int Row { get; }

    /// <summary>
    ///     列
    /// </summary>
    public int Column { get; }

    /// <summary>
    ///     DEM图像绘制的格网边长
    /// </summary>
    public int DemCellSize { get; }

    /// <summary>
    ///     高程值最高点
    /// </summary>
    public double MaxValue { private set; get; }

    /// <summary>
    ///     高程值最低点
    /// </summary>
    public double MinValue { private set; get; }

    // /// <summary>
    // /// 判断点是否在边界内
    // /// </summary>
    // /// <param name="x"></param>
    // /// <param name="y"></param>
    // public bool IsPointInEdge(float x, float y)
    // {
    //     using var myGraphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
    //     using var myRegion = new Region();
    //     myGraphicsPath.Reset();
    //     if (_edge == null || _edge.Length == 0)
    //     {
    //         return true;
    //     }
    //     myGraphicsPath.AddPolygon(_edge);
    //     myRegion.MakeEmpty();
    //     myRegion.Union(myGraphicsPath);
    //     //返回判断点是否在多边形里
    //     return myRegion.IsVisible(new SKPoint(x, y));
    // }
    // /// <summary>
    // /// 根据DEM高程值绘制图像
    // /// </summary>
    // /// <returns>DEM图像</returns>
    // public Bitmap DEMBitmap()
    // {
    //     var bitmap = new Bitmap(this.Column * DEMCellSize, this.Row * DEMCellSize);
    //     Graphics graphics = Graphics.FromImage(bitmap);
    //     graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
    //     using var myGraphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
    //     using var myRegion = new Region();
    //     myGraphicsPath.Reset();
    //     myGraphicsPath.AddPolygon(_edge);
    //     myRegion.MakeEmpty();
    //     myRegion.Union(myGraphicsPath);
    //     double maxData = 0;
    //     int maxX = 0;
    //     int maxY = 0;
    //     for (int y = 0; y < this.Row; y++)
    //     {
    //         for (int x = 0; x < this.Column; x++)
    //         {
    //             if (!myRegion.IsVisible(new SKPoint(y, x)))
    //             {
    //                 continue;
    //             }
    //             if (maxData < AltitudeData[y, x])
    //             {
    //                 maxData = AltitudeData[y, x];
    //                 maxX = x;
    //                 maxY = y;
    //             }
    //             var colorIndex = (int)((AltitudeData[y, x] - this.MinValue) / (MaxValue - MinValue) * _colors.Length);
    //             if (colorIndex < 0)
    //             {
    //                 colorIndex = 0;
    //             }
    //             else if (colorIndex > _colors.Length - 1)
    //             {
    //                 colorIndex = _colors.Length - 1;
    //             }
    //             Color color = _colors[colorIndex];
    //             graphics.FillRectangle(new SolidBrush(color), new Rectangle(x * DEMCellSize, y * DEMCellSize, DEMCellSize, DEMCellSize));
    //         }
    //     }
    //     myRegion.Dispose();
    //     myGraphicsPath.Dispose();
    //     // 绘制最大值点
    //     // var maxPen = new Pen(Color.Blue, DEMCellSize / 2f);
    //     // graphics.DrawEllipse(maxPen, new Rectangle((maxX * DEMCellSize) - (4 * DEMCellSize), (maxY * DEMCellSize) - (DEMCellSize * 4), DEMCellSize * 8, DEMCellSize * 8));
    //     // graphics.DrawEllipse(maxPen, new Rectangle((maxX * DEMCellSize) - (2 * DEMCellSize), (maxY * DEMCellSize) - (DEMCellSize * 2), DEMCellSize * 4, DEMCellSize * 4));
    //     // graphics.DrawLine(maxPen, (maxX - 8) * DEMCellSize, maxY * DEMCellSize, (maxX + 8) * DEMCellSize, maxY * DEMCellSize);
    //     // graphics.DrawLine(maxPen, maxX * DEMCellSize, (maxY - 8) * DEMCellSize, maxX * DEMCellSize, (maxY + 8) * DEMCellSize);
    //     var pen = new Pen(Color.Black, DEMCellSize / 5f);
    //     // for (int i = 0; i < _edge.Length - 1; i++)
    //     // {
    //     //     graphics.DrawLine(pen, _edge[i].Y * DEMCellSize, _edge[i].X * DEMCellSize, _edge[i + 1].Y * DEMCellSize, _edge[i + 1].X * DEMCellSize);
    //     // }
    //     bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);//旋转图片
    //     // 绘制等高线
    //     var marchingSquares = new MarchingSquares(AltitudeData, 15, MinValue, MaxValue, _edge);
    //     int length = DEMCellSize;
    //     foreach (var lines in marchingSquares.ContourData)
    //     {
    //         foreach (var line in lines.Value)
    //         {
    //             graphics.DrawLine(pen,
    //                        line.startPoint.Y * length,
    //                        bitmap.Height - (line.startPoint.X * length),
    //                        line.endPoint.Y * length,
    //                        bitmap.Height - (line.endPoint.X * length));
    //         }
    //     }
    //     graphics.Dispose();
    //     return bitmap;
    // }
    /// <summary>
    ///     在DEM中随机取点，默认取100个
    /// </summary>
    /// <param name="count">所取点的个数</param>
    /// <returns>随机点的合集</returns>
    public List<AltitudePoint> GetRandomPoints(int count = 100)
    {
        var list = new List<AltitudePoint>();
        var random = new Random();
        for (var n = 0; n < count; n++)
        {
            var randomNumber = random.Next(Row * Column);
            var x = randomNumber / Column;
            var y = randomNumber - x * Column;
            list.Add(new AltitudePoint(x, y, AltitudeData[x, y]));
        }

        list.Sort((p1, p2) => p1.X * Column + p1.Y > p2.X * Column + p2.Y ? 1 : -1);
        return list;
    }

    /// <summary>
    ///     基础值初始化,最大值，最小值
    /// </summary>
    private void AnalyseData()
    {
        MinValue = AltitudeData[0, 0];
        MaxValue = AltitudeData[0, 0];
        for (var x = 0; x < Row; x++)
        for (var y = 0; y < Column; y++)
        {
            MinValue = MinValue < AltitudeData[x, y] ? MinValue : AltitudeData[x, y];
            MaxValue = MaxValue > AltitudeData[x, y] ? MaxValue : AltitudeData[x, y];
        }
    }
}

internal class MarchingSquares
{
    public MarchingSquares(double interval, double min, double max)
    {
        ContourData = new Dictionary<double, List<Line>>();
        for (double value = 0; value < max; value += interval)
            if (value > min)
                ContourData.Add(value, new List<Line>());
        // DoMSAlgorithm();
    }

    public Dictionary<double, List<Line>> ContourData { get; }

    // private void DoMSAlgorithm()
    // {
    //     const int length = 1;
    //     using var myGraphicsPath = new System.Drawing.Drawing2D.GraphicsPath();
    //     using var myRegion = new Region();
    //     myGraphicsPath.Reset();
    //     myGraphicsPath.AddPolygon(_edge);
    //     myRegion.MakeEmpty();
    //     myRegion.Union(myGraphicsPath);
    //     for (int n = 0; n < _initialData.GetLength(1) - length; n++)
    //     {
    //         for (int m = 0; m < _initialData.GetLength(0) - length; m++)
    //         {
    //             if (!myRegion.IsVisible(m, n))
    //             {
    //                 continue;
    //             }
    //             foreach (var contour in ContourData)
    //             {
    //                 switch (GetBinaryIndex(contour.Key, m, n))
    //                 {
    //                     #region 第一,二行
    //                     case "0001":
    //                     case "1110":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m, n, m, n + length),
    //                             CalExactPosition(contour.Key, m + length, n + length, m, n + length)));
    //                         break;
    //                     case "0010":
    //                     case "1101":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m + length, n, m + length, n + length),
    //                             CalExactPosition(contour.Key, m + length, n + length, m, n + length)));
    //                         break;
    //                     case "0100":
    //                     case "1011":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m, n, m + length, n),
    //                             CalExactPosition(contour.Key, m + length, n + length, m + length, n)));
    //                         break;
    //                     case "1000":
    //                     case "0111":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m, n, m + length, n),
    //                             CalExactPosition(contour.Key, m, n, m, n + length)));
    //                         break;
    //                     #endregion
    //                     #region 第三行
    //                     case "0011":
    //                     case "1100":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m, n, m, n + length),
    //                             CalExactPosition(contour.Key, m + length, n, m + length, n + length)));
    //                         break;
    //                     case "0110":
    //                     case "1001":
    //                         contour.Value.Add(
    //                             new Line(CalExactPosition(contour.Key, m, n, m + length, n),
    //                             CalExactPosition(contour.Key, m + length, n + length, m, n + length)));
    //                         break;
    //                         #endregion
    //                         #region 特殊情况，两条线
    //                         //case "1010":
    //                         //	if (initialData[m + 1, n + 1] >= contour.Key)
    //                         //	{
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m, n, m + length, n),
    //                         //			CalExactPosition(contour.Key, m + length, n + length, m + length, n)));
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m + length, n + length, m, n + length),
    //                         //			CalExactPosition(contour.Key, m, n + length, m, n)));
    //                         //	}
    //                         //	else
    //                         //	{
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m, n, m, n + length),
    //                         //			CalExactPosition(contour.Key, m, n, m + length, n)));
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m + length, n, m + length, n + length),
    //                         //			CalExactPosition(contour.Key, m + length, n + length, m, n + length)));
    //                         //	}
    //                         //	break;
    //                         //case "0101":
    //                         //	if (initialData[m + 1, n + 1] < contour.Key)
    //                         //	{
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m, n, m + length, n),
    //                         //			CalExactPosition(contour.Key, m + length, n + length, m + length, n)));
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m + length, n + length, m, n + length),
    //                         //			CalExactPosition(contour.Key, m, n + length, m, n)));
    //                         //	}
    //                         //	else
    //                         //	{
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m, n, m, n + 1),
    //                         //			CalExactPosition(contour.Key, m, n, m + 1, n)));
    //                         //		contour.Value.Add(
    //                         //			new Line(CalExactPosition(contour.Key, m + 1, n, m + 1, n + 1),
    //                         //			CalExactPosition(contour.Key, m + 1, n + 1, m, n + 1)));
    //                         //	}
    //                         //	break;
    //                         #endregion
    //                 }
    //             }
    //         }
    //     }
    //     myRegion.Dispose();
    //     myGraphicsPath.Dispose();
    // }
    //private string GetBinaryIndex(double value, int x, int y)
    //{
    //    string result = null;
    //    result += _initialData[x, y] >= value ? "1" : "0";
    //    result += _initialData[x + 1, y] >= value ? "1" : "0";
    //    result += _initialData[x + 1, y + 1] >= value ? "1" : "0";
    //    result += _initialData[x, y + 1] >= value ? "1" : "0";
    //    return result;
    //}

    //private SKPoint CalExactPosition(double value, int startX, int startY, int endX, int endY)
    //{
    //    var radio = (value - _initialData[startX, startY]) / (_initialData[endX, endY] - _initialData[startX, startY]);
    //    return new SKPoint((float)(startX + radio * (endX - startX)), (float)(startY + radio * (endY - startY)));
    //}
}

internal class Line(SKPoint startPoint, SKPoint endPoint)
{
    public SKPoint EndPoint = endPoint;
    public SKPoint StartPoint = startPoint;
}