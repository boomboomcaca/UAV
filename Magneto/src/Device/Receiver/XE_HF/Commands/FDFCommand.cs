using System;
using System.Text;
using Magneto.Device.XE_HF.Protocols;
using Magneto.Device.XE_HF.Protocols.Data;

namespace Magneto.Device.XE_HF.Commands;

/*单频测向实现方案：
 * 以测向模式为主引导所有参数走向
 * 连续测向模式下需要先停止当前测向才能设置参数
 */
internal class FdfCommand : CommandBase
{
    #region 成员变量

    //窄带参数设置
    private IqFluxRequest _nbRequest;

    //音频解调请求
    private AudioDemodulationRequest _audioRequest;

    //静噪开关及门限控制
    private SquelchActivationRequest _squelchRequest;

    //测向模式
    private XedFindMode _dfMode;

    //测向质量门限设置
    //private DFQualityThresholdConfig _dfQualityConfig;
    //删除通道指令
    private ManualChannelsDeletionRequest _delManualChannelsRequest;

    //手动通道指令（常规和导航模式使用）
    private ManualChannelsAddtionRequest _addManualChannelsRequest;

    //连续测向指令（连续测向模式使用）
    private ContinuousDfRequest _continuousDfRequest;

    //21版本Homing测向控制参数（通过LG319软件抓包发现21版本Homing模式下会发送此结构，但开发文档中没有任何地方有该结构的描述，此处不清楚字段用Unclear代替）
    private HomingRequest _homingRequest;

    #endregion

    #region CommandBase

    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        if ((XedFindMode)device.DFindMode == XedFindMode.Homing)
        {
            ProgramingInfo.NumOfChannels.Value = 2;
            ProgramingInfo.Channels = new ChannelProgramming[2];
        }
        else
        {
            ProgramingInfo.NumOfChannels.Value = 1;
            ProgramingInfo.Channels = new ChannelProgramming[1];
        }

