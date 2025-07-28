using System;
using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

/// <summary>
///     频谱测量参数
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SalSweepParams
{
    /// <summary>
    ///     执行的扫描次数
    /// </summary>
    public uint NumSweeps;

    /// <summary>
    ///     本次扫描的段数
    /// </summary>
    public uint NumSegments;

    /// <summary>
    ///     执行FFT之前加窗类型
    /// </summary>
    public readonly SalWindowType Window;

    /// <summary>
    ///     自定义数值，随着每一段数据返回
    /// </summary>
    public UIntPtr UserWorkspace;

    /// <summary>
    ///     频谱返回的数据类型
    /// </summary>
    public readonly int DataType;

    /// <summary>
    ///     reserved
    /// </summary>
    public readonly int Reserved1;

    /// <summary>
    ///     非0开启同步扫描
    /// </summary>
    public readonly int SyncSweepEnable;

    /// <summary>
    ///     monitorMode为salMonitorMode_on在此时间间隔返回数据
    /// </summary>
    public double SweepInterval;

    /// <summary>
    ///     第一段扫描的起始时间秒
    /// </summary>
    public readonly uint SyncSweepSec;

    /// <summary>
    ///     第一段扫描的起始时间纳秒
    /// </summary>
    public readonly uint SyncSweepNSec;

    /// <summary>
    ///     取消或开启监听模式
    /// </summary>
    public readonly SalMonitorMode MonitorMode;

    /// <summary>
    ///     monitorMode是salMonitorMode_on时，在此间隔回发结果
    /// </summary>
    public readonly double MonitorInterval;

    /// <summary>
    ///     Parameter used internally
    /// </summary>
    public UIntPtr Reserved;
}