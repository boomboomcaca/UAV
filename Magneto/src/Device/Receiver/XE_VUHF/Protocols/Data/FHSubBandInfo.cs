using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class FhSubBandInfo
{
    //[0x8405]Centre frequency of the band in Hz
    public UInt32Field CentreFrequency;

    //[0x840A]Average level in dBm
    public ShortField Level;

    //[0x840A]Average level in dBm (OBSOLETE, use Level field bases on short type)
    public CharField LevelObsolete;

    //[0x8414] Sub-band information
    public GroupField SubBand;

    //[0x8406] Width of the band in Hz
    public UInt32Field Width;

    public FhSubBandInfo(byte[] value, ref int startIndex)
    {
        SubBand = new GroupField(value, ref startIndex);
        CentreFrequency = new UInt32Field(value, ref startIndex);
        Width = new UInt32Field(value, ref startIndex);
        LevelObsolete = new CharField(value, ref startIndex);
        Level = new ShortField(value, ref startIndex);
    }
}