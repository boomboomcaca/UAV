/*********************************************************************************************
 *
 * 文件名称:    ...Tracker800\Client\Source\DCComponent\Commons\DC.Commons.Algorithm\SignalProcess\SignalProcess.cs
 *
 * 作    者:    jacberg
 *
 * 创作日期:    2017/08/03
 *
 * 修    改:    无
 *
 * 备    注:	信号处理基础库，包含信号滤波，FIR滤波器设计，FM解调，功率谱估计等
 *
 *********************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     此类包含一些信号处理的方法，如FFT IFFT等
/// </summary>
public static class SignalProcess
{
    /// <summary>
    ///     常量π
    /// </summary>
    public const float Pi = 3.1415926535897931f;

    private static int _fftn = 1024;

    /// <summary>
    ///     计算FFT的点数
    /// </summary>
    public static int N
    {
        private get => _fftn;
        set
        {
            var expN = value; // 要求 n 为 2 的整数幂
            var pow = Math.Log(value, 2);
            // 目前限制傅里叶点数为[128,4096]
            while (pow is >= 7 and <= 12 && expN > 1)
            {
                if (expN % 2 != 0) throw new ArgumentException("傅里叶点数必须为2的整数幂！");
                expN /= 2;
            }

            _fftn = value * 2;
        }
    }

    public static double Angle(double real, double imag)
    {
        return Math.Atan2(imag, real);
    }

    public static double[] Angle(double[] real, double[] imag)
    {
        var len = real.Length == imag.Length ? real.Length : 0;
        var result = new double[len];
        for (var i = 0; i < len; i++) result[i] = Math.Atan2(imag[i], result[i]);
        return result;
    }

    public static void Freqz(float[] b, float[] a, out Complex[] h, out float[] w)
    {
        if (b == null || a == null) throw new ArgumentNullException(nameof(a));
        h = new Complex[N / 2];
        w = new float[N / 2];
        float[] real;
        float[] imag;
        // FIR
        if (a.Length == 1)
        {
            // FFTB
            real = new float[N];
            imag = new float[N];
            Array.Copy(b, imag, b.Length);
            Fft(ref imag, ref real);
            var fftB = new Complex[N];
            for (var i = 0; i < N; i++) fftB[i] = new Complex(imag[i], real[i]);
            for (var i = 0; i < N / 2; i++)
            {
                h[i] = fftB[i];
                w[i] = (float)h[i].Phase;
            }
        }
        // IIR
        else
        {
            var maxLen = a.Length > b.Length ? a.Length : b.Length;
            Array.Resize(ref a, maxLen);
            Array.Resize(ref b, maxLen);
            // FFTA
            real = new float[N];
            imag = new float[N];
            Array.Copy(a, real, a.Length);
            Fft(ref real, ref imag);
            var fftA = new Complex[N];
            for (var i = 0; i < N; i++) fftA[i] = new Complex(real[i], imag[i]);
            // FFTB
            real = new float[N];
            imag = new float[N];
            Array.Copy(b, imag, b.Length);
            Fft(ref imag, ref real);
            var fftB = new Complex[N];
            for (var i = 0; i < N; i++) fftB[i] = new Complex(imag[i], real[i]);
            for (var i = 0; i < N / 2; i++)
            {
                h[i] = fftB[i] / fftA[i];
                w[i] = (float)h[i].Phase;
            }
        }
    }

    private static void Swap(ref float a, ref float b)
    {
        (a, b) = (b, a);
    }

    private static void Bitrp(ref float[] xreal, ref float[] ximag, int n)
    {
        // 位反转置换 Bit-reversal Permutation
        int i;
        int p;
        for (i = 1, p = 0; i < n; i *= 2) p++;
        for (i = 0; i < n; i++)
        {
            var a = i;
            var b = 0;
            int j;
            for (j = 0; j < p; j++)
            {
                b = (b << 1) + (a & 1); // b = b * 2 + a % 2;
                a >>= 1; // a = a / 2;
            }

            if (b > i)
            {
                Swap(ref xreal[i], ref xreal[b]);
                Swap(ref ximag[i], ref ximag[b]);
            }
        }
    }

    public static void Fft(Complex[] f, int length)
    {
        if (f == null || length <= 1) return;
        int i;
        int j;
        int mBuf;
        int m;
        Complex t;
        /*-------计算分解的级数M=log2(length)-------*/
        for (i = length, m = 1; (i /= 2) != 1; m++)
        {
        }

        /*-------按照倒位序重新排列原信号-------*/
        for (i = 1, j = length / 2; i <= length - 2; i++)
        {
            if (i < j)
            {
                t = f[j];
                f[j] = f[i];
                f[i] = t;
            }

            var k = length / 2;
            // 防止k = j = 0 added hufb
            while (k <= j && k != 0)
            {
                j -= k;
                k /= 2;
            }

            j += k;
        }

        /*-------FFT算法-------*/
        for (mBuf = 1; mBuf <= m; mBuf++)
        {
            var la = (int)Math.Pow(2, mBuf);
            var lb = la / 2;
            /*-------碟形运算-------*/
            int l;
            for (l = 1; l <= lb; l++)
            {
                var r = (int)((l - 1) * Math.Pow(2, m - mBuf));
                int n;
                for (n = l - 1; n < length - 1; n += la) //遍历每个分组，分组总数为length/la
                {
                    var lc = n + lb;
                    t = f[lc] * new Complex(Math.Cos(2 * Pi * r / length), -Math.Sin(2 * Pi * r / length));
                    f[lc] = f[n] - t;
                    f[n] += t;
                }
            }
        }
    }

    /// <summary>
    ///     将传入的同相正交分量数组进行FFT变换，输出依然为同相正交分量数组，频谱应为ABS(Complex(real,image))
    /// </summary>
    /// <param name="xreal"></param>
    /// <param name="ximage"></param>
    public static void Fft(ref float[] xreal, ref float[] ximage)
    {
        var n = xreal.Length == ximage.Length ? xreal.Length : 0;
        if (n == 0) throw new Exception("I分量和Q分量数组为空或者长度不匹配！");
        // 目前限制必须为2的整数幂，暂不处理其他点数的频谱扩展、平均等 20170927 hufb modified
        // 向量点数不够则补零，超过则截断，加窗等操作外部自己处理
        //if (n < N)
        //{
        Array.Resize(ref xreal, N);
        Array.Resize(ref ximage, N);
        n = N;
        //}
        //else
        //{
        //    int expN = n;    // 要求 n 为 2 的整数幂
        //    while (expN > 1)
        //    {
        //        if (expN % 2 != 0)
        //        {
        //            throw new ArgumentException("必须为2的整数幂！");
        //        }
        //        expN /= 2;
        //    }
        //}
        // 快速傅立叶变换，将复数 x 变换后仍保存在 x 中，xreal, ximag 分别是 x 的实部和虚部
        var wreal = new float[N / 2];
        var wimag = new float[N / 2];
        int m;
        int k;
        int j;
        Bitrp(ref xreal, ref ximage, n);
        // 计算 1 的前 n / 2 个 n 次方根的共轭复数 W'j = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
        var arg = -2 * Pi / n;
        var treal = (float)Math.Cos(arg);
        var timag = (float)Math.Sin(arg);
        wreal[0] = 1.0f;
        wimag[0] = 0.0f;
        for (j = 1; j < n / 2; j++)
        {
            wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
            wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
        }

        for (m = 2; m <= n; m *= 2)
            for (k = 0; k < n; k += m)
                for (j = 0; j < m / 2; j++)
                {
                    var index1 = k + j;
                    var index2 = index1 + m / 2;
                    var t = n * j / m;
                    treal = wreal[t] * xreal[index2] - wimag[t] * ximage[index2];
                    timag = wreal[t] * ximage[index2] + wimag[t] * xreal[index2];
                    var ureal = xreal[index1];
                    var uimag = ximage[index1];
                    xreal[index1] = ureal + treal;
                    ximage[index1] = uimag + timag;
                    xreal[index2] = ureal - treal;
                    ximage[index2] = uimag - timag;
                }
    }

    /// <summary>
    ///     傅里叶变换移动[-Fs/2,Fs/2]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fftData"></param>
    public static IList<T> FftShift<T>(IList<T> fftData)
    {
        var result = new List<T>();
        var length = fftData.Count;
        var index = (int)Math.Floor(fftData.Count / 2.0);
        for (var i = 0; i < index; i++) result.Add(fftData[length - index + i]);
        for (var i = 0; i < length - index; i++) result.Add(fftData[i]);
        return result;
    }

    /// <summary>
    ///     傅里叶变换移动[-Fs/2,Fs/2]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fftData"></param>
    public static T[] FftShift<T>(T[] fftData)
    {
        var result = new T[fftData.Length];
        var length = fftData.Length;
        var index = (int)Math.Floor(fftData.Length / 2.0);
        for (var i = 0; i < index; i++) result[i] = fftData[length - index + i];
        for (var i = 0; i < length - index; i++) result[index + i] = fftData[i];
        return result;
    }

    //public static void FFTShift<T>(T[] fftData)
    //{
    //    T temp;
    //    int N = fftData.Length;
    //    int index = (int)Math.Floor(fftData.Length / 2.0);
    //    for (int i = 0; i < index; i++)
    //    {
    //        temp = fftData[i];
    //        fftData[i] = fftData[index + i];
    //        fftData[index + i] = temp;
    //    }
    //}
    /// <summary>
    ///     对返回N/2点的傅里叶进行翻转拼接 返回点数N
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fftData"></param>
    public static IEnumerable<T> FftShiftOverlap<T>(IEnumerable<T> fftData)
    {
        var enumerable = fftData as T[] ?? fftData.ToArray();
        var left = enumerable.Reverse();
        var result = left.Concat(enumerable);
        return result;
    }

    /// <summary>
    ///     返回标准化频谱数据0-N/2
    ///     <para>如果只传入xreal，则默认为实数处理，否则按照复数处理</para>
    /// </summary>
    /// <param name="xreal">传入数据同相分量数组</param>
    /// <param name="ximage">传入数据正交分量数组</param>
    /// <returns>返回IQ数据的标准频谱</returns>
    public static float[] NormFft(float[] xreal, float[] ximage = null)
    {
        if (xreal == null) throw new Exception("I数据为空！");
        ximage ??= new float[xreal.Length];
        var n = xreal.Length == ximage.Length ? xreal.Length : 0;
        if (n == 0) throw new Exception("I分量和Q分量数组长度不相等！");
        if (n < N)
        {
            Array.Resize(ref xreal, N);
            Array.Resize(ref ximage, N);
            _ = N;
        }

        /**********************当点数比傅里叶计算点数大是分段截断求平均***********************/
        var fft = new float[N / 2];
        Fft(ref xreal, ref ximage);
        for (var i = 0; i < N / 2; i++) fft[i] = (float)Math.Sqrt(xreal[i] * xreal[i] + ximage[i] * ximage[i]);
        return fft;
    }

    /// <summary>
    ///     提供复数数组，进行fft返回标准频谱
    /// </summary>
    /// <param name="data">传入的复数数组</param>
    public static float[] NormFft(Complex[] data)
    {
        if (data == null || data.Length == 0) return null;
        //点数不够则补零
        if (data.Length < N) Array.Resize(ref data, N);
        var xreal = new float[data.Length];
        var ximage = new float[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            xreal[i] = (float)data[i].Real;
            ximage[i] = (float)data[i].Imaginary;
        }

        return NormFft(xreal, ximage);
    }

    /// <summary>
    ///     FFT变换
    /// </summary>
    /// <param name="qData"></param>
    /// <param name="isBothIq">data数组里面是否包含IQ组成的序列，是则进行拆分，否则当做实数序列进行运算</param>
    public static float[] NormFft(float[] qData, bool isBothIq)
    {
        float[] ximage = null;
        float[] xreal;
        if (qData != null)
        {
            // 只包含实数部分
            if (!isBothIq)
            {
                if (qData.Length < N) Array.Resize(ref qData, N);
                xreal = qData;
            }
            else
            {
                if (qData.Length % 2 != 0) throw new Exception("数据长度错误！必须为IQ数据对，即偶数个数据点！！");
                xreal = new float[qData.Length / 2];
                ximage = new float[qData.Length / 2];
                for (var i = 0; i < qData.Length / 2; i++)
                {
                    xreal[i] = qData[2 * i];
                    ximage[i] = qData[2 * i + 1];
                }

                if (xreal.Length < N)
                {
                    Array.Resize(ref xreal, N);
                    Array.Resize(ref ximage, N);
                }
            }
        }
        else
        {
            throw new Exception("IQdata数据为空，无法进行运算！");
        }

        var fft = NormFft(xreal, ximage);
        return fft;
    }

    /// <summary>
    ///     傅里叶逆变换
    /// </summary>
    /// <param name="xreal"></param>
    /// <param name="ximag"></param>
    public static void Ifft(ref float[] xreal, ref float[] ximag)
    {
        if (xreal == null || ximag == null || xreal.Length != ximag.Length)
            throw new ArgumentNullException(nameof(xreal));
        // 快速傅立叶逆变换
        var n = xreal.Length;
        var wreal = new float[N / 2];
        var wimag = new float[N / 2];
        int m;
        int k;
        int j;
        Bitrp(ref xreal, ref ximag, n);
        // 计算 1 的前 n / 2 个 n 次方根 Wj = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
        var arg = 2 * Pi / n;
        var treal = (float)Math.Cos(arg);
        var timag = (float)Math.Sin(arg);
        wreal[0] = 1.0f;
        wimag[0] = 0.0f;
        for (j = 1; j < n / 2; j++)
        {
            wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
            wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
        }

        for (m = 2; m <= n; m *= 2)
            for (k = 0; k < n; k += m)
                for (j = 0; j < m / 2; j++)
                {
                    var index1 = k + j;
                    var index2 = index1 + m / 2;
                    var t = n * j / m;
                    treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                    timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                    var ureal = xreal[index1];
                    var uimag = ximag[index1];
                    xreal[index1] = ureal + treal;
                    ximag[index1] = uimag + timag;
                    xreal[index2] = ureal - treal;
                    ximag[index2] = uimag - timag;
                }

        for (j = 0; j < n; j++)
        {
            xreal[j] /= n;
            ximag[j] /= n;
        }
    }

    /// <summary>
    ///     对信号添加高斯窗
    /// </summary>
    /// <param name="data"></param>
    public static void GaussWin(ref float[] data)
    {
        if (data == null) return;
        var l = 0;
        if (data != null) l = data.Length;
        const float a = 2.5f;
        var n = l - 1;
        for (var i = 0; i < l; i++)
            data[i] *= (float)Math.Exp(-(1 / 2) * CommonMethods.Square(a * (i - n / 2f) / (n / 2f)));
    }

    /// <summary>
    ///     对信号添加海明窗
    /// </summary>
    /// <param name="data"></param>
    public static void HammingWin(ref float[] data)
    {
        if (data == null) return;
        var n = 0;
        if (data != null) n = data.Length;
        for (var i = 0; i < n; i++) data[i] *= 0.54f - 0.46f * (float)Math.Cos(2 * Pi * i / (n - 1));
    }

    /// <summary>
    ///     对信号添加汉宁窗
    /// </summary>
    /// <param name="data"></param>
    public static void HanningWin(ref float[] data)
    {
        if (data == null) return;
        var n = 0;
        if (data != null) n = data.Length;
        for (var i = 0; i < n; i++) data[i] *= 0.5f * (1f - (float)Math.Cos(2 * Pi * i / (n + 1)));
    }

    /// <summary>
    ///     矩形窗相当于输入信号乘1
    /// </summary>
    /// <param name="data"></param>
    public static void RectangleWin(ref float[] data)
    {
        _ = data;
    }

    /// <summary>
    ///     线性卷积
    /// </summary>
    /// <param name="seq1">序列1</param>
    /// <param name="seq2">序列2</param>
    public static IList<double> Convolution(double[] seq1, double[] seq2)
    {
        var m = seq1.Length;
        var n = seq2.Length;
        var conv = new double[m + n - 1];
        //float[] output = new float[m];
        var tempSeq = new double[m + n - 1];
        // do convolution 
        for (var i = 0; i < m; i++) //将Tab_A后面补0
            tempSeq[i] += seq1[i];
        for (var i = 0; i < m + n - 1; i++)
        {
            double temp = 0;
            for (int k = i, j = 0; k >= 0; k--, j++)
                if (j < n)
                    temp += tempSeq[k] * seq2[j];
            conv[i] = temp;
        }

        //for (int i = 0; i < m + n - 1; i++)
        //{
        //    xx[i] = 0.0f;
        //    for (int j = 0; j < m; j++)
        //    {
        //        if (i - j > 0 && i - j < n)
        //            xx[i] += seq1[j] * seq2[i - j];
        //    }
        //}
        //// set value to the output array 
        //for (int i = 0; i < m; i++)
        //    output[i] = xx[i + (n - 1) / 2];
        return conv;
    }

    /// <summary>
    ///     计算校准后的向量角与对应的正交分量
    /// </summary>
    /// <param name="iData">I分量</param>
    /// <param name="qData">Q分量</param>
    /// <param name="correctedQData">校正后的Q分量</param>
    public static void CorrectIqPhase(double[] iData, double[] qData, out double[] correctedQData)
    {
        var angle1 = CommonMethods.VectorAngle(iData, qData);
        var corr = Pi / 2 - angle1 * Pi / 180;
        var sf = Math.Sin(corr);
        var cf = 1 / Math.Cos(corr);
        correctedQData = new double[qData.Length];
        for (var i = 0; i < qData.Length; i++) correctedQData[i] = (qData[i] - iData[i] * sf) * cf;
        CommonMethods.VectorAngle(iData, correctedQData);
        //if (Math.Abs(90 - angle2) > 2 && ++iterator < 10)
        //{
        //    CorrectIQPhase(iData, correctedQData, out correctedQData, out angles);
        //}
    }

    /// <summary>
    ///     IQ imbalance compensation on phase and amplitude
    /// </summary>
    /// <param name="inputsig"></param>
    /// <param name="eps">控制精度</param>
    /// <param name="enableadapt">自动调整</param>
    public static Complex[] IqImbalanceCompensator(Complex[] inputsig, double eps = 1e-4, bool enableadapt = true)
    {
        var len = inputsig.Length;
        var outsig = new Complex[len];
        // 暂不输出调整的复数系数
        //Complex[] coefs = new Complex[len];
        var w = new Complex();
        for (var i = 0; i < len; i++)
        {
            outsig[i] = inputsig[i] + w * Complex.Conjugate(inputsig[i]);
            if (enableadapt) w -= eps * outsig[i] * outsig[i];
            //if (outputCoefFlag)
            //{
            //    coefs[i] = w;
            //}
        }

        return outsig;
    }

    public static short[] CorrectIqAmp(short[] idata, short[] qdata)
    {
        const double eps = 1e-4;
        var y = new Complex[idata.Length];
        var w = new Complex();
        var temp = new Complex[idata.Length];
        for (var i = 0; i < idata.Length; i++) temp[i] = new Complex(idata[i], qdata[i]);
        for (var i = 0; i < idata.Length; i++)
        {
            y[i] = temp[i] + w * Complex.Conjugate(temp[i]);
            w -= eps * y[i] * y[i];
        }

        var q = new short[idata.Length];
        for (var i = 0; i < idata.Length; i++) q[i] = (short)y[i].Imaginary;
        return q;
    }

    public static void CorrectIq(double[] iData, double[] qData, out double[] ni, out double[] nq, bool self = false)
    {
        ni = new double[iData.Length];
        nq = new double[qData.Length];
        // 只计算NQ
        if (self)
        {
            var imaxIndex = CommonMethods.FindPeaksValleys(iData)[0];
            var iminIndex = CommonMethods.FindPeaksValleys(iData)[1];
            var ipeaks = new double[imaxIndex.Count];
            var ivalleys = new double[iminIndex.Count];
            for (var i = 0; i < imaxIndex.Count; i++) ipeaks[i] = iData[imaxIndex[i]];
            for (var i = 0; i < iminIndex.Count; i++) ivalleys[i] = iData[iminIndex[i]];
            var mi = CommonMethods.Mean(ipeaks);
            var mti = CommonMethods.Mean(ivalleys);
            var qmaxIndex = CommonMethods.FindPeaksValleys(qData)[0];
            var qminIndex = CommonMethods.FindPeaksValleys(qData)[1];
            var qpeaks = new double[qmaxIndex.Count];
            var qvalleys = new double[qminIndex.Count];
            for (var i = 0; i < qmaxIndex.Count; i++) qpeaks[i] = qData[qmaxIndex[i]];
            for (var i = 0; i < qminIndex.Count; i++) qvalleys[i] = qData[qminIndex[i]];
            var mq = CommonMethods.Mean(qpeaks);
            var mtq = CommonMethods.Mean(qvalleys);
            var a = mi / mq;
            var b = mti / mtq;
            var beta = (a + b) / 2;
            for (var i = 0; i < qData.Length; i++) nq[i] = beta * qData[i];
        }
        else
        {
            var pi = CommonMethods.Norm(iData);
            var pq = CommonMethods.Norm(qData);
            double beta1 = 1;
            if (pq != 0) beta1 = pi / pq;
            for (var i = 0; i < qData.Length; i++) nq[i] = beta1 * qData[i];
        }

        CorrectIqPhase(iData, nq, out nq);
        // 对I分量首先进行去直流归一化
        var meanI = CommonMethods.Mean(iData);
        for (var i = 0; i < qData.Length; i++) ni[i] = iData[i] - meanI;
        var maxNi = CommonMethods.Max(ni);
        var minNi = CommonMethods.Min(ni);
        if (maxNi - minNi != 0)
            for (var i = 0; i < ni.Length; i++)
                ni[i] = 2 * (ni[i] - minNi) / (maxNi - minNi) - 1;
        var meanNq = CommonMethods.Mean(nq);
        for (var i = 0; i < qData.Length; i++) nq[i] -= meanNq;
        var maxNq = CommonMethods.Max(nq);
        var minNq = CommonMethods.Min(nq);
        if (maxNq - minNq == 0) return;
        for (var i = 0; i < nq.Length; i++) nq[i] = 2 * (nq[i] - minNq) / (maxNq - minNq) - 1;
    }

    /// <summary>
    ///     抽取
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="n">抽取倍数[2,13]</param>
    /// <param name="sample">原采样率，用于抽取前滤波，防止混叠</param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"></exception>
    public static double[] Decimate(double[] data, int n, double sample = 22050d)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (n > 13) throw new ArgumentException("抽取倍数太大可能会导致信号失真，建议逐级抽取！！");
        var hn = new double[n + 1];
        var den = new double[] { 1 };
        var result = new double[(int)Math.Ceiling(1.0 * data.Length / n)];
        // 其实，低通滤波的频率应为1/(N+2)*Fs
        FilterDesignBase.FirFilterDesign(1.0 / n * sample, 0, sample, n, FilterType.LowPass, WindowType.HanningWin, hn);
        FilterDesignBase.SignalFilter(hn, den, data, out var temp);
        var j = 0;
        for (var i = 0; i < temp.Length; i++)
            //每隔三个元素取一个元素，即4倍下采样
            if (i % n == 0)
            {
                result[j] = temp[i];
                j++;
            }

        return result;
    }

    /// <summary>
    ///     %% 输入待检测的频谱数据
    /// </summary>
    /// <param name="st"></param>
    public static bool SignalExist(double[] st)
    {
        //防止负电平出现导致判断不稳定结果
        var temp = new double[st.Length - 1];
        Array.Copy(st, 1, temp, 0, temp.Length);
        var maxIndex = CommonMethods.FindPeaksValleys(temp)[0];
        var peaks = new double[maxIndex.Count];
        for (var i = 0; i < maxIndex.Count; i++) peaks[i] = temp[maxIndex[i]];
        var bn = CommonMethods.Mean(peaks);
        if (bn < -100) return false;
        var e1 = CommonMethods.Mean(temp);
        var e2 = CommonMethods.Mean(temp.Where(item => item > e1).ToArray());
        var e3 = CommonMethods.Mean(temp.Where(item => item > e2).ToArray());
        var m = e3 / e1 - 1;
        if (m > 0)
        {
            if (m > 0.0245 /* && m < 0.1*/) return true;
            return false;
        }

        return m is > -2.8 and < -0.1;
    }

    /// <summary>
    ///     符号速率估计
    /// </summary>
    /// <param name="x">中频连续IQ数据</param>
    /// <param name="fs">采样率（单位：MHz）</param>
    /// <returns>估计的波特率（单位：bps）</returns>
    public static double SymbolRateEstimate(Complex[] x, double fs)
    {
        var n = x.Length;
        var m = n / 16;
        Fft(x, n);
        x = FftShift(x);
        var temp = new Complex[n / 2 - m / 2 + 1];
        for (var alpha = 0; alpha < n / 2 - m / 2 + 1; alpha++)
        {
            var k = 0;
            var vector1 = new Complex[m];
            for (var j = n / 2 + alpha - m / 2; j < n / 2 + alpha + m / 2; j++) vector1[k++] = x[j];
            k = 0;
            var vector2 = new Complex[m];
            for (var j = n / 2 - alpha - m / 2; j < n / 2 - alpha + m / 2; j++) vector2[k++] = Complex.Conjugate(x[j]);
            temp[alpha] = CommonMethods.VectorsInProduct(vector1, vector2);
        }

        ////////////////////PLOT FIGURE PART//////////////////////
        //Complex[] btemp = new Complex[2 * temp.Length - 1];
        //k = 0;
        //for (int i = temp.Length - 1; i > 0; i--)
        //{
        //    btemp[k++] = temp[i];
        //}
        //k = 0;
        //for (int i = 0; i < temp.Length; i++)
        //{
        //    btemp[temp.Length - 1 + k++] = temp[i];
        //}
        //k = 0;
        //Complex[] b = new Complex[N];
        //for (int i = M / 2; i < N - M / 2; i++)
        //{
        //    b[i] = btemp[k++];
        //}
        temp[0] = 0; // 排除直流分量
        var peakPos = CommonMethods.MaxPositionWithAbs(temp);
        var symbolRate = 2 * 1.0e6 * peakPos / n * fs;
        return symbolRate;
    }

    /// <summary>
    ///     载波频率估计算法
    /// </summary>
    /// <param name="st"></param>
    /// <param name="fs"></param>
    public static double FcEstimate(Complex[] st, double fs)
    {
        var n = st.Length;
        // 一般收到数据帧为4096/8192点
        //if (N > 4096)
        //{
        //    N = 4096;
        //    Array.Resize(ref st, N);
        //}
        Fft(st, n);
        var temp = new Complex[(int)Math.Ceiling((n - 1) / 2.0)];
        var yt = new double[temp.Length];
        for (var i = 1; i < temp.Length; i++)
        {
            temp[i] = st[i];
            yt[i] = st[i].Magnitude;
        }

        var threshold = CommonMethods.Max(yt) * 0.62;
        var ps = yt.Where(t => t > threshold).ToList();
        /////// multiple peaks
        var psCount = ps.Count;
        if (psCount == 2)
            return CommonMethods.Mean([fs * Array.IndexOf(yt, ps[0]) / n, fs * Array.IndexOf(yt, ps[1]) / n]);
        if (psCount is > 2 and <= 20)
        {
            var flag = false;
            var f1 = fs * Array.IndexOf(yt, ps[0]) / n;
            var f2 = 0.0d;
            for (var i = 1; i < psCount; i++)
            {
                var deltaTemp = Math.Abs(10 * Math.Log10(ps[i]) - 10 * Math.Log10(ps[0]));
                if (deltaTemp <= 3)
                {
                    flag = true;
                    f2 = fs * Array.IndexOf(yt, ps[i]) / n;
                }
            }

            double fc;
            if (flag)
                fc = (f1 + f2) / 2.0d;
            else
                fc = f1;
            return fc;
        }

        ///////// single peak
        double a1 = 0;
        double a2 = 0;
        var index = CommonMethods.MaxPositionWithAbs(temp);
        if (index >= 2 && index <= n - 1)
            if (st[index] != 0)
            {
                a1 = (st[index - 1] / st[index]).Real;
                a2 = (st[index + 1] / st[index]).Real;
            }

        var delta1 = 0.0d;
        var delta2 = 0.0d;
        if (Math.Abs(a1 - 1) > 1e-9) delta1 = a1 / (1 - a1);
        if (Math.Abs(a2 - 1) > 1e-9) delta2 = -a2 / (1 - a2);
        // 为了更准确的估计载波频率，在峰值附近估计频率的偏差进行误差补偿  
        double delta;
        if (delta1 > 0 && delta2 > 0)
            delta = delta2;
        else
            delta = delta1;
        //yt.ToList().ForEach(item => System.Diagnostics.Debug.WriteLine(item));
        return (index + delta) / n * fs;
    }

    /// <summary>
    ///     频域估计信号载波
    /// </summary>
    /// <param name="iq">时域信号iq数据</param>
    public static double FcEstimateFreq(Complex[] iq)
    {
        Fft(iq, iq.Length);
        var dftabs = new double[iq.Length];
        for (var i = 0; i < iq.Length; i++) dftabs[i] = iq[i].Magnitude * 2 / iq.Length;
        var ns = iq.Length / 2;
        var sum1 = 0.0d;
        var sum2 = 0.0d;
        for (var i = 0; i < ns; i++)
        {
            sum1 += i * Math.Pow(dftabs[i], 2);
            sum2 += Math.Pow(dftabs[i], 2);
        }

        var fc = 0.0d;
        if (sum2 != 0) fc = sum1 / sum2;
        return fc;
    }

    /// <summary>
    ///     时域估计信号载波
    /// </summary>
    /// <param name="iq">时域信号</param>
    /// <param name="fs">采样率</param>
    public static double FcEstimateTime(Complex[] iq, double fs)
    {
        var ns = iq.Length;
        var st = new double[iq.Length];
        for (var i = 0; i < ns; i++) st[i] = iq[i].Magnitude;
        var mz = 0.0d;
        var sx = new List<double>();
        for (var i = 0; i < ns - 1; i++)
            if (st[i] * st[i + 1] <= 0)
            {
                mz++;
                var temp = 1 / fs * (i + st[i]) / (st[i] - st[i + 1]);
                sx.Add(temp);
            }

        var sy = new double[sx.Count - 1];
        for (var i = 0; i < sx.Count - 1; i++) sy[i] = sx[i + 1] - sx[i];
        var fc = 0.0d;
        var sum = sy.Sum();
        if (sum != 0.0d) fc = (mz - 1) / (2 * sum);
        return fc;
    }

    public static double FcEstimate(short[] iData, short[] qData, double fs)
    {
        if (iData == null || qData == null) throw new ArgumentNullException(nameof(iData));
        if (iData.Length != qData.Length) throw new ArgumentOutOfRangeException(nameof(iData));
        var st = new Complex[iData.Length];
        for (var i = 0; i < iData.Length; i++) st[i] = new Complex(iData[i], qData[i]);
        return FcEstimate(st, fs);
    }

    public static double FcEstimate(IList<double> st, double fs)
    {
        if (st == null) throw new ArgumentNullException(nameof(st));
        var signal = new Complex[st.Count];
        for (var i = 0; i < st.Count; i++) signal[i] = new Complex(st[i], 0);
        var fc = FcEstimate(signal, fs);
        return fc;
    }

    public static double FcEstimate(double[] iData, double[] qData, double fs)
    {
        if (iData == null || qData == null) throw new ArgumentNullException(nameof(iData));
        if (iData.Length != qData.Length) throw new ArgumentOutOfRangeException(nameof(iData));
        var st = new Complex[iData.Length];
        for (var i = 0; i < iData.Length; i++) st[i] = new Complex(iData[i], qData[i]);
        return FcEstimate(st, fs);
    }

    /// <summary>
    ///     复数相位等效于new Complex().Phase;
    /// </summary>
    /// <param name="c"></param>
    public static double Phase(Complex c)
    {
        double phase;
        if (c.Real > 0)
        {
            phase = Math.Atan(c.Imaginary / c.Real);
        }
        else if (c.Real < 0)
        {
            if (c.Imaginary < 0)
                phase = Math.Atan(c.Imaginary / c.Real) - Math.PI;
            else
                // >=0
                phase = Math.Atan(c.Imaginary / c.Real) + Math.PI;
        }
        else if (c.Imaginary <= 0)
        {
            phase = -Math.PI / 2;
        }
        else
        {
            phase = Math.PI / 2;
        }

        return phase;
    }

    /// <summary>
    ///     IQ复数相位
    /// </summary>
    /// <param name="real"></param>
    /// <param name="imag"></param>
    public static double Phase(double real, double imag)
    {
        double phase;
        if (real > 0)
        {
            phase = Math.Atan(imag / real);
        }
        else if (real < 0)
        {
            if (imag < 0)
                phase = Math.Atan(imag / real) - Math.PI;
            else
                // >=0
                phase = Math.Atan(imag / real) + Math.PI;
        }
        else if (imag <= 0)
        {
            phase = -Math.PI / 2;
        }
        else
        {
            phase = Math.PI / 2;
        }

        return phase;
    }

    /// <summary>
    ///     IQ复数相位
    /// </summary>
    /// <param name="real"></param>
    /// <param name="imag"></param>
    public static double Phase(short real, short imag)
    {
        var dreal = real * 1.0;
        double phase;
        if (dreal > 0)
        {
            phase = Math.Atan(imag / dreal);
        }
        else if (dreal < 0)
        {
            if (imag < 0)
                phase = Math.Atan(imag / dreal) - Math.PI;
            else
                // >=0
                phase = Math.Atan(imag / dreal) + Math.PI;
        }
        else if (imag <= 0)
        {
            phase = -Math.PI / 2;
        }
        else
        {
            phase = Math.PI / 2;
        }

        return phase;
    }

    public static double[] Phase(short[] idata, short[] qdata)
    {
        var n = idata.Length;
        var phi = new double[n];
        for (var i = 0; i < n; i++) phi[i] = Phase(idata[i], qdata[i]);
        return phi;
    }

    public static double[] UnwrapPhase(double[] phi, double cutoff = Math.PI)
    {
        var m = phi.Length;
        var unphi = new double[m];
        var diff = new double[m - 1];
        var dpCorr = new double[m - 1];
        var roundDown = new int[m - 1];
        unphi[0] = phi[0];
        for (var i = 0; i < m - 1; i++)
        {
            diff[i] = phi[i + 1] - phi[i];
            dpCorr[i] = diff[i] / (2 * Math.PI);
            if (Math.Abs(Rem(dpCorr[i], 1)) <= 0.5) roundDown[i] = 1;
        }

        for (var i = 0; i < m - 1; i++)
        {
            if (roundDown[i] == 1) dpCorr[i] = (int)dpCorr[i];
            dpCorr[i] = Math.Round(dpCorr[i]);
            if (Math.Abs(diff[i]) < cutoff) dpCorr[i] = 0;
        }

        for (var i = 1; i < m - 1; i++) dpCorr[i] += dpCorr[i - 1];
        for (var i = 1; i < m; i++) unphi[i] = phi[i] - 2 * Math.PI * dpCorr[i - 1];
        return unphi;
    }

    public static double Rem(double x, double y)
    {
        if (y != 0) return x - (int)(x / y) * y;
        return double.NaN;
    }

    public static double[] Rem(double[] x, double y)
    {
        double[] result = null;
        if (y != 0)
        {
            result = new double[x.Length];
            for (var i = 0; i < result.Length; i++) result[i] = Rem(x[i], y);
        }

        return result;
    }
}

