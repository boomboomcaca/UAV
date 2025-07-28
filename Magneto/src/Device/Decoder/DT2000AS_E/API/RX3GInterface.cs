using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;

namespace Magneto.Device.DT2000AS.API;

/// <summary>
///     驱动版本vc_demo_5G网口_20220727
/// </summary>
internal static class Rx3GInterface
{
    internal const string DllName = "RX5GDll";

    static Rx3GInterface()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "DT2000AS_E", new[] { DllName });
    }

    [DllImport(DllName, EntryPoint = "config_ip", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ConfigIp(string ip);

    [DllImport(DllName, EntryPoint = "RX3GInit", CallingConvention = CallingConvention.Cdecl)]
    public static extern uint RX3GInit();

    [DllImport(DllName, EntryPoint = "GetDeviceNum", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetDeviceNum();

    [DllImport(DllName, EntryPoint = "get_state", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetState();

    [DllImport(DllName, EntryPoint = "start_scan", CallingConvention = CallingConvention.Cdecl)]
    public static extern void StartScan();

    [DllImport(DllName, EntryPoint = "stop_scan", CallingConvention = CallingConvention.Cdecl)]
    public static extern void StopScan();

    [DllImport(DllName, EntryPoint = "close_device", CallingConvention = CallingConvention.Cdecl)]
    public static extern void CloseDevice();

    [DllImport(DllName, EntryPoint = "ResetScanList", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ResetScanList();

    [DllImport(DllName, EntryPoint = "get_one_cdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneCdmaCell(ref Cdma2000Bcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_evdo_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneEvdoCell(ref EvdoBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_gsm_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneGsmCell(ref GsmBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_lte_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneLteCell(ref LteEnbConfig bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_wcdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneWcdmaCell(ref UmtsBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_tdscdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneTdScdmaCell(ref UmtsBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_nr5g_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneNrCell(ref Nr5GgNbStr gnbStr);

    [DllImport(DllName, EntryPoint = "scan_freq_config", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ScanFreqConfig(long freq, char gsmEn, char cdmaEn, char evdoEn, char wcdmaEn,
        char tdscdmaEn,
        char lteTddEn, char lteFddEn, char nr5GEn, char range);

    [DllImport(DllName, EntryPoint = "Add_5GScan_FreqBand", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Add5GScanFreqBand(long lowf, long upperf);

    [DllImport(DllName, EntryPoint = "set_device_gain", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetDeviceGain(int gain);
}