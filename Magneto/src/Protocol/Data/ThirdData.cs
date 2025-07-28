// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-27-2023
// ***********************************************************************
// <copyright file="ThirdData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json.Linq;

namespace Magneto.Protocol.Data;

/// <summary>
///     Class ResponseModel.
/// </summary>
[Serializable]
[MessagePackObject]
public class ResponseModel
{
    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="ResponseModel" /> is result.
    /// </summary>
    /// <value><c>true</c> if result; otherwise, <c>false</c>.</value>
    [Key("result")]
    public bool Result { get; set; }

    /// <summary>
    ///     Gets or sets the message.
    /// </summary>
    /// <value>The message.</value>
    [Key("message")]
    public string Message { get; set; }
}

/// <summary>
///     Class ResponseModel.
///     Implements the <see cref="Magneto.Protocol.Data.ResponseModel" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="Magneto.Protocol.Data.ResponseModel" />
[Serializable]
[MessagePackObject]
public class ResponseModel<T> : ResponseModel
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public T Data { get; set; }
}

/// <summary>
///     Class RequestModel.
/// </summary>
[Serializable]
[MessagePackObject]
public class RequestModel
{
    /// <summary>
    ///     Gets or sets the secret.
    /// </summary>
    /// <value>The secret.</value>
    [Key("secret")]
    public string Secret { get; set; }
}

/// <summary>
///     Class RequestModel.
///     Implements the <see cref="Magneto.Protocol.Data.RequestModel" />
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="Magneto.Protocol.Data.RequestModel" />
[Serializable]
[MessagePackObject]
public class RequestModel<T> : RequestModel
{
    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public T Data { get; set; }
}

