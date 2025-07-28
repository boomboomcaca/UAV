using System;
using System.Text;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF_28.Commands;

/*单频测向实现方案：
 * 以测向模式为主引导所有参数走向
 * 连续测向模式下需要先停止当前测向才能设置参数
 * 注意事项：
 * 1.灵敏模式下发送完宽带参数需要延时一段时间才能发送其它参数
 * modified by linxia: 20181029
 * 测向模式修改为 常规、快速、灵敏三种，原归航模式去掉
 * 常规：原常规 + 灵敏, dfbw <= 1M，resolution取3.125k, span取1M，否则resolution取25k,span取实际带宽
 * 快速：原连续测向 + 快速
 * 灵敏：原连续测向 + 灵敏
 */
internal class FdfCommand : CommandBase
{
    #region 成员变量

    //测向模式
    private DFindMode _dfMode;

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

    //连续测向_灵敏即现在的灵敏模式下，测向带宽固定为1.25MHz，连续测向_快速即现在的快速模式下，测向带宽固定为100kHz
    private const uint Bw100K = 100000;
    private const uint Bw1250K = 1250000;

    #endregion

    #region CommandBase

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
        var centerFrequencyHz = (uint)(device.Frequency * 1000000);
        var bwHz = (uint)(device.DfBandWidth * 1000);
        var rbwHz = device.ResolutionBandwidth * 1000d;
        GetBbInfo(centerFrequencyHz, bwHz, false, ref rbwHz, out var minFreq, out var maxFreq);
        //宽带参数设置
        BbRequest = new BbInterceptionRequest();
        BbRequest.ChannelNo.Value = 1;
        BbRequest.UdpFftwbPort.Value = 0;
        BbRequest.DetectionMode.Value = device.DetectionMode;
        BbRequest.IntTime.Value = (ushort)device.XeIntTime;
        BbRequest.Resolution.Value = rbwHz;
        BbRequest.Sensitivity.Value = 0; //常规下只使用灵敏，连续测向的灵敏快速不在此处设置
        BbRequest.RelativeThreshold.Value = 0;
        BbRequest.ThresholdMinValue.Value = 20;
        BbRequest.ThresholdMaxValue.Value = 20;
        BbRequest.Turbo.Value = 0;
        BbRequest.PhaseNo.Value = PhaseNo;
        BbRequest.MeasurementsRequested.Value = 0x00000007;
        BbRequest.Band.FMin.Value = minFreq;
        BbRequest.Band.FMax.Value = maxFreq;
        //测向请求
        _dfMode = device.DFindMode;
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
        _continuousDfRequest.Bandwidth.Value = device.DFindMode == DFindMode.Feebleness ? Bw1250K : Bw100K;
        _continuousDfRequest.Threshold.Value = (short)(device.LevelThreshold - 107);
        _continuousDfRequest.UdpPort.Value = (ushort)device.UdpDfPort;
        _continuousDfRequest.Mode.Value = (byte)(device.DFindMode == DFindMode.Feebleness ? 1 : 0);
        _continuousDfRequest.Start.Value = (sbyte)(device.DFindMode != DFindMode.Normal ? 1 : 0);
        //发送宽带参数，灵敏模式下发送完宽带参数需要延时一段时间才能发送其它参数，否则其它参数极有可能未生效
        SendCmd(BbRequest.GetBytes(), 1500);
        if (_dfMode == DFindMode.Normal)
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
        if (_dfMode == DFindMode.Normal)
            //如果是常规模式需要先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
        else
            //如果是连续测向模式，需要先停止
            ResetContinueDfState(false);
        base.Stop();
    }

    public override void SetDfMode(DFindMode currMode)
    {
        base.SetDfMode(currMode);
        if (_dfMode == DFindMode.Normal && currMode != DFindMode.Normal)
        {
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            _continuousDfRequest.CentreFrequency.Value = manualChan.CentreFrequency.Value;
            _continuousDfRequest.Bandwidth.Value = currMode == DFindMode.Feebleness ? Bw1250K : Bw100K;
            _continuousDfRequest.Threshold.Value = manualChan.Threshold.Value;
            _continuousDfRequest.Mode.Value = (byte)(currMode == DFindMode.Feebleness ? 1 : 0);
            _continuousDfRequest.Start.Value = 1;
            //删除手动通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //重新下发宽带参数
            //_bbRequest.MeasurementsRequested.Value = 0;
            //SendCmd(_bbRequest.GetBytes(), 1500);
            //CheckBroadBandByBW(manualChan.CentreFrequency.Value, manualChan.Bandwidth.Value, _continuousDFRequest.Bandwidth.Value);
            //下发连续测向指令
            SendCmd(_continuousDfRequest.GetBytes());
        }
        else if (_dfMode != DFindMode.Normal && currMode == DFindMode.Normal)
        {
            ResetContinueDfState(false);
            //重新下发宽带参数
            BbRequest.MeasurementsRequested.Value = 0x00000006;
            SendCmd(BbRequest.GetBytes(), 1500);
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            var newDfbw = _dfMode == DFindMode.Feebleness ? 150000 : Bw100K;
            CheckBroadBandByBw(_continuousDfRequest.CentreFrequency.Value, manualChan.Bandwidth.Value, newDfbw,
                Device.ResolutionBandwidth * 1e3);
            //var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.CentreFrequency.Value = _continuousDfRequest.CentreFrequency.Value;
            manualChan.Bandwidth.Value = newDfbw;
            manualChan.Threshold.Value = _continuousDfRequest.Threshold.Value;
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else if (_dfMode == DFindMode.Gate && currMode == DFindMode.Feebleness)
        {
            ResetContinueDfState(false);
            //TODO: 是否重新下发宽带参数？？
            _continuousDfRequest.Bandwidth.Value = Bw1250K;
            _continuousDfRequest.Mode.Value = 1;
            ResetContinueDfState(true);
        }
        else if (_dfMode == DFindMode.Feebleness && currMode == DFindMode.Gate)
        {
            ResetContinueDfState(false);
            //TODO: 是否重新下发宽带参数？？
            _continuousDfRequest.Bandwidth.Value = Bw100K;
            _continuousDfRequest.Mode.Value = 0;
            ResetContinueDfState(true);
        }

        //连续测向_灵敏模式下质量门限无效，从该模式切换到其它模式需要重新下发质量门限参数，否则质量门限无效
        if (_dfMode == DFindMode.Feebleness) SendCmd(_dfQualityConfig.GetBytes());
        //保存当前测向模式
        _dfMode = currMode;
    }

    public override void SetLevelThreshold(int levelThreshold)
    {
        base.SetLevelThreshold(levelThreshold);
        if (_dfMode == DFindMode.Normal)
        {
            //先删除原来的通道
            SendCmd(_delManualChannelsRequest.GetBytes());
            //再添加新的通道
            var manualChan = _addManualChannelsRequest.ManualChannels[0];
            manualChan.Threshold.Value = (short)(levelThreshold - 107);
            SendCmd(_addManualChannelsRequest.GetBytes());
        }
        else //连续测向模式
        {
            //先停止原来的连续测向通道
            ResetContinueDfState(false);
            //再重新开启连续测向通道
            _continuousDfRequest.Threshold.Value = (short)(levelThreshold - 107);
            ResetContinueDfState(true);
        }
    }

    public override void SetQualityThreshold(int quality)
    {
        base.SetQualityThreshold(quality);
        _dfQualityConfig.QualityMask.Value = XeAssister.GetQualityMark(quality);
        SendCmd(_dfQualityConfig.GetBytes());
    }

    public override void SetResolution(double resolution)
    {
        if (Device != null) Device.ResolutionBandwidth = resolution;
        var rbwHz = resolution * 1000d;
        if (BbRequest != null) BbRequest.Resolution.Value = rbwHz;
        SendCmd(BbRequest?.GetBytes());
    }

    #endregion

    #region 其它辅助函数

    private void CheckBroadBandByFreq(uint newFreq, double dfbw, double rbw)
    {
        SetBbFminFmax(ref BbRequest.Band.FMin.Value, ref BbRequest.Band.FMax.Value, newFreq, dfbw, rbw);
        BbRequest.Resolution.Value = rbw;
        SendCmd(BbRequest.GetBytes(), 1500);
    }

    private void CheckBroadBandByBw(uint freq, double dfbw, double newDfbw, double rbw)
    {
        //带宽大于1M时宽带范围以带宽来设置，小于等于1M时设置1M
        if (newDfbw > Bw1M || (dfbw <= Bw1M) ^ (newDfbw <= Bw1M))
        {
            BbRequest.Resolution.Value = newDfbw <= Bw1M ? Resolution3125 : Resolution25000;
            SetBbFminFmax(ref BbRequest.Band.FMin.Value, ref BbRequest.Band.FMax.Value, freq, newDfbw, rbw);
            BbRequest.Resolution.Value = rbw;
            SendCmd(BbRequest.GetBytes(), 1500);
        }
    }

    private void ResetContinueDfState(bool onoff)
    {
        _continuousDfRequest.Start.Value = (sbyte)(onoff ? 1 : 0);
        SendCmd(_continuousDfRequest.GetBytes(), 500);
    }

    /// <summary>
    ///     设置宽带的最小最大频率,所有参数单位Hz
    /// </summary>
    /// <param name="fmin"></param>
    /// <param name="fmax"></param>
    /// <param name="centerFreq"></param>
    /// <param name="dfbw"></param>
    /// <param name="resolution"></param>
    private void SetBbFminFmax(ref uint fmin, ref uint fmax, uint centerFreq, double dfbw, double resolution)
    {
        var span = (uint)dfbw;
        var minFreq = centerFreq - span / 2d;
        minFreq -= minFreq % resolution;
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

    #endregion
}