public static class FilterDesignBase
{
    private const double Pi = Math.PI;
    private const double TwoPi = 2 * Pi;

    public static void FirFilterDesign(double fc1, double fc2, double fs, int order, FilterType filtertype,
        WindowType wintype, double[] hn)
    {
        // FIR窗口滤波器,返回滤波器系数
        // 参数说明
        // order:滤波器阶数
        // filtertype:类型,0-低通,1-高通,2-带通,3-带阻
        // fc1:边界频率,对于带通或带阻,下边界频率
        // fc2:对于带通或带阻,上边界频率
        // fs:采样频率
        // wintype:窗口类型,0-矩形,1-图基,2-三角,3-汉宁,4-海明,5-布拉克曼,6-凯塞
        int i;
        int n2;
        int mid;
        double s;
        var wc2 = 0.0d;
        var beta = 0.0d;
        fc1 /= fs;
        fc2 /= fs;
        if (wintype == WindowType.KaiserWin)
            // 如果选择的是凯塞窗,需要设定beta参数.
            beta = 7.865d;
        else if (wintype == WindowType.GaussWin) beta = 2.5d;
        if (order % 2 == 0)
        {
            n2 = order / 2 - 1;
            mid = 1;
        }
        else
        {
            n2 = order / 2;
            mid = 0;
        }

        var delay = order / 2.0; //延时常数
        var wc1 = 2.0 * Pi * fc1; //计算角频率 
        //如果是带通或带阻类型,还需要计算上边界角频率
        if (filtertype is FilterType.BandPass or FilterType.BandStop) wc2 = 2.0 * Pi * fc2;
        switch (filtertype)
        {
            case FilterType.LowPass:
                for (i = 0; i <= n2; i++)
                {
                    s = i - delay;
                    hn[i] = Math.Sin(wc1 * s) / (Pi * s) * Window(wintype, order + 1, i, beta);
                    hn[order - i] = hn[i];
                }

                if (mid == 1) hn[order / 2] = wc1 / Pi;
                break;
            case FilterType.HighPass:
                for (i = 0; i <= n2; i++)
                {
                    s = i - delay;
                    hn[i] = (Math.Sin(Pi * s) - Math.Sin(wc1 * s)) / (Pi * s);
                    hn[i] *= Window(wintype, order + 1, i, beta);
                    hn[order - i] = hn[i];
                }

                if (mid == 1) hn[order / 2] = 1.0 - wc1 / Pi;
                break;
            case FilterType.BandPass:
                for (i = 0; i <= n2; i++)
                {
                    s = i - delay;
                    hn[i] = (Math.Sin(wc2 * s) - Math.Sin(wc1 * s)) / (Pi * s);
                    hn[i] *= Window(wintype, order + 1, i, beta);
                    hn[order - i] = hn[i];
                }

                if (mid == 1) hn[order / 2] = (wc2 - wc1) / Pi;
                break;
            case FilterType.BandStop:
                for (i = 0; i <= n2; i++)
                {
                    s = i - delay;
                    hn[i] = (Math.Sin(wc1 * s) + Math.Sin(Pi * s) - Math.Sin(wc2 * s)) / (Pi * s);
                    hn[i] *= Window(wintype, order + 1, i, beta);
                    hn[order - i] = hn[i];
                }

                if (mid == 1) hn[order / 2] = (wc1 + Pi - wc2) / Pi;
                break;
        }
    }

