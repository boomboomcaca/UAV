// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ParameterNames.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Magneto.Protocol.Define;

/// <summary>
///     参数名称定义
/// </summary>
public static class ParameterNames
{
    #region 功能参数

    /// <summary>
    ///     自动门限与手动门限切换
    /// </summary>
    public const string AutoThreshold = "autoThreshold";

    /// <summary>
    ///     门限值
    /// </summary>
    public const string ThresholdValue = "thresholdValue";

    #endregion

    #region 统计开关

    /// <summary>
    ///     原始数据保存开关
    /// </summary>
    public const string RawSwitch = "rawSwitch";

    /// <summary>
    ///     是否开启超过测量门限以后才保存数据
    /// </summary>
    public const string RecByThreshold = "recByThreshold";

    /// <summary>
    ///     录制数据保存开关
    /// </summary>
    public const string SaveDataSwitch = "saveDataSwitch";

    /// <summary>
    ///     日报数据开关
    /// </summary>
    public const string MrdSwitch = "mrdSwitch";

    /// <summary>
    ///     单位换算开关
    /// </summary>
    public const string UnitSelection = "unitSelection";

    /// <summary>
    ///     频谱最大值显示开关
    /// </summary>
    public const string MaximumSwitch = "maximumSwitch";

    /// <summary>
    ///     频谱最小值显示开关
    /// </summary>
    public const string MinimumSwitch = "minimumSwitch";

    /// <summary>
    ///     平均值显示开关
    /// </summary>
    public const string MeanSwitch = "meanSwitch";

    /// <summary>
    ///     噪声显示开关
    /// </summary>
    public const string NoiseSwitch = "noiseSwitch";

    /// <summary>
    ///     The threshold switch
    /// </summary>
    public const string ThresholdSwitch = "thresholdSwitch";

    #endregion

    #region 常用参数

    /// <summary>
    ///     中心频率 MHz
    /// </summary>
    public const string Frequency = "frequency";

    /// <summary>
    ///     频偏 MHz
    /// </summary>
    public const string FrequencyOffset = "frequencyOffset";

    /// <summary>
    ///     中频带宽、频谱带宽 KHz
    /// </summary>
    public const string IfBandwidth = "ifBandwidth";

    /// <summary>
    ///     测向带宽 KHz
    /// </summary>
    public const string DfBandwidth = "dfBandwidth";

    /// <summary>
    ///     测向分辨率
    /// </summary>
    public const string ResolutionBandwidth = "resolutionBandwidth";

    /// <summary>
    ///     解调带宽、滤波带宽 KHz
    /// </summary>
    public const string FilterBandwidth = "filterBandwidth";

    /// <summary>
    ///     起始频率 MHz
    /// </summary>
    public const string StartFrequency = "startFrequency";

    /// <summary>
    ///     结束频率 MHz
    /// </summary>
    public const string StopFrequency = "stopFrequency";

    /// <summary>
    ///     频率步进 KHz
    /// </summary>
    public const string StepFrequency = "stepFrequency";

    /// <summary>
    ///     射频模式
    /// </summary>
    public const string RfMode = "rfMode";

    /// <summary>
    ///     衰减控制
    /// </summary>
    public const string AttCtrlType = "attCtrlType";

    /// <summary>
    ///     射频衰减
    /// </summary>
    public const string RfAttenuation = "rfAttenuation";

    /// <summary>
    ///     中频衰减
    /// </summary>
    public const string IfAttenuation = "ifAttenuation";

    /// <summary>
    ///     频段列表
    /// </summary>
    public const string ScanSegments = "scanSegments";

    /// <summary>
    ///     频率列表
    /// </summary>
    public const string MscanPoints = "mscanPoints";

    /// <summary>
    ///     频率表扫描测向的频率列表
    /// </summary>
    public const string MscanDfPoints = "mscandfPoints";

    /// <summary>
    ///     离散测向的频率列表
    /// </summary>
    public const string MfdfPoints = "mfdfPoints";

    /// <summary>
    ///     中频通道
    /// </summary>
    public const string DdcChannels = "ddcChannels";

    /// <summary>
    ///     测向模式
    /// </summary>
    public const string DfindMode = "dfindMode";

    /// <summary>
    ///     扫描模式
    /// </summary>
    public const string ScanMode = "scanMode";

    /// <summary>
    ///     电平门限
    /// </summary>
    public const string LevelThreshold = "levelThreshold";

    /// <summary>
    ///     质量门限
    /// </summary>
    public const string QualityThreshold = "qualityThreshold";

    /// <summary>
    ///     静噪门限
    /// </summary>
    public const string SquelchThreshold = "squelchThreshold";

    /// <summary>
    ///     测量门限
    /// </summary>
    public const string MeasureThreshold = "measureThreshold";

    /// <summary>
    ///     解调模式
    /// </summary>
    public const string DemMode = "demMode";

    /// <summary>
    ///     检波方式
    /// </summary>
    public const string Detector = "detector";