        for (var i = 0; i < ProgramingInfo.Channels.Length; ++i)
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
            chan.FmFilter.Value = 2; //TODO:单频测向未暴露此参数
            chan.LevelUnits.Value = 1;
        }

        SendCmd(ProgramingInfo.GetBytes());
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        //TODO:由于边界问题暂不考虑常规模式下宽带频谱的输出,即单频测向下永不输出宽带频谱
        BbRequest.UdpFftwbPort.Value = 0;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = DefaultResolution;
        BbRequest.Sensitivity.Value = (byte)(device.Sensitivity ? 0 : 1);
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = 0; //TODO:后续待完善
        BbRequest.MeasurementsRequested.Value =
            (uint)((XedFindMode)device.DFindMode == XedFindMode.Normal ? 0x00000006 : 0); //TODO:
        BbRequest.Band.FMin.Value = GetBbFmin((uint)(device.Frequency * 1000000));
        BbRequest.Band.FMax.Value = GetBbFmax((uint)(device.Frequency * 1000000));
        //窄带参数
        _nbRequest = new IqFluxRequest();
        _nbRequest.ChannelNo.Value = 2;
        _nbRequest.HomingIdChannel.Value = 1;
        _nbRequest.HomingIdentify.Value =
            (uint)Identifier.FixDfHoming; //TODO:Tracker identifier used in the results(direction-finding)
        _nbRequest.HomingThreshold.Value = (short)(device.LevelThreshold - 107);
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
        track.Width.Value = device.DfBandWidth * 1000;
        track.Mode.Value = 0;
        track.Fft.Value = 1;
        track.FftPort.Value = (ushort)device.UdpNbfftPort;
        track.FftIntegration.Value = 1;
        //21版本特有，Homing模式请求
        _homingRequest = new HomingRequest();
        _homingRequest.Start.Value = 1;
        _homingRequest.Unclear.Value = 1; //抓包值
        _homingRequest.HomingThreshold.Value = (short)(device.LevelThreshold - 107);
        _homingRequest.Info.HomingIdChannel.Value = 1;
        _homingRequest.Info.HomingIdentifier.Value = (uint)Identifier.FixDfHoming;
        //音频请求
        _audioRequest = new AudioDemodulationRequest();
        _audioRequest.ChannelNo.Value = 2;
        _audioRequest.ChannelId.Value = 1;
        _audioRequest.ModulationType.Value = XeAssister.GetDemoduMode(device.DemMode);
        _audioRequest.Bfo.Value = 0;
        _audioRequest.Frequency.Value = 22050;
        _audioRequest.UdpPort.Value = (ushort)device.UdpAudioPort;
        _audioRequest.Action.Value = (sbyte)(device.AudioSwitch ? 1 : 0);
        _squelchRequest = new SquelchActivationRequest();
        _squelchRequest.ChannelNo.Value = 2;
        _squelchRequest.ChannelId.Value = 1;
        _squelchRequest.Threshold.Value = (short)(device.SquelchThreshold - 107);
        _squelchRequest.Activation.Value = (sbyte)(device.SquelchSwitch ? 1 : 0);
        //测向请求
        _dfMode = (XedFindMode)device.DFindMode;
        //_dfQualityConfig = new DFQualityThresholdConfig();
        //_dfQualityConfig.QualityMask.Value = XE_Assister.GetQualityMark(device.QualityThreshold);
        _delManualChannelsRequest = new ManualChannelsDeletionRequest();
        _delManualChannelsRequest.NumOfChannels.Value = 0;
        _addManualChannelsRequest = new ManualChannelsAddtionRequest();
        _addManualChannelsRequest.NumOfManualChannels.Value = 1;
        _addManualChannelsRequest.ManualChannels = new ManualChannel[1];
        var manualChan = _addManualChannelsRequest.ManualChannels[0] = new ManualChannel();
        manualChan.Identifier.Value = (uint)Identifier.FixDfNormal; //TODO:
        manualChan.CentreFrequency.Value = (uint)(device.Frequency * 1000000);
        manualChan.Bandwidth.Value = (uint)(device.DfBandWidth * 1000);
        manualChan.ThresholdType.Value = 0;
        manualChan.Threshold.Value = (short)(device.LevelThreshold - 107);
        _continuousDfRequest = new ContinuousDfRequest();
        _continuousDfRequest.Identifier.Value = (uint)Identifier.FixDfContinue; //TODO:
        _continuousDfRequest.CentreFrequency.Value = (uint)(device.Frequency * 1000000);
        _continuousDfRequest.Bandwidth.Value = (uint)(device.Sensitivity ? 292 : 25000);
        _continuousDfRequest.Threshold.Value = (short)(device.LevelThreshold - 107);
        _continuousDfRequest.UdpPort.Value = (ushort)device.UdpDfPort;
        _continuousDfRequest.Mode.Value = (byte)(device.Sensitivity ? 1 : 0);
        _continuousDfRequest.Start.Value = (sbyte)((XedFindMode)device.DFindMode == XedFindMode.Continue ? 1 : 0);
        //灵敏模式下发送完宽带参数需要延时一段时间才能发送其它参数
        var interval = device.Sensitivity ? 1500 : 0;
        if (_dfMode == XedFindMode.Homing)
        {
            //发送宽带参数
            SendCmd(BbRequest.GetBytes(Version), interval);
            //发送窄带参数
            SendCmd(_nbRequest.GetBytes(Version));
            //发送Homing
            SendCmd(_homingRequest.GetBytes(Version));
            //发送音频参数
            SendCmd(_audioRequest.GetBytes());
            //发送静噪门限参数
            SendCmd(_squelchRequest.GetBytes());
            //发送测向门限
            //SendCmd(_dfQualityConfig.GetBytes());
            //TODO:
        }
        else if (_dfMode == XedFindMode.Normal)
        {
            //发送宽带参数
            SendCmd(BbRequest.GetBytes(Version), interval);
            //发送手动测向通道
            SendCmd(_addManualChannelsRequest.GetBytes());
            //发送测向门限
            //SendCmd(_dfQualityConfig.GetBytes());
        }
        else //_dfMode == XEDFindMode.Continue
        {
            //发送宽带参数
            SendCmd(BbRequest.GetBytes(Version), interval);
            //发送连续测向指令
            SendCmd(_continuousDfRequest.GetBytes());
            //发送测向门限
            //SendCmd(_dfQualityConfig.GetBytes());
        }
    }

    public override void Stop()
    {
        if (_dfMode == XedFindMode.Homing)
            ResetHomingDfState(false);
        else if (_dfMode == XedFindMode.Continue)
            //如果是连续测向模式，需要先停止，否则会影响下一次的导航模式
            ResetContinueDfState(false);
        else if (_dfMode == XedFindMode.Normal)
            //如果是常规模式需要先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
        base.Stop();
    }

    public override void SetCenterFrequency(uint freq)
    {
        base.SetCenterFrequency(freq);
        if (_dfMode == XedFindMode.Homing)
        {
            //21版本需要先停止原来的Homing测向再重新打开
            ResetHomingDfState(false);
            //检查宽带参数是否需要重置
            CheckBroadBand(freq);
            //重新下发窄带参数
            var track = _nbRequest.Tracks[0];
            track.CentreFrequency.Value = freq;
            SendCmd(_nbRequest.GetBytes(Version));
            //重新下发Homing测向指令
            ResetHomingDfState(true);
            //如果之前请求了音频数据还需要重新下发音频请求
            if (_audioRequest.Action.Value == 1) SendCmd(_audioRequest.GetBytes());
        }
        else if (_dfMode == XedFindMode.Normal)
        {
            CheckBroadBand(freq);
            //先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //再添加新的通道
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.CentreFrequency.Value = freq;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else //_dfMode == XEDFindMode.Continue
        {
            //先停止原来的连续测向通道
            ResetContinueDfState(false);
            //检查宽带参数
            CheckBroadBand(freq);
            //再重新开启连续测向通道
            _continuousDfRequest.CentreFrequency.Value = freq;
            ResetContinueDfState(true);
        }
    }

    public override void SetBandwidth(uint bw)
    {
        base.SetBandwidth(bw);
        if (_dfMode == XedFindMode.Homing)
        {
            //TODO:
            var track = _nbRequest.Tracks[0];
            if (Math.Abs(bw - track.Width.Value) > 1e-9)
            {
                ResetHomingDfState(false);
                track.Width.Value = bw;
                SendCmd(_nbRequest.GetBytes(Version));
                ResetHomingDfState(true);
                //如果之前请求了音频数据还需要重新下发音频请求
                if (_audioRequest.Action.Value == 1) SendCmd(_audioRequest.GetBytes());
            }
        }
        else if (_dfMode == XedFindMode.Normal)
        {
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            if (manualChan.Bandwidth.Value != bw)
            {
                //先删除原来的通道
                SendCmd(_delManualChannelsRequest.GetBytes());
                //再添加新的通道
                manualChan.Bandwidth.Value = bw;
                SendCmd(_addManualChannelsRequest.GetBytes());
            }
        } //_dfMode == XEDFindMode.Continue
        //20180427:连续测向模式下带宽跟随灵敏度模式改变，此处不再需要设置参数，以防止出现灵敏度模式和带宽不匹配的情况
        //if (_continuousDFRequest.Bandwidth.Value != bw)
        //{
        //    //先停止原来的连续测向通道
        //    _continuousDFRequest.Start.Value = 0;
        //    SendCmd(_continuousDFRequest.GetBytes());
        //    //再重新开启连续测向通道
        //    _continuousDFRequest.Bandwidth.Value = bw;
        //    _continuousDFRequest.Start.Value = 1;
        //    SendCmd(_continuousDFRequest.GetBytes());
        //}
    }

    public override void SetDfMode(XedFindMode currMode)
    {
        Device.DFindMode = (int)currMode;
        //1. Homing --> Normal or Continue
        if (_dfMode == XedFindMode.Homing && currMode != XedFindMode.Homing)
        {
            //21版本停止Homing模式
            ResetHomingDfState(false);
            //停止当前通道所有工作
            StopProc(2, 0);
            //再重新设置所有参数
            ResetProgrammingInfo(1);
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = (uint)(currMode == XedFindMode.Normal ? 0x00000006 : 0);
            SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
            var track = _nbRequest.Tracks[0];
            if (currMode == XedFindMode.Normal)
            {
                //添加现在的通道参数
                var manualChan = _addManualChannelsRequest.ManualChannels[0];
                manualChan.CentreFrequency.Value = track.CentreFrequency.Value;
                manualChan.Bandwidth.Value = (uint)track.Width.Value;
                manualChan.Threshold.Value = _nbRequest.HomingThreshold.Value;
                SendCmd(_addManualChannelsRequest.GetBytes());
            }
            else if (currMode == XedFindMode.Continue)
            {
                _continuousDfRequest.CentreFrequency.Value = track.CentreFrequency.Value;
                _continuousDfRequest.Bandwidth.Value = (uint)(BbRequest.Sensitivity.Value == 0 ? 292 : 25000);
                _continuousDfRequest.Threshold.Value = _nbRequest.HomingThreshold.Value;
                _continuousDfRequest.Mode.Value = (byte)(BbRequest.Sensitivity.Value == 0 ? 1 : 0);
                _continuousDfRequest.Start.Value = 1;
                SendCmd(_continuousDfRequest.GetBytes());
            }
        }
        else if (_dfMode != XedFindMode.Homing && currMode == XedFindMode.Homing)
        {
            //2. Normal or Continue --> Homing 
            if (_dfMode == XedFindMode.Continue)
                //如果是连续测向模式，需要先停止
                ResetContinueDfState(false);
            else if (_dfMode == XedFindMode.Normal)
                //如果是常规模式需要先删除原来的通道
                SendCmd(_delManualChannelsRequest.GetBytes());
            //停止当前通道所有工作
            StopProc(-1, 0);
            //再重新设置所有参数
            ResetProgrammingInfo(2);
            //重新下发宽带参数,归航模式下灵敏度模式只能为快速，0：灵敏，1：快速
            BbRequest.MeasurementsRequested.Value = 0;
            BbRequest.Sensitivity.Value = 1;
            SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
            //重新设置窄带参数等
            var track = _nbRequest.Tracks[0];
            if (_dfMode == XedFindMode.Normal)
            {
                var manualChan = _addManualChannelsRequest.ManualChannels[0];
                track.CentreFrequency.Value = manualChan.CentreFrequency.Value;
                track.Width.Value = manualChan.Bandwidth.Value > 1000000 ? 150000 : manualChan.Bandwidth.Value;
                _nbRequest.HomingThreshold.Value = manualChan.Threshold.Value;
            }
            else if (_dfMode == XedFindMode.Continue)
            {
                track.CentreFrequency.Value = _continuousDfRequest.CentreFrequency.Value;
                track.Width.Value = 150000;
                _nbRequest.HomingThreshold.Value = _continuousDfRequest.Threshold.Value;
            }

            _homingRequest.Start.Value = 1;
            _homingRequest.HomingThreshold.Value = _nbRequest.HomingThreshold.Value;
            //System.Threading.Thread.Sleep(50);
            SendCmd(_nbRequest.GetBytes(Version));
            SendCmd(_homingRequest.GetBytes(Version));
            SendCmd(_audioRequest.GetBytes());
            SendCmd(_squelchRequest.GetBytes());
        }
        else if (_dfMode == XedFindMode.Normal && currMode == XedFindMode.Continue)
        {
            SendCmd(_delManualChannelsRequest.GetBytes());
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = 0;
            SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            _continuousDfRequest.CentreFrequency.Value = manualChan.CentreFrequency.Value;
            _continuousDfRequest.Bandwidth.Value = (uint)(BbRequest.Sensitivity.Value == 0 ? 292 : 25000);
            _continuousDfRequest.Threshold.Value = manualChan.Threshold.Value;
            _continuousDfRequest.Mode.Value = (byte)(BbRequest.Sensitivity.Value == 0 ? 1 : 0);
            _continuousDfRequest.Start.Value = 1;
            SendCmd(_continuousDfRequest.GetBytes());
        }
        else if (_dfMode == XedFindMode.Continue && currMode == XedFindMode.Normal)
        {
            ResetContinueDfState(false);
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = 0x00000006;
            SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.CentreFrequency.Value = _continuousDfRequest.CentreFrequency.Value;
            manualChan.Bandwidth.Value = 150000;
            manualChan.Threshold.Value = _continuousDfRequest.Threshold.Value;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }

        //保存当前测向模式
        _dfMode = currMode;
    }

    //public override void SetAgcControll(bool agc, int ifAtt, int rfAtt)
    //{
    //    base.SetAgcControll(agc, ifAtt, rfAtt);
    //    //如果原来是连续测向模式需要先停止
    //    if (_dfMode == XEDFindMode.Continue)
    //    {
    //        ResetContinueDFState(false);
    //    }
    //    //设置AGC
    //    SendCmd(_programingInfo.GetBytes());
    //    if (_dfMode == XEDFindMode.Continue)
    //    {
    //        ResetContinueDFState(true);
    //    }
    //}
    public override void SetAttenuation(int att)
    {
        Device.Attenuation = att;
        if (ProgramingInfo?.Channels == null) return;
        //自动切换到手动或者手动切换到自动需要重新下发宽带参数
        var bAgcChange = (ProgramingInfo.Channels[0].AgcType.Value == 2 && att != -1) ||
                         (ProgramingInfo.Channels[0].AgcType.Value == 1 && att == -1);
        base.SetAttenuation(att);
        if (_dfMode == XedFindMode.Continue)
        {
            //如果原来是连续测向模式需要先停止
            ResetContinueDfState(false);
        }
        else if (_dfMode == XedFindMode.Homing)
        {
            //停止Homing模式
            _nbRequest.HomingIdChannel.Value = 0;
            SendCmd(_nbRequest.GetBytes(Version));
            ResetHomingDfState(false);
        }

        //设置AGC
        SendCmd(ProgramingInfo.GetBytes());
        if (bAgcChange) SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
        if (_dfMode == XedFindMode.Continue)
        {
            //SendCmd(_bbRequest.GetBytes(Version));
            ResetContinueDfState(true);
        }
        else if (_dfMode == XedFindMode.Homing)
        {
            //重新打开Homing模式
            _nbRequest.HomingIdChannel.Value = 1;
            SendCmd(_nbRequest.GetBytes(Version));
            ResetHomingDfState(true);
            if (_audioRequest.Action.Value == 1) SendCmd(_audioRequest.GetBytes());
        }
    }

    public override void SetLevelThreshold(int levelThreshold)
    {
        base.SetLevelThreshold(levelThreshold);
        if (_dfMode == XedFindMode.Homing)
        {
            ResetHomingDfState(false);
            _nbRequest.HomingThreshold.Value = (short)levelThreshold;
            SendCmd(_nbRequest.GetBytes(Version));
            _homingRequest.HomingThreshold.Value = (short)levelThreshold;
            ResetHomingDfState(true);
            //如果之前请求了音频数据还需要重新下发音频请求
            if (_audioRequest.Action.Value == 1) SendCmd(_audioRequest.GetBytes());
        }
        else if (_dfMode == XedFindMode.Normal)
        {
            //先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //再添加新的通道
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.Threshold.Value = (short)levelThreshold;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else //_dfMode == XEDFindMode.Continue
        {
            //先停止原来的连续测向通道
            ResetContinueDfState(false);
            //再重新开启连续测向通道
            _continuousDfRequest.Threshold.Value = (short)levelThreshold;
            ResetContinueDfState(true);
        }
    }

    //public override void SetQualityThreshold(ushort qualityMark)
    //{
    //    _dfQualityConfig.QualityMask.Value = qualityMark;
    //    //TODO:超短波有效
    //    SendCmd(_dfQualityConfig.GetBytes());
    //}
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

    public override void SetAmpli(bool onoff)
    {
        base.SetAmpli(onoff);
        //如果是连续测向模式需要先停止再设置参数
        if (_dfMode == XedFindMode.Continue) ResetContinueDfState(false);
        SendCmd(ProgramingInfo.GetBytes());
        if (_dfMode == XedFindMode.Continue) ResetContinueDfState(true);
    }

    public override void SetSensitivity(bool onoff)
    {
        base.SetSensitivity(onoff);
        //Homing模式下只能为快速，跟随测向模式改变，此处不需要再设置
        if (_dfMode == XedFindMode.Homing) return;
        var sensitivity = (byte)(onoff ? 0 : 1);
        if (BbRequest.Sensitivity.Value == sensitivity) return;
        base.SetSensitivity(onoff);
        if (_dfMode == XedFindMode.Continue) ResetContinueDfState(false);
        SendCmd(BbRequest.GetBytes(Version));
        if (_dfMode == XedFindMode.Continue)
        {
            _continuousDfRequest.Bandwidth.Value = (uint)(onoff ? 292 : 25000);
            _continuousDfRequest.Mode.Value = (byte)(onoff ? 1 : 0);
            ResetContinueDfState(true);
        }
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

    public override void SetAudioSwitch(bool onoff)
    {
        base.SetAudioSwitch(onoff);
        _audioRequest.Action.Value = (sbyte)(onoff ? 1 : 0);
        SendCmd(_audioRequest.GetBytes());
    }

    #endregion

    #region 其它辅助函数

    /// <summary>
    ///     检查中心频率是否在宽带范围内，如果不在则先调整宽带参数
    /// </summary>
    /// <param name="freq"></param>
    private void CheckBroadBand(uint freq)
    {
        //if (freq < _bbRequest.Band.FMin.Value || freq > _bbRequest.Band.FMax.Value)
        //{
        //    _bbRequest.Band.FMin.Value = GetBBFmin(freq);
        //    _bbRequest.Band.FMax.Value = GetBBFmax(freq);
        //    SendCmd(_bbRequest.GetBytes(Version));
        //}
        BbRequest.Band.FMin.Value = GetBbFmin(freq);
        BbRequest.Band.FMax.Value = GetBbFmax(freq);
        SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
    }

    /// <summary>
    ///     重新设置通道参数，主要为个数改变时，即宽带和窄带切换时
    /// </summary>
    /// <param name="numOfChans"></param>
    private void ResetProgrammingInfo(int numOfChans)
    {
        if (numOfChans == ProgramingInfo.NumOfChannels.Value) return;
        var oldChan = ProgramingInfo.Channels[0];
        var chans = new ChannelProgramming[numOfChans];
        for (var i = 0; i < numOfChans; ++i)
        {
            chans[i] = new ChannelProgramming();
            var tempChan = chans[i];
            tempChan.ChannelNo.Value = (byte)(i + 1);
            tempChan.FMin.Value = oldChan.FMin.Value;
            tempChan.FMax.Value = oldChan.FMax.Value;
            tempChan.AgcType.Value = oldChan.AgcType.Value;
            tempChan.RfAttenuator.Value = oldChan.RfAttenuator.Value;
            tempChan.IfAttenuator.Value = oldChan.IfAttenuator.Value;
            tempChan.AmpliConfig.Value = oldChan.AmpliConfig.Value;
            Array.Copy(oldChan.Antenna.Value, tempChan.Antenna.Value, oldChan.Antenna.Value.Length);
            tempChan.FmFilter.Value = oldChan.FmFilter.Value;
            tempChan.LevelUnits.Value = oldChan.LevelUnits.Value;
        }

        ProgramingInfo.NumOfChannels.Value = (byte)numOfChans;
        ProgramingInfo.Channels = chans;
        SendCmd(ProgramingInfo.GetBytes());
    }

    private void ResetContinueDfState(bool onoff)
    {
        _continuousDfRequest.Start.Value = (sbyte)(onoff ? 1 : 0);
        //TODO:若果时连续测向灵敏模式下停止需要发送参数后延时一段时间再发送其它参数，否则可能极大概率参数未生效
        SendCmd(_continuousDfRequest.GetBytes());
    }

    private void ResetHomingDfState(bool onoff)
    {
        _homingRequest.Start.Value = (byte)(onoff ? 1 : 0);
        SendCmd(_homingRequest.GetBytes(Version));
    }

    ///// <summary>
    ///// 匹配最佳宽带频谱点数
    ///// </summary>
    ///// <param name="bw">当前带宽，单位Hz</param>
    ///// <returns>分辨率，单位Hz</returns>
    //private double GetResolution(uint bw)
    //{
    //    double resolution = 25000;
    //    switch (bw)
    //    {
    //        case 40000000: resolution = 100000; break;
    //        case 20000000: resolution = 50000; break;
    //        case 10000000: resolution = 25000; break;
    //        case 5000000: resolution = 12500; break;
    //        case 2000000: resolution = 6250; break;
    //        default: break;
    //    }
    //    return resolution;
    //}

    #endregion
}