    private static double Window(WindowType winType, int nI, int i, double beta)
    {
        var w = 1.0;
        switch (winType)
        {
            case WindowType.RectWin: // 矩形窗
                w = 1.0;
                break;
            case WindowType.TukeyWin: // 图基窗
                var k = (nI - 2) / 10;
                if (i <= k) w = 0.5 * (1.0 - Math.Cos(i * Pi / (k + 1)));
                if (i > nI - k - 2) w = 0.5 * (1.0 - Math.Cos((nI - i - 1) * Pi / (k + 1)));
                break;
            case WindowType.BartlettWin: // 三角
                w = 1.0 - Math.Abs(1.0 - 2 * i / (nI - 1.0));
                break;
            case WindowType.GaussWin:
                double n = nI - 1;
                var index = i - n / 2;
                w = Math.Exp(-(1.0 / 2) * CommonMethods.Square(beta * index / (n / 2.0)));
                break;
            case WindowType.HanningWin: // 汉宁
                w = 0.5 * (1.0 - Math.Cos(2 * i * Pi / (nI - 1)));
                break;
            case WindowType.HammingWin: // 海明
                w = 0.54 - 0.46 * Math.Cos(2 * i * Pi / (nI - 1));
                break;
            case WindowType.BlackManWin: // 布拉克曼
                w = 0.42 - 0.5 * Math.Cos(2 * i * Pi / (nI - 1)) + 0.08 * Math.Cos(4 * i * Pi / (nI - 1));
                break;
            case WindowType.KaiserWin: // 凯塞
                w = Kaiser(i, nI, beta);
                break;
        }

        return w;
    }

