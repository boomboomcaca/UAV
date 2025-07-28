using System.Collections.Generic;
using System.Linq;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.PCOMS;

public partial class Pcoms
{
    private void StartSuppressing()
    {
        var dev = Suppressor as DeviceBase;
        dev?.Start(FeatureType.PCOMS, this);
    }

    private void StopSuppressing()
    {
        var dev = Suppressor as DeviceBase;
        dev?.Stop();
    }

    private void OnSuppressingData(ref List<object> data)
    {
        if (data.Any(item => item is SDataRadioSuppressing))
            SendData(data.Where(item => item is SDataRadioSuppressing).ToList());
        data.RemoveAll(item => item is SDataRadioSuppressing);
    }
}