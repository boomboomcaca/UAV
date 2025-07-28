using System;
using System.Collections.Generic;
using System.Threading;
using Magneto.Contract;
using Magneto.Device.DT1200AS.API;
using Magneto.Protocol.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.DT1200AS;

public partial class Dt1200As
{
    /// <summary>
    ///     读取数据
    /// </summary>
    private void ReadData()
    {
        while (_readDataCts?.IsCancellationRequested == false)
        {
            try
            {
                List<object> dataList = new();
                object data = null;
                data = ReadLte();
                if (data != null) dataList.Add(data);
                data = ReadWcdma();
                if (data != null) dataList.Add(data);
                data = ReadTdscdma();
                if (data != null) dataList.Add(data);
                data = ReadGsm();
                if (data != null) dataList.Add(data);
                data = ReadCdma1X();
                if (data != null) dataList.Add(data);
                data = ReadEvdo();
                if (data != null) dataList.Add(data);
                if (dataList.Count > 0 && _run) SendData(dataList);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                //ignore
            }

            Thread.Sleep(Second * 1000);
        }
    }

    private SDataCellular ReadLte()
    {
        SDataCellular station = null;
        if (_workState != 2) return null;
        try
        {
            var bcch = new LteEnbConfig();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_lte_cell(ref bcch);
            if (ret == 1)
                station = new SDataCellular
                {
                    DuplexMode = bcch.Duplex == 1 ? DuplexMode.TdLte : DuplexMode.LteFdd,
                    Channel = (int)bcch.Uarfcn,
                    Frequency = bcch.Freq / 1000000.0,
                    Bandwidth = (int)bcch.BandWidth,
                    Mcc = bcch.Mcc,
                    Mnc = bcch.Mnc,
                    Lac = bcch.Tac,
                    Ci = bcch.Ci,
                    RxPower = bcch.Rssi,
                    Timestamp = Utils.GetNowTimestamp(),
                    ExInfos = new Dictionary<string, ExtendedInfo>
                    {
                        { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.FakeBsFlag != 0) },
                        { "UARFCN", new ExtendedInfo("UARFCN", bcch.Uarfcn) },
                        { "Frequency", new ExtendedInfo("Frequency", bcch.Freq) },
                        { "BandWidth", new ExtendedInfo("BandWidth", bcch.BandWidth) },
                        { "Duplex", new ExtendedInfo("Duplex", bcch.Duplex) },
                        { "CP_TYPE", new ExtendedInfo("CP_TYPE", bcch.CpType) },
                        { "NDLRB", new ExtendedInfo("NDLRB", bcch.Ndlrb) },
                        { "MCC", new ExtendedInfo("MCC", bcch.Mcc) },
                        { "MNC", new ExtendedInfo("MNC", bcch.Mnc) },
                        { "TAC", new ExtendedInfo("TAC", bcch.Tac) },
                        { "CI", new ExtendedInfo("CI", bcch.Ci) },
                        { "freqband", new ExtendedInfo("freqband", bcch.Freqband) },
                        { "RSSI", new ExtendedInfo("RSSI", bcch.Rssi) },
                        { "RSRP", new ExtendedInfo("RSRP", bcch.Rsrp) },
                        { "RSRQ", new ExtendedInfo("RSRQ", bcch.Rsrq) },
                        { "SNR", new ExtendedInfo("SNR", bcch.Snr) }
                    }
                };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            station = null;
        }

