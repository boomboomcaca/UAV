using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.G213;

public partial class G213
{
    //private bool _alarm = false;

    private void ReleaseResource()
    {
        _running = false;
        Utils.CancelTask(_tcpDataProcessTask, _tcpDataProcessCts);
        Utils.CancelTask(_sendDataAsyncTask, _sendDataAsyncCts);
        _udpServer?.Dispose();
        _sendDataCache.Clear();
    }

    private void InitSocket()
    {
        using var ping = new Ping();
        try
        {
            var res = ping.Send(SlaveIp0, 500);
            if (res != null && res.Status != IPStatus.Success) throw new Exception($"与设备{SlaveIp0}连接失败");
        }
        catch (Exception)
        {
            throw new Exception($"与设备{SlaveIp0}连接失败");
        }

        try
        {
            var res = ping.Send(SlaveIp1, 500);
            if (res != null && res.Status != IPStatus.Success) throw new Exception($"与设备{SlaveIp0}连接失败");
        }
        catch (Exception)
        {
            throw new Exception($"与设备{SlaveIp1}连接失败");
        }

        _udpServer = new UdpClient(SlavePort, AddressFamily.InterNetwork);
        _udpServer.Client.ReceiveTimeout = 20000;
    }

    private void InitTasks()
    {
        _tcpDataProcessCts = new CancellationTokenSource();
        _tcpDataProcessTask = new Task(TcpDataProcessProc);
        _tcpDataProcessTask.Start();
        _sendDataAsyncCts = new CancellationTokenSource();
        _sendDataAsyncTask = new Task(SendDataProc);
        _sendDataAsyncTask.Start();
    }

    /// <summary>
    ///     用于发送设置类指令
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="deviceId"></param>
    private void SendCmd(byte[] cmd, int deviceId)
    {
        var ip = deviceId == 0 ? SlaveIp0 : SlaveIp1;
        var port = deviceId == 0 ? SlavePort0 : SlavePort1;
        var iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        _udpServer.Send(cmd, cmd.Length, iPEndPoint);
    }
}