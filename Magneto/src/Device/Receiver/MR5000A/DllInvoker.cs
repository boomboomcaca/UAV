using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;

namespace Magneto.Device.MR5000A;

internal class DllInvoker
{
    private const string LibRadecryptPath = "libradecrypt";
    private const string LibSsedfPath = "libsse-df";

    static DllInvoker()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "Common", new[] { LibRadecryptPath, LibSsedfPath });
    }

    [DllImport(LibRadecryptPath, EntryPoint = "generate_radio_key", CallingConvention = CallingConvention.Cdecl,
        CharSet = CharSet.Ansi)]
    internal static extern int GenerateRadioKey(string token);

    [DllImport(LibSsedfPath, EntryPoint = "get_estimated_signal_count_in_nine_channel",
        CallingConvention = CallingConvention.Cdecl)]
    internal static extern int EstimateAngleCount(int[] iq, int samplingCount, float[] calibratedValues,
        bool using16Bit = false, float coe = 0.01f, int method = 3);

    [DllImport(LibSsedfPath, EntryPoint = "get_sse_doa_in_nine_channel", CallingConvention = CallingConvention.Cdecl)]
    internal static extern int GetSSE(float[] result, int angleCount, long frequency, float aperture, int[] iq,
        int samplingCount, float[] calibratedValues, bool using16Bit = false);
}