using System.Net.Sockets;

namespace Magneto.Device.EthAntController;

public partial class EthAntController
{
    /// <summary>
    ///     初始化网络
    /// </summary>
    private void InitNetworks()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };
        // 设置TCP心跳选项，用于检测物理网络连接状态
        // var bytes = new byte[12];
        // BitConverter.GetBytes(1).CopyTo(bytes, 0);
        // BitConverter.GetBytes(1000).CopyTo(bytes, 4);
        // BitConverter.GetBytes(500).CopyTo(bytes, 8);
        // _socket.IOControl(IOControlCode.KeepAliveValues, bytes, null);
        _socket.Connect(Ip, Port);
    }

    /// <summary>
    ///     关闭套接字
    /// </summary>
    private void ReleaseSocket()
    {
        if (_socket == null) return;
        _socket.Close();
        _socket = null;
    }
}