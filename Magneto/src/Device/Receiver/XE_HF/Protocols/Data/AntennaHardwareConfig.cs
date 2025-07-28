using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

internal class AntennaHardwareConfig
{
    //Configuration of the sub-ranges described in the following table
    public readonly AntennaSubRangeConfig[] SubRanges;

    //[0x800B] Antenna information
    public GroupField Antenna;

    //[0x2016] Name of the antenna
    public MultiBytesField AntennaName;

    //[0x2026] Antenna directivity :
    //1 : Omni directional
    //2 : Directional
    public UCharField Directivity;

    //[0x2030] Identification used for the CDS networks
    public MultiBytesField Identification;

    //[0x2031] Number of antenna sub-ranges
    public UCharField NumOfSubRanges;

    //[0x2027] Antenna polarisation :
    //1 : Horizontal
    //2 : Vertical
    public UCharField Polarisation;

    //[0x2029] Type of antenna :
    //0 : Listening (0 dipole)
    //1 : Direction-finding (5 dipoles)
    //2 : Direction-finding (3 dipoles)
    //3 : Listening Virtual (0 dipole)
    public UCharField Type;

    public AntennaHardwareConfig(byte[] value, ref int startIndex)
    {
        Antenna = new GroupField(value, ref startIndex);
        AntennaName = new MultiBytesField(value, ref startIndex);
        Identification = new MultiBytesField(value, ref startIndex);
        Type = new UCharField(value, ref startIndex);
        Directivity = new UCharField(value, ref startIndex);
        Polarisation = new UCharField(value, ref startIndex);
        NumOfSubRanges = new UCharField(value, ref startIndex);
        var tempSubRanges = new List<AntennaSubRangeConfig>();
        for (var i = 0; i < NumOfSubRanges.Value; ++i)
        {
            var tempSubRange = new AntennaSubRangeConfig(value, ref startIndex);
            tempSubRanges.Add(tempSubRange);
        }

        SubRanges = tempSubRanges.ToArray();
    }
}