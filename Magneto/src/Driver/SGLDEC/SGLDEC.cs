using System;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Define;

namespace Magneto.Driver.SGLDEC;

public partial class Sgldec : DriverBase
{
    public Sgldec(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        var success = base.Start(dataPort, mediaType);
        (Decoder as DeviceBase)?.Start(FeatureType.SGLDEC, dataPort);
        return success;
    }

    public override bool Stop()
    {
        (Decoder as DeviceBase)?.Stop();
        return base.Stop();
    }
}