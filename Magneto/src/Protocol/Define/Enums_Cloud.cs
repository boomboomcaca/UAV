// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Enums_Cloud.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Text.Json.Serialization;
using Magneto.Protocol.Extensions;
using MessagePack;

namespace Magneto.Protocol.Define;

#region 云端交互

/// <summary>
///     站点类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<StationType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<StationType>))]
public enum StationType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None = 0,

    /// <summary>
    ///     固定监测站
    /// </summary>
    [Key("stationaryCategory")] Stationary,

    /// <summary>
    ///     移动监测站
    /// </summary>
    [Key("movableCategory")] Movable,

    /// <summary>
    ///     可搬移监测站
    /// </summary>
    [Key("mobileCategory")] Mobile,

    /// <summary>
    ///     便携式监测设备
    /// </summary>
    [Key("portableCategory")] Portable,

    /// <summary>
    ///     空中监测站
    /// </summary>
    [Key("airCategory")] Air,

    /// <summary>
    ///     传感器
    /// </summary>
    [Key("sensorCategory")] Sensor
}

/// <summary>
///     模块类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ModuleType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleType>))]
public enum ModuleType
{
    /// <summary>
    ///     表示未知或任意（特定情况下）模块类型
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     表示设备模块类型
    /// </summary>
    [Key("device")] Device,

    /// <summary>
    ///     表示驱动/功能模块类型
    /// </summary>
    [Key("driver")] Driver
}

/// <summary>
///     模块分类
/// </summary>
[Flags]
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ModuleCategory>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleCategory>))]
public enum ModuleCategory : long
{
    /// <summary>
    ///     未知
    /// </summary>
    [Key("none")] None = 0,

    /// <summary>
    ///     测向定位
    /// </summary>
    [Key("directionFinding")] DirectionFinding = 1,

    /// <summary>
    ///     监测扫描
    /// </summary>
    [Key("monitoring")] Monitoring = DirectionFinding << 1,

    /// <summary>
    ///     天线控制
    /// </summary>
    [Key("antennaControl")] AntennaControl = Monitoring << 1,

    /// <summary>
    ///     控制器
    ///     TODO : 现在这个枚举只是给环境监控使用，如果以后有其他控制器，需要区分
    /// </summary>
    [Key("control")] Control = AntennaControl << 1,

    /// <summary>
    ///     GPS
    /// </summary>
    [Key("gps")] Gps = Control << 1,

    /// <summary>
    ///     电子罗盘
    /// </summary>
    [Key("compass")] Compass = Gps << 1,

    /// <summary>
    ///     解码器
    /// </summary>
    [Key("decoder")] Decoder = Compass << 1,

    /// <summary>
    ///     传感器
    /// </summary>
    [Key("sensor")] Sensor = Decoder << 1,

    /// <summary>
    ///     管制干预
    /// </summary>
    [Key("radioSuppressing")] RadioSuppressing = Sensor << 1,

    /// <summary>
    ///     开关矩阵
    /// </summary>
    [Key("switchArray")] SwitchArray = RadioSuppressing << 1,

    /// <summary>
    ///     存储模块
    /// </summary>
    [Key("ioStorage")] IoStorage = SwitchArray << 1,

    /// <summary>
    ///     云台/转台
    /// </summary>
    [Key("swivel")] Swivel = IoStorage << 1,

    /// <summary>
    ///     控制箱
    /// </summary>
    [Key("icb")] Icb = Swivel << 1,

    /// <summary>
    ///     图像识别设备
    /// </summary>
    [Key("recognizer")] Recognizer = Icb << 1,

    /// <summary>
    ///     主动雷达
    /// </summary>
    [Key("radar")] Radar = Recognizer << 1,

    /// <summary>
    ///     Uav解码器
    /// </summary>
    [Key("uavDecoder")] UavDecoder = Radar << 1,

    /// <summary>
    ///     信号破解器
    /// </summary>
    [Key("cracker")] Cracker = UavDecoder << 1
}

/// <summary>
///     模块状态
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ModuleState>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<ModuleState>))]
public enum ModuleState
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     空闲
    /// </summary>
    [Key("idle")] Idle,

    /// <summary>
    ///     忙碌
    /// </summary>
    [Key("busy")] Busy,

    /// <summary>
    ///     设备占用
    /// </summary>
    [Key("deviceBusy")] DeviceBusy,

    /// <summary>
    ///     离线
    /// </summary>
    [Key("offline")] Offline,

    /// <summary>
    ///     故障
    /// </summary>
    [Key("fault")] Fault,

    /// <summary>
    ///     禁用
    /// </summary>
    [Key("disabled")] Disabled
}

/// <summary>
///     日志类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<LogType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<LogType>))]
public enum LogType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None = 0,

    /// <summary>
    ///     The message
    /// </summary>
    [Key("message")] Message,

    /// <summary>
    ///     The warning
    /// </summary>
    [Key("warning")] Warning,

    /// <summary>
    ///     The error
    /// </summary>
    [Key("error")] Error
}

