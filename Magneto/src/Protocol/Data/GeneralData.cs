// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-28-2023
// ***********************************************************************
// <copyright file="GeneralData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Protocol.Data;

/// <summary>
///     Class SDataRaw.
/// </summary>
[MessagePackObject]
public class SDataRaw
{
    /// <summary>
    ///     Gets or sets the type.
    /// </summary>
    /// <value>The type.</value>
    [Key("type")]
    public SDataType Type { get; protected set; }

    /// <summary>
    ///     Converts to bytes.
    /// </summary>
    /// <returns>System.Byte[].</returns>
    public virtual byte[] ToBytes()
    {
        return null;
    }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return Type.ToString();
    }
}

/// <summary>
///     日志数据
/// </summary>
public class SDataMessage : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataMessage" /> class.
    /// </summary>
    public SDataMessage()
    {
        Type = SDataType.Log;
    }

    /// <summary>
    ///     Gets or sets the type of the log.
    /// </summary>
    /// <value>The type of the log.</value>
    [Key("logType")]
    public LogType LogType { get; set; }

    /// <summary>
    ///     Gets or sets the error code.
    /// </summary>
    /// <value>The error code.</value>
    [Key("errorCode")]
    public int ErrorCode { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    public string Description { get; set; }

    /// <summary>
    ///     Gets or sets the detail.
    /// </summary>
    /// <value>The detail.</value>
    [Key("detail")]
    public string Detail { get; set; }
}

/// <summary>
///     GPS数据
/// </summary>
public class SDataGps : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataGps" /> class.
    /// </summary>
    public SDataGps()
    {
        Type = SDataType.Gps;
    }

    /// <summary>
    ///     经度 单位：度°
    /// </summary>
    /// <value>The longitude.</value>
    [Key("longitude")]
    public double Longitude { get; set; }

    /// <summary>
    ///     纬度 单位：度°
    /// </summary>
    /// <value>The latitude.</value>
    [Key("latitude")]
    public double Latitude { get; set; }

    /// <summary>
    ///     海拔高度 单位：米
    /// </summary>
    /// <value>The altitude.</value>
    [Key("altitude")]
    public float Altitude { get; set; }

    /// <summary>
    ///     航向 单位：度
    /// </summary>
    /// <value>The heading.</value>
    [Key("heading")]
    public float Heading { get; set; }

    /// <summary>
    ///     GPS运行速度 单位：KM/小时
    /// </summary>
    /// <value>The speed.</value>
    [Key("speed")]
    public ushort Speed { get; set; }

    /// <summary>
    ///     GPS所接受的卫星数量，单位：个（0~12）
    /// </summary>
    /// <value>The satellites.</value>
    [Key("satellites")]
    public int Satellites { get; set; }
}

/// <summary>
///     罗盘数据
/// </summary>
public class SDataCompass : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataCompass" /> class.
    /// </summary>
    public SDataCompass()
    {
        Type = SDataType.Compass;
    }

    /// <summary>
    ///     方位角
    /// </summary>
    /// <value>The heading.</value>
    [Key("heading")]
    public float Heading { get; set; }

    /// <summary>
    ///     滚动角
    /// </summary>
    /// <value>The rolling.</value>
    [Key("rolling")]
    public float Rolling { get; set; }

    /// <summary>
    ///     俯仰角
    /// </summary>
    /// <value>The pitch.</value>
    [Key("pitch")]
    public float Pitch { get; set; }
}

/// <summary>
///     模块状态改变数据
/// </summary>
public class SDataStateChange : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataStateChange" /> class.
    /// </summary>
    public SDataStateChange()
    {
        Type = SDataType.ModuleStateChange;
    }

    /// <summary>
    ///     状态发生变化的模块编号
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public Guid Id { get; set; }

    /// <summary>
    ///     模块类型
    /// </summary>
    /// <value>The type of the module.</value>
    [Key("moduleType")]
    public ModuleType ModuleType { get; set; }

    /// <summary>
    ///     模块变化后的状态
    /// </summary>
    /// <value>The state.</value>
    [Key("state")]
    public ModuleState State { get; set; }

    /// <summary>
    ///     状态的内容
    /// </summary>
    /// <value>The content.</value>
    [Key("content")]
    public string Content { get; set; }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        return $"ModuleStateChange:ID:{Id},Type:{ModuleType},State:{State}";
    }
}

/// <summary>
///     计划结果数据
/// </summary>
public class SDataCrondResult : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataCrondResult" /> class.
    /// </summary>
    public SDataCrondResult()
    {
        Type = SDataType.CrondResult;
    }

    /// <summary>
    ///     计划编号
    /// </summary>
    /// <value>The crond identifier.</value>
    [Key("crondId")]
    public string CrondId { get; set; }

    /// <summary>
    ///     执行结果
    /// </summary>
    /// <value><c>true</c> if result; otherwise, <c>false</c>.</value>
    [Key("result")]
    public bool Result { get; set; }

    /// <summary>
    ///     描述信息
    /// </summary>
    /// <value>The description.</value>
    [Key("description")]
    public string Description { get; set; }
}

/// <summary>
///     心跳数据
/// </summary>
public class SDataHeartBeat : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataHeartBeat" /> class.
    /// </summary>
    public SDataHeartBeat()
    {
        Type = SDataType.HeartBeat;
    }

    /// <summary>
    ///     推荐的心跳间隔时间，单位：秒
    /// </summary>
    /// <value>The interval.</value>
    [Key("interval")]
    public int Interval { get; set; }

    /// <summary>
    ///     失活的极限次数
    ///     大于或等于该次数，即时间（秒）=internal*maximumDownbeats，
    ///     表示客户、服务两段经协商后确定连接“已断开”，
    ///     需要双方主动断开连接并释放相关资源
    /// </summary>
    /// <value>The maximum downbeats.</value>
    [Key("maximumDownbeats")]
    public int MaximumDownbeats { get; set; }
}

