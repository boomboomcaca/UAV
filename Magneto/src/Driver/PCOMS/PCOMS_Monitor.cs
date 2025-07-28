using System.Collections.Generic;
using System.Linq;
using Magneto.Contract;
using Magneto.Contract.BaseClass;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Driver.PCOMS;

public partial class Pcoms
{
    private void StartMonitoring()
    {
        _timer.Start();
        (Decoder as DeviceBase)?.Start(FeatureType.BsDecoding, this);
    }

    private void StopMonitoring()
    {
        _timer.Stop();
        (Decoder as DeviceBase)?.Stop();
    }

    private void SetMonitorParameter(string name)
    {
        if (name == "bandList")
            if (_freqBands != null)
                lock (_syncLocker)
                {
                    _datas.RemoveAll(p => !_freqBands.Any(q =>
                        (q.DuplexMode == p.DuplexMode || q.DuplexMode == DuplexMode.None) &&
                        p.Frequency.CompareWith(q.StartFrequency) >= 0 &&
                        p.Frequency.CompareWith(q.StopFrequency) <= 0));
                }
    }

    private void OnMonitoringData(List<object> datas)
    {
        if (datas?.Any() != true) return;
        foreach (var item in datas)
        {
            if (item is not SDataCellular data) continue;
            var legal = _freqBands?.Any(p => (p.DuplexMode == data.DuplexMode || p.DuplexMode == DuplexMode.None) &&
                                             data.Frequency.CompareWith(p.StartFrequency) >= 0 &&
                                             data.Frequency.CompareWith(p.StopFrequency) <= 0) != false;
            if (!legal) continue;
            lock (_syncLocker)
            {
                var index = _datas.FindIndex(p =>
                    p.DuplexMode == data.DuplexMode && p.Mnc == data.Mnc && p.Mcc == data.Mcc
                    && p.Lac == data.Lac && p.Ci == data.Ci);
                if (index < 0)
                    _datas.Add(data);
                else
                    _datas[index] = data;
            }
        }
    }
}