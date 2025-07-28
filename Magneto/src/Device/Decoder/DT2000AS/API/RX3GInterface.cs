using System.Runtime.InteropServices;

namespace Magneto.Device.DT2000AS.API;

/// <summary>
///     驱动版本vc_demo_5G_20211208
/// </summary>
internal static class Rx3GInterface
{
    public const string DllName = "library\\DT2000AS\\x64\\RX5GDll.dll";

    [DllImport(DllName, EntryPoint = "RX3GInit", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RX3GInit();

    [DllImport(DllName, EntryPoint = "GetDeviceNum", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetDeviceNum();

    [DllImport(DllName, EntryPoint = "get_state", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_state();

    [DllImport(DllName, EntryPoint = "start_scan", CallingConvention = CallingConvention.Cdecl)]
    public static extern void start_scan();

    [DllImport(DllName, EntryPoint = "stop_scan", CallingConvention = CallingConvention.Cdecl)]
    public static extern void stop_scan();

    [DllImport(DllName, EntryPoint = "close_device", CallingConvention = CallingConvention.Cdecl)]
    public static extern void close_device();

    [DllImport(DllName, EntryPoint = "ResetScanList", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ResetScanList();

    [DllImport(DllName, EntryPoint = "get_one_cdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_cdma_cell(ref Cdma2000Bcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_evdo_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_evdo_cell(ref EvdoBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_gsm_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_gsm_cell(ref GsmBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_lte_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_lte_cell(ref LteEnbConfig bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_wcdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_wcdma_cell(ref UmtsBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_tdscdma_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_one_tdscdma_cell(ref UmtsBcch bcchStr);

    [DllImport(DllName, EntryPoint = "get_one_nr5g_cell", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetOneNrCell(ref Nr5GgNbStr gnbStr);

    [DllImport(DllName, EntryPoint = "scan_freq_config", CallingConvention = CallingConvention.Cdecl)]
    public static extern int ScanFreqConfig(long freq, char gsmEn, char cdmaEn, char evdoEn, char wcdmaEn,
        char tdscdmaEn,
        char lteTddEn, char lteFddEn, char nr5GEn, char range);

    [DllImport(DllName, EntryPoint = "Add_5GScan_FreqBand", CallingConvention = CallingConvention.Cdecl)]
    public static extern int Add_5GScan_FreqBand(long lowf, long upperf);

    [DllImport(DllName, EntryPoint = "set_device_gain", CallingConvention = CallingConvention.Cdecl)]
    public static extern int set_device_gain(int gain);
}