/// <summary>
///     IQ数据
/// </summary>
public class SDataIq : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataIq" /> class.
    /// </summary>
    public SDataIq()
    {
        Type = SDataType.Iq;
    }

    /// <summary>
    ///     Gets or sets the timestamp.
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    ///     中心频率 单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     中频带宽 单位：kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("badwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     采样率 单位：kHz
    /// </summary>
    /// <value>The sampling rate.</value>
    [Key("samplingRate")]
    public double SamplingRate { get; set; }

    /// <summary>
    ///     采集信号时的衰减（包含中频和射频衰减），单位：dB
    /// </summary>
    /// <value>The attenuation.</value>
    [Key("attenuation")]
    public int Attenuation { get; set; }

    /// <summary>
    ///     16位I/Q 数据序列，与data32可任意其中之一为null
    /// </summary>
    /// <value>The data16.</value>
    [Key("data16")]
    public short[] Data16 { get; set; }

    /// <summary>
    ///     32位I/Q数据序列，与data16可任意其中之一为null
    /// </summary>
    /// <value>The data32.</value>
    [Key("data32")]
    public int[] Data32 { get; set; }
}

/// <summary>
///     电平数据
/// </summary>
public class SDataLevel : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataLevel" /> class.
    /// </summary>
    public SDataLevel()
    {
        Type = SDataType.Level;
    }

    /// <summary>
    ///     中心频率 单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     滤波带宽或中频带宽 单位：kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     表示当前数据是否为场强，true表示场强，false表示电平
    /// </summary>
    /// <value><c>true</c> if this instance is strength field; otherwise, <c>false</c>.</value>
    [Key("isStrengthField")]
    public bool IsStrengthField { get; set; }

    /// <summary>
    ///     表示电平或场强数据，单位：dBμV或dBμV/m
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float Data { get; set; }

    /// <summary>
    ///     Converts to bytes.
    /// </summary>
    /// <returns>System.Byte[].</returns>
    public override byte[] ToBytes()
    {
        var bytes = new byte[21];
        var offset = 0;
        var buffer = BitConverter.GetBytes(Frequency);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(Bandwidth);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(IsStrengthField);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(Data);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        return bytes;
    }
}

/// <summary>
///     频谱数据
/// </summary>
//[MessagePackObject] //基类加了特性以后这个特性可以不加
public class SDataSpectrum : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSpectrum" /> class.
    /// </summary>
    public SDataSpectrum()
    {
        Type = SDataType.Spectrum;
    }

    /// <summary>
    ///     中心频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     频谱带宽，单位 kHz
    /// </summary>
    /// <value>The span.</value>
    [Key("span")]
    public double Span { get; set; }

    /// <summary>
    ///     表示当前数据是否为场强，true表示场强，false表示电平
    /// </summary>
    /// <value><c>true</c> if this instance is strength field; otherwise, <c>false</c>.</value>
    [Key("isStrengthField")]
    public bool IsStrengthField { get; set; }

    /// <summary>
    ///     0: 关闭最大最小值开关
    ///     1: 开启最大最小值开关并且有值
    ///     2: 开启开关 并且 无值
    ///     0     1      2     3
    ///     最大值 最小值 平均值 噪声
    ///     例如：开启最大值开关，最大值字段<see cref="Maximum" />不为null，则本数组下发的数据如下:
    ///     1 0 0 0
    ///     开启最大值开关，最大值字段<see cref="Maximum" />为null，则本数组下发的数据如下:
    ///     2 0 0 0
    /// </summary>
    /// <value>The data mark.</value>
    [Key("dataMark")]
    public byte[] DataMark { get; set; } = new byte[4];

    /// <summary>
    ///     频谱数据 可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public short[] Data { get; set; }

    /// <summary>
    ///     噪声数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The noise.</value>
    [Key("noise")]
    public short[] Noise { get; set; }

    /// <summary>
    ///     最小谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The minimum.</value>
    [Key("minimum")]
    public short[] Minimum { get; set; }

    /// <summary>
    ///     最大谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The maximum.</value>
    [Key("maximum")]
    public short[] Maximum { get; set; }

    /// <summary>
    ///     平均谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The mean.</value>
    [Key("mean")]
    public short[] Mean { get; set; }
}

/// <summary>
///     扫描数据
/// </summary>
public class SDataScan : SDataRaw
{
    /// <summary>
    ///     表示包实时扫描数据集合，可为null，单位：dBμV或dBμV/m
    /// </summary>
    [Key("data")] public short[] Data;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataScan" /> class.
    /// </summary>
    public SDataScan()
    {
        Type = SDataType.Scan;
    }

    /// <summary>
    ///     表示频段索引号，适用于多频段扫描的情况，索引从0开始
    /// </summary>
    /// <value>The segment offset.</value>
    [Key("segmentOffset")]
    public int SegmentOffset { get; set; }

