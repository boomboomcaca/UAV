using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X02] – BROADBAND INTERCEPTION REQUEST
internal class BbInterceptionRequest
{
    //Interception band (cf. below)
    public readonly BandDef Band;

    //Cf. masks request message
    public readonly DemMask Mask; //TODO:确认是否为List

    //[0x2032] Channel number on which broadband interception is launched
    public UCharField ChannelNo;

    //[0x2037] Detection mode (not used) :
    //1 : detection achieved before integration 
    //n : nT mode, detection after integration
    public UShortField DetectionMode;

    public MessageHeader Header;

    //[0x4513] NT mode integration time in ms (not used in the TRC6200 product, used only in Esmeralda XE product through the CDS missions).
    public UShortField IntTime;

    //[0x4512] OR logic for the following flags :
    /*0x00000001 : broadband FFT
       0x00000002 : broadband direction-finding results
       0x00000004 : extraction results
       0x00000008 : tracking results (not used)
       0x00000010 : ITU measurement results
       0x00000020 : percentage occupancy results
       0x00000040 : maintenance direction-finding results
       0x00000080 : narrow band IQ flux
       0x00000100 : mission
       0x00000200 : CTP mode (synchronization with a jammer)
       0x00000400 : Burst extraction on one direction-finding cycle for satellite communications
       0x00000800 : optional direction-finding results for frequency extraction*/
    public UInt32Field MeasurementsRequested;

    //[0x2004] Current time of the operation in ns (not used in the TRC6200 product, used only in Esmeralda XE product through the CDS missions).
    public UInt64Field OperatingTime;

    //[0x4511] Phase number used in the results relative to this request
    public UInt32Field PhaseNo;

    //[0x450E] Indicates the threshold calculation mode :
    //0 : absolute threshold
    //1 : relative threshold with respect to the average noise level calculated under Linux
    public CharField RelativeThreshold;

    //[0x450C] Resolution in Hz
    public DoubleField Resolution;

    //[0x450D] Sensitivity : 0 : sensitive, 1 : fast
    public UCharField Sensitivity;

    //[25版本特有] [0x4514] Extraction threshold max value (absolute, -174 to +20) or relative, 0 to +100)
    public ShortField ThresholdMaxValue;

    //[0x450F] Extraction threshold min value (absolute, -174 to +20) or relative, 0 to +100)
    public ShortField ThresholdMinValue;

    //[0x4510] Turbo mode activation :
    //0 : normal
    //1 : fast interception without DF
    //2 : fast interception with DF (TRC6200 unit only)
    public UCharField Turbo;

    //[0x202C] Broadband FFT reception port
    public UShortField UdpFftwbPort;

    //public List<MaskingReq>
    public BbInterceptionRequest()
    {
        Header = new MessageHeader(MessageId.MreIntLb, 0);
        ChannelNo = new UCharField(0x2032);
        UdpFftwbPort = new UShortField(0x202C);
        DetectionMode = new UShortField(0x2037);
        Resolution = new DoubleField(0x450C);
        Sensitivity = new UCharField(0x450D);
        RelativeThreshold = new CharField(0x450E);
        ThresholdMinValue = new ShortField(0x450F);
        ThresholdMaxValue = new ShortField(0x4514);
        Turbo = new UCharField(0x4510);
        PhaseNo = new UInt32Field(0x4511);
        MeasurementsRequested = new UInt32Field(0x4512);
        IntTime = new UShortField(0x4513);
        OperatingTime = new UInt64Field(0x2004);
        Band = new BandDef();
        Mask = new DemMask();
    }

    public byte[] GetBytes()
    {
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(UdpFftwbPort.GetBytes());
        bytes.AddRange(DetectionMode.GetBytes());
        bytes.AddRange(Resolution.GetBytes());
        bytes.AddRange(Sensitivity.GetBytes());
        bytes.AddRange(RelativeThreshold.GetBytes());
        bytes.AddRange(ThresholdMinValue.GetBytes());
        bytes.AddRange(ThresholdMaxValue.GetBytes());
        bytes.AddRange(Turbo.GetBytes());
        bytes.AddRange(PhaseNo.GetBytes());
        bytes.AddRange(MeasurementsRequested.GetBytes());
        bytes.AddRange(IntTime.GetBytes());
        bytes.AddRange(OperatingTime.GetBytes());
        bytes.AddRange(Band.GetBytes());
        bytes.AddRange(Mask.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() +
                        UCharField.GetSize() +
                        UShortField.GetSize() +
                        UShortField.GetSize() +
                        DoubleField.GetSize() +
                        UCharField.GetSize() +
                        CharField.GetSize() +
                        ShortField.GetSize() +
                        ShortField.GetSize() +
                        UCharField.GetSize() +
                        UInt32Field.GetSize() +
                        UInt32Field.GetSize() +
                        UShortField.GetSize() +
                        UInt64Field.GetSize() +
                        Band.GetSize() +
                        Mask.GetSize();
        return totalSize;
    }
}

internal class DemMask
{
    //List of bands to be masked
    public readonly BandDef[] Bands;

    //[0x4515] Mask information
    public GroupField Mask;

    //[0x2035] Number of bands to be masked
    public UShortField NumberOfBands;

    public DemMask()
    {
        Mask = new GroupField(0x4515, 0);
        NumberOfBands = new UShortField(0x2035);
        Bands = null;
    }

    public byte[] GetBytes()
    {
        NumberOfBands.Value = (ushort)(Bands == null ? 0 : Bands.Length);
        Mask.DataSize = GetSize() - GroupField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Mask.GetBytes());
        bytes.AddRange(NumberOfBands.GetBytes());
        if (Bands != null)
            foreach (var band in Bands)
                bytes.AddRange(band.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = GroupField.GetSize() + UShortField.GetSize();
        if (Bands != null)
            foreach (var band in Bands)
                totalSize += band.GetSize();
        return totalSize;
    }
}

internal class BandDef
{
    //[0x2036] Interception band information
    public GroupField Band;

    //[0x2010] Maximum interception frequency in Hz
    public UInt32Field FMax;

    //[0x200F] Minimum interception frequency in Hz
    public UInt32Field FMin;

    public BandDef()
    {
        Band = new GroupField(0x2036, 0);
        FMin = new UInt32Field(0x200F);
        FMax = new UInt32Field(0x2010);
    }

    public byte[] GetBytes()
    {
        Band.DataSize = GetSize() - GroupField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Band.GetBytes());
        bytes.AddRange(FMin.GetBytes());
        bytes.AddRange(FMax.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return GroupField.GetSize() + UInt32Field.GetSize() + UInt32Field.GetSize();
    }
}