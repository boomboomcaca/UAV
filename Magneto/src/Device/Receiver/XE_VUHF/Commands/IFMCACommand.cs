using System;
using System.Collections.Generic;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Device.XE_VUHF.Protocols.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF.Commands;

internal class IfmcaCommand : CommandBase
{
    #region 成员变量

    //窄带参数设置
    private IqFluxRequest _nbRequest;

    // 极小值，用于浮点数比较大小
    private readonly double _epsilon = 1.0E-7d;

    //上一次的子通道信息
    private IfMultiChannelTemplate[] _preChannels;

    //每个子通道对应的窄带信息
    private IqFluxTrack[] _arrIqFluxTrack;

    //每个子通道对应的音频信息
    private AudioDemodulationRequest[] _arrAudioRequst;

    //静噪门限信息
    private SquelchActivationRequest _squelchRequest;

    #endregion

    #region CommandBase

    protected override void InitCommand(DeviceParams device)
    {
        InitMembers(ref _arrIqFluxTrack, ref _arrAudioRequst, device);
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
        //宽带参数设置，带宽固定为40M
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort;
        BbRequest.Resolution.Value = DefaultResolution; //TODO:中频多路中不暴露频谱分辨率
        BbRequest.Sensitivity.Value = (byte)(device.Sensitivity ? 0 : 1); //TODO:
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = 0; //TODO:后续完善
        BbRequest.MeasurementsRequested.Value = 0x00000001;
        BbRequest.Band.FMin.Value = GetBbFmin((uint)(device.Frequency * 1000000));
        BbRequest.Band.FMax.Value = GetBbFmax((uint)(device.Frequency * 1000000));
        SendCmd(BbRequest.GetBytes(Version), device.Sensitivity ? 1500 : 0);
        //解析多路窄带信息
        if (device.IfMultiChannels.Length > 0)
        {
            _preChannels = ParseMultiChannelInfo(device.IfMultiChannels);
            //多路窄带参数设置
            _nbRequest = new IqFluxRequest();
            _nbRequest.ChannelNo.Value = 2;
            _nbRequest.HomingIdChannel.Value = 0;
            _nbRequest.NumOfTracks.Value = (byte)_preChannels.Length;
            _nbRequest.Tracks = new IqFluxTrack[_preChannels.Length];
            for (var i = 0; i < _preChannels.Length; ++i)
            {
                //_nbRequest.Tracks[i] = new IQFluxTrack();
                //var track = _nbRequest.Tracks[i];
                //var info = _preMultiChannels[i];
                //track.ChannelNo.Value = (byte)(i + 1);
                //track.Modification.Value = 1;
                //track.Activation.Value = (sbyte)(info.IFSwitch ? 1 : 0);
                //track.IQPort.Value = 0;
                //track.Type.Value = 1;
                //track.CentreFrequency.Value = (uint)(info.Frequency * 1000000);
                //track.Width.Value = info.IFBandWidth * 1000;
                //track.Mode.Value = 0;
                //track.FFT.Value = 1;
                //track.FFTPort.Value = (ushort)device._udpNBFFTPort;
                //track.FFTIntegration.Value = 1;//Number of FFTs to be included(1 by default)
                var info = _preChannels[i];
                _arrIqFluxTrack[i].Activation.Value = (sbyte)(i == 0 ? 1 : info.IfSwitch ? 1 : 0);
                _arrIqFluxTrack[i].CentreFrequency.Value = (uint)(info.Frequency * 1000000);
                _arrIqFluxTrack[i].Width.Value = info.FilterBandwidth * 1000;
                _nbRequest.Tracks[i] = _arrIqFluxTrack[i];
            }

            SendCmd(_nbRequest.GetBytes(Version));
            //先发送窄带再发对应的音频请求
            for (var i = 0; i < _preChannels.Length; ++i)
            {
                var info = _preChannels[i];
                if (info.IfSwitch && info.AudioSwitch)
                {
                    var audioRequest = _arrAudioRequst[i];
                    audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(info.DemMode);
                    audioRequest.Action.Value = 1;
                    SendCmd(audioRequest.GetBytes());
                }
            }

            SendCmd(_squelchRequest.GetBytes()); //实测会影响几个通道，暂不暴露不开启
        }
    }