    /// <summary>
    ///     XdB带宽
    /// </summary>
    public const string Xdb = "xdB";

    /// <summary>
    ///     β带宽
    /// </summary>
    public const string BetaValue = "betaValue";

    /// <summary>
    ///     衰减
    /// </summary>
    public const string Attenuation = "attenuation";

    /// <summary>
    ///     等待时间
    /// </summary>
    public const string HoldTime = "holdTime";

    /// <summary>
    ///     驻留时间
    /// </summary>
    public const string DwellTime = "dwellTime";

    /// <summary>
    ///     FFT模式
    /// </summary>
    public const string FftMode = "fftMode";

    /// <summary>
    ///     测量时间
    /// </summary>
    public const string MeasureTime = "measureTime";

    /// <summary>
    ///     带宽测量模式
    /// </summary>
    public const string BandMeasureMode = "bandMeasureMode";

    /// <summary>
    ///     增益
    /// </summary>
    public const string Gain = "gain";

    /// <summary>
    ///     The integration count
    /// </summary>
    public const string IntegrationCount = "integrationCount";

    /// <summary>
    ///     测向积分时间
    /// </summary>
    public const string IntegrationTime = "integrationTime";

    #endregion

    #region 数据开关

    /// <summary>
    ///     IQ数据开关
    /// </summary>
    public const string IqSwitch = "iqSwitch";

    /// <summary>
    ///     电平开关
    /// </summary>
    public const string LevelSwitch = "levelSwitch";

    /// <summary>
    ///     频谱开关
    /// </summary>
    public const string SpectrumSwitch = "spectrumSwitch";

    /// <summary>
    ///     音频开关
    /// </summary>
    public const string AudioSwitch = "audioSwitch";

    /// <summary>
    ///     ITU开关
    /// </summary>
    public const string ItuSwitch = "ituSwitch";

    /// <summary>
    ///     调制识别开关
    /// </summary>
    public const string MrSwitch = "mrSwitch";

    /// <summary>
    ///     解调开关
    /// </summary>
    public const string DemodulationSwitch = "demodulationSwitch";

    /// <summary>
    ///     占用度统计开关
    /// </summary>
    public const string OccupancySwitch = "occupancySwitch";

    /// <summary>
    ///     静噪开关
    /// </summary>
    public const string SquelchSwitch = "squelchSwitch";

    /// <summary>
    ///     中频输出开关
    /// </summary>
    public const string IfSwitch = "ifSwitch";

    /// <summary>
    ///     驻留开关
    /// </summary>
    public const string DwellSwitch = "dwellSwitch";

    #endregion

    #region 安装参数

    /// <summary>
    ///     IP地址
    /// </summary>
    public const string IpAddress = "ipAddress";

    /// <summary>
    ///     端口号
    /// </summary>
    public const string Port = "port";

    /// <summary>
    ///     接收机
    /// </summary>
    public const string Receiver = "receiver";

    /// <summary>
    ///     测向机
    /// </summary>
    public const string Dfinder = "dfinder";

    /// <summary>
    ///     天线控制器
    /// </summary>
    public const string AntennaController = "antennaController";

    /// <summary>
    ///     压制机
    /// </summary>
    public const string Suppressor = "suppressor";

    /// <summary>
    ///     开关阵（继电器开关）
    /// </summary>
    public const string SwitchArray = "switchArray";

    /// <summary>
    ///     The pre amp switch
    /// </summary>
    public const string PreAmpSwitch = "preAmpSwitch";

    #endregion

    #region 天线相关

    /// <summary>
    ///     选择的天线集合
    /// </summary>
    public const string Antennas = "antennas";

    /// <summary>
    ///     安装的天线集合
    /// </summary>
    public const string AntennaSet = "antennaSet";

    /// <summary>
    ///     天线选择模式
    /// </summary>
    public const string AntennaSelectionMode = "antennaSelectionMode";

    /// <summary>
    ///     当前选择的天线
    /// </summary>
    public const string AntennaId = "antennaID";

    /// <summary>
    ///     测向极化方式
    ///     由于历史原因，以后测向极化方式都叫这个名字，与监测或天线的极化方式分开
    /// </summary>
    public const string DfPolarization = "dfPolarization";

    /// <summary>
    ///     监测极化方式
    /// </summary>
    public const string Polarization = "polarization";

    #endregion

    #region ITU命名

    /// <summary>
    ///     实时频率
    /// </summary>
    public const string ItuFrequency = "frequency";

    /// <summary>
    ///     电平
    /// </summary>
    public const string ItuLevel = "level";

    /// <summary>
    ///     场强
    /// </summary>
    public const string ItuStrength = "fieldStrength";

    /// <summary>
    ///     AM调制深度 %
    /// </summary>
    public const string ItuAmDepth = "amDepth";

    /// <summary>
    ///     AM正向调制深度 %
    /// </summary>
    public const string ItuAmDepthPos = "amDepthPos";

