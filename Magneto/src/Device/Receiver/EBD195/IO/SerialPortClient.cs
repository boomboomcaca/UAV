using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace Magneto.Device.EBD195.IO;

public class SerialPortClient : IClient
{
    private readonly object _lockComport = new();

    // 串口通信
    private SerialPort _serialPort;

    public SerialPortClient(string com, int baudrate)
    {
        InitSerialPort(com, baudrate);
    }

    public event EventHandler<string> DataReceived;

    public int BytesToRead => _serialPort.BytesToRead;

    // 上次从串口读取到数据的时间，心跳检测使用
    public DateTime LastGetDataTime { get; set; } = DateTime.Now;

    public bool Init(out string err)
    {
        err = "";
        return true;
    }

    public void DiscardInBuffer()
    {
        _serialPort.DiscardInBuffer();
    }

    /// <summary>
    ///     发送命令
    /// </summary>
    /// <param name="cmd">命令字符</param>
    public void SendCmd(string cmd)
    {
        try
        {
            if (_serialPort == null) return;
            if (!_serialPort.IsOpen) return;
            cmd += "\r\n";
            var buffer = Encoding.ASCII.GetBytes(cmd);
            lock (_lockComport)
            {
                _serialPort.Write(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Trace.WriteLine($"发送命令'{cmd}'错误:{ex.Message}");
#endif
            // 记录日志在上层进行
            //throw ex;
        }
    }

    public void Close()
    {
        ReleaseSerialPort();
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SerialPortClient()
    {
        Dispose(false);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    ///     初始化串口连接
    /// </summary>
    private void InitSerialPort(string port, int baudrate = 9600)
    {
        LastGetDataTime = DateTime.Now;
        _serialPort = new SerialPort(port, baudrate, Parity.None, 8, StopBits.Two);
        _serialPort.Open();
        _serialPort.ReceivedBytesThreshold = 1;
        _serialPort.DiscardInBuffer();
        _serialPort.DataReceived += SerialPort_DataReceived;
        _serialPort.PinChanged += SerialPort_PinChanged;
    }

    /// <summary>
    ///     关闭串口
    /// </summary>
    private void ReleaseSerialPort()
    {
        if (_serialPort == null) return;
        try
        {
            // 需要先关闭串口再回收线程,不然会报错[System.ObjectDisposedException 已关闭 Safe handle],这会导致程序崩溃
            // 关闭线程的时候,串口读写的资源被GC了,后面再对串口操作会发生handle被关闭的错误,因此需要先关闭串口再关闭线程
            // 相关链接:https://www.cnblogs.com/dekun_1986/archive/2009/11/17/1604407.html
            if (_serialPort is { IsOpen: true })
            {
                _serialPort.PinChanged -= SerialPort_PinChanged;
                _serialPort.DataReceived -= SerialPort_DataReceived;
                lock (_lockComport)
                {
                    _serialPort.Close();
                }
            }
        }
        catch (IOException e)
        {
#if DEBUG
            Trace.WriteLine("关闭串口失败：" + e);
#endif
        }
        finally
        {
            _serialPort = null;
        }
    }

    #region 事件

    private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
    {
        var res = false;
        switch (e.EventType)
        {
            case SerialPinChange.CDChanged:
                res = _serialPort.CDHolding;
                break;
            case SerialPinChange.DsrChanged:
                res = _serialPort.DsrHolding;
                break;
        }

        if (!res)
        {
            ////////串口连接中断
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (e.EventType == SerialData.Chars)
        {
            var buffer = new byte[_serialPort.ReadBufferSize];
            var data = "";
            int recvCount;
            lock (_lockComport)
            {
                recvCount = _serialPort.Read(buffer, 0, buffer.Length);
            }

            data = Encoding.ASCII.GetString(buffer, 0, recvCount);
            DataReceived?.Invoke(this, data);
        }

        LastGetDataTime = DateTime.Now;
        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
    }

    #endregion 事件
}