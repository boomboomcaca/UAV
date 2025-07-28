/*********************************************************************************************
 *
 * 文件名称:    ...Tracker800\Client\Source\DCComponent\Commons\DC.Commons.Algorithm\AXcorrAlgorithm\AXcorrAlgorithm.cs
 *
 * 作    者:    jacberg
 *
 * 创作日期:    2017/08/03
 *
 * 修    改:    无
 *
 * 备    注:	相关运算，包含自相关、互相关，协方差矩阵
 *
 *********************************************************************************************/

using System;
using System.Numerics;

#pragma warning disable 1717
namespace Magneto.Contract.Algorithm;

/// <summary>
///     相关运算的实现（自相关、互相关），包含复数，实数，有偏，无偏，归一化
/// </summary>
public static class AXcorrAlgorithm
{
    /*
     * Description : Calculate the autocorrelation of input sequence,default is not biased or unbiased
     * Parameter   : discreteSeq - input sequence with length seq_length and type float
     * 				 autoCorr    - output sequence with type float,the autocorrelation of discrete_seq
     * 				 seq_length   - the length of input sequence
     * */
    /// <summary>
    ///     计算输入序列的自相关序列，默认无偏
    /// </summary>
    /// <param name="discreteSeq">输入的double序列</param>
    /// <param name="autoCorr">输入序列discrete_seq的自相关序列</param>
    public static void AutoCorr(double[] discreteSeq, ref double[] autoCorr)
    {
        var seqLength = discreteSeq.Length;
        for (var m = 0; m < seqLength; m++)
        {
            double temp = 0;
            for (var n = 0; n <= seqLength - 1 - m; n++) temp += discreteSeq[n + m] * discreteSeq[n];
            if (m == 0)
            {
                autoCorr[seqLength - 1] = temp;
            }
            else
            {
                autoCorr[seqLength - 1 + m] = temp;
                autoCorr[seqLength - 1 - m] = temp;
            }
        }
    }

    /*
     * Description : Calculate the autocorrelation of input sequence,default is not biased or unbiased
     * Parameter   : discreteSeq - input sequence with length seq_length and type float
     * 				 autoCorr    - output sequence with type float,the autocorrelation of discrete_seq
     * 				 seq_length   - the length of input sequence
     * 				 option       - specifies a normalization option for the cross-correlation,
     * 				 				'biased': Biased estimate of the autocorrelation function
     * 				 				'unbiased': Unbiased estimate of the autocorrelation function
     * */
    /// <summary>
    ///     计算输入序列的自相关，最后一个参数可以设置“有偏”还是“无偏”
    /// </summary>
    /// <param name="discreteSeq">输入double序列</param>
    /// <param name="autoCorr">自相关的输出结果</param>
    /// <param name="option">有偏和无偏的选项，参数设置：biased，unbiased</param>
    public static void AutoCorr(double[] discreteSeq, ref double[] autoCorr, XcorrOptions option)
    {
        var seqLength = discreteSeq.Length;
        if (autoCorr == null) autoCorr = new double[discreteSeq.Length * 2 - 1];
        if (option == XcorrOptions.UnBiased)
        {
            for (var m = 0; m < seqLength; m++)
            {
                double temp = 0;
                for (var n = 0; n <= seqLength - 1 - m; n++) temp += discreteSeq[n + m] * discreteSeq[n];
                temp /= seqLength - m;
                if (m == 0)
                {
                    autoCorr[seqLength - 1] = temp;
                }
                else
                {
                    autoCorr[seqLength - 1 + m] = temp;
                    autoCorr[seqLength - 1 - m] = temp;
                }
            }

            Array.Reverse(autoCorr);
        }
        else if (option == XcorrOptions.Biased)
        {
            for (var m = 0; m < seqLength; m++)
            {
                double temp = 0;
                for (var n = 0; n <= seqLength - 1 - m; n++) temp += discreteSeq[n + m] * discreteSeq[n];
                temp /= seqLength;
                if (m == 0)
                {
                    autoCorr[seqLength - 1] = temp;
                }
                else
                {
                    autoCorr[seqLength - 1 + m] = temp;
                    autoCorr[seqLength - 1 - m] = temp;
                }
            }

            Array.Reverse(autoCorr);
        }
        else
        {
            throw new Exception("Invalid Arithmetic Mode option:Must be biased or unbiased");
        }
    } //end function

