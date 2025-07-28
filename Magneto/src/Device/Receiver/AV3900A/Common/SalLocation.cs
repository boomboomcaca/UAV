using System.Runtime.InteropServices;

namespace Magneto.Device.AV3900A.Common;

[StructLayout(LayoutKind.Sequential)]
public struct SalLocation
{
    public readonly double Latitude;
    public readonly double Longitude;
    public readonly double Altitude;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 132, ArraySubType = UnmanagedType.I1)]
    public readonly byte[] Reserved;
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    //public string Coordinate;
    //public double Heading;
    //public double TrackAngle;
    //public double Speed;
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 24)]
    //public string Unit;
}