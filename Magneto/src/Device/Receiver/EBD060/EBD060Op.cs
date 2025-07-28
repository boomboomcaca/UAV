using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD060;

public partial class Ebd060
{
    /// <summary>
    ///     解析安装高低端天线与正北方向的夹角
    /// </summary>
    private void ParseCorrValue()
    {
        try
        {
            if (North == string.Empty)
            {
                _lowCorrValue = 0;
                _highCorrValue = 0;
            }
            else
            {
                var corrValues = North.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                if (corrValues.Length == 2)
                {
                    _lowCorrValue = float.Parse(corrValues[0]);
                    _highCorrValue = float.Parse(corrValues[1]);
                }
                else
                {
                    throw new Exception("EBD060安装参数北偏角校正值解析错误！");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("EBD060安装参数北偏角校正值解析错误！Error: " + ex);
        }
    }

    #region 线程函数

    /// <summary>
    ///     采集数据线程方法
    /// </summary>
    private void CaptureData()
    {
        while (!_dataCaptureTokenSource.IsCancellationRequested)
            try
            {
                var offset = 0;
                var headerBytes = new byte[9];
                _tcpSocket.Receive(headerBytes, 0, 1, SocketFlags.None);
                offset += 1;
                var flag = headerBytes[0];
                switch (flag)
                {
                    case (byte)TcpExt.Receive:
                        _tcpSocket.Receive(headerBytes, offset, 8, SocketFlags.None);
                        break;
                    case (byte)TcpExt.Send:
                        offset += 6;
                        _tcpSocket.Receive(headerBytes, offset, 2, SocketFlags.None);
                        break;
                }

                //解析数据头部
                var header = new Ebd060Header(headerBytes, 0);
                if (header.Modifier != (byte)TcpExt.Receive) continue;
                int total = header.Total;
                var buffer = new byte[total];
                offset = 0;
                var remainBytes = total;
                while (remainBytes > 0)
                {
                    var readBytes = _tcpSocket.Receive(buffer, offset, remainBytes, SocketFlags.None);
                    remainBytes -= readBytes;
                    offset += readBytes;
                }

                if (header.Type == (byte)LdDdf.Data)
                {
                    var msgType = BitConverter.ToInt16(buffer, 2);
                    if (msgType is (short)Flags.Ffm or (short)Flags.Spectrum)
                    {
                        var data = new byte[total];
                        Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                        if (TaskState == TaskState.Start) _dataQueue.EnQueue(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EBD060数据接收失败:" + ex);
            }
    }

    /// <summary>
    ///     加工处理数据线程方法
    /// </summary>
    private void ProcessData()
    {
        while (!_dataProcessTokenSource.IsCancellationRequested)
            try
            {
                var result = _dataQueue.DeQueue();
                if (result != null)
                {
                    var messageType = BitConverter.ToInt16(result, 2);
                    switch (messageType)
                    {
                        case (short)Flags.Ffm:
                            ParseDfData(result);
                            break;
                        case (short)Flags.Spectrum:
                            ParseSpetrum(result);
                            break;
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("EBD060数据解析失败:" + ex);
            }
    }

    /// <summary>
    ///     解析测向数据
    /// </summary>
    /// <param name="data"></param>
    private void ParseDfData(byte[] data)
    {
        var startIndex = 0;
        while (startIndex < data.Length)
        {
            var ffm = new FfmMessage(data, ref startIndex);
            ToDFind(ffm);
            ToLevel(ffm);
        }
    }

    /// <summary>
    ///     解析频谱数据
    /// </summary>
    /// <param name="data"></param>
    private void ParseSpetrum(byte[] data)
    {
        var startIndex = 0;
        while (startIndex < data.Length)
        {
            var ifm = new IfSpetrumMessage(data, ref startIndex);
            ToSpectrum(ifm);
        }
    }

    /// <summary>
    ///     发送测向数据
    /// </summary>
    /// <param name="data"></param>
    private void ToDFind(FfmMessage data)
    {
        var result = new List<object>();
        SDataDfind dataDFind = new()
        {
            BandWidth = _dfBandWidth,
            Frequency = _frequency,
            Azimuth = data.Azimuth / 100.0f,
            Quality = data.Quality
        };
        //根据配置的北偏角校正值校正示向度
        if (_frequency <= 1300)
            dataDFind.Azimuth += 0;
        else
            dataDFind.Azimuth += 1;
        if (dataDFind.Azimuth < 0)
            dataDFind.Azimuth += 360;
        else if (dataDFind.Azimuth > 360) dataDFind.Azimuth -= 360;
        if (dataDFind.Azimuth == 0 && dataDFind.Quality == 0) return;
        result.Add(dataDFind);
        result = result.Where(item => item != null).ToList();
        if (result.Any())
            if (TaskState == TaskState.Start)
                SendData(result);
    }

    /// <summary>
    ///     发送电平数据
    /// </summary>
    /// <param name="data"></param>
    private void ToLevel(FfmMessage data)
    {
        var result = new List<object>();
        var dataLevel = new SDataLevel
        {
            Frequency = _frequency,
            Bandwidth = _dfBandWidth,
            Data = data.Level / 100.0f
        };
        result.Add(dataLevel);
        if (result is { Count: > 0 })
            if (TaskState == TaskState.Start)
                SendData(result);
    }

    /// <summary>
    ///     发送频谱数据
    /// </summary>
    /// <param name="spectrumData"></param>
    private void ToSpectrum(IfSpetrumMessage spectrumData)
    {
        var result = new List<object>();
        var dataSpectrum = new SDataSpectrum
        {
            Frequency = _frequency,
            Span = _dfBandWidth,
            Data = new short[spectrumData.Data.Length]
        };
        for (var i = 0; i < spectrumData.Length; ++i) dataSpectrum.Data[i] = (short)(spectrumData.Data[i] * 10);
        result.Add(dataSpectrum);
        if (result.Count > 0)
            if (TaskState == TaskState.Start)
                SendData(result);
    }

    #endregion
}