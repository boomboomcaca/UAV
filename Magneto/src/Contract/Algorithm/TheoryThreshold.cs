// ***************************************************************************************************************************************
//   
//  文件名称:    TheoryThreshold.cs
// 
//  作    者:    王 刚 卫
//   
//  创作日期:    2017-04-27
//   
//  备    注:    自动门限算法类
// *****************************************************************************************************************************************

using System;
using System.Collections.Generic;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     理论门限封装类 采用信号特征计算频段信号的理论门限值
/// </summary>
public class TheoryThreshold
{
    #region 构造函数

    /// <summary>
    ///     构造函数
    /// </summary>
    public TheoryThreshold()
    {
    }

    #endregion

    #region 实例公共方法

    /// <summary>
    ///     依据实时扫描数据 计算理论门限值
    /// </summary>
    /// <param name="scanData">扫描数据</param>
    /// <param name="startFreq">频段起始频率 MHz</param>
    /// <param name="stopFreq">频段结束频率 MHz</param>
    /// <param name="stepFreq">扫描步进KHz</param>
    /// <returns>返回理论计算的门限值</returns>
    //public float[] CalThreshold(float[] scanData, double startFreq, double stopFreq, float stepFreq, ref float[] sn)
    public float[] CalThreshold(float[] scanData, double startFreq, double stopFreq, float stepFreq)
    {
        // 计算门限
        var resultData = Calculate(startFreq, stepFreq, scanData);
        // 加上滑窗函数作为其最终门限值
        var thresholdData = Windows(resultData);
        //// 计算信噪比
        //sn = new float[scanData.Length];
        //for (int i = 0; i <= scanData.Length - 1; i++)
        //{
        //    if (scanData[i] > thresholdData[i])
        //        sn[i] = scanData[i] - thresholdData[i];
        //    else
        //        sn[i] = 0;
        //}
        return thresholdData;
    }

    #endregion

    #region 变量/属性

    private const double R = 50.0d;

    /// <summary>
    ///     获取/设置门限段数 范围 1 ~ 200
    /// </summary>
    public int SegmentNumber { get; set; } = 1;

    private readonly int _smoothingFactor = 10;
    private float _thresholdMargion = 3;

    /// <summary>
    ///     获取/设置门限容差　范围 1 ~ 30
    /// </summary>
    public float ThresholdMargin
    {
        get => _thresholdMargion;
        set
        {
            if (value is > 0 and <= 30)
            {
                _thresholdMargion = value;
            }
            else
            {
                _thresholdMargion = 6;
                throw new Exception("门限容差数据值设置错误，允许的值范围为 1 ~ 30");
            }
        }
    }

    private float _minLevel = -100.0f;

    /// <summary>
    ///     获取/设置 幅度最小值
    /// </summary>
    public float MinLevel
    {
        get => _minLevel;
        set
        {
            if (value is >= -100.0f and <= 150.0f)
            {
                _minLevel = value;
            }
            else
            {
                _minLevel = -100.0f;
                throw new Exception("最小电平值设置错误，允许的值范围为 -100.0f ~ 150.0f");
            }
        }
    }

    #endregion

    #region 类公共方法

    /// <summary>
    ///     dBuV to fW
    /// </summary>
    /// <param name="input"></param>
    public static float Convert_dBuV_fW(float input)
    {
        var v = Math.Pow(10, input / 10 - 9) / R * Math.Pow(10, 12);
        return (float)v;
    }

    /// <summary>
    ///     fW to dBuV
    /// </summary>
    /// <param name="input"></param>
    public static float Convert_fW_dBuV(float input)
    {
        var v = 10 * Math.Log10(input / Math.Pow(10, 12) * R) + 90;
        return (float)v;
    }

    #endregion

    #region 私有方法

    /// <summary>
    ///     FindPeak 计算
    /// </summary>
    /// <param name="startFreq"></param>
    /// <param name="stepFreq"></param>
    /// <param name="scanData"></param>
    private float[] Calculate(double startFreq, float stepFreq, float[] scanData)
    {
        // 波形信息表
        var slWaveShapeInfo = new SortedList<int, WaveShapeInfo>();
        var resultData = new float[scanData.Length];
        // 获取波形信息
        GetWaveShapeInfo(scanData, ref slWaveShapeInfo);
        // 获取信号轮廓
        GetSignalOutLine(slWaveShapeInfo, ref resultData);
        // 获取门限值
        GetThreshold(startFreq, stepFreq, ref resultData);
        return resultData;
    }

