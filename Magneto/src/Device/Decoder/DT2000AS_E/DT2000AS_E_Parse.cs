using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Magneto.Contract;
using Magneto.Device.DT2000AS.API;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT2000AS_E;

public partial class Dt2000AsE
{
    private void ProcessData()
    {
        while (_readDataCts?.IsCancellationRequested == false)
            try
            {
                for (var i = 0; i < 15; i++)
                    if (i % 5 == 0) //GSM
                    {
                        if (!Gsm) continue;
                        ProcessGsmData();
                    }
                    else if (i % 5 == 1) //LTE
                    {
                        if (!Lte) continue;
                        ProcessLteData();
                    }
                    else if (i is 2 or 8) //NR
                    {
                        if (!Nr) continue;
                        ProcessNrData();
                    }
                    else if (i is 3 or 9) //WCDMA
                    {
                        if (!Wcdma) continue;
                        ProcessWcdmaData();
                    }
                    else if (i is 4 or 12) //CDMA
                    {
                        if (!Cdma1X) continue;
                        ProcessCdmaData();
                    }
                    else if (i == 7) //EVDO
                    {
                        if (!Evdo) continue;
                        ProcessEvdoData();
                    }
                    else if (i == 13) //TDSCDMA
                    {
                        if (!TdScdma) continue;
                        ProcessTdscdmaData();
                    }
                    else
                    {
                        Thread.Sleep(ScanInterval);
                    }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
            }
    }

