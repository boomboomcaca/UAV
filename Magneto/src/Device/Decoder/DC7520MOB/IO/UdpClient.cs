using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.DC7520MOB.IO;

public class UdpClient : IClient
{
    private const int ReceiveBufferSize = 2 * 1024 * 1024;
    private readonly AutoResetEvent _autoResetEvent;
    private readonly ConcurrentQueue<string> _buffer;
    private readonly string _ip;
    private readonly int _port;
    private bool _disposed;
    private volatile bool _isSync;
    private Task _receiveTask;
    private CancellationTokenSource _receiveTokenSource;

    /// <summary>
    ///     发送命令及接收数据套接字
    /// </summary>
    private Socket _socket;

    public UdpClient(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _buffer = new ConcurrentQueue<string>();
        _autoResetEvent = new AutoResetEvent(true);
    }

    public int Timeout { get; set; } = 45000;
    public event EventHandler<string> DataReceived;
    public event EventHandler<bool> ConnectionChanged;

    public bool Init(bool[] channels, out string err)
    {
        err = null;
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                SendTimeout = 3000,
                ReceiveTimeout = 3000,
                ReceiveBufferSize = ReceiveBufferSize
            };
            ConnectionChanged?.Invoke(this, true);
        }
        catch (SocketException)
        {
            err = "初始化UDP通信异常";
            return false;
        }

        _receiveTokenSource = new CancellationTokenSource();
        _receiveTask = new Task(ReceiveData, _receiveTokenSource.Token);
        _receiveTask.Start();
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

    public bool SendSyncCmd(string cmd, out string recv)
    {
        recv = null;
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return false;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        var sb = new StringBuilder();
        var result = false;
        _autoResetEvent.WaitOne();
        _isSync = true;
        _buffer.Clear();
        var stopWatch = new Stopwatch();
        try
        {
            var msg = Encoding.Default.GetBytes(cmd);
            var ip = new IPEndPoint(IPAddress.Parse(_ip), _port);
            _socket.SendTo(msg, ip);
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

            _isSync = false;
            stopWatch.Stop();
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

    public bool SendCommands(string[] cmds, out List<string> recv)
    {
        recv = new List<string>();
        if (cmds?.Any() != true) return false;
        if (_socket?.Connected != true) return false;
        _autoResetEvent.WaitOne();
        _buffer.Clear();
        var result = false;
        var stopWatch = new Stopwatch();
        try
        {
            foreach (var command in cmds)
            {
                var sb = new StringBuilder();
                var cmd = command;
                if (cmd == null) return false;
                if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
                _isSync = true;
                _buffer.Clear();
                stopWatch.Reset();
                _socket.Send(Encoding.Default.GetBytes(cmd));
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
            }

            _isSync = false;
        }
        finally
        {
            _isSync = false;
            stopWatch.Reset();
            _autoResetEvent.Set();
        }

        return result;
    }

    public void SendCommand(string cmd)
    {
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        _buffer.Clear();
        var msg = Encoding.Default.GetBytes(cmd);
        var ip = new IPEndPoint(IPAddress.Parse(_ip), _port);
        _autoResetEvent.WaitOne();
        try
        {
            _socket.SendTo(msg, ip);
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
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return (false, null);
        var sb = new StringBuilder();
        var result = false;
        _autoResetEvent.WaitOne();
        _isSync = true;
        _buffer.Clear();
        var stopWatch = new Stopwatch();
        var ip = new IPEndPoint(IPAddress.Parse(_ip), _port);
        try
        {
            await _socket.SendToAsync(Encoding.Default.GetBytes(cmd), SocketFlags.None, ip, token)
                .ConfigureAwait(false);
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
        if (_socket?.Connected != true) return (false, null);
        _autoResetEvent.WaitOne();
        var result = false;
        var ip = new IPEndPoint(IPAddress.Parse(_ip), _port);
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
                await _socket.SendToAsync(Encoding.Default.GetBytes(cmd), SocketFlags.None, ip, token)
                    .ConfigureAwait(false);
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

    ~UdpClient()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
        }

        Utils.CancelTask(_receiveTask, _receiveTokenSource);
        _socket?.Dispose();
        _disposed = true;
    }

    private void ReceiveData()
    {
        while (_receiveTokenSource?.IsCancellationRequested == false)
            try
            {
                var buffer = new byte[ReceiveBufferSize];
                EndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                var cnt = _socket.ReceiveFrom(buffer, SocketFlags.None, ref sender);
                if (cnt > 0)
                {
                    var s = Encoding.Default.GetString(buffer, 0, cnt);
                    if (_isSync) _buffer.Enqueue(s);
                    DataReceived?.Invoke(this, s);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                Thread.Sleep(20);
            }
    }
}