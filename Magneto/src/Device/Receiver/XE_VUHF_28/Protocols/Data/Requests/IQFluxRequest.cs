using System.Collections.Generic;
using Magneto.Device.XE_VUHF_28.Protocols.Field;

namespace Magneto.Device.XE_VUHF_28.Protocols.Data;

//[0X0C] – IQ FLUX REQUEST
//Broadband and narrow band IQ flux requests have the same format. Homing request to have DF results in listening mode.
internal class IqFluxRequest
{
    //[0x2032] Number of the channel from which the IQ flux is extracted. If equal to 0, the numbers of the channels
    //contained in each track which are used to manage the multiple-acquisition recordings
    public UCharField ChannelNo;

    //[0x8302] Recording duration in ms. Infinite if it is equal to 0
    public UInt32Field Duration;

    public MessageHeader Header;

    //[0x204B] Id of the narrow band channel for Homing (0 : No Homing)
    public UCharField HomingIdChannel;

    //[0x6005] Track identifier used in the results (direction-finding)
    public UInt32Field HomingIdentify;

    //[0x450F] Homing threshold value (absolute, -174 to +20 dBm)
    public ShortField HomingThreshold;

    //[0x831B] [1,4] Number of tracks
    public UCharField NumOfTracks;

    //List of recording tracks
    public IqFluxTrack[] Tracks;

    //[0x8304] Type of trigger :
    //0 : internal trigger, 1 : external trigger
    public UCharField Trigger;

    public IqFluxRequest()
    {
        Header = new MessageHeader(MessageId.MreDemFluxIq, 0);
        ChannelNo = new UCharField(0x2032);
        Duration = new UInt32Field(0x8302);
        Trigger = new UCharField(0x8304);
        HomingIdChannel = new UCharField(0x204B);
        HomingIdentify = new UInt32Field(0x6005);
        HomingThreshold = new ShortField(0x450F);
        NumOfTracks = new UCharField(0x831B);
        Tracks = null;
    }

    public byte[] GetBytes()
    {
        NumOfTracks.Value = (byte)(Tracks == null ? 0 : Tracks.Length);
        Header.ContentSize = GetSize() - MessageHeader.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Header.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(Duration.GetBytes());
        bytes.AddRange(Trigger.GetBytes());
        bytes.AddRange(HomingIdChannel.GetBytes());
        bytes.AddRange(HomingIdentify.GetBytes());
        bytes.AddRange(HomingThreshold.GetBytes());
        bytes.AddRange(NumOfTracks.GetBytes());
        if (Tracks != null)
            foreach (var track in Tracks)
                bytes.AddRange(track.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        var totalSize = MessageHeader.GetSize() + UCharField.GetSize() + UInt32Field.GetSize()
                        + UCharField.GetSize() + UCharField.GetSize() + UInt32Field.GetSize()
                        + ShortField.GetSize() + UCharField.GetSize();
        if (Tracks != null)
            foreach (var track in Tracks)
                totalSize += track.GetSize();
        return totalSize;
    }
}

internal class IqFluxTrack
{
    //[0x204D] Information if channel is activated
    public CharField Activation;

    //[0x830A] Centre frequency in Hz
    public UInt32Field CentreFrequency;

    //[0x204B] Number of the logic or physical channel (e.g. for biacquisition). The logic channels have a unique number for the entire MRE
    public UCharField ChannelNo;

    //[0x8312] Request to send the associated FFTs :
    //0 : no FFT, 1 : FFT
    public UCharField Fft;

    //[0x8313] Number of FFTs to be included (1 by default)
    public UCharField FftIntegration;

    //[0x202C] UDP reception port for the FFTs if requested,//手册为0x202B,但抓包发现为0x202C
    public UShortField FftPort;

    //[0x201E] Resolution of the FFTs in Hz (Obsolete)
    public DoubleField FftResolution;

    //[0x8306] Name of the result file, len : 80
    public MultiBytesField FileName;

    //[0x830F] IQ flux reception TCP port
    public UShortField IqPort;

    //[0x8311] Transmission mode :
    //0 : no flux, 1 : continuous mode, 2 : block mode (this message is followed by the channel sampling request)
    public UCharField Mode;

    //[0x204C] Information if channel is modified
    public CharField Modification;

    //[0x8200] Track information
    public GroupField Track;

    //[0x8310] Type of channel :
    //0 : broadband, 1 : narrow band
    public UCharField Type;

    //[0x8301] Effective bandwidth in Hz
    public DoubleField Width;

    public IqFluxTrack()
    {
        Track = new GroupField(0x8200, 0);
        ChannelNo = new UCharField(0x204B);
        Modification = new CharField(0x204C);
        Activation = new CharField(0x204D);
        IqPort = new UShortField(0x830F);
        Type = new UCharField(0x8310);
        CentreFrequency = new UInt32Field(0x830A);
        Width = new DoubleField(0x8301);
        Mode = new UCharField(0x8311);
        FileName = new MultiBytesField(0x8306, 80);
        Fft = new UCharField(0x8312);
        FftPort = new UShortField(0x202C);
        FftResolution = new DoubleField(0x201E);
        FftIntegration = new UCharField(0x8313);
    }

    public byte[] GetBytes()
    {
        Track.DataSize = GetSize() - GroupField.GetSize();
        var bytes = new List<byte>();
        bytes.AddRange(Track.GetBytes());
        bytes.AddRange(ChannelNo.GetBytes());
        bytes.AddRange(Modification.GetBytes());
        bytes.AddRange(Activation.GetBytes());
        bytes.AddRange(IqPort.GetBytes());
        bytes.AddRange(Type.GetBytes());
        bytes.AddRange(CentreFrequency.GetBytes());
        bytes.AddRange(Width.GetBytes());
        bytes.AddRange(Mode.GetBytes());
        bytes.AddRange(FileName.GetBytes());
        bytes.AddRange(Fft.GetBytes());
        bytes.AddRange(FftPort.GetBytes());
        bytes.AddRange(FftResolution.GetBytes());
        bytes.AddRange(FftIntegration.GetBytes());
        return bytes.ToArray();
    }

    public int GetSize()
    {
        return GroupField.GetSize() +
               UCharField.GetSize() +
               CharField.GetSize() +
               CharField.GetSize() +
               UShortField.GetSize() +
               UCharField.GetSize() +
               UInt32Field.GetSize() +
               DoubleField.GetSize() +
               UCharField.GetSize() +
               FileName.GetSize() +
               UCharField.GetSize() +
               UShortField.GetSize() +
               DoubleField.GetSize() +
               UCharField.GetSize();
    }
}