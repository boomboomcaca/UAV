using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.EBD100;

public partial class Ebd100
{
    #region 事件

    /// <summary>
    ///     数据接收拼装解析
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            if (e.EventType == SerialData.Chars)
                lock (_lockComport)
                {
                    var buffer = new byte[_serialPort.ReadBufferSize];
                    var data = "";
                    var recvCount = _serialPort.Read(buffer, 0, buffer.Length);

                    data = Encoding.ASCII.GetString(buffer, 0, recvCount);
                    ReceivedData(data);
                }

            _lastGetDataTime = DateTime.Now;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    #endregion 事件

    #region 数据解析

    /// <summary>
    ///     接收并组装数据
    /// </summary>
    /// <param name="data"></param>
    private void ReceivedData(string data)
    {
        foreach (var t in data)
            if (t == '\r' || t == '\n')
            {
                if (_recvData is "" or "\r" or "\n")
                {
                    _recvData = t.ToString();
                }
                else
                {
                    AnalysisData(_recvData);
                    _recvData = "";
                }
            }
            else
            {
                if (_recvData is "" or "\r" or "\n")
                    _recvData = t.ToString();
                else
                    _recvData += t;
            }
    }

    /// <summary>
    ///     数据分析
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void AnalysisData(string data)
    {
        var a = new[] { ',' };
        var arr = data.Split(a);
        if (arr.Length < 1) return;
        if (arr[0][..1].Trim().Equals("A")) DdfAnalysis(arr);
        if (arr[0][..1].Trim().Equals("C")) CompassAnalysis(arr);
    }

    /// <summary>
    ///     示向度分析
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void DdfAnalysis(string[] data)
    {
        try
        {
            if (!_isRunning) return;
            // 将形如：A98,87,274,49的数据根据","分隔为字符串数组
            var len = data.Length;
            var ddf = data[0][1..];
            var isHasDdf = false;
            var isHasLevel = false;
            if (short.TryParse(ddf, out var num))
            {
                _ddf = num;
                isHasDdf = true;
            }

            _quality = 0;
            _level = 0;
            switch (len)
            {
                case 2:
                    if (short.TryParse(data[1].Trim(), out num)) _quality = num; //取得测向机测向质量
                    break;
                case 4:
                case 5:
                case 6:
                    if (short.TryParse(data[1].Trim(), out num)) _quality = num;
                    if (short.TryParse(data[2].Trim(), out num))
                    {
                        _level = num; //测向机测向电平
                        isHasLevel = true;
                    }

                    break;
            }

            var datas = new List<object>();
            //if (isHasDDF && _quality > _qualityThreshold)
            if (isHasDdf)
            {
                SDataDfind dfData = new()
                {
                    Frequency = _frequency,
                    BandWidth = double.Parse(_dfBandWidth),
                    Quality = _quality,
                    Azimuth = _ddf
                };
                datas.Add(dfData);
            }

            if ((_media & MediaType.Level) > 0 && isHasLevel)
            {
                var levelData = new SDataLevel
                {
                    Frequency = _frequency,
                    Bandwidth = double.Parse(_dfBandWidth),
                    Data = _level
                };
                datas.Add(levelData);
            }

            if (_isRunning) SendData(datas);
        }
        catch
        {
        }
    }

    /// <summary>
    ///     电子罗盘分析
    ///     罗盘是五秒获取一次数据，因此不需要缓存
    /// </summary>
    /// <param name="data">要分析的字符串</param>
    private void CompassAnalysis(string[] data)
    {
        var ePsilon = 0.00001d;
        try
        {
            var str = data[0][1..];
            if (str == "999") str = "0";
            if (!HaveCompass) return;
            Encoding.ASCII.GetBytes(str);
            _compass = short.Parse(str);
            // 加补偿角度值
            _compass += (short)ExtraAngle;
            // 保证范围在0~360之间
            _compass = (short)((_compass % 360 + 360) % 360);
            if (ReportingDirection)
            {
                var compassData = new SDataCompass
                {
                    Heading = _compass
                };
                if (_data == null || Math.Abs(compassData.Heading - _data.Heading) > ePsilon)
                {
                    SendMessageData(new List<object> { compassData });
                    _data = compassData;
                }
            }
        }
        catch
        {
        }
    }

    #endregion 数据解析
}