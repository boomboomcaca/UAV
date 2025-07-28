using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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