using System;
using System.Numerics;
using Magneto.Contract.Algorithm;

namespace Magneto.Device.ESMD;

internal static class Utilities
{
    public static int Log2N(int value)
    {
        var n = 0;
        while ((value >>= 1) > 0) n++;
        return n;
    }

    public static float Window(ref float[] data, WindowType windowType)
    {
        var pi = (float)Math.PI;
        var coe = 0.0f;
        var length = data.Length;
        switch (windowType)
        {
            case WindowType.RectWin:
                for (var i = 0; i < length; ++i) data[i] = 1;
                coe = -1;
                break;
            case WindowType.HanningWin:
            {
                var pi2L = pi * 2.0f / length;
                for (var i = 0; i < length; ++i) data[i] = (float)(1 - Math.Cos(pi2L * i)) / 2;
            }
                coe = 5.08f;
                break;
            case WindowType.BlackManWin:
            {
                var pi2L = pi * 2.0f / length;
                var pi4L = pi * 4.0f / length;
                for (var i = 0; i < length; ++i)
                    data[i] = (float)(0.42f - 0.5f * Math.Cos(pi2L * i) + 0.08f * Math.Cos(pi4L * i));
            }
                coe = 6.60f;
                break;
        }

        return coe;
    }

    public static Complex[] GetWindowData(float[] data, float[] windowValue, int length)
    {
        if (data == null || data.Length < length * 2
                         || windowValue == null || windowValue.Length != length)
            return null;
        var outputData = new Complex[length];
        for (var index = 0; index < length; ++index)
        {
            outputData[index] = new Complex(data[2 * index], data[2 * index + 1]);
            outputData[index] *= windowValue[index];
        }

        return outputData;
    }

    public static void Fft(ref Complex[] x)
    {
        var log2N = Log2N(x.Length);
        int i;
        int k;
        int l;
        int le;
        int le1;
        int ip;
        float ain;
        Complex t;
        Complex u;
        Complex w;
        var n = 1 << log2N;
        var nv2 = n >> 1;
        var nm1 = n - 1;
        var j = 0;
        for (i = 0; i < nm1; i++)
        {
            if (i < j)
            {
                t = x[j];
                x[j] = x[i];
                x[i] = t;
            }

            k = nv2;
            while (k <= j)
            {
                j -= k;
                k >>= 1;
            }

            j += k;
        }

        var pi = (float)Math.PI;
        for (l = 1; l <= log2N; l++)
        {
            le = 1 << l;
            le1 = le >> 1;
            ain = pi / le1;
            u = new Complex(1, 0);
            w = new Complex(Math.Cos(ain), -Math.Sin(ain));
            for (j = 0; j < le1; j++)
            {
                for (i = j; i < n; i += le)
                {
                    ip = i + le1;
                    t = x[ip] * u;
                    x[ip] = x[i] - t;
                    x[i] += t;
                }

                u *= w;
            }
        }
    }

    public static float GetLevel(float[] data)
    {
        if (data == null || data.Length % 2 != 0) return 0.0f;
        var epsilon = 1.0E-7d;
        var length = data.Length / 2;
        var sum = 0.0d;
        var increment = 0;
        for (var index = 0; index < length; ++index)
        {
            var real = data[2 * index];
            var image = data[2 * index + 1];
            if (Math.Abs(real - 0.0f) > epsilon || Math.Abs(image - 0.0f) > epsilon)
            {
                sum += Math.Log10(real * real + image * image);
                increment++;
            }
        }

        if (increment == 0) return 0.0f;
        return (float)(10 * sum / increment);
    }
}