using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X82] – REQUEST FOR ITU
//If the message concerns a narrow band channel, it must be interpreted as configuration followed by activation of ITU measurements.
//If the message concerns an interception channel, it corresponds to configuration only. The activation of measurements being included in the interception request (0x02).
internal class ItuRequest
{
    //[0x510D] Acquisition time for an elementary measurement in seconds in floating format
    public DoubleField AcquisitionTime;

    //[0x510E] Antenna charging time in seconds, in floating format
    public DoubleField AntennaChargeTime;

    //[0x510F] Antenna discharge time in seconds, in floating format
    public DoubleField AntennaDischargeTime;

    //[0x5105] Band calculation threshold at X % in floating format
    public DoubleField BetaBandThreshold;

    //[0x204B] Id of the narrow band channel
    public UCharField ChannelId;

    //[0x2032] Number of the channel
    public UCharField ChannelNo;

    //[0x510A] Type of FFT window :
    //0 : Hamming
    //1 : Hanning
    //2 : Rectangular
    //3 : Blackman
    //4 : Blackman Harris 74
    //5 : Blackman Harris 92
    //6 : Flat Top
    public CharField FftWindow;

    //[0x5110] Field charging time in seconds, in floating format
    public DoubleField FieldChargeTime;

    //[0x5111] Field discharge time in seconds, in floating format
    public DoubleField FieldDischargeTime;

    //[0x5106] Weighting of VQM AM values in floating format
    public DoubleField GammaVqmam;

    //[0x5107] Weighting of VQM FM values in floating format
    public DoubleField GammaVqmfm;

    //[0x5108] Weighting of VQM PM values in floating format
    public DoubleField GammaVqmpm;

    public MessageHeader Header;

    //[0x5101] Measurement mode :
    //0 : Single
    //1 : Repetitive
    //2 : Continuous loop
    public CharField Mode;

    //[0x5109] Number of FFT points
    public Int32Field NumOfFftPoints;

    //[0x510B] Integration (result provided after N elementary measurements).
    public UShortField NumOfIntegrations;

    //[0x5102] Number of loops in continuous loop mode.
    public UShortField NumOfLoops;

    //[0x510C] Type of integration :
    //0 : Normal (no integration)
    //1 : Linear average
    //2 : Root mean square
    //3 : Min Hold
    //4 : Max Hold
    public CharField TypeOfIntegration;

    //[0x202C] UDP reception port for the results, //TODO:手册为0x202B,但抓包发现为0x202C
    public UShortField UdpPort;

    //[0x5103] Band calculation threshold at X dB (1) in dB in floating format
    public DoubleField XdB1Threshold;

    //[0x5104] Band calculation threshold at X dB (2) in dB in floating format
    public DoubleField XdB2Threshold;

    public ItuRequest()
    {
        Header = new MessageHeader(MessageId.MreDemMesureuit, 0);
        ChannelNo = new UCharField(0x2032);
        UdpPort = new UShortField(0x202C);
        AntennaChargeTime = new DoubleField(0x510E);
        AntennaDischargeTime = new DoubleField(0x510F);
        FieldChargeTime = new DoubleField(0x5110);
        FieldDischargeTime = new DoubleField(0x5111);
        XdB1Threshold = new DoubleField(0x5103);
        XdB2Threshold = new DoubleField(0x5104);
        BetaBandThreshold = new DoubleField(0x5105);
        GammaVqmam = new DoubleField(0x5106);
        GammaVqmfm = new DoubleField(0x5107);
        GammaVqmpm = new DoubleField(0x5108);
        NumOfFftPoints = new Int32Field(0x5109);
        FftWindow = new CharField(0x510A);
        Mode = new CharField(0x5101);
        NumOfLoops = new UShortField(0x5102);
        NumOfIntegrations = new UShortField(0x510B);
        TypeOfIntegration = new CharField(0x510C);
        AcquisitionTime = new DoubleField(0x510D);
        ChannelId = new UCharField(0x204B);
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(UdpPort.GetBytes());
        bytes.AddRange(AntennaChargeTime.GetBytes());
        bytes.AddRange(AntennaDischargeTime.GetBytes());
        bytes.AddRange(FieldChargeTime.GetBytes());
        bytes.AddRange(FieldDischargeTime.GetBytes());
        bytes.AddRange(XdB1Threshold.GetBytes());
        bytes.AddRange(XdB2Threshold.GetBytes());
        bytes.AddRange(BetaBandThreshold.GetBytes());
        bytes.AddRange(GammaVqmam.GetBytes());
        bytes.AddRange(GammaVqmfm.GetBytes());
        bytes.AddRange(GammaVqmpm.GetBytes());
        bytes.AddRange(NumOfFftPoints.GetBytes());
        bytes.AddRange(FftWindow.GetBytes());
        bytes.AddRange(Mode.GetBytes());
        bytes.AddRange(NumOfLoops.GetBytes());
        bytes.AddRange(NumOfIntegrations.GetBytes());
        bytes.AddRange(TypeOfIntegration.GetBytes());
        bytes.AddRange(AcquisitionTime.GetBytes());
        bytes.AddRange(ChannelId.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return MessageHeader.GetSize() + UCharField.GetSize() + UShortField.GetSize() + DoubleField.GetSize() +
               DoubleField.GetSize() + DoubleField.GetSize() + DoubleField.GetSize() + DoubleField.GetSize() +
               DoubleField.GetSize() + DoubleField.GetSize() + DoubleField.GetSize() + DoubleField.GetSize() +
               DoubleField.GetSize() + Int32Field.GetSize() + CharField.GetSize() + CharField.GetSize() +
               UShortField.GetSize() + UShortField.GetSize() + CharField.GetSize() + DoubleField.GetSize() +
               UCharField.GetSize();
    }
}