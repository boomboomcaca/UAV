using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Magneto.Device.TX3010A.Protocol;

public abstract class RadioSuppressingBase : IDisposable
{
    protected readonly string IpAddress;
    protected readonly int Port;
    protected bool Disposed;
    protected bool Init;
    protected string Name;
    protected Socket Socket;

    protected RadioSuppressingBase(string ipAddress, int port, int index)
    {
        IpAddress = ipAddress;
        Port = port;
        Index = index;
    }

    public bool Enabled { get; set; }
    public bool IsInitialized => Init;

    /// <summary>
    ///     设备所属的子系统编号
    ///     用来区分是哪个子系统的设备
    /// </summary>
    public int Index { get; set; }

    public abstract DeviceType DeviceType { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RadioSuppressingBase()
    {
        Dispose(false);
    }

    public virtual bool Initialized()
    {
        try
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true,
                SendTimeout = 5000,
                ReceiveTimeout = 5000 // 避免存在查询操作，造成套接字永久等待
            };
            Socket.Connect(IpAddress, Port);
            SetTcpKeepAlive(Socket);
            Enabled = true;
            return true;
        }
        catch (SocketException)
        {
            Trace.WriteLine($"{Name}连接失败");
            Enabled = false;
            return false;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"{Name}初始化异常，异常信息：{ex}");
            Enabled = false;
            return false;
        }
    }

    public abstract bool Reinitialize();

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed) return;
        Socket?.Dispose();
        Enabled = false;
        Init = false;
        Disposed = true;
    }

    protected void SetTcpKeepAlive(Socket sokcet)
    {
        //设置TCP-keepalive模式，若1秒钟之内没有收到探测包回复则再尝试以500ms为间隔发送10次，如果一直都没有回复则认为TCP连接已经断开（为了检测网线断连等异常情况）
        var bytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0xE8, 0x03, 0x00, 0x00, 0xF4, 0x01, 0x00, 0x00 };
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // 这段代码在linux下会报错Socket.IOControl handles Windows-specific control codes and is not supported
            sokcet.IOControl(IOControlCode.KeepAliveValues, bytes, null);
        }
        else
        {
            sokcet.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            sokcet.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);
            sokcet.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 1);
            sokcet.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }
    }

    #region 指令发送

    /// <summary>
    ///     用于发送设置类指令
    /// </summary>
    /// <param name="cmd"></param>
    protected void SendCmd(string cmd)
    {
        if (Socket?.Connected != true) return;
        Trace.WriteLine($"{Name}==> {cmd}");
        var buffer = Encoding.ASCII.GetBytes(cmd + "\r\n");
        Socket.Send(buffer);
    }

    /// <summary>
    ///     用于发送查询类指令并获取查询结果
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns>查询结果</returns>
    protected string SendSyncCmd(string cmd)
    {
        var sendBuffer = Encoding.ASCII.GetBytes(cmd + "\r\n");
        Socket.Send(sendBuffer);
        var buffer = new byte[1024];
        var recvCount = Socket.Receive(buffer, SocketFlags.None);
        var result = string.Empty;
        if (recvCount > 1)
        {
            if (buffer[recvCount - 2] == '\r')
                result = Encoding.ASCII.GetString(buffer, 0, recvCount - 2);
            else
                result = Encoding.ASCII.GetString(buffer, 0, recvCount);
        }

        return result;
    }

    #endregion
}

public class DeviceDisconnectedEventArgs : EventArgs
{
    public DeviceDisconnectedEventArgs(int subsystemIndex, int channelNumber, int deviceType)
    {
        SubsystemIndex = subsystemIndex;
        ChannelNumber = channelNumber;
        DeviceType = deviceType;
    }

    public DeviceDisconnectedEventArgs(int subsystemIndex, int channelNumber) : this(subsystemIndex, channelNumber, 1)
    {
    }

    public DeviceDisconnectedEventArgs(int subsystemIndex) : this(subsystemIndex, 0, 0)
    {
    }

    public int SubsystemIndex { get; }
    public int ChannelNumber { get; }
    public int DeviceType { get; }
}