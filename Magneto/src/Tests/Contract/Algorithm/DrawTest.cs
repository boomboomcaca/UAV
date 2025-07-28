using System;
using System.IO;
using Magneto.Contract.Algorithm;
using NUnit.Framework;
using SkiaSharp;

// using System.Drawing;
// using System.Drawing.Drawing2D;
// using System.Drawing.Imaging;

namespace Tests.Contract.Algorithm;

public class DrawTest
{
    // private Color[] _allColors;
    private SKColor[] _allSkColors;
    private double _cellsize;
    private int _cols;
    private double _leftBottomX;
    private double _leftBottomY;
    private PointLatLng _leftTop;
    private double[,] _rasterData;
    private PointLatLng _rightBottom;
    private int _rows;

    [OneTimeSetUp]
    public void Setup()
    {
        _rows = 100;
        _cols = 100;
        _leftBottomX = 0;
        _leftBottomY = 0;
        _leftTop = new PointLatLng(104, 30);
        _rightBottom = new PointLatLng(105, 29);
        _cellsize = 1 / 1000d;
        _rasterData = new double[1000, 1000];
        var num = 255;
        for (var i = 0; i < 1000; i++)
        for (var j = 0; j < 1000; j++)
        {
            _rasterData[i, j] = num;
            num--;
            if (num == 0) num = 255;
        }

        // GetColorsTest(100);
        GetSkColorsTest(100);
    }

    // [Test]
    // [TestCase(100)]
    // public void GetColorsTest(int colorsCount)
    // {
    //     var colorBlend = new Color[] { Color.FromArgb(200, 8, 8, 255), Color.FromArgb(200, 238, 255, 62), Color.FromArgb(200, 255, 0, 0) };
    //     var bit = new Bitmap(5, colorsCount);
    //     var allColors = new Color[bit.Height];
    //     using (var g = Graphics.FromImage(bit))
    //     {
    //         var rect = new Rectangle(0, 0, bit.Width, bit.Height);
    //         using var gradbrush = new LinearGradientBrush(rect, colorBlend[0], colorBlend[2], LinearGradientMode.Vertical);
    //         var blend = new System.Drawing.Drawing2D.ColorBlend
    //         {
    //             Colors = colorBlend
    //         };
    //         float posGap = 1.0f / (colorBlend.Length - 1);
    //         var colorPos = new float[colorBlend.Length];
    //         for (int i = 0; i < colorPos.Length - 1; i++)
    //         {
    //             colorPos[i] = i * posGap;
    //         }
    //         colorPos[^1] = 1;
    //         blend.Positions = colorPos;
    //         gradbrush.InterpolationColors = blend;
    //         g.FillRectangle(gradbrush, rect);
    //         for (int i = 0; i < allColors.Length; i++)
    //         {
    //             allColors[i] = bit.GetPixel(0, i);
    //             Console.WriteLine($"{allColors[i]}");
    //         }
    //         gradbrush.Dispose();
    //         _allColors = allColors;
    //     }
    //     var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test");
    //     if (!Directory.Exists(dir))
    //     {
    //         Directory.CreateDirectory(dir);
    //     }
    //     bit.Save(Path.Combine(dir, "test.bmp"));
    //     bit.Dispose();
    // }
    // [Test]
    // public void GetBitmapFromArray()
    // {
    //     // if (_allColors == null)
    //     // {
    //     //     _allColors = GetColors(100);
    //     // }
    //     var maxX = _leftBottomX + ((_cols - 1) * _cellsize) + (_cellsize / 2.0) + (_cellsize / 2.0);
    //     var minX = _leftBottomX - (_cellsize / 2.0) + (_cellsize / 2.0);
    //     var maxY = _leftBottomY + ((_rows - 1) * _cellsize) + (_cellsize / 2.0) - (_cellsize / 2.0);
    //     var minY = _leftBottomY - (_cellsize / 2.0) - (_cellsize / 2.0);
    //     _leftTop = new PointLatLng(maxY, minX);
    //     _rightBottom = new PointLatLng(minY, maxX);
    //     var maxValue = double.MinValue;
    //     var minValue = double.MaxValue;
    //     for (int i = 0; i < _rows; i++)
    //     {
    //         for (int j = 0; j < _cols; j++)
    //         {
    //             if (double.IsNaN(_rasterData[i, j]))
    //             {
    //                 continue;
    //             }
    //             if (_rasterData[i, j] > maxValue)
    //             {
    //                 maxValue = _rasterData[i, j];
    //             }
    //             else if (_rasterData[i, j] < minValue)
    //             {
    //                 minValue = _rasterData[i, j];
    //             }
    //         }
    //     }
    //     var result = new Bitmap(_rasterData.GetLength(1), _rasterData.GetLength(0));
    //     var rect = new Rectangle(0, 0, result.Width, result.Height);
    //     BitmapData bmpData =
    //         result.LockBits(rect, ImageLockMode.ReadWrite,
    //             result.PixelFormat);
    //     // Get the address of the first line.
    //     IntPtr ptr = bmpData.Scan0;
    //     // Declare an array to hold the bytes of the bitmap.
    //     int bytes = Math.Abs(bmpData.Stride) * result.Height;
    //     var rgbValues = new byte[bytes];
    //     // Copy the RGB values into the array.
    //     Marshal.Copy(ptr, rgbValues, 0, bytes);
    //     for (int i = 0; i < result.Height; i++) //绾佃酱
    //     {
    //         for (int j = 0; j < result.Width; j++) //妯酱
    //         {
    //             int index = ((i * result.Width) + j) * 4;
    //             double value = _rasterData[i, j];
    //             if (double.IsNaN(value))
    //             {
    //                 rgbValues[index + 3] = 0;
    //             }
    //             else
    //             {
    //                 try
    //                 {
    //                     var colorIndex = (int)((value - minValue) / (maxValue - minValue) * _allColors.Length);
    //                     if (colorIndex < 0)
    //                     {
    //                         colorIndex = 0;
    //                     }
    //                     else if (colorIndex > _allColors.Length - 1)
    //                     {
    //                         colorIndex = _allColors.Length - 1;
    //                     }
    //                     Color c = _allColors[colorIndex];
    //                     rgbValues[index] = c.B;
    //                     rgbValues[index + 1] = c.G;
    //                     rgbValues[index + 2] = c.R;
    //                     rgbValues[index + 3] = 120;
    //                 }
    //                 catch
    //                 {
    //                 }
    //             }
    //         }
    //     }
    //     // Copy the RGB values back to the bitmap
    //     Marshal.Copy(rgbValues, 0, ptr, bytes);
    //     // Unlock the bits.
    //     result.UnlockBits(bmpData);
    //     var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test");
    //     if (!Directory.Exists(dir))
    //     {
    //         Directory.CreateDirectory(dir);
    //     }
    //     result.Save(Path.Combine(dir, "System.Drawing.Common.bmp"));
    // }
    [Test]
    [TestCase(100)]
    public void GetSkColorsTest(int colorsCount)
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
        for (var i = 0; i < allColors.Length; i++)
        {
            allColors[i] = bitmap.GetPixel(0, i);
            Console.WriteLine(
                $"Color [A={allColors[i].Alpha}, R={allColors[i].Red}, G={allColors[i].Green}, B={allColors[i].Blue}]");
        }

