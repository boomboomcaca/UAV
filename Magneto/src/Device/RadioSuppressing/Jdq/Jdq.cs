using System;
using System.Net.Sockets;
using System.Threading;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device.Jdq;

public partial class Jdq(Guid deviceId) : DeviceBase(deviceId)
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TcpClient _tcpClient = new();

    public override bool Initialized(ModuleInfo device)
    {
        var re = base.Initialized(device);

        if (IsDemo) return re;
        _tcpClient.Connect(IpAddress, 50000);
        _tcpClient.Client.Send(new byte[] { 0xCC, 0xDD, 0xA1, 0x01, 0x00, 0x00, 0xFF, 0xFF, 0xA0, 0x40 });

        return re;
    }

    public override void Dispose()
    {
        _cancellationTokenSource.Cancel();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}