    /// <summary>
    ///     表示当前段的起始频率，若为离散扫描，此字段无效，单位：MHz
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     表示当前段的结束频率，若为离散扫描，此字段无效，单位：MHz
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     表示当前段的扫描步进，若为离散扫描，此字段无效，单位：kHz
    /// </summary>
    /// <value>The step frequency.</value>
    [Key("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     表示当前包数据在当前频段或离散频点的偏移量
    /// </summary>
    /// <value>The offset.</value>
    [Key("offset")]
    public int Offset { get; set; }

    /// <summary>
    ///     表示当前频段或所有离散频点的总点数
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public int Total { get; set; }

    /// <summary>
    ///     表示当前数据是否为场强，true表示场强，false表示电平
    /// </summary>
    /// <value><c>true</c> if this instance is strength field; otherwise, <c>false</c>.</value>
    [Key("isStrengthField")]
    public bool IsStrengthField { get; set; }

    /// <summary>
    ///     Gets or sets the data mark.
    /// </summary>
    /// <value>The data mark.</value>
    [Key("dataMark")]
    public byte[] DataMark { get; set; } = new byte[4];

    /// <summary>
    ///     最小谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The minimum.</value>
    [Key("minimum")]
    public short[] Minimum { get; set; }

    /// <summary>
    ///     最大谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The maximum.</value>
    [Key("maximum")]
    public short[] Maximum { get; set; }

    /// <summary>
    ///     平均谱数据集合，可为null，单位dBμV或dBμV/m
    /// </summary>
    /// <value>The mean.</value>
    [Key("mean")]
    public short[] Mean { get; set; }

    /// <summary>
    ///     门限数据
    /// </summary>
    /// <value>The threshold.</value>
    [Key("threshold")]
    public short[] Threshold { get; set; }

    /// <summary>
    ///     Converts to bytes.
    /// </summary>
    /// <returns>System.Byte[].</returns>
    public override byte[] ToBytes()
    {
        var len = 37 + Data.Length * 4 * 4;
        var bytes = new byte[len];
        var offset = 0;
        var buffer = BitConverter.GetBytes(SegmentOffset);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(StartFrequency);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(StopFrequency);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(StepFrequency);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(Offset);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(Total);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        buffer = BitConverter.GetBytes(IsStrengthField);
        Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
        offset += buffer.Length;
        if (Data != null)
            foreach (var t in Data)
            {
                buffer = BitConverter.GetBytes(t);
                Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
                offset += buffer.Length;
            }

        if (Minimum != null)
            foreach (var t in Minimum)
            {
                buffer = BitConverter.GetBytes(t);
                Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
                offset += buffer.Length;
            }

        if (Maximum != null)
            foreach (var t in Maximum)
            {
                buffer = BitConverter.GetBytes(t);
                Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
                offset += buffer.Length;
            }

        if (Mean != null)
            foreach (var t in Mean)
            {
                buffer = BitConverter.GetBytes(t);
                Buffer.BlockCopy(buffer, 0, bytes, offset, buffer.Length);
                offset += buffer.Length;
            }

        return bytes;
    }
}

/// <summary>
///     Class SDataAudio.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataAudio : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAudio" /> class.
    /// </summary>
    public SDataAudio()
    {
        Type = SDataType.Audio;
    }

    /// <summary>
    ///     音频格式
    /// </summary>
    /// <value>The format.</value>
    [Key("format")]
    public AudioFormat Format { get; set; } = AudioFormat.Pcm;

    /// <summary>
    ///     采样率 单位Hz
    /// </summary>
    /// <value>The sampling rate.</value>
    [Key("samplingRate")]
    public int SamplingRate { get; set; } = 44100;

    /// <summary>
    ///     通道数
    /// </summary>
    /// <value>The channels.</value>
    [Key("channels")]
    public int Channels { get; set; } = 1;

    /// <summary>
    ///     数据传输率
    /// </summary>
    /// <value>The bytes per second.</value>
    [Key("bytesPerSecond")]
    public int BytesPerSecond { get; set; } = 88200;

    /// <summary>
    ///     块大小
    /// </summary>
    /// <value>The block align.</value>
    [Key("blockAlign")]
    public int BlockAlign { get; set; } = 2;

    /// <summary>
    ///     采样位数
    /// </summary>
    /// <value>The bits per sample.</value>
    [Key("bitsPerSample")]
    public int BitsPerSample { get; set; } = 16;

    /// <summary>
    ///     音频字节流数据
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}

/// <summary>
///     Class SDataITU.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataItu : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataItu" /> class.
    /// </summary>
    public SDataItu()
    {
        Type = SDataType.Itu;
    }

    /// <summary>
    ///     信号中心频率，单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     场强，单位：dBμV/m
    /// </summary>
    /// <value>The field strength.</value>
    [Key("fieldStrength")]
    public double FieldStrength { get; set; }

    /// <summary>
    ///     实际带宽，单位：kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     信号的调制模式或模式识别结果
    /// </summary>
    /// <value>The modulation.</value>
    [Key("modulation")]
    public Modulation Modulation { get; set; }

    /// <summary>
    ///     表示其它ITU项，由键值对组成
    ///     <see cref="ItuMisc" />
    /// </summary>
    /// <value>The misc.</value>
    [Key("misc")]
    public Dictionary<string, object> Misc { get; set; }
}

/// <summary>
///     中频多路功能子通道数据
/// </summary>
public class SDataDdc : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDdc" /> class.
    /// </summary>
    public SDataDdc()
    {
        Type = SDataType.Ddc;
    }

    /// <summary>
    ///     通道号，从0开始
    /// </summary>
    /// <value>The channel number.</value>
    [Key("channelNumber")]
    public int ChannelNumber { get; set; }

    /// <summary>
    ///     表示具体的业务数据集合，object包含电平、频谱、音频等数据
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<object> Data { get; set; }
}

/// <summary>
///     短信数据
/// </summary>
public class SDataSms : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSms" /> class.
    /// </summary>
    public SDataSms()
    {
        Type = SDataType.Sms;
    }

    /// <summary>
    ///     信号中心频率，单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     主叫号码
    /// </summary>
    /// <value>The dialer.</value>
    [Key("dialer")]
    public string Dialer { get; set; }

    /// <summary>
    ///     被叫号码
    /// </summary>
    /// <value>The dialee.</value>
    [Key("dialee")]
    public string Dialee { get; set; }

    /// <summary>
    ///     短信文本
    /// </summary>
    /// <value>The text.</value>
    [Key("text")]
    public string Text { get; set; }
}