    /// <summary>
    ///     GetPeek 获取波形信息
    /// </summary>
    /// <param name="scanData"></param>
    /// <param name="slWaveShapeInfo"></param>
    private void GetWaveShapeInfo(float[] scanData, ref SortedList<int, WaveShapeInfo> slWaveShapeInfo)
    {
        // 波形信息
        var wsInfo = new WaveShapeInfo();
        // 序号
        var iIndex = 0;
        // 搜索起始点
        var iStartPoint = 0;
        var preDiffValue = scanData[0];
        wsInfo.TroughLocationLeft = 0;
        wsInfo.TroughValueLeft = scanData[0];
        slWaveShapeInfo.Clear();
        iStartPoint++;
        float fCurValue;
        if (scanData.Length > 1 && scanData[1] <= scanData[0])
        {
            wsInfo.PeakLocationLeft = 0;
            wsInfo.PeakValue = scanData[0];
            /////// scanData[1] ???
            for (var i = 1; i < scanData.Length - 1; i++)
            {
                if (scanData[i + 1] > scanData[i])
                {
                    fCurValue = scanData[i];
                    if (Math.Abs(fCurValue - preDiffValue) < 1e-9)
                    {
                        wsInfo.TroughLocationLeft = i;
                        break;
                    }

                    wsInfo.TroughLocationRight = i;
                    wsInfo.TroughValueRight = scanData[i];
                    slWaveShapeInfo.Add(iIndex, wsInfo);
                    iIndex++;
                    iStartPoint = i;
                    break;
                }

                if (Math.Abs(scanData[i] - preDiffValue) > 1e-9) preDiffValue = scanData[i];
            }
        }

        for (var i = iStartPoint; i < scanData.Length - 1; i++)
        {
            fCurValue = scanData[i];
            // 查找波峰
            if (fCurValue > scanData[i + 1] && fCurValue > scanData[i - 1])
            {
                // 尖锐波峰
                wsInfo.PeakLocation = i;
                wsInfo.PeakLocationLeft = i;
                wsInfo.PeakLocationRight = i;
                wsInfo.PeakValue = scanData[i];
                if (iIndex > 0)
                {
                    wsInfo.TroughLocationLeft = wsInfo.TroughLocationRight;
                    wsInfo.TroughValueLeft = wsInfo.TroughValueRight;
                }

                for (var j = i; j < scanData.Length - 1; j++)
                {
                    if (scanData[j + 1] > scanData[j])
                    {
                        wsInfo.TroughLocationRight = j;
                        wsInfo.TroughValueRight = scanData[j];
                        iStartPoint = j;
                        i = iStartPoint;
                        slWaveShapeInfo.Add(iIndex, wsInfo);
                        iIndex++;
                        break;
                    }

                    if (j + i == scanData.Length - 1)
                    {
                        wsInfo.TroughLocationRight = j;
                        wsInfo.TroughValueRight = scanData[j];
                        iStartPoint = j;
                        i = iStartPoint;
                        slWaveShapeInfo.Add(iIndex, wsInfo);
                        iIndex++;
                        break;
                    }
                }
            }
            else
            {
                // 平顶峰
                if (fCurValue > scanData[i - 1] && Math.Abs(fCurValue - scanData[i + 1]) < 1e-9)
                    // 平顶峰值的左边位置
                    wsInfo.PeakLocationLeft = i;
                if (Math.Abs(fCurValue - scanData[i - 1]) < 1e-9 && fCurValue > scanData[i + 1])
                {
                    // 平顶峰值的右边位置
                    wsInfo.PeakLocationRight = i;
                    wsInfo.PeakValue = scanData[i];
                    if (iIndex > 0)
                    {
                        wsInfo.TroughLocationLeft = wsInfo.TroughLocationRight;
                        wsInfo.TroughValueLeft = wsInfo.TroughValueRight;
                    }

                    for (var j = i; j < scanData.Length - 1; j++)
                    {
                        if (scanData[j + 1] > scanData[j])
                        {
                            wsInfo.TroughLocationRight = j;
                            wsInfo.TroughValueRight = scanData[j];
                            iStartPoint = j;
                            if (wsInfo.PeakLocationRight - wsInfo.PeakLocationLeft > 1)
                                wsInfo.PeakLocation = wsInfo.PeakLocationLeft +
                                                      (wsInfo.PeakLocationRight - wsInfo.PeakLocationLeft) / 2;
                            else
                                wsInfo.PeakLocation = i;
                            i = iStartPoint;
                            slWaveShapeInfo.Add(iIndex, wsInfo);
                            iIndex++;
                            break;
                        }

                        if (j + i == scanData.Length - 1)
                        {
                            wsInfo.TroughLocationRight = j;
                            wsInfo.TroughValueRight = scanData[j];
                            iStartPoint = j;
                            if (wsInfo.PeakLocationRight - wsInfo.PeakLocationLeft > 1)
                                wsInfo.PeakLocation = wsInfo.PeakLocationLeft +
                                                      (wsInfo.PeakLocationRight - wsInfo.PeakLocationLeft) / 2;
                            else
                                wsInfo.PeakLocation = i;
                            i = iStartPoint;
                            slWaveShapeInfo.Add(iIndex, wsInfo);
                            iIndex++;
                            break;
                        }
                    }
                }
            }

            if (Math.Abs(fCurValue - preDiffValue) > 1e-9) preDiffValue = fCurValue;
        }

        slWaveShapeInfo.TryGetValue(slWaveShapeInfo.Count - 1, out wsInfo);
        if (wsInfo.TroughLocationRight == scanData.Length - 1) return;
        if (scanData[^1] > wsInfo.PeakValue)
        {
            wsInfo.TroughLocationLeft = wsInfo.TroughLocationRight;
            wsInfo.TroughLocationRight = scanData.Length - 1;
            wsInfo.TroughValueLeft = wsInfo.TroughValueRight;
            wsInfo.PeakLocation = scanData.Length - 1;
            wsInfo.PeakLocationLeft = scanData.Length - 1;
            wsInfo.PeakLocationRight = scanData.Length - 1;
            wsInfo.PeakValue = scanData[^1];
            slWaveShapeInfo.Add(iIndex, wsInfo);
        }
        else
        {
            wsInfo.TroughLocationRight = scanData.Length - 1;
            wsInfo.TroughValueRight = scanData[^1];
            wsInfo.PeakLocationRight = scanData.Length - 1;
            wsInfo.PeakLocation = wsInfo.PeakLocationLeft + (wsInfo.PeakLocationRight - wsInfo.PeakLocationLeft) / 2;
            slWaveShapeInfo.Remove(slWaveShapeInfo.Count - 1);
            slWaveShapeInfo.Add(slWaveShapeInfo.Count, wsInfo);
        }
    }

