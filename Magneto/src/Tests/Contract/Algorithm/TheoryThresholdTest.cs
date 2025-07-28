using System;
using System.Diagnostics;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.Algorithm;
using NUnit.Framework;

namespace Tests.Contract.Algorithm;

public class TheoryThresholdTest
{
    private TheoryThreshold _threshold;

    [Test(Description = "获取自动门限")]
    [Order(1)]
    [TestCase(88d, 108d, 25d)]
    [TestCase(20d, 100d, 25d)]
    [TestCase(20d, 3000d, 25d)]
    [TestCase(20d, 8000d, 25d)]
    public void CalThresholdTest(double start, double stop, double step)
    {
        var count = 100;
        Stopwatch watch = new();
        _threshold = new TheoryThreshold();
        var total = Utils.GetTotalCount(start, stop, step);
        var array = new float[total];
        var rd = new Random();
        for (var i = 0; i < array.Length; i++) array[i] = (float)(rd.NextDouble() * 100);
        watch.Start();
        for (var i = 0; i < count; i++) _threshold.CalThreshold(array, start, stop, (float)step);
        watch.Stop();
        var span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 次数:100,总点数:801,总耗时:24ms,平均耗时:0.24ms
        // 次数:100,总点数:3201,总耗时:63ms,平均耗时:0.63ms
        // 次数:100,总点数:119201,总耗时:2498ms,平均耗时:24.98ms
        // 次数:100,总点数:319201,总耗时:6751ms,平均耗时:67.51ms
    }

    [Test(Description = "测试数组初始化效率")]
    public void ArrayInitTest()
    {
        const int total = 1000000;
        const int count = 1000;
        var iniArray = Enumerable.Repeat<float>(-999, total).ToArray();
        var watch = new Stopwatch();
        // 1
        watch.Start();
        watch.Stop();
        var span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法1:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 2
        watch.Restart();
        for (var i = 0; i < count; i++)
        {
            var array = new float[total];
            for (var j = 0; j < total; j++) array[i] = -999;
        }

        watch.Stop();
        span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法2:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 3
        watch.Restart();
        for (var i = 0; i < count; i++)
        {
            var array = new float[total];
            iniArray.CopyTo(array, 0);
        }

        watch.Stop();
        span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法3:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 方法1:次数:1000,总点数:1000000,总耗时:1885ms,平均耗时:1.885ms
        // 方法2:次数:1000,总点数:1000000,总耗时:2338ms,平均耗时:2.338ms
        // 方法3:次数:1000,总点数:1000000,总耗时:1346ms,平均耗时:1.346ms
    }

    [Test(Description = "测试计算最大值效率")]
    public void MaxCalcTest()
    {
        const int total = 1000000;
        const int count = 1000;
        var max = new float[total];
        var array = new float[total];
        var rd = new Random();
        for (var i = 0; i < total; i++)
        {
            max[i] = rd.NextSingle();
            array[i] = rd.NextSingle();
        }

        var watch = new Stopwatch();
        // 1
        watch.Start();
        for (var i = 0; i < count; i++)
        for (var j = 0; j < total; j++)
            if (max[j] < array[j])
                max[j] = array[j];
        watch.Stop();
        var span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法1:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 2
        watch.Restart();
        for (var i = 0; i < count; i++)
        for (var j = 0; j < total; j++)
            max[j] = Math.Max(max[j], array[j]);
        watch.Stop();
        span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法2:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 方法1:次数:1000,总点数:1000000,总耗时:4491ms,平均耗时:4.491ms
        // 方法2:次数:1000,总点数:1000000,总耗时:12721ms,平均耗时:12.721ms
    }

    [Test(Description = "测试拷贝数组的效率")]
    public void CopyArrayDataTest()
    {
        const int total = 1000000;
        const int count = 1000;
        var array = new float[total];
        var rd = new Random();
        for (var i = 0; i < total; i++) array[i] = rd.NextSingle();
        var data = new float[total];
        var watch = new Stopwatch();
        // 1
        watch.Start();
        for (var i = 0; i < count; i++)
        for (var j = 0; j < total; j++)
            data[j] = array[j];
        watch.Stop();
        var span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法1:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 2
        watch.Restart();
        for (var i = 0; i < count; i++) Array.Copy(array, 0, data, 0, total);
        watch.Stop();
        span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法2:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 3
        watch.Restart();
        for (var i = 0; i < count; i++) Buffer.BlockCopy(array, 0, data, 0, total * sizeof(float));
        watch.Stop();
        span = (double)watch.ElapsedMilliseconds / count;
        Trace.WriteLine($"方法3:次数:{count},总点数:{total},总耗时:{watch.ElapsedMilliseconds}ms,平均耗时:{span}ms");
        // 方法1:次数:10000,总点数:1000000,总耗时:31546ms,平均耗时:3.1546ms
        // 方法2:次数:10000,总点数:1000000,总耗时:3146ms,平均耗时:0.3146ms
        // 方法3:次数:10000,总点数:1000000,总耗时:2867ms,平均耗时:0.2867ms
    }
}