    /*
     * Description : Calculate the autocorrelation of input sequence,default is not biased or unbiased
     * Parameter   : discreteSeq - input sequence with length seq_length and type complex<float>
     * 				 autoCorr    - output sequence with type complex<float>,the autocorrelation
     * 				 				of discrete_seq
     * 				 seq_length   - the length of input sequence
     * */
    /// <summary>
    ///     计算复数序列的自相关
    /// </summary>
    /// <param name="discreteSeq">输入的复数序列</param>
    /// <param name="autoCorr">返回的输入复数序列的自相关序列</param>
    public static void AutoCorrComplex(Complex[] discreteSeq, ref Complex[] autoCorr)
    {
        var seqLength = discreteSeq.Length;
        for (var m = 0; m < seqLength; m++)
        {
            Complex temp = 0;
            for (var n = 0; n <= seqLength - 1 - m; n++) temp += discreteSeq[n + m] * Complex.Conjugate(discreteSeq[n]);
            if (m == 0)
            {
                autoCorr[seqLength - 1] = temp;
            }
            else
            {
                autoCorr[seqLength - 1 + m] = Complex.Conjugate(temp);
                autoCorr[seqLength - 1 - m] = temp;
            }
        }

        Array.Reverse(autoCorr);
    }

    /*
     * Description : Calculate the autocorrelation of input sequence,default is not biased or unbiased
     * Parameter   : discreteSeq - input sequence with length seq_length and type complex<float>
     * 				 autoCorr    - output sequence with type complex<float>,the autocorrelation
     * 				 				of discrete_seq
     * 				 seq_length   - the length of input sequence
     * 				 option       - specifies a normalization option for the cross-correlation,
     * 				 				'biased': Biased estimate of the autocorrelation function
     * 				 				'unbiased': Unbiased estimate of the autocorrelation function
     * */
    /// <summary>
    ///     返回输入复数序列的自相关序列，可以设置“有偏”和“无偏”
    /// </summary>
    /// <param name="discreteSeq">输入的复数序列</param>
    /// <param name="autoCorr">返回的自相关复数序列</param>
    /// <param name="option">设置选项：biased，unbiased</param>
    public static void AutoCorrComplex(Complex[] discreteSeq, ref Complex[] autoCorr, XcorrOptions option)
    {
        var seqLength = discreteSeq.Length;
        if (option == XcorrOptions.UnBiased)
        {
            for (var m = 0; m < seqLength; m++)
            {
                Complex temp = 0;
                for (var n = 0; n <= seqLength - 1 - m; n++)
                    temp += discreteSeq[n + m] * Complex.Conjugate(discreteSeq[n]);
                temp /= seqLength - m;
                if (m == 0)
                {
                    autoCorr[seqLength - 1] = temp;
                }
                else
                {
                    autoCorr[seqLength - 1 + m] = Complex.Conjugate(temp);
                    autoCorr[seqLength - 1 - m] = temp;
                }
            }

            Array.Reverse(autoCorr);
        }
        else if (option == XcorrOptions.Biased)
        {
            for (var m = 0; m < seqLength; m++)
            {
                Complex temp = 0;
                for (var n = 0; n <= seqLength - 1 - m; n++)
                    temp += discreteSeq[n + m] * Complex.Conjugate(discreteSeq[n]);
                temp /= seqLength;
                if (m == 0)
                {
                    autoCorr[seqLength - 1] = temp;
                }
                else
                {
                    autoCorr[seqLength - 1 + m] = Complex.Conjugate(temp);
                    autoCorr[seqLength - 1 - m] = temp;
                }
            }

            Array.Reverse(autoCorr);
        }
        else
        {
            throw new Exception("Invalid Arithmetic Mode option:Must be 'biased' or 'unbiased'!");
        }
    }

