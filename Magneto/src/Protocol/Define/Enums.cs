// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Enums.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Magneto.Protocol.Extensions;
using MessagePack;

namespace Magneto.Protocol.Define;

/// <summary>
///     功能类型
/// </summary>
[Flags]
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<FeatureType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<FeatureType>))]
public enum FeatureType : long
{
    /// <summary>
    ///     未知功能
    /// </summary>
    [Key("none")] [Description("未知功能")] None = 1, // 这里必须为1，否则序列化的结果不符合前端需求

    /// <summary>
    ///     单频测量
    /// </summary>
    [Key("ffm")] [Description("单频测量")] Ffm = None << 1,

    /// <summary>
    ///     单频测向
    /// </summary>
    [Key("ffdf")] [Description("单频测向")] Ffdf = Ffm << 1,

    /// <summary>
    ///     比幅测向
    /// </summary>
    [Key("ampdf")] [Description("比幅测向")] AmpDf = Ffdf << 1,

    /// <summary>
    ///     宽带测向
    /// </summary>
    [Key("wbdf")] [Description("宽带测向")] Wbdf = AmpDf << 1,

    /// <summary>
    ///     扫描测向
    /// </summary>
    [Key("scandf")] [Description("扫描测向")] ScanDf = Wbdf << 1,

    /// <summary>
    ///     空间谱测向
    /// </summary>
    [Key("sse")] [Description("空间谱测向")] Sse = ScanDf << 1,

    /// <summary>
    ///     频段扫描
    /// </summary>
    [Key("scan")] [Description("频段扫描")] Scan = Sse << 1,

    /// <summary>
    ///     驻留频段扫描/频段搜索
    /// </summary>
    [Key("fscne")] [Description("频段搜索")] FScne = Scan << 1,

    /// <summary>
    ///     离散扫描
    /// </summary>
    [Key("mscan")] [Description("离散扫描")] MScan = FScne << 1,

    /// <summary>
    ///     驻留离散扫描/离散搜索
    /// </summary>
    [Key("mscne")] [Description("离散搜索")] MScne = MScan << 1,

    /// <summary>
    ///     中频多路功能
    /// </summary>
    [Key("ifmca")] [Description("中频多路")] Ifmca = MScne << 1,

    /// <summary>
    ///     TDOA功能
    /// </summary>
    [Key("tdoa")] [Description("TDOA")] Tdoa = Ifmca << 1,

    /// <summary>
    ///     ？
    /// </summary>
    [Key("remc")] [Description("REMC")] Remc = Tdoa << 1,

    /// <summary>
    ///     单车场强定位
    /// </summary>
    [Key("ssoa")] [Description("场强定位")] Ssoa = Remc << 1,

    // /// <summary>
    // /// POA功能
    // /// </summary>
    // [Key("poa")]
    // POA = SSOA << 1,
    /// <summary>
    ///     新信号截获功能
    /// </summary>
    [Key("nsic")] [Description("新信号截获")] Nsic = Ssoa << 1,

    /// <summary>
    ///     中频输出，通常作为多通道功能的辅助通道
    /// </summary>
    [Key("ifout")] [Description("中频输出")] Ifout = Nsic << 1,

    /// <summary>
    ///     考试保障
    /// </summary>
    [Key("ese")] [Description("考试保障")] Ese = Ifout << 1,

    /// <summary>
    ///     无人机侦测
    /// </summary>
    [Key("uavd")] [Description("无人机侦测")] Uavd = Ese << 1,

    /// <summary>
    ///     电磁环境采集
    /// </summary>
    [Key("emdc")] [Description("电磁环境采集")] Emdc = Uavd << 1,

    /// <summary>
    ///     日报/月报扫描（仅计划任务使用）
    /// </summary>
    [Key("mrscan")] [Description("日报月报扫描")]
    Report = Emdc << 1,

    /// <summary>
    ///     信号普查（仅计划任务使用）
    /// </summary>
    [Key("sglmgr")] [Description("信号普查")] SignalCensus = Report << 1,

