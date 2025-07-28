using System;
using System.Collections.Generic;
using System.Text;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;

namespace Magneto.Device.XE_VUHF_28.Commands;

/*单频测量实现方案:
 * 由于设备不返回电平数据，因此不管有没有打开ITU开关都进行ITU测量，并使用ITU测量结果中的平均电平作为该频点的电平值
 */
internal class FfmCommand : CommandBase
{
    #region 成员变量

    //窄带参数设置
    private IqFluxRequest _nbRequest;

    //ITU测量设置
    private ItuRequest _ituRequest;

    //音频解调请求
    private AudioDemodulationRequest _audioRequest;

    //静噪开关及门限控制
    private SquelchActivationRequest _squelchRequest;
    private IqSamplesRequest _iqSamplesRequest;

    #endregion

    #region CommandBase

    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 2;
        ProgramingInfo.Channels = new ChannelProgramming[2];
        var channels = new List<ChannelProgramming>();
        for (var i = 0; i < 2; ++i)
        {
            var chan = new ChannelProgramming();
            chan.ChannelNo.Value = (byte)(i + 1);
            chan.FMin.Value = 0;
            chan.FMax.Value = 0;
            if (device.Attenuation == -1)
            {
                //AGC
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
            chan.FmFilter.Value = 2; //TODO:单频测量暂未暴露此参数
            chan.LevelUnits.Value = 1;
            ProgramingInfo.Channels[i] = chan;
            channels.Add(chan);
        }

        channels.Reverse();
        SendCmd(ProgramingInfo.GetBytes());
        var centerFrequencyHz = (uint)(device.Frequency * 1000000);
        var bwHz = (uint)(device.IfBandWidth * 1000);
        var rbwHz = DefaultResolution;
        GetBbInfo(centerFrequencyHz, bwHz, true, ref rbwHz, out var minFreq, out var maxFreq);
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort; //TODO:暂时不返回宽带频谱
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = rbwHz; //device.Resolution * 1000;//TODO:影响宽带FFT,但单频测量中不需要宽带FFT结果所以固定为25k
        BbRequest.Sensitivity.Value = 1; //0：灵敏，1：快速
        BbRequest.RelativeThreshold.Value = 1;
        BbRequest.ThresholdMinValue.Value = 0x96;
        BbRequest.ThresholdMaxValue.Value = 0x96;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = PhaseNo; //TODO:后续完善
        BbRequest.MeasurementsRequested.Value = 0x0000000D;
        BbRequest.Band.FMin.Value = minFreq;
        BbRequest.Band.FMax.Value = maxFreq;
        SendCmd(BbRequest.GetBytes());
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 2;
        ProgramingInfo.Channels = channels.ToArray();
        SendCmd(ProgramingInfo.GetBytes());
        //窄带参数设置
        _nbRequest = new IqFluxRequest();
        _nbRequest.ChannelNo.Value = 2;
        _nbRequest.HomingIdChannel.Value = 0;
        _nbRequest.NumOfTracks.Value = 1;
        _nbRequest.Tracks = new IqFluxTrack[1];
        _nbRequest.Tracks[0] = new IqFluxTrack();
        var track = _nbRequest.Tracks[0];
        track.ChannelNo.Value = 1;
        track.Modification.Value = 1;
        track.Activation.Value = 1;
        track.IqPort.Value = 0;
        track.Type.Value = 1;
        track.CentreFrequency.Value = (uint)(device.Frequency * 1000000);
        track.Width.Value = device.FilterBandWidth * 1000;
        track.Mode.Value = 0;
        track.Fft.Value = 1;
        track.FftPort.Value = (ushort)device.UdpNbfftPort;
        track.FftResolution.Value = DefaultResolution;
        track.FftIntegration.Value = 1; //Number of FFTs to be included(1 by default)
        SendCmd(_nbRequest.GetBytes());
        //音频请求
        _audioRequest = new AudioDemodulationRequest();
        _audioRequest.ChannelNo.Value = 2;
        _audioRequest.ChannelId.Value = 1;
        _audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(device.DemMode);
        _audioRequest.Bfo.Value = 0;
        _audioRequest.Frequency.Value = 44100;
        _audioRequest.UdpPort.Value = (ushort)device.UdpAudioPort;
        _audioRequest.Action.Value = (sbyte)(device.AudioSwitch ? 1 : 0);
        if (device.AudioSwitch) SendCmd(_audioRequest.GetBytes(), 100);
        _squelchRequest = new SquelchActivationRequest();
        _squelchRequest.ChannelNo.Value = 2;
        _squelchRequest.ChannelId.Value = 1;
        _squelchRequest.Threshold.Value = (short)(device.SquelchThreshold - 107);
        _squelchRequest.Activation.Value = (sbyte)(device.SquelchSwitch ? 1 : 0);
        SendCmd(_squelchRequest.GetBytes());
        //ITU测量请求
        _ituRequest = new ItuRequest();
        _ituRequest.ChannelNo.Value = 2;
        _ituRequest.ChannelId.Value = 1;
        _ituRequest.UdpPort.Value = (ushort)device.UdpNbituPort;
        _ituRequest.AntennaChargeTime.Value = 0.001;
        _ituRequest.AntennaDischargeTime.Value = 0.6;
        _ituRequest.FieldChargeTime.Value = 0.001;
        _ituRequest.FieldDischargeTime.Value = 0.6;
        _ituRequest.XdB1Threshold.Value = 6;
        _ituRequest.XdB2Threshold.Value = device.XdB;
        _ituRequest.BetaBandThreshold.Value = device.Beta;
        _ituRequest.GammaVqmam.Value = 1.424214; //TODO:抓包数据
        _ituRequest.GammaVqmfm.Value = 1.424214;
        _ituRequest.GammaVqmpm.Value = 1.424214;
        _ituRequest.NumOfFftPoints.Value = 2048;
        _ituRequest.FftWindow.Value = 5;
        _ituRequest.Mode.Value = 1;
        _ituRequest.NumOfLoops.Value = 2;
        _ituRequest.NumOfIntegrations.Value = 1;
        _ituRequest.TypeOfIntegration.Value = 0;
        _ituRequest.AcquisitionTime.Value = 0.2; //TODO:
        if (!device.IqSwitch && device.ItuSwitch) SendCmd(_ituRequest.GetBytes());
        //IQ请求
        _iqSamplesRequest = new IqSamplesRequest();
        _iqSamplesRequest.ChannelNo.Value = 2;
        _iqSamplesRequest.ChannelId.Value = 1;
        _iqSamplesRequest.Starting.Value = 1;
        _iqSamplesRequest.Number.Value = 0;
        if (device.IqSwitch) SendCmd(_iqSamplesRequest.GetBytes());
    }

