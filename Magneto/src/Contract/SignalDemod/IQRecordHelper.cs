using System.Collections.Generic;
using System.Linq;

namespace Magneto.Contract.SignalDemod;

public static class IqRecordHelper
{
    public static long GetIqDataLen(double samplingRate, Dictionary<double, long> iqDataLenConfig)
    {
        if (iqDataLenConfig?.Any() != true) return 64 * 1024;
        var bws = iqDataLenConfig.Select(kv => kv.Key).OrderBy(p => p).ToList();
        var bw = bws.FirstOrDefault();
        var index = 0;
        do
        {
            if (bw.CompareWith(samplingRate) >= 0) break;
            bw = bws[index];
            index++;
        } while (index < bws.Count);

        return iqDataLenConfig[bw];
    }
}