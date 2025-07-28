// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="Enums_Parameter.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text.Json.Serialization;
using Magneto.Protocol.Extensions;
using MessagePack;

namespace Magneto.Protocol.Define;

#region 参数信息

/// <summary>
///     配置方式-增删改
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ConfigType>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<ConfigType>))]
public enum ConfigType
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     The add
    /// </summary>
    [Key("add")] Add,

    /// <summary>
    ///     The modify
    /// </summary>
    [Key("modify")] Modify,

    /// <summary>
    ///     The delete
    /// </summary>
    [Key("delete")] Delete
}

/// <summary>
///     射频模式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<RfMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<RfMode>))]
public enum RfMode
{
    /// <summary>
    ///     常规模式
    /// </summary>
    [Key("normal")] Normal,

    /// <summary>
    ///     低噪声模式
    /// </summary>
    [Key("lowNoise")] LowNoise,

    /// <summary>
    ///     低失真模式
    /// </summary>
    [Key("lowDistort")] LowDistort
}

/// <summary>
///     扫描模式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<ScanMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<ScanMode>))]
public enum ScanMode
{
    /// <summary>
    ///     全景扫描
    /// </summary>
    [Key("pscan")] Pscan,

    /// <summary>
    ///     频点扫描
    /// </summary>
    [Key("fscan")] Fscan,

    /// <summary>
    ///     离散扫描
    /// </summary>
    [Key("mscan")] MScan
}

/// <summary>
///     测向模式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<DFindMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<DFindMode>))]
public enum DFindMode
{
    /// <summary>
    ///     常规信号
    /// </summary>
    [Key("normal")] Normal,

    /// <summary>
    ///     弱小信号
    /// </summary>
    [Key("feebleness")] Feebleness,

    /// <summary>
    ///     突发信号
    /// </summary>
    [Key("gate")] Gate
}

/// <summary>
///     检波方式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<DetectMode>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<DetectMode>))]
public enum DetectMode
{
    /// <summary>
    ///     快速
    /// </summary>
    [Key("fast")] Fast,

    /// <summary>
    ///     峰值
    /// </summary>
    [Key("pos")] Pos,

    /// <summary>
    ///     均值
    /// </summary>
    [Key("avg")] Avg,

    /// <summary>
    ///     均方根
    /// </summary>
    [Key("rms")] Rms
}

/// <summary>
///     解调模式
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<Modulation>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<Modulation>))]
public enum Modulation
{
    /// <summary>
    ///     The none
    /// </summary>
    [Key("none")] None,

    /// <summary>
    ///     The fm
    /// </summary>
    [Key("fm")] Fm,

    /// <summary>
    ///     The am
    /// </summary>
    [Key("am")] Am,

    /// <summary>
    ///     The pm
    /// </summary>
    [Key("pm")] Pm,

    /// <summary>
    ///     The DMR
    /// </summary>
    [Key("dmr")] Dmr,

    /// <summary>
    ///     The d PMR
    /// </summary>
    [Key("dpmr")] DPmr,

    /// <summary>
    ///     The cw
    /// </summary>
    [Key("cw")] Cw,

    /// <summary>
    ///     The LSB
    /// </summary>
    [Key("lsb")] Lsb,

    /// <summary>
    ///     The usb
    /// </summary>
    [Key("usb")] Usb,

    /// <summary>
    ///     The pulse
    /// </summary>
    [Key("pulse")] Pulse,

    /// <summary>
    ///     The iq
    /// </summary>
    [Key("iq")] Iq,

    /// <summary>
    ///     The isb
    /// </summary>
    [Key("isb")] Isb,

    /// <summary>
    ///     The tv
    /// </summary>
    [Key("tv")] Tv,

    /// <summary>
    ///     The ask
    /// </summary>
    [Key("ask")] Ask,

    /// <summary>
    ///     The 2 FSK
    /// </summary>
    [Key("2fsk")] _2FSK,

    /// <summary>
    ///     The 4 FSK
    /// </summary>
    [Key("4fsk")] _4FSK,

    /// <summary>
    ///     The BPSK
    /// </summary>
    [Key("bpsk")] Bpsk,

    /// <summary>
    ///     The QPSK
    /// </summary>
    [Key("qpsk")] Qpsk,

    /// <summary>
    ///     The 8 PSK
    /// </summary>
    [Key("8psk")] _8PSK,

    /// <summary>
    ///     The 16 qam
    /// </summary>
    [Key("16qam")] _16QAM,

    /// <summary>
    ///     The DPSK
    /// </summary>
    [Key("dpsk")] Dpsk,

    /// <summary>
    ///     The tetra
    /// </summary>
    [Key("tetra")] Tetra,

    /// <summary>
    ///     The PDT
    /// </summary>
    [Key("pdt")] Pdt,

    /// <summary>
    ///     The NXDN
    /// </summary>
    [Key("nxdn")] Nxdn,

    /// <summary>
    ///     The LFM
    /// </summary>
    [Key("lfm")] Lfm,

    /// <summary>
    ///     The GMSK
    /// </summary>
    [Key("gmsk")] Gmsk
}

