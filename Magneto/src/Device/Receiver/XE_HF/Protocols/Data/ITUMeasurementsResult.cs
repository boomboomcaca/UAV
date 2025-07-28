using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

//[0X83] – RESULT OF ITU MEAUREMENTS
//All the results over 8 bytes are in floating format.
internal class ItuMeasurementsResult
{
    //[0x5218] VQM modulation factor
    public DoubleField AmvqmIndex;

    //[0x5202] Average antenna level in dBm, -174 if wrong measure
    public DoubleField AverageAntennaLevel;

    //[0x5207] Average field level in dBμV/M, -174 if wrong measure
    public DoubleField AverageFieldLevel;

    //[0x520C] Band at X %
    public DoubleField BetaBand;

    //[0x521E] Measured centre frequency
    public DoubleField CentreFrequency;

    //[0x5220] Deviation between the measured frequency and the selected frequency
    public DoubleField Deviation;

    //[0x2003] Actual measurement duration in seconds
    public DoubleField Duration;

    public MessageHeader Header;

    //[0x2022] Identifier of the extraction result or of the manual channel
    public UInt32Field Identifier;

    //[0x5205] LinLog antenna level in dBm, -174 if wrong measure
    public DoubleField LinlogAntennaLevel;

    //[0x520A] LinLog field level in dBμV/M, -174 if wrong measure
    public DoubleField LinlogFieldLevel;

    //[0x5206] Maximum antenna level in dBm, -174 if wrong measure
    public DoubleField MaxAntennaLevel;

    //[0x520B] Maximum field level in dBμV/M, -174 if wrong measure
    public DoubleField MaxFieldLevel;

    //[0x520E] Max. frequency at X %
    public DoubleField MaxFreqBeta;

    //[0x5211] Max. frequency at X dB at threshold 1
    public DoubleField MaxFreqXdB1;

    //[0x5214] Max. frequency at X dB at threshold 2
    public DoubleField MaxFreqXdB2;

    //[0x520D] Min. frequency at X %
    public DoubleField MinFreqBeta;

    //[0x5210] Min. frequency at X dB at threshold 1
    public DoubleField MinFreqXdB1;

    //[0x5213] Min. frequency at X dB at threshold 2
    public DoubleField MinFreqXdB2;

    //[0x5219] Minus peak modulation factor
    public DoubleField MinusPeakAmIndex;

    //[0x5216] Minus peak frequency excursion
    public DoubleField MinusPeakFreqExcursion;

    //[0x521C] Minus peak phase excursion
    public DoubleField MinusPeakPhaseExcursion;

    //[0x5221] Number of samples in the calculation
    public UInt32Field NumOfSamples;

    //[0x202D] Current active phase number
    public UInt32Field PhaseNo;

    //[0x521A] Plus peak modulation factor
    public DoubleField PlusPeakAmIndex;

    //[0x5217] Plus peak frequency excursion
    public DoubleField PlusPeakFreqExcursion;

    //[0x521D] Plus peak phase excursion
    public DoubleField PlusPeakPhaseExcursion;

    //[0x5204] Quasi-peak antenna level in dBm, -174 if wrong measure
    public DoubleField QuasiPeakAntennaLevel;

    //[0x5209] Quasi-peak field level in dBμV/M, -174 if wrong measure
    public DoubleField QuasiPeakFieldLevel;

    //[0x521F] Reference level for XdB band calculations in dBm
    public DoubleField ReferenceLevel;

    //[0x2004] Calculation time in ns (see § 2.3.2.2)
    public UInt64Field Time;

    //[0x5203] VQM antenna level in dBm, -174 if wrong measure
    public DoubleField VqmAntennaLevel;

    //[0x5208] VQM field level in dBμV/M, -174 if wrong measure
    public DoubleField VqmFieldLevel;

    //[0x5215] VQM frequency excursion
    public DoubleField VqmFreqExcursion;

    //[0x521B] VQM phase excursion
    public DoubleField VqmPhaseExcursion;

    //[0x520F] Band at X dB at threshold 1
    public DoubleField XdB1Band;

    //[0x5212] Band at X dB at threshold 2
    public DoubleField XdB2Band;

    public ItuMeasurementsResult(byte[] value, ref int startIndex)
    {
        Header = new MessageHeader(value, ref startIndex);
        PhaseNo = new UInt32Field(value, ref startIndex);
        Identifier = new UInt32Field(value, ref startIndex);
        Time = new UInt64Field(value, ref startIndex);
        NumOfSamples = new UInt32Field(value, ref startIndex);
        Duration = new DoubleField(value, ref startIndex);
        CentreFrequency = new DoubleField(value, ref startIndex);
        Deviation = new DoubleField(value, ref startIndex);
        AverageAntennaLevel = new DoubleField(value, ref startIndex);
        VqmAntennaLevel = new DoubleField(value, ref startIndex);
        QuasiPeakAntennaLevel = new DoubleField(value, ref startIndex);
        LinlogAntennaLevel = new DoubleField(value, ref startIndex);
        MaxAntennaLevel = new DoubleField(value, ref startIndex);
        AverageFieldLevel = new DoubleField(value, ref startIndex);
        VqmFieldLevel = new DoubleField(value, ref startIndex);
        QuasiPeakFieldLevel = new DoubleField(value, ref startIndex);
        LinlogFieldLevel = new DoubleField(value, ref startIndex);
        MaxFieldLevel = new DoubleField(value, ref startIndex);
        ReferenceLevel = new DoubleField(value, ref startIndex);
        XdB1Band = new DoubleField(value, ref startIndex);
        XdB2Band = new DoubleField(value, ref startIndex);
        BetaBand = new DoubleField(value, ref startIndex);
        MinFreqXdB1 = new DoubleField(value, ref startIndex);
        MinFreqXdB2 = new DoubleField(value, ref startIndex);
        MinFreqBeta = new DoubleField(value, ref startIndex);
        MaxFreqXdB1 = new DoubleField(value, ref startIndex);
        MaxFreqXdB2 = new DoubleField(value, ref startIndex);
        MaxFreqBeta = new DoubleField(value, ref startIndex);
        AmvqmIndex = new DoubleField(value, ref startIndex);
        PlusPeakAmIndex = new DoubleField(value, ref startIndex);
        MinusPeakAmIndex = new DoubleField(value, ref startIndex);
        VqmFreqExcursion = new DoubleField(value, ref startIndex);
        PlusPeakFreqExcursion = new DoubleField(value, ref startIndex);
        MinusPeakFreqExcursion = new DoubleField(value, ref startIndex);
        VqmPhaseExcursion = new DoubleField(value, ref startIndex);
        PlusPeakPhaseExcursion = new DoubleField(value, ref startIndex);
        MinusPeakPhaseExcursion = new DoubleField(value, ref startIndex);
    }
}