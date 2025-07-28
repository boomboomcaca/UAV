using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.DC7530MOB.IO;

public class TcpClient : IClient
{
    private const int ReceiveBufferSize = 2 * 1024 * 1024;
    private const int SendBufferSize = 1024;
    private readonly AutoResetEvent _autoResetEvent;
    private readonly ConcurrentQueue<string> _buffer;
    private readonly Task _heartbeatTask = null;
    private readonly CancellationTokenSource _heartbeatTokenSource = null;
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

    public TcpClient(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _buffer = new ConcurrentQueue<string>();
        _autoResetEvent = new AutoResetEvent(true);
    }

    public bool IsConnected { get; private set; }
    public int Timeout { get; set; } = 45000;
    public event EventHandler<string> DataReceived;
    public event EventHandler<bool> ConnectionChanged;

    public bool Init(bool[] channels, out string err)
    {
        err = null;
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 3000,
                ReceiveTimeout = 3000,
                ReceiveBufferSize = ReceiveBufferSize,
                SendBufferSize = SendBufferSize,
                NoDelay = true
            };
            _socket.Connect(IPAddress.Parse(_ip), _port);
            IsConnected = _socket.Connected;
        }
        catch (SocketException)
        {
            err = "建立TCP通信异常";
            return false;
        }

        if (!IsConnected)
        {
            err = "TCP连接失败";
            return false;
        }

        SetHeartBeat();
        _receiveTokenSource = new CancellationTokenSource();
        _receiveTask = new Task(ReceiveData, _receiveTokenSource.Token);
        _receiveTask.Start();
        var sb = new StringBuilder();
        for (var i = 0; i < channels.Length; i++)
            if (channels[i]) // 各通道检查
            {
                var cmd = $"CH{i + 1}:RDID\r\n";
                SendCommand(cmd); //第一次有可能没有正确的结果
                Thread.Sleep(1000);
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
            err = $"通道初始化失败，DC7530MOB_5G设备异常，异常信息：{errorInfo.TrimEnd(',')}";
            return false;
        }

        return true;
    }

    public void SendCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return;
        _autoResetEvent.WaitOne();
        _buffer.Clear();
        try
        {
            _socket.Send(Encoding.Default.GetBytes(cmd));
        }
        finally
        {
            _autoResetEvent.Set();
        }
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
                stopWatch.Reset();
                _isSync = true;
                _buffer.Clear();
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
        if (cmd == null) return false;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return false;
        var sb = new StringBuilder();
        var result = false;
        _autoResetEvent.WaitOne();
        _isSync = true;
        var stopWatch = new Stopwatch();
        try
        {
            _buffer.Clear();
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
        try
        {
            await _socket.SendAsync(Encoding.Default.GetBytes(cmd), SocketFlags.None, token).ConfigureAwait(false);
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
                await _socket.SendAsync(Encoding.Default.GetBytes(cmd), SocketFlags.None, token).ConfigureAwait(false);
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

    ~TcpClient()
    {
        Dispose(false);
    }

    protected void Dispose(bool disposing)
    {
        IsConnected = false;
        if (_disposed) return;
        if (disposing)
        {
        }

        _autoResetEvent.Dispose();
        Utils.CancelTask(_receiveTask, _receiveTokenSource);
        Utils.CancelTask(_heartbeatTask, _heartbeatTokenSource);
        _socket?.Dispose();
        _disposed = true;
    }

    private void ReceiveData()
    {
        while (_receiveTokenSource?.IsCancellationRequested == false)
        {
            if (_socket?.Connected != true) break;
            try
            {
                // if (_socket.Available == 0)
                // {
                //     Thread.Sleep(50);
                //     continue;
                // }
                var buffer = new byte[ReceiveBufferSize];
                var cnt = _socket.Receive(buffer);
                if (cnt > 0)
                {
                    var s = Encoding.Default.GetString(buffer, 0, cnt);
                    if (_isSync) _buffer.Enqueue(s);
                    DataReceived?.Invoke(this, s);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode is SocketError.TimedOut or SocketError.NoData)
                {
                    Thread.Sleep(50);
                    continue;
                }

                IsConnected = _socket?.Connected == true;
                ConnectionChanged?.Invoke(this, IsConnected);
                break;
            }
            catch
            {
                Thread.Sleep(20);
            }
        }
    }

    /// <summary>
    ///     设置心跳包
    /// </summary>
    private void SetHeartBeat()
    {
        var bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xE8, 0x03, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00 };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _socket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
        else
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }
        //_heartbeatTokenSource = new CancellationTokenSource();
        //_heartbeatTask = new Task(KeepAlive, _heartbeatTokenSource.Token);
        //_heartbeatTask.Start();
    }
    ///// <summary>
    ///// 心跳检查线程函数
    ///// 实现 tcp 连接的心跳检查；子类可重载此方法，实现其它连接方式的心跳检查
    ///// </summary>
    //private void KeepAlive()
    //{
    //    string cmd = "CH2:AT^RESET\r\n";
    //    byte[] data = Encoding.Default.GetBytes(cmd);
    //    while (_heartbeatTokenSource?.IsCancellationRequested == false)
    //    {
    //        try
    //        {
    //            lock (_syncObj)
    //            {
    //                _socket.Send(data);
    //            }
    //        }
    //        catch (SocketException)
    //        {
    //            break;
    //        }
    //        Thread.Sleep(1000);
    //    }
    //}
}