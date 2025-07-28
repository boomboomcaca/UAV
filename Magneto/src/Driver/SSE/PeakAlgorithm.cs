using System.Collections.Generic;
using System.Linq;

namespace Magneto.Driver.SSE;

public static class PeakAlgorithm
{
    public static List<int> GetPeak(float[] data, int count)
    {
        var yAxis = data.ToList();
        // 峰距
        var disPeak = 3;
        // 峰距为3时得到的脚标
        var index = GetPeaksIndex(TrendSign(OneDiff(yAxis)));
        // 已进行的判断
        var level = 1;
        // 扩大峰距范围范围算法
        while (disPeak > level)
        {
            level++;
            var result = DoPeakInstance(yAxis, index, level);
            index = result;
        }

        // 获取两侧满足条件的边峰序列
        // index = GetBothSidePeakIndex(xAxis, yAxis, 3, index);
        var minFz = 10.0f;
        // 根据最小峰值序列进行筛选
        index = FindMinPeakValue(minFz, yAxis, index);
        var purge = new List<int>();
        foreach (var t in index)
        {
            // 需要先去重再获取
            if (purge.Contains(t)) continue;
            purge.Add(t);
        }

        Sort(purge, yAxis);
        return purge.Take(count).ToList();
    }

    /// <summary>
    ///     第一次寻峰（基本峰距为1）算法
    /// </summary>
    /// <param name="data"></param>
    private static float[] OneDiff(List<float> data)
    {
        var result = new float[data.Count];
        for (var i = 0; i < result.Length; i++)
            if (i == result.Length - 1)
                result[i] = data[0] - data[i];
            else
                result[i] = data[i + 1] - data[i];
        return result;
    }

    private static int[] TrendSign(float[] data)
    {
        var sign = new int[data.Length];
        for (var i = 0; i < sign.Length; i++)
            if (data[i] > 0)
                sign[i] = 1;
            else if (data[i] == 0)
                sign[i] = 0;
            else
                sign[i] = -1;
        for (var i = sign.Length - 1; i >= 0; i--)
            if (sign[i] == 0)
            {
                if (i == sign.Length - 1)
                {
                    if (sign[0] >= 0)
                        sign[i] = 1;
                    else
                        sign[i] = -1;
                }
                else if (sign[i + 1] >= 0)
                {
                    sign[i] = 1;
                }
                else
                {
                    sign[i] = -1;
                }
            }

        return sign;
    }

    private static List<int> GetPeaksIndex(int[] diff)
    {
        var data = new List<int>();
        for (var i = 0; i != diff.Length; i++)
            if (i == diff.Length - 1)
            {
                if (diff[0] - diff[i] == -2) data.Add(i + 1);
            }
            else if (diff[i + 1] - diff[i] == -2)
            {
                data.Add(i + 1);
            }

        return data; //相当于原数组的下标
    }

    /// <summary>
    ///     扩大寻峰范围算法
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    /// <param name="level"></param>
    private static List<int> DoPeakInstance(List<float> data, List<int> index, int level)
    {
        //相当于原数组的下标
        var result = new List<int>();
        foreach (var t in index)
        {
            //判断是否超出下界和上界
            // if (index[i] - level >= 0 && index[i] + level < data.Count)
            var num = t;
            if (num >= data.Count) num -= data.Count;
            var num1 = t + level;
            var num2 = t - level;
            if (num1 >= data.Count) num1 -= data.Count;
            if (num2 < 0) num2 += data.Count;
            if (data[num1] <= data[num] && data[num2] <= data[num]) result.Add(num);
        }

        return result;
    }

    /// <summary>
    ///     获取两侧满足条件的边峰序列
    /// </summary>
    /// <param name="xAxis"></param>
    /// <param name="yAxis"></param>
    /// <param name="fj"></param>
    /// <param name="index"></param>
    private static List<int> GetBothSidePeakIndex(List<float> xAxis, List<float> yAxis, int fj, List<int> index)
    {
        //获取数据首尾两侧最大峰值（0，FJ）点序和（Date.CountFJ-FJ,Data.Count）点序
        var topIndex = 0;
        var bottomIndex = yAxis.Count - 1;
        for (var i = 0; i < fj; i++)
        {
            if (yAxis[i] >= yAxis[topIndex]) topIndex = i;
            if (yAxis[yAxis.Count - 1 - i] >= yAxis[bottomIndex]) bottomIndex = yAxis.Count - 1 - i;
        }

        //判断是否满足条件检索条件
        var newTopIndex = topIndex;
        var newBottomIndex = bottomIndex;
        for (var i = 0; i <= fj; i++)
        {
            if (yAxis[topIndex + i] >= yAxis[topIndex]) newTopIndex = topIndex + i;
            if (yAxis[bottomIndex - i] >= yAxis[bottomIndex]) newBottomIndex = bottomIndex - i;
        }

        topIndex = newTopIndex;
        bottomIndex = newBottomIndex;
        //添加到结果序列
        if (topIndex <= fj && topIndex != 0) index.Insert(0, topIndex);
        if (bottomIndex >= bottomIndex - fj && bottomIndex != xAxis.Count - 1) index.Add(bottomIndex);
        return index;
    }

    /// <summary>
    ///     根据最小峰值序列进行筛选
    /// </summary>
    /// <param name="minFz"></param>
    /// <param name="yAxis"></param>
    /// <param name="index"></param>
    private static List<int> FindMinPeakValue(float minFz, List<float> yAxis, List<int> index)
    {
        var finalresult = new List<int>();
        foreach (var t in index)
            if (yAxis[t] >= minFz)
                finalresult.Add(t);

        return finalresult;
    }

    private static void Sort(List<int> index, List<float> yAxis)
    {
        for (var i = 0; i < index.Count; i++)
        for (var j = i + 1; j < index.Count; j++)
            if (yAxis[index[i]] < yAxis[index[j]])
                (index[i], index[j]) = (index[j], index[i]);
    }
}