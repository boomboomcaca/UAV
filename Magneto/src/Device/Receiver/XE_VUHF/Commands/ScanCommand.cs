using System;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols.Data;

namespace Magneto.Device.XE_VUHF.Commands;

/*
 * 频段扫描实现方案：
 * 1.运行时不可修改参数
 * 2.将快速截收和灵敏度模式合并为1个参数扫描模式，按速度快慢分为以下三种
 *   快速：快速截收，25k，快速模式
 *   常规：步进可调，快速模式
 *   灵敏：25K及以下步进可调，灵敏模式
 * modified by linxia: 20181109
 * 频段扫描只保留常规模式，删除原快速和灵敏对应代码
 */
internal class ScanCommand : CommandBase
{
    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = (byte)(device.Chan == 1 ? 1 : 2);
        ProgramingInfo.Channels = new ChannelProgramming[ProgramingInfo.NumOfChannels.Value];
        for (var i = 0; i < ProgramingInfo.Channels.Length; ++i)
        {
            var chan = ProgramingInfo.Channels[i] = new ChannelProgramming();
            chan.ChannelNo.Value = (byte)(i + 1);
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
            chan.FmFilter.Value = (byte)(device.FmFilter ? 1 : 2);
            chan.LevelUnits.Value = 1;
        }

        SendCmd(ProgramingInfo.GetBytes());
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = (byte)device.Chan;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = device.StepFrequency * 1000;
        BbRequest.Sensitivity.Value = 1; //灵敏度模式只使用快速
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = 0;
        BbRequest.MeasurementsRequested.Value = 0x00000001;
        BbRequest.Band.FMin.Value = (uint)(device.StartFrequency * 1000000);
        BbRequest.Band.FMax.Value = (uint)(device.StopFrequency * 1000000);
        BbRequest.Mask.NumberOfBands.Value = 0;
        SendCmd(BbRequest.GetBytes(Version));
        /*TODO: 在未接测向天线的情况下，测试中发现在使用过单频测量或者单频测向功能后，第一次启动全频段扫描且放大器为打开状态时，扫描数据明显异常，停止任务再重新启动恢复正常，
         * 理论上这两次操作没有任何分别，下发的参数也一致，且在每次任务停止时都发送了停止所有通道工作的指令的，此处尝试重新发送一次通道设置指令，现象消失 ==！
         * */
        SendCmd(ProgramingInfo.GetBytes());
    }
}