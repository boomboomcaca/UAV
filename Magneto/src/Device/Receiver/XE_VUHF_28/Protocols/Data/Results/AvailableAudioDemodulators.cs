using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X89] – LIST OF AUDIO DEMODULATORS AVAILABLE
internal class AvailableAudioDemodulators
{
    //List of demodulators available for the sensor
    public DemodulatorInfo[] Demodulators;

    public MessageHeader Header;

    //[0x203F] Number of demodulators
    public UCharField NumOfDemodulators;

    public AvailableAudioDemodulators(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        NumOfDemodulators = new UCharField(value, ref startIndex);
        var tempDemodulators = new List<DemodulatorInfo>();
        for (var i = 0; i < NumOfDemodulators.Value; ++i)
        {
            var tempDemodulator = new DemodulatorInfo(value, ref startIndex);
            tempDemodulators.Add(tempDemodulator);
        }

        Demodulators = tempDemodulators.ToArray();
    }
}

internal class DemodulatorInfo
{
    //[0x2017] Indicates whether the BFO (frequency offset of the BF tone) is available for this modulation :
    //0 : BFO available, 1 : BFO unavailable
    public UCharField BfoAvailable;

    //[0x2040] Demodulator information
    public GroupField Demodulator;

    //[0x200B] Type of modulation:
    //0 : A3E, 1 : F3E, 2 : H3E- , 3 : H3E+ , 4 : J3E- , 5 : J3E+ , 6 : A0, 
    //7 : F1B, 8 : A1A, 9 : N0N, 10 : R3E- , 11 : R3E+ , 12 : G3E
    public UInt32Field TypeOfModulation;

    public DemodulatorInfo(byte[] value, ref int startIndex)
    {
        Demodulator = new GroupField(value, ref startIndex);
        TypeOfModulation = new UInt32Field(value, ref startIndex);
        BfoAvailable = new UCharField(value, ref startIndex);
    }
}