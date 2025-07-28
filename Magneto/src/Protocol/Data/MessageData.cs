// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="MessageData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using Magneto.Protocol.Define;
using MessagePack;

/*
    发送到云端的消息数据
*/
namespace Magneto.Protocol.Data;

/// <summary>
///     航空监测频点信息
/// </summary>
public class SDataAvicgFrequencies : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAvicgFrequencies" /> class.
    /// </summary>
    public SDataAvicgFrequencies()
    {
        Type = SDataType.AvicgFrequencies;
    }

    /// <summary>
    ///     中心频率
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     中频带宽
    /// </summary>
    /// <value>If bandwidth.</value>
    [Key("ifBandwidth")]
    public double IfBandwidth { get; set; }

    /// <summary>
    ///     数据时间 精确到分钟的时间戳（非当前时间）
    /// </summary>
    /// <value>The timestamp.</value>
    [Key("timestamp")]
    public ulong Timestamp { get; set; }

    /// <summary>
    ///     相对路径
    /// </summary>
    /// <value>The relative path.</value>
    [Key("relativePath")]
    public string RelativePath { get; set; }

    /// <summary>
    ///     频点信息
    /// </summary>
    /// <value>The frequencies.</value>
    [Key("frequencies")]
    public List<FrequenciesInfo> Frequencies { get; set; }
}

/// <summary>
///     航空监测频点信息
/// </summary>
public class SDataAvicgFrequencyChannels : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataAvicgFrequencyChannels" /> class.
    /// </summary>
    public SDataAvicgFrequencyChannels()
    {
        Type = SDataType.AvicgFrequencyChannels;
    }

    /// <summary>
    ///     总通道数
    /// </summary>
    /// <value>The total.</value>
    [Key("total")]
    public int Total { get; set; }

    /// <summary>
    ///     当前控守的频点信息
    /// </summary>
    /// <value>The frequencies.</value>
    [Key("frequencies")]
    public double[] Frequencies { get; set; }
}

/// <summary>
///     Struct FrequenciesInfo
/// </summary>
[MessagePackObject]
public struct FrequenciesInfo
{
    /// <summary>
    ///     子通道频率
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     带宽
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     音频时长
    /// </summary>
    /// <value>The duration.</value>
    [Key("duration")]
    public float Duration { get; set; }

    /// <summary>
    ///     场强值
    /// </summary>
    /// <value>The field strength.</value>
    [Key("fieldStrength")]
    public float FieldStrength { get; set; }

    /// <summary>
    ///     文件名
    /// </summary>
    /// <value>The file.</value>
    [Key("file")]
    public string File { get; set; }
}

/// <summary>
///     频谱评估文件保存状态信息
/// </summary>
public class SDataSpevlFileSaveInfo : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataSpevlFileSaveInfo" /> class.
    /// </summary>
    public SDataSpevlFileSaveInfo()
    {
        Type = SDataType.SpevlFileSaveInfo;
    }

    /// <summary>
    ///     文件名称
    /// </summary>
    /// <value>The name of the file.</value>
    [Key("fileName")]
    public string FileName { get; set; }

    /// <summary>
    ///     当前文件尺寸，单位字节
    /// </summary>
    /// <value>The size of the current file.</value>
    [Key("currentFileSize")]
    public long CurrentFileSize { get; set; }

    /// <summary>
    ///     文件最大尺寸，单位字节
    /// </summary>
    /// <value>The maximum size of the file.</value>
    [Key("maxFileSize")]
    public long MaxFileSize { get; set; }

    /// <summary>
    ///     进度，0-100
    /// </summary>
    /// <value>The progress.</value>
    [Key("progress")]
    public int Progress { get; set; }

    /// <summary>
    ///     文件创建时间
    /// </summary>
    /// <value>The created time.</value>
    [Key("createdTime")]
    public ulong CreatedTime { get; set; }
}

/// <summary>
///     射电天文电测测试数据信息
/// </summary>
public class SDataFastTestData : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataFastTestData" /> class.
    /// </summary>
    public SDataFastTestData()
    {
        Type = SDataType.FastTestData;
    }

    /// <summary>
    ///     当前角度
    /// </summary>
    /// <value>The angle.</value>
    [Key("angle")]
    public float Angle { get; set; }

    /// <summary>
    ///     当前极化方式
    /// </summary>
    /// <value>The polarization.</value>
    [Key("polarization")]
    public string Polarization { get; set; }

    /// <summary>
    ///     当前起始频率
    /// </summary>
    /// <value>The start frequency.</value>
    [Key("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     当前结束频率
    /// </summary>
    /// <value>The stop frequency.</value>
    [Key("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     状态 0-测试中，1-测试完毕，2-取消,3-测试故障,4-弱干扰等待
    /// </summary>
    /// <value>The state.</value>
    [Key("state")]
    public int State { get; set; }

    /// <summary>
    ///     当前子任务ID
    /// </summary>
    /// <value>The task identifier.</value>
    [Key("taskId")]
    public string TaskId { get; set; }

    /// <summary>
    ///     频点信息
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public FastTestResultInfo Data { get; set; }
}

/// <summary>
///     Struct FastTestResultInfo
/// </summary>
[MessagePackObject]
public struct FastTestResultInfo
{
    /// <summary>
    ///     仪表读数
    /// </summary>
    /// <value>The meter.</value>
    [Key("meter")]
    public float[] Meter { get; set; }

    /// <summary>
    ///     系统增益
    /// </summary>
    /// <value>The system gain.</value>
    [Key("systemGain")]
    public float[] SystemGain { get; set; }

    /// <summary>
    ///     天线增益
    /// </summary>
    /// <value>The antenna gain.</value>
    [Key("antennaGain")]
    public float[] AntennaGain { get; set; }

    /// <summary>
    ///     系统噪声温度
    /// </summary>
    /// <value>The noise temperature.</value>
    [Key("noiseTemperature")]
    public float[] NoiseTemperature { get; set; }

    /// <summary>
    ///     天线口面测量值
    /// </summary>
    /// <value>The antenna measure.</value>
    [Key("antennaMeasure")]
    public float[] AntennaMeasure { get; set; }

    /// <summary>
    ///     功率谱密度1(单位dBW/m²Hz²，修约小数点后两位)
    /// </summary>
    /// <value>The PSD1.</value>
    [Key("psd1")]
    public float[] Psd1 { get; set; }

    /// <summary>
    ///     功率谱密度2(单位dBJy，修约小数点后两位)
    /// </summary>
    /// <value>The PSD2.</value>
    [Key("psd2")]
    public float[] Psd2 { get; set; }
}