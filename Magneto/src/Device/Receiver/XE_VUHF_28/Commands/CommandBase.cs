using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magneto.Device.XE_VUHF_28.Protocols;
using Magneto.Device.XE_VUHF_28.Protocols.Data;
using Magneto.Protocol.Define;

namespace Magneto.Device.XE_VUHF_28.Commands;

internal enum Identifier
{
    FixFqBb = 0,
    FixFqNb = 1,
    ScanBb = 2,
    FixDfHoming = 3,
    FixDfNormal = 4,
    FixDfContinue = 5,
    WbdfBb = 6
}

internal delegate void SendCmdDelegate(byte[] sendBuffer, int interval = 0);

internal abstract class CommandBase
{
    #region 成员变量

    //编程通道设置
    protected ReceiverProgramingInfo ProgramingInfo;

    //宽带参数设置
    protected BbInterceptionRequest BbRequest;

    //数据协议版本
    protected uint Version = 28;

    //接收机支持最小频率,单位Hz
    protected const uint Fmin = 20000000;

    //接收机支持最大频率,单位Hz
    protected const uint Fmax = 3000000000;

    //最大中频带宽,单位Hz
    protected const uint IfBand = 40000000;

    //默认的分辨率,单位Hz
    protected const double DefaultResolution = 25000;

    //放大器开时对应的值，bit0/1 : RF head(1 normal, 2 high sensibility), bit5 : SG3 antenna ampli, bit6: SG1 antenna ampli
    //因此放大全开为01100010即98，只开高端放大为00100010即34，如果只开射频放大即为2
    protected readonly int AmpliValue = 98;

    //数据发送函数
    protected SendCmdDelegate SendCmd;
    protected DeviceParams Device;
    internal static uint PhaseNo;

    #endregion

    #region 初始化及停止

    /// <summary>
    ///     初始化指令集（在启动任务时调用）
    /// </summary>
    /// <param name="device"></param>
    /// <param name="sendCmdFunc"></param>
    public virtual uint Init(DeviceParams device, SendCmdDelegate sendCmdFunc)
    {
        Device = device;
        SendCmd = sendCmdFunc;
        Version = device.Version;
        if (PhaseNo == uint.MaxValue)
            PhaseNo = 0;
        else
            PhaseNo++;
        InitCmd(device);
        return PhaseNo;
    }

    protected void InitCmd(DeviceParams device)
    {
        var delManualChannelsRequest = new ManualChannelsDeletionRequest();
        delManualChannelsRequest.NumOfChannels.Value = 0;
        SendCmd(delManualChannelsRequest.GetBytes());
        SetAntenna(Device.CurrAntenna, Device.AmpliConfig);
        InitCommand(device);
    }

    /// <summary>
    ///     初始化指令集(子类必须重载)
    /// </summary>
    /// <param name="device"></param>
    protected abstract void InitCommand(DeviceParams device);

    /// <summary>
    ///     停止任务时调用，子类根据各自功能特点重载
    /// </summary>
    public virtual void Stop()
    {
        //停止所有通道
        StopProc(-1, 0);
        ProgramingInfo = null;
    }

    #endregion

    #region 参数设置

    /// <summary>
    ///     设置窄带中心频率
    /// </summary>
    /// <param name="freq">频率，单位Hz</param>
    public virtual void SetCenterFrequency(double freq)
    {
        if (Device == null) return;
        Device.Frequency = freq;
        Restart();
    }

    /// <summary>
    ///     设置带宽
    /// </summary>
    /// <param name="bw">带宽，单位Hz</param>
    public virtual void SetIfBandwidth(double bw)
    {
        if (Device == null) return;
        Device.IfBandWidth = bw;
        Restart();
    }

    public virtual void SetFilterBandwidth(double bw)
    {
        if (Device == null) return;
        Device.FilterBandWidth = bw;
        Restart();
    }

    /// <summary>
    ///     设置带宽，单独对应于测向功能带宽
    /// </summary>
    /// <param name="bw"></param>
    public virtual void SetDfBandwidth(double bw)
    {
        if (Device == null) return;
        Device.DfBandWidth = bw;
        Restart();
    }

    /// <summary>
    ///     设置衰减
    /// </summary>
    /// <param name="att"></param>
    public virtual void SetAttenuation(int att)
    {
        if (Device == null) return;
        Device.Attenuation = att;
        Restart();
    }

    /// <summary>
    ///     设置放大器开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetAmpli(bool onoff)
    {
        if (Device == null) return;
        Device.AmpliConfig = onoff;
        Restart();
    }

    /// <summary>
    ///     设置解调模式
    /// </summary>
    /// <param name="dem"></param>
    public virtual void SetDemodulation(uint dem)
    {
        if (Device != null) Device.DemMode = XeAssister.GetDemoduMode(dem);
    }

