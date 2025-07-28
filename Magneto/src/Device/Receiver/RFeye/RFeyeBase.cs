using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.RFeye;

public partial class RFeye
{
    private void InitResources()
    {
        //检查非托管资源并释放
        ReleaseResources();
        //初始化用于与设备通信的套接字
        InitNetworks();
        //初始化线程
        InitTasks();
        //初始化ITU计算类
        _itu = new Itu();
        _itu.Initialize();
        //初始成功后再启动心跳检查线程
        SetHeartBeat(_tcpSocket);
    }

    #region 释放资源

    private void ReleaseResources()
    {
        ReleaseTasks();
        ReleaseNetworks();
        ReleaseQueues();
        _itu?.Dispose();
    }

    private void ReleaseNetworks()
    {
        Utils.CloseSocket(_tcpSocket);
        Utils.CloseSocket(_gpsSocket);
    }

    private void ReleaseQueues()
    {
        _dataQueue?.Clear();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_dataCaptureTask, _dataCaptureTokenSource);
        Utils.CancelTask(_dataProcessTask, _dataProcessTokenSource);
        Utils.CancelTask(_gpsProcessTask, _gpsProcessTokenSource);
        Utils.CancelTask(_freqsSwitchTask, _freqsSwitchTokenSource);
    }

    #endregion

    #region 初始化

    private void InitNetworks()
    {
        var ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        _tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _tcpSocket.Connect(ep);
        _tcpSocket.NoDelay = true;
        //发送客户端连接认证(只有发送了该消息，设备才会返数据)
        var linkPacket = new Packet();
        linkPacket.BeginPacket(PacketType.Link, -1);
        linkPacket.AddField(PacketKey.LinkFieldClientAuthResp, 0);
        linkPacket.EndPacket();
        SendPacket(linkPacket, _tcpSocket);
        if (!GpsSwitch) return;
        _gpsSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _gpsSocket.Connect(ep);
        _gpsSocket.NoDelay = true;
        SendPacket(linkPacket, _gpsSocket);
    }

    private void InitTasks()
    {
        _dataCaptureTokenSource = new CancellationTokenSource();
        _dataCaptureTask = new Task(DataCaptureProc, _dataCaptureTokenSource.Token);
        _dataCaptureTask.Start();
        _dataProcessTokenSource = new CancellationTokenSource();
        _dataProcessTask = new Task(DataProcessProc, _dataProcessTokenSource.Token);
        _dataProcessTask.Start();
        if (!GpsSwitch) return;
        _gpsProcessTokenSource = new CancellationTokenSource();
        _gpsProcessTask = new Task(GpsProcessProc, _gpsProcessTokenSource.Token);
        _gpsProcessTask.Start();
    }

    #endregion
}