using Magneto.Device.XE_VUHF.Protocols.Field;

namespace Magneto.Device.XE_VUHF.Protocols.Data;

internal class GainSoftwareConfig
{
    //[0x8028] Gain information
    public GroupField Gain;

    //[0x801E] Value of the IF attenuation
    public UCharField IfAtt;

    //[0x801C] RF amplification (OBSOLETE since using complete amplification configuration in PROG_RX message)
    public CharField RfAmpli;

    //[0x801D] Value of the RF attenuation
    public UCharField RfAtt;

    public GainSoftwareConfig(byte[] value, ref int startIndex)
    {
        Gain = new GroupField(value, ref startIndex);
        RfAmpli = new CharField(value, ref startIndex);
        RfAtt = new UCharField(value, ref startIndex);
        IfAtt = new UCharField(value, ref startIndex);
    }
}