    /*
     * Description : Calculate the cross-correlation of input sequences,default is not biased or unbiased
     * Parameter   : discrete_seq_x - first input sequence with type float
     * 				 discrete_seq_y - second input sequence with type float
     * 				 cross_corr     - output sequence with type float,the cross-correlation of input sequences
     *
     * */
    /// <summary>
    ///     返回两个double实序列的互相关序列
    /// </summary>
    /// <param name="discreteSeqX">输入序列1</param>
    /// <param name="discreteSeqY">输入序列2</param>
    /// <param name="crossCorr">返回的互相关序列结果</param>
    public static void CrossCorr(double[] discreteSeqX, double[] discreteSeqY, ref double[] crossCorr)
    {
        /*
         * Compare the length of the input sequences,if they were not equal,insert some zeros
         * at the end of the shorter one
         * */
        var seqLengthX = discreteSeqX.Length;
        var seqLengthY = discreteSeqY.Length;
        var seqLength = Math.Max(seqLengthX, seqLengthY);
        Console.WriteLine(seqLength);
        var tempSeqX = new double[seqLength];
        var tempSeqY = new double[seqLength];
        for (var i = 0; i < seqLength; i++)
        {
            if (i < seqLengthX)
                tempSeqX[i] = discreteSeqX[i];
            else
                tempSeqX[i] = 0;
            if (i < seqLengthY)
                tempSeqY[i] = discreteSeqY[i];
            else
                tempSeqY[i] = 0;
        }

        //Calculate cross-correlation
        for (var m = 0; m < 2 * seqLength - 1; m++)
        {
            double temp = 0;
            // Corresponding to the index of cross-corr under zero
            if (m < seqLength - 1)
                for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                    temp += tempSeqX[n] * tempSeqY[n + (seqLength - 1 - m)];
            // Calculate the cross-correlation
            else
                for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                    temp += tempSeqX[n + (m - (seqLength - 1))] * tempSeqY[n];
            crossCorr[m] = temp;
        }
    }

    /*
     * Description : Calculate the cross-correlation of input sequences
     * Parameter   : discreteSeqX - first input sequence with type float
     * 				 discreteSeqY - second input sequence with type float
     * 				 crossCorr     - float output sequence,the cross-correlation of input sequences
     * 				 seq_length_x   - the length of discrete_seq_x
     * 				 seq_length_y   - the length of discrete_seq_y
     * 				 option         - specifies a normalization option for the cross-correlation,
     * 				 				  'biased'  : Biased estimate of the autocorrelation function
     * 				 				  'unbiased': Unbiased estimate of the autocorrelation function
     *
     * */
    /// <summary>
    ///     返回两个double实序列的互相关序列，可以设置“有偏”和“无偏”
    /// </summary>
    /// <param name="discreteSeqX">输入序列1</param>
    /// <param name="discreteSeqY">输入序列2</param>
    /// <param name="crossCorr">返回的互相关序列结果</param>
    /// <param name="option">选项设置：biased，unbiased，coeff</param>
    public static void CrossCorr(double[] discreteSeqX, double[] discreteSeqY, ref double[] crossCorr,
        XcorrOptions option)
    {
        /*
         * Compare the length of the input sequences,if they were not equal,insert some zeros
         * at the end of the shorter one
         * */
        var xLength = discreteSeqX.Length;
        var yLength = discreteSeqY.Length;
        var seqLength = Math.Max(xLength, yLength);
        var tempX = new double[seqLength];
        var tempY = new double[seqLength];
        for (var i = 0; i < seqLength; i++)
        {
            if (i < xLength)
                tempX[i] = discreteSeqX[i];
            else
                tempX[i] = 0;
            if (i < yLength)
                tempY[i] = discreteSeqY[i];
            else
                tempY[i] = 0;
        }

        //Calculate cross-correlation
        if (option == XcorrOptions.UnBiased)
        {
            for (var m = 0; m < 2 * seqLength - 1; m++)
            {
                double temp = 0;
                // Corresponding to the index of cross-corr under zero
                if (m < seqLength - 1)
                    for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                        temp += tempX[n] * tempY[n + (seqLength - 1 - m)];
                // Calculate the cross-correlation
                else
                    for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                        temp += tempX[n + (m - (seqLength - 1))] * tempY[n];
                temp /= m < seqLength - 1 ? m + 1 : 2 * seqLength - 1 - m;
                crossCorr[m] = temp;
            }
        }
        else if (option == XcorrOptions.Biased)
        {
            for (var m = 0; m < 2 * seqLength - 1; m++)
            {
                double temp = 0;
                // Corresponding to the index of cross-corr under zero
                if (m < seqLength - 1)
                    for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                        temp += tempX[n] * tempY[n + (seqLength - 1 - m)];
                // Calculate the cross-correlation
                else
                    for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                        temp += tempX[n + (m - (seqLength - 1))] * tempY[n];
                temp /= seqLength;
                crossCorr[m] = temp;
            }
        }
        else if (option == XcorrOptions.Coeff)
        {
            //discrete_seq_x = Normlize(discrete_seq_x);
            //discrete_seq_y = Normlize(discrete_seq_y);
            CrossCorr(discreteSeqX, discreteSeqY, ref crossCorr, XcorrOptions.Biased);
            // 归一化到[-1,1]
            CommonMethods.Normalize(ref crossCorr);
            //Normlize(crossCorr);
        }
        else
        {
            throw new Exception("Invalid Arithmetic Mode option:Must be 'biased' or 'unbiased'!");
        }
    }

