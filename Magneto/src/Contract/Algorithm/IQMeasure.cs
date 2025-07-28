using System;

namespace Magneto.Contract.Algorithm;

public class IqMeasure
{
    // # RC filter with rolloff alpha=0.5
    private double[] GetRcFilter(double[] t, double beta)
    {
        // np.sinc(t)*np.cos(np.pi*beta*t)/(1-(2*beta*t)**2)
        var filter = new double[t.Length];
        for (var i = 0; i < t.Length; i++)
        {
            var sinc = Math.Sin(Math.PI * t[i]) / (Math.PI * t[i]);
            var num1 = Math.Cos(Math.PI * beta * t[i]);
            var num2 = 1 - Math.Pow(2 * beta * t[i], 2);
            filter[i] = sinc * num1 / num2;
            if (!double.IsFinite(filter[i])) filter[i] = filter[i - 1];
        }

        return filter;
    }

    private double[] GetRrcFilter(double[] t, double beta)
    {
        // (np.sin(np.pi*t*(1-beta))+4*beta*t*np.cos(np.pi*t*(1+beta)))/(np.pi*t*(1-(4*beta*t)**2))
        var filter = new double[t.Length];
        for (var i = 0; i < t.Length; i++)
        {
            var num1 = Math.Sin(Math.PI * t[i] * (1 - beta));
            var num2 = 4 * beta * t[i] * Math.Cos(Math.PI * t[i] * (1 + beta));
            var num3 = Math.PI * t[i] * (1 - Math.Pow(4 * beta * t[i], 2));
            filter[i] = (num1 + num2) / num3;
            if (!double.IsFinite(filter[i])) filter[i] = filter[i - 1];
        }

        return filter;
    }

    private double[] GetRectFilter(double[] t, double cycle)
    {
        var filter = new double[t.Length];
        for (var i = 0; i < t.Length; i++)
            if (Math.Abs(t[i] / cycle) < 0.5)
                filter[i] = 1;
            else
                filter[i] = 0;
        return filter;
    }

    private double[] GetTriangFilter(double[] t, double cycle)
    {
        var filter = new double[t.Length];
        for (var i = 0; i < t.Length; i++)
        {
            var abs = Math.Abs(t[i] / cycle);
            var sign = abs < 1 ? 1 : 0;
            filter[i] = (1 - abs) * sign;
        }

        return filter;
    }

    public double[] GetFilter(string name, double[] t, double cycle, double rolloff = 1)
    {
        if (t == null || t.Length == 0) return null;
        var tc = Array.ConvertAll(t, item => item / cycle);
        return name switch
        {
            "rect" => GetRectFilter(t, cycle),
            "triang" => GetTriangFilter(t, cycle),
            "rc" => GetRcFilter(tc, rolloff),
            "rrc" => GetRrcFilter(tc, rolloff),
            _ => null
        };
    }

    public void GetSignal(double[] d, out double[] t, out double[] xt)
    {
        // """Generate the transmit signal as sum(d[k]*g(t-kT))"""
        var cycle = 1d;
        var fs = 100;
        var start = -2 * cycle;
        var stop = (d.Length + 2) * cycle;
        var step = 1d / fs;
        var total = (int)((stop - start) / step + 1);
        t = new double[total];
        for (var i = 0; i < total; i++) t[i] = start + step * i;
        var array = new[] { 1e-8 };
        var g0 = GetFilter("rc", array, cycle, 0.5);
        xt = new double[t.Length];
        for (var k = 0; k < d.Length; k++)
        {
            var ts = new double[t.Length];
            for (var i = 0; i < t.Length; i++) ts[i] = t[i] - k * cycle;
            var xts = GetFilter("rc", ts, cycle, 0.5);
            for (var i = 0; i < xt.Length; i++) xt[i] += d[k] * xts[i] / g0[0];
        }
    }
}