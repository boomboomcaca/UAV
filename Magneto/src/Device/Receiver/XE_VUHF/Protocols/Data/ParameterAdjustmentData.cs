using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

//[0X60] – INFORMATION ADJUSTMENT PARAMETERS
//Message used especially for adjusting the relative extraction threshold.
internal class ParameterAdjustmentData
{
    //[0x8500] Code of the information adjustment to be made
    public UCharField Cause;

    public MessageHeader Header;

    //[0x8501] Code of the parameter to be adjusted by the operator
    public UCharField ParamToAdjust;

    //[0x8502] Possible value of the parameter in floating format
    public FloatField Value;

    public ParameterAdjustmentData(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Cause = new UCharField(value, ref startIndex);
        ParamToAdjust = new UCharField(value, ref startIndex);
        Value = new FloatField(value, ref startIndex);
    }
}