    /// <summary>
    ///     获取信号轮廓
    /// </summary>
    /// <param name="slWaveShapeInfo"></param>
    /// <param name="resultData"></param>
    private void GetSignalOutLine(SortedList<int, WaveShapeInfo> slWaveShapeInfo, ref float[] resultData)
    {
        var wsInfo1 = new WaveShapeInfo();
        try
        {
            for (var i = 0; i < slWaveShapeInfo.Count - 1; i++)
            {
                slWaveShapeInfo.TryGetValue(i, out var wsInfo);
                if (i == 0 && wsInfo.PeakLocationLeft != 0)
                    for (var j = 0; j < wsInfo.PeakLocationLeft; j++)
                        resultData[j] = wsInfo.TroughValueLeft +
                                        j * (wsInfo.PeakValue - wsInfo.TroughValueLeft) / wsInfo.PeakLocationLeft;
                slWaveShapeInfo.TryGetValue(i + 1, out wsInfo1);
                if (wsInfo.PeakLocationLeft != wsInfo.PeakLocationRight)
                    for (var j = wsInfo.PeakLocationLeft; j <= wsInfo.PeakLocationRight; j++)
                        resultData[j] = wsInfo.PeakValue;
                for (var j = wsInfo.PeakLocationRight; j <= wsInfo1.PeakLocationLeft; j++)
                    resultData[j] = wsInfo.PeakValue + (j - wsInfo.PeakLocationRight) *
                        (wsInfo1.PeakValue - wsInfo.PeakValue) / (wsInfo1.PeakLocationLeft - wsInfo.PeakLocationRight);
            }

            for (var j = wsInfo1.PeakLocationLeft; j <= wsInfo1.TroughLocationRight; j++)
                resultData[j] = wsInfo1.PeakValue;
        }
        catch
        {
            // ignored
        }
    }

