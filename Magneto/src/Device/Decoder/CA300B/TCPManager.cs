using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Magneto.Device.CA300B;

public class TcpManager
{
    #region 成员变量定义

    /// <summary>
    ///     TCP Socket
    /// </summary>
    private Socket _tcpSocket;

    /// <summary>
    ///     获取网络套接字
    /// </summary>
    public Socket TcpSocket => _tcpSocket;

    /// <summary>
    ///     数据接收缓冲区
    /// </summary>
    private readonly byte[] _buffer = new byte[1024 * 100];

    /// <summary>
    ///     是否接收到数据
    /// </summary>
    private bool _receive;

    /// <summary>
    ///     接收到的数据
    /// </summary>
    private byte[] _receiveResult;

    private string _ip = string.Empty;
    private int _port;

    #endregion

    #region 公有函数

    /// <summary>
    ///     初始化TCP连接
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public bool InitTcpSocket(string ip, int port)
    {
        CloseConnected();
        _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _tcpSocket.Connect(IPAddress.Parse(ip), port);
        _tcpSocket.ReceiveTimeout = 20000;
        _tcpSocket.NoDelay = true;
        _ip = ip;
        _port = port;
        return _tcpSocket.Connected;
    }

    /// <summary>
    ///     设置时间超时值
    /// </summary>
    /// <param name="time"></param>
    public void SetReceiveTimeout(int time)
    {
        _tcpSocket.ReceiveTimeout = time;
    }

    /// <summary>
    ///     重连
    /// </summary>
    public bool Reconnect()
    {
        return InitTcpSocket(_ip, _port);
    }

    /// <summary>
    ///     释放资源
    /// </summary>
    public void Dispose()
    {
        CloseConnected();
    }

    /// <summary>
    ///     发送数据
    /// </summary>
    /// <param name="data">数据内容</param>
    /// <param name="isRecv">是否等待数据返回</param>
    /// <param name="async"></param>
    /// <param name="sleep">延迟发送</param>
    /// <param name="isCmdStr"></param>
    public byte[] Send(byte[] data, bool isRecv = false, bool async = true, int sleep = 0, bool isCmdStr = true)
    {
        if (data?.Any() != true) return null;
        Trace.WriteLine(isCmdStr ? $"==>{Encoding.ASCII.GetString(data)}" : $"==>{BitConverter.ToString(data)}");
        byte[] recData = null;
        if (sleep > 0) Thread.Sleep(sleep);
        _tcpSocket.Send(data);
        if (isRecv) recData = Receive(async);
        return recData;
    }

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="async">同步标志</param>
    /// <returns>接收到的的数据，没有数据则返回Null</returns>
    public byte[] Receive(bool async = true)
    {
        var recvData = async ? AsyncRecvData() : SyncRecvData();
        if (recvData?.Any() == true) Trace.WriteLine($"-->> {BitConverter.ToString(recvData)}");
        return recvData;
    }

    #endregion

    #region 私有函数

    /// <summary>
    ///     同步接收数据
    /// </summary>
    private byte[] AsyncRecvData()
    {
        try
        {
            var length = _tcpSocket.Receive(_buffer, SocketFlags.None);
            if (length > 0)
            {
                var buff = new byte[length];
                Buffer.BlockCopy(_buffer, 0, buff, 0, length);
                return buff;
            }

            return null;
        }
        catch (Exception ex)
        {
            if (ex is SocketException)
            {
                // 由于连接方在一段时间后没有正确答复或连接的主机没有反应，连接尝试失败。
                // 出现的异常基本上都是接收数据超时。
            }

            return null;
        }
    }