    /// <summary>
    ///     计算矩阵的协方差矩阵或者相关矩阵
    ///     协方差自动以矩阵的列作为信号矢量进行运算
    /// </summary>
    /// <param name="matrix">输入矩阵</param>
    /// <param name="isCorrCov">返回相关矩阵还是协方差矩阵</param>
    /// <returns>返回矩阵的协方差矩阵（按矩阵的列向量）</returns>
    public static double[,] CovCorrMatrix(double[,] matrix, bool isCorrCov)
    {
        var row = matrix.GetLength(0);
        var column = matrix.GetLength(1);
        var cov = new double[column, column];
        var meanColumn = new double[column];
        // standardize data
        if (isCorrCov)
            for (var i = 0; i < column; i++)
            {
                // 计算列的期望
                var oneColumn = CommonMethods.GetOneColumn(matrix, i);
                var tempAve = CommonMethods.Mean(oneColumn);
                var columnStd = Math.Sqrt(CommonMethods.StdSigma(oneColumn));
                for (var j = 0; j < oneColumn.Length; j++)
                {
                    matrix[j, i] -= tempAve;
                    if (columnStd != 0)
                        matrix[j, i] /= columnStd;
                    else
                        matrix[j, i] = double.NaN;
                }
            }

        // Compute and remove mean          
        double tmpSum = 0;
        for (var i = 0; i < column; i++)
        {
            for (var j = 0; j < row; j++) tmpSum += matrix[j, i];
            meanColumn[i] = tmpSum / row;
            tmpSum = 0;
        }

        double newSum = 0;
        for (var i = 0; i < column; i++)
        for (var j = 0; j < column; j++)
        {
            for (var k = 0; k < row; k++) newSum += (matrix[k, i] - meanColumn[i]) * (matrix[k, j] - meanColumn[j]);
            cov[i, j] = newSum / (row - 1);
            newSum = 0;
        }

        #region interpretation

        /*
        if (!is_zero_mean)
        {
            // Compute and remove mean
            for (int i = 0; i < d; i++)
            {
                // 计算列的期望
                tmp = CommonMethods.GetOneColumn(X, i);
                double tempAve = CommonMethods.Mean(tmp);
                for (int j = 0; j < tmp.Length; j++)
                {
                    X[j, i] -= tempAve;
                }
            }
            // Calc corr matrix
            for (int i = 0; i < d; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        R[i, j] += m2[k, i] * m2[k, j];
                    }
                    R[j, i] = R[i, j]; // When i=j this is unnecassary work
                }
            }
        }
        else
        {
            // Calc corr matrix
            for (int i = 0; i < d; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        R[i, j] += X[k, i] * X[k, j];
                    }
                    R[j, i] = R[i, j]; // When i=j this is unnecassary work
                }
            }
        }
        for (int i = 0; i < d; i++)
        {
            for (int j = 0; j < d; j++)
            {
                R[i, j] /= n;
            }
        }
        /*
        n = v[0].size();
        double* mean_col = new double[n];//计算列的期望
        double tmpSum = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
                tmpSum += v[j][i];
            mean_col[i] = tmpSum / m;
            tmpSum = 0;
        }
        cout << "n=" << n << endl;
        double** cov = new double*[n];
        for (int i = 0; i < n; i++)
            cov[i] = new double[n];
        double newSum = 0;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < m; k++)
                    newSum += (v[k][i] - mean_col[i]) * (v[k][j] - mean_col[j]);
                cov[i][j] = newSum / (m - 1);
                newSum = 0;
            }
        }
        cout << "协方差矩阵的计算结果为：" << endl;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                cout << cov[i][j] << "  ";
            cout << endl;
        }
        **/

        #endregion interpretation

        return cov;
    }

