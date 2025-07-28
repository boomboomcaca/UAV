using System;
using System.Text;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;

namespace Magneto.Device.XE_VUHF_28.Commands;

internal class WbdfCommand : CommandBase
{
    private DfQualityThresholdConfig _qualityConfig;

    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 1;
        var chan = new ChannelProgramming();
        chan.ChannelNo.Value = 1;
        chan.FMin.Value = 0;
        chan.FMax.Value = 0;
        if (device.Attenuation == -1)
        {
            chan.AgcType.Value = 2;
            chan.RfAttenuator.Value = 0;
            chan.IfAttenuator.Value = 0;
        }
        else
        {
            int rfAtt = 0, ifAtt = 0;
            PartAttenuation(device.Attenuation, ref rfAtt, ref ifAtt);
            chan.AgcType.Value = 1;
            chan.RfAttenuator.Value = (byte)rfAtt;
            chan.IfAttenuator.Value = (byte)ifAtt;
        }

        chan.AmpliConfig.Value = (byte)(device.AmpliConfig ? AmpliValue : 1);
        var antenna = Encoding.ASCII.GetBytes(device.CurrAntenna);
        Array.Copy(antenna, chan.Antenna.Value, antenna.Length);
        chan.FmFilter.Value = 2; //TODO: 宽带测向未暴露此参数
        chan.LevelUnits.Value = 1;
        ProgramingInfo.Channels = new[] { chan };
        SendCmd(ProgramingInfo.GetBytes());
        var centerFrequencyHz = (uint)(device.Frequency * 1000000);
        var bwHz = (uint)(device.DfBandWidth * 1000);
        var rbwHz = device.ResolutionBandwidth * 1000d;
        GetBbInfo(centerFrequencyHz, bwHz, false, ref rbwHz, out var minFreq, out var maxFreq);
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = rbwHz;
        BbRequest.Sensitivity.Value = 1; //(byte)(device.Sensitivity ? 0 : 1);
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = (short)(device.LevelThreshold - 107);
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = PhaseNo;
        BbRequest.MeasurementsRequested.Value = 0x00000007;
        BbRequest.Band.FMin.Value = minFreq;
        BbRequest.Band.FMax.Value = maxFreq;
        BbRequest.Mask.NumberOfBands.Value = 0;
        SendCmd(BbRequest.GetBytes());
        //测向质量
        _qualityConfig = new DfQualityThresholdConfig();
        _qualityConfig.QualityMask.Value = XeAssister.GetQualityMark(device.QualityThreshold);
        SendCmd(_qualityConfig.GetBytes());
    }

    public override void SetLevelThreshold(int levelThreshold)
    {
        base.SetLevelThreshold(levelThreshold);
        BbRequest.ThresholdMinValue.Value = (short)(levelThreshold - 107);
        SendCmd(BbRequest.GetBytes());
    }

    public override void SetQualityThreshold(int quality)
    {
        base.SetQualityThreshold(quality);
        _qualityConfig.QualityMask.Value = XeAssister.GetQualityMark(quality);
        SendCmd(_qualityConfig.GetBytes());
    }
}