    /// <summary>
    ///     ITU测量
    /// </summary>
    [Key("itum")] [Description("ITU测量")] Itum = SignalCensus << 1,

    /// <summary>
    ///     离散扫描测向（频率表扫描测向 原子化服务里面用的，暂时不动）
    /// </summary>
    [Key("mscandf")] [Description("离散扫描测向")]
    MScanDf = Itum << 1,

    /// <summary>
    ///     The DPX
    /// </summary>
    [Key("dpx")] [Description("荧光谱")] Dpx = MScanDf << 1,

    /// <summary>
    ///     音视频处理
    /// </summary>
    [Key("avprocess")] [Description("音视频处理")]
    AvProcess = Dpx << 1,

    /// <summary>
    ///     广播电视解调
    /// </summary>
    [Key("rtv")] [Description("广播电视解调")] Rtv = AvProcess << 1,

    /// <summary>
    ///     电磁环境分析
    /// </summary>
    [Key("emda")] [Description("电磁环境分析")] Emda = Rtv << 1,

    /// <summary>
    ///     单频测向
    /// </summary>
    [Key("fdf")] [Description("单频测向")] Fdf = Emda << 1,

    /// <summary>
    ///     无人机管制
    /// </summary>
    [Key("uavs")] [Description("无人机管制")] Uavs = Fdf << 1,

    /// <summary>
    ///     卫星通信管制
    /// </summary>
    [Key("satels")] [Description("卫星通信管制")]
    Satels = Uavs << 1,

    /// <summary>
    ///     公众通信管制
    /// </summary>
    [Key("pcoms")] [Description("公众通信管制")] Pcoms = Satels << 1,

    /// <summary>
    ///     全频段管制
    /// </summary>
    [Key("fbands")] [Description("全频段管制")] Fbands = Pcoms << 1,

    /// <summary>
    ///     The bs decoding
    /// </summary>
    [Key("bsdec")] [Description("基站监测")] BsDecoding = Fbands << 1,

    /// <summary>
    ///     The iqretri
    /// </summary>
    [Key("iqretri")] [Description("流盘分析")] Iqretri = BsDecoding << 1,

    /// <summary>
    ///     The sgldec
    /// </summary>
    [Key("sgldec")] [Description("信号解调")] Sgldec = Iqretri << 1,

    /// <summary>
    ///     The MFDF
    /// </summary>
    [Key("mfdf")] [Description("离散测向")] Mfdf = Sgldec << 1,

    /// <summary>
    ///     The avicg
    /// </summary>
    [Key("avicg")] [Description("航空监测多路监听")]
    Avicg = Mfdf << 1,

    /// <summary>
    ///     The amia
    /// </summary>
    [Key("amia")] [Description("航空监测干扰分析")]
    Amia = Avicg << 1,

    /// <summary>
    ///     频谱评估
    /// </summary>
    [Key("spevl")] [Description("频谱评估")] Spevl = Amia << 1,

    /// <summary>
    ///     射电天文电测
    /// </summary>
    [Key("fastemt")] [Description("射电天文电测")]
    Fastemt = Spevl << 1,

    /// <summary>
    ///     GSM-R专项监测
    /// </summary>
    [Key("gsmr")] [Description("GSM-R专项监测")]
    Gsmr = Fastemt << 1,

    /// <summary>
    ///     The uav definition
    /// </summary>
    [Key("uavdef")] [Description] UavDef = Gsmr << 1
}

/// <summary>
///     共享数据类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SDataType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SDataType>))]
public enum SDataType
{
    /// <summary>
    ///     未知的数据类型
    /// </summary>
    [Key("none")] [DataType(NeedSampling = false, CanDrop = true)]
    None = 0,

    /// <summary>
    ///     日志数据
    /// </summary>
    [Key("log")] [DataType(NeedSampling = false, CanDrop = false)]
    Log,

    /// <summary>
    ///     GPS数据
    /// </summary>
    [Key("gps")] [DataType(NeedSampling = false, CanDrop = true)]
    Gps,

