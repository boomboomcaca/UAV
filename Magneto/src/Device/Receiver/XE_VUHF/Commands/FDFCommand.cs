using System;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Device.XE_VUHF.Protocols.Data;

namespace Magneto.Device.XE_VUHF.Commands;

internal class FdfCommand : CommandBase
{
    protected override void InitCommand(DeviceParams device)
    {
        //通道参数设置
        ProgramingInfo = new ReceiverProgramingInfo();
        ProgramingInfo.NumOfChannels.Value = 1;
        ProgramingInfo.Channels = new ChannelProgramming[1];
        var chan = ProgramingInfo.Channels[0] = new ChannelProgramming();
        chan.ChannelNo.Value = 1;
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
        SendCmd(ProgramingInfo.GetBytes());
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = 0;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = device.DfBandWidth * 1000 > Bw1M ? Resolution25000 : Resolution3125;
        BbRequest.Sensitivity.Value = 0; //常规下只使用灵敏，连续测向的灵敏快速不在此处设置
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = 0;
        BbRequest.MeasurementsRequested.Value =
            (uint)((XedFindMode)device.DFindMode == XedFindMode.Normal ? 0x00000006 : 0); //TODO:
        SetBbFminFmax(ref BbRequest.Band.FMin.Value, ref BbRequest.Band.FMax.Value,
            (uint)(device.Frequency * 1000000), device.DfBandWidth * 1000);
        //测向请求
        _dfMode = (XedFindMode)device.DFindMode;
        _dfQualityConfig = new DfQualityThresholdConfig();
        _dfQualityConfig.QualityMask.Value = XeAssister.GetQualityMark(device.QualityThreshold);
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
        _continuousDfRequest.Bandwidth.Value =
            (XedFindMode)device.DFindMode == XedFindMode.Sensitivity ? Bw2343 : Bw75000;
        _continuousDfRequest.Threshold.Value = (short)(device.LevelThreshold - 107);
        _continuousDfRequest.UdpPort.Value = (ushort)device.UdpDfPort;
        _continuousDfRequest.Mode.Value = (byte)((XedFindMode)device.DFindMode == XedFindMode.Sensitivity ? 1 : 0);
        _continuousDfRequest.Start.Value = (sbyte)((XedFindMode)device.DFindMode != XedFindMode.Normal ? 1 : 0);
        //发送宽带参数，灵敏模式下发送完宽带参数需要延时一段时间才能发送其它参数，否则其它参数极有可能未生效
        SendCmd(BbRequest.GetBytes(Version), 1500);
        if (_dfMode == XedFindMode.Normal)
        {
            //发送手动测向通道
            SendCmd(_addManualChannelsRequest.GetBytes());
            //发送测向门限
            SendCmd(_dfQualityConfig.GetBytes());
        }
        else //连续测向
        {
            //发送连续测向指令
            SendCmd(_continuousDfRequest.GetBytes());
            //发送测向门限
            SendCmd(_dfQualityConfig.GetBytes());
        }
    }

    public override void Stop()
    {
        if (_dfMode == XedFindMode.Normal)
            //如果是常规模式需要先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
        else
            //如果是连续测向模式，需要先停止
            ResetContinueDfState(false);
        base.Stop();
    }