    /// <summary>
    ///     产生常用窗函数
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="wintype">窗口类型</param>
    /// <param name="n">窗口宽度</param>
    /// <param name="beta">用于凯瑟窗的贝塞尔函数系数，默认值为7.865；用于高斯窗时表示标准差，默认值为2.5，beta增大窗口减小，反之亦然。</param>
    public static IList<T> Window<T>(WindowType wintype, int n, double beta = 7.865d)
    {
        var tempWin = new double[n];
        for (var i = 0; i < n; i++) tempWin[i] = Window(wintype, n, i, beta);
        return Array.ConvertAll(tempWin, d => (T)Convert.ChangeType(d, typeof(T)));
    }

    private static double Kaiser(int i, int n, double beta)
    {
        var b1 = Bessel0(beta);
        var a = 2.0 * i / (n - 1) - 1.0;
        var a2 = a * a;
        var beta1 = beta * Math.Sqrt(1.0 - a2);
        var b2 = Bessel0(beta1);
        return b2 / b1;
    }

    private static double Bessel0(double x)
    {
        var d = 1.0;
        var sum = 1.0;
        var y = x / 2.0;
        for (var i = 1; i <= 25; i++)
        {
            d = d * y / i;
            var d2 = d * d;
            sum += d2;
            if (d2 < sum * 1.0e-8) break;
        }

        return sum;
    }

