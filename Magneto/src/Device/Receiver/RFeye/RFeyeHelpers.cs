using System;
using System.Linq;
using System.Numerics;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeye;

public partial class RFeye
{
    #region 其它辅助函数

    private double GetStep(int resBw)
    {
        var pos = Array.IndexOf(Consts.ArrResBw, resBw);
        if (pos >= 0) return Consts.ArrStep[pos];
        return 0;
    }

    private int GetRbwHz(FeatureType featureType, double step)
    {
        var approxStep = GetStepApproxValue(featureType, step);
        return GetResBw(featureType, approxStep);
    }

    private int GetResBw(FeatureType featureType, double specSpan)
    {
        try
        {
            if (featureType is FeatureType.FFM or FeatureType.MScan or FeatureType.MScne or FeatureType.FScne)
            {
                var pos = Array.IndexOf(Consts.ArrSpecSpan, specSpan);
                return Consts.ArrResBwFixFq[pos];
            }

            if (featureType == FeatureType.SCAN)
            {
                var pos = Array.IndexOf(Consts.ArrStep, specSpan);
                return Consts.ArrResBw[pos];
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private double GetStepApproxValue(FeatureType featureType, double srcValue)
    {
        double[] arrValue = null;
        if (featureType == FeatureType.SCAN)
            arrValue = Consts.ArrStep.OrderBy(p => p).ToArray();
        else //"FSCNE"
            arrValue = Consts.ArrSpecSpan.OrderBy(p => p).ToArray();
        var pos = 0;
        var deviation = Math.Abs(srcValue - arrValue[0]);
        for (var i = 1; i < arrValue.Length; ++i)
        {
            var tempDeviation = Math.Abs(srcValue - arrValue[i]);
            if (tempDeviation < deviation)
            {
                deviation = tempDeviation;
                pos = i;
            }
            else
            {
                break;
            }
        }

        return arrValue[pos];
    }

    /// <summary>
    ///     获取两个点的距离，单位米
    /// </summary>
    /// <param name="lantitude1"></param>
    /// <param name="longitude1"></param>
    /// <param name="lantitude2"></param>
    /// <param name="longitude2"></param>
    private double GetDistance(double lantitude1, double longitude1, double lantitude2, double longitude2)
    {
        var dLat1InRad = lantitude1 * (Math.PI / 180);
        var dLong1InRad = longitude1 * (Math.PI / 180);
        var dLat2InRad = lantitude2 * (Math.PI / 180);
        var dLong2InRad = longitude2 * (Math.PI / 180);
        var dLongitude = dLong2InRad - dLong1InRad;
        var dLatitude = dLat2InRad - dLat1InRad;
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Consts.EarthAxis * c * 1000;
    }

    /// <summary>
    ///     在实时IQ数据采集时，中心频率会分为两部份进行设置，freqMhz + offsetHz，(offsetHz范围为(4.5Mhz, 5.5Mhz])
    ///     例如101.7Mhz = 97Mhz + 470000Hz
    /// </summary>
    /// <param name="frequency">频率</param>
    /// <param name="freqMHz">Center frequency</param>
    /// <param name="offsetHz">DDS offset frequency</param>
    private void GetFreqMHzAndOffsetHz(double frequency, out int freqMHz, out int offsetHz)
    {
        //TODO:保证不损失精度，否则如99.6MHz会成为99+599999
        var tempFreq = new decimal(frequency);
        var tempFreqMHz = (int)tempFreq;
        var tempOffsetHz = (int)((tempFreq - tempFreqMHz) * 1000000);
        //int tempFreqMHz = (int)frequency;
        //int tempOffsetHz = (int)((frequency - tempFreqMHz) * 1000000);
        if (tempOffsetHz <= 500000)
        {
            tempOffsetHz += 5 * 1000000;
            tempFreqMHz -= 5;
        }
        else
        {
            tempOffsetHz += 4 * 1000000;
            tempFreqMHz -= 4;
        }

        freqMHz = tempFreqMHz;
        offsetHz = tempOffsetHz;
    }

    private double GetFrequency(int freqMHz, int offsetHz)
    {
        var freq = new decimal(freqMHz + offsetHz / 1000000d);
        return decimal.ToDouble(freq);
    }

    /// <summary>
    ///     获取不大于x的2的整数次幂
    /// </summary>
    /// <param name="x"></param>
    private int Log2N(int x)
    {
        var n = 0;
        while ((x >>= 1) > 0) n++;
        return n;
    }

    /// <summary>
    ///     FFT变换（原接收机FFT变换）
    /// </summary>
    /// <param name="x">待进行FFT计算的IQ数据组</param>
    private void Fft(ref Complex[] x)
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

        var pi = 3.1415926535897932384626433832795f;
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

    #endregion
}