    /// <summary>
    ///     电子罗盘数据
    /// </summary>
    [Key("compass")] [DataType(NeedSampling = false, CanDrop = true)]
    Compass,

    /// <summary>
    ///     状态变更数据
    /// </summary>
    [Key("moduleStateChange")] [DataType(NeedSampling = false, CanDrop = false)]
    ModuleStateChange,

    /// <summary>
    ///     计划结果数据
    /// </summary>
    /// /
    [Key("crondResult")] [DataType(NeedSampling = false, CanDrop = false)]
    CrondResult,

    /// <summary>
    ///     任务改变消息
    /// </summary>
    [Key("taskChangeInfo")] [DataType(NeedSampling = false, CanDrop = false)]
    TaskChangeInfo,

    /// <summary>
    ///     心跳数据
    /// </summary>
    [Key("heartBeat")] [DataType(NeedSampling = false, CanDrop = true)]
    HeartBeat,

    /// <summary>
    ///     IQ数据
    /// </summary>
    [Key("iq")] [DataType(NeedSampling = false, CanDrop = false)]
    Iq,

    /// <summary>
    ///     电平数据
    /// </summary>
    [Key("level")] [DataType(NeedSampling = true, CanDrop = false)]
    Level,

    /// <summary>
    ///     频谱数据
    /// </summary>
    [Key("spectrum")] [DataType(NeedSampling = true, CanDrop = false)]
    Spectrum,

    /// <summary>
    ///     扫描数据
    /// </summary>
    [Key("scan")] [DataType(NeedSampling = false, CanDrop = false)]
    Scan,

    /// <summary>
    ///     音频数据
    /// </summary>
    [Key("audio")] [DataType(SendDirectly = true)]
    Audio,

    /// <summary>
    ///     ITU数据
    /// </summary>
    [Key("itu")] [DataType(NeedSampling = true, CanDrop = false)]
    Itu,

    /// <summary>
    ///     DDC中频子通道数据
    /// </summary>
    [Key("ddc")] [DataType(NeedSampling = false, CanDrop = false)]
    Ddc,

    /// <summary>
    ///     短信数据
    /// </summary>
    [Key("sms")] [DataType(NeedSampling = false, CanDrop = false)]
    Sms,

    /// <summary>
    ///     示向度
    /// </summary>
    [Key("dfind")] [DataType(NeedSampling = true, CanDrop = false)]
    Dfind,

    /// <summary>
    ///     宽带示向度
    /// </summary>
    [Key("dfpan")] [DataType(NeedSampling = true, CanDrop = false)]
    DfPan,

    /// <summary>
    ///     扫描示向度
    /// </summary>
    [Key("dfscan")] [DataType(NeedSampling = true, CanDrop = false)]
    DfScan,

    /// <summary>
    ///     开关数据（开关状态、电压、电流等）
    /// </summary>
    [Key("switchState")] [DataType(NeedSampling = false, CanDrop = true)]
    SwitchState,

    /// <summary>
    ///     环境数据（温度、湿度、气压等）
    /// </summary>
    [Key("environment")] [DataType(NeedSampling = false, CanDrop = true)]
    Environment,

    /// <summary>
    ///     安防报警信息
    /// </summary>
    [Key("securityAlarm")] [DataType(NeedSampling = false, CanDrop = false)]
    SecurityAlarm,

    /// <summary>
    ///     空间谱测向数据
    /// </summary>
    [Key("sse")] [DataType(NeedSampling = true, CanDrop = false)]
    Sse,

    /// <summary>
    ///     调制识别信息
    /// </summary>
    [Key("recognize")] [DataType(NeedSampling = true, CanDrop = false)]
    Recognize,

    /// <summary>
    ///     占用度数据
    /// </summary>
    [Key("occupancy")] [DataType(NeedSampling = true, CanDrop = false)]
    Occupancy,

