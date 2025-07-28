using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Magneto.Device.G33DDC.SDK;

internal static class Helper
{
    /// <summary>
    ///     计算这个数是2的多少次幂
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int Find2Xn(uint value)
    {
        if (value == 0) return 0;
        var n = value;
        var m = 0;
        while (n > 1)
        {
            n = n / 2;
            m++;
        }

        return m;
    }

    public static void GetNormalizedWindowCoeffs(float[] buffer, int count, double fixedCoeff)
    {
        int n;
        var a0 = (1 - 0.16) / 2;
        var a1 = 0.5;
        var a2 = 0.16 / 2;
        double sum = 0;
        for (n = 0; n < count; n++)
        {
            buffer[n] = (float)(a0 - a1 * Math.Cos(2 * Math.PI * n / (count - 1)) +
                                a2 * Math.Cos(4 * Math.PI * n / (count - 1)));
            sum += buffer[n];
        }

        sum = fixedCoeff / sum;
        for (n = 0; n < count; n++) buffer[n] = (float)(buffer[n] * sum);
    }

    public static void Fft(ref float[] data, ulong nn)
    {
        ulong m;
        ulong i;
        // reverse-binary reindexing
        var n = nn << 1;
        ulong j = 1;
        for (i = 1; i < n; i += 2)
        {
            if (j > i)
            {
                var t = data[j - 1];
                data[j - 1] = data[i - 1];
                data[i - 1] = t;
                t = data[j];
                data[j] = data[i];
                data[i] = t;
            }

            m = nn;
            while (m >= 2 && j > m)
            {
                j -= m;
                m >>= 1;
            }

            j += m;
        }

        // here begins the Danielson-Lanczos section
        ulong mmax = 2;
        while (n > mmax)
        {
            var istep = mmax << 1;
            var theta = -(2 * Math.PI / mmax);
            var wtemp = Math.Sin(0.5 * theta);
            var wpr = -2.0 * wtemp * wtemp;
            var wpi = Math.Sin(theta);
            var wr = 1.0;
            var wi = 0.0;
            for (m = 1; m < mmax; m += 2)
            {
                for (i = m; i <= n; i += istep)
                {
                    j = i + mmax;
                    var tempr = wr * data[j - 1] - wi * data[j];
                    var tempi = wr * data[j] + wi * data[j - 1];
                    data[j - 1] = (float)(data[i - 1] - tempr);
                    data[j] = (float)(data[i] - tempi);
                    data[i - 1] = (float)(data[i - 1] + tempr);
                    data[i] = (float)(data[i] + tempi);
                }

                wtemp = wr;
                wr += wr * wpr - wi * wpi;
                wi += wi * wpr + wtemp * wpi;
            }

            mmax = istep;
        }
    }

