using System.Reflection;
using System.Runtime.InteropServices;
using Magneto.Contract;

namespace Magneto.Driver.UAVD.Algorithm;

internal class UavDll
{
    internal const string LibPath = "libuav";

    static UavDll()
    {
        Utils.ResolveDllImport(Assembly.GetExecutingAssembly(), "Common", new[] { LibPath });
    }

    /// <summary>
    ///     初始化无人机识别样本
    /// </summary>
    /// <param name="drones">无人机样本序列</param>
    /// <param name="droneLength">无人机样本序列长度</param>
    /// <param name="mergedGap">信号最大合并带宽，如500，表示两个相邻信号若相隔500kHz及以内，则认为是同一个信号</param>
    /// <param name="droppedWidth">最大信号舍弃带宽，如1000，表示信号带宽若小于1000kHz及以内，则舍弃该信号</param>
    [DllImport(LibPath, EntryPoint = "init_enhanced_uav_schema", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void InitEnhancedUavSchema(DroneEx[] drones = null,
        int droneLength = 0,
        double mergedGap = 500,
        double droppedWidth = 1000);

    /// <summary>
    ///     开始无人机
    /// </summary>
    /// <param name="startFrequency">捕获范围内扫描起始频率</param>
    /// <param name="stopFrequency">捕获范围内扫描结束频率</param>
    /// <param name="stepFrequency">捕获范围内扫描步进</param>
    /// <param name="snr">信噪比门限，由用户输入</param>
    /// <param name="spectras">频谱数据序列</param>
    /// <param name="spetraLength">频谱数据序列长度，由用户输入</param>
    /// <param name="droneByteArray">识别信息字节数组</param>
    /// <param name="droneByteArrayLength">识别信息字节数组大小</param>
    /// <returns></returns>
    [DllImport(LibPath, EntryPoint = "attach_enhanced_uav", CallingConvention = CallingConvention.Cdecl)]
    internal static extern void AttachEnhancedUav(double startFrequency,
        double stopFrequency,
        double stepFrequency,
        int snr,
        Spectra[] spectras,
        int spetraLength,
        byte[] droneByteArray,
        ref int droneByteArrayLength);
}