    public override void SetCenterFrequency(uint freq)
    {
        base.SetCenterFrequency(freq);
        if (_dfMode == XedFindMode.Normal)
        {
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            //先检查宽带参数
            CheckBroadBandByFreq(freq, manualChan.Bandwidth.Value);
            //删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //再添加新的通道
            manualChan.CentreFrequency.Value = freq;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else //_dfMode == XEDFindMode.Continue
        {
            //先停止原来的连续测向通道
            ResetContinueDfState(false);
            //检查宽带参数
            CheckBroadBandByFreq(freq, _continuousDfRequest.Bandwidth.Value);
            //再重新开启连续测向通道
            _continuousDfRequest.CentreFrequency.Value = freq;
            ResetContinueDfState(true);
        }
    }

    public override void SetDfBandwidth(uint bw)
    {
        base.SetDfBandwidth(bw);
        if (_dfMode == XedFindMode.Normal && bw != Bw2343 && bw != Bw75000)
        {
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            if (manualChan.Bandwidth.Value != bw)
            {
                //先检查宽带参数
                CheckBroadBandByBw(manualChan.CentreFrequency.Value, manualChan.Bandwidth.Value, bw);
                //先删除原来的通道
                SendCmd(_delManualChannelsRequest.GetBytes());
                //再添加新的通道
                manualChan.Bandwidth.Value = bw;
                SendCmd(_addManualChannelsRequest.GetBytes());
            }
        }
        //_dfMode == XEDFindMode.Continue
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
        base.SetDfMode(currMode);
        if (_dfMode == XedFindMode.Normal && currMode != XedFindMode.Normal)
        {
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            _continuousDfRequest.CentreFrequency.Value = manualChan.CentreFrequency.Value;
            _continuousDfRequest.Bandwidth.Value = (uint)(currMode == XedFindMode.Sensitivity ? 2343 : 75000);
            _continuousDfRequest.Threshold.Value = manualChan.Threshold.Value;
            _continuousDfRequest.Mode.Value = (byte)(currMode == XedFindMode.Sensitivity ? 1 : 0);
            _continuousDfRequest.Start.Value = 1;
            //删除手动通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = 0;
            SendCmd(BbRequest.GetBytes(Version), 1500);
            CheckBroadBandByBw(manualChan.CentreFrequency.Value, manualChan.Bandwidth.Value,
                _continuousDfRequest.Bandwidth.Value);
            //下发连续测向指令
            SendCmd(_continuousDfRequest.GetBytes());
        }
        else if (_dfMode != XedFindMode.Normal && currMode == XedFindMode.Normal)
        {
            ResetContinueDfState(false);
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = 0x00000006;
            SendCmd(BbRequest.GetBytes(Version), 1500);
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.CentreFrequency.Value = _continuousDfRequest.CentreFrequency.Value;
            manualChan.Bandwidth.Value = 150000;
            manualChan.Threshold.Value = _continuousDfRequest.Threshold.Value;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else if (_dfMode == XedFindMode.Fast && currMode == XedFindMode.Sensitivity)
        {
            ResetContinueDfState(false);
            //TODO: 是否重新下发宽带参数？？
            _continuousDfRequest.Bandwidth.Value = 2343;
            _continuousDfRequest.Mode.Value = 1;
            ResetContinueDfState(true);
        }
        else if (_dfMode == XedFindMode.Sensitivity && currMode == XedFindMode.Fast)
        {
            ResetContinueDfState(false);
            //TODO: 是否重新下发宽带参数？？
            _continuousDfRequest.Bandwidth.Value = 75000;
            _continuousDfRequest.Mode.Value = 0;
            ResetContinueDfState(true);
        }

        //连续测向_灵敏模式下质量门限无效，从该模式切换到其它模式需要重新下发质量门限参数，否则质量门限无效
        if (_dfMode == XedFindMode.Sensitivity) SendCmd(_dfQualityConfig.GetBytes());
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
        if (_dfMode != XedFindMode.Normal)
            //如果原来是连续测向模式需要先停止
            ResetContinueDfState(false);
        //设置AGC
        SendCmd(ProgramingInfo.GetBytes());
        if (bAgcChange) SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
        if (_dfMode != XedFindMode.Normal)
            //SendCmd(_bbRequest.GetBytes(Version));
            ResetContinueDfState(true);
    }

    public override void SetLevelThreshold(int levelThreshold)
    {
        base.SetLevelThreshold(levelThreshold);
        if (_dfMode == XedFindMode.Normal)
        {
            //先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //再添加新的通道
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.Threshold.Value = (short)levelThreshold;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else //连续测向模式
        {
            //先停止原来的连续测向通道
            ResetContinueDfState(false);
            //再重新开启连续测向通道
            _continuousDfRequest.Threshold.Value = (short)levelThreshold;
            ResetContinueDfState(true);
        }
    }

    public override void SetQualityThreshold(ushort qualityMark)
    {
        base.SetQualityThreshold(qualityMark);
        _dfQualityConfig.QualityMask.Value = qualityMark;
        SendCmd(_dfQualityConfig.GetBytes());
    }

    public override void SetAmpli(bool onoff)
    {
        base.SetAmpli(onoff);
        //如果是连续测向模式需要先停止再设置参数
        if (_dfMode != XedFindMode.Normal) ResetContinueDfState(false);
        SendCmd(ProgramingInfo.GetBytes());
        if (_dfMode != XedFindMode.Normal) ResetContinueDfState(true);
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

    private void CheckBroadBandByFreq(uint newFreq, double dfbw)
    {
        if (newFreq < BbRequest.Band.FMin.Value || newFreq > BbRequest.Band.FMax.Value)
        {
            SetBbFminFmax(ref BbRequest.Band.FMin.Value, ref BbRequest.Band.FMax.Value, newFreq, dfbw);
            SendCmd(BbRequest.GetBytes(Version), 1500);
        }
    }

    private void CheckBroadBandByBw(uint freq, double dfbw, double newDfbw)
    {
        //带宽大于1M时宽带范围以带宽来设置，小于等于1M时设置1M
        if (newDfbw > Bw1M || (dfbw <= Bw1M) ^ (newDfbw <= Bw1M))
        {
            BbRequest.Resolution.Value = newDfbw <= Bw1M ? Resolution3125 : Resolution25000;
            SetBbFminFmax(ref BbRequest.Band.FMin.Value, ref BbRequest.Band.FMax.Value, freq, newDfbw);
            SendCmd(BbRequest.GetBytes(Version), 1500);
        }
    }

    /// <summary>
    ///     连续测向模式（现在的快速和灵敏模式）下使用，检查中心频率是否在宽带范围内，如果不在则先调整宽带参数
    /// </summary>
    /// <param name="freq"></param>
    private void CheckBroadBand(uint freq)
    {
        if (freq < BbRequest.Band.FMin.Value || freq > BbRequest.Band.FMax.Value)
        {
            BbRequest.Band.FMin.Value = GetBbFmin(freq);
            BbRequest.Band.FMax.Value = GetBbFmax(freq);
            SendCmd(BbRequest.GetBytes(Version), BbRequest.Sensitivity.Value == 0 ? 1500 : 0);
        }
    }

    private void ResetContinueDfState(bool onoff)
    {
        _continuousDfRequest.Start.Value = (sbyte)(onoff ? 1 : 0);
        SendCmd(_continuousDfRequest.GetBytes());
    }

    /// <summary>
    ///     设置宽带的最小最大频率,所有参数单位Hz
    /// </summary>
    /// <param name="fmin"></param>
    /// <param name="fmax"></param>
    /// <param name="centerFreq"></param>
    /// <param name="dfbw"></param>
    private void SetBbFminFmax(ref uint fmin, ref uint fmax, uint centerFreq, double dfbw)
    {
        var resolution = Resolution3125;
        var span = Bw1M;
        if (dfbw > Bw1M)
        {
            resolution = Resolution25000;
            span = (uint)dfbw;
        }

        var minFreq = centerFreq - span / 2d;
        minFreq = minFreq - minFreq % resolution;
        if (minFreq < Fmin) minFreq = Fmin;
        var maxFreq = minFreq + span;
        if (maxFreq > Fmax)
        {
            maxFreq = Fmax;
            minFreq = Fmax - span;
        }

        fmin = (uint)minFreq;
        fmax = (uint)maxFreq;
    }

    #region 成员变量

    //测向模式
    private XedFindMode _dfMode;

    //测向质量门限设置
    private DfQualityThresholdConfig _dfQualityConfig;

    //删除通道指令
    private ManualChannelsDeletionRequest _delManualChannelsRequest;

    //手动通道指令（常规模式使用）
    private ManualChannelsAddtionRequest _addManualChannelsRequest;

    //连续测向指令（连续测向模式使用）
    private ContinuousDfRequest _continuousDfRequest;

    //常规测向模式下，测向带宽 <= 1MHz 时宽带范围取1MHz，分辨率使用3.125kHz；当测向带宽 > 1MHz 时宽带范围取带宽，分辨率使用25kHz
    private const uint Bw1M = 1000000;
    private const double Resolution3125 = 3125;

    private const double Resolution25000 = 25000;

    //连续测向_灵敏即现在的灵敏模式下，测向带宽固定为2.343kHz，连续测向_快速即现在的快速模式下，测向带宽固定为75kHz
    private const uint Bw75000 = 75000;
    private const uint Bw2343 = 2343;

    #endregion
}