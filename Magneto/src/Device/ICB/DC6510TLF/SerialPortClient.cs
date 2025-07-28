using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace Magneto.Device.DC6510TLF;

/// <summary>
///     串口处理
/// </summary>
public sealed class SerialPortClient
{
    private readonly AutoResetEvent _autoResetEvent;
    private readonly int _baudrate;
    private readonly ConcurrentQueue<string> _buffer;
    private readonly string _com;
    private bool _disposed;
    private volatile bool _isSync;
    private SerialPort _serialPort;

    public SerialPortClient(string com, int baudrate)
    {
        _com = com;
        _baudrate = baudrate;
        _buffer = new ConcurrentQueue<string>();
        _autoResetEvent = new AutoResetEvent(true);
    }

    public bool IsConnected { get; private set; }
    public int Timeout { get; set; } = 45000;
    public event EventHandler<string> DataReceived;

    ~SerialPortClient()
    {
        Dispose(false);
    }

    public bool Init(out string err)
    {
        err = null;
        _buffer.Clear();
        try
        {
            _serialPort = new SerialPort(_com, _baudrate, Parity.None, 8, StopBits.One)
            {
                WriteTimeout = 3000,
                ReadTimeout = 3000
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            IsConnected = _serialPort.IsOpen;
        }
        catch (IOException)
        {
            err = "初始化串口异常";
            return false;
        }

        if (!IsConnected)
        {
            err = "串口连接失败";
            return false;
        }

        return true;
    }

    /// <summary>
    ///     发送指令
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="check">1返回true,-1返回false，其他不返回</param>
    /// <returns></returns>
    public bool SendAndReceive(string cmd, Func<string, int> check)
    {
        if (!IsConnected || string.IsNullOrEmpty(cmd)) return false;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        var sb = new StringBuilder();
        var result = false;
        _autoResetEvent.WaitOne();
        _isSync = true;
        var stopWatch = new Stopwatch();
        try
        {
            _buffer.Clear();
            _serialPort.WriteLine(cmd);
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < Timeout)
            {
                if (!_buffer.TryDequeue(out var temp))
                {
                    Thread.Sleep(20);
                    continue;
                }

                sb.Append(temp);
                var s = check(temp);
                if (s == 1)
                {
                    result = true;
                    break;
                }

                if (s == -1)
                {
                    result = false;
                    break;
                }
            }
        }
        finally
        {
            _isSync = false;
            stopWatch.Reset();
            _autoResetEvent.Set();
        }

        return result;
    }

    /// <summary>
    ///     发送指令
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public string SendAndReceive2(string cmd)
    {
        if (!IsConnected || string.IsNullOrEmpty(cmd)) return string.Empty;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        var sb = new StringBuilder();
        _autoResetEvent.WaitOne();
        _isSync = true;
        var stopWatch = new Stopwatch();
        try
        {
            _buffer.Clear();
            _serialPort.WriteLine(cmd);
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < Timeout)
            {
                if (!_buffer.TryDequeue(out var temp))
                {
                    Thread.Sleep(20);
                    continue;
                }

                sb.Append(temp);
                break;
            }
        }
        finally
        {
            _isSync = false;
            stopWatch.Reset();
            _autoResetEvent.Set();
        }

        return sb.ToString();
    }

    public void SendCommand(string cmd)
    {
        if (!IsConnected || string.IsNullOrEmpty(cmd)) return;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        _autoResetEvent.WaitOne();
        _buffer.Clear();
        try
        {
            _serialPort.WriteLine(cmd);
        }
        finally
        {
            _autoResetEvent.Set();
        }
    }

    public void Close()
    {
        _serialPort.Close();
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        IsConnected = false;
        if (_disposed) return;
        if (disposing) _serialPort?.Dispose();
        _disposed = true;
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (_serialPort?.IsOpen != true) return;
        try
        {
            var str = _serialPort.ReadExisting();
            if (_isSync)
            {
                var enumerable = _buffer.Append(str);
            }

            DataReceived?.Invoke(this, str);
        }
        catch
        {
        }
    }
}