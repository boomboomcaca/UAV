using System;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Device.XE_VUHF.Protocols.Data;

namespace Magneto.Device.XE_VUHF.Commands;

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

    #endregion

    #region CommandBase

    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 2;
        ProgramingInfo.Channels = new ChannelProgramming[2];
        for (var i = 0; i < 2; ++i)
        {
            ProgramingInfo.Channels[i] = new ChannelProgramming();
            var chan = ProgramingInfo.Channels[i];
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
        }

        SendCmd(ProgramingInfo.GetBytes());
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = 0; //(ushort)device._udpBBFFTPort;//TODO:暂时不返回宽带频谱
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value =
            DefaultResolution; //device.Resolution * 1000;//TODO:影响宽带FFT,但单频测量中不需要宽带FFT结果所以固定为25k
        BbRequest.Sensitivity.Value = 1; //(byte)(device.Sensitivity ? 0 : 1);//TODO:暂不暴露
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = 0; //TODO:后续完善
        BbRequest.MeasurementsRequested.Value = 0; //TODO:单频测量暂不需要宽带数据
        BbRequest.Band.FMin.Value = GetBbFmin((uint)(device.Frequency * 1000000));
        BbRequest.Band.FMax.Value = GetBbFmax((uint)(device.Frequency * 1000000));
        SendCmd(BbRequest.GetBytes(Version));
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
        track.Width.Value = device.IfBandWidth * 1000;
        track.Mode.Value = 0;
        track.Fft.Value = 1;
        track.FftPort.Value = (ushort)device.UdpNbfftPort;
        track.FftIntegration.Value = 1; //Number of FFTs to be included(1 by default)
        SendCmd(_nbRequest.GetBytes(Version));
        //音频请求
        _audioRequest = new AudioDemodulationRequest();
        _audioRequest.ChannelNo.Value = 2;
        _audioRequest.ChannelId.Value = 1;
        _audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(device.DemMode);
        _audioRequest.Bfo.Value = 0;
        _audioRequest.Frequency.Value = 44100; //TODO:44100
        _audioRequest.UdpPort.Value = (ushort)device.UdpAudioPort;
        _audioRequest.Action.Value = (sbyte)(device.AudioSwitch ? 1 : 0);
        SendCmd(_audioRequest.GetBytes());
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
        SendCmd(_ituRequest.GetBytes());
    }

    public override void SetCenterFrequency(uint freq)
    {
        base.SetCenterFrequency(freq);
        _nbRequest.Tracks[0].CentreFrequency.Value = freq;
        //先停止ITU
        StopProc(2, 0x00000010);
        //如果不在宽带范围内则先设置宽带参数
        if (freq < BbRequest.Band.FMin.Value || freq > BbRequest.Band.FMax.Value)
        {
            BbRequest.Band.FMin.Value = GetBbFmin(freq);
            BbRequest.Band.FMax.Value = GetBbFmax(freq);
            SendCmd(BbRequest.GetBytes(Version));
        }

        //再发送窄带信息
        SendCmd(_nbRequest.GetBytes(Version));
        //打开ITU测量获取电平值
        SendCmd(_ituRequest.GetBytes());
    }

    public override void SetBandwidth(uint bw)
    {
        base.SetBandwidth(bw);
        if (bw > 1000000) throw new Exception($"单频测量中该参数无效, bw = {bw.ToString()} Hz.");
        if (_nbRequest is { Tracks: not null })
        {
            _nbRequest.Tracks[0].Width.Value = bw;
            //需要先停止ITU测量
            StopProc(2, 0x00000010);
            //设置带宽
            SendCmd(_nbRequest.GetBytes(Version));
            //重新打开ITU测量
            SendCmd(_ituRequest.GetBytes());
            //如果当前请求了音频数据则需要重新下发音频请求
            if (_audioRequest.Action.Value == 1) SendCmd(_audioRequest.GetBytes());
        }
    }

    public override void SetAttenuation(int att)
    {
        Device.Attenuation = att;
        if (ProgramingInfo?.Channels == null) return;
        //自动切换到手动或者手动切换到自动需要重新下发宽带参数
        var bAgcChange = (ProgramingInfo.Channels[0].AgcType.Value == 2 && att != -1) ||
                         (ProgramingInfo.Channels[0].AgcType.Value == 1 && att == -1);
        base.SetAttenuation(att);
        //需要先停止ITU测量
        StopProc(2, 0x00000010);
        //设置AGC
        SendCmd(ProgramingInfo.GetBytes());
        if (bAgcChange) SendCmd(BbRequest.GetBytes(Version));
        //重新打开ITU测量
        SendCmd(_ituRequest.GetBytes());
    }

    public override void SetAmpli(bool onoff)
    {
        base.SetAmpli(onoff);
        //需要先停止ITU测量
        StopProc(2, 0x00000010);
        //设置放大器开关
        SendCmd(ProgramingInfo.GetBytes());
        //重新打开ITU测量
        SendCmd(_ituRequest.GetBytes());
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
        base.SetXdBAndBeta(xdb, beta);
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

    public override void SetDetectionMode(ushort mode)
    {
        base.SetDetectionMode(mode);
        SendCmd(BbRequest.GetBytes(Version));
    }

    public override void SetIntegrationTime(ushort time)
    {
        base.SetIntegrationTime(time);
        SendCmd(BbRequest.GetBytes(Version));
    }

    #endregion
}