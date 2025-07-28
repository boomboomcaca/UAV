using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X9E] – SEND TABLE OF MEMORY SCANNING EMISSION
//List of emission for memory scanning
internal class MemoryScanningEmissions
{
    //List of emission
    public MScanEmission[] Emissions;

    public MessageHeader Header;

    //[0x9300] Number of emission
    public UInt32Field NumOfEmission;

    public MemoryScanningEmissions()
    {
        Header = new MessageHeader(MessageId.MreVcyTableEmission, 0);
        NumOfEmission = new UInt32Field(0x9300);
        Emissions = null;
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        NumOfEmission.Value = (uint)(Emissions == null ? 0 : Emissions.Length);
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(NumOfEmission.GetBytes());
        if (Emissions != null)
            foreach (var emission in Emissions)
                bytes.AddRange(emission.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() + UInt32Field.GetSize();
        if (Emissions != null)
            foreach (var emission in Emissions)
                totalSize += emission.GetSize();
        return totalSize;
    }
}

internal class MScanEmission
{
    //[0x2039] Type of automatic gain control : 1 : manual, 2 : auto
    public UCharField AgcType;

    //[0x2014] [1, 16] Amplification configuration: 
    //bit 0/1 : RF head (1 normal, 2 high sensibility) for IRS compatibility
    //bit 2 : DF ampli
    //bit 3 : Listening ampli
    //bit 5 : Sub-Range 3 ampli (DF antenna)
    public UCharField AmpliConfig;

    //[0x2016] Current antenna selected
    public MultiBytesField Antenna;

    //[0x2002] Bandwidth in Hz
    public UInt32Field Bandwidth;

    //[0x2017] Translation frequency in Hz
    public Int32Field Bfo;

    //[0x830A] Centre frequency in Hz
    public UInt64Field CentreFrequency;

    //[0x9302] Emission information
    public GroupField Emission;

    //[0x9317] Type of emission : 0 : discrete frequency,  1 : band of frequency
    public UCharField EmissionType;

    //[0x2015] Positioning of the broadcast filter, 2 : inactive, 1 : active
    public UCharField FmFilter;

    //[0x9301] Identifier of the emission
    public UInt32Field Identifier;

    //[0x2013] Position of the IF attenuator, [0, 63]
    public UCharField IfAttenuator;

    //[0x9309] Request of IQ flux, 0,1
    public UCharField IqFlux;

    //[0x9307] Request of IQ recording: 0, 1 
    public UCharField IqRecord;

    //[0x9310] ITU measurements, 0,1
    public UCharField Itu;

    //[0x9306] Request of listening: 0,1
    public UCharField Listening;

    //[0x9313] Preservation of scanning duration (ms)
    public Int64Field PreservationDuration;

    //[0x9312] Duration between 2 scanning emission (ms)
    public Int64Field ReloadingDuration;

    //[0x450C] Resolution in Hz
    public DoubleField Resolution;

    //[0x2012] Position of the RF attenuator, 0, 10, 20, 32
    public UCharField RfAttenuator;

    //[0x450D] Sensitivity: 0 : sensitive, 1 : fast
    public UCharField Sensitivity;

    //[0x2019] Channel threshold : in dBm (relative), in dB (absolute)
    public ShortField Threshold;

    //[0x450E] Type Emission threshold, 0 : Absolute, 1 : Relative
    public UCharField ThresholdType;

    //[0x9311] Emission total duration (ms)
    public Int64Field TotalDuration;

    //[0x200B]  Type of modulation :       
    //0 : A3E, 1 : F3E, 2 : H3E- , 3 : H3E+ , 4 : J3E- , 5 : J3E+ , 6 : A0, 
    //7 : F1B, 8 : A1A, 9 : N0N, 10 : R3E- , 11 : R3E+ , 12 : G3E
    public UInt32Field TypeOfModulation;

    //[0x9308] Request of Wave recording: 0, 1
    public UCharField WaveRecord;

    public MScanEmission()
    {
        Emission = new GroupField(0x9302, 0);
        Identifier = new UInt32Field(0x9301);
        EmissionType = new UCharField(0x9317);
        CentreFrequency = new UInt64Field(0x830A);
        Bandwidth = new UInt32Field(0x2002);
        ThresholdType = new UCharField(0x450E);
        Threshold = new ShortField(0x2019);
        Resolution = new DoubleField(0x450C);
        Bfo = new Int32Field(0x2017);
        TypeOfModulation = new UInt32Field(0x200B);
        AgcType = new UCharField(0x2039);
        RfAttenuator = new UCharField(0x2012);
        IfAttenuator = new UCharField(0x2013);
        AmpliConfig = new UCharField(0x2014);
        FmFilter = new UCharField(0x2015);
        Antenna = new MultiBytesField(0x2016, 32);
        Sensitivity = new UCharField(0x450D);
        Listening = new UCharField(0x9306);
        IqRecord = new UCharField(0x9307);
        WaveRecord = new UCharField(0x9308);
        IqFlux = new UCharField(0x9309);
        Itu = new UCharField(0x9310);
        TotalDuration = new Int64Field(0x9311);
        ReloadingDuration = new Int64Field(0x9312);
        PreservationDuration = new Int64Field(0x9313);
    }

    public byte[] GetBytes()
    {
        Emission.DataSize = GetSize() - GroupField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Emission.GetBytes());
        bytes.AddRange(Identifier.GetBytes());
        bytes.AddRange(EmissionType.GetBytes());
        bytes.AddRange(CentreFrequency.GetBytes());
        bytes.AddRange(Bandwidth.GetBytes());
        bytes.AddRange(ThresholdType.GetBytes());
        bytes.AddRange(Threshold.GetBytes());
        bytes.AddRange(Resolution.GetBytes());
        bytes.AddRange(Bfo.GetBytes());
        bytes.AddRange(TypeOfModulation.GetBytes());
        bytes.AddRange(AgcType.GetBytes());
        bytes.AddRange(RfAttenuator.GetBytes());
        bytes.AddRange(IfAttenuator.GetBytes());
        bytes.AddRange(AmpliConfig.GetBytes());
        bytes.AddRange(FmFilter.GetBytes());
        bytes.AddRange(Antenna.GetBytes());
        bytes.AddRange(Sensitivity.GetBytes());
        bytes.AddRange(Listening.GetBytes());
        bytes.AddRange(IqRecord.GetBytes());
        bytes.AddRange(WaveRecord.GetBytes());
        bytes.AddRange(IqFlux.GetBytes());
        bytes.AddRange(Itu.GetBytes());
        bytes.AddRange(TotalDuration.GetBytes());
        bytes.AddRange(ReloadingDuration.GetBytes());
        bytes.AddRange(PreservationDuration.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return Emission.Size +
               Identifier.Size +
               EmissionType.Size +
               CentreFrequency.Size +
               Bandwidth.Size +
               ThresholdType.Size +
               Threshold.Size +
               Resolution.Size +
               Bfo.Size +
               TypeOfModulation.Size +
               AgcType.Size +
               RfAttenuator.Size +
               IfAttenuator.Size +
               AmpliConfig.Size +
               FmFilter.Size +
               Antenna.Size +
               Sensitivity.Size +
               Listening.Size +
               IqRecord.Size +
               WaveRecord.Size +
               IqFlux.Size +
               Itu.Size +
               TotalDuration.Size +
               ReloadingDuration.Size +
               PreservationDuration.Size;
    }
}