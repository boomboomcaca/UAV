using Magneto.Contract.Algorithm;
using NUnit.Framework;

namespace Tests.Contract.Algorithm;

public class IqMeasureTest
{
    [Test]
    public void GetFilterTest()
    {
        var meas = new IqMeasure();
        var cycle = 1d;
        var fs = 100;
        var start = -3 * cycle;
        var stop = 3 * cycle;
        var step = 1d / fs;
        var total = (int)((stop - start) / step + 1);
        var t = new double[total];
        for (var i = 0; i < total; i++) t[i] = start + step * i;
        var filterRc = meas.GetFilter("rc", t, cycle, 0.5);
        var filterRrc = meas.GetFilter("rrc", t, cycle, 0.5);
        var filterRect = meas.GetFilter("rect", t, cycle);
        var filterTriang = meas.GetFilter("triang", t, cycle);
        string.Join(",", t);
        string.Join(",", filterRc);
        string.Join(",", filterRrc);
        string.Join(",", filterRect);
        string.Join(",", filterTriang);
    }

    [Test]
    public void GetSignalTest()
    {
        var cycle = 1d;
        var meas = new IqMeasure();
        var d = new double[] { -1, 1, 1, -1, -1, 1, 1, -1, 1, -1, -1, 1, 1, -1, 1, -1 };
        meas.GetSignal(d, out var t, out var xt);
        string.Join(",", t);
        string.Join(",", xt);
        var strSum = new string[d.Length];
        for (var k = 0; k < d.Length; k++)
        {
            var ts = new double[t.Length];
            for (var i = 0; i < t.Length; i++) ts[i] = t[i] - k * cycle;
            var xts = meas.GetFilter("rc", ts, cycle, 0.5);
            for (var i = 0; i < xt.Length; i++) xts[i] = d[k] * xts[i];
            strSum[k] = string.Join(",", xts);
        }
    }
}