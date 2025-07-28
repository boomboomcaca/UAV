using System.Runtime.InteropServices;

namespace Magneto.Device.DT1000AS.Driver.Base;

[StructLayout(LayoutKind.Sequential)]
public struct SmsStruct
{
    public readonly ushort NumPacketRx;
    public ushort dataIdx;
    public readonly ushort sms_len;
    public byte sms_flag;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
    public byte[] dataBuff;
}