    /// <summary>
    ///     占用度信号列表
    /// </summary>
    [Key("occupancySignals")] [DataType(NeedSampling = false, CanDrop = true)]
    OccupancySignals,

    /// <summary>
    ///     文件保存通知
    /// </summary>
    [Key("fileSaved")] [DataType(NeedSampling = false, CanDrop = false)]
    FileSavedNotification,

    /// <summary>
    ///     任务信息
    /// </summary>
    [Key("taskList")] [DataType(NeedSampling = false, CanDrop = false)]
    TaskList,

    /// <summary>
    ///     场强定位信息
    /// </summary>
    [Key("ssoa")] [DataType(NeedSampling = true, CanDrop = false)]
    Ssoa,

    /// <summary>
    ///     模板采集数据
    /// </summary>
    [Key("nsicTemplate")] [DataType(NeedSampling = true, CanDrop = false)]
    NsicTemplate,

    /// <summary>
    ///     信号比对结果数据
    /// </summary>
    [Key("nsicResult")] [DataType(NeedSampling = true, CanDrop = false)]
    NsicResult,

    /// <summary>
    ///     信号列表数据
    /// </summary>
    [Key("signalsList")] [DataType(NeedSampling = false, CanDrop = true)]
    SignalsList,

    /// <summary>
    ///     考试保障结果数据
    /// </summary>
    [Key("eseResult")] [DataType(NeedSampling = true, CanDrop = false)]
    EseResult,

    /// <summary>
    ///     无人机侦测结果数据
    /// </summary>
    [Key("uavd")] [DataType(NeedSampling = false, CanDrop = false)]
    Uavd,

    /// <summary>
    ///     信号捕获数据
    /// </summary>
    [Key("capture")] [DataType(NeedSampling = false, CanDrop = false)]
    Capture,

    /// <summary>
    ///     空闲频点
    /// </summary>
    [Key("freeFreqPoints")] [DataType(NeedSampling = false, CanDrop = true)]
    FreeSignals,

    /// <summary>
    ///     电视解调数据
    /// </summary>
    [Key("tvImage")] [DataType(NeedSampling = false, CanDrop = false)]
    TvImage,

    /// <summary>
    ///     天线因子
    /// </summary>
    [Key("factor")] [DataType(NeedSampling = false, CanDrop = true)]
    Factor,

    /// <summary>
    ///     离散扫描测向数据
    /// </summary>
    [Key("mscandf")] [DataType(NeedSampling = true, CanDrop = false)]
    MScanDf,

    /// <summary>
    ///     测向信号列表
    /// </summary>
    [Key("dfSignalList")] [DataType(NeedSampling = true, CanDrop = false)]
    DfSigalList,

    /// <summary>
    ///     荧光谱数据
    /// </summary>
    [Key("dpx")] [DataType(NeedSampling = true, CanDrop = false)]
    Dpx,

    /// <summary>
    ///     视频频道信息
    /// </summary>
    [Key("videoChannel")] [DataType(NeedSampling = false, CanDrop = true)]
    VideoChannel,

    /// <summary>
    ///     节目播放结果信息
    /// </summary>
    [Key("playResult")] [DataType(NeedSampling = true, CanDrop = false)]
    PlayResult,

    /// <summary>
    ///     回放数据查询结果
    /// </summary>
    [Key("dvrFileInfo")] [DataType(NeedSampling = true, CanDrop = false)]
    DvrFileInfo,

    /// <summary>
    ///     回放进度数据
    /// </summary>
    [Key("playbackProgress")] [DataType(NeedSampling = true, CanDrop = false)]
    PlaybackProgress,

    /// <summary>
    ///     查询进度数据
    /// </summary>
    [Key("searchProgress")] [DataType(NeedSampling = true, CanDrop = false)]
    SearchProgress,

    /// <summary>
    ///     音频识别数据
    /// </summary>
    [Key("audioRecognition")] [DataType(NeedSampling = false, CanDrop = true)]
    AudioRecognition,

