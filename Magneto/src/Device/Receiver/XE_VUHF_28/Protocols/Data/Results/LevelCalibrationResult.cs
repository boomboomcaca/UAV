using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X75] – LEVEL CALIBRATION RESULTS
internal class LevelCalibrationResult
{
    //[0x8B06] Floating format level after broadband filter correction
    public FloatField Bb;

    //[0x8B07] Floating format level after BB + IF correction
    public FloatField Bbif;

    //[0x8B02] Block number
    public UShortField BlockNo;

    //[0x8B09] Fully corrected level in floating format
    public FloatField CorrectedLevel;

    //[0x8B04] Frequency of the result in Hz
    public UInt32Field Frequency;

    public MessageHeader Header;

    //[0x8B03] Head number
    public UCharField HeadNo;

    //[0x8B05] Floating format measurement without correction
    public FloatField Raw;

    //[0x8B08] Floating format level after correction by the broadband filter, the IF attenuator and the position of
    //the head on the reference antenna (sub-range 2).
    public FloatField Ref;

    public LevelCalibrationResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        BlockNo = new UShortField(value, ref startIndex);
        HeadNo = new UCharField(value, ref startIndex);
        Frequency = new UInt32Field(value, ref startIndex);
        Raw = new FloatField(value, ref startIndex);
        Bb = new FloatField(value, ref startIndex);
        Bbif = new FloatField(value, ref startIndex);
        Ref = new FloatField(value, ref startIndex);
        CorrectedLevel = new FloatField(value, ref startIndex);
    }
}