    public static double[] Convolution(double[] input, double[] impulse)
    {
        // 计算卷积积分
        // input:输入信号
        // n1:输入信号长度
        // impulse:冲击响应信号
        // n2:冲击响应信号长度
        // result:卷积输出
        int minN;
        int maxN;
        int k;
        int i;
        var n1 = input.Length;
        var n2 = impulse.Length;
        var ts = new double[4096];
        var th = new double[4096];
        var sum = new double[8192];
        var result = new double[n1 + n2 - 1];
        if (n1 + n2 > 8192) return result;
        //初始化sum
        for (var j = 0; j < n1 + n2 - 1; j++) sum[j] = 0;
        if (n1 >= n2)
        {
            maxN = n1;
            minN = n2;
            for (i = 0; i < maxN; i++) ts[i] = input[i];
            for (i = 0; i < minN; i++) th[i] = impulse[i];
        }
        else
        {
            maxN = n2;
            minN = n1;
            for (i = 0; i < maxN; i++) ts[i] = impulse[i];
            for (i = 0; i < minN; i++) th[i] = input[i];
        }

        for (k = 0; k < n1 + n2 - 1; k++)
        {
            if (k < minN - 1)
                for (i = 0; i <= k; i++)
                    sum[k] += ts[i] * th[k - i];
            int temp;
            if (k >= minN - 1 && k < maxN)
            {
                temp = minN - 1;
                for (i = k - (minN - 1); i < k - (minN - 1) + minN; i++)
                {
                    sum[k] += ts[i] * th[temp];
                    temp--;
                }
            }

            if (k >= maxN && k < minN + maxN - 1)
            {
                temp = minN - 1;
                for (i = k - (minN - 1); i < k - (minN - 1) + (maxN + minN - k - 1); i++)
                {
                    sum[k] += ts[i] * th[temp];
                    temp--;
                }
            }

            result[k] = sum[k];
        }

        return result;
    }

    public static void FourieTrans(float[] pfReal, float[] pfImag, uint iNumOfSamples)
    {
        //公式直接方法求傅立叶转换,速度慢.
        //ianliu
        var pfTemp = new float[4096 * 2];
        uint k;
        uint n;
        Array.Copy(pfReal, pfTemp, iNumOfSamples * 2);
        //memcpy(pfTemp, pfReal, iNumOfSamples * 2 * sizeof(float));
        for (k = 0; k < iNumOfSamples; k++) pfReal[k] = 0;
        for (k = 0; k < iNumOfSamples; k++)
            for (n = 0; n < iNumOfSamples; n++)
            {
                pfReal[k] = (float)(pfReal[k] + pfTemp[n] * Math.Cos(TwoPi / iNumOfSamples * k * n));
                pfImag[k] = (float)(pfImag[k] - pfTemp[n] * Math.Sin(TwoPi / iNumOfSamples * k * n));
            }
    }

    /// <summary>
    ///     返回复数的模
    /// </summary>
    /// <param name="pfRealBuffer">同相分量数组</param>
    /// <param name="pfImagBuffer">正交分量数组</param>
    /// <param name="pfResBuffer">复数的模结果数组</param>
    public static void GetComplexMode(float[] pfRealBuffer, float[] pfImagBuffer, float[] pfResBuffer)
    {
        var iNum = pfRealBuffer.Length == pfImagBuffer.Length ? pfRealBuffer.Length : 0;
        if (iNum == 0) return;
        //返回复数的模
        for (var i = 0; i < iNum; i++)
            pfResBuffer[i] = (float)Math.Sqrt(pfRealBuffer[i] * pfRealBuffer[i] + pfImagBuffer[i] * pfImagBuffer[i]);
    }

    /// <summary>
    ///     计算脉冲响应
    /// </summary>
    /// <param name="b">滤波器系数分子</param>
    /// <param name="a">滤波器分母</param>
    /// <param name="t">时间序列</param>
    /// <param name="h">脉冲响应序列</param>
    /// <param name="n">脉冲响应长度</param>
    /// <param name="type">系统响应类型</param>
    /// <param name="fs">滤波器对应的采样率</param>
    public static void SystemResponse(double[] b, double[] a, out double[] t, out double[] h, int n = 20,
        ResponseType type = ResponseType.ImpulseResponse, double fs = 1)
    {
        if (b == null && a == null) throw new ArgumentNullException(nameof(b));
        t = new double[n];
        var x = new double[n];
        if (type == ResponseType.ImpulseResponse)
            x[0] = 1.0d;
        else
            // 阶跃响应
            for (var i = 0; i < n; i++)
                x[i] = 1.0d;
        SignalFilter(b, a, x, out h);
        for (var i = 0; i < n; i++) t[i] = i / fs;
    }

    /// <summary>
    ///     有限脉冲响应实现带通滤波器设计（线性相位）
    /// </summary>
    /// <param name="freqStart">滤波器起始频率</param>
    /// <param name="freqStop">滤波器截止频率</param>
    /// <param name="freqSample">采样率</param>
    /// <param name="n">滤波器阶数</param>
    /// <param name="windowFlag">窗函数选择,0:矩形窗；1：海明窗；2：汉宁窗；默认选择海明；</param>
    /// <param name="filterCoeffs">返回的滤波器系数</param>
    public static void FirBandPass(double freqStart, double freqStop, double freqSample, int n, byte windowFlag,
        ref double[] filterCoeffs)
    {
        var wn1 = freqStart / freqSample * 2;
        var wn2 = freqStop / freqSample * 2;
        var order = n + 1;
        int i;
        //计算窗
        var wind = new double[order];
        //窗函数计算Order阶系数
        var temp = 2.0 * Pi / (order - 1); //
        switch (windowFlag)
        {
            case 0:
                for (i = 0; i < order; i++) //矩形窗
                    wind[i] = 1;
                break;
            case 1:
                for (i = 0; i < order; i++) //海明窗
                    wind[i] = 0.54 - 0.46 * Math.Cos(temp * i);
                break;
            case 2:
                for (i = 0; i < order; i++) //汉宁窗
                    wind[i] = 0.5 * (1.0 - Math.Cos(temp * i));
                break;
            default:
                for (i = 0; i < order; i++) //海明窗
                    wind[i] = 0.54 - 0.46 * Math.Cos(temp * i);
                break;
        }

        var tmpNum1 = (order - 1) / 2;
        //const bool tmpNodd = false;
        // 	if (Order % 2 != 0)
        // 		tmpNodd = TRUE;
        var tmpNum2 = tmpNum1 + 1;
        var tmpArr = new double[tmpNum2];
        for (var j = 0; j < tmpNum2; j++)
            tmpArr[j] = j + 0.5;

        // for (int j = 0; j < tmpNum2; j++)
        // {
        //     tmpArr[j] = j;
        // }

        var b = new double[tmpNum2 + 1];
        for (i = 0; i < tmpNum2; i++)
            b[i] = wn2 / 2 * (Math.Sin(tmpArr[i] * Pi * wn2) / (tmpArr[i] * Pi * wn2)) -
                   wn1 / 2 * (Math.Sin(tmpArr[i] * Pi * wn1) / (tmpArr[i] * Pi * wn1));

        for (i = 0; i <= tmpNum2; i++) b[i] = 4 * b[i];

        var h = new double[order];
        var tmpNum3 = order / 2;

        for (i = 0; i < tmpNum3; i++) h[i] = b[tmpNum3 - i - 1] / 2;
        for (i = tmpNum3; i < order; i++) h[i] = b[i - tmpNum3] / 2;
        for (i = 0; i < order; i++) h[i] *= wind[i];
        double f0;
        if (Math.Abs(wn2 - 1) < 1e-9)
            f0 = 1;
        else
            f0 = (wn1 + wn2) / 2;
        double tmpReal = 0;
        double tmpImag = 0;
        //tmpR = (cos(2*PI*(0:L-1)*(f0/2))*(b.'));
        for (i = 0; i < order; i++)
        {
            tmpReal += Math.Cos(2 * Pi * i * (f0 / 2)) * h[i];
            tmpImag += Math.Sin(2 * Pi * i * (f0 / 2)) * h[i];
        }

        var tmpM = Math.Sqrt(tmpReal * tmpReal + tmpImag * tmpImag);
        if (tmpM != 0)
            for (i = 0; i < order; i++)
                h[i] /= tmpM;
        Array.Copy(h, filterCoeffs, order);
        //memcpy(pModul, h, tmpNum * sizeof(double));
    }

