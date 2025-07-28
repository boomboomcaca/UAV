using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract;

namespace Magneto.Device.ADS_B_PS;

public partial class AdsBPs
{
    private bool InitNetworks()
    {
        var state = true;
        if (TcpAlive)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(IPAddress.Parse(DeviceIp), DevicePort);
            state = _socket.Connected;
            if (_socket.Connected)
            {
                var ip = (_socket.LocalEndPoint as IPEndPoint)?.Address.ToString();
                if (!string.IsNullOrWhiteSpace(ip)) LocalIp = ip;
            }
        }

        var point = new IPEndPoint(IPAddress.Parse(LocalIp), LocalPort);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Bind(point);
        return state;
    }

    private void InitTasks()
    {
        _readDataCts = new CancellationTokenSource();
        _parseDataCts = new CancellationTokenSource();
        _readDataTask = new Task(ReadData, _readDataCts.Token);
        _readDataTask.Start();
        _parseDataTask = new Task(ParseData, _readDataCts.Token);
        _readDataTask.Start();
    }

    private void ReleaseTasks()
    {
        Utils.CancelTask(_readDataTask, _readDataCts);
        Utils.CancelTask(_parseDataTask, _parseDataCts);
    }

    private void ReleaseResources()
    {
        ReleaseTasks();
        Utils.CancelTask(_heartBeatTask, _heartBeatCts);
        Utils.CloseSocket(_socket);
        Utils.CloseSocket(_dataSocket);
    }
}