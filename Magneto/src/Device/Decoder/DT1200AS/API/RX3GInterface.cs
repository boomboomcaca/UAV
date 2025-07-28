using System.Runtime.InteropServices;

namespace Magneto.Device.DT1200AS.API;

public unsafe delegate int IqCbFn(short* x, int len);

public unsafe delegate int FmCbFn(float* x, int len);

internal static class Rx3GInterface
{
    public const string DllName = "library\\DT1200AS\\RX3GDll.dll";

    [DllImport(DllName, EntryPoint = "RX3GInit", CallingConvention = CallingConvention.StdCall)]
    public static extern void RX3GInit();

    [DllImport(DllName, EntryPoint = "GetDataFrequency", CallingConvention = CallingConvention.Cdecl)]
    public static extern long GetDataFrequency();

    [DllImport(DllName, EntryPoint = "GetPPM", CallingConvention = CallingConvention.Cdecl)]
    public static extern double GetPPM();

    [DllImport(DllName, EntryPoint = "GetDeviceNum", CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetDeviceNum();

    [DllImport(DllName, EntryPoint = "get_state", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_state();

    [DllImport(DllName, EntryPoint = "get_scan_statistic", CallingConvention = CallingConvention.Cdecl)]
    public static extern void get_scan_statistic(ref int roundNum, ref float meanCputime, ref float meanScantime,
        ref float meanCellnum);

    [DllImport(DllName, EntryPoint = "AddFreq2ScanList", CallingConvention = CallingConvention.Cdecl)]
    public static extern void AddFreq2ScanList(long f, int sampleRate, int bandWidth, int buffsize, int commType);

    [DllImport(DllName, EntryPoint = "start_spectrum_scan", CallingConvention = CallingConvention.Cdecl)]
    public static extern void start_spectrum_scan(long fLow, long fHigh);

    [DllImport(DllName, EntryPoint = "start_IQ_Sampling", CallingConvention = CallingConvention.Cdecl)]
    public static extern void start_IQ_Sampling(long centf, int sampleRate, int bw, int gain, IqCbFn iqCall);

    [DllImport(DllName, EntryPoint = "start_FM_Rx", CallingConvention = CallingConvention.Cdecl)]
    public static extern void start_FM_Rx(long centf, FmCbFn fmCall);

    [DllImport(DllName, EntryPoint = "start_AM_Rx", CallingConvention = CallingConvention.Cdecl)]
    public static extern void start_AM_Rx(long centf, FmCbFn fmCall);

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

    [DllImport(DllName, EntryPoint = "get_gsmr_spectrum", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_gsmr_spectrum(float[] downlinkPsd, ref int downlinkLen, ref long downlinkFreq,
        float[] uplinkPsd, ref int uplinkLen, ref long uplinkFreq);

    [DllImport(DllName, EntryPoint = "get_spectrum", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_spectrum(float[] psd, int maxLen, ref long startFreq);

    [DllImport(DllName, EntryPoint = "get_Interfere", CallingConvention = CallingConvention.Cdecl)]
    public static extern int get_Interfere(ref long f, ref int bw, ref int rssi);
}