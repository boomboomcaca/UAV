using System.Runtime.InteropServices;

namespace GsmReceiver.Base;

[StructLayout(LayoutKind.Sequential)]
public struct RachStruct
{
    public readonly byte request_RA;
    public readonly int request_FN;
    public readonly short rach_rxlevel;
    public readonly short rach_TN;
}