/// <summary>
///     单频测向的示向度数据
/// </summary>
public class SDataDfind : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfind" /> class.
    /// </summary>
    public SDataDfind()
    {
        Type = SDataType.Dfind;
    }

    /// <summary>
    ///     中心频率 单位MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     测向带宽 单位kHz
    /// </summary>
    /// <value>The width of the band.</value>
    [Key("bandWidth")]
    public double BandWidth { get; set; }

    /// <summary>
    ///     示向度 单位度°
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public float Azimuth { get; set; }

    /// <summary>
    ///     测向质量 单位%
    /// </summary>
    /// <value>The quality.</value>
    [Key("quality")]
    public float Quality { get; set; }

    /// <summary>
    ///     示向度最优值 单位 度°
    /// </summary>
    /// <value>The optimal azimuth.</value>
    [Key("optimalAzimuth")]
    public float OptimalAzimuth { get; set; }

    /// <summary>
    ///     概率区间 单位 度
    /// </summary>
    /// <value>The probability interval.</value>
    [Key("probabilityInterval")]
    public float ProbabilityInterval { get; set; }
}

/// <summary>
///     宽带测向数据
/// </summary>
public class SDataDfpan : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfpan" /> class.
    /// </summary>
    public SDataDfpan()
    {
        Type = SDataType.DfPan;
    }

    /// <summary>
    ///     带宽范围内的中心频率，单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     中频带宽或跨距，单位：kHz
    /// </summary>
    /// <value>The span.</value>
    [Key("span")]
    public double Span { get; set; }

    /// <summary>
    ///     带宽范围内频点或信道的测向示向度数据集合，单位：度
    ///     需要归一到 0~360
    ///     无效值为-1
    /// </summary>
    /// <value>The azimuths.</value>
    [Key("azimuths")]
    public float[] Azimuths { get; set; }

    /// <summary>
    ///     示向度最优值
    /// </summary>
    /// <value>The optimal azimuths.</value>
    [Key("optimalAzimuths")]
    public float[] OptimalAzimuths { get; set; }

    /// <summary>
    ///     带宽范围内频点或信道的测向质量数据集合，单位：%
    ///     需要归一到 0~360
    ///     无效值为-1
    /// </summary>
    /// <value>The qualities.</value>
    [Key("qualities")]
    public float[] Qualities { get; set; }
}

/// <summary>
///     扫描测向数据
/// </summary>
public class SDataDfScan : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfScan" /> class.
    /// </summary>
    public SDataDfScan()
    {
        Type = SDataType.DfScan;
    }

    /// <summary>
    ///     表示频段索引号，适用于多频段扫描的情况，索引从0开始
    /// </summary>
    /// <value>The segment offset.</value>
    [Key("segmentOffset")]
    public int SegmentOffset { get; set; }

    /// <summary>
    ///     扫描频段的起始频率，单位：MHz
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     扫描频段的结束频率，单位：MHz
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     扫描频段的步进，单位：kHz
    /// </summary>
    /// <value>The step frequency.</value>
    [Key("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     扫描频点偏移值
    /// </summary>
    /// <value>The offset.</value>
    [Key("offset")]
    public int Offset { get; set; }

    /// <summary>
    ///     当前扫描包含的频点数
    /// </summary>
    /// <value>The count.</value>
    [Key("count")]
    public int Count { get; set; }

    /// <summary>
    ///     可输出示向度数据的频率或信道在整个频段中的索引集合
    /// </summary>
    /// <value>The indices.</value>
    [Key("indices")]
    public int[] Indices { get; set; }

    /// <summary>
    ///     与索引集合相匹配频率或信道的示向度集合
    ///     需要归一到 0~360
    ///     无效值为-1
    /// </summary>
    /// <value>The azimuths.</value>
    [Key("azimuths")]
    public float[] Azimuths { get; set; }

    /// <summary>
    ///     示向度最优值
    /// </summary>
    /// <value>The optimal azimuths.</value>
    [Key("optimalAzimuths")]
    public float[] OptimalAzimuths { get; set; }

    /// <summary>
    ///     与索引集合相匹配频率或信道的测向质量集合
    ///     需要归一到 0~360
    ///     无效值为-1
    /// </summary>
    /// <value>The qualities.</value>
    [Key("qualities")]
    public float[] Qualities { get; set; }
}

/// <summary>
///     宽带测向自动提取的信号。
/// </summary>
public class SDataSignals : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSignals" /> class.
    /// </summary>
    public SDataSignals()
    {
        Type = SDataType.SignalsList;
    }

    /// <summary>
    ///     信号数据集合
    /// </summary>
    [Key("signals")]
    public List<Signal> Signals { get; set; }
}

/// <summary>
///     环境监控开关状态数据
/// </summary>
public class SDataSwitchState : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSwitchState" /> class.
    /// </summary>
    public SDataSwitchState()
    {
        Type = SDataType.SwitchState;
    }

    /// <summary>
    ///     模块ID
    /// </summary>
    /// <value>The module identifier.</value>
    [Key("moduleId")]
    public Guid ModuleId { get; set; }

    /// <summary>
    ///     边缘端ID
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     开关名
    /// </summary>
    /// <value>The name.</value>
    [Key("name")]
    public string Name { get; set; }

    /// <summary>
    ///     显示名
    /// </summary>
    /// <value>The display.</value>
    [Key("display")]
    public string Display { get; set; }

    /// <summary>
    ///     开关状态
    /// </summary>
    /// <value>The state.</value>
    [Key("state")]
    public SwitchState State { get; set; }

    /// <summary>
    ///     电源类型
    /// </summary>
    /// <value>The type of the switch.</value>
    [Key("switchType")]
    public SwitchType SwitchType { get; set; } = SwitchType.Unknown;

    /// <summary>
    ///     详细信息
    /// </summary>
    /// <value>The information.</value>
    [Key("info")]
    public List<EnvInfo> Info { get; set; }
}

/// <summary>
///     环境监控环境数据
/// </summary>
public class SDataEnvironment : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataEnvironment" /> class.
    /// </summary>
    public SDataEnvironment()
    {
        Type = SDataType.Environment;
    }

    /// <summary>
    ///     模块ID
    /// </summary>
    /// <value>The module identifier.</value>
    [Key("moduleId")]
    public Guid ModuleId { get; set; }

    /// <summary>
    ///     边缘端ID
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     详细信息
    /// </summary>
    /// <value>The information.</value>
    [Key("info")]
    public List<EnvInfo> Info { get; set; }
}

