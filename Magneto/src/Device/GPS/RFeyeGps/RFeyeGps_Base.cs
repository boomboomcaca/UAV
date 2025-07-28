using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.RFeyeGps;

public partial class RFeyeGps
{
    private void ReleaseResource()
    {
        Utils.CancelTask(_dataTask, _dataCts);
        Utils.CloseSocket(_socket);
    }

    private void InitSocket()
    {
        //创建TCP连接
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        _socket.Connect(ep);
        //建立于设备的连接认证，否则会被设备端主动关闭
        CreateAuthentication(_socket);
        SetHeartBeat(_socket);
    }

    private void InitTask()
    {
        _dataCts = new CancellationTokenSource();
        _dataTask = new Task(DataProc, _dataCts.Token);
        _dataTask.Start();
    }

    /// <summary>
    ///     与设备建立认证关系
    /// </summary>
    /// <param name="client"></param>
    private static void CreateAuthentication(Socket client)
    {
        client.ReceiveTimeout = 5000;
        var state = ClientConnectionState.TcpConnectionEstablished;
        var recvBuffer = new byte[1024 * 1024];
        while (state != ClientConnectionState.ConnectionActive)
        {
            ReceiveData(recvBuffer, 0, DataSize.HeaderSize << 2, client);
            var header = Header.Parse(recvBuffer, 0);
            if (header.HeaderCode != 0xAABBCCDD) continue;
            ReceiveData(recvBuffer, DataSize.HeaderSize << 2, ((int)header.PacketSize - DataSize.HeaderSize) << 2,
                client);
            var footerCode = BitConverter.ToUInt32(recvBuffer, ((int)header.PacketSize - 1) << 2);
            if (footerCode != 0xDDCCBBAA) continue;
            if (header.PacketTypeData == (int)PacketType.Link)
            {
                var packet = Packet.Parse(recvBuffer, 0);
                foreach (var field in packet.ListFieldInfo)
                {
                    switch (field.FieldName)
                    {
                        case PacketKey.LinkFieldServerGreeting:
                            state = ClientConnectionState.ReceivedGreeting;
                            break;
                        case PacketKey.LinkFieldServerAuthReq:
                            state = ClientConnectionState.ReceivedAuthenticationRequest;
                            break;
                        case PacketKey.LinkFieldServerCobfirm:
                            state = ClientConnectionState.ReceivedAuthenticationOk;
                            break;
                    }

                    SetConnectionState(ref state, client);
                }
            }
        }

        client.ReceiveTimeout = -1;
    }

    private static void SetConnectionState(ref ClientConnectionState recvState, Socket client)
    {
        switch (recvState)
        {
            case ClientConnectionState.ReceivedGreeting:
            {
                var linkPacket = new Packet();
                linkPacket.BeginPacket(PacketType.Link, -1);
                linkPacket.AddField(PacketKey.LinkFieldClientConnReq, 0);
                linkPacket.AddParamString(PacketKey.LinkParamClientId, Dns.GetHostName());
                linkPacket.EndPacket();
                SendPacket(linkPacket, client);
                recvState = ClientConnectionState.SentClientConnectionRequest;
                break;
            }
            case ClientConnectionState.ReceivedAuthenticationRequest:
            {
                var linkPacket = new Packet();
                linkPacket.BeginPacket(PacketType.Link, -1);
                linkPacket.AddField(PacketKey.LinkFieldClientAuthResp, 0);
                linkPacket.AddParamString(PacketKey.LinkParamClientAuth, "Client Authentication Word");
                linkPacket.EndPacket();
                SendPacket(linkPacket, client);
                recvState = ClientConnectionState.SentAuthenticationResponse;
                break;
            }
            case ClientConnectionState.ReceivedAuthenticationOk:
                recvState = ClientConnectionState.ConnectionActive;
                break;
        }
    }

    /// <summary>
    ///     发送数据请求
    /// </summary>
    /// <param name="packet">数据请求</param>
    /// <param name="socket">要发送数据的套接字</param>
    private static void SendPacket(Packet packet, Socket socket)
    {
        var packetBuffer = packet.GetBytes();
        //当前待发送数据请求包的长度（字节数）
        var packetLen = packetBuffer.Length;
        //总共已发送的字节数
        var totalSentLen = 0;
        //循环发送，确保数据全部发送完毕
        while (totalSentLen < packetLen)
        {
            var sentLen = socket.Send(packetBuffer, totalSentLen, packetLen - totalSentLen, SocketFlags.None);
            totalSentLen += sentLen;
        }
    }

    /// <summary>
    ///     接收数据包
    /// </summary>
    /// <param name="socket"></param>
    /// <returns>可能为null(如收到无效数据等)</returns>
    private static Packet ReceivePacket(Socket socket)
    {
        var recvBuffer = new byte[4];
        //收取数据头并校验
        ReceiveData(recvBuffer, 0, 4, socket);
        var headerCode = BitConverter.ToUInt32(recvBuffer, 0);
        if (headerCode != 0xAABBCCDD) return null;
        //收取包头并解析
        Array.Resize(ref recvBuffer, DataSize.HeaderSize << 2);
        ReceiveData(recvBuffer, 4, (DataSize.HeaderSize - 1) << 2, socket);
        var header = Header.Parse(recvBuffer, 0);
        if (header.PacketSize <= DataSize.HeaderSize) return null;
        //收取剩下的数据包,并校验包尾
        Array.Resize(ref recvBuffer, (int)header.PacketSize << 2);
        ReceiveData(recvBuffer, DataSize.HeaderSize << 2, ((int)header.PacketSize - DataSize.HeaderSize) << 2, socket);
        var footerCode = BitConverter.ToUInt32(recvBuffer, recvBuffer.Length - 4);
        if (footerCode != 0xDDCCBBAA) return null;
        //解析数据包并返回
        var responsePacket = Packet.Parse(recvBuffer, 0);
        return responsePacket;
    }

    /// <summary>
    ///     读取指定长度的数据到数组
    /// </summary>
    /// <param name="recvBuffer">接收数据缓冲区</param>
    /// <param name="offset">缓冲区的偏移</param>
    /// <param name="bytesToRead">要读取的字节数</param>
    /// <param name="socket">要接收数据的套接字</param>
    private static void ReceiveData(byte[] recvBuffer, int offset, int bytesToRead, Socket socket)
    {
        //当前已接收到的字节数
        var totalRecvLen = 0;
        //循环接收数据，确保接收完指定字节数
        while (totalRecvLen < bytesToRead)
        {
            var recvLen = socket.Receive(recvBuffer, offset + totalRecvLen, bytesToRead - totalRecvLen,
                SocketFlags.None);
            if (recvLen <= 0)
                //远程主机使用close或shutdown关闭连接，并且所有数据已被接收的时候此处不会抛异常而是立即返回0，
                //为避免出现此情况将导致该函数死循环，此处直接抛SocketException异常
                //10054:远程主机强迫关闭了一个现有连接
                throw new SocketException(10054);
            totalRecvLen += recvLen;
        }
    }
}