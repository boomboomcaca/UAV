using System.Collections.Generic;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF.Commands;

internal class DeviceParams
{
    public uint Version { get; set; }
    public int UdpBbfftPort { get; set; }
    public int UdpNbituPort { get; set; }
    public int UdpNbfftPort { get; set; }
    public int UdpAudioPort { get; set; }
    public int UdpDfPort { get; set; }
    public double Frequency { get; set; }
    public double IfBandWidth { get; set; }
    public double DfBandWidth { get; set; }
    public double ChannelBandWidth { get; set; }
    public double StartFrequency { get; set; }
    public double StopFrequency { get; set; }
    public double StepFrequency { get; set; }
    public int Attenuation { get; set; }
    public float XdB { get; set; }
    public float Beta { get; set; }
    public int DFindMode { get; set; }
    public int Chan { get; set; }
    public int LevelThreshold { get; set; }
    public int QualityThreshold { get; set; }
    public int SquelchThreshold { get; set; }
    public int XeIntTime { get; set; }
    public int MaxChanCount { get; set; }
    public ushort DetectionMode { get; set; }
    public Modulation DemMode { get; set; }
    public bool SquelchSwitch { get; set; }
    public bool AudioSwitch { get; set; }
    public bool AmpliConfig { get; set; }
    public bool Sensitivity { get; set; }
    public bool FmFilter { get; set; }
    public string CurrAntenna { get; set; }
    public Dictionary<string, object>[] Frequencys { get; set; }
    public Dictionary<string, object>[] IfMultiChannels { get; set; }
}