    /// <summary>
    ///     航班信息
    /// </summary>
    [Key("ads-b")] [DataType(NeedSampling = false, CanDrop = true)]
    AdsB,

    /// <summary>
    ///     船舶信息
    /// </summary>
    [Key("ais")] [DataType(NeedSampling = false, CanDrop = true)]
    Ais,

    /// <summary>
    ///     电磁环境分析信号集合
    /// </summary>
    [Key("emdaSignals")] [DataType(NeedSampling = false, CanDrop = true)]
    EmdaSignals,

    /// <summary>
    ///     角度数据，适用于比幅测向，转台等角度数据
    /// </summary>
    [Key("angle")] [DataType(NeedSampling = true, CanDrop = false)]
    Angle,

    /// <summary>
    ///     无线电管制数据
    /// </summary>
    [Key("radioSuppressing")] [DataType(SendDirectly = true)]
    RadioSuppressing,

    /// <summary>
    ///     基站解码数据
    /// </summary>
    [Key("bsDecoding")] [DataType(NeedSampling = false, CanDrop = true)]
    BsDecoding,

    /// <summary>
    ///     离散扫描信号列表
    /// </summary>
    [Key("mscanSignals")] [DataType(NeedSampling = false, CanDrop = true)]
    MScanSignals,

    /// <summary>
    ///     信号解调数据
    /// </summary>
    [Key("signalDemod")] [DataType(NeedSampling = false, CanDrop = false)]
    SignalDemod,

    /// <summary>
    ///     信号精确分析通知消息
    /// </summary>
    [Key("iqRecordNotice")] [DataType(NeedSampling = false, CanDrop = false)]
    IqRecordNotice,

    /// <summary>
    ///     服务器资源监视 CPU
    /// </summary>
    [Key("srmCPU")] [DataType(NeedSampling = false, CanDrop = true)]
    SrmCpu,

    /// <summary>
    ///     服务器资源监视 内存
    /// </summary>
    [Key("srmMemory")] [DataType(NeedSampling = false, CanDrop = true)]
    SrmMemory,

    /// <summary>
    ///     服务器资源监视 磁盘
    /// </summary>
    [Key("srmHDD")] [DataType(NeedSampling = false, CanDrop = true)]
    SrmHdd,

    /// <summary>
    ///     数据更新速率
    /// </summary>
    [Key("fps")] [DataType(NeedSampling = false, CanDrop = true)]
    Fps,

    /// <summary>
    ///     原始数据存储大小数据
    /// </summary>
    [Key("rawDataLength")] [DataType(NeedSampling = false, CanDrop = true)]
    RawDataLength,

    /// <summary>
    ///     IQ星座图
    /// </summary>
    [Key("iqConstellations")] [DataType(NeedSampling = false, CanDrop = true)]
    IqConstellations,

    /// <summary>
    ///     IQ眼图
    /// </summary>
    [Key("iqEyeDiagram")] [DataType(NeedSampling = false, CanDrop = true)]
    IqEyeDiagram,

    /// <summary>
    ///     离散测向频点信息（统计数据）
    /// </summary>
    [Key("mfdfSignal")] [DataType(NeedSampling = false, CanDrop = true)]
    MfdfSignal,

    /// <summary>
    ///     航空监测频率数据
    /// </summary>
    [Key("avicgFrequencies")] [DataType(NeedSampling = false, CanDrop = true)]
    AvicgFrequencies,

    /// <summary>
    ///     航空监测控守通道信息
    /// </summary>
    [Key("avicgFrequencyChannels")] [DataType(NeedSampling = false, CanDrop = true)]
    AvicgFrequencyChannels,

    /// <summary>
    ///     频谱评估文件保存状态数据
    /// </summary>
    [Key("spevlFileSaveInfo")] [DataType(NeedSampling = false, CanDrop = true)]
    SpevlFileSaveInfo,

    /// <summary>
    ///     The signal decode notice
    /// </summary>
    [Key("signalDecodeNotice")] [DataType(NeedSampling = false, CanDrop = false)]
    SignalDecodeNotice,

