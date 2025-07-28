using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.EBD195.IO;

public class TcpClient : IClient
{
    private const int ReceiveBufferSize = 4 * 1024;
    private const int SendBufferSize = 1024;
    private readonly AutoResetEvent _autoResetEvent;
    private readonly Task _heartbeatTask = null;
    private readonly CancellationTokenSource _heartbeatTokenSource = null;
    private readonly string _ip;
    private readonly int _port;
    private bool _disposed;
    private Task _receiveTask;
    private CancellationTokenSource _receiveTokenSource;

    /// <summary>
    ///     发送命令套接字
    /// </summary>
    private Socket _socket;

    public TcpClient(string ip, int port)
    {
        _ip = ip;
        _port = port;
        _autoResetEvent = new AutoResetEvent(true);
    }

    public bool IsConnected { get; private set; }

    public int BytesToRead => 1;

    //上次从数据接口读取到数据的时间，心跳检测使用
    public DateTime LastGetDataTime { get; set; } = DateTime.Now;
    public event EventHandler<string> DataReceived;

    public void DiscardInBuffer()
    {
    }

    public bool Init(out string err)
    {
        LastGetDataTime = DateTime.Now;
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
        return true;
    }

    public void SendCmd(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return;
        if (!cmd.EndsWith("\r\n")) cmd += "\r\n";
        if (_socket?.Connected != true || string.IsNullOrEmpty(cmd)) return;
        _autoResetEvent.WaitOne();
        try
        {
            _socket.Send(Encoding.Default.GetBytes(cmd));
        }
        finally
        {
            _autoResetEvent.Set();
        }
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
                if (_socket.Available == 0)
                {
                    Thread.Sleep(50);
                    continue;
                }

                var buffer = new byte[ReceiveBufferSize];
                var cnt = _socket.Receive(buffer);
                if (cnt > 0)
                {
                    var s = Encoding.Default.GetString(buffer, 0, cnt);
                    DataReceived?.Invoke(this, s);
                    LastGetDataTime = DateTime.Now;
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
}