/// <summary>
///     测向体制
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<DfindMethod>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<DfindMethod>))]
public enum DfindMethod
{
    /// <summary>
    ///     干涉仪
    /// </summary>
    [Key("ci")] Ci,

    /// <summary>
    ///     空间谱
    /// </summary>
    [Key("sse")] Sse
}

/// <summary>
///     天线型号枚举
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<AntennaModel>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<AntennaModel>))]
public enum AntennaModel
{
    /// <summary>
    ///     The default
    /// </summary>
    [Key("Default")] Default,

    /// <summary>
    ///     The d H8911
    /// </summary>
    [Key("DH8911")] Dh8911,

    /// <summary>
    ///     The m a800 f
    /// </summary>
    [Key("MA800F")] Ma800F,

    /// <summary>
    ///     The m a802 p
    /// </summary>
    [Key("MA802P")] Ma802P,

    /// <summary>
    ///     The t S2021
    /// </summary>
    [Key("TS2021")] Ts2021,

    /// <summary>
    ///     The ad D195 071
    /// </summary>
    [Key("ADD195_071")] Add195071,

    /// <summary>
    ///     The h e309
    /// </summary>
    [Key("HE309")] He309,

    /// <summary>
    ///     The h F902 v
    /// </summary>
    [Key("HF902V")] Hf902V,

    /// <summary>
    ///     The h F902 h
    /// </summary>
    [Key("HF902H")] Hf902H,

    /// <summary>
    ///     The h e314 a1
    /// </summary>
    [Key("HE314A1")] He314A1,

    /// <summary>
    ///     The h F214
    /// </summary>
    [Key("HF214")] Hf214,

    /// <summary>
    ///     The h F907 om
    /// </summary>
    [Key("HF907OM")] Hf907Om,

    /// <summary>
    ///     The ad D071
    /// </summary>
    [Key("ADD071")] Add071,

    /// <summary>
    ///     The ad D075
    /// </summary>
    [Key("ADD075")] Add075,

    /// <summary>
    ///     The ad D119
    /// </summary>
    [Key("ADD119")] Add119,

    /// <summary>
    ///     The ad D157 h
    /// </summary>
    [Key("ADD157_H")] Add157H,

    /// <summary>
    ///     The ad D157 v
    /// </summary>
    [Key("ADD157_V")] Add157V,

    /// <summary>
    ///     The ad D175
    /// </summary>
    [Key("ADD175")] Add175,

    /// <summary>
    ///     The ad D195
    /// </summary>
    [Key("ADD195")] Add195,

    /// <summary>
    ///     The ad D196
    /// </summary>
    [Key("ADD196")] Add196,

    /// <summary>
    ///     The ad D197 h
    /// </summary>
    [Key("ADD197_H")] Add197H,

    /// <summary>
    ///     The ad D197 v
    /// </summary>
    [Key("ADD197_V")] Add197V,

    /// <summary>
    ///     The ad D295
    /// </summary>
    [Key("ADD295")] Add295,

    /// <summary>
    ///     The h e010
    /// </summary>
    [Key("HE010")] He010,

    /// <summary>
    ///     The h e016 h
    /// </summary>
    [Key("HE016H")] He016H,

    /// <summary>
    ///     The h e016 v
    /// </summary>
    [Key("HE016V")] He016V,

    /// <summary>
    ///     The h e500
    /// </summary>
    [Key("HE500")] He500,

    /// <summary>
    ///     The h e600
    /// </summary>
    [Key("HE600")] He600,

    /// <summary>
    ///     The h K014
    /// </summary>
    [Key("HK014")] Hk014,

    /// <summary>
    ///     The h K033
    /// </summary>
    [Key("HK033")] Hk033,

    /// <summary>
    ///     The h K309
    /// </summary>
    [Key("HK309")] Hk309,

    /// <summary>
    ///     The h L033
    /// </summary>
    [Key("HL033")] Hl033,

    /// <summary>
    ///     The h L040
    /// </summary>
    [Key("HL040")] Hl040
}

/// <summary>
///     参数显示样式枚举
/// </summary>
[MessagePackFormatter(typeof(MessagePackEnumAsStringFormatter<DisplayStyle>))]
[JsonConverter(typeof(JsonEnumAsStringFormatter<DisplayStyle>))]
public enum DisplayStyle
{
    /// <summary>
    ///     默认类型
    ///     无法确定的归类
    /// </summary>
    [Key("default")] Default,

    /// <summary>
    ///     开关类型
    /// </summary>
    [Key("switch")] Switch,

    /// <summary>
    ///     The radio
    /// </summary>
    /// <font color="red">Badly formed XML comment.</font>
    [Key("radio")] Radio,

    /// <summary>
    ///     选项类型 &gt;4
    /// </summary>
    [Key("dropdown")] Dropdown,

    /// <summary>
    ///     数值档位
    /// </summary>
    [Key("slider")] Slider,

    /// <summary>
    ///     The input
    /// </summary>
    /// <font color="red">Badly formed XML comment.</font>
    [Key("input")] Input,

    /// <summary>
    ///     针对带宽的类型
    /// </summary>
    [Key("bandwidth")] Bandwidth,

    /// <summary>
    ///     工控按钮，参数值不发生变也下发到功能和设备
    /// </summary>
    [Key("ics")] Ics
}

#endregion