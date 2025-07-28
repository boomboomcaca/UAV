using System;
using System.Collections.Generic;
using Magneto.Contract;
using Magneto.Device.DT1000AS.Driver;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1000AS;

public partial class Dt1000As
{
    private void ReceiverOnDataReceived(object sender, GsmData e)
    {
        var bcch = e.Data;
        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.Gsm,
            Mnc = bcch.MNC,
            Channel = bcch.C0,
            Frequency = GetGsmFrequency(bcch.C0),
            Bandwidth = 200 * 1000,
            Mcc = bcch.MCC,
            // 中国移动TD系统使用00，中国联通GSM系统使用01，中国移动GSM系统使用02，中国电信CDMA系统使用03，中国卫星全球星网的 MNC 是 04
            Lac = bcch.LAC,
            Ci = bcch.CI,
            RxPower = bcch.RSSI,
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
                { "UARFCN", new ExtendedInfo("UARFCN", bcch.C0) },
                { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
                { "MNC", new ExtendedInfo("MNC", bcch.MNC) },
                { "LAC", new ExtendedInfo("LAC", bcch.LAC) },
                { "CI", new ExtendedInfo("CI", bcch.CI) },
                { "BSIC", new ExtendedInfo("CI", bcch.BSIC) },
                { "RSSI", new ExtendedInfo("RSSI", bcch.RSSI) }
            }
        };
        SendData(new List<object> { station });
    }

    private static double GetGsmFrequency(ushort arfcn)
    {
        var list = new List<Tuple<string, int, int, Func<int, double>, int>>
        {
            new("P-GSM", 1, 124, p => 890 + 0.2 * p, 45),
            new("GSM850", 128, 251, p => 824.2 + 0.2 * (p - 128), 45),
            new("GSM450", 259, 293, p => 450.6 + 0.2 * (p - 259), 10),
            new("GSM480", 306, 340, p => 479 + 0.2 * (p - 306), 10),
            new("GSM750", 438, 511, p => 747.2 + 0.2 * (p - 438), 30),
            new("GSM-R", 940, 974, p => 890 + 0.2 * (p - 1024), 45),
            new("DCS1800", 512, 885, p => 1710.2 + 0.2 * (p - 512), 95),
            new("E-GSM", 975, 1023, p => 890 + 0.2 * (p - 1024), 45)
        };
        var gsmInfo = list.Find(p => arfcn >= p.Item2 && arfcn <= p.Item3);
        if (gsmInfo == null) return 0;
        return gsmInfo.Item4(arfcn) + gsmInfo.Item5;
    }
}