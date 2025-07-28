using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Magneto.Contract;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DC7520MOB;

public partial class Dc7520Mob
{
    private List<SDataCellular> ParseCh1Data(string s, CommandType commandType)
    {
        var list = new List<SDataCellular>();
        if (commandType != CommandType.Ch1Gsm && commandType != CommandType.Ch2Wcdma) return list;
        var pattern = @"(\^)?NETSCAN\s*\:\s*((?<value>[0-9a-zA-Z\-\._]*)\s*\,\s*)+(?<value>[0-9a-zA-Z\-\._]+)\s*";
        var matches = Regex.Matches(s, pattern);
        if (matches.Any() != true) return list;
        foreach (Match match in matches)
        {
            if (!match.Success) continue;
            var valueCaps = match.Groups["value"].Captures;
            if (valueCaps.Any() != true) continue;
            SDataCellular data = null;
            switch (commandType)
            {
                case CommandType.Ch1Gsm:
                    data = GetGsm(valueCaps);
                    break;
                case CommandType.Ch2Wcdma:
                    data = GetWcdma(valueCaps);
                    break;
            }

            if (data == null) continue;
            list.Add(data);
        }

        return list;
    }

    private List<SDataCellular> ParseCh2Data(string s, CommandType commandType)
    {
        var list = new List<SDataCellular>();
        if (commandType != CommandType.Ch2Gsm && commandType != CommandType.Ch2Wcdma &&
            commandType != CommandType.TdScdma &&
            commandType != CommandType.Lte)
            return list;
        var pattern = @"(\^)?NETSCAN\s*\:\s*((?<value>[0-9a-zA-Z\-\._]*)\s*\,\s*)+(?<value>[0-9a-zA-Z\-\._]+)\s*";
        var matches = Regex.Matches(s, pattern);
        if (matches.Any() != true) return list;
        foreach (Match match in matches)
        {
            if (!match.Success) continue;
            var valueCaps = match.Groups["value"].Captures;
            if (valueCaps.Any() != true) continue;
            SDataCellular data = null;
            switch (commandType)
            {
                case CommandType.Ch2Gsm:
                    data = GetGsm(valueCaps);
                    break;
                case CommandType.Ch2Wcdma:
                    data = GetWcdma(valueCaps);
                    break;
                case CommandType.TdScdma:
                    data = GetTdscdma(valueCaps);
                    break;
                case CommandType.Lte:
                    data = GetLte(valueCaps);
                    break;
            }

            if (data == null) continue;
            list.Add(data);
        }

        return list;
    }

    private void ParseCh3Data(string s, CommandType commandType, ref SDataCellular data)
    {
        Console.WriteLine($"{commandType}解析数据:{s}");
        switch (commandType)
        {
            case CommandType.Cdma20001XSiq:
            case CommandType.Cdma2000EvdoSiq:
                data = ParseCdmaData(s, commandType);
                break;
            case CommandType.Cdma2000EvdoBsin:
            case CommandType.Cdma20001XBsin:
                AddGpsToCdma(s, ref data);
                break;
            case CommandType.Cdma2000EvdoSid:
                AddSectorIdToCdma(s, ref data);
                break;
        }
    }