/// <summary>
///     环境监控告警数据
/// </summary>
public class SDataSecurityAlarm : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSecurityAlarm" /> class.
    /// </summary>
    public SDataSecurityAlarm()
    {
        Type = SDataType.SecurityAlarm;
    }

    /// <summary>
    ///     模块ID
    /// </summary>
    /// <value>The module identifier.</value>
    [Key("moduleId")]
    public Guid ModuleId { get; set; }

    /// <summary>
    ///     边缘端ID
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     详细信息
    /// </summary>
    /// <value>The information.</value>
    [Key("info")]
    public List<EnvInfo> Info { get; set; }
}

/// <summary>
///     调制识别数据
/// </summary>
public class SDataRecognize : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataRecognize" /> class.
    /// </summary>
    public SDataRecognize()
    {
        Type = SDataType.Recognize;
    }

    /// <summary>
    ///     Gets or sets the recognize list.
    /// </summary>
    /// <value>The recognize list.</value>
    [Key("recognizeInfo")]
    public List<RecognizeItem> RecognizeList { get; set; }
}

/// <summary>
///     完成数据文件更新的通知数据
/// </summary>
public class FileSavedNotification : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="FileSavedNotification" /> class.
    /// </summary>
    public FileSavedNotification()
    {
        Type = SDataType.FileSavedNotification;
    }

    /// <summary>
    ///     文件通知类型
    /// </summary>
    /// <value>The type of the notification.</value>
    [Key("notificationType")]
    public FileNotificationType NotificationType { get; set; }

    /// <summary>
    ///     任务编号
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public string TaskId { get; set; }

    /// <summary>
    ///     驱动编号
    /// </summary>
    /// <value>The driver identifier.</value>
    [Key("driverId")]
    public string DriverId { get; set; }

    /// <summary>
    ///     模块编号
    /// </summary>
    /// <value>The plugin identifier.</value>
    [Key("pluginId")]
    public string PluginId { get; set; }

    /// <summary>
    ///     数据文件根路径
    /// </summary>
    /// <value>The root path.</value>
    [Key("rootPath")]
    public string RootPath { get; set; }

    /// <summary>
    ///     数据文件相对路径
    /// </summary>
    /// <value>The relative path.</value>
    [Key("relativePath")]
    public string RelativePath { get; set; }

    /// <summary>
    ///     文件名（不带后缀）
    /// </summary>
    /// <value>The name of the file.</value>
    [Key("fileName")]
    public string FileName { get; set; }

    /// <summary>
    ///     机器ID
    /// </summary>
    /// <value>The computer identifier.</value>
    [Key("computerId")]
    public string ComputerId { get; set; }

    /// <summary>
    ///     参数
    /// </summary>
    /// <value>The parameters.</value>
    [Key("parameters")]
    public string Parameters { get; set; }

    /// <summary>
    ///     数据类型
    /// </summary>
    /// <value>The type of the data.</value>
    [Key("dataType")]
    public FileDataType DataType { get; set; }

    /// <summary>
    ///     开始记录时间
    /// </summary>
    /// <value>The begin record time.</value>
    [Key("beginRecordTime")]
    public ulong BeginRecordTime { get; set; }

    /// <summary>
    ///     结束记录时间
    /// </summary>
    /// <value>The end record time.</value>
    [Key("endRecordTime")]
    public ulong? EndRecordTime { get; set; }

    /// <summary>
    ///     文件最后修改时间
    /// </summary>
    /// <value>The last modified time.</value>
    [Key("lastModifiedTime")]
    public ulong LastModifiedTime { get; set; }

    /// <summary>
    ///     记录数
    /// </summary>
    /// <value>The record count.</value>
    [Key("recordCount")]
    public long RecordCount { get; set; }

    /// <summary>
    ///     文件大小（Byte）
    /// </summary>
    /// <value>The size.</value>
    [Key("size")]
    public long Size { get; set; }

    /// <summary>
    ///     Clones this instance.
    /// </summary>
    /// <returns>FileSavedNotification.</returns>
    public FileSavedNotification Clone()
    {
        return MemberwiseClone() as FileSavedNotification;
    }
}

/// <summary>
///     Class SDataTask.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataTask : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataTask" /> class.
    /// </summary>
    public SDataTask()
    {
        Type = SDataType.TaskList;
    }

    /// <summary>
    ///     当前的任务集合
    /// </summary>
    /// <value>The tasks.</value>
    [Key("taskInfo")]
    public List<RunningTaskInfo> Tasks { get; set; }

    /// <summary>
    ///     Returns a <see cref="System.String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
    public override string ToString()
    {
        var str = "TaskList:";
        Tasks.ForEach(item => str += item + "; ");
        return str;
    }
}

/// <summary>
///     单站场强定位结果数据
/// </summary>
public class SDataSsoa : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSsoa" /> class.
    /// </summary>
    public SDataSsoa()
    {
        Type = SDataType.Ssoa;
    }

    /// <summary>
    ///     中心频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     频谱带宽，单位 kHz
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     左上角经度
    /// </summary>
    /// <value>The left top longitude.</value>
    [Key("leftTopLongitude")]
    public double LeftTopLongitude { get; set; }

    /// <summary>
    ///     左上角纬度
    /// </summary>
    /// <value>The left top latitude.</value>
    [Key("leftTopLatitude")]
    public double LeftTopLatitude { get; set; }

    /// <summary>
    ///     右下角经度
    /// </summary>
    /// <value>The right bottom longitude.</value>
    [Key("rightBottomLongitude")]
    public double RightBottomLongitude { get; set; }

    /// <summary>
    ///     右下角纬度
    /// </summary>
    /// <value>The right bottom latitude.</value>
    [Key("rightBottomLatitude")]
    public double RightBottomLatitude { get; set; }

    /// <summary>
    ///     最大值经度
    /// </summary>
    /// <value>The maximum longitude.</value>
    [Key("maxLongitude")]
    public double MaxLongitude { get; set; }

    /// <summary>
    ///     最大值纬度
    /// </summary>
    /// <value>The maximum latitude.</value>
    [Key("maxLatitude")]
    public double MaxLatitude { get; set; }

    /// <summary>
    ///     图片数据
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}