    /// <summary>
    ///     平滑移动滤波
    /// </summary>
    /// <param name="filterK">滤波器系数组</param>
    /// <param name="data">待滤波的数据或信号片段</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void MovingAvg(double[] filterK, ref float[] data)
    {
        if (data == null || filterK == null) throw new ArgumentNullException(nameof(data));
        var mPreIFilterN = data;
        var numIn = data.Length;
        var filterN = filterK.Length;
        //NumOut = NumIn;// - FilterN + 1;
        var tmpRes = new float[numIn];
        for (var i = 0; i < numIn; i++)
        {
            float tmpSum = 0;
            for (var j = 0; j < filterN; j++)
                if (i + j - filterN < 0)
                    tmpSum += (float)(mPreIFilterN[i + j] * filterK[j]);
                else
                    tmpSum += (float)(data[i + j - filterN] * filterK[j]);
            tmpRes[i] = tmpSum;
        }

        Array.Copy(data, numIn - filterN - 1, mPreIFilterN, 0, filterN);
        Array.Copy(tmpRes, data, numIn);
        //memcpy(mPreIFilterN, &Data[NumIn - FilterN - 1], FilterN * sizeof(short));
        //memcpy(Data, tmpRes, NumOut * sizeof(short));
    }

    /// <summary>
    ///     对输入信号进行滤波 滤波器类型Direct-II
    /// </summary>
    /// <param name="num">滤波器分子</param>
    /// <param name="den">滤波器分母</param>
    /// <param name="input">输入信号</param>
    /// <param name="output">滤波器输出结果</param>
    public static void SignalFilter(double[] num, double[] den, double[] input, out double[] output)
    {
        // <param name="zi">输入滤波器时延矢量，为MAX(LENGTH(DEN),LENGTH(NUM))-1长度的矢量</param>
        // <param name="zf">输出信号延迟矢量</param>
        var lenNum = num.Length;
        var lenDen = den.Length;
        var lenInput = input.Length;
        if (den[0] == 0)
            throw new ArgumentException("den[0] cannot be zero!");
        if (input == null || num == null || den == null)
            throw new ArgumentNullException($"{num},{den},{input}");
        if (Math.Abs(den[0] - 1) > 1e-9)
        {
            for (var i = 1; i != lenDen; i++) den[i] /= den[0];
            for (var i = 1; i != lenNum; i++) num[i] /= den[0];
            den[0] = 1;
        }

        var na = lenDen - 1;
        var nb = lenNum - 1;
        var len = na > nb ? na : nb;
        var zi = new double[len];
        output = new double[lenInput];
        // bool zi_null = zi == null;

        var zf = new double[len];
        // 1.
        {
            var a = new double[len + 1];
            Array.Copy(den, a, lenDen);
            var b = new double[len + 1];
            Array.Copy(num, b, lenNum);
            var zfLast = new double[len];
            Array.Copy(zi, zfLast, len);
            for (var i = 0; i != lenInput; i++)
            {
                output[i] = num[0] * input[i] + zfLast[0];
                zf[len - 1] = b[len] * input[i] - a[len] * output[i];
                for (var j = len - 2; j >= 0; j--) zf[j] = b[j + 1] * input[i] + zfLast[j + 1] - a[j + 1] * output[i];
                Array.Copy(zf, zfLast, len);
            }
        }
    }

    /// <summary>
    ///     16位音频数据滤波
    /// </summary>
    /// <param name="num">滤波器系数</param>
    /// <param name="den">滤波器系数</param>
    /// <param name="data">音频流</param>
    internal static byte[] AudioFilter(double[] num, double[] den, byte[] data)
    {
        if (data == null || data.Length % 2 != 0) throw new ArgumentException("data=null || data.length % 2 !=0");
        // 该方法只处理16位
        var block = new byte[2];
        var origin = new short[data.Length / 2];
        for (var j = 0; j < data.Length; j += 2)
        {
            Array.Copy(data, j, block, 0, 2);
            origin[j / 2] = BitConverter.ToInt16(block, 0);
        }

        double max = origin.Max();
        double min = origin.Min();
        var constDen = max - min;
        var audio = new double[origin.Length];
        for (var j = 0; j < audio.Length; j++) audio[j] = 2 * (origin[j] - min) / constDen - 1;
        SignalFilter(num, den, audio, out var midAudio);
        var resultAudio = new byte[midAudio.Length * 2];
        for (var j = 0; j < midAudio.Length; j++)
        {
            var recoveryAudio = (midAudio[j] + 1) / 2 * (max - min) + min;
            var tempBytes = BitConverter.GetBytes((short)recoveryAudio);
            Array.Copy(tempBytes, 0, resultAudio, 2 * j, tempBytes.Length);
        }

        if (data.Length % 2 != 0)
        {
            Array.Resize(ref resultAudio, resultAudio.Length + 1);
            Array.Copy(data, data.Length - 1, resultAudio, resultAudio.Length - 1, 1);
        }

        return resultAudio;
    }

    /// <summary>
    ///     对8位、16位、32位的音频流进行滤波
    /// </summary>
    /// <param name="num">滤波器系数</param>
    /// <param name="den">滤波器系数</param>
    /// <param name="data">音频流</param>
    /// <param name="bits">音频流位数</param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    /// <exception cref="InvalidDataException"></exception>
    public static byte[] AudioFilterWithBits(double[] num, double[] den, byte[] data, byte bits = 16)
    {
        // Audio Sample Bits Usually Use 16 Bits To Pcik Up,This Method Is Just For Extension;
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length % bits != 0 && bits == 32)
            throw new InvalidDataException("Data length is not matched with Bits!");
        byte[] resultAudio = null;
        double max;
        double min;
        double constDen;
        double[] audio;
        double[] midAudio;
        int[] origin;
        switch (bits)
        {
            #region 8位音频流

            case 8:
                {
                    origin = new int[data.Length];
                    for (var j = 0; j < data.Length; j++) origin[j] = data[j];
                    max = origin.Max();
                    min = origin.Min();
                    constDen = max - min;
                    audio = new double[origin.Length];
                    for (var j = 0; j < audio.Length; j++) audio[j] = 2 * (origin[j] - min) / constDen - 1;
                    SignalFilter(num, den, audio, out midAudio);
                    resultAudio = new byte[midAudio.Length];
                    for (var j = 0; j < midAudio.Length; j++)
                    {
                        var recoveryAudio = (midAudio[j] + 1) / 2 * (max - min) + min;
                        resultAudio[j] = BitConverter.GetBytes(recoveryAudio)[0];
                    }
                }
                break;

            #endregion 8位音频流

            #region 16位音频流

            case 16:
                resultAudio = AudioFilter(num, den, data);
                break;

            #endregion 16位音频流

            #region 32位音频流

            case 32:
                {
                    var blockLength = bits >> 3;
                    origin = new int[data.Length / blockLength];
                    var block = new byte[blockLength];
                    for (var j = 0; j < data.Length; j += blockLength)
                    {
                        Array.Copy(data, j, block, 0, blockLength);
                        origin[j / 4] = BitConverter.ToInt32(block, 0);
                    }

                    max = origin.Max();
                    min = origin.Min();
                    constDen = max - min;
                    audio = new double[origin.Length];
                    for (var j = 0; j < audio.Length; j++) audio[j] = 2 * (origin[j] - min) / constDen - 1;
                    SignalFilter(num, den, audio, out midAudio);
                    resultAudio = new byte[midAudio.Length * 4];
                    for (var j = 0; j < midAudio.Length; j++)
                    {
                        var recoveryAudio = (midAudio[j] + 1) / 2 * (max - min) + min;
                        var tempBytes = BitConverter.GetBytes((int)recoveryAudio);
                        Array.Copy(tempBytes, 0, resultAudio, 4 * j, tempBytes.Length);
                    }
                }
                break;

                #endregion 32位音频流
        }

        return resultAudio;
    }
}

