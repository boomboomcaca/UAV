using System;
using System.Collections.Generic;
using Magneto.Contract.BaseClass;
using Magneto.Contract.Interface;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace DPX;

public partial class Dpx : DriverBase
{
    private readonly FluoSpectrumProcessor _fluoSpectrumProcessor;

    public Dpx(Guid functionId) : base(functionId)
    {
        _fluoSpectrumProcessor = new FluoSpectrumProcessor();
        _fluoSpectrumProcessor.ProcessCompleted += FluoDataReceived;
    }

    public override bool Start(IDataPort dataPort, MediaType mediaType)
    {
        if (!base.Start(dataPort, mediaType)) return false;
        _fluoSpectrumProcessor.Start();
        (Receiver as DeviceBase)?.Start(FeatureType.FFM, this);
        return true;
    }

    public override bool Stop()
    {
        base.Stop();
        (Receiver as DeviceBase)?.Stop();
        _fluoSpectrumProcessor.Stop();
        return true;
    }

    public override void SetParameter(string name, object value)
    {
        base.SetParameter(name, value);
        if (name == ParameterNames.Frequency)
        {
            if (AntennaController is IAntennaController antennaController
                && double.TryParse(value.ToString(), out var freq))
                antennaController.Frequency = freq;
            ClearData();
        }

        _fluoSpectrumProcessor.Reset();
    }

    public override void OnData(List<object> data)
    {
        var obj = data.Find(item => item is SDataSpectrum);
        if (obj is not SDataSpectrum spectrum) return;
        _fluoSpectrumProcessor.AddData(spectrum);
    }

    private void FluoDataReceived(object sender, SDataFluoSpec data)
    {
        SendData(new List<object> { data });
    }
}