/// <summary>
///     信号模板数据
/// </summary>
public class SDataNsicTemplate : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataNsicTemplate" /> class.
    /// </summary>
    public SDataNsicTemplate()
    {
        Type = SDataType.NsicTemplate;
    }

    /// <summary>
    ///     Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("templateId")]
    public string TemplateId { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SegmentTemplateData[] Data { get; set; }
}

/// <summary>
///     信号比对结果数据
/// </summary>
public class SDataNsicResult : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataNsicResult" /> class.
    /// </summary>
    public SDataNsicResult()
    {
        Type = SDataType.NsicResult;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SegmentResultData[] Data { get; set; }
}

/// <summary>
///     频段占用度数据
/// </summary>
public class SDataOccupancy : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataOccupancy" /> class.
    /// </summary>
    public SDataOccupancy()
    {
        Type = SDataType.Occupancy;
    }

    /// <summary>
    ///     总占用度
    /// </summary>
    /// <value>The total occupancy.</value>
    [Key("totalOccupancy")]
    public double TotalOccupancy { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SegmentOccupancyData[] Data { get; set; }
}

/// <summary>
///     信号列表数据
/// </summary>
public class SDataSignalsList : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSignalsList" /> class.
    /// </summary>
    public SDataSignalsList()
    {
        Type = SDataType.SignalsList;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SignalsData[] Data { get; set; }
}

/// <summary>
///     空间谱测向数据
/// </summary>
public class SDataSse : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSse" /> class.
    /// </summary>
    public SDataSse()
    {
        Type = SDataType.Sse;
    }

    /// <summary>
    ///     频率，单位：MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     测向带宽，单位：KHz（中频带宽）
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     谱估计结果数据，若数组长度为360，则精度为1度，若数组长度为3600，则精度为0.1度
    ///     取值范围：0 ~ 100，点数与角度相关，角度取值为[0~360)，
    ///     注：稳定的波峰对应的角度即为同频多信号的相对零号天线的来波角度（示向度）
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public float[] Data { get; set; }

    /// <summary>
    ///     估算出的示向度结果
    /// </summary>
    /// <value>The results.</value>
    [Key("results")]
    public float[] Results { get; set; }

    /// <summary>
    ///     示向度个数
    /// </summary>
    /// <value>The azimuth count.</value>
    [Key("azimuthCount")]
    public int AzimuthCount { get; set; }
}

/// <summary>
///     考试保障结果数据
/// </summary>
public class SDataEseResult : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataEseResult" /> class.
    /// </summary>
    public SDataEseResult()
    {
        Type = SDataType.EseResult;
    }

    /// <summary>
    ///     中心频率
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     是否解码成功
    /// </summary>
    /// <value><c>true</c> if result; otherwise, <c>false</c>.</value>
    [Key("result")]
    public bool Result { get; set; }

    /// <summary>
    ///     解码结果
    /// </summary>
    /// <value>The decoder.</value>
    [Key("decoder")]
    public string Decoder { get; set; }

    /// <summary>
    ///     Gets or sets the system.
    /// </summary>
    /// <value>The system.</value>
    [IgnoreMember]
    public string System { get; set; }

    /// <summary>
    ///     音频数据存放路径（相对路径）
    /// </summary>
    /// <value>The audio file.</value>
    [Key("audioFile")]
    public string AudioFile { get; set; }
}

/// <summary>
///     无人机侦测结果数据
/// </summary>
public class SDataUavd : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataUavd" /> class.
    /// </summary>
    public SDataUavd()
    {
        Type = SDataType.Uavd;
    }

    /// <summary>
    ///     无人机型号
    /// </summary>
    /// <value>The model.</value>
    [Key("model")]
    public string Model { get; set; }

    /// <summary>
    ///     首次出现时间
    /// </summary>
    /// <value>The first time.</value>
    [Key("firstTime")]
    public ulong FirstTime { get; set; }

    /// <summary>
    ///     最后发现时间
    /// </summary>
    /// <value>The last time.</value>
    [Key("lastTime")]
    public ulong LastTime { get; set; }

    /// <summary>
    ///     无人机出现次数
    /// </summary>
    /// <value>The occurrences.</value>
    [Key("occurrences")]
    public int Occurrences { get; set; }

    /// <summary>
    ///     示向度
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public float Azimuth { get; set; }

    /// <summary>
    ///     无人机详细信息
    /// </summary>
    /// <value>The details.</value>
    [Key("details")]
    public List<DroneData> Details { get; set; }
}

/// <summary>
///     信号捕获数据
/// </summary>
public class SDataCapture : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataCapture" /> class.
    /// </summary>
    public SDataCapture()
    {
        Type = SDataType.Capture;
    }

    /// <summary>
    ///     中心频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     数据集合
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<CaptureStruct> Data { get; set; }
}

/// <summary>
///     空闲频点数据
/// </summary>
public class SDataFreeSignals : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFreeSignals" /> class.
    /// </summary>
    public SDataFreeSignals()
    {
        Type = SDataType.FreeSignals;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public FreeSignalsData[] Data { get; set; }
}

/// <summary>
///     Class SDataTVImage.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataTvImage : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataTvImage" /> class.
    /// </summary>
    public SDataTvImage()
    {
        Type = SDataType.TvImage;
    }

    /// <summary>
    ///     Gets or sets the frequency.
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     Gets or sets the bandwidth.
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     Gets or sets the time.
    /// </summary>
    /// <value>The time.</value>
    [Key("time")]
    public DateTime Time { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}