    //TODO:如果原来已打开的子通道不在当前中心频率-带宽的范围内是怎么处理？
    public override void SetCenterFrequency(uint freq)
    {
        base.SetCenterFrequency(freq);
        BbRequest.Band.FMin.Value = GetBbFmin(freq);
        BbRequest.Band.FMax.Value = GetBbFmax(freq);
        SendCmd(BbRequest.GetBytes(Version));
    }

    //public override void SetAgcControll(bool agc, int ifAtt, int rfAtt)
    //{
    //    base.SetAgcControll(agc, ifAtt, rfAtt);
    //    SendCmd(_programingInfo.GetBytes());
    //}
    public override void SetAttenuation(int att)
    {
        base.SetAttenuation(att);
        SendCmd(ProgramingInfo.GetBytes());
    }

    public override void SetAmpli(bool onoff)
    {
        base.SetAmpli(onoff);
        SendCmd(ProgramingInfo.GetBytes());
    }

    public override void SetSensitivity(bool onoff)
    {
        //TODO:确定宽带信息修改后是否需要重新下发窄带参数
        base.SetSensitivity(onoff);
        SendCmd(BbRequest.GetBytes(Version));
    }

    //客户端保证：设置到服务端的通道的数量只会修改或者增加不会减少，客户端某个通道的的关闭对应的是IFSwitch开关
    public override void SetIfMultiChannels(Dictionary<string, object>[] channels)
    {
        base.SetIfMultiChannels(channels);
        var currChannels = ParseMultiChannelInfo(channels);
        if (currChannels == null || currChannels.Length == 0) return;
        //表示是否只是音频参数改变
        var onlyAudio = false;
        var isFirstIfSwitch = false;
        if (!IsAdd(currChannels))
        {
            var index = -1;
            var param = GetChangedItem(currChannels, ref index);
            if (param != null && index != -1)
            {
                var currChannel = currChannels[index];
                if (param is ParameterNames.AudioSwitch or ParameterNames.DemMode)
                {
                    _arrAudioRequst[index].Action.Value = (sbyte)(currChannel.AudioSwitch ? 1 : 0);
                    _arrAudioRequst[index].ModulationType.Value = XeAssister.GetDemoduMode(currChannel.DemMode);
                    SendCmd(_arrAudioRequst[index].GetBytes());
                    onlyAudio = true;
                }
                else if (param == ParameterNames.IfSwitch)
                {
                    if (currChannel.IfSwitch == false)
                    {
                        //关闭音频
                        _arrAudioRequst[index].Action.Value = 0;
                        SendCmd(_arrAudioRequst[index].GetBytes());
                    }

                    if (index == 0) isFirstIfSwitch = true;
                }
            }
        }

        if (!onlyAudio)
        {
            if (_preChannels == null || _preChannels.Length != currChannels.Length)
            {
                _nbRequest = new IqFluxRequest();
                _nbRequest.ChannelNo.Value = 2;
                _nbRequest.HomingIdChannel.Value = 0;
                _nbRequest.NumOfTracks.Value = (byte)currChannels.Length;
                _nbRequest.Tracks = new IqFluxTrack[currChannels.Length];
            }

            for (var i = 0; i < currChannels.Length; ++i)
            {
                var currChannel = currChannels[i];
                var track = _arrIqFluxTrack[i];
                track.Activation.Value = (sbyte)(i == 0 ? 1 : currChannel.IfSwitch ? 1 : 0);
                track.CentreFrequency.Value = (uint)(currChannel.Frequency * 1000000);
                track.Width.Value = currChannel.FilterBandwidth * 1000;
                _nbRequest.Tracks[i] = track;
            }

            if (!isFirstIfSwitch)
                //发送窄带通道信息
                SendCmd(_nbRequest.GetBytes(Version));
            //发送音频信息
            for (var i = 0; i < currChannels.Length; ++i)
                if (currChannels[i].AudioSwitch && currChannels[i].IfSwitch)
                {
                    var audioRequest = _arrAudioRequst[i];
                    audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(currChannels[i].DemMode);
                    audioRequest.Action.Value = 1;
                    SendCmd(audioRequest.GetBytes());
                }
        }

        //保存最新的通道参数信息
        _preChannels = currChannels;
    }

    #endregion

    #region 其它辅助函数