    /*
     * Description : Calculate the cross-correlation of input sequences,default is not biased or unbiased
     * Parameter   : discreteSeqX - first input sequence with type float
     * 				 discreteSeqY - second input sequence with type float
     * 				 crossCorr    - output sequence with type float,the cross-correlation of input sequences
     *
     * */
    /// <summary>
    ///     返回两个复数序列的互相关序列
    /// </summary>
    /// <param name="discreteSeqX">输入复数序列1</param>
    /// <param name="discreteSeqY">输入复数序列2</param>
    /// <param name="crossCorr">返回的互相关复数序列结果</param>
    public static void CrossCorrComplex(Complex[] discreteSeqX, Complex[] discreteSeqY, ref Complex[] crossCorr)
    {
        /*
         * Compare the length of the input sequences,if they were not equal,insert some zeros
         * at the end of the shorter one
         * */
        var seqLengthX = discreteSeqX.Length;
        var seqLengthY = discreteSeqY.Length;
        var seqLength = Math.Max(seqLengthX, seqLengthY);
        var tempSeqX = new Complex[seqLength];
        var tempSeqY = new Complex[seqLength];
        for (var i = 0; i < seqLength; i++)
        {
            if (i < seqLengthX)
                tempSeqX[i] = discreteSeqX[i];
            else
                tempSeqX[i] = 0;
            if (i < seqLengthY)
                tempSeqY[i] = discreteSeqY[i];
            else
                tempSeqY[i] = 0;
        }

        //Calculate cross-correlation
        for (var m = 0; m < 2 * seqLength - 1; m++)
        {
            Complex temp = 0;
            // Corresponding to the index of cross-corr under zero
            if (m < seqLength - 1)
            {
                for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                    temp += Complex.Conjugate(tempSeqX[n]) * tempSeqY[n + (seqLength - 1 - m)];
                // temp = temp;
            }
            // Calculate the cross-correlation
            else
            {
                for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                    temp += tempSeqX[n + (m - (seqLength - 1))] * Complex.Conjugate(tempSeqY[n]);
                temp = Complex.Conjugate(temp);
            }

            crossCorr[m] = temp;
        }

        Array.Reverse(crossCorr);
    }