/// <summary>
///     Class SDataFactor.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataFactor : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFactor" /> class.
    /// </summary>
    public SDataFactor()
    {
        Type = SDataType.Factor;
    }

    /// <summary>
    ///     频段索引号，仅频段有意义
    /// </summary>
    /// <value>The segment offset.</value>
    [Key("segmentOffset")]
    public int SegmentOffset { get; set; }

    /// <summary>
    ///     起始频率 单位MHz，仅频段有意义
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     结束频率 单位MHz，仅频段有意义
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     频率步进 单位kHz，仅频段有意义
    /// </summary>
    /// <value>The step frequency.</value>
    [Key("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     数据个数
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public int Total { get; set; }

    /// <summary>
    ///     天线因子数据
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public short[] Data { get; set; }
}

/// <summary>
///     离散扫描测向数据
/// </summary>
public class SDataMScanDf : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataMScanDf" /> class.
    /// </summary>
    public SDataMScanDf()
    {
        Type = SDataType.MScanDf;
    }

    /// <summary>
    ///     频率表总数
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public ushort Total { get; set; }

    /// <summary>
    ///     频点序号
    /// </summary>
    /// <value>The index.</value>
    [Key("index")]
    public ushort Index { get; set; }

    /// <summary>
    ///     频率
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     电平 dBμV
    /// </summary>
    /// <value>The level.</value>
    [Key("level")]
    public double Level { get; set; }

    /// <summary>
    ///     示向度   单位：°  取值范围：[0,360)基准方位为地理正北，顺时针方向递增
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public double Azimuth { get; set; }

    /// <summary>
    ///     俯仰角   单位：°，取值范围：[-900,900]水平方向为0°，向上为正
    /// </summary>
    /// <value>The elevation.</value>
    [Key("elevation")]
    public double Elevation { get; set; }

    /// <summary>
    ///     测向质量   示向度的可信程度，单位：%  0-100
    /// </summary>
    /// <value>The quality.</value>
    [Key("quality")]
    public double Quality { get; set; }
}

/// <summary>
///     测向信号列表数据
/// </summary>
public class SDataDfSignalList : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDfSignalList" /> class.
    /// </summary>
    public SDataDfSignalList()
    {
        Type = SDataType.DfSigalList;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public DfSignalData[] Data { get; set; }
}

/// <summary>
///     视频频道信息
/// </summary>
public class SDataVideoChannel : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataVideoChannel" /> class.
    /// </summary>
    public SDataVideoChannel()
    {
        Type = SDataType.VideoChannel;
    }

    /// <summary>
    ///     频道节目信息集合
    /// </summary>
    /// <value>The programs.</value>
    [Key("programs")]
    public List<ChannelProgramInfo> Programs { get; set; }
}

/// <summary>
///     节目播放结果信息
/// </summary>
public class SDataPlayResult : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataPlayResult" /> class.
    /// </summary>
    public SDataPlayResult()
    {
        Type = SDataType.PlayResult;
    }

    /// <summary>
    ///     当前频道的频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     频道的电视制式
    /// </summary>
    /// <value>The standard.</value>
    [Key("standard")]
    public string Standard { get; set; }

    /// <summary>
    ///     操作类型
    /// </summary>
    /// <value>The type of the operate.</value>
    [Key("operateType")]
    public OperateType OperateType { get; set; }

    /// <summary>
    ///     节目号
    /// </summary>
    /// <value>The program number.</value>
    [Key("programNumber")]
    public int ProgramNumber { get; set; }

    /// <summary>
    ///     节目名称
    /// </summary>
    /// <value>The name of the program.</value>
    [Key("programName")]
    public string ProgramName { get; set; }

    /// <summary>
    ///     播放结果 true-成功;false-失败
    /// </summary>
    /// <value><c>true</c> if result; otherwise, <c>false</c>.</value>
    [Key("result")]
    public bool Result { get; set; }

    /// <summary>
    ///     视频文件路径
    /// </summary>
    /// <value>The URI.</value>
    [Key("uri")]
    public string Uri { get; set; }
}

/// <summary>
///     回放数据查询结果
/// </summary>
public class SDataDvrFileInfo : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataDvrFileInfo" /> class.
    /// </summary>
    public SDataDvrFileInfo()
    {
        Type = SDataType.DvrFileInfo;
    }

    /// <summary>
    ///     查询到的文件数量
    /// </summary>
    /// <value>The count.</value>
    [Key("count")]
    public int Count { get; set; }

    /// <summary>
    ///     文件列表
    /// </summary>
    /// <value>The files.</value>
    [Key("list")]
    public List<DvrFileInfo> Files { get; set; }
}

/// <summary>
///     回放进度数据
/// </summary>
public class SDataPlaybackProgress : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataPlaybackProgress" /> class.
    /// </summary>
    public SDataPlaybackProgress()
    {
        Type = SDataType.PlaybackProgress;
    }

    /// <summary>
    ///     当前播放位置 单位 秒
    /// </summary>
    /// <value>The current position.</value>
    [Key("currentPosition")]
    public int CurrentPosition { get; set; }

    /// <summary>
    ///     总时长 单位 秒
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public int Total { get; set; }

    /// <summary>
    ///     播放进度 0-100%
    /// </summary>
    /// <value>The progress.</value>
    [Key("progress")]
    public int Progress { get; set; }
}

/// <summary>
///     查询进度数据
/// </summary>
public class SDataSearchProgress : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSearchProgress" /> class.
    /// </summary>
    public SDataSearchProgress()
    {
        Type = SDataType.SearchProgress;
    }

    /// <summary>
    ///     Gets or sets the message.
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }

    /// <summary>
    ///     Gets or sets the progress.
    /// </summary>
    /// <value>The progress.</value>
    [Key("progress")]
    public double Progress { get; set; }
}

