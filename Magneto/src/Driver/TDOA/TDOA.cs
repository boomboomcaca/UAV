using System;
using System.Collections.Generic;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.TDOA;

public partial class Tdoa : DriverBase
{
    private short _factor;
    private double _frequency;

    public Tdoa(Guid driverId) : base(driverId)
    {
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        CanPause = false;
        if (!base.Start(dataPort, mediaType)) return false;
        (Receiver as DeviceBase)?.Start(FeatureType.TDOA, this);
        return true;
    }

    public override bool Stop()
    {
        (Receiver as DeviceBase)?.Stop();
        return base.Stop();
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency && AntennaController is IAntennaController antennaController
                                             && double.TryParse(value.ToString(), out var freq))
            antennaController.Frequency = freq;
    }

    public override void OnData(List<object> data)
    {
        SendData(data);
        var iq = (SDataIq)data.Find(item => item is SDataIq);
        if (iq != null && !Utils.IsNumberEquals(_frequency, iq.Frequency)
                       && AntennaController is IAntennaController antennaController)
        {
            _frequency = iq.Frequency;
            _factor = antennaController.GetFactor(_frequency);
            var factor = new SDataFactor
            {
                Data = new short[1]
            };
            factor.Data[0] = _factor;
            data.Insert(0, factor);
        }
    }
}