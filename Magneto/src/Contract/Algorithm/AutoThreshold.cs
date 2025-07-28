using System;
using System.Linq;

namespace Magneto.Contract.Algorithm;

/// <summary>
///     自动门限封装类
/// </summary>
public class AutoThreshold
{
    #region 构造函数

    private const float InvalidThresholdValue = 99999;

    #endregion

    #region 私有方法

    /// <summary>
    ///     依据扫描数据取得滑窗系数
    /// </summary>
    /// <param name="scandata">扫描数据</param>
    /// <returns>滑窗系数</returns>
    private float[] Windows(float[] scandata)
    {
        var temparray = new float[scandata.Length + _iSmoothingFactor];
        //按照前偶后奇的原则为扫描数据首尾添加数据
        for (var i = 0; i <= _iSmoothingFactor / 2 - 1; i++) temparray[i] = scandata[0];
        Array.Copy(scandata, 0, temparray, _iSmoothingFactor / 2, scandata.Length);
        for (var i = scandata.Length + _iSmoothingFactor / 2; i <= temparray.Length - 1; i++)
            temparray[i] = scandata[^1];
        //计算窗平均值
        for (var i = 0; i <= scandata.Length - 1; i++)
        {
            float sum = 0;
            for (var j = 0; j <= _iSmoothingFactor - 1; j++) sum += temparray[i + j];
            sum /= _iSmoothingFactor;
            //将门限值上加上容差值
            scandata[i] = sum + _iThresholdMargion;
        }

        return scandata;
    }

    #endregion

    #region"变量定义"

    /// <summary>
    ///     私有变量，表示分段数
    /// </summary>
    private int _iSegmentNumber = 200;

    /// <summary>
    ///     私有变量，表示平滑系数
    /// </summary>
    private int _iSmoothingFactor = 1;

    /// <summary>
    ///     私有变量，表示最小电平
    /// </summary>
    private float _fMinLevel = -3;

    /// <summary>
    ///     私有变量，表示容差
    /// </summary>
    private float _iThresholdMargion = 5;

    #endregion

    #region"属性"

    /// <summary>
    ///     门限段数，整型变量；取值范围1～200
    /// </summary>
    public int SegmentNumber
    {
        get => _iSegmentNumber;
        set
        {
            if (value is > 0 and <= 1000000)
            {
                _iSegmentNumber = value;
            }
            else
            {
                _iSegmentNumber = 50;
                var ex = new Exception("门限段数输入值超过允许范围，取值范围是1～200");
                throw ex;
            }
        }
    }

    /// <summary>
    ///     平滑系数，整型变量；
    /// </summary>
    public int SmoothingFactor
    {
        get => _iSmoothingFactor;
        set => _iSmoothingFactor = value < 1 ? 1 : value;
    }

    /// <summary>
    ///     定义门限容差，整型变量；取值范围0～30
    /// </summary>
    public float ThreadsholdMargion
    {
        get => _iThresholdMargion;
        set
        {
            try
            {
                if (value is >= 0 and <= 30)
                    _iThresholdMargion = value;
                // TODO:参数约束
                else if (value < 0)
                    _iThresholdMargion = 0;
                else if (value > 30) _iThresholdMargion = 30;
            }
            catch
            {
                // throw;
            }
        }
    }

    /// <summary>
    ///     最小电平，浮点类型变量；取值范围－100～150dBuV
    /// </summary>
    public float MinSingleLevel
    {
        get => _fMinLevel;
        set
        {
            if (value is >= -100 and <= 150)
                _fMinLevel = value;
            else
                throw new Exception("最小电平输入值超过允许范围，取值范围是-100～150");
        }
    }

    #endregion

    #region"方法"

