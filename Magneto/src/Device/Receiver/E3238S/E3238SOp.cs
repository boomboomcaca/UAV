using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.E3238S;

public partial class E3238S
{
    /// <summary>
    ///     接收数据线程方法
    /// </summary>
    private void ReceiveDataThread()
    {
        var buffer = new byte[524336 * 2]; //存放socket 一次接收到的数据
        var first = true; //获取第一包数据标志 OK
        try
        {
            while (!_receiveDataTokenSource.IsCancellationRequested)
            {
                if (first)
                {
                    _socket.Receive(buffer, 2, SocketFlags.None);
                    if (buffer[0] == 79 && buffer[1] == 107)
                    {
                        first = false;
                        continue;
                    }
                }

                _socket.Receive(buffer, 4, SocketFlags.None);
                Array.Reverse(buffer, 0, 4);
                var length = BitConverter.ToInt32(buffer, 0);
                var result = new byte[length + 4]; //存放完整的一包数据 数据标志（4 Bytes）+数据
                var dataIndex = 0;
                while (length + 4 > 0)
                {
                    var datalength = _socket.Receive(buffer, length + 4, SocketFlags.None);
                    Array.Copy(buffer, 0, result, dataIndex, datalength);
                    length -= datalength;
                    dataIndex += datalength;
                }

                Array.Reverse(result, 0, 4);
                BitConverter.ToInt32(result, 0);
                if (_isRunning)
                    lock (_locker)
                    {
                        _dataQueue.EnQueue(result);
                    }
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
        }
    }

    /// <summary>
    ///     解析数据线程方法
    /// </summary>
    private void PaseDataThread()
    {
        while (!_paseDataTaskTokenSource.IsCancellationRequested)
        {
            byte[] result = null;
            lock (_locker)
            {
                result = _dataQueue.DeQueue();
            }

            if (_isRunning)
            {
                if (result == null)
                    continue;
                var data = new byte[result.Length - 4];
                var commandTag = BitConverter.ToInt32(result, 0);
                Array.Copy(result, 4, data, 0, data.Length);
                switch (commandTag)
                {
                    case E3238SCommandTag.SpewE3238SRawDataTag:
                        PaseFrequencyDomainData(data);
                        break;
                }
            }
        }
    }

    /// <summary>
    ///     解析频域数据 单频测量 频段扫描 中频多路分析 使用
    /// </summary>
    /// <param name="buffer"></param>
    private void PaseFrequencyDomainData(byte[] buffer)
    {
        if (CurFeature == FeatureType.SCAN)
            FrequencyDomationDataToFScan(buffer);
        else if (CurFeature == FeatureType.FFM) FrequencyDomationDataToSpectrum(buffer);
    }

    /// <summary>
    ///     解析频段扫描数据 频段扫描使用
    /// </summary>
    private void FrequencyDomationDataToFScan(byte[] buffer)
    {
        var frequencydata = new FrequencyData(buffer);
        var offset = Marshal.SizeOf(typeof(FrequencyData));
        var startFrequency = frequencydata.startFrequency;
        var stopFrequency = frequencydata.stopFrequency;
        var numpoints = frequencydata.numPoints; //实际返回的点数
        var segment = frequencydata.segment; //返回的频段数
        var total = Utils.GetTotalCount(_startFrequency, _stopFrequency, _stepFrequency); //客户端需要总点数
        //处理多段扫描时，返回起始频率与设置下去的起始频率不一致的问题 
        if (Math.Abs(Math.Round(startFrequency / 1000000, 1) - _startFrequency) > 1e-9 && segment == 0)
        {
            SendCommand($"startFrequency:{_startFrequency} MHz");
            SendCommand($"stopFrequency:{_stopFrequency} MHz");
            return;
        }

        var result = new float[numpoints];
        for (var i = 0; i < numpoints; i++)
        {
            Array.Reverse(buffer, offset, 4);
            var voltage = BitConverter.ToSingle(buffer, offset);
            result[i] = (float)(10 * Math.Log10(voltage)) + 117;
            offset += 4;
        }

        if (numpoints >= total)
        {
            var oldFrequency = new float[numpoints];
            for (var i = 0; i < total; i++)
                oldFrequency[i] = (float)(startFrequency / 1000000 + i * _stepFrequency / 1000);
            var data = new float[total];
            double step = (stopFrequency / 1000 - startFrequency / 1000) / (numpoints - 1);
            for (var i = 0; i < data.Length; i++)
            {
                var index = Utils.GetCurrIndex(oldFrequency[i], startFrequency / 1000000, step);
                data[i] = result[index];
            }

            var ds = new SDataScan
            {
                StepFrequency = _stepFrequency,
                StartFrequency = _startFrequency,
                StopFrequency = _stopFrequency,
                Total = total,
                Offset = 0,
                Data = Array.ConvertAll(data, item => (short)(item * 10))
            };
            var obj = new List<object> { ds };
            if (_isRunning) SendData(obj);
        }
        else
        {
            var thistotal = Utils.GetTotalCount(startFrequency / 1e6, stopFrequency / 1e6, _stepFrequency);
            var data = CommonInterpolit.InterSample(result, thistotal, result.Length);
            var currIndex = Utils.GetCurrIndex(startFrequency / 1e6, _startFrequency, _stepFrequency);
            var ds = new SDataScan
            {
                StepFrequency = _stepFrequency,
                StartFrequency = _startFrequency,
                StopFrequency = _stopFrequency,
                Total = total,
                Offset = currIndex,
                Data = Array.ConvertAll(data, item => (short)(item * 10))
            };
            var obj = new List<object> { ds };
            if (_isRunning) SendData(obj);
        }
    }

    /// <summary>
    ///     转化为频谱数据 单频测量使用
    /// </summary>
    private void FrequencyDomationDataToSpectrum(byte[] buffer)
    {
        var frequencydata = new FrequencyData(buffer);
        var offset = Marshal.SizeOf(typeof(FrequencyData));
        var startFrequency = frequencydata.startFrequency;
        var numpoints = frequencydata.numPoints;
        //处理启动单频测量时，偶发性的中心频率设置不成功的问题
        if (Math.Abs(startFrequency - (_frequency - _ifBandwidth / 1000 / 2) * 1000000) >
            _ifBandwidth * 1000 / (numpoints - 1))
        {
            if (_resetFixFqParms)
            {
                SendCommand($"centerFrequency:{_frequency} MHz");
                SendCommand($"spanFrequency:{_ifBandwidth} KHz");
                _resetFixFqParms = false;
            }
            else
            {
                _resetFixFqParms = true;
            }

            return;
        }

        var spectrum = new float[numpoints];
        for (var i = 0; i < numpoints; i++)
        {
            Array.Reverse(buffer, offset, 4);
            var voltage = BitConverter.ToSingle(buffer, offset);
            spectrum[i] = (float)(10 * Math.Log10(voltage)) + 117; //此处加117转为dbuV后与原厂软件的频谱数据波形，幅度大小一致 
            offset += 4;
        }

        var ds = new SDataSpectrum
        {
            Data = Array.ConvertAll(spectrum, item => (short)(item * 10)),
            Frequency = _frequency,
            Span = _ifBandwidth
        };
        var objds = new List<object> { ds };
        SendData(objds);
        var dl = new SDataLevel
        {
            Data = GetLevel(spectrum, _filterBandwidth, _ifBandwidth, numpoints),
            Frequency = _frequency,
            Bandwidth = _ifBandwidth
        };
        var objdl = new List<object> { dl };
        SendData(objdl);
    }

    /// <summary>
    ///     获取电平值(解调带宽范围内的平均值)
    /// </summary>
    /// <returns></returns>
    private static float GetLevel(float[] spectrum, double bandwidth, double span, int numpoints)
    {
        var nu = (int)(numpoints * bandwidth / span / 1000);
        if (nu == 0) return spectrum[spectrum.Length / 2];

        var arrary = new float[nu];
        Array.Copy(spectrum, spectrum.Length / 2 - nu - 1, arrary, 0, nu);
        var totalLevel = arrary.Sum();
        return totalLevel / arrary.Length;
    }
}