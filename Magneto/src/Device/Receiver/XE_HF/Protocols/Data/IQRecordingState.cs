using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0x0B] IQ recording status
internal class IqRecordingState
{
    //[0x830A] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x2032] Number of the channel concerned
    public UCharField Channel;

    public MessageHeader Header;

    //[0x8308] Number of IQs recorded :
    //Total number (RAM recording complete)
    //Number written (recording into file in progress)
    public UCharField IqNo;

    //[0x8309] Sampling frequency in Hz
    public UInt32Field SamplingFrequency;

    //[0x8307] Recording state :
    //0 : recording complete
    //1 : RAM recording complete
    //2 : recording into file in progress
    //3 : error during recording
    //4 : recording stopped
    //5 : recording in progress
    public UCharField State;

    public IqRecordingState(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        Channel = new UCharField(value, ref startIndex);
        State = new UCharField(value, ref startIndex);
        IqNo = new UCharField(value, ref startIndex);
        SamplingFrequency = new UInt32Field(value, ref startIndex);
        CentreFrequency = new UInt32Field(value, ref startIndex);
    }
}