    /*
     * Description : Calculate the cross-correlation of input sequences
     * Parameter   : discreteSeqX - first input sequence with type float
     * 				 discreteSeqY - second input sequence with type float
     * 				 crossCorr     - float output sequence,the cross-correlation of input sequences
     * 				 seq_length_x   - the length of discrete_seq_x
     * 				 seq_length_y   - the length of discrete_seq_y
     * 				 option         - specifies a normalization option for the cross-correlation,
     * 				 				  'biased'  : Biased estimate of the autocorrelation function
     * 				 				  'unbiased': Unbiased estimate of the autocorrelation function
     *
     * */
    /// <summary>
    ///     返回两个复数序列的互相关序列，可以设置“有偏”和“无偏”
    /// </summary>
    /// <param name="discreteSeqX">输入复数序列1</param>
    /// <param name="discreteSeqY">输入复数序列2</param>
    /// <param name="crossCorr">返回的互相关复数序列结果</param>
    /// <param name="option">设置选项：biased，unbiased</param>
    public static void CrossCorrComplex(Complex[] discreteSeqX, Complex[] discreteSeqY, ref Complex[] crossCorr,
        XcorrOptions option)
    {
        /*
         * Compare the length of the input sequences,if they were not equal,insert some zeros
         * at the end of the shorter one
         * */
        var seqLengthX = discreteSeqX.Length;
        var seqLengthY = discreteSeqY.Length;
        var seqLength = Math.Max(seqLengthX, seqLengthY);
        var tempSeqX = new Complex[seqLength];
        var tempSeqY = new Complex[seqLength];
        for (var i = 0; i < seqLength; i++)
        {
            if (i < seqLengthX)
                tempSeqX[i] = discreteSeqX[i];
            else
                tempSeqX[i] = 0;
            if (i < seqLengthY)
                tempSeqY[i] = discreteSeqY[i];
            else
                tempSeqY[i] = 0;
        }

        //Calculate cross-correlation
        if (option == XcorrOptions.UnBiased)
        {
            for (var m = 0; m < 2 * seqLength - 1; m++)
            {
                Complex temp = 0;
                // Corresponding to the index of cross-corr under zero
                if (m < seqLength - 1)
                {
                    for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                        temp += Complex.Conjugate(tempSeqX[n]) * tempSeqY[n + (seqLength - 1 - m)];
                    // temp = temp;
                }
                // Calculate the cross-correlation
                else
                {
                    for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                        temp += tempSeqX[n + (m - (seqLength - 1))] * Complex.Conjugate(tempSeqY[n]);
                    temp = Complex.Conjugate(temp);
                }

                temp /= m < seqLength - 1 ? m + 1 : 2 * seqLength - 1 - m;
                crossCorr[m] = temp;
            }

            Array.Reverse(crossCorr);
        }
        else if (option == XcorrOptions.Biased)
        {
            for (var m = 0; m < 2 * seqLength - 1; m++)
            {
                Complex temp = 0;
                // Corresponding to the index of cross-corr under zero
                if (m < seqLength - 1)
                {
                    for (var n = 0; n <= seqLength - 1 - (seqLength - 1 - m); n++)
                        temp += Complex.Conjugate(tempSeqX[n]) * tempSeqY[n + (seqLength - 1 - m)];
                    // temp = temp;
                }
                // Calculate the cross-correlation
                else
                {
                    for (var n = 0; n <= seqLength - 1 - (m - (seqLength - 1)); n++)
                        temp += tempSeqX[n + (m - (seqLength - 1))] * Complex.Conjugate(tempSeqY[n]);
                    temp = Complex.Conjugate(temp);
                }

                temp /= seqLength;
                crossCorr[m] = temp;
            }

            Array.Reverse(crossCorr);
        }
        else
        {
            throw new Exception("Invalid Arithmetic Mode option:Must be 'biased' or 'unbiased'!");
        }
    }
}

/// <summary>
///     相关运算的选项 {有偏，无偏，系数}
/// </summary>
public enum XcorrOptions
{
    /// <summary>
    ///     默认值
    /// </summary>
    None = 0,

    /// <summary>
    ///     有偏估计
    /// </summary>
    Biased = 1,

    /// <summary>
    ///     无偏估计
    /// </summary>
    UnBiased = 2,

    /// <summary>
    ///     归一化相关函数 [-1,1],目前采用(0,1]
    /// </summary>
    Coeff = 4
}