    public override void SetIfBandwidth(double bw)
    {
        base.SetIfBandwidth(bw);
        StopProc(-1, 0);
        InitCmd(Device);
    }

    public override void SetFilterBandwidth(double bw)
    {
        base.SetFilterBandwidth(bw);
        StopProc(-1, 0);
        InitCmd(Device);
    }

    public override void SetAttenuation(int att)
    {
        if (Device != null) Device.Attenuation = att;
        if (ProgramingInfo?.Channels == null) return;
        //自动切换到手动或者手动切换到自动需要重新下发宽带参数
        var bAgcChange = (ProgramingInfo.Channels[0].AgcType.Value == 2 && att != -1) ||
                         (ProgramingInfo.Channels[0].AgcType.Value == 1 && att == -1);
        base.SetAttenuation(att);
        if (bAgcChange)
        {
            StopProc(-1, 0);
            InitCmd(Device);
        }
        else
        {
            //需要先停止ITU测量
            StopProc(2, 0x00000010);
            //设置AGC
            SendCmd(ProgramingInfo.GetBytes());
            //重新打开ITU测量
            SendCmd(_ituRequest.GetBytes());
        }
    }

    public override void SetDemodulation(uint dem)
    {
        base.SetDemodulation(dem);
        _audioRequest.ModulationType.Value = dem;
        SendCmd(_audioRequest.GetBytes());
    }

    public override void SetSquelchThreshold(int squelchThreshold)
    {
        base.SetSquelchThreshold(squelchThreshold);
        _squelchRequest.Threshold.Value = (short)squelchThreshold;
        if (_squelchRequest.Activation.Value == 1) SendCmd(_squelchRequest.GetBytes());
    }

    public override void SetSquelchSwitch(bool onoff)
    {
        base.SetSquelchSwitch(onoff);
        _squelchRequest.Activation.Value = (sbyte)(onoff ? 1 : 0);
        SendCmd(_squelchRequest.GetBytes());
    }

    public override void SetXdBAndBeta(float xdb, float beta)
    {
        base.SetXdBAndBeta((short)xdb, beta);
        _ituRequest.XdB2Threshold.Value = xdb;
        _ituRequest.BetaBandThreshold.Value = beta;
        //先停止原来的测量
        StopProc(2, 0x00000010);
        //再重新设置ITU测量参数
        SendCmd(_ituRequest.GetBytes());
    }

    public override void SetAudioSwitch(bool onoff)
    {
        base.SetAudioSwitch(onoff);
        _audioRequest.Action.Value = (sbyte)(onoff ? 1 : 0);
        SendCmd(_audioRequest.GetBytes());
    }

    public override void SetItuSwitch(bool onoff)
    {
        StopProc(2, 0);
        base.SetItuSwitch(onoff);
        if (!Device.IqSwitch && onoff) SendCmd(_ituRequest.GetBytes());
    }

    public override void SetIqSwitch(bool onoff)
    {
        StopProc(-1, 0);
        base.SetIqSwitch(onoff);
        InitCmd(Device);
    }

    #endregion
}