    /// <summary>
    ///     单频测向示向度概率分布统计信息
    /// </summary>
    [Key("dfProbDist")] [DataType(NeedSampling = false, CanDrop = true)]
    DfindProbDist,

    /// <summary>
    ///     射电天文电测测试数据
    /// </summary>
    [Key("fastTestData")] [DataType(NeedSampling = false, CanDrop = true)]
    FastTestData,

    /// <summary>
    ///     干涉仪测向报表数据
    /// </summary>
    [Key("dfStatCI")] [DataType(NeedSampling = false, CanDrop = true)]
    DfStatCi,

    /// <summary>
    ///     空间谱测向报表数据
    /// </summary>
    [Key("dfStatSSE")] [DataType(NeedSampling = false, CanDrop = true)]
    DfStatSse,

    /// <summary>
    ///     最优值统计数据
    /// </summary>
    [Key("dfStatOptimal")] [DataType(NeedSampling = false, CanDrop = true)]
    DfStatOptimal,

    /// <summary>
    ///     离散扫描报表数据
    /// </summary>
    [Key("mfdfStat")] [DataType(NeedSampling = false, CanDrop = true)]
    MfdfStat,

    /// <summary>
    ///     频段扫描报表数据
    /// </summary>
    [Key("scanStat")] [DataType(NeedSampling = false, CanDrop = true)]
    ScanStat,

    /// <summary>
    ///     单频测量报表数据
    /// </summary>
    [Key("ffmStat")] [DataType(NeedSampling = false, CanDrop = true)]
    FfmStat
}

/// <summary>
///     任务状态
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<TaskState>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<TaskState>))]
public enum TaskState
{
    /// <summary>
    ///     The start
    /// </summary>
    [Key("start")] Start,

    /// <summary>
    ///     The pause
    /// </summary>
    [Key("pause")] Pause,

    /// <summary>
    ///     The stop
    /// </summary>
    [Key("stop")] Stop,

    /// <summary>
    ///     The new
    /// </summary>
    [Key("new")] New
}

/// <summary>
///     Enum MediaType
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<MediaType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<MediaType>))]
[Flags]
public enum MediaType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None = 0,

    /// <summary>
    ///     The audio
    /// </summary>
    [Key("audio")] Audio = 0x01,

    /// <summary>
    ///     The spectrum
    /// </summary>
    [Key("spectrum")] Spectrum = Audio << 1,

    /// <summary>
    ///     The iq
    /// </summary>
    [Key("iq")] Iq = Spectrum << 1,

    /// <summary>
    ///     The level
    /// </summary>
    [Key("level")] Level = Iq << 1,

    /// <summary>
    ///     The dfind
    /// </summary>
    [Key("dfind")] Dfind = Level << 1,

    /// <summary>
    ///     The itu
    /// </summary>
    [Key("itu")] Itu = Dfind << 1,

    /// <summary>
    ///     The scan
    /// </summary>
    [Key("scan")] Scan = Itu << 1,

    /// <summary>
    ///     The tdoa
    /// </summary>
    [Key("tdoa")] Tdoa = Scan << 1,

    /// <summary>
    ///     The dfpan
    /// </summary>
    [Key("dfpan")] Dfpan = Tdoa << 1
}

#region 环境监控

/// <summary>
///     Enum SwitchState
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SwitchState>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SwitchState>))]
public enum SwitchState
{
    /// <summary>
    ///     The disabled
    /// </summary>
    [Key("disabled")] Disabled,

    /// <summary>
    ///     The on
    /// </summary>
    [Key("on")] On,

    /// <summary>
    ///     The off
    /// </summary>
    [Key("off")] Off,

    /// <summary>
    ///     The invalid
    /// </summary>
    [Key("invalid")] Invalid
}

/// <summary>
///     环境监控电源类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SwitchType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SwitchType>))]
public enum SwitchType
{
    /// <summary>
    ///     The unknown
    /// </summary>
    [Key("未知")] Unknown,