    /// <summary>
    ///     由于频段信号特性，分段获取其门限
    /// </summary>
    /// <param name="startFreq"></param>
    /// <param name="stepFreq"></param>
    /// <param name="resultData"></param>
    private void GetThreshold(double startFreq, float stepFreq, ref float[] resultData)
    {
        // 按信号分成7段 存每段的平均值
        var values = new float[7];
        // 存每段的累加次数
        var times = new int[7];
        // 分成7段，计算每段的累加次和每段的值的和
        for (var i = 0; i < resultData.Length; i++)
        {
            var freq = startFreq + i * (stepFreq / 1000);
            int segmentNo;
            if (freq <= 76.0d)
                segmentNo = 0;
            else if (freq is > 76.0d and <= 118.0d)
                segmentNo = 1;
            else if (freq is > 118.0d and <= 167.0d)
                segmentNo = 2;
            else if (freq is > 167.0d and <= 235.0d)
                segmentNo = 3;
            else if (freq is > 235.0d and <= 335.0d)
                segmentNo = 4;
            else if (freq is > 335.0d and < 400)
                segmentNo = 5;
            else
                segmentNo = 6;
            values[segmentNo] += resultData[i];
            times[segmentNo]++;
        }

        // 分成7段，求每段的平均值
        for (var i = 0; i < 7; i++)
            if (times[i] != 0)
                values[i] /= times[i];
        // 分成7段，计算每个频率点的门限值
        for (var i = 0; i < resultData.Length; i++)
        {
            var freq = startFreq + i * (stepFreq / 1000);
            int segmentNo;
            if (freq <= 76.0d)
                segmentNo = 0;
            else if (freq is > 76.0d and <= 118.0d)
                segmentNo = 1;
            else if (freq is > 118.0d and <= 167.0d)
                segmentNo = 2;
            else if (freq is > 167.0d and <= 235.0d)
                segmentNo = 3;
            else if (freq is > 235.0d and <= 335.0d)
                segmentNo = 4;
            else if (freq is > 335.0d and < 400)
                segmentNo = 5;
            else
                segmentNo = 6;
            if (resultData[i] > values[segmentNo])
                resultData[i] = (float)(values[segmentNo] + (resultData[i] - values[segmentNo]) * 0.1);
            resultData[i] += _thresholdMargion;
        }
    }

    /// <summary>
    ///     加窗
    ///     返回平滑窗口系数
    /// </summary>
    /// <param name="data"></param>
    private float[] Windows(float[] data)
    {
        // 平滑窗口宽度
        _ = new float[_smoothingFactor];
        var tempArray = new float[data.Length + _smoothingFactor];
        var tempValue = new float[data.Length];
        // 按照前偶后奇的原则为扫描数据首尾添加数据
        for (var i = 0; i <= _smoothingFactor / 2 - 1; i++) tempArray[i] = data[0];
        Array.Copy(data, 0, tempArray, _smoothingFactor / 2, data.Length);
        for (var i = data.Length + _smoothingFactor / 2; i <= tempArray.Length - 1; i++) tempArray[i] = data[^1];
        // 计算窗平均值
        for (var i = 0; i <= data.Length - 1; i++)
        {
            float sum = 0;
            for (var j = 0; j <= _smoothingFactor - 1; j++) sum += tempArray[i + j];
            sum /= _smoothingFactor;
            // 把门限值加上容差值
            tempValue[i] = sum;
        }

        return tempValue;
    }

    #endregion
}

/// <summary>
///     波形信息
/// </summary>
public struct WaveShapeInfo
{
    /// <summary>
    ///     波峰中心位置
    /// </summary>
    public int PeakLocation { get; set; }

    /// <summary>
    ///     波峰为平顶时，波峰的左边位置
    /// </summary>
    public int PeakLocationLeft { get; set; }

    /// <summary>
    ///     波峰为平顶时，波峰的右边位置
    /// </summary>
    public int PeakLocationRight { get; set; }

    /// <summary>
    ///     波峰值
    /// </summary>
    public float PeakValue { get; set; }

    /// <summary>
    ///     波谷左边位置
    /// </summary>
    public int TroughLocationLeft { get; set; }

    /// <summary>
    ///     波谷右边位置
    /// </summary>
    public int TroughLocationRight { get; set; }

    /// <summary>
    ///     左边波谷值
    /// </summary>
    public float TroughValueLeft { get; set; }

    /// <summary>
    ///     右边波谷值
    /// </summary>
    public float TroughValueRight { get; set; }
}