    /// <summary>
    ///     依据实时扫描数据，取得扫描数据门限值,单位与扫描数据相同
    /// </summary>
    /// <param name="scandata">扫描数据</param>
    /// <returns>对应扫描数据每一个频点的门限数组</returns>
    /// <remarks>用于实时测量门限计算</remarks>
    public float[] GetThreshold(float[] scandata)
    {
        if (scandata == null) return null;
        if (scandata.Length == 0) return [];
        if (_iSegmentNumber > scandata.Length) _iSegmentNumber = scandata.Length;
        var fScanData = new float[scandata.Length]; //存储门限数据
        var averageValuePerSegment = new float[_iSegmentNumber]; //存储每段平均数
        var dataindex = scandata.Length / _iSegmentNumber; //每段包括的数据个数
        float[] tmpArray; //= new float[dataindex];//临时存储每段的数据
        var avgLevel = scandata.Sum(); //整个频段内的平均功率
        avgLevel /= scandata.Length;
        ////////////////////////////////////////////////
        float sum;
        if (scandata.Length % _iSegmentNumber > 0) dataindex++;
        //按设置的段数循环,计算每段的平均值
        for (var i = 0; i <= _iSegmentNumber - 1; i++)
            if (scandata.Length - i * dataindex > dataindex)
            {
                tmpArray = new float[dataindex];
                sum = 0;
                Array.Copy(scandata, i * dataindex, tmpArray, 0, dataindex); //将原始扫描数据拆分成设置的几段
                for (var j = 0; j <= dataindex - 1; j++)
                {
                    //判断每个扫描数据是否小于最小电平
                    if (tmpArray[j] < _fMinLevel) tmpArray[j] = _fMinLevel;
                    sum += tmpArray[j]; //求每段数据的算术和
                }

                averageValuePerSegment[i] = sum / dataindex; //计算每段扫描数据的平均值
                //如果分段平均功率大于整个平均功率，则认为该段是连续的信号;这里还有需要完善的地方，就是NI分段扫描，参考电平不一样
                //会造成识别错误。
                if (avgLevel < averageValuePerSegment[i]) averageValuePerSegment[i] = avgLevel;
                for (var k = i * dataindex; k <= (i + 1) * dataindex - 1; k++) fScanData[k] = averageValuePerSegment[i];
            }
            else
            {
                tmpArray = new float[dataindex];
                Array.Copy(scandata, i * dataindex, tmpArray, 0, scandata.Length - i * dataindex);
                sum = 0;
                for (var j = 0; j <= tmpArray.Length - 1; j++)
                {
                    //判断每个扫描数据是否小于最小电平
                    if (tmpArray[j] < _fMinLevel) tmpArray[j] = _fMinLevel;
                    sum += tmpArray[j]; //求每段数据的算术和
                }

                averageValuePerSegment[i] = sum / tmpArray.Length; //计算每段扫描数据的平均值
                for (var k = i * dataindex; k <= scandata.Length - 1; k++) fScanData[k] = averageValuePerSegment[i];
                break;
            }

        return Windows(fScanData);
    }

    /// <summary>
    ///     依据长期扫描统计数据的电平最大值，提取信号电平
    /// </summary>
    /// <param name="maxlevel">电平最大值</param>
    /// <returns>信号数组</returns>
    /// <remarks>用于后期统计计算</remarks>
    public float[] GetSingal(float[] maxlevel)
    {
        try
        {
            return GetThreshold(maxlevel);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    /// <summary>
    ///     依据长期统计的信号电平，信号电平最大值，提取门限值
    /// </summary>
    /// <param name="level">信号电平值</param>
    /// <param name="levelMax">信号电平最大值</param>
    /// <returns>门限值</returns>
    /// <remarks>用于后期统计计算</remarks>
    public float[] GetThreshold(float[] level, float[] levelMax)
    {
        if (level == null || levelMax == null) return null;
        if (levelMax.Length != level.Length) return null;
        var threshold = new float[level.Length];
        for (var i = 0; i < level.Length; i++)
        {
            threshold[i] = InvalidThresholdValue; //对门限赋初值
            if (levelMax[i] >= level[i]) //提取信号
            {
                var count = 0;
                var n = i - 1;
                threshold[i] = 0;
                //计算信号左右各3个没有信号点的背噪平均功率
                while (n >= 0 && count < 2)
                {
                    if (levelMax[n] < level[n])
                    {
                        threshold[i] += levelMax[n];
                        count++;
                    }

                    n--;
                }

                n = i + 1;
                while (n < level.Length && count < 2)
                {
                    if (levelMax[n] < level[n])
                    {
                        threshold[i] += levelMax[n];
                        count++;
                    }

                    n++;
                }

                threshold[i] /= count;
            }
        }

        return threshold;
    }

    #endregion
}