    /// <summary>
    ///     设置静噪门限
    /// </summary>
    /// <param name="squelchThreshold">静噪门限，单位dbm</param>
    public virtual void SetSquelchThreshold(int squelchThreshold)
    {
        if (Device != null) Device.SquelchThreshold = squelchThreshold;
    }

    /// <summary>
    ///     设置静噪开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetSquelchSwitch(bool onoff)
    {
        if (Device != null) Device.SquelchSwitch = onoff;
    }

    /// <summary>
    ///     设置ITU测量中的xdb参数和beta参数
    /// </summary>
    /// <param name="xdb"></param>
    /// <param name="beta"></param>
    public virtual void SetXdBAndBeta(float xdb, float beta)
    {
        if (Device != null)
        {
            Device.XdB = xdb;
            Device.Beta = beta;
        }
    }

    /// <summary>
    ///     设置宽带起始频率
    /// </summary>
    /// <param name="startFreq">起始频率, 单位Hz</param>
    public virtual void SetStartFrequency(double startFreq)
    {
        if (Device == null) return;
        Device.StartFrequency = startFreq;
        Restart();
    }

    /// <summary>
    ///     设置宽带结束频率
    /// </summary>
    /// <param name="stopFreq">结束频率，单位Hz</param>
    public virtual void SetStopFrequency(double stopFreq)
    {
        if (Device == null) return;
        Device.StopFrequency = stopFreq;
        Restart();
    }

    /// <summary>
    ///     设置宽带分辨率
    /// </summary>
    /// <param name="resolution">分辨率(扫描步进), 单位Hz</param>
    public virtual void SetResolution(double resolution)
    {
        if (Device != null) Device.ResolutionBandwidth = resolution;
        var rbwHz = resolution * 1000d;
        if (BbRequest != null)
        {
            BbRequest.Resolution.Value = rbwHz;
            //宽带起始频率需要能整除分辨率
            BbRequest.Band.FMin.Value = (uint)(BbRequest.Band.FMin.Value - BbRequest.Band.FMin.Value % rbwHz);
            BbRequest.Band.FMax.Value = (uint)(BbRequest.Band.FMax.Value - BbRequest.Band.FMax.Value % rbwHz);
            if (resolution > 25000)
                //分辨率大于25k时只能为快速模式
                BbRequest.Sensitivity.Value = 1;
        }
    }

    /// <summary>
    ///     设置当前的测向模式
    /// </summary>
    /// <param name="currMode"></param>
    public virtual void SetDfMode(DFindMode currMode)
    {
        if (Device == null) return;
        Device.DFindMode = currMode;
    }

    /// <summary>
    ///     设置测向电平门限
    /// </summary>
    /// <param name="levelThreshold">测向电平门限，单位dbm</param>
    public virtual void SetLevelThreshold(int levelThreshold)
    {
        if (Device == null) return;
        Device.LevelThreshold = levelThreshold;
    }

    /// <summary>
    ///     设置测向质量门限
    /// </summary>
    /// <param name="quality">质量门限分为10个等级，范围[0,9]</param>
    public virtual void SetQualityThreshold(int quality)
    {
        if (Device == null) return;
        Device.QualityThreshold = quality;
    }

    /// <summary>
    ///     设置音频开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetAudioSwitch(bool onoff)
    {
        if (Device == null) return;
        Device.AudioSwitch = onoff;
        Restart();
    }

    public virtual void SetItuSwitch(bool onoff)
    {
        if (Device == null) return;
        Device.ItuSwitch = onoff;
    }

    public virtual void SetIqSwitch(bool onoff)
    {
        if (Device == null) return;
        Device.IqSwitch = onoff;
    }

    /// <summary>
    ///     设置中频多路通道
    /// </summary>
    /// <param name="channels"></param>
    public virtual void SetIfMultiChannels(Dictionary<string, object>[] channels)
    {
        if (Device == null) return;
        Device.DdcChannels = channels;
    }

    /// <summary>
    ///     设置天线信息，通常是在原厂测向天线和J4口所配置的监测天线切换时需要
    /// </summary>
    public void SetAntenna(string currAntenna, bool ampliConfig)
    {
        if (Device != null)
        {
            Device.CurrAntenna = currAntenna;
            Device.AmpliConfig = ampliConfig;
        }

        //确定此消息是否需要
        var antenna = Encoding.ASCII.GetBytes(currAntenna);
        var antennaRequest = new AntennaModificationRequest();
        Array.Copy(antenna, antennaRequest.Antenna.Value, antenna.Length);
        SendCmd(antennaRequest.GetBytes(), 100);
        SendCmd(antennaRequest.GetBytes());
        //
        if (ProgramingInfo == null) return;
        if (ProgramingInfo.Channels?.Any() == true)
            foreach (var chan in ProgramingInfo.Channels)
            {
                chan.AmpliConfig.Value = (byte)(ampliConfig ? AmpliValue : 1);
                Array.Copy(antenna, chan.Antenna.Value, antenna.Length);
            }

        SendCmd(ProgramingInfo.GetBytes());
    }

