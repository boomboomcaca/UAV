using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalTimeDataParms
{
    /// <summary>
    ///     时域测量的中心频率
    /// </summary>
    public double CenterFrequency;

    /// <summary>
    ///     采样率
    /// </summary>
    public double SampleRate;

    /// <summary>
    ///     数据通过网络发送之前，需要累计的数据量
    /// </summary>
    public uint NumTransferSamples;

    /// <summary>
    ///     需要采集的数据总量
    /// </summary>
    public ulong NumSamples;

    /// <summary>
    ///     返回的数据类型
    /// </summary>
    public int DataType;
}