    private void ProcessGsmData()
    {
        var bcch = new GsmBcch();
        var count = Rx3GInterface.GetOneGsmCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.Gsm,
            Channel = bcch.C0,
            Frequency = bcch.centerfrequency / 1000000.0
        };
        station.Bandwidth = GetBandwidth(DuplexMode.Gsm, station.Frequency);
        station.Mcc = bcch.MCC;
        station.Mnc = bcch.MNC;
        // 中国移动TD系统使用00，中国联通GSM系统使用01，中国移动GSM系统使用02，中国电信CDMA系统使用03，中国卫星全球星网的 MNC 是 04
        station.Lac = bcch.LAC;
        station.Ci = bcch.CI;
        station.RxPower = bcch.RSSI;
        station.Timestamp = Utils.GetNowTimestamp();
        station.ExInfos = new Dictionary<string, ExtendedInfo>
        {
            { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
            { "UARFCN", new ExtendedInfo("UARFCN", bcch.C0) },
            { "Frequency", new ExtendedInfo("Frequency", bcch.centerfrequency) },
            { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
            { "MNC", new ExtendedInfo("MNC", bcch.MNC) },
            { "LAC", new ExtendedInfo("LAC", bcch.LAC) },
            { "CI", new ExtendedInfo("CI", bcch.CI) },
            { "RSSI", new ExtendedInfo("RSSI", bcch.RSSI) }
        };
        SendData(new List<object> { station });
    }

    private void ProcessCdmaData()
    {
        var bcch = new Cdma2000Bcch();
        var count = Rx3GInterface.GetOneCdmaCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.Cdma20001X, //2;
            Channel = bcch.CDMA_FREQ,
            Frequency = bcch.centerfrequency / 1000000.0
        };
        station.Bandwidth = GetBandwidth(DuplexMode.Cdma20001X, station.Frequency);
        station.Mcc = bcch.MCC;
        station.Mnc = bcch.SID; //bcch.SID.ToString().PadLeft(2, '0');
        station.Lac = bcch.NID;
        station.Ci = bcch.BASE_ID;
        station.BsGps = new GpsDatum
        {
            Longitude = bcch.BASE_LONG,
            Latitude = bcch.BASE_LAT
        };
        station.RxPower = bcch.rssi;
        station.Timestamp = Utils.GetNowTimestamp();
        station.ExInfos = new Dictionary<string, ExtendedInfo>
        {
            { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
            { "UARFCN", new ExtendedInfo("UARFCN", bcch.CDMA_FREQ) },
            { "Frequency", new ExtendedInfo("Frequency", bcch.centerfrequency) },
            { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
            { "SID", new ExtendedInfo("MNC", bcch.SID) },
            { "NID", new ExtendedInfo("LAC", bcch.NID) },
            { "BASE_ID", new ExtendedInfo("CI", bcch.BASE_ID) },
            { "RSSI", new ExtendedInfo("RSSI", bcch.rssi) }
        };
        SendData(new List<object> { station });
    }

    private void ProcessEvdoData()
    {
        var bcch = new EvdoBcch();
        var count = Rx3GInterface.GetOneEvdoCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        string sectorIdStr;
        if (bcch.sector_id_flag == 1)
        {
            var sb = new StringBuilder();
            for (var i = 24; i < 32; i++) sb.Append(bcch.sector_id[i].ToString("X"));
            sectorIdStr = sb.ToString();
        }
        else
        {
            sectorIdStr = bcch.SectorID24.ToString("X");
        }

        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.Cdma20001XEvdo, //5;
            Channel = bcch.CDMA_FREQ,
            Frequency = bcch.centerfrequency / 1000000.0
        };
        station.Bandwidth = GetBandwidth(DuplexMode.Cdma20001XEvdo, station.Frequency);
        station.Mcc = bcch.MCC;
        station.Mnc = bcch.SID;
        station.Lac = bcch.NID;
        station.Ci = Convert.ToUInt32(sectorIdStr, 16);
        station.BsGps = new GpsDatum
        {
            Longitude = bcch.BASE_LONG,
            Latitude = bcch.BASE_LAT
        };
        station.RxPower = bcch.rssi;
        station.Timestamp = Utils.GetNowTimestamp();
        station.ExInfos = new Dictionary<string, ExtendedInfo>
        {
            { "UARFCN", new ExtendedInfo("UARFCN", bcch.CDMA_FREQ) },
            { "Frequency", new ExtendedInfo("Frequency", bcch.centerfrequency) },
            { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
            { "SID", new ExtendedInfo("MNC", bcch.SID) },
            { "NID", new ExtendedInfo("LAC", bcch.NID) },
            { "SectorID", new ExtendedInfo("SectorID", bcch.SectorID24) },
            { "RSSI", new ExtendedInfo("RSSI", bcch.rssi) }
        };
        SendData(new List<object> { station });
    }

    private void ProcessWcdmaData()
    {
        var bcch = new UmtsBcch();
        var count = Rx3GInterface.GetOneWcdmaCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        WcdmaChannelNumToFreq(bcch.UARFCN, out var freq);
        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.Wcdma, //4;
            Channel = bcch.UARFCN,
            Frequency = bcch.centerfrequency != 0 ? bcch.centerfrequency / 1000000.0 : freq
        };
        station.Bandwidth = GetBandwidth(DuplexMode.Wcdma, station.Frequency);
        station.Mcc = bcch.MCC;
        station.Mnc = bcch.MNC;
        station.Lac = bcch.LAC;
        station.Ci = bcch.CI;
        station.RxPower = bcch.RSCP;
        station.Timestamp = Utils.GetNowTimestamp();
        station.ExInfos = new Dictionary<string, ExtendedInfo>
        {
            { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
            { "UARFCN", new ExtendedInfo("UARFCN", bcch.UARFCN) },
            { "Frequency", new ExtendedInfo("Frequency", bcch.centerfrequency) },
            { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
            { "MNC", new ExtendedInfo("MNC", bcch.MNC) },
            { "LAC", new ExtendedInfo("LAC", bcch.LAC) },
            { "CI", new ExtendedInfo("CI", bcch.CI) },
            { "RSCP", new ExtendedInfo("RSSI", bcch.RSCP) },
            { "RSSI", new ExtendedInfo("RSRP", bcch.RSSI) }
        };
        SendData(new List<object> { station });
    }

    private void ProcessTdscdmaData()
    {
        var bcch = new UmtsBcch();
        var count = Rx3GInterface.GetOneTdScdmaCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        var station = new SDataCellular
        {
            DuplexMode = DuplexMode.TdScdma, // 3;
            Channel = bcch.UARFCN,
            Frequency = bcch.centerfrequency / 1000000.0
        };
        station.Bandwidth = GetBandwidth(DuplexMode.TdScdma, station.Frequency);
        station.Mcc = bcch.MCC;
        station.Mnc = bcch.MNC;
        station.Lac = bcch.LAC;
        station.Ci = bcch.CI;
        station.RxPower = bcch.RSSI;
        station.Timestamp = Utils.GetNowTimestamp();
        station.ExInfos = new Dictionary<string, ExtendedInfo>
        {
            { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
            { "UARFCN", new ExtendedInfo("UARFCN", bcch.UARFCN) },
            { "Frequency", new ExtendedInfo("Frequency", bcch.centerfrequency) },
            { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
            { "MNC", new ExtendedInfo("MNC", bcch.MNC) },
            { "LAC", new ExtendedInfo("LAC", bcch.LAC) },
            { "CI", new ExtendedInfo("CI", bcch.CI) },
            { "RSSI", new ExtendedInfo("RSSI", bcch.RSSI) },
            { "RSCP", new ExtendedInfo("RSRP", bcch.RSCP) }
        };
        SendData(new List<object> { station });
    }

    private void ProcessLteData()
    {
        var bcch = new LteEnbConfig();
        var count = Rx3GInterface.GetOneLteCell(ref bcch);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (bcch.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        var duplexMode = bcch.duplex == 1 ? DuplexMode.TdLte : DuplexMode.LteFdd;
        var freq = bcch.freq / 1000000.0;
        var station = new SDataCellular
        {
            DuplexMode = duplexMode,
            Channel = (int)bcch.UARFCN,
            Frequency = bcch.freq / 1000000.0,
            Bandwidth = bcch.BandWidth == 0 ? GetBandwidth(duplexMode, freq) : bcch.BandWidth * 1e3,
            Mcc = (uint)bcch.MCC,
            Mnc = (uint)bcch.MNC[0],
            Lac = bcch.TAC,
            Ci = (uint)bcch.CI,
            RxPower = bcch.RSSI,
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.fake_bs_flag != 0) },
                { "UARFCN", new ExtendedInfo("UARFCN", bcch.UARFCN) },
                { "Frequency", new ExtendedInfo("Frequency", bcch.freq) },
                { "BandWidth", new ExtendedInfo("BandWidth", bcch.BandWidth) },
                { "Duplex", new ExtendedInfo("Duplex", bcch.duplex) },
                { "CP_TYPE", new ExtendedInfo("CP_TYPE", bcch.CP_TYPE) },
                { "NDLRB", new ExtendedInfo("NDLRB", bcch.NDLRB) },
                { "MCC", new ExtendedInfo("MCC", bcch.MCC) },
                { "MNC", new ExtendedInfo("MNC", bcch.MNC) },
                { "TAC", new ExtendedInfo("TAC", bcch.TAC) },
                { "CI", new ExtendedInfo("CI", bcch.CI) },
                { "freqband", new ExtendedInfo("freqband", bcch.freqband) },
                { "RSSI", new ExtendedInfo("RSSI", bcch.RSSI) },
                { "RSRP", new ExtendedInfo("RSRP", bcch.RSRP) },
                { "RSRQ", new ExtendedInfo("RSRQ", bcch.RSRQ) },
                { "SNR", new ExtendedInfo("SNR", bcch.snr) }
            }
        };
        SendData(new List<object> { station });
    }

    private void ProcessNrData()
    {
        var nr = new Nr5GgNbStr();
        var count = Rx3GInterface.GetOneNrCell(ref nr);
        if (count < 1)
        {
            Thread.Sleep(5);
            return;
        }

        if (nr.absoluteFrequencySSB <= 0)
        {
            Thread.Sleep(5);
            return;
        }

        if (nr.MCC == 0)
        {
            Thread.Sleep(5);
            return;
        }

        if (nr.Beam_result?.Any() != true)
        {
            Thread.Sleep(5);
            return;
        }

        var stationItem = new SDataCellular
        {
            DuplexMode = DuplexMode.Nr5G,
            Frequency = Math.Round(nr.freqSSB / 1000000.0, 6),
            Channel = nr.absoluteFrequencySSB,
            Bandwidth = nr.BandWidth * 1e3,
            Mcc = (uint)nr.MCC,
            Mnc = (uint)nr.MNC[0],
            Ci = (uint)nr.CI,
            Lac = (uint)nr.TAC,
            RxPower = nr.SSB_RSSI,
            Timestamp = Utils.GetNowTimestamp(),
            ExInfos = new Dictionary<string, ExtendedInfo>
            {
                { "UARFCN", new ExtendedInfo("UARFCN", nr.absoluteFrequencySSB) },
                { "FreqSSB", new ExtendedInfo("Frequency", nr.freqSSB) },
                { "BandWidth", new ExtendedInfo("BandWidth", nr.BandWidth) },
                { "Duplex", new ExtendedInfo("Duplex", nr.duplex) },
                { "CP_TYPE", new ExtendedInfo("CP_TYPE", nr.CP_TYPE) },
                { "MCC", new ExtendedInfo("MCC", nr.MCC) },
                { "MNC", new ExtendedInfo("MNC", nr.MNC) },
                { "TAC", new ExtendedInfo("TAC", nr.TAC) },
                { "CI", new ExtendedInfo("CI", nr.CI) },
                { "FreqBand", new ExtendedInfo("FreqBand", nr.freqband) },
                { "SSB_RSSI", new ExtendedInfo("RSSI", nr.SSB_RSSI) },
                { "PCI", new ExtendedInfo("SNR", nr.PCI) },
                { "SFN", new ExtendedInfo("SFN", nr.SFN) }
            }
        };
        for (var i = 0; i < nr.Beam_result.Length; i++)
        {
            if (!nr.Beam_result[i].decoded_flag) continue;
            stationItem.ExInfos.Add($"Beam{i}_PBCHRS_CINR",
                new ExtendedInfo($"Beam{i}_PBCHRS_CINR", Math.Round(nr.Beam_result[i].rspbch_cinr, 2)));
            stationItem.ExInfos.Add($"Beam{i}_PBCHRS_RP",
                new ExtendedInfo($"Beam{i}_PBCHRS_RP", Math.Round(nr.Beam_result[i].rspbch_rp, 2)));
            stationItem.ExInfos.Add($"Beam{i}_PBCHRS_RQ",
                new ExtendedInfo($"Beam{i}_PBCHRS_RQ", Math.Round(nr.Beam_result[i].rspbch_rq, 2)));
            stationItem.ExInfos.Add($"Beam{i}_PSS_CINR",
                new ExtendedInfo($"Beam{i}_PSS_CINR", Math.Round(nr.Beam_result[i].pss_cinr, 2)));
            stationItem.ExInfos.Add($"Beam{i}_PSS_RP",
                new ExtendedInfo($"Beam{i}_PSS_RP", Math.Round(nr.Beam_result[i].pss_rp, 2)));
            stationItem.ExInfos.Add($"Beam{i}_PSS_RQ",
                new ExtendedInfo($"Beam{i}_PSS_RQ", Math.Round(nr.Beam_result[i].pss_rq, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSB_CINR",
                new ExtendedInfo($"Beam{i}_SSB_CINR", Math.Round(nr.Beam_result[i].ssb_cinr, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSB_RP",
                new ExtendedInfo($"Beam{i}_SSB_RP", Math.Round(nr.Beam_result[i].ssb_rp, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSB_RQ",
                new ExtendedInfo($"Beam{i}_SSB_RQ", Math.Round(nr.Beam_result[i].ssb_rq, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSB_RSSI",
                new ExtendedInfo($"Beam{i}_SSB_RSSI", Math.Round(nr.Beam_result[i].ssb_rssi, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSS_CINR",
                new ExtendedInfo($"Beam{i}_SSS_CINR", Math.Round(nr.Beam_result[i].sss_cinr, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSS_RP",
                new ExtendedInfo($"Beam{i}_SSS_RP", Math.Round(nr.Beam_result[i].sss_rp, 2)));
            stationItem.ExInfos.Add($"Beam{i}_SSS_RQ",
                new ExtendedInfo($"Beam{i}_SSS_RQ", Math.Round(nr.Beam_result[i].sss_rq, 2)));
        }

        SendData(new List<object> { stationItem });
    }

    private static bool WcdmaChannelNumToFreq(int channelNum, out double downlinkFreq)
    {
        downlinkFreq = 0;
        if (channelNum is >= 10550 and <= 10850)
            downlinkFreq = channelNum / 5d; //信道频率信道号/5
        else if (channelNum is >= 3070 and <= 3100)
            downlinkFreq = channelNum / 5d + 340;
        else
            return false;
        return true;
    }

    private static double GetBandwidth(DuplexMode mode, double freq)
    {
        switch (mode)
        {
            case DuplexMode.Gsm:
                if (freq is >= 1845 and <= 1860)
                    return 15 * 1e3;
                return 0.2 * 1e3;
            case DuplexMode.LteFdd:
            case DuplexMode.TdLte:
            {
                if (freq is >= 1845 and <= 1860)
                    return 15 * 1e3;
                return 20 * 1e3;
            }
            case DuplexMode.Wcdma:
                return 5 * 1e3;
            case DuplexMode.Cdma20001X:
            case DuplexMode.Cdma20001XEvdo:
                return 1.23 * 1e3;
            case DuplexMode.TdScdma:
                return 1.6 * 1e3;
        }

        return 0.2 * 1e3;
    }
}