using System;
using System.Text;
using Magneto.Device.XE_HF.Protocols;
using Magneto.Device.XE_HF.Protocols.Data;

namespace Magneto.Device.XE_HF.Commands;

/*
 * 频段扫描实现方案：
 * 1.运行时不可修改参数
 * 2.将快速截收和灵敏度模式合并为1个参数扫描模式，按速度快慢分为以下三种
 *   快速：快速截收，25k，快速模式
 *   常规：步进可调，快速模式
 *   灵敏：25K及以下步进可调，灵敏模式
 */
internal class ScanCommand : CommandBase
{
    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 1;
        ProgramingInfo.Channels = new ChannelProgramming[1];
        ProgramingInfo.Channels[0] = new ChannelProgramming();
        var chan = ProgramingInfo.Channels[0];
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
        chan.FmFilter.Value = 2;
        chan.LevelUnits.Value = 1;
        SendCmd(ProgramingInfo.GetBytes());
        //宽带参数设置
        var sensitivity = (byte)(device.ScanMode == (int)XeScanMode.Sensitivity ? 0 : 1);
        var turbo = (byte)(device.ScanMode == (int)XeScanMode.Fast ? 1 : 0);
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = device.StepFrequency * 1000;
        BbRequest.Sensitivity.Value = sensitivity; //(byte)(device.Sensitivity ? 0 : 1);
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = turbo; //(byte)(device.Turbo ? 1 : 0);
        BbRequest.PhaseNo.Value = 0; //TODO:后续完善
        BbRequest.MeasurementsRequested.Value = 0x00000001;
        BbRequest.Band.FMin.Value = (uint)(device.StartFrequency * 1000000);
        BbRequest.Band.FMax.Value = (uint)(device.StopFrequency * 1000000);
        BbRequest.Mask.NumberOfBands.Value = 0;
        SendCmd(BbRequest.GetBytes(Version));
    }
}