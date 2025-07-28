using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X71] – SOFTWARE CONFIGURATION RESULT
internal class SoftwareConfigResult
{
    //List of software configurations for each channel
    public ChannelSoftwareConfig[] Channels;

    public MessageHeader Header;

    //[0x2032] Number of logic channels (all blocks included)
    public UCharField NumOfChannels;

    public SoftwareConfigResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfChannels = new UCharField(value, ref startIndex);
        var tempChannels = new List<ChannelSoftwareConfig>();
        for (var i = 0; i < NumOfChannels.Value; ++i)
        {
            var tempChannel = new ChannelSoftwareConfig(value, ref startIndex);
            tempChannels.Add(tempChannel);
        }

        Channels = tempChannels.ToArray();
    }
}

internal class ChannelSoftwareConfig
{
    //[0x8702] Channel information
    public GroupField Channel;

    //[0x8026] List of possible effective bands in narrow band (50*DOUBLE)
    public UserField<double> EffectiveNbBands;

    //[0x8025] List of turbo interception resolutions (50*DOUBLE)
    public UserField<double> FastResolutions;

    //[0x8027] List of Fe associated with the effective bands in narrow band (50*DOUBLE)
    public UserField<double> FeNBs;

    //[0x8023] Indicates whether the FM filter and the amplifier is available
    public CharField FmFilter;

    //[0x2002] Width in Hz of a max. unitary band
    public UInt32Field MaxUnitaryBand;

    //[0x8029] List of channels NB MAX concerning each listening filter (50 * UCHAR)
    public UserField<byte> NbMaxChannels; //TODO:

    //[0x8021] Number of effective bands in narrow band
    public UCharField NumOfEffectiveNbBands;

    //[0x8020] Number of interception resolutions in turbo mode
    public UCharField NumOfFastInterceptionResolutions;

    //[0x8022] Number of RF gains
    public UCharField NumOfGains;

    //[0x801F] Number of interception resolutions
    public UCharField NumOfInterceptionResolutions;

    //[0x8024] List of interception resolutions (50*DOUBLE)
    public UserField<double> Resolutions;

    //List of RF gains
    public GainSoftwareConfig[] RfGains;

    public ChannelSoftwareConfig(byte[] value, ref int startIndex)
    {
        Channel = new GroupField(value, ref startIndex);
        NumOfInterceptionResolutions = new UCharField(value, ref startIndex);
        NumOfFastInterceptionResolutions = new UCharField(value, ref startIndex);
        NumOfEffectiveNbBands = new UCharField(value, ref startIndex);
        NumOfGains = new UCharField(value, ref startIndex);
        MaxUnitaryBand = new UInt32Field(value, ref startIndex);
        FmFilter = new CharField(value, ref startIndex);
        Resolutions = new UserField<double>(value, ref startIndex);
        FastResolutions = new UserField<double>(value, ref startIndex);
        EffectiveNbBands = new UserField<double>(value, ref startIndex);
        FeNBs = new UserField<double>(value, ref startIndex);
        NbMaxChannels = new UserField<byte>(value, ref startIndex);
        var tempGains = new List<GainSoftwareConfig>();
        for (var i = 0; i < NumOfGains.Value; ++i)
        {
            var tempGain = new GainSoftwareConfig(value, ref startIndex);
            tempGains.Add(tempGain);
        }

        RfGains = tempGains.ToArray();
    }
}

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