    /// <summary>
    ///     The ac
    /// </summary>
    [Key("交流")] Ac,

    /// <summary>
    ///     The dc
    /// </summary>
    [Key("直流")] Dc
}

/// <summary>
///     Enum SecurityAlarm
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SecurityAlarm>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SecurityAlarm>))]
public enum SecurityAlarm
{
    /// <summary>
    ///     The smoke
    /// </summary>
    [Description("烟雾报警")] [Key("smoke")] Smoke,

    /// <summary>
    ///     The fire
    /// </summary>
    [Description("火灾报警")] [Key("fire")] Fire,

    /// <summary>
    ///     The flooding
    /// </summary>
    [Description("浸水报警")] [Key("flooding")]
    Flooding,

    /// <summary>
    ///     The infrared
    /// </summary>
    [Description("红外报警")] [Key("infrared")]
    Infrared,

    /// <summary>
    ///     The gate access
    /// </summary>
    [Description("门磁报警")] [Key("gateAccess")]
    GateAccess,

    /// <summary>
    ///     The over temperature
    /// </summary>
    [Description("高温报警")] [Key("overTemperature")]
    OverTemperature,

    /// <summary>
    ///     The current overload
    /// </summary>
    [Description("电流过载")] [Key("currentOverload")]
    CurrentOverload,

    /// <summary>
    ///     The voltage over load
    /// </summary>
    [Description("电压过载")] [Key("voltageOverload")]
    VoltageOverLoad
}

/// <summary>
///     Enum EnvironmentDataType
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<EnvironmentDataType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<EnvironmentDataType>))]
public enum EnvironmentDataType
{
    /// <summary>
    ///     The voltage
    /// </summary>
    [Key("voltage")] Voltage,

    /// <summary>
    ///     The current
    /// </summary>
    [Key("current")] Current,

    /// <summary>
    ///     The temperature
    /// </summary>
    [Key("temperature")] Temperature,

    /// <summary>
    ///     The humidity
    /// </summary>
    [Key("humidity")] Humidity,

    /// <summary>
    ///     The air pressure
    /// </summary>
    [Key("airPressure")] AirPressure
}

#endregion

#region 广播电视解调

/// <summary>
///     电视制式
/// </summary>
[Flags]
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<TvStandard>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<TvStandard>))]
public enum TvStandard
{
    /// <summary>
    ///     The anafm
    /// </summary>
    [Key("ANAFM")] [Description("模拟广播")] Anafm = 0x01,

    /// <summary>
    ///     The anatv
    /// </summary>
    [Key("ANATV")] [Description("模拟电视")] Anatv = 0x02,

    /// <summary>
    ///     The DVBC
    /// </summary>
    [Key("DVBC")] [Description("DVBC")] Dvbc = 0x04,

    /// <summary>
    ///     The DVBT
    /// </summary>
    [Key("DVBT")] [Description("DVBT")] Dvbt = 0x08,

    /// <summary>
    ///     The DVB t2
    /// </summary>
    [Key("DVBT2")] [Description("DVBT2")] Dvbt2 = 0x10,

    /// <summary>
    ///     The DTMB
    /// </summary>
    [Key("DTMB")] [Description("DTMB")] Dtmb = 0x20,

    /// <summary>
    ///     The CMMB
    /// </summary>
    [Key("CMMB")] [Description("CMMB")] Cmmb = 0x40
}

/// <summary>
///     播放控制枚举
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<PlayControlMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<PlayControlMode>))]
public enum PlayControlMode
{
    /// <summary>
    ///     The real play
    /// </summary>
    [Key("realPlay")] [Description("实时播放")]
    RealPlay,

    /// <summary>
    ///     The real stop
    /// </summary>
    [Key("realStop")] [Description("实时停止")]
    RealStop,

    /// <summary>
    ///     The real pause
    /// </summary>
    [Key("realPause")] [Description("实时暂停")]
    RealPause,

