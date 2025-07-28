using System;
using System.Collections.Generic;
using System.Text;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;

namespace Magneto.Device.XE_VUHF_28.Commands;

/*
 * 中频多路实现方案：
 * 保持第一路窄带始终为打开状态，否则所有数据都会异常
 */
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
        var centerFrequencyHz = (uint)(device.Frequency * 1000000);
        var bwHz = (uint)(device.IfBandWidth * 1000);
        var rbwHz = DefaultResolution;
        GetBbInfo(centerFrequencyHz, bwHz, true, ref rbwHz, out var minFreq, out var maxFreq);
        //宽带参数设置，带宽固定为40M
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = (ushort)device.UdpBbfftPort;
        BbRequest.Resolution.Value = rbwHz;
        BbRequest.Sensitivity.Value = 1;
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = PhaseNo; //TODO:后续完善
        BbRequest.MeasurementsRequested.Value = 0x00000001;
        BbRequest.Band.FMin.Value = minFreq;
        BbRequest.Band.FMax.Value = maxFreq;
        SendCmd(BbRequest.GetBytes());
        //解析多路窄带信息
        _preChannels = ParseMultiChannelInfo(device.DdcChannels);
        if (_preChannels.Length > 0)
        {
            //多路窄带参数设置
            _nbRequest = new IqFluxRequest();
            _nbRequest.ChannelNo.Value = 2;
            _nbRequest.HomingIdChannel.Value = 0;
            _nbRequest.NumOfTracks.Value = (byte)_preChannels.Length;
            _nbRequest.Tracks = new IqFluxTrack[_preChannels.Length];
            for (var i = 0; i < _preChannels.Length; ++i)
            {
                var info = _preChannels[i];
                _arrIqFluxTrack[i].Activation.Value = 1; //(sbyte)(i == 0 ? 1 : (info.IFSwitch ? 1 : 0));
                _arrIqFluxTrack[i].CentreFrequency.Value =
                    GetChannelCentreFrequency((uint)(info.Frequency * 1000000)); //(uint)(info.Frequency * 1000000);
                _arrIqFluxTrack[i].Width.Value = info.FilterBandwidth * 1000;
                _nbRequest.Tracks[i] = _arrIqFluxTrack[i];
            }

            SendCmd(_nbRequest.GetBytes());
            //先发送窄带再发对应的音频请求
            for (var i = 0; i < _preChannels.Length; ++i)
            {
                var info = _preChannels[i];
                if (info.IfSwitch && info.AudioSwitch)
                {
                    var audioRequest = _arrAudioRequst[i];
                    audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(info.DemMode);
                    audioRequest.Action.Value = 1;
                    audioRequest.Frequency.Value = 44100;
                    SendCmd(audioRequest.GetBytes());
                }
            }

            SendCmd(_squelchRequest.GetBytes()); //实测会影响几个通道，暂不暴露不开启
        }
    }

    //客户端保证：在任务启动后设置到服务端的通道的数量只会修改或者增加不会减少，客户端某个通道的的关闭对应的是IFSwitch开关
    public override void SetIfMultiChannels(Dictionary<string, object>[] channels)
    {
        base.SetIfMultiChannels(channels);
        //解析多路窄带信息
        var currChannels = ParseMultiChannelInfo(channels);
        var isChanged = IsChannelsChanged(currChannels);
        if (!isChanged) return;
        //多路窄带参数设置
        _nbRequest = new IqFluxRequest();
        _nbRequest.ChannelNo.Value = 2;
        _nbRequest.HomingIdChannel.Value = 0;
        _nbRequest.NumOfTracks.Value = (byte)currChannels.Length;
        _nbRequest.Tracks = new IqFluxTrack[currChannels.Length];
        for (var i = 0; i < currChannels.Length; ++i)
        {
            var info = currChannels[i];
            //_arrIQFluxTrack[i].Activation.Value = (sbyte)(i == 0 ? 1 : (info.IFSwitch ? 1 : 0));
            //新版版XE中如果将打开的子通道关闭Activation=0会导致设备断连，所以针对设备一直设置为打开，只是返回客户端时根据通道的IFSwitch开关进行数据过滤
            _arrIqFluxTrack[i].Activation.Value = 1; //(sbyte)(i == 0 ? 1 : (info.IFSwitch ? 1 : 0));
            _arrIqFluxTrack[i].CentreFrequency.Value =
                GetChannelCentreFrequency((uint)(info.Frequency * 1000000)); //(uint)(info.Frequency * 1000000);
            _arrIqFluxTrack[i].Width.Value = info.FilterBandwidth * 1000;
            _nbRequest.Tracks[i] = _arrIqFluxTrack[i];
        }

        SendCmd(_nbRequest.GetBytes());
        //先发送窄带再发对应的音频请求
        for (var i = 0; i < currChannels.Length; ++i)
        {
            var info = currChannels[i];
            var audioRequest = _arrAudioRequst[i];
            audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(info.DemMode);
            audioRequest.Action.Value = (sbyte)(info.IfSwitch && info.AudioSwitch ? 1 : 0);
            SendCmd(audioRequest.GetBytes());
        }

        //实测会影响几个通道，暂不暴露不开启
        SendCmd(_squelchRequest.GetBytes());
        //保存最新的通道参数信息
        _preChannels = currChannels;
    }

    #endregion

    #region 其它辅助函数

    private static IfMultiChannelTemplate[] ParseMultiChannelInfo(Dictionary<string, object>[] parameters)
    {
        if (parameters == null) return Array.Empty<IfMultiChannelTemplate>();
        var infos = new List<IfMultiChannelTemplate>();
        foreach (var item in parameters)
        {
            var channel = (IfMultiChannelTemplate)item;
            infos.Add(channel);
        }

        return infos.ToArray();
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
            audioRequest.Frequency.Value = 44100; //TODO:44100
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
    ///     因为子通道参数类型为Dictionary
    ///     &lt;string, object&gt;
    ///     []无法在框架层通过简单的比较判断出参数是否改变来过滤出未改变的情况，
    ///     每次在有参数修改时都会有通道参数同时设置下来造成频繁的给设备发送指令，此处增加判断过滤
    /// </summary>
    /// <param name="currChannels"></param>
    /// <returns></returns>
    private bool IsChannelsChanged(IfMultiChannelTemplate[] currChannels)
    {
        var isChanged = true;
        if (_preChannels.Length == currChannels.Length)
        {
            for (var i = 0; i < _preChannels.Length; ++i)
            {
                var preChannel = _preChannels[i];
                var currChannel = currChannels[i];
                if (currChannel.IfSwitch != preChannel.IfSwitch
                    || Math.Abs(currChannel.Frequency - preChannel.Frequency) > _epsilon
                    || Math.Abs(currChannel.FilterBandwidth - preChannel.FilterBandwidth) > _epsilon
                    || currChannel.DemMode != preChannel.DemMode
                    || currChannel.AudioSwitch != preChannel.AudioSwitch)
                    return true;
            }

            isChanged = false;
        }

        return isChanged;
    }

    /// <summary>
    ///     由于新版XE的子通道Activation参数无法正常使用即子通道不能关闭，但在切换宽带中心频率后可能会存在部分甚至全部子通道都不在最新的宽带中心频率-带宽范围内，
    ///     为了保证设备能正常运行，此处将不在范围内的中心频率统一设置成最新范围内的最小值，由于这类子通道的IFSwitch开关客户端已置为false,所以这部分通道的数据也不会再发回客户端
    /// </summary>
    /// <param name="frequency">子通道中心频率，单位Hz</param>
    /// <returns></returns>
    private uint GetChannelCentreFrequency(uint frequency)
    {
        var result = frequency;
        if (frequency < BbRequest.Band.FMin.Value || frequency > BbRequest.Band.FMax.Value)
            result = BbRequest.Band.FMin.Value;
        return result;
    }

    #endregion
}