    #endregion

    #region 辅助函数

    private void Restart()
    {
        Stop();
        InitCmd(Device);
    }

    /// <summary>
    ///     This message must be sent to stop processing on a channel.
    /// </summary>
    /// <param name="chan">-1表示停止所有通道</param>
    /// <param name="measurements">0 for all the processing on the channel.</param>
    protected void StopProc(int chan, uint measurements)
    {
        var stopRequest = new StopProcessing();
        stopRequest.ChannelId.Value = (sbyte)chan;
        stopRequest.Measurements.Value = measurements;
        SendCmd?.Invoke(stopRequest.GetBytes(), 100);
    }

    /// <summary>
    ///     获取宽带最小频率，要能整除分辨率
    /// </summary>
    /// <param name="centerFreq">当前的中心频率，单位Hz</param>
    /// <param name="bw"></param>
    /// <param name="resolution">分辨率，单位Hz</param>
    /// <returns></returns>
    protected uint GetBbFmin(uint centerFreq, uint bw = IfBand, double resolution = 25000)
    {
        var minFreq = centerFreq - bw / 2;
        minFreq = (uint)(minFreq - minFreq % resolution);
        return minFreq < Fmin ? Fmin : minFreq;
    }

    /// <summary>
    ///     获取宽带最大频率，要能整除分辨率
    /// </summary>
    /// <param name="centerFreq">当前的中心频率，单位Hz</param>
    /// <param name="bw"></param>
    /// <param name="resolution">分辨率，单位Hz</param>
    /// <returns></returns>
    protected uint GetBbFmax(uint centerFreq, uint bw = IfBand, double resolution = 25000)
    {
        var maxFreq = centerFreq + bw / 2;
        maxFreq = (uint)(maxFreq - maxFreq % resolution);
        return maxFreq > Fmax ? Fmax : maxFreq;
    }

    protected void GetBbInfo(uint centerFrequencyHz, uint bwHz, bool autoRbw, ref double rbwHz, out uint minFreqHz,
        out uint maxFreqHz)
    {
        if (autoRbw) rbwHz = GetRbwHz(bwHz);
        if (centerFrequencyHz + bwHz / 2 > Fmax)
        {
            maxFreqHz = centerFrequencyHz + bwHz / 2;
            maxFreqHz = (uint)(maxFreqHz - maxFreqHz % rbwHz);
            maxFreqHz = maxFreqHz > Fmax ? Fmax : maxFreqHz;
            minFreqHz = maxFreqHz - bwHz;
            minFreqHz = (uint)(minFreqHz - minFreqHz % rbwHz);
        }
        else
        {
            minFreqHz = centerFrequencyHz - bwHz / 2;
            minFreqHz = (uint)(minFreqHz + minFreqHz % rbwHz);
            minFreqHz = minFreqHz < Fmin ? Fmin : minFreqHz;
            maxFreqHz = minFreqHz + bwHz;
            maxFreqHz = (uint)(maxFreqHz + maxFreqHz % rbwHz);
        }
    }

    protected double GetRbwHz(uint bwHz)
    {
        double rbwHz;
        if (bwHz > 25_000_000)
            rbwHz = 25000;
        else if (bwHz > 12_500_000)
            rbwHz = 12500;
        else if (bwHz > 6_250_000)
            rbwHz = 6250;
        else if (bwHz > 3_125_000)
            rbwHz = 3125;
        else if (bwHz > 10_000)
            rbwHz = 1562.5;
        else // <250kHz
            rbwHz = 781.25;
        return rbwHz;
    }

    /// <summary>
    ///     获取射频衰减和中频衰减组合
    /// </summary>
    /// <param name="att"></param>
    /// <param name="rfAtt"></param>
    /// <param name="ifAtt"></param>
    protected static void PartAttenuation(int att, ref int rfAtt, ref int ifAtt)
    {
        //超短波，射频范围0,15，中频范围[0, 31]，以下为通过LGMRE软件上强弱调整得到的组合规律
        att = att < 0 ? 0 : att > 46 ? 46 : att;
        if (att <= 20)
        {
            rfAtt = 0;
            ifAtt = att;
        }
        else
        {
            rfAtt = 15;
            ifAtt = att - rfAtt;
        }
    }

    #endregion
}