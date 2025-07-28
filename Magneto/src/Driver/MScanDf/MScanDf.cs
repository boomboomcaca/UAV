using System;
using System.Collections.Generic;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.MScanDf;

public partial class MScanDf : DriverBase
{
    public MScanDf(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        (Receiver as DeviceBase)?.Start(FeatureType.MScanDf, this);
        // System.Threading.Tasks.Task.Run(DataProcess);
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        (Receiver as DeviceBase)?.Stop();
        return true;
    }

    public override void OnData(List<object> data)
    {
        if (data.Exists(item => item is SDataMScanDf)) CanPause = true;
        SendData(data);
    }
}