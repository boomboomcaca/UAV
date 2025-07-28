using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device;

public partial class AdsBUsb
{
    private void InitSerialPort()
    {
        if (_serialPort == null)
        {
            _serialPort = new SerialPort();
            _serialPort.PortName = Com;
            _serialPort.BaudRate = 57600;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.ReadTimeout = 10000;
            _serialPort.WriteTimeout = 10000;
        }

        //关闭再打开
        _serialPort.DataReceived += _dataPort_DataReceived;
        if (_serialPort.IsOpen) _serialPort.Close();
        _serialPort.Open();
    }

    private void _dataPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        _lastGetDataTime = DateTime.Now;
        DataProcess();
    }

    /// <summary>
    ///     初始化所有任务
    /// </summary>
    private void InitAllTask()
    {
        // 心跳线程
        _heartBeatTokenSource = new CancellationTokenSource();
        _heartBeatTask = new Task(KeepAlive, _heartBeatTokenSource.Token);
        _heartBeatTask.Start();
    }

    /// <summary>
    ///     释放非托管资源
    /// </summary>
    private void ReleaseResource()
    {
        Utils.CancelTask(_heartBeatTask, _heartBeatTokenSource);
        try
        {
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= _dataPort_DataReceived;
                if (_serialPort.IsOpen) _serialPort.Close();
                _serialPort.Dispose();
            }
        }
        finally
        {
            _serialPort = null;
        }
    }

    /// <summary>
    ///     读取指定长度数据
    /// </summary>
    private byte[] ReadDataBuffer(int length)
    {
        var receivedBuffer = new byte[length];
        var total = length;
        var offset = 0;
        while (offset < total)
        {
            var receivedLength = _serialPort.Read(receivedBuffer, offset, total);
            offset += receivedLength;
            total -= receivedLength;
        }

        return receivedBuffer;
    }

    /// <summary>
    ///     数据处理
    /// </summary>
    private void DataProcess()
    {
        try
        {
            var bufferHeader = ReadDataBuffer(Marshal.SizeOf(typeof(PacketHeader)));
            //串口速度较慢，不缓存
            var packetHeader = new PacketHeader(bufferHeader, 0);
            if (packetHeader.MessageID == (byte)MessageId.TrafficeReportMessageId)
            {
                var bufferData = ReadDataBuffer(packetHeader.PayloadLength + 2); //2:CRC校验高低两位
                var list = new List<FlightInfo>();
                var flightInfo = ParseAircraft(bufferData);
                if (flightInfo == null) return;
                var datum = flightInfo.Value;
                if (string.IsNullOrEmpty(datum.PlaneAddress)) return;
                Console.WriteLine(
                    $"    ICAO:{datum.PlaneAddress}   呼号:{datum.FlightNumber}   应答机编码:{datum.TransponderCode}  位置:{datum.Longitude}E,{datum.Latitude}N   水平速度:{datum.HorizontalSpeed}km/h    垂直速度:{datum.VerticalSpeed}m/s   方向:{datum.Azimuth}    海拔:{datum.Altitude}m");
                list.Add(datum);
                if (list.Count > 0)
                {
                    var data = new SDataAdsB
                    {
                        Data = list
                    };
                    SendMessageData(new List<object> { data });
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is TimeoutException)
                // 目前这里并不能判断设备连接异常
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
        }
    }

    // 心跳检测线程
    private void KeepAlive()
    {
        while (_heartBeatTokenSource?.IsCancellationRequested == false)
        {
            // 心跳检测
            // 连上以后会自动发送数据过来，不需要主动发送信息
            if (DateTime.Now.Subtract(_lastGetDataTime).TotalMilliseconds > 20000) //20s超时
            {
                SendMessage(new SDataMessage
                {
                    LogType = LogType.Warning,
                    ErrorCode = (int)InternalMessageType.DeviceRestart,
                    Description = DeviceId.ToString(),
                    Detail = DeviceInfo.DisplayName
                });
                break;
            }

            Thread.Sleep(1000);
        }
    }
}