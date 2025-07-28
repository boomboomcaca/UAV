using System;
using System.Collections.Generic;
using System.Text;
using Magneto.Device.XE_VUHF.Protocols;
using Magneto.Device.XE_VUHF.Protocols.Data;

namespace Magneto.Device.XE_VUHF.Commands;

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
    //放大器开时对应的值
    protected readonly int AmpliValue = 98;

    //默认的分辨率,单位Hz
    protected readonly double DefaultResolution = 25000;

    //接收机支持最大频率,单位Hz
    protected readonly uint Fmax = 3000000000;

    //接收机支持最小频率,单位Hz
    protected readonly uint Fmin = 20000000;

    //最大中频带宽,单位Hz
    protected readonly uint IfBand = 40000000;

    //宽带参数设置
    protected BbInterceptionRequest BbRequest;

    protected DeviceParams Device;

    //编程通道设置
    protected ReceiverProgramingInfo ProgramingInfo;

    //数据发送函数
    protected SendCmdDelegate SendCmd;

    //数据协议版本
    protected uint Version = 25;

    #region 初始化及停止

    /// <summary>
    ///     初始化指令集（在启动任务时调用）
    /// </summary>
    /// <param name="device"></param>
    /// <param name="sendCmdFunc"></param>
    public virtual void Init(DeviceParams device, SendCmdDelegate sendCmdFunc)
    {
        SendCmd = sendCmdFunc;
        Version = device.Version;
        Device = device;
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
    }

    #endregion

    #region 参数设置

    /// <summary>
    ///     设置窄带中心频率
    /// </summary>
    /// <param name="freq">频率，单位Hz</param>
    public virtual void SetCenterFrequency(uint freq)
    {
        Device.Frequency = freq;
    }

    /// <summary>
    ///     设置带宽
    /// </summary>
    /// <param name="bw">带宽，单位Hz</param>
    public virtual void SetBandwidth(uint bw)
    {
        Device.IfBandWidth = bw;
    }

    /// <summary>
    ///     设置带宽，单独对应于测向功能带宽, 单位Hz
    /// </summary>
    /// <param name="bw"></param>
    public virtual void SetDfBandwidth(uint bw)
    {
        Device.DfBandWidth = bw;
    }

    /*
    /// <summary>
    /// 设置增益控制方式
    /// </summary>
    /// <param name="agc">是否为自动AGC</param>
    /// <param name="ifAtt">中频衰减</param>
    /// <param name="rfAtt">射频衰减</param>
    public virtual void SetAgcControll(bool agc, int ifAtt, int rfAtt)
    {
        if (_programingInfo != null && _programingInfo.Channels != null)
        {
            foreach (var chan in _programingInfo.Channels)
            {
                if (agc)
                {
                    chan.AGCType.Value = 2;
                    chan.IFAttenuator.Value = 0;
                    chan.RFAttenuator.Value = 0;
                }
                else
                {
                    chan.AGCType.Value = 1;
                    chan.IFAttenuator.Value = (byte)ifAtt;
                    chan.RFAttenuator.Value = (byte)rfAtt;
                }
            }
        }
    }
     * */
    /// <summary>
    ///     设置衰减
    /// </summary>
    /// <param name="att"></param>
    public virtual void SetAttenuation(int att)
    {
        Device.Attenuation = att;
        int rfAtt = 0, ifAtt = 0;
        if (att != -1) PartAttenuation(att, ref rfAtt, ref ifAtt);
        if (ProgramingInfo is { Channels: not null })
            foreach (var chan in ProgramingInfo.Channels)
                if (att == -1)
                {
                    chan.AgcType.Value = 2;
                    chan.IfAttenuator.Value = 0;
                    chan.RfAttenuator.Value = 0;
                }
                else
                {
                    chan.AgcType.Value = 1;
                    chan.IfAttenuator.Value = (byte)ifAtt;
                    chan.RfAttenuator.Value = (byte)rfAtt;
                }
    }

    /// <summary>
    ///     设置放大器开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetAmpli(bool onoff)
    {
        Device.AmpliConfig = onoff;
        if (ProgramingInfo is { Channels: not null })
            foreach (var chan in ProgramingInfo.Channels)
                chan.AmpliConfig.Value = (byte)(onoff ? AmpliValue : 1);
    }

    /// <summary>
    ///     设置解调模式
    /// </summary>
    /// <param name="dem"></param>
    public virtual void SetDemodulation(uint dem)
    {
        Device.DemMode = XeAssister.GetDemoduMode(dem);
    }

    /// <summary>
    ///     设置静噪门限
    /// </summary>
    /// <param name="squelchThreshold">静噪门限，单位dbm</param>
    public virtual void SetSquelchThreshold(int squelchThreshold)
    {
        Device.SquelchThreshold = squelchThreshold;
    }

    /// <summary>
    ///     设置静噪开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetSquelchSwitch(bool onoff)
    {
        Device.SquelchSwitch = onoff;
    }

    /// <summary>
    ///     设置ITU测量中的xdb参数和beta参数
    /// </summary>
    /// <param name="xdb"></param>
    /// <param name="beta"></param>
    public virtual void SetXdBAndBeta(float xdb, float beta)
    {
        Device.XdB = xdb;
        Device.Beta = beta;
    }

    /// <summary>
    ///     设置宽带起始频率
    /// </summary>
    /// <param name="startFreq">起始频率, 单位Hz</param>
    public virtual void SetStartFrequency(uint startFreq)
    {
        Device.StartFrequency = startFreq;
        if (BbRequest != null) BbRequest.Band.FMin.Value = startFreq;
    }

    /// <summary>
    ///     设置宽带结束频率
    /// </summary>
    /// <param name="stopFreq">结束频率，单位Hz</param>
    public virtual void SetStopFrequency(uint stopFreq)
    {
        Device.StopFrequency = stopFreq;
        if (BbRequest != null) BbRequest.Band.FMax.Value = stopFreq;
    }

    /// <summary>
    ///     设置宽带分辨率
    /// </summary>
    /// <param name="resolution">分辨率(扫描步进), 单位Hz</param>
    public virtual void SetResolution(double resolution)
    {
        Device.StepFrequency = resolution;
        if (BbRequest != null)
        {
            BbRequest.Resolution.Value = resolution;
            //宽带起始频率需要能整除分辨率
            BbRequest.Band.FMin.Value = (uint)(BbRequest.Band.FMin.Value - BbRequest.Band.FMin.Value % resolution);
            BbRequest.Band.FMax.Value = (uint)(BbRequest.Band.FMax.Value - BbRequest.Band.FMax.Value % resolution);
            if (resolution > 25000)
                //分辨率大于25k时只能为快速模式
                BbRequest.Sensitivity.Value = 1;
        }
    }

    ///// <summary>
    ///// 设置宽带快速截收模式(短波无此参数)
    ///// </summary>
    ///// <param name="onoff"></param>
    //public virtual void SetTurbo(bool onoff)
    //{
    //    if (_bbRequest != null)
    //    {
    //        _bbRequest.Turbo.Value = (byte)(onoff ? 1 : 0);
    //        if (onoff)
    //        {
    //            //快速截收模式下,不能为灵敏模式，分辨率只能为25k
    //            _bbRequest.Sensitivity.Value = 1;
    //            _bbRequest.Resolution.Value = 25000;
    //            //同时校验起始频率
    //            _bbRequest.Band.FMin.Value = (uint)(_bbRequest.Band.FMin.Value - _bbRequest.Band.FMin.Value % 25000);
    //            _bbRequest.Band.FMax.Value = (uint)(_bbRequest.Band.FMax.Value - _bbRequest.Band.FMax.Value % 25000);
    //        }
    //    }
    //}
    /// <summary>
    ///     设置FM陷波()
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetFmFilter(bool onoff)
    {
        Device.FmFilter = onoff;
        if (ProgramingInfo is { Channels: not null })
            foreach (var chan in ProgramingInfo.Channels)
                chan.FmFilter.Value = (byte)(onoff ? 1 : 2);
    }

    /// <summary>
    ///     设置灵敏模式
    /// </summary>
    /// <param name="onoff">on:灵敏，off:快速</param>
    public virtual void SetSensitivity(bool onoff)
    {
        Device.Sensitivity = onoff;
        if (BbRequest != null) BbRequest.Sensitivity.Value = (byte)(onoff ? 0 : 1);
    }

    /// <summary>
    ///     设置当前的测向模式
    /// </summary>
    /// <param name="currMode"></param>
    public virtual void SetDfMode(XedFindMode currMode)
    {
        Device.DFindMode = (int)currMode;
    }

    /// <summary>
    ///     设置测向电平门限
    /// </summary>
    /// <param name="levelThreshold">测向电平门限，单位dbm</param>
    public virtual void SetLevelThreshold(int levelThreshold)
    {
        Device.LevelThreshold = levelThreshold;
    }

    /// <summary>
    ///     设置测向质量门限
    /// </summary>
    /// <param name="qualityMark">质量门限分为10个等级，范围[0,9]</param>
    public virtual void SetQualityThreshold(ushort qualityMark)
    {
        Device.QualityThreshold = qualityMark;
    }

    /// <summary>
    ///     设置音频开关
    /// </summary>
    /// <param name="onoff"></param>
    public virtual void SetAudioSwitch(bool onoff)
    {
        Device.AudioSwitch = onoff;
    }

    /// <summary>
    ///     设置中频多路通道
    /// </summary>
    /// <param name="channels"></param>
    public virtual void SetIfMultiChannels(Dictionary<string, object>[] channels)
    {
        Device.IfMultiChannels = channels;
    }

    /// <summary>
    ///     设置天线信息，通常是在原厂测向天线和J4口所配置的监测天线切换时需要
    /// </summary>
    public void SetAntenna(string currAntenna, bool ampliConfig)
    {
        Device.CurrAntenna = currAntenna;
        Device.AmpliConfig = ampliConfig;
        //确定此消息是否需要
        var antenna = Encoding.ASCII.GetBytes(currAntenna);
        AntennaModificationRequest antennaRequest = new();
        Array.Copy(antenna, antennaRequest.Antenna.Value, antenna.Length);
        SendCmd(antennaRequest.GetBytes());
        //
        foreach (var chan in ProgramingInfo.Channels)
        {
            chan.AmpliConfig.Value = (byte)(ampliConfig ? AmpliValue : 1);
            Array.Copy(antenna, chan.Antenna.Value, antenna.Length);
        }

        SendCmd(ProgramingInfo.GetBytes());
    }

    //用于测试使用，定版后再去掉
    public virtual void SetDetectionMode(ushort mode)
    {
        Device.DetectionMode = mode;
        if (BbRequest != null) BbRequest.DetectionMode.Value = mode;
    }

    //用于测试使用，定版后再去掉
    public virtual void SetIntegrationTime(ushort time)
    {
        Device.XeIntTime = time;
        if (BbRequest != null) BbRequest.IntTime.Value = time;
    }

    #endregion

    #region 辅助函数

    /// <summary>
    ///     This message must be sent to stop processing on a channel.
    /// </summary>
    /// <param name="chan">-1表示停止所有通道</param>
    /// <param name="measurements">0 for all the processing on the channel.</param>
    protected void StopProc(int chan, uint measurements)
    {
        StopProcessing stopRequest = new();
        stopRequest.ChannelId.Value = (sbyte)chan;
        stopRequest.Measurements.Value = measurements;
        SendCmd?.Invoke(stopRequest.GetBytes());
    }

    /// <summary>
    ///     重新设置通道参数，主要为个数改变时，即宽带和窄带切换时
    /// </summary>
    /// <param name="numOfChans"></param>
    protected void ResetProgrammingInfo(int numOfChans)
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

    /// <summary>
    ///     获取宽带最小频率，要能整除分辨率
    /// </summary>
    /// <param name="centerFreq">当前的中心频率，单位Hz</param>
    /// <param name="resolution">分辨率，单位Hz</param>
    /// <returns></returns>
    protected uint GetBbFmin(uint centerFreq, double resolution = 25000)
    {
        var minFreq = centerFreq - IfBand / 2;
        minFreq = (uint)(minFreq - minFreq % resolution);
        return minFreq < Fmin ? Fmin : minFreq;
    }

    /// <summary>
    ///     获取宽带最大频率，要能整除分辨率
    /// </summary>
    /// <param name="centerFreq">当前的中心频率，单位Hz</param>
    /// <param name="resolution">分辨率，单位Hz</param>
    /// <returns></returns>
    protected uint GetBbFmax(uint centerFreq, double resolution = 25000)
    {
        var maxFreq = centerFreq + IfBand / 2;
        maxFreq = (uint)(maxFreq - maxFreq % resolution);
        return maxFreq > Fmax ? Fmax : maxFreq;
    }

    /// <summary>
    ///     获取射频衰减和中频衰减组合
    /// </summary>
    /// <param name="att"></param>
    /// <param name="rfAtt"></param>
    /// <param name="ifAtt"></param>
    protected static void PartAttenuation(int att, ref int rfAtt, ref int ifAtt)
    {
        //超短波，射频范围0,10,20，中频范围[0, 31]，以下为通过LG319软件上强弱调整得到的组合规律
        att = att < 0 ? 0 : att > 51 ? 51 : att;
        if (att <= 20)
        {
            rfAtt = 0;
            ifAtt = att;
        }
        else if (att <= 30)
        {
            rfAtt = 10;
            ifAtt = att - rfAtt;
        }
        else
        {
            rfAtt = 20;
            ifAtt = att - rfAtt;
        }
    }

    #endregion
}