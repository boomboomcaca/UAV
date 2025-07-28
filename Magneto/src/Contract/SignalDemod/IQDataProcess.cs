using System;
using System.IO;
using Magneto.Protocol.Data;

namespace Magneto.Contract.SignalDemod;

public sealed class IqDataProcess : IDisposable
{
    /// <summary>
    ///     IQ数据文件名称
    /// </summary>
    private string _dataFileName = "";

    private bool _disposed;

    /// <summary>
    ///     文件对象
    /// </summary>
    private FileStream _fs;

    /// <summary>
    ///     是否第一包数据
    /// </summary>
    private bool _isFirst = true;

    /// <summary>
    ///     IQ数据需采集的长度
    /// </summary>
    private long _needDataLen;

    private double _preFreq;
    private double _preIfBw;

    /// <summary>
    ///     当前已写入的数据长度
    /// </summary>
    private long _writeLen;

    /// <summary>
    ///     一行固定长度
    /// </summary>
    private int FixSize => 33; //为除IQ数组外的其它几项字节长度(28字节)+ 0代表16位IQ 1代表32位IQ(1字节)  IQ数组长度（4字节)

    #region 事件

    /// <summary>
    ///     数据可读取事件
    /// </summary>
    public event EventHandler DataCanRead;

    #endregion

    ~IqDataProcess()
    {
        Dispose(false);
    }

    #region 公共方法

    /// <summary>
    ///     开始
    /// </summary>
    /// <param name="needDataLen">IQ数据需采集的长度</param>
    public void Start(long needDataLen)
    {
        _isFirst = true;
        _needDataLen = needDataLen;
        _dataFileName = "";
        if (_fs != null)
        {
            _fs.Dispose();
            _fs = null;
        }
    }