    public static bool AddSample_16(IntPtr intPtr, ref float[] buffer, ref uint offset, uint count, uint size,
        ref short[] iq)
    {
        short[] samples;
        if (size <= count)
        {
            var start = (int)(count - size) << 1;
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new short[size * 2];
            Marshal.Copy(newIntPtr, samples, 0, (int)size * 2);
            for (var i = 0; i < size; i++)
            {
                buffer[i * 2] = samples[i * 2] / 32768f;
                buffer[i * 2 + 1] = samples[i * 2 + 1] / 32768f;
                iq[i * 2] = samples[i * 2];
                iq[i * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        samples = new short[count * 2];
        Marshal.Copy(intPtr, samples, 0, (int)count * 2);
        var remain = size - offset;
        if (remain <= count)
        {
            var toRemove = count - remain;
            Buffer.BlockCopy(buffer, (int)toRemove * 2 * sizeof(float), buffer, 0,
                (int)(offset - toRemove) * 2 * sizeof(float));
            Buffer.BlockCopy(iq, (int)toRemove * 2 * sizeof(short), iq, 0,
                (int)(offset - toRemove) * 2 * sizeof(short));
            for (var i = 0; i < count; i++)
            {
                var index = offset - toRemove + i;
                buffer[index * 2] = samples[i * 2] / 32768f;
                buffer[index * 2 + 1] = samples[i * 2 + 1] / 32768f;
                iq[index * 2] = samples[i * 2];
                iq[index * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        for (var i = 0; i < count; i++)
        {
            var index = offset + i;
            buffer[index * 2] = samples[i * 2] / 32768f;
            buffer[index * 2 + 1] = samples[i * 2 + 1] / 32768f;
            iq[index * 2] = samples[i * 2];
            iq[index * 2 + 1] = samples[i * 2 + 1];
        }

        offset += count;
        return false;
    }

    public static bool AddSample_32(IntPtr intPtr, ref float[] buffer, ref uint offset, uint count, uint size,
        ref int[] iq)
    {
        int[] samples;
        if (size <= count)
        {
            var start = (int)(count - size) << 1;
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new int[size * 2];
            Marshal.Copy(newIntPtr, samples, 0, (int)size * 2);
            for (var i = 0; i < size; i++)
            {
                buffer[i * 2] = samples[i * 2] / 2147483648.0f;
                buffer[i * 2 + 1] = samples[i * 2 + 1] / 2147483648.0f;
                iq[i * 2] = samples[i * 2];
                iq[i * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        samples = new int[count * 2];
        Marshal.Copy(intPtr, samples, 0, (int)count * 2);
        var remain = size - offset;
        if (remain <= count)
        {
            var toRemove = count - remain;
            Buffer.BlockCopy(buffer, (int)toRemove * 2 * sizeof(float), buffer, 0,
                (int)(offset - toRemove) * 2 * sizeof(float));
            Buffer.BlockCopy(iq, (int)toRemove * 2 * sizeof(int), iq, 0, (int)(offset - toRemove) * 2 * sizeof(int));
            for (var i = 0; i < count; i++)
            {
                var index = offset - toRemove + i;
                buffer[index * 2] = samples[i * 2] / 2147483648.0f;
                buffer[index * 2 + 1] = samples[i * 2 + 1] / 2147483648.0f;
                iq[index * 2] = samples[i * 2];
                iq[index * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        for (var i = 0; i < count; i++)
        {
            var index = offset + i;
            buffer[index * 2] = samples[i * 2] / 2147483648.0f;
            buffer[index * 2 + 1] = samples[i * 2 + 1] / 2147483648.0f;
            iq[index * 2] = samples[i * 2];
            iq[index * 2 + 1] = samples[i * 2 + 1];
        }

        offset += count;
        return false;
    }

    public static bool AddSample_16(IntPtr intPtr, out List<float[]> list, ref uint offset, uint count, uint size,
        out short[] samples)
    {
        list = new List<float[]>();
        samples = new short[size * 2];
        var index = count % size;
        offset += index;
        while (count >= size)
        {
            var start = (int)offset << 1;
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new short[size * 2];
            var buffer = new float[size * 2];
            Marshal.Copy(newIntPtr, samples, 0, (int)size * 2);
            for (var i = 0; i < size; i++)
            {
                buffer[i * 2] = samples[i * 2] / 32768f;
                buffer[i * 2 + 1] = samples[i * 2 + 1] / 32768f;
            }

            offset += size;
            count -= size;
            list.Add(buffer);
        }

        return true;
    }

    public static bool AddSample_32(IntPtr intPtr, out List<float[]> list, ref uint offset, uint count, uint size,
        out int[] samples)
    {
        list = new List<float[]>();
        samples = new int[size * 2];
        var index = count % size;
        offset += index;
        while (count >= size)
        {
            var start = (int)offset << 1;
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new int[size * 2];
            var buffer = new float[size * 2];
            Marshal.Copy(newIntPtr, samples, 0, (int)size * 2);
            for (var i = 0; i < size; i++)
            {
                buffer[i * 2] = samples[i * 2] / 2147483648.0f;
                buffer[i * 2 + 1] = samples[i * 2 + 1] / 2147483648.0f;
            }

            offset += size;
            count -= size;
            list.Add(buffer);
        }

        return true;
    }

    public static bool AddSample_F32(IntPtr intPtr, ref float[] buffer, ref uint offset, uint count, uint size)
    {
        float[] samples;
        if (size <= count)
        {
            var start = (int)(count - size) << 1;
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new float[size * 2];
            Marshal.Copy(newIntPtr, samples, 0, (int)size * 2);
            for (var i = 0; i < size; i++)
            {
                buffer[i * 2] = samples[i * 2];
                buffer[i * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        samples = new float[count * 2];
        Marshal.Copy(intPtr, samples, 0, (int)count * 2);
        var remain = size - offset;
        if (remain <= count)
        {
            var toRemove = count - remain;
            Buffer.BlockCopy(buffer, (int)toRemove * 2 * sizeof(float), buffer, 0,
                (int)(offset - toRemove) * 2 * sizeof(float));
            for (var i = 0; i < count; i++)
            {
                var index = offset - toRemove + i;
                buffer[index * 2] = samples[i * 2];
                buffer[index * 2 + 1] = samples[i * 2 + 1];
            }

            offset = size;
            return true;
        }

        for (var i = 0; i < count; i++)
        {
            var index = offset + i;
            buffer[index * 2] = samples[i * 2];
            buffer[index * 2 + 1] = samples[i * 2 + 1];
        }

        offset += count;
        return false;
    }

    public static bool AddSamples_Real_Mono_F32(IntPtr intPtr, ref float[] buffer, ref uint offset, uint count,
        uint size)
    {
        float[] samples;
        if (size <= count)
        {
            var start = (int)(count - size);
            var newIntPtr = IntPtr.Add(intPtr, start);
            samples = new float[size];
            Marshal.Copy(newIntPtr, samples, 0, (int)size);
            for (var i = 0; i < size; i++) buffer[i] = samples[i];
            offset = size;
            return true;
        }

        samples = new float[count];
        Marshal.Copy(intPtr, samples, 0, (int)count);
        var remain = size - offset;
        if (remain <= count)
        {
            var toRemove = count - remain;
            Buffer.BlockCopy(buffer, (int)toRemove * sizeof(float), buffer, 0,
                (int)(offset - toRemove) * sizeof(float));
            for (var i = 0; i < count; i++)
            {
                var index = offset - toRemove + i;
                buffer[index] = samples[i];
            }

            offset = size;
            return true;
        }

        for (var i = 0; i < count; i++)
        {
            var index = offset + i;
            buffer[index] = samples[i];
        }

        offset += count;
        return false;
    }

    /// <summary>
    ///     从集合中获取匹配的频率
    /// </summary>
    /// <param name="array"></param>
    /// <param name="frequency"></param>
    /// <param name="isGreater">如果没有找到匹配的值，则:true:找比frequency大的最相近的值,false:找比frequency小的最相近的值</param>
    /// <returns></returns>
    public static int FindNearestFrequency(uint[] array, uint frequency, bool isGreater)
    {
        var index = Array.IndexOf(array, frequency);
        if (index >= 0) return index;
        Array.Sort(array);
        if (isGreater)
        {
            for (var i = 0; i < array.Length; i++)
                if (array[i] >= frequency)
                    return i;
        }
        else
        {
            for (var i = array.Length - 1; i >= 0; i--)
                if (array[i] <= frequency)
                    return i;
        }

        return -1;
    }

    public static float GetLevel(float[] data)
    {
        if (data == null || data.Length % 2 != 0) return 0.0f;
        const double epsilon = 1.0E-7d;
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

    public static float GetRmsLevel(float[] data)
    {
        if (data == null || data.Length % 2 != 0) return 0.0f;
        const double epsilon = 1.0E-7d;
        var length = data.Length / 2;
        var sum = 0.0d;
        var increment = 0;
        for (var index = 0; index < length; ++index)
        {
            var real = data[2 * index];
            var image = data[2 * index + 1];
            if (Math.Abs(real - 0.0f) > epsilon || Math.Abs(image - 0.0f) > epsilon)
            {
                sum += real * real + image * image;
                increment++;
            }
        }

        if (increment == 0) return 0.0f;
        return (float)(10 * Math.Log10(sum / increment));
    }

    public static float GetMaxLevel(float[] data)
    {
        if (data == null || data.Length % 2 != 0) return 0.0f;
        const double epsilon = 1.0E-7d;
        var length = data.Length / 2;
        var max = float.MinValue;
        var increment = 0;
        for (var index = 0; index < length; ++index)
        {
            var real = data[2 * index];
            var image = data[2 * index + 1];
            if (Math.Abs(real - 0.0f) > epsilon || Math.Abs(image - 0.0f) > epsilon)
            {
                var lv = 10 * Math.Log10(real * real + image * image);
                if (max < lv) max = (float)lv;
                increment++;
            }
        }

        if (increment == 0) return 0.0f;
        return max;
    }
}