    /// <summary>
    ///     The playback play
    /// </summary>
    [Key("playbackPlay")] [Description("回放播放")]
    PlaybackPlay,

    /// <summary>
    ///     The playback stop
    /// </summary>
    [Key("playbackStop")] [Description("回放停止")]
    PlaybackStop,

    /// <summary>
    ///     The playback pause
    /// </summary>
    [Key("playbackPause")] [Description("回放暂停")]
    PlaybackPause
}

/// <summary>
///     速度控制枚举
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<SpeedControlMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<SpeedControlMode>))]
public enum SpeedControlMode
{
    /// <summary>
    ///     The slow
    /// </summary>
    [Key("slow")] [Description("慢放")] Slow,

    /// <summary>
    ///     The fast
    /// </summary>
    [Key("fast")] [Description("快放")] Fast,

    /// <summary>
    ///     The single
    /// </summary>
    [Key("single")] [Description("单帧")] Single,

    /// <summary>
    ///     The normal
    /// </summary>
    [Key("normal")] [Description("正常速度")] Normal
}

/// <summary>
///     操作类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<OperateType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<OperateType>))]
public enum OperateType
{
    /// <summary>
    ///     The real play
    /// </summary>
    [Key("realPlay")] [Description("实时节目播放")]
    RealPlay,

    /// <summary>
    ///     The real play start
    /// </summary>
    [Key("realPlayStart")] [Description("实时节目播放开始")]
    RealPlayStart,

    /// <summary>
    ///     The real play stop
    /// </summary>
    [Key("realPlayStop")] [Description("实时节目播放停止")]
    RealPlayStop,

    /// <summary>
    ///     The playback
    /// </summary>
    [Key("playback")] [Description("录像回放")]
    Playback,

    /// <summary>
    ///     The record start
    /// </summary>
    [Key("recordStart")] [Description("录像开始")]
    RecordStart,

    /// <summary>
    ///     The record stop
    /// </summary>
    [Key("recordStop")] [Description("录像结束")]
    RecordStop,

    /// <summary>
    ///     The other
    /// </summary>
    [Key("other")] [Description("其他操作")] Other
}

#endregion

/// <summary>
///     开关的用途
/// </summary>
public enum SwitchUsage
{
    /// <summary>
    ///     未知
    /// </summary>
    None,

    /// <summary>
    ///     监测
    /// </summary>
    RadioMonitoring,

    /// <summary>
    ///     管制
    /// </summary>
    RadioSuppressing,

    /// <summary>
    ///     其它
    /// </summary>
    Others
}

/// <summary>
///     边缘端能力枚举
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<EdgeCapacity>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<EdgeCapacity>))]
[Flags]
public enum EdgeCapacity : long
{
    /// <summary>
    ///     默认值
    /// </summary>
    [Key("none")] None = 0x0,

    /// <summary>
    ///     航空监测能力
    /// </summary>
    [Key("ads-b")] AdsB = 0x01,

    /// <summary>
    ///     水上监测能力
    /// </summary>
    [Key("ais")] Ais = AdsB << 1,

    /// <summary>
    ///     GPS能力
    /// </summary>
    [Key("gps")] Gps = Ais << 1,

    /// <summary>
    ///     罗盘能力
    /// </summary>
    [Key("compass")] Compass = Gps << 1,

    /// <summary>
    ///     基站解码能力
    /// </summary>
    [Key("bsDecoding")] BsDecoding = Compass << 1
}

/// <summary>
///     定义一个枚举类型，表示报警状态
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<AlarmStatus>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<AlarmStatus>))]
public enum AlarmStatus
{
    /// <summary>
    ///     未处理
    /// </summary>
    [Key("unprocessed")] Unprocessed,

    /// <summary>
    ///     已处理
    /// </summary>
    [Key("processed")] Processed,

    /// <summary>
    ///     已确认
    /// </summary>
    [Key("confirmed")] Confirmed,

    /// <summary>
    ///     解除报警
    /// </summary>
    [Key("allClear")] AllClear
}