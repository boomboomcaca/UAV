using System;
using System.Linq;

namespace Magneto.Device.XE_HF.Protocols;

/// <summary>
///     此滤波器仅适用于XE音频滤波
/// </summary>
internal class AudioFilter
{
    // 带阻滤波器 2900-3200
    private static readonly double[] _num =
    {
        0.8293825580592, -5.357849993815, 17.99096752295, -39.31616241739,
        61.37780276924, -70.90100546875, 61.37780276924, -39.31616241739,
        17.99096752295, -5.357849993815, 0.8293825580592
    };

    private static readonly double[] _den =
    {
        1, -6.223667592474, 20.12646851801, -42.35166050287,
        63.65155350813, -70.77136644827, 58.95337076984, -36.32687672735,
        15.98460208707, -4.575459020202, 0.6803108174359
    };

    public static byte[] Filter(byte[] data)
    {
        var origin = new short[data.Length / 2];
        for (var j = 0; j < data.Length; j += 2)
            origin[j / 2] = BitConverter.ToInt16(new[] { data[j], data[j + 1] }, 0);
        double max = origin.Max();
        double min = origin.Min();
        var audio = new double[origin.Length];
        for (var j = 0; j < audio.Length; j++) audio[j] = 2 * (origin[j] - min) / (max - min) - 1;
        SignalFilter(audio, out var midAudio);
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

    public static byte[] Filter(short[] data)
    {
        double max = data.Max();
        double min = data.Min();
        var audio = new double[data.Length];
        for (var j = 0; j < audio.Length; j++) audio[j] = 2 * (data[j] - min) / (max - min) - 1;
        SignalFilter(audio, out var midAudio);
        var resultAudio = new byte[midAudio.Length * 2];
        for (var j = 0; j < midAudio.Length; j++)
        {
            var recoveryAudio = (midAudio[j] + 1) / 2 * (max - min) + min;
            var tempBytes = BitConverter.GetBytes((short)recoveryAudio);
            Array.Copy(tempBytes, 0, resultAudio, 2 * j, tempBytes.Length);
        }

        return resultAudio;
    }

    private static void SignalFilter(double[] input, out double[] output)
    {
        var lenNum = _num.Length;
        var lenDen = _den.Length;
        var lenInput = input.Length;
        if (Math.Abs(_den[0] - 1) > 1e-9)
        {
            for (var i = 1; i != lenDen; i++)
                _den[i] /= _den[0];
            for (var i = 1; i != lenNum; i++)
                _num[i] /= _den[0];
            _den[0] = 1;
        }

        int na = lenDen - 1, nb = lenNum - 1;
        var len = na > nb ? na : nb;
        var zi = new double[len];
        double[] zf = null;
        output = new double[lenInput];

        zf = new double[len];
        // 1.
        {
            var a = new double[len + 1];
            Array.Copy(_den, a, lenDen);
            var b = new double[len + 1];
            Array.Copy(_num, b, lenNum);
            var zfLast = new double[len];
            Array.Copy(zi, zfLast, len);
            for (var i = 0; i != lenInput; i++)
            {
                output[i] = _num[0] * input[i] + zfLast[0];
                zf[len - 1] = b[len] * input[i] - a[len] * output[i];
                for (var j = len - 2; j >= 0; j--)
                    zf[j] = b[j + 1] * input[i] + zfLast[j + 1] - a[j + 1] * output[i];
                Array.Copy(zf, zfLast, len);
            }
        }
    }
}