    private SDataCellular GetGsm(CaptureCollection valueCaps)
    {
        if (valueCaps.Count != 10) return null;
        var arfcn = valueCaps[0].Value;
        var lac = valueCaps[3].Value;
        var mcc = valueCaps[4].Value;
        var mnc = valueCaps[5].Value;
        var bsic = valueCaps[6].Value;
        var rxlvel = valueCaps[7].Value;
        var cid = valueCaps[8].Value;
        var band = valueCaps[9].Value;
        var channel = ToInt32(arfcn);
        var nMnc = ToInt32(mnc);
        if (channel is > 0 and < 124 or > 512 and < 599)
        {
            int[] arrGsmmnc = { 0, 1, 2, 6, 7, 20 };
            if (!arrGsmmnc.Contains(nMnc)) return null;
        }

        var success = ChanNumToFreq(DuplexMode.Gsm, arfcn, out var freq);
        if (!success) return null;
        var rxPower = ToInt32(rxlvel);
        var data = new SDataCellular
        {
            DuplexMode = DuplexMode.Gsm,
            Frequency = freq,
            Channel = ToInt32(arfcn),
            Mnc = ToUInt32(mnc),
            Mcc = ToUInt32(mcc),
            Lac = ToUInt32(lac),
            Ci = ToUInt32(cid, true),
            Bandwidth = GetBandwidth(DuplexMode.Gsm, freq),
            RxPower = rxPower,
            FieldStrength = GetFieldStrengthFormRxPower(rxPower, freq),
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "arfcn", new ExtendedInfo("绝对无线频道编号", arfcn) },
                { "lac", new ExtendedInfo("位置区编号", lac) },
                { "mnc", new ExtendedInfo("系统编号", mnc) },
                { "mcc", new ExtendedInfo("国家编号", mcc) },
                { "bsic", new ExtendedInfo("bsic", bsic) },
                { "rxlvel", new ExtendedInfo("接收电平", rxlvel) },
                { "cid", new ExtendedInfo("小区编号", cid) },
                { "band", new ExtendedInfo("带宽", band) }
            }
        };
        return data;
    }

    private SDataCellular GetWcdma(CaptureCollection valueCaps)
    {
        if (valueCaps.Count != 11) return null;
        var arfcn = valueCaps[0].Value;
        var lac = valueCaps[3].Value;
        var mcc = valueCaps[4].Value;
        var mnc = valueCaps[5].Value;
        var bsic = valueCaps[6].Value;
        var rxlvel = valueCaps[7].Value;
        var cid = valueCaps[8].Value;
        var band = valueCaps[9].Value;
        var psc = valueCaps[10].Value;
        var success = ChanNumToFreq(DuplexMode.Wcdma, arfcn, out var freq);
        if (!success) return null;
        var rxPower = ToInt32(rxlvel);
        var data = new SDataCellular
        {
            DuplexMode = DuplexMode.Wcdma,
            Frequency = freq,
            Channel = ToInt32(arfcn),
            Mnc = ToUInt32(mnc),
            Mcc = ToUInt32(mcc),
            Lac = ToUInt32(lac, true),
            Ci = ToUInt32(cid, true),
            Bandwidth = GetBandwidth(DuplexMode.Wcdma, freq),
            RxPower = rxPower,
            FieldStrength = GetFieldStrengthFormRxPower(rxPower, freq),
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "arfcn", new ExtendedInfo("绝对无线频道编号", arfcn) },
                { "lac", new ExtendedInfo("位置区编号", lac) },
                { "mnc", new ExtendedInfo("系统编号", mnc) },
                { "mcc", new ExtendedInfo("国家编号", mcc) },
                { "bsic", new ExtendedInfo("bsic", bsic) },
                { "rxlvel", new ExtendedInfo("接收电平", rxlvel) },
                { "cid", new ExtendedInfo("小区编号", cid) },
                { "band", new ExtendedInfo("带宽", band) },
                { "psc", new ExtendedInfo("psc", psc) }
            }
        };
        return data;
    }

    private SDataCellular GetTdscdma(CaptureCollection valueCaps)
    {
        if (valueCaps.Count != 11) return null;
        var arfcn = valueCaps[0].Value;
        var lac = valueCaps[3].Value;
        var mcc = valueCaps[4].Value;
        var mnc = valueCaps[5].Value;
        var bsic = valueCaps[6].Value;
        var rxlvel = valueCaps[7].Value;
        var cid = valueCaps[8].Value;
        var band = valueCaps[9].Value;
        var psc = valueCaps[10].Value;
        var success = ChanNumToFreq(DuplexMode.TdScdma, arfcn, out var freq);
        if (!success) return null;
        var rxPower = ToInt32(rxlvel);
        var data = new SDataCellular
        {
            DuplexMode = DuplexMode.TdScdma,
            Frequency = freq,
            Channel = ToInt32(arfcn),
            Mnc = ToUInt32(mnc),
            Mcc = ToUInt32(mcc),
            Lac = ToUInt32(lac, true),
            Ci = ToUInt32(cid, true),
            Bandwidth = GetBandwidth(DuplexMode.TdScdma, freq),
            RxPower = rxPower,
            FieldStrength = GetFieldStrengthFormRxPower(rxPower, freq),
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "arfcn", new ExtendedInfo("绝对无线频道编号", arfcn) },
                { "lac", new ExtendedInfo("位置区编号", lac) },
                { "mnc", new ExtendedInfo("系统编号", mnc) },
                { "mcc", new ExtendedInfo("国家编号", mcc) },
                { "bsic", new ExtendedInfo("bsic", bsic) },
                { "rxlvel", new ExtendedInfo("接收电平", rxlvel) },
                { "cid", new ExtendedInfo("小区编号", cid) },
                { "band", new ExtendedInfo("带宽", band) },
                { "psc", new ExtendedInfo("psc", psc) }
            }
        };
        return data;
    }

    private SDataCellular GetLte(CaptureCollection valueCaps)
    {
        if (valueCaps.Count != 12) return null;
        var arfcn = valueCaps[0].Value;
        var lac = valueCaps[3].Value;
        var mcc = valueCaps[4].Value;
        var mnc = valueCaps[5].Value;
        var bsic = valueCaps[6].Value;
        var rxlvel = valueCaps[7].Value;
        var cid = valueCaps[8].Value;
        var lteband = valueCaps[9].Value;
        var pci = valueCaps[11].Value;
        var eNbid = ToInt32(cid[..^2]);
        var cellId = ToInt32(cid[^2..]);
        var ci = ToUInt64(eNbid + cellId.ToString());
        var channel = ToInt32(arfcn);
        var nMnc = ToInt32(mnc);
        if (channel is > 0 and < 124 or > 512 and < 599)
        {
            int[] arrLtemnc = { 11, 3, 5 };
            if (!arrLtemnc.Contains(nMnc)) return null;
        }

        var duplexMode = IsLteTdd(channel) ? DuplexMode.TdLte : DuplexMode.LteFdd;
        var success = ChanNumToFreq(duplexMode, arfcn, out var freq);
        if (!success) return null;
        var rxPower = ToInt32(rxlvel);
        var data = new SDataCellular
        {
            DuplexMode = duplexMode,
            Frequency = freq,
            Channel = ToInt32(arfcn),
            Mnc = (uint)nMnc,
            Mcc = ToUInt32(mcc),
            Lac = ToUInt32(lac, true),
            Ci = (uint)ci,
            Bandwidth = GetBandwidth(duplexMode, freq),
            RxPower = rxPower,
            FieldStrength = GetFieldStrengthFormRxPower(rxPower, freq),
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "arfcn", new ExtendedInfo("绝对无线频道编号", arfcn) },
                { "lac", new ExtendedInfo("位置区编号", lac) },
                { "mnc", new ExtendedInfo("系统编号", mnc) },
                { "mcc", new ExtendedInfo("国家编号", mcc) },
                { "bsic", new ExtendedInfo("bsic", bsic) },
                { "rxlvel", new ExtendedInfo("接收电平", rxlvel) },
                { "cid", new ExtendedInfo("cid", cid) },
                { "eNBID", new ExtendedInfo("eNBID", eNbid) },
                { "CellID", new ExtendedInfo("CellID", cellId) },
                { "lteband", new ExtendedInfo("频段", lteband) },
                { "pci", new ExtendedInfo("物理小区号", pci) }
            }
        };
        return data;
    }

    private SDataCellular ParseCdmaData(string s, CommandType commandType)
    {
        if (commandType != CommandType.Cdma20001XSiq && commandType != CommandType.Cdma2000EvdoSiq) return null;
        var pattern = @"(\^)?SIQ\s*\:\s*((?<value>[0-9a-zA-Z\-\._%]*)\s*\,\s*){14}(?<value>[0-9a-zA-Z\-\._%]+)\s*";
        var match = Regex.Match(s, pattern);
        if (!match.Success) return null;
        var valueCaps = match.Groups["value"].Captures;
        if (valueCaps.Any() != true) return null;
        var bandClass = valueCaps[0].Value;
        var chan = valueCaps[1].Value;
        var sid = valueCaps[2].Value;
        var nid = valueCaps[3].Value;
        var pn = valueCaps[4].Value;
        var sci = valueCaps[5].Value;
        var ecio = valueCaps[6].Value;
        var rxPowerbufValue = valueCaps[7].Value;
        var txPower = valueCaps[8].Value;
        var txAdj = valueCaps[9].Value;
        var fer = valueCaps[10].Value;
        var tAdd = valueCaps[11].Value;
        var tDrop = valueCaps[12].Value;
        var tComp = valueCaps[13].Value;
        var tTdrop = valueCaps[14].Value;
        var duplexMode = commandType == CommandType.Cdma20001XSiq
            ? DuplexMode.Cdma20001X
            : DuplexMode.Cdma20001XEvdo;
        var success = ChanNumToFreq(duplexMode, chan, out var freq);
        if (!success) return null;
        var rxPower = ToInt32(rxPowerbufValue);
        var data = new SDataCellular
        {
            DuplexMode = duplexMode,
            Frequency = freq,
            Channel = ToInt32(chan),
            Mnc = ToUInt32(sid),
            Lac = ToUInt32(nid),
            Bandwidth = GetBandwidth(duplexMode, freq),
            RxPower = rxPower,
            FieldStrength = GetFieldStrengthFormRxPower(rxPower, freq),
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "band_class", new ExtendedInfo("band_class", bandClass) },
                { "arfcn", new ExtendedInfo("绝对无线频道编号", chan) },
                { "nid", new ExtendedInfo("位置区编号", nid) },
                { "pn", new ExtendedInfo("pn", pn) },
                { "sci", new ExtendedInfo("sci", sci) },
                { "ecio", new ExtendedInfo("ecio", ecio) },
                { "rx_power", new ExtendedInfo("接收电平", rxPower) },
                { "tx_power", new ExtendedInfo("发射电平", txPower) },
                { "tx_adj", new ExtendedInfo("tx_adj", txAdj) },
                { "fer", new ExtendedInfo("fer", fer) },
                { "t_add", new ExtendedInfo("t_add", tAdd) },
                { "t_drop", new ExtendedInfo("t_drop", tDrop) },
                { "t_comp", new ExtendedInfo("t_comp", tComp) },
                { "t_tdrop", new ExtendedInfo("t_tdrop", tTdrop) }
            }
        };
        return data;
    }

    private void AddGpsToCdma(string s, ref SDataCellular dataCdma)
    {
        if (dataCdma == null) return;
        var pattern = @"(\^)?BSIN\s*\:\s*((?<value>\w*)\s*\,\s*){4}(?<value>\w+)";
        var match = Regex.Match(s, pattern);
        if (!match.Success) return;
        var valueCaps = match.Groups["value"].Captures;
        if (valueCaps.Any() != true) return;
        var bsid = valueCaps[0].Value;
        //var sid = valueCaps[0].Value;
        //var nid = valueCaps[0].Value;
        var bslong = valueCaps[3].Value;
        var bslat = valueCaps[4].Value;
        var gps = new GpsDatum
        {
            Longitude = ToDouble(bslong) * 0.25 / 3600,
            Latitude = ToDouble(bslat) * 0.25 / 3600
        };
        dataCdma.ExInfos.Add("bsid", new ExtendedInfo("小区编号", bsid));
        dataCdma.BsGps = gps;
        dataCdma.Ci = ToUInt32(bsid);
        dataCdma.Mcc = 460;
    }

    private void AddSectorIdToCdma(string s, ref SDataCellular dataCdma)
    {
        var pattern = @"(\^)?CURRSID\s*\:\s*((?<value>\w*)\s*\,\s*){2}(?<value>\w+)";
        var match = Regex.Match(s, pattern);
        if (!match.Success) return;
        var valueCaps = match.Groups["value"].Captures;
        if (valueCaps.Any() != true) return;
        var id = valueCaps[2].Value;
        if (id.Length >= 32)
        {
            var sectorId = ToInt32(id.Substring(26, 6));
            dataCdma.Ci = (uint)sectorId;
            dataCdma.ExInfos.Add("sectorId", new ExtendedInfo("扇区编号", sectorId));
        }
        else
        {
            dataCdma.Ci = 0;
            dataCdma.ExInfos.Add("sectorId", new ExtendedInfo("扇区编号", string.Empty));
        }
    }
}

internal enum CommandType
{
    None,
    Ch1Gsm,
    Ch1Wcdma,
    Ch2Gsm,
    Ch2Wcdma,
    TdScdma,
    Lte,
    Cdma20001XMode,
    Cdma20001XSiq,
    Cdma20001XBsin,
    Cdma2000EvdoMode,
    Cdma2000EvdoSiq,
    Cdma2000EvdoBsin,
    Cdma2000EvdoSid
}