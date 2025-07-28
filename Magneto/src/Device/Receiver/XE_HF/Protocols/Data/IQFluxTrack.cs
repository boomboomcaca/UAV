using System.Collections.Generic;
using Magneto.Device.XE_HF.Protocols.Field;

namespace Magneto.Device.XE_HF.Protocols.Data;

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
        Track.DataSize = GetSize() - Track.GetSize();
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
        return Track.GetSize() +
               ChannelNo.GetSize() +
               Modification.GetSize() +
               Activation.GetSize() +
               IqPort.GetSize() +
               Type.GetSize() +
               CentreFrequency.GetSize() +
               Width.GetSize() +
               Mode.GetSize() +
               FileName.GetSize() +
               Fft.GetSize() +
               FftPort.GetSize() +
               FftResolution.GetSize() +
               FftIntegration.GetSize();
    }
}