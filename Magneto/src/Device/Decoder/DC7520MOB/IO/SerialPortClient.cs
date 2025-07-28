using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magneto.Device.DC7520MOB.IO;

public class SerialPortClient : IClient
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
    public event EventHandler<bool> ConnectionChanged;
    public int Timeout { get; set; } = 45000;
    public event EventHandler<string> DataReceived;

    public bool Init(bool[] channels, out string err)
    {
        err = null;
        _buffer.Clear();
        try
        {
            _serialPort = new SerialPort(_com, _baudrate)
            {
                WriteTimeout = 3000,
                ReadTimeout = 3000
            };
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            IsConnected = _serialPort.IsOpen;
            ConnectionChanged?.Invoke(this, true);
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

        var sb = new StringBuilder();
        for (var i = 0; i < channels.Length; i++)
            if (channels[i]) // 各通道检查
            {
                var cmd = $"CH{i + 1}:RDID";
                var b = SendSyncCmd(cmd, out _);
                if (!b)
                {
                    var error = $"CH{i + 1}";
                    sb.Append(error).Append(',');
                }
            }

        var errorInfo = sb.ToString();
        if (!string.IsNullOrEmpty(errorInfo))
        {
            err = $"通道初始化失败，DC7520MOB设备异常，异常信息：{errorInfo.TrimEnd(',')}";
            return false;
        }

        return true;
    }

    public bool SendCommands(string[] cmds, out List<string> recv)
    {
        recv = new List<string>();
        if (cmds?.Any() != true) return false;
        if (_serialPort?.IsOpen != true) return false;
        _autoResetEvent.WaitOne();
        _buffer.Clear();
        var result = false;
        var stopWatch = new Stopwatch();
        try
        {
            _isSync = true;
            _buffer.Clear();
            foreach (var command in cmds)
            {
                var sb = new StringBuilder();
                var cmd = command;
                if (cmd == null) return false;
                if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
                stopWatch.Reset();
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
                    var s = sb.ToString();
                    if (s.Contains("OK"))
                    {
                        result = true;
                        break;
                    }

                    if (s.Contains("ERROR"))
                    {
                        result = false;
                        break;
                    }
                }

                stopWatch.Stop();
                if (!result) break;
                recv.Add(sb.ToString());
                _isSync = false;
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

    public bool SendSyncCmd(string cmd, out string recv)
    {
        recv = null;
        if (IsConnected || string.IsNullOrEmpty(cmd)) return false;
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
                var s = sb.ToString();
                if (s.Contains("OK"))
                {
                    result = true;
                    break;
                }

                if (s.Contains("ERROR"))
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

        recv = sb.ToString();
        return result;
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

    public async Task<(bool success, string data)> SendCommandAsync(string cmd, CancellationToken token)
    {
        if (cmd == null) return (false, null);
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        if (_serialPort?.IsOpen != true || string.IsNullOrEmpty(cmd)) return (false, null);
        var sb = new StringBuilder();
        var result = false;
        _autoResetEvent.WaitOne();
        _isSync = true;
        _buffer.Clear();
        var stopWatch = new Stopwatch();
        try
        {
            _serialPort.WriteLine(cmd);
            stopWatch.Start();
            while (stopWatch.ElapsedMilliseconds < Timeout && !token.IsCancellationRequested)
            {
                if (!_buffer.TryDequeue(out var temp))
                {
                    await Task.Delay(20, token).ConfigureAwait(false);
                    continue;
                }

                sb.Append(temp);
                var s = sb.ToString();
                if (s.Contains("OK"))
                {
                    result = true;
                    break;
                }

                if (s.Contains("ERROR"))
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

        var recv = sb.ToString();
        return (result, recv);
    }

    public async Task<(bool success, List<string> datas)> SendCommandsAsync(string[] cmds, CancellationToken token)
    {
        var recv = new List<string>();
        if (cmds?.Any() != true) return (false, null);
        if (_serialPort?.IsOpen != true) return (false, null);
        _autoResetEvent.WaitOne();
        var result = false;
        var stopWatch = new Stopwatch();
        try
        {
            foreach (var command in cmds)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    break;
                }

                var sb = new StringBuilder();
                var cmd = command;
                if (cmd == null) return (false, null);
                if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
                stopWatch.Reset();
                _isSync = true;
                _buffer.Clear();
                _serialPort.WriteLine(cmd);
                stopWatch.Start();
                while (stopWatch.ElapsedMilliseconds < Timeout && !token.IsCancellationRequested)
                {
                    if (!_buffer.TryDequeue(out var temp))
                    {
                        await Task.Delay(20, token).ConfigureAwait(false);
                        continue;
                    }

                    sb.Append(temp);
                    var s = sb.ToString();
                    if (s.Contains("OK"))
                    {
                        result = true;
                        break;
                    }

                    if (s.Contains("ERROR"))
                    {
                        result = false;
                        break;
                    }
                }

                stopWatch.Stop();
                if (!result) break;
                recv.Add(sb.ToString());
                _isSync = false;
            }
        }
        finally
        {
            _isSync = false;
            stopWatch.Reset();
            _autoResetEvent.Set();
        }

        return (result, recv);
    }

    public void Close()
    {
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
            if (_isSync) _ = _buffer.Append(str);
            DataReceived?.Invoke(this, str);
        }
        catch
        {
        }
    }
}