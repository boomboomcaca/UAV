using System.Runtime.InteropServices;

namespace Magneto.Device.G33DDC.SDK;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct G33DdcDeviceInfo
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public readonly string DevicePath;

    [MarshalAs(UnmanagedType.U1)] public readonly byte InterfaceType;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 9)]
    public readonly string SerialNumber;

    [MarshalAs(UnmanagedType.U4)] public readonly uint ChannelCount;
    [MarshalAs(UnmanagedType.U4)] public readonly uint DDCTypeCount;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct G3XddcDdcInfo
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint SampleRate;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Bandwidth;
    [MarshalAs(UnmanagedType.U4)] public readonly uint BitsPerSample;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
public struct G3XddcAmsCaptureRange
{
    [MarshalAs(UnmanagedType.U4)] public readonly uint Tune;
    [MarshalAs(UnmanagedType.U4)] public readonly uint Lock;
}