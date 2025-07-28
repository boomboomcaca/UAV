/*********************************************************************************************
 *
 * 文件名称:    ..\Tracker800\Server\Source\Device\Decoder\S7000\Client.cs
 *
 * 作    者:	王 喜 进
 *
 * 创作日期:    2018/06/28
 *
 * 修    改:
 *
 * 备    注:	此模块实现与设备的通信、控制、数据传输等功能。
 *
 *********************************************************************************************/

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device;

/// <summary>
///     连接客户端类
/// </summary>
public class Client
{
    /// <summary>
    ///     S7000 IP地址
    /// </summary>
    private readonly string _ip;

    /// <summary>
    ///     查找到一个稳定的频道
    /// </summary>
    public readonly Action<Exception> OnException = null;

    /// <summary>
    ///     接收数据缓存
    /// </summary>
    private byte[] _buffer;

    ///// <summary>
    /////     是否接收到数据
    ///// </summary>
    //private bool _receive = false;

    ///// <summary>
    /////     接收到的数据
    ///// </summary>
    //private byte[] _receiveResult;

    /// <summary>
    ///     客户端连接套接字
    /// </summary>
    private Socket _socket;

    /// <summary>
    ///     构造函数
    /// </summary>
    /// <param name="ip"></param>
    public Client(string ip)
    {
        _ip = ip;
        _buffer = new byte[1024 * 100];
    }

    /// <summary>
    ///     套接字属性，暴露给外部（心跳检查）使用
    /// </summary>
    public Socket Socket => _socket;

    /// <summary>
    ///     与S7000建立网络连接：在初始化时调用或者在设备出现返回空数据时调用
    /// </summary>
    /// <returns>true=连接成功；false=连接失败</returns>
    public bool Connect()
    {
        try
        {
            // 关闭已存在的连接，再重新建立连接
            CloseConnected();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = true,
                ReceiveTimeout = 10000
            };
            _socket.Connect(IPAddress.Parse(_ip), S7000Protocol.Port);
            return _socket.Connected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"连接S7000失败：{ex.Message}");
            if (ex is SocketException && OnException != null) OnException(ex);
            return false;
        }
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        CloseConnected();
        _buffer = null;
        //_receiveResult = null;
    }

    /// <summary>
    ///     发送指令
    /// </summary>
    /// <param name="data">指令</param>
    /// <param name="recvData">是否接收指令回复</param>
    /// <param name="sleep">暂停时间</param>
    /// <param name="async">同步异步标志</param>
    /// <returns>指令响应回复</returns>
    public byte[] Send(byte[] data, bool recvData = false, int sleep = 0, bool async = true)
    {
        try
        {
            byte[] rec = null;
            Console.WriteLine($"Send Data ==>:{Encoding.UTF8.GetString(data)}");
            _socket.Send(data);
            if (recvData)
            {
                rec = Receive(async);
                Console.WriteLine($"Recv Data <==:{Utils.GetGb2312String(rec)}");
            }

            if (sleep > 0)
                // Thread.Sleep(sleep);
                Task.Delay(sleep).ConfigureAwait(false).GetAwaiter().GetResult();
            // 重置心跳检查计时器
            S7000.ResetSeconds();
            return rec;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SendError:{ex.Message}");
            if (ex is SocketException && OnException != null) OnException(ex);
            return null;
        }
    }

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="async">同步标志</param>
    /// <returns>接收到的的数据，没有数据则返回Null</returns>
    public byte[] Receive(bool async)
    {
        return RecvData();
        //if (async)
        //{
        //    return RecvDataAsync();
        //}
        //else
        //{
        //    return RecvData();
        //}
    }

    /// <summary>
    ///     同步接收数据
    /// </summary>
    /// <returns>接收到的的数据，没有数据则返回Null</returns>
    private byte[] RecvData()
    {
        try
        {
            var length = _socket.Receive(_buffer);
            if (length == 0) return Array.Empty<byte>();
            var buff = new byte[length];
            Buffer.BlockCopy(_buffer, 0, buff, 0, length);
            return buff;
        }
        catch (Exception ex)
        {
            if (ex is SocketException && OnException != null)
                // 由于连接方在一段时间后没有正确答复或连接的主机没有反应，连接尝试失败。
                // 出现的异常基本上都是接收数据超时。
                OnException(ex);
            Console.WriteLine($"RecvError:{ex.Message}");
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    ///     关闭已有连接
    /// </summary>
    private void CloseConnected()
    {
        if (_socket?.Connected == true)
        {
            _socket.Close();
            _socket.Dispose();
            _socket = null;
        }
    }
}