    private static IfMultiChannelTemplate[] ParseMultiChannelInfo(Dictionary<string, object>[] parameters)
    {
        if (parameters.Length == 0) return null;
        var infos = new IfMultiChannelTemplate[parameters.Length];
        for (var i = 0; i < parameters.Length; ++i)
            infos[i] = new IfMultiChannelTemplate
            {
                Frequency = (double)parameters[i][ParameterNames.Frequency],
                FilterBandwidth = (double)parameters[i][ParameterNames.IfBandwidth],
                DemMode = (Modulation)parameters[i][ParameterNames.DemMode],
                AudioSwitch = (bool)parameters[i][ParameterNames.AudioSwitch],
                IfSwitch = (bool)parameters[i][ParameterNames.IfSwitch]
            };

        return infos;
    }

    private void InitMembers(ref IqFluxTrack[] arrIqFluxTrack, ref AudioDemodulationRequest[] arrAudioRequest,
        DeviceParams device)
    {
        arrIqFluxTrack = new IqFluxTrack[device.MaxChanCount];
        arrAudioRequest = new AudioDemodulationRequest[device.MaxChanCount];
        for (var i = 0; i < device.MaxChanCount; ++i)
        {
            //IQFluxTrack
            arrIqFluxTrack[i] = new IqFluxTrack();
            var track = arrIqFluxTrack[i];
            track.ChannelNo.Value = (byte)(i + 1);
            track.Modification.Value = 1;
            track.Activation.Value = 1; //(sbyte)(info.IFSwitch ? 1 : 0);
            track.IqPort.Value = 0;
            track.Type.Value = 1;
            track.CentreFrequency.Value = 0; //(uint)(info.Frequency * 1000000);
            track.Width.Value = 0; //info.IFBandWidth * 1000;
            track.Mode.Value = 0;
            track.Fft.Value = 1;
            track.FftPort.Value = (ushort)device.UdpNbfftPort;
            track.FftIntegration.Value = 1; //Number of FFTs to be included(1 by default)
            //AudioDemodulationRequest
            arrAudioRequest[i] = new AudioDemodulationRequest();
            var audioRequest = arrAudioRequest[i];
            audioRequest.ChannelNo.Value = 2;
            audioRequest.ChannelId.Value = (byte)(i + 1);
            audioRequest.ModulationType.Value = 0; //XE_Assister.GetDemoduMode(info.DemMode);
            audioRequest.Bfo.Value = 0;
            audioRequest.Frequency.Value = 22050; //TODO:44100
            audioRequest.UdpPort.Value = (ushort)device.UdpAudioPort;
            audioRequest.Action.Value = 1; //(sbyte)(info.AudioSwitch ? 1 : 0);
        }

        //中频多路未暴露静噪门限参数，可直接将静噪门限关闭
        _squelchRequest = new SquelchActivationRequest();
        _squelchRequest.ChannelNo.Value = 2;
        _squelchRequest.ChannelId.Value = 1;
        _squelchRequest.Threshold.Value = -174;
        _squelchRequest.Activation.Value = 0;
    }

    /// <summary>
    ///     子通道是否新增(客户端参数遵循只增不减的原则，即使关闭某个子通道也只是IFSwitch开关置为false)
    /// </summary>
    /// <param name="currChannels"></param>
    /// <returns></returns>
    private bool IsAdd(IfMultiChannelTemplate[] currChannels)
    {
        return _preChannels == null || _preChannels.Length < currChannels.Length;
    }

    /// <summary>
    ///     寻找变化的参数项，若不是新增才会调用此函数
    /// </summary>
    /// <param name="currChannels"></param>
    /// <param name="index">改变的子通道</param>
    /// <returns>改变的参数名</returns>
    private string GetChangedItem(IfMultiChannelTemplate[] currChannels, ref int index)
    {
        for (var i = 0; i < _preChannels.Length; ++i)
        {
            var preChannel = _preChannels[i];
            var currChannel = currChannels[i];
            if (currChannel.IfSwitch != preChannel.IfSwitch)
            {
                index = i;
                return ParameterNames.IfSwitch;
            }

            if (Math.Abs(currChannel.Frequency - preChannel.Frequency) > _epsilon)
            {
                index = i;
                return ParameterNames.Frequency;
            }

            if (Math.Abs(currChannel.FilterBandwidth - preChannel.FilterBandwidth) > _epsilon)
            {
                index = i;
                return ParameterNames.IfBandwidth;
            }

            if (currChannel.DemMode != preChannel.DemMode)
            {
                index = i;
                return ParameterNames.DemMode;
            }

            if (currChannel.AudioSwitch != preChannel.AudioSwitch)
            {
                index = i;
                return ParameterNames.AudioSwitch;
            }
        }

        index = -1;
        return null;
    }

    #endregion
}