    /// <summary>
    ///     接收数据
    /// </summary>
    /// <param name="recvBuffer">数据缓冲区</param>
    /// <param name="timeout"></param>
    /// <param name="isSelfCheckInfo">是否是接收自检信息</param>
    /// <param name="freq">频率</param>
    public int ReceiveDataEx(byte[] recvBuffer, int timeout = 500, bool isSelfCheckInfo = true, float freq = 0)
    {
        var totalLen = 0;
        var isRevcOver = false;
        var tmpBuffer = new byte[4096];
        try
        {
            int recvLen;
            if (isSelfCheckInfo)
            {
                SetReceiveTimeout(timeout);
                while (true) recvLen = _tcpSocket.Receive(recvBuffer, SocketFlags.None);
            }

            if (freq == 0) recvLen = _tcpSocket.Receive(tmpBuffer, SocketFlags.None);
            while (!isRevcOver)
            {
                recvLen = _tcpSocket.Receive(tmpBuffer, SocketFlags.None);
                isRevcOver = tmpBuffer[recvLen - 2] == 0x0D && tmpBuffer[recvLen - 1] == 0x0A;
                Array.Copy(tmpBuffer, 0, recvBuffer, totalLen, recvLen);
                totalLen += recvLen;
            }
        }
        catch
        {
            SetReceiveTimeout(15 * 1000);
        }

        if (recvBuffer?.Any() == true && totalLen > 0)
            Trace.WriteLine($"-->> {BitConverter.ToString(recvBuffer, 0, Math.Min(totalLen, recvBuffer.Length))}");
        return totalLen;
    }

    /// <summary>
    ///     读取指定长度的数据到数组
    /// </summary>
    /// <param name="recvBuffer">接收数据缓冲区</param>
    /// <param name="offset">缓冲区的偏移</param>
    /// <param name="bytesToRead">要读取的字节数</param>
    /// <param name="endFlag">结束标识</param>
    public int ReceiveData(byte[] recvBuffer, int offset, int bytesToRead = 0, string endFlag = "")
    {
        //当前已接收到的字节数
        var totalRecvLen = 0;
        if (bytesToRead > 0)
        {
            //循环接收数据，确保接收完指定字节数
            while (totalRecvLen < bytesToRead)
            {
                var recvLen = _tcpSocket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen,
                    SocketFlags.None);
                if (recvLen <= 0)
                    //远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候此处不会抛异常而是立即返回0，
                    //为避免出现此情况将导致该函数死循环，此处直接抛SocketException异常
                    //10054:远程主机强迫关闭了一个现有连接
                    throw new SocketException(10054);
                totalRecvLen += recvLen;
            }
        }
        else
        {
            var strRecEnd = string.Empty;
            while (endFlag != strRecEnd)
            {
                var recvLen = _tcpSocket.Receive(recvBuffer, SocketFlags.None);
                if (recvLen > 0) strRecEnd = Encoding.Default.GetString(recvBuffer, 0, recvLen);
            }
        }

        return totalRecvLen;
    }

    /// <summary>
    ///     数据接收回调
    /// </summary>
    /// <param name="ar">异步操作状态</param>
    private void DataCallback(IAsyncResult ar)
    {
        try
        {
            var s = (Socket)ar.AsyncState;
            if (s != null)
            {
                var length = s.EndReceive(ar);
                if (length > 0)
                {
                    _receiveResult = new byte[length];
                    Buffer.BlockCopy(_buffer, 0, _receiveResult, 0, length);
                    _receive = true;
                }
            }
        }
        catch (Exception ex)
        {
            // System.ObjectDisposedException:“无法访问已释放的对象。ObjectDisposed_ObjectName_Name”
            Console.WriteLine(ex);
        }
    }

    /// <summary>
    ///     异步接收数据
    /// </summary>
    private byte[] SyncRecvData()
    {
        if (_tcpSocket?.Connected != true) return null;
        var timer = 0;
        _receive = false;
        _receiveResult = null;
        _tcpSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, DataCallback, _tcpSocket);
        while (timer < 10000)
            if (!_receive)
            {
                Thread.Sleep(1);
                timer++;
            }
            else
            {
                break;
            }

        return _receiveResult;
    }

    /// <summary>
    ///     关闭已有连接
    /// </summary>
    private void CloseConnected()
    {
        if (_tcpSocket?.Connected == true)
        {
            _tcpSocket.Close();
            _tcpSocket.Dispose();
            _tcpSocket = null;
        }
    }

    #endregion
}