/// <summary>
///     参数类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ParameterDataType>))]
public enum ParameterDataType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     The string
    /// </summary>
    [Key("string")] String,

    /// <summary>
    ///     The bool
    /// </summary>
    [Key("bool")] Bool,

    /// <summary>
    ///     The number
    /// </summary>
    [Key("number")] Number,

    /// <summary>
    ///     The list
    /// </summary>
    [Key("list")] List
}

/// <summary>
///     音频类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<AudioFormat>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<AudioFormat>))]
public enum AudioFormat
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     The PCM
    /// </summary>
    [Key("pcm")] Pcm,

    /// <summary>
    ///     The pcmono
    /// </summary>
    [Key("pcmono")] Pcmono,

    /// <summary>
    ///     The gs M610
    /// </summary>
    [Key("gsm610")] Gsm610,

    /// <summary>
    ///     The m p3
    /// </summary>
    [Key("mp3")] Mp3
}

/// <summary>
///     极化方式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<Polarization>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<Polarization>))]
public enum Polarization
{
    /// <summary>
    ///     The vertical
    /// </summary>
    [Key("vertical")] Vertical,

    /// <summary>
    ///     The horizontal
    /// </summary>
    [Key("horizontal")] Horizontal
}

/// <summary>
///     天线类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<AntennaType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<AntennaType>))]
public enum AntennaType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     The monitoring
    /// </summary>
    [Key("monitoring")] Monitoring,

    /// <summary>
    ///     The direction finding
    /// </summary>
    [Key("directionFinding")] DirectionFinding
}

/// <summary>
///     天线选择模式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<AntennaSelectionMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<AntennaSelectionMode>))]
public enum AntennaSelectionMode
{
    /// <summary>
    ///     自动
    /// </summary>
    [Key("auto")] Auto,

    /// <summary>
    ///     手动
    /// </summary>
    [Key("manual")] Manual,

    /// <summary>
    ///     极化方式
    /// </summary>
    [Key("polarization")] Polarization
}

/// <summary>
///     文件数据类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<FileDataType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<FileDataType>))]
public enum FileDataType
{
    /// <summary>
    ///     原始数据
    /// </summary>
    [Key("raw")] Raw,

    /// <summary>
    ///     月报扫描
    /// </summary>
    [Key("report")] Report,

    /// <summary>
    ///     信号普查
    /// </summary>
    [Key("census")] SignalCensus,

    /// <summary>
    ///     音频数据
    /// </summary>
    [Key("wav")] Wav,

    /// <summary>
    ///     流盘数据
    /// </summary>
    [Key("ssd")] Ssd,

    /// <summary>
    ///     IQ数据
    /// </summary>
    [Key("iq")] Iq,

    /// <summary>
    ///     频谱评估数据
    /// </summary>
    [Key("spevl")] Spevl
}

/// <summary>
///     文件数据类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<FileNotificationType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<FileNotificationType>))]
public enum FileNotificationType
{
    /// <summary>
    ///     创建
    /// </summary>
    [Key("create")] Created,

    /// <summary>
    ///     更新
    /// </summary>
    [Key("modified")] Modified,

    /// <summary>
    ///     删除
    /// </summary>
    [Key("delete")] Delete
}

/// <summary>
///     业务日志类型
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<BusinessLogType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<BusinessLogType>))]
public enum BusinessLogType
{
    /// <summary>
    ///     The task
    /// </summary>
    [Key("task")] Task,

    /// <summary>
    ///     The device
    /// </summary>
    [Key("device")] Device,

    /// <summary>
    ///     The warn
    /// </summary>
    [Key("warn")] Warn
}

/// <summary>
///     Enum BusinessLogLevel
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<BusinessLogLevel>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<BusinessLogLevel>))]
public enum BusinessLogLevel
{
    /// <summary>
    ///     The information
    /// </summary>
    [Key("info")] Info,

    /// <summary>
    ///     The warn
    /// </summary>
    [Key("warn")] Warn,

    /// <summary>
    ///     The error
    /// </summary>
    [Key("error")] Error
}

/// <summary>
///     Enum DuplexMode
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<DuplexMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<DuplexMode>))]
public enum DuplexMode
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None = 0,

    /// <summary>
    ///     The GSM
    /// </summary>
    [Key("gsm")] Gsm = 1,

    /// <summary>
    ///     The wcdma
    /// </summary>
    [Key("wcdma")] Wcdma = 2,

    /// <summary>
    ///     The td scdma
    /// </summary>
    [Key("td_scdma")] TdScdma = 3,

    /// <summary>
    ///     The CDM a2000 1 x
    /// </summary>
    [Key("cdma2000_1x")] Cdma20001X = 4,

    /// <summary>
    ///     The CDM a2000 1x evdo
    /// </summary>
    [Key("cdma2000_1x_evdo")] Cdma20001XEvdo = 5,

    /// <summary>
    ///     The lte FDD
    /// </summary>
    [Key("lte_fdd")] LteFdd = 6,

    /// <summary>
    ///     The td lte
    /// </summary>
    [Key("td_lte")] TdLte = 7,

    /// <summary>
    ///     The nr 5 g
    /// </summary>
    [Key("nr_5g")] Nr5G = 8
}

#endregion