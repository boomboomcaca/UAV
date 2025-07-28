// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="IQExtData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using Magneto.Protocol.Define;
using MessagePack;

namespace Magneto.Protocol.Data;

/// <summary>
///     IQ星座图
/// </summary>
public class SDataIqConstellations : SDataRaw
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SDataIqConstellations" /> class.
    /// </summary>
    public SDataIqConstellations()
    {
        Type = SDataType.IqConstellations;
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
    ///     I路数据
    /// </summary>
    /// <value>The i data.</value>
    [Key("iData")]
    public int[] Data { get; set; }

    /// <summary>
    ///     Q路数据
    /// </summary>
    /// <value>The q data.</value>
    [Key("qData")]
    public int[] QData { get; set; }
}