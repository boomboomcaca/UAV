using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class FftInfo
{
    //[0x8208] FFT information
    public GroupField Fft;

    //[0x200F] Minimum frequency of the FFT in Hz
    public UInt32Field FMin;

    //[0x8200] List of levels in dBm (N*CHAR) [-128,20]
    public UserField<sbyte> Levels;

    //[0x2032] Number of FFT bins
    public UInt32Field NumOfBins;

    public FftInfo(byte[] value, ref int startIndex)
    {
        Fft = new GroupField(value, ref startIndex);
        NumOfBins = new UInt32Field(value, ref startIndex);
        FMin = new UInt32Field(value, ref startIndex);
        Levels = new UserField<sbyte>(value, ref startIndex);
    }
}