    /// <summary>
    ///     AM负向调制深度 %
    /// </summary>
    public const string ItuAmDepthNeg = "amDepthNeg";

    /// <summary>
    ///     FM最大频偏 kHz
    /// </summary>
    public const string ItuFmDev = "fmDev";

    /// <summary>
    ///     FM正向调制深度 kHz
    /// </summary>
    public const string ItuFmDevPos = "fmDevPos";

    /// <summary>
    ///     FM负向调制深度 kHz
    /// </summary>
    public const string ItuFmDevNeg = "fmDevNeg";

    /// <summary>
    ///     相位偏移 rad
    /// </summary>
    public const string ItuPmDepth = "pmDepth";

    /// <summary>
    ///     xdb带宽 kHz
    /// </summary>
    public const string ItuXdb = "xdb";

    /// <summary>
    ///     beta带宽 kHz
    /// </summary>
    public const string ItuBeta = "beta";

    /// <summary>
    ///     通量功率密度 W/m²
    /// </summary>
    public const string ItuFpd = "fpd";

    /// <summary>
    ///     无用发射 dBm
    /// </summary>
    public const string ItuUne = "une";

    /// <summary>
    ///     脉冲测量-上升时间 ms
    /// </summary>
    public const string ItuPumRise = "pumRise";

    /// <summary>
    ///     脉冲测量-下降时间 ms
    /// </summary>
    public const string ItuPumFall = "pumFall";

    /// <summary>
    ///     脉冲测量-脉冲宽度 ms
    /// </summary>
    public const string ItuPumWidth = "pumWidth";

    /// <summary>
    ///     频率使用（占用度） %
    /// </summary>
    public const string ItuOccupancy = "occupancy";

    #endregion

    #region 压制参数

    /// <summary>
    ///     射频管控
    ///     常规参数，表示包含特定通道多条待压制参数信息
    /// </summary>
    public const string RftxSegments = "rftxSegments";

    /// <summary>
    ///     功放功率
    /// </summary>
    public const string Powers = "powers";

    /// <summary>
    ///     监测管制开关
    ///     常规参数，表示用于切换监测与管制的参数，true为管制，false为监测
    /// </summary>
    public const string RmsSwitch = "rmsSwitch";

    /// <summary>
    ///     管制频段
    /// </summary>
    public const string RftxBands = "rftxBands";

    /// <summary>
    ///     物理通道号
    ///     表示当前管制使用的物理通道编号，适用于特定频段范围内的压制，关联特定的功率放大器。
    /// </summary>
    public const string PhysicalChannelNumber = "physicalChannelNumber";

    /// <summary>
    ///     逻辑通道号
    ///     表示当前管制使用的逻辑通道编号，从属于上述物理通道，
    ///     对于性能较强的压制设备，其可能支持一个物理通道内同时多个多模式的信号压制，此时需要设置相应的逻辑通道编号，
    /// </summary>
    public const string LogicalChannelNumber = "logicalChannelNumber";

    /// <summary>
    ///     射频开关
    ///     表示当前压制使能
    /// </summary>
    public const string RftxSwitch = "rftxSwitch";

    /// <summary>
    ///     压制模式
    ///     表示频率压制模式，分为：0 - 定频，1 - 跳频， 2 - 扫频
    /// </summary>
    public const string RftxFrequencyMode = "rftxFrequencyMode";

    /// <summary>
    ///     调制模式
    ///     表示当前压制使用的调制方式，如AM, FM, QPKS等
    /// </summary>
    public const string Modulation = "modulation";

    /// <summary>
    ///     调制源
    ///     表示在使用FM或AM调制方式进行压制时，调制信号的来源，
    ///     分为：0 - 1kHz单音，1 - 网络语音， 2 - 噪声
    /// </summary>
    public const string ModulationSource = "modulationSource";

    /// <summary>
    ///     调制带宽
    ///     表示调制信号的带宽，单位: kHz
    /// </summary>
    public const string Bandwidth = "bandwidth";

    /// <summary>
    ///     调制速率
    ///     表示调制信号的符号率，码元率等，单位：kpbs
    /// </summary>
    public const string Baudrate = "baudrate";

    /// <summary>
    ///     跳频频点
    ///     表示跳频压制模式下的离散频表，单位：MHz
    /// </summary>
    public const string Frequencies = "frequencies";

    /// <summary>
    ///     逻辑通道数
    ///     配置压制机对应物理通道可用的逻辑通道数量
    /// </summary>
    public const string LogicalChannelCount = "logicalChannelCount";

    /// <summary>
    ///     是否开启卫星导航压制GNSS
    /// </summary>
    public const string EnableSuppressGnss = "enableSuppressGNSS";

    /// <summary>
    ///     通道频段信息
    /// </summary>
    public const string ChannelSubBands = "channelSubBands";

    /// <summary>
    ///     通道最大功率
    /// </summary>
    public const string ChannelMaxPower = "channelMaxPower";

    #endregion
}