/// <summary>
///     荧光谱数据
/// </summary>
public class SDataFluoSpec : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFluoSpec" /> class.
    /// </summary>
    public SDataFluoSpec()
    {
        Type = SDataType.Dpx;
    }

    /// <summary>
    ///     中心频率 MHz
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     频谱带宽，单位 kHz
    /// </summary>
    /// <value>The span.</value>
    [Key("span")]
    public double Span { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[][] Data { get; set; }

    /// <summary>
    ///     最小电平，单位dBuv
    /// </summary>
    /// <value>The minimum level.</value>
    [Key("minLevel")]
    public short MinLevel { get; set; }

    /// <summary>
    ///     最大电平，单位dBuv
    /// </summary>
    /// <value>The maximum level.</value>
    [Key("maxLevel")]
    public short MaxLevel { get; set; }
}

/// <summary>
///     荧光谱数据
/// </summary>
public class SDataAudioRecognition : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAudioRecognition" /> class.
    /// </summary>
    public SDataAudioRecognition()
    {
        Type = SDataType.AudioRecognition;
    }

    /// <summary>
    ///     音频产生的时间
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timestamp")]
    public ulong Timestamp { get; set; }

    /// <summary>
    ///     音频识别结果
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }

    /// <summary>
    ///     关键词
    /// </summary>
    /// <value>The keywords.</value>
    [Key("keywords")]
    public List<string> Keywords { get; set; }
}

/// <summary>
///     航班信息集合
/// </summary>
public class SDataAdsB : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAdsB" /> class.
    /// </summary>
    public SDataAdsB()
    {
        Type = SDataType.AdsB;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<FlightInfo> Data { get; set; }
}

/// <summary>
///     船舶信息集合
/// </summary>
public class SDataAis : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAis" /> class.
    /// </summary>
    public SDataAis()
    {
        Type = SDataType.Ais;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<AisInfo> Data { get; set; }
}

/// <summary>
///     电磁环境分析信号数据
/// </summary>
public class SDataEmdaSignals : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataEmdaSignals" /> class.
    /// </summary>
    public SDataEmdaSignals()
    {
        Type = SDataType.EmdaSignals;
    }

    /// <summary>
    ///     频段序号
    /// </summary>
    /// <value>The index of the segment.</value>
    [Key("segmentIndex")]
    public int SegmentIndex { get; set; }

    /// <summary>
    ///     信号数据集合
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<EmdaInfo> Data { get; set; }
}

/// <summary>
///     Class SDataTaskChangeInfo.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataTaskChangeInfo : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataTaskChangeInfo" /> class.
    /// </summary>
    public SDataTaskChangeInfo()
    {
        Type = SDataType.TaskChangeInfo;
    }

    /// <summary>
    ///     Gets or sets the active task.
    /// </summary>
    /// <value>The active task.</value>
    [Key("activeTask")]
    public Guid ActiveTask { get; set; }

    /// <summary>
    ///     Gets or sets the device identifier.
    /// </summary>
    /// <value>The device identifier.</value>
    [Key("deviceId")]
    public Guid DeviceId { get; set; }

    /// <summary>
    ///     Gets or sets the stopped tasks.
    /// </summary>
    /// <value>The stopped tasks.</value>
    [Key("stopped")]
    public List<Guid> StoppedTasks { get; set; }

    /// <summary>
    ///     Gets or sets the suspended tasks.
    /// </summary>
    /// <value>The suspended tasks.</value>
    [Key("suspended")]
    public List<Guid> SuspendedTasks { get; set; }
}

/// <summary>
///     Class SDataAngle.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataAngle : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAngle" /> class.
    /// </summary>
    public SDataAngle()
    {
        Type = SDataType.Angle;
    }

    /// <summary>
    ///     方位角，单位：度
    /// </summary>
    /// <value>The azimuth.</value>
    [Key("azimuth")]
    public float Azimuth { get; set; }

    /// <summary>
    ///     俯仰角，单位：度
    /// </summary>
    /// <value>The elevation.</value>
    [Key("elevation")]
    public float Elevation { get; set; }

    /// <summary>
    ///     极化角，单位：度
    /// </summary>
    /// <value>The polarization.</value>
    [Key("polarization")]
    public float Polarization { get; set; }

    /// <summary>
    ///     是否完成相关运行或行为，如达到预设角度，或达到硬件限位角度
    /// </summary>
    /// <value><c>true</c> if completed; otherwise, <c>false</c>.</value>
    [Key("completed")]
    public bool Completed { get; set; }
}

/// <summary>
///     Class SDataRadioSuppressing.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
public class SDataRadioSuppressing : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataRadioSuppressing" /> class.
    /// </summary>
    public SDataRadioSuppressing()
    {
        Type = SDataType.RadioSuppressing;
    }

    /// <summary>
    ///     通道编号
    /// </summary>
    /// <value>The channel number.</value>
    [Key("channelNumber")]
    public int ChannelNumber { get; set; }

    /// <summary>
    ///     通道功率，单位：瓦特（W），-1表示无效值，或功放处于关闭状态
    /// </summary>
    /// <value>The power.</value>
    [Key("power")]
    public float Power { get; set; }

    /// <summary>
    ///     是否驻波告警
    /// </summary>
    /// <value><c>true</c> if VSW; otherwise, <c>false</c>.</value>
    [Key("vsw")]
    public bool Vsw { get; set; }

    /// <summary>
    ///     是否过热告警
    /// </summary>
    /// <value><c>true</c> if [over heating]; otherwise, <c>false</c>.</value>
    [Key("overHeating")]
    public bool OverHeating { get; set; }

    /// <summary>
    ///     告警信息
    /// </summary>
    /// <value>The warning.</value>
    [Key("warning")]
    public string Warning { get; set; }
}

/// <summary>
///     Class SDataMScanSignals.
///     Implements the <see cref="Magneto.Protocol.Data.SDataRaw" />
/// </summary>
/// <seealso cref="Magneto.Protocol.Data.SDataRaw" />
[Serializable]
[MessagePackObject]
public class SDataMScanSignals : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataMScanSignals" /> class.
    /// </summary>
    public SDataMScanSignals()
    {
        Type = SDataType.MScanSignals;
    }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public List<MScanSignalsResult> Data { get; set; }
}