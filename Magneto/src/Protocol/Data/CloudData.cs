// ***********************************************************************
// Assembly         : Protocol
// Author           : Joker
// Created          : 03-27-2023
//
// Last Modified By : Joker
// Last Modified On : 03-28-2023
// ***********************************************************************
// <copyright file="CloudData.cs" company="成都阿莱夫信息技术有限公司">
//     @Aleph Co. Ltd. 2023
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;

namespace Magneto.Protocol.Data;

#region Http Restful API

/// <summary>
///     Struct TemplateDataSendToCloud
/// </summary>
[MessagePackObject]
public struct TemplateDataSendToCloud
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [Key("edgeId")]
    [JsonProperty("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("id")]
    [JsonProperty("id")]
    public string TemplateId { get; set; }

    /// <summary>
    ///     Gets or sets the location.
    /// </summary>
    /// <value>The location.</value>
    [Key("coordinate")]
    [JsonProperty("coordinate")]
    public string Location { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("template")]
    [JsonProperty("template")]
    public SegmentTemplateData[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    [Key("parameters")]
    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    [JsonProperty("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("endTime")]
    [JsonProperty("endTime")]
    public ulong StopTime { get; set; }
}

/// <summary>
///     新信号截获比对结果存储到云端
/// </summary>
[MessagePackObject]
public struct ResultDataSendToCloud
{
    /// <summary>
    ///     Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("templateId")]
    [JsonProperty("templateId")]
    public string TemplateId { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    [JsonProperty("data")]
    public SegmentResultData[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    [JsonProperty("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("endTime")]
    [JsonProperty("endTime")]
    public ulong StopTime { get; set; }
}

#region 考试保障

/// <summary>
///     考试保障比对结果存储到云端
/// </summary>
[MessagePackObject]
public struct EseResultDataSendToCloud
{
    /// <summary>
    ///     Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("templateId")]
    [JsonProperty("templateId")]
    public string TemplateId { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [Key("data")]
    [JsonProperty("data")]
    public EseSegmentResultData[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the start time.
    /// </summary>
    /// <value>The start time.</value>
    [Key("startTime")]
    [JsonProperty("startTime")]
    public ulong StartTime { get; set; }

    /// <summary>
    ///     Gets or sets the stop time.
    /// </summary>
    /// <value>The stop time.</value>
    [Key("endTime")]
    [JsonProperty("endTime")]
    public ulong StopTime { get; set; }
}

/// <summary>
///     考试保障白名单查询
/// </summary>
[MessagePackObject]
public struct EseWhiteListFromCloud
{
    /// <summary>
    ///     Gets or sets the identifier.
    /// </summary>
    /// <value>The identifier.</value>
    [Key("id")]
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    ///     Gets or sets the frequency.
    /// </summary>
    /// <value>The frequency.</value>
    [Key("frequency")]
    [JsonProperty("frequency")]
    public double Frequency { get; set; }

    /// <summary>
    ///     Gets or sets the bandwidth.
    /// </summary>
    /// <value>The bandwidth.</value>
    [Key("bandwidth")]
    [JsonProperty("bandwidth")]
    public double Bandwidth { get; set; }

    /// <summary>
    ///     Gets or sets the creation time.
    /// </summary>
    /// <value>The creation time.</value>
    [Key("createTime")]
    [JsonProperty("createTime")]
    public string CreateTime { get; set; }

    /// <summary>
    ///     Gets or sets the update time.
    /// </summary>
    /// <value>The update time.</value>
    [Key("updateTime")]
    [JsonProperty("updateTime")]
    public string UpdateTime { get; set; }
}

#endregion

/// <summary>
///     Struct TemplateDataFromCloud
/// </summary>
[MessagePackObject]
public struct TemplateDataFromCloud
{
    /// <summary>
    ///     Gets or sets the template identifier.
    /// </summary>
    /// <value>The template identifier.</value>
    [Key("id")]
    [JsonProperty("id")]
    public string TemplateId { get; set; }

    /// <summary>
    ///     Gets or sets the location.
    /// </summary>
    /// <value>The location.</value>
    [JsonProperty("coordinate")]
    [Key("coordinate")]
    public string Location { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [JsonProperty("template")]
    [Key("template")]
    public SegmentTemplateData[] Data { get; set; }

    /// <summary>
    ///     Gets or sets the parameters.
    /// </summary>
    /// <value>The parameters.</value>
    [JsonProperty("parameters")]
    [Key("parameters")]
    public Dictionary<string, object> Parameters { get; set; }
}

/// <summary>
///     台站信息
/// </summary>
public struct StationSignalsInfo
{
    /// <summary>
    ///     发射频率下限
    /// </summary>
    /// <value>The frequency ef begin.</value>
    [JsonProperty("freqEfb")]
    public string FrequencyEfBegin { get; set; }

    /// <summary>
    ///     发射频率上限
    /// </summary>
    /// <value>The frequency ef end.</value>
    [JsonProperty("freqEfe")]
    public string FrequencyEfEnd { get; set; }

    /// <summary>
    ///     发射带宽
    /// </summary>
    /// <value>The frequency e band.</value>
    [JsonProperty("freqEBand")]
    public string FrequencyEBand { get; set; }

    /// <summary>
    ///     接收带宽
    /// </summary>
    /// <value>The frequency r band.</value>
    [JsonProperty("freqRBand")]
    public string FrequencyRBand { get; set; }

    /// <summary>
    ///     台站名称
    /// </summary>
    /// <value>The name of the station.</value>
    [JsonProperty("statName")]
    public string StationName { get; set; }

    /// <summary>
    ///     台站经度
    /// </summary>
    /// <value>The station longitude.</value>
    [JsonProperty("statLg")]
    public string StationLongitude { get; set; }

    /// <summary>
    ///     台站纬度
    /// </summary>
    /// <value>The station latitude.</value>
    [JsonProperty("statLa")]
    public string StationLatitude { get; set; }

    /// <summary>
    ///     解调模式
    /// </summary>
    /// <value>The dem mode.</value>
    [JsonProperty("demodmode")]
    public string DemMode { get; set; }

    /// <summary>
    ///     台站类型
    /// </summary>
    /// <value>The type of the station.</value>
    [JsonProperty("stationType")]
    public string StationType { get; set; }

    /// <summary>
    ///     技术体制
    /// </summary>
    /// <value>The technical system.</value>
    [JsonProperty("ts")]
    public string TechnicalSystem { get; set; }
}

/// <summary>
///     电磁环境存储到云端的数据
/// </summary>
/// <typeparam name="T">T为Signals2Cloud"或"ElectromagneticData" /></typeparam>
public struct Emdc2CloudData<T>
{
    /// <summary>
    ///     Gets or sets the edge identifier.
    /// </summary>
    /// <value>The edge identifier.</value>
    [JsonProperty("edgeId")]
    public string EdgeId { get; set; }

    /// <summary>
    ///     Gets or sets the segments.
    /// </summary>
    /// <value>The segments.</value>
    [JsonProperty("segments")]
    public List<T> Segments { get; set; }
}

/// <summary>
///     信号信息
/// </summary>
/// <typeparam name="T">T为<see cref="SignalsResult" />或<see cref="FreeSignalsResult" /></typeparam>
public struct Signals2Cloud<T>
{
    /// <summary>
    ///     Gets or sets the start frequency.
    /// </summary>
    /// <value>The start frequency.</value>
    [JsonProperty("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the stop frequency.
    /// </summary>
    /// <value>The stop frequency.</value>
    [JsonProperty("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the step frequency.
    /// </summary>
    /// <value>The step frequency.</value>
    [JsonProperty("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [JsonProperty("data")]
    public List<T> Data { get; set; }
}

/// <summary>
///     电磁数据
/// </summary>
public struct ElectromagneticData
{
    /// <summary>
    ///     Gets or sets the start frequency.
    /// </summary>
    /// <value>The start frequency.</value>
    [JsonProperty("startFrequency")]
    public double StartFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the stop frequency.
    /// </summary>
    /// <value>The stop frequency.</value>
    [JsonProperty("stopFrequency")]
    public double StopFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the step frequency.
    /// </summary>
    /// <value>The step frequency.</value>
    [JsonProperty("stepFrequency")]
    public double StepFrequency { get; set; }

    /// <summary>
    ///     Gets or sets the maximum.
    /// </summary>
    /// <value>The maximum.</value>
    [JsonProperty("maximum")]
    public float[] Maximum { get; set; }

    /// <summary>
    ///     Gets or sets the minimum.
    /// </summary>
    /// <value>The minimum.</value>
    [JsonProperty("minimum")]
    public float[] Minimum { get; set; }

    /// <summary>
    ///     Gets or sets the average.
    /// </summary>
    /// <value>The average.</value>
    [JsonProperty("average")]
    public float[] Average { get; set; }

    /// <summary>
    ///     Gets or sets the threshold.
    /// </summary>
    /// <value>The threshold.</value>
    [JsonProperty("threshold")]
    public float[] Threshold { get; set; }

    /// <summary>
    ///     Gets or sets the occupancy.
    /// </summary>
    /// <value>The occupancy.</value>
    [JsonProperty("occupancy")]
    public float[] Occupancy { get; set; }
}

/// <summary>
///     云端查询到的字典数据（通用）
/// </summary>
public struct DictionaryData
{
    /// <summary>
    ///     Gets or sets the type of the dictionary.
    /// </summary>
    /// <value>The type of the dictionary.</value>
    [JsonProperty("dicNo")]
    public string DictionaryType { get; set; }

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    /// <value>The data.</value>
    [JsonProperty("data")]
    public DictionaryKeyValue[] Data { get; set; }
}

/// <summary>
///     Struct DictionaryKeyValue
/// </summary>
public struct DictionaryKeyValue
{
    /// <summary>
    ///     Gets or sets the key.
    /// </summary>
    /// <value>The key.</value>
    [JsonProperty("key")]
    public string Key { get; set; }

    /// <summary>
    ///     Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    [JsonProperty("value")]
    public string Value { get; set; }
}

#endregion