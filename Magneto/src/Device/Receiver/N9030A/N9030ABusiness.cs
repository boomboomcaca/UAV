using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.N9030A;

public partial class N9030A
{
    /// <summary>
    ///     读取频谱仪数据。
    /// </summary>
    /// <returns>各种监测数据。</returns>
    protected string ReadData(int timeOut)
    {
        try
        {
            Socket.ReceiveTimeout = timeOut;
            var buf = new List<byte>();
            while (true)
            {
                var current = new byte[Socket.Available];
                Socket.Receive(current, current.Length, SocketFlags.None);
                buf.AddRange(current);
                if (current.Contains((byte)0x0a)) break;
            }

            return Encoding.ASCII.GetString(buf.ToArray<byte>(), 0, buf.Count);
        }
        catch (Exception ex)
        {
            if (ex is SocketException)
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
            return null;
        }
    }

    /// <summary>
    ///     获得marker电平
    /// </summary>
    /// <returns></returns>
    private float GetLevel()
    {
        var level = 0.0f;
        try
        {
            var str = SendCommand("CALC:MARK1:Y?");
            if (!string.IsNullOrEmpty(str))
            {
                level = float.Parse(str);
                if (level is <= -999 or >= 999) level = 0.0f;
            }

            return level;
        }
        catch
        {
            return level;
        }
    }

    /// <summary>
    ///     获取频谱数据
    /// </summary>
    /// <param name="data">电平数组</param>
    private void GetSpectrum(out float[] data)
    {
        var readdata = SendCommand("CALC:DATA1?");
        if (!string.IsNullOrEmpty(readdata))
        {
            readdata = readdata.Substring(0, readdata.Length - 1);
            var strtmp = readdata.Split(',');
            data = new float[strtmp.Length / 2];
            for (var i = 0; i < data.Length; ++i) data[i] = float.Parse(strtmp[2 * i + 1]);
            return;
        }

        data = null;
    }

    private double GetItu()
    {
        SendCommand("CONFigure:OBWidth");
        SendCommand("CONFigure:OBWidth:NDEFault");
        SendCommand("INITiate:OBWidth");
        var xdb = SendCommand("FETCh:OBWidth:XDB?");
        SendCommand("CONF:SAN");
        SendCommand("CONF:OBW:NDEF");
        SendCommand("CONF:SAN");
        return double.Parse(xdb);
    }

    /// <summary>
    ///     发送电平数据
    /// </summary>
    private void SendLevel()
    {
        if (!LevelSwitch) return;
        var level = GetLevel();
        if (level is > 999.0f or < -999.0f) return;
        var data = new List<object>();
        var generalLevel = new SDataLevel
        {
            Bandwidth = _ifBandwidth,
            Frequency = Frequency,
            Data = level
        };
        data.Add(generalLevel);
        SendData(data);
    }

    /// <summary>
    ///     单频测量线程
    /// </summary>
    private void SendSpectrum()
    {
        try
        {
            if (!SpectrumSwitch) return;
            GetSpectrum(out var spdata);
            var data = new List<object>();
            var generalSpectrum = new SDataSpectrum
            {
                Span = _ifBandwidth,
                Frequency = Frequency,
                Data = Array.ConvertAll(spdata, item => (short)(item * 10))
            };
            data.Add(generalSpectrum);
            SendData(data);
        }
        catch
        {
        }
    }

    private void SendItu()
    {
        var ituData = new SDataItu
        {
            Bandwidth = GetItu() / 1000000
        };
        var data = new List<object> { ituData };
        SendData(data);
    }

    /// <summary>
    ///     频段扫描线程
    /// </summary>
    private void SendScan()
    {
        var count = Utils.GetTotalCount(StartFrequency, StopFrequency, StepFrequency);
        // 点数除不尽 小数点后四舍五入               
        var generalScan = new SDataScan
        {
            Offset = 0,
            StartFrequency = StartFrequency,
            StopFrequency = StopFrequency,
            StepFrequency = StepFrequency,
            Total = count
        };
        var startFreq = StartFrequency;
        var stopFreq = StopFrequency;
        if (count - 40001 <= 0)
        {
            if (!_isSendScanCmd)
            {
                SendScanCmd(startFreq, stopFreq, count);
                _isSendScanCmd = true;
            }

            SendScanData(generalScan);
        }
        else
        {
            while (!_dataTokenSource.IsCancellationRequested)
                if (count - 40001 <= 0)
                {
                    SendScanCmd(startFreq, stopFreq, count);
                    SendScanData(generalScan);
                    break;
                }
                else
                {
                    count -= 40001;
                    stopFreq = startFreq + StepFrequency / 1000 * 40000;
                    SendScanCmd(startFreq, stopFreq, 40001);
                    startFreq = stopFreq + StepFrequency;
                    SendScanData(generalScan);
                }
        }
    }

    /// <summary>
    ///     发送频段扫描命令
    /// </summary>
    /// <param name="startFreq"></param>
    /// <param name="stopFreq"></param>
    /// <param name="count"></param>
    private void SendScanCmd(double startFreq, double stopFreq, int count)
    {
        SendCommand("SENS:FREQ:STAR " + startFreq + "MHz");
        SendCommand("SENS:FREQ:STOP " + stopFreq + "MHz");
        SendCommand("SENS:SWE:POIN " + count);
    }

    /// <summary>
    ///     发送频段扫描数据
    /// </summary>
    /// <param name="generalScan"></param>
    private void SendScanData(SDataScan generalScan)
    {
        GetSpectrum(out var scanData);
        var data = new List<object>();
        generalScan.Data = Array.ConvertAll(scanData, item => (short)(item * 10));
        data.Add(generalScan);
        SendData(data);
    }

    /// <summary>
    ///     离散扫描线程
    /// </summary>
    private void SendMscan()
    {
        for (var i = 0; i < MscanPoints.Length; i++)
        {
            var item = MscanPoints[i];
            foreach (var property in item)
                SetParameter(property.Key, property.Value);
            var level = GetLevel();
            // 离散扫描不需要频谱
            //GetSpectrum(out spData);
            var generalScan = new SDataScan
            {
                Offset = i,
                Data = new short[1],
                Total = MscanPoints.Length
            };
            generalScan.Data[0] = (short)(level * 10);
            var result = new List<object> { generalScan };

            SendData(result);
        }
    }
}