        _allSkColors = allColors;
        using (var memStream = new MemoryStream())
        using (var wstream = new SKManagedWStream(memStream))
        {
            bitmap.Encode(wstream, SKEncodedImageFormat.Png, 100);
            var data = memStream.ToArray();
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "testSK.png"), data);
        }
    }

    [Test]
    public void GetSkBitmapFromArray()
    {
        // if (_allColors == null)
        // {
        //     _allColors = GetColors(100);
        // }
        var maxX = _leftBottomX + (_cols - 1) * _cellsize + _cellsize / 2.0 + _cellsize / 2.0;
        var minX = _leftBottomX - _cellsize / 2.0 + _cellsize / 2.0;
        var maxY = _leftBottomY + (_rows - 1) * _cellsize + _cellsize / 2.0 - _cellsize / 2.0;
        var minY = _leftBottomY - _cellsize / 2.0 - _cellsize / 2.0;
        _leftTop = new PointLatLng(maxY, minX);
        _rightBottom = new PointLatLng(minY, maxX);
        var maxValue = double.MinValue;
        var minValue = double.MaxValue;
        for (var i = 0; i < _rows; i++)
        for (var j = 0; j < _cols; j++)
        {
            if (double.IsNaN(_rasterData[i, j])) continue;
            if (_rasterData[i, j] > maxValue)
                maxValue = _rasterData[i, j];
            else if (_rasterData[i, j] < minValue) minValue = _rasterData[i, j];
        }

        var result = new SKBitmap(_rasterData.GetLength(1), _rasterData.GetLength(0));
        for (var i = 0; i < result.Height; i++) //绾佃酱
        for (var j = 0; j < result.Width; j++) //妯酱
        {
            var value = _rasterData[i, j];
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
                }
        }

        using (var memStream = new MemoryStream())
        using (var wstream = new SKManagedWStream(memStream))
        {
            result.Encode(wstream, SKEncodedImageFormat.Png, 100);
            var data = memStream.ToArray();
            var dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(Path.Combine(dir, "SkiaSharp.png"), data);
        }
    }
}