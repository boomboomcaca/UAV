using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Define;

namespace Magneto.Device.RFeyeGps;

public partial class RFeyeGps : DeviceBase
{
    private CancellationTokenSource _dataCts;
    private Task _dataTask;
    private Socket _socket;

    public RFeyeGps(Guid deviceId) : base(deviceId)
    {
    }

    public override bool Initialized(ModuleInfo device)
    {
        var result = base.Initialized(device);
        if (result)
        {
            ReleaseResource();
            InitSocket();
            InitTask();
        }

        return result;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        ReleaseResource();
    }
}