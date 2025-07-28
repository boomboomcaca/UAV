using System;
using Magneto.Contract.Algorithm;
using NUnit.Framework;

namespace Tests.Contract.Algorithm;

public class AutoThresholdTest
{
    [Test(Description = "获取自动门限")]
    [Order(1)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void GetThresholdTest(int length)
    {
        try
        {
            var rd = new Random(DateTime.Now.Millisecond);
            float[] data = null;
            if (length >= 0)
            {
                data = new float[length];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (float)rd.NextDouble() * 20;
                    if (i < 30)
                        data[i] -= 10;
                    else if (i < 60)
                        data[i] += 20;
                    else
                        data[i] += 10;
                }
            }

            var threshold = new AutoThreshold();
            threshold.GetThreshold(data);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test(Description = "提取信号电平")]
    [Order(1)]
    [TestCase(-1)]
    [TestCase(0)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void GetSinalTest(int length)
    {
        try
        {
            var rd = new Random(DateTime.Now.Millisecond);
            float[] data = null;
            if (length >= 0)
            {
                data = new float[length];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (float)rd.NextDouble() * 20;
                    if (i < 30)
                        data[i] -= 10;
                    else if (i < 60)
                        data[i] += 20;
                    else
                        data[i] += 10;
                }
            }

            var threshold = new AutoThreshold();
            threshold.GetSingal(data);
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test(Description = "依据长期统计的信号电平，信号电平最大值，提取门限值")]
    [Order(1)]
    [TestCase(-1, -1, 20)]
    [TestCase(0, 0, 20)]
    [TestCase(0, -1, 20)]
    [TestCase(-1, 0, 20)]
    [TestCase(100, 50, 20)]
    [TestCase(50, 100, 20)]
    [TestCase(100, 100, 20)]
    public void GetThresholdTest(int dataLen, int maxLen, int count)
    {
        try
        {
            var rd = new Random(DateTime.Now.Millisecond);
            float[] data = null;
            float[] max = null;
            if (dataLen >= 0)
            {
                data = new float[dataLen];
                for (var i = 0; i < data.Length; i++)
                {
                    data[i] = (float)rd.NextDouble() * 20;
                    if (i < 30)
                        data[i] -= 10;
                    else if (i < 60)
                        data[i] += 20;
                    else
                        data[i] += 10;
                }
            }

            if (maxLen > 0)
            {
                max = new float[maxLen];
                for (var j = 0; j < count; j++)
                for (var i = 0; i < max.Length; i++)
                {
                    var num = (float)rd.NextDouble() * 20;
                    if (i < 30)
                        num -= 10;
                    else if (i < 60)
                        num += 20;
                    else
                        num += 10;
                    max[i] = Math.Max(num, max[i]);
                }
            }

            var threshold = new AutoThreshold();
            var tr = threshold.GetThreshold(data, max);
            var res = tr == null ? "null" : tr.Length.ToString();
            Console.WriteLine($"DataLen:{dataLen},MaxLen{maxLen},Result:{res}");
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }
}