    /// <summary>
    ///     停止
    /// </summary>
    private void Stop()
    {
        if (_fs != null)
        {
            _fs.Flush();
            _fs.Dispose();
            _fs = null;
        }

        _writeLen = 0;
        DataCanRead?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="data"></param>
    public void Revice(SDataIq data)
    {
        if (_writeLen < _needDataLen)
            WriteData(data);
        else
            Stop();
    }

    /// <summary>
    ///     读取一帧IQ数据
    /// </summary>
    /// <returns>null：无数据或读取出错  非null:IQ数据</returns>
    public SDataIq ReadData()
    {
        try
        {
            if (string.IsNullOrEmpty(_dataFileName) || !File.Exists(_dataFileName)) return null;
            _fs ??= new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var dataLen = 0;
            var bRead = new byte[FixSize];
            _ = _fs.Read(bRead, 0, bRead.Length);
            var result = ConvertData(bRead, out var dataType, out var arrayLen);
            if (result == null) return null;
            //判断数据类型和长度
            if (dataType != 255 && arrayLen != 0)
            {
                if (dataType == 0)
                {
                    result.Data16 = new short[arrayLen];
                    dataLen = 2 * arrayLen;
                }
                else
                {
                    result.Data32 = new int[arrayLen];
                    dataLen = 4 * arrayLen;
                }
            }

            bRead = new byte[dataLen];
            var success = ConvertIqArrayData(result, dataType == 0, bRead);
            if (!success) return null;
            return result;
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
        }

        _fs?.Dispose();
        _disposed = true;
    }

    #endregion

    #region 写数据

    /// <summary>
    ///     写数据
    /// </summary>
    /// <param name="data"></param>
    private void WriteData(SDataIq data)
    {
        // 如果第一次接收数据或中心频率改变
        if (_isFirst || Math.Abs(data.Frequency * 1000000 - _preFreq * 1000000) > 1e-9 ||
            Math.Abs(data.Bandwidth * 1000 - _preIfBw * 1000) > 1e-9)
        {
            if (_fs != null)
                try
                {
                    _fs.Flush();
                    _fs.Dispose();
                    _fs = null;
                }
                catch
                {
                    // ignored
                }

            try
            {
                _dataFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PublicDefine.PathSgldec,
                    $"iqData_{data.Frequency}MHz_{data.Bandwidth}kHz_{DateTime.Now:yyyMMddHHmmssfff}.txt");
                if (File.Exists(_dataFileName)) File.Delete(_dataFileName);
                _fs = new FileStream(_dataFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                _isFirst = false;
                _writeLen = 0;
            }
            catch
            {
                return;
            }
        }

        _preFreq = data.Frequency;
        _preIfBw = data.Bandwidth;
        var dataVal = ConvertData(data, out var writeLen);
        if (dataVal != null)
        {
            _fs.Write(dataVal, 0, dataVal.Length);
            _fs.Flush();
            _writeLen += writeLen;
        }
    }

    /// <summary>
    ///     转换数据
    /// </summary>
    /// <param name="data">IQ数据</param>
    /// <param name="writeLen"></param>
    /// <returns>转换后的字节数据</returns>
    private byte[] ConvertData(SDataIq data, out int writeLen)
    {
        var isData32 = false;
        var index = 0;
        var dataLen = FixSize;
        if (data.Data16 != null) dataLen += 2 * data.Data16.Length;
        if (data.Data32 != null)
        {
            dataLen += 4 * data.Data32.Length;
            isData32 = true;
        }

        var bResult = new byte[dataLen];
        var bTemp = BitConverter.GetBytes(data.Frequency); //频率
        Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        index += bTemp.Length;
        bTemp = BitConverter.GetBytes(data.Bandwidth); //中频带宽
        Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        index += bTemp.Length;
        bTemp = BitConverter.GetBytes(data.SamplingRate); //采样率
        Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        index += bTemp.Length;
        bTemp = BitConverter.GetBytes(data.Attenuation); //衰减
        Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        index += bTemp.Length;
        bResult[index] = isData32 ? (byte)1 : (byte)0; //16位还是32位IQ
        index += 1;
        bTemp = BitConverter.GetBytes(isData32 ? data.Data32.Length : data.Data16!.Length); //数组长度
        Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        index += bTemp.Length;
        if (data.Data16 != null)
        {
            bTemp = CovertIq16(data.Data16);
            Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
            writeLen = data.Data16.Length;
        }
        else
        {
            bTemp = CovertIq32(data.Data32);
            Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
            writeLen = data.Data32!.Length;
        }

        return bResult;
    }

    /// <summary>
    ///     转换16位IQ
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte[] CovertIq16(short[] data)
    {
        var index = 0;
        var bResult = new byte[data.Length * 2];
        foreach (var t in data)
        {
            var bTemp = BitConverter.GetBytes(t);
            Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
        }

        return bResult;
    }

    /// <summary>
    ///     转换32位IQ
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static byte[] CovertIq32(int[] data)
    {
        var index = 0;
        var bResult = new byte[data.Length * 4];
        foreach (var t in data)
        {
            var bTemp = BitConverter.GetBytes(t);
            Array.Copy(bTemp, 0, bResult, index, bTemp.Length);
            index += bTemp.Length;
        }

        return bResult;
    }

    #endregion

    #region 读数据

    /// <summary>
    ///     转换数据
    /// </summary>
    /// <param name="data">字节数组</param>
    /// <param name="dataType"></param>
    /// <param name="dataLen"></param>
    /// <returns>IQ数据对象</returns>
    private static SDataIq ConvertData(byte[] data, out byte dataType, out int dataLen)
    {
        var index = 0;
        var result = new SDataIq
        {
            Frequency = BitConverter.ToDouble(data, index) //频率
        };
        index += 8;
        result.Bandwidth = BitConverter.ToDouble(data, index); //中频带宽
        index += 8;
        result.SamplingRate = BitConverter.ToDouble(data, index); //采样率
        index += 8;
        result.Attenuation = (int)BitConverter.ToSingle(data, index); //衰减
        index += 4;
        dataType = data[index];
        index += 1;
        dataLen = BitConverter.ToInt32(data, index);
        return result;
    }

    /// <summary>
    ///     转换IQ数组数据
    /// </summary>
    /// <param name="iq">iq数据包</param>
    /// <param name="isShort">true:16位IQ false:32位IQ</param>
    /// <param name="data">IQ数据</param>
    private static bool ConvertIqArrayData(SDataIq iq, bool isShort, byte[] data)
    {
        var arrayLen = isShort ? iq.Data16?.Length ?? 0 : iq.Data32?.Length ?? 0;
        if (arrayLen == 0) return false;
        var index = 0;
        for (var i = 0; i < arrayLen; i++)
            if (isShort)
            {
                iq.Data16![i] = BitConverter.ToInt16(data, index);
                index += 2;
            }
            else
            {
                iq.Data32![i] = BitConverter.ToInt32(data, index);
                index += 4;
            }

        return true;
    }

    #endregion
}