        return station;
    }

    private SDataCellular ReadWcdma()
    {
        SDataCellular station = null;
        if (_workState != 2) return null;
        try
        {
            var bcch = new UmtsBcch();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_wcdma_cell(ref bcch);
            if (ret == 1)
            {
                station = new SDataCellular
                {
                    DuplexMode = DuplexMode.Wcdma, //4;
                    Channel = bcch.Uarfcn,
                    Frequency = bcch.Centerfrequency / 1000000.0
                };
                station.Bandwidth = GetBandwidth(DuplexMode.Wcdma, station.Frequency);
                station.Mcc = bcch.Mcc;
                station.Mnc = bcch.Mnc;
                station.Lac = bcch.Lac;
                station.Ci = bcch.Ci;
                station.RxPower = bcch.Rssi;
                station.Timestamp = Utils.GetNowTimestamp();
                station.ExInfos = new Dictionary<string, ExtendedInfo>
                {
                    { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.FakeBsFlag != 0) },
                    { "UARFCN", new ExtendedInfo("UARFCN", bcch.Uarfcn) },
                    { "Frequency", new ExtendedInfo("Frequency", bcch.Centerfrequency) },
                    { "MCC", new ExtendedInfo("MCC", bcch.Mcc) },
                    { "MNC", new ExtendedInfo("MNC", bcch.Mnc) },
                    { "LAC", new ExtendedInfo("LAC", bcch.Lac) },
                    { "CI", new ExtendedInfo("CI", bcch.Ci) },
                    { "RSCP", new ExtendedInfo("RSSI", bcch.Rscp) },
                    { "RSSI", new ExtendedInfo("RSRP", bcch.Rssi) }
                };
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            station = null;
        }

        return station;
    }

    private SDataCellular ReadTdscdma()
    {
        SDataCellular station = null;
        if (_workState != 2) return null;
        try
        {
            var bcch = new UmtsBcch();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_tdscdma_cell(ref bcch);
            if (ret == 1)
            {
                station = new SDataCellular
                {
                    DuplexMode = DuplexMode.TdScdma, // 3;
                    Channel = bcch.Uarfcn,
                    Frequency = bcch.Centerfrequency / 1000000.0
                };
                station.Bandwidth = GetBandwidth(DuplexMode.TdScdma, station.Frequency);
                station.Mcc = bcch.Mcc;
                station.Mnc = bcch.Mnc;
                station.Lac = bcch.Lac;
                station.Ci = bcch.Ci;
                station.RxPower = bcch.Rssi;
                station.Timestamp = Utils.GetNowTimestamp();
                station.ExInfos = new Dictionary<string, ExtendedInfo>
                {
                    { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.FakeBsFlag != 0) },
                    { "UARFCN", new ExtendedInfo("UARFCN", bcch.Uarfcn) },
                    { "Frequency", new ExtendedInfo("Frequency", bcch.Centerfrequency) },
                    { "MCC", new ExtendedInfo("MCC", bcch.Mcc) },
                    { "MNC", new ExtendedInfo("MNC", bcch.Mnc) },
                    { "LAC", new ExtendedInfo("LAC", bcch.Lac) },
                    { "CI", new ExtendedInfo("CI", bcch.Ci) },
                    { "RSSI", new ExtendedInfo("RSSI", bcch.Rssi) },
                    { "RSCP", new ExtendedInfo("RSRP", bcch.Rscp) }
                };
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            station = null;
        }

        return station;
    }

    private SDataCellular ReadGsm()
    {
        if (_workState != 2) return null;
        try
        {
            var bcch = new GsmBcch();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_gsm_cell(ref bcch);
            if (ret == 1)
            {
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
        }

        return null;
    }

    private SDataCellular ReadCdma1X()
    {
        SDataCellular station = null;
        if (_workState != 2) return null;
        try
        {
            var bcch = new Cdma2000Bcch();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_cdma_cell(ref bcch);
            if (ret == 1)
            {
                station = new SDataCellular
                {
                    DuplexMode = DuplexMode.Cdma20001X, //2;
                    Channel = bcch.CdmaFreq,
                    Frequency = bcch.Centerfrequency / 1000000.0
                };
                station.Bandwidth = GetBandwidth(DuplexMode.Cdma20001X, station.Frequency);
                station.Mcc = bcch.Mcc;
                station.Mnc = bcch.Sid; //bcch.SID.ToString().PadLeft(2, '0');
                station.Lac = bcch.Nid;
                station.Ci = bcch.BaseId;
                station.BsGps = new GpsDatum
                {
                    Longitude = bcch.BaseLong,
                    Latitude = bcch.BaseLat
                };
                station.RxPower = bcch.Rssi;
                station.Timestamp = Utils.GetNowTimestamp();
                station.ExInfos = new Dictionary<string, ExtendedInfo>
                {
                    { "isFakeStation", new ExtendedInfo("是否是伪基站", bcch.FakeBsFlag != 0) },
                    { "UARFCN", new ExtendedInfo("UARFCN", bcch.CdmaFreq) },
                    { "Frequency", new ExtendedInfo("Frequency", bcch.Centerfrequency) },
                    { "MCC", new ExtendedInfo("MCC", bcch.Mcc) },
                    { "SID", new ExtendedInfo("MNC", bcch.Sid) },
                    { "NID", new ExtendedInfo("LAC", bcch.Nid) },
                    { "BASE_ID", new ExtendedInfo("CI", bcch.BaseId) },
                    { "RSSI", new ExtendedInfo("RSSI", bcch.Rssi) }
                };
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            station = null;
        }

        return station;
    }

    private SDataCellular ReadEvdo()
    {
        SDataCellular station = null;
        if (_workState != 2) return null;
        try
        {
            var bcch = new EvdoBcch();
            if (_run == false) return null;
            var ret = Rx3GInterface.get_one_evdo_cell(ref bcch);
            if (ret == 1)
            {
                station = new SDataCellular
                {
                    DuplexMode = DuplexMode.Cdma20001XEvdo, //5;
                    Channel = bcch.CdmaFreq,
                    Frequency = bcch.Centerfrequency / 1000000.0
                };
                station.Bandwidth = GetBandwidth(DuplexMode.Cdma20001XEvdo, station.Frequency);
                station.Mcc = bcch.Mcc;
                station.Mnc = bcch.Sid;
                station.Lac = bcch.Nid;
                station.Ci = bcch.SectorId24;
                station.BsGps = new GpsDatum
                {
                    Longitude = bcch.BaseLong,
                    Latitude = bcch.BaseLat
                };
                station.RxPower = bcch.Rssi;
                station.Timestamp = Utils.GetNowTimestamp();
                station.ExInfos = new Dictionary<string, ExtendedInfo>
                {
                    { "UARFCN", new ExtendedInfo("UARFCN", bcch.CdmaFreq) },
                    { "Frequency", new ExtendedInfo("Frequency", bcch.Centerfrequency) },
                    { "MCC", new ExtendedInfo("MCC", bcch.Mcc) },
                    { "SID", new ExtendedInfo("MNC", bcch.Sid) },
                    { "NID", new ExtendedInfo("LAC", bcch.Nid) },
                    { "SectorID", new ExtendedInfo("SectorID", bcch.SectorId24) },
                    { "RSSI", new ExtendedInfo("RSSI", bcch.Rssi) }
                };
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            station = null;
        }

        return station;
    }

    private double GetBandwidth(DuplexMode mode, double freq)
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