public static class ModulateDemode
{
    /// <summary>
    ///     FM解调
    /// </summary>
    /// <param name="iData">信号I分量</param>
    /// <param name="qData">信号Q分量</param>
    public static int[] FmDeModulate(int[] iData, int[] qData)
    {
        var audio = Array.Empty<int>();
        if (iData.Length == qData.Length)
        {
            var length = qData.Length;
            audio = new int[length];
            audio[0] = 0;
            for (var i = 1; i < length; i++)
            {
                // 归一化模值
                var norm = Math.Sqrt(Math.Pow(iData[i], 2.0) + Math.Pow(qData[i], 2.0));
                audio[i - 1] = (int)((iData[i - 1] * qData[i] - iData[i] * qData[i - 1]) / norm);
            }
        }

        return audio;
    }

    /// <summary>
    ///     变换音频采样率 并解调
    /// </summary>
    /// <param name="data">原数据</param>
    /// <param name="originFs">原采样率</param>
    /// <param name="afterFs">音频采样率</param>
    /// <param name="samplingBits">采样位数</param>
    public static byte[] ChangeSampleRate(int[] data, int originFs, int afterFs, int samplingBits)
    {
        var result = Array.Empty<byte>();
        var realData = Array.Empty<int>();
        if (originFs > afterFs)
            realData = Sample(data, afterFs, originFs);
        else if (originFs < afterFs) realData = InterSample(data, afterFs, originFs);
        if (samplingBits != 8)
        {
            byte[] resTemp;
            switch (samplingBits)
            {
                case 14:
                case 16:
                    result = new byte[realData.Length * 2];
                    for (var i = 0; i < realData.Length; i++)
                    {
                        resTemp = BitConverter.GetBytes(realData[i]);
                        result[2 * i] = resTemp[0];
                        result[2 * i + 1] = resTemp[1];
                    }

                    break;
                case 15:
                    break;
                default:
                    if (samplingBits == 24)
                    {
                        result = new byte[realData.Length * 3];
                        for (var i = 0; i < realData.Length; i++)
                        {
                            resTemp = BitConverter.GetBytes(realData[i]);
                            result[3 * i] = resTemp[0];
                            result[3 * i + 1] = resTemp[1];
                            result[3 * i + 2] = resTemp[2];
                        }
                    }

                    break;
            }
        }
        else
        {
            result = new byte[realData.Length];
            for (var i = 0; i < realData.Length; i++) result[i] = BitConverter.GetBytes(realData[i])[0];
        }

        return result;
    }

    /// <summary>
    ///     计算样本采用多少位进行采样
    /// </summary>
    /// <param name="sampleData"></param>
    public static int GetSampleBits(int[] sampleData)
    {
        var max = sampleData.Max();
        var min = sampleData.Min();
        if (max < Math.Abs(min)) max = Math.Abs(min);
        var wordLen = Math.Log(max * 2, 2.0);
        wordLen = Math.Log(wordLen, 2.0);
        return (int)Math.Pow(2.0, (int)wordLen + 1);
    }

    /// <summary>
    ///     插值采样
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="afterFs"></param>
    /// <param name="originFs"></param>
    public static T[] InterSample<T>(T[] data, int afterFs, int originFs)
    {
        if (originFs <= afterFs) return data;
        return Sample(data, afterFs, originFs);
    }

    private static T[] Sample<T>(T[] data, int afterFs, int originFs)
    {
        T[] sampleData;
        if (originFs > afterFs)
        {
            // 采样比
            var ratio = originFs / (double)afterFs;
            var picLength = (int)(data.Length / ratio) /*+ 1*/;
            var sampData = new T[picLength][];
            sampleData = new T[picLength];
            var picCount = (int)Math.Round(ratio);
            var iNowNum = 0;
            for (var i = 0; i < picLength; i++)
                if (iNowNum + picCount < data.Length)
                {
                    sampData[i] = new T[picCount];
                    Array.Copy(data, iNowNum, sampData[i], 0, picCount);
                    sampleData[i] = sampData[i][sampData[i].Length / 2];
                    iNowNum += picCount;
                }
                else
                {
                    sampData[i] = [];
                    sampleData[i] = sampleData[i - 1];
                }
        }
        else
        {
            sampleData = InterSample(data, afterFs, originFs);
        }

        return sampleData;
    }
}

public static class PsdEstimate
{
    /// <summary>
    ///     自相关法计算功率谱密度
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="data">输入信号</param>
    /// <param name="fs">采样率</param>
    /// <param name="psd">功率谱</param>
    /// <param name="freq">频率</param>
    /// <param name="n">傅里叶点数[128,256,512,1024,2048]</param>
    public static void XcorrPsd<T>(IList<T> data, double fs, out IList<T> psd, out IList<double> freq, int n = 512)
    {
        psd = null;
        freq = null;
        var word = (byte)Math.Log(n, 2);
        if (word is < 7 or > 12) return;
        // 相关结果
        double[] corrResult = null;
        var temp = Array.ConvertAll(data.ToArray(), t => (double)Convert.ChangeType(t, typeof(double)));
        AXcorrAlgorithm.AutoCorr(temp, ref corrResult, XcorrOptions.UnBiased);
        SignalProcess.N = n;
        // 保证结果全是正数  
        var fftResult =
            SignalProcess.NormFft(Array.ConvertAll(corrResult,
                a => float.Parse(a.ToString(CultureInfo.InvariantCulture))));
        // 最大值归一化功率谱
        fftResult = CommonMethods.VectorDivisionConst(fftResult, fftResult.Max());
        // 单位转换
        for (var i = 0; i < fftResult.Length; i++) fftResult[i] = (float)(10 * Math.Log10(fftResult[i] + 1e-6));
        // 功率谱
        psd = Array.ConvertAll(fftResult, f =>
        {
            var t = default(T);
            try
            {
                t = (T)Convert.ChangeType(f, typeof(T));
            }
            catch
            {
                // 忽略异常
            }

            return t;
        });
        // 频率
        freq = new List<double>();
        for (var k = 0; k < n; k++) freq.Add(k * fs / (2 * n));
    }

    /// <summary>
    ///     周期图法谱估计
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="fs"></param>
    /// <param name="psd"></param>
    /// <param name="freq"></param>
    /// <param name="n"></param>
    /// <param name="wintype"></param>
    /// <exception cref="ArgumentNullException"><paramref name="data" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"></exception>
    public static void PeriodOgram<T>(IList<T> data, double fs, out IList<T> psd, out IList<double> freq, int n = 512,
        WindowType wintype = WindowType.RectWin)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        psd = null;
        freq = null;
        var wordLen = (int)Math.Log(n, 2);
        if (Math.Abs(Math.Pow(2, wordLen) - n) > 1e-9 || wordLen < 7 || wordLen > 12)
            throw new ArgumentException("傅里叶点数必须为2的整数幂，且大于等于128小于等于4096!");
        var temp = Array.ConvertAll(data.ToArray(), t => (float)Convert.ChangeType(t, typeof(float)));
        SignalProcess.N = n;
        Array.Resize(ref temp, n * 2);
        var tempWinType = WindowType.RectWin;
        // 加窗
        switch (wintype)
        {
            case WindowType.HammingWin:
                tempWinType = WindowType.HammingWin;
                SignalProcess.HammingWin(ref temp);
                break;
            case WindowType.HanningWin:
                tempWinType = WindowType.HanningWin;
                SignalProcess.HanningWin(ref temp);
                break;
            case WindowType.GaussWin:
                tempWinType = WindowType.GaussWin;
                SignalProcess.GaussWin(ref temp);
                break;
            default:
                SignalProcess.RectangleWin(ref temp);
                break;
        }

        // FFT
        var xReal = new float[n * 2];
        var xImage = new float[n * 2];
        var realFft = new float[n];
        var complexFft = new Complex[n];
        Array.Copy(temp, xReal, temp.Length);
        SignalProcess.Fft(ref xReal, ref xImage);
        for (var i = 0; i < xReal.Length / 2; i++) complexFft[i] = new Complex(xReal[i], xImage[i]);
        // Mod
        var win = FilterDesignBase.Window<double>(tempWinType, temp.Length).ToArray();
        var innerProduct = CommonMethods.VectorsInProduct(win, win);
        for (var i = 0; i < complexFft.Length; i++) complexFft[i] *= Complex.Conjugate(complexFft[i]);
        for (var i = 0; i < complexFft.Length; i++)
        {
            var density = (float)(complexFft[i].Real / innerProduct / fs);
            realFft[i] = (float)(10 * Math.Log10(density + 1e-6));
        }

        // 功率谱
        psd = Array.ConvertAll(realFft, f =>
        {
            var t = default(T);
            try
            {
                t = (T)Convert.ChangeType(f, typeof(T));
            }
            catch
            {
                // 忽略异常
            }

            return t;
        });
        // 频率
        freq = new List<double>();
        for (var k = 0; k < n; k++) freq.Add(k * fs / (2 * n));
    }

    /**
          * The Burg method for AR power spectral estimation现代谱估计
          * x       : input signal
          * n       : number of input signal
          * p       : the AR model order
          * a       : output of coefficients of AR model --- a(0), a(1), ..., a(p)
          * v       : pointer to the variance of exciting white noise
          */
    public static void Burg(double[] x, int p, double[] a)
    {
        if (x == null || p < 0) throw new ArgumentNullException(nameof(x));
        int i;
        int k;
        var n = x.Length;
        var b = new double[p + 1];
        var ef = new double[n];
        var eb = new double[n];
        var v = new[] { 0.0d };
        a[0] = 1.0;
        var r0 = 0.0;
        for (i = 0; i < n; i++) r0 += x[i] * x[i];
        r0 /= n;
        v[0] = r0;
        for (i = 1; i < n; i++)
        {
            ef[i] = x[i];
            eb[i - 1] = x[i - 1];
        }

        for (k = 1; k <= p; k++)
        {
            var sumn = 0.0;
            var sumd = 0.0;
            for (i = k; i < n; i++)
            {
                sumn += ef[i] * eb[i - 1];
                sumd += ef[i] * ef[i] + eb[i - 1] * eb[i - 1];
            }

            a[k] = -2 * sumn / sumd;
            for (i = 1; i < k; i++) b[i] = a[i] + a[k] * a[k - i];
            for (i = 1; i < k; i++) a[i] = b[i];
            v[0] = (1.0 - a[k] * a[k]) * v[0];
            for (i = n - 1; i >= k + 1; i--)
            {
                ef[i] += a[k] * eb[i - 1];
                eb[i - 1] = eb[i - 2] + a[k] * ef[i - 1];
            }
        }
    }
    //
    // * The Bartlett method of power spectral estimation.巴特利特平均图估计法
    // * ak       : AR coefficients
    // * n1       : number of AR coefficients
    // * bk       : MA coefficients
    // * n2       : number of MA coefficients
    // * pxx      : spectral density at L frequencies: w = 0, 2*pi/L, ..., 2*pi(L-1)/L
    // * L        : the points number of pxx
    //// * v        : the variance of exciting white noise
    //
}

