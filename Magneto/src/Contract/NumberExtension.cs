using System;

namespace Magneto.Contract;

public static class NumberExtension
{
    public const float FloatDelta = 1E-06f;
    public const double DoubleDelta = 1E-09;

    /// <summary>
    ///     比较大小
    /// </summary>
    /// <param name="original">原始浮点数</param>
    /// <param name="compare">比较浮点数</param>
    /// <param name="delta">误差</param>
    /// <returns>相等返回true；否则返回false</returns>
    public static bool EqualTo(this double original, double compare, double delta = DoubleDelta)
    {
        return Math.Abs(original - compare) < 1e-9
               || Math.Abs(original - compare) < delta;
    }

    /// <summary>
    ///     比较大小
    /// </summary>
    /// <param name="original">原始浮点数</param>
    /// <param name="compare">比较浮点数</param>
    /// <param name="delta">误差</param>
    /// <returns>相等返回true；否则返回false</returns>
    public static bool EqualTo(this float original, float compare, float delta = FloatDelta)
    {
        return Math.Abs(original - compare) < 1e-9
               || Math.Abs(original - compare) < delta;
    }

    /// <summary>
    ///     比较大小
    /// </summary>
    /// <param name="original">原始浮点数</param>
    /// <param name="compare">比较浮点数</param>
    /// <param name="delta">误差</param>
    /// <returns>相等返回0；原始浮点数大于比较浮点数返回1；原始浮点数小于比较浮点数返回-1</returns>
    public static int CompareWith(this double original, double compare, double delta = DoubleDelta)
    {
        if (original.EqualTo(compare, delta))
            return 0;
        if (original > compare)
            return 1;
        return -1;
    }

    /// <summary>
    ///     比较大小
    /// </summary>
    /// <param name="original">原始浮点数</param>
    /// <param name="compare">比较浮点数</param>
    /// <param name="delta">误差</param>
    /// <returns>相等返回0；原始浮点数大于比较浮点数返回1；原始浮点数小于比较浮点数返回-1</returns>
    public static int CompareWith(this float original, float compare, float delta = FloatDelta)
    {
        if (original.EqualTo(compare, delta))
            return 0;
        if (original > compare)
            return 1;
        return -1;
    }

    /// <summary>
    ///     比较【字符串数值】和【浮点数与倍数的乘积】大小
    /// </summary>
    /// <param name="str">字符串</param>
    /// <param name="number">浮点数</param>
    /// <param name="m">倍数</param>
    /// <returns>相等返回true；否则返回false</returns>
    public static bool IsValueEqual(string str, double number, double m)
    {
        var isLegal = double.TryParse(str, out var value);
        if (!isLegal) return false;
        return number.EqualTo(value * m);
    }
}