/// <summary>
///     Class AnalysisDataInfo.
/// </summary>
[Serializable]
[MessagePackObject]
public class AnalysisDataInfo
{
    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the device identifier.
    /// </summary>
    /// <value>The device identifier.</value>
    [Key("deviceId")]
    public string DeviceId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the file.
    /// </summary>
    /// <value>The name of the file.</value>
    [Key("fileName")]
    public string FileName { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [calculate symbol rate].
    /// </summary>
    /// <value><c>true</c> if [calculate symbol rate]; otherwise, <c>false</c>.</value>
    [Key("calculateSymbolRate")]
    public bool CalculateSymbolRate { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public byte[] Data { get; set; }
}

/// <summary>
///     Class AnalysisResult.
/// </summary>
[Serializable]
[MessagePackObject]
public class AnalysisResult
{
    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="AnalysisResult" /> is success.
    /// </summary>
    /// <value><c>true</c> if success; otherwise, <c>false</c>.</value>
    [Key("success")]
    public bool Success { get; set; }

    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the device identifier.
    /// </summary>
    /// <value>The device identifier.</value>
    [Key("deviceId")]
    public string DeviceId { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    public SDataSignalDemod Data { get; set; }
}

/// <summary>
///     频域图
/// </summary>
[Serializable]
public class TraceSpectrum
{
    /// <summary>
    ///     中心频率，单位：Hz
    /// </summary>
    public double CenterFreq;

    /// <summary>
    ///     频谱轨迹数据
    /// </summary>
    public float[] Data;

    /// <summary>
    ///     扫描宽度，单位：Hz
    /// </summary>
    public double Span;

    /// <summary>
    ///     显示的标题
    /// </summary>
    public string Title;

    /// <summary>
    ///     横轴单位
    /// </summary>
    public string Xunit;

    /// <summary>
    ///     Y轴参考，单位：Yunit
    /// </summary>
    public double YRef;

    /// <summary>
    ///     Y轴刻度，单位：Yunit/div
    /// </summary>
    public double Yscale;

    /// <summary>
    ///     纵轴单位
    /// </summary>
    public string Yunit;
}

/// <summary>
///     时域图
/// </summary>
[Serializable]
public class TraceTime
{
    /// <summary>
    ///     时域谱轨迹数据
    /// </summary>
    public float[] Data;

    /// <summary>
    ///     时域分析开始时间
    /// </summary>
    public double StartTime;

    /// <summary>
    ///     时域分析结束时间
    /// </summary>
    public double StopTime;

    /// <summary>
    ///     显示的标题
    /// </summary>
    public string Title;

    /// <summary>
    ///     横轴单位
    /// </summary>
    public string Xunit;

    /// <summary>
    ///     Y轴参考，单位：Yunit
    /// </summary>
    public double YRef;

    /// <summary>
    ///     Y轴刻度，单位：Yunit/div
    /// </summary>
    public double Yscale;

    /// <summary>
    ///     纵轴单位
    /// </summary>
    public string Yunit;
}

/// <summary>
///     IQ星座图
/// </summary>
[Serializable]
public class TraceIq
{
    /// <summary>
    ///     IQ图横轴轨迹数据
    /// </summary>
    public float[] Data;

    /// <summary>
    ///     IQ图纵轴轨迹数据
    /// </summary>
    public float[] QData;

    /// <summary>
    ///     显示的标题
    /// </summary>
    public string Title;

    /// <summary>
    ///     横轴单位
    /// </summary>
    public string Xunit;

    /// <summary>
    ///     Y轴参考，单位：Yunit
    /// </summary>
    public double YRef;

    /// <summary>
    ///     Y轴刻度，单位：Yunit/div
    /// </summary>
    public double Yscale;

    /// <summary>
    ///     纵轴单位
    /// </summary>
    public string Yunit;
}

/// <summary>
///     Class FreqDomainResult.
/// </summary>
[Serializable]
public class FreqDomainResult
{
    /// <summary>
    ///     瀑布图
    /// </summary>
    public List<TraceSpectrum> SpectrogramTraces = new();

    /// <summary>
    ///     频谱图
    /// </summary>
    public TraceSpectrum SpectrumTrace = new();
}

/// <summary>
///     Class DemodResult.
/// </summary>
[Serializable]
public class DemodResult
{
    /// <summary>
    ///     矢量误差图
    /// </summary>
    public TraceTime ErrVectTimeTrace = new();

    /// <summary>
    ///     IQ星座图
    /// </summary>
    public TraceIq IqTrace = new();

    /// <summary>
    ///     相位误差图
    /// </summary>
    public TraceTime PhaseErrTrace = new();
}

/// <summary>
///     Class MathResult.
/// </summary>
[Serializable]
public class MathResult
{
    /// <summary>
    ///     公式计算图
    /// </summary>
    /// <value>The math spectrum.</value>
    public TraceSpectrum MathSpectrum { get; set; }
}

/// <summary>
///     Class TimeDomainResult.
/// </summary>
[Serializable]
public class TimeDomainResult
{
    /// <summary>
    ///     时域图（幅度）
    /// </summary>
    /// <value>The time trace.</value>
    public TraceTime TimeTrace { get; set; }

    /// <summary>
    ///     时域图（IQ）
    /// </summary>
    /// <value>The iq trace.</value>
    public TraceIq IqTrace { get; set; }
}

/// <summary>
///     结果单元。
/// </summary>
[Serializable]
public class ResultItem
{
    /// <summary>
    ///     单元值。
    /// </summary>
    private object _itemValue;

    /// <summary>
    ///     单元名称。
    /// </summary>
    /// <value>The name of the item.</value>
    public string ItemName { get; set; }

    /// <summary>
    ///     单元数据类型。
    /// </summary>
    /// <value>The type of the item.</value>
    public string ItemType { get; set; }

    /// <summary>
    ///     Gets or sets the item value.
    /// </summary>
    /// <value>The item value.</value>
    public object ItemValue
    {
        get => _itemValue;
        set
        {
            if (value is JArray jArray)
                _itemValue = jArray.Select(item => item.ToObject(typeof(ResultItem))).ToArray();
            else
                _itemValue = value;
        }
    }

    /// <summary>
    ///     单元单位。
    /// </summary>
    /// <value>The item unit.</value>
    public string ItemUnit { get; set; }

    /// <summary>
    ///     单元描述。
    /// </summary>
    /// <value>The item description.</value>
    public string ItemDescription { get; set; }
}

/// <summary>
///     Class AnalysisResultData.
/// </summary>
[Serializable]
public class AnalysisResultData
{
    /// <summary>
    ///     频域图形数据
    /// </summary>
    /// <value>The freq domain result.</value>
    public FreqDomainResult FreqDomainResult { get; set; }

    /// <summary>
    ///     时域图形数据
    /// </summary>
    /// <value>The time domain result.</value>
    public TimeDomainResult TimeDomainResult { get; set; }

    /// <summary>
    ///     公式计算图形数据
    /// </summary>
    /// <value>The math result.</value>
    public MathResult MathResult { get; set; }

    /// <summary>
    ///     解调图形数据
    /// </summary>
    /// <value>The demod result.</value>
    public DemodResult DemodResult { get; set; }

    /// <summary>
    ///     结果单元集合
    /// </summary>
    /// <value>The result items.</value>
    public List<ResultItem> ResultItems { get; set; }
}