/// <summary>
///     http://soundfile.sapp.org/doc/WaveFormat/
/// </summary>
public sealed class WavFile
{
    /// <summary>
    ///     是否第一次写入
    /// </summary>
    private static bool _isFirstWrite = true;

    /// <summary>
    ///     数据拷贝的起始索引
    /// </summary>
    private static int _startIndex;

    /// <summary>
    ///     缓存所有的流
    /// </summary>
    private static byte[] _cacheBytes;

    /// <summary>
    ///     采样率
    /// </summary>
    private static int _sampleRate = 22050;

    /// <summary>
    ///     数据位
    /// </summary>
    private static readonly byte _bits = 16;

    private readonly int _dataLength;

    static WavFile()
    {
        _cacheBytes = [];
    }

    private WavFile(int sampleRateRef, int dataLengthRef)
    {
        _dataLength = dataLengthRef;
        _sampleRate = sampleRateRef;
    }

    /// <summary>
    ///     44字节的wav格式数据头
    /// </summary>
    public byte[] Header()
    {
        var headerData = new byte[13][];
        // "RIFF"
        headerData[0] = [0x52, 0x49, 0x46, 0x46]; // ChunkID
        // 文件长度
        headerData[1] = IntToBytes(36 + _dataLength, true); // ChunkSize
        // "WAVE"
        headerData[2] = [0x57, 0x41, 0x56, 0x45]; // Format
        // "fmt "
        headerData[3] = [0x66, 0x6d, 0x74, 0x20]; // Subchunk1ID    
        // 格式类别 10H为OCM形式的声音数据                       
        headerData[4] = [16, 0, 0, 0]; // Subchunk1Size                         
        headerData[5] = [1, 0]; // AudioFormat
        // 通道数 单声道为1，双声道为2
        headerData[6] = [1, 0]; // NumChannels
        // 采样率（每秒样本数），表示每个通道的播放速度，
        headerData[7] = IntToBytes(_sampleRate, true); // SampleRate
        // 波形音频数据传送速率，其值为通道数×每秒数据位数×每样本的数据位数／8。
        // 播放软件利用此值可以估计缓冲区的大小。
        headerData[8] = IntToBytes(_sampleRate * 2 * 1, true); // ByteRate
        headerData[9] = [(byte)(_bits / 8), 0]; // BlockAlign
        // 数据块的调整数（按字节算的），其值为通道数×每样本的数据位值／8。播放软件需要一次处理多个该值大小的字节数据，以便将其值用于缓冲区的调整。
        headerData[10] = [_bits, 0]; // BitsPerSample
        // 数据标记符 "data"
        headerData[11] = [0x64, 0x61, 0x74, 0x61]; // Subchunk2ID
        headerData[12] = IntToBytes(_dataLength, true); // Subchunk2Size
        var headerBytesLength = 0;
        foreach (var t in headerData)
            headerBytesLength += t.Length;

        var headerBytes = new byte[headerBytesLength];
        var index = 0;
        foreach (var t in headerData)
            foreach (var t1 in t)
            {
                headerBytes[index] = t1;
                index++;
            }

        return headerBytes;
    }

    public static byte[] IntToBytes(int intValue, bool little)
    {
        var intBytes = BitConverter.GetBytes(intValue);
        if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);
        if (little) Array.Reverse(intBytes);
        return intBytes;
    }

    /// <summary>
    ///     将PCM byte[]保存为.wav文件
    /// </summary>
    /// <param name="data">PCM数据流</param>
    /// <param name="path">保存.wav文件的路径</param>
    public static void WriteWav(byte[] data, string path)
    {
        if (string.IsNullOrEmpty(path)) path = $"{Environment.CurrentDirectory}\\audio.wav";
        var raw = new WavFile(_sampleRate, data.Length);
        var headerBytes = raw.Header();
        var total = new byte[headerBytes.Length + data.Length];
        for (var i = 0; i < total.Length; i++)
            if (i < headerBytes.Length)
                total[i] = headerBytes[i];
            else
                total[i] = data[i - headerBytes.Length];
        var fs = new FileStream(path, FileMode.OpenOrCreate);
        lock (fs)
        {
            try
            {
                fs.Write(total, 0, total.Length);
            }
            finally
            {
                fs.Close();
            }
        }

        //File.WriteAllBytes(path, total);
        var read = File.ReadAllBytes(path);
        Debug.WriteLine(read.Length == total.Length
            ? $"save {path}.wav file success!!"
            : $"save {path}.wav file failed!!");
    }

    /// <summary>
    ///     将PCM byte[]保存为.wav文件
    /// </summary>
    /// <param name="data">PCM数据流</param>
    /// <param name="isContinue">是否连续写入</param>
    /// <param name="path">保存.wav文件的路径</param>
    public static void WriteWav(byte[] data, bool isContinue, string path = "")
    {
        _ = isContinue;
        if (string.IsNullOrEmpty(path)) path = $"{Environment.CurrentDirectory}\\audio.wav";
        Array.Resize(ref _cacheBytes, _cacheBytes.Length + data.Length);
        Array.Copy(data, 0, _cacheBytes, _startIndex, data.Length);
        var raw = new WavFile(_sampleRate, _cacheBytes.Length - 36 > 0 ? _cacheBytes.Length - 36 : _cacheBytes.Length);
        var headerBytes = raw.Header();
        if (_isFirstWrite)
        {
            _isFirstWrite = false;
            var temp = _cacheBytes.ToList();
            temp.InsertRange(0, headerBytes);
            _cacheBytes = temp.ToArray<byte>();
        }
        else
        {
            var temp = _cacheBytes.ToList();
            temp.RemoveRange(0, headerBytes.Length);
            temp.InsertRange(0, headerBytes);
            _cacheBytes = temp.ToArray<byte>();
        }

        var fs = new FileStream(path, FileMode.OpenOrCreate);
        lock (fs)
        {
            try
            {
                fs.Write(_cacheBytes, 0, _cacheBytes.Length);
            }
            finally
            {
                fs.Close();
            }
        }

        _startIndex = _cacheBytes.Length;
    }

    public static void Reset()
    {
        _cacheBytes = [];
        _isFirstWrite = true;
        _startIndex = 0;
    }

    /// <summary>
    ///     从.wav文件里面读取音频数据byte[]
    /// </summary>
    /// <param name="path">.wav文件路径</param>
    /// <param name="bits">数据是多少位的</param>
    public static byte[] ReadWav(string path, out byte bits)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(path);
        using var fs = new FileStream(path, FileMode.Open);
        bits = 16;
        byte[] audioData;
        if (path[path.LastIndexOf('.')..].Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
            audioData = new byte[fs.Length];
            _ = fs.Read(audioData, 0, (int)fs.Length);
            bits = audioData[34];
            Array.Copy(audioData, 44, audioData, 0, audioData.Length - 44);
            Array.Resize(ref audioData, audioData.Length - 44);
        }
        else
        {
            audioData = new byte[fs.Length];
            _ = fs.Read(audioData, 0, (int)fs.Length);
        }

        fs.Close();
        return audioData;
    }
}

/// <summary>
///     滤波器类型
/// </summary>
public enum FilterType : byte
{
    /// <summary>
    ///     低通滤波器
    /// </summary>
    LowPass = 0,

    /// <summary>
    ///     高通滤波器
    /// </summary>
    HighPass = 1,

    /// <summary>
    ///     带通滤波器
    /// </summary>
    BandPass = 2,

    /// <summary>
    ///     带阻滤波器
    /// </summary>
    BandStop = 4
}

/// <summary>
///     数字系统的响应类型
/// </summary>
public enum ResponseType
{
    /// <summary>
    ///     脉冲响应
    /// </summary>
    ImpulseResponse,

    /// <summary>
    ///     阶跃响应
    /// </summary>
    StepResponse
}

/// <summary>
///     窗口类型
/// </summary>
public enum WindowType
{
    /// <summary>
    ///     矩形窗
    /// </summary>
    RectWin = 0,

    /// <summary>
    ///     图基窗
    /// </summary>
    TukeyWin = 1,

    /// <summary>
    ///     巴特利特三角窗
    /// </summary>
    BartlettWin = 2,

    /// <summary>
    ///     汉宁窗
    /// </summary>
    HanningWin = 4,

    /// <summary>
    ///     海明窗
    /// </summary>
    HammingWin = 8,

    /// <summary>
    ///     布兰克曼窗
    /// </summary>
    BlackManWin = 16,

    /// <summary>
    ///     凯瑟窗
    /// </summary>
    KaiserWin = 32,

    /// <summary>
    ///     高斯窗
    /// </summary>
    GaussWin = 64
}